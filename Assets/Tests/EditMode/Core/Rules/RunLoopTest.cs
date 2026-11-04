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
            var run = new RunState();
            Assert.That(Apply(run, content, new StartRun(Difficulty, StarterKit, seed, NowUnix)).Accepted, Is.True);
            Assert.That(Apply(run, content, new AcceptDecree()).Accepted, Is.True, "arrange: accept decree");
            return run;
        }

        private static void Record(List<GameEvent> events, CommandResult result) => events.AddRange(result.Events);

        private CommandResult Apply(RunState run, ContentDb content, ICommand command) =>
            GameRules.Apply(run, _meta, content, command);
    }
}
