using System;
using System.Collections.Generic;
using Crucible.Core.Content;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// <c>Quality</c> số nguyên (brewing.md §Formulas, chốt 2026-11-04): <c>Q = essence × (1 + add)</c>, rồi mỗi
    /// <c>mul%</c> theo thứ tự tăng dần <c>Q = ⌊Q × mul / 100⌋</c>, mỗi bước <c>min(cap, Q)</c>. Sắp tăng dần để floor
    /// từng bước không phụ thuộc thứ tự kích hoạt Effect.
    /// </summary>
    public static class QualityFormula
    {
        public const int MinMulPercent = 100;
        public const int MaxMulPercent = 500;

        public static long Compute(long essence, long addPotency, IReadOnlyList<int> mulPercents, long cap)
        {
            if (essence < 0) throw new ArgumentOutOfRangeException(nameof(essence), essence, "Must be ≥ 0.");
            if (addPotency < 0) throw new ArgumentOutOfRangeException(nameof(addPotency), addPotency, "Must be ≥ 0.");
            if (mulPercents == null) throw new ArgumentNullException(nameof(mulPercents));
            if (cap < 0 || cap > BrewingData.MaxQualityCap)
                throw new ArgumentOutOfRangeException(nameof(cap), cap, $"Must be in [0, {BrewingData.MaxQualityCap}].");

            var sorted = new int[mulPercents.Count];
            for (var i = 0; i < sorted.Length; i++)
            {
                var mul = mulPercents[i];
                if (mul < MinMulPercent || mul > MaxMulPercent)
                    throw new ArgumentOutOfRangeException(nameof(mulPercents), mul,
                        $"Each multiplier must be in [{MinMulPercent}, {MaxMulPercent}].");
                sorted[i] = mul;
            }

            Array.Sort(sorted);

            // essence × (1 + add) có thể tràn long → so với cap bằng phép chia trước.
            var factor = addPotency >= long.MaxValue ? long.MaxValue : addPotency + 1;
            var quality = essence > cap / factor ? cap : Math.Min(cap, essence * factor);

            // quality ≤ cap ≤ MaxQualityCap = long.MaxValue / MaxMulPercent nên quality × mul không tràn.
            for (var i = 0; i < sorted.Length; i++)
                quality = Math.Min(cap, quality * sorted[i] / 100);

            return quality;
        }
    }
}
