namespace Crucible.Core.Commands
{
    /// <summary>Đã soi <see cref="TargetUid"/> bằng Kính <see cref="LensUid"/> (đã tiêu hao); <see cref="RevealedMask"/> = chỉ các dòng mới lộ.</summary>
    public sealed class ItemInspected : GameEvent
    {
        public ItemInspected(int targetUid, int lensUid, int revealedMask)
        {
            TargetUid = targetUid;
            LensUid = lensUid;
            RevealedMask = revealedMask;
        }

        public int TargetUid { get; }
        public int LensUid { get; }
        public int RevealedMask { get; }
    }
}
