using System;

namespace Crucible.Core.Random
{
    /// <summary>PCG32 (XSH RR 64/32, O'Neill) — PRNG tất định của lõi (ADR-004).</summary>
    public struct Pcg32
    {
        private const ulong Multiplier = 6364136223846793005UL;

        private ulong _state;
        private ulong _inc;

        /// <summary>Tương đương <c>pcg32_srandom_r(seed, sequence)</c> của bản cài đặt tham chiếu.</summary>
        public Pcg32(ulong seed, ulong sequence)
        {
            _state = 0;
            _inc = (sequence << 1) | 1UL;
            NextUInt();
            unchecked { _state += seed; }
            NextUInt();
        }

        public Pcg32State State
        {
            get => new Pcg32State(_state, _inc);
            set
            {
                _state = value.State;
                _inc = value.Inc;
            }
        }

        public uint NextUInt()
        {
            unchecked
            {
                var old = _state;
                _state = old * Multiplier + _inc;
                var xorShifted = (uint)(((old >> 18) ^ old) >> 27);
                var rotation = (int)(old >> 59);
                return (xorShifted >> rotation) | (xorShifted << (-rotation & 31));
            }
        }

        /// <summary>Số nguyên đều trong <c>[0, maxExclusive)</c> — Lemire, có loại bỏ nên không lệch modulo.</summary>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Must be positive.");

            unchecked
            {
                var range = (uint)maxExclusive;
                var product = (ulong)NextUInt() * range;
                var low = (uint)product;
                if (low < range)
                {
                    var threshold = (0u - range) % range;
                    while (low < threshold)
                    {
                        product = (ulong)NextUInt() * range;
                        low = (uint)product;
                    }
                }

                return (int)(product >> 32);
            }
        }

        /// <summary>
        /// true với xác suất <paramref name="percent"/>%. Luôn rút đúng một lần (kể cả 0 / 100) để chuỗi RNG không
        /// phụ thuộc giá trị phần trăm. Không dùng <see cref="NextInt"/> vì vòng loại bỏ có thể rút thêm; độ lệch
        /// của phép nhân-dịch không loại bỏ ≤ 100 / 2^32, không đáng kể.
        /// </summary>
        public bool Roll(int percent)
        {
            if (percent < 0 || percent > 100)
                throw new ArgumentOutOfRangeException(nameof(percent), percent, "Must be in [0, 100].");

            var bucket = (int)(((ulong)NextUInt() * 100UL) >> 32);
            return bucket < percent;
        }
    }
}
