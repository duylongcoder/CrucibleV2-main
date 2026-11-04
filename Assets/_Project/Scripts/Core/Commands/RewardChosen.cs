using Crucible.Core.Content;

namespace Crucible.Core.Commands
{
    /// <summary>Đã nhận lựa chọn <see cref="Index"/> (<see cref="Reward"/>).</summary>
    public sealed class RewardChosen : GameEvent
    {
        public RewardChosen(int index, RewardId reward, int itemUid, int bagAdded)
        {
            Index = index;
            Reward = reward;
            ItemUid = itemUid;
            BagAdded = bagAdded;
        }

        public int Index { get; }
        public RewardId Reward { get; }
        /// <summary>Uid vật phẩm mới vào Túi; 0 khi phần thưởng không phải vật phẩm.</summary>
        public int ItemUid { get; }
        /// <summary>Số chỗ Túi thực sự thêm (Nới Túi); 0 với loại khác.</summary>
        public int BagAdded { get; }
    }
}
