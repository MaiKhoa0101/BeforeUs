using BeforeUs.Core;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// C4 — D2: phân công trong lượt là dự định, sửa tự do, không gì được tính cho tới EndTurn.
    /// design.md §2, §5.
    /// </summary>
    [TestFixture]
    public class TurnPlanTests
    {
        [Test]
        public void MotHoatDongTraNangLuongDungMotLan_KhongPhuThuocSoNguoi()
        {
            // design.md §2: "mỗi hoạt động trả năng lượng một lần (không phụ thuộc số người)".
            TestGame g = TestGame.New();

            Assert.IsTrue(g.Plan.Assign(ActivityKind.Gather, g.MemberIds(1)).Succeeded);
            Assert.AreEqual(1, g.Plan.EnergySpent);

            Assert.IsTrue(g.Plan.Assign(ActivityKind.Gather, g.MemberIds(5)).Succeeded);
            Assert.AreEqual(1, g.Plan.EnergySpent, "cử 5 người vẫn là một hoạt động, vẫn 1 năng lượng");
        }

        [Test]
        public void MotHoatDongXuatHienToiDaMotLan_GanLaiThiThayThe()
        {
            // D2: trạng thái là map hoạt động → tập memberId, nên gán lại là THAY THẾ chứ không cộng dồn.
            TestGame g = TestGame.New();

            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(3));
            Assert.AreEqual(3, g.Plan.PartySizeOn(ActivityKind.Gather));

            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(1));
            Assert.AreEqual(1, g.Plan.PartySizeOn(ActivityKind.Gather));
            Assert.AreEqual(1, g.Plan.EnergySpent);
        }

        [Test]
        public void SuaPhanCongTuDoNhieuLan_KhongGiDuocTinh()
        {
            // D2: "mọi thứ chỉ là dự định. Không có gì được tính cho tới khi người chơi bấm kết thúc lượt".
            TestGame g = TestGame.New();
            int moraleBefore = g.State.Morale;
            int neuronBefore = g.State.Neuron;
            int crewBefore = g.State.Crew;

            for (int i = 0; i < 20; i++)
            {
                g.Plan.Assign(ActivityKind.Gather, g.MemberIds(1 + i % 5));
                g.Plan.Assign(ActivityKind.Morale, new int[0]);
                g.Plan.ClearAll();
                g.Plan.Assign(ActivityKind.Morale, g.MemberIds(2));
            }

            Assert.AreEqual(moraleBefore, g.State.Morale);
            Assert.AreEqual(neuronBefore, g.State.Neuron);
            Assert.AreEqual(crewBefore, g.State.Crew);
            Assert.AreEqual(1, g.State.Turn);
        }

        [Test]
        public void MoiThanhVienToiDaMotHoatDongMoiLuot()
        {
            TestGame g = TestGame.New();
            int[] ids = g.MemberIds(2);

            Assert.IsTrue(g.Plan.Assign(ActivityKind.Gather, ids).Succeeded);

            PlanResult result = g.Plan.Assign(ActivityKind.Morale, ids[0]);
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(PlanError.MemberAlreadyAssigned, result.Error);
            Assert.AreEqual(0, g.Plan.PartySizeOn(ActivityKind.Morale), "thất bại thì không đổi gì");
        }

        [Test]
        public void CungMotNguoiHaiLanTrongMotDanhSach_ThiTuChoi()
        {
            TestGame g = TestGame.New();
            int id = g.MemberIds(1)[0];

            PlanResult result = g.Plan.Assign(ActivityKind.Gather, id, id);
            Assert.AreEqual(PlanError.DuplicateMember, result.Error);
        }

        [Test]
        public void ThanhVienKhongTonTai_ThiTuChoi()
        {
            TestGame g = TestGame.New();
            PlanResult result = g.Plan.Assign(ActivityKind.Gather, 9999);
            Assert.AreEqual(PlanError.UnknownMember, result.Error);
        }

        [Test]
        public void VuotNganSachNangLuong_ThiTuChoi()
        {
            // Giai đoạn 1: energy_max = 4. Kiếm ăn 1 + Nâng tinh thần 1 + Khám phá 3 = 5 > 4.
            TestGame g = TestGame.New().AtTurn(6);

            Assert.IsTrue(g.Plan.Assign(ActivityKind.Gather, g.MemberIds(1)).Succeeded);
            Assert.IsTrue(g.Plan.Assign(ActivityKind.Morale, g.State.Members[1].Id).Succeeded);
            Assert.AreEqual(2, g.Plan.EnergySpent);
            Assert.AreEqual(2, g.Plan.EnergyRemaining);

            PlanResult result = g.Plan.Assign(ActivityKind.Explore, g.State.Members[2].Id);
            Assert.AreEqual(PlanError.NotEnoughEnergy, result.Error);
            Assert.AreEqual(2, g.Plan.EnergySpent, "từ chối thì ngân sách không đổi");
        }

        [Test]
        public void BoMotHoatDong_ThiTraLaiNangLuong()
        {
            TestGame g = TestGame.New().AtTurn(6);

            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(1));
            g.Plan.Assign(ActivityKind.Morale, g.State.Members[1].Id);
            Assert.AreEqual(2, g.Plan.EnergySpent);

            g.Plan.Clear(ActivityKind.Morale);
            Assert.AreEqual(1, g.Plan.EnergySpent);

            Assert.IsTrue(g.Plan.Assign(ActivityKind.Explore, g.State.Members[2].Id).Succeeded);
            Assert.AreEqual(4, g.Plan.EnergySpent);
        }

        [Test]
        public void DanhSachRong_TuongDuongBoHoatDong()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(2));
            Assert.AreEqual(1, g.Plan.EnergySpent);

            Assert.IsTrue(g.Plan.Assign(ActivityKind.Gather, new int[0]).Succeeded);
            Assert.AreEqual(0, g.Plan.EnergySpent);
            Assert.AreEqual(0, g.Plan.PartySizeOn(ActivityKind.Gather));
        }

        [Test]
        public void DemNguoiRanhVaNguoiDaCu()
        {
            TestGame g = TestGame.New();
            Assert.AreEqual(5, g.Plan.IdleMemberCount);

            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(3));
            Assert.AreEqual(3, g.Plan.AssignedMemberCount);
            Assert.AreEqual(2, g.Plan.IdleMemberCount);
            Assert.IsTrue(g.Plan.IsBusy(g.MemberIds(1)[0]));
            Assert.IsFalse(g.Plan.IsBusy(g.State.Members[4].Id));
        }

        [Test]
        public void SauEndTurn_DuDinhXoaSach()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            g.Turn.EndTurn();

            Assert.AreEqual(0, g.Plan.AssignedMemberCount);
            Assert.AreEqual(0, g.Plan.EnergySpent);
        }

        // ---------- CanAfford: truy vấn cho UI làm mờ thẻ ----------

        [Test]
        public void CanAfford_DuNangLuongThiDung()
        {
            // Giai đoạn 1: energy_max = 4. Kiếm ăn 1, Nâng tinh thần 1, Khám phá 3.
            TestGame g = TestGame.New().AtTurn(6);

            Assert.IsTrue(g.Plan.CanAfford(ActivityKind.Gather));
            Assert.IsTrue(g.Plan.CanAfford(ActivityKind.Explore));
            Assert.AreEqual(1, g.Plan.EnergyCostToAdd(ActivityKind.Gather));
            Assert.AreEqual(3, g.Plan.EnergyCostToAdd(ActivityKind.Explore));
        }

        [Test]
        public void CanAfford_HetNganSachThiSai()
        {
            TestGame g = TestGame.New().AtTurn(6);

            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(1));
            g.Plan.Assign(ActivityKind.Morale, g.State.Members[1].Id);
            Assert.AreEqual(2, g.Plan.EnergyRemaining);

            Assert.IsFalse(g.Plan.CanAfford(ActivityKind.Explore), "Khám phá tốn 3, chỉ còn 2");
        }

        [Test]
        public void CanAfford_HoatDongDaCoNguoi_ThiLuonDung()
        {
            // Ca biên: một hoạt động trả năng lượng đúng một lần mỗi lượt (D2),
            // nên cử thêm người vào hoạt động đã có thì không tốn thêm gì — kể cả khi hết sạch năng lượng.
            TestGame g = TestGame.New().AtTurn(6);

            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(1));
            g.Plan.Assign(ActivityKind.Gather, g.State.Members[1].Id);
            Assert.AreEqual(0, g.Plan.EnergyRemaining, "4 năng lượng đã dùng hết");

            Assert.AreEqual(0, g.Plan.EnergyCostToAdd(ActivityKind.Explore));
            Assert.IsTrue(g.Plan.CanAfford(ActivityKind.Explore), "thêm người vào Khám phá đang có thì miễn phí");
            Assert.IsTrue(g.Plan.CanAfford(ActivityKind.Gather));

            Assert.IsFalse(g.Plan.CanAfford(ActivityKind.Morale), "hoạt động mới thì vẫn phải trả");
        }

        [Test]
        public void CanAfford_KhongCoTacDungPhu()
        {
            TestGame g = TestGame.New().AtTurn(6);
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(2));

            int spentBefore = g.Plan.EnergySpent;
            int assignedBefore = g.Plan.AssignedMemberCount;

            for (int i = 0; i < 50; i++)
            {
                foreach (ActivityKind activity in ActivityRules.All)
                {
                    g.Plan.CanAfford(activity);
                    g.Plan.EnergyCostToAdd(activity);
                }
            }

            Assert.AreEqual(spentBefore, g.Plan.EnergySpent);
            Assert.AreEqual(assignedBefore, g.Plan.AssignedMemberCount);
            Assert.AreEqual(6, g.State.Turn, "truy vấn không được làm lượt trôi đi");
        }

        [Test]
        public void CanAfford_KhongNoiGiVeMoKhoa()
        {
            // Tách bạch có chủ đích: Săn bắt chưa mở khóa nhưng vẫn "đủ năng lượng".
            // UI cần cả hai truy vấn để nói đúng lý do làm mờ thẻ.
            TestGame g = TestGame.New().WithStage(2);

            Assert.IsFalse(g.Plan.IsUnlocked(ActivityKind.Hunt));
            Assert.IsTrue(g.Plan.CanAfford(ActivityKind.Hunt));
        }

        [Test]
        public void CanAfford_KhopVoiKetQuaAssignThucTe()
        {
            // Bất biến: CanAfford đúng khi và chỉ khi Assign không trả NotEnoughEnergy.
            foreach (ActivityKind activity in ActivityRules.All)
            {
                TestGame g = TestGame.New(1, GameMode.Mvp, "s2_stone_tools", "s2_scavenge_meat")
                    .WithStage(2).AtTurn(6);

                g.Plan.Assign(ActivityKind.Explore, g.MemberIds(1));
                g.Plan.Assign(ActivityKind.Hunt, g.State.Members[1].Id);

                bool predicted = g.Plan.CanAfford(activity);
                PlanResult result = g.Plan.Assign(activity, g.State.Members[2].Id);
                bool ranOutOfEnergy = result.Error == PlanError.NotEnoughEnergy;

                Assert.AreEqual(predicted, !ranOutOfEnergy, activity.ToString());
            }
        }

        [Test]
        public void NangLuongToiDaTheoGiaiDoan()
        {
            // design.md §4: energy_max = 4 / 5 / 6 / 7.
            TestGame g = TestGame.New();
            Assert.AreEqual(4, g.Plan.EnergyMax);

            g.WithStage(2);
            Assert.AreEqual(5, g.Plan.EnergyMax);

            g.WithStage(4);
            Assert.AreEqual(7, g.Plan.EnergyMax);
        }
    }
}
