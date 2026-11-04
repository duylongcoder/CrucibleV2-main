using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class GameRulesRerollTest
    {
        private const int StartGold = 20;

        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = StateFactory.CreateWorkbenchRun(_content, new[] { Coal });
            _run.Gold = StartGold;
            StateFactory.StockShop(_run, _content, Iron, Coal, Copper, Ash, Lens);
        }

        [Test]
        public void Apply_RerollThreeTimes_ChargesTwoThreeFour()
        {
            var spent = new int[3];
            for (var i = 0; i < spent.Length; i++)
            {
                var before = _run.Gold;
                Assert.That(Apply(new Reroll()).Accepted, Is.True, $"reroll {i}");
                spent[i] = before - _run.Gold;
            }

            Assert.That(spent, Is.EqualTo(new[] { 2, 3, 4 }));
        }

        [Test]
        public void Apply_Reroll_RestocksSoldSlots()
        {
            Assert.That(Apply(new Buy(0)).Accepted, Is.True);

            var result = Apply(new Reroll());

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
            Assert.That(_run.Shop.Slots.All(slot => slot.IsAvailable), Is.True);
        }

        [Test]
        public void Apply_Reroll_EmitsShopRerolledWithPrice()
        {
            var result = Apply(new Reroll());

            var rerolled = (ShopRerolled)result.Events.Single();
            Assert.That(rerolled.Price, Is.EqualTo(RerollBase));
        }

        [Test]
        public void Apply_Reroll_OnlyAdvancesShopStream()
        {
            var before = _run.Rng.Capture();

            var result = Apply(new Reroll());

            Assert.That(result.Accepted, Is.True);
            MarketRulesShopTest.AssertOnlyStreamAdvanced(before, _run.Rng.Capture(), RngStream.Shop);
        }

        [Test]
        public void Apply_RerollWithGoldBelowPrice_RejectsWithNotEnoughGold()
        {
            _run.Gold = RerollBase - 1;

            AssertRejectedUnchanged(new Reroll(), RejectReason.NotEnoughGold);
        }

        [Test]
        public void Apply_RerollWhenResultPending_RejectsWithGridLocked()
        {
            _run.WorkbenchStep = WorkbenchStep.ResultPending;

            AssertRejectedUnchanged(new Reroll(), RejectReason.GridLocked);
        }

        [Test]
        public void Apply_RerollOutsideWorkbench_RejectsWithWrongPhase()
        {
            _run.Phase = RunPhase.Spoils;

            AssertRejectedUnchanged(new Reroll(), RejectReason.WrongPhase);
        }

        [Test]
        public void Apply_RerollWithoutRun_RejectsWithRunNotActive()
        {
            var result = GameRules.Apply(new RunState(), _meta, _content, new Reroll());

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
