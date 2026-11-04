namespace Crucible.Core.Commands
{
    /// <summary>Đã reroll Chợ với giá <see cref="Price"/> (view đọc lại hàng).</summary>
    public sealed class ShopRerolled : GameEvent
    {
        public ShopRerolled(int price)
        {
            Price = price;
        }

        public int Price { get; }
    }
}
