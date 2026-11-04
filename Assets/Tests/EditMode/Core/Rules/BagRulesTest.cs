using System;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class BagRulesTest
    {
        private ContentDb _content;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
        }

        [Test]
        public void UsedSlots_BagAndGridHaveItems_CountsBoth()
        {
            var run = CreateRun(new[] { Iron, Coal, Ash }, new[] { Iron, null, Coal });

            Assert.That(BagRules.UsedSlots(run), Is.EqualTo(5));
        }

        [Test]
        public void UsedSlots_SameItemIdTwice_CountsEachInstance()
        {
            var run = CreateRun(new[] { Iron, Iron });

            Assert.That(BagRules.UsedSlots(run), Is.EqualTo(2));
        }

        [Test]
        public void UsedSlots_EmptyBagAndGrid_ReturnsZero()
        {
            var run = CreateRun(new string[0]);

            Assert.That(BagRules.UsedSlots(run), Is.EqualTo(0));
        }

        [Test]
        public void FreeSlots_BelowCapacity_ReturnsCapacityMinusUsed()
        {
            var run = CreateRun(new[] { Iron, Coal, Ash }, new[] { Iron, Coal });

            Assert.That(BagRules.FreeSlots(run), Is.EqualTo(3));
        }

        [Test]
        public void FreeSlots_AtCapacity_ReturnsZero()
        {
            var run = CreateRun(Repeat(Iron, 6), new[] { Iron, Coal });

            Assert.That(BagRules.FreeSlots(run), Is.EqualTo(0));
        }

        [Test]
        public void FreeSlots_UsedAboveCapacity_ReturnsZero()
        {
            var run = CreateRun(Repeat(Iron, 10));

            Assert.That(BagRules.FreeSlots(run), Is.EqualTo(0));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void HasRoom_FreeSlotsEqualCount_ReturnsTrue(int count)
        {
            var run = CreateRun(Repeat(Iron, Capacity - count));

            Assert.That(BagRules.HasRoom(run, count), Is.True);
        }

        [TestCase(1)]
        [TestCase(4)]
        public void HasRoom_FreeSlotsBelowCount_ReturnsFalse(int count)
        {
            var run = CreateRun(Repeat(Iron, Capacity - count + 1));

            Assert.That(BagRules.HasRoom(run, count), Is.False);
        }

        [Test]
        public void HasRoom_GridFillsRemainingCapacity_ReturnsFalse()
        {
            // Túi 7 + lưới 1 = 8 = sức chứa.
            var run = CreateRun(Repeat(Iron, 7), new[] { Coal });

            Assert.That(BagRules.HasRoom(run), Is.False);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void HasRoom_CountBelowOne_Throws(int count)
        {
            var run = CreateRun(new[] { Iron });

            Assert.Throws<ArgumentOutOfRangeException>(() => BagRules.HasRoom(run, count));
        }

        [Test]
        public void CanExpand_BelowMax_ReturnsTrue()
        {
            var run = CreateRun(new string[0]);

            Assert.That(BagRules.CanExpand(run, _content), Is.True);
        }

        [Test]
        public void CanExpand_AtMax_ReturnsFalse()
        {
            var run = CreateRun(new string[0]);
            run.BagCapacity = MaxBagCapacity;

            Assert.That(BagRules.CanExpand(run, _content), Is.False);
        }

        [Test]
        public void Expand_WithinMax_AddsFullAmount()
        {
            var run = CreateRun(new string[0]);

            var added = BagRules.Expand(run, _content, 2);

            Assert.That(added, Is.EqualTo(2));
            Assert.That(run.BagCapacity, Is.EqualTo(Capacity + 2));
        }

        [Test]
        public void Expand_PastMax_ClampsToMax()
        {
            var run = CreateRun(new string[0]);
            run.BagCapacity = MaxBagCapacity - 1;

            var added = BagRules.Expand(run, _content, 3);

            Assert.That(added, Is.EqualTo(1));
            Assert.That(run.BagCapacity, Is.EqualTo(MaxBagCapacity));
        }

        [Test]
        public void Expand_AtMax_ReturnsZero()
        {
            var run = CreateRun(new string[0]);
            run.BagCapacity = MaxBagCapacity;

            var added = BagRules.Expand(run, _content, 2);

            Assert.That(added, Is.EqualTo(0));
            Assert.That(run.BagCapacity, Is.EqualTo(MaxBagCapacity));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Expand_AmountBelowOne_Throws(int amount)
        {
            var run = CreateRun(new string[0]);

            Assert.Throws<ArgumentOutOfRangeException>(() => BagRules.Expand(run, _content, amount));
            Assert.That(run.BagCapacity, Is.EqualTo(Capacity));
        }

        private const int Capacity = 8;

        private RunState CreateRun(string[] bag, string[] grid = null)
        {
            var run = StateFactory.CreateWorkbenchRun(_content, bag, grid);
            run.BagCapacity = Capacity;
            return run;
        }

        private static string[] Repeat(string id, int count)
        {
            var ids = new string[count];
            for (var i = 0; i < count; i++)
                ids[i] = id;
            return ids;
        }
    }
}
