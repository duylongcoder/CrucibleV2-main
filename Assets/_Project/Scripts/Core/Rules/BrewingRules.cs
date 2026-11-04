using System;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Luật <see cref="Commands.Brew"/> (brewing.md §Thành phẩm & tiêu hao, §Kích hoạt ma thuật). Khớp → tung ma thuật
    /// rồi sinh dòng thuộc tính (stream <c>Chance</c>); phế phẩm → không rút random. Chưa chạy Xúc tác (F-007) — <c>Quality</c> =
    /// <c>BaseEssence</c> qua <see cref="QualityFormula"/> với 0 / ∅.
    /// </summary>
    public static class BrewingRules
    {
        private static readonly int[] NoMultipliers = Array.Empty<int>();

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
            ItemInstance result;
            if (recipe != null)
            {
                var quality = QualityFormula.Compute(recipe.BaseEssence, 0, NoMultipliers, content.Brewing.QualityCap);
                // Chance (TECH §3.4): 1. tung ma thuật — đúng 1 lần, kể cả 0%; 2. dòng thuộc tính (F-008).
                var magicChance = MagicChanceFormula.ForBrew(run.Grid, recipe, content);
                ref var chance = ref run.Rng.Get(RngStream.Chance);
                var magic = chance.Roll(magicChance);
                var lines = StatLineRules.Roll(ref chance, recipe);
                result = new ItemInstance(run.NextUid, recipe.Product, magic, quality, lines);
            }
            else
            {
                result = new ItemInstance(run.NextUid, content.ScrapItemId);
            }

            // Chụp lưới trước khi tiêu hao (brewing.md §BrewLog).
            var log = BrewLog.Create(run.Grid, result, recipe == null);

            Array.Clear(run.Grid, 0, run.Grid.Length);
            run.NextUid++;
            run.Bag.Add(result);
            run.WorkbenchStep = WorkbenchStep.ResultPending;
            run.LastBrewResultUid = result.Uid;
            run.LastBrew = log;

            return CommandResult.Accept(new GameEvent[] { new BrewCompleted(result.Uid, log) });
        }
    }
}
