namespace Crucible.Core.Commands
{
    /// <summary>Bắt đầu run mới (run-flow §Khởi đầu run). Seed và thời điểm do Runtime truyền vào.</summary>
    public sealed class StartRun : ICommand
    {
        public StartRun(string difficultyId, string starterKitId, ulong seed, long nowUnix)
        {
            DifficultyId = difficultyId;
            StarterKitId = starterKitId;
            Seed = seed;
            NowUnix = nowUnix;
        }

        public string DifficultyId { get; }
        public string StarterKitId { get; }
        public ulong Seed { get; }
        public long NowUnix { get; }
    }
}
