using System.Collections.Generic;
using Crucible.Core.Content;
using Crucible.Core.State;

namespace Crucible.Core.Rules
{
    /// <summary>
    /// Kết quả <see cref="CatalystRunner.EvaluateBrew"/>: phần cộng vào <c>Quality</c> / dòng thuộc tính và danh sách Effect
    /// đã kích hoạt theo thứ tự (catalyst D3). Chưa sửa state — action vàng / tạo nguyên liệu áp ở <see cref="CatalystRunner.ApplyBrewOutcome"/>.
    /// </summary>
    public sealed class CatalystBrewOutcome
    {
        private readonly List<int> _mulPercents = new List<int>();
        private readonly List<StatLine> _statBonuses = new List<StatLine>();
        private readonly List<int> _catalystUids = new List<int>();
        private readonly List<int> _effectIndices = new List<int>();
        private readonly List<CatalystEffect> _effects = new List<CatalystEffect>();

        public long EssenceBonus { get; private set; }
        public long PotencyBonus { get; private set; }
        public IReadOnlyList<int> MulPercents => _mulPercents;
        /// <summary>Theo thứ tự kích hoạt; cùng thuộc tính có thể lặp (<see cref="StatLineRules.ApplyBonuses"/> cộng dồn).</summary>
        public IReadOnlyList<StatLine> StatBonuses => _statBonuses;
        public int ActivationCount => _effects.Count;
        public int CatalystUidAt(int i) => _catalystUids[i];
        public int EffectIndexAt(int i) => _effectIndices[i];
        public CatalystEffect EffectAt(int i) => _effects[i];

        internal void Add(int catalystUid, int effectIndex, CatalystEffect effect)
        {
            _catalystUids.Add(catalystUid);
            _effectIndices.Add(effectIndex);
            _effects.Add(effect);
            switch (effect.Action)
            {
                case EffectActionKind.AddEssence:
                    EssenceBonus += effect.Amount;
                    break;
                case EffectActionKind.AddPotency:
                    PotencyBonus += effect.Amount;
                    break;
                case EffectActionKind.MulPotency:
                    _mulPercents.Add(effect.Amount);
                    break;
                case EffectActionKind.AddStat:
                    _statBonuses.Add(new StatLine(effect.Stat, effect.Amount));
                    break;
            }
        }
    }
}
