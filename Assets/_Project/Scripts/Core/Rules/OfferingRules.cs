using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>Dâng (run-flow §Dâng; ui-season §B): dâng 1 thành phẩm đạt chuẩn → mô phỏng Trận, hoặc để cận thần ra về.</summary>
    public static class OfferingRules
    {
        /// <summary>Đạt chuẩn = khoáng có công thức (thành phẩm). Nguyên liệu gốc, phế phẩm, phụ gia, Kính, Xúc tác → không.</summary>
        public static bool IsOfferable(ItemData item)
        {
            return item.Kind == ItemKind.Mineral && item.Recipe != null;
        }

        public static bool HasOfferable(RunState run, ContentDb content)
        {
            for (var i = 0; i < run.Bag.Count; i++)
            {
                if (IsOfferable(content.GetItem(run.Bag[i].ItemId)))
                    return true;
            }

            return false;
        }

        /// <summary>Từ chối: <c>RunNotActive</c> → <c>WrongPhase</c> → <c>ItemNotFound</c> → <c>ItemNotOfferable</c>. Không rút RNG.</summary>
        public static CommandResult Offer(RunState run, ContentDb content, Offer command)
        {
            // Validate — chưa chạm state.
            var reason = ValidateOffering(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);
            var index = BagRules.IndexOfUid(run, command.Uid);
            if (index < 0)
                return CommandResult.Reject(RejectReason.ItemNotFound);
            var offered = run.Bag[index];
            if (!IsOfferable(content.GetItem(offered.ItemId)))
                return CommandResult.Reject(RejectReason.ItemNotOfferable);

            // Mutate.
            run.Bag.RemoveAt(index);
            var battle = content.Battle;
            // F-008: truyền giá trị Sắc bén / Phản đòn / Hấp thụ từ dòng thuộc tính của khoáng (D1).
            var hero = BattleStats.ForHero(offered.Quality, 0, 0, 0, content.GetEquipment(run.DecreeEquipment), battle);
            var log = BattleSimulator.Simulate(run.Season, run.DecreeEquipment, offered, hero, Opponent(run, content),
                battle);
            return EnterBattle(run, log, new ItemOffered(run.Season, command.Uid));
        }

        /// <summary>Chỉ khi Túi không có thành phẩm đạt chuẩn (D3) — Thua qua <see cref="BattleLog.Forfeit"/>.</summary>
        public static CommandResult DismissCourtier(RunState run, ContentDb content, DismissCourtier command)
        {
            // Validate — chưa chạm state.
            var reason = ValidateOffering(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);
            if (HasOfferable(run, content))
                return CommandResult.Reject(RejectReason.HasOfferableItem);

            // Mutate.
            var hero = BattleStats.ForHero(0, 0, 0, 0, content.GetEquipment(run.DecreeEquipment), content.Battle);
            var log = BattleLog.Forfeit(run.Season, run.DecreeEquipment, hero, Opponent(run, content));
            return EnterBattle(run, log, new CourtierDismissed(run.Season));
        }

        private static RejectReason ValidateOffering(RunState run)
        {
            if (!run.IsActive)
                return RejectReason.RunNotActive;
            return run.Phase == RunPhase.Offering ? RejectReason.None : RejectReason.WrongPhase;
        }

        private static Combatant Opponent(RunState run, ContentDb content)
        {
            content.TryFindDifficulty(run.DifficultyId, out var difficulty);
            var percent = difficulty?.OpponentStatPercent ?? SeasonModifier.NeutralPercent;
            return BattleStats.ForOpponent(content.Season(run.Season).Opponent, percent,
                ModifierRules.Active(run, content).OpponentStatPercent);
        }

        private static CommandResult EnterBattle(RunState run, BattleLog log, GameEvent first)
        {
            run.LastBattle = log;
            run.Phase = RunPhase.Battle;
            return CommandResult.Accept(new[] { first, new BattleStarted(run.Season, log) });
        }

    }
}
