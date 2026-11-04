namespace Crucible.Core.Content
{
    /// <summary>Một slot công thức dạng Id chuỗi: nguyên liệu (có thể <c>RequiresMagic</c>) hoặc <c>Gap</c>.</summary>
    public sealed class RecipeSlotData
    {
        private RecipeSlotData(bool isGap, string itemId, bool requiresMagic)
        {
            IsGap = isGap;
            ItemId = itemId;
            RequiresMagic = requiresMagic;
        }

        public static RecipeSlotData Gap { get; } = new RecipeSlotData(true, null, false);

        public static RecipeSlotData Ingredient(string itemId, bool requiresMagic = false) =>
            new RecipeSlotData(false, itemId, requiresMagic);

        public bool IsGap { get; }
        public string ItemId { get; }
        public bool RequiresMagic { get; }
    }
}
