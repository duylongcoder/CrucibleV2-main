using System.Collections.Generic;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Chiến lợi phẩm (run-flow §Chiến lợi phẩm): vàng <c>SpoilsGold</c> tặng thẳng + tối đa
    /// <see cref="ContentDb.MaxSpoilsOptions"/> lựa chọn rút từ bể Mùa (stream <c>Reward</c>), lọc theo "nhận được ngay"
    /// trước khi rút. Rời Spoils bằng <see cref="Commands.ChooseReward"/> hoặc <see cref="Commands.SkipReward"/> (luôn dùng được — D4).
    /// </summary>
    public static class SpoilsRules
    {
        /// <summary>
        /// <c>AckBattle</c> gọi khi Thắng và Mùa &lt; <c>SeasonCount</c>: cộng vàng, rút lựa chọn, <c>Phase = Spoils</c>,
        /// thêm <see cref="SpoilsStarted"/>. Không validate — người gọi đảm bảo.
        /// </summary>
        public static void Enter(RunState run, ContentDb content, List<GameEvent> events)
        {
            var season = content.Season(run.Season);
            run.Gold += season.SpoilsGold;
            DrawOptions(run, content, season);
            run.Phase = RunPhase.Spoils;
            events.Add(new SpoilsStarted(run.Season, season.SpoilsGold, run.SpoilsOptions.ToArray()));
        }

        /// <summary>Nhận được ngay: vật phẩm cần chỗ Túi (+ suất Xúc tác nếu là Xúc tác); Nới Túi cần chưa ở trần.</summary>
        public static bool IsClaimable(RunState run, ContentDb content, Reward reward)
        {
            return ClaimReason(run, content, reward) == RejectReason.None;
        }

        /// <summary>Từ chối: <c>RunNotActive</c> → <c>WrongPhase</c> → <c>InvalidRewardIndex</c> → <c>CatalystSlotsFull</c> / <c>BagFull</c> / <c>BagAtMaxCapacity</c>. Không rút RNG.</summary>
        public static CommandResult ChooseReward(RunState run, ContentDb content, ChooseReward command)
        {
            // Validate — chưa chạm state.
            var reason = ValidateSpoils(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);
            if (command.Index < 0 || command.Index >= run.SpoilsOptions.Count)
                return CommandResult.Reject(RejectReason.InvalidRewardIndex);
            var rewardId = run.SpoilsOptions[command.Index];
            var reward = content.GetReward(rewardId);
            reason = ClaimReason(run, content, reward);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);

            // Mutate.
            var itemUid = 0;
            var bagAdded = 0;
            if (reward.Kind == RewardKind.Item)
            {
                itemUid = run.NextUid++;
                run.Bag.Add(new ItemInstance(itemUid, reward.Item));
            }
            else
            {
                bagAdded = BagRules.Expand(run, content, reward.Amount);
            }

            var events = new List<GameEvent> { new RewardChosen(command.Index, rewardId, itemUid, bagAdded) };
            Leave(run, content, events);
            return CommandResult.Accept(events);
        }

        /// <summary>Không nhận gì (vàng đã cộng vẫn giữ). Từ chối: <c>RunNotActive</c> → <c>WrongPhase</c>.</summary>
        public static CommandResult SkipReward(RunState run, ContentDb content, SkipReward command)
        {
            // Validate — chưa chạm state.
            var reason = ValidateSpoils(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);

            // Mutate.
            var events = new List<GameEvent> { new RewardSkipped(run.Season) };
            Leave(run, content, events);
            return CommandResult.Accept(events);
        }

        private static RejectReason ValidateSpoils(RunState run)
        {
            if (!run.IsActive)
                return RejectReason.RunNotActive;
            return run.Phase == RunPhase.Spoils ? RejectReason.None : RejectReason.WrongPhase;
        }

        /// <summary>Thứ tự như <c>Buy</c>: suất Xúc tác trước chỗ Túi.</summary>
        private static RejectReason ClaimReason(RunState run, ContentDb content, Reward reward)
        {
            if (reward.Kind == RewardKind.ExpandBag)
                return BagRules.CanExpand(run, content) ? RejectReason.None : RejectReason.BagAtMaxCapacity;
            if (reward.IsCatalyst && BagRules.CountCatalysts(run, content) >= content.Market.CatalystSlots)
                return RejectReason.CatalystSlotsFull;
            return BagRules.HasRoom(run) ? RejectReason.None : RejectReason.BagFull;
        }

        private static void Leave(RunState run, ContentDb content, List<GameEvent> events)
        {
            run.SpoilsOptions.Clear();
            SeasonRules.StartNextSeason(run, content, events);
        }

        /// <summary>
        /// R3: ứng viên = mục bể trọng số &gt; 0 và nhận được. Đảm bảo Xúc tác → lượt đầu chỉ trong ứng viên Xúc tác (nếu
        /// có). Mỗi lượt đúng 1 <c>NextInt(Σ trọng số)</c> kể cả còn 1 ứng viên (D12); không lặp. Kết quả theo thứ tự
        /// khai báo bể (D3). Buffer cấp phát 1 lần mỗi lần vào Spoils (không phải vòng nóng).
        /// </summary>
        private static void DrawOptions(RunState run, ContentDb content, SeasonContent season)
        {
            var pool = season.RewardPool;
            var candidates = new List<int>(pool.Count);
            for (var i = 0; i < pool.Count; i++)
            {
                if (pool[i].Weight > 0 && IsClaimable(run, content, content.GetReward(pool[i].Reward)))
                    candidates.Add(i);
            }

            var picked = new bool[pool.Count];
            var count = 0;
            ref var rng = ref run.Rng.Get(RngStream.Reward);
            if (season.GuaranteeCatalyst)
            {
                var index = Pick(ref rng, pool, candidates, content, catalystOnly: true);
                if (index >= 0)
                {
                    picked[index] = true;
                    candidates.Remove(index);
                    count++;
                }
            }

            while (count < ContentDb.MaxSpoilsOptions && candidates.Count > 0)
            {
                var index = Pick(ref rng, pool, candidates, content, catalystOnly: false);
                picked[index] = true;
                candidates.Remove(index);
                count++;
            }

            run.SpoilsOptions.Clear();
            for (var i = 0; i < pool.Count; i++)
            {
                if (picked[i])
                    run.SpoilsOptions.Add(pool[i].Reward);
            }
        }

        /// <summary>Chọn theo trọng số tích lũy trên ứng viên (thứ tự bể); −1 và không rút khi không có ứng viên phù hợp.</summary>
        private static int Pick(ref Pcg32 rng, IReadOnlyList<RewardOption> pool, List<int> candidates, ContentDb content,
                                bool catalystOnly)
        {
            var total = 0;
            for (var c = 0; c < candidates.Count; c++)
            {
                if (Matches(pool[candidates[c]], content, catalystOnly))
                    total += pool[candidates[c]].Weight;
            }

            if (total == 0)
                return -1;

            var roll = rng.NextInt(total);
            for (var c = 0; c < candidates.Count; c++)
            {
                var option = pool[candidates[c]];
                if (!Matches(option, content, catalystOnly))
                    continue;
                if (roll < option.Weight)
                    return candidates[c];
                roll -= option.Weight;
            }

            // Không tới được: roll < total = Σ trọng số ứng viên phù hợp.
            return -1;
        }

        private static bool Matches(RewardOption option, ContentDb content, bool catalystOnly) =>
            !catalystOnly || content.GetReward(option.Reward).IsCatalyst;
    }
}
