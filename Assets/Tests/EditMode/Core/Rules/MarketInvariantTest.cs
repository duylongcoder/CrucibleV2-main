using System;
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
    /// <summary>Bất biến kinh tế qua <see cref="GameRules.Apply"/> (economy.md AC-2b, AC-4, AC-7).</summary>
    public class MarketInvariantTest
    {
        private const int StepsPerSeed = 2000;
        private const int StepsPerTurn = 25;
        private const int TurnGold = 20;

        private ContentDb _content;
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
        }

        [Test]
        public void Apply_RandomMarketAndGridCommands_KeepsInvariants([Values(1, 2, 3, 4, 5)] int seed)
        {
            var run = CreateRun((ulong)seed);
            var random = new System.Random(seed);
            var accepted = 0;

            for (var step = 0; step < StepsPerSeed; step++)
            {
                // Giả lượt mới (F-009 chưa có): làm mới hàng định kỳ + vàng thưởng (F-011) để fuzz không cạn vàng.
                if (step % StepsPerTurn == 0)
                {
                    MarketRules.RefreshShop(run, _content);
                    run.Gold += TurnGold;
                }

                var before = StateHasher.Hash(run, _meta);
                var result = Apply(run, NextCommand(random, run));
                if (result.Accepted)
                    accepted++;
                else
                    Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before), $"step {step}: rejected changed state");

                // Giả AckBrewResult (F-009 chưa có): mở lại lưới để fuzz tiếp.
                run.WorkbenchStep = WorkbenchStep.Editing;

                Assert.That(run.Gold, Is.GreaterThanOrEqualTo(0), $"step {step}");
                Assert.That(BagRules.UsedSlots(run), Is.LessThanOrEqualTo(run.BagCapacity), $"step {step}");
                Assert.That(BagRules.CountCatalysts(run, _content), Is.LessThanOrEqualTo(CatalystSlots), $"step {step}");
            }

            Assert.That(accepted, Is.GreaterThan(StepsPerSeed / 10), "fuzz should exercise accepted commands");
        }

        [Test]
        public void Apply_BuyToFullThenBrew_StaysWithinCapacity()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Coal }, new[] { Iron, Coal });
            run.BagCapacity = 6;
            run.Gold = 100;
            StateFactory.StockShop(run, _content, Iron, Coal, Copper, Ash, Lens);

            var bought = 0;
            for (var slot = 0; slot < 5; slot++)
            {
                if (Apply(run, new Buy(slot)).Accepted)
                    bought++;
            }

            Assert.That(bought, Is.EqualTo(3));
            Assert.That(BagRules.UsedSlots(run), Is.EqualTo(run.BagCapacity));
            Assert.That(Apply(run, new Brew(0)).Accepted, Is.True);
            Assert.That(BagRules.UsedSlots(run), Is.LessThanOrEqualTo(run.BagCapacity));
        }

        [Test]
        public void Apply_BuyThenSellSameItem_LosesGold([Values(70, 100, 130)] int seasonPercent)
        {
            for (var slot = 0; slot < IngredientSlotCount + 1; slot++)
            {
                var run = StateFactory.CreateWorkbenchRun(_content, new string[0]);
                run.Gold = 100;
                run.SeasonPricePercent = seasonPercent;
                StateFactory.StockShop(run, _content, Iron, Bone, Silver, Sand, Tongs);

                Assert.That(Apply(run, new Buy(slot)).Accepted, Is.True, $"slot {slot}");
                Assert.That(Apply(run, new Sell(run.Bag.Last().Uid)).Accepted, Is.True, $"slot {slot}");

                Assert.That(run.Gold, Is.LessThan(100), $"slot {slot}");
            }
        }

        [Test]
        public void Apply_BuyThenSellSameItem_LosesGoldUnderAnyModifier(
            [Values(ModNeutral, ModDear, ModBrutal, ModNoSword, ModSwordOnly)] string modifier,
            [Values(70, 130)] int seasonPercent)
        {
            for (var slot = 0; slot < IngredientSlotCount + 1; slot++)
            {
                var run = StateFactory.CreateWorkbenchRun(_content, new string[0]);
                run.Gold = 100;
                run.SeasonPricePercent = seasonPercent;
                run.ActiveModifier = ModifierOf(_content, modifier);
                StateFactory.StockShop(run, _content, Iron, Bone, Silver, Sand, Lens);

                Assert.That(Apply(run, new Buy(slot)).Accepted, Is.True, $"slot {slot}");
                Assert.That(Apply(run, new Sell(run.Bag.Last().Uid)).Accepted, Is.True, $"slot {slot}");

                Assert.That(run.Gold, Is.LessThan(100), $"slot {slot}");
            }
        }

        [Test]
        public void Apply_SameSeedSameCommands_SameHash()
        {
            var first = CreateRun(7);
            var second = CreateRun(7);
            var commands = new ICommand[] { new Reroll(), new Buy(0), new Reroll(), new Buy(1, 3), new Sell(1), new Buy(2) };

            MarketRules.RefreshShop(first, _content);
            MarketRules.RefreshShop(second, _content);
            foreach (var command in commands)
            {
                Apply(first, command);
                Apply(second, command);
            }

            Assert.That(first.Shop.RerollCount, Is.EqualTo(2));
            Assert.That(StateHasher.Hash(second, _meta), Is.EqualTo(StateHasher.Hash(first, _meta)));
        }

        private RunState CreateRun(ulong seed)
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron, Bellows, Coal, Steel }, new[] { Copper });
            run.BagCapacity = 10;
            run.Gold = 30;
            run.Seed = seed;
            run.Rng = Crucible.Core.Random.RngStreams.Create(seed);
            return run;
        }

        private CommandResult Apply(RunState run, ICommand command) => GameRules.Apply(run, _meta, _content, command);

        /// <summary>Tham số ngẫu nhiên, có cả slot / uid / ô sai để sinh lệnh bị từ chối.</summary>
        private static ICommand NextCommand(System.Random random, RunState run)
        {
            switch (random.Next(7))
            {
                case 0:
                case 1: return new Buy(random.Next(-1, 7), random.Next(3) == 0 ? random.Next(-1, 10) : (int?)null);
                case 2: return new Sell(RandomUid(random, run));
                case 3: return new Reroll();
                case 4: return new PlaceItem(RandomUid(random, run), random.Next(-1, GridRules.CellCount + 1));
                case 5: return new RemoveFromGrid(random.Next(-1, GridRules.CellCount + 1));
                case 6: return new Brew(0);
                default: throw new InvalidOperationException();
            }
        }

        private static int RandomUid(System.Random random, RunState run) => random.Next(0, run.NextUid + 2);
    }
}
