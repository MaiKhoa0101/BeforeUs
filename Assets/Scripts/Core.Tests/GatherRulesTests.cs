using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Plants;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// Hái lượm v7 (docs/design-v7-draft.md §A). Thay hoàn toàn bộ test của công thức
    /// `gather_yield` ở design.md §5.1, vốn đã bị xoá.
    /// </summary>
    [TestFixture]
    public class GatherRulesTests
    {
        private BalanceConfig _config;
        private PlantCatalog _plants;

        [SetUp]
        public void SetUp()
        {
            _config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
            _plants = TestOverrides.StandardPlants;
        }

        private static GameState StateAt(int region, params string[] discovered)
        {
            GameState state = new GameState { Turn = 1, Stage = 1, Region = region };
            state.DiscoveredPlantIds.AddRange(discovered);
            return state;
        }

        /// <summary>Trạng thái đã biết HẾT loài của vùng 0, để khám phá không làm nhiễu phép đo.</summary>
        private GameState AllKnownInRegion0()
        {
            GameState state = new GameState { Turn = 1, Stage = 1, Region = 0 };
            foreach (PlantData p in _plants.All)
            {
                if (p.GrowsIn(0))
                {
                    state.DiscoveredPlantIds.Add(p.Id);
                }
            }

            return state;
        }

        private static Member WithSpeed(double speed, int id = 1)
        {
            return new Member(id, speed, 1.0, 5, null);
        }

        // ---------- dữ liệu ----------

        [Test]
        public void NapDuocPlantsJson_VaCoLoaiBietSanTuDau()
        {
            Assert.Greater(_plants.All.Count, 0);
            CollectionAssert.Contains(_plants.KnownFromStartIds(), "la_non");
        }

        [Test]
        public void DuLieuHong_ThiNapHong()
        {
            Assert.Throws<PlantDataException>(() => PlantCatalog.Load(string.Empty));
            Assert.Throws<PlantDataException>(() => PlantCatalog.Load("{ khong phai json"));

            // Không loài nào biết sẵn => lượt 1 người chơi không biết hái gì, và khung đảm bảo
            // không có loài nào để bảo đảm. Phải nổ chứ không được để ván chết câm.
            string noStarter = TestPaths.ReadPlantsJson().Replace("\"discover_weight\": 0", "\"discover_weight\": 9");
            PlantDataException error = Assert.Throws<PlantDataException>(() => PlantCatalog.Load(noStarter));
            StringAssert.Contains("discover_weight = 0", error.Message);
        }

        [Test]
        public void LoaiChiThuocVungCuaNo()
        {
            PlantData suDung = _plants.Get("qua_sung");
            Assert.IsTrue(suDung.GrowsIn(0));
            Assert.IsFalse(suDung.GrowsIn(1), "quả sung chỉ có ở rừng nhiệt đới");

            List<PlantData> savan = _plants.KnownInRegion(new[] { "qua_sung", "hat_co" }, 1);
            Assert.AreEqual(1, savan.Count);
            Assert.AreEqual("hat_co", savan[0].Id);
        }

        // ---------- tỉ lệ tìm thấy, §A.3 ----------

        [Test]
        public void TiLeTimThay_CoBanLaFindChance()
        {
            // Đọc số từ chính plants.json. Viết cứng 0.80 ở đây nghĩa là mỗi vòng chỉnh cân bằng
            // sẽ làm đỏ một test về CÔNG THỨC — và công thức thì không đổi khi số đổi.
            PlantData la = _plants.Get("la_non");
            double p = GatherRules.FindChance(la, WithSpeed(1.0), _config, ClimatePhase.Wet, 1.0);
            Assert.AreEqual(la.FindChance, p, 1e-12);
        }

        [Test]
        public void PhaKho_NhanVoiDryMultiplierCuaTungLoai()
        {
            // §A.8: mùa khô tác động lên TỈ LỆ, không trừ thẳng một con số đều tay.
            Member m = WithSpeed(1.0);
            PlantData la = _plants.Get("la_non");
            PlantData ong = _plants.Get("to_ong");

            Assert.AreEqual(
                la.FindChance * la.DryMultiplier,
                GatherRules.FindChance(la, m, _config, ClimatePhase.Dry, 1.0), 1e-12);
            Assert.AreEqual(
                ong.FindChance * ong.DryMultiplier,
                GatherRules.FindChance(ong, m, _config, ClimatePhase.Dry, 1.0), 1e-12);
        }

        [Test]
        public void MuaKhoDoiLoaiNaoDangDiTim_ChuKhongPhatDeuTay()
        {
            // Ý định thiết kế [ĐỀ XUẤT] ở §A.8: tổ ong không giảm chút nào nên mùa khô đẩy
            // người chơi sang tìm nó.
            Member m = WithSpeed(1.0);
            double laWet = GatherRules.FindChance(_plants.Get("la_non"), m, _config, ClimatePhase.Wet, 1.0);
            double laDry = GatherRules.FindChance(_plants.Get("la_non"), m, _config, ClimatePhase.Dry, 1.0);
            double ongWet = GatherRules.FindChance(_plants.Get("to_ong"), m, _config, ClimatePhase.Wet, 1.0);
            double ongDry = GatherRules.FindChance(_plants.Get("to_ong"), m, _config, ClimatePhase.Dry, 1.0);

            Assert.Less(laDry, laWet);
            Assert.AreEqual(ongWet, ongDry, 1e-12);
        }

        [Test]
        public void Speed_TangCoHoiTimLoaiHiem_NhieuHonHanLoaiPhoBien()
        {
            // D4 của Khoa: "speed là yếu tố tăng khả năng tìm thấy hoa quả hiếm".
            Member nhanh = WithSpeed(1.2);
            Member trungBinh = WithSpeed(1.0);

            double hiemTang = GatherRules.FindChance(_plants.Get("to_ong"), nhanh, _config, ClimatePhase.Wet, 1.0)
                              - GatherRules.FindChance(_plants.Get("to_ong"), trungBinh, _config, ClimatePhase.Wet, 1.0);
            double phoBienTang = GatherRules.FindChance(_plants.Get("la_non"), nhanh, _config, ClimatePhase.Wet, 1.0)
                                 - GatherRules.FindChance(_plants.Get("la_non"), trungBinh, _config, ClimatePhase.Wet, 1.0);

            // Luật thật: phần cộng tỉ lệ với (1 − find_chance). Viết cứng "gấp hơn 3 lần" là
            // viết cứng KHOẢNG CÁCH giữa hai con số trong plants.json, và khoảng cách đó co lại
            // mỗi khi cả bảng được nhân chung một hệ số — đúng cái đã xảy ra ở vòng chỉnh ×0,80.
            double tiLeKyVong = (1.0 - _plants.Get("to_ong").FindChance)
                                / (1.0 - _plants.Get("la_non").FindChance);

            Assert.Greater(hiemTang, 0);
            Assert.Greater(hiemTang, phoBienTang, "loài càng hiếm thì speed càng giúp nhiều");
            Assert.AreEqual(tiLeKyVong, hiemTang / phoBienTang, 1e-9,
                "phần cộng đúng bằng tỉ lệ với (1 − find_chance)");
        }

        [Test]
        public void SpeedThap_ThiTiLeGiam()
        {
            double cham = GatherRules.FindChance(_plants.Get("cu_mai"), WithSpeed(0.8), _config, ClimatePhase.Wet, 1.0);
            double trungBinh = GatherRules.FindChance(_plants.Get("cu_mai"), WithSpeed(1.0), _config, ClimatePhase.Wet, 1.0);
            Assert.Less(cham, trungBinh);
        }

        [Test]
        public void DoKho_BaoMonMoiTiLeTimThay()
        {
            // §A.7: cơ chế duy nhất kết thúc ván ở lì.
            Member m = WithSpeed(1.0);
            PlantData cu = _plants.Get("cu_mai");

            Assert.AreEqual(cu.FindChance, GatherRules.FindChance(cu, m, _config, ClimatePhase.Wet, 1.0), 1e-12);
            Assert.AreEqual(cu.FindChance * 0.5, GatherRules.FindChance(cu, m, _config, ClimatePhase.Wet, 0.5), 1e-12);
            Assert.AreEqual(0.00, GatherRules.FindChance(cu, m, _config, ClimatePhase.Wet, 0.0), 1e-12);
        }

        [Test]
        public void TiLeLuonNamTrong0Den1()
        {
            Member ratNhanh = WithSpeed(5.0);
            double p = GatherRules.FindChance(_plants.Get("to_ong"), ratNhanh, _config, ClimatePhase.Wet, 1.0);
            Assert.That(p, Is.InRange(0.0, 1.0));
        }

        // ---------- sàn bắt buộc, §A.6 ----------

        [Test]
        public void SanBatBuoc_LaMotPhanBaLamTronLen()
        {
            Assert.AreEqual(0, GatherRules.FloorForCrew(_config, 0));
            Assert.AreEqual(1, GatherRules.FloorForCrew(_config, 1));
            Assert.AreEqual(1, GatherRules.FloorForCrew(_config, 3));
            Assert.AreEqual(2, GatherRules.FloorForCrew(_config, 4));
            Assert.AreEqual(2, GatherRules.FloorForCrew(_config, 6));
            Assert.AreEqual(3, GatherRules.FloorForCrew(_config, 8));
            Assert.AreEqual(4, GatherRules.FloorForCrew(_config, 12));
        }

        [Test]
        public void SanBatBuoc_KhongBaoGioVuotQuaSiSo()
        {
            for (int crew = 0; crew <= 30; crew++)
            {
                Assert.LessOrEqual(GatherRules.FloorForCrew(_config, crew), crew, "sĩ số " + crew);
            }
        }

        // ---------- khám phá, §A.3 bước 1 ----------

        [Test]
        public void TiLeKhamPha_TangTheoSoNguoiNhungCoTran()
        {
            Assert.AreEqual(0.20, GatherRules.DiscoverChance(_config, 1), 1e-12);
            Assert.AreEqual(0.40, GatherRules.DiscoverChance(_config, 5), 1e-12);
            Assert.AreEqual(0.60, GatherRules.DiscoverChance(_config, 20), 1e-12, "có trần 0.60");
        }

        [Test]
        public void KhamPha_ChiRutLoaiCuaVungHienTai()
        {
            GameState savan = StateAt(1, "la_non");
            for (int seed = 1; seed <= 50; seed++)
            {
                PlantData found = _plants.DrawUndiscovered(savan.DiscoveredPlantIds, 1, new XorShift128Random(seed));
                Assert.IsNotNull(found);
                Assert.IsTrue(found.GrowsIn(1), found.Id + " không mọc ở savan");
            }
        }

        [Test]
        public void KhamPha_HetLoaiThiTraNull()
        {
            List<string> all = new List<string>();
            foreach (PlantData p in _plants.All)
            {
                all.Add(p.Id);
            }

            Assert.IsNull(_plants.DrawUndiscovered(all, 0, new XorShift128Random(1)));
        }

        [Test]
        public void KhamPha_LoaiDeGapHonThiRaSomHon()
        {
            // discover_weight: qua_sung 5, to_ong 1 — gặp sung thường xuyên hơn hẳn.
            Dictionary<string, int> counts = new Dictionary<string, int>();
            GameState state = StateAt(0, "la_non");

            IRandom random = new XorShift128Random(99);
            for (int i = 0; i < 4000; i++)
            {
                PlantData found = _plants.DrawUndiscovered(state.DiscoveredPlantIds, 0, random);
                counts.TryGetValue(found.Id, out int current);
                counts[found.Id] = current + 1;
            }

            Assert.Greater(counts["qua_sung"], counts["to_ong"] * 3);
        }

        // ---------- giải quyết một lần Kiếm ăn, §A.3 ----------

        /// <summary>
        /// Bộ loài một phần tử với find_chance thấp: gần như luôn rút trượt, nên khung đảm bảo
        /// là thứ duy nhất giữ cho họ không về tay không. guaranteed_total_speed = 2.0.
        /// </summary>
        private static PlantCatalog HiemCatalog()
        {
            return PlantCatalog.Load(
                "{\"plants\":[{\"id\":\"x\",\"name\":\"X\",\"description\":\"d\",\"find_chance\":0.01,"
                + "\"nutrition\":2,\"regions\":[0],\"dry_multiplier\":1.0,"
                + "\"guaranteed_total_speed\":2.0,\"discover_weight\":0}]}");
        }

        [Test]
        public void KhungDamBao_TongSpeedDuNguongThiKhongBaoGioVeTayKhong()
        {
            // E2: ngưỡng là TỔNG nhanh nhẹn của cả nhóm. Hai người × 1.0 = 2.0, vừa đủ ngưỡng.
            PlantCatalog hiem = HiemCatalog();
            GameState state = StateAt(0, "x");
            IRandom random = new XorShift128Random(7);

            for (int i = 0; i < 300; i++)
            {
                GatherOutcome outcome = GatherRules.Resolve(
                    _config, hiem, state, new[] { WithSpeed(1.0, 1), WithSpeed(1.0, 2) },
                    ClimatePhase.Wet, 1.0, random);
                Assert.GreaterOrEqual(outcome.Food, 2, "đủ tổng nhanh nhẹn thì luôn mang về ít nhất một lần tìm thấy");
            }
        }

        [Test]
        public void KhungDamBao_TongSpeedDuoiNguongThiCoTheVeTayKhong()
        {
            // Cũng hai người, nhưng tổng 1.8 < 2.0. Chính số người KHÔNG cứu được họ —
            // nếu ngưỡng tính theo từng người thì test này sẽ xanh sai.
            PlantCatalog hiem = HiemCatalog();
            GameState state = StateAt(0, "x");
            IRandom random = new XorShift128Random(7);

            bool sawEmpty = false;
            for (int i = 0; i < 300 && !sawEmpty; i++)
            {
                GatherOutcome outcome = GatherRules.Resolve(
                    _config, hiem, state, new[] { WithSpeed(0.9, 1), WithSpeed(0.9, 2) },
                    ClimatePhase.Wet, 1.0, random);
                if (outcome.Food == 0)
                {
                    sawEmpty = true;
                }
            }

            Assert.IsTrue(sawEmpty, "dưới ngưỡng tổng thì phải có lúc về tay không");
        }

        [Test]
        public void KhungDamBao_ChoCaNhomMotLan_ChuKhongPhaiMoiNguoiMotLan()
        {
            // Đây là ranh giới giữa cách đọc ĐÚNG (E2) và cách đọc SAI (bản đầu).
            // Sáu người × 1.0 = tổng 6.0, thừa ngưỡng 2.0. Nếu khung đảm bảo tính theo từng người
            // thì sàn thức ăn sẽ là 6 × 2 = 12. Theo E2 thì sàn chỉ là 2.
            PlantCatalog hiem = HiemCatalog();
            GameState state = StateAt(0, "x");
            IRandom random = new XorShift128Random(31);

            int sawFloor = 0;
            for (int i = 0; i < 400; i++)
            {
                GatherOutcome outcome = GatherRules.Resolve(
                    _config, hiem, state,
                    new[] { WithSpeed(1.0, 1), WithSpeed(1.0, 2), WithSpeed(1.0, 3),
                            WithSpeed(1.0, 4), WithSpeed(1.0, 5), WithSpeed(1.0, 6) },
                    ClimatePhase.Wet, 1.0, random);

                Assert.GreaterOrEqual(outcome.Food, 2);
                if (outcome.Food == 2)
                {
                    sawFloor++;
                }
            }

            Assert.Greater(sawFloor, 300, "phần lớn các lượt phải dừng đúng ở sàn 2, không phải 12");
        }

        [Test]
        public void KhungDamBao_KhongNoKhiDaCoNguoiTuTimThay()
        {
            // Tìm thấy chắc chắn (find_chance 1.0) thì khung đảm bảo không được cộng thêm lần nữa.
            PlantCatalog chac = PlantCatalog.Load(
                "{\"plants\":[{\"id\":\"x\",\"name\":\"X\",\"description\":\"d\",\"find_chance\":1.0,"
                + "\"nutrition\":2,\"regions\":[0],\"dry_multiplier\":1.0,"
                + "\"guaranteed_total_speed\":1.0,\"discover_weight\":0}]}");

            GameState state = StateAt(0, "x");
            GatherOutcome outcome = GatherRules.Resolve(
                _config, chac, state, new[] { WithSpeed(1.0, 1), WithSpeed(1.0, 2) },
                ClimatePhase.Wet, 1.0, new XorShift128Random(5));

            Assert.AreEqual(2, outcome.Finds.Count, "hai lần tự tìm thấy, không có lần nào từ khung đảm bảo");
            foreach (PlantFind find in outcome.Finds)
            {
                Assert.IsFalse(find.FromGuarantee);
            }
        }

        [Test]
        public void LoaiVuaKhamPha_ThamGiaLuonLuotDo()
        {
            // §A.3: "loài vừa khám phá tham gia luôn bước 2 của chính lượt này".
            PlantCatalog hai = PlantCatalog.Load(
                "{\"plants\":["
                + "{\"id\":\"a\",\"name\":\"A\",\"description\":\"d\",\"find_chance\":1.0,\"nutrition\":1,"
                + "\"regions\":[0],\"dry_multiplier\":1.0,\"guaranteed_total_speed\":1.0,\"discover_weight\":0},"
                + "{\"id\":\"b\",\"name\":\"B\",\"description\":\"d\",\"find_chance\":1.0,\"nutrition\":10,"
                + "\"regions\":[0],\"dry_multiplier\":1.0,\"guaranteed_total_speed\":1.0,\"discover_weight\":1}]}");

            bool sawBoth = false;
            for (int seed = 1; seed <= 100 && !sawBoth; seed++)
            {
                GameState state = StateAt(0, "a");
                GatherOutcome outcome = GatherRules.Resolve(
                    _config, hai, state, new[] { WithSpeed(1.0) }, ClimatePhase.Wet, 1.0, new XorShift128Random(seed));

                if (outcome.DiscoveredPlantId == "b")
                {
                    Assert.AreEqual(11, outcome.Food, "cả A lẫn B đều được tìm trong chính lượt khám phá");
                    sawBoth = true;
                }
            }

            Assert.IsTrue(sawBoth);
        }

        [Test]
        public void KhamPha_GhiVaoGameState_VaChiMotLanMoiLuot()
        {
            GameState state = StateAt(0, "la_non");
            int before = state.DiscoveredPlantIds.Count;

            GatherOutcome outcome = GatherRules.Resolve(
                _config, _plants, state, new[] { WithSpeed(1.0), WithSpeed(1.0), WithSpeed(1.0) },
                ClimatePhase.Wet, 1.0, new XorShift128Random(3));

            int added = state.DiscoveredPlantIds.Count - before;
            Assert.LessOrEqual(added, 1, "khám phá là MỘT lần cho cả nhóm, không phải mỗi người");
            Assert.AreEqual(added == 1, outcome.DiscoveredPlantId != null);
        }

        [Test]
        public void CungSeed_ChoCungKetQua()
        {
            GameState a = StateAt(0, "la_non", "cu_mai");
            GameState b = StateAt(0, "la_non", "cu_mai");
            Member[] party = { WithSpeed(1.0, 1), WithSpeed(1.1, 2) };

            GatherOutcome first = GatherRules.Resolve(_config, _plants, a, party, ClimatePhase.Wet, 1.0, new XorShift128Random(42));
            GatherOutcome second = GatherRules.Resolve(_config, _plants, b, party, ClimatePhase.Wet, 1.0, new XorShift128Random(42));

            Assert.AreEqual(first.Food, second.Food);
            Assert.AreEqual(first.DiscoveredPlantId, second.DiscoveredPlantId);
            Assert.AreEqual(first.Finds.Count, second.Finds.Count);
        }

        [Test]
        public void KhongCuAi_ThiKhongCoGi()
        {
            GameState state = StateAt(0, "la_non");
            GatherOutcome outcome = GatherRules.Resolve(
                _config, _plants, state, new Member[0], ClimatePhase.Wet, 1.0, new XorShift128Random(1));

            Assert.AreEqual(0, outcome.Food);
            Assert.AreEqual(0, outcome.Finds.Count);
            Assert.IsNull(outcome.DiscoveredPlantId);
        }

        [Test]
        public void ThucAnBangTongDinhDuongCuaNhungLanTimThay()
        {
            GameState state = StateAt(0, "la_non", "cu_mai", "to_ong");
            GatherOutcome outcome = GatherRules.Resolve(
                _config, _plants, state, new[] { WithSpeed(1.0, 1), WithSpeed(1.0, 2) },
                ClimatePhase.Wet, 1.0, new XorShift128Random(11));

            int sum = 0;
            foreach (PlantFind find in outcome.Finds)
            {
                sum += find.Nutrition;
            }

            Assert.AreEqual(sum, outcome.Food);
        }

        // ---------- dự báo, §A.3 ----------

        [Test]
        public void DuBao_KhongRutSoVaKhongDoiGi()
        {
            GameState state = StateAt(0, "la_non", "cu_mai");
            int discoveredBefore = state.DiscoveredPlantIds.Count;

            GatherForecast first = GatherRules.Forecast(
                _config, _plants, state, new[] { WithSpeed(1.0) }, ClimatePhase.Wet, 1.0);

            for (int i = 0; i < 20; i++)
            {
                GatherForecast again = GatherRules.Forecast(
                    _config, _plants, state, new[] { WithSpeed(1.0) }, ClimatePhase.Wet, 1.0);
                Assert.AreEqual(first.Expected, again.Expected, 1e-12);
                Assert.AreEqual(first.Guaranteed, again.Guaranteed);
            }

            Assert.AreEqual(discoveredBefore, state.DiscoveredPlantIds.Count);
        }

        [Test]
        public void DuBao_SanChacChanTinhTheoTongNhanhNhen()
        {
            GameState state = StateAt(0, "la_non");

            // lá non: guaranteed_total_speed = 2.0, nutrition = 3.
            GatherForecast nhanh = GatherRules.Forecast(
                _config, _plants, state, new[] { WithSpeed(1.0, 1), WithSpeed(1.1, 2) }, ClimatePhase.Wet, 1.0);
            Assert.AreEqual(3, nhanh.Guaranteed, "tổng 2.1 đạt ngưỡng → chắc chắn MỘT lần lá non, không phải mỗi người một lần");

            GatherForecast cham = GatherRules.Forecast(
                _config, _plants, state, new[] { WithSpeed(0.9, 1), WithSpeed(0.8, 2) }, ClimatePhase.Wet, 1.0);
            Assert.AreEqual(0, cham.Guaranteed, "tổng 1.7 dưới ngưỡng thì không có gì chắc chắn");
        }

        [Test]
        public void DuBao_KyVongKhopVoiTrungBinhThucTe()
        {
            GameState state = AllKnownInRegion0();
            Member[] party = { WithSpeed(1.0, 1), WithSpeed(1.0, 2) };

            double expected = GatherRules.Forecast(_config, _plants, state, party, ClimatePhase.Wet, 1.0).Expected;

            // Chạy thật nhiều lần trên một bản sao trạng thái đã biết hết loài của vùng,
            // để khám phá không làm nhiễu phép so.
            IRandom random = new XorShift128Random(2024);
            long total = 0;
            const int runs = 20000;
            for (int i = 0; i < runs; i++)
            {
                GameState copy = AllKnownInRegion0();
                total += GatherRules.Resolve(_config, _plants, copy, party, ClimatePhase.Wet, 1.0, random).Food;
            }

            double actual = (double)total / runs;
            Assert.That(actual, Is.EqualTo(expected).Within(expected * 0.05),
                "kỳ vọng " + expected + " vs trung bình thực tế " + actual);
        }
    }
}
