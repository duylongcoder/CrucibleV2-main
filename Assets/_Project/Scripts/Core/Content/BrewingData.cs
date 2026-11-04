using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>
    /// Tuning toàn game của Lò nung (brewing.md §Tuning Knobs). Bảng theo tier: chỉ số = tier − 1, đủ
    /// <see cref="TierCount"/> phần tử, giá trị phần trăm 0–<see cref="MaxPercent"/> (ContentDb validate).
    /// </summary>
    public sealed class BrewingData
    {
        /// <summary>Trần lớn nhất cho phép: <c>QualityCap × 500</c> (Mul tối đa) không tràn <c>long</c>.</summary>
        public const long MaxQualityCap = long.MaxValue / 500;
        /// <summary>Tier Khoáng / Phụ gia: 1–10.</summary>
        public const int TierCount = 10;
        public const int MaxPercent = 100;
        /// <summary><c>MagicChance</c> tối đa của một phụ gia (brewing.md §Formulas).</summary>
        public const int MaxAdditiveMagicChance = 50;

        public BrewingData(long qualityCap, IReadOnlyList<int> magicInheritByTier, IReadOnlyList<int> magicCapByTier,
                           IReadOnlyList<int> magicRetainByTier)
        {
            QualityCap = qualityCap;
            MagicInheritByTier = magicInheritByTier;
            MagicCapByTier = magicCapByTier;
            MagicRetainByTier = magicRetainByTier;
        }

        /// <summary>Trần <c>Quality</c> (mặc định 10^15); ≤ <see cref="MaxQualityCap"/>.</summary>
        public long QualityCap { get; }
        /// <summary>% ma thuật mỗi nguyên liệu đã ma thuật góp, theo tier của nguyên liệu (mặc định 5 × tier).</summary>
        public IReadOnlyList<int> MagicInheritByTier { get; }
        /// <summary>Trần % ma thuật theo tier thành phẩm (mặc định 90).</summary>
        public IReadOnlyList<int> MagicCapByTier { get; }
        /// <summary>% giữ lại của tổng, theo tier thành phẩm (mặc định 100).</summary>
        public IReadOnlyList<int> MagicRetainByTier { get; }
    }
}
