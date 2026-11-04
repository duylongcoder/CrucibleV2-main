using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.State;
using NUnit.Framework;

namespace Crucible.Tests.Core.Logs
{
    public class BrewLogTest
    {
        private static readonly ItemId IronId = new ItemId(1);
        private static readonly ItemId CoalId = new ItemId(2);
        private static readonly ItemId ProductId = new ItemId(5);
        private static readonly ItemId ScrapId = new ItemId(9);

        [Test]
        public void Create_ItemsWithLeadingAndTrailingEmptyCells_TicksSpanFirstToLastItem()
        {
            var grid = Grid((2, IronId), (6, CoalId));

            var log = BrewLog.Create(grid, Product(), false);

            Assert.That(log.FirstCell, Is.EqualTo(2));
            Assert.That(log.Ticks.Count, Is.EqualTo(5));
        }

        [Test]
        public void Create_EmptyCellBetweenItems_RecordsWaitTick()
        {
            var grid = Grid((0, IronId), (2, CoalId));

            var log = BrewLog.Create(grid, Product(), false);

            Assert.That(log.Ticks[1].HasItem, Is.False);
        }

        [Test]
        public void Create_ItemCell_RecordsItemIdAndMagic()
        {
            var grid = new ItemInstance[9];
            grid[3] = new ItemInstance(10, IronId, magic: true);

            var log = BrewLog.Create(grid, Product(), false);

            Assert.That(log.Ticks[0].HasItem, Is.True);
            Assert.That(log.Ticks[0].ItemId, Is.EqualTo(IronId));
            Assert.That(log.Ticks[0].Magic, Is.True);
        }

        [Test]
        public void Create_TicksInOrder_CellsAreConsecutive()
        {
            var grid = Grid((1, IronId), (4, CoalId), (7, IronId));

            var log = BrewLog.Create(grid, Product(), false);

            Assert.That(log.Ticks.Select(tick => tick.Cell), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7 }));
        }

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(8)]
        public void Create_SingleItem_HasOneTick(int cell)
        {
            var grid = Grid((cell, IronId));

            var log = BrewLog.Create(grid, Product(), false);

            Assert.That(log.Ticks.Count, Is.EqualTo(1));
            Assert.That(log.FirstCell, Is.EqualTo(cell));
        }

        [Test]
        public void Create_FullGrid_HasNineTicks()
        {
            var grid = Enumerable.Range(0, 9).Select(cell => new ItemInstance(10 + cell, IronId)).ToArray();

            var log = BrewLog.Create(grid, Product(), false);

            Assert.That(log.FirstCell, Is.EqualTo(0));
            Assert.That(log.Ticks.Count, Is.EqualTo(9));
        }

        [Test]
        public void Create_ProductResult_RecordsProductAndMagic()
        {
            var log = BrewLog.Create(Grid((0, IronId)), new ItemInstance(30, ProductId, magic: true, quality: 18), false);

            Assert.That(log.Result.IsScrap, Is.False);
            Assert.That(log.Result.ItemId, Is.EqualTo(ProductId));
            Assert.That(log.Result.Magic, Is.True);
        }

        [Test]
        public void Create_ScrapResult_MarksScrap()
        {
            var log = BrewLog.Create(Grid((0, IronId)), new ItemInstance(30, ScrapId), true);

            Assert.That(log.Result.IsScrap, Is.True);
        }

        [Test]
        public void Create_GridModifiedAfterCreate_LogUnchanged()
        {
            var grid = Grid((0, IronId), (1, CoalId));
            var log = BrewLog.Create(grid, Product(), false);

            grid[0] = null;
            grid[1] = new ItemInstance(99, ProductId);

            Assert.That(log.Ticks[0].ItemId, Is.EqualTo(IronId));
            Assert.That(log.Ticks[1].ItemId, Is.EqualTo(CoalId));
        }

        [Test]
        public void Create_EmptyGrid_ThrowsArgumentException()
        {
            Assert.That(() => BrewLog.Create(new ItemInstance[9], Product(), false), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void Create_GridNotNineCells_ThrowsArgumentException()
        {
            var grid = new[] { new ItemInstance(10, IronId) };

            Assert.That(() => BrewLog.Create(grid, Product(), false), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void LogTypes_PublicMembers_ExposeNoRecipeTypes()
        {
            var forbidden = new[] { typeof(Recipe), typeof(RecipeSlot), typeof(RecipeData), typeof(RecipeSlotData) };
            var logTypes = new[] { typeof(BrewLog), typeof(BrewTick), typeof(BrewLogResult) };

            var leaks = logTypes
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(property => (type, property.Name, property.PropertyType)))
                .Where(member => MentionsAny(member.PropertyType, forbidden))
                .Select(member => $"{member.type.Name}.{member.Name}")
                .ToArray();

            Assert.That(leaks, Is.Empty);
        }

        [Test]
        public void LogTypes_PublicMembers_ExposeNoStatLines()
        {
            var forbidden = new[] { typeof(StatLine), typeof(StatLineData) };
            var logTypes = new[] { typeof(BrewLog), typeof(BrewTick), typeof(BrewLogResult), typeof(BrewCompleted) };

            var leaks = logTypes
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(property => (type, property.Name, property.PropertyType)))
                .Where(member => MentionsAny(member.PropertyType, forbidden))
                .Select(member => $"{member.type.Name}.{member.Name}")
                .ToArray();

            Assert.That(leaks, Is.Empty);
        }

        private static bool MentionsAny(Type type, Type[] forbidden)
        {
            if (forbidden.Contains(type))
                return true;
            if (type.IsArray)
                return MentionsAny(type.GetElementType(), forbidden);
            return type.IsGenericType && type.GetGenericArguments().Any(argument => MentionsAny(argument, forbidden));
        }

        private static ItemInstance Product() => new ItemInstance(30, ProductId);

        private static ItemInstance[] Grid(params (int cell, ItemId id)[] items)
        {
            var grid = new ItemInstance[9];
            foreach (var (cell, id) in items)
                grid[cell] = new ItemInstance(10 + cell, id);
            return grid;
        }
    }
}
