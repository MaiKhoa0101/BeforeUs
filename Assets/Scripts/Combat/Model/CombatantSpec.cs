using System;
using BeforeUs.Core;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Mô tả bất biến của một đơn vị trước khi trận bắt đầu. <see cref="Combatant"/> là phần
    /// thay đổi theo thời gian; tách đôi để lớp hiển thị biết cái gì đứng yên, cái gì phải vẽ lại.
    /// </summary>
    public sealed class CombatantSpec
    {
        public CombatantSpec(
            int id,
            CombatSide side,
            double speed,
            double strength,
            double maxHp,
            double moveSpeed,
            double attackDamage,
            double attackInterval,
            double attackRange,
            BeastAttackStyle attackStyle = BeastAttackStyle.Random,
            string speciesId = null,
            CombatVector spawnPosition = default)
        {
            if (speed <= 0.0) throw new ArgumentOutOfRangeException(nameof(speed), "speed phải > 0");
            if (strength <= 0.0) throw new ArgumentOutOfRangeException(nameof(strength), "strength phải > 0");
            if (maxHp <= 0.0) throw new ArgumentOutOfRangeException(nameof(maxHp), "maxHp phải > 0");
            if (moveSpeed <= 0.0) throw new ArgumentOutOfRangeException(nameof(moveSpeed));
            if (attackDamage <= 0.0) throw new ArgumentOutOfRangeException(nameof(attackDamage));
            if (attackInterval <= 0.0) throw new ArgumentOutOfRangeException(nameof(attackInterval));
            if (attackRange <= 0.0) throw new ArgumentOutOfRangeException(nameof(attackRange));

            Id = id;
            Side = side;
            Speed = speed;
            Strength = strength;
            MaxHp = maxHp;
            MoveSpeed = moveSpeed;
            AttackDamage = attackDamage;
            AttackInterval = attackInterval;
            AttackRange = attackRange;
            AttackStyle = attackStyle;
            SpeciesId = speciesId;
            SpawnPosition = spawnPosition;
        }

        /// <summary>Với bên bộ tộc: đúng `Member.Id` của Core. Với thú: số thứ tự trong trận, đếm từ 0.</summary>
        public int Id { get; }

        public CombatSide Side { get; }

        /// <summary>Hệ số design.md §3. Nhân vào tốc độ di chuyển, chia vào khoảng cách giữa hai đòn.</summary>
        public double Speed { get; }

        /// <summary>Hệ số design.md §3. Nhân vào sát thương.</summary>
        public double Strength { get; }

        public double MaxHp { get; }

        public double MoveSpeed { get; }

        public double AttackDamage { get; }

        public double AttackInterval { get; }

        public double AttackRange { get; }

        /// <summary>Chỉ có nghĩa với bên thú (design.md §9).</summary>
        public BeastAttackStyle AttackStyle { get; }

        /// <summary>Id loài trong beasts.json. Null với thành viên bộ tộc.</summary>
        public string SpeciesId { get; }

        /// <summary>Thú sinh ra ở đâu. Với thành viên thì bỏ qua — người chơi tự thả (design.md §9).</summary>
        public CombatVector SpawnPosition { get; }

        /// <summary>Tốc độ di chuyển thật, đã tính hệ số riêng.</summary>
        public double EffectiveMoveSpeed
        {
            get { return MoveSpeed * Speed; }
        }

        /// <summary>Sát thương thật mỗi đòn, đã tính hệ số riêng.</summary>
        public double EffectiveAttackDamage
        {
            get { return AttackDamage * Strength; }
        }

        /// <summary>Số giây thật giữa hai đòn: `speed` cao thì đánh dày hơn (design.md §3).</summary>
        public double EffectiveAttackInterval
        {
            get { return AttackInterval / Speed; }
        }

        /// <summary>
        /// Dựng đơn vị cho một thành viên bộ tộc: chỉ số nền lấy từ hồ sơ, hệ số riêng lấy từ
        /// <see cref="Member"/> của Core. `hp` của §3 là số nguyên 3–7 và dùng thẳng làm máu.
        /// </summary>
        public static CombatantSpec ForMember(Member member, TribeCombatProfile profile)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            return new CombatantSpec(
                member.Id,
                CombatSide.Tribe,
                member.Speed,
                member.Strength,
                member.Hp,
                profile.MoveSpeed,
                profile.AttackDamage,
                profile.AttackInterval,
                profile.AttackRange);
        }
    }
}
