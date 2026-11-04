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
    /// <summary><c>OnShopRefresh</c> qua vòng lượt (catalyst R8; economy AC-2c).</summary>
    public class GameRulesShopRefreshCatalystTest
    {
        private const int StartingGold = 1000;
        private const int RefreshGold = 1;

        private ContentDb _content;
        private MetaState _meta;

        [SetUp]
        public void SetUp()
        {
            _content = CreateWithEffects(bellows: new[]
            {
                Effect(EffectTrigger.OnShopRefresh, EffectActionData.GainGold(RefreshGold)),
            });
            _meta = new MetaState();
        }

        [Test]
        public void Apply_AcceptDecree_FiresShopRefreshOnceBeforeTurnStarted()
        {
            var run = DecreeRunWithBellows();

            var events = Apply(run, new AcceptDecree()).Events.ToList();

            Assert.That(events.OfType<CatalystActivated>().Count(), Is.EqualTo(1));
            Assert.That(events.FindIndex(e => e is CatalystActivated),
                Is.LessThan(events.FindIndex(e => e is TurnStarted)));
            Assert.That(run.Gold, Is.EqualTo(StartingGold + RefreshGold));
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(5)]
        public void Apply_SeasonWithRerolls_ShopRefreshFiresOncePerTurn(int rerollsPerTurn)
        {
            var run = DecreeRunWithBellows();
            var events = new List<GameEvent>(Apply(run, new AcceptDecree()).Events);

            for (var turn = 1; turn <= TurnsPerSeason; turn++)
            {
                for (var r = 0; r < rerollsPerTurn; r++)
                    events.AddRange(Apply(run, new Reroll()).Events);
                events.AddRange(Apply(run, new SkipTurn()).Events);
            }

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Offering));
            Assert.That(events.OfType<CatalystActivated>().Count(), Is.EqualTo(TurnsPerSeason));
        }

        [Test]
        public void Apply_Reroll_DoesNotFireShopRefresh()
        {
            var run = DecreeRunWithBellows();
            Apply(run, new AcceptDecree());
            var goldBefore = run.Gold;
            var rerollPrice = PriceFormula.Reroll(RerollBase, RerollStep, 0);

            var events = Apply(run, new Reroll()).Events;

            Assert.That(events.OfType<CatalystActivated>(), Is.Empty);
            Assert.That(run.Gold, Is.EqualTo(goldBefore - rerollPrice));
        }

        [Test]
        public void Apply_AckBrewResult_FiresShopRefreshForNextTurn()
        {
            var run = DecreeRunWithBellows();
            StateFactory.AddItem(run, _content, Iron);
            Apply(run, new AcceptDecree());
            var iron = run.Bag.Last();
            Apply(run, new PlaceItem(iron.Uid, 0));
            Apply(run, new Brew(1793750400));

            var events = Apply(run, new AckBrewResult()).Events;

            Assert.That(run.Turn, Is.EqualTo(2));
            Assert.That(events.OfType<CatalystActivated>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Apply_SkipLastTurn_OfferingDoesNotFireShopRefresh()
        {
            var run = DecreeRunWithBellows();
            Apply(run, new AcceptDecree());
            Apply(run, new SkipTurn());
            Apply(run, new SkipTurn());

            var events = Apply(run, new SkipTurn()).Events;

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Offering));
            Assert.That(events.OfType<CatalystActivated>(), Is.Empty);
        }

        private RunState DecreeRunWithBellows()
        {
            var run = StateFactory.CreateDecreeRun();
            run.Gold = StartingGold;
            StateFactory.AddItem(run, _content, Bellows);
            return run;
        }

        private CommandResult Apply(RunState run, ICommand command)
        {
            var result = GameRules.Apply(run, _meta, _content, command);
            Assert.That(result.Accepted, Is.True, $"{command.GetType().Name}: {result.Reason}");
            return result;
        }
    }
}
