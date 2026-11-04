namespace Crucible.Core.Commands
{
    /// <summary>Lý do Command bị từ chối.</summary>
    public enum RejectReason
    {
        None = 0,
        UnknownCommand = 1,
        UnknownContent = 2,
        RunAlreadyActive = 3,
        RunNotActive = 4,
        WrongPhase = 5,
        GridLocked = 6,
        InvalidCell = 7,
        ItemNotFound = 8,
        ItemNotPlaceable = 9,
        CellEmpty = 10,
        EmptyGrid = 11,
    }
}
