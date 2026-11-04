using Crucible.Core.Content;

namespace Crucible.Core.State
{
    /// <summary>Một dòng thuộc tính phụ đã sinh trên thành phẩm (ẩn tới khi soi — GDD §6.2).</summary>
    public readonly struct StatLine
    {
        public StatLine(StatKind stat, int value)
        {
            Stat = stat;
            Value = value;
        }

        public StatKind Stat { get; }
        public int Value { get; }
    }
}
