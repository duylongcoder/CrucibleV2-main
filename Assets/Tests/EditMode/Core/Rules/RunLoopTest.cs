using System.Collections.Generic;
using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary>Vòng lượt từ <see cref="StartRun"/> tới Dâng qua chuỗi Command thật (run-loop R6, R9, R10).</summary>
    public class RunLoopTest
    {
        private const long NowUnix = 1793750400;

        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _meta = new MetaState();
        }

        [Test]
        public void Apply_StartRunThenFullSeasonOfSkips_ReachesOfferingAfterConfiguredTurns([Values(1, 3, 4)] int turns)
        {
            var content = ContentFactory.Create(turnsPerSeason: turns);
            var run = StartAndAccept(content, 11);

            for (var turn = 1; turn <= turns; turn++)
            {
                Assert.That(run.Phase, Is.EqualTo(RunPhase.Workbench), $"turn {turn}");
                Assert.That(run.Turn, Is.EqualTo(turn));
                Assert.That(Apply(run, content, new SkipTurn()).Accepted, Is.True, $"skip {turn}");
            }

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Offering));
        }

        [Test]
        public void Apply_MixedBrewAndSkip_ThreeTurnStartedPerSeason([Values(0, 2, 5)] int rerolls)
        {
            var content = ContentFactory.Create();
            var run = new RunState();
            var events = new List<GameEvent>();
            Record(events, Apply(run, content, new StartRun(Difficulty, StarterKit, 5, NowUnix)));
            run.Gold = 100;
            Record(events, Apply(run, content, new AcceptDecree()));

            for (var i = 0; i < rerolls; i++)
                Record(events, Apply(run, content, new Reroll()));
            Record(events, Apply(run, content, new PlaceItem(run.Bag[0].Uid, 0)));
            Record(events, Apply(run, content, new Brew(NowUnix)));
            Record(events, Apply(run, content, new AckBrewResult()));
            Record(events, Apply(run, content, new SkipTurn()));
            Record(events, Apply(run, content, new PlaceItem(run.Bag[0].Uid, 0)));
            Record(events, Apply(run, content, new Brew(NowUnix)));
            Record(events, Apply(run, content, new AckBrewResult()));

            Assert.That(events.OfType<TurnStarted>().Select(e => e.Turn), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Offering));
        }

        [Test]
        public void Apply_SameSeed_SameSeasonPriceAndTurnOneShop()
        {
            var content = ContentFactory.Create();
            var first = StartAndAccept(content, 2026);
            var second = StartAndAccept(content, 2026);

            Assert.That(second.SeasonPricePercent, Is.EqualTo(first.SeasonPricePercent));
            Assert.That(second.Shop.Slots.Select(slot => slot.ItemId),
                Is.EqualTo(first.Shop.Slots.Select(slot => slot.ItemId)));
            Assert.That(first.Shop.Slots.Count, Is.EqualTo(IngredientSlotCount + 1));
        }

        [Test]
        public void Apply_ExtraRerollBeforeBrew_SameMagicResult()
        {
            var content = ContentFactory.Create();
            var sawMagic = false;
            var sawPlain = false;
            for (ulong seed = 0; seed < 200; seed++)
            {
                var plain = BrewCastIronWithAsh(content, seed, rerolls: 0);
                var rerolled = BrewCastIronWithAsh(content, seed, rerolls: 1);

                Assert.That(rerolled, Is.EqualTo(plain), $"seed {seed}");
                sawMagic |= plain;
                sawPlain |= !plain;
            }

            Assert.That(sawMagic && sawPlain, Is.True, "chance should produce both outcomes");
        }

        [Test]
        public void Apply_SameSeed_SameSeasonOneEquipment()
        {
            var content = ContentFactory.Create();
            var seen = new HashSet<EquipmentId>();
            for (ulong seed = 0; seed < 50; seed++)
            {
                var first = StartRun(content, seed);
                var second = StartRun(content, seed);

                Assert.That(second.DecreeEquipment, Is.EqualTo(first.DecreeEquipment), $"seed {seed}");
                seen.Add(first.DecreeEquipment);
            }

            Assert.That(seen.Count, Is.GreaterThan(1), "seeds should yield different equipment");
        }

        [Test]
        public void Apply_ExtraRerollInSeasonOne_SameSeasonTwoEquipment()
        {
            var content = ContentFactory.Create();
            for (ulong seed = 0; seed < 30; seed++)
            {
                var plain = LoseSeasonOne(content, seed, rerolls: 0);
                var rerolled = LoseSeasonOne(content, seed, rerolls: 1);

                Assert.That(rerolled.Season, Is.EqualTo(2), "arrange: season 2");
                Assert.That(rerolled.DecreeEquipment, Is.EqualTo(plain.DecreeEquipment), $"seed {seed}");
            }
        }

        [Test]
        public void Apply_FullSeasonWithOffer_ReachesSeasonTwoOrSpoils()
        {
            var content = ContentFactory.Create();
            var run = StartAndAccept(content, 77);
            run.Bag.Add(new ItemInstance(run.NextUid++, IdOf(content, Coal)));
            PlaceFromBag(run, content, Iron, 0);
            PlaceFromBag(run, content, Coal, 1);
            Assert.That(Apply(run, content, new Brew(NowUnix)).Accepted, Is.True, "arrange: brew");
            var steel = run.Bag.Last();
            Assert.That(content.GetItem(steel.ItemId).Id, Is.EqualTo(Steel), "arrange: recipe");
            Assert.That(Apply(run, content, new AckBrewResult()).Accepted, Is.True);
            Assert.That(Apply(run, content, new SkipTurn()).Accepted, Is.True);
            Assert.That(Apply(run, content, new SkipTurn()).Accepted, Is.True);
            var decreeEquipment = run.DecreeEquipment;

            Assert.That(Apply(run, content, new Offer(steel.Uid)).Accepted, Is.True, "offer");
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Battle));
            Assert.That(run.LastBattle.Equipment, Is.EqualTo(decreeEquipment));
            Assert.That(Apply(run, content, new AckBattle()).Accepted, Is.True, "ack");

            if (run.LastBattle.HeroWon)
            {
                Assert.That(run.Phase, Is.EqualTo(RunPhase.Spoils));
                Assert.That(run.Reputation, Is.EqualTo(StartingReputation));
            }
            else
            {
                Assert.That(run.Phase, Is.EqualTo(RunPhase.Decree));
                Assert.That(run.Season, Is.EqualTo(2));
            }
        }

        [Test]
        public void Apply_SkipAllTurnsAndDismissThreeSeasons_ExilesInSeasonThree()
        {
            var content = ContentFactory.Create();
            var run = StartRun(content, 5);
            var events = new List<GameEvent>();

            for (var season = 1; season <= StartingReputation; season++)
            {
                Assert.That(run.Season, Is.EqualTo(season));
                Record(events, Apply(run, content, new AcceptDecree()));
                for (var turn = 0; turn < TurnsPerSeason; turn++)
                    Record(events, Apply(run, content, new SkipTurn()));
                Record(events, Apply(run, content, new DismissCourtier()));
                Record(events, Apply(run, content, new AckBattle()));
            }

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Ended));
            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Exiled));
            Assert.That(run.Season, Is.EqualTo(StartingReputation));
            Assert.That(events.OfType<ReputationLost>().Select(e => e.Remaining), Is.EqualTo(new[] { 2, 1, 0 }));
        }

        [Test]
        public void Apply_WinEverySeason_ReachesRunEndWonThroughSpoils([Values(1, 2, 3)] int seed)
        {
            var content = CreateWeakOpponentsWithSpoils();
            var run = StartRun(content, (ulong)seed);
            var events = new List<GameEvent>();

            for (var season = 1; season <= SeasonCount; season++)
            {
                Assert.That(run.Season, Is.EqualTo(season), "season");
                WinSeason(run, content, events);
                if (season == SeasonCount)
                    break;

                Assert.That(run.Phase, Is.EqualTo(RunPhase.Spoils), $"season {season}");
                ICommand leave = season % 2 == 1 && run.SpoilsOptions.Count > 0 ? new ChooseReward(0) : new SkipReward();
                Record(events, Apply(run, content, leave));
            }

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Ended));
            Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Won));
            Assert.That(events.OfType<SpoilsStarted>().Count(), Is.EqualTo(SeasonCount - 1));
            Assert.That(events.OfType<SeasonStarted>().Count(), Is.EqualTo(SeasonCount - 1));
        }

        [Test]
        public void Apply_SameSeedThroughSpoils_SameHash()
        {
            var content = CreateWeakOpponentsWithSpoils();
            var first = StartRun(content, 11);
            var second = StartRun(content, 11);

            for (var season = 1; season <= 3; season++)
            {
                WinSeason(first, content, new List<GameEvent>());
                WinSeason(second, content, new List<GameEvent>());
                Apply(first, content, new ChooseReward(0));
                Apply(second, content, new ChooseReward(0));
            }

            Assert.That(StateHasher.Hash(second, _meta), Is.EqualTo(StateHasher.Hash(first, _meta)));
        }

        /// <summary>Đối thủ HP 1 / ATK 0 mọi Mùa + Chiến lợi phẩm mặc định.</summary>
        private static ContentDb CreateWeakOpponentsWithSpoils()
        {
            var opponents = CreateOpponents()
                .Select(o => new OpponentData(o.Id, o.Stars, o.Kind, 1, 0, 0, 0, 0)).ToArray();
            return ContentFactory.Create(battle: CreateBattle(CreateSpoilsSeasons(), opponents));
        }

        /// <summary>Chiếu chỉ → lượt 1 nung Thép [Sắt, Than] (thêm tay 2 nguyên liệu) → bỏ qua phần còn lại → Dâng Thép → xem Trận.</summary>
        private void WinSeason(RunState run, ContentDb content, List<GameEvent> events)
        {
            Record(events, Apply(run, content, new AcceptDecree()));
            run.Bag.Add(new ItemInstance(run.NextUid++, IdOf(content, Iron)));
            run.Bag.Add(new ItemInstance(run.NextUid++, IdOf(content, Coal)));
            run.BagCapacity = System.Math.Max(run.BagCapacity, BagRules.UsedSlots(run));
            PlaceFromBag(run, content, Iron, 0);
            PlaceFromBag(run, content, Coal, 1);
            Assert.That(Apply(run, content, new Brew(NowUnix)).Accepted, Is.True, "arrange: brew");
            var steel = run.Bag.Last();
            Assert.That(content.GetItem(steel.ItemId).Id, Is.EqualTo(Steel), "arrange: recipe");
            Record(events, Apply(run, content, new AckBrewResult()));
            for (var turn = 1; turn < TurnsPerSeason; turn++)
                Record(events, Apply(run, content, new SkipTurn()));
            Record(events, Apply(run, content, new Offer(steel.Uid)));
            Assert.That(run.LastBattle.HeroWon, Is.True, "arrange: win");
            Record(events, Apply(run, content, new AckBattle()));
        }

        /// <summary>Mùa 1: (reroll ×n) → Bỏ qua ×3 → ra về → Thua → Chiếu chỉ Mùa 2.</summary>
        private RunState LoseSeasonOne(ContentDb content, ulong seed, int rerolls)
        {
            var run = StartAndAccept(content, seed);
            run.Gold = 10;
            for (var i = 0; i < rerolls; i++)
                Assert.That(Apply(run, content, new Reroll()).Accepted, Is.True, "arrange: reroll");
            for (var turn = 0; turn < TurnsPerSeason; turn++)
                Assert.That(Apply(run, content, new SkipTurn()).Accepted, Is.True, "arrange: skip");
            Assert.That(Apply(run, content, new DismissCourtier()).Accepted, Is.True, "arrange: dismiss");
            Assert.That(Apply(run, content, new AckBattle()).Accepted, Is.True, "arrange: ack");
            return run;
        }

        private RunState StartRun(ContentDb content, ulong seed)
        {
            var run = new RunState();
            Assert.That(Apply(run, content, new StartRun(Difficulty, StarterKit, seed, NowUnix)).Accepted, Is.True);
            return run;
        }

        /// <summary>Lưới [Sắt, Tro, Than] → Gang có Tro (10%); trả về <c>Magic</c> của thành phẩm.</summary>
        private bool BrewCastIronWithAsh(ContentDb content, ulong seed, int rerolls)
        {
            var run = StartAndAccept(content, seed);
            run.Gold = 10;
            run.Bag.Add(new ItemInstance(run.NextUid++, IdOf(content, Coal)));
            for (var i = 0; i < rerolls; i++)
                Assert.That(Apply(run, content, new Reroll()).Accepted, Is.True, "arrange: reroll");

            PlaceFromBag(run, content, Iron, 0);
            PlaceFromBag(run, content, Ash, 1);
            PlaceFromBag(run, content, Coal, 2);
            Assert.That(Apply(run, content, new Brew(NowUnix)).Accepted, Is.True, "arrange: brew");
            Assert.That(content.GetItem(run.Bag.Last().ItemId).Id, Is.EqualTo(CastIron), "arrange: recipe");
            return run.Bag.Last().Magic;
        }

        private void PlaceFromBag(RunState run, ContentDb content, string itemId, int cell)
        {
            var id = IdOf(content, itemId);
            var uid = run.Bag.First(item => item.ItemId.Equals(id)).Uid;
            Assert.That(Apply(run, content, new PlaceItem(uid, cell)).Accepted, Is.True, $"arrange: place {itemId}");
        }

        private RunState StartAndAccept(ContentDb content, ulong seed)
        {
            var run = StartRun(content, seed);
            Assert.That(Apply(run, content, new AcceptDecree()).Accepted, Is.True, "arrange: accept decree");
            return run;
        }

        private static void Record(List<GameEvent> events, CommandResult result)
        {
            Assert.That(result.Accepted, Is.True, $"arrange: rejected {result.Reason}");
            events.AddRange(result.Events);
        }

        private CommandResult Apply(RunState run, ContentDb content, ICommand command) =>
            GameRules.Apply(run, _meta, content, command);
    }
}
