namespace Crucible.Core.Rules
{
    /// <summary>Số học trận không tràn <c>long</c> (forge-battle D9).</summary>
    public static class BattleMath
    {
        /// <summary>⌊x × num / den⌋ chính xác với x, num ≥ 0, den ≥ 1: <c>(x / den) × num + (x % den) × num / den</c>.</summary>
        public static long MulDivFloor(long x, long num, long den)
        {
            return x / den * num + x % den * num / den;
        }
    }
}
