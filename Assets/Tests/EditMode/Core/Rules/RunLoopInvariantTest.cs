using System;
using System.Collections.Generic;
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

        [Test]
        public void Apply_RandomCommandsThroughBattle_KeepsResolutionInvariants([Values(1, 2, 3, 4, 5)] int seed)
        {
            var run = StartRun((ulong)seed);
            var random = new System.Random(seed);
            var battlesPerSeason = new int[SeasonCount + 1];
            var acksPerSeason = new int[SeasonCount + 1];

            for (var step = 0; step < MaxStepsToResolution && !IsResolved(run); step++)
            {
                var before = StateHasher.Hash(run, _meta);
                var reputation = run.Reputation;
                var season = run.Season;
                var command = NextSeasonCommand(random, run);
                var result = Apply(run, command);
                if (!result.Accepted)
                {
                    Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before), $"step {step}: rejected changed state");
                    continue;
                }

                foreach (var e in result.Events)
                {
                    if (e is BattleStarted started)
                    {
                        battlesPerSeason[started.Season]++;
                        Assert.That(started.Log.Equipment, Is.EqualTo(run.DecreeEquipment), $"step {step}: forged equipment");
                    }
                }

                if (command is AckBattle)
                    acksPerSeason[season]++;
                Assert.That(run.Reputation, Is.LessThanOrEqualTo(reputation), $"step {step}: reputation increased");
                Assert.That(battlesPerSeason[season], Is.LessThanOrEqualTo(1), $"step {step}: battles in season {season}");
                Assert.That(acksPerSeason[season], Is.LessThanOrEqualTo(1), $"step {step}: acks in season {season}");
                if (run.Phase == RunPhase.Workbench)
                    Assert.That(run.Turn, Is.InRange(1, TurnsPerSeason), $"step {step}");
                else
                    Assert.That(run.Turn, Is.EqualTo(0), $"step {step}");
                Assert.That(BagRules.UsedSlots(run), Is.LessThanOrEqualTo(run.BagCapacity), $"step {step}");
            }
        }

        [Test]
        public void Apply_RandomCommandsFromStartRun_AlwaysReachesSpoilsOrEnded()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var run = StartRun((ulong)seed);
                var random = new System.Random(seed);

                for (var step = 0; step < MaxStepsToResolution && !IsResolved(run); step++)
                    Apply(run, NextSeasonCommand(random, run));

                Assert.That(IsResolved(run), Is.True, $"seed {seed}: stuck in {run.Phase}, season {run.Season}");
            }
        }

        [Test]
        public void Spoils_EveryOfferedOptionIsClaimable()
        {
            var content = CreateWithSpoils();
            for (ulong seed = 0; seed < 1000; seed++)
            {
                var optionCount = WonSpoilsRun(content, seed).SpoilsOptions.Count;
                for (var i = 0; i < optionCount; i++)
                {
                    var run = WonSpoilsRun(content, seed);
                    var result = GameRules.Apply(run, _meta, content, new ChooseReward(i));

                    Assert.That(result.Accepted, Is.True, $"seed {seed}, option {i}: {result.Reason}");
                    Assert.That(BagRules.UsedSlots(run), Is.LessThanOrEqualTo(run.BagCapacity), $"seed {seed}");
                    Assert.That(BagRules.CountCatalysts(run, content), Is.LessThanOrEqualTo(CatalystSlots), $"seed {seed}");
                    Assert.That(run.BagCapacity, Is.LessThanOrEqualTo(MaxBagCapacity), $"seed {seed}");
                }
            }
        }

        /// <summary>Thắng Mùa 1…7 (theo seed) với Túi ngẫu nhiên: đầy/không, 0–5 Xúc tác, sức chứa tới trần → đã vào Spoils.</summary>
        private RunState WonSpoilsRun(ContentDb content, ulong seed)
        {
            var random = new System.Random((int)seed);
            var catalysts = new[] { Bellows, Mortar, Tongs };
            var bag = new List<string>();
            var catalystCount = random.Next(CatalystSlots + 1);
            for (var i = 0; i < catalystCount; i++)
                bag.Add(catalysts[random.Next(catalysts.Length)]);
            var capacity = random.Next(System.Math.Max(1, bag.Count), MaxBagCapacity + 1);
            var fill = random.Next(capacity - bag.Count + 1);
            for (var i = 0; i < fill; i++)
                bag.Add(Iron);

            var run = StateFactory.CreateWonBattleRun(content, 1 + random.Next(SeasonCount - 1), bag.ToArray());
            run.BagCapacity = capacity;
            run.Rng = Crucible.Core.Random.RngStreams.Create(seed);
            Assert.That(GameRules.Apply(run, _meta, content, new AckBattle()).Accepted, Is.True, "arrange: ack");
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Spoils), "arrange: spoils");
            return run;
        }

        [Test]
        public void Apply_RandomCommandsWithInspect_RevealedLinesOnlyGrow([Values(1, 2, 3, 4, 5)] int seed)
        {
            var content = ContentFactory.Create(steelStatLines: new[]
            {
                new StatLineData(StatKind.Sharpness, 70, 1, 9), new StatLineData(StatKind.Absorb, 50, 1, 9),
            });
            var run = new RunState();
            Assert.That(GameRules.Apply(run, _meta, content, new StartRun(Difficulty, StarterKit, (ulong)seed, NowUnix)).Accepted,
                Is.True, "arrange: start");
            for (var i = 0; i < 3; i++)
                StateFactory.AddItem(run, content, Lens);
            run.Bag.Add(new ItemInstance(run.NextUid++, IdOf(content, Coal)));
            run.Bag.Add(new ItemInstance(run.NextUid++, IdOf(content, Coal)));
            run.Gold = StartGold;
            var random = new System.Random(seed);
            var masks = new Dictionary<int, int>();
            var inspected = 0;

            for (var step = 0; step < MaxSteps && run.Phase != RunPhase.Offering; step++)
            {
                var before = StateHasher.Hash(run, _meta);
                var command = random.Next(4) == 0
                    ? new Inspect(RandomUid(random, run), RandomUid(random, run))
                    : NextCommand(random, run);
                var result = GameRules.Apply(run, _meta, content, command);
                if (!result.Accepted)
                    Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before), $"step {step}: rejected changed state");
                else if (command is Inspect)
                    inspected++;

                foreach (var item in run.Bag)
                {
                    if (masks.TryGetValue(item.Uid, out var previous))
                        Assert.That(item.RevealedMask & previous, Is.EqualTo(previous), $"step {step}: uid {item.Uid} lost a line");
                    Assert.That(item.RevealedMask >> InspectRules.LineCount(item), Is.EqualTo(0), $"step {step}: mask beyond lines");
                    masks[item.Uid] = item.RevealedMask;
                }
            }

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Offering), "reached offering");
        }

        private const int MaxStepsToResolution = 20000;

        private static bool IsResolved(RunState run) => run.Phase == RunPhase.Spoils || run.Phase == RunPhase.Ended;

        /// <summary>Lệnh của <see cref="NextCommand"/> + Dâng (2/18), ra về (1/18), xem xong Trận (1/18).</summary>
        private static ICommand NextSeasonCommand(System.Random random, RunState run)
        {
            switch (random.Next(18))
            {
                case 14:
                case 15: return new Offer(RandomUid(random, run));
                case 16: return new DismissCourtier();
                case 17: return new AckBattle();
                default: return NextCommand(random, run);
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
