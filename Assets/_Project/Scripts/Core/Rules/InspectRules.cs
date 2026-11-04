using System.Collections.Generic;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>Kính soi (GDD §6.2): lộ dòng ẩn của thành phẩm trong Túi (stream <c>Inspect</c>), tiêu hao Kính.</summary>
    public static class InspectRules
    {
        /// <summary>Chỉ thành phẩm (có công thức) có dòng ẩn; phế phẩm, nguyên liệu gốc, Kính, Xúc tác → không.</summary>
        public static bool IsInspectable(ItemData item)
        {
            return item.Recipe != null;
        }

        /// <summary>Số dòng ẩn của thành phẩm = <c>Quality</c> + dòng phụ (<c>Magic</c> không tính — AC-5).</summary>
        public static int LineCount(ItemInstance item)
        {
            return 1 + item.Lines.Count;
        }

        /// <summary>
        /// Từ chối (D8): <c>RunNotActive</c> → <c>WrongPhase</c> → <c>GridLocked</c> → <c>ItemNotFound</c> (target) →
        /// <c>ItemNotInspectable</c> → <c>ItemNotFound</c> (Kính) → <c>ItemNotLens</c> → <c>NothingToReveal</c>. Hợp lệ: dòng
        /// chưa lộ ≤ <c>RevealLines</c> → lộ hết, không rút RNG (D6); ngược lại mỗi dòng lộ 1 <c>NextInt(còn lại)</c> trên
        /// stream <c>Inspect</c> (D7). Kính tiêu hao.
        /// </summary>
        public static CommandResult Inspect(RunState run, ContentDb content, Inspect command)
        {
            // Validate — chưa chạm state.
            var reason = ValidatePhase(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);
            var targetIndex = BagRules.IndexOfUid(run, command.TargetUid);
            if (targetIndex < 0)
                return CommandResult.Reject(RejectReason.ItemNotFound);
            var target = run.Bag[targetIndex];
            if (!IsInspectable(content.GetItem(target.ItemId)))
                return CommandResult.Reject(RejectReason.ItemNotInspectable);
            var lensIndex = BagRules.IndexOfUid(run, command.LensUid);
            if (lensIndex < 0)
                return CommandResult.Reject(RejectReason.ItemNotFound);
            var lens = content.GetItem(run.Bag[lensIndex].ItemId);
            if (lens.Kind != ItemKind.Lens)
                return CommandResult.Reject(RejectReason.ItemNotLens);
            var lineCount = LineCount(target);
            var hidden = ((1 << lineCount) - 1) & ~target.RevealedMask;
            if (hidden == 0)
                return CommandResult.Reject(RejectReason.NothingToReveal);

            // Mutate.
            var revealed = PickLines(run, hidden, lineCount, lens.RevealLines);
            target.RevealedMask |= revealed;
            run.Bag.RemoveAt(lensIndex);
            return CommandResult.Accept(new GameEvent[] { new ItemInspected(command.TargetUid, command.LensUid, revealed) });
        }

        private static RejectReason ValidatePhase(RunState run)
        {
            if (!run.IsActive)
                return RejectReason.RunNotActive;
            if (run.Phase == RunPhase.Workbench)
                return run.WorkbenchStep == WorkbenchStep.Editing ? RejectReason.None : RejectReason.GridLocked;
            return run.Phase == RunPhase.Offering ? RejectReason.None : RejectReason.WrongPhase;
        }

        /// <summary>Danh sách dòng chưa lộ theo thứ tự (0 = Quality, rồi Lines); chọn không lặp bằng <c>NextInt(còn lại)</c>.</summary>
        private static int PickLines(RunState run, int hidden, int lineCount, int revealLines)
        {
            var unrevealed = new List<int>(lineCount);
            for (var i = 0; i < lineCount; i++)
            {
                if ((hidden & (1 << i)) != 0)
                    unrevealed.Add(i);
            }

            if (unrevealed.Count <= revealLines)
                return hidden;

            ref var rng = ref run.Rng.Get(RngStream.Inspect);
            var picked = 0;
            for (var k = 0; k < revealLines; k++)
            {
                var j = rng.NextInt(unrevealed.Count);
                picked |= 1 << unrevealed[j];
                unrevealed.RemoveAt(j);
            }

            return picked;
        }
    }
}
