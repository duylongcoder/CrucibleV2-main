using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><see cref="ModifierRules"/> (spoils R10, R15, D12).</summary>
    public class ModifierRulesTest
    {
        [Test]
        public void Active_NoModifier_ReturnsNeutral()
        {
            var content = ContentFactory.Create();
            var run = StateFactory.CreateActiveRun();

            Assert.That(ModifierRules.Active(run, content), Is.SameAs(SeasonModifier.Neutral));
        }

        [Test]
        public void Active_WithModifier_ReturnsResolvedModifier()
        {
            var content = ContentFactory.Create();
            var run = StateFactory.CreateActiveRun();
            run.ActiveModifier = ModifierOf(content, ModDear);

            Assert.That(ModifierRules.Active(run, content).Id, Is.EqualTo(ModDear));
        }

        [Test]
        public void RollNext_OnlyNoModifierWeight_LeavesNextNull()
        {
            var content = ContentFactory.Create();
            var run = StateFactory.CreateActiveRun();
            run.NextModifier = ModifierOf(content, ModDear);

            ModifierRules.RollNext(run, content);

            Assert.That(run.NextModifier, Is.Null);
        }

        [Test]
        public void RollNext_OnlyOneModifier_SetsNext()
        {
            var content = CreateWithModifier(ModBrutal);
            var run = StateFactory.CreateActiveRun();

            ModifierRules.RollNext(run, content);

            Assert.That(run.NextModifier, Is.EqualTo(ModifierOf(content, ModBrutal)));
        }

        [Test]
        public void RollNext_DrawsSeasonStreamOnceWithTotalWeight()
        {
            var content = ContentFactory.Create();
            var run = StateFactory.CreateActiveRun();
            var expected = run.Rng.Get(RngStream.Season);
            expected.NextInt(content.Season(2).ModifierTotalWeight);

            ModifierRules.RollNext(run, content);

            Assert.That(run.Rng.Get(RngStream.Season).State, Is.EqualTo(expected.State));
        }

        [Test]
        public void RollNext_ZeroWeightModifier_NeverChosen()
        {
            var content = CreateWithPoolOnSeason(2, 0, new ModifierOptionData(ModBrutal, 0), new ModifierOptionData(ModDear, 1));

            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = CreateRun(seed);
                ModifierRules.RollNext(run, content);

                Assert.That(run.NextModifier, Is.EqualTo(ModifierOf(content, ModDear)), $"seed {seed}");
            }
        }

        [Test]
        public void RollNext_MixedWeights_YieldsBothNoneAndModifier()
        {
            var content = CreateWithPoolOnSeason(2, 1, new ModifierOptionData(ModDear, 1));
            var sawNone = false;
            var sawDear = false;

            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = CreateRun(seed);
                ModifierRules.RollNext(run, content);
                sawNone |= run.NextModifier == null;
                sawDear |= run.NextModifier == ModifierOf(content, ModDear);
            }

            Assert.That(sawNone && sawDear, Is.True);
        }

        [Test]
        public void RollNext_UsesPoolOfFollowingSeason()
        {
            var content = CreateWithPoolOnSeason(4, 0, new ModifierOptionData(ModBrutal, 1));
            var run = StateFactory.CreateActiveRun();
            run.Season = 3;

            ModifierRules.RollNext(run, content);

            Assert.That(run.NextModifier, Is.EqualTo(ModifierOf(content, ModBrutal)));
        }

        [Test]
        public void RollNext_OnlySeasonStreamAdvances()
        {
            var content = CreateWithModifier(ModDear);
            var run = StateFactory.CreateActiveRun();
            var before = run.Rng.Capture();

            ModifierRules.RollNext(run, content);

            MarketRulesShopTest.AssertOnlyStreamAdvanced(before, run.Rng.Capture(), RngStream.Season);
        }

        private static ContentDb CreateWithPoolOnSeason(int season, int noModifierWeight, params ModifierOptionData[] pool)
        {
            var seasons = CreateSeasons();
            var data = seasons[season - 1];
            seasons[season - 1] = new SeasonData(data.OpponentId, data.ForgePool, modifierPool: pool,
                noModifierWeight: noModifierWeight);
            return ContentFactory.Create(battle: CreateBattle(seasons));
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
