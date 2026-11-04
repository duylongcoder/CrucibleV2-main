using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><c>AckBattle</c> → Resolution qua <see cref="GameRules.Apply"/> (forge-battle R9, D6, D7, D11; run-flow AC-5).</summary>
    public class GameRulesAckBattleTest
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
        public void Apply_AckLossWithReputationLeft_LosesOneReputation()
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(1), season: 1, reputation: 3);

            var result = Apply(run, new AckBattle());

            Assert.That(run.Reputation, Is.EqualTo(2));
            Assert.That(result.Events.OfType<ReputationLost>().Single().Remaining, Is.EqualTo(2));
        }

        [Test]
        public void Apply_AckLossWithReputationLeft_EntersNextSeasonDecree()
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(3), season: 3, reputation: 2);

            var result = Apply(run, new AckBattle());

            Assert.That(run.Season, Is.EqualTo(4));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Decree));
            Assert.That(run.Turn, Is.EqualTo(0));
            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.None));
            Assert.That(result.Events.OfType<SeasonStarted>().Single().Season, Is.EqualTo(4));
        }

        [Test]
        public void Apply_AckLossWithReputationLeft_RollsNextSeasonDecree()
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(1), season: 1, reputation: 3);
            var market = run.Rng.Get(RngStream.Market);
            var expectedPrice = SeasonPriceMin + market.NextInt(SeasonPriceMax - SeasonPriceMin + 1);
            var forge = run.Rng.Get(RngStream.Forge);
            var expectedEquipment = ForgeRulesTest.ExpectedEquipment(_content, 2, ref forge);

            Apply(run, new AckBattle());

            Assert.That(run.SeasonPricePercent, Is.EqualTo(expectedPrice));
            Assert.That(run.DecreeEquipment, Is.EqualTo(expectedEquipment));
        }

        [Test]
        public void Apply_AckLossAtReputationOne_ExilesRun()
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(2), season: 2, reputation: 1);

            var result = Apply(run, new AckBattle());

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Ended));
            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Exiled));
            Assert.That(run.Reputation, Is.EqualTo(0));
            Assert.That(run.Season, Is.EqualTo(2));
            Assert.That(result.Events.OfType<RunEnded>().Single().Outcome, Is.EqualTo(RunOutcome.Exiled));
        }

        [Test]
        public void Apply_AckLossInLastSeasonAtReputationOne_Exiles()
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(SeasonCount), season: SeasonCount, reputation: 1);

            Apply(run, new AckBattle());

            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Exiled));
        }

        [Test]
        public void Apply_AckLossInLastSeasonWithReputationLeft_WinsRun()
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(SeasonCount), season: SeasonCount, reputation: 2);

            var result = Apply(run, new AckBattle());

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Ended));
            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Won));
            Assert.That(run.Reputation, Is.EqualTo(1));
            Assert.That(result.Events.OfType<RunEnded>().Single().Outcome, Is.EqualTo(RunOutcome.Won));
        }

        [Test]
        public void Apply_AckWinInLastSeason_WinsRun()
        {
            var run = StateFactory.CreateBattleRun(_content, WinLog(SeasonCount), season: SeasonCount, reputation: 1);

            Apply(run, new AckBattle());

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Ended));
            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Won));
            Assert.That(run.Reputation, Is.EqualTo(1));
        }

        [Test]
        public void Apply_AckWinBeforeLastSeason_EntersSpoils()
        {
            var run = StateFactory.CreateBattleRun(_content, WinLog(5), season: 5, reputation: 2);

            var result = Apply(run, new AckBattle());

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Spoils));
            Assert.That(run.Season, Is.EqualTo(5));
            Assert.That(result.Events.OfType<SpoilsStarted>().Single().Season, Is.EqualTo(5));
        }

        [Test]
        public void Apply_AckWin_KeepsReputation()
        {
            var run = StateFactory.CreateBattleRun(_content, WinLog(1), season: 1, reputation: 3);

            var result = Apply(run, new AckBattle());

            Assert.That(result.Accepted, Is.True);
            Assert.That(run.Reputation, Is.EqualTo(3));
            Assert.That(result.Events.OfType<ReputationLost>(), Is.Empty);
        }

        [Test]
        public void Apply_AckForfeit_CountsAsLoss()
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(1), season: 1, reputation: 3);
            Assert.That(run.LastBattle.EndReason, Is.EqualTo(BattleEndReason.Forfeit), "arrange");

            Apply(run, new AckBattle());

            Assert.That(run.Reputation, Is.EqualTo(2));
        }

        [Test]
        public void Apply_Ack_KeepsLastBattle()
        {
            var log = WinLog(1);
            var run = StateFactory.CreateBattleRun(_content, log, season: 1);

            Assert.That(Apply(run, new AckBattle()).Accepted, Is.True, "arrange: ack");

            Assert.That(run.LastBattle, Is.Not.Null.And.SameAs(log));
        }

        [TestCase(RunPhase.Decree)]
        [TestCase(RunPhase.Workbench)]
        [TestCase(RunPhase.Offering)]
        [TestCase(RunPhase.Spoils)]
        [TestCase(RunPhase.Ended)]
        public void Apply_Ack_RejectsOutsideBattle(RunPhase phase)
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(1));
            run.Phase = phase;
            var before = StateHasher.Hash(run, _meta);

            var result = Apply(run, new AckBattle());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.WrongPhase));
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_Ack_RejectsWhenNotActive()
        {
            Assert.That(Apply(new RunState(), new AckBattle()).Reason, Is.EqualTo(RejectReason.RunNotActive));
        }

        [TestCase("AcceptDecree")]
        [TestCase("SkipTurn")]
        [TestCase("Offer")]
        [TestCase("DismissCourtier")]
        [TestCase("AckBattle")]
        public void Apply_AnyRunCommandAfterEnded_RejectsWrongPhase(string commandName)
        {
            var run = StateFactory.CreateBattleRun(_content, LossLog(2), season: 2, reputation: 1);
            Apply(run, new AckBattle());
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Ended), "arrange");
            ICommand command = commandName switch
            {
                "AcceptDecree" => new AcceptDecree(),
                "SkipTurn" => new SkipTurn(),
                "Offer" => new Offer(1),
                "DismissCourtier" => new DismissCourtier(),
                _ => new AckBattle(),
            };

            Assert.That(Apply(run, command).Reason, Is.EqualTo(RejectReason.WrongPhase));
        }

        private BattleLog LossLog(int season) => BattleLog.Forfeit(season, EquipmentOf(_content, Sword),
            new Combatant(HeroBaseHp, HeroBaseAtk, 0, 0, 0), new Combatant(50, 5, 0, 0, 0));

        private BattleLog WinLog(int season) => BattleSimulator.Simulate(season, EquipmentOf(_content, Sword),
            new ItemInstance(90, IdOf(_content, Steel), quality: 40), new Combatant(1000, 1000, 0, 0, 0),
            new Combatant(1, 0, 0, 0, 0), _content.Battle);

        private CommandResult Apply(RunState run, ICommand command) => GameRules.Apply(run, _meta, _content, command);
    }
}
