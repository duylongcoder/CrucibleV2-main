using System;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Content
{
    public class ContentDbMarketTest
    {
        private static readonly RunData Run = new RunData(StartingReputation, StartingBagCapacity, MaxBagCapacity, SeasonCount);

        [Test]
        public void Create_ValidMarket_BuildsPoolsInItemIdOrder()
        {
            var db = ContentFactory.Create();

            Assert.That(db.ShopPool(1), Is.EqualTo(new[]
            {
                IdOf(db, Ash), IdOf(db, Sand), IdOf(db, Coal), IdOf(db, Copper), IdOf(db, Iron),
            }));
            Assert.That(db.ShopPool(2), Is.EqualTo(new[] { IdOf(db, Bone) }));
            Assert.That(db.ShopPool(3), Is.EqualTo(new[] { IdOf(db, Silver) }));
            Assert.That(db.ShopPool(4), Is.Empty);
            Assert.That(db.Lenses, Is.EqualTo(new[] { IdOf(db, Lens) }));
            Assert.That(db.Catalysts, Is.EqualTo(new[] { IdOf(db, Bellows), IdOf(db, Mortar), IdOf(db, Tongs) }));
        }

        [Test]
        public void Create_NegativeMarketPrice_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                Create(Market(), new ItemData("ore_bad", ItemKind.Mineral, 1, marketPrice: -1)));

            Assert.That(ex.Message, Does.Contain("ore_bad"));
        }

        [TestCase(0)]
        [TestCase(11)]
        public void Create_ShopMaxTierOutOfRange_Throws(int tier)
        {
            var weights = new int[Math.Max(tier, 0)];
            for (var i = 0; i < weights.Length; i++)
                weights[i] = 1;

            Assert.Throws<ArgumentException>(() => Create(Market(shopMaxTier: tier, tierWeights: weights)));
        }

        [TestCase(0)]
        [TestCase(6)]
        public void Create_IngredientSlotCountOutOfRange_Throws(int count)
        {
            Assert.Throws<ArgumentException>(() => Create(Market(ingredientSlotCount: count)));
        }

        [Test]
        public void Create_TierWeightsWrongLength_Throws()
        {
            Assert.Throws<ArgumentException>(() => Create(Market(tierWeights: new[] { 70, 30 })));
        }

        [TestCase(-1, 25, 5)]
        [TestCase(0, 0, 0)]
        public void Create_TierWeightsNegativeOrAllZero_Throws(int w1, int w2, int w3)
        {
            Assert.Throws<ArgumentException>(() => Create(Market(tierWeights: new[] { w1, w2, w3 })));
        }

        [TestCase(-1, 50)]
        [TestCase(50, -1)]
        [TestCase(0, 0)]
        public void Create_UtilityWeightsInvalid_Throws(int lens, int catalyst)
        {
            Assert.Throws<ArgumentException>(() => Create(Market(lensWeight: lens, catalystWeight: catalyst)));
        }

        [TestCase(0, 130)]
        [TestCase(131, 130)]
        [TestCase(70, 1001)]
        public void Create_SeasonPriceRangeInvalid_Throws(int min, int max)
        {
            Assert.Throws<ArgumentException>(() => Create(Market(seasonMin: min, seasonMax: max)));
        }

        [TestCase(-1)]
        [TestCase(100)]
        public void Create_SellRateOutOfRange_Throws(int rate)
        {
            Assert.Throws<ArgumentException>(() => Create(Market(sellRate: rate)));
        }

        [TestCase(-1, 1, 1)]
        [TestCase(2, -1, 1)]
        [TestCase(2, 1, -1)]
        public void Create_NegativeRerollOrScrapPrice_Throws(int rerollBase, int rerollStep, int scrapPrice)
        {
            Assert.Throws<ArgumentException>(() =>
                Create(Market(rerollBase: rerollBase, rerollStep: rerollStep, scrapPrice: scrapPrice)));
        }

        [Test]
        public void Create_CatalystSlotsBelowOne_Throws()
        {
            Assert.Throws<ArgumentException>(() => Create(Market(catalystSlots: 0)));
        }

        private static MarketData Market(int shopMaxTier = 3, int ingredientSlotCount = 4, int[] tierWeights = null,
                                         int lensWeight = 50, int catalystWeight = 50, int seasonMin = 70,
                                         int seasonMax = 130, int sellRate = 75, int rerollBase = 2, int rerollStep = 1,
                                         int catalystSlots = 5, int scrapPrice = 1) =>
            new MarketData(shopMaxTier, ingredientSlotCount, tierWeights ?? new[] { 70, 25, 5 }, lensWeight,
                catalystWeight, seasonMin, seasonMax, sellRate, rerollBase, rerollStep, catalystSlots, scrapPrice);

        private static ContentDb Create(MarketData market, params ItemData[] items) =>
            ContentDb.Create(items, Array.Empty<DifficultyData>(), Array.Empty<StarterKitData>(), Run, CreateBrewing(),
                market, CreateBattle());
    }
}
