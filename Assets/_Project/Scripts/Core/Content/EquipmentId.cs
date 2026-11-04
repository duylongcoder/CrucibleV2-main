using System;

namespace Crucible.Core.Content
{
    /// <summary>Chỉ số trang bị trong <see cref="ContentDb"/> — gán theo thứ tự sắp ordinal của Id chuỗi.</summary>
    public readonly struct EquipmentId : IEquatable<EquipmentId>
    {
        public EquipmentId(int index)
        {
            Index = index;
        }

        public int Index { get; }

        public bool Equals(EquipmentId other) => Index == other.Index;
        public override bool Equals(object obj) => obj is EquipmentId other && Equals(other);
        public override int GetHashCode() => Index;
        public override string ToString() => $"EquipmentId({Index})";

        public static bool operator ==(EquipmentId left, EquipmentId right) => left.Equals(right);
        public static bool operator !=(EquipmentId left, EquipmentId right) => !left.Equals(right);
    }
}
