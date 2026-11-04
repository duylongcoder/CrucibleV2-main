using System;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Content
{
    /// <summary>Phần thưởng + Chiến lợi phẩm theo Mùa trong <see cref="ContentDb"/> (spoils R1, D2, D5).</summary>
    public class ContentDbRewardTest
    {
        [Test]
        public void Create_ValidRewards_ResolvesKindItemAndAmount()
        {
            var content = ContentFactory.CreateWithSpoils();

            var mithril = content.GetReward(RewardOf(content, RewardMithril));
            var bag = content.GetReward(RewardOf(content, RewardBag));
            var bellows = content.GetReward(RewardOf(content, RewardBellows));

            Assert.That(mithril.Kind, Is.EqualTo(RewardKind.Item));
            Assert.That(mithril.Item, Is.EqualTo(IdOf(content, Mithril)));
            Assert.That(mithril.IsCatalyst, Is.False);
            Assert.That(mithril.Id, Is.EqualTo(RewardMithril));
            Assert.That(bag.Kind, Is.EqualTo(RewardKind.ExpandBag));
            Assert.That(bag.Amount, Is.EqualTo(RewardBagAmount));
            Assert.That(bellows.IsCatalyst, Is.True);
        }

        [Test]
        public void TryFindReward_KnownId_ReturnsSortedIndex()
        {
            var content = ContentFactory.CreateWithSpoils();

            Assert.That(content.TryFindReward(RewardBag, out var bag), Is.True);
            Assert.That(content.TryFindReward(RewardSilver, out var silver), Is.True);
            Assert.That(bag.Index, Is.EqualTo(0));
            Assert.That(silver.Index, Is.EqualTo(5));
        }

        [Test]
        public void TryFindReward_UnknownId_ReturnsFalse()
        {
            Assert.That(ContentFactory.CreateWithSpoils().TryFindReward("reward_none", out _), Is.False);
        }

        [Test]
        public void Create_SeasonRewardPool_ResolvedInDeclarationOrder()
        {
            var content = ContentFactory.CreateWithSpoils();

            var pool = content.Season(2).RewardPool;

            Assert.That(pool.Count, Is.EqualTo(6));
            Assert.That(pool[0].Reward, Is.EqualTo(RewardOf(content, RewardMithril)));
            Assert.That(pool[0].Weight, Is.EqualTo(30));
            Assert.That(pool[5].Reward, Is.EqualTo(RewardOf(content, RewardMortar)));
            Assert.That(content.Season(2).SpoilsGold, Is.EqualTo(2 * SpoilsGoldPerSeason));
            Assert.That(content.Season(2).GuaranteeCatalyst, Is.True);
        }

        [Test]
        public void Create_DuplicateRewardId_Throws()
        {
            var rewards = new[]
            {
                new RewardData(RewardMithril, RewardKind.Item, Mithril),
                new RewardData(RewardMithril, RewardKind.Item, Silver),
            };

            Assert.Throws<ArgumentException>(() => ContentFactory.Create(rewards: rewards));
        }

        [Test]
        public void Create_ItemRewardUnknownItem_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                ContentFactory.Create(rewards: new[] { new RewardData("reward_x", RewardKind.Item, "ore_none") }));
        }

        [Test]
        public void Create_ItemRewardWithRecipe_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                ContentFactory.Create(rewards: new[] { new RewardData("reward_x", RewardKind.Item, Steel) }));
        }

        [Test]
        public void Create_ItemRewardScrap_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                ContentFactory.Create(rewards: new[] { new RewardData("reward_x", RewardKind.Item, Scrap) }));
        }

        [Test]
        public void Create_ItemRewardWithAmount_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                ContentFactory.Create(rewards: new[] { new RewardData("reward_x", RewardKind.Item, Iron, 1) }));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Create_ExpandBagAmountBelowOne_Throws(int amount)
        {
            Assert.Throws<ArgumentException>(() =>
                ContentFactory.Create(rewards: new[] { new RewardData("reward_x", RewardKind.ExpandBag, amount: amount) }));
        }

        [Test]
        public void Create_ExpandBagWithItem_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                ContentFactory.Create(rewards: new[] { new RewardData("reward_x", RewardKind.ExpandBag, Iron, 1) }));
        }

        [Test]
        public void Create_SeasonPoolUnknownReward_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateWithPool(new RewardOptionData("reward_none", 1)));
        }

        [Test]
        public void Create_SeasonPoolDuplicateReward_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                CreateWithPool(new RewardOptionData(RewardLens, 1), new RewardOptionData(RewardLens, 2)));
        }

        [Test]
        public void Create_SeasonPoolNegativeWeight_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateWithPool(new RewardOptionData(RewardLens, -1)));
        }

        [Test]
        public void Create_SeasonPoolNullEntry_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateWithPool(new RewardOptionData[] { null }));
        }

        [TestCase(-1)]
        [TestCase(ContentDb.MaxSpoilsGold + 1)]
        public void Create_SpoilsGoldOutOfRange_Throws(int gold)
        {
            var seasons = CreateSeasons();
            seasons[0] = new SeasonData(seasons[0].OpponentId, seasons[0].ForgePool, gold);

            Assert.Throws<ArgumentException>(() => ContentFactory.Create(battle: CreateBattle(seasons)));
        }

        [Test]
        public void Create_GuaranteeCatalystWithoutCatalystReward_Throws()
        {
            var seasons = CreateSeasons();
            seasons[1] = new SeasonData(seasons[1].OpponentId, seasons[1].ForgePool, 0,
                new[] { new RewardOptionData(RewardLens, 1), new RewardOptionData(RewardBellows, 0) }, true);

            Assert.Throws<ArgumentException>(() => ContentFactory.Create(battle: CreateBattle(seasons)));
        }

        [TestCase("gold")]
        [TestCase("pool")]
        public void Create_LastSeasonDeclaresSpoils_Throws(string field)
        {
            var seasons = CreateSeasons();
            var last = seasons[SeasonCount - 1];
            seasons[SeasonCount - 1] = field switch
            {
                "gold" => new SeasonData(last.OpponentId, last.ForgePool, 1),
                _ => new SeasonData(last.OpponentId, last.ForgePool, 0, new[] { new RewardOptionData(RewardLens, 1) }),
            };

            Assert.Throws<ArgumentException>(() => ContentFactory.Create(battle: CreateBattle(seasons)));
        }

        private static ContentDb CreateWithPool(params RewardOptionData[] pool)
        {
            var seasons = CreateSeasons();
            seasons[0] = new SeasonData(seasons[0].OpponentId, seasons[0].ForgePool, 0, pool);
            return ContentFactory.Create(battle: CreateBattle(seasons));
        }
    }
}
