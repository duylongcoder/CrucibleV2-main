namespace Crucible.Core.Content
{
    /// <summary>Lựa chọn bể trang bị đã resolve.</summary>
    public readonly struct ForgeOption
    {
        public ForgeOption(EquipmentId equipment, int weight)
        {
            Equipment = equipment;
            Weight = weight;
        }

        public EquipmentId Equipment { get; }
        public int Weight { get; }
    }
}
