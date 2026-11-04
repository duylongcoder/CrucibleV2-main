using System;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary>Bất biến vòng lượt qua lệnh ngẫu nhiên từ <see cref="StartRun"/> (run-loop R7, R9, R10; GDD §6.1 AC-1).</summary>
    public class RunLoopInvariantTest
    {
        private const int MaxSteps = 3000;
        private const int StartGold = 30;
        private const long NowUnix = 1793750400;

        private ContentDb _content;
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
        }

        [Test]
        public void Apply_RandomCommandsFromStartRun_KeepsLoopInvariants([Values(1, 2, 3, 4, 5)] int seed)
        {
            var run = StartRun((ulong)seed);
            var random = new System.Random(seed);
            var turnsStarted = 0;

            for (var step = 0; step < MaxSteps && run.Phase != RunPhase.Offering; step++)
            {
                var before = StateHasher.Hash(run, _meta);
                var result = Apply(run, NextCommand(random, run));
                if (result.Accepted)
                    turnsStarted += CountTurnStarted(result);
                else
                    Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before), $"step {step}: rejected changed state");

                if (run.Phase == RunPhase.Workbench)
                    Assert.That(run.Turn, Is.InRange(1, TurnsPerSeason), $"step {step}");
                else
                    Assert.That(run.Turn, Is.EqualTo(0), $"step {step}");
                Assert.That(turnsStarted, Is.LessThanOrEqualTo(TurnsPerSeason), $"step {step}");
                Assert.That(run.Gold, Is.GreaterThanOrEqualTo(0), $"step {step}");
                Assert.That(BagRules.UsedSlots(run), Is.LessThanOrEqualTo(run.BagCapacity), $"step {step}");
            }
        }

        [Test]
        public void Apply_RandomCommandsFromStartRun_AlwaysReachesOffering()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var run = StartRun((ulong)seed);
                var random = new System.Random(seed);
                var turnsStarted = 0;

                for (var step = 0; step < MaxSteps && run.Phase != RunPhase.Offering; step++)
                {
                    var result = Apply(run, NextCommand(random, run));
                    if (result.Accepted)
                        turnsStarted += CountTurnStarted(result);
                }

                Assert.That(run.Phase, Is.EqualTo(RunPhase.Offering), $"seed {seed}");
                Assert.That(turnsStarted, Is.EqualTo(TurnsPerSeason), $"seed {seed}");
            }
        }

        [Test]
        public void Apply_SameSeedSameCommands_SameHashEveryStep()
        {
            var first = StartRun(7);
            var second = StartRun(7);
            var random = new System.Random(7);

            for (var step = 0; step < 500; step++)
            {
                var command = NextCommand(random, first);
                Apply(first, command);
                Apply(second, command);

                Assert.That(StateHasher.Hash(second, _meta), Is.EqualTo(StateHasher.Hash(first, _meta)), $"step {step}");
            }
        }

        private RunState StartRun(ulong seed)
        {
            var run = new RunState();
            Assert.That(Apply(run, new StartRun(Difficulty, StarterKit, seed, NowUnix)).Accepted, Is.True);
            run.Gold = StartGold;
            return run;
        }

        private CommandResult Apply(RunState run, ICommand command) => GameRules.Apply(run, _meta, _content, command);

        private static int CountTurnStarted(CommandResult result)
        {
            var count = 0;
            foreach (var e in result.Events)
            {
                if (e is TurnStarted)
                    count++;
            }

            return count;
        }

        /// <summary>Trọng số 14: lệnh vòng (Accept 1, Skip 1, Ack 2) = 4/14 ≥ 1/4; tham số có giá trị sai để sinh lệnh bị từ chối.</summary>
        private static ICommand NextCommand(System.Random random, RunState run)
        {
            switch (random.Next(14))
            {
                case 0: return new AcceptDecree();
                case 1: return new SkipTurn();
                case 2:
                case 3: return new AckBrewResult();
                case 4:
                case 5: return new Brew(NowUnix);
                case 6:
                case 7: return new Buy(random.Next(-1, 7), random.Next(3) == 0 ? random.Next(-1, 10) : (int?)null);
                case 8: return new Sell(RandomUid(random, run));
                case 9: return new Reroll();
                case 10:
                case 11:
                case 12: return new PlaceItem(RandomUid(random, run), random.Next(-1, GridRules.CellCount + 1));
                case 13: return new RemoveFromGrid(random.Next(-1, GridRules.CellCount + 1));
                default: throw new InvalidOperationException();
            }
        }

        private static int RandomUid(System.Random random, RunState run) => random.Next(0, run.NextUid + 2);
    }
}
