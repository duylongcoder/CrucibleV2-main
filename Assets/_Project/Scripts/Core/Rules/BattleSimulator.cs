using System;
using System.Collections.Generic;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Mô phỏng Trận tất định, không RNG (battle.md §Luật cốt lõi). Mỗi hiệp ta đánh trước; đối thủ chỉ đánh khi cả hai
    /// còn đứng (forge-battle D5). Cả hai gục cùng hiệp → ta thắng. Hết <c>MaxRounds</c> → tổng sát thương, bằng → ta thắng.
    /// </summary>
    public static class BattleSimulator
    {
        private const int Percent = 100;

        public static BattleLog Simulate(int season, EquipmentId equipment, ItemInstance offered, Combatant hero,
                                         Combatant opponent, BattleData data)
        {
            if (offered == null) throw new ArgumentNullException(nameof(offered));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var rounds = new List<BattleRound>(data.MaxRounds);
            var heroHp = hero.Hp;
            var opponentHp = opponent.Hp;
            var heroDealt = 0L;
            var opponentDealt = 0L;
            var knockout = false;

            while (rounds.Count < data.MaxRounds)
            {
                // 1. Ta đánh; dội ngược từ Phản đòn đối thủ.
                var heroDamage = Hit(hero, opponent, data);
                var reflectToHero = Reflect(heroDamage, opponent, data);
                opponentHp -= heroDamage;
                heroHp -= reflectToHero;
                heroDealt += heroDamage;
                opponentDealt += reflectToHero;

                // 2. Đối thủ chỉ đánh khi cả hai còn đứng (D5).
                var opponentAttacked = heroHp > 0 && opponentHp > 0;
                var opponentDamage = 0L;
                var reflectToOpponent = 0L;
                if (opponentAttacked)
                {
                    opponentDamage = Hit(opponent, hero, data);
                    reflectToOpponent = Reflect(opponentDamage, hero, data);
                    heroHp -= opponentDamage;
                    opponentHp -= reflectToOpponent;
                    opponentDealt += opponentDamage;
                    heroDealt += reflectToOpponent;
                }

                rounds.Add(new BattleRound(heroDamage, reflectToHero, opponentAttacked, opponentDamage, reflectToOpponent,
                    Math.Max(0, heroHp), Math.Max(0, opponentHp)));

                if (heroHp <= 0 || opponentHp <= 0)
                {
                    knockout = true;
                    break;
                }
            }

            // Gục: đối thủ gục (kể cả cùng gục) → ta thắng. Hết hiệp: tổng sát thương, bằng → ta thắng.
            var heroWon = knockout ? opponentHp <= 0 : heroDealt >= opponentDealt;
            return new BattleLog(season, equipment, true, offered.ItemId, offered.Magic, hero, opponent, rounds.ToArray(),
                heroDealt, opponentDealt, knockout ? BattleEndReason.Knockout : BattleEndReason.RoundLimit, heroWon);
        }

        /// <summary><c>max(1, ⌊⌊ATK × (100 + SB) / 100⌋ × (100 − min(HTCap, HT)) / 100⌋)</c>.</summary>
        private static long Hit(Combatant attacker, Combatant defender, BattleData data)
        {
            var baseHit = BattleMath.MulDivFloor(attacker.Atk, Percent + attacker.Sharpness, Percent);
            var absorb = Math.Min(data.AbsorbCapPercent, defender.Absorb);
            return Math.Max(1, BattleMath.MulDivFloor(baseHit, Percent - absorb, Percent));
        }

        /// <summary>Dội ngược <c>⌊nhận × min(PDCap, PD) / 100⌋</c> — không bị Hấp thụ, không dội tiếp.</summary>
        private static long Reflect(long damage, Combatant defender, BattleData data)
        {
            return BattleMath.MulDivFloor(damage, Math.Min(data.ReflectCapPercent, defender.Reflect), Percent);
        }
    }
}
