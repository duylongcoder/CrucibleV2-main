namespace Crucible.Core.Logs
{
    /// <summary>Một hiệp: ta đánh trước, đối thủ đánh nếu cả hai còn đứng. HP sau hiệp chặn dưới 0.</summary>
    public readonly struct BattleRound
    {
        public BattleRound(long heroDamage, long reflectToHero, bool opponentAttacked, long opponentDamage,
                           long reflectToOpponent, long heroHpAfter, long opponentHpAfter)
        {
            HeroDamage = heroDamage;
            ReflectToHero = reflectToHero;
            OpponentAttacked = opponentAttacked;
            OpponentDamage = opponentDamage;
            ReflectToOpponent = reflectToOpponent;
            HeroHpAfter = heroHpAfter;
            OpponentHpAfter = opponentHpAfter;
        }

        /// <summary>Sát thương đòn của ta lên đối thủ.</summary>
        public long HeroDamage { get; }
        /// <summary>Dội ngược ta nhận từ Phản đòn của đối thủ.</summary>
        public long ReflectToHero { get; }
        public bool OpponentAttacked { get; }
        /// <summary>Sát thương đòn của đối thủ lên ta (0 khi không đánh).</summary>
        public long OpponentDamage { get; }
        /// <summary>Dội ngược đối thủ nhận từ Phản đòn của ta.</summary>
        public long ReflectToOpponent { get; }
        public long HeroHpAfter { get; }
        public long OpponentHpAfter { get; }
    }
}
