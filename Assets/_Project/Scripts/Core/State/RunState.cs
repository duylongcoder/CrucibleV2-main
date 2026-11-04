using System.Collections.Generic;
using Crucible.Core.Random;

namespace Crucible.Core.State
{
    /// <summary>State mutable của run — chỉ luật trong Core sửa (TECH §3.2). Phần nền.</summary>
    public sealed class RunState
    {
        /// <summary>false = chưa có run (trước <c>StartRun</c>).</summary>
        public bool IsActive { get; set; }
        public ulong Seed { get; set; }
        /// <summary>null khi chưa có run.</summary>
        public RngStreams Rng { get; set; }
        public string DifficultyId { get; set; }
        public string StarterKitId { get; set; }
        public long StartedAtUnix { get; set; }
        public int Season { get; set; }
        public RunPhase Phase { get; set; }
        public int Gold { get; set; }
        public int Reputation { get; set; }
        public int BagCapacity { get; set; }
        /// <summary>Thứ tự list = thứ tự Xúc tác.</summary>
        public List<ItemInstance> Bag { get; } = new List<ItemInstance>();
        public int NextUid { get; set; }
    }
}
