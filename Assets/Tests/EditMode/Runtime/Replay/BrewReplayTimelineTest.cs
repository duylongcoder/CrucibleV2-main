using System;
using System.Collections.Generic;
using Crucible.Runtime.Replay;
using NUnit.Framework;

namespace Crucible.Tests.Runtime.Replay
{
    public class BrewReplayTimelineTest
    {
        // Bội số nhị phân chính xác → biên không lệch do sai số float.
        private const float TickSeconds = 2f;

        [TestCase(1, 27f)]
        [TestCase(2, 13.5f)]
        [TestCase(3, 9f)]
        public void DurationSeconds_FullGrid_MatchesFormula(int timescale, float expected)
        {
            Assert.That(BrewReplayTimeline.DurationSeconds(9, 3f, timescale), Is.EqualTo(expected).Within(1e-4f));
        }

        [Test]
        public void New_Timeline_AtFirstTickNotFinished()
        {
            var timeline = new BrewReplayTimeline(3, TickSeconds, 1);

            Assert.That(timeline.CurrentTick, Is.EqualTo(0));
            Assert.That(timeline.IsFinished, Is.False);
        }

        [Test]
        public void Advance_LessThanOneTick_StaysOnFirstTick()
        {
            var timeline = new BrewReplayTimeline(3, TickSeconds, 1);

            timeline.Advance(1.5f);

            Assert.That(timeline.CurrentTick, Is.EqualTo(0));
        }

        [Test]
        public void Advance_OneTickAtATime_VisitsEveryTickInOrder()
        {
            var timeline = new BrewReplayTimeline(4, TickSeconds, 1);
            var visited = new List<int>();

            for (var i = 0; i < 4; i++)
            {
                visited.Add(timeline.CurrentTick);
                timeline.Advance(TickSeconds);
            }

            Assert.That(visited, Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void Advance_JustBeforeEnd_NotFinishedOnLastTick()
        {
            var timeline = new BrewReplayTimeline(3, TickSeconds, 1);

            timeline.Advance(5.5f);

            Assert.That(timeline.IsFinished, Is.False);
            Assert.That(timeline.CurrentTick, Is.EqualTo(2));
        }

        [Test]
        public void Advance_ExactlyFullDuration_Finished()
        {
            var timeline = new BrewReplayTimeline(3, TickSeconds, 1);

            timeline.Advance(6f);

            Assert.That(timeline.IsFinished, Is.True);
        }

        [Test]
        public void Advance_PastEnd_StaysFinishedOnLastTick()
        {
            var timeline = new BrewReplayTimeline(3, TickSeconds, 1);

            timeline.Advance(100f);

            Assert.That(timeline.IsFinished, Is.True);
            Assert.That(timeline.CurrentTick, Is.EqualTo(2));
        }

        [Test]
        public void Advance_Timescale3_FinishesInOneThirdOfDuration()
        {
            var timeline = new BrewReplayTimeline(9, 3f, 3);

            timeline.Advance(9f);

            Assert.That(timeline.IsFinished, Is.True);
        }

        [Test]
        public void Advance_Timescale3_NotFinishedBeforeOneThirdOfDuration()
        {
            var timeline = new BrewReplayTimeline(9, 3f, 3);

            timeline.Advance(8.5f);

            Assert.That(timeline.IsFinished, Is.False);
        }

        [Test]
        public void SetTimescale_MidReplay_KeepsCurrentTick()
        {
            var timeline = new BrewReplayTimeline(4, TickSeconds, 1);
            timeline.Advance(3f);

            timeline.SetTimescale(2);

            Assert.That(timeline.CurrentTick, Is.EqualTo(1));
            Assert.That(timeline.Timescale, Is.EqualTo(2));
        }

        [Test]
        public void SetTimescale_MidReplay_RemainingTicksPlayAtNewRate()
        {
            // 4 nhịp × 2 s; sau 3 s ở 1× còn 2.5 nhịp → ở 2× còn 2.5 s.
            var timeline = new BrewReplayTimeline(4, TickSeconds, 1);
            timeline.Advance(3f);
            timeline.SetTimescale(2);

            timeline.Advance(2.25f);
            var finishedEarly = timeline.IsFinished;
            timeline.Advance(0.25f);

            Assert.That(finishedEarly, Is.False);
            Assert.That(timeline.IsFinished, Is.True);
        }

        [Test]
        public void Skip_MidReplay_FinishedImmediately()
        {
            var timeline = new BrewReplayTimeline(9, TickSeconds, 1);
            timeline.Advance(3f);

            timeline.Skip();

            Assert.That(timeline.IsFinished, Is.True);
        }

        [Test]
        public void Skip_BeforeAdvance_Finished()
        {
            var timeline = new BrewReplayTimeline(9, TickSeconds, 1);

            timeline.Skip();

            Assert.That(timeline.IsFinished, Is.True);
        }

        [TestCase(0, 2f, 1)]
        [TestCase(3, 0f, 1)]
        [TestCase(3, 2f, 0)]
        [TestCase(3, 2f, 4)]
        public void Constructor_InvalidArguments_Throws(int tickCount, float tickSeconds, int timescale)
        {
            Assert.That(() => new BrewReplayTimeline(tickCount, tickSeconds, timescale),
                Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Advance_NegativeDelta_Throws()
        {
            var timeline = new BrewReplayTimeline(3, TickSeconds, 1);

            Assert.That(() => timeline.Advance(-0.1f), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [TestCase(0)]
        [TestCase(4)]
        public void SetTimescale_OutOfRange_Throws(int timescale)
        {
            var timeline = new BrewReplayTimeline(3, TickSeconds, 1);

            Assert.That(() => timeline.SetTimescale(timescale), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }
}
