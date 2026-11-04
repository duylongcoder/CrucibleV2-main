using System.Collections.Generic;
using Crucible.Core.Logs;
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

        /// <summary>Lưới 3×3 theo thứ tự quét hàng = 9 tick; null = ô trống. Vật phẩm trên lưới không nằm trong <see cref="Bag"/>.</summary>
        public ItemInstance[] Grid { get; } = new ItemInstance[9];
        public WorkbenchStep WorkbenchStep { get; set; }
        /// <summary>Uid kết quả lượt Nung gần nhất; 0 = chưa có.</summary>
        public int LastBrewResultUid { get; set; }
        /// <summary>Nhật ký lượt Nung gần nhất (lưu cùng save để phát lại / hiện kết quả); null = chưa có.</summary>
        public BrewLog LastBrew { get; set; }

        /// <summary>Hệ số giá Mùa S (%) — rút ở Chiếu chỉ (stream <c>Market</c>); 100 = trung tính.</summary>
        public int SeasonPricePercent { get; set; }
        /// <summary>Hàng Chợ lượt hiện tại.</summary>
        public ShopState Shop { get; } = new ShopState();
    }
}
