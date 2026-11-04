namespace Crucible.Core.Content
{
    /// <summary>Phần thưởng đã resolve.</summary>
    public sealed class Reward
    {
        public Reward(string id, RewardKind kind, ItemId item, int amount, bool isCatalyst)
        {
            Id = id;
            Kind = kind;
            Item = item;
            Amount = amount;
            IsCatalyst = isCatalyst;
        }

        /// <summary>Id chuỗi (save lưu Id, không lưu chỉ số).</summary>
        public string Id { get; }
        public RewardKind Kind { get; }
        /// <summary>Vô nghĩa khi <see cref="Kind"/> ≠ Item.</summary>
        public ItemId Item { get; }
        /// <summary>Số chỗ Túi thêm khi <see cref="Kind"/> = ExpandBag.</summary>
        public int Amount { get; }
        /// <summary>Item là Xúc tác (chiếm suất <c>CatalystSlots</c>).</summary>
        public bool IsCatalyst { get; }
    }
}
