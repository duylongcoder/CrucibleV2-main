using System;
using System.Linq;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><see cref="MarketRules.RefreshShop"/> và <see cref="MarketRules.RollSeasonPrice"/> (economy.md §Chợ).</summary>
    public class MarketRulesShopTest
    {
        private const int UtilitySlot = IngredientSlotCount;

        private ContentDb _content;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _run = StateFactory.CreateWorkbenchRun(_content, new string[0]);
        }

        [Test]
        public void RefreshShop_Default_FillsFourIngredientsAndOneUtility()
        {
            MarketRules.RefreshShop(_run, _content);

            var slots = _run.Shop.Slots;
            Assert.That(slots.Count, Is.EqualTo(IngredientSlotCount + 1));
            Assert.That(slots.All(slot => slot.IsAvailable), Is.True);
            for (var i = 0; i < IngredientSlotCount; i++)
                Assert.That(KindOf(_content, slots[i]), Is.EqualTo(ItemKind.Mineral).Or.EqualTo(ItemKind.Additive), $"slot {i}");
            Assert.That(KindOf(_content, slots[UtilitySlot]), Is.EqualTo(ItemKind.Lens).Or.EqualTo(ItemKind.Catalyst));
        }

        [Test]
        public void RefreshShop_ManySeeds_NeverOffersTierAboveMaxOrRecipe()
        {
            var offered = 0;
            for (ulong seed = 0; seed < 2000; seed++)
            {
                var run = CreateRun(_content, seed);
                for (var turn = 0; turn < 5; turn++)
                {
                    MarketRules.RefreshShop(run, _content);
                    Assert.That(run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1), $"seed {seed}");
                    for (var i = 0; i < IngredientSlotCount; i++)
                    {
                        var item = _content.GetItem(run.Shop.Slots[i].ItemId);
                        Assert.That(item.Recipe, Is.Null, $"seed {seed}: {item.Id}");
                        Assert.That(item.Tier, Is.InRange(1, ShopMaxTier), $"seed {seed}: {item.Id}");
                        offered++;
                    }
                }
            }

            Assert.That(offered, Is.EqualTo(2000 * 5 * IngredientSlotCount));
        }

        [Test]
        public void RefreshShop_ManyDraws_TierShareMatchesWeights()
        {
            var counts = new int[ShopMaxTier + 1];
            const int refreshes = 2500;
            for (var r = 0; r < refreshes; r++)
            {
                MarketRules.RefreshShop(_run, _content);
                Assert.That(_run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
                for (var i = 0; i < IngredientSlotCount; i++)
                    counts[_content.GetItem(_run.Shop.Slots[i].ItemId).Tier]++;
            }

            // Seed cố định → tất định; đổi thuật toán rút có thể phải xem lại dung sai.
            const double total = refreshes * IngredientSlotCount;
            Assert.That(counts[1] / total, Is.EqualTo(0.70).Within(0.03));
            Assert.That(counts[2] / total, Is.EqualTo(0.25).Within(0.03));
            Assert.That(counts[3] / total, Is.EqualTo(0.05).Within(0.03));
        }

        [Test]
        public void RefreshShop_TierWithoutItems_RedistributesWeight()
        {
            var content = CreateContent(
                new ItemData("ore_a", ItemKind.Mineral, 1, marketPrice: 3),
                new ItemData("add_b", ItemKind.Additive, 2, marketPrice: 4),
                new ItemData(Lens, ItemKind.Lens, 1, marketPrice: 6));
            var run = CreateRun(content, StateFactory.Seed);

            for (var r = 0; r < 200; r++)
            {
                MarketRules.RefreshShop(run, content);
                Assert.That(run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
                for (var i = 0; i < IngredientSlotCount; i++)
                {
                    Assert.That(run.Shop.Slots[i].IsAvailable, Is.True);
                    Assert.That(content.GetItem(run.Shop.Slots[i].ItemId).Tier, Is.InRange(1, 2));
                }
            }
        }

        [Test]
        public void RefreshShop_NoIngredientPool_LeavesIngredientSlotsEmpty()
        {
            var content = CreateContent(
                new ItemData(Lens, ItemKind.Lens, 1, marketPrice: 6),
                new ItemData(Bellows, ItemKind.Catalyst, 1, marketPrice: 10));
            var run = CreateRun(content, StateFactory.Seed);

            MarketRules.RefreshShop(run, content);

            Assert.That(run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
            for (var i = 0; i < IngredientSlotCount; i++)
                Assert.That(run.Shop.Slots[i].IsAvailable, Is.False, $"slot {i}");
            Assert.That(run.Shop.Slots[UtilitySlot].IsAvailable, Is.True);
        }

        [Test]
        public void RefreshShop_CatalystSlotsFull_UtilityIsAlwaysLens()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Bellows, Mortar, Tongs, Bellows, Mortar });

            for (var r = 0; r < 500; r++)
            {
                MarketRules.RefreshShop(run, _content);
                Assert.That(run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
                Assert.That(KindOf(_content, run.Shop.Slots[UtilitySlot]), Is.EqualTo(ItemKind.Lens), $"refresh {r}");
            }
        }

        [Test]
        public void RefreshShop_CatalystWeightZero_UtilityIsAlwaysLens()
        {
            var content = ContentFactory.Create(market: CreateMarket(catalystWeight: 0));
            var run = CreateRun(content, StateFactory.Seed);

            for (var r = 0; r < 500; r++)
            {
                MarketRules.RefreshShop(run, content);
                Assert.That(run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
                Assert.That(KindOf(content, run.Shop.Slots[UtilitySlot]), Is.EqualTo(ItemKind.Lens), $"refresh {r}");
            }
        }

        [Test]
        public void RefreshShop_BothUtilityKinds_OffersEachKind()
        {
            var lens = 0;
            var catalyst = 0;
            for (var r = 0; r < 200; r++)
            {
                MarketRules.RefreshShop(_run, _content);
                Assert.That(_run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
                if (KindOf(_content, _run.Shop.Slots[UtilitySlot]) == ItemKind.Lens)
                    lens++;
                else
                    catalyst++;
            }

            Assert.That(lens, Is.GreaterThan(0));
            Assert.That(catalyst, Is.GreaterThan(0));
        }

        [Test]
        public void RefreshShop_NoUtilityContent_LeavesUtilitySlotEmpty()
        {
            var content = CreateContent(new ItemData("ore_a", ItemKind.Mineral, 1, marketPrice: 3));
            var run = CreateRun(content, StateFactory.Seed);

            MarketRules.RefreshShop(run, content);

            Assert.That(run.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
            Assert.That(run.Shop.Slots[0].IsAvailable, Is.True);
            Assert.That(run.Shop.Slots[UtilitySlot].IsAvailable, Is.False);
        }

        [Test]
        public void RefreshShop_AfterRerolls_ResetsRerollCount()
        {
            _run.Shop.RerollCount = 3;

            MarketRules.RefreshShop(_run, _content);

            Assert.That(_run.Shop.RerollCount, Is.EqualTo(0));
            Assert.That(MarketRules.RerollPrice(_run, _content), Is.EqualTo(RerollBase));
        }

        [Test]
        public void RefreshShop_SameSeed_SameSlots()
        {
            var first = CreateRun(_content, 42);
            var second = CreateRun(_content, 42);

            MarketRules.RefreshShop(first, _content);
            MarketRules.RefreshShop(second, _content);

            Assert.That(first.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
            Assert.That(second.Shop.Slots.Select(slot => slot.ItemId), Is.EqualTo(first.Shop.Slots.Select(slot => slot.ItemId)));
        }

        [Test]
        public void RefreshShop_OnlyAdvancesShopStream()
        {
            var before = _run.Rng.Capture();

            MarketRules.RefreshShop(_run, _content);

            AssertOnlyStreamAdvanced(before, _run.Rng.Capture(), RngStream.Shop);
        }

        [Test]
        public void RollSeasonPrice_ManySeeds_StaysWithinRange()
        {
            var sawMin = false;
            var sawMax = false;
            for (ulong seed = 0; seed < 5000; seed++)
            {
                var run = CreateRun(_content, seed);
                MarketRules.RollSeasonPrice(run, _content);

                Assert.That(run.SeasonPricePercent, Is.InRange(SeasonPriceMin, SeasonPriceMax), $"seed {seed}");
                sawMin |= run.SeasonPricePercent == SeasonPriceMin;
                sawMax |= run.SeasonPricePercent == SeasonPriceMax;
            }

            Assert.That(sawMin, Is.True);
            Assert.That(sawMax, Is.True);
        }

        [Test]
        public void RollSeasonPrice_OnlyAdvancesMarketStream()
        {
            var before = _run.Rng.Capture();

            MarketRules.RollSeasonPrice(_run, _content);

            AssertOnlyStreamAdvanced(before, _run.Rng.Capture(), RngStream.Market);
        }

        internal static void AssertOnlyStreamAdvanced(Pcg32State[] before, Pcg32State[] after, RngStream advanced)
        {
            for (var i = 0; i < RngStreams.Count; i++)
            {
                if (i == (int)advanced)
                    Assert.That(after[i], Is.Not.EqualTo(before[i]), $"{(RngStream)i} should advance");
                else
                    Assert.That(after[i], Is.EqualTo(before[i]), $"{(RngStream)i} should not advance");
            }
        }

        private static ItemKind KindOf(ContentDb content, ShopSlot slot) => content.GetItem(slot.ItemId).Kind;

        private static RunState CreateRun(ContentDb content, ulong seed)
        {
            var run = StateFactory.CreateWorkbenchRun(content, new string[0]);
            run.Seed = seed;
            run.Rng = RngStreams.Create(seed);
            return run;
        }

        private static ContentDb CreateContent(params ItemData[] items) =>
            ContentDb.Create(items, Array.Empty<DifficultyData>(), Array.Empty<StarterKitData>(),
                new RunData(StartingReputation, StartingBagCapacity, MaxBagCapacity, SeasonCount), CreateBrewing(),
                CreateMarket(), CreateBattle());
    }
}
