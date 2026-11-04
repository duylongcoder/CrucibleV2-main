namespace Crucible.Core.Content
{
    /// <summary>Điều kiện whitelist của Effect (economy.md). Các điều kiện của một Effect nối AND, mỗi loại tối đa 1 lần.</summary>
    public enum EffectConditionKind
    {
        /// <summary>Lưới (trước tiêu hao) có ≥ 1 vật phẩm <c>ItemId</c>.</summary>
        IngredientIs = 0,
        /// <summary>Công thức khớp ra thành phẩm <c>ItemId</c>.</summary>
        RecipeIs = 1,
        /// <summary>Số phụ gia trên lưới ≥ <c>Value</c>.</summary>
        AdditiveCount = 2,
        /// <summary>Tung <c>Roll(Value%)</c> — luôn tung khi trigger cháy.</summary>
        Chance = 3,
    }
}
