using System;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>Rút loại trang bị của Mùa ở Chiếu chỉ (run-flow §Chiếu chỉ; stream <c>Forge</c>).</summary>
    public static class ForgeRules
    {
        /// <summary>
        /// Đúng 1 lần <c>NextInt(Σ trọng số)</c> trên stream <c>Forge</c> (kể cả bể 1 lựa chọn — D4), chọn theo trọng số tích
        /// lũy theo thứ tự khai báo bể của <c>run.Season</c>; trọng số = <c>⌊w × ForgeWeight% / 100⌋</c> của modifier đang áp
        /// (spoils R14; trung tính = trọng số khai báo). Ghi <see cref="RunState.DecreeEquipment"/>.
        /// </summary>
        public static void RollEquipment(RunState run, ContentDb content)
        {
            var season = content.Season(run.Season);
            var modifier = ModifierRules.Active(run, content);
            var pool = season.ForgePool;
            var total = 0L;
            for (var i = 0; i < pool.Count; i++)
                total += Weight(pool[i], modifier);
            if (total < 1 || total > int.MaxValue)
                throw new InvalidOperationException($"Forge weights sum {total} of season {run.Season} is outside [1, {int.MaxValue}].");

            ref var rng = ref run.Rng.Get(RngStream.Forge);
            var roll = rng.NextInt((int)total);
            for (var i = 0; i < pool.Count; i++)
            {
                var weight = Weight(pool[i], modifier);
                if (roll < weight)
                {
                    run.DecreeEquipment = pool[i].Equipment;
                    return;
                }

                roll -= (int)weight;
            }

            throw new InvalidOperationException($"Forge roll outside pool of season {run.Season}.");
        }

        private static long Weight(ForgeOption option, SeasonModifier modifier) =>
            ContentDb.ScaleWeight(option.Weight, modifier.ForgeWeightPercent(option.Equipment));
    }
}
