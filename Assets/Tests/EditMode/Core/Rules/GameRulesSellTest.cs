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
    public class GameRulesSellTest
    {
        // Túi: uid 1 Thép, 2 Bễ, 3 Phế phẩm, 4 Sao thép, 5 Sắt; lưới ô 2: uid 6 Than. S = 100; vàng 10.
        private const int SteelUid = 1;
        private const int BellowsUid = 2;
        private const int ScrapUid = 3;
        private const int StarsteelUid = 4;
        private const int GridUid = 6;
        private const int StartGold = 10;

        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = StateFactory.CreateWorkbenchRun(_content, new[] { Steel, Bellows, Scrap, Starsteel, Iron },
                new[] { null, null, Coal });
            _run.Gold = StartGold;
        }

        [Test]
        public void Apply_SellFromBag_RemovesItemAndAddsSellPrice()
        {
            var result = Apply(new Sell(SteelUid));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Bag.Any(item => item.Uid == SteelUid), Is.False);
            Assert.That(_run.Gold, Is.EqualTo(StartGold + 9));
            var sold = (ItemSold)result.Events.Single();
            Assert.That(sold.Uid, Is.EqualTo(SteelUid));
            Assert.That(sold.Price, Is.EqualTo(9));
        }

        [Test]
        public void Apply_SellFromGrid_ClearsCellAndAddsSellPrice()
        {
            var result = Apply(new Sell(GridUid));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Grid[2], Is.Null);
            Assert.That(_run.Gold, Is.EqualTo(StartGold + CoalPrice * SellRatePercent / 100));
            Assert.That(result.Events.OfType<GridChanged>().Count(), Is.EqualTo(1));
            Assert.That(result.Events.OfType<ItemSold>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Apply_SellCatalyst_AddsSellPrice()
        {
            var result = Apply(new Sell(BellowsUid));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Bag.Any(item => item.Uid == BellowsUid), Is.False);
            Assert.That(_run.Gold, Is.EqualTo(StartGold + BellowsPrice * SellRatePercent / 100));
        }

        [Test]
        public void Apply_SellScrap_AddsScrapSellPrice()
        {
            Apply(new Sell(ScrapUid));

            Assert.That(_run.Gold, Is.EqualTo(StartGold + ScrapSellPrice));
        }

        [Test]
        public void Apply_SellHighTierProduct_UsesItsMarketPrice()
        {
            Apply(new Sell(StarsteelUid));

            Assert.That(_run.Gold, Is.EqualTo(StartGold + StarsteelPrice * SellRatePercent / 100));
        }

        [Test]
        public void Apply_Sell_KeepsOtherBagOrder()
        {
            var result = Apply(new Sell(BellowsUid));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Bag.Select(item => item.Uid), Is.EqualTo(new[] { 1, 3, 4, 5 }));
        }

        [Test]
        public void Apply_Sell_DoesNotDrawRandom()
        {
            var before = _run.Rng.Capture();

            var result = Apply(new Sell(SteelUid));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Rng.Capture(), Is.EqualTo(before));
        }

        [Test]
        public void Apply_SellUnknownUid_RejectsWithItemNotFound()
        {
            AssertRejectedUnchanged(new Sell(99), RejectReason.ItemNotFound);
        }

        [Test]
        public void Apply_SellWhenResultPending_RejectsWithGridLocked()
        {
            _run.WorkbenchStep = WorkbenchStep.ResultPending;

            AssertRejectedUnchanged(new Sell(SteelUid), RejectReason.GridLocked);
        }

        [Test]
        public void Apply_SellOutsideWorkbench_RejectsWithWrongPhase()
        {
            _run.Phase = RunPhase.Decree;

            AssertRejectedUnchanged(new Sell(SteelUid), RejectReason.WrongPhase);
        }

        [Test]
        public void Apply_SellWithoutRun_RejectsWithRunNotActive()
        {
            var result = GameRules.Apply(new RunState(), _meta, _content, new Sell(SteelUid));

            Assert.That(result.Reason, Is.EqualTo(RejectReason.RunNotActive));
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
