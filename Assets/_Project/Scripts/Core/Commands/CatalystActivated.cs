using Crucible.Core.Content;

namespace Crucible.Core.Commands
{
    /// <summary>Effect của Xúc tác <see cref="CatalystUid"/> đã kích hoạt (sau <c>BrewCompleted</c> / trước <c>TurnStarted</c>).</summary>
    public sealed class CatalystActivated : GameEvent
    {
        public CatalystActivated(int catalystUid, int effectIndex, EffectTrigger trigger, bool blocked)
        {
            CatalystUid = catalystUid;
            EffectIndex = effectIndex;
            Trigger = trigger;
            Blocked = blocked;
        }

        public int CatalystUid { get; }
        public int EffectIndex { get; }
        public EffectTrigger Trigger { get; }
        /// <summary>Action bị chặn (vd <c>CreateIngredient</c> khi Túi đầy).</summary>
        public bool Blocked { get; }
    }
}
