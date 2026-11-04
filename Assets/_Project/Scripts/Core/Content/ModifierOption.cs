namespace Crucible.Core.Content
{
    /// <summary>Mục bể modifier đã resolve.</summary>
    public readonly struct ModifierOption
    {
        public ModifierOption(ModifierId modifier, int weight)
        {
            Modifier = modifier;
            Weight = weight;
        }

        public ModifierId Modifier { get; }
        public int Weight { get; }
    }
}
