using System;
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
            {
                var item = run.Bag[i];
                writer.Write(item.Uid);
                writer.Write(item.ItemId.Index);
            }

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

        private static void WriteMeta(CanonicalWriter writer, MetaState meta)
        {
            writer.Write(meta.Renown);
        }
    }
}
