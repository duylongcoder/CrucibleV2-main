namespace Crucible.Core.Commands
{
    /// <summary>Đã dâng vật phẩm <see cref="Uid"/> ở Mùa <see cref="Season"/>.</summary>
    public sealed class ItemOffered : GameEvent
    {
        public ItemOffered(int season, int uid)
        {
            Season = season;
            Uid = uid;
        }

        public int Season { get; }
        public int Uid { get; }
    }
}
