using System;
using System.Collections.Generic;

namespace BeforeUs.Core
{
    /// <summary>
    /// Một thành viên bộ tộc (design.md §3). Chỉ số sinh ra ngẫu nhiên và cố định suốt đời:
    /// không có cơ chế bị thương, nên không có setter nào ở đây.
    /// </summary>
    public sealed class Member
    {
        public Member(int id, double speed, double strength, int hp, IReadOnlyList<string> traits)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "id thành viên đếm từ 1");
            }

            if (hp < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(hp), "hp phải >= 1");
            }

            Id = id;
            Speed = speed;
            Strength = strength;
            Hp = hp;
            Traits = traits ?? EmptyTraits;
        }

        private static readonly string[] EmptyTraits = new string[0];

        public int Id { get; }

        /// <summary>Nhân vào lượng hái lượm của người đó và tốc độ đánh trong combat (design.md §3, §5.1).</summary>
        public double Speed { get; }

        /// <summary>Nhân vào sát thương trong combat (design.md §3).</summary>
        public double Strength { get; }

        /// <summary>Máu trong combat. Quái thú kiểu "săn kẻ yếu" nhắm người có hp thấp nhất (design.md §9).</summary>
        public int Hp { get; }

        /// <summary>Đặc điểm ngoại hình, dạng "nhóm:lựa chọn". Core không diễn giải; F1 dùng để ghép chân dung.</summary>
        public IReadOnlyList<string> Traits { get; }

        public override string ToString()
        {
            return "Member#" + Id;
        }
    }
}
