using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;

namespace Crucible.Tests.Core.Rules
{
    public class GameRulesStartRunTest
    {
        private const ulong Seed = 424242;
        private const long NowUnix = 1793750400;

        private ContentDb _content;
        private RunState _run;
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _run = new RunState();
            _meta = new MetaState();
        }

        [Test]
        public void Apply_ValidStartRun_EntersSeasonOneDecree()
        {
            var result = GameRules.Apply(_run, _meta, _content, ValidStartRun());

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.IsActive, Is.True);
            Assert.That(_run.Season, Is.EqualTo(1));
            Assert.That(_run.Phase, Is.EqualTo(RunPhase.Decree));
            Assert.That(_run.Gold, Is.EqualTo(0));
            Assert.That(_run.Reputation, Is.EqualTo(ContentFactory.StartingReputation));
            Assert.That(_run.BagCapacity, Is.EqualTo(ContentFactory.StartingBagCapacity));
            Assert.That(_run.Seed, Is.EqualTo(Seed));
        }

        [Test]
        public void Apply_ValidStartRun_FillsBagWithStarterKitInOrder()
        {
            var expected = ContentFactory.StarterKitItems.Select(id =>
            {
                _content.TryFindItem(id, out var itemId);
                return itemId;
            }).ToArray();

            GameRules.Apply(_run, _meta, _content, ValidStartRun());

            Assert.That(_run.Bag.Select(item => item.ItemId).ToArray(), Is.EqualTo(expected));
        }

        [Test]
        public void Apply_ValidStartRun_AssignsStrictlyIncreasingUids()
        {
            GameRules.Apply(_run, _meta, _content, ValidStartRun());

            var uids = _run.Bag.Select(item => item.Uid).ToArray();
            Assert.That(uids, Is.Ordered.Ascending);
            Assert.That(uids, Is.Unique);
            Assert.That(_run.NextUid, Is.GreaterThan(uids.Max()));
        }

        [Test]
        public void Apply_ValidStartRun_RaisesRunStarted()
        {
            var result = GameRules.Apply(_run, _meta, _content, ValidStartRun());

            Assert.That(result.Events.Count, Is.EqualTo(1));
            Assert.That(result.Events[0], Is.TypeOf<RunStarted>());
        }

        [Test]
        public void Apply_UnknownDifficulty_RejectsWithUnknownContent()
        {
            var command = new StartRun("diff_missing", ContentFactory.StarterKit, Seed, NowUnix);

            var result = GameRules.Apply(_run, _meta, _content, command);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(RejectReason.UnknownContent));
        }

        [Test]
        public void Apply_UnknownStarterKit_RejectsWithUnknownContent()
        {
            var command = new StartRun(ContentFactory.Difficulty, "kit_missing", Seed, NowUnix);

            var result = GameRules.Apply(_run, _meta, _content, command);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(RejectReason.UnknownContent));
        }

        [Test]
        public void Apply_StartRunWhileRunActive_RejectsWithRunAlreadyActive()
        {
            GameRules.Apply(_run, _meta, _content, ValidStartRun());

            var result = GameRules.Apply(_run, _meta, _content, ValidStartRun());

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(RejectReason.RunAlreadyActive));
        }

        [Test]
        public void Apply_UnregisteredCommandType_RejectsWithUnknownCommand()
        {
            var result = GameRules.Apply(_run, _meta, _content, new UnregisteredCommand());

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(RejectReason.UnknownCommand));
        }

        [Test]
        public void Apply_RejectedCommand_LeavesStateHashUnchanged()
        {
            GameRules.Apply(_run, _meta, _content, ValidStartRun());
            var before = StateHasher.Hash(_run, _meta);

            var result = GameRules.Apply(_run, _meta, _content, ValidStartRun());

            Assert.That(result.Accepted, Is.False);
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_SameSeedOnFreshStates_ProducesSameStateHash()
        {
            var otherRun = new RunState();
            var otherMeta = new MetaState();

            GameRules.Apply(_run, _meta, _content, ValidStartRun());
            GameRules.Apply(otherRun, otherMeta, _content, ValidStartRun());

            Assert.That(StateHasher.Hash(otherRun, otherMeta), Is.EqualTo(StateHasher.Hash(_run, _meta)));
        }

        private static StartRun ValidStartRun() =>
            new StartRun(ContentFactory.Difficulty, ContentFactory.StarterKit, Seed, NowUnix);

        private sealed class UnregisteredCommand : ICommand
        {
        }
    }
}
