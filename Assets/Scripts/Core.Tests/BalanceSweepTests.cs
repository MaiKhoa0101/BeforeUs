using System;
using System.Collections.Generic;
using System.Globalization;
using BeforeUs.Core.Config;
using BeforeUs.Core.Plants;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// Vòng chỉnh cân bằng (Khoa ủy quyền 2026-11-09). Mỗi test ở đây là một PHÉP ĐO, không
    /// phải một chốt chặn: nó quét một khóa qua nhiều giá trị và in bảng, rồi `docs/balance-log.md`
    /// ghi lại giá trị nào được chọn và vì sao.
    ///
    /// Cố ý KHÔNG có assertion về dải Cổng 2 ở đây — việc đó là của BalanceTests. Test ở đây chỉ
    /// bảo đảm phép quét chạy được và cho số khác nhau, để một ngày nào đó nó hỏng thì ta biết.
    /// </summary>
    [TestFixture]
    [Category("sweep")]
    [Explicit("Công cụ ĐO, không phải chốt chặn: mỗi test chạy hàng nghìn ván. Gọi đích danh khi cần một vòng chỉnh cân bằng.")]
    public sealed class BalanceSweepTests
    {
        /// <summary>Ít ván hơn C8 để quét nhanh. Sai số chuẩn ≈ 2,2 điểm % ở 500 ván.</summary>
        private const int SweepGames = 500;

        private static BalanceStats Measure(BalanceConfig config, BotStyle style, PlantCatalog plants = null)
        {
            return BalanceSimulation.Run(
                config,
                new BotOptions { Style = style, ApplyNodeEffects = false, Plants = plants },
                style.ToString(),
                SweepGames);
        }

        /// <summary>
        /// Nhân mọi `find_chance` và/hoặc `guaranteed_total_speed` trong plants.json với một hệ số,
        /// TRONG BỘ NHỚ. File trên đĩa không bị chạm — đây là ĐO, không phải ĐỔI.
        /// </summary>
        private static PlantCatalog ScaledPlants(double findScale, double guaranteeScale)
        {
            string json = TestPaths.ReadPlantsJson();
            json = Rescale(json, "find_chance", findScale, 1.0);
            json = Rescale(json, "guaranteed_total_speed", guaranteeScale, double.MaxValue);
            return PlantCatalog.Load(json);
        }

        private static string Rescale(string json, string key, double scale, double cap)
        {
            if (scale == 1.0)
            {
                return scale == 1.0 ? json : json;
            }

            return System.Text.RegularExpressions.Regex.Replace(
                json,
                "\"" + key + "\": ([0-9.]+)",
                m =>
                {
                    double v = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) * scale;
                    if (v > cap) v = cap;
                    return "\"" + key + "\": " + v.ToString("0.0000", CultureInfo.InvariantCulture);
                });
        }

        private static string Row(string label, BalanceStats s)
        {
            return label.PadRight(25)
                   + s.WinRateText.PadRight(16)
                   + Num(s.MedianWinTurns).PadRight(9)
                   + Num(s.P10WinTurns).PadRight(7)
                   + Num(s.P90WinTurns).PadRight(8)
                   + (s.EarlyLossRate5 * 100).ToString("F1", CultureInfo.InvariantCulture).PadRight(7)
                   + (s.EarlyLossRate10 * 100).ToString("F1", CultureInfo.InvariantCulture);
        }

        private const string Header = "ô                        balanced        trung vị p10    p90     <5%    <10%";

        private static string Num(double v)
        {
            return v <= 0 ? "-" : v.ToString("F1", CultureInfo.InvariantCulture);
        }

        [Test]
        public void Do_BotCoNangOThucAnKhong()
        {
            // Ba bậc Ổ đựng thức ăn (0,2 / 0,5 / 1,0) là [ĐỀ XUẤT] chưa ai đo. Nếu bot balanced
            // gần như không bao giờ nâng ổ thì cả hệ D3 nằm NGOÀI mọi phép đo của C8 — và nó sẽ
            // gặp người thật lần đầu vào đúng hôm Khoa chơi.
            //
            // CHỈ ĐO, không chỉnh gì dựa trên kết quả này.
            BalanceConfig config = TestOverrides.StandardConfig();
            int[] tierCount = new int[4];
            int sitesCleared = 0;
            int wins = 0;

            for (int i = 0; i < SweepGames; i++)
            {
                BotGame game = new BotGame(
                    config, new BotOptions { Style = BotStyle.Balanced, ApplyNodeEffects = false },
                    BalanceSimulation.BaseSeed + i);

                GameResult r = game.Play();
                if (r.Won)
                {
                    wins++;
                }

                int tier = game.State.FoodNestTier;
                tierCount[tier < 0 ? 0 : (tier > 3 ? 3 : tier)]++;
                sitesCleared += game.SitesClearedEver;
            }

            Console.WriteLine("=== Ổ đựng thức ăn: bot balanced có nâng không? (" + SweepGames + " ván) ===");
            for (int t = 0; t <= 3; t++)
            {
                Console.WriteLine("  bậc " + t + " : " + tierCount[t] + " ván  ("
                                  + (tierCount[t] * 100.0 / SweepGames).ToString("F1", CultureInfo.InvariantCulture) + "%)");
            }

            int nangIt = SweepGames - tierCount[0];
            Console.WriteLine("  có nâng ít nhất một bậc : "
                              + (nangIt * 100.0 / SweepGames).ToString("F1", CultureInfo.InvariantCulture) + "%");
            Console.WriteLine("  công trình dọn TB/ván   : "
                              + (sitesCleared / (double)SweepGames).ToString("F2", CultureInfo.InvariantCulture));
            Console.WriteLine("  thắng                   : "
                              + (wins * 100.0 / SweepGames).ToString("F1", CultureInfo.InvariantCulture) + "%");
        }

        [Test]
        public void Quet_HeSoBaoMonDoKho()
        {
            // Câu E4, mở từ 2026-11-08: `find_chance_multiplier` nên là bao nhiêu.
            // Nay đo được thay vì đoán.
            double[] values = { -0.10, -0.12, -0.15, -0.20, -0.25 };

            Console.WriteLine("=== Quét difficulty.per_level.find_chance_multiplier (" + SweepGames + " ván/ô) ===");
            Console.WriteLine(Header);

            List<string> lines = new List<string>();
            foreach (double v in values)
            {
                BalanceConfig config = TestOverrides.ConfigWith(
                    new KeyValuePair<string, string>(
                        "\"find_chance_multiplier\": -0.10",
                        "\"find_chance_multiplier\": " + v.ToString("0.00", CultureInfo.InvariantCulture)));

                string line = Row(v.ToString("0.00", CultureInfo.InvariantCulture), Measure(config, BotStyle.Balanced));
                Console.WriteLine(line);
                lines.Add(line);
            }

            Assert.AreEqual(values.Length, lines.Count);
        }

        [Test]
        public void Quet_TiLeTimThayThucVat()
        {
            // Lượng thức ăn của v7 cao hơn hẳn v6: một người hái lượm biết hết loài của vùng 0
            // mang về kỳ vọng 7,8 thức ăn, trong khi v6 cho đúng 3. Đây là phép quét hệ số chung
            // cho mọi find_chance, tức "cả vùng nghèo đi bao nhiêu".
            double[] values = { 1.00, 0.80, 0.60, 0.50, 0.40 };

            Console.WriteLine("=== Quét hệ số nhân cho mọi find_chance (" + SweepGames + " ván/ô) ===");
            Console.WriteLine(Header);

            BalanceConfig config = TestOverrides.StandardConfig();
            foreach (double v in values)
            {
                Console.WriteLine(Row(
                    "×" + v.ToString("0.00", CultureInfo.InvariantCulture),
                    Measure(config, BotStyle.Balanced, ScaledPlants(v, 1.0))));
            }
        }

        [Test]
        public void Quet_NguongKhungDamBao()
        {
            // Khung đảm bảo KHÔNG chịu hệ số bào mòn của độ khó — nó bỏ qua phép rút hoàn toàn.
            // Nên nó dựng một sàn thức ăn mà thời gian không bào mòn được. Quét xem nâng ngưỡng
            // lên thì sàn đó lùi đi bao nhiêu.
            double[] values = { 1.00, 1.50, 2.00, 3.00 };

            Console.WriteLine("=== Quét hệ số nhân cho mọi guaranteed_total_speed (" + SweepGames + " ván/ô) ===");
            Console.WriteLine(Header);

            BalanceConfig config = TestOverrides.StandardConfig();
            foreach (double v in values)
            {
                Console.WriteLine(Row(
                    "×" + v.ToString("0.00", CultureInfo.InvariantCulture),
                    Measure(config, BotStyle.Balanced, ScaledPlants(1.0, v))));
            }
        }

        [Test]
        public void Quet_KetHop_TiLeTimThayVaNguongTienHoa()
        {
            // Hai phép quét rời cho thấy chúng kéo hai hướng NGƯỢC NHAU:
            //   - hạ find_chance giết thêm nhiều bộ tộc, nhưng làm ván DÀI RA (trung vị 52 → 58);
            //   - trung vị của người THẮNG bị chặn bởi ngưỡng neuron, không bởi thức ăn.
            // Nên phải chỉnh cả hai cùng lúc: một cái hạ tỉ lệ thắng, một cái kéo trung vị về dải.
            double[] finds = { 0.60, 0.50, 0.45 };
            string[] neurons = { "[150, 250, 190, 240]", "[120, 200, 190, 240]", "[100, 170, 190, 240]" };

            Console.WriteLine("=== Quét kết hợp find_chance × evolution.required_neuron.mvp ("
                              + SweepGames + " ván/ô) ===");
            Console.WriteLine(Header);

            foreach (string n in neurons)
            {
                BalanceConfig config = TestOverrides.ConfigWith(
                    new KeyValuePair<string, string>("\"mvp\": [150, 250, 190, 240]", "\"mvp\": " + n));

                foreach (double f in finds)
                {
                    string label = "×" + f.ToString("0.00", CultureInfo.InvariantCulture)
                                   + " " + n.Substring(0, n.IndexOf(','));
                    Console.WriteLine(Row(label, Measure(config, BotStyle.Balanced, ScaledPlants(f, 1.0))));
                }
            }
        }

        [Test]
        public void Quet_SoNguoiChetMoiLanDoi()
        {
            // Hạ thức ăn giết thêm bộ tộc nhưng làm ván DÀI RA, mà trung vị đang sát mép trên
            // của dải. Cần một đòn giết bộ tộc mà KHÔNG đổi nhịp của người thắng:
            // số người chết mỗi lần đói là đúng loại đó.
            int[] deaths = { 1, 2, 3 };
            double[] finds = { 1.00, 0.80 };

            Console.WriteLine("=== Quét starvation_deaths × find_chance (" + SweepGames + " ván/ô) ===");
            Console.WriteLine(Header);

            foreach (int d in deaths)
            {
                BalanceConfig config = TestOverrides.ConfigWith(
                    new KeyValuePair<string, string>("\"starvation_deaths\": 1", "\"starvation_deaths\": " + d));

                foreach (double f in finds)
                {
                    string label = "chết " + d + " × tìm "
                                   + f.ToString("0.00", CultureInfo.InvariantCulture);
                    Console.WriteLine(Row(label, Measure(config, BotStyle.Balanced, ScaledPlants(f, 1.0))));
                }
            }
        }

        [Test]
        public void Quet_KetHop_KhauPhanBac2VaBaoMon()
        {
            // upkeep giai đoạn 2 là khóa SỐ NGUYÊN và nó nhảy quá thô: 2 → 54,4%, 3 → 31,0%,
            // không có gì ở giữa. Cần khóa thứ hai mịn hơn để hạ cánh vào 30–45%.
            // Bào mòn độ khó cũng cắn về sau, nên nó không đụng gì tới mở màn.
            double[] erosion = { -0.13, -0.15, -0.16, -0.18 };

            Console.WriteLine("=== Quét find_chance_multiplier với upkeep [1,2,2,3] ("
                              + SweepGames + " ván/ô) ===");
            Console.WriteLine(Header);

            foreach (double e in erosion)
            {
                BalanceConfig config = TestOverrides.ConfigWith(
                    new KeyValuePair<string, string>(
                        "\"upkeep_per_member\": [1, 1, 2, 3]", "\"upkeep_per_member\": [1, 2, 2, 3]"),
                    new KeyValuePair<string, string>(
                        "\"find_chance_multiplier\": -0.10",
                        "\"find_chance_multiplier\": " + e.ToString("0.00", CultureInfo.InvariantCulture)));

                Console.WriteLine(Row("bào mòn " + e.ToString("0.00", CultureInfo.InvariantCulture),
                                      Measure(config, BotStyle.Balanced)));
            }
        }

        [Test]
        public void Quet_KhauPhan_DonCanMuonCanVeSAU()
        {
            // Supervisor, 2026-11-09: "Đừng kéo tỉ lệ thắng xuống bằng cách tăng sát thương của
            // những cú trừng phạt sớm. Kéo bằng những thứ cắn về sau."
            //
            // upkeep_per_member đúng loại đó: giai đoạn 1 giữ nguyên 1, chỉ giai đoạn 2 trở đi
            // mới nặng lên — mở màn không đổi một chút nào.
            Console.WriteLine("=== Quét upkeep_per_member, starvation_deaths = 1 (" + SweepGames + " ván/ô) ===");
            Console.WriteLine(Header);

            string[] values = { "[1, 1, 2, 3]", "[1, 2, 2, 3]", "[1, 2, 3, 3]", "[1, 3, 3, 4]" };

            foreach (string v in values)
            {
                BalanceConfig config = TestOverrides.ConfigWith(
                    new KeyValuePair<string, string>("\"upkeep_per_member\": [1, 1, 2, 3]", "\"upkeep_per_member\": " + v));

                Console.WriteLine(Row(v, Measure(config, BotStyle.Balanced)));
            }
        }
    }
}
