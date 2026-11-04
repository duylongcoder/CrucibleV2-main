using System;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Luật <see cref="Commands.Brew"/> (brewing.md §Thành phẩm & tiêu hao). F-002: chưa tung ma thuật (F-003), chưa
    /// chạy Xúc tác (F-007) — <c>Quality</c> = <c>BaseEssence</c> qua <see cref="QualityFormula"/> với 0 / ∅.
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
            if (IsGridEmpty(run))
                return CommandResult.Reject(RejectReason.EmptyGrid);

            var recipe = RecipeMatcher.Match(run.Grid, content);
            if (recipe == null && !content.HasScrap)
                throw new InvalidOperationException("Content has no scrap item; a failed brew has no result.");

            // Mutate.
            ItemInstance result;
            if (recipe != null)
            {
                var quality = QualityFormula.Compute(recipe.BaseEssence, 0, NoMultipliers, content.Brewing.QualityCap);
                result = new ItemInstance(run.NextUid, recipe.Product, magic: false, quality: quality);
            }
            else
            {
                result = new ItemInstance(run.NextUid, content.ScrapItemId);
            }

            Array.Clear(run.Grid, 0, run.Grid.Length);
            run.NextUid++;
            run.Bag.Add(result);
            run.WorkbenchStep = WorkbenchStep.ResultPending;
            run.LastBrewResultUid = result.Uid;

            return CommandResult.Accept(new GameEvent[] { new BrewCompleted(result.Uid) });
        }

        private static bool IsGridEmpty(RunState run)
        {
            for (var i = 0; i < run.Grid.Length; i++)
            {
                if (run.Grid[i] != null)
                    return false;
            }

            return true;
        }
    }
}
