using System;

namespace Crucible.Core.Content
{
    /// <summary>Chỉ số phần thưởng trong <see cref="ContentDb"/> — gán theo thứ tự sắp ordinal của Id chuỗi.</summary>
    public readonly struct RewardId : IEquatable<RewardId>
    {
        public RewardId(int index)
        {
            Index = index;
        }

        public int Index { get; }

        public bool Equals(RewardId other) => Index == other.Index;
        public override bool Equals(object obj) => obj is RewardId other && Equals(other);
        public override int GetHashCode() => Index;
        public override string ToString() => $"RewardId({Index})";

        public static bool operator ==(RewardId left, RewardId right) => left.Equals(right);
        public static bool operator !=(RewardId left, RewardId right) => !left.Equals(right);
    }
}
