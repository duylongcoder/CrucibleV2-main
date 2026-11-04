namespace Crucible.Core.State
{
    /// <summary>State giữa các run (TECH §3.2). Phần nền: Danh vọng.</summary>
    public sealed class MetaState
    {
        public int Renown { get; set; }
        /// <summary>Timescale phát lại ưa thích 1–3 (GDD §6.3).</summary>
        public int Timescale { get; set; } = 1;
    }
}
