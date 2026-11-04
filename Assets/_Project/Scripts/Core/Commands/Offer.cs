namespace Crucible.Core.Commands
{
    /// <summary>Dâng thành phẩm <see cref="Uid"/> trong Túi (tiêu hao). Khay cận thần là state view (forge-battle D2).</summary>
    public sealed class Offer : ICommand
    {
        public Offer(int uid)
        {
            Uid = uid;
        }

        public int Uid { get; }
    }
}
