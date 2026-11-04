namespace Crucible.Core.State
{
    /// <summary>Kết cục run (run-flow §Kết thúc run). <see cref="None"/> khi run chưa kết thúc.</summary>
    public enum RunOutcome
    {
        None = 0,
        Won = 1,
        Exiled = 2,
    }
}
