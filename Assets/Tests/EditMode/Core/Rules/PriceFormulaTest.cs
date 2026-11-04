using System;
using Crucible.Core.Rules;
using NUnit.Framework;

namespace Crucible.Tests.Core.Rules
{
    public class PriceFormulaTest
    {
        [Test]
        public void Buy_GddExample_ReturnsTwelve()
        {
            Assert.That(PriceFormula.Buy(10, 120, 100), Is.EqualTo(12));
        }

        [Test]
        public void Buy_FractionalResult_Floors()
        {
            Assert.That(PriceFormula.Buy(7, 130, 100), Is.EqualTo(9));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Buy_ResultBelowOne_ReturnsOne(int marketPrice)
        {
            Assert.That(PriceFormula.Buy(marketPrice, 70, 100), Is.EqualTo(1));
        }

        [TestCase(-1, 100, 100)]
        [TestCase(10, -1, 100)]
        [TestCase(10, 100, -1)]
        public void Buy_NegativeInput_Throws(int marketPrice, int seasonPercent, int modifierPercent)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PriceFormula.Buy(marketPrice, seasonPercent, modifierPercent));
        }

        [Test]
        public void Sell_GddExample_ReturnsNine()
        {
            Assert.That(PriceFormula.Sell(12, 75), Is.EqualTo(9));
        }

        [Test]
        public void Sell_BuyPriceOne_ReturnsZero()
        {
            Assert.That(PriceFormula.Sell(1, 75), Is.EqualTo(0));
        }

        [Test]
        public void Sell_AnyBuyPrice_PositiveAndBelowBuyPrice()
        {
            for (var buy = 2; buy <= 500; buy++)
                Assert.That(PriceFormula.Sell(buy, 75), Is.InRange(1, buy - 1), $"buy {buy}");
        }

        [TestCase(-1, 75)]
        [TestCase(10, -1)]
        public void Sell_NegativeInput_Throws(int buyPrice, int sellRate)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PriceFormula.Sell(buyPrice, sellRate));
        }

        [TestCase(0, 2)]
        [TestCase(1, 3)]
        [TestCase(2, 4)]
        public void Reroll_CountZeroToTwo_ReturnsTwoThreeFour(int rerollCount, int expected)
        {
            Assert.That(PriceFormula.Reroll(2, 1, rerollCount), Is.EqualTo(expected));
        }

        [TestCase(-1, 1, 0)]
        [TestCase(2, -1, 0)]
        [TestCase(2, 1, -1)]
        public void Reroll_NegativeInput_Throws(int rerollBase, int rerollStep, int rerollCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PriceFormula.Reroll(rerollBase, rerollStep, rerollCount));
        }
    }
}
