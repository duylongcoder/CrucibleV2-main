using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><c>DismissCourtier</c> qua <see cref="GameRules.Apply"/> (forge-battle R5, D3; run-flow AC-7).</summary>
    public class GameRulesDismissCourtierTest
    {
        private ContentDb _content;
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
        }

        [Test]
        public void Apply_DismissWithNoProduct_EntersBattleWithForfeitLog()
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { Iron, Scrap });

            var result = Apply(run, new DismissCourtier());

            Assert.That(result.Accepted, Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Battle));
            Assert.That(run.LastBattle.EndReason, Is.EqualTo(BattleEndReason.Forfeit));
            Assert.That(run.LastBattle.HeroWon, Is.False);
            Assert.That(run.LastBattle.Equipment, Is.EqualTo(run.DecreeEquipment));
        }

        [Test]
        public void Apply_DismissWithNoProduct_KeepsBag()
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { Iron, Scrap });

            Assert.That(Apply(run, new DismissCourtier()).Accepted, Is.True, "arrange: dismiss");

            Assert.That(run.Bag.Select(item => item.Uid), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Apply_DismissWithEmptyBag_IsAccepted()
        {
            var run = StateFactory.CreateOfferingRun(_content, new string[0]);

            Assert.That(Apply(run, new DismissCourtier()).Accepted, Is.True);
        }

        [Test]
        public void Apply_DismissWithNoProduct_EmitsCourtierDismissedThenBattleStarted()
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { Iron });

            var result = Apply(run, new DismissCourtier());

            Assert.That(result.Events.Select(e => e.GetType()),
                Is.EqualTo(new[] { typeof(CourtierDismissed), typeof(BattleStarted) }));
            Assert.That(((BattleStarted)result.Events[1]).Log, Is.SameAs(run.LastBattle));
        }

        [Test]
        public void Apply_DismissWithProduct_RejectsHasOfferableItem()
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { Iron, CastIron });
            var before = StateHasher.Hash(run, _meta);

            var result = Apply(run, new DismissCourtier());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.HasOfferableItem));
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        [TestCase(RunPhase.Decree)]
        [TestCase(RunPhase.Workbench)]
        [TestCase(RunPhase.Battle)]
        public void Apply_Dismiss_RejectsOutsideOffering(RunPhase phase)
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { Iron });
            run.Phase = phase;
            var before = StateHasher.Hash(run, _meta);

            var result = Apply(run, new DismissCourtier());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.WrongPhase));
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_Dismiss_RejectsWhenNotActive()
        {
            Assert.That(Apply(new RunState(), new DismissCourtier()).Reason, Is.EqualTo(RejectReason.RunNotActive));
        }

        private CommandResult Apply(RunState run, ICommand command) => GameRules.Apply(run, _meta, _content, command);
    }
}
