using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    public class MarketRulesPriceTest
    {
        private ContentDb _content;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _run = StateFactory.CreateWorkbenchRun(_content, new string[0]);
        }

        [Test]
        public void BuyPrice_SeasonPercent_ScalesMarketPrice()
        {
            _run.SeasonPricePercent = 130;

            Assert.That(MarketRules.BuyPrice(_run, _content, IdOf(_content, Iron)), Is.EqualTo(5));
        }

        [Test]
        public void SellPrice_Mineral_IsSellRateOfBuyPrice()
        {
            Assert.That(MarketRules.SellPrice(_run, _content, IdOf(_content, Steel)), Is.EqualTo(9));
        }

        [TestCase(70)]
        [TestCase(130)]
        public void SellPrice_Scrap_ReturnsScrapSellPriceAtAnySeason(int seasonPercent)
        {
            _run.SeasonPricePercent = seasonPercent;

            Assert.That(MarketRules.SellPrice(_run, _content, IdOf(_content, Scrap)), Is.EqualTo(ScrapSellPrice));
        }

        [Test]
        public void SellPrice_SameItemDifferentQualityAndMagic_SamePrice()
        {
            var steel = IdOf(_content, Steel);
            _run.Bag.Add(new ItemInstance(1, steel, magic: false, quality: 5));
            _run.Bag.Add(new ItemInstance(2, steel, magic: true, quality: 900));

            var first = MarketRules.SellPrice(_run, _content, _run.Bag[0].ItemId);
            var second = MarketRules.SellPrice(_run, _content, _run.Bag[1].ItemId);

            Assert.That(first, Is.GreaterThan(0));
            Assert.That(second, Is.EqualTo(first));
        }

        [TestCase(0, 2)]
        [TestCase(2, 4)]
        public void RerollPrice_AfterRerolls_IncreasesByStep(int rerollCount, int expected)
        {
            _run.Shop.RerollCount = rerollCount;

            Assert.That(MarketRules.RerollPrice(_run, _content), Is.EqualTo(expected));
        }
    }
}
