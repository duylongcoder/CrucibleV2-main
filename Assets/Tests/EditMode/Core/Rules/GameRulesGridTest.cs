using System;
using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class GameRulesGridTest
    {
        private ContentDb _content;
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
        }

        [Test]
        public void Apply_PlaceItemOnEmptyCell_MovesItemFromBagToCell()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron, Coal });

            var result = Apply(run, new PlaceItem(2, 4));

            Assert.That(result.Accepted, Is.True);
            Assert.That(run.Grid[4].Uid, Is.EqualTo(2));
            Assert.That(run.Bag.Select(item => item.Uid), Is.EqualTo(new[] { 1 }));
            Assert.That(result.Events.Single(), Is.TypeOf<GridChanged>());
        }

        [Test]
        public void Apply_PlaceItemOnOccupiedCell_ReturnsOccupantToBagEnd()
        {
            // Túi: uid 1 (sắt), 2 (than); lưới ô 0: uid 3 (tro).
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron, Coal }, new[] { Ash });

            Apply(run, new PlaceItem(1, 0));

            Assert.That(run.Grid[0].Uid, Is.EqualTo(1));
            Assert.That(run.Bag.Select(item => item.Uid), Is.EqualTo(new[] { 2, 3 }));
        }

        [TestCase(Scrap)]
        [TestCase(Lens)]
        [TestCase(Bellows)]
        public void Apply_PlaceItemNotPlaceable_RejectsWithItemNotPlaceable(string itemId)
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { itemId });

            var result = Apply(run, new PlaceItem(1, 0));

            AssertRejected(result, RejectReason.ItemNotPlaceable);
        }

        [Test]
        public void Apply_PlaceItemUnknownUid_RejectsWithItemNotFound()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron });

            var result = Apply(run, new PlaceItem(99, 0));

            AssertRejected(result, RejectReason.ItemNotFound);
        }

        [TestCase("place -1")]
        [TestCase("place 9")]
        [TestCase("remove 9")]
        [TestCase("move to 9")]
        public void Apply_GridCommandCellOutOfRange_RejectsWithInvalidCell(string command)
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron }, new[] { Coal });
            ICommand cmd;
            switch (command)
            {
                case "place -1": cmd = new PlaceItem(1, -1); break;
                case "place 9": cmd = new PlaceItem(1, 9); break;
                case "remove 9": cmd = new RemoveFromGrid(9); break;
                default: cmd = new MoveCell(0, 9); break;
            }

            var result = Apply(run, cmd);

            AssertRejected(result, RejectReason.InvalidCell);
        }

        [Test]
        public void Apply_RemoveFromGrid_AppendsItemToBagEnd()
        {
            // Túi: uid 1, 2; lưới ô 3: uid 3.
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron, Coal }, new[] { null, null, null, Ash });

            Apply(run, new RemoveFromGrid(3));

            Assert.That(run.Grid[3], Is.Null);
            Assert.That(run.Bag.Select(item => item.Uid), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void Apply_RemoveFromEmptyCell_RejectsWithCellEmpty()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron });

            var result = Apply(run, new RemoveFromGrid(0));

            AssertRejected(result, RejectReason.CellEmpty);
        }

        [Test]
        public void Apply_MoveCellToEmptyCell_MovesItem()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron });

            Apply(run, new MoveCell(0, 5));

            Assert.That(run.Grid[0], Is.Null);
            Assert.That(run.Grid[5].Uid, Is.EqualTo(1));
        }

        [Test]
        public void Apply_MoveCellToOccupiedCell_SwapsItems()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Coal });

            Apply(run, new MoveCell(0, 1));

            Assert.That(run.Grid[0].Uid, Is.EqualTo(2));
            Assert.That(run.Grid[1].Uid, Is.EqualTo(1));
        }

        [Test]
        public void Apply_MoveCellFromEmptyCell_RejectsWithCellEmpty()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron });

            var result = Apply(run, new MoveCell(3, 0));

            AssertRejected(result, RejectReason.CellEmpty);
        }

        [Test]
        public void Apply_MoveCellToSameCell_RejectsWithInvalidCell()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron });

            var result = Apply(run, new MoveCell(0, 0));

            AssertRejected(result, RejectReason.InvalidCell);
        }

        [Test]
        public void Apply_GridCommandOutsideWorkbench_RejectsWithWrongPhase(
            [Values("place", "remove", "move", "brew")] string command)
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron }, new[] { Coal });
            run.Phase = RunPhase.Offering;

            var result = Apply(run, CreateCommand(command));

            AssertRejected(result, RejectReason.WrongPhase);
        }

        [Test]
        public void Apply_GridCommandWithoutActiveRun_RejectsWithRunNotActive(
            [Values("place", "remove", "move", "brew")] string command)
        {
            var result = Apply(new RunState(), CreateCommand(command));

            AssertRejected(result, RejectReason.RunNotActive);
        }

        [Test]
        public void Apply_GridCommandAfterBrew_RejectsWithGridLocked(
            [Values("place", "remove", "move", "brew")] string command)
        {
            // Túi: uid 1 (sắt), 2 (than); lưới ô 0: uid 3 (sắt), ô 1: uid 4 (than).
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron, Coal }, new[] { Iron, Coal });
            Apply(run, new Brew(0));

            var result = Apply(run, CreateCommand(command));

            AssertRejected(result, RejectReason.GridLocked);
        }

        [TestCase("place unknown uid")]
        [TestCase("move from empty")]
        [TestCase("brew empty")]
        public void Apply_RejectedGridCommand_LeavesStateHashUnchanged(string command)
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron, Coal });
            var before = StateHasher.Hash(run, _meta);
            ICommand cmd;
            switch (command)
            {
                case "place unknown uid": cmd = new PlaceItem(99, 0); break;
                case "move from empty": cmd = new MoveCell(0, 1); break;
                default: cmd = new Brew(0); break;
            }

            var result = Apply(run, cmd);

            Assert.That(result.Accepted, Is.False);
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        private CommandResult Apply(RunState run, ICommand command) => GameRules.Apply(run, _meta, _content, command);

        private static ICommand CreateCommand(string name)
        {
            switch (name)
            {
                case "place": return new PlaceItem(1, 2);
                case "remove": return new RemoveFromGrid(0);
                case "move": return new MoveCell(0, 2);
                case "brew": return new Brew(0);
                default: throw new ArgumentOutOfRangeException(nameof(name), name, null);
            }
        }

        private static void AssertRejected(CommandResult result, RejectReason reason)
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(reason));
        }
    }
}
