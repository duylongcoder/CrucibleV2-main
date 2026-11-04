using System;
using Crucible.Core.Content;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Content
{
    /// <summary>Modifier Mùa trong <see cref="ContentDb"/> (spoils R9, R12, R14, D6).</summary>
    public class ContentDbModifierTest
    {
        [Test]
        public void Create_ModifierWithoutParams_IsNeutral()
        {
            var content = ContentFactory.Create();

            var neutral = content.GetModifier(ModifierOf(content, ModNeutral));

            Assert.That(neutral.Id, Is.EqualTo(ModNeutral));
            Assert.That(neutral.BuyPercent, Is.EqualTo(100));
            Assert.That(neutral.SellPercent, Is.EqualTo(100));
            Assert.That(neutral.LensPercent, Is.EqualTo(100));
            Assert.That(neutral.RerollPercent, Is.EqualTo(100));
            Assert.That(neutral.OpponentStatPercent, Is.EqualTo(100));
            Assert.That(neutral.FreeRerolls, Is.EqualTo(0));
            Assert.That(neutral.ForgeWeightPercent(EquipmentOf(content, Sword)), Is.EqualTo(100));
        }

        [Test]
        public void Create_ModifierParams_Resolved()
        {
            var content = ContentFactory.Create();

            var dear = content.GetModifier(ModifierOf(content, ModDear));
            var noSword = content.GetModifier(ModifierOf(content, ModNoSword));

            Assert.That(dear.BuyPercent, Is.EqualTo(DearBuyPercent));
            Assert.That(dear.SellPercent, Is.EqualTo(DearSellPercent));
            Assert.That(dear.LensPercent, Is.EqualTo(DearLensPercent));
            Assert.That(dear.RerollPercent, Is.EqualTo(DearRerollPercent));
            Assert.That(dear.FreeRerolls, Is.EqualTo(DearFreeRerolls));
            Assert.That(dear.OpponentStatPercent, Is.EqualTo(100));
            Assert.That(noSword.ForgeWeightPercent(EquipmentOf(content, Sword)), Is.EqualTo(0));
            Assert.That(noSword.ForgeWeightPercent(EquipmentOf(content, Spear)), Is.EqualTo(100));
        }

        [Test]
        public void TryFindModifier_KnownId_ReturnsSortedIndex()
        {
            var content = ContentFactory.Create();

            Assert.That(content.TryFindModifier(ModBrutal, out var brutal), Is.True);
            Assert.That(content.TryFindModifier(ModSwordOnly, out var swordOnly), Is.True);
            Assert.That(brutal.Index, Is.EqualTo(0));
            Assert.That(swordOnly.Index, Is.EqualTo(4));
            Assert.That(content.TryFindModifier("mod_none", out _), Is.False);
        }

        [Test]
        public void Create_SeasonModifierPool_Resolved()
        {
            var content = CreateWithModifier(ModDear);

            var season = content.Season(3);

            Assert.That(season.ModifierPool.Count, Is.EqualTo(1));
            Assert.That(season.ModifierPool[0].Modifier, Is.EqualTo(ModifierOf(content, ModDear)));
            Assert.That(season.NoModifierWeight, Is.EqualTo(0));
            Assert.That(season.ModifierTotalWeight, Is.EqualTo(1));
        }

        [Test]
        public void Create_DuplicateModifierId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                CreateWithModifiers(new ModifierData(ModNeutral), new ModifierData(ModNeutral)));
        }

        [TestCase(ModifierParam.BuyPrice, 0)]
        [TestCase(ModifierParam.BuyPrice, 1001)]
        [TestCase(ModifierParam.RerollPrice, 0)]
        [TestCase(ModifierParam.OpponentStats, 1001)]
        [TestCase(ModifierParam.LensPrice, -5)]
        public void Create_ScaleOutOfRange_Throws(ModifierParam param, int value)
        {
            Assert.Throws<ArgumentException>(() =>
                CreateWithModifiers(new ModifierData("mod_x", new[] { new ModifierParamData(param, value) })));
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void Create_FreeRerollsOutOfRange_Throws(int value)
        {
            Assert.Throws<ArgumentException>(() => CreateWithModifiers(new ModifierData("mod_x",
                new[] { new ModifierParamData(ModifierParam.FreeRerolls, value) })));
        }

        [Test]
        public void Create_DuplicateParam_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateWithModifiers(new ModifierData("mod_x", new[]
            {
                new ModifierParamData(ModifierParam.BuyPrice, 110), new ModifierParamData(ModifierParam.BuyPrice, 120),
            })));
        }

        [Test]
        public void Create_DuplicateForgeWeightForSameEquipment_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateWithModifiers(new ModifierData("mod_x", new[]
            {
                new ModifierParamData(ModifierParam.ForgeWeight, 50, Sword),
                new ModifierParamData(ModifierParam.ForgeWeight, 70, Sword),
            })));
        }

        [Test]
        public void Create_ForgeWeightUnknownEquipment_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateWithModifiers(new ModifierData("mod_x",
                new[] { new ModifierParamData(ModifierParam.ForgeWeight, 50, "eq_none") })));
        }

        [Test]
        public void Create_ForgeWeightWithoutEquipment_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateWithModifiers(new ModifierData("mod_x",
                new[] { new ModifierParamData(ModifierParam.ForgeWeight, 50) })));
        }

        [Test]
        public void Create_NonForgeParamWithEquipment_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateWithModifiers(new ModifierData("mod_x",
                new[] { new ModifierParamData(ModifierParam.BuyPrice, 120, Sword) })));
        }

        [Test]
        public void Create_SellModifierAllowsProfit_Throws()
        {
            // SellRate 75 × 134 = 10 050 ≥ 10 000 → bán lại có thể ≥ giá mua.
            Assert.Throws<ArgumentException>(() => CreateWithModifiers(new ModifierData("mod_x",
                new[] { new ModifierParamData(ModifierParam.SellPrice, 134) })));
        }

        [Test]
        public void Create_SellModifierJustBelowProfit_IsAllowed()
        {
            Assert.DoesNotThrow(() => CreateWithModifiers(new ModifierData("mod_x",
                new[] { new ModifierParamData(ModifierParam.SellPrice, 133) })));
        }

        [Test]
        public void Create_FirstSeasonModifierPool_Throws()
        {
            var seasons = CreateSeasons();
            seasons[0] = new SeasonData(seasons[0].OpponentId, seasons[0].ForgePool,
                modifierPool: new[] { new ModifierOptionData(ModDear, 1) });

            Assert.Throws<ArgumentException>(() => ContentFactory.Create(battle: CreateBattle(seasons)));
        }

        [Test]
        public void Create_SeasonModifierWeightsSumZero_Throws()
        {
            var seasons = CreateSeasons();
            seasons[2] = new SeasonData(seasons[2].OpponentId, seasons[2].ForgePool,
                modifierPool: new[] { new ModifierOptionData(ModDear, 0) }, noModifierWeight: 0);

            Assert.Throws<ArgumentException>(() => ContentFactory.Create(battle: CreateBattle(seasons)));
        }

        [TestCase("unknown")]
        [TestCase("duplicate")]
        [TestCase("negative")]
        [TestCase("negative none")]
        public void Create_InvalidSeasonModifierPool_Throws(string kind)
        {
            var seasons = CreateSeasons();
            var data = seasons[2];
            seasons[2] = kind switch
            {
                "unknown" => new SeasonData(data.OpponentId, data.ForgePool,
                    modifierPool: new[] { new ModifierOptionData("mod_none", 1) }),
                "duplicate" => new SeasonData(data.OpponentId, data.ForgePool,
                    modifierPool: new[] { new ModifierOptionData(ModDear, 1), new ModifierOptionData(ModDear, 1) }),
                "negative" => new SeasonData(data.OpponentId, data.ForgePool,
                    modifierPool: new[] { new ModifierOptionData(ModDear, -1) }),
                _ => new SeasonData(data.OpponentId, data.ForgePool,
                    modifierPool: new[] { new ModifierOptionData(ModDear, 2) }, noModifierWeight: -1),
            };

            Assert.Throws<ArgumentException>(() => ContentFactory.Create(battle: CreateBattle(seasons)));
        }

        [Test]
        public void Create_ModifierZeroesForgePool_Throws()
        {
            var seasons = CreateSingleOptionSeasons(Sword);
            seasons[3] = new SeasonData(seasons[3].OpponentId, seasons[3].ForgePool,
                modifierPool: new[] { new ModifierOptionData(ModNoSword, 1) });

            Assert.Throws<ArgumentException>(() => ContentFactory.Create(battle: CreateBattle(seasons)));
        }

        [Test]
        public void Create_ZeroWeightModifierZeroingForgePool_IsAllowed()
        {
            var seasons = CreateSingleOptionSeasons(Sword);
            seasons[3] = new SeasonData(seasons[3].OpponentId, seasons[3].ForgePool,
                modifierPool: new[] { new ModifierOptionData(ModNoSword, 0) });

            Assert.DoesNotThrow(() => ContentFactory.Create(battle: CreateBattle(seasons)));
        }

        private static ContentDb CreateWithModifiers(params ModifierData[] modifiers) =>
            ContentFactory.Create(modifiers: modifiers);
    }
}
