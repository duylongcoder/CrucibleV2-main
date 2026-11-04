using System;
using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Effect primitive = Trigger + Condition (AND) + đúng 1 Action (economy.md; catalyst D2).</summary>
    public sealed class EffectData
    {
        public EffectData(EffectTrigger trigger, EffectActionData action, IReadOnlyList<EffectConditionData> conditions = null)
        {
            Trigger = trigger;
            Action = action;
            Conditions = conditions ?? Array.Empty<EffectConditionData>();
        }

        public EffectTrigger Trigger { get; }
        public EffectActionData Action { get; }
        public IReadOnlyList<EffectConditionData> Conditions { get; }
    }
}
