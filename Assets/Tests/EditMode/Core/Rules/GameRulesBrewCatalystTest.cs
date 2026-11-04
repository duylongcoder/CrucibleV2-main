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
    /// <summary>Xúc tác trong lượt Nung qua <see cref="GameRules.Apply"/> (catalyst R3–R7, R9; brewing AC-1).</summary>
    public class GameRulesBrewCatalystTest
    {
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _meta = new MetaState();
        }

        [Test]
        public void Apply_BrewWithQualityCatalysts_ComputesExampleQuality()
        {
            // brewing AC-1: Essence 18, +3 Potency, ×150 → 18 × 4 = 72 → 108.
            var content = CreateWithEffects(
                bellows: new[] { Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddPotency(3)) },
                mortar: new[] { Effect(EffectTrigger.OnRecipeMatch, EffectActionData.MulPotency(150)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows, Mortar }, new[] { Iron, Coal });

            Brew(run, content);

            Assert.That(Result(run).Quality, Is.EqualTo(108));
        }

        [Test]
        public void Apply_BrewWithAddEssence_AddsToBaseEssence()
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddEssence(5)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });

            Brew(run, content);

            Assert.That(Result(run).Quality, Is.EqualTo(SteelEssence + 5));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Apply_BrewWithReorderedQualityCatalysts_SameQuality(bool bellowsFirst)
        {
            var bag = bellowsFirst ? new[] { Bellows, Mortar } : new[] { Mortar, Bellows };
            // (18 + 1) × (1 + 3) = 76 → ×130 = 98 → ×150 = 147 (sắp tăng dần; áp 150 trước sẽ ra 148).
            var content = CreateWithEffects(
                bellows: new[]
                {
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddPotency(3)),
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.MulPotency(150)),
                },
                mortar: new[]
                {
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.MulPotency(130)),
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddEssence(1)),
                });
            var run = StateFactory.CreateWorkbenchRun(content, bag, new[] { Iron, Coal });

            Brew(run, content);

            Assert.That(Result(run).Quality, Is.EqualTo(147));
        }

        [Test]
        public void Apply_BrewMulPotencyAboveCap_ClampsToQualityCap()
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnRecipeMatch, EffectActionData.MulPotency(500)) },
                qualityCap: 50);
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });

            Brew(run, content);

            Assert.That(Result(run).Quality, Is.EqualTo(50));
        }

        [Test]
        public void Apply_BrewScrapWithQualityCatalysts_ScrapUnchanged()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddEssence(5)),
                Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddStat(StatKind.Sharpness, 5)),
            });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Copper });

            Brew(run, content);

            Assert.That(Result(run).ItemId, Is.EqualTo(IdOf(content, Scrap)));
            Assert.That(Result(run).Quality, Is.EqualTo(0));
            Assert.That(Result(run).Lines, Is.Empty);
        }

        [Test]
        public void Apply_BrewAddStat_ProductHasBoostedHiddenLine()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddStat(StatKind.Sharpness, 7)),
            });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });

            Brew(run, content);

            Assert.That(Result(run).Lines, Is.EqualTo(new[] { new StatLine(StatKind.Sharpness, 7) }));
            Assert.That(Result(run).RevealedMask, Is.EqualTo(0));
        }

        [TestCase(EffectTrigger.OnBrewStart)]
        [TestCase(EffectTrigger.OnRecipeMatch)]
        [TestCase(EffectTrigger.OnBrewEnd)]
        public void Apply_BrewMatchingGainGold_AddsGold(EffectTrigger trigger)
        {
            var content = CreateWithEffects(bellows: new[] { Effect(trigger, EffectActionData.GainGold(5)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });

            Brew(run, content);

            Assert.That(run.Gold, Is.EqualTo(5));
        }

        [TestCase(new[] { Iron, Copper }, 5)]
        [TestCase(new[] { Iron, Coal }, 0)]
        public void Apply_BrewFailGainGold_OnlyOnScrap(string[] grid, int expectedGold)
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnBrewFail, EffectActionData.GainGold(5)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, grid);

            Brew(run, content);

            Assert.That(run.Gold, Is.EqualTo(expectedGold));
        }

        [Test]
        public void Apply_BrewCreateIngredient_AddsItemAfterResult()
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnBrewEnd, EffectActionData.CreateIngredient(Coal)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });
            var resultUid = run.NextUid;

            Brew(run, content);

            Assert.That(run.Bag.Select(item => item.ItemId),
                Is.EqualTo(new[] { IdOf(content, Bellows), IdOf(content, Steel), IdOf(content, Coal) }));
            Assert.That(run.Bag[2].Uid, Is.EqualTo(resultUid + 1));
            Assert.That(run.NextUid, Is.EqualTo(resultUid + 2));
            Assert.That(run.LastBrewResultUid, Is.EqualTo(resultUid));
        }

        [Test]
        public void Apply_BrewCreateIngredientBagFull_BlockedAndLogged()
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnBrewEnd, EffectActionData.CreateIngredient(Coal)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows, Iron }, new[] { Iron });
            run.BagCapacity = 3;

            Brew(run, content);

            Assert.That(run.Bag.Count, Is.EqualTo(3));
            Assert.That(BagRules.UsedSlots(run), Is.LessThanOrEqualTo(run.BagCapacity));
            Assert.That(run.LastBrew.Activations.Single().Blocked, Is.True);
        }

        [Test]
        public void Apply_BrewTwoCreateIngredientOneRoom_FirstInBagOrderWins()
        {
            var content = CreateWithEffects(
                bellows: new[] { Effect(EffectTrigger.OnBrewEnd, EffectActionData.CreateIngredient(Coal)) },
                mortar: new[] { Effect(EffectTrigger.OnBrewEnd, EffectActionData.CreateIngredient(Copper)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Mortar, Bellows }, new[] { Iron });
            run.BagCapacity = 4;

            Brew(run, content);

            Assert.That(run.Bag.Last().ItemId, Is.EqualTo(IdOf(content, Copper)));
            Assert.That(run.LastBrew.Activations.Select(a => (a.CatalystUid, a.Blocked)),
                Is.EqualTo(new[] { (1, false), (2, true) }));
        }

        [Test]
        public void Apply_BrewWithCatalysts_LogsActivationsAndEventsInOrder()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddPotency(1)),
                Effect(EffectTrigger.OnBrewStart, EffectActionData.GainGold(2)),
            });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });

            var events = Brew(run, content).Events;

            Assert.That(events[0], Is.InstanceOf<BrewCompleted>());
            var activated = events.Skip(1).Cast<CatalystActivated>()
                .Select(e => (e.CatalystUid, e.EffectIndex, e.Trigger, e.Blocked)).ToArray();
            var logged = run.LastBrew.Activations.Select(a => (a.CatalystUid, a.EffectIndex, a.Trigger, a.Blocked)).ToArray();
            Assert.That(activated, Is.EqualTo(new[]
            {
                (1, 1, EffectTrigger.OnBrewStart, false), (1, 0, EffectTrigger.OnRecipeMatch, false),
            }));
            Assert.That(logged, Is.EqualTo(activated));
        }

        [Test]
        public void Apply_BrewChanceOrder_CatalystDrawsAfterMagicAndLines()
        {
            var content = CreateWithEffects(
                bellows: new[] { Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(5), EffectConditionData.Chance(50)) },
                steelStatLines: new[] { new StatLineData(StatKind.Sharpness, 50, 1, 10) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });
            var copy = run.Rng.Get(RngStream.Chance);
            copy.Roll(0);
            if (copy.Roll(50))
                copy.NextInt(10);
            var expectedGold = copy.Roll(50) ? 5 : 0;

            Brew(run, content);

            Assert.That(run.Rng.Get(RngStream.Chance).State, Is.EqualTo(copy.State));
            Assert.That(run.Gold, Is.EqualTo(expectedGold));
        }

        [Test]
        public void Apply_BrewWithCatalysts_SameStateSameHash()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnBrewEnd, EffectActionData.CreateIngredient(Coal), EffectConditionData.Chance(50)),
                Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddPotency(2)),
            });
            var first = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });
            var second = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });

            Brew(first, content);
            Brew(second, content);

            Assert.That(StateHasher.Hash(second, _meta), Is.EqualTo(StateHasher.Hash(first, _meta)));
        }

        [Test]
        public void Apply_BrewWithEffectlessCatalyst_ResultUnchanged()
        {
            var content = Create();
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows }, new[] { Iron, Coal });
            var copy = run.Rng.Get(RngStream.Chance);
            copy.NextUInt();

            var events = Brew(run, content).Events;

            Assert.That(Result(run).Quality, Is.EqualTo(SteelEssence));
            Assert.That(run.Rng.Get(RngStream.Chance).State, Is.EqualTo(copy.State));
            Assert.That(run.LastBrew.Activations, Is.Empty);
            Assert.That(events.Count, Is.EqualTo(1));
        }

        private CommandResult Brew(RunState run, ContentDb content)
        {
            var result = GameRules.Apply(run, _meta, content, new Brew(1793750400));
            Assert.That(result.Accepted, Is.True, result.Reason.ToString());
            return result;
        }

        private static ItemInstance Result(RunState run) => run.Bag.Single(item => item.Uid == run.LastBrewResultUid);
    }
}
