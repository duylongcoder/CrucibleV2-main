namespace Crucible.Core.Content
{
    /// <summary>Bậc khó (phần nền: chỉ Id).</summary>
    public sealed class DifficultyData
    {
        public DifficultyData(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
