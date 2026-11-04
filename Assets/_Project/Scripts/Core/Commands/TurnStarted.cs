namespace Crucible.Core.Commands
{
    /// <summary>Lượt Bàn luyện <see cref="Turn"/> đã bắt đầu: lưới mở, hàng Chợ vừa làm mới (view đọc lại hàng).</summary>
    public sealed class TurnStarted : GameEvent
    {
        public TurnStarted(int season, int turn)
        {
            Season = season;
            Turn = turn;
        }

        public int Season { get; }
        public int Turn { get; }
    }
}
