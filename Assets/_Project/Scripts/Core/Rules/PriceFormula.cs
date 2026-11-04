using System;

namespace Crucible.Core.Rules
{
    /// <summary>Công thức giá thuần (economy.md §Formulas). Tham số âm là lỗi lập trình.</summary>
    public static class PriceFormula
    {
        /// <summary><c>max(1, ⌊marketPrice × seasonPercent × modifierPercent / 10000⌋)</c>.</summary>
        public static int Buy(int marketPrice, int seasonPercent, int modifierPercent)
        {
            RequireNonNegative(marketPrice, nameof(marketPrice));
            RequireNonNegative(seasonPercent, nameof(seasonPercent));
            RequireNonNegative(modifierPercent, nameof(modifierPercent));

            var price = (long)marketPrice * seasonPercent * modifierPercent / 10000;
            return (int)Math.Max(1L, Math.Min(price, int.MaxValue));
        }

        /// <summary><c>max(0, ⌊buyPrice × sellRatePercent / 100⌋)</c>.</summary>
        public static int Sell(int buyPrice, int sellRatePercent)
        {
            RequireNonNegative(buyPrice, nameof(buyPrice));
            RequireNonNegative(sellRatePercent, nameof(sellRatePercent));

            return (int)((long)buyPrice * sellRatePercent / 100);
        }

        /// <summary><c>rerollBase + rerollCount × rerollStep</c> (rerollCount từ 0).</summary>
        public static int Reroll(int rerollBase, int rerollStep, int rerollCount)
        {
            RequireNonNegative(rerollBase, nameof(rerollBase));
            RequireNonNegative(rerollStep, nameof(rerollStep));
            RequireNonNegative(rerollCount, nameof(rerollCount));

            return (int)Math.Min(rerollBase + (long)rerollCount * rerollStep, int.MaxValue);
        }

        private static void RequireNonNegative(int value, string name)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(name, value, "Must not be negative.");
        }
    }
}
