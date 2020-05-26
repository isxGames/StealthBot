using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using StealthBot.Core;
using StealthBot.Core.Config;

namespace StealthBotIpc
{
    /// <summary>
    /// Describes all information StealthBot needs to share with StealthBotUI during updates.
    /// </summary>
    [ProtoContract()]
    public class SbStateObject
    {
        /// <summary>
        /// Name of the session this StateObject holds info for.
        /// </summary>
        [ProtoMember(1)]
        public string SessionName;

        /// <summary>
        /// Name of the character this StateObject has info for.
        /// </summary>
        [ProtoMember(2)]
        public string CharacterName;

        /// <summary>
        /// Any new log messages we need to display.
        /// </summary>
        [ProtoMember(3)]
        public List<string> LogMessages;

        /// <summary>
        /// If true, says that we need to remove any existing information for this session and start over.
        /// </summary>
        [ProtoMember(4)]
        public bool Initializing;

        /// <summary>
        /// Percent of ship's shield remaining
        /// </summary>
        [ProtoMember(5)]
        public double ShieldPct;

        /// <summary>
        /// Percent of ship's armor remaining
        /// </summary>
        [ProtoMember(6)]
        public double ArmorPct;

        /// <summary>
        /// Percent of ship's structure remaining
        /// </summary>
        [ProtoMember(7)]
        public double StructurePct;

        /// <summary>
        /// Percent of ship's capacitor remaining
        /// </summary>
        [ProtoMember(8)]
        public double CapacitorPct;

        /// <summary>
        /// Name of the active ship.
        /// </summary>
        [ProtoMember(9)]
        public string ShipName;

        /// <summary>
        /// Type of the active ship.
        /// </summary>
        [ProtoMember(10)]
        public string ShipType;

        /// <summary>
        /// Elapsed runtime of StealthBot
        /// </summary>
        [ProtoMember(11)]
        public string ElapsedRuntime;

        /// <summary>
        /// This session's currently locked targets.
        /// </summary>
        [ProtoMember(12)]
        public List<string> LockedTargets;

        /// <summary>
        /// This session's currently queued targets.
        /// </summary>
        [ProtoMember(13)]
        public List<string> QueuedTargets;

        /// <summary>
        /// The asteroids in range of this character.
        /// </summary>
        [ProtoMember(14)]
        public List<string> AsteroidsInRange;

        /// <summary>
        /// The asteroids out of range of this character.
        /// </summary>
        [ProtoMember(15)]
        public List<string> AsteroidsOutOfRange;

        /// <summary>
        /// The destination queue of this session.
        /// </summary>
        [ProtoMember(16)]
        public List<string> DestinationQueue;

        /// <summary>
        /// The NPCs targeting this player and presenting a threat.
        /// </summary>
        [ProtoMember(17)]
        public List<string> Threats;

        /// <summary>
        /// DataGridView representing all items mined or moved.
        /// </summary>
        [ProtoMember(18)]
        public Dictionary<string, int> ItemsMined_Moved;

        /// <summary>
        /// DataGridView representing all ammo or crystals consumed.
        /// </summary>
        [ProtoMember(19)]
        public Dictionary<string, int> Ammo_CrystalsUsed;

        /// <summary>
        /// Number of total trips/dropoffs/missions/etc
        /// </summary>
        [ProtoMember(20)]
        public int TotalTrips;

        /// <summary>
        /// Average time per trip/dropoff/mission/etc
        /// </summary>
        [ProtoMember(21)]
        public string AverageTimePerTrip;

        /// <summary>
        /// Dictionary of Configuration profiles used by the StealthBot session.
        /// </summary>
        [ProtoMember(22)]
        public Dictionary<string, Dictionary<string, ConfigProperty>> ConfigurationProfiles = new Dictionary<string, Dictionary<string, ConfigProperty>>();

        /// <summary>
        /// Instantiate a new SBStateObject with all params necessary.
        /// </summary>
        /// <param name="logMessages"></param>
        public SbStateObject(string sessionName, string characterName, bool initializing,
            double shieldPct, double armorPct, double structurePct, double capacitorPct,
            string shipName, string shipType, string elapsedRuntime)
        {
            SessionName = sessionName;
            CharacterName = characterName;
            Initializing = initializing;
            ShieldPct = shieldPct;
            ArmorPct = armorPct;
            StructurePct = structurePct;
            CapacitorPct = capacitorPct;
            ShipName = shipName;
            ShipType = shipType;
            ElapsedRuntime = elapsedRuntime;
        }

        /// <summary>
        /// Constructor used for deserialization.
        /// </summary>
        public SbStateObject()
        {

        }
    }
}
