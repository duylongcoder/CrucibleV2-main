using Crucible.Core.Content;
using Crucible.Core.Logs;

namespace Crucible.Core.Rules
{
    /// <summary>Quy đổi chỉ số đầu trận (battle.md §Formulas).</summary>
    public static class BattleStats
    {
        private const int Percent = 100;

        /// <summary>
        /// Dũng sĩ ta: <c>ATK = HeroBaseAtk + ⌊Q × AtkPerQ / 100⌋</c>, <c>HP = HeroBaseHp + ⌊Q × HpPerQ / 100⌋</c>; SB/PD/HT =
        /// <c>⌊v × Aff / 100⌋</c> (Aff 100 nếu hợp, ngược lại <c>OffAffinityPercent</c>). F-010 truyền v = 0 (F-008 cấp dòng).
        /// </summary>
        public static Combatant ForHero(long quality, int sharpness, int reflect, int absorb, EquipmentData equipment,
                                        BattleData data)
        {
            return new Combatant(
                data.HeroBaseHp + BattleMath.MulDivFloor(quality, equipment.HpPerQ, Percent),
                data.HeroBaseAtk + BattleMath.MulDivFloor(quality, equipment.AtkPerQ, Percent),
                ApplyAffinity(sharpness, StatKind.Sharpness, equipment, data),
                ApplyAffinity(reflect, StatKind.Reflect, equipment, data),
                ApplyAffinity(absorb, StatKind.Absorb, equipment, data));
        }

        /// <summary>
        /// Đối thủ × <paramref name="statPercent"/> (bậc khó) × <paramref name="modifierPercent"/> (modifier Mùa) / 10000
        /// (floor); HP sau scale ≥ 1 (forge-battle D10).
        /// </summary>
        public static Combatant ForOpponent(OpponentData opponent, int statPercent, int modifierPercent = 100)
        {
            var num = (long)statPercent * modifierPercent;
            var hp = BattleMath.MulDivFloor(opponent.Hp, num, Percent * Percent);
            return new Combatant(
                hp < 1 ? 1 : hp,
                BattleMath.MulDivFloor(opponent.Atk, num, Percent * Percent),
                (int)BattleMath.MulDivFloor(opponent.Sharpness, num, Percent * Percent),
                (int)BattleMath.MulDivFloor(opponent.Reflect, num, Percent * Percent),
                (int)BattleMath.MulDivFloor(opponent.Absorb, num, Percent * Percent));
        }

        private static int ApplyAffinity(int value, StatKind stat, EquipmentData equipment, BattleData data)
        {
            var affinity = equipment.HasAffinity(stat) ? Percent : data.OffAffinityPercent;
            return Scale(value, affinity);
        }

        /// <summary>Giá trị ≤ <c>MaxStatValue</c> × 1000% × 1000% vẫn vừa <c>int</c>.</summary>
        private static int Scale(int value, int percent) => (int)BattleMath.MulDivFloor(value, percent, Percent);
    }
}
