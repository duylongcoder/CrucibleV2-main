namespace Crucible.Core.Content
{
    /// <summary>Dữ liệu vật phẩm bất biến cho luật (Id, Kind, Tier, công thức nếu là thành phẩm).</summary>
    public sealed class ItemData
    {
        public ItemData(string id, ItemKind kind, int tier, RecipeData recipe = null)
        {
            Id = id;
            Kind = kind;
            Tier = tier;
            Recipe = recipe;
        }

        public string Id { get; }
        public ItemKind Kind { get; }
        public int Tier { get; }
        /// <summary>null = nguyên liệu gốc (chỉ từ Chợ / bộ khởi đầu / chiến lợi phẩm).</summary>
        public RecipeData Recipe { get; }
    }
}
