namespace Crucible.Core.Content
{
    /// <summary>Đối thủ cố định của một Mùa (run-flow §Chiếu chỉ; battle.md §Đối thủ). Chỉ số trước hệ số bậc khó.</summary>
    public sealed class OpponentData
    {
        public OpponentData(string id, int stars, BattleKind kind, int hp, int atk, int sharpness, int reflect, int absorb)
        {
            Id = id;
            Stars = stars;
            Kind = kind;
            Hp = hp;
            Atk = atk;
            Sharpness = sharpness;
            Reflect = reflect;
            Absorb = absorb;
        }

        public string Id { get; }
        /// <summary>Mức khó định tính ★1–5 (designer gán).</summary>
        public int Stars { get; }
        public BattleKind Kind { get; }
        public int Hp { get; }
        public int Atk { get; }
        public int Sharpness { get; }
        public int Reflect { get; }
        public int Absorb { get; }
    }
}
