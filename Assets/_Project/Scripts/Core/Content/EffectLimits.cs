namespace Crucible.Core.Content
{
    /// <summary>Khoảng hợp lệ của Effect (catalyst D2, D8) — chặn content sai / tràn số; số cân bằng chốt ở <c>balance</c>.</summary>
    public static class EffectLimits
    {
        public const int MaxEffectsPerCatalyst = 3;
        public const int MaxConditionsPerEffect = 3;
        public const int MaxAddEssence = 1000;
        public const int MaxAddPotency = 100;
        /// <summary>100 % vô nghĩa → tối thiểu 101.</summary>
        public const int MinMulPotency = 101;
        /// <summary>= <c>QualityFormula.MaxMulPercent</c>.</summary>
        public const int MaxMulPotency = 500;
        public const int MaxGainGold = 1000;
        public const int MinAdditiveCount = 1;
        /// <summary>Lưới 9 ô, slot đầu/cuối công thức là nguyên liệu → tối đa 8 phụ gia có nghĩa.</summary>
        public const int MaxAdditiveCount = 8;
        public const int MinChance = 1;
        public const int MaxChance = 100;
    }
}
