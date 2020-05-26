using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LavishScriptAPI;
using StealthBot.Core.Config;
using StealthBotIpc;
using StealthBot.BehaviorModules;
using StealthBot.ActionModules;

namespace StealthBot.Core
{
    public sealed class SbUiCommunication : ModuleBase
    {
        //IPC client for communication
    	readonly SbIpcClient _sbIpcClient = new SbIpcClient();

        //List of received UiStateObjects containing any commands we need to process
    	readonly List<UiStateObject> _receivedStateObjects = new List<UiStateObject>();

        public SbUiCommunication()
        {
            //Attach the event handler for queueing received UiStateObjects
            _sbIpcClient.UiStateObjectReceived += _sbIpcClient_UiStateObjectReceived;
            
            //Module should be enabled
            IsEnabled = true;
            //Pulse freq of 1
            PulseFrequency = 1;
            //Set the object name
            ModuleName = "SbUiCommunication";
        }

        private void _sbIpcClient_UiStateObjectReceived(object sender, UiStateObjectEventArgs e)
        {
            lock (_receivedStateObjects)
            {
                _receivedStateObjects.Add(e.UiStateObject);
            }
        }

        /// <summary>
        /// Get the list of received UiStateObjects
        /// </summary>
        public List<UiStateObject> ReceivedStateObjects
        {
            get
            {
                lock (_receivedStateObjects)
                {
                    return _receivedStateObjects;
                }
            }
        }

        public override void Pulse()
        {
            var methodName = "Pulse";
			LogTrace(methodName);

        	if (!ShouldPulse()) 
				return;

            //if (StealthBot.Config.MainConfig.EnableUiIpc)
            //{
            //    SendStateObject();
            //}
        }

        public override bool OutOfFrameCleanup()
        {
            var methodName = "OutOfFrameCleanup";
			LogTrace(methodName);

            if (!IsCleanedUpOutOfFrame)
            {
                _sbIpcClient.Dispose();
                IsCleanedUpOutOfFrame = true;
            }

            return IsCleanedUpOutOfFrame;
        }

        private void SendStateObject()
        {
            var methodName = "SendStateObject";
			LogTrace(methodName);

            //Parse out the session name
            var sessionName = string.Empty;
            LavishScript.DataParse("${Session}", ref sessionName);

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "Post-Session"));

            //Build and send the state object
            //Get our ship type if we can, otherwise just use the typeID
            var shipType = string.Empty;
            shipType = Enum.IsDefined(typeof(TypeIDs), StealthBot.MeCache.ToEntity.TypeId) ?
				((TypeIDs)StealthBot.MeCache.ToEntity.TypeId).ToString() : StealthBot.MeCache.ToEntity.TypeId.ToString();

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "Ship type"));

            //Instantiate a new state object to work with
            var stateObject = new SbStateObject(
               sessionName, StealthBot.MeCache.Name, false, StealthBot.MeCache.Ship.ShieldPct, StealthBot.MeCache.Ship.ArmorPct,
               StealthBot.MeCache.Ship.StructurePct, StealthBot.MeCache.Ship.CapacitorPct, StealthBot.MeCache.Ship.Name,
               shipType, StealthBot.Instance.RunTime.Elapsed.ToString());

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "Post-instantiate"));

            //Get recent log messages
            /*List<string> recentMessages;
            lock (Core.StealthBot.Logging.RecentMessages)
            {
                recentMessages = new List<string>(Core.StealthBot.Logging.RecentMessages);
                Core.StealthBot.Logging.RecentMessages.Clear();
            }
            stateObject.LogMessages = recentMessages;
			 */

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "Recent messages"));

            //Get asteroids in range
            stateObject.AsteroidsInRange = new List<string>();
            foreach (var asteroid in StealthBot.AsteroidBelts.AsteroidsInRange)
            {
                stateObject.AsteroidsInRange.Add(string.Format("{0} {1} {2}", asteroid.Distance, asteroid.Name, asteroid.ID));
            }

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "Asteroids in range"));

            //Get asteroids out of range
            stateObject.AsteroidsOutOfRange = new List<string>();
            foreach (var asteroid in StealthBot.AsteroidBelts.AsteroidsOutOfRange)
            {
				stateObject.AsteroidsOutOfRange.Add(string.Format("{0} {1} {2}", asteroid.Distance, asteroid.Name, asteroid.ID));
            }

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "Asteroids out of range"));

            //Get everything in the target queue
            stateObject.QueuedTargets = new List<string>();
            foreach (var queueTarget in StealthBot.TargetQueue.Targets)
            {
                if (StealthBot.EntityProvider.EntityWrappersById.ContainsKey(queueTarget.Id))
                {
                    var entity = Core.StealthBot.EntityProvider.EntityWrappersById[queueTarget.Id];
                    stateObject.QueuedTargets.Add(string.Format("{0} {1} {2} {3} {4}", entity.Distance, entity.Name, entity.ID,
                        queueTarget.Priority, queueTarget.SubPriority));
                }
                else
                {
                    stateObject.QueuedTargets.Add(string.Format("{0} - Invalid Entity", queueTarget.Id));
                }
            }

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "Queued targets"));

            //Get all locked targets
            stateObject.LockedTargets = new List<string>();
            foreach (var entity in StealthBot.MeCache.Targets)
            {
                stateObject.LockedTargets.Add(string.Format("{0} {1} {2}", entity.Distance, entity.Name, entity.ID));
            }

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "Entities"));

            //Get everything in the destination queue
            stateObject.DestinationQueue = new List<string>();
            foreach (var destination in StealthBot.Movement.DestinationQueue)
            {
                var formattedString = destination.Type.ToString();
                switch (destination.Type)
                {
                    case DestinationTypes.BookMark:
                        formattedString = string.Format("{0} {1} {2}", formattedString, destination.BookMarkId, destination.SolarSystemId);
                        break;
                    case DestinationTypes.Celestial:
                        var entityName = "NOT ON GRID";
                        if (Core.StealthBot.EntityProvider.EntityWrappersById.ContainsKey(destination.EntityId))
                        {
                            entityName = StealthBot.EntityProvider.EntityWrappersById[destination.EntityId].Name;
                        }
                        formattedString = string.Format("{0} {1} {2} {3}", formattedString, entityName, destination.EntityId, destination.Distance);
                        break;
                    case DestinationTypes.Entity:
                        goto case DestinationTypes.Celestial;
                    case DestinationTypes.FleetMember:
                        formattedString = string.Format("{0} {1} {2} {3}", formattedString, destination.FleetMemberName, destination.FleetMemberId,
                            destination.FleetMemberEntityId);
                        break;
                    case DestinationTypes.SolarSystem:
                        formattedString = string.Format("{0} {1}", formattedString, destination.SolarSystemId);
                        break;
                    case DestinationTypes.Undock:
                        formattedString = string.Format("{0} Undock", formattedString);
                        break;
                }
                stateObject.DestinationQueue.Add(formattedString);
            }

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "desti queue"));

            //Get all threats
            stateObject.Threats = new List<string>();
            foreach (var entity in StealthBot.Attackers.ThreatEntities)
            {
                stateObject.Threats.Add(string.Format("{0} {1} {2}", entity.Distance, entity.Name, entity.ID));
            }

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "threats"));

            //Get statistical data
            stateObject.ItemsMined_Moved = new Dictionary<string, int>(StealthBot.Statistics.MinedItemsMovedByItemName);
            stateObject.Ammo_CrystalsUsed = new Dictionary<string, int>(StealthBot.Statistics.QuantityChargesUsedByName);

            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "dicts"));

            //Send configuration profiles too
            stateObject.ConfigurationProfiles = new Dictionary<string, Dictionary<string, ConfigProperty>>(StealthBot.ConfigurationManager.ConfigProfilesByName);

            //Transmit the state object we've built
            _sbIpcClient.SendSbStateObject(stateObject);
            //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                //methodName, "send"));
        }
    }
}
