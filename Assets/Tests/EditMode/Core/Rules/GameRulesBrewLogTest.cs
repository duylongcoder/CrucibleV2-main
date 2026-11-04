using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class GameRulesBrewLogTest
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
        public void Apply_BrewMatchingGrid_StoresLogWithProductResult()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Coal });

            Brew(run, _content);

            var product = run.Bag.Last();
            Assert.That(run.LastBrew, Is.Not.Null);
            Assert.That(run.LastBrew.Result.IsScrap, Is.False);
            Assert.That(run.LastBrew.Result.ItemId, Is.EqualTo(IdOf(_content, Steel)));
            Assert.That(run.LastBrew.Result.Magic, Is.EqualTo(product.Magic));
        }

        [Test]
        public void Apply_BrewNonMatchingGrid_StoresLogWithScrapResult()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Coal, Iron });

            Brew(run, _content);

            Assert.That(run.LastBrew.Result.IsScrap, Is.True);
        }

        [Test]
        public void Apply_BrewGridWithGap_LogTicksMatchGridBeforeBrew()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { null, Iron, null, Coal });

            Brew(run, _content);

            var ticks = run.LastBrew.Ticks;
            Assert.That(run.LastBrew.FirstCell, Is.EqualTo(1));
            Assert.That(ticks.Select(tick => tick.HasItem), Is.EqualTo(new[] { true, false, true }));
            Assert.That(ticks[0].ItemId, Is.EqualTo(IdOf(_content, Iron)));
            Assert.That(ticks[2].ItemId, Is.EqualTo(IdOf(_content, Coal)));
        }

        [Test]
        public void Apply_BrewSameGridMatchedAndScrap_TicksIdentical()
        {
            var noSteelRecipe = ContentFactory.Create(steelHasRecipe: false);
            var grid = new[] { null, Iron, Coal };
            var matchedRun = StateFactory.CreateWorkbenchRun(_content, new string[0], grid);
            var scrapRun = StateFactory.CreateWorkbenchRun(noSteelRecipe, new string[0], grid);

            Brew(matchedRun, _content);
            Brew(scrapRun, noSteelRecipe);

            Assert.That(matchedRun.LastBrew.Result.IsScrap, Is.Not.EqualTo(scrapRun.LastBrew.Result.IsScrap));
            Assert.That(scrapRun.LastBrew.FirstCell, Is.EqualTo(matchedRun.LastBrew.FirstCell));
            Assert.That(scrapRun.LastBrew.Ticks, Is.EqualTo(matchedRun.LastBrew.Ticks));
        }

        [Test]
        public void Apply_Brew_EventCarriesStoredLog()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Coal });

            var result = Brew(run, _content);

            var completed = result.Events.OfType<BrewCompleted>().Single();
            Assert.That(completed.Log, Is.Not.Null);
            Assert.That(completed.Log, Is.SameAs(run.LastBrew));
        }

        [Test]
        public void Apply_BrewEmptyGrid_LastBrewUnchanged()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron });
            var previous = PreviousLog();
            run.LastBrew = previous;

            Brew(run, _content);

            Assert.That(run.LastBrew, Is.SameAs(previous));
        }

        [Test]
        public void Apply_BrewWhileResultPending_LastBrewUnchanged()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Coal });
            Brew(run, _content);
            var first = run.LastBrew;
            Assume.That(first, Is.Not.Null);

            Brew(run, _content);

            Assert.That(run.LastBrew, Is.SameAs(first));
        }

        [Test]
        public void Apply_BrewTwice_LastBrewIsLatest()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new string[0], new[] { Iron, Coal });
            Brew(run, _content);
            // Chưa có AckBrewResult (F-009): đưa về Editing và xếp lưới mới trực tiếp.
            run.WorkbenchStep = WorkbenchStep.Editing;
            run.Grid[0] = new ItemInstance(run.NextUid++, IdOf(_content, Coal));
            run.Grid[1] = new ItemInstance(run.NextUid++, IdOf(_content, Iron));

            Brew(run, _content);

            Assert.That(run.LastBrew.Result.IsScrap, Is.True);
            Assert.That(run.LastBrew.Ticks[0].ItemId, Is.EqualTo(IdOf(_content, Coal)));
        }

        private BrewLog PreviousLog()
        {
            var grid = new ItemInstance[9];
            grid[0] = new ItemInstance(90, IdOf(_content, Iron));
            return BrewLog.Create(grid, new ItemInstance(91, IdOf(_content, Scrap)), true);
        }

        private CommandResult Brew(RunState run, ContentDb content) =>
            GameRules.Apply(run, _meta, content, new Brew(1793750400));
    }
}
