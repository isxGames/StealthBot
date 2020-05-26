using System;   
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StealthBot.Core.Config;
using StealthBot.Core.EventCommunication;
using StealthBot.Core.Interfaces;

namespace StealthBot.Core
{
    public sealed class TargetQueue : ModuleBase, ITargetQueue
    {
        private EventHandler<FleetNeedCombatAssistEventArgs> _fleetNeedCombatAssist;
        private readonly List<FleetNeedCombatAssistEventArgs> _fleetNeedCombatAssistEventArgsToValidate = new List<FleetNeedCombatAssistEventArgs>(); 

        private readonly List<QueueTarget> _targets = new List<QueueTarget>(); 

        public ReadOnlyCollection<QueueTarget> Targets
        {
            get { return _targets.AsReadOnly(); }
        }

        private readonly IMeCache _meCache;
        private readonly IEntityProvider _entityProvider;
        private readonly IMiningConfiguration _miningConfiguration;
        private readonly IMainConfiguration _mainConfiguration;

        public TargetQueue(IMeCache meCache, IEntityProvider entityProvider, IMiningConfiguration miningConfiguration, IMainConfiguration mainConfiguration)
        {
			IsEnabled = true;
			ModuleName = "TargetQueue";
			PulseFrequency = 1;
            ModuleManager.ModulesToPulse.Add(this);

            _meCache = meCache;
            _entityProvider = entityProvider;
            _miningConfiguration = miningConfiguration;
            _mainConfiguration = mainConfiguration;
        }

        public override bool Initialize()
        {
            _fleetNeedCombatAssist = OnFleetNeedCombatAssist;
            StealthBot.EventCommunications.FleetNeedCombatAssistEvent.EventRaised += _fleetNeedCombatAssist;

            IsInitialized = true;
            return IsInitialized;
        }

        public override bool OutOfFrameCleanup()
        {
            if (StealthBot.EventCommunications.FleetNeedCombatAssistEvent != null)
// ReSharper disable DelegateSubtraction
                StealthBot.EventCommunications.FleetNeedCombatAssistEvent.EventRaised -= _fleetNeedCombatAssist;
// ReSharper restore DelegateSubtraction

            _fleetNeedCombatAssist = null;

            IsCleanedUpOutOfFrame = true;
            return IsCleanedUpOutOfFrame;
        }

        public override void Pulse()
		{
            var methodName = "TargetQueue";
			LogTrace(methodName);

			if (!ShouldPulse()) 
				return;

			StartPulseProfiling();

            PruneQueue();

            ValidateFleetNeedCombatAssistEventArgs();

			EndPulseProfiling();
		}

        private void PruneQueue()
        {
            var methodName = "PruneQueue";
			LogTrace(methodName);

			//Since the MasterCache now holds cached versions of -all- valid entities, check if our QueueTarget is
			//in the cache. If it isn't, it isn't a valid entity and we can remove it.
            for (var index = 0; index < _targets.Count; index++)
            {
                //Core.StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
				//	"PruneQueue", String.Format("Checking entity {0} ({1}), distance {2}",
				//	tempEntity.Name, tempEntity.ID, tempEntity._distance)));
                var queueTarget = _targets[index];
                if (!_entityProvider.EntityWrappersById.ContainsKey(queueTarget.Id) ||
                    _entityProvider.EntityWrappersById[queueTarget.Id].Distance >= (int)Ranges.Warp * 0.95)
                {
                    //StealthBot.Logging.LogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                    //    "PruneQueue", String.Format("Removing QueueTarget {0}",
                    //    Targets[idx].ID)));
                    _targets.RemoveAt(index);
                    index--;
                }
            }
        }

        public bool IsQueued(Int64 entityId)
        {
            var methodName = "IsQueued";
			LogTrace(methodName);

            return _targets.Any(target => target.Id == entityId);
        }

        /// <summary>
        /// Enqueue a target.
        /// </summary>
        /// <param name="entityId">Target's ID</param>
        /// <param name="priority">Priority of the target, lower number is higher priority</param>
        /// <param name="type">Type of the target, determines what handles it</param>
        public void EnqueueTarget(Int64 entityId, int priority, TargetTypes type)
        {
            EnqueueTarget(entityId, priority, 0, type);
        }

        /// <summary>
        /// Enqueue a target.
        /// </summary>
        /// <param name="entityId">Target's ID</param>
        /// <param name="priority">Priority of the target, lower number is higher priority</param>
        /// <param name="subPriority">Sub-priority for sorting</param>
        /// <param name="type">Type of the target, determines what handles it</param>
        public void EnqueueTarget(Int64 entityId, int priority, int subPriority, TargetTypes type)
        {
            var methodName = "EnqueueTarget";
			LogTrace(methodName, "{0},{1},{2},{3}", entityId, priority, subPriority, type);

            if (!IsQueued(entityId))
            {
				LogMessage(methodName, LogSeverityTypes.Debug, "Queueing entity with id {0}, priority {1}, sub priority {2}, type {3}",
					entityId, priority, subPriority, type);
                _targets.Add(new QueueTarget(entityId, priority, subPriority, type));
            }
            else
            {
                var existingTarget = _targets.FirstOrDefault(queueTarget => queueTarget.Id == entityId);

            	if (existingTarget == null) return;

            	var shouldUpdate = false;

            	if (priority < existingTarget.Priority)
            	{
            		LogMessage(methodName, LogSeverityTypes.Debug, "Queue target {0} has increased in priority to {1}.",
            		           existingTarget.Id, priority);
            		shouldUpdate = true;
            	}
            	else if (priority == existingTarget.Priority && subPriority < existingTarget.SubPriority)
            	{
            		LogMessage(methodName, LogSeverityTypes.Debug, "Queue target {0} has increased in subpriority to {1}.",
            		           existingTarget.Id, subPriority);
            		shouldUpdate = true;
            	}
            	else if (type != existingTarget.Type)
            	{
            		LogMessage(methodName, LogSeverityTypes.Debug, "Queue target {0} has changed type to {1}.",
            		           existingTarget.Id, type);
            		shouldUpdate = true;
            	}

            	if (shouldUpdate)
            	{
                    existingTarget.UpdateTarget(priority, subPriority, type);
            	}
            }
        }

        public void DequeueTarget(Int64 entityId)
        {
            var methodName = "DequeueTarget";
			LogTrace(methodName, "Entity: {0}", entityId);

            for (var index = 0; index < _targets.Count; index++)
            {
                if (_targets[index].Id != entityId) 
					continue;

                _targets.RemoveAt(index);
            	return;
            }
        }

        /// <summary>
        /// Handle the FleetNeedCombatAssist event. Queue the eventargs for in-pulse validation.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see>
        ///                         <cref>StealthBot.Core.EventCommunication.FleetNeedCombatAssistEventArgs</cref>
        ///                     </see>
        ///     instance containing the event data.</param>
        private void OnFleetNeedCombatAssist(object sender, FleetNeedCombatAssistEventArgs e)
        {
            var methodName = "OnFleetNeedCombatAssist";
            LogTrace(methodName, "Sender: {0}, FleetMember: {1}, SolarSystem: {2}, Target: {3}",
                     sender, e.SendingFleetMemberCharId, e.SolarSystemId, e.Target.Id);

            lock (_fleetNeedCombatAssistEventArgsToValidate)
            {
                if (!_fleetNeedCombatAssistEventArgsToValidate.Contains(e))
                {
                    _fleetNeedCombatAssistEventArgsToValidate.Add(e);
                }
            }
        }

        /// <summary>
        /// Validates any fleet combat assistance requests and if valid, queues the requested target.
        /// </summary>
        private void ValidateFleetNeedCombatAssistEventArgs()
        {
            var methodName = "ValidateFleetNeedCombatAssistEventArgs";
            LogTrace(methodName);

            lock (_fleetNeedCombatAssistEventArgsToValidate)
            {
                if (!_fleetNeedCombatAssistEventArgsToValidate.Any())
                    return;

                var inCombatCapableMode = InCombatCapableMode();

                if (!inCombatCapableMode)
                {
                    //LogMessage(methodName, LogSeverityTypes.Debug, "Not in a combat-capable mode - ignoring combat assist requests.");
                    return;
                }
                
                foreach (var eventArgs in _fleetNeedCombatAssistEventArgsToValidate)
                {
                    if (_meCache.SolarSystemId != eventArgs.SolarSystemId)
                    {
                        //LogMessage(methodName, LogSeverityTypes.Debug, "Combat assist request from player {0} is invalid - request solar system {1}, current solar system {2}.",
                        //    eventArgs.SendingFleetMemberId, eventArgs.SolarSystemId, _meCache.SolarSystemId);
                        continue;
                    }

                    if (!_entityProvider.EntityWrappersById.ContainsKey(eventArgs.Target.Id))
                    {
                        //LogMessage(methodName, LogSeverityTypes.Debug, "Combat assist request from player {0} is invalid - requested entity ID {1} doesn't exist.",
                        //    eventArgs.SendingFleetMemberId, eventArgs.Target.Id);
                        continue;
                    }

                    var existingTarget = _targets.FirstOrDefault(target => target.Id == eventArgs.Target.Id);

                    if (existingTarget == null)
                        _targets.Add(eventArgs.Target);
                }

                _fleetNeedCombatAssistEventArgsToValidate.Clear();
            }
        }

        private bool InCombatCapableMode()
        {
            var methodName = "InCombatCapableMode";
            LogTrace(methodName);

            switch (_mainConfiguration.ActiveBehavior)
            {
                case BotModes.Mining:
                    return _meCache.Ship.Drones.Count > 0 &&
                           !_miningConfiguration.UseMiningDrones;
                case BotModes.Ratting:
                    return true;
                case BotModes.Missioning:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Clear the target queue.
        /// </summary>
        public void ClearQueue()
        {
            _targets.Clear();
        }
    }
}
