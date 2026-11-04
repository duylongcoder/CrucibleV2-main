using System;
using System.Collections.Generic;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Chạy Effect của Xúc tác (economy.md §Xúc tác & Effect primitive; plan catalyst R3–R8). Mọi Xúc tác trong Túi có
    /// hiệu lực, theo pha → thứ tự Túi → thứ tự khai báo. Điều kiện <c>Chance</c> luôn được tung khi trigger cháy.
    /// </summary>
    public static class CatalystRunner
    {
        /// <summary>
        /// Xét pha <c>OnBrewStart</c> → <c>OnRecipeMatch</c> (khớp) | <c>OnBrewFail</c> (<paramref name="recipe"/> null) →
        /// <c>OnBrewEnd</c> trên lưới chưa tiêu hao. Chỉ rút <paramref name="chance"/>; không sửa state.
        /// </summary>
        public static CatalystBrewOutcome EvaluateBrew(IReadOnlyList<ItemInstance> bag, IReadOnlyList<ItemInstance> grid,
                                                       Recipe recipe, ContentDb content, ref Pcg32 chance)
        {
            if (bag == null) throw new ArgumentNullException(nameof(bag));
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var outcome = new CatalystBrewOutcome();
            var additives = CountAdditives(grid, content);
            EvaluatePhase(EffectTrigger.OnBrewStart, bag, grid, recipe, additives, content, ref chance, outcome);
            EvaluatePhase(recipe != null ? EffectTrigger.OnRecipeMatch : EffectTrigger.OnBrewFail, bag, grid, recipe,
                additives, content, ref chance, outcome);
            EvaluatePhase(EffectTrigger.OnBrewEnd, bag, grid, recipe, additives, content, ref chance, outcome);
            return outcome;
        }

        /// <summary>
        /// Sau khi kết quả đã vào Túi và lưới đã trống: áp <c>GainGold</c> / <c>CreateIngredient</c> theo thứ tự kích hoạt
        /// (<c>CreateIngredient</c> không còn chỗ → bị chặn). Trả mọi kích hoạt (kể cả action <c>Quality</c>) theo thứ tự.
        /// </summary>
        public static CatalystActivation[] ApplyBrewOutcome(RunState run, ContentDb content, CatalystBrewOutcome outcome)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            if (outcome.ActivationCount == 0)
                return Array.Empty<CatalystActivation>();

            var activations = new CatalystActivation[outcome.ActivationCount];
            for (var i = 0; i < activations.Length; i++)
            {
                var effect = outcome.EffectAt(i);
                var blocked = !ApplyAction(run, effect);
                activations[i] = new CatalystActivation(outcome.CatalystUidAt(i), outcome.EffectIndexAt(i), effect.Trigger,
                    blocked);
            }

            return activations;
        }

        /// <summary>
        /// <c>OnShopRefresh</c> — gọi trong <c>SeasonRules.StartTurn</c> ngay sau <c>MarketRules.RefreshShop</c>. Điều kiện
        /// <c>Chance</c> rút stream <c>Shop</c>; action áp ngay; mỗi kích hoạt thêm 1 <see cref="CatalystActivated"/>.
        /// </summary>
        public static void RunShopRefresh(RunState run, ContentDb content, List<GameEvent> events)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (events == null) throw new ArgumentNullException(nameof(events));

            ref var shop = ref run.Rng.Get(RngStream.Shop);
            // Nguyên liệu tạo ra nối cuối Túi và không phải Xúc tác → chỉ duyệt các vật phẩm có từ trước.
            var count = run.Bag.Count;
            for (var i = 0; i < count; i++)
            {
                var item = run.Bag[i];
                if (content.GetItem(item.ItemId).Kind != ItemKind.Catalyst)
                    continue;

                var effects = content.EffectsOf(item.ItemId);
                for (var e = 0; e < effects.Count; e++)
                {
                    var effect = effects[e];
                    if (effect.Trigger != EffectTrigger.OnShopRefresh)
                        continue;
                    // Validator: OnShopRefresh chỉ có điều kiện Chance.
                    if (effect.ChancePercent > 0 && !shop.Roll(effect.ChancePercent))
                        continue;

                    var blocked = !ApplyAction(run, effect);
                    events.Add(new CatalystActivated(item.Uid, e, effect.Trigger, blocked));
                }
            }
        }

        private static void EvaluatePhase(EffectTrigger phase, IReadOnlyList<ItemInstance> bag,
                                          IReadOnlyList<ItemInstance> grid, Recipe recipe, int additives,
                                          ContentDb content, ref Pcg32 chance, CatalystBrewOutcome outcome)
        {
            for (var i = 0; i < bag.Count; i++)
            {
                var item = bag[i];
                if (item == null || content.GetItem(item.ItemId).Kind != ItemKind.Catalyst)
                    continue;

                var effects = content.EffectsOf(item.ItemId);
                for (var e = 0; e < effects.Count; e++)
                {
                    var effect = effects[e];
                    if (effect.Trigger != phase)
                        continue;

                    // Chance tung trước và luôn tung (GDD §6.1) — số lần rút không phụ thuộc điều kiện khác.
                    var active = effect.ChancePercent == 0 || chance.Roll(effect.ChancePercent);
                    if (effect.HasIngredientCondition && !Contains(grid, effect.Ingredient))
                        active = false;
                    if (effect.HasRecipeCondition && (recipe == null || recipe.Product != effect.RecipeProduct))
                        active = false;
                    if (additives < effect.MinAdditives)
                        active = false;

                    if (active)
                        outcome.Add(item.Uid, e, effect);
                }
            }
        }

        /// <summary>Áp action ngoài <c>Quality</c>; false = bị chặn. Action <c>Quality</c> đã gom vào outcome → true.</summary>
        private static bool ApplyAction(RunState run, CatalystEffect effect)
        {
            switch (effect.Action)
            {
                case EffectActionKind.GainGold:
                    run.Gold += effect.Amount;
                    return true;
                case EffectActionKind.CreateIngredient:
                    if (!BagRules.HasRoom(run))
                        return false;
                    run.Bag.Add(new ItemInstance(run.NextUid, effect.ActionItem));
                    run.NextUid++;
                    return true;
                default:
                    return true;
            }
        }

        private static bool Contains(IReadOnlyList<ItemInstance> grid, ItemId itemId)
        {
            for (var i = 0; i < grid.Count; i++)
            {
                if (grid[i] != null && grid[i].ItemId == itemId)
                    return true;
            }

            return false;
        }

        private static int CountAdditives(IReadOnlyList<ItemInstance> grid, ContentDb content)
        {
            var count = 0;
            for (var i = 0; i < grid.Count; i++)
            {
                if (grid[i] != null && content.GetItem(grid[i].ItemId).Kind == ItemKind.Additive)
                    count++;
            }

            return count;
        }
    }
}
