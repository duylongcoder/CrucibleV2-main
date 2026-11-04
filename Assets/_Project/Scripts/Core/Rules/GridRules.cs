using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Luật chỉnh lưới: <see cref="PlaceItem"/>, <see cref="RemoveFromGrid"/>, <see cref="MoveCell"/> (brewing.md §Lưới).
    /// Vật phẩm trên lưới rời <see cref="RunState.Bag"/> nhưng vẫn tính vào sức chứa Túi — rút/trả không cần kiểm sức chứa.
    /// </summary>
    public static class GridRules
    {
        /// <summary>Lưới cố định 3×3 (brewing.md — không phải knob tuning).</summary>
        public const int CellCount = 9;

        private static readonly GameEvent[] GridChangedEvents = { new GridChanged() };

        public static CommandResult Place(RunState run, ContentDb content, PlaceItem command)
        {
            var reason = ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);
            if (!IsValidCell(command.Cell))
                return CommandResult.Reject(RejectReason.InvalidCell);

            var bagIndex = BagRules.IndexOfUid(run, command.Uid);
            if (bagIndex < 0)
                return CommandResult.Reject(RejectReason.ItemNotFound);
            var item = run.Bag[bagIndex];
            if (!IsPlaceable(content.GetItem(item.ItemId).Kind))
                return CommandResult.Reject(RejectReason.ItemNotPlaceable);

            run.Bag.RemoveAt(bagIndex);
            var occupant = run.Grid[command.Cell];
            if (occupant != null)
                run.Bag.Add(occupant);
            run.Grid[command.Cell] = item;
            return CommandResult.Accept(GridChangedEvents);
        }

        public static CommandResult Remove(RunState run, RemoveFromGrid command)
        {
            var reason = ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);
            if (!IsValidCell(command.Cell))
                return CommandResult.Reject(RejectReason.InvalidCell);
            var item = run.Grid[command.Cell];
            if (item == null)
                return CommandResult.Reject(RejectReason.CellEmpty);

            run.Grid[command.Cell] = null;
            run.Bag.Add(item);
            return CommandResult.Accept(GridChangedEvents);
        }

        public static CommandResult Move(RunState run, MoveCell command)
        {
            var reason = ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);
            if (!IsValidCell(command.FromCell) || !IsValidCell(command.ToCell) || command.FromCell == command.ToCell)
                return CommandResult.Reject(RejectReason.InvalidCell);
            if (run.Grid[command.FromCell] == null)
                return CommandResult.Reject(RejectReason.CellEmpty);

            var moving = run.Grid[command.FromCell];
            run.Grid[command.FromCell] = run.Grid[command.ToCell];
            run.Grid[command.ToCell] = moving;
            return CommandResult.Accept(GridChangedEvents);
        }

        /// <summary>Số ô lưới đang có vật phẩm (tính vào sức chứa Túi — <see cref="BagRules.UsedSlots"/>).</summary>
        public static int CountItems(RunState run)
        {
            var count = 0;
            for (var i = 0; i < run.Grid.Length; i++)
            {
                if (run.Grid[i] != null)
                    count++;
            }

            return count;
        }

        /// <summary>Run đang chơi → phase Workbench → bước Editing. <see cref="RejectReason.None"/> = hợp lệ.</summary>
        public static RejectReason ValidateEditable(RunState run)
        {
            if (!run.IsActive)
                return RejectReason.RunNotActive;
            if (run.Phase != RunPhase.Workbench)
                return RejectReason.WrongPhase;
            if (run.WorkbenchStep != WorkbenchStep.Editing)
                return RejectReason.GridLocked;
            return RejectReason.None;
        }

        private static bool IsValidCell(int cell) => cell >= 0 && cell < CellCount;

        private static bool IsPlaceable(ItemKind kind) => kind == ItemKind.Mineral || kind == ItemKind.Additive;
    }
}
