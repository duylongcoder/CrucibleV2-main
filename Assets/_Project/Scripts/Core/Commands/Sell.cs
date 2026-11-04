namespace Crucible.Core.Commands
{
    /// <summary>Bán một vật phẩm trong Túi hoặc trên lưới.</summary>
    public sealed class Sell : ICommand
    {
        public Sell(int uid)
        {
            Uid = uid;
        }

        public int Uid { get; }
    }
}
