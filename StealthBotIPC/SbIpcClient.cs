using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using ProtoBuf;

namespace StealthBotIpc
{
    public class SbIpcClient : IDisposable
    {
        #region constants
        static readonly string SB_PIPE_NAME = "sbUiServerPipe";
        static readonly string SB_SERVER_NAME = ".";
        static readonly int SIZE_BUFFER = 65536;
        #endregion

        #region Events
        public event EventHandler<UiStateObjectEventArgs> UiStateObjectReceived;
        public event EventHandler<GenericExceptionEventArgs> ExceptionThrown;
        #endregion

        private Thread _listenerThread;

        #region Cross-thread locks
        //used to synchronize between connect/disconnect and transer
        private ManualResetEvent _connectionGate = new ManualResetEvent(true);
        //these cross-thread locks are used to prevent any blockage on reads/writes.
        private readonly object _connectionLock = new object();
        private readonly object _queueLock = new object();
        #endregion

        private Queue<byte[]> _serializedMessages = new Queue<byte[]>();

        private NamedPipeClientStream _pipeClientStream;

        private bool _isDisposed = false;

        #region Constructors, destructors
        public SbIpcClient()
        {
            //_startTryConnect();
        }

        ~SbIpcClient()
        {
            _dispose(true);
        }
        #endregion

        #region IDisposable stuff
        private void _dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                //Set isDisposed
                _isDisposed = true;

                //if calling from Finalize do fancy things...
                if (disposing)
                {
                    //clear the stream if not null
                    lock (_connectionLock)
                    {
                        //End the connection thread
                        if (_listenerThread != null &&
                            _listenerThread.ThreadState == ThreadState.Background)
                        {
                            _listenerThread.Join();
                        }

                        //Kill the pipe stream
                        if (_pipeClientStream != null)
                        {
                            _pipeClientStream.Close();
                        }
                    }
                }
                else
                {
                    if (_listenerThread != null)
                    {
                        _listenerThread.Abort();
                    }
                    if (_pipeClientStream != null)
                    {
                        _pipeClientStream.Close();
                    }
                }

            }
        }

        public void Dispose()
        {
            //Suppress deconstructor call
            GC.SuppressFinalize(this);
            _dispose(false);
        }
        #endregion

        /// <summary>
        /// Send a state object to the UI via a named pipe.
        /// </summary>
        /// <param name="sbStateObject"></param>
        public void SendSbStateObject(SbStateObject sbStateObject)
        {
            //Reference for message
            byte[] message = null;
            //Serialize our object into a memorystream
            using (MemoryStream memStream = new MemoryStream())
            {
                Serializer.Serialize<SbStateObject>(memStream, sbStateObject);

                //Set the message
                message = memStream.ToArray();
            }

            //Send that message, yo.
            _sendMessage(message);
        }

        /// <summary>
        /// Start the thread running the worker method for connecting the stream.
        /// </summary>
        private void _startTryConnect()
        {
            //Flip the manual reset event to block
            _connectionGate.Reset();
            //Set the thread, start it
            if (_listenerThread == null)
            {
                _listenerThread = new Thread(new ThreadStart(this._tryConnect));
                _listenerThread.Name = "SbIpcClientConnection";
                _listenerThread.IsBackground = true;
                _listenerThread.Start();
            }
        }

        /// <summary>
        /// Worker method for trying to connect the pipe stream.
        /// </summary>
        private void _tryConnect()
        {
            //Just to be safe, make sure the manual reset event is blocking
            _connectionGate.Reset();

            //get the connection lock
            lock (_connectionLock)
            {
                //Set the pipe stream
                _pipeClientStream = new NamedPipeClientStream(SB_SERVER_NAME, SB_PIPE_NAME, PipeDirection.InOut, PipeOptions.Asynchronous);

                //Wait for a connection...
                while (!_pipeClientStream.IsConnected)
                {
                    //Use a try/catch to catch any failed connection exceptions
                    try
                    {
                        //Seems like this is an extremely CPU-intensive call.
                        //Reduce the timeout and increase sleep if CPU use becomes
                        //too high.
                        _pipeClientStream.Connect(10);
                    }
                    catch (Exception) { }
                    Thread.Sleep(5000);
                }

                //Make sure the stream's readmode is message, not sure why.
                _pipeClientStream.ReadMode = PipeTransmissionMode.Message;

                //Start a new async read with a hefty buffer, using the buffer as the stateobject
                byte[] readBuffer = new byte[SIZE_BUFFER];
                _pipeClientStream.BeginRead(readBuffer, 0, SIZE_BUFFER, new AsyncCallback(this._endRead), readBuffer);

                //We're connected, set the manual reset event to allow waiting threads to continue
                _connectionGate.Set();
                //Send any queued messages
                _sendQueuedMessages();
            }
        }

        /// <summary>
        /// End an asynchronous read operation, fire off the StateObjectReceived event if we read
        /// the whole object, and start a new asynchronous read operation.
        /// </summary>
        /// <param name="iAsyncResult"></param>
        private void _endRead(IAsyncResult iAsyncResult)
        {
            //Get the length of everything read in the stream, blocking 'til we're done reading
            int length = _pipeClientStream.EndRead(iAsyncResult);
            //Get the buffer from the async result
            byte[] previousBuffer = (byte[])iAsyncResult.AsyncState;

            //If we've read -something-
            if (length > 0)
            {
                //copy the start buffer into the new buffer
                byte[] endBuffer = new byte[length];
                Array.Copy(previousBuffer, 0, endBuffer, 0, length);

                //Woohoo. We just received a state object.
                using (MemoryStream memStream = new MemoryStream(endBuffer))
                {
                    //Deserialize it and fire the event if possible
                    UiStateObject stateObject = Serializer.Deserialize<UiStateObject>(memStream);
                    if (UiStateObjectReceived != null)
                        UiStateObjectReceived(this, new UiStateObjectEventArgs(stateObject));
                }
            }

            //Get the connection lock and start another asynch read
            lock (_connectionLock)
            {
                //Sanitize the read buffer.
                previousBuffer = new byte[SIZE_BUFFER];
                _pipeClientStream.BeginRead(previousBuffer, 0, SIZE_BUFFER, new AsyncCallback(this._endRead), previousBuffer);
            }
        }

        /// <summary>
        /// If connected, send a message asynchronously. Otherwise queue it for sending when connected.
        /// </summary>
        /// <param name="message"></param>
        private void _sendMessage(byte[] message)
        {
            //If we're not waiting on connect...
            if (_connectionGate.WaitOne(100))
            {
                //Get the lock
                lock (_connectionLock)
                {
                    //If the connection is connected, start an async write and flush so it does the write
                    if (_pipeClientStream != null &&
                        _pipeClientStream.IsConnected)
                    {
                        _pipeClientStream.BeginWrite(message, 0, message.Length, new AsyncCallback(this._endSendMessage), null);
                    }
                    //Otherwise just enqueue the message for sending and start trying to connect
                    else
                    {
                        _serializedMessages.Enqueue(message);
                        _startTryConnect();
                    }
                }
            }
            else
            {
                //Timed out waiting on connection, enqueue the message since it's already trying to connect
                _serializedMessages.Enqueue(message);
            }
        }

        /// <summary>
        /// End the sending of a message asynchronously and flush the stream.
        /// </summary>
        /// <param name="iAsyncResult"></param>
        private void _endSendMessage(IAsyncResult iAsyncResult)
        {
            //Get the connection lock
            lock (_connectionLock)
            {
                _pipeClientStream.EndWrite(iAsyncResult);
                _pipeClientStream.Flush();
            }
        }

        /// <summary>
        /// Enqueue a message for later sending.
        /// </summary>
        /// <param name="message"></param>
        private void _enqueueMessage(byte[] message)
        {
            //Get the queue lock and queue the message
            lock (_queueLock)
            {
                _serializedMessages.Enqueue(message);
            }
        }

        /// <summary>
        /// Send any queued messages.
        /// </summary>
        private void _sendQueuedMessages()
        {
            //Get the queue lock
            lock (_queueLock)
            {
                //Send any queued messages
                while (_serializedMessages.Count > 0)
                {
                    _sendMessage(_serializedMessages.Dequeue());
                }
            }
        }
    }
}
