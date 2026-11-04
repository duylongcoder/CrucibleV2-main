using System;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Luật <see cref="Commands.Brew"/> (brewing.md §Thành phẩm & tiêu hao, §Kích hoạt ma thuật; economy.md §Xúc tác). Stream
    /// <c>Chance</c> (TECH §3.4): khớp → tung ma thuật → dòng thuộc tính → điều kiện <c>Chance</c> của Xúc tác; phế phẩm → chỉ
    /// Xúc tác. <c>Quality</c> = <see cref="QualityFormula"/> với phần cộng của Xúc tác; action vàng / tạo nguyên liệu áp sau
    /// khi kết quả vào Túi (catalyst D7).
    /// </summary>
    public static class BrewingRules
    {
        public static CommandResult Brew(RunState run, ContentDb content, Brew command)
        {
            // Validate — chưa chạm state.
            var reason = GridRules.ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);
            if (GridRules.CountItems(run) == 0)
                return CommandResult.Reject(RejectReason.EmptyGrid);

            var recipe = RecipeMatcher.Match(run.Grid, content);
            if (recipe == null && !content.HasScrap)
                throw new InvalidOperationException("Content has no scrap item; a failed brew has no result.");

            // Mutate.
            ref var chance = ref run.Rng.Get(RngStream.Chance);
            ItemInstance result;
            CatalystBrewOutcome catalysts;
            if (recipe != null)
            {
                // Chance (TECH §3.4): 1. tung ma thuật — đúng 1 lần, kể cả 0%; 2. dòng thuộc tính (F-008); 3. Xúc tác (F-007).
                var magicChance = MagicChanceFormula.ForBrew(run.Grid, recipe, content);
                var magic = chance.Roll(magicChance);
                var lines = StatLineRules.Roll(ref chance, recipe);
                catalysts = CatalystRunner.EvaluateBrew(run.Bag, run.Grid, recipe, content, ref chance);
                var quality = QualityFormula.Compute(recipe.BaseEssence + catalysts.EssenceBonus, catalysts.PotencyBonus,
                    catalysts.MulPercents, content.Brewing.QualityCap);
                lines = StatLineRules.ApplyBonuses(lines, catalysts.StatBonuses, BattleData.MaxStatValue);
                result = new ItemInstance(run.NextUid, recipe.Product, magic, quality, lines);
            }
            else
            {
                catalysts = CatalystRunner.EvaluateBrew(run.Bag, run.Grid, null, content, ref chance);
                result = new ItemInstance(run.NextUid, content.ScrapItemId);
            }

            // Chụp lưới trước khi tiêu hao (brewing.md §BrewLog); log tạo sau khi biết action nào bị chặn.
            var grid = (ItemInstance[])run.Grid.Clone();

            Array.Clear(run.Grid, 0, run.Grid.Length);
            run.NextUid++;
            run.Bag.Add(result);
            var activations = CatalystRunner.ApplyBrewOutcome(run, content, catalysts);
            var log = BrewLog.Create(grid, result, recipe == null, activations);
            run.WorkbenchStep = WorkbenchStep.ResultPending;
            run.LastBrewResultUid = result.Uid;
            run.LastBrew = log;

            var events = new GameEvent[1 + activations.Length];
            events[0] = new BrewCompleted(result.Uid, log);
            for (var i = 0; i < activations.Length; i++)
            {
                var a = activations[i];
                events[i + 1] = new CatalystActivated(a.CatalystUid, a.EffectIndex, a.Trigger, a.Blocked);
            }

            return CommandResult.Accept(events);
        }
    }
}
