using Crucible.Core.Content;

namespace Crucible.Core.State
{
    /// <summary>Một ô hàng Chợ. <see cref="None"/> = ô rỗng (đã bán hoặc không có hàng để bày).</summary>
    public readonly struct ShopSlot
    {
        public static readonly ShopSlot None = default;

        public ShopSlot(ItemId itemId)
        {
            ItemId = itemId;
            IsAvailable = true;
        }

        /// <summary>Vô nghĩa khi <see cref="IsAvailable"/> false.</summary>
        public ItemId ItemId { get; }
        public bool IsAvailable { get; }
    }
}
