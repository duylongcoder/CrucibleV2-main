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
    /// <summary><c>ChooseReward</c> qua <see cref="GameRules.Apply"/> (spoils R5, R7).</summary>
    public class GameRulesChooseRewardTest
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
        public void Apply_ChooseItemReward_AddsItemToBagEnd()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron, Coal }, 2, RewardLens, RewardMithril);
            var uid = run.NextUid;

            Apply(run, new ChooseReward(1));

            Assert.That(run.Bag.Count, Is.EqualTo(3));
            Assert.That(run.Bag[2].ItemId, Is.EqualTo(IdOf(_content, Mithril)));
            Assert.That(run.Bag[2].Uid, Is.EqualTo(uid));
            Assert.That(run.Bag[2].Magic, Is.False);
            Assert.That(run.Bag[2].Quality, Is.EqualTo(0));
        }

        [Test]
        public void Apply_ChooseItemReward_IncrementsNextUid()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2, RewardLens);
            var uid = run.NextUid;

            Apply(run, new ChooseReward(0));

            Assert.That(run.NextUid, Is.EqualTo(uid + 1));
        }

        [Test]
        public void Apply_ChooseExpandBag_IncreasesCapacityByAmount()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2, RewardBag);
            run.BagCapacity = 8;
            var uid = run.NextUid;

            Apply(run, new ChooseReward(0));

            Assert.That(run.BagCapacity, Is.EqualTo(8 + RewardBagAmount));
            Assert.That(run.Bag.Count, Is.EqualTo(1));
            Assert.That(run.NextUid, Is.EqualTo(uid));
        }

        [Test]
        public void Apply_ChooseExpandBagNearCap_ClampsToMax()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2, RewardBag);
            run.BagCapacity = MaxBagCapacity - 1;

            var result = Apply(run, new ChooseReward(0));

            Assert.That(run.BagCapacity, Is.EqualTo(MaxBagCapacity));
            Assert.That(result.Events.OfType<RewardChosen>().Single().BagAdded, Is.EqualTo(1));
        }

        [Test]
        public void Apply_Choose_EntersNextSeasonDecree()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2, RewardLens, RewardSilver);

            Apply(run, new ChooseReward(0));

            Assert.That(run.Season, Is.EqualTo(3));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Decree));
            Assert.That(run.Turn, Is.EqualTo(0));
            Assert.That(run.SpoilsOptions, Is.Empty);
        }

        [Test]
        public void Apply_Choose_EmitsRewardChosenThenSeasonStarted()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2, RewardLens, RewardSilver);
            var uid = run.NextUid;

            var result = Apply(run, new ChooseReward(1));

            Assert.That(result.Events.Count, Is.EqualTo(2));
            var chosen = (RewardChosen)result.Events[0];
            Assert.That(chosen.Index, Is.EqualTo(1));
            Assert.That(chosen.Reward, Is.EqualTo(RewardOf(_content, RewardSilver)));
            Assert.That(chosen.ItemUid, Is.EqualTo(uid));
            Assert.That(chosen.BagAdded, Is.EqualTo(0));
            Assert.That(((SeasonStarted)result.Events[1]).Season, Is.EqualTo(3));
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void Apply_ChooseInvalidIndex_RejectsWithoutChange(int index)
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2, RewardLens, RewardSilver);

            AssertRejected(run, new ChooseReward(index), RejectReason.InvalidRewardIndex);
        }

        [Test]
        public void Apply_ChooseWithNoOptions_RejectsInvalidRewardIndex()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2);

            AssertRejected(run, new ChooseReward(0), RejectReason.InvalidRewardIndex);
        }

        [Test]
        public void Apply_ChooseItemWhenBagFull_RejectsBagFull()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron, Coal }, 2, RewardLens);
            run.BagCapacity = 2;

            AssertRejected(run, new ChooseReward(0), RejectReason.BagFull);
        }

        [Test]
        public void Apply_ChooseCatalystWhenSlotsFull_RejectsCatalystSlotsFull()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Bellows, Mortar, Tongs, Bellows, Mortar }, 2,
                RewardBellows);

            AssertRejected(run, new ChooseReward(0), RejectReason.CatalystSlotsFull);
        }

        [Test]
        public void Apply_ChooseExpandAtMax_RejectsBagAtMaxCapacity()
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2, RewardBag);
            run.BagCapacity = MaxBagCapacity;

            AssertRejected(run, new ChooseReward(0), RejectReason.BagAtMaxCapacity);
        }

        [TestCase(RunPhase.Decree)]
        [TestCase(RunPhase.Workbench)]
        [TestCase(RunPhase.Offering)]
        [TestCase(RunPhase.Battle)]
        [TestCase(RunPhase.Ended)]
        public void Apply_ChooseOutsideSpoils_RejectsWrongPhase(RunPhase phase)
        {
            var run = StateFactory.CreateSpoilsRun(_content, new[] { Iron }, 2, RewardLens);
            run.Phase = phase;

            AssertRejected(run, new ChooseReward(0), RejectReason.WrongPhase);
        }

        [Test]
        public void Apply_ChooseWhenNotActive_RejectsRunNotActive()
        {
            Assert.That(GameRules.Apply(new RunState(), _meta, _content, new ChooseReward(0)).Reason,
                Is.EqualTo(RejectReason.RunNotActive));
        }

        private void AssertRejected(RunState run, ICommand command, RejectReason reason)
        {
            var before = StateHasher.Hash(run, _meta);

            var result = GameRules.Apply(run, _meta, _content, command);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(reason));
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        private CommandResult Apply(RunState run, ICommand command)
        {
            var result = GameRules.Apply(run, _meta, _content, command);
            Assert.That(result.Accepted, Is.True, $"arrange: rejected {result.Reason}");
            return result;
        }
    }
}
