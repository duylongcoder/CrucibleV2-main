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
    /// <summary>
    /// RandomAgent trọn run tới RunEnd (full-run R4–R9; run-flow AC-1/2/3/5/7; GDD §6.1 AC-1 Editor; economy AC-1/AC-4).
    /// Mỗi hồ sơ 500 seed: chuẩn (đối thủ 100 %) phủ Trục xuất, dễ (<see cref="EasyOpponentStatPercent"/>) phủ Thắng tới Mùa 8.
    /// </summary>
    public class FullRunInvariantTest
    {
        private const int RunsPerProfile = 500;
        private const int ReplayRuns = 1000;
        private const int MaxCommandsPerRun = 5000;
        private const int StandardProfile = 100;
        /// <summary>Gợi ý công thức của agent ở hồ sơ dễ — 25 % mặc định chỉ cho ~1/500 run Thắng (Implementation Notes).</summary>
        private const int EasyRecipeHintPercent = 75;

        [Test]
        public void Agent_FiveHundredRuns_EveryRunReachesEnded([Values(StandardProfile, EasyOpponentStatPercent)] int profile)
        {
            var content = CreateFullRun(profile);
            for (ulong seed = 0; seed < RunsPerProfile; seed++)
            {
                var record = RunRecorder.Play(content, new MetaState(), seed, AgentFor(profile, seed), MaxCommandsPerRun);

                Assert.That(record.Ended, Is.True,
                    $"seed {seed}: stuck in {record.Run.Phase}, season {record.Run.Season} after {record.Commands.Count} commands");
            }
        }

        [Test]
        public void Agent_FiveHundredRuns_StepInvariantsHold([Values(StandardProfile, EasyOpponentStatPercent)] int profile)
        {
            var content = CreateFullRun(profile);
            for (ulong seed = 0; seed < RunsPerProfile; seed++)
                RunRecorder.Play(content, new MetaState(), seed, AgentFor(profile, seed), MaxCommandsPerRun,
                    c => CheckStep(c, content));
        }

        [Test]
        public void Agent_FiveHundredRuns_SeasonInvariantsHold([Values(StandardProfile, EasyOpponentStatPercent)] int profile)
        {
            var content = CreateFullRun(profile);
            for (ulong seed = 0; seed < RunsPerProfile; seed++)
            {
                var season = new SeasonTracker();
                RunRecorder.Play(content, new MetaState(), seed, AgentFor(profile, seed), MaxCommandsPerRun,
                    c => season.Observe(c));
            }
        }

        [Test]
        public void Agent_FiveHundredRuns_OutcomeMatchesReputationAndSeason(
            [Values(StandardProfile, EasyOpponentStatPercent)] int profile)
        {
            var content = CreateFullRun(profile);
            for (ulong seed = 0; seed < RunsPerProfile; seed++)
            {
                var battles = 0;
                var record = RunRecorder.Play(content, new MetaState(), seed, AgentFor(profile, seed), MaxCommandsPerRun,
                    c => battles += Count<BattleStarted>(c.Result));
                var run = record.Run;

                Assert.That(run.Outcome == RunOutcome.Exiled, Is.EqualTo(run.Reputation == 0), $"seed {seed}: exiled ⇔ reputation 0");
                Assert.That(run.Outcome == RunOutcome.Won, Is.EqualTo(run.Season == SeasonCount && run.Reputation >= 1),
                    $"seed {seed}: won ⇔ last season with reputation");
                Assert.That(battles, Is.EqualTo(run.Season), $"seed {seed}: one battle per season");
            }
        }

        [Test]
        public void Agent_EndedRun_RejectsEveryRunCommand()
        {
            var content = CreateFullRun();
            var meta = new MetaState();
            var run = RunRecorder.Play(content, meta, 0, new RandomAgent(0), MaxCommandsPerRun).Run;
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Ended), "arrange: ended");
            var uid = run.Bag.Count > 0 ? run.Bag[0].Uid : 1;
            var commands = new ICommand[]
            {
                new StartRun(Difficulty, StarterKit, 1, RandomAgent.NowUnix), new PlaceItem(uid, 0), new RemoveFromGrid(0),
                new MoveCell(0, 1), new Brew(RandomAgent.NowUnix), new ReorderCatalyst(uid, 0), new Buy(0), new Sell(uid),
                new Inspect(uid, uid), new Reroll(), new AcceptDecree(), new SkipTurn(), new AckBrewResult(), new Offer(uid),
                new DismissCourtier(), new AckBattle(), new ChooseReward(0), new SkipReward(),
            };

            foreach (var command in commands)
            {
                var before = StateHasher.Hash(run, meta);
                var result = GameRules.Apply(run, meta, content, command);

                Assert.That(result.Accepted, Is.False, command.GetType().Name);
                Assert.That(StateHasher.Hash(run, meta), Is.EqualTo(before), command.GetType().Name);
            }
        }

        [Test]
        public void Agent_ThousandRuns_CoversEveryBranch()
        {
            var coverage = new Coverage();
            foreach (var profile in new[] { StandardProfile, EasyOpponentStatPercent })
            {
                var content = CreateFullRun(profile);
                for (ulong seed = 0; seed < RunsPerProfile; seed++)
                {
                    var record = RunRecorder.Play(content, new MetaState(), seed, AgentFor(profile, seed), MaxCommandsPerRun,
                        coverage.Observe);
                    coverage.ObserveEnd(record.Run, content);
                }
            }

            TestContext.WriteLine(coverage.Report());
            coverage.AssertEveryBranch();
        }

        [Test]
        public void Agent_ThousandRuns_ReplaySameHashEveryStep()
        {
            var content = CreateFullRun();
            var hashes = new List<ulong>();
            for (ulong seed = 0; seed < ReplayRuns; seed++)
            {
                hashes.Clear();
                var record = RunRecorder.Play(content, new MetaState(), seed, new RandomAgent((int)seed), MaxCommandsPerRun,
                    c => hashes.Add(StateHasher.Hash(c.Run, c.Meta)));

                var meta = new MetaState();
                var replay = RunRecorder.Start(content, meta, seed);
                for (var step = 0; step < record.Commands.Count; step++)
                {
                    GameRules.Apply(replay, meta, content, record.Commands[step]);
                    if (StateHasher.Hash(replay, meta) != hashes[step])
                        Assert.Fail($"seed {seed}, step {step}: replay hash differs ({record.Commands[step].GetType().Name})");
                }
            }
        }

        private static RandomAgent AgentFor(int profile, ulong seed) =>
            profile == EasyOpponentStatPercent
                ? new RandomAgent((int)seed, recipeHintPercent: EasyRecipeHintPercent)
                : new RandomAgent((int)seed);

        /// <summary>I1–I7 (full-run R5). Thông điệp chỉ dựng khi vi phạm (vòng nóng ~10⁶ bước).</summary>
        private static void CheckStep(StepContext c, ContentDb content)
        {
            var run = c.Run;
            if (!c.Result.Accepted && StateHasher.Hash(run, c.Meta) != c.HashBefore)
                Fail(c, "I1 rejected command changed state");

            if (run.Phase == RunPhase.Workbench ? run.Turn < 1 || run.Turn > TurnsPerSeason : run.Turn != 0)
                Fail(c, $"I2 turn {run.Turn}");

            if (BagRules.UsedSlots(run) > run.BagCapacity || run.BagCapacity > MaxBagCapacity)
                Fail(c, $"I3 used {BagRules.UsedSlots(run)} / capacity {run.BagCapacity}");
            if (BagRules.CountCatalysts(run, content) > CatalystSlots)
                Fail(c, $"I3 catalysts {BagRules.CountCatalysts(run, content)}");

            if (run.Gold < 0)
                Fail(c, $"I4 gold {run.Gold}");
            foreach (var slot in run.Shop.Slots)
            {
                if (!slot.IsAvailable)
                    continue;
                var data = content.GetItem(slot.ItemId);
                if (MarketRules.BuyPrice(run, content, slot.ItemId) < 1 || data.Tier > ShopMaxTier || data.Recipe != null)
                    Fail(c, $"I4 shop slot {data.Id}");
            }

            foreach (var item in run.Bag)
            {
                if (MarketRules.SellPrice(run, content, item.ItemId) < 0)
                    Fail(c, $"I4 sell price {content.GetItem(item.ItemId).Id}");
            }

            if (run.Reputation > c.ReputationBefore || run.Reputation < 0 || run.Reputation > StartingReputation)
                Fail(c, $"I5 reputation {c.ReputationBefore}→{run.Reputation}");
            if (run.Reputation == 0 && (run.Phase != RunPhase.Ended || run.Outcome != RunOutcome.Exiled))
                Fail(c, "I5 reputation 0 not exiled immediately");

            if (run.Season < c.SeasonBefore || run.Season > c.SeasonBefore + 1 || run.Season < 1 || run.Season > SeasonCount)
                Fail(c, $"I6 season {c.SeasonBefore}→{run.Season}");
            if (run.Season > c.SeasonBefore && Count<SeasonStarted>(c.Result) != 1)
                Fail(c, "I6 season advanced without SeasonStarted");

            if (run.Phase == RunPhase.Spoils && (run.LastBattle == null || !run.LastBattle.HeroWon || run.Season >= SeasonCount))
                Fail(c, "I7 spoils without a won battle before the last season");
        }

        private static void Fail(StepContext c, string what) => Assert.Fail($"{what} — {c.Where}");

        private static int Count<T>(CommandResult result) where T : GameEvent
        {
            var count = 0;
            if (result.Events == null)
                return 0;
            foreach (var e in result.Events)
            {
                if (e is T)
                    count++;
            }

            return count;
        }

        /// <summary>Bất biến theo Mùa (full-run R6): Mùa đóng ở <see cref="SeasonStarted"/> hoặc <see cref="RunEnded"/>.</summary>
        private sealed class SeasonTracker
        {
            private int _battles;
            private int _acks;
            private int _offers;
            private int _turns;
            private bool _hasEquipment;
            private EquipmentId _equipment;

            public void Observe(StepContext c)
            {
                if (!_hasEquipment)
                {
                    _equipment = c.EquipmentBefore;
                    _hasEquipment = true;
                }

                if (!c.Result.Accepted)
                    return;

                if (c.Command is AckBattle)
                    _acks++;
                foreach (var e in c.Result.Events)
                {
                    switch (e)
                    {
                        case TurnStarted _:
                            _turns++;
                            break;
                        case ItemOffered offered:
                            _offers++;
                            if (BagRules.IndexOfUid(c.Run, offered.Uid) >= 0)
                                Fail(c, $"offered uid {offered.Uid} still in bag");
                            break;
                        case BattleStarted started:
                            _battles++;
                            if (started.Log.Equipment != _equipment)
                                Fail(c, "AC-2 forged equipment differs from decree");
                            if (c.Command is DismissCourtier && started.Log.HeroWon)
                                Fail(c, "AC-7 dismissed courtier but won");
                            break;
                        case SeasonStarted _:
                        case RunEnded _:
                            Close(c);
                            break;
                    }
                }

                if (c.Run.Phase != RunPhase.Ended && c.Run.Season == c.SeasonBefore && c.Run.DecreeEquipment != _equipment)
                    Fail(c, "decree equipment changed within season");
            }

            private void Close(StepContext c)
            {
                if (_battles != 1 || _acks != 1 || _offers > 1 || _turns != TurnsPerSeason)
                    Fail(c, $"AC-3 season closed with battles {_battles}, acks {_acks}, offers {_offers}, turns {_turns}");
                _battles = _acks = _offers = _turns = 0;
                _equipment = c.Run.DecreeEquipment;
            }
        }

        /// <summary>Đếm nhánh (full-run R8) — mỗi nhánh phải ≥ 1.</summary>
        private sealed class Coverage
        {
            private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();
            private static readonly string[] Branches =
            {
                "Ended·Exiled", "Ended·Won", "reached season 8", "lost season 8 but won run", "ChooseReward accepted",
                "SkipReward accepted", "DismissCourtier accepted", "brewed product", "brewed scrap", "CatalystActivated",
                "ItemInspected", "ActiveModifier set", "bag expanded",
            };

            public Coverage()
            {
                foreach (var branch in Branches)
                    _counts[branch] = 0;
            }

            public int TurnsStarted { get; private set; }
            public int Commands { get; private set; }

            public void Observe(StepContext c)
            {
                Commands++;
                if (c.Run.ActiveModifier.HasValue)
                    _counts["ActiveModifier set"]++;
                if (!c.Result.Accepted)
                    return;

                switch (c.Command)
                {
                    case ChooseReward _: _counts["ChooseReward accepted"]++; break;
                    case SkipReward _: _counts["SkipReward accepted"]++; break;
                    case DismissCourtier _: _counts["DismissCourtier accepted"]++; break;
                }

                foreach (var e in c.Result.Events)
                {
                    switch (e)
                    {
                        case TurnStarted _: TurnsStarted++; break;
                        case BrewCompleted brewed: _counts[brewed.Log.Result.IsScrap ? "brewed scrap" : "brewed product"]++; break;
                        case CatalystActivated _: _counts["CatalystActivated"]++; break;
                        case ItemInspected _: _counts["ItemInspected"]++; break;
                        case RewardChosen chosen when chosen.BagAdded > 0: _counts["bag expanded"]++; break;
                    }
                }
            }

            public void ObserveEnd(RunState run, ContentDb content)
            {
                if (run.Outcome == RunOutcome.Exiled) _counts["Ended·Exiled"]++;
                if (run.Outcome == RunOutcome.Won) _counts["Ended·Won"]++;
                if (run.Season == content.Run.SeasonCount) _counts["reached season 8"]++;
                if (run.Outcome == RunOutcome.Won && run.LastBattle != null && !run.LastBattle.HeroWon)
                    _counts["lost season 8 but won run"]++;
            }

            public string Report()
            {
                var text = new System.Text.StringBuilder($"commands {Commands}, turns started {TurnsStarted}");
                foreach (var branch in Branches)
                    text.Append("; ").Append(branch).Append(' ').Append(_counts[branch]);
                return text.ToString();
            }

            public void AssertEveryBranch()
            {
                foreach (var branch in Branches)
                    Assert.That(_counts[branch], Is.GreaterThanOrEqualTo(1), branch);
            }
        }
    }
}
