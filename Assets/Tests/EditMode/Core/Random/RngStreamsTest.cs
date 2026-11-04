using System.Linq;
using Crucible.Core.Random;
using NUnit.Framework;

namespace Crucible.Tests.Core.Random
{
    public class RngStreamsTest
    {
        private const ulong Seed = 987654321;

        private static readonly RngStream[] AllStreams =
        {
            RngStream.Shop, RngStream.Chance, RngStream.Inspect, RngStream.Forge,
            RngStream.Market, RngStream.Season, RngStream.Reward,
        };

        private static readonly RngStream[] StreamsOtherThanShop = AllStreams.Skip(1).ToArray();

        [Test]
        public void Create_SameSeed_ProducesIdenticalSequences([ValueSource(nameof(AllStreams))] RngStream stream)
        {
            var first = RngStreams.Create(Seed);
            var second = RngStreams.Create(Seed);

            var firstValues = Draw(first, stream, 3);
            var secondValues = Draw(second, stream, 3);

            Assert.That(secondValues, Is.EqualTo(firstValues));
        }

        [Test]
        public void Create_SameSeed_StreamsProduceDistinctFirstValues()
        {
            var streams = RngStreams.Create(Seed);

            var firstValues = AllStreams.Select(stream => streams.Get(stream).NextUInt()).ToArray();

            Assert.That(firstValues, Is.Unique);
        }

        [Test]
        public void Get_DrawFromShop_LeavesOtherStreamsUnchanged(
            [ValueSource(nameof(StreamsOtherThanShop))] RngStream other)
        {
            var drawn = RngStreams.Create(Seed);
            var untouched = RngStreams.Create(Seed);

            Draw(drawn, RngStream.Shop, 5);

            Assert.That(Draw(drawn, other, 3), Is.EqualTo(Draw(untouched, other, 3)));
        }

        [Test]
        public void Create_SeedZero_StreamsProduceDistinctFirstValues()
        {
            var streams = RngStreams.Create(0);

            var firstValues = AllStreams.Select(stream => streams.Get(stream).NextUInt()).ToArray();

            Assert.That(firstValues, Is.Unique);
            Assert.That(firstValues, Has.Some.Not.EqualTo(0u));
        }

        [Test]
        public void Restore_FromCapture_ContinuesIdentically()
        {
            var original = RngStreams.Create(Seed);
            foreach (var stream in AllStreams)
                Draw(original, stream, 2);
            var captured = original.Capture();
            var restored = RngStreams.Create(1);

            restored.Restore(captured);

            foreach (var stream in AllStreams)
                Assert.That(restored.Get(stream).NextUInt(), Is.EqualTo(original.Get(stream).NextUInt()), stream.ToString());
        }

        private static uint[] Draw(RngStreams streams, RngStream stream, int count)
        {
            var values = new uint[count];
            for (var i = 0; i < count; i++)
                values[i] = streams.Get(stream).NextUInt();
            return values;
        }
    }
}
