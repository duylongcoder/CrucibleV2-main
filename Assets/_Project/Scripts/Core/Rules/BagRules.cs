using System;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Luật Túi (economy.md §Túi): Túi và lưới dùng chung sức chứa; nới tới trần <see cref="RunData.MaxBagCapacity"/>;
    /// sắp thứ tự Xúc tác. Mọi luật thêm vật phẩm vào Túi phải hỏi <see cref="HasRoom"/> trước khi mutate.
    /// </summary>
    public static class BagRules
    {
        private static readonly GameEvent[] CatalystsReorderedEvents = { new CatalystsReordered() };

        /// <summary>Số vật phẩm trong Túi + trên lưới; mỗi vật phẩm 1 chỗ.</summary>
        public static int UsedSlots(RunState run)
        {
            return run.Bag.Count + GridRules.CountItems(run);
        }

        public static int FreeSlots(RunState run)
        {
            return Math.Max(0, run.BagCapacity - UsedSlots(run));
        }

        public static bool HasRoom(RunState run, int count = 1)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be at least 1.");
            return FreeSlots(run) >= count;
        }

        public static bool CanExpand(RunState run, ContentDb content)
        {
            return run.BagCapacity < content.Run.MaxBagCapacity;
        }

        /// <summary>Nới Túi tối đa tới trần; trả số chỗ thực sự thêm (0 khi đã ở trần).</summary>
        public static int Expand(RunState run, ContentDb content, int amount)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be at least 1.");

            var added = Math.Max(0, Math.Min(amount, content.Run.MaxBagCapacity - run.BagCapacity));
            run.BagCapacity += added;
            return added;
        }

        public static CommandResult ReorderCatalyst(RunState run, ContentDb content, ReorderCatalyst command)
        {
            // Validate — chưa chạm state.
            var reason = GridRules.ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);

            var bagIndex = IndexOfUid(run, command.Uid);
            if (bagIndex < 0)
                return CommandResult.Reject(RejectReason.ItemNotFound);
            if (!IsCatalyst(run.Bag[bagIndex], content))
                return CommandResult.Reject(RejectReason.ItemNotCatalyst);

            var catalystCount = 0;
            var from = -1;
            for (var i = 0; i < run.Bag.Count; i++)
            {
                if (!IsCatalyst(run.Bag[i], content))
                    continue;
                if (i == bagIndex)
                    from = catalystCount;
                catalystCount++;
            }

            if (command.ToIndex < 0 || command.ToIndex >= catalystCount || command.ToIndex == from)
                return CommandResult.Reject(RejectReason.InvalidCatalystIndex);

            // Mutate: dịch các Xúc tác nằm giữa from và ToIndex một bước trong chính các ô list của Xúc tác,
            // vật phẩm khác không đổi chỉ số.
            var positions = new int[catalystCount];
            var k = 0;
            for (var i = 0; i < run.Bag.Count; i++)
            {
                if (IsCatalyst(run.Bag[i], content))
                    positions[k++] = i;
            }

            var moving = run.Bag[positions[from]];
            var step = command.ToIndex > from ? 1 : -1;
            for (var c = from; c != command.ToIndex; c += step)
                run.Bag[positions[c]] = run.Bag[positions[c + step]];
            run.Bag[positions[command.ToIndex]] = moving;

            return CommandResult.Accept(CatalystsReorderedEvents);
        }

        /// <summary>Số Xúc tác trong Túi (Xúc tác không lên lưới).</summary>
        public static int CountCatalysts(RunState run, ContentDb content)
        {
            var count = 0;
            for (var i = 0; i < run.Bag.Count; i++)
            {
                if (IsCatalyst(run.Bag[i], content))
                    count++;
            }

            return count;
        }

        /// <summary>Chỉ số list của vật phẩm uid trong Túi; −1 = không có (kể cả khi đang trên lưới).</summary>
        public static int IndexOfUid(RunState run, int uid)
        {
            for (var i = 0; i < run.Bag.Count; i++)
            {
                if (run.Bag[i].Uid == uid)
                    return i;
            }

            return -1;
        }

        private static bool IsCatalyst(ItemInstance item, ContentDb content) =>
            content.GetItem(item.ItemId).Kind == ItemKind.Catalyst;
    }
}
