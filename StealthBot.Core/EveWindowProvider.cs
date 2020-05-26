using System;
using EVE.ISXEVE;
using EVE.ISXEVE.Interfaces;
using StealthBot.Core.Interfaces;

namespace StealthBot.Core
{
    public class EveWindowProvider : IEveWindowProvider
    {
        public IEveInvWindow GetInventoryWindow()
        {
            return EVEWindow.GetInventoryWindow();
        }

        public EVEWindow GetWindowByItemId(Int64 itemId)
        {
            return EVEWindow.GetWindowByItemId(itemId);
        }
        
        public EVEWindow GetWindowByName(string name)
        {
            return EVEWindow.GetWindowByName(name);
        }

        public EVEWindow GetWindowByCaption(string caption)
        {
            return EVEWindow.GetWindowByCaption(caption);
        }
    }
}
