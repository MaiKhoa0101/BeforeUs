using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;
using BeforeUs.Core.Save;
using BeforeUs.Core.Sites;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// C7 × v7 §F.2 — lưu và tải ván QUA MỘT LẦN DI CƯ.
    ///
    /// Vì sao tách thành một bộ riêng: di cư là thao tác **một chiều, phá huỷ, xoá người khỏi
    /// bộ tộc vĩnh viễn**, và nó là phần mới nhất của Core. Hỏng ở đây không làm game văng —
    /// nó làm người chơi tải ván lên và thấy một bộ tộc sai, hoặc thấy lại những người lẽ ra
    /// đã mất. Đó là loại hỏng im lặng đắt nhất.
    ///
    /// Dùng TestGame (combat lập trình sẵn) chứ không dùng BotGame, vì phép kiểm này cần
    /// di cư xảy ra ĐÚNG lúc mình muốn, không phải lúc xác suất cho phép.
    /// </summary>
    [TestFixture]
    public sealed class SaveMigrationTests
    {
        /// <summary>Dựng một ván đứng ngay trước lần di cư, với đoàn đã chọn sẵn.</summary>
        private static TestGame AtMigrationEdge(IReadOnlyList<int> takeAlong = null)
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(6);   // Khám phá mở từ lượt 6
            g.Combat.ThenWin();

            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(2));
            g.MeetGatherFloor();

            if (takeAlong == null)
            {
                g.TakeEveryoneIfMigrating();
            }
            else
            {
                PlanResult r = g.Plan.ChooseMigrationParty(takeAlong);
                Assert.IsTrue(r.Succeeded, r.Message);
            }

            return g;
        }

        private static GameState RoundTrip(GameState state, XorShift128Random random, BalanceConfig config)
        {
            string json = SaveGame.Write(state, random, GameMode.Mvp);
            return SaveGame.Read(json, config).State;
        }

        private static void AssertSameTribe(GameState a, GameState b, string what)
        {
            Assert.AreEqual(a.Crew, b.Crew, what + ": sĩ số");
            Assert.AreEqual(a.Region, b.Region, what + ": vùng");
            Assert.AreEqual(a.RegionCapacityOpen, b.RegionCapacityOpen, what + ": chỗ đã mở");
            Assert.AreEqual(a.RegionCapacityMax, b.RegionCapacityMax, what + ": trần chỗ");
            Assert.AreEqual(a.BuildLevel, b.BuildLevel, what + ": cấp công trình");
            Assert.AreEqual(a.FoodNestTier, b.FoodNestTier, what + ": bậc ổ");
            CollectionAssert.AreEqual(a.ClearedSiteIds, b.ClearedSiteIds, what + ": công trình đã dọn");
            CollectionAssert.AreEqual(a.DiscoveredPlantIds, b.DiscoveredPlantIds, what + ": loài đã khám phá");

            List<int> idsA = new List<int>();
            List<int> idsB = new List<int>();
            for (int i = 0; i < a.Members.Count; i++) idsA.Add(a.Members[i].Id);
            for (int i = 0; i < b.Members.Count; i++) idsB.Add(b.Members[i].Id);
            CollectionAssert.AreEqual(idsA, idsB, what + ": danh sách thành viên");
        }

        [Test]
        public void LuuGiuaChung_TaiLen_DiCu_LuuLai_TaiLen_VanKhop()
        {
            // Đúng chuỗi Supervisor yêu cầu: lưu → tải → di cư → lưu → tải.
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(6);

            // --- lưu giữa chừng, trước khi di cư ---
            GameState beforeState = g.State;
            GameState reloaded = RoundTrip(beforeState, g.Random, g.Config);
            AssertSameTribe(beforeState, reloaded, "trước di cư");
            Assert.AreEqual(0, reloaded.Region);

            // --- di cư trên ván gốc ---
            g.Combat.ThenWin();
            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(2));
            g.MeetGatherFloor();
            g.TakeEveryoneIfMigrating();
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.IsTrue(outcome.Migrated, "lượt này phải di cư");
            Assert.AreEqual(1, g.State.Region);

            // --- lưu sau di cư, tải lại ---
            GameState afterReload = RoundTrip(g.State, g.Random, g.Config);
            AssertSameTribe(g.State, afterReload, "sau di cư");

            // Số chỗ phải là của VÙNG MỚI, không phải của vùng cũ còn sót lại.
            Assert.AreEqual(7, afterReload.RegionCapacityOpen, "vùng 1 mở sẵn 7 chỗ");
            Assert.AreEqual(14, afterReload.RegionCapacityMax, "trần vùng 1");
        }

        [Test]
        public void NguoiBiBoLai_MatHAN_OCaHaiPhia()
        {
            // Bộ tộc 5 người, chỉ mang 2 đi. Ba người kia phải biến mất khỏi ván gốc
            // VÀ khỏi file lưu — không được "sống lại" sau một vòng lưu–tải.
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(6);
            int[] all = g.AllMemberIds();
            Assert.AreEqual(5, all.Length);

            g.Combat.ThenWin();
            g.Plan.Assign(ActivityKind.Explore, new[] { all[0], all[1] });
            g.MeetGatherFloor();
            Assert.IsTrue(g.Plan.ChooseMigrationParty(all[0], all[1]).Succeeded);

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.IsTrue(outcome.Migrated);
            Assert.AreEqual(3, outcome.LeftBehindMemberIds.Count, "ba người ở lại");
            CollectionAssert.AreEquivalent(
                new[] { all[2], all[3], all[4] }, outcome.LeftBehindMemberIds);

            // Người bị bỏ lại KHÔNG phải người chết.
            CollectionAssert.IsEmpty(outcome.DeadMemberIds, "bỏ lại không phải chết");
            Assert.AreEqual(0, outcome.MoraleFromDeaths, "không trừ tinh thần theo on_member_death");

            // Phía ván gốc. Người sinh ra TRONG lượt đó cũng được đi theo (lúc người chơi chọn
            // đoàn thì nó chưa tồn tại), nên sĩ số là 2 cộng số trẻ vừa sinh.
            Assert.AreEqual(2 + outcome.BornMemberIds.Count, g.State.Crew);
            foreach (int gone in new[] { all[2], all[3], all[4] })
            {
                Assert.IsNull(g.State.FindMember(gone), "người ở lại phải biến mất khỏi GameState");
            }

            // Phía file lưu.
            GameState after = RoundTrip(g.State, g.Random, g.Config);
            Assert.AreEqual(g.State.Crew, after.Crew, "tải lên không được làm ai sống lại");
            foreach (int gone in new[] { all[2], all[3], all[4] })
            {
                Assert.IsNull(after.FindMember(gone), "người ở lại không được quay về sau khi tải");
            }

            AssertSameTribe(g.State, after, "sau khi bỏ người lại");
        }

        [Test]
        public void CongTrinhCuaVungCu_KhongTheoSangVungMoi_KeCaQuaLuuTai()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(3);

            // Dọn một công trình ở vùng 0 trước đã.
            g.AssignBuild("nhanh_cay_gay", g.MemberIds(2));
            g.MeetGatherFloor();
            g.Turn.EndTurn();

            Assert.AreEqual(1, g.State.ClearedSiteIds.Count);
            Assert.AreEqual(1, g.State.BuildLevel);

            // Rồi di cư.
            g.AtTurn(6);
            g.Combat.ThenWin();
            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(2));
            g.MeetGatherFloor();
            g.TakeEveryoneIfMigrating();
            g.Turn.EndTurn();

            Assert.AreEqual(1, g.State.Region);
            Assert.AreEqual(0, g.State.ClearedSiteIds.Count, "công trình thuộc về vùng cũ");
            Assert.AreEqual(0, g.State.BuildLevel, "cấp công trình tụt về 0 — chủ ý của Khoa");

            GameState after = RoundTrip(g.State, g.Random, g.Config);
            Assert.AreEqual(0, after.ClearedSiteIds.Count, "tải lên không được làm công trình cũ sống lại");
            Assert.AreEqual(0, after.BuildLevel);
            AssertSameTribe(g.State, after, "sau di cư có công trình cũ");
        }

        [Test]
        public void TaiVanDaDiCu_KhongBiGhiDeSoChoCuaVung()
        {
            // Bẫy cụ thể: constructor của TurnSystem chép số chỗ của vùng từ sites.json khi
            // RegionCapacityMax == 0. Nếu nó chép cả với ván ĐÃ TẢI thì ván ở vùng 1 sẽ bị
            // kéo về số của vùng hiện tại và xoá sạch công trình đã dọn.
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(6);
            g.Combat.ThenWin();
            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(2));
            g.MeetGatherFloor();
            g.TakeEveryoneIfMigrating();
            g.Turn.EndTurn();

            // Dọn một công trình Ở VÙNG MỚI.
            g.AtTurn(9);
            g.AssignBuild("to_moi_bo", g.MemberIds(3));
            g.MeetGatherFloor();
            g.Turn.EndTurn();

            Assert.AreEqual(1, g.State.ClearedSiteIds.Count);
            int capBefore = g.State.LairCapacity(g.Config);

            // Dựng lại ván từ file lưu, đúng đường C7 phải đi.
            LoadedGame loaded = SaveGame.Read(
                SaveGame.Write(g.State, g.Random, GameMode.Mvp), g.Config);

            MemberFactory factory = new MemberFactory(MemberStatRanges.AverageFromConfig(g.Config));
            TurnSystem turn = new TurnSystem(
                g.Config, loaded.State, loaded.Random, factory,
                new Climate(g.Config), new Difficulty(g.Config),
                NoPurchasedNodes.Instance, new ScriptedCombatResolver(),
                events: null, odds: null,
                plants: TestOverrides.SinglePlant, sites: TestOverrides.StandardSites);

            Assert.AreEqual(1, loaded.State.Region);
            Assert.AreEqual(1, loaded.State.ClearedSiteIds.Count, "công trình của vùng mới phải còn");
            Assert.AreEqual(7, loaded.State.RegionCapacityOpen);
            Assert.AreEqual(14, loaded.State.RegionCapacityMax);
            Assert.AreEqual(capBefore, loaded.State.LairCapacity(g.Config), "sức chứa không đổi qua lưu–tải");

            // Và hệ slot của ván tải lại phải nhìn thấy đúng bộ công trình của vùng 1.
            foreach (SiteView site in turn.Sites.VisibleSites(loaded.State.Stage, NoPurchasedNodes.Instance))
            {
                Assert.AreNotEqual("nhanh_cay_gay", site.Id, "công trình vùng 0 không được hiện ở vùng 1");
                Assert.AreNotEqual("to_moi_bo", site.Id, "công trình đã dọn không hiện lại");
            }
        }

        [Test]
        public void ChonDoanQuaSoCho_ThiTuChoi()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(6);
            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(2));

            Assert.AreEqual(7, g.Plan.MigrationCapacity, "vùng 1 mở sẵn 7 chỗ");

            int[] quaNhieu = new int[8];
            for (int i = 0; i < 8; i++)
            {
                quaNhieu[i] = i + 1;
            }

            Assert.AreEqual(PlanError.MigrationPartyTooLarge, g.Plan.ChooseMigrationParty(quaNhieu).Error);
        }

        [Test]
        public void ChuaChonDoan_ThiKhongKetThucLuotDuoc()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(6);
            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(2));
            g.MeetGatherFloor();

            Assert.AreEqual(PlanError.MigrationPartyNotChosen, g.Plan.ValidateForEndTurn().Error);
            Assert.Throws<InvalidOperationException>(() => g.Turn.EndTurn());
        }

        [Test]
        public void XemTruocDiCu_KhopVoiThuThatSuXayRa()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(6);
            int[] all = g.AllMemberIds();

            g.Combat.ThenWin();
            g.Plan.Assign(ActivityKind.Explore, new[] { all[0], all[1] });
            g.MeetGatherFloor();
            g.Plan.ChooseMigrationParty(all[0], all[1]);

            MigrationPreview preview = g.Turn.PreviewMigration();
            Assert.IsTrue(preview.HasNextRegion);
            Assert.AreEqual(1, preview.NextRegion);
            Assert.AreEqual(7, preview.CapacityOpen);
            Assert.AreEqual(5, preview.Crew);
            Assert.AreEqual(2, preview.Chosen);
            Assert.AreEqual(3, preview.LeftBehind);

            // Gọi nhiều lần không đổi gì — truy vấn thuần.
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(3, g.Turn.PreviewMigration().LeftBehind);
            }

            Assert.AreEqual(5, g.State.Crew, "xem trước không được chạm vào ván");

            TurnOutcome outcome = g.Turn.EndTurn();
            Assert.AreEqual(preview.LeftBehind, outcome.LeftBehindMemberIds.Count,
                "con số hiện trước khi bấm phải đúng bằng con số xảy ra");
        }

        [Test]
        public void HetVung_ThiThangKhamPhaChiLaMotTranThang()
        {
            TestGame g = TestGame.New(1, GameMode.Mvp).AtTurn(6);
            g.State.Region = g.Config.RegionMaxIndex;   // đã ở vùng cuối của chế độ MVP

            Assert.IsFalse(g.Turn.PreviewMigration().HasNextRegion);
            Assert.AreEqual(0, g.Plan.MigrationCapacity);

            g.Combat.ThenWin();
            g.Plan.Assign(ActivityKind.Explore, g.MemberIds(2));
            g.MeetGatherFloor();

            // Không còn vùng nào thì cũng không phải chọn đoàn — luật không được chặn vô cớ.
            Assert.IsTrue(g.Plan.ValidateForEndTurn().Succeeded);

            TurnOutcome outcome = g.Turn.EndTurn();
            Assert.IsFalse(outcome.Migrated);
            Assert.AreEqual(0, outcome.LeftBehindMemberIds.Count);
            Assert.AreEqual(5, g.State.Crew, "không ai bị bỏ lại khi không đi đâu cả");
        }
    }
}
