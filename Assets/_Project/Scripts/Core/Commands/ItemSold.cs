namespace Crucible.Core.Commands
{
    /// <summary>Đã bán vật phẩm <see cref="Uid"/>, nhận <see cref="Price"/> vàng.</summary>
    public sealed class ItemSold : GameEvent
    {
        public ItemSold(int uid, int price)
        {
            Uid = uid;
            Price = price;
        }

        public int Uid { get; }
        public int Price { get; }
    }
}
