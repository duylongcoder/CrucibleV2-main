using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Công thức đã phân giải (TECH §3.1) — chỉ luật trong Core đọc; không bao giờ đưa ra View (khoảng cách ẩn).</summary>
    public sealed class Recipe
    {
        private readonly ItemId[] _allowedAdditives;

        public Recipe(ItemId product, RecipeSlot[] pattern, ItemId[] allowedAdditives, long baseEssence)
        {
            Product = product;
            Pattern = pattern;
            _allowedAdditives = allowedAdditives;
            BaseEssence = baseEssence;
        }

        public ItemId Product { get; }
        public IReadOnlyList<RecipeSlot> Pattern { get; }
        public long BaseEssence { get; }

        public bool IsAllowedAdditive(ItemId item)
        {
            for (var i = 0; i < _allowedAdditives.Length; i++)
            {
                if (_allowedAdditives[i] == item)
                    return true;
            }

            return false;
        }
    }
}
