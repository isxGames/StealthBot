using System.Collections.Generic;
using System.IO;
using System.Linq;
using StealthBot.Core.Extensions;
using StealthBot.Core.Interfaces;

namespace StealthBot.Core
{
    public class PlayerShipDatabase : ModuleBase
    {
        private string _playerShipDatabasePath = string.Empty;
        private FileReadCallback<PlayerShip> _readCompleted;
        private FileWriteCallback _writeCompleted;

        public List<PlayerShip> PlayerShips;

        public PlayerShipDatabase()
        {
            ModuleManager.Modules.Add(this);

            _readCompleted = new FileReadCallback<PlayerShip>(ReadCompleted);
            _writeCompleted = new FileWriteCallback(WriteCompleted);
        }

        public override bool Initialize()
        {
            if (IsInitialized) return true;

            if (!_isInitializing)
            {
                _isInitializing = true;

                _playerShipDatabasePath = Path.Combine(StealthBot.ConfigDirectory, string.Format("{0} Ships.bin", StealthBot.MeCache.Name));

                if (File.Exists(_playerShipDatabasePath))
                    StealthBot.FileManager.QueueDeserialize(_playerShipDatabasePath, _readCompleted);
                else
                {
                    PlayerShips = new List<PlayerShip>();
                    IsInitialized = true;
                    _isInitializing = false;
                }
            }

            return IsInitialized;
        }

        private void ReadCompleted(List<PlayerShip> results)
        {
            PlayerShips = results;

            IsInitialized = true;
            _isInitializing = false;
        }

        public override bool OutOfFrameCleanup()
        {
            if (IsCleanedUpOutOfFrame) return true;

            if (!_isCleaningUp)
            {
                if (PlayerShips.Count == 0)
                    IsCleanedUpOutOfFrame = true;

                _isCleaningUp = true;
                StealthBot.FileManager.QueueOverwriteSerialize(_playerShipDatabasePath, PlayerShips, _writeCompleted);
            }

            return IsCleanedUpOutOfFrame;
        }

        private void WriteCompleted()
        {
            _isCleaningUp = false;
            IsCleanedUpOutOfFrame = true;
        }

        public PlayerShip GetBestShipForRole(ShipRoles role)
        {
            return GetBestShipForRole(role, DamageTypes.All);
        }

        public PlayerShip GetBestShipForRole(ShipRoles role, DamageTypes tankTypes)
        {
            var methodName = "GetBestShipForRole";
            LogTrace(methodName, "{0} {1}", role, (int)tankTypes);

            var shipsMatchingRole = PlayerShips.Where(ship => ship.ShipRole == role).OrderByDescending(ship => ship.TankTypes.GetMatchingBitCount(tankTypes));

            if (shipsMatchingRole.Count() == 0)
            {
                LogMessage(methodName, LogSeverityTypes.Standard, "Error: Could find no ships matching role \"{0}\".", role);
                return null;
            }

            //First try to find a ship matching perfectly.
            var bestTankMatch = shipsMatchingRole.FirstOrDefault();
            LogMessage(methodName, LogSeverityTypes.Standard, "Found ship \"{0}\" matching role \"{1}\" closest matching requested tank types.", bestTankMatch.ShipName, role);
            return bestTankMatch;
        }
    }
}
