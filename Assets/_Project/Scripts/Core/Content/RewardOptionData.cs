namespace Crucible.Core.Content
{
    /// <summary>Một mục trong bể Chiến lợi phẩm của Mùa (dạng Id chuỗi, trước khi resolve).</summary>
    public sealed class RewardOptionData
    {
        public RewardOptionData(string rewardId, int weight)
        {
            RewardId = rewardId;
            Weight = weight;
        }

        public string RewardId { get; }
        /// <summary>Trọng số ≥ 0; 0 = không bao giờ ra.</summary>
        public int Weight { get; }
    }
}
