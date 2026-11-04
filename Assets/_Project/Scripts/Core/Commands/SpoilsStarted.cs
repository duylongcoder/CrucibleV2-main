namespace Crucible.Core.Commands
{
    /// <summary>Thắng trận Mùa <see cref="Season"/> (chưa phải Mùa cuối) → Chiến lợi phẩm (F-011).</summary>
    public sealed class SpoilsStarted : GameEvent
    {
        public SpoilsStarted(int season)
        {
            Season = season;
        }

        public int Season { get; }
    }
}
