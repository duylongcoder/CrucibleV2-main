using System;
using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Dữ liệu vật phẩm bất biến cho luật (Id, Kind, Tier, công thức nếu là thành phẩm, % ma thuật nếu là phụ gia).</summary>
    public sealed class ItemData
    {
        public ItemData(string id, ItemKind kind, int tier, RecipeData recipe = null, int magicChance = 0,
                        int marketPrice = 0, int revealLines = 0, IReadOnlyList<EffectData> effects = null)
        {
            Effects = effects ?? Array.Empty<EffectData>();
            RevealLines = revealLines;
            Id = id;
            Kind = kind;
            Tier = tier;
            Recipe = recipe;
            MagicChance = magicChance;
            MarketPrice = marketPrice;
        }

        public string Id { get; }
        public ItemKind Kind { get; }
        public int Tier { get; }
        /// <summary>null = nguyên liệu gốc (chỉ từ Chợ / bộ khởi đầu / chiến lợi phẩm).</summary>
        public RecipeData Recipe { get; }
        /// <summary>% ma thuật phụ gia góp khi nằm trong khoảng trống (0–50); 0 với mọi loại khác.</summary>
        public int MagicChance { get; }
        /// <summary>Giá chuẩn trước hệ số Mùa / modifier (economy.md §Formulas); ≥ 0.</summary>
        public int MarketPrice { get; }
        /// <summary>Số dòng một Kính soi lộ (≥ 1 với Kính; 0 với loại khác — GDD §6.2: Kính thường 1, Kính tốt 3).</summary>
        public int RevealLines { get; }
        /// <summary>Effect của Xúc tác (0…<see cref="EffectLimits.MaxEffectsPerCatalyst"/>); loại khác phải rỗng.</summary>
        public IReadOnlyList<EffectData> Effects { get; }
    }
}
