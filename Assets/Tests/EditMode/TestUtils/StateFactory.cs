using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;

namespace Crucible.Tests.TestUtils
{
    /// <summary>Dựng state run trực tiếp (không qua luật) cho test.</summary>
    public static class StateFactory
    {
        public const ulong Seed = 20261104;

        public static RunState CreateActiveRun()
        {
            var run = new RunState
            {
                IsActive = true,
                Seed = Seed,
                Rng = RngStreams.Create(Seed),
                DifficultyId = ContentFactory.Difficulty,
                StarterKitId = ContentFactory.StarterKit,
                StartedAtUnix = 1793750400,
                Season = 1,
                Phase = RunPhase.Decree,
                Gold = 0,
                Reputation = 3,
                BagCapacity = 8,
                NextUid = 4,
                SeasonPricePercent = 100,
            };
            run.Bag.Add(new ItemInstance(1, new ItemId(0)));
            run.Bag.Add(new ItemInstance(2, new ItemId(1)));
            run.Bag.Add(new ItemInstance(3, new ItemId(2)));
            return run;
        }

        /// <summary>Run active ở Chiếu chỉ Mùa 1 (Turn 0), Túi = 3 vật phẩm của <see cref="CreateActiveRun"/>, Chợ rỗng.</summary>
        public static RunState CreateDecreeRun()
        {
            var run = CreateActiveRun();
            run.Phase = RunPhase.Decree;
            run.Turn = 0;
            return run;
        }

        /// <summary>
        /// Run đang ở Bàn luyện, bước Editing. Uid: Túi trước (1, 2, …) rồi tới lưới theo thứ tự ô.
        /// <paramref name="gridItemIds"/>: tối đa 9 phần tử, null = ô trống.
        /// </summary>
        public static RunState CreateWorkbenchRun(ContentDb content, string[] bagItemIds, string[] gridItemIds = null)
        {
            var run = CreateActiveRun();
            run.Bag.Clear();
            run.Phase = RunPhase.Workbench;
            run.WorkbenchStep = WorkbenchStep.Editing;
            run.Turn = 1;
            run.BagCapacity = 14;

            var uid = 1;
            foreach (var id in bagItemIds)
                run.Bag.Add(new ItemInstance(uid++, ContentFactory.IdOf(content, id)));

            if (gridItemIds != null)
            {
                for (var cell = 0; cell < gridItemIds.Length && cell < GridRules.CellCount; cell++)
                {
                    if (gridItemIds[cell] != null)
                        run.Grid[cell] = new ItemInstance(uid++, ContentFactory.IdOf(content, gridItemIds[cell]));
                }
            }

            run.NextUid = uid;
            return run;
        }

        /// <summary>Dựng hàng Chợ tay theo thứ tự ô; null = ô rỗng. Số reroll = 0.</summary>
        public static void StockShop(RunState run, ContentDb content, params string[] slotItemIds)
        {
            run.Shop.Slots.Clear();
            run.Shop.RerollCount = 0;
            foreach (var id in slotItemIds)
                run.Shop.Slots.Add(id == null ? ShopSlot.None : new ShopSlot(ContentFactory.IdOf(content, id)));
        }
    }
}
