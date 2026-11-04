namespace Crucible.Core.Content
{
    /// <summary>Một phần thưởng Chiến lợi phẩm (dạng Id chuỗi, trước khi resolve — từ RewardDef).</summary>
    public sealed class RewardData
    {
        public RewardData(string id, RewardKind kind, string itemId = null, int amount = 0)
        {
            Id = id;
            Kind = kind;
            ItemId = itemId;
            Amount = amount;
        }

        public string Id { get; }
        public RewardKind Kind { get; }
        /// <summary>Vật phẩm nhận được khi <see cref="Kind"/> = Item (phải là nguyên liệu gốc — spoils D2); null với loại khác.</summary>
        public string ItemId { get; }
        /// <summary>Số chỗ Túi thêm khi <see cref="Kind"/> = ExpandBag (≥ 1); 0 với loại khác.</summary>
        public int Amount { get; }
    }
}
