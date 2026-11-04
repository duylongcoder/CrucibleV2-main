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
    public class GameRulesReorderCatalystTest
    {
        // Túi: uid 1 Sắt, 2 C1 (Bễ), 3 Than, 4 C2 (Cối), 5 C3 (Kẹp); lưới ô 0: uid 6 Sắt.
        private const int IronUid = 1;
        private const int C1 = 2;
        private const int C2 = 4;
        private const int C3 = 5;
        private const int GridUid = 6;

        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron, Bellows, Coal, Mortar, Tongs }, new[] { Iron });
        }

        [Test]
        public void Apply_MoveCatalystForward_ReordersCatalystsOnly()
        {
            var result = Apply(new ReorderCatalyst(C1, 2));

            Assert.That(result.Accepted, Is.True);
            Assert.That(BagUids(), Is.EqualTo(new[] { 1, 4, 3, 5, 2 }));
            Assert.That(result.Events.Single(), Is.TypeOf<CatalystsReordered>());
        }

        [Test]
        public void Apply_MoveCatalystBackward_ReordersCatalystsOnly()
        {
            Apply(new ReorderCatalyst(C3, 0));

            Assert.That(BagUids(), Is.EqualTo(new[] { 1, 5, 3, 2, 4 }));
        }

        [Test]
        public void Apply_MoveCatalystToAdjacent_SwapsPair()
        {
            Apply(new ReorderCatalyst(C2, 0));

            Assert.That(BagUids(), Is.EqualTo(new[] { 1, 4, 3, 2, 5 }));
        }

        [Test]
        public void Apply_ReorderCatalyst_KeepsNonCatalystIndices()
        {
            var result = Apply(new ReorderCatalyst(C3, 0));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Bag[0].ItemId, Is.EqualTo(IdOf(_content, Iron)));
            Assert.That(_run.Bag[2].ItemId, Is.EqualTo(IdOf(_content, Coal)));
        }

        [Test]
        public void Apply_ReorderCatalyst_KeepsCountsAndCapacity()
        {
            var capacity = _run.BagCapacity;

            var result = Apply(new ReorderCatalyst(C1, 2));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Bag.Count, Is.EqualTo(5));
            Assert.That(_run.BagCapacity, Is.EqualTo(capacity));
            Assert.That(_run.Grid[0].Uid, Is.EqualTo(GridUid));
        }

        [Test]
        public void Apply_ReorderCatalyst_DoesNotDrawRandom()
        {
            var before = CaptureRng();

            var result = Apply(new ReorderCatalyst(C1, 2));

            Assert.That(result.Accepted, Is.True);
            Assert.That(CaptureRng(), Is.EqualTo(before));
        }

        [Test]
        public void Apply_ReorderCatalystSameIndex_RejectsWithInvalidCatalystIndex()
        {
            AssertRejectedUnchanged(new ReorderCatalyst(C2, 1), RejectReason.InvalidCatalystIndex);
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void Apply_ReorderCatalystIndexOutOfRange_RejectsWithInvalidCatalystIndex(int toIndex)
        {
            AssertRejectedUnchanged(new ReorderCatalyst(C1, toIndex), RejectReason.InvalidCatalystIndex);
        }

        [Test]
        public void Apply_ReorderCatalystNotCatalyst_RejectsWithItemNotCatalyst()
        {
            AssertRejectedUnchanged(new ReorderCatalyst(IronUid, 0), RejectReason.ItemNotCatalyst);
        }

        [Test]
        public void Apply_ReorderCatalystUidOnGrid_RejectsWithItemNotFound()
        {
            AssertRejectedUnchanged(new ReorderCatalyst(GridUid, 0), RejectReason.ItemNotFound);
        }

        [Test]
        public void Apply_ReorderCatalystUnknownUid_RejectsWithItemNotFound()
        {
            AssertRejectedUnchanged(new ReorderCatalyst(99, 0), RejectReason.ItemNotFound);
        }

        [Test]
        public void Apply_ReorderCatalystWhenResultPending_RejectsWithGridLocked()
        {
            _run.WorkbenchStep = WorkbenchStep.ResultPending;

            AssertRejectedUnchanged(new ReorderCatalyst(C1, 2), RejectReason.GridLocked);
        }

        [Test]
        public void Apply_ReorderCatalystOutsideWorkbench_RejectsWithWrongPhase()
        {
            _run.Phase = RunPhase.Offering;

            AssertRejectedUnchanged(new ReorderCatalyst(C1, 2), RejectReason.WrongPhase);
        }

        [Test]
        public void Apply_ReorderCatalystWithoutRun_RejectsWithRunNotActive()
        {
            var result = GameRules.Apply(new RunState(), _meta, _content, new ReorderCatalyst(C1, 2));

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(RejectReason.RunNotActive));
        }

        private CommandResult Apply(ICommand command) => GameRules.Apply(_run, _meta, _content, command);

        private int[] BagUids() => _run.Bag.Select(item => item.Uid).ToArray();

        private void AssertRejectedUnchanged(ICommand command, RejectReason reason)
        {
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(command);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(reason));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        private ulong[] CaptureRng()
        {
            var states = new ulong[RngStreams.Count * 2];
            for (var i = 0; i < RngStreams.Count; i++)
            {
                var state = _run.Rng.Get((RngStream)i).State;
                states[i * 2] = state.State;
                states[i * 2 + 1] = state.Inc;
            }

            return states;
        }
    }
}
