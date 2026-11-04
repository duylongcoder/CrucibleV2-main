using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Content Rèn & Trận (battle.md §Tuning Knobs): hằng số trận + trang bị + đối thủ + khai báo từng Mùa.</summary>
    public sealed class BattleData
    {
        /// <summary>Trần giá trị SB / PD / HT (thô và sau scale) — giữ phép nhân trận trong <c>long</c> (forge-battle D9).</summary>
        public const int MaxStatValue = 10_000;

        public BattleData(int heroBaseHp, int heroBaseAtk, int offAffinityPercent, int absorbCapPercent,
                          int reflectCapPercent, int maxRounds, IReadOnlyList<EquipmentData> equipment,
                          IReadOnlyList<OpponentData> opponents, IReadOnlyList<SeasonData> seasons)
        {
            HeroBaseHp = heroBaseHp;
            HeroBaseAtk = heroBaseAtk;
            OffAffinityPercent = offAffinityPercent;
            AbsorbCapPercent = absorbCapPercent;
            ReflectCapPercent = reflectCapPercent;
            MaxRounds = maxRounds;
            Equipment = equipment;
            Opponents = opponents;
            Seasons = seasons;
        }

        public int HeroBaseHp { get; }
        public int HeroBaseAtk { get; }
        /// <summary>% giữ lại của thuộc tính không hợp loại trang bị (0–100).</summary>
        public int OffAffinityPercent { get; }
        /// <summary>Trần Hấp thụ % (<c>HTCap</c>).</summary>
        public int AbsorbCapPercent { get; }
        /// <summary>Trần Phản đòn % (<c>PDCap</c>).</summary>
        public int ReflectCapPercent { get; }
        public int MaxRounds { get; }
        public IReadOnlyList<EquipmentData> Equipment { get; }
        public IReadOnlyList<OpponentData> Opponents { get; }
        /// <summary>Đúng <c>RunData.SeasonCount</c> phần tử, chỉ số = Mùa − 1.</summary>
        public IReadOnlyList<SeasonData> Seasons { get; }
    }
}
