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

        /// <summary><c>⌊buyPrice × sellRatePercent × modifierPercent / 10000⌋</c>.</summary>
        public static int Sell(int buyPrice, int sellRatePercent, int modifierPercent = 100)
        {
            RequireNonNegative(buyPrice, nameof(buyPrice));
            RequireNonNegative(sellRatePercent, nameof(sellRatePercent));
            RequireNonNegative(modifierPercent, nameof(modifierPercent));

            var price = (long)buyPrice * sellRatePercent * modifierPercent / 10000;
            return (int)Math.Min(price, int.MaxValue);
        }

        /// <summary>
        /// Lần thứ <paramref name="rerollCount"/> (từ 0): 0 khi còn trong <paramref name="freeRerolls"/>, ngược lại
        /// <c>⌊(rerollBase + (rerollCount − freeRerolls) × rerollStep) × modifierPercent / 100⌋</c> (spoils D8).
        /// </summary>
        public static int Reroll(int rerollBase, int rerollStep, int rerollCount, int freeRerolls = 0,
                                 int modifierPercent = 100)
        {
            RequireNonNegative(rerollBase, nameof(rerollBase));
            RequireNonNegative(rerollStep, nameof(rerollStep));
            RequireNonNegative(rerollCount, nameof(rerollCount));
            RequireNonNegative(freeRerolls, nameof(freeRerolls));
            RequireNonNegative(modifierPercent, nameof(modifierPercent));

            if (rerollCount < freeRerolls)
                return 0;
            var price = Math.Min(rerollBase + (long)(rerollCount - freeRerolls) * rerollStep, int.MaxValue);
            return (int)Math.Min(price * modifierPercent / 100, int.MaxValue);
        }

        private static void RequireNonNegative(int value, string name)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(name, value, "Must not be negative.");
        }
    }
}
