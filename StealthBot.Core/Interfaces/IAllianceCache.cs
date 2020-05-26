using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StealthBot.Core.Interfaces
{
    public interface IAllianceCache : IModule
    {
        ReadOnlyCollection<CachedAlliance> CachedAlliances { get; }
        Dictionary<int, CachedAlliance> CachedAlliancesById { get; }
        bool IsDatabaseReady { get; }
        bool IsDatabaseBuilding { get; }
        void RegenerateAllianceDatabase();
    }
}
