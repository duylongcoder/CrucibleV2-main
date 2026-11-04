using System;
using System.Collections.Generic;

namespace Crucible.Core.Commands
{
    /// <summary>Kết quả <c>GameRules.Apply</c>: chấp nhận (+ sự kiện) hoặc từ chối (+ lý do).</summary>
    public readonly struct CommandResult
    {
        private CommandResult(bool accepted, RejectReason reason, IReadOnlyList<GameEvent> events)
        {
            Accepted = accepted;
            Reason = reason;
            Events = events;
        }

        public bool Accepted { get; }
        public RejectReason Reason { get; }
        public IReadOnlyList<GameEvent> Events { get; }

        public static CommandResult Accept(IReadOnlyList<GameEvent> events) =>
            new CommandResult(true, RejectReason.None, events);

        public static CommandResult Reject(RejectReason reason) =>
            new CommandResult(false, reason, Array.Empty<GameEvent>());
    }
}
