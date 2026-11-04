using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><see cref="SeasonRules.EnterDecree"/> (run-loop R1).</summary>
    public class SeasonRulesEnterDecreeTest
    {
        private ContentDb _content;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
        }

        [Test]
        public void EnterDecree_FromOffering_SetsDecreeAndTurnZero()
        {
            var run = StateFactory.CreateActiveRun();
            run.Phase = RunPhase.Offering;
            run.Turn = 3;

            SeasonRules.EnterDecree(run, _content);

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Decree));
            Assert.That(run.Turn, Is.EqualTo(0));
        }

        [Test]
        public void EnterDecree_Default_RollsSeasonPriceInRange()
        {
            var sawNonNeutral = false;
            for (ulong seed = 0; seed < 500; seed++)
            {
                var run = CreateRun(seed);
                SeasonRules.EnterDecree(run, _content);

                Assert.That(run.SeasonPricePercent, Is.InRange(SeasonPriceMin, SeasonPriceMax), $"seed {seed}");
                sawNonNeutral |= run.SeasonPricePercent != MarketRules.NeutralPercent;
            }

            Assert.That(sawNonNeutral, Is.True);
        }

        [Test]
        public void EnterDecree_BeforeLastSeason_OnlyAdvancesMarketForgeAndSeasonStreams()
        {
            var run = StateFactory.CreateActiveRun();
            var before = run.Rng.Capture();

            SeasonRules.EnterDecree(run, _content);

            var after = run.Rng.Capture();
            for (var i = 0; i < RngStreams.Count; i++)
            {
                var stream = (RngStream)i;
                if (stream == RngStream.Market || stream == RngStream.Forge || stream == RngStream.Season)
                    Assert.That(after[i], Is.Not.EqualTo(before[i]), $"{stream} should advance");
                else
                    Assert.That(after[i], Is.EqualTo(before[i]), $"{stream} should not advance");
            }
        }

        [Test]
        public void EnterDecree_Always_RollsEquipmentOfCurrentSeason([Values(1, 2, 8)] int season)
        {
            var run = StateFactory.CreateActiveRun();
            run.Season = season;
            var forge = run.Rng.Get(RngStream.Forge);
            var expected = ForgeRulesTest.ExpectedEquipment(_content, season, ref forge);

            SeasonRules.EnterDecree(run, _content);

            Assert.That(run.DecreeEquipment, Is.EqualTo(expected));
        }

        [Test]
        public void EnterDecree_LastSeason_DoesNotDrawSeasonStream()
        {
            var run = StateFactory.CreateActiveRun();
            run.Season = SeasonCount;
            run.NextModifier = new ModifierId(1);
            var before = run.Rng.Get(RngStream.Season).State;

            SeasonRules.EnterDecree(run, _content);

            Assert.That(run.Rng.Get(RngStream.Season).State, Is.EqualTo(before));
            Assert.That(run.NextModifier, Is.Null);
        }

        [Test]
        public void EnterDecree_ShiftsNextModifierToActive()
        {
            var run = StateFactory.CreateActiveRun();
            run.Season = 3;
            run.NextModifier = ModifierOf(_content, ModDear);

            SeasonRules.EnterDecree(run, _content);

            Assert.That(run.ActiveModifier, Is.EqualTo(ModifierOf(_content, ModDear)));
        }

        [Test]
        public void EnterDecree_NoAnnouncedModifier_HasNoActiveModifier()
        {
            var run = StateFactory.CreateActiveRun();
            run.ActiveModifier = ModifierOf(_content, ModDear);

            SeasonRules.EnterDecree(run, _content);

            Assert.That(run.ActiveModifier, Is.Null);
        }

        [Test]
        public void EnterDecree_ModifierSeasons_AnnouncesNextSeasonModifier()
        {
            var content = CreateWithModifier(ModBrutal);
            var run = StateFactory.CreateActiveRun();

            SeasonRules.EnterDecree(run, content);

            Assert.That(run.ActiveModifier, Is.Null);
            Assert.That(run.NextModifier, Is.EqualTo(ModifierOf(content, ModBrutal)));
        }

        [Test]
        public void EnterDecree_DrawsMarketForgeAndSeasonOnceEach()
        {
            var run = StateFactory.CreateActiveRun();
            run.Season = 2;
            var market = run.Rng.Get(RngStream.Market);
            market.NextInt(SeasonPriceMax - SeasonPriceMin + 1);
            var forge = run.Rng.Get(RngStream.Forge);
            ForgeRulesTest.ExpectedEquipment(_content, 2, ref forge);
            var season = run.Rng.Get(RngStream.Season);
            season.NextInt(_content.Season(3).ModifierTotalWeight);

            SeasonRules.EnterDecree(run, _content);

            Assert.That(run.Rng.Get(RngStream.Market).State, Is.EqualTo(market.State));
            Assert.That(run.Rng.Get(RngStream.Forge).State, Is.EqualTo(forge.State));
            Assert.That(run.Rng.Get(RngStream.Season).State, Is.EqualTo(season.State));
        }

        [Test]
        public void EnterDecree_SwordOnlyModifier_AlwaysForgesSword()
        {
            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = CreateRun(seed);
                run.Season = 3;
                run.NextModifier = ModifierOf(_content, ModSwordOnly);

                SeasonRules.EnterDecree(run, _content);

                Assert.That(run.DecreeEquipment, Is.EqualTo(EquipmentOf(_content, Sword)), $"seed {seed}");
            }
        }

        [Test]
        public void EnterDecree_NoSwordModifier_NeverForgesSword()
        {
            for (ulong seed = 0; seed < 200; seed++)
            {
                var run = CreateRun(seed);
                run.Season = 3;
                run.NextModifier = ModifierOf(_content, ModNoSword);

                SeasonRules.EnterDecree(run, _content);

                Assert.That(run.DecreeEquipment, Is.Not.EqualTo(EquipmentOf(_content, Sword)), $"seed {seed}");
            }
        }

        [Test]
        public void EnterDecree_SameSeed_SameSeasonPrice()
        {
            var first = CreateRun(99);
            var second = CreateRun(99);

            SeasonRules.EnterDecree(first, _content);
            SeasonRules.EnterDecree(second, _content);

            Assert.That(second.SeasonPricePercent, Is.EqualTo(first.SeasonPricePercent));
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
