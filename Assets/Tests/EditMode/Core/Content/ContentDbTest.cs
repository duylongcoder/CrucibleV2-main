using System;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;

namespace Crucible.Tests.Core.Content
{
    public class ContentDbTest
    {
        private static readonly RunData Run = new RunData(3, 8, 8);
        private static readonly BrewingData Brewing = ContentFactory.CreateBrewing();

        [Test]
        public void Create_ItemsInReverseOrder_AssignsIdsByOrdinalSort()
        {
            var db = CreateWithItems("c_item", "b_item", "a_item");

            db.TryFindItem("a_item", out var a);
            db.TryFindItem("b_item", out var b);
            db.TryFindItem("c_item", out var c);

            Assert.That(a.Index, Is.LessThan(b.Index));
            Assert.That(b.Index, Is.LessThan(c.Index));
        }

        [Test]
        public void TryFindItem_KnownId_ReturnsMatchingItem()
        {
            var db = CreateWithItems("ore_iron", "add_ash");

            var found = db.TryFindItem("ore_iron", out var itemId);

            Assert.That(found, Is.True);
            Assert.That(db.GetItem(itemId).Id, Is.EqualTo("ore_iron"));
        }

        [Test]
        public void TryFindItem_UnknownId_ReturnsFalse()
        {
            var db = CreateWithItems("ore_iron");

            var found = db.TryFindItem("ore_gold", out _);

            Assert.That(found, Is.False);
        }

        [Test]
        public void Create_DuplicateItemId_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CreateWithItems("ore_iron", "ore_iron"));
        }

        private static ContentDb CreateWithItems(params string[] ids)
        {
            var items = Array.ConvertAll(ids, id => new ItemData(id, ItemKind.Mineral, 1));
            return ContentDb.Create(items, Array.Empty<DifficultyData>(), Array.Empty<StarterKitData>(), Run, Brewing);
        }
    }
}
