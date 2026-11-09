using System;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C3. design.md §11.</summary>
    [TestFixture]
    public class DifficultyTests
    {
        private string _json;
        private Difficulty _mvp;
        private BalanceConfig _mvpConfig;

        [SetUp]
        public void SetUp()
        {
            _json = TestPaths.ReadBalanceJson();
            _mvpConfig = BalanceConfig.FromJson(_json, GameMode.Mvp);
            _mvp = new Difficulty(_mvpConfig);
        }

        [Test]
        public void ChuKyTheoMode()
        {
            Assert.AreEqual(12, _mvp.PeriodTurns);
            Assert.AreEqual(20, new Difficulty(BalanceConfig.FromJson(_json, GameMode.Full)).PeriodTurns);
        }

        [Test]
        public void BacDoKho_MvpTangMoi12Luot()
        {
            Assert.AreEqual(0, _mvp.LevelAt(1));
            Assert.AreEqual(0, _mvp.LevelAt(11));
            Assert.AreEqual(1, _mvp.LevelAt(12));
            Assert.AreEqual(1, _mvp.LevelAt(23));
            Assert.AreEqual(2, _mvp.LevelAt(24));
            Assert.AreEqual(4, _mvp.LevelAt(48));
        }

        [Test]
        public void BacDoKho_BanDayDuTangMoi20Luot()
        {
            Difficulty full = new Difficulty(BalanceConfig.FromJson(_json, GameMode.Full));
            Assert.AreEqual(0, full.LevelAt(19));
            Assert.AreEqual(1, full.LevelAt(20));
            Assert.AreEqual(2, full.LevelAt(40));
        }

        [Test]
        public void KhopVoiCongThucCuaMoPhong()
        {
            // before_us_sim.py:49 — lvl = turn // diff_period
            for (int turn = 1; turn <= 150; turn++)
            {
                Assert.AreEqual(turn / 12, _mvp.LevelAt(turn), "lượt " + turn);
            }
        }

        [Test]
        public void MoiBac_TiLeThangCombatGiam2DiemPhanTram()
        {
            Assert.AreEqual(0.0, _mvp.CombatOddsDelta(0), 1e-12);
            Assert.AreEqual(-0.02, _mvp.CombatOddsDelta(1), 1e-12);
            Assert.AreEqual(-0.06, _mvp.CombatOddsDelta(3), 1e-12);
        }

        [Test]
        public void MoiBac_QuaiThuAnThem1Nguoi()
        {
            Assert.AreEqual(0, _mvp.RaidQuotaDelta(0));
            Assert.AreEqual(1, _mvp.RaidQuotaDelta(1));
            Assert.AreEqual(3, _mvp.RaidQuotaDelta(3));
        }

        [Test]
        public void MoiBac_SuKienXauThemMot()
        {
            Assert.AreEqual(-2, _mvp.BadEventFoodDelta(2));
            Assert.AreEqual(-2, _mvp.BadEventMoraleDelta(2));
        }

        [Test]
        public void MoiBac_TiLeTimThayThucVatGiamTheoHeSoTrongJson()
        {
            // v7 §A.7: thay cho "hái lượm −1 mỗi bậc" của v6.
            // Hệ số đọc từ balance.json chứ không viết cứng — nó là con số cân bằng và đã đổi
            // một lần rồi (−0.10 → −0.18, xem balance-log.md #10). Test này canh CÔNG THỨC.
            double k = _mvpConfig.Data.Difficulty.PerLevel.FindChanceMultiplier;

            Assert.AreEqual(1.0, _mvp.FindChanceMultiplier(0), 1e-12);
            Assert.AreEqual(1.0 + k, _mvp.FindChanceMultiplier(1), 1e-12);
            Assert.AreEqual(1.0 + k * 3, _mvp.FindChanceMultiplier(3), 1e-12);
            Assert.Less(_mvp.FindChanceMultiplier(3), _mvp.FindChanceMultiplier(1), "bậc cao thì bào mòn nhiều hơn");
        }

        [Test]
        public void HeSoBaoMon_ChanODuoi0_VanLaCoCheKetThucVan()
        {
            // Đây là cơ chế duy nhất kết thúc ván ở lì (design.md §11).
            Assert.AreEqual(0.0, _mvp.FindChanceMultiplier(10), 1e-12);
            Assert.AreEqual(0.0, _mvp.FindChanceMultiplier(50), 1e-12, "không bao giờ âm");
        }

        [Test]
        public void SoCongThemDaMangSanDau_CongThangKhongDaoDau()
        {
            // Cạm bẫy đã biết: before_us_sim.py viết các hệ số này dương rồi TRỪ.
            // balance.json viết sẵn dấu âm, nên Core phải CỘNG. Test này giữ đúng quy ước đó.
            int level = 3;
            double baseHunt = 0.50;
            Assert.AreEqual(0.44, baseHunt + _mvp.CombatOddsDelta(level), 1e-12);

            int baseBadFood = -3;
            Assert.AreEqual(-6, baseBadFood + _mvp.BadEventFoodDelta(level));
        }

        [Test]
        public void ThamSoKhongHopLe_ThiBaoLoi()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _mvp.LevelAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _mvp.FindChanceMultiplier(-1));
        }
    }
}
