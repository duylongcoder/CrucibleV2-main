namespace Crucible.Core.Content
{
    /// <summary>
    /// Tham số modifier Mùa (run-flow §Modifier Mùa — MVP chỉ đổi số). Scale % (1–1000): giá mua, giá bán, giá Kính soi,
    /// giá reroll, chỉ số đối thủ, trọng số một trang bị trong bể. Offset: số reroll miễn phí mỗi lượt (0–2).
    /// </summary>
    public enum ModifierParam
    {
        BuyPrice = 0,
        SellPrice = 1,
        /// <summary>Giá mua Kính soi — thay <see cref="BuyPrice"/> cho vật phẩm Kính (spoils D7).</summary>
        LensPrice = 2,
        RerollPrice = 3,
        OpponentStats = 4,
        /// <summary>Trọng số một trang bị trong bể Rèn (cần <c>EquipmentId</c>).</summary>
        ForgeWeight = 5,
        FreeRerolls = 6,
    }
}
