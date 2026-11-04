using System;
using System.Collections.Generic;
using Crucible.Core.Content;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// <c>MagicChance%</c> (brewing.md §Formulas, tham số hóa 2026-11-04):
    /// <c>min(Cap[tierSP], ⌊(Σ Inherit[tier ngl ma thuật] + Σ MagicChance phụ gia) × Retain[tierSP] / 100⌋)</c>.
    /// </summary>
    public static class MagicChanceFormula
    {
        public static int Compute(IReadOnlyList<int> magicIngredientTiers, IReadOnlyList<int> additiveChances,
                                  int productTier, BrewingData brewing)
        {
            if (magicIngredientTiers == null) throw new ArgumentNullException(nameof(magicIngredientTiers));
            if (additiveChances == null) throw new ArgumentNullException(nameof(additiveChances));
            if (brewing == null) throw new ArgumentNullException(nameof(brewing));
            ValidateTier(productTier, nameof(productTier));

            var sum = 0;
            for (var i = 0; i < magicIngredientTiers.Count; i++)
            {
                ValidateTier(magicIngredientTiers[i], nameof(magicIngredientTiers));
                sum += brewing.MagicInheritByTier[magicIngredientTiers[i] - 1];
            }

            for (var i = 0; i < additiveChances.Count; i++)
                sum += additiveChances[i];

            var retained = sum * brewing.MagicRetainByTier[productTier - 1] / BrewingData.MaxPercent;
            return Math.Min(brewing.MagicCapByTier[productTier - 1], retained);
        }

        /// <summary>
        /// Gom dữ liệu từ lưới đã khớp <paramref name="recipe"/> rồi gọi <see cref="Compute"/>: tier mọi nguyên liệu
        /// (<c>Mineral</c>) có <c>Magic</c>, <c>MagicChance</c> mọi phụ gia (lưới đã khớp ⇒ phụ gia chỉ ở <c>Gap</c>).
        /// </summary>
        public static int ForBrew(IReadOnlyList<ItemInstance> grid, Recipe recipe, ContentDb content)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var magicTiers = new List<int>(grid.Count);
            var additiveChances = new List<int>(grid.Count);
            for (var i = 0; i < grid.Count; i++)
            {
                if (grid[i] == null)
                    continue;

                var data = content.GetItem(grid[i].ItemId);
                if (data.Kind == ItemKind.Additive)
                    additiveChances.Add(data.MagicChance);
                else if (data.Kind == ItemKind.Mineral && grid[i].Magic)
                    magicTiers.Add(data.Tier);
            }

            var productTier = content.GetItem(recipe.Product).Tier;
            return Compute(magicTiers, additiveChances, productTier, content.Brewing);
        }

        private static void ValidateTier(int tier, string paramName)
        {
            if (tier < 1 || tier > BrewingData.TierCount)
                throw new ArgumentOutOfRangeException(paramName, tier, $"Tier must be in [1, {BrewingData.TierCount}].");
        }
    }
}
