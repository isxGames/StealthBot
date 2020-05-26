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
    /// <summary>
    /// Provides the IPC Server object for SBUI to SB communication.
    /// </summary>
    public sealed class SbIpcServer : IDisposable
    {
        static readonly string SB_PIPE_NAME = "sbUiServerPipe";
        static readonly int SIZE_BUFFER = 65536;

        public event EventHandler<SbStateObjectEventArgs> SbStateObjectReceived;
        public event EventHandler<GenericExceptionEventArgs> ExceptionThrown;

        private List<NamedPipeServerStream> _connectedPipeStreams = new List<NamedPipeServerStream>();
        private Dictionary<string, NamedPipeServerStream> _sessions_namedPipes = new Dictionary<string, NamedPipeServerStream>();

        private bool _isDisposed = false;

        #region Constructor/deconstructor
        public SbIpcServer()
        {
            SbStateObjectReceived += new EventHandler<SbStateObjectEventArgs>(SbIpcServer_SbStateObjectReceived);
            NamedPipeServerStream connection = new NamedPipeServerStream(SB_PIPE_NAME, PipeDirection.InOut, -1,
                PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            connection.BeginWaitForConnection(new AsyncCallback(this._pipeConnected), new AsyncPipeStateWrapper(connection, null));
        }

        ~SbIpcServer()
        {
            _dispose(true);
        }
        #endregion

        #region IDisposable stuff
        /// <summary>
        /// Clean up the instance, release any resources.
        /// </summary>
        /// <param name="disposing"></param>
        private void _dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                //flag isDisposed
                _isDisposed = true;

                //if we're calling from the deconsructor do special stuff
                if (disposing)
                {
                    //Nothing special to do
                }

                //Clear the dictionary of pipes/sizes and list of pipes
                foreach (NamedPipeServerStream stream in _connectedPipeStreams)
                {
                    stream.Close();
                }
                _connectedPipeStreams.Clear();
                _sessions_namedPipes.Clear();
                
            }
        }

        /// <summary>
        /// Clean up the instance, release any resources.
        /// </summary>
        public void Dispose()
        {
            //No need to deconstruct if we're disposing
            GC.SuppressFinalize(this);
            _dispose(false);
        }
        #endregion

        #region Event Handlers
        void SbIpcServer_SbStateObjectReceived(object sender, SbStateObjectEventArgs e)
        {
            //If no charname ad it to the dictionary using session, otherwise use charname
            if (e.SbStateObject.CharacterName == string.Empty)
            {
                if (!_sessions_namedPipes.ContainsKey(e.SbStateObject.SessionName))
                {
                    _sessions_namedPipes.Add(e.SbStateObject.SessionName, e.SendingStream);
                }
            }
            else
            {
                //Check if I've already got it set by session name, if so remove it
                if (!_sessions_namedPipes.ContainsKey(e.SbStateObject.CharacterName))
                {
                    if (_sessions_namedPipes.ContainsKey(e.SbStateObject.SessionName))
                    {
                        _sessions_namedPipes.Remove(e.SbStateObject.SessionName);
                    }

                    _sessions_namedPipes.Add(e.SbStateObject.CharacterName, e.SendingStream);
                }
            }
        }
        #endregion

        /// <summary>
        /// Handle a new connection on the server stream. End the waitfor connection and restart waiting on a new serverstream.
        /// </summary>
        /// <param name="iAsyncResult"></param>
        private void _pipeConnected(IAsyncResult iAsyncResult)
        {
            //Get the connecting client from the asyncstate and end the connection process
            AsyncPipeStateWrapper wrapper = iAsyncResult.AsyncState as AsyncPipeStateWrapper;
            NamedPipeServerStream connectingClient = wrapper.NamedPipeServerStream;
            connectingClient.EndWaitForConnection(iAsyncResult);

            //Double-check the damn connection is actually connected
            if (connectingClient.IsConnected)
            {
                //Lock the connecting client and start a read on it
                byte[] readBuffer = new byte[SIZE_BUFFER];
                lock (connectingClient)
                {
                    connectingClient.BeginRead(readBuffer, 0, SIZE_BUFFER,
                        new AsyncCallback(_endRead), new AsyncPipeStateWrapper(connectingClient, readBuffer));
                }
                //The namedPipeServerStream BECOMES the connection object after it connects.
                //Lock the list to avoid thread issues
                lock (_connectedPipeStreams)
                {
                    _connectedPipeStreams.Add(connectingClient);
                }
            }

            //Restart the pipe waiting for connection
            NamedPipeServerStream newConnection = new NamedPipeServerStream(SB_PIPE_NAME, PipeDirection.InOut, -1, 
                PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            newConnection.BeginWaitForConnection(new AsyncCallback(this._pipeConnected), new AsyncPipeStateWrapper(newConnection, null));
        }

        /// <summary>
        /// End the asynchronous read operation on a connected named pipe.
        /// </summary>
        /// <param name="iAsyncResult"></param>
        private void _endRead(IAsyncResult iAsyncResult)
        {
            //Get the instance of the connection we're ending for
            AsyncPipeStateWrapper wrapper = iAsyncResult.AsyncState as AsyncPipeStateWrapper;
            NamedPipeServerStream connection = wrapper.NamedPipeServerStream;
            if (connection != null && connection.IsConnected)
            {
                try
                {
                    //Get the length read
                    int length = connection.EndRead(iAsyncResult);
                    byte[] readBuffer = wrapper.Buffer;

                    //If we have somethign read...
                    if (length > 0)
                    {
                        byte[] destinationArray = new byte[length];
                        Array.Copy(readBuffer, 0, destinationArray, 0, length);
                        //Deserialize the read object
                        using (MemoryStream memStream = new MemoryStream(readBuffer))
                        {
                            //Still figuring out why this excepts
                            try
                            {
                                SbStateObject stateObject = Serializer.Deserialize<SbStateObject>(memStream);
                                //Cool, we have ourselves our very own SbStateObject. Fire the event.
                                if (SbStateObjectReceived != null)
                                    SbStateObjectReceived(this, new SbStateObjectEventArgs(stateObject, connection));
                            }
                            catch (ProtoException) { }
                        }
                    }
                    //lock the connection and begin reading
                    lock (connection)
                    {
                        //Sanitize the read buffer. FFS, examples...
                        readBuffer = new byte[SIZE_BUFFER];
                        connection.BeginRead(readBuffer, 0, SIZE_BUFFER,
                            new AsyncCallback(this._endRead), new AsyncPipeStateWrapper(connection, readBuffer));
                    }
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Send a UiStateObject to a specified session via named pipe.
        /// </summary>
        /// <param name="uiStateObject"></param>
        public void SendUiStateObject(string session, UiStateObject uiStateObject)
        {
            //If we've got a connection for the passed session and it's an active connection...
            if (_sessions_namedPipes.ContainsKey(session))
            {
                //Get lock object on the connection
                NamedPipeServerStream connection = _sessions_namedPipes[session];
                lock (connection)
                {
                    //Check if it's connected, if so begin the write.
                    if (connection.IsConnected)
                    {
                        byte[] message;
                        //Get the bytes from the object...
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            Serializer.Serialize<UiStateObject>(memStream, uiStateObject);
                            message = memStream.ToArray();
                        }

                        //Start the write
                        connection.BeginWrite(message, 0, message.Length,
                            new AsyncCallback(_endWrite), new AsyncPipeStateWrapper(connection, null));
                    }
                }
            }
        }

        /// <summary>
        /// End an asynchronous write operation and flush the stream written to.
        /// </summary>
        /// <param name="iAsyncResult"></param>
        private void _endWrite(IAsyncResult iAsyncResult)
        {
            AsyncPipeStateWrapper wrapper = iAsyncResult.AsyncState as AsyncPipeStateWrapper;
            NamedPipeServerStream connection = wrapper.NamedPipeServerStream;

            //Lock on the connection
            lock (connection)
            {
                //End the write operation and flush.
                connection.EndWrite(iAsyncResult);
                connection.Flush();
            }
        }
    }
}
