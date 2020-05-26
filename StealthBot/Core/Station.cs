using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EVE.ISXEVE;
using EVE.ISXEVE.Interfaces;
using LavishScriptAPI;
using StealthBot.Core.Interfaces;

namespace StealthBot.Core
{
    internal class Station : ModuleBase, IStation
    {
        private readonly List<int> _stationTypeIds; 
        public ReadOnlyCollection<int> StationTypeIDs
        {
            get { return _stationTypeIds.AsReadOnly(); }
        }

        private readonly IIsxeveProvider _isxeveProvider;
        private readonly IEveWindowProvider _eveWindowProvider;

        internal Station(IIsxeveProvider isxeveProvider, IEveWindowProvider eveWindowProvider)
        {
            _isxeveProvider = isxeveProvider;
            _eveWindowProvider = eveWindowProvider;

            IsEnabled = false;
            ModuleName = "Station";

            _stationTypeIds = new List<int>
                {
                    (int) TypeIDs.Concord_Starbase,
                    (int) TypeIDs.Minmatar_Trade_Post,
                    (int) TypeIDs.Amarr_Trade_Post,
                    (int) TypeIDs.Minmatar_Hub
                };
        }

        public bool IsDockedAtStation(long stationId)
        {
            var methodName = "IsDockedAtStation";
			LogTrace(methodName, "StationID: {0}", stationId);
            
			return !StealthBot.MeCache.InSpace && StealthBot.MeCache.InStation && StealthBot.MeCache.StationId == stationId;
        }

        /// <summary>
        /// Undock from the current station.
        /// </summary>
        public void Undock()
        {
            var methodName = "Undock";
			LogTrace(methodName);

            //StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
            //    "Undock", String.Format("Undocking from station {0}", Core.MeCache.Me.Station.Name)));
            StealthBot.Config.MovementConfig.HomeStation = StealthBot.MeCache.Me.Station.Name;
            _isxeveProvider.Eve.Execute(ExecuteCommand.CmdExitStation);
        }

        public bool IsStationHangarActive
        {
            get
            {
				if (!StealthBot.Ship.IsInventoryOpen) return false;

                var inventoryWindow = _eveWindowProvider.GetInventoryWindow();
				if (LavishScriptObject.IsNullOrInvalid(inventoryWindow)) return false;

            	var activeChild = inventoryWindow.ActiveChild;
				if (LavishScriptObject.IsNullOrInvalid(activeChild)) return false;

				return activeChild.ItemId == StealthBot.MeCache.StationId && activeChild.Name == "StationItems";
            }
        }

		public void MakeStationHangarActive()
		{
			var methodName = "MakeStationHangarActive";
			LogTrace(methodName);

			if (!StealthBot.Ship.IsInventoryOpen) return;

			var childWindow = GetStationItemsChildWindow();
			if (LavishScriptObject.IsNullOrInvalid(childWindow)) return;

			childWindow.MakeActive();
		}

    	private IEveInvChildWindow GetStationItemsChildWindow()
    	{
    		var methodName = "GetStationItemsChildWindow";
    		LogTrace(methodName);

            var inventoryWindow = _eveWindowProvider.GetInventoryWindow();
    		if (LavishScriptObject.IsNullOrInvalid(inventoryWindow)) return null;

    		var childWindow = inventoryWindow.GetChildWindow(StealthBot.MeCache.StationId, "StationItems");
    		if (LavishScriptObject.IsNullOrInvalid(childWindow))
    		{
    			LogMessage(methodName, LogSeverityTypes.Debug, "Error: Child window for ID {0}, Name StationItems was null or invalid.", StealthBot.MeCache.StationId);
    			return null;
    		}

    		return childWindow;
    	}

		private IEveInvChildWindow GetStationCorpHangarChildWindow()
		{
			var methodName = "GetStationCorpHangarChildWindow";
			LogTrace(methodName);

            var inventoryWindow = _eveWindowProvider.GetInventoryWindow();
			if (LavishScriptObject.IsNullOrInvalid(inventoryWindow)) return null;

			var folder = String.Format("Folder{0}", StealthBot.Config.CargoConfig.DropoffLocation.HangarDivision);
			var childWindow = inventoryWindow.GetChildWindow("StationCorpHangar", folder);
			if (LavishScriptObject.IsNullOrInvalid(childWindow))
			{
				LogMessage(methodName, LogSeverityTypes.Debug, "Error: Child window for Name StationCorpHangar, Location {0} was null or invalid.", folder);
				return null;
			}

			return childWindow;
		}

		private IEveInvChildWindow GetStationCorpHangarsContainerChildWindow()
		{
			var methodName = "GetStationCorpHangarContainerChildWindow";
			LogTrace(methodName);

            var inventoryWindow = _eveWindowProvider.GetInventoryWindow();
			if (LavishScriptObject.IsNullOrInvalid(inventoryWindow)) return null;

			var childWindow = inventoryWindow.GetChildWindow("StationCorpHangars");
			if (LavishScriptObject.IsNullOrInvalid(childWindow))
			{
				LogMessage(methodName, LogSeverityTypes.Debug, "Error: Child window for station Name StationCorpHangars was null.");
			}

			return childWindow;
		}

    	public bool IsStationCorpHangarActive
		{
			get
			{
                var inventoryWindow = _eveWindowProvider.GetInventoryWindow();
				if (LavishScriptObject.IsNullOrInvalid(inventoryWindow)) return false;

				var activeChild = inventoryWindow.ActiveChild;
				if (LavishScriptObject.IsNullOrInvalid(activeChild)) return false;

				return activeChild.ItemId == StealthBot.MeCache.StationId && activeChild.Name == "StationCorpHangar";
			}
		}

    	public void MakeStationCorpHangarActive()
    	{
    		var childWindow = GetStationCorpHangarChildWindow();

			if (LavishScriptObject.IsNullOrInvalid(childWindow)) return;

    		childWindow.MakeActive();
    	}

		public void InitializeStationCorpHangars()
		{
			var childWindow = GetStationCorpHangarsContainerChildWindow();

			if (LavishScriptObject.IsNullOrInvalid(childWindow)) return;

			childWindow.MakeActive();
		}
    }
}
