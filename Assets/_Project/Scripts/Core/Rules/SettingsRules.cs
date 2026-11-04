using Crucible.Core.Commands;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>Luật cài đặt người chơi lưu trong <see cref="MetaState"/>. Không chạm run, không rút random.</summary>
    public static class SettingsRules
    {
        /// <summary>Tập timescale cố định 1×–3× (brewing.md §Tuning — không phải knob).</summary>
        public const int MinTimescale = 1;
        public const int MaxTimescale = 3;

        public static CommandResult SetTimescale(MetaState meta, SetTimescale command)
        {
            if (command.Timescale < MinTimescale || command.Timescale > MaxTimescale)
                return CommandResult.Reject(RejectReason.InvalidTimescale);

            meta.Timescale = command.Timescale;
            return CommandResult.Accept(new GameEvent[] { new TimescaleChanged(command.Timescale) });
        }
    }
}
