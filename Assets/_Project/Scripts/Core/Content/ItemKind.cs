namespace Crucible.Core.Content
{
    /// <summary>Loại vật phẩm (GDD §Content). Reagent để sau MVP. Phế phẩm là một def chung (<see cref="Scrap"/>).</summary>
    public enum ItemKind
    {
        Mineral = 0,
        Additive = 1,
        Lens = 2,
        Catalyst = 3,
        Scrap = 4,
    }
}
