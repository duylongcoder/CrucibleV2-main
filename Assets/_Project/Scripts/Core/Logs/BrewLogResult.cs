using Crucible.Core.Content;

namespace Crucible.Core.Logs
{
    /// <summary>Sự kiện kết quả cuối của <see cref="BrewLog"/>: thành phẩm hoặc phế phẩm.</summary>
    public readonly struct BrewLogResult
    {
        public BrewLogResult(bool isScrap, ItemId itemId, bool magic)
        {
            IsScrap = isScrap;
            ItemId = itemId;
            Magic = magic;
        }

        public bool IsScrap { get; }
        public ItemId ItemId { get; }
        public bool Magic { get; }
    }
}
