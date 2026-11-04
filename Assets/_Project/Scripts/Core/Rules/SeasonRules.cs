using System.Collections.Generic;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Vòng lượt trong một Mùa (run-flow §Trạng thái): Chiếu chỉ → Bàn luyện × <c>TurnsPerSeason</c> → Dâng.
    /// Mỗi lượt bắt đầu ở Editing với hàng Chợ mới và kết thúc bằng <see cref="Commands.AckBrewResult"/> (sau Nung)
    /// hoặc <see cref="Commands.SkipTurn"/>. Dâng → Mùa kế / RunEnd thuộc F-009 phần 2.
    /// </summary>
    public static class SeasonRules
    {
        /// <summary>
        /// Vào Chiếu chỉ của <c>run.Season</c>: <c>Phase = Decree</c>, <c>Turn = 0</c>; modifier đã công bố thành modifier
        /// đang áp; rút hệ số giá Mùa (stream <c>Market</c>) → loại trang bị (stream <c>Forge</c>, có áp modifier) → modifier
        /// Mùa sau (stream <c>Season</c>, trừ Mùa cuối) — giữ thứ tự Market → Forge → Season (hợp đồng replay). Không
        /// validate — người gọi đảm bảo run đang active.
        /// </summary>
        public static void EnterDecree(RunState run, ContentDb content)
        {
            run.Phase = RunPhase.Decree;
            run.Turn = 0;
            run.ActiveModifier = run.NextModifier;
            run.NextModifier = null;
            MarketRules.RollSeasonPrice(run, content);
            ForgeRules.RollEquipment(run, content);
            if (run.Season < content.Run.SeasonCount)
                ModifierRules.RollNext(run, content);
        }

        public static CommandResult AcceptDecree(RunState run, ContentDb content, AcceptDecree command)
        {
            // Validate — chưa chạm state.
            if (!run.IsActive)
                return CommandResult.Reject(RejectReason.RunNotActive);
            if (run.Phase != RunPhase.Decree)
                return CommandResult.Reject(RejectReason.WrongPhase);

            // Mutate.
            var events = new List<GameEvent> { new DecreeAccepted(run.Season) };
            run.Phase = RunPhase.Workbench;
            StartTurn(run, content, 1, events);
            return CommandResult.Accept(events);
        }

        /// <summary>Luôn hợp lệ ở Editing, kể cả lưới trống (brewing AC-10). Vật phẩm trên lưới về cuối Túi theo ô 0→8.</summary>
        public static CommandResult SkipTurn(RunState run, ContentDb content, SkipTurn command)
        {
            // Validate — chưa chạm state.
            var reason = GridRules.ValidateEditable(run);
            if (reason != RejectReason.None)
                return CommandResult.Reject(reason);

            // Mutate.
            var events = new List<GameEvent>();
            var returned = false;
            for (var cell = 0; cell < run.Grid.Length; cell++)
            {
                if (run.Grid[cell] == null)
                    continue;
                run.Bag.Add(run.Grid[cell]);
                run.Grid[cell] = null;
                returned = true;
            }

            if (returned)
                events.Add(new GridChanged());
            events.Add(new TurnSkipped(run.Season, run.Turn));
            EndTurn(run, content, events);
            return CommandResult.Accept(events);
        }

        /// <summary>Kết thúc lượt sau khi xem kết quả Nung. Giữ <c>LastBrew</c> / <c>LastBrewResultUid</c> tới lần Nung kế (run-loop D4).</summary>
        public static CommandResult AckBrewResult(RunState run, ContentDb content, AckBrewResult command)
        {
            // Validate — chưa chạm state.
            if (!run.IsActive)
                return CommandResult.Reject(RejectReason.RunNotActive);
            if (run.Phase != RunPhase.Workbench)
                return CommandResult.Reject(RejectReason.WrongPhase);
            if (run.WorkbenchStep != WorkbenchStep.ResultPending)
                return CommandResult.Reject(RejectReason.NoPendingResult);

            // Mutate.
            var events = new List<GameEvent>();
            EndTurn(run, content, events);
            return CommandResult.Accept(events);
        }

        /// <summary>
        /// Resolution sau Trận (run-flow §Trạng thái; xét Uy tín trước Mùa): Thua → Uy tín −1, về 0 → Trục xuất; Mùa cuối →
        /// Thắng run; Thắng → Spoils (<see cref="SpoilsRules.Enter"/>); Thua còn Uy tín → Chiếu chỉ Mùa kế. Giữ <c>LastBattle</c> (forge-battle D11).
        /// </summary>
        public static CommandResult AckBattle(RunState run, ContentDb content, AckBattle command)
        {
            // Validate — chưa chạm state.
            if (!run.IsActive)
                return CommandResult.Reject(RejectReason.RunNotActive);
            if (run.Phase != RunPhase.Battle)
                return CommandResult.Reject(RejectReason.WrongPhase);

            // Mutate.
            var events = new List<GameEvent>();
            var won = run.LastBattle != null && run.LastBattle.HeroWon;
            if (!won)
            {
                run.Reputation--;
                events.Add(new ReputationLost(run.Reputation));
                if (run.Reputation <= 0)
                    return EndRun(run, RunOutcome.Exiled, events);
            }

            if (run.Season >= content.Run.SeasonCount)
                return EndRun(run, RunOutcome.Won, events);

            if (won)
                SpoilsRules.Enter(run, content, events);
            else
                StartNextSeason(run, content, events);
            return CommandResult.Accept(events);
        }

        /// <summary>Sang Chiếu chỉ Mùa kế + <see cref="SeasonStarted"/> — sau Thua còn Uy tín hoặc rời Chiến lợi phẩm.</summary>
        public static void StartNextSeason(RunState run, ContentDb content, List<GameEvent> events)
        {
            run.Season++;
            EnterDecree(run, content);
            events.Add(new SeasonStarted(run.Season));
        }

        /// <summary>Kết thúc run: giữ <c>IsActive</c> (D6) — mọi lệnh run sau đó bị <c>WrongPhase</c>.</summary>
        private static CommandResult EndRun(RunState run, RunOutcome outcome, List<GameEvent> events)
        {
            run.Phase = RunPhase.Ended;
            run.Outcome = outcome;
            events.Add(new RunEnded(outcome));
            return CommandResult.Accept(events);
        }

        /// <summary>
        /// Lưới mở + hàng Chợ mới (stream <c>Shop</c>). Lưới đã trống (Nung tiêu hao / Bỏ qua trả về Túi).
        /// F-007: kích <c>OnShopRefresh</c> ngay sau <see cref="MarketRules.RefreshShop"/>, trước <see cref="TurnStarted"/>.
        /// </summary>
        private static void StartTurn(RunState run, ContentDb content, int turn, List<GameEvent> events)
        {
            run.Turn = turn;
            run.WorkbenchStep = WorkbenchStep.Editing;
            MarketRules.RefreshShop(run, content);
            events.Add(new TurnStarted(run.Season, turn));
        }

        /// <summary>Lượt kế, hoặc Dâng sau lượt cuối (xóa hàng Chợ — Dâng không mua bán, run-loop D3).</summary>
        private static void EndTurn(RunState run, ContentDb content, List<GameEvent> events)
        {
            if (run.Turn < content.Run.TurnsPerSeason)
            {
                StartTurn(run, content, run.Turn + 1, events);
                return;
            }

            run.Phase = RunPhase.Offering;
            run.Turn = 0;
            run.WorkbenchStep = WorkbenchStep.Editing;
            run.Shop.Slots.Clear();
            run.Shop.RerollCount = 0;
            events.Add(new OfferingStarted(run.Season));
        }
    }
}
