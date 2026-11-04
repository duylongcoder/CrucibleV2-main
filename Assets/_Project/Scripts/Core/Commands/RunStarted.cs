namespace Crucible.Core.Commands
{
    /// <summary>Run mới đã bắt đầu.</summary>
    public sealed class RunStarted : GameEvent
    {
        public RunStarted(ulong seed)
        {
            Seed = seed;
        }

        public ulong Seed { get; }
    }
}
