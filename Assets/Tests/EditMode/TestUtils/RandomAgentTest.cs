using System;
using System.Text;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;
using NUnit.Framework;

namespace Crucible.Tests.TestUtils
{
    /// <summary>Hành vi của <see cref="RandomAgent"/> (full-run R1, R2).</summary>
    public class RandomAgentTest
    {
        private const int MaxDriveSteps = 5000;
        private const int MaxDriveSeeds = 50;

        private ContentDb _content;
        private MetaState _meta;

        public enum Target
        {
            Decree,
            Editing,
            ResultPending,
            Offering,
            Battle,
            Spoils,
            Ended,
        }

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.CreateFullRun(ContentFactory.EasyOpponentStatPercent);
            _meta = new MetaState();
        }

        [Test]
        public void Next_AnyPhase_DoesNotChangeState([Values] Target target)
        {
            var run = Reach(target);
            var agent = new RandomAgent(11);
            var before = StateHasher.Hash(run, _meta);

            for (var i = 0; i < 100; i++)
                agent.Next(run, _content);

            Assert.That(StateHasher.Hash(run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Next_SameSeed_SameCommandSequence()
        {
            var first = RunRecorder.Start(_content, new MetaState(), 5);
            var second = RunRecorder.Start(_content, new MetaState(), 5);
            var firstAgent = new RandomAgent(9);
            var secondAgent = new RandomAgent(9);
            var firstMeta = new MetaState();
            var secondMeta = new MetaState();

            for (var step = 0; step < 200; step++)
            {
                var a = firstAgent.Next(first, _content);
                var b = secondAgent.Next(second, _content);
                Assert.That(Describe(b), Is.EqualTo(Describe(a)), $"step {step}");
                GameRules.Apply(first, firstMeta, _content, a);
                GameRules.Apply(second, secondMeta, _content, b);
            }
        }

        [Test]
        public void Next_NoNoiseInSinglePhase_ReturnsAcceptedCommand(
            [Values(Target.Decree, Target.ResultPending, Target.Battle, Target.Spoils)] Target target)
        {
            var run = Reach(target);
            var agent = new RandomAgent(3, noisePercent: 0);

            var result = GameRules.Apply(run, _meta, _content, agent.Next(run, _content));

            Assert.That(result.Accepted, Is.True, result.Reason.ToString());
        }

        [Test]
        public void Next_RecipeHintWithIngredientsInBag_BrewsProduct()
        {
            var run = StateFactory.CreateWorkbenchRun(_content, new[] { ContentFactory.Iron, ContentFactory.Coal });
            var agent = new RandomAgent(1, noisePercent: 0, recipeHintPercent: 100);
            BrewCompleted brewed = null;

            for (var i = 0; i < 3 && brewed == null; i++)
            {
                var result = GameRules.Apply(run, _meta, _content, agent.Next(run, _content));
                foreach (var e in result.Events)
                    brewed = e as BrewCompleted ?? brewed;
            }

            Assert.That(brewed, Is.Not.Null, "no brew within 3 commands");
            Assert.That(brewed.Log.Result.IsScrap, Is.False);
        }

        /// <summary>Lái run (hồ sơ dễ, agent không nhiễu, gợi ý 100 %) tới <paramref name="target"/>; thử seed 0, 1… tới khi tới được.</summary>
        private RunState Reach(Target target)
        {
            for (var seed = 0; seed < MaxDriveSeeds; seed++)
            {
                var run = RunRecorder.Start(_content, _meta, (ulong)seed);
                var driver = new RandomAgent(seed, noisePercent: 0, recipeHintPercent: 100);
                for (var step = 0; step < MaxDriveSteps && !IsAt(run, target) && run.Phase != RunPhase.Ended; step++)
                    GameRules.Apply(run, _meta, _content, driver.Next(run, _content));
                if (IsAt(run, target))
                    return run;
            }

            Assert.Fail($"arrange: no seed below {MaxDriveSeeds} reaches {target}");
            return null;
        }

        private static bool IsAt(RunState run, Target target)
        {
            switch (target)
            {
                case Target.Decree: return run.Phase == RunPhase.Decree;
                case Target.Editing: return run.Phase == RunPhase.Workbench && run.WorkbenchStep == WorkbenchStep.Editing;
                case Target.ResultPending: return run.Phase == RunPhase.Workbench && run.WorkbenchStep == WorkbenchStep.ResultPending;
                case Target.Offering: return run.Phase == RunPhase.Offering;
                case Target.Battle: return run.Phase == RunPhase.Battle;
                case Target.Spoils: return run.Phase == RunPhase.Spoils;
                case Target.Ended: return run.Phase == RunPhase.Ended;
                default: throw new ArgumentOutOfRangeException(nameof(target));
            }
        }

        /// <summary>Kiểu + mọi property public (Command là record bất biến).</summary>
        private static string Describe(ICommand command)
        {
            var text = new StringBuilder(command.GetType().Name);
            foreach (var property in command.GetType().GetProperties())
                text.Append(' ').Append(property.Name).Append('=').Append(property.GetValue(command));
            return text.ToString();
        }
    }
}
