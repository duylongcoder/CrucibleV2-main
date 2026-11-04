namespace Crucible.Core.Commands
{
    /// <summary>Đã mua món ở ô <see cref="Slot"/> thành vật phẩm <see cref="Uid"/> với giá <see cref="Price"/>.</summary>
    public sealed class ItemBought : GameEvent
    {
        public ItemBought(int slot, int uid, int price)
        {
            Slot = slot;
            Uid = uid;
            Price = price;
        }

        public int Slot { get; }
        public int Uid { get; }
        public int Price { get; }
    }
}
