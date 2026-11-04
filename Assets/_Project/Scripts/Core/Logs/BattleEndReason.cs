namespace Crucible.Core.Logs
{
    /// <summary>Cách trận kết thúc.</summary>
    public enum BattleEndReason
    {
        /// <summary>Có người gục.</summary>
        Knockout = 0,
        /// <summary>Hết <c>MaxRounds</c>, phân thắng theo tổng sát thương.</summary>
        RoundLimit = 1,
        /// <summary>Không có gì để dâng — Thua, không vào trận.</summary>
        Forfeit = 2,
    }
}
