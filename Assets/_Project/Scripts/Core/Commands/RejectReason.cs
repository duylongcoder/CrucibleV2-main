namespace Crucible.Core.Commands
{
    /// <summary>Lý do Command bị từ chối.</summary>
    public enum RejectReason
    {
        None = 0,
        UnknownCommand = 1,
        UnknownContent = 2,
        RunAlreadyActive = 3,
    }
}
