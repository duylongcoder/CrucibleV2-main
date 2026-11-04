namespace Crucible.Core.Random
{
    /// <summary>7 stream RNG độc lập (ADR-004). Thứ tự enum = thứ tự chụp/khôi phục — không đổi.</summary>
    public enum RngStream
    {
        Shop = 0,
        Chance = 1,
        Inspect = 2,
        Forge = 3,
        Market = 4,
        Season = 5,
        Reward = 6,
    }
}
