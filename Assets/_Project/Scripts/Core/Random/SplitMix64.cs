namespace Crucible.Core.Random
{
    /// <summary>SplitMix64 (Steele, Lea, Flood 2014) — dùng để dẫn seed cho các stream PCG32.</summary>
    public static class SplitMix64
    {
        private const ulong GoldenGamma = 0x9E3779B97F4A7C15UL;

        public static ulong Next(ref ulong state)
        {
            unchecked
            {
                state += GoldenGamma;
                var z = state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }
    }
}
