using System;
using System.Collections.Generic;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Đầu vào bất biến của một trận. Dựng từ `CombatRequest` của Core cộng với số liệu
    /// Assets/Data/beasts.json (B2); <see cref="BattleSimulation"/> không biết gì về vòng lượt.
    /// </summary>
    public sealed class BattleSetup
    {
        public BattleSetup(
            BattleKind kind,
            BattleField field,
            IReadOnlyList<CombatantSpec> tribe,
            IReadOnlyList<CombatantSpec> beasts,
            double timeLimitSeconds,
            BattleOptions options = null)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (tribe == null) throw new ArgumentNullException(nameof(tribe));
            if (beasts == null) throw new ArgumentNullException(nameof(beasts));
            if (beasts.Count == 0) throw new ArgumentException("Trận phải có ít nhất một con thú", nameof(beasts));
            if (timeLimitSeconds <= 0.0) throw new ArgumentOutOfRangeException(nameof(timeLimitSeconds));

            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < tribe.Count; i++)
            {
                CombatantSpec spec = tribe[i];
                if (spec == null) throw new ArgumentException("tribe chứa phần tử null", nameof(tribe));
                if (spec.Side != CombatSide.Tribe) throw new ArgumentException("tribe chứa đơn vị không thuộc bộ tộc", nameof(tribe));
                if (!seen.Add(spec.Id)) throw new ArgumentException("id thành viên trùng: " + spec.Id, nameof(tribe));
            }

            for (int i = 0; i < beasts.Count; i++)
            {
                CombatantSpec spec = beasts[i];
                if (spec == null) throw new ArgumentException("beasts chứa phần tử null", nameof(beasts));
                if (spec.Side != CombatSide.Beast) throw new ArgumentException("beasts chứa đơn vị không phải thú", nameof(beasts));

                // Thú sinh ra đã ở đích thì trận kết thúc trước khi ai kịp làm gì — gần như luôn là
                // lỗi dựng sân. Nổ ở đây dễ sửa hơn nhiều so với một bảng B3 toàn số 0%.
                if (ReachedGoal(kind, field, spec.SpawnPosition.X))
                {
                    throw new ArgumentException(
                        "Thú #" + spec.Id + " sinh ra đã ở đích (x=" + spec.SpawnPosition.X + ", goalX=" + field.GoalX + ")",
                        nameof(beasts));
                }
            }

            Kind = kind;
            Field = field;
            Tribe = tribe;
            Beasts = beasts;
            TimeLimitSeconds = timeLimitSeconds;
            Options = options ?? BattleOptions.Default;
        }

        public BattleKind Kind { get; }

        public BattleField Field { get; }

        /// <summary>Những người được cử đi. Người chơi thả từng người xuống sân trong lúc trận chạy.</summary>
        public IReadOnlyList<CombatantSpec> Tribe { get; }

        public IReadOnlyList<CombatantSpec> Beasts { get; }

        /// <summary>design.md §9: một trận kéo dài khoảng 20–40 giây.</summary>
        public double TimeLimitSeconds { get; }

        /// <summary>Đòn bẩy thử nghiệm. Mặc định <see cref="BattleOptions.Default"/> — không bật gì.</summary>
        public BattleOptions Options { get; }

        /// <summary>
        /// Kiểu tấn công của trận, lấy từ con thú đầu tiên. MVP mỗi trận chỉ có một loài
        /// (design.md §9 gắn một kiểu tấn công cho mỗi loài), nên một con số là đủ.
        /// </summary>
        public BeastAttackStyle AttackStyle
        {
            get { return Beasts[0].AttackStyle; }
        }

        /// <summary>Thú ở toạ độ x này đã tới đích chưa.</summary>
        internal static bool ReachedGoal(BattleKind kind, BattleField field, double x)
        {
            return kind == BattleKind.Lure
                ? x >= field.GoalX - field.GoalRadius
                : x <= field.GoalX + field.GoalRadius;
        }
    }
}
