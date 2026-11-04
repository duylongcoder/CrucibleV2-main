namespace Crucible.Core.Commands
{
    /// <summary>Nhận lựa chọn thứ <see cref="Index"/> trong <c>RunState.SpoilsOptions</c> rồi sang Mùa kế (spoils R5).</summary>
    public sealed class ChooseReward : ICommand
    {
        public ChooseReward(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }
}
