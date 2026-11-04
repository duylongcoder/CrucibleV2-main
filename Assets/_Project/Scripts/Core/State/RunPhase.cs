namespace Crucible.Core.State
{
    /// <summary>Giai đoạn trong một Mùa (TECH §3.2).</summary>
    public enum RunPhase
    {
        Decree = 0,
        Workbench = 1,
        Offering = 2,
        Battle = 3,
        Spoils = 4,
        Ended = 5,
    }
}
