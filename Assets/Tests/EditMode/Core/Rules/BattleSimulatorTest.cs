using System.Linq;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><see cref="BattleSimulator"/> (forge-battle R7, R8; battle.md AC-1/2/4/5/7).</summary>
    public class BattleSimulatorTest
    {
        private static readonly ItemInstance Offered = new ItemInstance(7, new ItemId(3), magic: true, quality: 40);

        private BattleData _battle;

        [SetUp]
        public void SetUp()
        {
            _battle = ContentFactory.CreateBattle();
        }

        [Test]
        public void Simulate_FormulaExample_HeroWinsInRoundFive()
        {
            var log = Run(new Combatant(108, 30, 20, 0, 0), new Combatant(150, 20, 0, 0, 10));

            Assert.That(log.HeroWon, Is.True);
            Assert.That(log.EndReason, Is.EqualTo(BattleEndReason.Knockout));
            Assert.That(log.Rounds.Count, Is.EqualTo(5));
            Assert.That(log.Rounds.Select(r => r.HeroDamage), Is.All.EqualTo(32));
            Assert.That(log.Rounds.Take(4).Select(r => r.OpponentDamage), Is.All.EqualTo(20));
        }

        [Test]
        public void Simulate_FormulaExample_RecordsHpAfterEachRound()
        {
            var log = Run(new Combatant(108, 30, 20, 0, 0), new Combatant(150, 20, 0, 0, 10));

            Assert.That(log.Rounds[3].HeroHpAfter, Is.EqualTo(28));
            Assert.That(log.Rounds[3].OpponentHpAfter, Is.EqualTo(22));
        }

        [Test]
        public void Simulate_HeroKnocksOutInRoundOne_OpponentDoesNotAttack()
        {
            var log = Run(new Combatant(100, 100, 0, 0, 0), new Combatant(50, 30, 0, 0, 0));

            Assert.That(log.Rounds.Count, Is.EqualTo(1));
            Assert.That(log.Rounds[0].OpponentAttacked, Is.False);
            Assert.That(log.HeroWon, Is.True);
        }

        [Test]
        public void Simulate_OpponentKnocksOutHero_HeroLoses()
        {
            var log = Run(new Combatant(10, 1, 0, 0, 0), new Combatant(1000, 50, 0, 0, 0));

            Assert.That(log.HeroWon, Is.False);
            Assert.That(log.EndReason, Is.EqualTo(BattleEndReason.Knockout));
            Assert.That(log.Rounds.Count, Is.EqualTo(1));
        }

        [Test]
        public void Simulate_BothFallFromReflectInOneRound_HeroWins()
        {
            var log = Run(new Combatant(10, 100, 0, 0, 0), new Combatant(50, 0, 0, 50, 0));

            Assert.That(log.Rounds[0].HeroHpAfter, Is.EqualTo(0));
            Assert.That(log.Rounds[0].OpponentHpAfter, Is.EqualTo(0));
            Assert.That(log.HeroWon, Is.True);
        }

        [Test]
        public void Simulate_HeroFallsFromReflectOpponentStands_HeroLosesWithoutCounter()
        {
            var log = Run(new Combatant(10, 40, 0, 0, 0), new Combatant(1000, 50, 0, 50, 0));

            Assert.That(log.HeroWon, Is.False);
            Assert.That(log.Rounds.Count, Is.EqualTo(1));
            Assert.That(log.Rounds[0].OpponentAttacked, Is.False);
        }

        [Test]
        public void Simulate_RoundLimitHeroDealtMore_HeroWins()
        {
            var log = Run(new Combatant(1000, 10, 0, 0, 0), new Combatant(1000, 5, 0, 0, 0));

            Assert.That(log.EndReason, Is.EqualTo(BattleEndReason.RoundLimit));
            Assert.That(log.Rounds.Count, Is.EqualTo(MaxRounds));
            Assert.That(log.HeroWon, Is.True);
        }

        [Test]
        public void Simulate_RoundLimitOpponentDealtMore_HeroLoses()
        {
            var log = Run(new Combatant(1000, 5, 0, 0, 0), new Combatant(1000, 10, 0, 0, 0));

            Assert.That(log.EndReason, Is.EqualTo(BattleEndReason.RoundLimit));
            Assert.That(log.HeroWon, Is.False);
        }

        [Test]
        public void Simulate_RoundLimitEqualTotals_HeroWins()
        {
            var log = Run(new Combatant(1000, 10, 0, 0, 0), new Combatant(1000, 10, 0, 0, 0));

            Assert.That(log.HeroTotalDealt, Is.EqualTo(log.OpponentTotalDealt));
            Assert.That(log.HeroWon, Is.True);
        }

        [Test]
        public void Simulate_TotalsIncludeReflectDamage()
        {
            var battle = ContentFactory.CreateBattle(maxRounds: 1);

            var log = BattleSimulator.Simulate(1, default, Offered, new Combatant(1000, 10, 0, 20, 0),
                new Combatant(1000, 10, 0, 0, 0), battle);

            Assert.That(log.HeroTotalDealt, Is.EqualTo(12));
            Assert.That(log.OpponentTotalDealt, Is.EqualTo(10));
        }

        [Test]
        public void Simulate_AbsorbAboveCap_ClampedToCap()
        {
            var log = Run(new Combatant(1000, 100, 0, 0, 0), new Combatant(1000, 0, 0, 0, 90));

            Assert.That(log.Rounds[0].HeroDamage, Is.EqualTo(25));
        }

        [Test]
        public void Simulate_ReflectAboveCap_ClampedToCap()
        {
            var log = Run(new Combatant(1000, 100, 0, 0, 0), new Combatant(1000, 0, 0, 80, 0));

            Assert.That(log.Rounds[0].ReflectToHero, Is.EqualTo(50));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Simulate_DamageWouldRoundToZero_DealsOne(long atk)
        {
            var log = Run(new Combatant(1000, atk, 0, 0, 0), new Combatant(1000, 0, 0, 0, 75));

            Assert.That(log.Rounds.Select(r => r.HeroDamage), Is.All.EqualTo(1));
        }

        [Test]
        public void Simulate_SharpnessIncreasesBaseHit()
        {
            var log = Run(new Combatant(1000, 30, 20, 0, 0), new Combatant(1000, 0, 0, 0, 0));

            Assert.That(log.Rounds[0].HeroDamage, Is.EqualTo(36));
        }

        [Test]
        public void Simulate_ReflectDamageIsNotAbsorbedNorReflected()
        {
            var log = Run(new Combatant(1000, 100, 0, 50, 50), new Combatant(1000, 0, 0, 50, 0));

            Assert.That(log.Rounds[0].ReflectToHero, Is.EqualTo(50));
            Assert.That(log.Rounds[0].HeroHpAfter, Is.EqualTo(1000 - 50 - 1));
        }

        [Test]
        public void Simulate_HpAfterIsNeverNegative()
        {
            var log = Run(new Combatant(5, 500, 0, 0, 0), new Combatant(10, 0, 0, 50, 0));

            Assert.That(log.Rounds.Select(r => r.HeroHpAfter), Is.All.GreaterThanOrEqualTo(0));
            Assert.That(log.Rounds.Select(r => r.OpponentHpAfter), Is.All.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Simulate_Always_RecordsInputsInLog()
        {
            var hero = new Combatant(108, 30, 20, 0, 0);
            var opponent = new Combatant(150, 20, 0, 0, 10);

            var log = BattleSimulator.Simulate(3, new EquipmentId(2), Offered, hero, opponent, _battle);

            Assert.That(log.Season, Is.EqualTo(3));
            Assert.That(log.Equipment, Is.EqualTo(new EquipmentId(2)));
            Assert.That(log.HasOfferedItem, Is.True);
            Assert.That(log.OfferedItem, Is.EqualTo(Offered.ItemId));
            Assert.That(log.OfferedMagic, Is.True);
            Assert.That(log.Hero, Is.EqualTo(hero));
            Assert.That(log.Opponent, Is.EqualTo(opponent));
        }

        [Test]
        public void Simulate_SameInputs_ReturnEqualLogs()
        {
            var random = new System.Random(1104);
            for (var i = 0; i < 1000; i++)
            {
                var hero = RandomCombatant(random);
                var opponent = RandomCombatant(random);

                var first = Run(hero, opponent);
                var second = Run(hero, opponent);

                AssertLogsEqual(second, first, $"set {i}");
            }
        }

        internal static void AssertLogsEqual(BattleLog actual, BattleLog expected, string message = null)
        {
            Assert.That(actual, Is.Not.Null, message);
            Assert.That(actual.Rounds, Is.EqualTo(expected.Rounds), message);
            Assert.That(actual.HeroWon, Is.EqualTo(expected.HeroWon), message);
            Assert.That(actual.EndReason, Is.EqualTo(expected.EndReason), message);
            Assert.That(actual.HeroTotalDealt, Is.EqualTo(expected.HeroTotalDealt), message);
            Assert.That(actual.OpponentTotalDealt, Is.EqualTo(expected.OpponentTotalDealt), message);
            Assert.That(actual.Hero, Is.EqualTo(expected.Hero), message);
            Assert.That(actual.Opponent, Is.EqualTo(expected.Opponent), message);
            Assert.That(actual.Equipment, Is.EqualTo(expected.Equipment), message);
            Assert.That(actual.Season, Is.EqualTo(expected.Season), message);
        }

        private static Combatant RandomCombatant(System.Random random) =>
            new Combatant(random.Next(1, 500), random.Next(0, 80), random.Next(0, 100), random.Next(0, 100),
                random.Next(0, 100));

        private BattleLog Run(Combatant hero, Combatant opponent) =>
            BattleSimulator.Simulate(1, default, Offered, hero, opponent, _battle);
    }
}
