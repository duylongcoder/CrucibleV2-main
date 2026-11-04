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
    /// <summary><see cref="AckBrewResult"/> (run-loop R5, R6).</summary>
    public class GameRulesAckBrewResultTest
    {
        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Coal });
            StateFactory.StockShop(_run, _content, Iron, Coal, Copper, Ash, Lens);
        }

        [Test]
        public void Apply_AckAfterBrewOnTurnOne_StartsTurnTwoEditing()
        {
            BrewNow();

            var result = Apply(new AckBrewResult());

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Turn, Is.EqualTo(2));
            Assert.That(_run.WorkbenchStep, Is.EqualTo(WorkbenchStep.Editing));
            Assert.That(result.Events.OfType<TurnStarted>().Single().Turn, Is.EqualTo(2));
        }

        [Test]
        public void Apply_AckAfterBrew_ReopensGridCommands()
        {
            BrewNow();
            Apply(new AckBrewResult());

            var result = Apply(new PlaceItem(_run.LastBrewResultUid, 0));

            Assert.That(result.Accepted, Is.True);
        }

        [Test]
        public void Apply_AckAfterBrew_KeepsBrewResultInBag()
        {
            BrewNow();
            var resultUid = _run.LastBrewResultUid;

            Assert.That(Apply(new AckBrewResult()).Accepted, Is.True);
            Assert.That(BagRules.IndexOfUid(_run, resultUid), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Apply_AckAfterBrew_KeepsLastBrew()
        {
            BrewNow();
            var log = _run.LastBrew;
            var resultUid = _run.LastBrewResultUid;

            Assert.That(Apply(new AckBrewResult()).Accepted, Is.True);
            Assert.That(_run.LastBrew, Is.SameAs(log));
            Assert.That(_run.LastBrewResultUid, Is.EqualTo(resultUid));
        }

        [Test]
        public void Apply_AckOnLastTurn_EntersOffering()
        {
            _run.Turn = TurnsPerSeason;
            BrewNow();

            var result = Apply(new AckBrewResult());

            Assert.That(_run.Phase, Is.EqualTo(RunPhase.Offering));
            Assert.That(_run.Turn, Is.EqualTo(0));
            Assert.That(result.Events.OfType<OfferingStarted>().Single().Season, Is.EqualTo(1));
        }

        [Test]
        public void Apply_AckWhileEditing_RejectsNoPendingResult()
        {
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(new AckBrewResult());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.NoPendingResult));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_AckOutsideWorkbench_RejectsWrongPhase([Values(RunPhase.Decree, RunPhase.Offering)] RunPhase phase)
        {
            BrewNow();
            _run.Phase = phase;
            _run.Turn = 0;
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(new AckBrewResult());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.WrongPhase));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_AckWithoutRun_RejectsRunNotActive()
        {
            _run = new RunState();

            var result = Apply(new AckBrewResult());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.RunNotActive));
        }

        private void BrewNow()
        {
            Assert.That(Apply(new Brew(1793750400)).Accepted, Is.True, "arrange: brew");
        }

        private CommandResult Apply(ICommand command) => GameRules.Apply(_run, _meta, _content, command);
    }
}
