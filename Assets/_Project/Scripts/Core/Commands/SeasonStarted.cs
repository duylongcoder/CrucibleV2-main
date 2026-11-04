namespace Crucible.Core.Commands
{
    /// <summary>Sang Mùa <see cref="Season"/>: đã vào Chiếu chỉ và rút xong.</summary>
    public sealed class SeasonStarted : GameEvent
    {
        public SeasonStarted(int season)
        {
            Season = season;
        }

        public int Season { get; }
    }
}
