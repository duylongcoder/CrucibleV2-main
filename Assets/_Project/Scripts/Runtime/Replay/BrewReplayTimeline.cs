using System;
using Crucible.Core.Rules;

namespace Crucible.Runtime.Replay
{
    /// <summary>
    /// Đồng hồ phát lại một <c>BrewLog</c> (brewing.md §Phát lại) — C# thuần, view gọi <see cref="Advance"/> mỗi frame.
    /// Phát mọi nhịp theo thứ tự, mỗi nhịp <c>tickSeconds / timescale</c>; chỉ xong khi nhịp cuối chạy hết.
    /// Tua (<see cref="Skip"/>) chỉ là hiển thị: kết quả đã tính + lưu lúc Nung.
    /// float chỉ quyết định khung hình nào đổi nhịp — không vào state / hash / save (ADR-004 áp cho Core).
    /// </summary>
    public sealed class BrewReplayTimeline
    {
        private readonly float _tickSeconds;
        // Tiến độ theo đơn vị nhịp → đổi timescale giữa chừng không làm nhảy nhịp.
        private float _progressTicks;

        public BrewReplayTimeline(int tickCount, float tickSeconds, int timescale)
        {
            if (tickCount < 1) throw new ArgumentOutOfRangeException(nameof(tickCount), tickCount, "Need at least one tick.");
            if (!(tickSeconds > 0f)) throw new ArgumentOutOfRangeException(nameof(tickSeconds), tickSeconds, "Must be positive.");
            ValidateTimescale(timescale);

            TickCount = tickCount;
            _tickSeconds = tickSeconds;
            Timescale = timescale;
        }

        public int TickCount { get; }
        /// <summary>0…TickCount−1; giữ nhịp cuối khi đã xong.</summary>
        public int CurrentTick => Math.Min((int)_progressTicks, TickCount - 1);
        /// <summary>true = được hiện kết quả.</summary>
        public bool IsFinished => _progressTicks >= TickCount;
        public int Timescale { get; private set; }

        /// <summary>Đổi tốc độ, giữ nguyên tiến độ.</summary>
        public void SetTimescale(int timescale)
        {
            ValidateTimescale(timescale);
            Timescale = timescale;
        }

        public void Advance(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Must not be negative.");
            if (IsFinished)
                return;

            _progressTicks = Math.Min(TickCount, _progressTicks + deltaSeconds * Timescale / _tickSeconds);
        }

        public void Skip()
        {
            _progressTicks = TickCount;
        }

        public static float DurationSeconds(int tickCount, float tickSeconds, int timescale) =>
            tickCount * tickSeconds / timescale;

        private static void ValidateTimescale(int timescale)
        {
            if (timescale < SettingsRules.MinTimescale || timescale > SettingsRules.MaxTimescale)
                throw new ArgumentOutOfRangeException(nameof(timescale), timescale, "Timescale must be 1–3.");
        }
    }
}
