using System.Collections.Generic;

namespace StealthBot.BehaviorModules
{
    public interface IBehaviorManager
    {
        Dictionary<BotModes, BehaviorBase> Behaviors { get; }
    }
}