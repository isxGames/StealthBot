using System.Collections.Generic;

namespace StealthBot.Core.Interfaces
{
    public interface IRandomWaitObject
    {
        void AddWait(KeyValuePair<int, int> rangeToWait, double chance);
        bool ShouldWait();
    }
}
