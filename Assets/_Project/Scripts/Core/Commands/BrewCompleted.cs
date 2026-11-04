using Crucible.Core.Logs;

namespace Crucible.Core.Commands
{
    /// <summary>Lượt Nung đã tính xong; kết quả (thành phẩm hoặc phế phẩm) đã vào Túi. Không chứa thông tin công thức.</summary>
    public sealed class BrewCompleted : GameEvent
    {
        public BrewCompleted(int resultUid, BrewLog log)
        {
            ResultUid = resultUid;
            Log = log;
        }

        public int ResultUid { get; }
        /// <summary>Cùng đối tượng với <c>RunState.LastBrew</c>.</summary>
        public BrewLog Log { get; }
    }
}
