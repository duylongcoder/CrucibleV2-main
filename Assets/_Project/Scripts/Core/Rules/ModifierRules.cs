using System;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>Modifier Mùa (run-flow §Modifier Mùa): rút modifier Mùa sau ở Chiếu chỉ (stream <c>Season</c>), tra modifier đang áp.</summary>
    public static class ModifierRules
    {
        /// <summary>Modifier áp cho Mùa hiện tại; <see cref="SeasonModifier.Neutral"/> khi không có.</summary>
        public static SeasonModifier Active(RunState run, ContentDb content)
        {
            return run.ActiveModifier.HasValue ? content.GetModifier(run.ActiveModifier.Value) : SeasonModifier.Neutral;
        }

        /// <summary>
        /// Chỉ gọi khi <c>run.Season &lt; SeasonCount</c>: đúng 1 <c>NextInt(ModifierTotalWeight)</c> trên stream <c>Season</c>
        /// (kể cả bể chỉ có "không modifier" — D12) từ bể của Mùa <c>run.Season + 1</c>; phần "không modifier" đứng trước bể.
        /// </summary>
        public static void RollNext(RunState run, ContentDb content)
        {
            var next = content.Season(run.Season + 1);
            ref var rng = ref run.Rng.Get(RngStream.Season);
            var roll = rng.NextInt(next.ModifierTotalWeight);
            run.NextModifier = null;
            if (roll < next.NoModifierWeight)
                return;

            roll -= next.NoModifierWeight;
            var pool = next.ModifierPool;
            for (var i = 0; i < pool.Count; i++)
            {
                if (roll < pool[i].Weight)
                {
                    run.NextModifier = pool[i].Modifier;
                    return;
                }

                roll -= pool[i].Weight;
            }

            throw new InvalidOperationException($"Modifier roll outside pool of season {run.Season + 1}.");
        }
    }
}
