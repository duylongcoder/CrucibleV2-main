using System.Linq;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Logs;
using Crucible.Core.Rules;
using Crucible.Core.State;
using Crucible.Tests.TestUtils;
using NUnit.Framework;
using static Crucible.Tests.TestUtils.ContentFactory;

namespace Crucible.Tests.Core.Rules
{
    /// <summary><c>Offer(uid)</c> qua <see cref="GameRules.Apply"/> (forge-battle R4, R6; run-flow AC-2; battle AC-1).</summary>
    public class GameRulesOfferTest
    {
        private const int SteelUid = 2;
        private const long SteelQuality = 40;

        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = CreateRun(_content);
        }

        [Test]
        public void Apply_OfferProduct_RemovesItemFromBag()
        {
            Apply(new Offer(SteelUid));

            Assert.That(_run.Bag.Select(item => item.Uid), Is.EqualTo(new[] { 1, 3 }));
        }

        [Test]
        public void Apply_OfferProduct_EntersBattleWithLog()
        {
            var result = Apply(new Offer(SteelUid));

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Phase, Is.EqualTo(RunPhase.Battle));
            Assert.That(_run.LastBattle, Is.Not.Null);
            Assert.That(_run.LastBattle.EndReason, Is.Not.EqualTo(BattleEndReason.Forfeit));
        }

        [Test]
        public void Apply_OfferProduct_LogUsesDecreeEquipment([Values(Sword, Armor)] string equipment)
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { Iron, Steel }, equipmentId: equipment);

            GameRules.Apply(run, _meta, _content, new Offer(2));

            Assert.That(run.LastBattle.Equipment, Is.EqualTo(run.DecreeEquipment));
        }

        [Test]
        public void Apply_OfferProduct_LogMatchesDirectSimulation()
        {
            var offered = _run.Bag[1];
            var battle = _content.Battle;
            var expected = BattleSimulator.Simulate(1, _run.DecreeEquipment, offered,
                BattleStats.ForHero(SteelQuality, 0, 0, 0, _content.GetEquipment(_run.DecreeEquipment), battle),
                BattleStats.ForOpponent(_content.Season(1).Opponent, 100), battle);

            Apply(new Offer(SteelUid));

            BattleSimulatorTest.AssertLogsEqual(_run.LastBattle, expected);
        }

        [Test]
        public void Apply_OfferProduct_EmitsItemOfferedThenBattleStarted()
        {
            var result = Apply(new Offer(SteelUid));

            Assert.That(result.Events.Select(e => e.GetType()), Is.EqualTo(new[] { typeof(ItemOffered), typeof(BattleStarted) }));
            var offered = (ItemOffered)result.Events[0];
            var started = (BattleStarted)result.Events[1];
            Assert.That(offered.Season, Is.EqualTo(1));
            Assert.That(offered.Uid, Is.EqualTo(SteelUid));
            Assert.That(started.Season, Is.EqualTo(1));
            Assert.That(started.Log, Is.SameAs(_run.LastBattle));
        }

        [Test]
        public void Apply_OfferProduct_DoesNotAdvanceAnyRngStream()
        {
            var before = _run.Rng.Capture();

            Assert.That(Apply(new Offer(SteelUid)).Accepted, Is.True, "arrange: offer");

            Assert.That(_run.Rng.Capture(), Is.EqualTo(before));
        }

        [Test]
        public void Apply_OfferMagicProduct_LogCarriesMagicFlag()
        {
            _run.Bag[1] = new ItemInstance(SteelUid, IdOf(_content, Steel), magic: true, quality: SteelQuality);

            Apply(new Offer(SteelUid));

            Assert.That(_run.LastBattle.OfferedMagic, Is.True);
            Assert.That(_run.LastBattle.OfferedItem, Is.EqualTo(IdOf(_content, Steel)));
        }

        [Test]
        public void Apply_OfferHarderDifficulty_ScalesOpponent()
        {
            var content = ContentFactory.Create(opponentStatPercent: 200);
            var run = CreateRun(content);

            GameRules.Apply(run, _meta, content, new Offer(SteelUid));

            Assert.That(run.LastBattle.Opponent.Hp, Is.EqualTo(2 * content.Season(1).Opponent.Hp));
        }

        [Test]
        public void Apply_Offer_RejectsWhenNotActive()
        {
            var run = new RunState();
            var before = StateHasher.Hash(run, _meta);

            var result = GameRules.Apply(run, _meta, _content, new Offer(SteelUid));

            Assert.That(result.Reason, Is.EqualTo(RejectReason.RunNotActive));
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        [TestCase(RunPhase.Decree)]
        [TestCase(RunPhase.Workbench)]
        [TestCase(RunPhase.Battle)]
        [TestCase(RunPhase.Spoils)]
        [TestCase(RunPhase.Ended)]
        public void Apply_Offer_RejectsOutsideOffering(RunPhase phase)
        {
            _run.Phase = phase;
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(new Offer(SteelUid));

            Assert.That(result.Reason, Is.EqualTo(RejectReason.WrongPhase));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [TestCase(0)]
        [TestCase(99)]
        public void Apply_OfferUnknownUid_RejectsItemNotFound(int uid)
        {
            var before = StateHasher.Hash(_run, _meta);

            var result = Apply(new Offer(uid));

            Assert.That(result.Reason, Is.EqualTo(RejectReason.ItemNotFound));
            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [TestCase(Iron)]
        [TestCase(Scrap)]
        [TestCase(Lens)]
        [TestCase(Bellows)]
        public void Apply_OfferNonProduct_RejectsItemNotOfferable(string id)
        {
            var run = StateFactory.CreateOfferingRun(_content, new[] { id, Steel });
            var before = StateHasher.Hash(run, _meta);

            var result = GameRules.Apply(run, _meta, _content, new Offer(1));

            Assert.That(result.Reason, Is.EqualTo(RejectReason.ItemNotOfferable));
            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        /// <summary>Dâng Mùa 1, Kiếm; Túi = [Sắt (1), Thép Q40 (2), Tro (3)].</summary>
        private static RunState CreateRun(ContentDb content)
        {
            var run = StateFactory.CreateOfferingRun(content, new[] { Iron, Steel, Ash });
            run.Bag[1] = new ItemInstance(SteelUid, IdOf(content, Steel), quality: SteelQuality);
            return run;
        }

        private CommandResult Apply(ICommand command) => GameRules.Apply(_run, _meta, _content, command);
    }
}
