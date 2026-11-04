using System.Collections.Generic;

namespace Crucible.Core.Content
{
    /// <summary>Khai báo một Mùa (theo thứ tự Mùa 1…N): đối thủ cố định + bể trang bị có trọng số.</summary>
    public sealed class SeasonData
    {
        public SeasonData(string opponentId, IReadOnlyList<ForgeOptionData> forgePool)
        {
            OpponentId = opponentId;
            ForgePool = forgePool;
        }

        public string OpponentId { get; }
        public IReadOnlyList<ForgeOptionData> ForgePool { get; }
    }
}
