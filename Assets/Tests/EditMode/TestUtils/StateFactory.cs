using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Tests.TestUtils
{
    /// <summary>Dựng state run đang chơi trực tiếp (không qua luật) cho test <c>StateHasher</c>.</summary>
    public static class StateFactory
    {
        public const ulong Seed = 20261104;

        public static RunState CreateActiveRun()
        {
            var run = new RunState
            {
                IsActive = true,
                Seed = Seed,
                Rng = RngStreams.Create(Seed),
                DifficultyId = ContentFactory.Difficulty,
                StarterKitId = ContentFactory.StarterKit,
                StartedAtUnix = 1793750400,
                Season = 1,
                Phase = RunPhase.Decree,
                Gold = 0,
                Reputation = 3,
                BagCapacity = 8,
                NextUid = 4,
            };
            run.Bag.Add(new ItemInstance(1, new ItemId(0)));
            run.Bag.Add(new ItemInstance(2, new ItemId(1)));
            run.Bag.Add(new ItemInstance(3, new ItemId(2)));
            return run;
        }
    }
}
