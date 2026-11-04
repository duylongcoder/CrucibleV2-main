using System.Linq;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using NUnit.Framework;

namespace Crucible.Tests.Core.Logs
{
    /// <summary><see cref="BattleLog"/> (forge-battle R8; ADR-007).</summary>
    public class BattleLogTest
    {
        [Test]
        public void Forfeit_Always_IsLossWithNoRounds()
        {
            var log = BattleLog.Forfeit(2, new EquipmentId(1), new Combatant(100, 10, 0, 0, 0),
                new Combatant(80, 9, 0, 0, 0));

            Assert.That(log.HeroWon, Is.False);
            Assert.That(log.EndReason, Is.EqualTo(BattleEndReason.Forfeit));
            Assert.That(log.Rounds, Is.Empty);
            Assert.That(log.HasOfferedItem, Is.False);
        }

        [Test]
        public void Forfeit_Always_KeepsSeasonAndEquipment()
        {
            var log = BattleLog.Forfeit(2, new EquipmentId(1), default, default);

            Assert.That(log.Season, Is.EqualTo(2));
            Assert.That(log.Equipment, Is.EqualTo(new EquipmentId(1)));
        }

        [Test]
        public void BattleLog_HasNoQualityOrHiddenLineMembers()
        {
            var names = typeof(BattleLog).GetProperties().Select(p => p.Name)
                .Concat(typeof(BattleRound).GetProperties().Select(p => p.Name))
                .Concat(typeof(Combatant).GetProperties().Select(p => p.Name));

            Assert.That(names, Has.None.Contains("Quality").And.None.Contains("Line").And.None.Contains("Recipe"));
        }
    }
}
