using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>Luật <see cref="StartRun"/> (run-flow §Khởi đầu run).</summary>
    public static class RunStartRules
    {
        private const int FirstSeason = 1;
        private const int FirstUid = 1;

        public static CommandResult Apply(RunState run, ContentDb content, StartRun command)
        {
            // Validate — chưa chạm state.
            if (run.IsActive)
                return CommandResult.Reject(RejectReason.RunAlreadyActive);
            if (!content.TryFindDifficulty(command.DifficultyId, out _))
                return CommandResult.Reject(RejectReason.UnknownContent);
            if (!content.TryFindStarterKit(command.StarterKitId, out var kit))
                return CommandResult.Reject(RejectReason.UnknownContent);

            var kitItems = new ItemId[kit.ItemIds.Count];
            for (var i = 0; i < kitItems.Length; i++)
            {
                if (!content.TryFindItem(kit.ItemIds[i], out kitItems[i]))
                    return CommandResult.Reject(RejectReason.UnknownContent);
            }

            // Mutate.
            run.IsActive = true;
            run.Seed = command.Seed;
            run.Rng = RngStreams.Create(command.Seed);
            run.DifficultyId = command.DifficultyId;
            run.StarterKitId = command.StarterKitId;
            run.StartedAtUnix = command.NowUnix;
            run.Season = FirstSeason;
            run.Gold = 0;
            run.Reputation = content.Run.StartingReputation;
            run.BagCapacity = content.Run.StartingBagCapacity;

            run.Bag.Clear();
            var uid = FirstUid;
            for (var i = 0; i < kitItems.Length; i++)
                run.Bag.Add(new ItemInstance(uid++, kitItems[i]));
            run.NextUid = uid;

            System.Array.Clear(run.Grid, 0, run.Grid.Length);
            run.WorkbenchStep = WorkbenchStep.Editing;
            run.LastBrewResultUid = 0;
            run.LastBrew = null;
            run.Shop.Slots.Clear();
            run.Shop.RerollCount = 0;
            // Rút của Chiếu chỉ Mùa 1 (hệ số giá…) ngay khi vào Decree — đọc lại save không rút lại (run-loop D1).
            SeasonRules.EnterDecree(run, content);

            return CommandResult.Accept(new GameEvent[] { new RunStarted(command.Seed) });
        }
    }
}
