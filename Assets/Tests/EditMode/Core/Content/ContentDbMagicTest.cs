using System;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;

namespace Crucible.Tests.Core.Content
{
    public class ContentDbMagicTest
    {
        private static readonly RunData Run = new RunData(3, 8, 8);

        [Test]
        public void Create_MagicTableWrongLength_ThrowsArgumentException(
            [Values("inherit", "cap", "retain")] string table)
        {
            var brewing = BrewingWith(table, new int[BrewingData.TierCount - 1]);

            Assert.Throws<ArgumentException>(() => Create(brewing, Basics()));
        }

        [Test]
        public void Create_MagicTableValueOutsideZeroToHundred_ThrowsArgumentException(
            [Values("inherit", "cap", "retain")] string table, [Values(-1, 101)] int value)
        {
            var values = ContentFactory.Fill(50);
            values[3] = value;
            var brewing = BrewingWith(table, values);

            Assert.Throws<ArgumentException>(() => Create(brewing, Basics()));
        }

        [TestCase(-1)]
        [TestCase(51)]
        public void Create_AdditiveMagicChanceOutsideRange_ThrowsArgumentException(int chance)
        {
            var items = new[] { new ItemData("ash", ItemKind.Additive, 1, magicChance: chance) };

            Assert.Throws<ArgumentException>(() => Create(ContentFactory.CreateBrewing(), items));
        }

        [Test]
        public void Create_NonAdditiveWithMagicChance_ThrowsArgumentException()
        {
            var items = new[] { new ItemData("iron", ItemKind.Mineral, 1, magicChance: 10) };

            Assert.Throws<ArgumentException>(() => Create(ContentFactory.CreateBrewing(), items));
        }

        [TestCase(ItemKind.Mineral, 0)]
        [TestCase(ItemKind.Mineral, 11)]
        [TestCase(ItemKind.Additive, 0)]
        public void Create_MineralOrAdditiveTierOutsideOneToTen_ThrowsArgumentException(ItemKind kind, int tier)
        {
            var items = new[] { new ItemData("item", kind, tier) };

            Assert.Throws<ArgumentException>(() => Create(ContentFactory.CreateBrewing(), items));
        }

        private static ItemData[] Basics() => new[] { new ItemData("iron", ItemKind.Mineral, 1) };

        private static BrewingData BrewingWith(string table, int[] values) => new BrewingData(
            ContentFactory.QualityCap,
            table == "inherit" ? values : ContentFactory.DefaultInherit(),
            table == "cap" ? values : ContentFactory.Fill(90),
            table == "retain" ? values : ContentFactory.Fill(100));

        private static ContentDb Create(BrewingData brewing, ItemData[] items) =>
            ContentDb.Create(items, Array.Empty<DifficultyData>(), Array.Empty<StarterKitData>(), Run, brewing);
    }
}
