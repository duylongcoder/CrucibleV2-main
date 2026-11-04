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
    /// <summary><c>Inspect(target, lens)</c> qua <see cref="GameRules.Apply"/> (inspect R5, R6, D3, D6, D7, D8; GDD §6.2 AC-1/2/5).</summary>
    public class GameRulesInspectTest
    {
        /// <summary>Quality + 3 dòng phụ = 4 dòng.</summary>
        private static readonly StatLine[] ThreeLines =
        {
            new StatLine(StatKind.Sharpness, 10), new StatLine(StatKind.Reflect, 5), new StatLine(StatKind.Absorb, 3),
        };

        private const int AllFourLines = 0b1111;

        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _meta = new MetaState();
        }

        [Test]
        public void Apply_InspectFourLinesWithLens1_RevealsExactlyOne()
        {
            var (run, content, target, lens) = Arrange(1, ThreeLines);

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(BitCount(target.RevealedMask), Is.EqualTo(1));
        }

        [Test]
        public void Apply_InspectTwice_RevealsDifferentLine()
        {
            for (ulong seed = 0; seed < 50; seed++)
            {
                var (run, content, target, lens) = Arrange(1, ThreeLines, seed);
                var second = StateFactory.AddItem(run, content, Lens);
                Apply(run, content, new Inspect(target.Uid, lens.Uid));
                var first = target.RevealedMask;

                Apply(run, content, new Inspect(target.Uid, second.Uid));

                Assert.That(BitCount(target.RevealedMask), Is.EqualTo(2), $"seed {seed}");
                Assert.That(target.RevealedMask & first, Is.EqualTo(first), $"seed {seed}: kept first line");
            }
        }

        [Test]
        public void Apply_InspectUnrevealedAtMostRevealLines_RevealsAll()
        {
            var (run, content, target, lens) = Arrange(3, ThreeLines);
            target.RevealedMask = 0b0001;

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(target.RevealedMask, Is.EqualTo(AllFourLines));
        }

        [Test]
        public void Apply_InspectRevealAll_DoesNotDrawInspectStream()
        {
            var (run, content, target, lens) = Arrange(3, ThreeLines);
            target.RevealedMask = 0b0001;
            var before = run.Rng.Capture();

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(run.Rng.Capture(), Is.EqualTo(before));
        }

        [Test]
        public void Apply_InspectPartial_DrawsOncePerRevealedLineOnInspectStreamOnly()
        {
            var (run, content, target, lens) = Arrange(2, ThreeLines);
            var before = run.Rng.Capture();
            var expected = run.Rng.Get(RngStream.Inspect);
            expected.NextInt(4);
            expected.NextInt(3);

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(run.Rng.Get(RngStream.Inspect).State, Is.EqualTo(expected.State));
            MarketRulesShopTest.AssertOnlyStreamAdvanced(before, run.Rng.Capture(), RngStream.Inspect);
            Assert.That(BitCount(target.RevealedMask), Is.EqualTo(2));
        }

        [Test]
        public void Apply_InspectPartial_PicksAmongUnrevealedByDraw()
        {
            var (run, content, target, lens) = Arrange(1, ThreeLines);
            target.RevealedMask = 0b0010;
            var copy = run.Rng.Get(RngStream.Inspect);
            var unrevealed = new[] { 0, 2, 3 };
            var expectedBit = 1 << unrevealed[copy.NextInt(3)];

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(target.RevealedMask, Is.EqualTo(0b0010 | expectedBit));
        }

        [Test]
        public void Apply_InspectOneHiddenLineWithLens3_RevealsOneAndConsumesLens()
        {
            var (run, content, target, lens) = Arrange(3, ThreeLines);
            target.RevealedMask = 0b0111;

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(target.RevealedMask, Is.EqualTo(AllFourLines));
            Assert.That(BagRules.IndexOfUid(run, lens.Uid), Is.EqualTo(-1));
        }

        [Test]
        public void Apply_InspectQualityOnlyProduct_RevealsQuality()
        {
            var (run, content, target, lens) = Arrange(1);

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(target.RevealedMask, Is.EqualTo(0b0001));
        }

        [Test]
        public void Apply_Inspect_ConsumesLensOnly()
        {
            var (run, content, target, lens) = Arrange(1, ThreeLines);
            var count = run.Bag.Count;

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(run.Bag.Count, Is.EqualTo(count - 1));
            Assert.That(BagRules.IndexOfUid(run, lens.Uid), Is.EqualTo(-1));
            Assert.That(BagRules.IndexOfUid(run, target.Uid), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Apply_Inspect_EmitsItemInspectedWithNewBitsOnly()
        {
            var (run, content, target, lens) = Arrange(3, ThreeLines);
            target.RevealedMask = 0b0101;

            var result = Apply(run, content, new Inspect(target.Uid, lens.Uid));

            var inspected = (ItemInspected)result.Events.Single();
            Assert.That(inspected.TargetUid, Is.EqualTo(target.Uid));
            Assert.That(inspected.LensUid, Is.EqualTo(lens.Uid));
            Assert.That(inspected.RevealedMask, Is.EqualTo(0b1010));
        }

        [Test]
        public void Apply_InspectMagicProduct_MagicNotCountedAsLine()
        {
            var content = ContentFactory.Create();
            var run = StateFactory.CreateWorkbenchRun(content, new string[0]);
            var target = new ItemInstance(run.NextUid++, IdOf(content, Steel), magic: true, quality: 30);
            run.Bag.Add(target);
            var lens = StateFactory.AddItem(run, content, Lens);

            Apply(run, content, new Inspect(target.Uid, lens.Uid));

            Assert.That(target.RevealedMask, Is.EqualTo(0b0001));
            Assert.That(target.Magic, Is.True);
            Assert.That(InspectRules.LineCount(target), Is.EqualTo(1));
        }

        [Test]
        public void Apply_InspectInOffering_Accepted()
        {
            var content = ContentFactory.Create();
            var run = StateFactory.CreateOfferingRun(content, new string[0]);
            var target = StateFactory.AddProduct(run, content, Steel, 30, ThreeLines);
            var lens = StateFactory.AddItem(run, content, Lens);

            Assert.That(Apply(run, content, new Inspect(target.Uid, lens.Uid)).Accepted, Is.True);
        }

        [Test]
        public void Apply_InspectFullyRevealed_RejectsNothingToRevealAndKeepsLens()
        {
            var (run, content, target, lens) = Arrange(1, ThreeLines);
            target.RevealedMask = AllFourLines;

            AssertRejected(run, content, new Inspect(target.Uid, lens.Uid), RejectReason.NothingToReveal);
            Assert.That(BagRules.IndexOfUid(run, lens.Uid), Is.GreaterThanOrEqualTo(0));
        }

        [TestCase(Iron)]
        [TestCase(Scrap)]
        [TestCase(Bellows)]
        public void Apply_InspectNonProduct_RejectsItemNotInspectable(string itemId)
        {
            var (run, content, _, lens) = Arrange(1, ThreeLines);
            var target = StateFactory.AddItem(run, content, itemId);

            AssertRejected(run, content, new Inspect(target.Uid, lens.Uid), RejectReason.ItemNotInspectable);
        }

        [Test]
        public void Apply_InspectTargetOnGrid_RejectsItemNotFound()
        {
            var content = ContentFactory.Create();
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Lens }, new[] { Steel });
            var lensUid = run.Bag[0].Uid;
            var gridUid = run.Grid[0].Uid;

            AssertRejected(run, content, new Inspect(gridUid, lensUid), RejectReason.ItemNotFound);
        }

        [Test]
        public void Apply_InspectUnknownTarget_RejectsItemNotFound()
        {
            var (run, content, _, lens) = Arrange(1, ThreeLines);

            AssertRejected(run, content, new Inspect(999, lens.Uid), RejectReason.ItemNotFound);
        }

        [Test]
        public void Apply_InspectLensNotInBag_RejectsItemNotFound()
        {
            var (run, content, target, _) = Arrange(1, ThreeLines);

            AssertRejected(run, content, new Inspect(target.Uid, 999), RejectReason.ItemNotFound);
        }

        [Test]
        public void Apply_InspectWithNonLens_RejectsItemNotLens()
        {
            var (run, content, target, _) = Arrange(1, ThreeLines);
            var notLens = StateFactory.AddItem(run, content, Iron);

            AssertRejected(run, content, new Inspect(target.Uid, notLens.Uid), RejectReason.ItemNotLens);
        }

        [Test]
        public void Apply_InspectTargetAsLens_RejectsItemNotLens()
        {
            var (run, content, target, _) = Arrange(1, ThreeLines);

            AssertRejected(run, content, new Inspect(target.Uid, target.Uid), RejectReason.ItemNotLens);
        }

        [Test]
        public void Apply_InspectFullyRevealedWithNonLens_RejectsItemNotLens()
        {
            var (run, content, target, _) = Arrange(1, ThreeLines);
            target.RevealedMask = AllFourLines;
            var notLens = StateFactory.AddItem(run, content, Iron);

            AssertRejected(run, content, new Inspect(target.Uid, notLens.Uid), RejectReason.ItemNotLens);
        }

        [Test]
        public void Apply_InspectNonProductWithNonLens_RejectsItemNotInspectable()
        {
            var (run, content, _, _) = Arrange(1, ThreeLines);
            var raw = StateFactory.AddItem(run, content, Iron);
            var notLens = StateFactory.AddItem(run, content, Coal);

            AssertRejected(run, content, new Inspect(raw.Uid, notLens.Uid), RejectReason.ItemNotInspectable);
        }

        [Test]
        public void Apply_InspectWhileResultPending_RejectsGridLocked()
        {
            var (run, content, target, lens) = Arrange(1, ThreeLines);
            run.WorkbenchStep = WorkbenchStep.ResultPending;

            AssertRejected(run, content, new Inspect(target.Uid, lens.Uid), RejectReason.GridLocked);
        }

        [TestCase(RunPhase.Decree)]
        [TestCase(RunPhase.Battle)]
        [TestCase(RunPhase.Spoils)]
        [TestCase(RunPhase.Ended)]
        public void Apply_InspectOutsideWorkbenchAndOffering_RejectsWrongPhase(RunPhase phase)
        {
            var (run, content, target, lens) = Arrange(1, ThreeLines);
            run.Phase = phase;

            AssertRejected(run, content, new Inspect(target.Uid, lens.Uid), RejectReason.WrongPhase);
        }

        [Test]
        public void Apply_InspectWhenNotActive_RejectsRunNotActive()
        {
            Assert.That(GameRules.Apply(new RunState(), _meta, ContentFactory.Create(), new Inspect(1, 2)).Reason,
                Is.EqualTo(RejectReason.RunNotActive));
        }

        /// <summary>Run Bàn luyện·Editing; Túi = [Sắt, thành phẩm Thép (Q 30, <paramref name="lines"/>), Kính (<paramref name="revealLines"/>)].</summary>
        private static (RunState run, ContentDb content, ItemInstance target, ItemInstance lens) Arrange(
            int revealLines, StatLine[] lines = null, ulong seed = StateFactory.Seed)
        {
            var content = ContentFactory.Create(lensRevealLines: revealLines);
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Iron });
            run.Seed = seed;
            run.Rng = RngStreams.Create(seed);
            var target = StateFactory.AddProduct(run, content, Steel, 30, lines ?? new StatLine[0]);
            var lens = StateFactory.AddItem(run, content, Lens);
            return (run, content, target, lens);
        }

        private static int BitCount(int mask)
        {
            var count = 0;
            for (; mask != 0; mask &= mask - 1)
                count++;
            return count;
        }

        private void AssertRejected(RunState run, ContentDb content, ICommand command, RejectReason reason)
        {
            var before = StateHasher.Hash(run, _meta);

            var result = GameRules.Apply(run, _meta, content, command);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(reason));
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        private CommandResult Apply(RunState run, ContentDb content, ICommand command)
        {
            var result = GameRules.Apply(run, _meta, content, command);
            Assert.That(result.Accepted, Is.True, $"arrange: rejected {result.Reason}");
            return result;
        }
    }
}
