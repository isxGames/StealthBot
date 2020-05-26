using System;
using StealthBot.Core;
using StealthBot.ActionModules;
using StealthBot.Core.Interfaces;

namespace StealthBot.BehaviorModules
{
	public sealed class BoostCanOrca : BehaviorBase
	{
		private BoostCanOrcaStates _boostCanOrcaState = BoostCanOrcaStates.Idle;

		public BoostCanOrca()
		{
			IsEnabled = true;
			ModuleName = "BoostCanOrca";
			BehaviorManager.BehaviorsToPulse.Add(BotModes.BoostCanOrca, this);
		}

		public override void Pulse()
		{
            var methodName = "Pulse";
			LogTrace(methodName);

			if (!ShouldPulse() || Core.StealthBot.Config.MainConfig.ActiveBehavior != BotModes.BoostCanOrca ||
			    Core.StealthBot.Defense.IsFleeing || Core.StealthBot.Movement.IsMoving) 
				return;

		    if (!Core.StealthBot.Ship.IsInventoryReady) return;

			SetPulseState();
			ProcessPulseState();
		}

		protected override void SetPulseState()
		{
            var methodName = "SetPulseState";
			LogTrace(methodName);

			if (Core.StealthBot.MeCache.FleetMembers.Count == 0)
			{
				_boostCanOrcaState = BoostCanOrcaStates.WaitingForFleet;
			}
			else if (_boostCanOrcaState == BoostCanOrcaStates.WaitingForFleet)
			{
				_boostCanOrcaState = BoostCanOrcaStates.Idle;
			}
		}

		protected override void ProcessPulseState()
		{
            var methodName = "ProcessPulseState";
			LogTrace(methodName);

			switch (_boostCanOrcaState)
			{
				case BoostCanOrcaStates.Idle:
					_boostCanOrcaState = BoostCanOrcaStates.GetInPosition;
					goto case BoostCanOrcaStates.GetInPosition;
				case BoostCanOrcaStates.GetInPosition:
					if (Core.StealthBot.MeCache.InStation)
					{
						LogMessage(methodName, LogSeverityTypes.Debug, "Undocking.");
						Core.StealthBot.Movement.QueueDestination(new Destination(DestinationTypes.Undock));
						return;
					}

                    //This is never getting to the WaitInPosition state, never setting it.
                    //Should be using ChangeBelt. In fact, should make SetCurrent* private.
                    //As such, this needs refactored.

					//Our only option here is to use belt bookmarks. Specifically, an ice belt.
					if (Core.StealthBot.Config.MovementConfig.OnlyUseBeltBookmarks &&
                        Core.StealthBot.Config.MiningConfig.IsIceMining)
					{
                        if (Core.StealthBot.AsteroidBelts.CurrentBookMarkedBelt == null)
                        {
                            Core.StealthBot.AsteroidBelts.ChangeBelts(false, false);
                        }
						//Also need to be an ice belt.

                        if (Core.StealthBot.AsteroidBelts.CurrentBookMarkedBelt != null)
                        {
                            if (DistanceTo(Core.StealthBot.AsteroidBelts.CurrentBookMarkedBelt.X,
                                Core.StealthBot.AsteroidBelts.CurrentBookMarkedBelt.Y,
                                Core.StealthBot.AsteroidBelts.CurrentBookMarkedBelt.Z) >= (int)Ranges.Warp)
                            {
								LogMessage(methodName, LogSeverityTypes.Standard, "Moving to bookmark {0}",
									Core.StealthBot.AsteroidBelts.CurrentBookMarkedBelt.BookmarkLabel);
                                Core.StealthBot.Movement.QueueDestination(
									new Destination(DestinationTypes.BookMark, Core.StealthBot.AsteroidBelts.CurrentBookMarkedBelt.Id) { Distance = 3500 });
                            }
                            else
                            {
								LogMessage(methodName, LogSeverityTypes.Standard, "In position at bookmark {0}, waiting in position.",
									Core.StealthBot.AsteroidBelts.CurrentBookMarkedBelt.BookmarkLabel);
                                _boostCanOrcaState = BoostCanOrcaStates.WaitInPosition;
                                goto case BoostCanOrcaStates.WaitInPosition;
                            }
                        }
                        else
                        {
                        	LogMessage(methodName, LogSeverityTypes.Standard,
                        	           "Could not set a bookmarked belt, ensure you have Ice Mining checked and have a valid ice belt; error.");
                            _boostCanOrcaState = BoostCanOrcaStates.Error;
                            goto case BoostCanOrcaStates.Error;
                        }
					}
					else
					{
						LogMessage(methodName, LogSeverityTypes.Standard, "Not using belt bookmarks or not ice mining; error.");
						_boostCanOrcaState = BoostCanOrcaStates.Error;
						goto case BoostCanOrcaStates.Error;
					}
					break;
				case BoostCanOrcaStates.WaitInPosition:
					//All I need to do here is make sure my ganglinks are on. This doesn't particularly belong in
					//NonOffensive since it's not a targeted action.
					if (Core.StealthBot.Ship.GangLinkModules.Count > 0 &&
                        !Core.StealthBot.Ship.GangLinkModules[0].IsActive)
					{
						LogMessage(methodName, LogSeverityTypes.Standard, "Activating ganglink modules.");
                        Core.StealthBot.Ship.ActivateModuleList(Core.StealthBot.Ship.GangLinkModules, true);
					}
					if (Core.StealthBot.Ship.DamageControlModules.Count > 0 &&
                        !Core.StealthBot.Ship.DamageControlModules[0].IsActive)
					{
						LogMessage(methodName, LogSeverityTypes.Standard, "Activating damage control module.");
					    Core.StealthBot.Ship.ActivateModuleList(Core.StealthBot.Ship.DamageControlModules, false);
					}
					break;
				case BoostCanOrcaStates.Error:
					//Get safe.
					
					break;
			}
		}

        protected override void _processCleanupState()
        {
            throw new NotImplementedException();
        }

        protected override void _setCleanupState()
        {
            throw new NotImplementedException();
        }
	}

	public enum BoostCanOrcaStates
	{
		Idle,
		GetInPosition,
		WaitInPosition,
		Error,
		WaitingForFleet
	}
}