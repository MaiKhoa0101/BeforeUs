using System;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Chỉ số đánh nhau NỀN của một thành viên bộ tộc, trước khi nhân hệ số riêng của từng người.
    ///
    /// design.md §3 chỉ cho hệ số: `speed` nhân vào tốc độ đánh, `strength` nhân vào sát thương.
    /// Bốn con số dưới đây là cái mà hệ số đó nhân vào — design.md không cho chúng, nên chúng là
    /// [ĐỀ XUẤT] và sống trong Assets/Data/beasts.json khối "tribe" (B2), không viết cứng trong mã.
    /// Đây đúng là những số B3 được phép chỉnh để tỉ lệ thắng thật bám công thức §5.2/§8.
    /// </summary>
    public sealed class TribeCombatProfile
    {
        public TribeCombatProfile(double moveSpeed, double attackDamage, double attackInterval, double attackRange)
        {
            if (moveSpeed <= 0.0) throw new ArgumentOutOfRangeException(nameof(moveSpeed));
            if (attackDamage <= 0.0) throw new ArgumentOutOfRangeException(nameof(attackDamage));
            if (attackInterval <= 0.0) throw new ArgumentOutOfRangeException(nameof(attackInterval));
            if (attackRange <= 0.0) throw new ArgumentOutOfRangeException(nameof(attackRange));

            MoveSpeed = moveSpeed;
            AttackDamage = attackDamage;
            AttackInterval = attackInterval;
            AttackRange = attackRange;
        }

        /// <summary>Đơn vị sân mỗi giây, trước khi nhân `speed` của thành viên.</summary>
        public double MoveSpeed { get; }

        /// <summary>Sát thương mỗi đòn, trước khi nhân `strength` của thành viên.</summary>
        public double AttackDamage { get; }

        /// <summary>Giây giữa hai đòn, trước khi chia cho `speed` của thành viên.</summary>
        public double AttackInterval { get; }

        public double AttackRange { get; }
    }
}
