namespace Crucible.Core.Commands
{
    /// <summary>Gỡ vật phẩm khỏi một ô lưới về cuối Túi.</summary>
    public sealed class RemoveFromGrid : ICommand
    {
        public RemoveFromGrid(int cell)
        {
            Cell = cell;
        }

        public int Cell { get; }
    }
}
