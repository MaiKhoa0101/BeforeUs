using System;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C3. design.md §10.</summary>
    [TestFixture]
    public class ClimateTests
    {
        private Climate _climate;

        [SetUp]
        public void SetUp()
        {
            _climate = new Climate(BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp));
        }

        [Test]
        public void DocDungCauHinh()
        {
            Assert.AreEqual(10, _climate.PhaseLength);
            Assert.AreEqual(ClimatePhase.Wet, _climate.FirstPhase);
        }

        [Test]
        public void Luot1Den10_Am()
        {
            for (int turn = 1; turn <= 10; turn++)
            {
                Assert.AreEqual(ClimatePhase.Wet, _climate.PhaseAt(turn), "lượt " + turn);
            }
        }

        [Test]
        public void Luot11Den20_Kho()
        {
            for (int turn = 11; turn <= 20; turn++)
            {
                Assert.AreEqual(ClimatePhase.Dry, _climate.PhaseAt(turn), "lượt " + turn);
            }
        }

        [Test]
        public void Luot21Den30_AmTroLai()
        {
            for (int turn = 21; turn <= 30; turn++)
            {
                Assert.AreEqual(ClimatePhase.Wet, _climate.PhaseAt(turn), "lượt " + turn);
            }
        }

        [Test]
        public void KhopVoiCongThucCuaMoPhong()
        {
            // before_us_sim.py:48 — dry = (turn - 1) // phase_len % 2 == 1
            for (int turn = 1; turn <= 150; turn++)
            {
                bool expectedDry = ((turn - 1) / 10) % 2 == 1;
                Assert.AreEqual(expectedDry, _climate.IsDry(turn), "lượt " + turn);
            }
        }

        [Test]
        public void ConBaoNhieuLuotNuaThiDoiPha()
        {
            Assert.AreEqual(10, _climate.TurnsUntilPhaseChange(1));
            Assert.AreEqual(2, _climate.TurnsUntilPhaseChange(9));
            Assert.AreEqual(1, _climate.TurnsUntilPhaseChange(10));
            Assert.AreEqual(10, _climate.TurnsUntilPhaseChange(11));
            Assert.AreEqual(1, _climate.TurnsUntilPhaseChange(20));
        }

        [Test]
        public void BaoTruocDungHaiLuot()
        {
            // design.md §10: "Báo trước 2 lượt khi sắp đổi pha".
            Assert.IsFalse(_climate.IsPhaseChangeImminent(8));
            Assert.IsTrue(_climate.IsPhaseChangeImminent(9));
            Assert.IsTrue(_climate.IsPhaseChangeImminent(10));
            Assert.IsFalse(_climate.IsPhaseChangeImminent(11));

            Assert.IsTrue(_climate.IsPhaseChangeImminent(19));
            Assert.IsTrue(_climate.IsPhaseChangeImminent(20));
            Assert.IsFalse(_climate.IsPhaseChangeImminent(21));
        }

        [Test]
        public void LuotNhoHon1_ThiBaoLoi()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _climate.PhaseAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _climate.TurnsUntilPhaseChange(-1));
        }
    }
}
