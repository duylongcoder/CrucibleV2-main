using System;
using Crucible.Core.Random;
using NUnit.Framework;

namespace Crucible.Tests.Core.Random
{
    public class Pcg32Test
    {
        [Test]
        public void NextUInt_Seed42Sequence54_ReturnsReferenceSequence()
        {
            // Vector tham chiếu pcg32-demo (pcg-c-basic), pcg32_srandom_r(42, 54).
            var rng = new Pcg32(42, 54);
            var expected = new uint[] { 0xa15c02b7, 0x7b47f409, 0xba1d3330, 0x83d2f293, 0xbfa4784b, 0xcbed606e };

            var actual = new uint[expected.Length];
            for (var i = 0; i < actual.Length; i++)
                actual[i] = rng.NextUInt();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void NextInt_PositiveMax_ReturnsValueInRange(
            [Values(0UL, 1UL, 42UL)] ulong seed,
            [Values(1, 2, 7, int.MaxValue)] int max)
        {
            var rng = new Pcg32(seed, 0);

            for (var i = 0; i < 100; i++)
            {
                var value = rng.NextInt(max);
                Assert.That(value, Is.InRange(0, max - 1));
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NextInt_NonPositiveMax_ThrowsArgumentOutOfRangeException(int max)
        {
            var rng = new Pcg32(42, 54);

            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(max));
        }

        [Test]
        public void NextInt_SixtyThousandSamplesMaxSix_EachBucketWithinFivePercent()
        {
            var rng = new Pcg32(12345, 678);
            var counts = new int[6];

            for (var i = 0; i < 60000; i++)
                counts[rng.NextInt(6)]++;

            foreach (var count in counts)
                Assert.That(count, Is.InRange(9500, 10500));
        }

        [Test]
        public void Roll_PercentZero_ReturnsFalse([Values(0UL, 1UL, 42UL)] ulong seed)
        {
            var rng = new Pcg32(seed, 0);

            for (var i = 0; i < 100; i++)
                Assert.That(rng.Roll(0), Is.False);
        }

        [Test]
        public void Roll_PercentHundred_ReturnsTrue([Values(0UL, 1UL, 42UL)] ulong seed)
        {
            var rng = new Pcg32(seed, 0);

            for (var i = 0; i < 100; i++)
                Assert.That(rng.Roll(100), Is.True);
        }

        [TestCase(0)]
        [TestCase(50)]
        [TestCase(100)]
        public void Roll_AnyValidPercent_AdvancesStateByOneDraw(int percent)
        {
            var rolled = new Pcg32(42, 54);
            var drawn = new Pcg32(42, 54);

            rolled.Roll(percent);
            drawn.NextUInt();

            Assert.That(rolled.State, Is.EqualTo(drawn.State));
        }

        [TestCase(-1)]
        [TestCase(101)]
        public void Roll_PercentOutsideZeroToHundred_ThrowsArgumentOutOfRangeException(int percent)
        {
            var rng = new Pcg32(42, 54);

            Assert.Throws<ArgumentOutOfRangeException>(() => rng.Roll(percent));
        }

        [Test]
        public void State_SetFromEarlierCapture_ReplaysSameSequence()
        {
            var rng = new Pcg32(42, 54);
            rng.NextUInt();
            var captured = rng.State;
            var original = new[] { rng.NextUInt(), rng.NextUInt(), rng.NextUInt() };

            rng.State = captured;
            var replayed = new[] { rng.NextUInt(), rng.NextUInt(), rng.NextUInt() };

            Assert.That(replayed, Is.EqualTo(original));
        }
    }
}
