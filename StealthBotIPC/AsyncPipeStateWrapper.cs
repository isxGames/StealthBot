using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;

namespace StealthBotIpc
{
    public class AsyncPipeStateWrapper
    {
        public NamedPipeServerStream NamedPipeServerStream;
        public byte[] Buffer;

        public AsyncPipeStateWrapper(NamedPipeServerStream namedPipeServerStream, byte[] buffer)
        {
            NamedPipeServerStream = namedPipeServerStream;
            Buffer = buffer;
        }
    }


}
