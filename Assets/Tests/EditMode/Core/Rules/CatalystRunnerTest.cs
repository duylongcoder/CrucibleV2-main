using System.Collections.Generic;
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
    /// <summary><see cref="CatalystRunner"/> gọi trực tiếp (catalyst R3, R4, R7, R8).</summary>
    public class CatalystRunnerTest
    {
        private const int BellowsUid = 1;
        private const int MortarUid = 2;

        [Test]
        public void EvaluateBrew_NoCatalysts_ReturnsNeutralAndDrawsNothing()
        {
            var content = Create();
            var rng = NewRng();
            var before = rng.State;

            var outcome = Evaluate(content, new[] { Item(content, 1, Iron) }, new[] { Iron, Coal }, ref rng);

            Assert.That(outcome.ActivationCount, Is.EqualTo(0));
            Assert.That(outcome.EssenceBonus, Is.EqualTo(0));
            Assert.That(outcome.PotencyBonus, Is.EqualTo(0));
            Assert.That(outcome.MulPercents, Is.Empty);
            Assert.That(rng.State, Is.EqualTo(before));
        }

        [TestCase(new[] { Iron, Coal }, new[] { EffectTrigger.OnBrewStart, EffectTrigger.OnRecipeMatch, EffectTrigger.OnBrewEnd })]
        [TestCase(new[] { Iron, Copper }, new[] { EffectTrigger.OnBrewStart, EffectTrigger.OnBrewFail, EffectTrigger.OnBrewEnd })]
        public void EvaluateBrew_MatchOrScrap_FiresOnlyItsPhases(string[] grid, EffectTrigger[] expected)
        {
            var content = CreateWithEffects(
                bellows: new[] { Gold(EffectTrigger.OnBrewStart), Gold(EffectTrigger.OnRecipeMatch), Gold(EffectTrigger.OnBrewFail) },
                mortar: new[] { Gold(EffectTrigger.OnBrewEnd) });
            var rng = NewRng();

            var outcome = Evaluate(content, Catalysts(content, Bellows, Mortar), grid, ref rng);

            Assert.That(Triggers(outcome), Is.EqualTo(expected));
        }

        [Test]
        public void EvaluateBrew_TwoCatalysts_OrdersByPhaseThenBagThenDeclaration()
        {
            var content = CreateWithEffects(
                bellows: new[] { Gold(EffectTrigger.OnBrewEnd), Gold(EffectTrigger.OnBrewStart) },
                mortar: new[] { Gold(EffectTrigger.OnBrewStart) });
            var rng = NewRng();
            var bag = new[] { Item(content, MortarUid, Mortar), Item(content, BellowsUid, Bellows) };

            var outcome = Evaluate(content, bag, new[] { Iron, Coal }, ref rng);

            Assert.That(Activations(outcome), Is.EqualTo(new[] { (MortarUid, 0), (BellowsUid, 1), (BellowsUid, 0) }));
        }

        [TestCase(new[] { Iron, Coal }, 1)]
        [TestCase(new[] { Iron, Copper }, 0)]
        public void EvaluateBrew_IngredientIs_ActivatesOnlyWhenOnGrid(string[] grid, int expectedActivations)
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), EffectConditionData.IngredientIs(Coal)),
            });
            var rng = NewRng();

            var outcome = Evaluate(content, Catalysts(content, Bellows), grid, ref rng);

            Assert.That(outcome.ActivationCount, Is.EqualTo(expectedActivations));
        }

        [TestCase(Steel, 1)]
        [TestCase(CastIron, 0)]
        public void EvaluateBrew_RecipeIs_ActivatesOnlyForThatProduct(string product, int expectedActivations)
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnRecipeMatch, EffectActionData.GainGold(1), EffectConditionData.RecipeIs(product)),
            });
            var rng = NewRng();

            var outcome = Evaluate(content, Catalysts(content, Bellows), new[] { Iron, Coal }, ref rng);

            Assert.That(outcome.ActivationCount, Is.EqualTo(expectedActivations));
        }

        [Test]
        public void EvaluateBrew_RecipeIsOnBrewEndWithScrap_DoesNotActivate()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), EffectConditionData.RecipeIs(Steel)),
            });
            var rng = NewRng();

            var outcome = Evaluate(content, Catalysts(content, Bellows), new[] { Iron, Copper }, ref rng);

            Assert.That(outcome.ActivationCount, Is.EqualTo(0));
        }

        [TestCase(new[] { Iron, Ash, Coal }, 0)]
        [TestCase(new[] { Iron, Ash, Bone, Coal }, 1)]
        [TestCase(new[] { Iron, Ash, Bone, Sand, Coal }, 1)]
        public void EvaluateBrew_AdditiveCount_ActivatesAtOrAboveThreshold(string[] grid, int expectedActivations)
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), EffectConditionData.AdditiveCount(2)),
            });
            var rng = NewRng();

            var outcome = Evaluate(content, Catalysts(content, Bellows), grid, ref rng);

            Assert.That(outcome.ActivationCount, Is.EqualTo(expectedActivations));
        }

        [Test]
        public void EvaluateBrew_ChanceConditionWithFailingOtherCondition_StillDrawsOnce()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), EffectConditionData.Chance(50),
                    EffectConditionData.IngredientIs(Copper)),
            });
            var rng = NewRng();
            var expected = rng;
            expected.NextUInt();

            var outcome = Evaluate(content, Catalysts(content, Bellows), new[] { Iron, Coal }, ref rng);

            Assert.That(outcome.ActivationCount, Is.EqualTo(0));
            Assert.That(rng.State, Is.EqualTo(expected.State));
        }

        [Test]
        public void EvaluateBrew_ChanceOutcome_MatchesSameSeedRoll()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), EffectConditionData.Chance(50)),
            });
            var seen = new HashSet<bool>();

            for (ulong seed = 0; seed < 40; seed++)
            {
                var rng = RngStreams.Create(seed).Get(RngStream.Chance);
                var copy = rng;
                var expected = copy.Roll(50);

                var outcome = Evaluate(content, Catalysts(content, Bellows), new[] { Iron, Coal }, ref rng);

                Assert.That(outcome.ActivationCount == 1, Is.EqualTo(expected), $"seed {seed}");
                seen.Add(expected);
            }

            Assert.That(seen, Is.EquivalentTo(new[] { true, false }));
        }

        [Test]
        public void EvaluateBrew_ConditionsAreAnded_OneFalseBlocksActivation()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), EffectConditionData.IngredientIs(Iron),
                    EffectConditionData.AdditiveCount(1)),
            });
            var rng = NewRng();

            var outcome = Evaluate(content, Catalysts(content, Bellows), new[] { Iron, Coal }, ref rng);

            Assert.That(outcome.ActivationCount, Is.EqualTo(0));
        }

        [Test]
        public void EvaluateBrew_QualityActions_SummedAndCollected()
        {
            var content = CreateWithEffects(
                bellows: new[]
                {
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddEssence(2)),
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddPotency(3)),
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.MulPotency(150)),
                },
                mortar: new[]
                {
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddPotency(1)),
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.MulPotency(120)),
                    Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddStat(StatKind.Sharpness, 5)),
                });
            var rng = NewRng();

            var outcome = Evaluate(content, Catalysts(content, Bellows, Mortar), new[] { Iron, Coal }, ref rng);

            Assert.That(outcome.EssenceBonus, Is.EqualTo(2));
            Assert.That(outcome.PotencyBonus, Is.EqualTo(4));
            Assert.That(outcome.MulPercents, Is.EqualTo(new[] { 150, 120 }));
            Assert.That(outcome.StatBonuses, Is.EqualTo(new[] { new StatLine(StatKind.Sharpness, 5) }));
        }

        [Test]
        public void EvaluateBrew_NonCatalystBagItems_Ignored()
        {
            var content = CreateWithEffects(bellows: new[] { Gold(EffectTrigger.OnBrewEnd) });
            var rng = NewRng();
            var bag = new[] { Item(content, 5, Iron), Item(content, BellowsUid, Bellows), Item(content, 6, Lens) };

            var outcome = Evaluate(content, bag, new[] { Iron, Coal }, ref rng);

            Assert.That(Activations(outcome), Is.EqualTo(new[] { (BellowsUid, 0) }));
        }

        [Test]
        public void RunShopRefresh_GainGold_AddsGoldAndEvent()
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnShopRefresh, EffectActionData.GainGold(7)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows });
            var events = new List<GameEvent>();

            CatalystRunner.RunShopRefresh(run, content, events);

            Assert.That(run.Gold, Is.EqualTo(7));
            var activated = events.OfType<CatalystActivated>().Single();
            Assert.That(activated.CatalystUid, Is.EqualTo(1));
            Assert.That(activated.Trigger, Is.EqualTo(EffectTrigger.OnShopRefresh));
            Assert.That(activated.Blocked, Is.False);
        }

        [Test]
        public void RunShopRefresh_ChanceDrawsFromShopStream()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnShopRefresh, EffectActionData.GainGold(7), EffectConditionData.Chance(50)),
            });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows });
            var shop = run.Rng.Get(RngStream.Shop);
            var expectedGold = shop.Roll(50) ? 7 : 0;
            var chanceBefore = run.Rng.Get(RngStream.Chance).State;

            CatalystRunner.RunShopRefresh(run, content, new List<GameEvent>());

            Assert.That(run.Rng.Get(RngStream.Shop).State, Is.EqualTo(shop.State));
            Assert.That(run.Rng.Get(RngStream.Chance).State, Is.EqualTo(chanceBefore));
            Assert.That(run.Gold, Is.EqualTo(expectedGold));
        }

        [Test]
        public void RunShopRefresh_CreateIngredient_AddsItemToBagEnd()
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnShopRefresh, EffectActionData.CreateIngredient(Coal)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows });
            var uid = run.NextUid;

            CatalystRunner.RunShopRefresh(run, content, new List<GameEvent>());

            Assert.That(run.Bag.Count, Is.EqualTo(2));
            Assert.That(run.Bag[1].ItemId, Is.EqualTo(IdOf(content, Coal)));
            Assert.That(run.Bag[1].Uid, Is.EqualTo(uid));
            Assert.That(run.NextUid, Is.EqualTo(uid + 1));
        }

        [Test]
        public void RunShopRefresh_CreateIngredientNoRoom_Blocked()
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnShopRefresh, EffectActionData.CreateIngredient(Coal)) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows, Iron });
            run.BagCapacity = 2;
            var uid = run.NextUid;
            var events = new List<GameEvent>();

            CatalystRunner.RunShopRefresh(run, content, events);

            Assert.That(run.Bag.Count, Is.EqualTo(2));
            Assert.That(run.NextUid, Is.EqualTo(uid));
            Assert.That(events.OfType<CatalystActivated>().Single().Blocked, Is.True);
        }

        [Test]
        public void RunShopRefresh_BrewTriggerEffects_Ignored()
        {
            var content = CreateWithEffects(bellows: new[] { Gold(EffectTrigger.OnBrewEnd) });
            var run = StateFactory.CreateWorkbenchRun(content, new[] { Bellows });
            var before = run.Rng.Capture();
            var events = new List<GameEvent>();

            CatalystRunner.RunShopRefresh(run, content, events);

            Assert.That(run.Gold, Is.EqualTo(0));
            Assert.That(events, Is.Empty);
            Assert.That(run.Rng.Capture(), Is.EqualTo(before));
        }

        private static EffectData Gold(EffectTrigger trigger) => Effect(trigger, EffectActionData.GainGold(1));

        private static Pcg32 NewRng() => RngStreams.Create(StateFactory.Seed).Get(RngStream.Chance);

        private static ItemInstance Item(ContentDb content, int uid, string id) => new ItemInstance(uid, IdOf(content, id));

        /// <summary>Uid theo thứ tự: 1, 2, …</summary>
        private static ItemInstance[] Catalysts(ContentDb content, params string[] ids) =>
            ids.Select((id, i) => Item(content, i + 1, id)).ToArray();

        private static CatalystBrewOutcome Evaluate(ContentDb content, ItemInstance[] bag, string[] gridIds, ref Pcg32 rng)
        {
            var grid = new ItemInstance[GridRules.CellCount];
            for (var cell = 0; cell < gridIds.Length; cell++)
                grid[cell] = Item(content, 100 + cell, gridIds[cell]);
            var recipe = RecipeMatcher.Match(grid, content);
            return CatalystRunner.EvaluateBrew(bag, grid, recipe, content, ref rng);
        }

        private static EffectTrigger[] Triggers(CatalystBrewOutcome outcome) =>
            Enumerable.Range(0, outcome.ActivationCount).Select(i => outcome.EffectAt(i).Trigger).ToArray();

        private static (int, int)[] Activations(CatalystBrewOutcome outcome) =>
            Enumerable.Range(0, outcome.ActivationCount)
                .Select(i => (outcome.CatalystUidAt(i), outcome.EffectIndexAt(i))).ToArray();
    }
}
