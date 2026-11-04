using Crucible.Core.Content;

namespace Crucible.Core.Logs
{
    /// <summary>Một Effect của Xúc tác đã kích hoạt (mọi điều kiện đúng). <see cref="Blocked"/>: action không áp được (Túi đầy).</summary>
    public readonly struct CatalystActivation
    {
        public CatalystActivation(int catalystUid, int effectIndex, EffectTrigger trigger, bool blocked)
        {
            CatalystUid = catalystUid;
            EffectIndex = effectIndex;
            Trigger = trigger;
            Blocked = blocked;
        }

        public int CatalystUid { get; }
        /// <summary>Chỉ số Effect trong <c>ContentDb.EffectsOf</c> của Xúc tác.</summary>
        public int EffectIndex { get; }
        public EffectTrigger Trigger { get; }
        public bool Blocked { get; }
    }
}
