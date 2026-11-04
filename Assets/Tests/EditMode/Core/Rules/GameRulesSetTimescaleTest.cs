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
    public class GameRulesSetTimescaleTest
    {
        private ContentDb _content;
        private MetaState _meta;
        private RunState _run;

        [SetUp]
        public void SetUp()
        {
            _content = ContentFactory.Create();
            _meta = new MetaState();
            _run = StateFactory.CreateWorkbenchRun(_content, new[] { Iron }, new[] { Coal });
        }

        [Test]
        public void NewMetaState_Timescale_IsOne()
        {
            Assert.That(new MetaState().Timescale, Is.EqualTo(1));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Apply_SetTimescaleInRange_StoresTimescale(int timescale)
        {
            Apply(timescale);

            Assert.That(_meta.Timescale, Is.EqualTo(timescale));
        }

        [Test]
        public void Apply_SetTimescaleInRange_EmitsTimescaleChanged()
        {
            var result = Apply(3);

            Assert.That(result.Events.Count, Is.EqualTo(1));
            Assert.That(result.Events[0], Is.TypeOf<TimescaleChanged>());
            Assert.That(((TimescaleChanged)result.Events[0]).Timescale, Is.EqualTo(3));
        }

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(-1)]
        public void Apply_SetTimescaleOutOfRange_RejectsWithInvalidTimescale(int timescale)
        {
            var result = Apply(timescale);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Reason, Is.EqualTo(RejectReason.InvalidTimescale));
        }

        [Test]
        public void Apply_SetTimescaleOutOfRange_StateHashUnchanged()
        {
            var before = StateHasher.Hash(_run, _meta);

            Apply(4);

            Assert.That(StateHasher.Hash(_run, _meta), Is.EqualTo(before));
        }

        [Test]
        public void Apply_SetTimescaleWithoutActiveRun_Accepted()
        {
            var result = GameRules.Apply(new RunState(), _meta, _content, new SetTimescale(2));

            Assert.That(result.Accepted, Is.True);
        }

        [Test]
        public void Apply_SetTimescaleWhileResultPending_AcceptedAndRunUnchanged()
        {
            _run.WorkbenchStep = WorkbenchStep.ResultPending;
            var gridItem = _run.Grid[0];

            var result = Apply(2);

            Assert.That(result.Accepted, Is.True);
            Assert.That(_run.Grid[0], Is.SameAs(gridItem));
            Assert.That(_run.WorkbenchStep, Is.EqualTo(WorkbenchStep.ResultPending));
        }

        [Test]
        public void Apply_SetTimescale_RunRngUnchanged()
        {
            var before = CaptureRng();

            Apply(2);

            Assert.That(CaptureRng(), Is.EqualTo(before));
        }

        private CommandResult Apply(int timescale) =>
            GameRules.Apply(_run, _meta, _content, new SetTimescale(timescale));

        private ulong[] CaptureRng()
        {
            var states = new ulong[RngStreams.Count * 2];
            for (var i = 0; i < RngStreams.Count; i++)
            {
                var state = _run.Rng.Get((RngStream)i).State;
                states[i * 2] = state.State;
                states[i * 2 + 1] = state.Inc;
            }

            return states;
        }
    }
}
