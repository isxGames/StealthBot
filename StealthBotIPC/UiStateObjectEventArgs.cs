using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StealthBotIpc
{
    public class UiStateObjectEventArgs : EventArgs
    {
        public UiStateObject UiStateObject;

        public UiStateObjectEventArgs(UiStateObject sbStateObject)
        {
            UiStateObject = sbStateObject;
        }
    }
}
