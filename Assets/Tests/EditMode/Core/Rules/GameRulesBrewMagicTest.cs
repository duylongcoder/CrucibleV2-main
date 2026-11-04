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
    public class GameRulesBrewMagicTest
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
        public void Apply_BrewMatching_MagicEqualsChanceRollAtComputedPercent()
        {
            // Gang + Xương trong khoảng trống → 20%.
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Bone, Coal });
            var chanceCopy = run.Rng.Get(RngStream.Chance);
            var expected = chanceCopy.Roll(BoneMagicChance);

            Brew(run);

            Assert.That(run.Bag.Last().Magic, Is.EqualTo(expected));
        }

        [Test]
        public void Apply_BrewMatching_AdvancesChanceStreamByOneDraw()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Bone, Coal });
            var chanceCopy = run.Rng.Get(RngStream.Chance);
            chanceCopy.NextUInt();

            Brew(run);

            Assert.That(run.Rng.Get(RngStream.Chance).State, Is.EqualTo(chanceCopy.State));
        }

        [Test]
        public void Apply_BrewMatching_LeavesOtherStreamsUnchanged()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Bone, Coal });
            var before = run.Rng.Capture();

            Brew(run);

            var after = run.Rng.Capture();
            for (var i = 0; i < RngStreams.Count; i++)
            {
                if (i != (int)RngStream.Chance)
                    Assert.That(after[i], Is.EqualTo(before[i]), ((RngStream)i).ToString());
            }
        }

        [Test]
        public void Apply_BrewMatchingZeroPercent_StillAdvancesChanceByOneDraw()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Coal });
            var chanceCopy = run.Rng.Get(RngStream.Chance);
            chanceCopy.NextUInt();

            Brew(run);

            Assert.That(run.Rng.Get(RngStream.Chance).State, Is.EqualTo(chanceCopy.State));
        }

        [Test]
        public void Apply_BrewMatchingHundredPercent_ProducesMagicProduct()
        {
            // Trần 100, hai phụ gia 50% trong khoảng trống → 100%.
            var content = ContentDb.Create(
                new[]
                {
                    new ItemData("iron", ItemKind.Mineral, 1),
                    new ItemData("coal", ItemKind.Mineral, 1),
                    new ItemData("spark", ItemKind.Additive, 1, magicChance: 50),
                    new ItemData("arcane", ItemKind.Mineral, 2, new RecipeData(
                        new[] { RecipeSlotData.Ingredient("iron"), RecipeSlotData.Gap, RecipeSlotData.Gap, RecipeSlotData.Ingredient("coal") },
                        new[] { "spark" }, 5)),
                },
                new DifficultyData[0], new StarterKitData[0], new RunData(3, 8, 12, 8),
                new BrewingData(QualityCap, DefaultInherit(), Fill(100), Fill(100)), CreateMarket());
            var run = StateFactory.CreateWorkbenchRun(content, new string[0], new[] { "iron", "spark", "spark", "coal" });

            GameRules.Apply(run, _meta, content, new Brew(0));

            Assert.That(run.Bag.Last().Magic, Is.True);
        }

        [Test]
        public void Apply_BrewNonMatching_LeavesAllStreamsUnchanged()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Coal, Iron });
            var before = run.Rng.Capture();

            Brew(run);

            Assert.That(run.Rng.Capture(), Is.EqualTo(before));
        }

        [Test]
        public void Apply_BrewNonMatching_ScrapHasNoMagic()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Coal, Bone, Iron });

            Brew(run);

            Assert.That(run.Bag.Last().Magic, Is.False);
        }

        [Test]
        public void Apply_SameStateSameGrid_ProducesSameMagic()
        {
            var first = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Bone, Coal });
            var second = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Bone, Coal });

            Brew(first);
            Brew(second);

            Assert.That(second.Bag.Last().Magic, Is.EqualTo(first.Bag.Last().Magic));
            Assert.That(StateHasher.Hash(second, _meta), Is.EqualTo(StateHasher.Hash(first, _meta)));
        }

        private void Brew(RunState run)
        {
            var result = GameRules.Apply(run, _meta, _content, new Brew(1793750400));
            Assert.That(result.Accepted, Is.True, "precondition: brew accepted");
        }
    }
}
