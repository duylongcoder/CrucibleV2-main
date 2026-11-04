namespace Crucible.Core.Content
{
    /// <summary>Action content của Effect (Id chuỗi); <c>ContentDb</c> resolve + validate.</summary>
    public sealed class EffectActionData
    {
        public EffectActionData(EffectActionKind kind, int amount = 0, StatKind stat = default, string itemId = null)
        {
            Kind = kind;
            Amount = amount;
            Stat = stat;
            ItemId = itemId;
        }

        public EffectActionKind Kind { get; }
        /// <summary>Lượng / % theo <see cref="Kind"/>; <c>CreateIngredient</c>: 0.</summary>
        public int Amount { get; }
        /// <summary>Chỉ <c>AddStat</c>.</summary>
        public StatKind Stat { get; }
        /// <summary>Chỉ <c>CreateIngredient</c>: nguyên liệu gốc được tạo.</summary>
        public string ItemId { get; }

        public static EffectActionData AddEssence(int amount) => new EffectActionData(EffectActionKind.AddEssence, amount);
        public static EffectActionData AddPotency(int amount) => new EffectActionData(EffectActionKind.AddPotency, amount);
        public static EffectActionData MulPotency(int percent) => new EffectActionData(EffectActionKind.MulPotency, percent);
        public static EffectActionData AddStat(StatKind stat, int amount) => new EffectActionData(EffectActionKind.AddStat, amount, stat);
        public static EffectActionData GainGold(int amount) => new EffectActionData(EffectActionKind.GainGold, amount);
        public static EffectActionData CreateIngredient(string itemId) => new EffectActionData(EffectActionKind.CreateIngredient, itemId: itemId);
    }
}
