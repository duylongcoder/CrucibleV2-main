using System;

namespace Crucible.Core.Content
{
    /// <summary>Chỉ số modifier trong <see cref="ContentDb"/> — gán theo thứ tự sắp ordinal của Id chuỗi.</summary>
    public readonly struct ModifierId : IEquatable<ModifierId>
    {
        public ModifierId(int index)
        {
            Index = index;
        }

        public int Index { get; }

        public bool Equals(ModifierId other) => Index == other.Index;
        public override bool Equals(object obj) => obj is ModifierId other && Equals(other);
        public override int GetHashCode() => Index;
        public override string ToString() => $"ModifierId({Index})";

        public static bool operator ==(ModifierId left, ModifierId right) => left.Equals(right);
        public static bool operator !=(ModifierId left, ModifierId right) => !left.Equals(right);
    }
}
