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

        [TestCase(12, 75, 100, 9)]
        [TestCase(12, 75, 50, 4)]
        [TestCase(12, 75, 120, 10)]
        public void Sell_WithModifier_FloorsProduct(int buy, int rate, int modifier, int expected)
        {
            Assert.That(PriceFormula.Sell(buy, rate, modifier), Is.EqualTo(expected));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Reroll_WithinFreeRerolls_ReturnsZero(int rerollCount)
        {
            Assert.That(PriceFormula.Reroll(2, 1, rerollCount, 2, 100), Is.EqualTo(0));
        }

        [TestCase(2, 2)]
        [TestCase(3, 3)]
        public void Reroll_AfterFreeRerolls_StartsAtBase(int rerollCount, int expected)
        {
            Assert.That(PriceFormula.Reroll(2, 1, rerollCount, 2, 100), Is.EqualTo(expected));
        }

        [TestCase(0, 200, 4)]
        [TestCase(2, 150, 6)]
        [TestCase(1, 50, 1)]
        public void Reroll_WithModifier_ScalesPrice(int rerollCount, int modifier, int expected)
        {
            Assert.That(PriceFormula.Reroll(2, 1, rerollCount, 0, modifier), Is.EqualTo(expected));
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
