using System;
using System.Collections.Generic;
using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary>Bất biến sức chứa Túi + lưới qua <see cref="GameRules.Apply"/> (economy.md AC-2b, phần đã có luật).</summary>
    public class BagCapacityInvariantTest
    {
        private const int Capacity = 8;
        private const int StepsPerSeed = 2000;

        private ContentDb _content;
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
        }

        [Test]
        public void Apply_RandomGridAndBrewCommands_NeverExceedCapacity([Values(1, 2, 3, 4, 5)] int seed)
        {
            var run = CreateFullRun();
            var random = new System.Random(seed);

            for (var step = 0; step < StepsPerSeed; step++)
            {
                Apply(run, NextCommand(random, run, includeBrew: true));
                // Giả AckBrewResult (F-009 chưa có): mở lại lưới để fuzz tiếp.
                run.WorkbenchStep = WorkbenchStep.Editing;

                Assert.That(BagRules.UsedSlots(run), Is.LessThanOrEqualTo(run.BagCapacity), $"step {step}");
            }
        }

        [Test]
        public void Apply_RandomGridCommands_ConservesItems([Values(1, 2, 3, 4, 5)] int seed)
        {
            var run = CreateFullRun();
            var expected = AllUids(run).OrderBy(uid => uid).ToArray();
            var random = new System.Random(seed);

            for (var step = 0; step < StepsPerSeed; step++)
            {
                Apply(run, NextCommand(random, run, includeBrew: false));

                Assert.That(AllUids(run).OrderBy(uid => uid).ToArray(), Is.EqualTo(expected), $"step {step}");
            }
        }

        [Test]
        public void Apply_PlaceAndRemoveAtFullCapacity_KeepsUsedSlots()
        {
            var run = CreateFullRun();
            var before = BagRules.UsedSlots(run);

            Apply(run, new PlaceItem(1, 4));
            Apply(run, new RemoveFromGrid(0));

            Assert.That(BagRules.UsedSlots(run), Is.EqualTo(before));
        }

        [Test]
        public void Apply_BrewAtFullCapacity_LeavesRoomForResult()
        {
            var run = CreateFullRun();

            var result = Apply(run, new Brew(0));

            Assert.That(result.Accepted, Is.True);
            Assert.That(BagRules.UsedSlots(run), Is.EqualTo(Capacity - 1));
            Assert.That(run.Bag.Last().Uid, Is.EqualTo(run.LastBrewResultUid));
        }

        [Test]
        public void Apply_ReorderCatalystThenBrew_KeepsCatalystOrder()
        {
            var run = CreateFullRun();
            // Xúc tác ban đầu: uid 2 (Bễ), 4 (Cối), 6 (Kẹp) → đưa Kẹp lên đầu.
            Apply(run, new ReorderCatalyst(CatalystUid(run, Tongs), 0));

            Apply(run, new Brew(0));

            Assert.That(CatalystUids(run), Is.EqualTo(new[] { 6, 2, 4 }));
        }

        /// <summary>Túi 6 (gồm 3 Xúc tác) + lưới [Sắt, Than] = 8 = sức chứa.</summary>
        private RunState CreateFullRun()
        {
            var run = StateFactory.CreateWorkbenchRun(_content,
                new[] { Iron, Bellows, Coal, Mortar, Ash, Tongs }, new[] { Iron, Coal });
            run.BagCapacity = Capacity;
            return run;
        }

        private CommandResult Apply(RunState run, ICommand command) => GameRules.Apply(run, _meta, _content, command);

        /// <summary>Tham số ngẫu nhiên, có cả uid / ô / chỉ số sai để sinh lệnh bị từ chối.</summary>
        private static ICommand NextCommand(System.Random random, RunState run, bool includeBrew)
        {
            var kind = random.Next(includeBrew ? 5 : 4);
            switch (kind)
            {
                case 0: return new PlaceItem(RandomUid(random, run), random.Next(-1, GridRules.CellCount + 1));
                case 1: return new RemoveFromGrid(random.Next(-1, GridRules.CellCount + 1));
                case 2: return new MoveCell(random.Next(-1, GridRules.CellCount + 1), random.Next(-1, GridRules.CellCount + 1));
                case 3: return new ReorderCatalyst(RandomUid(random, run), random.Next(-1, 4));
                case 4: return new Brew(0);
                default: throw new InvalidOperationException();
            }
        }

        private static int RandomUid(System.Random random, RunState run) => random.Next(0, run.NextUid + 2);

        private static IEnumerable<int> AllUids(RunState run) =>
            run.Bag.Select(item => item.Uid).Concat(run.Grid.Where(item => item != null).Select(item => item.Uid));

        private int[] CatalystUids(RunState run) =>
            run.Bag.Where(item => _content.GetItem(item.ItemId).Kind == ItemKind.Catalyst).Select(item => item.Uid).ToArray();

        private int CatalystUid(RunState run, string id) =>
            run.Bag.First(item => item.ItemId == IdOf(_content, id)).Uid;
    }
}
