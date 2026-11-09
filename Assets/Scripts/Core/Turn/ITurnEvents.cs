using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    /// <summary>
    /// Chỗ cắm cho hệ sự kiện (design.md §8). C4 chỉ mở hai điểm nối đúng chỗ design.md §2 ghi;
    /// toàn bộ luật sự kiện là việc của C6.
    /// </summary>
    public interface ITurnEvents
    {
        /// <summary>
        /// design.md §2 bước 1: đầu lượt người chơi, áp dụng sự kiện ngẫu nhiên (không cần chọn)
        /// đã được rút ở lượt máy trước, nếu có.
        /// </summary>
        void ApplyPendingAtTurnStart(TurnEventContext context);

        /// <summary>design.md §2 bước 3: lượt máy, rút sự kiện.</summary>
        void RunMachineTurn(TurnEventContext context);
    }

    /// <summary>
    /// Những gì hệ sự kiện được phép đọc và được phép đổi. Cố tình hẹp: sự kiện cộng tài nguyên,
    /// ghi chiến tích và giết người — nó không tự sửa thẳng GameState.
    /// </summary>
    public sealed class TurnEventContext
    {
        private readonly List<int> _deadMemberIds = new List<int>();

        internal TurnEventContext(
            BalanceConfig config,
            GameState state,
            IRandom random,
            ICombatResolver combatResolver,
            ClimatePhase phase,
            int difficultyLevel)
        {
            Config = config;
            State = state;
            Random = random;
            CombatResolver = combatResolver;
            Phase = phase;
            DifficultyLevel = difficultyLevel;
        }

        public BalanceConfig Config { get; }
        public GameState State { get; }
        public IRandom Random { get; }
        public ICombatResolver CombatResolver { get; }
        public ClimatePhase Phase { get; }
        public int DifficultyLevel { get; }

        /// <summary>Thức ăn sự kiện cộng vào lượt đang tính. Không để dành (design.md §8).</summary>
        public int Food { get; private set; }

        public int Morale { get; private set; }
        public int Neuron { get; private set; }

        /// <summary>Chiến tích "thắng trận" mà sự kiện ghi được (design.md §8).</summary>
        public int CombatWins { get; private set; }

        public IReadOnlyList<int> DeadMemberIds
        {
            get { return _deadMemberIds; }
        }

        public void AddFood(int amount)
        {
            Food += amount;
        }

        public void AddMorale(int amount)
        {
            Morale += amount;
        }

        public void AddNeuron(int amount)
        {
            Neuron += amount;
        }

        public void AddCombatWin()
        {
            CombatWins++;
        }

        public void KillMember(int memberId)
        {
            if (State.FindMember(memberId) == null)
            {
                throw new ArgumentException("Không có thành viên " + memberId, nameof(memberId));
            }

            if (!_deadMemberIds.Contains(memberId))
            {
                _deadMemberIds.Add(memberId);
            }
        }

        /// <summary>TurnSystem gọi sau khi đã chuyển các thay đổi này vào GameState.</summary>
        internal void Reset()
        {
            Food = 0;
            Morale = 0;
            Neuron = 0;
            CombatWins = 0;
            _deadMemberIds.Clear();
        }

        public void KillMembers(IReadOnlyList<int> memberIds)
        {
            if (memberIds == null)
            {
                return;
            }

            for (int i = 0; i < memberIds.Count; i++)
            {
                KillMember(memberIds[i]);
            }
        }
    }
}
