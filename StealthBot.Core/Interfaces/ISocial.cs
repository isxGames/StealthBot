using System.Collections.ObjectModel;
using EVE.ISXEVE;

namespace StealthBot.Core.Interfaces
{
    public interface ISocial
    {
        bool IsLocalSafe { get; }
        ReadOnlyCollection<Pilot> LocalPilots { get; }
    }
}
