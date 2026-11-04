namespace Crucible.Core.Commands
{
    /// <summary>Chuyển vật phẩm sang ô khác; ô đích có vật phẩm → đổi chỗ.</summary>
    public sealed class MoveCell : ICommand
    {
        public MoveCell(int fromCell, int toCell)
        {
            FromCell = fromCell;
            ToCell = toCell;
        }

        public int FromCell { get; }
        public int ToCell { get; }
    }
}
