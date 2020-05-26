using System.Collections.Generic;
using EVE.ISXEVE;

namespace StealthBot.Core.Interfaces
{
    public interface IAnomalyProvider
    {
        IList<SystemAnomaly> GetAnomalies();
    }
}