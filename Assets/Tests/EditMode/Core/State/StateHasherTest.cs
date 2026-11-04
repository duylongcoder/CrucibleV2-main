using System;
using System.Collections.Generic;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Random;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;

namespace Crucible.Tests.Core.State
{
    public class StateHasherTest
    {
        private static readonly Dictionary<string, Action<RunState>> Mutations = new Dictionary<string, Action<RunState>>
        {
            ["gold"] = run => run.Gold += 1,
            ["season"] = run => run.Season += 1,
            ["phase"] = run => run.Phase = RunPhase.Workbench,
            ["bag order"] = run => run.Bag.Reverse(),
            ["item uid"] = run => run.Bag[0] = new ItemInstance(99, run.Bag[0].ItemId),
            ["rng draw"] = run => run.Rng.Get(RngStream.Shop).NextUInt(),
            ["grid cell"] = run => run.Grid[4] = new ItemInstance(50, new ItemId(0)),
            ["workbench step"] = run => run.WorkbenchStep = WorkbenchStep.ResultPending,
            ["turn"] = run => run.Turn = 2,
            ["last brew uid"] = run => run.LastBrewResultUid = 3,
            ["last brew log"] = run => run.LastBrew = BrewLog.Create(
                new[] { new ItemInstance(40, new ItemId(0)), null, null, null, null, null, null, null, null },
                new ItemInstance(41, new ItemId(1)), true),
            ["item magic"] = run => run.Bag[0] = new ItemInstance(run.Bag[0].Uid, run.Bag[0].ItemId, magic: true),
            ["item quality"] = run => run.Bag[0] = new ItemInstance(run.Bag[0].Uid, run.Bag[0].ItemId, quality: 7),
            ["season price"] = run => run.SeasonPricePercent += 1,
            ["shop slot count"] = run => run.Shop.Slots.Add(ShopSlot.None),
            ["shop reroll count"] = run => run.Shop.RerollCount = 1,
            ["decree equipment"] = run => run.DecreeEquipment = new EquipmentId(2),
            ["outcome"] = run => run.Outcome = RunOutcome.Won,
            ["last battle"] = run => run.LastBattle = BattleLog.Forfeit(1, default, default, default),
            ["spoils options"] = run => run.SpoilsOptions.Add(new RewardId(1)),
            ["active modifier"] = run => run.ActiveModifier = new ModifierId(0),
            ["next modifier"] = run => run.NextModifier = new ModifierId(0),
        };

        private static IEnumerable<string> MutationNames => Mutations.Keys;

        [Test]
        public void Hash_TwoIdenticallyBuiltStates_ReturnsSameValue()
        {
            var first = StateHasher.Hash(StateFactory.CreateActiveRun(), new MetaState());
            var second = StateHasher.Hash(StateFactory.CreateActiveRun(), new MetaState());

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void Hash_StatesDifferInOneField_ReturnsDifferentValue([ValueSource(nameof(MutationNames))] string mutation)
        {
            var baseline = StateFactory.CreateActiveRun();
            var mutated = StateFactory.CreateActiveRun();
            Mutations[mutation](mutated);

            var baselineHash = StateHasher.Hash(baseline, new MetaState());
            var mutatedHash = StateHasher.Hash(mutated, new MetaState());

            Assert.That(mutatedHash, Is.Not.EqualTo(baselineHash));
        }

        [TestCase("season")]
        [TestCase("equipment")]
        [TestCase("hero")]
        [TestCase("opponent")]
        [TestCase("rounds")]
        [TestCase("offered item")]
        public void Hash_LastBattleDiffersInOneField_ReturnsDifferentValue(string field)
        {
            var battle = ContentFactory.CreateBattle();
            var item = new ItemInstance(5, new ItemId(1));
            var hero = new Combatant(100, 20, 0, 0, 0);
            var opponent = new Combatant(60, 7, 0, 0, 0);
            var baseline = StateFactory.CreateActiveRun();
            var mutated = StateFactory.CreateActiveRun();
            baseline.LastBattle = Crucible.Core.Rules.BattleSimulator.Simulate(1, default, item, hero, opponent, battle);
            mutated.LastBattle = field switch
            {
                "season" => Crucible.Core.Rules.BattleSimulator.Simulate(2, default, item, hero, opponent, battle),
                "equipment" => Crucible.Core.Rules.BattleSimulator.Simulate(1, new EquipmentId(1), item, hero, opponent, battle),
                "hero" => Crucible.Core.Rules.BattleSimulator.Simulate(1, default, item, new Combatant(101, 20, 0, 0, 0), opponent, battle),
                "opponent" => Crucible.Core.Rules.BattleSimulator.Simulate(1, default, item, hero, new Combatant(60, 7, 0, 0, 1), battle),
                "rounds" => Crucible.Core.Rules.BattleSimulator.Simulate(1, default, item, new Combatant(100, 25, 0, 0, 0), opponent, battle),
                _ => Crucible.Core.Rules.BattleSimulator.Simulate(1, default, new ItemInstance(5, new ItemId(2), magic: true), hero, opponent, battle),
            };

            Assert.That(StateHasher.Hash(mutated, new MetaState()),
                Is.Not.EqualTo(StateHasher.Hash(baseline, new MetaState())));
        }

        [Test]
        public void Hash_ShopSlotsDifferOnlyInItem_ReturnsDifferentValue()
        {
            var first = StateFactory.CreateActiveRun();
            var second = StateFactory.CreateActiveRun();
            first.Shop.Slots.Add(new ShopSlot(new ItemId(1)));
            second.Shop.Slots.Add(new ShopSlot(new ItemId(2)));

            Assert.That(StateHasher.Hash(second, new MetaState()), Is.Not.EqualTo(StateHasher.Hash(first, new MetaState())));
        }

        [Test]
        public void Hash_ShopSlotsDifferOnlyInAvailability_ReturnsDifferentValue()
        {
            var first = StateFactory.CreateActiveRun();
            var second = StateFactory.CreateActiveRun();
            first.Shop.Slots.Add(new ShopSlot(default(ItemId)));
            second.Shop.Slots.Add(ShopSlot.None);

            Assert.That(StateHasher.Hash(second, new MetaState()), Is.Not.EqualTo(StateHasher.Hash(first, new MetaState())));
        }

        [Test]
        public void Hash_MetaTimescaleDiffers_ReturnsDifferentValue()
        {
            var run = StateFactory.CreateActiveRun();

            var baselineHash = StateHasher.Hash(run, new MetaState { Timescale = 1 });
            var fasterHash = StateHasher.Hash(run, new MetaState { Timescale = 3 });

            Assert.That(fasterHash, Is.Not.EqualTo(baselineHash));
        }

        [Test]
        public void Hash_BagListCapacityDiffers_ReturnsSameValue()
        {
            var baseline = StateFactory.CreateActiveRun();
            var roomy = StateFactory.CreateActiveRun();
            roomy.Bag.Capacity = 64;

            var baselineHash = StateHasher.Hash(baseline, new MetaState());
            var roomyHash = StateHasher.Hash(roomy, new MetaState());

            Assert.That(roomyHash, Is.EqualTo(baselineHash));
        }
    }
}
