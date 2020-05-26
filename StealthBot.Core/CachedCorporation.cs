using System;
using ProtoBuf;

namespace StealthBot.Core
{
    [ProtoContract]
    public class CachedCorporation
    {
        [ProtoMember(1)]
        public int CorporationId;
        [ProtoMember(2)]
        public int MemberOfAlliance;
        [ProtoMember(3)]
        public string Name;
        [ProtoMember(4)]
        public string Ticker;
        [ProtoMember(5)]
        public DateTime LastUpdated;
    }
}
