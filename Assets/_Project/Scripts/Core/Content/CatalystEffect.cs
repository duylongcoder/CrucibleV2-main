namespace Crucible.Core.Content
{
    /// <summary>
    /// Effect đã resolve (Id chuỗi → <see cref="ItemId"/>). Mỗi loại điều kiện tối đa 1 lần nên lưu phẳng: điều kiện vắng
    /// = <c>Has*</c> false / giá trị 0.
    /// </summary>
    public sealed class CatalystEffect
    {
        public CatalystEffect(EffectTrigger trigger, EffectActionKind action, int amount, StatKind stat, ItemId actionItem,
                              bool hasIngredientCondition, ItemId ingredient, bool hasRecipeCondition, ItemId recipeProduct,
                              int minAdditives, int chancePercent)
        {
            Trigger = trigger;
            Action = action;
            Amount = amount;
            Stat = stat;
            ActionItem = actionItem;
            HasIngredientCondition = hasIngredientCondition;
            Ingredient = ingredient;
            HasRecipeCondition = hasRecipeCondition;
            RecipeProduct = recipeProduct;
            MinAdditives = minAdditives;
            ChancePercent = chancePercent;
        }

        public EffectTrigger Trigger { get; }
        public EffectActionKind Action { get; }
        public int Amount { get; }
        public StatKind Stat { get; }
        /// <summary>Chỉ <c>CreateIngredient</c>.</summary>
        public ItemId ActionItem { get; }
        public bool HasIngredientCondition { get; }
        public ItemId Ingredient { get; }
        public bool HasRecipeCondition { get; }
        public ItemId RecipeProduct { get; }
        /// <summary>0 = không có điều kiện <c>AdditiveCount</c>.</summary>
        public int MinAdditives { get; }
        /// <summary>0 = không có điều kiện <c>Chance</c>.</summary>
        public int ChancePercent { get; }
    }
}
