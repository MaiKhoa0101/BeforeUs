using System;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    /// <summary>Mọi thứ người chơi (hoặc bot) cần để cân nhắc một sự kiện (design.md §8).</summary>
    public sealed class EventDecisionContext
    {
        internal EventDecisionContext(
            BalanceConfig config,
            GameState state,
            IRandom random,
            ClimatePhase phase,
            int difficultyLevel,
            double estimatedWinChance)
        {
            Config = config;
            State = state;
            Random = random;
            Phase = phase;
            DifficultyLevel = difficultyLevel;
            EstimatedWinChance = estimatedWinChance;
        }

        public BalanceConfig Config { get; }
        public GameState State { get; }
        public IRandom Random { get; }
        public ClimatePhase Phase { get; }
        public int DifficultyLevel { get; }

        /// <summary>
        /// Tỉ lệ thắng ước lượng của trận đang cân nhắc, theo công thức design.md §8.
        /// Chỉ có nghĩa với Cơ hội combat. Bằng 0 nếu không có nguồn ước lượng (<see cref="ICombatOdds"/>).
        /// </summary>
        public double EstimatedWinChance { get; }
    }

    /// <summary>
    /// Quyết định của người chơi trong sự kiện (design.md §8). Core không tự quyết hộ:
    /// A2 hiện hộp thoại, còn bot ở C8 cài theo đúng before_us_sim.py.
    /// </summary>
    public interface IEventDecisions
    {
        /// <summary>
        /// Sự kiện không combat: người chơi có chọn được nhánh có lợi không.
        ///
        /// Giản lược có chủ đích của MVP, đúng như before_us_sim.py:104: nội dung sự kiện thật
        /// chỉ có ở events.json (I2), nên ở đây một sự kiện chỉ có hai kết quả tốt/xấu.
        /// design.md §8 ghi "người chơi cân nhắc được kết quả tốt khoảng 60%" — con số đó là
        /// thuộc tính của NGƯỜI CHƠI chứ không phải của luật, nên nó nằm ở bên cài interface này,
        /// không nằm trong Core.
        /// </summary>
        bool ChooseGoodOutcome(EventDecisionContext context);

        /// <summary>Cơ hội combat (tùy chọn): true là đánh, false là rút lui (không gì xảy ra).</summary>
        bool AttackOpportunity(EventDecisionContext context);
    }

    /// <summary>Không bao giờ nhận trận tùy chọn, và luôn trượt nhánh có lợi. Dùng làm mốc dưới trong test.</summary>
    public sealed class NeverAttackDecisions : IEventDecisions
    {
        public bool ChooseGoodOutcome(EventDecisionContext context)
        {
            return false;
        }

        public bool AttackOpportunity(EventDecisionContext context)
        {
            return false;
        }
    }

    /// <summary>Quyết định bằng hai hàm truyền vào. Tiện cho test và cho bot đơn giản.</summary>
    public sealed class DelegateEventDecisions : IEventDecisions
    {
        private readonly Func<EventDecisionContext, bool> _chooseGood;
        private readonly Func<EventDecisionContext, bool> _attack;

        public DelegateEventDecisions(
            Func<EventDecisionContext, bool> chooseGood,
            Func<EventDecisionContext, bool> attack)
        {
            _chooseGood = chooseGood ?? (c => false);
            _attack = attack ?? (c => false);
        }

        public bool ChooseGoodOutcome(EventDecisionContext context)
        {
            return _chooseGood(context);
        }

        public bool AttackOpportunity(EventDecisionContext context)
        {
            return _attack(context);
        }
    }
}
