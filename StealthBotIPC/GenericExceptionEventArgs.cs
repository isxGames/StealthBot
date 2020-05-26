using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StealthBotIpc
{
    public class GenericExceptionEventArgs : EventArgs
    {
        public List<string> ExceptionDetails;

        public GenericExceptionEventArgs(List<string> exceptionDetails)
        {
            ExceptionDetails = exceptionDetails;
        }
    }
}
