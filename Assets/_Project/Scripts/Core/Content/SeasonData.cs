using System;
using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>
    /// Khai báo một Mùa (theo thứ tự Mùa 1…N): đối thủ cố định + bể trang bị có trọng số + Chiến lợi phẩm (vàng, bể
    /// phần thưởng, có đảm bảo Xúc tác không). Mùa cuối không có Chiến lợi phẩm (spoils D5).
    /// </summary>
    public sealed class SeasonData
    {
        public SeasonData(string opponentId, IReadOnlyList<ForgeOptionData> forgePool, int spoilsGold = 0,
                          IReadOnlyList<RewardOptionData> rewardPool = null, bool guaranteeCatalyst = false)
        {
            OpponentId = opponentId;
            ForgePool = forgePool;
            SpoilsGold = spoilsGold;
            RewardPool = rewardPool ?? Array.Empty<RewardOptionData>();
            GuaranteeCatalyst = guaranteeCatalyst;
        }

        public string OpponentId { get; }
        public IReadOnlyList<ForgeOptionData> ForgePool { get; }
        /// <summary>Vàng tặng thẳng khi Thắng trận Mùa này (≥ 0).</summary>
        public int SpoilsGold { get; }
        /// <summary>Bể phần thưởng theo thứ tự khai báo; rỗng = không có lựa chọn.</summary>
        public IReadOnlyList<RewardOptionData> RewardPool { get; }
        /// <summary>≥ 1 lựa chọn là Xúc tác nếu còn chỗ Xúc tác (onboarding Mùa 2–3, run-flow AC-6b).</summary>
        public bool GuaranteeCatalyst { get; }
    }
}
