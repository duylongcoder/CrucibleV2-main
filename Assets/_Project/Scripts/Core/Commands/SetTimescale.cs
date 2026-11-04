namespace Crucible.Core.Commands
{
    /// <summary>Chọn timescale phát lại 1× / 2× / 3× — nhớ trong meta (GDD §6.3).</summary>
    public sealed class SetTimescale : ICommand
    {
        public SetTimescale(int timescale)
        {
            Timescale = timescale;
        }

        public int Timescale { get; }
    }
}
