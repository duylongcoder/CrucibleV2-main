namespace Crucible.Core.Content
{
    /// <summary>Tuning toàn game của Lò nung (brewing.md §Tuning Knobs).</summary>
    public sealed class BrewingData
    {
        /// <summary>Trần lớn nhất cho phép: <c>QualityCap × 500</c> (Mul tối đa) không tràn <c>long</c>.</summary>
        public const long MaxQualityCap = long.MaxValue / 500;

        public BrewingData(long qualityCap)
        {
            QualityCap = qualityCap;
        }

        /// <summary>Trần <c>Quality</c> (mặc định 10^15); ≤ <see cref="MaxQualityCap"/>.</summary>
        public long QualityCap { get; }
    }
}
