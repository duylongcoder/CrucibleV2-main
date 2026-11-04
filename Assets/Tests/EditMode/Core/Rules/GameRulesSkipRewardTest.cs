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
    /// <summary><c>SkipReward</c> qua <see cref="GameRules.Apply"/> — luôn dùng được ở Chiến lợi phẩm (spoils R6, R7, D4).</summary>
    public class GameRulesSkipRewardTest
    {
        private ContentDb _content;
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _content = CreateWithSpoils();
            _meta = new MetaState();
        }

        [Test]
        public void Apply_SkipWithNoOptions_EntersNextSeasonDecree()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 4);

            var result = Apply(run, new SkipReward());

            Assert.That(run.Season, Is.EqualTo(5));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Decree));
            Assert.That(result.Events.OfType<SeasonStarted>().Single().Season, Is.EqualTo(5));
        }

        [Test]
        public void Apply_SkipWithOptions_EntersNextSeasonWithoutReward()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 4, RewardBag, RewardMithril);
            var capacity = run.BagCapacity;

            Apply(run, new SkipReward());

            Assert.That(run.Season, Is.EqualTo(5));
            Assert.That(run.Bag.Count, Is.EqualTo(1));
            Assert.That(run.BagCapacity, Is.EqualTo(capacity));
            Assert.That(run.SpoilsOptions, Is.Empty);
        }

        [Test]
        public void Apply_Skip_EmitsRewardSkippedThenSeasonStarted()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 4, RewardLens);

            var result = Apply(run, new SkipReward());

            Assert.That(result.Events.Count, Is.EqualTo(2));
            Assert.That(((RewardSkipped)result.Events[0]).Season, Is.EqualTo(4));
            Assert.That(result.Events[1], Is.TypeOf<SeasonStarted>());
        }

        [Test]
        public void Apply_Skip_KeepsSpoilsGold()
        {
            var run = StateFactory.CreateWonBattleRun(_content, season: 2);
            Apply(run, new AckBattle());
            var gold = run.Gold;

            Apply(run, new SkipReward());

            Assert.That(run.Gold, Is.EqualTo(gold));
            Assert.That(gold, Is.EqualTo(2 * SpoilsGoldPerSeason), "arrange");
        }

        [TestCase(RunPhase.Decree)]
        [TestCase(RunPhase.Offering)]
        [TestCase(RunPhase.Battle)]
        [TestCase(RunPhase.Ended)]
        public void Apply_SkipOutsideSpoils_RejectsWrongPhase(RunPhase phase)
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 4, RewardLens);
            run.Phase = phase;
            var before = StateHasher.Hash(run, _meta);

            var result = GameRules.Apply(run, _meta, _content, new SkipReward());

            Assert.That(result.Reason, Is.EqualTo(RejectReason.WrongPhase));
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_SkipWhenNotActive_RejectsRunNotActive()
        {
            Assert.That(GameRules.Apply(new RunState(), _meta, _content, new SkipReward()).Reason,
                Is.EqualTo(RejectReason.RunNotActive));
        }

        private CommandResult Apply(RunState run, ICommand command)
        {
            var result = GameRules.Apply(run, _meta, _content, command);
            Assert.That(result.Accepted, Is.True, $"arrange: rejected {result.Reason}");
            return result;
        }
    }
}
