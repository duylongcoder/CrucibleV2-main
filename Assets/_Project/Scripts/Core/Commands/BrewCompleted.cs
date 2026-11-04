namespace Crucible.Core.Commands
{
    /// <summary>Lượt Nung đã tính xong; kết quả (thành phẩm hoặc phế phẩm) đã vào Túi. Không chứa thông tin công thức.</summary>
    public sealed class BrewCompleted : GameEvent
    {
        public BrewCompleted(int resultUid)
        {
            ResultUid = resultUid;
        }

        public int ResultUid { get; }
    }
}
