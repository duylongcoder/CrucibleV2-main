namespace Crucible.Core.Commands
{
    /// <summary>Mua món ở ô hàng <see cref="Slot"/>; có <see cref="Cell"/> → đặt thẳng lên ô lưới đó (vật phẩm cũ về Túi).</summary>
    public sealed class Buy : ICommand
    {
        public Buy(int slot, int? cell = null)
        {
            Slot = slot;
            Cell = cell;
        }

        public int Slot { get; }
        public int? Cell { get; }
    }
}
