namespace Crucible.Core.Random
{
    /// <summary>Trạng thái thô của một <see cref="Pcg32"/> — để chụp / khôi phục (save).</summary>
    public struct Pcg32State
    {
        public ulong State;
        public ulong Inc;

        public Pcg32State(ulong state, ulong inc)
        {
            State = state;
            Inc = inc;
        }
    }
}
