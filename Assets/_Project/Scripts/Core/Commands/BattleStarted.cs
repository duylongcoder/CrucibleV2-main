using Crucible.Core.Logs;

namespace Crucible.Core.Commands
{
    /// <summary>Trận Mùa <see cref="Season"/> đã tính xong; <see cref="Log"/> cùng đối tượng với <c>RunState.LastBattle</c>.</summary>
    public sealed class BattleStarted : GameEvent
    {
        public BattleStarted(int season, BattleLog log)
        {
            Season = season;
            Log = log;
        }

        public int Season { get; }
        public BattleLog Log { get; }
    }
}
