namespace Crucible.Core.Content
{
    /// <summary>Thông số chung của một run (từ RunDef).</summary>
    public sealed class RunData
    {
        public RunData(int startingReputation, int startingBagCapacity, int maxBagCapacity, int seasonCount)
        {
            StartingReputation = startingReputation;
            StartingBagCapacity = startingBagCapacity;
            MaxBagCapacity = maxBagCapacity;
            SeasonCount = seasonCount;
        }

        public int StartingReputation { get; }
        public int StartingBagCapacity { get; }
        /// <summary>Trần sức chứa Túi — Nới Túi không vượt quá (economy.md §Túi).</summary>
        public int MaxBagCapacity { get; }
        public int SeasonCount { get; }
    }
}
