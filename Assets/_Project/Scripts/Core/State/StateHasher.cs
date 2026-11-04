using System;
using Crucible.Core.Logs;
using Crucible.Core.Random;

namespace Crucible.Core.State
{
    /// <summary>
    /// Hash tất định của state — so sánh replay / phát hiện lệch (TECH §3.5). Thứ tự field cố định; plan nào thêm
    /// field gameplay phải ghi field đó ở đây và thêm một case vào <c>StateHasherTest.Hash_StatesDifferInOneField_*</c>.
    /// </summary>
    public static class StateHasher
    {
        public static ulong Hash(RunState run, MetaState meta)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            var writer = new CanonicalWriter();
            WriteRun(writer, run);
            WriteMeta(writer, meta);
            return writer.Hash;
        }

        private static void WriteRun(CanonicalWriter writer, RunState run)
        {
            writer.Write(run.IsActive);
            writer.Write(run.Seed);
            writer.Write(run.DifficultyId);
            writer.Write(run.StarterKitId);
            writer.Write(run.StartedAtUnix);
            writer.Write(run.Season);
            writer.Write((int)run.Phase);
            writer.Write(run.Gold);
            writer.Write(run.Reputation);
            writer.Write(run.BagCapacity);
            writer.Write(run.NextUid);

            writer.Write(run.Bag.Count);
            for (var i = 0; i < run.Bag.Count; i++)
                WriteItem(writer, run.Bag[i]);

            writer.Write(run.Grid.Length);
            for (var i = 0; i < run.Grid.Length; i++)
            {
                writer.Write(run.Grid[i] != null);
                if (run.Grid[i] != null)
                    WriteItem(writer, run.Grid[i]);
            }

            writer.Write((int)run.WorkbenchStep);
            writer.Write(run.LastBrewResultUid);
            WriteBrewLog(writer, run.LastBrew);

            writer.Write(run.Rng != null);
            if (run.Rng != null)
            {
                for (var i = 0; i < RngStreams.Count; i++)
                {
                    var state = run.Rng.Get((RngStream)i).State;
                    writer.Write(state.State);
                    writer.Write(state.Inc);
                }
            }
        }

        private static void WriteItem(CanonicalWriter writer, ItemInstance item)
        {
            writer.Write(item.Uid);
            writer.Write(item.ItemId.Index);
            writer.Write(item.Magic);
            writer.Write(item.Quality);
        }

        private static void WriteBrewLog(CanonicalWriter writer, BrewLog log)
        {
            writer.Write(log != null);
            if (log == null)
                return;

            writer.Write(log.FirstCell);
            writer.Write(log.Ticks.Count);
            for (var i = 0; i < log.Ticks.Count; i++)
            {
                var tick = log.Ticks[i];
                writer.Write(tick.HasItem);
                writer.Write(tick.ItemId.Index);
                writer.Write(tick.Magic);
            }

            writer.Write(log.Result.IsScrap);
            writer.Write(log.Result.ItemId.Index);
            writer.Write(log.Result.Magic);
        }

        private static void WriteMeta(CanonicalWriter writer, MetaState meta)
        {
            writer.Write(meta.Renown);
            writer.Write(meta.Timescale);
        }
    }
}
