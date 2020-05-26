using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;

namespace StealthBotIpc
{
    public class NamedPipeServerStreamEventArgs : EventArgs
    {
        public NamedPipeServerStream NamedPipeServerStream;

        public NamedPipeServerStreamEventArgs(NamedPipeServerStream namedPipeServerStream)
        {
            NamedPipeServerStream = namedPipeServerStream;
        }
    }
}
