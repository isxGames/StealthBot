using StealthBot.Core;
using StealthBot.ActionModules;
using StealthBot.Core.Interfaces;

namespace StealthBot.BehaviorModules
{
    public sealed class JumpStabilityTest : BehaviorBase
    {
        CachedBookMark _startBookMark, _endBookMark;

        public JumpStabilityTest()
        {
            ModuleName = "JumpStabilityTest";
            IsEnabled = true;
            PulseFrequency = 2;
            BehaviorManager.BehaviorsToPulse.Add(BotModes.JumpStabilityTest , this);
        }

        public override void Pulse()
        {
            var methodName = "Pulse";
			LogTrace(methodName);

            if (ShouldPulse() && !Core.StealthBot.Movement.IsMoving)
            {
                ProcessPulseState();
            }
        }

        protected override void SetPulseState()
        {
            
        }

        protected override void ProcessPulseState()
        {
            var methodName = "ProcessPulseState";
			LogTrace(methodName);

        	if (!GetCachedBookMarks()) 
				return;

        	//if I'm at the start, move to the end.
        	if (Core.StealthBot.MeCache.IsAtBookMark(_startBookMark))
        	{
        		MoveToEndBookMark();
        	}
        		//If I'm at the end, move to the start.
        	else if (Core.StealthBot.MeCache.IsAtBookMark(_endBookMark))
        	{
        		MoveToStartBookMark();
        	}
        		//If I'm at neither, move to the start.
        	else
        	{
        		MoveToStartBookMark();
        	}
        }

        protected override void _setCleanupState()
        {
            
        }

        protected override void _processCleanupState()
        {
            
        }

        private bool GetCachedBookMarks()
        {
            var methodName = "GetCachedBookMarks";
			LogTrace(methodName);

            if (_startBookMark == null)
            {
                _startBookMark = Core.StealthBot.BookMarkCache.FirstBookMarkMatching(
                    Core.StealthBot.Config.MovementConfig.JumpStabilityTestStartBookmark, false);
            }
            if (_endBookMark == null)
            {
                _endBookMark = Core.StealthBot.BookMarkCache.FirstBookMarkMatching(
                    Core.StealthBot.Config.MovementConfig.JumpStabilityTestEndBookmark, false);
            }

            if (_startBookMark != null && _endBookMark != null)
            {
                return true;
            }

        	LogMessage(methodName, LogSeverityTypes.Standard, "Error; Failed to find bookmarks for Jump Stability Test.");
        	return false;
        }

        private void MoveToStartBookMark()
        {
            var methodName = "MoveToStartBookMark";
			LogTrace(methodName);

			LogMessage(methodName, LogSeverityTypes.Standard, "Moving to start bookmark.");
			var startDestination = new Destination(DestinationTypes.BookMark, _startBookMark.Id) { Dock = true };
            Core.StealthBot.Movement.QueueDestination(startDestination);
        }

        private void MoveToEndBookMark()
        {
            var methodName = "MoveToEndBookMark";
			LogTrace(methodName);

			LogMessage(methodName, LogSeverityTypes.Standard, "Moving to end bookmark.");
			var endDestination = new Destination(DestinationTypes.BookMark, _endBookMark.Id) { Dock = true };
            Core.StealthBot.Movement.QueueDestination(endDestination);
        }
    }
}
