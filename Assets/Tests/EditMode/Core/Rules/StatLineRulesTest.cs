using System.Linq;
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
    /// <summary><see cref="StatLineRules"/> (inspect R4, R7, D9).</summary>
    public class StatLineRulesTest
    {
        [Test]
        public void Roll_NoStatLines_ReturnsEmptyWithoutDrawing()
        {
            var recipe = Recipe();
            var rng = RngStreams.Create(1).Get(RngStream.Chance);
            var before = rng.State;

            var lines = StatLineRules.Roll(ref rng, recipe);

            Assert.That(lines, Is.Empty);
            Assert.That(rng.State, Is.EqualTo(before));
        }

        [Test]
        public void Roll_Chance100_AlwaysHasLineInRange()
        {
            var recipe = Recipe(new StatLineData(StatKind.Sharpness, 100, 5, 15));

            for (ulong seed = 0; seed < 500; seed++)
            {
                var rng = RngStreams.Create(seed).Get(RngStream.Chance);

                var lines = StatLineRules.Roll(ref rng, recipe);

                Assert.That(lines.Length, Is.EqualTo(1), $"seed {seed}");
                Assert.That(lines[0].Stat, Is.EqualTo(StatKind.Sharpness), $"seed {seed}");
                Assert.That(lines[0].Value, Is.InRange(5, 15), $"seed {seed}");
            }
        }

        [Test]
        public void Roll_Chance0_NeverHasLine()
        {
            var recipe = Recipe(new StatLineData(StatKind.Sharpness, 0, 5, 15));

            for (ulong seed = 0; seed < 500; seed++)
            {
                var rng = RngStreams.Create(seed).Get(RngStream.Chance);

                Assert.That(StatLineRules.Roll(ref rng, recipe), Is.Empty, $"seed {seed}");
            }
        }

        [Test]
        public void Roll_MinEqualsMax_ValueIsMin()
        {
            var recipe = Recipe(new StatLineData(StatKind.Absorb, 100, 7, 7));
            var rng = RngStreams.Create(3).Get(RngStream.Chance);

            Assert.That(StatLineRules.Roll(ref rng, recipe).Single().Value, Is.EqualTo(7));
        }

        [Test]
        public void Roll_DrawCount_OneRollPlusOneValuePerPresentLine()
        {
            var recipe = Recipe(new StatLineData(StatKind.Sharpness, 100, 5, 15), new StatLineData(StatKind.Reflect, 0, 1, 4),
                new StatLineData(StatKind.Absorb, 100, 2, 2));
            var rng = RngStreams.Create(9).Get(RngStream.Chance);
            var expected = rng;
            expected.Roll(100);
            expected.NextInt(11);
            expected.Roll(0);
            expected.Roll(100);
            expected.NextInt(1);

            StatLineRules.Roll(ref rng, recipe);

            Assert.That(rng.State, Is.EqualTo(expected.State));
        }

        [Test]
        public void Roll_MultipleLines_KeepDeclarationOrder()
        {
            var recipe = Recipe(new StatLineData(StatKind.Absorb, 100, 1, 3), new StatLineData(StatKind.Sharpness, 100, 1, 3));
            var rng = RngStreams.Create(4).Get(RngStream.Chance);

            var lines = StatLineRules.Roll(ref rng, recipe);

            Assert.That(lines.Select(l => l.Stat), Is.EqualTo(new[] { StatKind.Absorb, StatKind.Sharpness }));
        }

        [Test]
        public void Roll_ValueRange_ReachesBothEnds()
        {
            var recipe = Recipe(new StatLineData(StatKind.Sharpness, 100, 5, 9));
            var sawMin = false;
            var sawMax = false;

            for (ulong seed = 0; seed < 2000 && !(sawMin && sawMax); seed++)
            {
                var rng = RngStreams.Create(seed).Get(RngStream.Chance);
                var value = StatLineRules.Roll(ref rng, recipe)[0].Value;
                sawMin |= value == 5;
                sawMax |= value == 9;
            }

            Assert.That(sawMin && sawMax, Is.True);
        }

        [Test]
        public void ValueOf_StatPresent_ReturnsValue()
        {
            var item = new ItemInstance(1, new ItemId(0), lines: new[]
            {
                new StatLine(StatKind.Reflect, 4), new StatLine(StatKind.Sharpness, 11),
            });

            Assert.That(StatLineRules.ValueOf(item, StatKind.Sharpness), Is.EqualTo(11));
        }

        [Test]
        public void ValueOf_StatAbsent_ReturnsZero()
        {
            var item = new ItemInstance(1, new ItemId(0), lines: new[] { new StatLine(StatKind.Reflect, 4) });

            Assert.That(StatLineRules.ValueOf(item, StatKind.Absorb), Is.EqualTo(0));
        }

        private static Recipe Recipe(params StatLineData[] lines) =>
            ContentDbStatLineTest.SteelRecipe(ContentFactory.Create(steelStatLines: lines));
    }
}
