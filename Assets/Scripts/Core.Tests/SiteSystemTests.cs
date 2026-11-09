using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;
using BeforeUs.Core.Sites;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>design-v7 §B — slot công trình, và §F.2 — slot thuộc về vùng.</summary>
    [TestFixture]
    public sealed class SiteCatalogTests
    {
        private static SiteCatalog Load(string json)
        {
            return SiteCatalog.Load(json);
        }

        // Trần 4 chỗ, mở sẵn 3, nên đúng MỘT công trình — khớp luật "mỗi công trình mở một chỗ".
        private const string MinimalJson =
            "{\"regions\":[{\"region\":0,\"capacity_max\":4,\"capacity_open_at_start\":3,\"sites\":["
            + "{\"id\":\"a\",\"kind\":\"wood\",\"stage\":1,\"strength_required\":2.0,"
            + "\"requires_node\":null,\"art_key\":\"k\"}]}],"
            + "\"food_nest\":{\"id\":\"nest\",\"art_key\":\"n\",\"tiers\":["
            + "{\"tier\":1,\"strength_required\":2.0,\"speed_threshold_relief\":0.2}]}}";

        [Test]
        public void NapDuocFileThat()
        {
            SiteCatalog catalog = TestOverrides.StandardSites;

            Assert.AreEqual(8, catalog.CapacityMax(0), "Khoa: vùng 1 có tối đa 8 chỗ");
            Assert.AreEqual(4, catalog.CapacityOpenAtStart(0), "Khoa: khởi đầu là 4 chỗ");
            Assert.AreEqual(14, catalog.CapacityMax(1), "Khoa: vùng 2 có 14 chỗ");
            Assert.AreEqual(7, catalog.CapacityOpenAtStart(1), "[ĐỀ XUẤT]");

            // Mỗi công trình mở đúng một chỗ, nên số công trình = trần − mở sẵn.
            Assert.AreEqual(8 - 4, catalog.InRegion(0).Count);
            Assert.AreEqual(14 - 7, catalog.InRegion(1).Count);
            Assert.Greater(catalog.InRegion(0).Count, 0);
            Assert.Greater(catalog.InRegion(1).Count, 0);
        }

        [Test]
        public void SlotThuocVeVung_KhongDungChung()
        {
            SiteCatalog catalog = TestOverrides.StandardSites;

            HashSet<string> region0 = new HashSet<string>();
            foreach (SiteData site in catalog.InRegion(0))
            {
                region0.Add(site.Id);
            }

            foreach (SiteData site in catalog.InRegion(1))
            {
                Assert.IsFalse(region0.Contains(site.Id), site.Id + " nằm ở cả hai vùng");
            }
        }

        [Test]
        public void VungKhongCoTrongFile_ThiRong_ChuKhongNem()
        {
            SiteCatalog catalog = TestOverrides.StandardSites;

            Assert.AreEqual(0, catalog.InRegion(99).Count);
            Assert.AreEqual(0, catalog.CapacityMax(99));
            Assert.IsFalse(catalog.HasRegion(99));
        }

        [Test]
        public void ThangBacO_TangDan()
        {
            SiteCatalog catalog = TestOverrides.StandardSites;

            Assert.AreEqual(3, catalog.FoodNestMaxTier);
            Assert.AreEqual(0.0, catalog.SpeedThresholdRelief(0), 1e-12, "ổ đất trống không có hiệu ứng số");

            double previous = 0;
            for (int tier = 1; tier <= catalog.FoodNestMaxTier; tier++)
            {
                double relief = catalog.SpeedThresholdRelief(tier);
                Assert.Greater(relief, previous, "bậc " + tier + " phải nới ngưỡng nhiều hơn bậc trước");
                previous = relief;
            }
        }

        [Test]
        public void BacNgoaiThang_TraVeBacCaoNhatKhongPhaiNem()
        {
            SiteCatalog catalog = TestOverrides.StandardSites;
            Assert.IsNull(catalog.FoodNestTier(0));
            Assert.IsNull(catalog.FoodNestTier(catalog.FoodNestMaxTier + 1));
        }

        [Test]
        public void ThuaKhoa_ThiNem()
        {
            // Cùng kỷ luật với balance.json và plants.json: khóa lạ nghĩa là file và mã đã lệch nhau.
            string json = MinimalJson.Replace("\"art_key\":\"k\"", "\"art_key\":\"k\",\"gia_tri_la\":1");
            Assert.Throws<SiteDataException>(() => Load(json));
        }

        [Test]
        public void ThieuKhoa_ThiNem()
        {
            string json = MinimalJson.Replace(",\"art_key\":\"k\"", "");
            Assert.Throws<SiteDataException>(() => Load(json));
        }

        [Test]
        public void KindKhongHieuDuoc_ThiNem()
        {
            string json = MinimalJson.Replace("\"kind\":\"wood\"", "\"kind\":\"kim_loai\"");
            SiteDataException ex = Assert.Throws<SiteDataException>(() => Load(json));
            StringAssert.Contains("kim_loai", ex.Message);
        }

        [Test]
        public void TrungIdSlot_ThiNem()
        {
            // Hai công trình trùng id, và cũng vừa đủ 2 = trần 5 − mở sẵn 3, để lỗi bắt được
            // là lỗi TRÙNG ID chứ không phải lỗi đếm.
            string json = MinimalJson.Replace("\"capacity_max\":4", "\"capacity_max\":5").Replace(
                "\"sites\":[",
                "\"sites\":[{\"id\":\"a\",\"kind\":\"wood\",\"stage\":1,\"strength_required\":1.0,"
                + "\"requires_node\":null,\"art_key\":\"k\"},");
            SiteDataException ex = Assert.Throws<SiteDataException>(() => Load(json));
            StringAssert.Contains("Trùng id", ex.Message);
        }

        [Test]
        public void SoCongTrinhKhongKhopSoChoPhaiMo_ThiNem()
        {
            // Trần 4, mở sẵn 3 → còn đúng 1 chỗ phải mở. Hai công trình nghĩa là một cái bấm xong
            // không mở được gì: một nút không làm gì cả.
            string json = MinimalJson.Replace(
                "\"sites\":[",
                "\"sites\":[{\"id\":\"b\",\"kind\":\"wood\",\"stage\":1,"
                + "\"strength_required\":1.0,\"requires_node\":null,\"art_key\":\"k\"},");
            SiteDataException ex = Assert.Throws<SiteDataException>(() => Load(json));
            StringAssert.Contains("chỗ phải mở", ex.Message);
        }

        [Test]
        public void VungKhongMoSanChoNao_ThiNem()
        {
            // 0 chỗ mở sẵn nghĩa là tới nơi không ai ở được — không di cư sang nổi.
            string json = MinimalJson.Replace("\"capacity_open_at_start\":3", "\"capacity_open_at_start\":0");
            Assert.Throws<SiteDataException>(() => Load(json));
        }

        [Test]
        public void VungKhongCoSlotNao_ThiNem()
        {
            // Xây dựng sẽ thành nút chết suốt cả vùng. Nổ lúc nạp còn hơn để người chơi phát hiện.
            string json = MinimalJson.Replace("\"capacity_max\":4", "\"capacity_max\":3").Replace(
                "\"sites\":[{\"id\":\"a\",\"kind\":\"wood\",\"stage\":1,\"strength_required\":2.0,"
                + "\"requires_node\":null,\"art_key\":\"k\"}]",
                "\"sites\":[]");
            Assert.Throws<SiteDataException>(() => Load(json));
        }

        [Test]
        public void BacOKhongLienTiep_ThiNem()
        {
            string json = MinimalJson.Replace("\"tier\":1", "\"tier\":2");
            Assert.Throws<SiteDataException>(() => Load(json));
        }

        [Test]
        public void BacOSauNoiNguongItHon_ThiNem()
        {
            string json = MinimalJson.Replace(
                "{\"tier\":1,\"strength_required\":2.0,\"speed_threshold_relief\":0.2}",
                "{\"tier\":1,\"strength_required\":2.0,\"speed_threshold_relief\":0.5},"
                + "{\"tier\":2,\"strength_required\":4.0,\"speed_threshold_relief\":0.3}");
            SiteDataException ex = Assert.Throws<SiteDataException>(() => Load(json));
            StringAssert.Contains("tăng dần", ex.Message);
        }
    }

    [TestFixture]
    public sealed class SiteSystemTests
    {
        private static TestGame NewGame()
        {
            return TestGame.New(1, GameMode.Mvp).AtTurn(3);
        }

        [Test]
        public void SlotChuaToiGiaiDoan_ThiChuaHienRa()
        {
            TestGame g = NewGame();

            foreach (SiteView site in g.Turn.Sites.VisibleSites(1, g.Nodes))
            {
                Assert.LessOrEqual(site.Stage, 1);
            }

            // Công trình giai đoạn 2 duy nhất của vùng 0 là tảng đá lớn, và nó đòi s2_stone_tools
            // (§B.3). Không mua nút thì lên giai đoạn 2 cũng chẳng thấy gì thêm — đúng luật.
            bool sawStage2 = false;
            foreach (SiteView site in g.Turn.Sites.VisibleSites(2, g.Nodes))
            {
                sawStage2 |= site.Stage == 2;
            }

            Assert.IsFalse(sawStage2, "chưa đập đá được thì tảng đá vẫn chưa hiện ra");

            TestGame coDa = TestGame.New(1, GameMode.Mvp, "s2_stone_tools").AtTurn(3);
            bool sawWithNode = false;
            foreach (SiteView site in coDa.Turn.Sites.VisibleSites(2, coDa.Nodes))
            {
                sawWithNode |= site.Stage == 2;
            }

            Assert.IsTrue(sawWithNode, "mua nút rồi thì giai đoạn 2 có thêm công trình");
        }

        [Test]
        public void SucManhLaTongCuaNhom_KhongPhaiCuaTungNguoi()
        {
            TestGame g = NewGame();

            // Chế độ trung bình: mỗi người strength = 1.0.
            Assert.AreEqual(3.0, g.Turn.Sites.StrengthOf(g.MemberIds(3)), 1e-12);

            // hoc_da_tru_mua cần 3.0: hai người không nổi, ba người vừa đủ.
            Assert.IsFalse(g.Turn.Sites.WouldClear("hoc_da_tru_mua", g.MemberIds(2)));
            Assert.IsTrue(g.Turn.Sites.WouldClear("hoc_da_tru_mua", g.MemberIds(3)));
        }

        [Test]
        public void NguoiDaChet_KhongTinhSucManh()
        {
            TestGame g = NewGame();
            int[] ids = g.MemberIds(3);

            g.State.Members.RemoveAt(0);

            Assert.AreEqual(2.0, g.Turn.Sites.StrengthOf(ids), 1e-12, "người không còn trong bộ tộc thì không góp sức");
        }

        [Test]
        public void WouldClear_LaTruyVanThuan_KhongDoiGi()
        {
            TestGame g = NewGame();

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(g.Turn.Sites.WouldClear("nhanh_cay_gay", g.MemberIds(2)));
            }

            Assert.AreEqual(0, g.State.ClearedSiteIds.Count);
            Assert.AreEqual(0, g.State.BuildLevel);
        }

        [Test]
        public void SlotKhongCoThat_ThiTraNull_VaKhongDonDuoc()
        {
            TestGame g = NewGame();

            Assert.IsNull(g.Turn.Sites.Find("khong_co_slot_nay"));
            Assert.IsFalse(g.Turn.Sites.WouldClear("khong_co_slot_nay", g.MemberIds(5)));
            Assert.AreSame(SiteResolution.None, g.Turn.Sites.Resolve("khong_co_slot_nay", g.MemberIds(5)));
        }

        [Test]
        public void ODungThucAn_HetBacThiKhongHienRaNua()
        {
            TestGame g = NewGame();
            SiteCatalog catalog = g.Turn.Sites.Catalog;

            g.State.FoodNestTier = catalog.FoodNestMaxTier;

            Assert.IsTrue(g.Turn.Sites.IsCleared(catalog.FoodNestId));
            foreach (SiteView site in g.Turn.Sites.VisibleSites(g.State.Stage, g.Nodes))
            {
                Assert.AreNotEqual(catalog.FoodNestId, site.Id);
            }
        }

        [Test]
        public void ODungThucAn_NguongLaCuaBacTIEPTHEO()
        {
            TestGame g = NewGame();
            SiteCatalog catalog = g.Turn.Sites.Catalog;

            SiteView tier0 = g.Turn.Sites.Find(catalog.FoodNestId);
            Assert.AreEqual(0, tier0.Tier);
            Assert.AreEqual(catalog.FoodNestTier(1).StrengthRequired, tier0.StrengthRequired, 1e-12);

            g.State.FoodNestTier = 1;
            SiteView tier1 = g.Turn.Sites.Find(catalog.FoodNestId);
            Assert.AreEqual(1, tier1.Tier);
            Assert.AreEqual(catalog.FoodNestTier(2).StrengthRequired, tier1.StrengthRequired, 1e-12);
        }

        [Test]
        public void BuildLevel_LaTongBaNguon()
        {
            TestGame g = NewGame();

            g.State.ClearedSiteIds.Add("nhanh_cay_gay");
            g.State.ClearedSiteIds.Add("bai_da_nho");
            g.State.FoodNestTier = 1;

            Assert.AreEqual(3, g.State.BuildLevel, "công trình đã dọn + bậc ổ");
            Assert.AreEqual(3, g.Turn.Sites.ClearedCount);
        }

        [Test]
        public void ChonSlotRoiBoHoatDongXayDung_ThiSlotCungBoTheo()
        {
            TestGame g = NewGame();

            g.AssignBuild("nhanh_cay_gay", g.MemberIds(2));
            Assert.AreEqual("nhanh_cay_gay", g.Plan.BuildTargetSiteId);

            g.Plan.Clear(ActivityKind.Build);
            Assert.IsNull(g.Plan.BuildTargetSiteId, "bỏ người mà giữ slot thì lượt sau sẽ dọn nhầm");
        }

        [Test]
        public void WouldClearSite_KhopVoiKetQuaThucTe()
        {
            TestGame g = NewGame();

            g.AssignBuild("hoc_da_tru_mua", g.MemberIds(2));
            Assert.IsFalse(g.Plan.WouldClearSite);

            g.AssignBuild("hoc_da_tru_mua", g.MemberIds(3));
            Assert.IsTrue(g.Plan.WouldClearSite);

            g.MeetGatherFloor();
            Assert.IsTrue(g.Turn.EndTurn().Site.Cleared, "dự báo phải khớp với thứ thật sự xảy ra");
        }

        [Test]
        public void VanKhongCoSitesJson_ThiXayDungNem_ChuKhongAmThamBoQua()
        {
            BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
            XorShift128Random random = new XorShift128Random(1);
            MemberFactory factory = new MemberFactory(MemberStatRanges.AverageFromConfig(config));
            GameState state = GameState.NewGame(config, random, factory);
            state.Turn = 3;

            TurnSystem turn = new TurnSystem(
                config, state, random, factory,
                new Climate(config), new Difficulty(config),
                NoPurchasedNodes.Instance, new ScriptedCombatResolver(),
                events: null, odds: null, plants: TestOverrides.SinglePlant);

            Assert.IsNull(turn.Sites);
            Assert.AreEqual(0.0, turn.SpeedThresholdRelief, 1e-12);

            turn.Plan.Assign(ActivityKind.Build, state.Members[0].Id, state.Members[1].Id);
            turn.Plan.Assign(ActivityKind.Gather, state.Members[2].Id, state.Members[3].Id);

            // Chưa có catalog thì ngay cả việc chọn slot cũng báo lỗi, chứ không trả về "ok".
            Assert.AreEqual(PlanError.SiteNotAvailable, turn.Plan.ChooseSite("nhanh_cay_gay").Error);
            Assert.Throws<InvalidOperationException>(() => turn.EndTurn());
        }
    }
}
