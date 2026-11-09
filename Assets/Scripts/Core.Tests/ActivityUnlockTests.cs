using BeforeUs.Core;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C4 — mở khóa năm hoạt động (design.md §5, D1).</summary>
    [TestFixture]
    public class ActivityUnlockTests
    {
        [Test]
        public void KiemAnVaNangTinhThan_MoTuDau()
        {
            TestGame g = TestGame.New();
            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Gather));
            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Morale));
        }

        [Test]
        public void XayDungVaSanBat_MoTuLuot3()
        {
            // D13 (Khoa quyết 2026-11-08): bỏ hẳn điều kiện nút, mở từ lượt 3 — cùng lượt với
            // trận hướng dẫn ở design.md §8. Thay cho D1 cũ (phải mua s2_stone_tools / s2_scavenge_meat).
            TestGame g = TestGame.New();

            foreach (int turn in new[] { 1, 2 })
            {
                g.AtTurn(turn);
                Assert.IsFalse(g.Plan.IsUnlocked(ActivityKind.Build), "lượt " + turn);
                Assert.IsFalse(g.Plan.IsUnlocked(ActivityKind.Hunt), "lượt " + turn);
            }

            g.AtTurn(3);
            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Build));
            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Hunt));
        }

        [Test]
        public void XayDungVaSanBat_KhongConPhuThuocGiaiDoanHayNut()
        {
            // D13 bỏ cả hai điều kiện cũ: giai đoạn 1 và chưa mua nút nào vẫn mở được từ lượt 3.
            TestGame g = TestGame.New().AtTurn(3);
            Assert.AreEqual(1, g.State.Stage);
            Assert.AreEqual(0, g.State.PurchasedNodeIds.Count);

            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Build));
            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Hunt));
        }

        [Test]
        public void KhamPha_MoTuLuot6()
        {
            TestGame g = TestGame.New();

            for (int turn = 1; turn <= 5; turn++)
            {
                g.AtTurn(turn);
                Assert.IsFalse(g.Plan.IsUnlocked(ActivityKind.Explore), "lượt " + turn);
            }

            g.AtTurn(6);
            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Explore));
        }

        [Test]
        public void KhamPha_DongLaiKhiHetVungChuaDen()
        {
            // design.md §5: "Từ lượt 6; còn vùng chưa đến". MVP: region_max = 1.
            TestGame g = TestGame.New().AtTurn(20);
            Assert.AreEqual(1, g.Config.RegionMaxIndex);
            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Explore));

            g.State.Region = 1;
            Assert.IsFalse(g.Plan.IsUnlocked(ActivityKind.Explore), "đã tới vùng cuối của MVP");
        }

        [Test]
        public void HoatDongChuaMoKhoa_ThiKhongCuNguoiDuoc()
        {
            TestGame g = TestGame.New();
            PlanResult result = g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(2));

            Assert.AreEqual(PlanError.ActivityLocked, result.Error);
            Assert.AreEqual(0, g.Plan.EnergySpent);
        }

        [Test]
        public void BanDayDu_KhamPhaMoToiVung5()
        {
            TestGame g = TestGame.New(1, GameMode.Full).AtTurn(20);
            g.State.Region = 1;
            Assert.IsTrue(g.Plan.IsUnlocked(ActivityKind.Explore), "bản đầy đủ còn 4 vùng nữa");

            g.State.Region = 5;
            Assert.IsFalse(g.Plan.IsUnlocked(ActivityKind.Explore));
        }
    }
}
