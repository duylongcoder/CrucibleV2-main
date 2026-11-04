namespace Crucible.Core.Content
{
    /// <summary>Modifier Mùa đã resolve, tra O(1). Tham số không khai báo = trung tính (100 %, 0 reroll miễn phí).</summary>
    public sealed class SeasonModifier
    {
        public const int NeutralPercent = 100;

        /// <summary>Không modifier: mọi % = 100, không reroll miễn phí.</summary>
        public static readonly SeasonModifier Neutral = new SeasonModifier(null, NeutralPercent, NeutralPercent,
            NeutralPercent, NeutralPercent, NeutralPercent, 0, null);

        private readonly int[] _forgeWeightPercents;

        /// <param name="forgeWeightPercents">Theo chỉ số <see cref="EquipmentId"/>; null = 100 % cho mọi trang bị.</param>
        public SeasonModifier(string id, int buyPercent, int sellPercent, int lensPercent, int rerollPercent,
                              int opponentStatPercent, int freeRerolls, int[] forgeWeightPercents)
        {
            Id = id;
            BuyPercent = buyPercent;
            SellPercent = sellPercent;
            LensPercent = lensPercent;
            RerollPercent = rerollPercent;
            OpponentStatPercent = opponentStatPercent;
            FreeRerolls = freeRerolls;
            _forgeWeightPercents = forgeWeightPercents;
        }

        /// <summary>Id chuỗi (save lưu Id); null với <see cref="Neutral"/>.</summary>
        public string Id { get; }
        public int BuyPercent { get; }
        public int SellPercent { get; }
        public int LensPercent { get; }
        public int RerollPercent { get; }
        public int OpponentStatPercent { get; }
        public int FreeRerolls { get; }

        public int ForgeWeightPercent(EquipmentId equipment) =>
            _forgeWeightPercents == null ? NeutralPercent : _forgeWeightPercents[equipment.Index];
    }
}
