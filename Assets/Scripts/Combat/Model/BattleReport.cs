using System;
using System.Collections.Generic;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Tóm tắt một trận đã chạy xong. Chưa phải `CombatResult` của Core: ở đây mới chỉ có người
    /// ngã xuống TRONG SÂN. Mức tổn thất theo luật (design.md §5.2 "mất 2 người", §8 quota) do
    /// <see cref="ICasualtyPolicy"/> áp lên sau.
    /// </summary>
    public sealed class BattleReport
    {
        private static readonly int[] NoDeaths = new int[0];

        internal BattleReport(
            bool won,
            BattleEndReason reason,
            double elapsedSeconds,
            IReadOnlyList<int> deadMemberIds,
            int beastsDown,
            int beastCount,
            int deployedCount,
            int tribeCount,
            BeastAttackStyle attackStyle,
            BattleKind kind)
        {
            Won = won;
            Reason = reason;
            ElapsedSeconds = elapsedSeconds;
            DeadMemberIds = deadMemberIds ?? NoDeaths;
            BeastsDown = beastsDown;
            BeastCount = beastCount;
            DeployedCount = deployedCount;
            TribeCount = tribeCount;
            AttackStyle = attackStyle;
            Kind = kind;
        }

        public bool Won { get; }

        public BattleEndReason Reason { get; }

        public double ElapsedSeconds { get; }

        /// <summary>Người ngã xuống trong sân, theo đúng thứ tự ngã.</summary>
        public IReadOnlyList<int> DeadMemberIds { get; }

        public int BeastsDown { get; }

        public int BeastCount { get; }

        /// <summary>Người chơi đã thả được bao nhiêu người trước khi trận kết thúc.</summary>
        public int DeployedCount { get; }

        /// <summary>Số người được cử đi trận, tức k của design.md §5.2.</summary>
        public int TribeCount { get; }

        public BeastAttackStyle AttackStyle { get; }

        public BattleKind Kind { get; }
    }
}
