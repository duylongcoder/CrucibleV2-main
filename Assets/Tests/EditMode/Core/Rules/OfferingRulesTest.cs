using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><see cref="OfferingRules.IsOfferable"/> / <see cref="OfferingRules.HasOfferable"/> (forge-battle R3).</summary>
    public class OfferingRulesTest
    {
        private ContentDb _content;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
        }

        [TestCase(Steel)]
        [TestCase(CastIron)]
        [TestCase(Starsteel)]
        public void IsOfferable_ProductMineral_ReturnsTrue(string id)
        {
            Assert.That(OfferingRules.IsOfferable(_content.GetItem(IdOf(_content, id))), Is.True);
        }

        [TestCase(Iron)]
        [TestCase(Silver)]
        [TestCase(Mithril)]
        [TestCase(Ash)]
        [TestCase(Lens)]
        [TestCase(Bellows)]
        [TestCase(Scrap)]
        public void IsOfferable_NonProduct_ReturnsFalse(string id)
        {
            Assert.That(OfferingRules.IsOfferable(_content.GetItem(IdOf(_content, id))), Is.False);
        }

        [Test]
        public void HasOfferable_BagWithProduct_ReturnsTrue()
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { Iron, Scrap, Steel });

            Assert.That(OfferingRules.HasOfferable(run, _content), Is.True);
        }

        [Test]
        public void HasOfferable_BagWithoutProduct_ReturnsFalse()
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { Iron, Scrap, Lens, Bellows });

            Assert.That(OfferingRules.HasOfferable(run, _content), Is.False);
        }
    }
}
