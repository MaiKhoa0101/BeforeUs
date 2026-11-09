using System;
using BeforeUs.Core;

namespace BeforeUs.Combat.Calibration
{
    /// <summary>
    /// Kết quả đo của MỘT ô trong bảng hiệu chỉnh: một tổ hợp (loại trận, k, giai đoạn, độ khó)
    /// chạy N trận.
    ///
    /// Con số so sánh lấy từ <c>ICombatOdds.WinChance</c> của Core, KHÔNG phải từ một bản chép tay
    /// công thức §5.2/§8 — đó là toàn bộ lý do Lead Core phơi hàm đó ra cho B3.
    /// </summary>
    public sealed class CalibrationCell
    {
        public CombatKind Kind { get; set; }
        public int PartySize { get; set; }
        public int Stage { get; set; }
        public int Region { get; set; }
        public int BuildLevel { get; set; }
        public int DifficultyLevel { get; set; }
        public double TimeLimitSeconds { get; set; }

        public int Battles { get; set; }
        public int Wins { get; set; }

        /// <summary>Tỉ lệ thắng của trận mô phỏng, đo được.</summary>
        public double MeasuredWinRate
        {
            get { return Battles == 0 ? 0.0 : Wins / (double)Battles; }
        }

        /// <summary>Tỉ lệ thắng theo công thức, lấy từ Core.</summary>
        public double FormulaWinChance { get; set; }

        /// <summary>Độ lệch, tính bằng ĐIỂM PHẦN TRĂM. Cổng 3 đòi trị tuyệt đối ≤ 10.</summary>
        public double DeltaPoints
        {
            get { return (MeasuredWinRate - FormulaWinChance) * 100.0; }
        }

        /// <summary>
        /// Sai số chuẩn của tỉ lệ đo được, tính bằng điểm phần trăm: 100·√(p(1−p)/n).
        /// Với 500 trận và tỉ lệ quanh 50% thì vào khoảng 2,2 điểm — nghĩa là chênh 3–4 điểm
        /// giữa hai lần chạy là NHIỄU, không phải tín hiệu. Đừng chỉnh đuổi theo nó.
        /// </summary>
        public double StandardErrorPoints
        {
            get
            {
                if (Battles <= 0)
                {
                    return 0.0;
                }

                double p = MeasuredWinRate;
                return 100.0 * Math.Sqrt(Math.Max(0.0, p * (1.0 - p)) / Battles);
            }
        }

        /// <summary>Độ lệch có vượt quá nhiễu đo không (2 sai số chuẩn).</summary>
        public bool DeltaIsSignificant
        {
            get { return Math.Abs(DeltaPoints) > 2.0 * StandardErrorPoints; }
        }

        // --- Phân bố cách kết thúc ---------------------------------------------------------

        public int EndedAllBeastsDown { get; set; }
        public int EndedBeastEscaped { get; set; }
        public int EndedTargetReached { get; set; }

        /// <summary>Số trận kết thúc vì hết giờ. Con số này cao là dấu hiệu mô phỏng có vấn đề.</summary>
        public int EndedTimeLimit { get; set; }

        public int EndedTribeDown { get; set; }

        public double TimeLimitShare
        {
            get { return Battles == 0 ? 0.0 : EndedTimeLimit / (double)Battles; }
        }

        public double AverageSeconds { get; set; }

        // --- Tổn thất ----------------------------------------------------------------------

        /// <summary>
        /// Số trận THẮNG mà vẫn mất người. design.md §5.2 ghi thắng vẫn có thể mất 1 người,
        /// trung bình 50% — con số này phải tự mọc ra từ trận đánh, nên nó cũng là thứ cần đo.
        /// </summary>
        public int WinsWithDeaths { get; set; }

        public double WinDeathShare
        {
            get { return Wins == 0 ? 0.0 : WinsWithDeaths / (double)Wins; }
        }

        public int TotalDeaths { get; set; }

        /// <summary>
        /// Số lần kẹp D11 phải ra tay (phòng thủ thua mà mô phỏng định giết quá nhiều).
        /// Cao nghĩa là mô phỏng đang đánh nặng tay hơn luật rất nhiều.
        /// </summary>
        public int LastSurvivorFloorHits { get; set; }
    }
}
