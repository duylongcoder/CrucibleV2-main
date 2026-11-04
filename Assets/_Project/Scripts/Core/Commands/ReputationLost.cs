namespace Crucible.Core.Commands
{
    /// <summary>Thua trận: còn <see cref="Remaining"/> Uy tín.</summary>
    public sealed class ReputationLost : GameEvent
    {
        public ReputationLost(int remaining)
        {
            Remaining = remaining;
        }

        public int Remaining { get; }
    }
}
