namespace Crucible.Core.Logs
{
    /// <summary>Chỉ số một bên đầu trận (battle.md §Chỉ số).</summary>
    public readonly struct Combatant
    {
        public Combatant(long hp, long atk, int sharpness, int reflect, int absorb)
        {
            Hp = hp;
            Atk = atk;
            Sharpness = sharpness;
            Reflect = reflect;
            Absorb = absorb;
        }

        public long Hp { get; }
        public long Atk { get; }
        public int Sharpness { get; }
        public int Reflect { get; }
        public int Absorb { get; }
    }
}
