namespace Crucible.Core.Commands
{
    /// <summary>Timescale phát lại đã đổi.</summary>
    public sealed class TimescaleChanged : GameEvent
    {
        public TimescaleChanged(int timescale)
        {
            Timescale = timescale;
        }

        public int Timescale { get; }
    }
}
