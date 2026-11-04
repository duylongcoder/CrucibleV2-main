namespace Crucible.Core.Content
{
    /// <summary>
    /// Dòng thuộc tính khởi đầu của công thức (GDD §6.2, chốt 2026-11-04): khi nung khớp, tung <see cref="ChancePercent"/>
    /// — có thì giá trị rút đều trong [<see cref="Min"/>, <see cref="Max"/>]. Mỗi <see cref="StatKind"/> tối đa 1 dòng / công thức.
    /// </summary>
    public sealed class StatLineData
    {
        public StatLineData(StatKind stat, int chancePercent, int min, int max)
        {
            Stat = stat;
            ChancePercent = chancePercent;
            Min = min;
            Max = max;
        }

        public StatKind Stat { get; }
        /// <summary>% xuất hiện (0–100).</summary>
        public int ChancePercent { get; }
        /// <summary>≥ 1 (inspect D5).</summary>
        public int Min { get; }
        /// <summary>≤ <c>BattleData.MaxStatValue</c>.</summary>
        public int Max { get; }
    }
}
