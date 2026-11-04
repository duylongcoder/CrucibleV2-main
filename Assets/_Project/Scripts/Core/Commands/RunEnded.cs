using Crucible.Core.State;

namespace Crucible.Core.Commands
{
    /// <summary>Run kết thúc với <see cref="Outcome"/>.</summary>
    public sealed class RunEnded : GameEvent
    {
        public RunEnded(RunOutcome outcome)
        {
            Outcome = outcome;
        }

        public RunOutcome Outcome { get; }
    }
}
