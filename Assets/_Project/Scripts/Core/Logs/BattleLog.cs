using System;
using System.Collections.Generic;
using Crucible.Core.Content;

namespace Crucible.Core.Logs
{
    /// <summary>
    /// Nhật ký Trận (battle.md §BattleLog): tính ngay khi Dâng, lưu cùng save, phát lại chỉ đọc. Bất biến; <b>không</b>
    /// chứa <c>Quality</c> hay dòng ẩn (ADR-007) — chỉ chỉ số đã quy đổi.
    /// </summary>
    public sealed class BattleLog
    {
        internal BattleLog(int season, EquipmentId equipment, bool hasOfferedItem, ItemId offeredItem, bool offeredMagic,
                           Combatant hero, Combatant opponent, BattleRound[] rounds, long heroTotalDealt,
                           long opponentTotalDealt, BattleEndReason endReason, bool heroWon)
        {
            Season = season;
            Equipment = equipment;
            HasOfferedItem = hasOfferedItem;
            OfferedItem = offeredItem;
            OfferedMagic = offeredMagic;
            Hero = hero;
            Opponent = opponent;
            Rounds = rounds ?? Array.Empty<BattleRound>();
            HeroTotalDealt = heroTotalDealt;
            OpponentTotalDealt = opponentTotalDealt;
            EndReason = endReason;
            HeroWon = heroWon;
        }

        public int Season { get; }
        /// <summary>Loại trang bị đã rèn = loại của Chiếu chỉ cùng Mùa.</summary>
        public EquipmentId Equipment { get; }
        /// <summary>false khi <see cref="BattleEndReason.Forfeit"/>.</summary>
        public bool HasOfferedItem { get; }
        public ItemId OfferedItem { get; }
        public bool OfferedMagic { get; }
        public Combatant Hero { get; }
        public Combatant Opponent { get; }
        public IReadOnlyList<BattleRound> Rounds { get; }
        /// <summary>Σ đòn ta + Σ dội ngược đối thủ nhận.</summary>
        public long HeroTotalDealt { get; }
        /// <summary>Σ đòn đối thủ + Σ dội ngược ta nhận.</summary>
        public long OpponentTotalDealt { get; }
        public BattleEndReason EndReason { get; }
        public bool HeroWon { get; }

        /// <summary>Không có gì để dâng: Thua, không hiệp nào.</summary>
        public static BattleLog Forfeit(int season, EquipmentId equipment, Combatant hero, Combatant opponent)
        {
            return new BattleLog(season, equipment, false, default, false, hero, opponent, Array.Empty<BattleRound>(), 0, 0,
                BattleEndReason.Forfeit, false);
        }
    }
}
