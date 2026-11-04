namespace Crucible.Core.Content
{
    /// <summary>Mục bể Chiến lợi phẩm đã resolve.</summary>
    public readonly struct RewardOption
    {
        public RewardOption(RewardId reward, int weight)
        {
            Reward = reward;
            Weight = weight;
        }

        public RewardId Reward { get; }
        public int Weight { get; }
    }
}
