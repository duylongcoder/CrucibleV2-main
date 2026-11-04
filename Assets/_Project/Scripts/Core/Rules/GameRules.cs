using System;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Reducer duy nhất: dispatch Command theo type. Mỗi luật validate mọi điều kiện trước khi chạm state / RNG;
    /// bị từ chối → state không đổi (ADR-001, TECH §3.3).
    /// </summary>
    public static class GameRules
    {
        public static CommandResult Apply(RunState run, MetaState meta, ContentDb content, ICommand command)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (meta == null) throw new ArgumentNullException(nameof(meta));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (command == null) throw new ArgumentNullException(nameof(command));

            switch (command)
            {
                case StartRun startRun:
                    return RunStartRules.Apply(run, content, startRun);
                case PlaceItem placeItem:
                    return GridRules.Place(run, content, placeItem);
                case RemoveFromGrid removeFromGrid:
                    return GridRules.Remove(run, removeFromGrid);
                case MoveCell moveCell:
                    return GridRules.Move(run, moveCell);
                case Brew brew:
                    return BrewingRules.Brew(run, content, brew);
                case ReorderCatalyst reorderCatalyst:
                    return BagRules.ReorderCatalyst(run, content, reorderCatalyst);
                case SetTimescale setTimescale:
                    return SettingsRules.SetTimescale(meta, setTimescale);
                default:
                    return CommandResult.Reject(RejectReason.UnknownCommand);
            }
        }
    }
}
