namespace Crucible.Core.Content
{
    /// <summary>Một lựa chọn trong bể trang bị của Mùa (dạng Id chuỗi, trước khi resolve).</summary>
    public sealed class ForgeOptionData
    {
        public ForgeOptionData(string equipmentId, int weight)
        {
            EquipmentId = equipmentId;
            Weight = weight;
        }

        public string EquipmentId { get; }
        /// <summary>Trọng số ≥ 0; 0 = không bao giờ ra.</summary>
        public int Weight { get; }
    }
}
