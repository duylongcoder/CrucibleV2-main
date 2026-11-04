using Crucible.Core.Content;

namespace Crucible.Core.State
{
    /// <summary>Một vật phẩm cụ thể trong run. <c>Lines</c> / <c>RevealedMask</c> thêm ở F-008.</summary>
    public sealed class ItemInstance
    {
        public ItemInstance(int uid, ItemId itemId, bool magic = false, long quality = 0)
        {
            Uid = uid;
            ItemId = itemId;
            Magic = magic;
            Quality = quality;
        }

        public int Uid { get; }
        public ItemId ItemId { get; }
        /// <summary>Đã Kích hoạt ma thuật (công khai — GDD §6.2).</summary>
        public bool Magic { get; }
        /// <summary>0 với nguyên liệu gốc / phế phẩm.</summary>
        public long Quality { get; }
    }
}
