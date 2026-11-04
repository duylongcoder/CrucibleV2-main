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
            ["last brew uid"] = run => run.LastBrewResultUid = 3,
            ["last brew log"] = run => run.LastBrew = BrewLog.Create(
                new[] { new ItemInstance(40, new ItemId(0)), null, null, null, null, null, null, null, null },
                new ItemInstance(41, new ItemId(1)), true),
            ["item magic"] = run => run.Bag[0] = new ItemInstance(run.Bag[0].Uid, run.Bag[0].ItemId, magic: true),
            ["item quality"] = run => run.Bag[0] = new ItemInstance(run.Bag[0].Uid, run.Bag[0].ItemId, quality: 7),
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
