using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary>Vào Chiến lợi phẩm qua <c>AckBattle</c> Thắng (spoils R2, R3, R4, D3, D12; run-flow AC-6, AC-6b).</summary>
    public class SpoilsRulesTest
    {
        private const int BagCapacityOfOfferingRun = 14;

        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _meta = new MetaState();
        }

        [Test]
        public void Enter_Win_AddsSpoilsGoldOnce()
        {
            var content = CreateWithSpoils();
            var run = StateFactory.CreateWonBattleRun(content, season: 4);
            run.Gold = 7;

            Apply(run, content, new AckBattle());

            Assert.That(run.Gold, Is.EqualTo(7 + 4 * SpoilsGoldPerSeason));
        }

        [Test]
        public void Enter_PoolOfSixClaimable_OffersThreeDistinctFromPool()
        {
            var content = CreateWithSpoils();
            var pool = content.Season(1).RewardPool.Select(o => o.Reward).ToArray();

            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = WonRun(content, 1, seed);

                Apply(run, content, new AckBattle());

                Assert.That(run.SpoilsOptions.Count, Is.EqualTo(ContentDb.MaxSpoilsOptions), $"seed {seed}");
                Assert.That(run.SpoilsOptions, Is.Unique, $"seed {seed}");
                Assert.That(run.SpoilsOptions, Is.SubsetOf(pool), $"seed {seed}");
            }
        }

        [Test]
        public void Enter_PoolOfTwoClaimable_OffersTwo()
        {
            var content = CreateWithSpoils(new[] { new RewardOptionData(RewardLens, 1), new RewardOptionData(RewardSilver, 1) });
            var run = StateFactory.CreateWonBattleRun(content, season: 1);

            Apply(run, content, new AckBattle());

            Assert.That(run.SpoilsOptions, Is.EquivalentTo(new[] { RewardOf(content, RewardLens), RewardOf(content, RewardSilver) }));
        }

        [Test]
        public void Enter_EmptyPool_OffersNoneAndStaysInSpoils()
        {
            var content = CreateWithSpoils(new RewardOptionData[0]);
            var run = StateFactory.CreateWonBattleRun(content, season: 1);

            Apply(run, content, new AckBattle());

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Spoils));
            Assert.That(run.SpoilsOptions, Is.Empty);
        }

        [Test]
        public void Enter_ZeroWeightOption_NeverOffered()
        {
            var content = CreateWithSpoils(new[]
            {
                new RewardOptionData(RewardLens, 1), new RewardOptionData(RewardSilver, 1),
                new RewardOptionData(RewardMithril, 0), new RewardOptionData(RewardBag, 1),
            });

            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = WonRun(content, 1, seed);

                Apply(run, content, new AckBattle());

                Assert.That(run.SpoilsOptions, Has.No.Member(RewardOf(content, RewardMithril)), $"seed {seed}");
            }
        }

        [Test]
        public void Enter_BagFull_OffersOnlyExpandBag()
        {
            var content = CreateWithSpoils();
            var run = StateFactory.CreateWonBattleRun(content, season: 1, Enumerable.Repeat(Iron, 8).ToArray());
            run.BagCapacity = 8;

            Apply(run, content, new AckBattle());

            Assert.That(run.SpoilsOptions, Is.EqualTo(new[] { RewardOf(content, RewardBag) }));
        }

        [Test]
        public void Enter_CatalystSlotsFull_OffersNoCatalyst()
        {
            var content = CreateWithSpoils();

            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = WonRun(content, 2, seed, Bellows, Mortar, Tongs, Bellows, Mortar);

                Apply(run, content, new AckBattle());

                Assert.That(run.SpoilsOptions.Any(id => content.GetReward(id).IsCatalyst), Is.False, $"seed {seed}");
                Assert.That(run.SpoilsOptions.Count, Is.EqualTo(ContentDb.MaxSpoilsOptions), $"seed {seed}");
            }
        }

        [Test]
        public void Enter_BagAtMaxCapacity_OffersNoExpandBag()
        {
            var content = CreateWithSpoils();

            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = WonRun(content, 1, seed);
                run.BagCapacity = MaxBagCapacity;

                Apply(run, content, new AckBattle());

                Assert.That(run.SpoilsOptions, Has.No.Member(RewardOf(content, RewardBag)), $"seed {seed}");
            }
        }

        [Test]
        public void Enter_GuaranteeCatalystWithRoom_AlwaysOffersCatalyst([Values(2, 3)] int season)
        {
            var content = CreateWithSpoils();

            for (ulong seed = 0; seed < 500; seed++)
            {
                var run = WonRun(content, season, seed);

                Apply(run, content, new AckBattle());

                Assert.That(run.SpoilsOptions.Any(id => content.GetReward(id).IsCatalyst), Is.True, $"seed {seed}");
            }
        }

        [Test]
        public void Enter_WithoutGuarantee_SometimesOffersNoCatalyst()
        {
            var content = CreateWithSpoils();
            var sawNoCatalyst = false;

            for (ulong seed = 0; seed < 200 && !sawNoCatalyst; seed++)
            {
                var run = WonRun(content, 1, seed);
                Apply(run, content, new AckBattle());
                sawNoCatalyst = !run.SpoilsOptions.Any(id => content.GetReward(id).IsCatalyst);
            }

            Assert.That(sawNoCatalyst, Is.True);
        }

        [Test]
        public void Enter_Options_OrderedByPoolDeclaration()
        {
            var content = CreateWithSpoils();
            var order = content.Season(2).RewardPool.Select(o => o.Reward).ToList();

            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = WonRun(content, 2, seed);

                Apply(run, content, new AckBattle());

                var positions = run.SpoilsOptions.Select(id => order.IndexOf(id)).ToArray();
                Assert.That(positions, Is.Ordered.Ascending, $"seed {seed}");
            }
        }

        [Test]
        public void Enter_SameSeed_SameOptions()
        {
            var content = CreateWithSpoils();
            var first = WonRun(content, 2, 77);
            var second = WonRun(content, 2, 77);

            Apply(first, content, new AckBattle());
            Apply(second, content, new AckBattle());

            Assert.That(second.SpoilsOptions, Is.EqualTo(first.SpoilsOptions));
        }

        [Test]
        public void Enter_OnlyRewardStreamAdvances()
        {
            var content = CreateWithSpoils();
            var run = StateFactory.CreateWonBattleRun(content, season: 2);
            var before = run.Rng.Capture();

            Apply(run, content, new AckBattle());

            MarketRulesShopTest.AssertOnlyStreamAdvanced(before, run.Rng.Capture(), RngStream.Reward);
        }

        [Test]
        public void Enter_ThreeEqualCandidates_DrawsOncePerOption()
        {
            var content = CreateWithSpoils(new[]
            {
                new RewardOptionData(RewardLens, 1), new RewardOptionData(RewardSilver, 1),
                new RewardOptionData(RewardMithril, 1),
            });
            var run = StateFactory.CreateWonBattleRun(content, season: 1);
            var expected = run.Rng.Get(RngStream.Reward);
            expected.NextInt(3);
            expected.NextInt(2);
            expected.NextInt(1);

            Apply(run, content, new AckBattle());

            Assert.That(run.Rng.Get(RngStream.Reward).State, Is.EqualTo(expected.State));
        }

        [Test]
        public void Enter_EmitsSpoilsStartedWithGoldAndOptions()
        {
            var content = CreateWithSpoils();
            var run = StateFactory.CreateWonBattleRun(content, season: 3);

            var result = Apply(run, content, new AckBattle());

            var started = result.Events.OfType<SpoilsStarted>().Single();
            Assert.That(started.Season, Is.EqualTo(3));
            Assert.That(started.Gold, Is.EqualTo(3 * SpoilsGoldPerSeason));
            Assert.That(started.Options, Is.EqualTo(run.SpoilsOptions));
        }

        [Test]
        public void IsClaimable_ItemWithRoom_ReturnsTrue()
        {
            var content = CreateWithSpoils();
            var run = StateFactory.CreateSpoilsRun(content, new[] { Iron }, 1);

            Assert.That(SpoilsRules.IsClaimable(run, content, content.GetReward(RewardOf(content, RewardMithril))), Is.True);
        }

        [Test]
        public void IsClaimable_ItemWhenBagFull_ReturnsFalse()
        {
            var content = CreateWithSpoils();
            var run = StateFactory.CreateSpoilsRun(content, Enumerable.Repeat(Iron, BagCapacityOfOfferingRun).ToArray(), 1);

            Assert.That(SpoilsRules.IsClaimable(run, content, content.GetReward(RewardOf(content, RewardMithril))), Is.False);
        }

        private static RunState WonRun(ContentDb content, int season, ulong seed, params string[] bag)
        {
            var run = StateFactory.CreateWonBattleRun(content, season, bag);
            run.Seed = seed;
            run.Rng = RngStreams.Create(seed);
            return run;
        }

        private CommandResult Apply(RunState run, ContentDb content, ICommand command)
        {
            var result = GameRules.Apply(run, _meta, content, command);
            Assert.That(result.Accepted, Is.True, $"arrange: {command.GetType().Name} rejected {result.Reason}");
            return result;
        }
    }
}
