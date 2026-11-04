namespace Crucible.Core.Content
{
    /// <summary>Slot công thức đã phân giải sang <see cref="ItemId"/>.</summary>
    public readonly struct RecipeSlot
    {
        private RecipeSlot(bool isGap, ItemId item, bool requiresMagic)
        {
            IsGap = isGap;
            Item = item;
            RequiresMagic = requiresMagic;
        }

        public static RecipeSlot Gap => new RecipeSlot(true, default, false);

        public static RecipeSlot Ingredient(ItemId item, bool requiresMagic) => new RecipeSlot(false, item, requiresMagic);

        public bool IsGap { get; }
        /// <summary>Vô nghĩa khi <see cref="IsGap"/>.</summary>
        public ItemId Item { get; }
        public bool RequiresMagic { get; }
    }
}
