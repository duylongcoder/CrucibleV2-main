using System;
using System.Linq;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Content
{
    /// <summary>Dòng khởi đầu của công thức + <c>RevealLines</c> của Kính trong <see cref="ContentDb"/> (inspect R1, R2, D4, D5).</summary>
    public class ContentDbStatLineTest
    {
        [Test]
        public void Create_RecipeStatLines_ResolvedInDeclarationOrder()
        {
            var content = ContentFactory.Create(steelStatLines: new[]
            {
                new StatLineData(StatKind.Reflect, 20, 3, 8), new StatLineData(StatKind.Sharpness, 60, 5, 15),
            });

            var lines = SteelRecipe(content).StatLines;

            Assert.That(lines.Select(l => l.Stat), Is.EqualTo(new[] { StatKind.Reflect, StatKind.Sharpness }));
            Assert.That(lines[1].ChancePercent, Is.EqualTo(60));
            Assert.That(lines[1].Min, Is.EqualTo(5));
            Assert.That(lines[1].Max, Is.EqualTo(15));
        }

        [Test]
        public void Create_RecipeWithoutStatLines_HasEmptyList()
        {
            Assert.That(SteelRecipe(ContentFactory.Create()).StatLines, Is.Empty);
        }

        [TestCase(-1)]
        [TestCase(101)]
        public void Create_StatLineChanceOutOfRange_Throws(int chance)
        {
            AssertRejected(new StatLineData(StatKind.Sharpness, chance, 1, 5));
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void Create_StatLineMinBelowOne_Throws(int min)
        {
            AssertRejected(new StatLineData(StatKind.Sharpness, 50, min, 5));
        }

        [Test]
        public void Create_StatLineMinAboveMax_Throws()
        {
            AssertRejected(new StatLineData(StatKind.Sharpness, 50, 6, 5));
        }

        [Test]
        public void Create_StatLineMaxAboveMaxStatValue_Throws()
        {
            AssertRejected(new StatLineData(StatKind.Sharpness, 50, 1, BattleData.MaxStatValue + 1));
        }

        [Test]
        public void Create_StatLineAtBounds_IsAllowed()
        {
            Assert.DoesNotThrow(() => ContentFactory.Create(steelStatLines: new[]
            {
                new StatLineData(StatKind.Sharpness, 0, 1, 1), new StatLineData(StatKind.Absorb, 100, 1, BattleData.MaxStatValue),
            }));
        }

        [Test]
        public void Create_DuplicateStatInRecipe_Throws()
        {
            AssertRejected(new StatLineData(StatKind.Absorb, 50, 1, 5), new StatLineData(StatKind.Absorb, 10, 2, 3));
        }

        [Test]
        public void Create_StatLinesNullEntry_Throws()
        {
            AssertRejected(new StatLineData[] { null });
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Create_LensWithoutRevealLines_Throws(int revealLines)
        {
            Assert.Throws<ArgumentException>(() => ContentFactory.Create(lensRevealLines: revealLines));
        }

        [Test]
        public void Create_Lens_ExposesRevealLines()
        {
            var content = ContentFactory.Create(lensRevealLines: 3);

            Assert.That(content.GetItem(IdOf(content, Lens)).RevealLines, Is.EqualTo(3));
        }

        [Test]
        public void Create_NonLensWithRevealLines_Throws()
        {
            Assert.Throws<ArgumentException>(() => ContentDb.Create(
                new[] { new ItemData(Iron, ItemKind.Mineral, 1, revealLines: 1) }, Array.Empty<DifficultyData>(),
                Array.Empty<StarterKitData>(), new RunData(StartingReputation, StartingBagCapacity, MaxBagCapacity,
                    SeasonCount), CreateBrewing(), CreateMarket(), CreateBattle()));
        }

        internal static Recipe SteelRecipe(ContentDb content) =>
            content.Recipes.Single(r => r.Product == IdOf(content, Steel));

        private static void AssertRejected(params StatLineData[] lines)
        {
            Assert.Throws<ArgumentException>(() => ContentFactory.Create(steelStatLines: lines));
        }
    }
}
