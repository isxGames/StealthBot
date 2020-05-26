using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVE.ISXEVE;
using StealthBot.ActionModules;
using StealthBot.Core.Extensions;
using StealthBot.Core.Interfaces;

namespace StealthBot.Core
{
    public class ShipHub : ModuleBase
    {

        public ShipHub()
        {
            IsEnabled = false;
            ModuleManager.Modules.Add(this);
        }

        public bool IsAtShipHub
        {
            get
            {
                var methodName = "IsAtShipHub";
                LogTrace(methodName);

                if (string.IsNullOrEmpty(StealthBot.Config.ShipHubConfig.HubLocation.BookmarkLabel))
                {
                    LogMessage(methodName, LogSeverityTypes.Standard, "Error: ShipHub BookmarkLabel is not set.");
                    return false;
                }

                var hubBookmark = GetShipHubBookmark();

                if (hubBookmark == null)
                {
                    LogMessage(methodName, LogSeverityTypes.Standard, "Error: Could not find bookmark for ShipHub BookmarkLabel \"{0}\".",
                        StealthBot.Config.ShipHubConfig.HubLocation.BookmarkLabel);
                    return false;
                }

                return StealthBot.Bookmarks.IsAtBookmark(hubBookmark);
            }
        }

        public void MoveToShipHub()
        {
            var methodName = "MoveToShipHub";
            LogTrace(methodName);

            var shipHubBookmark = GetShipHubBookmark();

            if (shipHubBookmark == null)
            {
                LogMessage(methodName, LogSeverityTypes.Standard, "Error: Could not find bookmark for ShipHub BookmarkLabel \"{0}\".",
                    StealthBot.Config.ShipHubConfig.HubLocation.BookmarkLabel);
                return;
            }

            var destination = new Destination(DestinationTypes.BookMark, shipHubBookmark.Id);
            StealthBot.Movement.QueueDestination(destination);
        }

        public void ChangeShip(string shipName)
        {
            var methodName = "ChangeShip";
            LogTrace(methodName);

            var matchingShip = StealthBot.MeCache.HangarShips.FirstOrDefault(ship => ship.GivenName.Contains(shipName, StringComparison.InvariantCultureIgnoreCase));

            if (matchingShip == null)
            {
                LogMessage(methodName, LogSeverityTypes.Standard, "Error: Could not find a ship with a given name of \"{0}\".", shipName);
                return;
            }

            LogMessage(methodName, LogSeverityTypes.Standard, "Making ship \"{0}\" active.", matchingShip.GivenName);
            matchingShip.MakeActive();

            StealthBot.ModuleManager.DelayPulseByTicks(5);
        }

        private CachedBookMark GetShipHubBookmark()
        {
            var methodName = "GetShipHubBookmark";
            LogTrace(methodName);

            return StealthBot.BookMarkCache.FirstBookMarkMatching(StealthBot.Config.ShipHubConfig.HubLocation.BookmarkLabel, false);
        }
    }
}
