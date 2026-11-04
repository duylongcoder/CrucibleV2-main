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
        public void EnterDecree_OnlyAdvancesMarketStream()
        {
            var run = StateFactory.CreateActiveRun();
            var before = run.Rng.Capture();

            SeasonRules.EnterDecree(run, _content);

            MarketRulesShopTest.AssertOnlyStreamAdvanced(before, run.Rng.Capture(), RngStream.Market);
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
