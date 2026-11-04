using System;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;

namespace Crucible.Tests.Core.Content
{
    public class ContentDbBagTest
    {
        private const string KitId = "kit_test";
        private static readonly BrewingData Brewing = ContentFactory.CreateBrewing();

        [Test]
        public void Create_StartingCapacityEqualsMax_Succeeds()
        {
            var db = Create(new RunData(3, 12, 12, 8));

            Assert.That(db.Run.MaxBagCapacity, Is.EqualTo(12));
        }

        [Test]
        public void Create_StartingCapacityAboveMax_Throws()
        {
            Assert.Throws<ArgumentException>(() => Create(new RunData(3, 13, 12, 8)));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Create_StartingCapacityBelowOne_Throws(int capacity)
        {
            Assert.Throws<ArgumentException>(() => Create(new RunData(3, capacity, 12, 8)));
        }

        [Test]
        public void Create_StarterKitFillsCapacity_Succeeds()
        {
            Assert.DoesNotThrow(() => Create(new RunData(3, 4, 12, 8), kitSize: 4));
        }

        [Test]
        public void Create_StarterKitExceedsCapacity_Throws()
        {
            var exception = Assert.Throws<ArgumentException>(() => Create(new RunData(3, 4, 12, 8), kitSize: 5));

            Assert.That(exception.Message, Does.Contain(KitId));
        }

        private static ContentDb Create(RunData run, int kitSize = 1)
        {
            var kitItems = new string[kitSize];
            for (var i = 0; i < kitSize; i++)
                kitItems[i] = "ore_iron";

            return ContentDb.Create(
                new[] { new ItemData("ore_iron", ItemKind.Mineral, 1) },
                Array.Empty<DifficultyData>(),
                new[] { new StarterKitData(KitId, kitItems) },
                run, Brewing, ContentFactory.CreateMarket());
        }
    }
}
