using System;

namespace Crucible.Core.Random
{
    /// <summary>Giữ 7 <see cref="Pcg32"/> độc lập, dẫn từ seed run bằng SplitMix64 (TECH §3.4).</summary>
    public sealed class RngStreams
    {
        public const int Count = 7;

        // Salt riêng mỗi stream = (chỉ số + 1) × golden gamma: khác 0 nên seed 0 vẫn cho 7 stream khác nhau.
        private const ulong SaltStep = 0x9E3779B97F4A7C15UL;

        private readonly Pcg32[] _streams = new Pcg32[Count];

        private RngStreams()
        {
        }

        public static RngStreams Create(ulong seed)
        {
            var streams = new RngStreams();
            for (var i = 0; i < Count; i++)
            {
                var mix = seed ^ unchecked((ulong)(i + 1) * SaltStep);
                var streamSeed = SplitMix64.Next(ref mix);
                var sequence = SplitMix64.Next(ref mix);
                streams._streams[i] = new Pcg32(streamSeed, sequence);
            }

            return streams;
        }

        public ref Pcg32 Get(RngStream stream)
        {
            return ref _streams[(int)stream];
        }

        /// <summary>Trạng thái 7 stream theo thứ tự enum <see cref="RngStream"/>.</summary>
        public Pcg32State[] Capture()
        {
            var states = new Pcg32State[Count];
            for (var i = 0; i < Count; i++)
                states[i] = _streams[i].State;
            return states;
        }

        public void Restore(Pcg32State[] states)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));
            if (states.Length != Count)
                throw new ArgumentException($"Expected {Count} stream states, got {states.Length}.", nameof(states));

            for (var i = 0; i < Count; i++)
                _streams[i].State = states[i];
        }
    }
}
