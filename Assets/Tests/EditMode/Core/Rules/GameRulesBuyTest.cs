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
    public class GameRulesBuyTest
    {
        // Hàng: ô 0 Sắt (4), 1 Than, 2 Đồng, 3 Tro, 4 Xúc tác Bễ; S = 100; vàng 20.
        private const int IronSlot = 0;
        private const int CatalystSlot = 4;
        private const int StartGold = 20;

        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = CreateRun(new[] { Coal });
        }

        [Test]
        public void Apply_BuyIngredient_AddsItemToBagEnd()
        {
            var expectedUid = _run.NextUid;

            var result = Apply(new Buy(IronSlot));

            Assert.That(result.Accepted, Is.True);
            var bought = _run.Bag.Last();
            Assert.That(bought.ItemId, Is.EqualTo(IdOf(_content, Iron)));
            Assert.That(bought.Uid, Is.EqualTo(expectedUid));
            Assert.That(bought.Magic, Is.False);
            Assert.That(bought.Quality, Is.EqualTo(0));
            Assert.That(_run.NextUid, Is.EqualTo(expectedUid + 1));
        }

        [Test]
        public void Apply_BuyIngredient_DeductsBuyPrice()
        {
            Apply(new Buy(IronSlot));

            Assert.That(_run.Gold, Is.EqualTo(StartGold - IronPrice));
        }

        [Test]
        public void Apply_BuyIngredient_MarksSlotUnavailable()
        {
            var result = Apply(new Buy(IronSlot));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Shop.Slots[IronSlot].IsAvailable, Is.False);
            Assert.That(_run.Shop.Slots.Skip(1).All(slot => slot.IsAvailable), Is.True);
        }

        [Test]
        public void Apply_BuyIngredient_EmitsItemBought()
        {
            var expectedUid = _run.NextUid;

            var result = Apply(new Buy(IronSlot));

            var bought = (ItemBought)result.Events.Single();
            Assert.That(bought.Slot, Is.EqualTo(IronSlot));
            Assert.That(bought.Uid, Is.EqualTo(expectedUid));
            Assert.That(bought.Price, Is.EqualTo(IronPrice));
        }

        [Test]
        public void Apply_BuyOntoEmptyCell_PlacesItemOnGrid()
        {
            var bagCount = _run.Bag.Count;

            var result = Apply(new Buy(IronSlot, 4));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Grid[4].ItemId, Is.EqualTo(IdOf(_content, Iron)));
            Assert.That(_run.Bag.Count, Is.EqualTo(bagCount));
            Assert.That(result.Events.OfType<GridChanged>().Count(), Is.EqualTo(1));
            Assert.That(result.Events.OfType<ItemBought>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Apply_BuyOntoOccupiedCell_ReturnsOccupantToBagEnd()
        {
            _run = CreateRun(new[] { Coal }, new[] { null, null, null, null, Copper });
            var occupantUid = _run.Grid[4].Uid;

            var result = Apply(new Buy(IronSlot, 4));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Grid[4].ItemId, Is.EqualTo(IdOf(_content, Iron)));
            Assert.That(_run.Bag.Last().Uid, Is.EqualTo(occupantUid));
        }

        [Test]
        public void Apply_BuyCatalyst_AddsToBag()
        {
            _run = CreateRun(new[] { Bellows, Mortar, Tongs, Bellows });

            var result = Apply(new Buy(CatalystSlot));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Bag.Count(item => _content.GetItem(item.ItemId).Kind == ItemKind.Catalyst), Is.EqualTo(5));
        }

        [Test]
        public void Apply_Buy_DoesNotDrawRandom()
        {
            var before = _run.Rng.Capture();

            var result = Apply(new Buy(IronSlot));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Rng.Capture(), Is.EqualTo(before));
        }

        [TestCase(-1)]
        [TestCase(5)]
        public void Apply_BuySlotOutOfRange_RejectsWithInvalidShopSlot(int slot)
        {
            AssertRejectedUnchanged(new Buy(slot), RejectReason.InvalidShopSlot);
        }

        [Test]
        public void Apply_BuyBeforeShopStocked_RejectsWithInvalidShopSlot()
        {
            _run.Shop.Slots.Clear();

            AssertRejectedUnchanged(new Buy(IronSlot), RejectReason.InvalidShopSlot);
        }

        [Test]
        public void Apply_BuySoldSlot_RejectsWithShopSlotEmpty()
        {
            StateFactory.StockShop(_run, _content, null, Coal, Copper, Ash, Bellows);

            AssertRejectedUnchanged(new Buy(0), RejectReason.ShopSlotEmpty);
        }

        [TestCase(-1)]
        [TestCase(9)]
        public void Apply_BuyOntoInvalidCell_RejectsWithInvalidCell(int cell)
        {
            AssertRejectedUnchanged(new Buy(IronSlot, cell), RejectReason.InvalidCell);
        }

        [Test]
        public void Apply_BuyCatalystOntoCell_RejectsWithItemNotPlaceable()
        {
            AssertRejectedUnchanged(new Buy(CatalystSlot, 0), RejectReason.ItemNotPlaceable);
        }

        [Test]
        public void Apply_BuyCatalystWhenSlotsFull_RejectsWithCatalystSlotsFull()
        {
            _run = CreateRun(new[] { Bellows, Mortar, Tongs, Bellows, Mortar });

            AssertRejectedUnchanged(new Buy(CatalystSlot), RejectReason.CatalystSlotsFull);
        }

        [Test]
        public void Apply_BuyWhenBagAndGridFull_RejectsWithBagFull()
        {
            _run = CreateFullRun();

            AssertRejectedUnchanged(new Buy(IronSlot), RejectReason.BagFull);
            Assert.That(_run.Gold, Is.EqualTo(StartGold));
        }

        [Test]
        public void Apply_BuyOntoOccupiedCellWhenFull_RejectsWithBagFull()
        {
            _run = CreateFullRun();

            AssertRejectedUnchanged(new Buy(IronSlot, 0), RejectReason.BagFull);
        }

        [Test]
        public void Apply_BuyWithGoldBelowPrice_RejectsWithNotEnoughGold()
        {
            _run.Gold = IronPrice - 1;

            AssertRejectedUnchanged(new Buy(IronSlot), RejectReason.NotEnoughGold);
        }

        [Test]
        public void Apply_BuyWithGoldEqualPrice_LeavesZeroGold()
        {
            _run.Gold = IronPrice;

            var result = Apply(new Buy(IronSlot));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Gold, Is.EqualTo(0));
        }

        [Test]
        public void Apply_BuyWhenResultPending_RejectsWithGridLocked()
        {
            _run.WorkbenchStep = WorkbenchStep.ResultPending;

            AssertRejectedUnchanged(new Buy(IronSlot), RejectReason.GridLocked);
        }

        [Test]
        public void Apply_BuyOutsideWorkbench_RejectsWithWrongPhase()
        {
            _run.Phase = RunPhase.Offering;

            AssertRejectedUnchanged(new Buy(IronSlot), RejectReason.WrongPhase);
        }

        [Test]
        public void Apply_BuyWithoutRun_RejectsWithRunNotActive()
        {
            var result = GameRules.Apply(new RunState(), _meta, _content, new Buy(IronSlot));

            Assert.That(result.Reason, Is.EqualTo(RejectReason.RunNotActive));
        }

        private RunState CreateRun(string[] bag, string[] grid = null)
        {
            var run = StateFactory.CreateWorkbenchRun(_content, bag, grid);
            run.Gold = StartGold;
            StateFactory.StockShop(run, _content, Iron, Coal, Copper, Ash, Bellows);
            return run;
        }

        /// <summary>Túi 5 + lưới 3 = sức chứa 8.</summary>
        private RunState CreateFullRun()
        {
            var run = CreateRun(new[] { Coal, Coal, Copper, Ash, Sand }, new[] { Iron, Coal, Copper });
            run.BagCapacity = 8;
            return run;
        }

        private CommandResult Apply(ICommand command) => GameRules.Apply(_run, _meta, _content, command);

        private void AssertRejectedUnchanged(ICommand command, RejectReason expected)
        {
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(command);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(expected));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }
    }
}
