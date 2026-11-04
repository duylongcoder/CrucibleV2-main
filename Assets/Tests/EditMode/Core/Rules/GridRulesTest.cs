using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class GridRulesTest
    {
        private ContentDb _content;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
        }

        [Test]
        public void CountItems_GridWithGaps_CountsOccupiedCells()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron },
                new[] { Iron, null, null, null, Coal, null, null, null, Ash });

            Assert.That(GridRules.CountItems(run), Is.EqualTo(3));
        }

        [Test]
        public void CountItems_EmptyGrid_ReturnsZero()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron });

            Assert.That(GridRules.CountItems(run), Is.EqualTo(0));
        }
    }
}
