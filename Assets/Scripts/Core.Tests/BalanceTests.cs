using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// C8 — bốn bot và test cân bằng (HANDOFF Cổng 2).
    ///
    /// Đây là so sánh THỐNG KÊ với before_us_sim.py, không phải so chuỗi: Core dùng xorshift128,
    /// sim dùng Mersenne Twister của Python, hai chuỗi không bao giờ trùng nhau.
    ///
    /// Dải mục tiêu của Cổng 2 là TIÊU CHÍ SẢN PHẨM chờ Khoa quyết, không phải tính đúng của Core,
    /// nên nó được báo bằng Assert.Warn chứ không làm đỏ bộ test. Bất biến thật thì assert cứng.
    /// </summary>
    [TestFixture]
    public class BalanceTests
    {
        private static readonly BotStyle[] AllStyles =
        {
            BotStyle.Balanced, BotStyle.Safe, BotStyle.Reckless, BotStyle.Careless
        };

        private static BalanceConfig _config;
        private static List<BalanceStats> _baseline;
        private static List<BalanceStats> _withNodeEffects;
        private static readonly Dictionary<string, BalanceStats> _variants = new Dictionary<string, BalanceStats>();

        [OneTimeSetUp]
        public void RunSimulations()
        {
            _config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
            _baseline = RunAll(new BotOptions(), "applyNodeEffects = false");
            _withNodeEffects = RunAll(new BotOptions { ApplyNodeEffects = true }, "applyNodeEffects = true");

            Report(BalanceSimulation.Table("Cổng 2 — applyNodeEffects = FALSE (để so với mô phỏng)", _baseline));
            Report(BalanceSimulation.Table("Cổng 2 — applyNodeEffects = TRUE", _withNodeEffects));
        }

        private static List<BalanceStats> RunAll(BotOptions template, string label)
        {
            List<BalanceStats> rows = new List<BalanceStats>();
            foreach (BotStyle style in AllStyles)
            {
                BotOptions options = new BotOptions
                {
                    Style = style,
                    ApplyNodeEffects = template.ApplyNodeEffects,
                    DisableD1 = template.DisableD1,
                    DisableD2 = template.DisableD2,
                    DisableD11 = template.DisableD11
                };

                rows.Add(BalanceSimulation.Run(_config, options, label));
            }

            return rows;
        }

        private static BalanceStats Of(List<BalanceStats> rows, BotStyle style)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Style == style)
                {
                    return rows[i];
                }
            }

            throw new InvalidOperationException("Không có bot " + style);
        }

        private static void Report(string text)
        {
            TestContext.Progress.WriteLine(text);
        }

        /// <summary>
        /// Cảnh báo một tiêu chí SẢN PHẨM chưa đạt, mà không làm đỏ bộ test.
        ///
        /// Cố ý KHÔNG dùng Assert.Warn: hàm đó có từ NUnit 3.6, còn Unity Test Framework dựng trên
        /// NUnit 3.5 (com.unity.ext.nunit 2.1.0) nên nó không biên dịch được trong Unity.
        /// tools/CoreTests ghim đúng NUnit 3.5.0 để lỗi loại này lộ ra ở `dotnet test`
        /// thay vì lộ ra trong Console của Khoa.
        /// </summary>
        private static void Warn(string message)
        {
            string banner = "*** CẢNH BÁO: " + message;
            TestContext.Progress.WriteLine(banner);
            TestContext.WriteLine(banner);
        }

        // ---------- bất biến thật: assert cứng ----------

        [Test]
        public void MoiVanDeuKetThuc_KhongTreoKhongNem()
        {
            foreach (BalanceStats row in _baseline)
            {
                Assert.AreEqual(BalanceSimulation.DefaultGames, row.Games, row.Style.ToString());
                Assert.That(row.WinRate, Is.InRange(0.0, 1.0));
            }
        }

        [Test]
        public void BotNeCombat_ThangItHonBotDanhNhau()
        {
            // design.md §1: "Né mọi combat tự chọn phải thua."
            BalanceStats safe = Of(_baseline, BotStyle.Safe);
            BalanceStats balanced = Of(_baseline, BotStyle.Balanced);

            Assert.LessOrEqual(safe.WinRate, balanced.WinRate,
                "bot né combat không được thắng nhiều hơn bot cân nhắc");
        }

        [Test]
        public void LapLaiDuoc_ChayHaiLanChoCungSoLieu()
        {
            BotOptions options = new BotOptions { Style = BotStyle.Balanced };
            BalanceStats first = BalanceSimulation.Run(_config, options, "lần 1", 300);
            BalanceStats second = BalanceSimulation.Run(_config, options, "lần 2", 300);

            Assert.AreEqual(first.Wins, second.Wins);
            Assert.AreEqual(first.MedianWinTurns, second.MedianWinTurns, 0.0);
        }

        [Test]
        public void SoLieuKhongTamThuong_BotPhaiThucSuChoi()
        {
            // Chốt chặn chống số liệu rỗng ruột: nếu mọi ván chết ở lượt 3 thì mọi con số trên
            // vẫn "hợp lệ" mà chẳng nói lên điều gì.
            BalanceStats balanced = Of(_baseline, BotStyle.Balanced);
            int deaths = 0;
            foreach (KeyValuePair<DeathCause, int> pair in balanced.Causes)
            {
                deaths += pair.Value;
            }

            Assert.AreEqual(BalanceSimulation.DefaultGames, balanced.Wins + balanced.Timeouts + deaths,
                "mọi ván phải được phân loại: thắng, hết lượt, hoặc chết có nguyên nhân");
            Assert.Less(balanced.Causes.ContainsKey(DeathCause.Unknown) ? balanced.Causes[DeathCause.Unknown] : 0,
                BalanceSimulation.DefaultGames / 20,
                "quá nhiều ván không phân loại được nguyên nhân thua");
        }

        // ---------- dải mục tiêu Cổng 2: báo, không làm đỏ ----------

        [Test]
        public void CongHai_DoiChieuVoiDaiMucTieu()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Đối chiếu dải Cổng 2 (applyNodeEffects = false) ===");

            BalanceStats balanced = Of(_baseline, BotStyle.Balanced);
            Check(sb, "balanced thắng 30–45%", balanced.WinRateText, balanced.WinRate >= 0.30 && balanced.WinRate <= 0.45);
            Check(sb, "balanced trung vị 42–52 lượt", Num(balanced.MedianWinTurns),
                balanced.MedianWinTurns >= 42 && balanced.MedianWinTurns <= 52);

            BalanceStats safe = Of(_baseline, BotStyle.Safe);
            Check(sb, "safe thắng <= 2%", safe.WinRateText, safe.WinRate <= 0.02);

            BalanceStats reckless = Of(_baseline, BotStyle.Reckless);
            Check(sb, "reckless thắng <= 20%", reckless.WinRateText, reckless.WinRate <= 0.20);

            BalanceStats careless = Of(_baseline, BotStyle.Careless);
            Check(sb, "careless thắng <= 20%", careless.WinRateText, careless.WinRate <= 0.20);

            Report(sb.ToString());

            if (sb.ToString().Contains("NGOÀI DẢI"))
            {
                Warn("CỔNG 2 CHƯA ĐẠT — có chỉ tiêu nằm ngoài dải. Đây là quyết định của Khoa, "
                     + "không phải lỗi Core. Xem bảng ngay trên và báo cáo C8.");
            }
        }

        private static void Check(StringBuilder sb, string label, string value, bool inRange)
        {
            sb.Append(inRange ? "  ĐẠT      " : "  NGOÀI DẢI ")
              .Append(label.PadRight(32))
              .Append("đo được: ").Append(value)
              .AppendLine();
        }

        private static string Num(double value)
        {
            return value <= 0 ? "-" : value.ToString("F1", CultureInfo.InvariantCulture);
        }

        // ---------- đo riêng phần đóng góp của D1, D2, D11 ----------

        [Test]
        public void DoRiengPhanDongGopCuaD1_D2_D11()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Phần đóng góp của từng sai lệch với mô phỏng (applyNodeEffects = false) ===");
            sb.AppendLine("Mỗi cột TẮT một quyết định để Core chơi giống before_us_sim.py hơn.");
            sb.AppendLine("Số trong ngoặc = dịch bao nhiêu điểm % so với cột ngay trước nó.");
            sb.AppendLine();
            sb.AppendLine("Bảng A — tắt từng cái một, trên nền Core hiện tại:");
            sb.AppendLine("bot        Core hiện tại    tắt D1          tắt D2          tắt D11");

            foreach (BotStyle style in AllStyles)
            {
                BalanceStats baseline = Of(_baseline, style);
                sb.Append(style.ToString().ToLowerInvariant().PadRight(11));
                sb.Append(baseline.WinRateText.PadRight(16));
                sb.Append(Shift(baseline, Variant(style, d1: true)).PadRight(16));
                sb.Append(Shift(baseline, Variant(style, d2: true)).PadRight(16));
                sb.Append(Shift(baseline, Variant(style, d11: true)));
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("Bảng B — tắt D1 TRƯỚC rồi mới tắt thêm, vì trên nền Core hiện tại tỉ lệ thắng");
            sb.AppendLine("kẹt sàn 0% nên mọi hiệu số đều bị che. Nền của bảng này là cột \"tắt D1\".");
            sb.AppendLine("bot        tắt D1           +tắt D2         +tắt D11        tắt cả ba (≈ sim)");

            foreach (BotStyle style in AllStyles)
            {
                BalanceStats noD1 = Variant(style, d1: true);
                sb.Append(style.ToString().ToLowerInvariant().PadRight(11));
                sb.Append(noD1.WinRateText.PadRight(17));
                sb.Append(Shift(noD1, Variant(style, d1: true, d2: true)).PadRight(16));
                sb.Append(Shift(noD1, Variant(style, d1: true, d11: true)).PadRight(16));
                sb.Append(Shift(noD1, Variant(style, d1: true, d2: true, d11: true)));
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("Bảng C — cấu hình \"tắt cả ba\" đặt cạnh before_us_sim.py mvp (thắng/trung vị/p10/p90):");
            sb.AppendLine("           Core tắt cả ba                        before_us_sim.py mvp");
            string[] simRef = { "37.6% tv=47.0 p10=44.0 p90=52.0", "0.0% tv=- p10=- p90=-",
                                "12.1% tv=42.0 p10=35.0 p90=49.0", "8.1% tv=38.0 p10=34.0 p90=44.0" };
            for (int i = 0; i < AllStyles.Length; i++)
            {
                sb.Append("  ").Append(AllStyles[i].ToString().ToLowerInvariant().PadRight(11))
                  .Append(WithMedian(Variant(AllStyles[i], d1: true, d2: true, d11: true)).PadRight(38))
                  .Append(simRef[i])
                  .AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("=== D11: tần suất thua Phòng thủ khi quota >= sĩ số ===");
            foreach (BalanceStats row in _baseline)
            {
                double share = 100.0 * row.GamesWithD11Save / row.Games;
                sb.Append("  ").Append(row.Style.ToString().ToLowerInvariant().PadRight(11))
                  .Append(share.ToString("F1", CultureInfo.InvariantCulture)).Append("% số ván có ít nhất một lần; ")
                  .Append("tổng ").Append(row.D11Saves).Append(" lần trên ").Append(row.Games).Append(" ván")
                  .AppendLine();
            }

            Report(sb.ToString());
            Assert.Pass("Xem bảng trong đầu ra test.");
        }

        private static BalanceStats Variant(BotStyle style, bool d1 = false, bool d2 = false, bool d11 = false)
        {
            string key = style + "|" + d1 + d2 + d11;
            BalanceStats cached;
            if (_variants.TryGetValue(key, out cached))
            {
                return cached;
            }

            BalanceStats stats = BalanceSimulation.Run(
                _config,
                new BotOptions { Style = style, DisableD1 = d1, DisableD2 = d2, DisableD11 = d11 },
                key);

            _variants[key] = stats;
            return stats;
        }

        private static string Shift(BalanceStats baseline, BalanceStats variant)
        {
            double shift = (variant.WinRate - baseline.WinRate) * 100;
            string sign = shift >= 0 ? "+" : "";
            return (variant.WinRate * 100).ToString("F1", CultureInfo.InvariantCulture)
                   + "% (" + sign + shift.ToString("F1", CultureInfo.InvariantCulture) + ")";
        }

        private static string WithMedian(BalanceStats stats)
        {
            return (stats.WinRate * 100).ToString("F1", CultureInfo.InvariantCulture)
                   + "% tv=" + Num(stats.MedianWinTurns)
                   + " p10=" + Num(stats.P10WinTurns)
                   + " p90=" + Num(stats.P90WinTurns);
        }

        // ---------- hiệu ứng nút ----------

        [Test]
        public void BatHieuUngNut_BaoTiLeThangMoi()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== applyNodeEffects: tắt so với bật ===");

            foreach (BotStyle style in AllStyles)
            {
                BalanceStats off = Of(_baseline, style);
                BalanceStats on = Of(_withNodeEffects, style);
                double shift = (on.WinRate - off.WinRate) * 100;

                sb.Append("  ").Append(style.ToString().ToLowerInvariant().PadRight(11))
                  .Append("tắt ").Append(off.WinRateText.PadRight(16))
                  .Append("bật ").Append(on.WinRateText.PadRight(16))
                  .Append("dịch ").Append(shift >= 0 ? "+" : "")
                  .Append(shift.ToString("F1", CultureInfo.InvariantCulture)).Append(" điểm %")
                  .AppendLine();
            }

            Report(sb.ToString());

            // HANDOFF Cổng 2: "nếu vượt 45%, đề xuất chỉnh hiệu ứng nút (không tự sửa)".
            BalanceStats balancedOn = Of(_withNodeEffects, BotStyle.Balanced);
            if (balancedOn.WinRate > 0.45)
            {
                Warn("Bật hiệu ứng nút làm bot balanced thắng " + balancedOn.WinRateText
                     + ", vượt mốc 45% của HANDOFF. Cần đề xuất chỉnh hiệu ứng nút cho Khoa.");
            }
        }
    }
}
