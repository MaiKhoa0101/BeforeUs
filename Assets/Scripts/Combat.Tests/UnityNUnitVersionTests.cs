using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    /// <summary>
    /// Khoá con số mà cổng API Unity (tools/CombatTests/UnityApiGate) đang ghim.
    ///
    /// Vì sao cần: cổng đó ghim NUnit 3.5.0 vì đó là bản Unity 6000.6.7f1 nạp. Nếu Unity nâng
    /// com.unity.ext.nunit, cổng sẽ kiểm theo một bản CŨ HƠN thực tế — nó vẫn xanh, vẫn trông như
    /// đang bảo vệ, nhưng đã hết tác dụng. Một cái gác ngủ quên còn tệ hơn không có gác, vì
    /// không ai nghĩ phải tự nhìn.
    ///
    /// Đọc thẳng FileVersion của tệp, KHÔNG nạp assembly: nunit.framework.dll của Unity là bản
    /// .NET Framework 4.5, nạp vào .NET 10 là nổ.
    /// </summary>
    [TestFixture]
    public sealed class UnityNUnitVersionTests
    {
        /// <summary>Phải khớp PackageReference NUnit trong UnityApiGate.csproj.</summary>
        private const string PinnedInGate = "3.5.0.0";

        [Test]
        public void BanNUnitCuaUnity_VanDungBanMaCongDangGhim()
        {
            string dll = FindUnityNUnit();
            if (dll == null)
            {
                Assert.Ignore(
                    "Không thấy Library/PackageCache/com.unity.ext.nunit*/ — máy này chưa mở Unity lần nào. "
                    + "Bỏ qua, không phải lỗi.");
                return;
            }

            string actual = FileVersionInfo.GetVersionInfo(dll).FileVersion;

            Assert.That(actual, Is.EqualTo(PinnedInGate),
                "Unity nay nạp NUnit " + actual + " chứ không phải " + PinnedInGate + ".\n"
                + "Cổng tools/CombatTests/UnityApiGate đang kiểm theo bản cũ hơn thực tế, tức nó\n"
                + "không còn bảo vệ gì nữa. Sửa PackageReference NUnit trong UnityApiGate.csproj\n"
                + "và hằng PinnedInGate ở file này cho khớp.\n"
                + "Tệp đọc được: " + dll);
        }

        private static string FindUnityNUnit()
        {
            string cache = Path.Combine(TestBattles.RepoRoot, "Library", "PackageCache");
            if (!Directory.Exists(cache))
            {
                return null;
            }

            // Tên thư mục gói có hậu tố băm đổi theo mỗi lần Unity nạp lại, nên phải dò chứ
            // không viết cứng được.
            foreach (string dir in Directory.GetDirectories(cache, "com.unity.ext.nunit*"))
            {
                string[] found = Directory.GetFiles(dir, "nunit.framework.dll", SearchOption.AllDirectories);
                if (found.Length > 0)
                {
                    return found[0];
                }
            }

            return null;
        }
    }
}
