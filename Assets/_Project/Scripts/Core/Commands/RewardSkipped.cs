namespace Crucible.Core.Commands
{
    /// <summary>Bỏ qua Chiến lợi phẩm Mùa <see cref="Season"/> (không nhận lựa chọn nào).</summary>
    public sealed class RewardSkipped : GameEvent
    {
        public RewardSkipped(int season)
        {
            Season = season;
        }

        public int Season { get; }
    }
}
