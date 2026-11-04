using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Random;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary>
    /// Thêm 1 reroll ở Mùa 1 không đổi chiến lợi phẩm Mùa 1 và Chiếu chỉ Mùa 2 (full-run R10; GDD §6.1 AC-3 phần loại trang
    /// bị + chiến lợi phẩm). Kịch bản cố định trên hồ sơ dễ: Thép lượt 1, Bỏ qua ×2, Dâng, Thắng → Spoils.
    /// </summary>
    public class RunStreamIndependenceTest
    {
        private const int SeedCount = 50;
        private const int StartGold = 30;

        private ContentDb _content;

        [SetUp]
        public void SetUp()
        {
            _content = CreateFullRun(EasyOpponentStatPercent);
        }

        [Test]
        public void Apply_ExtraRerollInSeasonOne_SameSpoilsOptions()
        {
            for (ulong seed = 0; seed < SeedCount; seed++)
            {
                var plain = PlaySeasonOne(seed, extraReroll: false);
                var rerolled = PlaySeasonOne(seed, extraReroll: true);

                Assert.That(rerolled.SpoilsOptions, Is.EqualTo(plain.SpoilsOptions), $"seed {seed}");
            }
        }

        [Test]
        public void Apply_ExtraRerollInSeasonOne_SameSeasonTwoDecree()
        {
            for (ulong seed = 0; seed < SeedCount; seed++)
            {
                var plain = EnterSeasonTwo(PlaySeasonOne(seed, extraReroll: false));
                var rerolled = EnterSeasonTwo(PlaySeasonOne(seed, extraReroll: true));

                Assert.That(rerolled.SeasonPricePercent, Is.EqualTo(plain.SeasonPricePercent), $"seed {seed}: price");
                Assert.That(rerolled.DecreeEquipment, Is.EqualTo(plain.DecreeEquipment), $"seed {seed}: equipment");
                Assert.That(rerolled.ActiveModifier, Is.EqualTo(plain.ActiveModifier), $"seed {seed}: active modifier");
                Assert.That(rerolled.NextModifier, Is.EqualTo(plain.NextModifier), $"seed {seed}: next modifier");
            }
        }

        [Test]
        public void Apply_ExtraRerollInSeasonOne_DifferentShopStream()
        {
            for (ulong seed = 0; seed < SeedCount; seed++)
            {
                var plain = PlaySeasonOne(seed, extraReroll: false);
                var rerolled = PlaySeasonOne(seed, extraReroll: true);

                Assert.That(rerolled.Rng.Get(RngStream.Shop).State, Is.Not.EqualTo(plain.Rng.Get(RngStream.Shop).State),
                    $"seed {seed}");
            }
        }

        private RunState PlaySeasonOne(ulong seed, bool extraReroll)
        {
            var meta = new MetaState();
            var run = RunRecorder.Start(_content, meta, seed);
            run.Gold = StartGold;
            var iron = FirstUid(run, Iron);
            var coal = StateFactory.AddItem(run, _content, Coal).Uid;

            Accept(run, meta, new AcceptDecree());
            if (extraReroll)
                Accept(run, meta, new Reroll());
            Accept(run, meta, new PlaceItem(iron, 0));
            Accept(run, meta, new PlaceItem(coal, 1));
            Accept(run, meta, new Brew(RandomAgent.NowUnix));
            Accept(run, meta, new AckBrewResult());
            Accept(run, meta, new SkipTurn());
            Accept(run, meta, new SkipTurn());
            Accept(run, meta, new Offer(run.LastBrewResultUid));
            Accept(run, meta, new AckBattle());
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Spoils), $"arrange seed {seed}: won season 1");
            return run;
        }

        private RunState EnterSeasonTwo(RunState run)
        {
            Accept(run, new MetaState(), new SkipReward());
            Assert.That(run.Season, Is.EqualTo(2), "arrange: season 2 decree");
            return run;
        }

        private void Accept(RunState run, MetaState meta, ICommand command)
        {
            var result = GameRules.Apply(run, meta, _content, command);
            Assert.That(result.Accepted, Is.True, $"arrange {command.GetType().Name}: {result.Reason}");
        }

        private int FirstUid(RunState run, string itemId)
        {
            var id = IdOf(_content, itemId);
            foreach (var item in run.Bag)
            {
                if (item.ItemId == id)
                    return item.Uid;
            }

            Assert.Fail($"arrange: no {itemId} in bag");
            return -1;
        }
    }
}
