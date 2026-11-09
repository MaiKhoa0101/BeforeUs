using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BeforeUs.Core;

namespace BeforeUs.Combat.Calibration
{
    /// <summary>
    /// Đổi các ô đo được thành bảng Markdown cho Khoa đọc.
    /// Mọi ô đều kèm sai số chuẩn: không có nó thì người đọc sẽ đuổi theo nhiễu.
    /// </summary>
    public static class CalibrationReport
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string VietnameseName(CombatKind kind)
        {
            switch (kind)
            {
                case CombatKind.Hunt: return "Săn bắt";
                case CombatKind.Explore: return "Khám phá";
                case CombatKind.Defence: return "Phòng thủ";
                case CombatKind.Opportunity: return "Cơ hội combat";
                default: return kind.ToString();
            }
        }

        /// <summary>Bảng chính: thật so với công thức, kèm độ lệch và sai số chuẩn.</summary>
        public static string WinRateTable(IReadOnlyList<CalibrationCell> cells)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("| Loại trận | k | GĐ | Vùng | Độ khó | Thật | Công thức | Lệch (điểm %) | ± SE | Vượt nhiễu? |");
            sb.AppendLine("|---|--:|--:|--:|--:|--:|--:|--:|--:|:--:|");

            foreach (CalibrationCell c in cells)
            {
                sb.AppendLine(string.Format(
                    Inv,
                    "| {0} | {1} | {2} | {3} | {4} | {5:F1}% | {6:F1}% | {7:+0.0;-0.0} | {8:F1} | {9} |",
                    VietnameseName(c.Kind),
                    c.PartySize,
                    c.Stage,
                    c.Region,
                    c.DifficultyLevel,
                    c.MeasuredWinRate * 100.0,
                    c.FormulaWinChance * 100.0,
                    c.DeltaPoints,
                    c.StandardErrorPoints,
                    c.DeltaIsSignificant ? "có" : "—"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Độ dốc theo k, tính bằng điểm % mỗi người, đặt cạnh con số của công thức.
        /// Đây là con số quyết định của B3 — nó không được chìm trong bảng lớn.
        /// </summary>
        public static string SlopeTable(IReadOnlyList<CalibrationCell> cells)
        {
            List<KeyValuePair<CombatKind, List<CalibrationCell>>> byKind = GroupByKind(cells);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("| Loại trận | Độ dốc thật (điểm %/người) | Độ dốc công thức | Gấp mấy lần |");
            sb.AppendLine("|---|--:|--:|--:|");

            foreach (KeyValuePair<CombatKind, List<CalibrationCell>> entry in byKind)
            {
                double measured = Slope(entry.Value, true);
                double formula = Slope(entry.Value, false);
                double ratio = Math.Abs(formula) < 1e-9 ? 0.0 : measured / formula;

                sb.AppendLine(string.Format(
                    Inv,
                    "| {0} | {1:F1} | {2:F1} | {3:F1}× |",
                    VietnameseName(entry.Key), measured, formula, ratio));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Hồi quy bình phương tối thiểu của tỉ lệ thắng theo k, trả về điểm % mỗi người.
        /// Dùng hồi quy chứ không lấy (đầu − cuối)/(k chênh lệch): khi đường cong đã dính trần
        /// 0% hoặc 100% ở hai đầu, phép trừ hai đầu cho một con số vô nghĩa.
        /// </summary>
        public static double Slope(IReadOnlyList<CalibrationCell> cells, bool measured)
        {
            int n = 0;
            double sumX = 0.0, sumY = 0.0, sumXy = 0.0, sumXx = 0.0;

            for (int i = 0; i < cells.Count; i++)
            {
                double x = cells[i].PartySize;
                double y = (measured ? cells[i].MeasuredWinRate : cells[i].FormulaWinChance) * 100.0;

                n++;
                sumX += x;
                sumY += y;
                sumXy += x * y;
                sumXx += x * x;
            }

            double denominator = (n * sumXx) - (sumX * sumX);
            return Math.Abs(denominator) < 1e-9 ? 0.0 : ((n * sumXy) - (sumX * sumY)) / denominator;
        }

        /// <summary>Cách trận kết thúc, tách theo kiểu trận. Tỉ lệ hết giờ là con số cần theo dõi.</summary>
        public static string EndReasonTable(IReadOnlyList<CalibrationCell> cells)
        {
            List<KeyValuePair<CombatKind, List<CalibrationCell>>> byKind = GroupByKind(cells);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("| Loại trận | Kiểu | Hạ hết thú | Thú thoát | Chạm mục tiêu | **Hết giờ** | Hết người | Giây/trận |");
            sb.AppendLine("|---|---|--:|--:|--:|--:|--:|--:|");

            foreach (KeyValuePair<CombatKind, List<CalibrationCell>> entry in byKind)
            {
                int battles = 0, down = 0, escaped = 0, reached = 0, timeout = 0, wiped = 0;
                double seconds = 0.0;

                foreach (CalibrationCell c in entry.Value)
                {
                    battles += c.Battles;
                    down += c.EndedAllBeastsDown;
                    escaped += c.EndedBeastEscaped;
                    reached += c.EndedTargetReached;
                    timeout += c.EndedTimeLimit;
                    wiped += c.EndedTribeDown;
                    seconds += c.AverageSeconds * c.Battles;
                }

                bool lure = entry.Key == CombatKind.Hunt || entry.Key == CombatKind.Opportunity;

                sb.AppendLine(string.Format(
                    Inv,
                    "| {0} | {1} | {2:F1}% | {3:F1}% | {4:F1}% | **{5:F1}%** | {6:F1}% | {7:F1} |",
                    VietnameseName(entry.Key),
                    lure ? "Dụ và hạ" : "Phòng thủ",
                    Percent(down, battles),
                    Percent(escaped, battles),
                    Percent(reached, battles),
                    Percent(timeout, battles),
                    Percent(wiped, battles),
                    battles == 0 ? 0.0 : seconds / battles));
            }

            return sb.ToString();
        }

        /// <summary>Tổn thất: thắng mà vẫn mất người (§5.2 nói trung bình 50%), và số lần kẹp D11.</summary>
        public static string CasualtyTable(IReadOnlyList<CalibrationCell> cells)
        {
            List<KeyValuePair<CombatKind, List<CalibrationCell>>> byKind = GroupByKind(cells);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("| Loại trận | Thắng mà vẫn mất người | §5.2 nói | Người chết/trận | Kẹp D11 |");
            sb.AppendLine("|---|--:|--:|--:|--:|");

            foreach (KeyValuePair<CombatKind, List<CalibrationCell>> entry in byKind)
            {
                int wins = 0, winsWithDeaths = 0, deaths = 0, battles = 0, floor = 0;
                foreach (CalibrationCell c in entry.Value)
                {
                    wins += c.Wins;
                    winsWithDeaths += c.WinsWithDeaths;
                    deaths += c.TotalDeaths;
                    battles += c.Battles;
                    floor += c.LastSurvivorFloorHits;
                }

                sb.AppendLine(string.Format(
                    Inv,
                    "| {0} | {1:F1}% | 50% | {2:F2} | {3} lần |",
                    VietnameseName(entry.Key),
                    Percent(winsWithDeaths, wins),
                    battles == 0 ? 0.0 : deaths / (double)battles,
                    floor));
            }

            return sb.ToString();
        }

        private static double Percent(int part, int whole)
        {
            return whole == 0 ? 0.0 : 100.0 * part / whole;
        }

        /// <summary>
        /// Gom theo loại trận, GIỮ NGUYÊN thứ tự xuất hiện. Trả về danh sách cặp chứ không phải
        /// Dictionary: thứ tự duyệt Dictionary không được bảo đảm, mà bảng báo cáo đổi thứ tự dòng
        /// giữa hai lần chạy là thứ làm người đọc tưởng số liệu đổi.
        /// </summary>
        private static List<KeyValuePair<CombatKind, List<CalibrationCell>>> GroupByKind(
            IReadOnlyList<CalibrationCell> cells)
        {
            List<KeyValuePair<CombatKind, List<CalibrationCell>>> groups =
                new List<KeyValuePair<CombatKind, List<CalibrationCell>>>();

            for (int i = 0; i < cells.Count; i++)
            {
                List<CalibrationCell> bucket = null;
                for (int g = 0; g < groups.Count; g++)
                {
                    if (groups[g].Key == cells[i].Kind)
                    {
                        bucket = groups[g].Value;
                        break;
                    }
                }

                if (bucket == null)
                {
                    bucket = new List<CalibrationCell>();
                    groups.Add(new KeyValuePair<CombatKind, List<CalibrationCell>>(cells[i].Kind, bucket));
                }

                bucket.Add(cells[i]);
            }

            return groups;
        }
    }
}
