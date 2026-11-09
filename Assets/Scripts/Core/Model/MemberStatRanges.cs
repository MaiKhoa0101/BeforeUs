using System;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    /// <summary>
    /// Dải chỉ số khi sinh thành viên (design.md §3). Số lấy từ balance.json khối "member_stats";
    /// không viết cứng ở đây (AGENTS.md).
    /// </summary>
    public sealed class MemberStatRanges
    {
        public MemberStatRanges(double speedMin, double speedMax, double strengthMin, double strengthMax, int hpMin, int hpMax)
        {
            if (speedMin > speedMax || strengthMin > strengthMax || hpMin > hpMax)
            {
                throw new ArgumentException("Dải chỉ số có cận dưới lớn hơn cận trên");
            }

            if (hpMin < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(hpMin), "hp phải >= 1");
            }

            SpeedMin = speedMin;
            SpeedMax = speedMax;
            StrengthMin = strengthMin;
            StrengthMax = strengthMax;
            HpMin = hpMin;
            HpMax = hpMax;
        }

        public double SpeedMin { get; }
        public double SpeedMax { get; }
        public double StrengthMin { get; }
        public double StrengthMax { get; }
        public int HpMin { get; }

        /// <summary>Cận trên, tính cả chính nó.</summary>
        public int HpMax { get; }

        /// <summary>Dải thật, đọc từ balance.json "member_stats" (design.md §3).</summary>
        public static MemberStatRanges FromConfig(BalanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            MemberStatsData m = config.Data.MemberStats;
            return new MemberStatRanges(m.Speed[0], m.Speed[1], m.Strength[0], m.Strength[1], m.Hp[0], m.Hp[1]);
        }

        /// <summary>
        /// Mọi thành viên đúng giá trị trung bình, dùng cho BalanceTests ở C8.
        /// design.md §3: "Giá trị trung bình của các hệ số là 1.0, để cân bằng khớp với mô phỏng
        /// (mô phỏng dùng giá trị trung bình)" — nên speed và strength lấy đúng 1.0.
        ///
        /// hp là GIÁ TRỊ DẪN XUẤT, không phải số của Khoa: lấy trung điểm dải hp trong balance.json
        /// (3–7 → 5). before_us_sim.py không dùng hp bao giờ, nên lựa chọn này không ảnh hưởng số liệu C8;
        /// nó chỉ cần xác định để ván chạy được.
        /// </summary>
        public static MemberStatRanges AverageFromConfig(BalanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            int[] hp = config.Data.MemberStats.Hp;
            int midpoint = (hp[0] + hp[1]) / 2;
            return new MemberStatRanges(1.0, 1.0, 1.0, 1.0, midpoint, midpoint);
        }
    }
}
