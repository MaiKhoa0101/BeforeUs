using System;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Phần thay đổi theo thời gian của một đơn vị. Lớp hiển thị ĐỌC lớp này mỗi khung hình mà vẽ;
    /// mọi setter đều `internal` nên chỉ <see cref="BattleSimulation"/> được quyền đổi.
    /// </summary>
    public sealed class Combatant
    {
        internal Combatant(CombatantSpec spec)
        {
            Spec = spec ?? throw new ArgumentNullException(nameof(spec));
            Hp = spec.MaxHp;
            Position = spec.SpawnPosition;

            // Cả hai bên đều phải lấy đà cho đòn đầu. Nếu cho đánh ngay từ bước đầu thì bên nào
            // được duyệt trước trong vòng lặp sẽ có một đòn miễn phí — thiên vị do thứ tự mã nguồn,
            // không phải do chỉ số.
            TimeUntilNextAttack = spec.EffectiveAttackInterval;
            LastAttackAtSeconds = -1.0;

            // Thú có mặt trên sân từ đầu; thành viên chờ người chơi thả (design.md §9).
            IsDeployed = spec.Side == CombatSide.Beast;
        }

        public CombatantSpec Spec { get; }

        public int Id
        {
            get { return Spec.Id; }
        }

        public CombatSide Side
        {
            get { return Spec.Side; }
        }

        public double Hp { get; internal set; }

        public double MaxHp
        {
            get { return Spec.MaxHp; }
        }

        public bool IsAlive
        {
            get { return Hp > 0.0; }
        }

        /// <summary>Đã được thả xuống sân chưa. Thành viên chưa thả thì không đánh được và cũng không bị đánh.</summary>
        public bool IsDeployed { get; internal set; }

        /// <summary>Đang thật sự có mặt và còn sống.</summary>
        public bool IsOnField
        {
            get { return IsDeployed && IsAlive; }
        }

        public CombatVector Position { get; internal set; }

        /// <summary>Còn mấy giây nữa ra đòn tiếp. Lớp hiển thị dùng để chạy hoạt ảnh lấy đà.</summary>
        public double TimeUntilNextAttack { get; internal set; }

        /// <summary>Giây thứ mấy của trận thì ra đòn gần nhất; −1 nếu chưa đánh lần nào.</summary>
        public double LastAttackAtSeconds { get; internal set; }

        /// <summary>Id đối thủ đang nhắm, hoặc null. Chỉ để vẽ, không phải trạng thái luật.</summary>
        public int? TargetId { get; internal set; }

        /// <summary>Giây thứ mấy của trận thì ngã xuống; −1 nếu còn sống.</summary>
        public double DiedAtSeconds { get; internal set; } = -1.0;

        public override string ToString()
        {
            return Side + "#" + Id + " hp=" + Hp.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
