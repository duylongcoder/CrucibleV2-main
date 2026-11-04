namespace Crucible.Core.Content
{
    /// <summary>Thông số chung của một run (từ RunDef).</summary>
    public sealed class RunData
    {
        public RunData(int startingReputation, int startingBagCapacity, int seasonCount)
        {
            StartingReputation = startingReputation;
            StartingBagCapacity = startingBagCapacity;
            SeasonCount = seasonCount;
        }

        public int StartingReputation { get; }
        public int StartingBagCapacity { get; }
        public int SeasonCount { get; }
    }
}
