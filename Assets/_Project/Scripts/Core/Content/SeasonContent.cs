using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Một Mùa đã resolve: đối thủ + bể trang bị (thứ tự khai báo) + tổng trọng số + Chiến lợi phẩm.</summary>
    public sealed class SeasonContent
    {
        public SeasonContent(OpponentData opponent, IReadOnlyList<ForgeOption> forgePool, int totalWeight,
                             int spoilsGold, IReadOnlyList<RewardOption> rewardPool, bool guaranteeCatalyst)
        {
            Opponent = opponent;
            ForgePool = forgePool;
            TotalWeight = totalWeight;
            SpoilsGold = spoilsGold;
            RewardPool = rewardPool;
            GuaranteeCatalyst = guaranteeCatalyst;
        }

        public OpponentData Opponent { get; }
        public IReadOnlyList<ForgeOption> ForgePool { get; }
        /// <summary>Σ trọng số, trong [1, int.MaxValue].</summary>
        public int TotalWeight { get; }
        public int SpoilsGold { get; }
        /// <summary>Bể phần thưởng theo thứ tự khai báo (Σ trọng số ≤ int.MaxValue).</summary>
        public IReadOnlyList<RewardOption> RewardPool { get; }
        public bool GuaranteeCatalyst { get; }
    }
}
