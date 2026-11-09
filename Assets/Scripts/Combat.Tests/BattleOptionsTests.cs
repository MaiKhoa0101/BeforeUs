using System.Collections.Generic;
using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    /// <summary>
    /// Đòn bẩy (A) — giới hạn số người đánh được cùng lúc một con thú.
    ///
    /// Tồn tại vì ở B3 nó cho kết quả "gần như không đổi gì". Trước khi báo đó là KẾT LUẬN,
    /// phải chắc nó không phải là BẢN CÀI HỎNG: một đòn bẩy gãy và một đòn bẩy vô dụng cho ra
    /// cùng một con số trên bảng, nhưng dẫn tới hai quyết định hoàn toàn khác nhau.
    /// </summary>
    [TestFixture]
    public sealed class BattleOptionsTests
    {
        /// <summary>Năm người đứng sát nhau quanh một con thú chậm, máu dày.</summary>
        private static BattleSimulation Swarm(int cap)
        {
            TribeCombatProfile profile = TestBattles.Profile(damage: 1.0, interval: 0.5, range: 2.5);
            List<CombatantSpec> tribe = TestBattles.Tribe(
                profile,
                TestBattles.Member(1), TestBattles.Member(2), TestBattles.Member(3),
                TestBattles.Member(4), TestBattles.Member(5));

            BattleSimulation battle = new BattleSimulation(
                new BattleSetup(
                    BattleKind.Lure,
                    TestBattles.LureField(),
                    tribe,
                    new[] { TestBattles.Beast(0, x: 5.0, hp: 1000.0, moveSpeed: 0.01, damage: 0.0001, interval: 9.0) },
                    30.0,
                    new BattleOptions(cap)),
                TestBattles.Random());

            for (int i = 0; i < 5; i++)
            {
                battle.TryDeploy(i + 1, new CombatVector(6.0, 3.0));
            }

            return battle;
        }

        private static double DamageDealtIn30Seconds(int cap)
        {
            BattleSimulation battle = Swarm(cap);
            battle.RunToEnd();
            return battle.Beasts[0].MaxHp - battle.Beasts[0].Hp;
        }

        [Test]
        public void MacDinh_KhongGioiHan_ThiCaNamNguoiCungDanh()
        {
            Assert.That(BattleOptions.Default.LimitsAttackers, Is.False);
            Assert.That(BattleOptions.Default.MaxAttackersPerBeast, Is.EqualTo(0));
        }

        [Test]
        public void GioiHan1Nguoi_ThiSatThuongGiamKhoangMotPhanNam()
        {
            double unlimited = DamageDealtIn30Seconds(0);
            double capped = DamageDealtIn30Seconds(1);

            Assert.That(unlimited, Is.GreaterThan(0.0));
            Assert.That(capped, Is.LessThan(unlimited * 0.3),
                "Năm người chụm một chỗ mà chỉ một người được đánh thì sát thương phải sụt hẳn");
        }

        [Test]
        public void GioiHanCangCao_SatThuongCangNhieu_VaDungLaiOMucKhongGioiHan()
        {
            double cap1 = DamageDealtIn30Seconds(1);
            double cap2 = DamageDealtIn30Seconds(2);
            double cap3 = DamageDealtIn30Seconds(3);
            double cap5 = DamageDealtIn30Seconds(5);
            double unlimited = DamageDealtIn30Seconds(0);

            Assert.That(cap2, Is.GreaterThan(cap1));
            Assert.That(cap3, Is.GreaterThan(cap2));
            Assert.That(cap5, Is.EqualTo(unlimited).Within(1e-9),
                "Giới hạn bằng đúng sĩ số thì không còn là giới hạn nữa");
        }

        [Test]
        public void GioiHanChonNguoiGANNHAT_KhongPhaiNguoiIdNho()
        {
            // Thiên vị theo id là loại lệch không ai nhìn ra. Người #5 đứng sát thú nhất,
            // nên chính #5 phải là người được đánh, dù id lớn nhất.
            TribeCombatProfile profile = TestBattles.Profile(damage: 1.0, interval: 0.5, range: 5.0);
            List<CombatantSpec> tribe = TestBattles.Tribe(
                profile,
                TestBattles.Member(1), TestBattles.Member(2), TestBattles.Member(3),
                TestBattles.Member(4), TestBattles.Member(5));

            BattleSimulation battle = new BattleSimulation(
                new BattleSetup(
                    BattleKind.Lure,
                    TestBattles.LureField(),
                    tribe,
                    new[] { TestBattles.Beast(0, x: 5.0, hp: 1000.0, moveSpeed: 0.01, damage: 0.0001, interval: 9.0) },
                    30.0,
                    new BattleOptions(1)),
                TestBattles.Random());

            // Càng id lớn càng đứng gần con thú ở x = 5.
            battle.TryDeploy(1, new CombatVector(9.0, 3.0));
            battle.TryDeploy(2, new CombatVector(8.5, 3.0));
            battle.TryDeploy(3, new CombatVector(8.0, 3.0));
            battle.TryDeploy(4, new CombatVector(7.5, 3.0));
            battle.TryDeploy(5, new CombatVector(6.0, 3.0));

            battle.RunToEnd();

            Assert.That(battle.FindTribeMember(5).LastAttackAtSeconds, Is.GreaterThan(0.0),
                "Người gần nhất phải là người được ra đòn");
            Assert.That(battle.FindTribeMember(1).LastAttackAtSeconds, Is.EqualTo(-1.0),
                "Người xa nhất không được ra đòn nào");
        }
    }
}
