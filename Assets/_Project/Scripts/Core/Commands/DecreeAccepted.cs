namespace Crucible.Core.Commands
{
    /// <summary>Đã nhận chiếu chỉ của Mùa <see cref="Season"/>.</summary>
    public sealed class DecreeAccepted : GameEvent
    {
        public DecreeAccepted(int season)
        {
            Season = season;
        }

        public int Season { get; }
    }
}
