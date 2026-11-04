namespace Crucible.Core.Commands
{
    /// <summary>Đặt vật phẩm từ Túi lên một ô lưới (ô đã có vật phẩm → vật phẩm cũ về cuối Túi).</summary>
    public sealed class PlaceItem : ICommand
    {
        public PlaceItem(int uid, int cell)
        {
            Uid = uid;
            Cell = cell;
        }

        public int Uid { get; }
        public int Cell { get; }
    }
}
