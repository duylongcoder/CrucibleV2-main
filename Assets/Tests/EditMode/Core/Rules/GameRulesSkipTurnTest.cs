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
    /// <summary><see cref="SkipTurn"/> (run-loop R3, R4, R6; brewing AC-10; economy AC-2b nhánh Bỏ qua).</summary>
    public class GameRulesSkipTurnTest
    {
        private static readonly string[] GridAtCorners = { Iron, null, null, null, Copper, null, null, null, Coal };

        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = CreateRun(gridItemIds: null);
        }

        [Test]
        public void Apply_SkipTurnWithEmptyGrid_Accepted()
        {
            var result = Apply(new SkipTurn());

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Turn, Is.EqualTo(2));
        }

        [Test]
        public void Apply_SkipTurnWithGridItems_ReturnsItemsToBagEndInCellOrder()
        {
            _run = CreateRun(GridAtCorners);
            var oldBag = _run.Bag.Select(item => item.Uid).ToArray();
            var gridUids = new[] { _run.Grid[0].Uid, _run.Grid[4].Uid, _run.Grid[8].Uid };

            Apply(new SkipTurn());

            Assert.That(_run.Grid.All(cell => cell == null), Is.True);
            Assert.That(_run.Bag.Select(item => item.Uid), Is.EqualTo(oldBag.Concat(gridUids)));
        }

        [Test]
        public void Apply_SkipTurnWithGridItems_EmitsGridChanged()
        {
            _run = CreateRun(GridAtCorners);

            var events = Apply(new SkipTurn()).Events;

            Assert.That(events.Count, Is.EqualTo(3));
            Assert.That(events[0], Is.TypeOf<GridChanged>());
            Assert.That(events[1], Is.TypeOf<TurnSkipped>());
            Assert.That(((TurnSkipped)events[1]).Season, Is.EqualTo(1));
            Assert.That(((TurnSkipped)events[1]).Turn, Is.EqualTo(1));
            Assert.That(events[2], Is.TypeOf<TurnStarted>());
            Assert.That(((TurnStarted)events[2]).Turn, Is.EqualTo(2));
        }

        [Test]
        public void Apply_SkipTurnWithEmptyGrid_EmitsNoGridChanged()
        {
            var events = Apply(new SkipTurn()).Events;

            Assert.That(events.OfType<GridChanged>(), Is.Empty);
            Assert.That(events.OfType<TurnSkipped>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Apply_SkipTurnWithGridItems_KeepsUsedSlots()
        {
            _run = CreateRun(GridAtCorners);
            _run.BagCapacity = BagRules.UsedSlots(_run);

            Assert.That(Apply(new SkipTurn()).Accepted, Is.True);

            Assert.That(BagRules.UsedSlots(_run), Is.EqualTo(_run.BagCapacity));
        }

        [Test]
        public void Apply_SkipTurnBeforeLastTurn_RefreshesShopAndResetsReroll()
        {
            _run.Shop.RerollCount = 2;
            var expected = CreateRun(gridItemIds: null);
            expected.Rng.Restore(_run.Rng.Capture());
            MarketRules.RefreshShop(expected, _content);

            Apply(new SkipTurn());

            Assert.That(_run.Shop.RerollCount, Is.EqualTo(0));
            Assert.That(_run.Shop.Slots.Select(slot => slot.ItemId), Is.EqualTo(expected.Shop.Slots.Select(slot => slot.ItemId)));
            Assert.That(_run.Shop.Slots.All(slot => slot.IsAvailable), Is.True);
        }

        [Test]
        public void Apply_SkipTurnOnLastTurn_EntersOffering()
        {
            _run.Turn = TurnsPerSeason;

            var events = Apply(new SkipTurn()).Events;

            Assert.That(_run.Phase, Is.EqualTo(RunPhase.Offering));
            Assert.That(_run.Turn, Is.EqualTo(0));
            Assert.That(events.OfType<OfferingStarted>().Single().Season, Is.EqualTo(1));
            Assert.That(events.OfType<TurnStarted>(), Is.Empty);
        }

        [Test]
        public void Apply_SkipTurnOnLastTurn_ClearsShop()
        {
            _run.Turn = TurnsPerSeason;
            _run.Shop.RerollCount = 1;

            Apply(new SkipTurn());

            Assert.That(_run.Shop.Slots, Is.Empty);
            Assert.That(_run.Shop.RerollCount, Is.EqualTo(0));
        }

        [Test]
        public void Apply_SkipTurnOnLastTurn_DoesNotAdvanceAnyStream()
        {
            _run.Turn = TurnsPerSeason;
            var before = _run.Rng.Capture();

            var result = Apply(new SkipTurn());

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Rng.Capture(), Is.EqualTo(before));
        }

        [Test]
        public void Apply_SkipTurnWhenResultPending_RejectsGridLocked()
        {
            _run.WorkbenchStep = WorkbenchStep.ResultPending;
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(new SkipTurn());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.GridLocked));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_SkipTurnOutsideWorkbench_RejectsWrongPhase([Values(RunPhase.Decree, RunPhase.Offering)] RunPhase phase)
        {
            _run.Phase = phase;
            _run.Turn = 0;
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(new SkipTurn());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.WrongPhase));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_SkipTurnWithoutRun_RejectsRunNotActive()
        {
            _run = new RunState();

            var result = Apply(new SkipTurn());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.RunNotActive));
        }

        private RunState CreateRun(string[] gridItemIds)
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Ash, Bone }, gridItemIds);
            StateFactory.StockShop(run, _content, Iron, Coal, Copper, Ash, Lens);
            return run;
        }

        private CommandResult Apply(ICommand command) => GameRules.Apply(_run, _meta, _content, command);
    }
}
