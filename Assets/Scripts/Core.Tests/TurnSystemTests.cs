using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Sites;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C4 — giải quyết lượt, từng quy tắc design.md §2–§6.</summary>
    [TestFixture]
    public class TurnSystemTests
    {
        // ---------- design.md §5: Kiếm ăn ----------

        [Test]
        public void KiemAn_ThucAnBangKNhanGatherYield()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(3));

            TurnOutcome outcome = g.Turn.EndTurn();

            // v7: TestGame dùng danh mục tối giản (một loài, dinh dưỡng 1, speed trung bình 1.0
            // đúng ngưỡng khung đảm bảo), nên thức ăn bằng ĐÚNG số người đi kiếm ăn.
            // Với danh mục thật thì con số này là ngẫu nhiên — xem GatherRulesTests.
            Assert.AreEqual(9, outcome.FoodFromActivities, "3 người × 3 thức ăn");
            Assert.AreEqual(3, outcome.Gather.Finds.Count);
        }

        [Test]
        public void KiemAn_TinhThanCongMotLanDuyNhat_KhongPhaiMoiNguoi()
        {
            // D2 + design.md §5: Kiếm ăn là MỘT hoạt động, "+1 tinh thần" là một lần.
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(1, outcome.OutcomeOf(ActivityKind.Gather).Morale,
                "5 người đi kiếm ăn vẫn chỉ +1 tinh thần");
        }

        [Test]
        public void KiemAn_NeuronTheoSoNguoi()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(4));

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(4, outcome.NeuronGained, "neuron_per_member = 1");
            Assert.AreEqual(4, g.State.Neuron);
        }

        // ---------- design.md §5: Nâng tinh thần ----------

        [Test]
        public void NangTinhThan_CongTheoDauNguoi()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Morale, g.MemberIds(3));
            g.MeetGatherFloor();

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(6, outcome.OutcomeOf(ActivityKind.Morale).Morale, "+2 mỗi người × 3");
        }

        // ---------- design.md §5: Xây dựng ----------

        [Test]
        public void XayDung_DuSucManhThiDonXongNgay()
        {
            // design-v7 §B.1 (D7 của Khoa): ăn cả ngã về không, không có thanh tiến trình.
            // Thành viên chế độ trung bình có strength = 1.0, nên ba người là đúng 3.0.
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(3);   // D13: Xây dựng mở từ lượt 3

            g.AssignBuild("hoc_da_tru_mua", g.MemberIds(3));
            g.MeetGatherFloor();

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.IsTrue(outcome.Site.Attempted);
            Assert.IsTrue(outcome.Site.Cleared, "3.0 sức mạnh đạt đúng ngưỡng 3.0");
            Assert.AreEqual(1, outcome.BuildLevelAfter);
            Assert.IsTrue(outcome.BuildLevelIncreased);
            Assert.Contains("hoc_da_tru_mua", g.State.ClearedSiteIds);
        }

        [Test]
        public void XayDung_ThieuSucManh_ThiPhiLuotVaKhongTichLuy()
        {
            // Khác hẳn cơ chế điểm xây cũ: hai lượt thiếu KHÔNG cộng lại thành một lượt đủ.
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(3);

            for (int turn = 0; turn < 2; turn++)
            {
                g.AssignBuild("hoc_da_tru_mua", g.MemberIds(2));
                g.MeetGatherFloor();

                TurnOutcome outcome = g.Turn.EndTurn();
                Assert.IsTrue(outcome.Site.Attempted);
                Assert.IsFalse(outcome.Site.Cleared, "2.0 sức mạnh chưa tới ngưỡng 3.0");
                Assert.AreEqual(0, outcome.BuildLevelAfter);
            }

            Assert.AreEqual(0, g.State.ClearedSiteIds.Count, "hai lượt thiếu vẫn là không có gì");
        }

        [Test]
        public void XayDung_DonXongThiSlotKhongHienRaNua()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(3);

            g.AssignBuild("nhanh_cay_gay", g.MemberIds(2));
            g.MeetGatherFloor();
            g.Turn.EndTurn();

            Assert.IsTrue(g.Turn.Sites.IsCleared("nhanh_cay_gay"));

            foreach (SiteView site in g.Turn.Sites.VisibleSites(g.State.Stage, g.Nodes))
            {
                Assert.AreNotEqual("nhanh_cay_gay", site.Id, "slot đã dọn thì biến mất");
            }

            Assert.AreEqual(
                PlanError.SiteNotAvailable,
                g.Plan.ChooseSite("nhanh_cay_gay").Error,
                "và cũng không chọn lại được");
        }

        [Test]
        public void XayDung_ChuaChonSlot_ThiKhongKetThucLuotDuoc()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(3);

            g.Plan.Assign(ActivityKind.Build, g.MemberIds(3));
            g.MeetGatherFloor();

            Assert.AreEqual(PlanError.SiteNotChosen, g.Plan.ValidateForEndTurn().Error);
            Assert.Throws<InvalidOperationException>(() => g.Turn.EndTurn());
        }

        [Test]
        public void XayDung_SlotLoaiDaGiaiDoan2_CanNutS2StoneTools()
        {
            // design-v7 §B.3 (D1 của Khoa): muốn dọn đá thì phải biết đập đá.
            TestGame chua = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);
            Assert.AreEqual(
                PlanError.SiteNotAvailable,
                chua.Plan.ChooseSite("tang_da_lon").Error,
                "chưa mua s2_stone_tools thì slot đá chưa hiện ra");

            TestGame co = TestGame.New(1, GameMode.Mvp, "s2_stone_tools").WithStage(2).AtTurn(3);
            Assert.IsTrue(co.Plan.ChooseSite("tang_da_lon").Succeeded);
        }

        [Test]
        public void XayDung_TinhThanCongMotLan()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(3);   // D13: Xây dựng mở từ lượt 3
            g.AssignBuild("hoc_da_tru_mua", g.MemberIds(3));
            g.MeetGatherFloor();

            TurnOutcome outcome = g.Turn.EndTurn();
            Assert.AreEqual(1, outcome.OutcomeOf(ActivityKind.Build).Morale);
        }

        [Test]
        public void OThucAn_NangCapThiNoiNguongKhungDamBao()
        {
            // design-v7 §B.4 theo cách đọc E2 của Khoa: nâng ổ thì ngưỡng "tổng nhanh nhẹn"
            // của các loài dễ đạt hơn. Cài bằng cách cộng vào tổng của nhóm (§B.4).
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(3);
            Assert.AreEqual(0.0, g.Turn.SpeedThresholdRelief, 1e-12, "ổ đất trống không có hiệu ứng số");

            g.AssignBuild("o_dung_thuc_an", g.MemberIds(2));   // bậc 1 cần 2.0
            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.IsTrue(outcome.Site.Cleared);
            Assert.AreEqual(1, outcome.Site.TierAfter);
            Assert.AreEqual(1, g.State.FoodNestTier);
            Assert.AreEqual(0.2, g.Turn.SpeedThresholdRelief, 1e-12);
            Assert.AreEqual(1, outcome.BuildLevelAfter, "mỗi bậc ổ cũng tính là +1 build_level");
        }

        [Test]
        public void SucChuaHang_TheoVungVaCongTrinh()
        {
            // Test này NÓI VỀ sức chứa, nên nó tự đặt số của vùng thay vì dùng số ghim của TestGame.
            TestGame g = TestGame.New();
            g.State.RegionCapacityOpen = 4;
            g.State.RegionCapacityMax = 8;

            Assert.AreEqual(4, g.State.LairCapacity(g.Config), "vùng 0 mở sẵn 4 chỗ");

            // v7 §F.2: mỗi công trình dọn xong mở thêm đúng một chỗ, chặn ở trần của vùng.
            g.State.ClearedSiteIds.Add("nhanh_cay_gay");
            g.State.ClearedSiteIds.Add("bai_da_nho");
            Assert.AreEqual(6, g.State.LairCapacity(g.Config));

            for (int i = 0; i < 10; i++)
            {
                g.State.ClearedSiteIds.Add("gia_" + i);
            }

            Assert.AreEqual(8, g.State.LairCapacity(g.Config), "không vượt trần của vùng");
        }

        // ---------- design.md §5.2: Săn bắt ----------

        [Test]
        public void SanBat_ThangThiCoThucAnTinhThanVaChienTich()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            g.Combat.ThenWin();

            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(3));
            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();

            ActivityOutcome hunt = outcome.OutcomeOf(ActivityKind.Hunt);
            Assert.IsTrue(hunt.CombatWon.Value);
            Assert.AreEqual(15, hunt.Food, "giai đoạn 2: 3 người × 5 thức ăn");
            Assert.AreEqual(3, hunt.Morale);
            Assert.AreEqual(1, g.State.Hunts, "chiến tích săn +1");
            Assert.AreEqual(1, g.State.CombatWins, "thắng trận bất kỳ cũng tính (design.md §7.1)");
        }

        [Test]
        public void SanBat_ThuaThiTruTinhThan_VaVanDuocNeuron()
        {
            // design.md §5: "Neuron nhận được = neuron[hoạt động] × k, nhận cả khi thất bại".
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            g.Combat.ThenLose();

            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(2));
            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();

            ActivityOutcome hunt = outcome.OutcomeOf(ActivityKind.Hunt);
            Assert.IsFalse(hunt.CombatWon.Value);
            Assert.AreEqual(0, hunt.Food);
            Assert.AreEqual(-2, hunt.Morale);
            Assert.AreEqual(12, hunt.Neuron, "6 neuron × 2 người, vẫn nhận dù thua");
            Assert.AreEqual(0, g.State.Hunts);
        }

        [Test]
        public void SanBat_NguoiChetTrongTranLaChetThat_KeCaKhiThang()
        {
            // design.md §9.
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            int[] party = g.MemberIds(3);
            g.Combat.ThenWin(party[0]);

            g.Plan.Assign(ActivityKind.Hunt, party);
            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(4, g.State.Crew);
            Assert.IsNull(g.State.FindMember(party[0]));
            CollectionAssert.Contains(outcome.DeadMemberIds, party[0]);
        }

        [Test]
        public void SanBat_ThuaCoTheChetNhieuHonSoNguoiDiSan()
        {
            // design.md §5.2: "Thua: mất 2 người" — không giới hạn ở người tham gia.
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            int[] all = g.AllMemberIds();
            g.Combat.ThenLose(all[0], all[4]);

            g.Plan.Assign(ActivityKind.Hunt, all[0]);
            g.Plan.Assign(ActivityKind.Gather, new[] { all[1], all[2], all[3] });   // đủ ăn, để cô lập cái chết do combat
            g.Turn.EndTurn();

            Assert.AreEqual(3, g.State.Crew);
            Assert.IsNull(g.State.FindMember(all[4]), "người ở nhà cũng chết dù không đi săn");
        }

        // ---------- design.md §5.2: Khám phá ----------

        [Test]
        public void KhamPha_ThangThiDiCuSangVungMoi()
        {
            TestGame g = TestGame.New().AtTurn(6);
            g.Combat.ThenWin();

            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(3));
            g.MeetGatherFloor();
            g.TakeEveryoneIfMigrating();
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(1, g.State.Region);
            Assert.AreEqual(1, g.State.MigratedEver);
            Assert.AreEqual(1, g.State.CombatWins);
            Assert.IsTrue(outcome.Migrated);
            Assert.AreEqual(3, outcome.OutcomeOf(ActivityKind.Explore).Morale);
            Assert.AreEqual(0, outcome.LeftBehindMemberIds.Count, "vùng 1 mở sẵn 7 chỗ, cả bộ tộc đi được");
        }

        [Test]
        public void KhamPha_KhongVuotQuaVungCuoiCuaMode()
        {
            TestGame g = TestGame.New().AtTurn(6);
            g.State.Region = 1;
            g.Nodes.Add("s2_stone_tools");

            // Vùng đã kịch trần MVP nên hoạt động đóng; không cử người được.
            PlanResult result = g.Plan.Assign(ActivityKind.Explore, g.MemberIds(2));
            Assert.AreEqual(PlanError.ActivityLocked, result.Error);
        }

        [Test]
        public void DiCuKhongLamTangHaiLuomCuaChinhLuotDo()
        {
            // gather_yield chốt từ đầu lượt: con số người chơi thấy trong dự báo phải là con số thật.
            TestGame g = TestGame.New().AtTurn(6);
            g.Combat.ThenWin();

            g.Plan.Assign(ActivityKind.Gather, new[] { g.State.Members[0].Id, g.State.Members[3].Id });
            g.Plan.Assign(ActivityKind.Explore, new[] { g.State.Members[1].Id, g.State.Members[2].Id });
            g.TakeEveryoneIfMigrating();

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(1, g.State.Region, "đã di cư");
            Assert.AreEqual(6, outcome.OutcomeOf(ActivityKind.Gather).Food, "hai người × 3 thức ăn");
        }

        // ---------- design.md §4: thức ăn và đói ----------

        [Test]
        public void ThucAnKhongDeDanh()
        {
            // design.md §4: thức ăn kiếm trong lượt chỉ dùng cho lượt đó, dư thì bỏ.
            TestGame g = TestGame.New();

            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            TurnOutcome rich = g.Turn.EndTurn();
            Assert.AreEqual(15, rich.FoodTotal, "5 người × 3, dư 10 so với nhu cầu 5");
            Assert.IsFalse(rich.Starved);

            // Lượt sau lên giai đoạn 3 (khẩu phần 2/người) và chỉ cử đúng sàn: dư của lượt trước bị bỏ.
            g.WithStage(3);
            g.MeetGatherFloor();
            TurnOutcome poor = g.Turn.EndTurn();
            Assert.AreEqual(6, poor.FoodTotal, "2 người × 3, không mang gì từ lượt trước sang");
            Assert.IsTrue(poor.Starved);
        }

        [Test]
        public void Doi_MatDungMotThanhVien()
        {
            // design.md §4 [ĐỀ XUẤT]: mất đúng 1 thành viên, chọn ngẫu nhiên.
            // Giai đoạn 3: khẩu phần 2/người nên 6 thức ăn của sàn không đủ nuôi 5 người.
            TestGame g = TestGame.New().WithStage(3).MeetGatherFloor();

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(6, outcome.FoodTotal, "đúng sàn: 2 người × 3 thức ăn");
            Assert.IsTrue(outcome.Starved);
            Assert.AreEqual(1, outcome.StarvationDeaths);
            Assert.AreEqual(4, g.State.Crew);
        }

        [Test]
        public void DuThucAnThiKhongDoi()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(2));

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(6, outcome.FoodTotal);
            Assert.AreEqual(5, outcome.FoodNeeded);
            Assert.IsFalse(outcome.Starved);
            Assert.AreEqual(5, g.State.Crew);
        }

        [Test]
        public void ThieuDungMotPhanAn_VanMatNguoi()
        {
            // Chưa có nút "Chia sẻ thức ăn" (giai đoạn 2) thì thiếu 1 vẫn chết 1.
            // Giai đoạn 3, khẩu phần 2/người: 5 người cần 10, ba người kiếm ăn mang về 9.
            TestGame g = TestGame.New().WithStage(3);
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(3));

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(9, outcome.FoodTotal);
            Assert.AreEqual(10, outcome.FoodNeeded, "thiếu đúng 1 phần ăn");
            Assert.IsTrue(outcome.Starved);
        }

        [Test]
        public void KhauPhanTinhTrenSoNguoiConSongSauCombat()
        {
            // Chết trong combat thì không còn miệng ăn ở cuối lượt.
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            int[] all = g.AllMemberIds();
            g.Combat.ThenLose(all[3], all[4]);

            g.Plan.Assign(ActivityKind.Gather, new[] { all[0], all[1], all[3] });
            g.Plan.Assign(ActivityKind.Hunt, all[2]);

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(3, g.State.Crew);
            Assert.AreEqual(6, outcome.FoodNeeded, "3 người sống × 2 khẩu phần ở giai đoạn 2");
            Assert.AreEqual(9, outcome.FoodTotal, "3 người × 3, kể cả người chết sau đó");
            Assert.IsFalse(outcome.Starved);
        }

        [Test]
        public void BoTocHetNguoi_ThiBaoThua()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            g.Combat.ThenLose(g.AllMemberIds());

            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(1));
            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(0, g.State.Crew);
            Assert.IsTrue(outcome.TribeWipedOut);
            Assert.IsFalse(outcome.Starved, "không còn ai thì không có ai chết đói nữa");
        }

        // ---------- design.md §4: tinh thần ----------

        [Test]
        public void TinhThanKhongAm()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            g.Combat.ThenLose();
            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(2));
            g.MeetGatherFloor();

            g.Turn.EndTurn();

            Assert.AreEqual(0, g.State.Morale, "−2 từ 0 vẫn chặn ở 0");
        }

        // ---------- design.md §6: sinh sản ----------

        [Test]
        public void SinhSan_DuTinhThanThiThemMotNguoi()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(2));
            g.Plan.Assign(ActivityKind.Morale, new[] { g.State.Members[2].Id, g.State.Members[3].Id });

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.IsFalse(outcome.Starved);
            Assert.AreEqual(1, outcome.BornMemberIds.Count);
            Assert.AreEqual(6, g.State.Crew);
            Assert.AreEqual(1, g.State.Morale, "4 (nâng) + 1 (kiếm ăn) − 4 (sinh) = 1");
        }

        [Test]
        public void SinhSan_ToiDaMotNguoiMoiLuot()
        {
            TestGame g = TestGame.New();
            g.State.Morale = 100;
            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(1, outcome.BornMemberIds.Count, "dù thừa tinh thần vẫn chỉ 1");
            Assert.AreEqual(6, g.State.Crew);
        }

        [Test]
        public void SinhSan_DungLaiKhiDayHang()
        {
            // design.md §6: chỉ sinh khi số thành viên < sức chứa hang (mặc định 6).
            TestGame g = TestGame.New();
            g.State.Morale = 100;

            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            g.Turn.EndTurn();
            Assert.AreEqual(6, g.State.Crew);

            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            TurnOutcome full = g.Turn.EndTurn();
            Assert.AreEqual(0, full.BornMemberIds.Count, "đã đầy hang");
            Assert.AreEqual(6, g.State.Crew);
        }

        [Test]
        public void SinhSan_KhongSinhKhiKhongConAi()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            g.State.Morale = 100;
            g.Combat.ThenLose(g.AllMemberIds());

            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(1));
            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(0, outcome.BornMemberIds.Count);
            Assert.AreEqual(0, g.State.Crew);
        }

        [Test]
        public void SinhSan_XayRaSauKhiAn()
        {
            // design.md §2 bước 4: ăn → sinh thành viên. Người mới không phải chịu khẩu phần của lượt này.
            TestGame g = TestGame.New();
            g.State.Morale = 4;
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(2));

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(5, outcome.FoodNeeded, "tính trên 5 người, chưa kể người vừa sinh");
            Assert.IsFalse(outcome.Starved);
            Assert.AreEqual(6, g.State.Crew);
        }

        [Test]
        public void ThanhVienMoiSinh_CoIdMoiKhongTrung()
        {
            TestGame g = TestGame.New();
            g.State.Morale = 4;
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(2));

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(6, outcome.BornMemberIds[0]);
            Assert.AreEqual(7, g.State.NextMemberId);

            HashSet<int> ids = new HashSet<int>();
            for (int i = 0; i < g.State.Members.Count; i++)
            {
                Assert.IsTrue(ids.Add(g.State.Members[i].Id));
            }
        }

        // ---------- design.md §2: vòng lượt ----------

        [Test]
        public void EndTurn_TangSoLuot()
        {
            TestGame g = TestGame.New();
            Assert.AreEqual(1, g.State.Turn);

            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(1, outcome.Turn, "báo cáo là của lượt vừa kết thúc");
            Assert.AreEqual(2, g.State.Turn);
        }

        [Test]
        public void ThuTuGiaiQuyet_XayDungSauCombat_NenSlotVuaDonKhongCuuDuocTranCungLuot()
        {
            // ĐẢO NGƯỢC so với trước v7. D7 của Khoa: "lượt sau slot đó sẽ được dọn".
            // Core cài lời hứa đó bằng cách đặt Xây dựng cuối thứ tự giải quyết.
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: mở từ lượt 3
            g.Combat.ThenWin();

            g.AssignBuild("nhanh_cay_gay", g.MemberIds(2));   // 2.0 ≥ 1.8, chắc chắn dọn xong
            g.Plan.Assign(ActivityKind.Hunt, new[] { g.State.Members[2].Id });
            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.IsTrue(outcome.Site.Cleared);
            Assert.AreEqual(1, g.State.BuildLevel);
            Assert.AreEqual(0, g.Combat.Requests[0].BuildLevel,
                "trận của lượt này vẫn dùng cấp công trình CŨ — slot chỉ có hiệu lực từ lượt sau");
        }

        [Test]
        public void DuBao_ChiTinhThucAnChacChan()
        {
            // design.md §2: "cuối lượt cần X thức ăn, đang có Y".
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3

            // Giai đoạn 2, khẩu phần 2 mỗi người (balance-log.md #9): 5 người × 2 = 10.
            Assert.AreEqual(10, g.Turn.FoodNeeded);
            Assert.AreEqual(0, g.Turn.ForecastFood);

            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(2));
            int chacChan = g.Turn.ForecastFood;
            Assert.Greater(chacChan, 0, "khung đảm bảo cho phần chắc chắn");

            g.Plan.Assign(ActivityKind.Hunt, new[] { g.State.Members[2].Id });
            Assert.AreEqual(chacChan, g.Turn.ForecastFood, "thức ăn từ săn bắt là không chắc chắn, không đưa vào dự báo");
        }

        [Test]
        public void NguoiChetODauLuotKhongThamGiaHoatDongSau()
        {
            // Giai đoạn 3 để có đủ 6 năng lượng cho Săn (2) + Khám phá (3) + Kiếm ăn (1).
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(3).AtTurn(3);
            int[] all = g.AllMemberIds();

            // Săn thua và cướp đi người đang được cử đi Khám phá ở cùng lượt.
            g.Combat.ThenLose(all[0], all[3]).ThenWin();

            g.AtTurn(6);
            g.Plan.Assign(ActivityKind.Hunt, all[0]);
            g.Plan.Assign(ActivityKind.Explore, new[] { all[3], all[4] });
            g.Plan.Assign(ActivityKind.Gather, new[] { all[1], all[2] });
            g.TakeEveryoneIfMigrating();

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(1, outcome.OutcomeOf(ActivityKind.Explore).PartySize,
                "người đã chết ở trận săn không đi khám phá nữa");
            Assert.AreEqual(1, g.Combat.Requests[1].PartySize);
        }

        // ---------- điểm nối cho C6 ----------

        [Test]
        public void SuKienDauLuot_CongThucAnVaoLuotDangTinh()
        {
            TestGame g = TestGame.New();
            g.Events.OnTurnStart = ctx => ctx.AddFood(4);

            g.Turn.BeginTurn();
            Assert.AreEqual(4, g.Turn.ForecastFood, "dự báo phải thấy thức ăn sự kiện");

            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();
            Assert.AreEqual(4, outcome.FoodFromEvents);
            Assert.AreEqual(10, outcome.FoodTotal, "4 từ sự kiện + 6 từ hai người đi kiếm ăn");
        }

        [Test]
        public void SuKienLuotMay_CongTaiNguyenVaChienTich()
        {
            TestGame g = TestGame.New();
            g.Events.OnMachineTurn = ctx =>
            {
                ctx.AddMorale(2);
                ctx.AddNeuron(3);
                ctx.AddCombatWin();
            };

            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(5, outcome.NeuronGained, "NeuronGained chỉ đếm phần từ hoạt động");
            Assert.AreEqual(8, g.State.Neuron, "5 từ kiếm ăn + 3 từ sự kiện");
            Assert.AreEqual(1, g.State.CombatWins);
        }

        [Test]
        public void SuKienGietNguoi_GiamSiSoVaVaoDanhSachChet()
        {
            TestGame g = TestGame.New();
            int victim = g.MemberIds(1)[0];
            g.Events.OnMachineTurn = ctx => ctx.KillMember(victim);

            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(4, g.State.Crew);
            CollectionAssert.Contains(outcome.DeadMemberIds, victim);
        }

        [Test]
        public void KhongCoHeSuKien_ThiVongLuotVanChay()
        {
            TestGame g = TestGame.New();
            g.Events.OnTurnStart = null;
            g.Events.OnMachineTurn = null;

            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            Assert.DoesNotThrow(() => g.Turn.EndTurn());
        }

        // ---------- tái lập ----------

        [Test]
        public void CungSeedVaCungDuDinh_ChoCungKetQua()
        {
            // Chỉ khẳng định điều thật sự được bảo đảm: cùng seed cho cùng kết quả.
            // KHÔNG khẳng định hai seed khác nhau cho kết quả khác nhau — với kịch bản "ai cũng đi
            // kiếm ăn, không combat, không sự kiện" thì ván gần như tất định, nên hai seed ra cùng
            // số liệu là hợp lệ chứ không phải lỗi. Tính phân kỳ của bộ sinh số đã có RandomTests lo.
            Assert.AreEqual(RunTenTurns(2026), RunTenTurns(2026));
            Assert.AreEqual(RunTenTurns(777), RunTenTurns(777));
        }

        private static string RunTenTurns(int seed)
        {
            TestGame g = TestGame.New(seed);
            for (int i = 0; i < 10; i++)
            {
                g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
                g.Turn.EndTurn();
                if (g.State.Crew == 0)
                {
                    break;
                }
            }

            return g.State.Turn + "/" + g.State.Crew + "/" + g.State.Morale + "/" + g.State.Neuron;
        }
    }
}
