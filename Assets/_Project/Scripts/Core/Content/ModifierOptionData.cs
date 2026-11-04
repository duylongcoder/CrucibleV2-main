namespace Crucible.Core.Content
{
    /// <summary>Một mục trong bể modifier của Mùa (dạng Id chuỗi, trước khi resolve).</summary>
    public sealed class ModifierOptionData
    {
        public ModifierOptionData(string modifierId, int weight)
        {
            ModifierId = modifierId;
            Weight = weight;
        }

        public string ModifierId { get; }
        /// <summary>Trọng số ≥ 0; 0 = không bao giờ ra.</summary>
        public int Weight { get; }
    }
}
