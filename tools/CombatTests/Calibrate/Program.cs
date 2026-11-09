using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BeforeUs.Combat;
using BeforeUs.Combat.Calibration;
using BeforeUs.Core;
using BeforeUs.Core.Config;

namespace BeforeUs.Combat.Calibrate
{
    /// <summary>
    /// Công cụ hiệu chỉnh combat, HANDOFF.md B3.
    ///   dotnet run --project tools/CombatTests/Calibrate            → 500 trận mỗi ô
    ///   dotnet run --project tools/CombatTests/Calibrate -- 200     → nhanh hơn, để lặp khi chỉnh số
    ///
    /// In Markdown ra màn hình. Nó KHÔNG tự sửa beasts.json và không bao giờ đụng balance.json:
    /// người chỉnh số là người đọc bảng.
    /// </summary>
    public static class Program
    {
        private static readonly CombatKind[] Kinds =
        {
            CombatKind.Hunt, CombatKind.Explore, CombatKind.Defence, CombatKind.Opportunity
        };

        private const int BaseSeed = 20261108;

        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            int battles = 500;
            if (args.Length > 0 && !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out battles))
            {
                Console.Error.WriteLine("Tham số đầu phải là số trận mỗi ô.");
                return 2;
            }

            string root = FindRepoRoot();
            BalanceConfig config = BalanceConfig.FromJson(
                File.ReadAllText(Path.Combine(root, "Assets", "Data", "balance.json")));
            BeastCatalog catalog = new BeastCatalog(BeastCatalog.LoadData(
                File.ReadAllText(Path.Combine(root, "Assets", "Data", "beasts.json"))));

            CombatCalibrator calibrator = new CombatCalibrator(config, catalog);

            Console.WriteLine("# Hiệu chỉnh combat — B3");
            Console.WriteLine();
            Console.WriteLine(
                battles + " trận mỗi ô, thả quân ngẫu nhiên, seed gốc " + BaseSeed + ".");
            Console.WriteLine("Con số \"công thức\" lấy từ `ProbabilisticCombatResolver.WinChance` của Core.");
            Console.WriteLine();

            List<CalibrationCell> baseline = Grid(calibrator, battles);

            Console.WriteLine("## 1. Tỉ lệ thắng thật so với công thức");
            Console.WriteLine();
            Console.WriteLine(CalibrationReport.WinRateTable(baseline));
            Console.WriteLine(Verdict(baseline));
            Console.WriteLine();

            Console.WriteLine("## 2. Độ dốc theo k — con số quyết định");
            Console.WriteLine();
            Console.WriteLine(CalibrationReport.SlopeTable(SlopeSlice(baseline)));
            Console.WriteLine();

            Console.WriteLine("## 3. Cách trận kết thúc");
            Console.WriteLine();
            Console.WriteLine(CalibrationReport.EndReasonTable(baseline));
            Console.WriteLine();

            Console.WriteLine("## 4. Tổn thất");
            Console.WriteLine();
            Console.WriteLine(CalibrationReport.CasualtyTable(baseline));
            Console.WriteLine();

            Console.WriteLine("## 5. Độ nhạy thời lượng trận");
            Console.WriteLine();
            Console.WriteLine(TimeSensitivity(calibrator, battles));
            Console.WriteLine();

            Console.WriteLine("## 6. Hai đòn bẩy làm thoải đường cong");
            Console.WriteLine();
            Console.WriteLine(Levers(calibrator, battles));

            return 0;
        }

        /// <summary>Lưới chính: k × giai đoạn × độ khó, cả bốn loại trận.</summary>
        private static List<CalibrationCell> Grid(CombatCalibrator calibrator, int battles)
        {
            List<CalibrationCell> cells = new List<CalibrationCell>();
            int seed = BaseSeed;

            foreach (CombatKind kind in Kinds)
            {
                // Giai đoạn 2 là giai đoạn MVP chơi thật lâu nhất (Săn bắt mở ở đó, thắng ở giai
                // đoạn 3), nên quét k đầy đủ ở giai đoạn 2 rồi mới đối chiếu giai đoạn 1 và 3.
                for (int k = 1; k <= 5; k++)
                {
                    cells.Add(calibrator.Measure(kind, k, 2, 0, 0, 0, battles, seed++));
                }

                foreach (int stage in new[] { 1, 3 })
                {
                    cells.Add(calibrator.Measure(kind, 3, stage, 0, 0, 0, battles, seed++));
                }

                foreach (int level in new[] { 1, 2, 4 })
                {
                    cells.Add(calibrator.Measure(kind, 3, 2, 0, 0, level, battles, seed++));
                }

                // Vùng 1 (savan) phải có mặt: linh cẩu và hổ răng kiếm CHỈ sống ở đó, nên lưới
                // chỉ đo vùng 0 sẽ không bao giờ chạm tới hai loài trong số ba loài của §9.
                cells.Add(calibrator.Measure(kind, 3, 2, 1, 0, 0, battles, seed++));
            }

            return cells;
        }

        /// <summary>Chỉ lấy các ô quét theo k ở cùng điều kiện, để hồi quy độ dốc có nghĩa.</summary>
        private static List<CalibrationCell> SlopeSlice(IReadOnlyList<CalibrationCell> cells)
        {
            List<CalibrationCell> slice = new List<CalibrationCell>();
            foreach (CalibrationCell c in cells)
            {
                if (c.Stage == 2 && c.DifficultyLevel == 0 && c.BuildLevel == 0 && c.Region == 0)
                {
                    slice.Add(c);
                }
            }

            return slice;
        }

        private static string TimeSensitivity(CombatCalibrator calibrator, int battles)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Thời lượng là câu hỏi về CẢM GIÁC và thuộc B4 của Khoa. Bảng này chỉ cho biết");
            sb.AppendLine("cái giá cân bằng của mỗi lựa chọn (k = 3, giai đoạn 2, độ khó 0).");
            sb.AppendLine();
            sb.AppendLine("| Loại trận | 20 giây | 30 giây | 40 giây | Công thức |");
            sb.AppendLine("|---|--:|--:|--:|--:|");

            int seed = BaseSeed + 5000;
            foreach (CombatKind kind in Kinds)
            {
                CalibrationCell at20 = calibrator.Measure(kind, 3, 2, 0, 0, 0, battles, seed, 20.0);
                CalibrationCell at30 = calibrator.Measure(kind, 3, 2, 0, 0, 0, battles, seed, 30.0);
                CalibrationCell at40 = calibrator.Measure(kind, 3, 2, 0, 0, 0, battles, seed, 40.0);
                seed++;

                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "| {0} | {1:F1}% | {2:F1}% | {3:F1}% | {4:F1}% |",
                    CalibrationReport.VietnameseName(kind),
                    at20.MeasuredWinRate * 100.0,
                    at30.MeasuredWinRate * 100.0,
                    at40.MeasuredWinRate * 100.0,
                    at30.FormulaWinChance * 100.0));
            }

            return sb.ToString();
        }

        private static string Levers(CombatCalibrator calibrator, int battles)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Cả hai MẶC ĐỊNH TẮT. Bảng này đo chúng kéo độ dốc về bao nhiêu và giá phải trả.");
            sb.AppendLine("Độ dốc đo bằng hồi quy trên k = 1..5, giai đoạn 2, độ khó 0.");
            sb.AppendLine();
            sb.AppendLine("| Cấu hình | Săn bắt | Khám phá | Phòng thủ | Cơ hội | Trung bình |");
            sb.AppendLine("|---|--:|--:|--:|--:|--:|");

            sb.AppendLine(LeverRow(calibrator, battles, "Tắt hết (mặc định)", null, BaseSeed + 9000));
            sb.AppendLine(LeverRow(calibrator, battles, "(A) tối đa 1 người/thú", new BattleOptions(1), BaseSeed + 9100));
            sb.AppendLine(LeverRow(calibrator, battles, "(A) tối đa 2 người/thú", new BattleOptions(2), BaseSeed + 9200));
            sb.AppendLine(LeverRow(calibrator, battles, "(A) tối đa 3 người/thú", new BattleOptions(3), BaseSeed + 9300));

            sb.AppendLine();
            sb.AppendLine("Công thức §5.2 cho +8,0 điểm/người với Săn bắt và Khám phá, §8 cho +5,0 với");
            sb.AppendLine("Phòng thủ và Cơ hội combat.");
            sb.AppendLine();
            sb.AppendLine("> Đòn bẩy (B) — phương sai chỉ số thú — chỉnh bằng `levers.beast_stat_variance`");
            sb.AppendLine("> trong beasts.json chứ không qua cờ chạy, vì nó đổi cách DỰNG trận chứ không");
            sb.AppendLine("> đổi cách chạy trận. Đặt giá trị rồi chạy lại công cụ này để xem bảng mới.");

            return sb.ToString();
        }

        private static string LeverRow(
            CombatCalibrator calibrator, int battles, string label, BattleOptions options, int seed)
        {
            StringBuilder row = new StringBuilder();
            row.Append("| ").Append(label).Append(" |");

            double total = 0.0;
            foreach (CombatKind kind in Kinds)
            {
                List<CalibrationCell> cells = new List<CalibrationCell>();
                for (int k = 1; k <= 5; k++)
                {
                    cells.Add(calibrator.Measure(kind, k, 2, 0, 0, 0, battles, seed + k, 0.0, options));
                }

                double slope = CalibrationReport.Slope(cells, true);
                total += slope;
                row.Append(string.Format(CultureInfo.InvariantCulture, " {0:F1} |", slope));
                seed += 10;
            }

            row.Append(string.Format(CultureInfo.InvariantCulture, " {0:F1} |", total / Kinds.Length));
            return row.ToString();
        }

        private static string Verdict(IReadOnlyList<CalibrationCell> cells)
        {
            int over = 0;
            double worst = 0.0;
            CalibrationCell worstCell = null;

            foreach (CalibrationCell c in cells)
            {
                double delta = Math.Abs(c.DeltaPoints);
                if (delta > 10.0)
                {
                    over++;
                }

                if (delta > worst)
                {
                    worst = delta;
                    worstCell = c;
                }
            }

            if (over == 0)
            {
                return "**Đạt Cổng 3:** mọi ô lệch ≤ 10 điểm %.";
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "**KHÔNG đạt Cổng 3:** {0}/{1} ô lệch quá 10 điểm %. Tệ nhất {2:F1} điểm ({3}, k={4}, GĐ={5}, vùng={6}, độ khó={7}).",
                over, cells.Count, worst,
                worstCell == null ? "?" : CalibrationReport.VietnameseName(worstCell.Kind),
                worstCell == null ? 0 : worstCell.PartySize,
                worstCell == null ? 0 : worstCell.Stage,
                worstCell == null ? 0 : worstCell.Region,
                worstCell == null ? 0 : worstCell.DifficultyLevel);
        }

        private static string FindRepoRoot()
        {
            string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };

            foreach (string start in starts)
            {
                DirectoryInfo dir = new DirectoryInfo(start);
                while (dir != null)
                {
                    if (File.Exists(Path.Combine(dir.FullName, "Assets", "Data", "balance.json")))
                    {
                        return dir.FullName;
                    }

                    dir = dir.Parent;
                }
            }

            throw new InvalidOperationException("Không tìm thấy gốc dự án.");
        }
    }
}
