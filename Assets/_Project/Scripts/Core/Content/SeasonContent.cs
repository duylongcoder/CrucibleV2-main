using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Một Mùa đã resolve: đối thủ + bể trang bị (thứ tự khai báo) + tổng trọng số.</summary>
    public sealed class SeasonContent
    {
        public SeasonContent(OpponentData opponent, IReadOnlyList<ForgeOption> forgePool, int totalWeight)
        {
            Opponent = opponent;
            ForgePool = forgePool;
            TotalWeight = totalWeight;
        }

        public OpponentData Opponent { get; }
        public IReadOnlyList<ForgeOption> ForgePool { get; }
        /// <summary>Σ trọng số, trong [1, int.MaxValue].</summary>
        public int TotalWeight { get; }
    }
}
