using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class GameRulesBrewTest
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
        public void Apply_BrewEmptyGrid_RejectsWithEmptyGrid()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron, Coal });

            var result = Brew(run);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(RejectReason.EmptyGrid));
        }

        [Test]
        public void Apply_BrewMatchingGrid_AddsProductWithBaseEssenceQuality()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Copper }, new[] { Iron, Coal });

            Brew(run);

            var result = run.Bag.Last();
            Assert.That(result.ItemId, Is.EqualTo(IdOf(_content, Steel)));
            Assert.That(result.Quality, Is.EqualTo(SteelEssence));
            Assert.That(result.Magic, Is.False);
        }

        [Test]
        public void Apply_BrewRecipeWithStatLines_ProductHasLines()
        {
            var content = ContentFactory.Create(steelStatLines: new[] { new StatLineData(StatKind.Sharpness, 100, 5, 15) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Copper }, new[] { Iron, Coal });

            GameRules.Apply(run, _meta, content, new Brew(0));

            var line = run.Bag.Last().Lines.Single();
            Assert.That(line.Stat, Is.EqualTo(StatKind.Sharpness));
            Assert.That(line.Value, Is.InRange(5, 15));
        }

        [Test]
        public void Apply_BrewProduct_StartsFullyHidden()
        {
            var content = ContentFactory.Create(steelStatLines: new[] { new StatLineData(StatKind.Sharpness, 100, 5, 15) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Copper }, new[] { Iron, Coal });

            GameRules.Apply(run, _meta, content, new Brew(0));

            Assert.That(run.Bag.Last().RevealedMask, Is.EqualTo(0));
        }

        [Test]
        public void Apply_BrewStatLines_DrawAfterMagicAndMatchDirectRoll()
        {
            var lines = new[] { new StatLineData(StatKind.Sharpness, 60, 5, 15), new StatLineData(StatKind.Absorb, 50, 1, 9) };
            var content = ContentFactory.Create(steelStatLines: lines);
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Copper }, new[] { Iron, Coal });
            var recipe = ContentDbStatLineTest.SteelRecipe(content);
            var chance = run.Rng.Get(RngStream.Chance);
            var expectedMagic = chance.Roll(MagicChanceFormula.ForBrew(run.Grid, recipe, content));
            var expectedLines = StatLineRules.Roll(ref chance, recipe);

            GameRules.Apply(run, _meta, content, new Brew(0));

            var product = run.Bag.Last();
            Assert.That(product.Magic, Is.EqualTo(expectedMagic));
            Assert.That(product.Lines, Is.EqualTo(expectedLines));
            Assert.That(run.Rng.Get(RngStream.Chance).State, Is.EqualTo(chance.State));
        }

        [Test]
        public void Apply_BrewScrapWithStatLineContent_NoLinesAndNoChanceDraw()
        {
            var content = ContentFactory.Create(steelStatLines: new[] { new StatLineData(StatKind.Sharpness, 100, 5, 15) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Copper }, new[] { Coal, Iron });
            var before = run.Rng.Get(RngStream.Chance).State;

            GameRules.Apply(run, _meta, content, new Brew(0));

            Assert.That(run.Bag.Last().Lines, Is.Empty);
            Assert.That(run.Rng.Get(RngStream.Chance).State, Is.EqualTo(before));
        }

        [Test]
        public void Apply_BrewNonMatchingGrid_AddsScrapWithZeroQuality()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Copper }, new[] { Coal, Iron });

            Brew(run);

            var result = run.Bag.Last();
            Assert.That(result.ItemId, Is.EqualTo(IdOf(_content, Scrap)));
            Assert.That(result.Quality, Is.EqualTo(0));
        }

        [Test]
        public void Apply_BrewOnlyAdditives_AddsScrap()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Ash, null, Sand });

            Brew(run);

            Assert.That(run.Bag.Last().ItemId, Is.EqualTo(IdOf(_content, Scrap)));
        }

        [Test]
        public void Apply_Brew_EmptiesGrid()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Ash, Coal });

            Brew(run);

            Assert.That(run.Grid, Has.All.Null);
        }

        [Test]
        public void Apply_Brew_RemovesConsumedItemsFromRun()
        {
            // Túi: uid 1; lưới: uid 2, 3, 4.
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Copper }, new[] { Iron, Ash, Coal });

            Brew(run);

            var remaining = run.Bag.Select(item => item.Uid).Concat(run.Grid.Where(item => item != null).Select(item => item.Uid));
            Assert.That(remaining.Intersect(new[] { 2, 3, 4 }), Is.Empty);
        }

        [Test]
        public void Apply_Brew_AddsExactlyOneItemToBag()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Copper, Copper }, new[] { Iron, Ash, Coal });
            var before = run.Bag.Count;

            Brew(run);

            Assert.That(run.Bag.Count, Is.EqualTo(before + 1));
        }

        [Test]
        public void Apply_Brew_AssignsNextUidToResult()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Copper }, new[] { Iron, Coal });
            var expectedUid = run.NextUid;

            Brew(run);

            Assert.That(run.Bag.Last().Uid, Is.EqualTo(expectedUid));
            Assert.That(run.NextUid, Is.EqualTo(expectedUid + 1));
        }

        [Test]
        public void Apply_Brew_EntersResultPendingWithResultUid()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Copper }, new[] { Iron, Coal });

            Brew(run);

            Assert.That(run.WorkbenchStep, Is.EqualTo(WorkbenchStep.ResultPending));
            Assert.That(run.LastBrewResultUid, Is.EqualTo(run.Bag.Last().Uid));
        }

        [Test]
        public void Apply_Brew_RaisesBrewCompletedWithResultUid()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Copper }, new[] { Iron, Coal });

            var result = Brew(run);

            Assert.That(result.Events.Count, Is.EqualTo(1));
            Assert.That(result.Events[0], Is.TypeOf<BrewCompleted>());
            Assert.That(((BrewCompleted)result.Events[0]).ResultUid, Is.EqualTo(run.Bag.Last().Uid));
        }

        [TestCase(0, Steel)]
        [TestCase(1, CastIron)]
        [TestCase(2, Scrap)]
        public void Apply_BrewIronCoalFamilyByGap_ProducesVariant(int gap, string expectedItem)
        {
            var cells = new string[9];
            cells[0] = Iron;
            cells[1 + gap] = Coal;
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], cells);

            Brew(run);

            Assert.That(run.Bag.Last().ItemId, Is.EqualTo(IdOf(_content, expectedItem)));
        }

        [Test]
        public void Apply_BrewWithAllowedAdditiveInGap_SameProductAndQualityAsWithout()
        {
            var withAdditive = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Ash, Coal });
            var withoutAdditive = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, null, Coal });

            Brew(withAdditive);
            Brew(withoutAdditive);

            Assert.That(withAdditive.Bag.Last().ItemId, Is.EqualTo(withoutAdditive.Bag.Last().ItemId));
            Assert.That(withAdditive.Bag.Last().Quality, Is.EqualTo(withoutAdditive.Bag.Last().Quality));
        }

        private CommandResult Brew(RunState run) => GameRules.Apply(run, _meta, _content, new Brew(1793750400));
    }
}
