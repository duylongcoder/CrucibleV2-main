using System;
using System.Collections.Generic;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Content
{
    public class ContentDbEffectValidationTest
    {
        private const string Extra = "zz_extra";

        [Test]
        public void Create_CatalystWithValidEffects_ResolvesEffectsInDeclaredOrder()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(5), EffectConditionData.Chance(50)),
                Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddPotency(3), EffectConditionData.RecipeIs(Steel),
                    EffectConditionData.IngredientIs(Iron), EffectConditionData.AdditiveCount(1)),
            });

            var effects = content.EffectsOf(IdOf(content, Bellows));

            Assert.That(effects.Count, Is.EqualTo(2));
            Assert.That(effects[0].Trigger, Is.EqualTo(EffectTrigger.OnBrewEnd));
            Assert.That(effects[0].Action, Is.EqualTo(EffectActionKind.GainGold));
            Assert.That(effects[0].Amount, Is.EqualTo(5));
            Assert.That(effects[0].ChancePercent, Is.EqualTo(50));
            Assert.That(effects[1].HasRecipeCondition, Is.True);
            Assert.That(effects[1].RecipeProduct, Is.EqualTo(IdOf(content, Steel)));
            Assert.That(effects[1].HasIngredientCondition, Is.True);
            Assert.That(effects[1].Ingredient, Is.EqualTo(IdOf(content, Iron)));
            Assert.That(effects[1].MinAdditives, Is.EqualTo(1));
            Assert.That(effects[1].ChancePercent, Is.EqualTo(0));
        }

        [Test]
        public void Create_CreateIngredientEffect_ResolvesActionItem()
        {
            var content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnShopRefresh, EffectActionData.CreateIngredient(Coal)),
            });

            Assert.That(content.EffectsOf(IdOf(content, Bellows))[0].ActionItem, Is.EqualTo(IdOf(content, Coal)));
        }

        [TestCase(ItemKind.Mineral)]
        [TestCase(ItemKind.Additive)]
        [TestCase(ItemKind.Lens)]
        public void Create_NonCatalystWithEffects_Throws(ItemKind kind)
        {
            var item = new ItemData(Extra, kind, 1, revealLines: kind == ItemKind.Lens ? 1 : 0,
                effects: new[] { Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1)) });

            Assert.That(() => Create(extraItems: new[] { item }), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void Create_CatalystWithMoreThanMaxEffects_Throws()
        {
            var effects = new EffectData[EffectLimits.MaxEffectsPerCatalyst + 1];
            for (var i = 0; i < effects.Length; i++)
                effects[i] = Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1));

            AssertInvalid(effects);
        }

        [Test]
        public void Create_EffectWithNullAction_Throws()
        {
            AssertInvalid(new EffectData(EffectTrigger.OnBrewEnd, null));
        }

        [Test]
        public void Create_EffectWithDuplicateConditionKind_Throws()
        {
            AssertInvalid(Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1),
                EffectConditionData.Chance(50), EffectConditionData.Chance(20)));
        }

        [Test]
        public void Create_EffectWithMoreThanMaxConditions_Throws()
        {
            AssertInvalid(Effect(EffectTrigger.OnRecipeMatch, EffectActionData.AddPotency(1),
                EffectConditionData.IngredientIs(Iron), EffectConditionData.RecipeIs(Steel),
                EffectConditionData.AdditiveCount(1), EffectConditionData.Chance(50)));
        }

        [TestCase(EffectActionKind.AddEssence, EffectTrigger.OnBrewStart)]
        [TestCase(EffectActionKind.AddPotency, EffectTrigger.OnBrewFail)]
        [TestCase(EffectActionKind.MulPotency, EffectTrigger.OnBrewEnd)]
        [TestCase(EffectActionKind.AddStat, EffectTrigger.OnShopRefresh)]
        public void Create_QualityActionOutsideOnRecipeMatch_Throws(EffectActionKind action, EffectTrigger trigger)
        {
            var amount = action == EffectActionKind.MulPotency ? 150 : 1;
            AssertInvalid(Effect(trigger, new EffectActionData(action, amount)));
        }

        [TestCase(EffectConditionKind.IngredientIs)]
        [TestCase(EffectConditionKind.RecipeIs)]
        [TestCase(EffectConditionKind.AdditiveCount)]
        public void Create_ShopRefreshWithNonChanceCondition_Throws(EffectConditionKind kind)
        {
            AssertInvalid(Effect(EffectTrigger.OnShopRefresh, EffectActionData.GainGold(1), ValidCondition(kind)));
        }

        [TestCase(EffectTrigger.OnBrewStart)]
        [TestCase(EffectTrigger.OnBrewFail)]
        public void Create_RecipeIsOnStartOrFail_Throws(EffectTrigger trigger)
        {
            AssertInvalid(Effect(trigger, EffectActionData.GainGold(1), EffectConditionData.RecipeIs(Steel)));
        }

        [Test]
        public void Create_RecipeIsTargetWithoutRecipe_Throws()
        {
            AssertInvalid(Effect(EffectTrigger.OnRecipeMatch, EffectActionData.GainGold(1), EffectConditionData.RecipeIs(Iron)));
        }

        [TestCase(Lens)]
        [TestCase(Mortar)]
        [TestCase(Scrap)]
        public void Create_IngredientIsNonIngredient_Throws(string itemId)
        {
            AssertInvalid(Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), EffectConditionData.IngredientIs(itemId)));
        }

        [TestCase(Steel)]
        [TestCase(Lens)]
        [TestCase(Mortar)]
        [TestCase(Scrap)]
        public void Create_CreateIngredientNonBaseItem_Throws(string itemId)
        {
            AssertInvalid(Effect(EffectTrigger.OnBrewEnd, EffectActionData.CreateIngredient(itemId)));
        }

        [Test]
        public void Create_CreateIngredientWithAmount_Throws()
        {
            AssertInvalid(Effect(EffectTrigger.OnBrewEnd, new EffectActionData(EffectActionKind.CreateIngredient, 2, itemId: Coal)));
        }

        [Test]
        public void Create_EffectUnknownItemId_Throws()
        {
            AssertInvalid(Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), EffectConditionData.IngredientIs("nope")));
        }

        private static IEnumerable<TestCaseData> OutOfRangeAmounts()
        {
            yield return new TestCaseData(EffectActionKind.AddEssence, 0);
            yield return new TestCaseData(EffectActionKind.AddEssence, EffectLimits.MaxAddEssence + 1);
            yield return new TestCaseData(EffectActionKind.AddPotency, 0);
            yield return new TestCaseData(EffectActionKind.AddPotency, EffectLimits.MaxAddPotency + 1);
            yield return new TestCaseData(EffectActionKind.MulPotency, EffectLimits.MinMulPotency - 1);
            yield return new TestCaseData(EffectActionKind.MulPotency, EffectLimits.MaxMulPotency + 1);
            yield return new TestCaseData(EffectActionKind.AddStat, 0);
            yield return new TestCaseData(EffectActionKind.AddStat, BattleData.MaxStatValue + 1);
            yield return new TestCaseData(EffectActionKind.GainGold, 0);
            yield return new TestCaseData(EffectActionKind.GainGold, EffectLimits.MaxGainGold + 1);
        }

        private static IEnumerable<TestCaseData> BoundaryAmounts()
        {
            yield return new TestCaseData(EffectActionKind.AddEssence, 1);
            yield return new TestCaseData(EffectActionKind.AddEssence, EffectLimits.MaxAddEssence);
            yield return new TestCaseData(EffectActionKind.AddPotency, 1);
            yield return new TestCaseData(EffectActionKind.AddPotency, EffectLimits.MaxAddPotency);
            yield return new TestCaseData(EffectActionKind.MulPotency, EffectLimits.MinMulPotency);
            yield return new TestCaseData(EffectActionKind.MulPotency, EffectLimits.MaxMulPotency);
            yield return new TestCaseData(EffectActionKind.AddStat, 1);
            yield return new TestCaseData(EffectActionKind.AddStat, BattleData.MaxStatValue);
            yield return new TestCaseData(EffectActionKind.GainGold, 1);
            yield return new TestCaseData(EffectActionKind.GainGold, EffectLimits.MaxGainGold);
        }

        [TestCaseSource(nameof(OutOfRangeAmounts))]
        public void Create_AmountOutOfRange_Throws(EffectActionKind action, int amount)
        {
            AssertInvalid(Effect(EffectTrigger.OnRecipeMatch, new EffectActionData(action, amount)));
        }

        [TestCaseSource(nameof(BoundaryAmounts))]
        public void Create_AmountAtBounds_Succeeds(EffectActionKind action, int amount)
        {
            Assert.That(() => CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnRecipeMatch, new EffectActionData(action, amount)) }),
                Throws.Nothing);
        }

        [TestCase(EffectConditionKind.AdditiveCount, EffectLimits.MinAdditiveCount - 1)]
        [TestCase(EffectConditionKind.AdditiveCount, EffectLimits.MaxAdditiveCount + 1)]
        [TestCase(EffectConditionKind.Chance, EffectLimits.MinChance - 1)]
        [TestCase(EffectConditionKind.Chance, EffectLimits.MaxChance + 1)]
        public void Create_ConditionValueOutOfRange_Throws(EffectConditionKind kind, int value)
        {
            AssertInvalid(Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1), new EffectConditionData(kind, value)));
        }

        [Test]
        public void Create_CatalystWithoutEffects_Succeeds()
        {
            var content = Create();

            Assert.That(content.EffectsOf(IdOf(content, Bellows)), Is.Empty);
        }

        [Test]
        public void EffectsOf_NonCatalyst_ReturnsEmpty()
        {
            var content = CreateWithEffects(bellows: new[] { Effect(EffectTrigger.OnBrewEnd, EffectActionData.GainGold(1)) });

            Assert.That(content.EffectsOf(IdOf(content, Iron)), Is.Empty);
        }

        private static EffectConditionData ValidCondition(EffectConditionKind kind)
        {
            switch (kind)
            {
                case EffectConditionKind.IngredientIs: return EffectConditionData.IngredientIs(Iron);
                case EffectConditionKind.RecipeIs: return EffectConditionData.RecipeIs(Steel);
                case EffectConditionKind.AdditiveCount: return EffectConditionData.AdditiveCount(1);
                default: return EffectConditionData.Chance(50);
            }
        }

        private static void AssertInvalid(params EffectData[] bellowsEffects)
        {
            Assert.That(() => CreateWithEffects(bellows: bellowsEffects), Throws.InstanceOf<ArgumentException>());
        }
    }
}
