namespace Crucible.Core.Commands
{
    /// <summary>Dùng Kính <see cref="LensUid"/> soi thành phẩm <see cref="TargetUid"/> (cả hai trong Túi — inspect D3).</summary>
    public sealed class Inspect : ICommand
    {
        public Inspect(int targetUid, int lensUid)
        {
            TargetUid = targetUid;
            LensUid = lensUid;
        }

        public int TargetUid { get; }
        public int LensUid { get; }
    }
}
