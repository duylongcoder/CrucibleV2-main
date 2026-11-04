namespace Crucible.Core.Commands
{
    /// <summary>
    /// Chuyển một Xúc tác trong Túi tới vị trí <see cref="ToIndex"/> của thứ tự Xúc tác (vị trí trên kệ, 0 = kích hoạt
    /// đầu tiên). Vật phẩm khác trong Túi giữ nguyên chỗ.
    /// </summary>
    public sealed class ReorderCatalyst : ICommand
    {
        public ReorderCatalyst(int uid, int toIndex)
        {
            Uid = uid;
            ToIndex = toIndex;
        }

        public int Uid { get; }
        public int ToIndex { get; }
    }
}
