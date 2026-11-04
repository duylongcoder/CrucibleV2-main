namespace Crucible.Core.Content
{
    /// <summary>Dữ liệu vật phẩm bất biến cho luật (phần nền: Id, Kind, Tier).</summary>
    public sealed class ItemData
    {
        public ItemData(string id, ItemKind kind, int tier)
        {
            Id = id;
            Kind = kind;
            Tier = tier;
        }

        public string Id { get; }
        public ItemKind Kind { get; }
        public int Tier { get; }
    }
}
