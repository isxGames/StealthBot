using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;

namespace StealthBotIpc
{
    public class SbStateObjectEventArgs : EventArgs
    {
        public SbStateObject SbStateObject;
        public NamedPipeServerStream SendingStream;

        public SbStateObjectEventArgs(SbStateObject sbStateObject, NamedPipeServerStream sendingStream)
        {
            SbStateObject = sbStateObject;
            SendingStream = sendingStream;
        }
    }
}
