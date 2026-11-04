using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>
    /// Tuning Chợ & kinh tế (economy.md §Tuning Knobs, từ RunDef). <see cref="TierWeights"/>: chỉ số = tier − 1, đủ
    /// <see cref="ShopMaxTier"/> phần tử. ContentDb validate mọi khoảng.
    /// </summary>
    public sealed class MarketData
    {
        /// <summary>Số ô hàng tối đa layout Chợ hiển thị được (3×2 — ui-ban-luyen §6): ô nguyên liệu + 1 ô tiện ích.</summary>
        public const int MaxShopSlots = 6;
        /// <summary>Trần hệ số giá Mùa (%) — chặn tràn số khi nhân giá.</summary>
        public const int MaxSeasonPricePercent = 1000;

        public MarketData(int shopMaxTier, int ingredientSlotCount, IReadOnlyList<int> tierWeights,
                          int utilityLensWeight, int utilityCatalystWeight, int seasonPriceMin, int seasonPriceMax,
                          int sellRatePercent, int rerollBase, int rerollStep, int catalystSlots, int scrapSellPrice)
        {
            ShopMaxTier = shopMaxTier;
            IngredientSlotCount = ingredientSlotCount;
            TierWeights = tierWeights;
            UtilityLensWeight = utilityLensWeight;
            UtilityCatalystWeight = utilityCatalystWeight;
            SeasonPriceMin = seasonPriceMin;
            SeasonPriceMax = seasonPriceMax;
            SellRatePercent = sellRatePercent;
            RerollBase = rerollBase;
            RerollStep = rerollStep;
            CatalystSlots = catalystSlots;
            ScrapSellPrice = scrapSellPrice;
        }

        /// <summary>Chợ chỉ bán nguyên liệu gốc tier ≤ giá trị này (mặc định 3).</summary>
        public int ShopMaxTier { get; }
        /// <summary>Số ô nguyên liệu mỗi lần bày (mặc định 4); cộng 1 ô tiện ích.</summary>
        public int IngredientSlotCount { get; }
        /// <summary>Trọng số rút tier cho ô nguyên liệu (mặc định 70/25/5).</summary>
        public IReadOnlyList<int> TierWeights { get; }
        public int UtilityLensWeight { get; }
        public int UtilityCatalystWeight { get; }
        public int SeasonPriceMin { get; }
        public int SeasonPriceMax { get; }
        /// <summary>% giá mua khi người chơi bán (mặc định 75; &lt; 100 để chặn mua-bán sinh lời).</summary>
        public int SellRatePercent { get; }
        public int RerollBase { get; }
        public int RerollStep { get; }
        /// <summary>Số Xúc tác tối đa cùng lúc (mặc định 5).</summary>
        public int CatalystSlots { get; }
        /// <summary>Giá bán cố định của phế phẩm (không nhân hệ số Mùa).</summary>
        public int ScrapSellPrice { get; }

        /// <summary>Tổng số ô hàng mỗi lần bày.</summary>
        public int SlotCount => IngredientSlotCount + 1;
    }
}
