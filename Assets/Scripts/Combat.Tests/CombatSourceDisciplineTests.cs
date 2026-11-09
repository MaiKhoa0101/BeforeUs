using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    /// <summary>
    /// Khoá hai ràng buộc bằng test thay vì bằng lời hứa. Cả hai đều là loại hỏng âm thầm:
    /// không có gì nổ, chỉ là số liệu cân bằng sai dần và không ai biết.
    ///
    /// HANDOFF.md §8 xếp "agent âm thầm đổi luật" là rủi ro số 1 của dự án, và cách nó xảy ra
    /// trong thực tế gần như luôn là: ai đó cần một con số, thấy công thức trong tài liệu,
    /// gõ lại vào file của mình. Test này chặn đúng lần thứ mười, không chỉ lần đầu.
    /// </summary>
    [TestFixture]
    public sealed class CombatSourceDisciplineTests
    {
        /// <summary>
        /// Nguyên liệu của công thức §5.2 và §8. Nhánh combat không được chạm vào bất cứ cái nào:
        /// cần tỉ lệ thắng thì gọi ICombatOdds.WinChance của Core (chỉ trong Calibration/),
        /// cần quota thì gọi ProbabilisticCombatResolver.RaidQuota.
        ///
        /// Danh sách này CỐ Ý hẹp và chính xác, không rộng. Bản đầu có thêm "PerBuildLevel",
        /// "PerRegion", "PerStage", "PerMember" và chúng báo động giả trên chính tên trường
        /// scaling của beasts.json — những chữ đó quá chung để làm tín hiệu. Bỏ chúng không mất
        /// độ phủ: đường DUY NHẤT tới hệ số của công thức là đi qua BalanceConfig.Data.CombatOdds
        /// hoặc .DefenceOdds, mà hai cái đó vẫn bị chặn. Một test báo động giả là một test
        /// người ta sẽ tắt đi.
        /// </summary>
        private static readonly string[] ForbiddenOddsTokens =
        {
            "combat_odds", "defence_odds", "base_win", "raid_quota_by_region",
            "CombatOdds", "DefenceOdds", "BaseWin", "RaidQuotaForRegion",
            "WinChance"
        };

        /// <summary>
        /// Nguồn ngẫu nhiên khác IRandom. AGENTS.md: System.Random cho chuỗi khác nhau giữa Mono và
        /// CoreCLR, còn UnityEngine.Random thì không lưu được trạng thái — trận sẽ không khôi phục
        /// đúng khi tải ván (C7), và không ai nhìn thấy.
        /// </summary>
        private static readonly string[] ForbiddenRandomTokens =
        {
            "System.Random", "UnityEngine.Random", "new Random("
        };

        [Test]
        public void MaNguonCombat_KhongChuaMotManhNaoCuaCongThucLuat()
        {
            List<string> offences = new List<string>();

            foreach (string path in CombatSourceFiles())
            {
                // Thư mục hiệu chỉnh là chỗ DUY NHẤT được phép gọi sang phía xác suất của Core,
                // và nó chỉ được GỌI WinChance chứ không tự tính. Xem CalibrationGuard bên dưới.
                if (IsCalibration(path))
                {
                    continue;
                }

                string text = File.ReadAllText(path);
                foreach (string token in ForbiddenOddsTokens)
                {
                    if (text.Contains(token))
                    {
                        offences.Add(Relative(path) + " chứa \"" + token + "\"");
                    }
                }
            }

            Assert.That(offences, Is.Empty,
                "Công thức design.md §5.2/§8 chỉ được cài đúng một lần, bên trong Core. "
                + "Cần con số thì gọi ICombatOdds.WinChance / ProbabilisticCombatResolver.RaidQuota.\n"
                + string.Join("\n", offences));
        }

        [Test]
        public void MaNguonCombat_ChiRutNgauNhienQuaIRandom()
        {
            List<string> offences = new List<string>();

            foreach (string path in CombatSourceFiles())
            {
                string text = File.ReadAllText(path);
                foreach (string token in ForbiddenRandomTokens)
                {
                    if (text.Contains(token))
                    {
                        offences.Add(Relative(path) + " chứa \"" + token + "\"");
                    }
                }
            }

            Assert.That(offences, Is.Empty,
                "Ngẫu nhiên chỉ được rút qua IRandom của Core (AGENTS.md).\n" + string.Join("\n", offences));
        }

        [Test]
        public void MaNguonCombat_KhongThamChieuUnityEngine()
        {
            // asmdef đặt noEngineReferences: true nên compiler đã chặn trong Unity.
            // Test này bắt sớm hơn thế: ngay ở `dotnet test`, trước khi ai mở Editor.
            List<string> offences = new List<string>();

            foreach (string path in CombatSourceFiles())
            {
                if (File.ReadAllText(path).Contains("using UnityEngine"))
                {
                    offences.Add(Relative(path));
                }
            }

            Assert.That(offences, Is.Empty,
                "Mô phỏng trận phải là C# thuần để lớp hiển thị gắn lên trên mà không sửa logic.\n"
                + string.Join("\n", offences));
        }

        [Test]
        public void Asmdef_CuaCombat_VanDatNoEngineReferences()
        {
            string asmdef = Path.Combine(TestBattles.CombatSourceRoot, "BeforeUs.Combat.asmdef");

            Assert.That(File.Exists(asmdef), Is.True, "Không thấy " + asmdef);
            Assert.That(File.ReadAllText(asmdef).Replace(" ", string.Empty),
                Does.Contain("\"noEngineReferences\":true"));
        }

        private static bool IsCalibration(string path)
        {
            string normalised = path.Replace('\\', '/');
            return normalised.Contains("/Calibration/");
        }

        private static IEnumerable<string> CombatSourceFiles()
        {
            string root = TestBattles.CombatSourceRoot;
            Assert.That(Directory.Exists(root), Is.True, "Không thấy thư mục mã nguồn combat: " + root);
            return Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
        }

        private static string Relative(string path)
        {
            return path.Substring(TestBattles.RepoRoot.Length).TrimStart('\\', '/');
        }
    }
}
