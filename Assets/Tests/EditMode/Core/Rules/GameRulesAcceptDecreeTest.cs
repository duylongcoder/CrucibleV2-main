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
    /// <summary><see cref="AcceptDecree"/> (run-loop R2, R3).</summary>
    public class GameRulesAcceptDecreeTest
    {
        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = StateFactory.CreateDecreeRun();
        }

        [Test]
        public void Apply_AcceptDecreeInDecree_EntersWorkbenchTurnOne()
        {
            var result = Apply(new AcceptDecree());

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Phase, Is.EqualTo(RunPhase.Workbench));
            Assert.That(_run.Turn, Is.EqualTo(1));
            Assert.That(_run.WorkbenchStep, Is.EqualTo(WorkbenchStep.Editing));
        }

        [Test]
        public void Apply_AcceptDecree_StocksShop()
        {
            _run.Shop.RerollCount = 2;

            Apply(new AcceptDecree());

            Assert.That(_run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
            Assert.That(_run.Shop.Slots.All(slot => slot.IsAvailable), Is.True);
            Assert.That(_run.Shop.RerollCount, Is.EqualTo(0));
        }

        [Test]
        public void Apply_AcceptDecree_EmitsDecreeAcceptedThenTurnStarted()
        {
            var events = Apply(new AcceptDecree()).Events;

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0], Is.TypeOf<DecreeAccepted>());
            Assert.That(((DecreeAccepted)events[0]).Season, Is.EqualTo(1));
            Assert.That(events[1], Is.TypeOf<TurnStarted>());
            Assert.That(((TurnStarted)events[1]).Season, Is.EqualTo(1));
            Assert.That(((TurnStarted)events[1]).Turn, Is.EqualTo(1));
        }

        [Test]
        public void Apply_AcceptDecree_OnlyAdvancesShopStream()
        {
            var before = _run.Rng.Capture();

            Apply(new AcceptDecree());

            MarketRulesShopTest.AssertOnlyStreamAdvanced(before, _run.Rng.Capture(), RngStream.Shop);
        }

        [Test]
        public void Apply_AcceptDecree_KeepsSeasonPrice()
        {
            _run.SeasonPricePercent = 117;

            var result = Apply(new AcceptDecree());

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.SeasonPricePercent, Is.EqualTo(117));
        }

        [Test]
        public void Apply_AcceptDecreeWithoutRun_RejectsRunNotActive()
        {
            _run = new RunState();
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(new AcceptDecree());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.RunNotActive));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_AcceptDecreeOutsideDecree_RejectsWrongPhase(
            [Values(RunPhase.Workbench, RunPhase.Offering, RunPhase.Battle, RunPhase.Spoils, RunPhase.Ended)] RunPhase phase)
        {
            _run.Phase = phase;
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(new AcceptDecree());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.WrongPhase));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        private CommandResult Apply(ICommand command) => GameRules.Apply(_run, _meta, _content, command);
    }
}
