namespace Crucible.Core.Commands
{
    /// <summary>Hết lượt Bàn luyện của Mùa <see cref="Season"/> — vào Dâng.</summary>
    public sealed class OfferingStarted : GameEvent
    {
        public OfferingStarted(int season)
        {
            Season = season;
        }

        public int Season { get; }
    }
}
