namespace Crucible.Core.Commands
{
    /// <summary>Nung lưới hiện tại. <see cref="NowUnix"/> dành cho mở khóa Sổ công thức (F-012).</summary>
    public sealed class Brew : ICommand
    {
        public Brew(long nowUnix)
        {
            NowUnix = nowUnix;
        }

        public long NowUnix { get; }
    }
}
