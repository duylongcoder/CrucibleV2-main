namespace Crucible.Core.Commands
{
    /// <summary>Lượt <see cref="Turn"/> đã bị bỏ qua (vật phẩm trên lưới đã về Túi).</summary>
    public sealed class TurnSkipped : GameEvent
    {
        public TurnSkipped(int season, int turn)
        {
            Season = season;
            Turn = turn;
        }

        public int Season { get; }
        public int Turn { get; }
    }
}
