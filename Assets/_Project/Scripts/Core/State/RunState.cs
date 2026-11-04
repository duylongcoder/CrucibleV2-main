using System.Collections.Generic;
using Crucible.Core.Content;
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
        /// <summary>Lượt Bàn luyện hiện tại: 1…<c>TurnsPerSeason</c> khi <see cref="Phase"/> = Workbench, 0 ở phase khác.</summary>
        public int Turn { get; set; }
        /// <summary>Uid kết quả lượt Nung gần nhất; 0 = chưa có.</summary>
        public int LastBrewResultUid { get; set; }
        /// <summary>Nhật ký lượt Nung gần nhất (lưu cùng save để phát lại / hiện kết quả); null = chưa có.</summary>
        public BrewLog LastBrew { get; set; }

        /// <summary>Hệ số giá Mùa S (%) — rút ở Chiếu chỉ (stream <c>Market</c>); 100 = trung tính.</summary>
        public int SeasonPricePercent { get; set; }
        /// <summary>Hàng Chợ lượt hiện tại.</summary>
        public ShopState Shop { get; } = new ShopState();

        /// <summary>Loại trang bị Mùa này — rút ở Chiếu chỉ (stream <c>Forge</c>).</summary>
        public EquipmentId DecreeEquipment { get; set; }
        /// <summary>Nhật ký Trận gần nhất (lưu cùng save; giữ sau <c>AckBattle</c> tới lần Dâng kế); null = chưa có.</summary>
        public BattleLog LastBattle { get; set; }
        /// <summary>Kết cục run; <see cref="RunOutcome.None"/> tới khi <see cref="Phase"/> = Ended.</summary>
        public RunOutcome Outcome { get; set; }
        /// <summary>Lựa chọn Chiến lợi phẩm đang bày (thứ tự = thứ tự khai báo bể); rỗng ngoài <see cref="RunPhase.Spoils"/>.</summary>
        public List<RewardId> SpoilsOptions { get; } = new List<RewardId>(ContentDb.MaxSpoilsOptions);
    }
}
