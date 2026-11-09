using System.Collections.Generic;
using BeforeUs.Core;
using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    [TestFixture]
    public sealed class BattleSimulationTests
    {
        private static BattleSimulation Lure(
            IReadOnlyList<CombatantSpec> tribe,
            IReadOnlyList<CombatantSpec> beasts,
            double timeLimit = 30.0,
            int seed = 12345)
        {
            return new BattleSimulation(
                new BattleSetup(BattleKind.Lure, TestBattles.LureField(), tribe, beasts, timeLimit),
                TestBattles.Random(seed));
        }

        private static BattleSimulation Prevent(
            IReadOnlyList<CombatantSpec> tribe,
            IReadOnlyList<CombatantSpec> beasts,
            double timeLimit = 30.0,
            int seed = 12345)
        {
            return new BattleSimulation(
                new BattleSetup(BattleKind.Prevent, TestBattles.PreventField(), tribe, beasts, timeLimit),
                TestBattles.Random(seed));
        }

        // --- Dụ và hạ ---------------------------------------------------------------------

        [Test]
        public void Lure_HaHetThu_ThiThang()
        {
            TribeCombatProfile profile = TestBattles.Profile(damage: 3.0, interval: 0.4);
            BattleSimulation battle = Lure(
                TestBattles.Tribe(profile, TestBattles.Member(1), TestBattles.Member(2)),
                new[] { TestBattles.Beast(0, x: 5.0, hp: 8.0, moveSpeed: 0.5) });

            battle.TryDeploy(1, new CombatVector(6.0, 3.0));
            battle.TryDeploy(2, new CombatVector(7.0, 3.0));

            BattleReport report = battle.RunToEnd();

            Assert.That(report.Won, Is.True);
            Assert.That(report.Reason, Is.EqualTo(BattleEndReason.AllBeastsDown));
            Assert.That(report.BeastsDown, Is.EqualTo(1));
        }

        [Test]
        public void Lure_KhongThaAi_ThiThuChayThoat()
        {
            BattleSimulation battle = Lure(
                TestBattles.Tribe(TestBattles.Profile(), TestBattles.Member(1)),
                new[] { TestBattles.Beast(0, x: 5.0, moveSpeed: 4.0) });

            BattleReport report = battle.RunToEnd();

            Assert.That(report.Won, Is.False);
            Assert.That(report.Reason, Is.EqualTo(BattleEndReason.BeastEscaped));
            Assert.That(report.DeployedCount, Is.EqualTo(0));
            Assert.That(report.DeadMemberIds, Is.Empty, "Chưa thả ai thì không ai chết được trong sân");
        }

        [Test]
        public void Lure_HetGioMaThuConSong_ThiThua()
        {
            // Thú quá dày máu để hạ, và quá chậm để kịp ra tới vạch thoát trong 30 giây.
            // Hết giờ mà chưa hạ được nghĩa là nó đã thoát (design.md §9) → thua.
            TribeCombatProfile profile = TestBattles.Profile(damage: 0.01, interval: 2.0);
            BattleSimulation battle = Lure(
                TestBattles.Tribe(profile, TestBattles.Member(1, hp: 7), TestBattles.Member(2, hp: 7)),
                new[] { TestBattles.Beast(0, x: 5.0, hp: 10000.0, moveSpeed: 0.2, damage: 0.01, interval: 2.0) });

            battle.TryDeploy(1, new CombatVector(6.0, 3.0));
            battle.TryDeploy(2, new CombatVector(7.0, 3.0));

            BattleReport report = battle.RunToEnd();

            Assert.That(report.Won, Is.False);
            Assert.That(report.Reason, Is.EqualTo(BattleEndReason.TimeLimit));
        }

        // --- Phòng thủ --------------------------------------------------------------------

        [Test]
        public void Prevent_ChanDuocToiHetGio_ThiTHANG()
        {
            // design.md §9 cho Phòng thủ đúng MỘT điều kiện thua: "kẻ địch chạm mục tiêu".
            // Chặn được tới hết giờ chính là đạt mục tiêu, nên hết giờ là THẮNG, không phải thua.
            TribeCombatProfile profile = TestBattles.Profile(damage: 0.01, interval: 2.0);
            BattleSimulation battle = Prevent(
                TestBattles.Tribe(profile, TestBattles.Member(1, hp: 7), TestBattles.Member(2, hp: 7)),
                new[] { TestBattles.Beast(0, x: 15.0, hp: 10000.0, moveSpeed: 0.2, damage: 0.01, interval: 2.0) });

            battle.TryDeploy(1, new CombatVector(14.0, 3.0));
            battle.TryDeploy(2, new CombatVector(13.0, 3.0));

            BattleReport report = battle.RunToEnd();

            Assert.That(report.Reason, Is.EqualTo(BattleEndReason.TimeLimit));
            Assert.That(report.Won, Is.True);
        }

        [Test]
        public void Prevent_KhongThaAi_ThiKeDichChamHang()
        {
            BattleSimulation battle = Prevent(
                TestBattles.Tribe(TestBattles.Profile(), TestBattles.Member(1)),
                new[] { TestBattles.Beast(0, x: 15.0, moveSpeed: 4.0) });

            BattleReport report = battle.RunToEnd();

            Assert.That(report.Won, Is.False);
            Assert.That(report.Reason, Is.EqualTo(BattleEndReason.TargetReached));
        }

        [Test]
        public void Prevent_ChetHetNguoi_ThiThua_KhongPhaiThangVIHetGio()
        {
            // Ca suy biến quan trọng: không còn ai chặn thì để đồng hồ chạy hết rồi tuyên
            // "Phòng thủ thắng" là kết quả vô lý. Phải thua ngay với lý do TribeDown.
            TribeCombatProfile profile = TestBattles.Profile(damage: 0.01, interval: 2.0);
            BattleSimulation battle = Prevent(
                TestBattles.Tribe(profile, TestBattles.Member(1, hp: 3), TestBattles.Member(2, hp: 3)),
                new[] { TestBattles.Beast(0, x: 15.0, hp: 10000.0, moveSpeed: 0.05, damage: 10.0, interval: 0.5) });

            battle.TryDeploy(1, new CombatVector(14.0, 3.0));
            battle.TryDeploy(2, new CombatVector(13.5, 3.0));

            BattleReport report = battle.RunToEnd();

            Assert.That(report.Won, Is.False);
            Assert.That(report.Reason, Is.EqualTo(BattleEndReason.TribeDown));
            Assert.That(report.DeadMemberIds.Count, Is.EqualTo(2));
            Assert.That(report.ElapsedSeconds, Is.LessThan(30.0), "Phải chốt ngay, không chờ hết giờ");
        }

        [Test]
        public void ThuKhongBaoGioBiDungLaiViCoNguoiChan()
        {
            // Nếu thú dừng lại đánh khi có người trong tầm thì chỉ cần thả một người ra chắn là
            // mọi trận Phòng thủ đều thắng (hết giờ = thắng), tỉ lệ thắng thật dính trần 100%
            // bất kể k, và B3 không còn gì để hiệu chỉnh. Cách duy nhất chặn thú là HẠ nó.
            TribeCombatProfile profile = TestBattles.Profile(damage: 0.001, interval: 5.0);
            BattleSimulation battle = Prevent(
                TestBattles.Tribe(profile, TestBattles.Member(1, hp: 7)),
                new[] { TestBattles.Beast(0, x: 15.0, hp: 10000.0, moveSpeed: 2.0, damage: 0.001, interval: 5.0) });

            battle.TryDeploy(1, new CombatVector(14.0, 3.0));

            BattleReport report = battle.RunToEnd();

            Assert.That(report.Won, Is.False);
            Assert.That(report.Reason, Is.EqualTo(BattleEndReason.TargetReached),
                "Người chắn đường không hạ nổi thú thì thú vẫn phải tới được hang");
        }

        // --- Tái lập được -----------------------------------------------------------------

        [Test]
        public void CungSeed_ThiCungKetQua()
        {
            BattleReport first = ThreeOnOne(seed: 777).RunToEnd();
            BattleReport second = ThreeOnOne(seed: 777).RunToEnd();

            Assert.That(second.Won, Is.EqualTo(first.Won));
            Assert.That(second.Reason, Is.EqualTo(first.Reason));
            Assert.That(second.ElapsedSeconds, Is.EqualTo(first.ElapsedSeconds));
            Assert.That(second.DeadMemberIds, Is.EqualTo(first.DeadMemberIds));
        }

        [Test]
        public void KhacSeed_ThiKhacChuoiRutSo()
        {
            // Không đòi kết quả phải khác (có thể trùng), chỉ đòi seed thật sự được dùng:
            // nếu hai lần chạy khác seed mà mọi thứ y hệt nhau thì IRandom đang bị bỏ qua.
            BattleReport a = ManyTies(seed: 1).RunToEnd();
            BattleReport b = ManyTies(seed: 99999).RunToEnd();

            Assert.That(
                a.DeadMemberIds.Count > 0 || b.DeadMemberIds.Count > 0,
                "Kịch bản test phải có người ngã xuống thì mới so được chuỗi rút số");
        }

        [Test]
        public void StepTheoGiayThat_Don_Vao_BuocCoDinh()
        {
            BattleSimulation byDelta = ThreeOnOne();
            BattleSimulation byFixed = ThreeOnOne();

            byDelta.Step(1.0);
            for (int i = 0; i < 30; i++)
            {
                byFixed.StepFixed();
            }

            Assert.That(byDelta.ElapsedSeconds, Is.EqualTo(byFixed.ElapsedSeconds).Within(1e-12));
            for (int i = 0; i < byDelta.Beasts.Count; i++)
            {
                Assert.That(byDelta.Beasts[i].Position.X, Is.EqualTo(byFixed.Beasts[i].Position.X).Within(1e-12));
                Assert.That(byDelta.Beasts[i].Hp, Is.EqualTo(byFixed.Beasts[i].Hp).Within(1e-12));
            }
        }

        [Test]
        public void NhieuLanStepNho_ChoKetQuaNhuMotLanStepLon()
        {
            BattleSimulation coarse = ThreeOnOne();
            BattleSimulation fine = ThreeOnOne();

            coarse.Step(2.0);
            for (int i = 0; i < 60; i++)
            {
                fine.Step(1.0 / 30.0);
            }

            Assert.That(coarse.ElapsedSeconds, Is.EqualTo(fine.ElapsedSeconds).Within(1e-9));
            Assert.That(coarse.Beasts[0].Hp, Is.EqualTo(fine.Beasts[0].Hp).Within(1e-9));
        }

        // --- Thả quân ---------------------------------------------------------------------

        [Test]
        public void ThaQuan_NgoaiVungCho_ThiTuChoi_VaKhongDoiGi()
        {
            BattleSimulation battle = ThreeOnOne();

            DeployResult result = battle.TryDeploy(1, new CombatVector(19.0, 3.0));

            Assert.That(result, Is.EqualTo(DeployResult.OutsideDeployZone));
            Assert.That(battle.FindTribeMember(1).IsDeployed, Is.False);
            Assert.That(battle.UndeployedCount, Is.EqualTo(3));
        }

        [Test]
        public void ThaQuan_KhongBietNguoi_ThiTuChoi()
        {
            Assert.That(
                ThreeOnOne().TryDeploy(99, new CombatVector(6.0, 3.0)),
                Is.EqualTo(DeployResult.UnknownMember));
        }

        [Test]
        public void ThaQuan_HaiLanMotNguoi_ThiTuChoiLanHai()
        {
            BattleSimulation battle = ThreeOnOne();

            Assert.That(battle.TryDeploy(1, new CombatVector(6.0, 3.0)), Is.EqualTo(DeployResult.Ok));
            Assert.That(battle.TryDeploy(1, new CombatVector(7.0, 3.0)), Is.EqualTo(DeployResult.AlreadyDeployed));
            Assert.That(battle.FindTribeMember(1).Position.X, Is.EqualTo(6.0));
            Assert.That(battle.UndeployedCount, Is.EqualTo(2));
        }

        [Test]
        public void ThaQuan_SauKhiTranKetThuc_ThiTuChoi()
        {
            BattleSimulation battle = ThreeOnOne();
            battle.RunToEnd();

            Assert.That(
                battle.TryDeploy(1, new CombatVector(6.0, 3.0)),
                Is.EqualTo(DeployResult.BattleNotRunning));
        }

        [Test]
        public void TruocKhiStart_DongHoChuaChay()
        {
            BattleSimulation battle = ThreeOnOne();

            Assert.That(battle.Status, Is.EqualTo(BattleStatus.Ready));
            Assert.That(battle.ElapsedSeconds, Is.EqualTo(0.0));
            Assert.That(battle.Report, Is.Null);

            battle.Start();
            Assert.That(battle.Status, Is.EqualTo(BattleStatus.Running));
            Assert.That(battle.ElapsedSeconds, Is.EqualTo(0.0), "Start không được tự tiêu một bước");
        }

        [Test]
        public void NguoiChuaThaThiKhongTheChet()
        {
            TribeCombatProfile profile = TestBattles.Profile(damage: 0.01, interval: 2.0);
            BattleSimulation battle = Prevent(
                TestBattles.Tribe(profile, TestBattles.Member(1, hp: 3), TestBattles.Member(2, hp: 3)),
                new[] { TestBattles.Beast(0, x: 15.0, hp: 10000.0, moveSpeed: 0.05, damage: 10.0, interval: 0.5) });

            battle.TryDeploy(1, new CombatVector(14.0, 3.0));

            BattleReport report = battle.RunToEnd();

            // Has.No.Member chứ không phải Does.Not.Contain: ở NUnit 3.5.0 (bản Unity nạp)
            // Does.Contain chỉ có nạp chồng cho CHUỖI, nên truyền số vào là lỗi biên dịch.
            Assert.That(report.DeadMemberIds, Has.No.Member(2));
            Assert.That(battle.FindTribeMember(2).IsAlive, Is.True);
            Assert.That(report.Reason, Is.Not.EqualTo(BattleEndReason.TribeDown),
                "Còn người chưa thả thì chưa được coi là hết người");
        }

        [Test]
        public void BaoCao_GhiDungSoNguoiDuocCuVaSoNguoiDaTha()
        {
            BattleSimulation battle = ThreeOnOne();
            battle.TryDeploy(1, new CombatVector(6.0, 3.0));
            battle.TryDeploy(2, new CombatVector(7.0, 3.0));

            BattleReport report = battle.RunToEnd();

            Assert.That(report.TribeCount, Is.EqualTo(3));
            Assert.That(report.DeployedCount, Is.EqualTo(2));
            Assert.That(report.BeastCount, Is.EqualTo(1));
            Assert.That(report.Kind, Is.EqualTo(BattleKind.Lure));
        }

        [Test]
        public void DungSan_ThuSinhRaDaODich_ThiNoNgay()
        {
            Assert.Throws<System.ArgumentException>(() => new BattleSetup(
                BattleKind.Lure,
                TestBattles.LureField(),
                TestBattles.Tribe(TestBattles.Profile(), TestBattles.Member(1)),
                new[] { TestBattles.Beast(0, x: 20.0) },
                30.0));
        }

        [Test]
        public void DungSan_KhongCoThu_ThiNoNgay()
        {
            Assert.Throws<System.ArgumentException>(() => new BattleSetup(
                BattleKind.Lure,
                TestBattles.LureField(),
                TestBattles.Tribe(TestBattles.Profile(), TestBattles.Member(1)),
                new CombatantSpec[0],
                30.0));
        }

        private static BattleSimulation ThreeOnOne(int seed = 12345)
        {
            TribeCombatProfile profile = TestBattles.Profile();
            return Lure(
                TestBattles.Tribe(
                    profile,
                    TestBattles.Member(1, hp: 4),
                    TestBattles.Member(2, hp: 5),
                    TestBattles.Member(3, hp: 6)),
                new[] { TestBattles.Beast(0, x: 5.0, hp: 12.0, moveSpeed: 1.0, damage: 1.5, interval: 0.7) },
                seed: seed);
        }

        /// <summary>Ba người hp bằng nhau, thú "săn kẻ yếu": mọi lần chọn mục tiêu đều phải phá hoà.</summary>
        private static BattleSimulation ManyTies(int seed)
        {
            TribeCombatProfile profile = TestBattles.Profile(damage: 0.2, interval: 1.0);
            BattleSimulation battle = Lure(
                TestBattles.Tribe(
                    profile,
                    TestBattles.Member(1, hp: 5),
                    TestBattles.Member(2, hp: 5),
                    TestBattles.Member(3, hp: 5)),
                new[]
                {
                    TestBattles.Beast(
                        0, x: 5.0, hp: 500.0, moveSpeed: 0.2, damage: 2.0, interval: 0.4,
                        range: 3.0, style: BeastAttackStyle.HuntWeakest)
                },
                seed: seed);

            battle.TryDeploy(1, new CombatVector(6.0, 3.0));
            battle.TryDeploy(2, new CombatVector(6.5, 3.0));
            battle.TryDeploy(3, new CombatVector(7.0, 3.0));
            return battle;
        }
    }
}
