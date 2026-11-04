using System.Collections.Generic;
using Crucible.Core.Content;

namespace Crucible.Core.Commands
{
    /// <summary>
    /// Thắng trận Mùa <see cref="Season"/> (chưa phải Mùa cuối) → Chiến lợi phẩm: đã cộng <see cref="Gold"/>, bày
    /// <see cref="Options"/> (bản sao của <c>RunState.SpoilsOptions</c>).
    /// </summary>
    public sealed class SpoilsStarted : GameEvent
    {
        public SpoilsStarted(int season, int gold, IReadOnlyList<RewardId> options)
        {
            Season = season;
            Gold = gold;
            Options = options;
        }

        public int Season { get; }
        public int Gold { get; }
        public IReadOnlyList<RewardId> Options { get; }
    }
}
