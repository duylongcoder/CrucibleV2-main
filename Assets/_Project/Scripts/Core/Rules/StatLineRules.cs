using System;
using System.Collections.Generic;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>Sinh dòng thuộc tính lúc nung (GDD §6.2; TECH §3.4 bước 2) và đọc giá trị dòng cho Trận.</summary>
    public static class StatLineRules
    {
        /// <summary>
        /// Theo thứ tự khai báo: đúng 1 <c>Roll(ChancePercent)</c> mỗi dòng (kể cả 0 / 100); có → đúng 1
        /// <c>NextInt(Max − Min + 1)</c> (kể cả Min = Max — D9), giá trị = Min + kết quả. Không dòng nào → không rút.
        /// </summary>
        public static StatLine[] Roll(ref Pcg32 chance, Recipe recipe)
        {
            var defs = recipe.StatLines;
            if (defs.Count == 0)
                return Array.Empty<StatLine>();

            var lines = new StatLine[defs.Count];
            var count = 0;
            for (var i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                if (chance.Roll(def.ChancePercent))
                    lines[count++] = new StatLine(def.Stat, def.Min + chance.NextInt(def.Max - def.Min + 1));
            }

            if (count == lines.Length)
                return lines;
            if (count == 0)
                return Array.Empty<StatLine>();
            Array.Resize(ref lines, count);
            return lines;
        }

        /// <summary>
        /// <c>AddStat</c> của Xúc tác (catalyst R6): theo thứ tự, cộng vào dòng cùng thuộc tính hoặc thêm dòng cuối; mỗi giá
        /// trị kẹp <c>≤ <paramref name="max"/></c>. Không có bonus → trả nguyên <paramref name="rolled"/>.
        /// </summary>
        public static StatLine[] ApplyBonuses(StatLine[] rolled, IReadOnlyList<StatLine> bonuses, int max)
        {
            if (bonuses.Count == 0)
                return rolled;

            var lines = new List<StatLine>(rolled);
            for (var b = 0; b < bonuses.Count; b++)
            {
                var bonus = bonuses[b];
                var index = IndexOf(lines, bonus.Stat);
                if (index < 0)
                    lines.Add(new StatLine(bonus.Stat, Math.Min(max, bonus.Value)));
                else
                    lines[index] = new StatLine(bonus.Stat, (int)Math.Min(max, (long)lines[index].Value + bonus.Value));
            }

            return lines.ToArray();
        }

        private static int IndexOf(List<StatLine> lines, StatKind stat)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Stat == stat)
                    return i;
            }

            return -1;
        }

        /// <summary>Giá trị dòng <paramref name="stat"/> (đọc cả dòng chưa lộ — Trận); 0 nếu không có.</summary>
        public static int ValueOf(ItemInstance item, StatKind stat)
        {
            var lines = item.Lines;
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Stat == stat)
                    return lines[i].Value;
            }

            return 0;
        }
    }
}
