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

        [Test]
        public void SellPrice_SameProductDifferentLines_SamePrice()
        {
            var plain = new ItemInstance(1, IdOf(_content, Steel), quality: 10);
            var rich = new ItemInstance(2, IdOf(_content, Steel), true, 90,
                new[] { new StatLine(StatKind.Sharpness, 50), new StatLine(StatKind.Absorb, 30) });

            Assert.That(MarketRules.SellPrice(_run, _content, rich.ItemId),
                Is.EqualTo(MarketRules.SellPrice(_run, _content, plain.ItemId)));
        }

        [Test]
        public void BuyPrice_ActiveBuyModifier_Applies()
        {
            _run.SeasonPricePercent = 120;
            _run.ActiveModifier = ModifierOf(_content, ModDear);

            // ⌊12 × 120 × 150 / 10000⌋ = 21
            Assert.That(MarketRules.BuyPrice(_run, _content, IdOf(_content, Steel)), Is.EqualTo(21));
        }

        [Test]
        public void BuyPrice_Lens_UsesLensModifierNotBuy()
        {
            _run.ActiveModifier = ModifierOf(_content, ModDear);

            // ⌊6 × 100 × 50 / 10000⌋ = 3
            Assert.That(MarketRules.BuyPrice(_run, _content, IdOf(_content, Lens)), Is.EqualTo(3));
        }

        [Test]
        public void SellPrice_ActiveSellModifier_Applies()
        {
            _run.ActiveModifier = ModifierOf(_content, ModDear);

            // mua ⌊12 × 150 / 100⌋ = 18 → bán ⌊18 × 75 × 120 / 10000⌋ = 16
            Assert.That(MarketRules.SellPrice(_run, _content, IdOf(_content, Steel)), Is.EqualTo(16));
        }

        [Test]
        public void SellPrice_Scrap_IgnoresModifier()
        {
            _run.ActiveModifier = ModifierOf(_content, ModDear);

            Assert.That(MarketRules.SellPrice(_run, _content, IdOf(_content, Scrap)), Is.EqualTo(ScrapSellPrice));
        }

        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 4)]
        [TestCase(3, 6)]
        public void RerollPrice_ActiveFreeRerolls_FreeThenScaled(int rerollCount, int expected)
        {
            _run.ActiveModifier = ModifierOf(_content, ModDear);
            _run.Shop.RerollCount = rerollCount;

            Assert.That(MarketRules.RerollPrice(_run, _content), Is.EqualTo(expected));
        }
    }
}
