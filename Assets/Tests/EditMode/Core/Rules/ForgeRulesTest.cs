using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><see cref="ForgeRules.RollEquipment"/> (forge-battle R2, D4).</summary>
    public class ForgeRulesTest
    {
        [Test]
        public void RollEquipment_SingleOptionPool_PicksIt()
        {
            var content = ContentFactory.Create(battle: CreateBattle(seasons: CreateSingleOptionSeasons(Sword)));
            var run = StateFactory.CreateActiveRun();

            ForgeRules.RollEquipment(run, content);

            Assert.That(run.DecreeEquipment, Is.EqualTo(new EquipmentId(3)));
        }

        [Test]
        public void RollEquipment_SingleOptionPool_StillAdvancesForgeOnce()
        {
            var content = ContentFactory.Create(battle: CreateBattle(seasons: CreateSingleOptionSeasons(Armor)));
            var run = StateFactory.CreateActiveRun();
            var expected = run.Rng.Get(RngStream.Forge);
            expected.NextInt(1);

            ForgeRules.RollEquipment(run, content);

            Assert.That(run.Rng.Get(RngStream.Forge).State, Is.EqualTo(expected.State));
        }

        [Test]
        public void RollEquipment_ZeroWeightOption_NeverPicked()
        {
            var seasons = CreateSeasons();
            seasons[0] = new SeasonData(OpponentOf(1),
                new[] { new ForgeOptionData(Sword, 0), new ForgeOptionData(Armor, 1), new ForgeOptionData(Spear, 1) });
            var content = ContentFactory.Create(battle: CreateBattle(seasons: seasons));
            var sword = EquipmentOf(content, Sword);

            for (ulong seed = 0; seed < 1000; seed++)
            {
                var run = CreateRun(seed);
                ForgeRules.RollEquipment(run, content);

                Assert.That(run.DecreeEquipment, Is.Not.EqualTo(sword), $"seed {seed}");
            }
        }

        [Test]
        public void RollEquipment_Weights_PickByCumulativeWeight([Values(1, 2)] int season)
        {
            var content = ContentFactory.Create();
            var sawDifferent = false;
            EquipmentId? first = null;
            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = CreateRun(seed);
                run.Season = season;
                var stream = run.Rng.Get(RngStream.Forge);
                var expected = ExpectedEquipment(content, season, ref stream);

                ForgeRules.RollEquipment(run, content);

                Assert.That(run.DecreeEquipment, Is.EqualTo(expected), $"seed {seed}");
                first ??= expected;
                sawDifferent |= expected != first.Value;
            }

            Assert.That(sawDifferent, Is.True, "pool should yield more than one equipment");
        }

        [Test]
        public void RollEquipment_DoesNotTouchOtherStreams()
        {
            var content = ContentFactory.Create();
            var run = StateFactory.CreateActiveRun();
            var before = run.Rng.Capture();

            ForgeRules.RollEquipment(run, content);

            MarketRulesShopTest.AssertOnlyStreamAdvanced(before, run.Rng.Capture(), RngStream.Forge);
        }

        [Test]
        public void RollEquipment_UsesPoolOfCurrentSeason()
        {
            var seasons = CreateSingleOptionSeasons(Sword);
            seasons[1] = new SeasonData(OpponentOf(2), new[] { new ForgeOptionData(Shield, 3) });
            var content = ContentFactory.Create(battle: CreateBattle(seasons: seasons));
            var run = StateFactory.CreateActiveRun();
            run.Season = 2;

            ForgeRules.RollEquipment(run, content);

            Assert.That(run.DecreeEquipment, Is.EqualTo(new EquipmentId(1)), "eq_shield = index 1");
        }

        /// <summary>Dự đoán kết quả rút từ một bản sao stream <c>Forge</c> (1 <c>NextInt(Σ)</c>, trọng số tích lũy).</summary>
        internal static EquipmentId ExpectedEquipment(ContentDb content, int season, ref Pcg32 stream)
        {
            var pool = content.Season(season).ForgePool;
            var roll = stream.NextInt(content.Season(season).TotalWeight);
            for (var i = 0; i < pool.Count; i++)
            {
                if (roll < pool[i].Weight)
                    return pool[i].Equipment;
                roll -= pool[i].Weight;
            }

            throw new System.InvalidOperationException("roll outside pool");
        }

        private static RunState CreateRun(ulong seed)
        {
            var run = StateFactory.CreateActiveRun();
            run.Seed = seed;
            run.Rng = RngStreams.Create(seed);
            return run;
        }
    }
}
