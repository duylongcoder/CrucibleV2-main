using System.Collections.Generic;
using Crucible.Core.Content;

namespace Crucible.Core.State
{
    /// <summary>Hàng Chợ của lượt hiện tại (economy.md §Chợ). Ô cuối là ô tiện ích; giá tính lúc cần, không lưu.</summary>
    public sealed class ShopState
    {
        /// <summary>Rỗng = chưa bày hàng (trước lần làm mới đầu tiên).</summary>
        public List<ShopSlot> Slots { get; } = new List<ShopSlot>(MarketData.MaxShopSlots);
        /// <summary>Số lần reroll trong lượt hiện tại (đặt lại khi làm mới đầu lượt).</summary>
        public int RerollCount { get; set; }
    }
}
