using System.Collections.Generic;
using EVE.ISXEVE;
using StealthBot.Core.Interfaces;

namespace StealthBot.Core
{
    /// <summary>
    /// Encapsulate access to system anomalies.
    /// </summary>
    public class AnomalyProvider : IAnomalyProvider
    {
        private readonly IShipCache _shipCache;

        public AnomalyProvider(IShipCache shipCache)
        {
            _shipCache = shipCache;
        }

        public IList<SystemAnomaly> GetAnomalies()
        {
            var scanner = _shipCache.Ship.Scanners.System;
            return scanner.GetAnomalies();
        }
    }
}
