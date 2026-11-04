namespace Crucible.Core.Content
{
    /// <summary>Loại phần thưởng Chiến lợi phẩm (run-flow §Chiến lợi phẩm). Thuốc thử để sau MVP.</summary>
    public enum RewardKind
    {
        /// <summary>Một vật phẩm nguyên liệu gốc vào cuối Túi (khoáng/phụ gia tier cao, Xúc tác, Kính).</summary>
        Item = 0,
        /// <summary>Nới Túi thêm <c>Amount</c> chỗ, cắt tại trần.</summary>
        ExpandBag = 1,
    }
}
