using System;
using System.Collections.Generic;
using Crucible.Core.Content;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>Khớp chuỗi nạp (lưới theo hàng) với công thức (brewing.md §Khớp công thức).</summary>
    public static class RecipeMatcher
    {
        /// <returns>Công thức khớp, hoặc null (→ phế phẩm). ContentDb bảo đảm tối đa một công thức khớp.</returns>
        public static Recipe Match(IReadOnlyList<ItemInstance> grid, ContentDb content)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var start = 0;
            while (start < grid.Count && grid[start] == null)
                start++;
            if (start == grid.Count)
                return null;

            var recipes = content.Recipes;
            for (var r = 0; r < recipes.Count; r++)
            {
                if (Matches(grid, start, recipes[r], content))
                    return recipes[r];
            }

            return null;
        }

        private static bool Matches(IReadOnlyList<ItemInstance> grid, int start, Recipe recipe, ContentDb content)
        {
            var pattern = recipe.Pattern;
            if (start + pattern.Count > grid.Count)
                return false;

            for (var i = 0; i < pattern.Count; i++)
            {
                var slot = pattern[i];
                var cell = grid[start + i];
                if (slot.IsGap)
                {
                    if (cell != null && !IsAllowedAdditiveInGap(cell, recipe, content))
                        return false;
                }
                else if (cell == null || cell.ItemId != slot.Item || (slot.RequiresMagic && !cell.Magic))
                {
                    return false;
                }
            }

            for (var i = start + pattern.Count; i < grid.Count; i++)
            {
                if (grid[i] != null)
                    return false;
            }

            return true;
        }

        private static bool IsAllowedAdditiveInGap(ItemInstance cell, Recipe recipe, ContentDb content) =>
            content.GetItem(cell.ItemId).Kind == ItemKind.Additive && recipe.IsAllowedAdditive(cell.ItemId);
    }
}
