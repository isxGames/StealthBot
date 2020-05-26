using ProtoBuf;

namespace StealthBot.Core
{
    [ProtoContract]
    public class CachedAlliance
    {
        [ProtoMember(1)]
        public int AllianceId;
        [ProtoMember(2)]
        public string Name;
        [ProtoMember(3)]
        public string Ticker;
    }
}
