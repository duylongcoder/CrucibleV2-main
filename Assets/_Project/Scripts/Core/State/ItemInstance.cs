using Crucible.Core.Content;

namespace Crucible.Core.State
{
    /// <summary>Một vật phẩm cụ thể trong run (phần nền: Uid + ItemId).</summary>
    public sealed class ItemInstance
    {
        public ItemInstance(int uid, ItemId itemId)
        {
            Uid = uid;
            ItemId = itemId;
        }

        public int Uid { get; }
        public ItemId ItemId { get; }
    }
}
