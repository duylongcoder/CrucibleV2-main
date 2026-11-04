using System;
using System.Collections.Generic;
using Crucible.Core.Commands;
using Crucible.Core.Content;
using Crucible.Core.Rules;
using Crucible.Core.State;

namespace Crucible.Tests.TestUtils
{
    /// <summary>Một bước của <see cref="RunRecorder.Play"/>: lệnh, kết quả và state ngay trước lệnh (đối tượng dùng lại giữa các bước).</summary>
    public sealed class StepContext
    {
        public ulong Seed { get; internal set; }
        public int Step { get; internal set; }
        public ICommand Command { get; internal set; }
        public CommandResult Result { get; internal set; }
        public ulong HashBefore { get; internal set; }
        public int ReputationBefore { get; internal set; }
        public int SeasonBefore { get; internal set; }
        public RunPhase PhaseBefore { get; internal set; }
        public EquipmentId EquipmentBefore { get; internal set; }
        public RunState Run { get; internal set; }
        public MetaState Meta { get; internal set; }

        /// <summary>Nhãn tái hiện cho thông điệp assert.</summary>
        public string Where => $"seed {Seed}, step {Step}, {Command.GetType().Name}, {PhaseBefore}→{Run.Phase}, season {Run.Season}";
    }

    /// <summary>Kết quả một run của <see cref="RunRecorder.Play"/>.</summary>
    public sealed class RunRecord
    {
        internal RunRecord(ulong seed, RunState run, IReadOnlyList<ICommand> commands)
        {
            Seed = seed;
            Run = run;
            Commands = commands;
        }

        public ulong Seed { get; }
        /// <summary>State cuối.</summary>
        public RunState Run { get; }
        /// <summary>Lệnh sau <see cref="StartRun"/>, theo thứ tự đã áp (cả lệnh bị từ chối).</summary>
        public IReadOnlyList<ICommand> Commands { get; }
        public bool Ended => Run.Phase == RunPhase.Ended;
    }

    /// <summary>Chạy một run bằng <see cref="RandomAgent"/> qua <see cref="GameRules.Apply"/> (full-run §2).</summary>
    public static class RunRecorder
    {
        /// <summary>StartRun(<paramref name="seed"/>) → lặp agent tới <see cref="RunPhase.Ended"/> hoặc <paramref name="maxCommands"/> lệnh.</summary>
        /// <param name="onStep">Gọi sau mỗi lệnh; null → không tính hash trước lệnh.</param>
        public static RunRecord Play(ContentDb content, MetaState meta, ulong seed, RandomAgent agent, int maxCommands,
                                     Action<StepContext> onStep = null)
        {
            var run = Start(content, meta, seed);
            var commands = new List<ICommand>();
            var context = new StepContext { Seed = seed, Run = run, Meta = meta };

            for (var step = 0; step < maxCommands && run.Phase != RunPhase.Ended; step++)
            {
                var command = agent.Next(run, content);
                if (onStep != null)
                {
                    context.Step = step;
                    context.Command = command;
                    context.HashBefore = StateHasher.Hash(run, meta);
                    context.ReputationBefore = run.Reputation;
                    context.SeasonBefore = run.Season;
                    context.PhaseBefore = run.Phase;
                    context.EquipmentBefore = run.DecreeEquipment;
                }

                var result = GameRules.Apply(run, meta, content, command);
                commands.Add(command);
                if (onStep != null)
                {
                    context.Result = result;
                    onStep(context);
                }
            }

            return new RunRecord(seed, run, commands);
        }

        /// <summary>Run mới đã qua <see cref="StartRun"/> (bậc khó + bộ khởi đầu mặc định).</summary>
        public static RunState Start(ContentDb content, MetaState meta, ulong seed)
        {
            var run = new RunState();
            var result = GameRules.Apply(run, meta, content,
                new StartRun(ContentFactory.Difficulty, ContentFactory.StarterKit, seed, RandomAgent.NowUnix));
            if (!result.Accepted)
                throw new InvalidOperationException($"StartRun rejected: {result.Reason}");
            return run;
        }
    }
}
