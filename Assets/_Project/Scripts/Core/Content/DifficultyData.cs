namespace Crucible.Core.Content
{
    /// <summary>Bậc khó: Id + hệ số chỉ số đối thủ.</summary>
    public sealed class DifficultyData
    {
        public DifficultyData(string id, int opponentStatPercent = 100)
        {
            Id = id;
            OpponentStatPercent = opponentStatPercent;
        }

        public string Id { get; }
        /// <summary>% nhân mọi chỉ số đối thủ (1–1000; 100 = nguyên def).</summary>
        public int OpponentStatPercent { get; }
    }
}
