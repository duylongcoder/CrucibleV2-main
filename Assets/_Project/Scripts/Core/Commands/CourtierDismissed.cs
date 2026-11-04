namespace Crucible.Core.Commands
{
    /// <summary>Cận thần ra về tay không ở Mùa <see cref="Season"/> (Thua).</summary>
    public sealed class CourtierDismissed : GameEvent
    {
        public CourtierDismissed(int season)
        {
            Season = season;
        }

        public int Season { get; }
    }
}
