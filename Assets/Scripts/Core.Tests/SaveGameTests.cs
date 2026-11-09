using System;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Save;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C7 — lưu và tải ván (design.md §14).</summary>
    [TestFixture]
    public class SaveGameTests
    {
        private BalanceConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
        }

        /// <summary>
        /// C7 dùng ĐÚNG con bot của C8, không còn driver riêng.
        ///
        /// Driver cũ (TestPlayer) có chính sách khác hẳn bot thật: nó chết quanh lượt 16 và
        /// 0/10 ván di cư được, trong khi bot balanced thắng 33,2% với trung vị 51 lượt.
        /// Nghĩa là mọi thứ C7 kiểm bằng nó đều KHÔNG đại diện cho ván thật — và cụ thể,
        /// lưu–tải qua một lần di cư chưa từng được đi qua. Đã bỏ TestPlayer hẳn.
        /// </summary>
        private static BotOptions Options(bool applyNodeEffects = false)
        {
            return new BotOptions { Style = BotStyle.Balanced, ApplyNodeEffects = applyNodeEffects };
        }

        private BotGame NewGame(int seed, bool applyNodeEffects = false)
        {
            return new BotGame(_config, Options(applyNodeEffects), seed);
        }

        private BotGame Resume(LoadedGame loaded, bool applyNodeEffects = false)
        {
            return BotGame.Resume(_config, Options(applyNodeEffects), loaded.State, (XorShift128Random)loaded.Random);
        }

        // ---------- tiêu chí "xong khi" của C7 ----------

        [Test]
        public void LuuRoiTaiGiuaVan_ChoKetQuaYHetChayLien()
        {
            // HANDOFF C7: "Lưu rồi tải giữa ván cho kết quả y hệt chạy liền."
            for (int seed = 1; seed <= 10; seed++)
            {
                BotGame straight = NewGame(seed);
                straight.PlayTurns(40);

                BotGame interrupted = NewGame(seed);
                interrupted.PlayTurns(17);

                string saved = SaveGame.Write(interrupted.State, interrupted.Random, GameMode.Mvp);
                LoadedGame loaded = SaveGame.Read(saved, _config);
                BotGame resumed = Resume(loaded);
                resumed.PlayTurns(23);

                Assert.AreEqual(straight.Fingerprint(), resumed.Fingerprint(), "seed " + seed);
            }
        }

        [Test]
        public void LuuTaiNhieuLanLienTiep_VanKhongTroi()
        {
            // Lưu và tải sau MỖI lượt: nếu có chút trạng thái nào không được lưu, 30 lần sẽ lộ ra.
            BotGame straight = NewGame(99);
            straight.PlayTurns(30);

            BotGame stepwise = NewGame(99);
            for (int i = 0; i < 30; i++)
            {
                stepwise.PlayTurns(1);
                LoadedGame loaded = SaveGame.Read(
                    SaveGame.Write(stepwise.State, stepwise.Random, GameMode.Mvp), _config);
                stepwise = Resume(loaded);
            }

            Assert.AreEqual(straight.Fingerprint(), stepwise.Fingerprint());
        }

        [Test]
        public void LuuTaiVoiHieuUngNutBat_CungKhongTroi()
        {
            BotGame straight = NewGame(7, applyNodeEffects: true);
            straight.PlayTurns(40);

            BotGame interrupted = NewGame(7, applyNodeEffects: true);
            interrupted.PlayTurns(21);
            LoadedGame loaded = SaveGame.Read(
                SaveGame.Write(interrupted.State, interrupted.Random, GameMode.Mvp), _config);
            BotGame resumed = Resume(loaded, applyNodeEffects: true);
            resumed.PlayTurns(19);

            Assert.AreEqual(straight.Fingerprint(), resumed.Fingerprint());
        }

        [Test]
        public void VanDungDeKiemChung_PhaiCoNghiaChuKhongTamThuong()
        {
            // Chốt chặn chống test rỗng ruột: nếu ván chết ngay lượt 2 thì mọi test C7 ở trên
            // vẫn xanh mà chẳng kiểm được gì. Test này bắt ván phải thật sự chạm vào
            // tiến hóa, mua nút, di cư và combat.
            int reachedStage2 = 0, migrated = 0, totalNodes = 0, totalTurns = 0, clearedSites = 0;

            for (int seed = 1; seed <= 10; seed++)
            {
                BotGame player = NewGame(seed);
                player.PlayTurns(40);

                if (player.State.Stage >= 2) reachedStage2++;
                if (player.State.MigratedEver > 0) migrated++;
                totalNodes += player.State.PurchasedNodeIds.Count;
                totalTurns += player.State.Turn;
                clearedSites += player.SitesClearedEver;
            }

            Console.WriteLine("=== Driver 10 ván (BotGame balanced, mvp, applyNodeEffects=false) ===");
            Console.WriteLine("  tới giai đoạn 2 : " + reachedStage2 + "/10");
            Console.WriteLine("  di cư           : " + migrated + "/10");
            Console.WriteLine("  nút mua được TB : " + (totalNodes / 10.0).ToString("0.0"));
            Console.WriteLine("  công trình TB   : " + (clearedSites / 10.0).ToString("0.0"));
            Console.WriteLine("  số lượt TB      : " + (totalTurns / 10.0).ToString("0.0"));

            // Ngưỡng đo TÍNH CÓ NGHĨA của ván dùng để kiểm C7, không đo cân bằng. Đặt dưới mức
            // đo được một khoảng, để một vòng chỉnh cân bằng không làm đỏ test C7 — nó chỉ cần
            // chắc rằng ván có thật sự chạm vào tiến hóa, mua nút, dọn công trình, di cư, combat.
            Assert.GreaterOrEqual(reachedStage2, 6, "phần lớn ván phải tiến hóa được lên giai đoạn 2");
            Assert.GreaterOrEqual(migrated, 6, "phần lớn ván phải di cư được — nếu không, lưu–tải qua di cư là vùng chưa ai đi");
            Assert.GreaterOrEqual(totalNodes / 10, 6, "trung bình phải mua được phần lớn nút bắt buộc");
            Assert.GreaterOrEqual(clearedSites / 10, 1, "phải dọn được công trình, nếu không §B không được kiểm");   // đếm CẢ VÁN, không phải cuối ván
            Assert.GreaterOrEqual(totalTurns / 10, 20, "ván phải chạy đủ dài để có nghĩa");
        }

        // ---------- nội dung file lưu ----------

        [Test]
        public void GiuNguyenTungTruongCuaTrangThai()
        {
            BotGame player = NewGame(5);
            player.PlayTurns(20);

            GameState before = player.State;
            LoadedGame loaded = SaveGame.Read(
                SaveGame.Write(before, player.Random, GameMode.Mvp), _config);
            GameState after = loaded.State;

            Assert.AreEqual(before.Turn, after.Turn);
            Assert.AreEqual(before.Stage, after.Stage);
            Assert.AreEqual(before.Region, after.Region);
            Assert.AreEqual(before.Neuron, after.Neuron);
            Assert.AreEqual(before.Morale, after.Morale);
            Assert.AreEqual(before.BuildLevel, after.BuildLevel);
            Assert.AreEqual(before.RegionCapacityOpen, after.RegionCapacityOpen);
            Assert.AreEqual(before.RegionCapacityMax, after.RegionCapacityMax);
            Assert.AreEqual(before.FoodNestTier, after.FoodNestTier);
            CollectionAssert.AreEqual(before.ClearedSiteIds, after.ClearedSiteIds);
            Assert.AreEqual(before.CombatWins, after.CombatWins);
            Assert.AreEqual(before.Hunts, after.Hunts);
            Assert.AreEqual(before.MigratedEver, after.MigratedEver);
            Assert.AreEqual(before.NextMemberId, after.NextMemberId);
            Assert.AreEqual(before.Crew, after.Crew);
            CollectionAssert.AreEqual(before.PurchasedNodeIds, after.PurchasedNodeIds);
        }

        [Test]
        public void ChiSoThanhVienKhongMatDoChinhXac()
        {
            // speed và strength là double. Mất dù một bit cuối thì lượng hái lượm sẽ lệch
            // ở đúng những người nằm sát ranh giới làm tròn.
            BalanceConfig config = _config;
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(config));
            GameState state = GameState.NewGame(config, new XorShift128Random(3), factory);

            LoadedGame loaded = SaveGame.Read(
                SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp), config);

            for (int i = 0; i < state.Members.Count; i++)
            {
                Assert.AreEqual(state.Members[i].Id, loaded.State.Members[i].Id);
                Assert.AreEqual(state.Members[i].Speed, loaded.State.Members[i].Speed, 0.0, "speed khớp từng bit");
                Assert.AreEqual(state.Members[i].Strength, loaded.State.Members[i].Strength, 0.0);
                Assert.AreEqual(state.Members[i].Hp, loaded.State.Members[i].Hp);
            }
        }

        [Test]
        public void DacDiemNgoaiHinhDuocGiuLai()
        {
            TraitCategory[] pool =
            {
                new TraitCategory("toc", new[] { "thang", "xoan" }),
                new TraitCategory("da", new[] { "sang", "toi" })
            };

            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(_config), pool);
            GameState state = GameState.NewGame(_config, new XorShift128Random(3), factory);

            LoadedGame loaded = SaveGame.Read(
                SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp), _config);

            for (int i = 0; i < state.Members.Count; i++)
            {
                CollectionAssert.AreEqual(state.Members[i].Traits, loaded.State.Members[i].Traits);
            }
        }

        [Test]
        public void TrangThaiNgauNhienTiepDungChoCu()
        {
            XorShift128Random random = new XorShift128Random(42);
            for (int i = 0; i < 37; i++)
            {
                random.NextDouble();
            }

            GameState state = NewEmptyState();
            LoadedGame loaded = SaveGame.Read(SaveGame.Write(state, random, GameMode.Mvp), _config);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(random.NextDouble(), loaded.Random.NextDouble(), 0.0, "lần rút thứ " + i);
            }
        }

        [Test]
        public void FileLuuLaJsonDocDuocBangMat()
        {
            BotGame player = NewGame(5);
            player.PlayTurns(8);

            string json = SaveGame.Write(player.State, player.Random, GameMode.Mvp);

            StringAssert.Contains("\"version\": 3", json);
            StringAssert.Contains("\"mode\": \"mvp\"", json);
            StringAssert.Contains("\"random_state\": \"xs128:", json);
            StringAssert.Contains("\"members\"", json);
            StringAssert.Contains("\"purchased_nodes\"", json);
            StringAssert.Contains("\"discovered_plants\"", json);
        }

        // ---------- từ chối ván hỏng ----------

        [Test]
        public void ChePhongSaiCheDo_ThiTuChoi()
        {
            GameState state = NewEmptyState();
            string json = SaveGame.Write(state, new XorShift128Random(1), GameMode.Full);

            SaveGameException error = Assert.Throws<SaveGameException>(() => SaveGame.Read(json, _config));
            StringAssert.Contains("chế độ", error.Message);
        }

        [Test]
        public void PhienBanLa_ThiTuChoi()
        {
            GameState state = NewEmptyState();
            string json = SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp)
                .Replace("\"version\": 3", "\"version\": 99");

            Assert.Throws<SaveGameException>(() => SaveGame.Read(json, _config));
        }

        [Test]
        public void ThieuTruong_ThiTuChoi()
        {
            GameState state = NewEmptyState();
            string json = SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp);
            string broken = json.Replace("\"morale\":", "\"tinh_than\":");

            Assert.Throws<SaveGameException>(() => SaveGame.Read(broken, _config));
        }

        [Test]
        public void JsonHong_ThiTuChoi()
        {
            Assert.Throws<SaveGameException>(() => SaveGame.Read(null, _config));
            Assert.Throws<SaveGameException>(() => SaveGame.Read(string.Empty, _config));
            Assert.Throws<SaveGameException>(() => SaveGame.Read("{ khong phai json", _config));
        }

        [Test]
        public void TrangThaiNgauNhienHong_ThiTuChoi()
        {
            GameState state = NewEmptyState();
            string json = SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp)
                .Replace("xs128:", "mt19937:");

            Assert.Throws<SaveGameException>(() => SaveGame.Read(json, _config));
        }

        [Test]
        public void IdThanhVienTrung_ThiTuChoi()
        {
            GameState state = NewEmptyState();
            state.Members.Add(new Member(1, 1, 1, 5, null));
            state.Members.Add(new Member(1, 1, 1, 5, null));
            state.NextMemberId = 2;

            string json = SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp);
            SaveGameException error = Assert.Throws<SaveGameException>(() => SaveGame.Read(json, _config));
            StringAssert.Contains("cùng id", error.Message);
        }

        [Test]
        public void NextMemberIdNhoHonIdDangSong_ThiTuChoi()
        {
            // Nếu lọt qua, người sinh tiếp theo sẽ trùng id với người đang sống, và combat
            // trả về deadMemberIds sẽ giết nhầm người.
            GameState state = NewEmptyState();
            state.Members.Add(new Member(7, 1, 1, 5, null));
            state.NextMemberId = 3;

            string json = SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp);
            SaveGameException error = Assert.Throws<SaveGameException>(() => SaveGame.Read(json, _config));
            StringAssert.Contains("next_member_id", error.Message);
        }

        [Test]
        public void LuotHoacGiaiDoanKhongHopLe_ThiTuChoi()
        {
            GameState state = NewEmptyState();
            string json = SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp);

            Assert.Throws<SaveGameException>(() => SaveGame.Read(json.Replace("\"turn\": 1", "\"turn\": 0"), _config));
            Assert.Throws<SaveGameException>(() => SaveGame.Read(json.Replace("\"stage\": 1", "\"stage\": 0"), _config));
        }

        [Test]
        public void ThamSoThieu_ThiBaoLoi()
        {
            GameState state = NewEmptyState();
            Assert.Throws<ArgumentNullException>(() => SaveGame.Write(null, new XorShift128Random(1), GameMode.Mvp));
            Assert.Throws<ArgumentNullException>(() => SaveGame.Write(state, null, GameMode.Mvp));
            Assert.Throws<ArgumentNullException>(
                () => SaveGame.Read(SaveGame.Write(state, new XorShift128Random(1), GameMode.Mvp), null));
        }

        private static GameState NewEmptyState()
        {
            return new GameState { Turn = 1, Stage = 1, NextMemberId = 1 };
        }
    }
}
