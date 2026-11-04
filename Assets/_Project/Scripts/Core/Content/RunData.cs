namespace Crucible.Core.Content
{
    /// <summary>Thông số chung của một run (từ RunDef).</summary>
    public sealed class RunData
    {
        public RunData(int startingReputation, int startingBagCapacity, int maxBagCapacity, int seasonCount, int turnsPerSeason = 3)
        {
            StartingReputation = startingReputation;
            StartingBagCapacity = startingBagCapacity;
            MaxBagCapacity = maxBagCapacity;
            SeasonCount = seasonCount;
            TurnsPerSeason = turnsPerSeason;
        }

        public int StartingReputation { get; }
        public int StartingBagCapacity { get; }
        /// <summary>Trần sức chứa Túi — Nới Túi không vượt quá (economy.md §Túi).</summary>
        public int MaxBagCapacity { get; }
        public int SeasonCount { get; }
        /// <summary>Số lượt Bàn luyện mỗi Mùa (run-flow Tuning Knobs: 3, an toàn 2–4); ≥ 1.</summary>
        public int TurnsPerSeason { get; }
    }
}
