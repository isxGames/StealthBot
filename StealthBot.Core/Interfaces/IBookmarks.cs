using System.Collections.ObjectModel;
using EVE.ISXEVE;

namespace StealthBot.Core.Interfaces
{
    public interface IBookmarks
    {
        ReadOnlyCollection<CachedBookMark> CachedBookmarks { get; }
        CachedBookMark TempCanBookmark { get; set; }
        bool IsAtBookmark();
        bool IsAtBookmark(CachedBookMark bookMark);
        bool IsStationBookMark(BookMark bookMark);
        void CreateSalvagingBookmark();
        void CreateTemporaryHaulingBookmark(IEntityWrapper canToBookmark);
        void RemoveTemporaryHaulingBookmarks();
        CachedBookMark GetHaulerPickupSystemBookMark();
        CachedBookMark GetTempCanBookmark();
    }
}
