namespace Crucible.Core.Content
{
    /// <summary>Một tham số của modifier (dạng Id chuỗi, trước khi resolve).</summary>
    public sealed class ModifierParamData
    {
        public ModifierParamData(ModifierParam param, int value, string equipmentId = null)
        {
            Param = param;
            Value = value;
            EquipmentId = equipmentId;
        }

        public ModifierParam Param { get; }
        /// <summary>% (1–1000) với tham số Scale; số reroll (0–2) với <see cref="ModifierParam.FreeRerolls"/>.</summary>
        public int Value { get; }
        /// <summary>Chỉ dùng với <see cref="ModifierParam.ForgeWeight"/>; null với tham số khác.</summary>
        public string EquipmentId { get; }
    }
}
