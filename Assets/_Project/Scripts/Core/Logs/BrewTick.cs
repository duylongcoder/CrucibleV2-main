using Crucible.Core.Content;

namespace Crucible.Core.Logs
{
    /// <summary>Một nhịp trong <see cref="BrewLog"/>: "nạp X" (<see cref="HasItem"/>) hoặc "chờ" (ô trống ở giữa).</summary>
    public readonly struct BrewTick
    {
        public BrewTick(int cell, bool hasItem, ItemId itemId, bool magic)
        {
            Cell = cell;
            HasItem = hasItem;
            ItemId = itemId;
            Magic = magic;
        }

        /// <summary>Ô lưới 0–8 (thứ tự quét hàng).</summary>
        public int Cell { get; }
        public bool HasItem { get; }
        /// <summary>Chỉ có nghĩa khi <see cref="HasItem"/>.</summary>
        public ItemId ItemId { get; }
        public bool Magic { get; }
    }
}
