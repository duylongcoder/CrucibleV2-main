using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Loại trang bị (battle.md §Dũng sĩ ta): quy đổi <c>Quality</c> thành ATK / HP + thuộc tính hợp 100%.</summary>
    public sealed class EquipmentData
    {
        public EquipmentData(string id, int atkPerQ, int hpPerQ, IReadOnlyList<StatKind> affinities)
        {
            Id = id;
            AtkPerQ = atkPerQ;
            HpPerQ = hpPerQ;
            Affinities = affinities;
        }

        public string Id { get; }
        /// <summary>% <c>Quality</c> quy thành ATK (0–100).</summary>
        public int AtkPerQ { get; }
        /// <summary>% <c>Quality</c> quy thành HP (0–100).</summary>
        public int HpPerQ { get; }
        /// <summary>Thuộc tính hợp 100%; thuộc tính khác dùng <see cref="BattleData.OffAffinityPercent"/>.</summary>
        public IReadOnlyList<StatKind> Affinities { get; }

        public bool HasAffinity(StatKind stat)
        {
            for (var i = 0; i < Affinities.Count; i++)
            {
                if (Affinities[i] == stat)
                    return true;
            }

            return false;
        }
    }
}
