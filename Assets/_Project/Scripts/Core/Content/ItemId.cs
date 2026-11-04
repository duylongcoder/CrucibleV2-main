using System;

namespace Crucible.Core.Content
{
    /// <summary>Chỉ số vật phẩm trong <see cref="ContentDb"/> — gán theo thứ tự sắp ordinal của Id chuỗi.</summary>
    public readonly struct ItemId : IEquatable<ItemId>
    {
        public ItemId(int index)
        {
            Index = index;
        }

        public int Index { get; }

        public bool Equals(ItemId other) => Index == other.Index;
        public override bool Equals(object obj) => obj is ItemId other && Equals(other);
        public override int GetHashCode() => Index;
        public override string ToString() => $"ItemId({Index})";

        public static bool operator ==(ItemId left, ItemId right) => left.Equals(right);
        public static bool operator !=(ItemId left, ItemId right) => !left.Equals(right);
    }
}
