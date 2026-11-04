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
        /// Đúng 1 lần <c>NextInt(TotalWeight)</c> trên stream <c>Forge</c> (kể cả bể 1 lựa chọn — D4), chọn theo trọng số tích
        /// lũy theo thứ tự khai báo bể của <c>run.Season</c>; ghi <see cref="RunState.DecreeEquipment"/>.
        /// </summary>
        public static void RollEquipment(RunState run, ContentDb content)
        {
            var season = content.Season(run.Season);
            ref var rng = ref run.Rng.Get(RngStream.Forge);
            var roll = rng.NextInt(season.TotalWeight);
            var pool = season.ForgePool;
            for (var i = 0; i < pool.Count; i++)
            {
                if (roll < pool[i].Weight)
                {
                    run.DecreeEquipment = pool[i].Equipment;
                    return;
                }

                roll -= pool[i].Weight;
            }

            throw new InvalidOperationException($"Forge roll outside pool of season {run.Season}.");
        }
    }
}
