namespace Crucible.Core.Content
{
    /// <summary>
    /// Action whitelist của Effect (economy.md). <c>AddMagicChance</c> không là action: phụ gia mang nó dưới dạng field
    /// <see cref="ItemData.MagicChance"/> (catalyst D1).
    /// </summary>
    public enum EffectActionKind
    {
        AddEssence = 0,
        AddPotency = 1,
        /// <summary><c>Amount</c> = % hệ số (101–500).</summary>
        MulPotency = 2,
        /// <summary>Cộng <c>Amount</c> vào dòng <c>Stat</c> của thành phẩm (thêm dòng nếu chưa có).</summary>
        AddStat = 3,
        GainGold = 4,
        /// <summary>Tạo 1 nguyên liệu gốc <c>ItemId</c> vào cuối Túi; Túi đầy → bị chặn.</summary>
        CreateIngredient = 5,
    }
}
