namespace Crucible.Core.Content
{
    /// <summary>Điều kiện content của Effect (Id chuỗi); <c>ContentDb</c> resolve + validate.</summary>
    public sealed class EffectConditionData
    {
        public EffectConditionData(EffectConditionKind kind, int value = 0, string itemId = null)
        {
            Kind = kind;
            Value = value;
            ItemId = itemId;
        }

        public EffectConditionKind Kind { get; }
        /// <summary><c>AdditiveCount</c>: n; <c>Chance</c>: %; loại khác 0.</summary>
        public int Value { get; }
        /// <summary><c>IngredientIs</c>: nguyên liệu; <c>RecipeIs</c>: thành phẩm; loại khác null.</summary>
        public string ItemId { get; }

        public static EffectConditionData IngredientIs(string itemId) => new EffectConditionData(EffectConditionKind.IngredientIs, itemId: itemId);
        public static EffectConditionData RecipeIs(string productId) => new EffectConditionData(EffectConditionKind.RecipeIs, itemId: productId);
        public static EffectConditionData AdditiveCount(int count) => new EffectConditionData(EffectConditionKind.AdditiveCount, count);
        public static EffectConditionData Chance(int percent) => new EffectConditionData(EffectConditionKind.Chance, percent);
    }
}
