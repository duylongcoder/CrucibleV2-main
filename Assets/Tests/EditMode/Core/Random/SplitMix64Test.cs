using Crucible.Core.Random;
using NUnit.Framework;

namespace Crucible.Tests.Core.Random
{
    public class SplitMix64Test
    {
        [Test]
        public void Next_ReferenceSeed_ReturnsReferenceSequence()
        {
            // Vector tham chiếu splitmix64.c (Vigna), seed 1234567 — đối chiếu bằng bản Python độc lập 2026-11-04.
            ulong state = 1234567;
            var expected = new[]
            {
                6457827717110365317UL, 3203168211198807973UL, 9817491932198370423UL,
                4593380528125082431UL, 16408922859458223821UL,
            };

            var actual = new ulong[expected.Length];
            for (var i = 0; i < actual.Length; i++)
                actual[i] = SplitMix64.Next(ref state);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
