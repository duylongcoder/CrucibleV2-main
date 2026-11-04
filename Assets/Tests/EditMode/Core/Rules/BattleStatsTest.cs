using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><see cref="BattleStats"/> (forge-battle R6; battle.md §Formulas).</summary>
    public class BattleStatsTest
    {
        private static readonly EquipmentData SwordData = new EquipmentData(Sword, 50, 20, new[] { StatKind.Sharpness });

        private BattleData _battle;

        [SetUp]
        public void SetUp()
        {
            _battle = ContentFactory.CreateBattle();
        }

        [Test]
        public void ForHero_SwordQuality40_ReturnsFormulaExample()
        {
            var hero = BattleStats.ForHero(40, 0, 0, 0, SwordData, _battle);

            Assert.That(hero.Atk, Is.EqualTo(30));
            Assert.That(hero.Hp, Is.EqualTo(108));
        }

        [Test]
        public void ForHero_QualityZero_ReturnsBaseStats()
        {
            var hero = BattleStats.ForHero(0, 0, 0, 0, SwordData, _battle);

            Assert.That(hero.Atk, Is.EqualTo(HeroBaseAtk));
            Assert.That(hero.Hp, Is.EqualTo(HeroBaseHp));
        }

        [Test]
        public void ForHero_StatMatchesAffinity_KeepsFullValue()
        {
            var hero = BattleStats.ForHero(40, 20, 0, 0, SwordData, _battle);

            Assert.That(hero.Sharpness, Is.EqualTo(20));
        }

        [TestCase(20, 10)]
        [TestCase(21, 10)]
        public void ForHero_StatOffAffinity_AppliesOffAffinityPercent(int value, int expected)
        {
            var hero = BattleStats.ForHero(40, 0, value, value, SwordData, _battle);

            Assert.That(hero.Reflect, Is.EqualTo(expected));
            Assert.That(hero.Absorb, Is.EqualTo(expected));
        }

        [Test]
        public void ForHero_ZeroStatValues_ReturnsZeroSecondaryStats()
        {
            var hero = BattleStats.ForHero(40, 0, 0, 0, SwordData, _battle);

            Assert.That(new[] { hero.Sharpness, hero.Reflect, hero.Absorb }, Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void ForHero_QualityAtMaxCap_DoesNotOverflow()
        {
            var hero = BattleStats.ForHero(BrewingData.MaxQualityCap, 0, 0, 0, SwordData, _battle);

            Assert.That(hero.Atk, Is.EqualTo(HeroBaseAtk + BrewingData.MaxQualityCap / 2));
        }

        [Test]
        public void ForOpponent_Percent100_ReturnsDefStats()
        {
            var opponent = BattleStats.ForOpponent(new OpponentData("o", 1, BattleKind.War, 101, 7, 3, 5, 9), 100);

            Assert.That(new[] { opponent.Hp, opponent.Atk, opponent.Sharpness, opponent.Reflect, opponent.Absorb },
                Is.EqualTo(new long[] { 101, 7, 3, 5, 9 }));
        }

        [Test]
        public void ForOpponent_Percent150_ScalesAllStatsWithFloor()
        {
            var opponent = BattleStats.ForOpponent(new OpponentData("o", 1, BattleKind.War, 101, 7, 3, 5, 9), 150);

            Assert.That(new[] { opponent.Hp, opponent.Atk, opponent.Sharpness, opponent.Reflect, opponent.Absorb },
                Is.EqualTo(new long[] { 151, 10, 4, 7, 13 }));
        }

        [Test]
        public void ForOpponent_ScaledHpBelowOne_ClampsHpToOne()
        {
            var opponent = BattleStats.ForOpponent(new OpponentData("o", 1, BattleKind.War, 1, 0, 0, 0, 0), 50);

            Assert.That(opponent.Hp, Is.EqualTo(1));
        }
    }
}
