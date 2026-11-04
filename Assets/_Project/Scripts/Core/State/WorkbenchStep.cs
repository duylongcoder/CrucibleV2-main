namespace Crucible.Core.State
{
    /// <summary>Bước trong một lượt Bàn luyện (TECH §3.2). <see cref="ResultPending"/> = đã Nung, lưới khóa.</summary>
    public enum WorkbenchStep
    {
        Editing = 0,
        ResultPending = 1,
    }
}
