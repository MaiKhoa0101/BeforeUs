using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Neuron;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C5 — neurons.json và NeuronNetwork (design.md §7).</summary>
    [TestFixture]
    public class NeuronNetworkTests
    {
        private NeuronData _data;

        [SetUp]
        public void SetUp()
        {
            _data = NeuronNetwork.LoadData(TestPaths.ReadNeuronsJson());
        }

        private NeuronNetwork Build(GameState state, GameMode mode = GameMode.Mvp, bool applyEffects = false)
        {
            BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), mode);
            return new NeuronNetwork(_data, config, state, applyEffects);
        }

        private static GameState StateAt(int stage, int neuron)
        {
            return new GameState { Turn = 1, Stage = stage, Neuron = neuron };
        }

        // ---------- dữ liệu ----------

        [Test]
        public void NapDuHaiGiaiDoan_DungCauTruc3Nhanh()
        {
            Assert.AreEqual(2, _data.Stages.Length);

            foreach (NeuronStageData stage in _data.Stages)
            {
                Assert.AreEqual(3, stage.Branches.Length, "giai đoạn " + stage.Stage);
                Assert.AreEqual(10, stage.Nodes.Length, "3 gốc + 3 cổng + 3 tùy chọn + 1 Tiến hóa");
            }
        }

        [Test]
        public void MoiNhanh_MotGocMotCongMotTuyChon_TuyChonNoiTuGoc()
        {
            // design.md §7: "1 nút gốc → 1 nút cổng (bắt buộc), cộng 1 nút tùy chọn nối từ nút gốc".
            foreach (NeuronStageData stage in _data.Stages)
            {
                foreach (NeuronBranchData branch in stage.Branches)
                {
                    string root = null;
                    int gates = 0, optionals = 0;

                    foreach (NeuronNodeData node in stage.Nodes)
                    {
                        if (node.Branch != branch.Id)
                        {
                            continue;
                        }

                        if (node.Type == "root")
                        {
                            root = node.Id;
                        }
                        else if (node.Type == "gate")
                        {
                            gates++;
                        }
                        else if (node.Type == "optional")
                        {
                            optionals++;
                        }
                    }

                    Assert.IsNotNull(root, "nhánh " + branch.Id + " giai đoạn " + stage.Stage + " thiếu nút gốc");
                    Assert.AreEqual(1, gates);
                    Assert.AreEqual(1, optionals);

                    foreach (NeuronNodeData node in stage.Nodes)
                    {
                        if (node.Branch == branch.Id && node.Type != "root")
                        {
                            Assert.AreEqual(root, node.Requires, node.Id + " phải nối từ nút gốc của nhánh");
                        }
                    }
                }
            }
        }

        [Test]
        public void TongGiaNutBatBuoc_KhopRequiredNeuronTrongBalanceJson()
        {
            // Đối chiếu CHÉO hai file dữ liệu: nếu ai đó đổi giá một nút trong neurons.json
            // mà không đổi evolution.required_neuron trong balance.json (hoặc ngược lại),
            // mô hình mua-từng-nút sẽ lệch khỏi chi phí phẳng của before_us_sim.py và test này gãy.
            foreach (GameMode mode in new[] { GameMode.Mvp, GameMode.Full })
            {
                BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), mode);

                foreach (NeuronStageData stage in _data.Stages)
                {
                    int mandatory = 0;
                    foreach (NeuronNodeData node in stage.Nodes)
                    {
                        if (node.Type == "root" || node.Type == "gate" || node.Type == "evolve")
                        {
                            mandatory += node.Cost.For(mode);
                        }
                    }

                    Assert.AreEqual(
                        config.RequiredNeuronForStage(stage.Stage),
                        mandatory,
                        "chế độ " + mode + ", giai đoạn " + stage.Stage);
                }
            }
        }

        [Test]
        public void MoiDieuKienMoKhoaTheoNut_PhaiTroToiNutCoThat()
        {
            // Sau D13 không hoạt động nào còn mở khóa bằng nút. Nhưng giữ phép kiểm này sống:
            // nếu sau này Khoa gắn lại một hoạt động vào một nút mà gõ sai id, hoạt động đó sẽ
            // vĩnh viễn không mở được MÀ KHÔNG CÓ LỖI NÀO. Test này bắt đúng ca đó.
            BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson());
            NeuronNetwork network = Build(StateAt(2, 0));

            foreach (ActivityKind activity in ActivityRules.All)
            {
                UnlockRule rule = config.UnlockOf(activity);
                if (rule.Kind != UnlockKind.Node)
                {
                    continue;
                }

                Assert.IsNotNull(network.Node(rule.NodeId),
                    activity + " mở khóa bằng nút " + rule.NodeId + " nhưng neurons.json không có nút đó");
            }
        }

        [Test]
        public void MoTaHieuUng_KhongDeLotTenBienRaManHinhNguoiChoi()
        {
            // effect_text được hiện thẳng cho người chơi ở màn mạng lưới neuron (A3).
            // Nó phải là tiếng Việt, không phải tên biến chép từ cột Hiệu ứng của design.md.
            string[] forbidden = { "_", "speed", "strength", "yield", "bonus", "applyNodeEffects" };

            foreach (NeuronStageData stage in _data.Stages)
            {
                foreach (NeuronNodeData node in stage.Nodes)
                {
                    Assert.IsNotNull(node.EffectText, node.Id);
                    Assert.IsNotEmpty(node.EffectText, node.Id);

                    string lowered = node.EffectText.ToLowerInvariant();
                    foreach (string token in forbidden)
                    {
                        Assert.IsFalse(
                            lowered.Contains(token),
                            node.Id + " để lọt \"" + token + "\" ra chữ hiện cho người chơi: " + node.EffectText);
                    }
                }
            }
        }

        [Test]
        public void MoiNutDeuCoTenTiengVietVaGiaDuong()
        {
            foreach (NeuronStageData stage in _data.Stages)
            {
                foreach (NeuronNodeData node in stage.Nodes)
                {
                    Assert.IsNotEmpty(node.Name, node.Id);
                    Assert.Greater(node.Cost.Mvp, 0, node.Id);
                    Assert.Greater(node.Cost.Full, 0, node.Id);
                }
            }
        }

        [Test]
        public void GiaTheoCheDo()
        {
            Assert.AreEqual(18, Build(StateAt(1, 0), GameMode.Mvp).Node("s1_climbing").Cost);
            Assert.AreEqual(10, Build(StateAt(1, 0), GameMode.Full).Node("s1_climbing").Cost);
            Assert.AreEqual(40, Build(StateAt(2, 0), GameMode.Mvp).Node("s2_evolve").Cost);
            Assert.AreEqual(15, Build(StateAt(2, 0), GameMode.Full).Node("s2_evolve").Cost);
        }

        [Test]
        public void DuLieuHong_ThiNapHong()
        {
            string json = TestPaths.ReadNeuronsJson();
            Assert.Throws<NeuronDataException>(() => NeuronNetwork.LoadData(string.Empty));
            Assert.Throws<NeuronDataException>(() => NeuronNetwork.LoadData("{ khong phai json"));
            Assert.Throws<NeuronDataException>(
                () => NeuronNetwork.LoadData(json.Replace("\"type\": \"gate\"", "\"type\": \"cong\"")));
            Assert.Throws<NeuronDataException>(
                () => NeuronNetwork.LoadData(json.Replace("\"kind\": \"speed_bonus\"", "\"kind\": \"bay_duoc\"")),
                "hiệu ứng lạ phải nổ, không được âm thầm bỏ qua");
        }

        // ---------- mua nút ----------

        [Test]
        public void MuaNut_TruNeuronVaGhiVaoGameState()
        {
            GameState state = StateAt(1, 20);
            NeuronNetwork network = Build(state);

            Assert.IsTrue(network.Buy("s1_climbing").Succeeded);
            Assert.AreEqual(2, state.Neuron);
            Assert.IsTrue(network.Has("s1_climbing"));
            CollectionAssert.Contains(state.PurchasedNodeIds, "s1_climbing");
        }

        [Test]
        public void ThieuNeuron_ThiKhongMuaDuoc()
        {
            GameState state = StateAt(1, 17);
            NeuronNetwork network = Build(state);

            PurchaseResult result = network.Buy("s1_climbing");
            Assert.AreEqual(PurchaseError.NotEnoughNeuron, result.Error);
            Assert.AreEqual(17, state.Neuron, "thất bại thì không trừ gì");
            Assert.IsFalse(network.Has("s1_climbing"));
        }

        [Test]
        public void ChuaMuaNutGoc_ThiKhongMuaDuocNutCong()
        {
            GameState state = StateAt(1, 500);
            NeuronNetwork network = Build(state);

            Assert.AreEqual(PurchaseError.MissingPrerequisite, network.Buy("s1_short_upright").Error);

            network.Buy("s1_climbing");
            Assert.IsTrue(network.Buy("s1_short_upright").Succeeded);
        }

        [Test]
        public void KhongMuaDuocNutCuaGiaiDoanKhac()
        {
            GameState state = StateAt(1, 500);
            NeuronNetwork network = Build(state);

            Assert.AreEqual(PurchaseError.WrongStage, network.Buy("s2_stone_tools").Error);
        }

        [Test]
        public void KhongMuaLaiNutDaMua()
        {
            GameState state = StateAt(1, 500);
            NeuronNetwork network = Build(state);

            network.Buy("s1_climbing");
            Assert.AreEqual(PurchaseError.AlreadyPurchased, network.Buy("s1_climbing").Error);
        }

        [Test]
        public void NutKhongTonTai()
        {
            Assert.AreEqual(PurchaseError.UnknownNode, Build(StateAt(1, 500)).Buy("s1_biet_bay").Error);
        }

        // ---------- tiến hóa: tiêu chí "xong khi" của C5 ----------

        [Test]
        public void KhongTienHoaDuocKhiThieuCong()
        {
            GameState state = StateAt(1, 1000);
            state.CombatWins = 5;                       // chiến tích dư thừa
            NeuronNetwork network = Build(state);

            BuyAll(network, "s1_climbing", "s1_young_leaves", "s1_grooming");
            BuyAll(network, "s1_short_upright", "s1_dig_roots");   // mới 2/3 cổng

            Assert.IsFalse(network.CanEvolve);
            Assert.AreEqual(PurchaseError.MissingGates, network.Buy("s1_evolve").Error);
            Assert.AreEqual(1, state.Stage);

            network.Buy("s1_alarm_call");                          // đủ 3 cổng
            Assert.IsTrue(network.CanEvolve);
        }

        [Test]
        public void KhongTienHoaDuocKhiThieuChienTich()
        {
            GameState state = StateAt(1, 1000);
            NeuronNetwork network = Build(state);
            BuyMandatoryGatesOfStage1(network);

            // design.md §7.1: rời giai đoạn 1 cần thắng 1 trận bất kỳ.
            Assert.AreEqual(0, state.CombatWins);
            Assert.IsFalse(network.CanEvolve);

            PurchaseResult result = network.Buy("s1_evolve");
            Assert.AreEqual(PurchaseError.MissingFeats, result.Error);
            StringAssert.Contains("thắng trận 0/1", result.Message);

            state.CombatWins = 1;
            Assert.IsTrue(network.CanEvolve);
        }

        [Test]
        public void TienHoa_LenGiaiDoanVaResetChienTichSanVaThangTran()
        {
            GameState state = StateAt(1, 1000);
            state.CombatWins = 3;
            state.Hunts = 2;
            state.MigratedEver = 1;

            NeuronNetwork network = Build(state);
            BuyMandatoryGatesOfStage1(network);

            int before = state.Neuron;
            Assert.IsTrue(network.Buy("s1_evolve").Succeeded);

            Assert.AreEqual(2, state.Stage);
            Assert.AreEqual(before - 15, state.Neuron, "trừ đúng giá nút Tiến hóa MVP");
            Assert.AreEqual(0, state.CombatWins, "chiến tích thắng trận về 0");
            Assert.AreEqual(0, state.Hunts, "chiến tích săn về 0");
            Assert.AreEqual(1, state.MigratedEver, "đã từng di cư thì GIỮ NGUYÊN (design.md §7)");
        }

        [Test]
        public void TienHoaKhoiGiaiDoan2_CanHaiLanSanThanhCong()
        {
            GameState state = StateAt(2, 1000);
            NeuronNetwork network = Build(state);
            BuyMandatoryGatesOfStage2(network);

            state.Hunts = 1;
            Assert.AreEqual(PurchaseError.MissingFeats, network.Buy("s2_evolve").Error);

            state.Hunts = 2;
            Assert.IsTrue(network.Buy("s2_evolve").Succeeded);
            Assert.AreEqual(3, state.Stage);
        }

        [Test]
        public void NeuronDuGiuSangGiaiDoanSau()
        {
            // design.md §7: "Neuron dư được giữ sang giai đoạn sau".
            GameState state = StateAt(1, 1000);
            state.CombatWins = 1;
            NeuronNetwork network = Build(state);
            BuyMandatoryGatesOfStage1(network);
            network.Buy("s1_evolve");

            Assert.AreEqual(1000 - 150, state.Neuron, "tổng nút bắt buộc giai đoạn 1 MVP là 150");
        }

        [Test]
        public void NutDaMuaOGiaiDoanTruocVanConHieuLuc()
        {
            GameState state = StateAt(1, 1000);
            state.CombatWins = 1;
            NeuronNetwork network = Build(state);
            BuyMandatoryGatesOfStage1(network);
            network.Buy("s1_evolve");

            Assert.IsTrue(network.Has("s1_dig_roots"), "lên giai đoạn 2 không làm mất nút đã mua");
        }

        // ---------- nối với mở khóa hoạt động (D1) ----------

        [Test]
        public void D13_MuaHaiNutCongKhongConDoiGiVeMoKhoa()
        {
            // D13: Xây dựng và Săn bắt mở từ lượt 3, không còn phụ thuộc nút.
            // s2_stone_tools và s2_scavenge_meat VẪN là nút cổng bắt buộc để tiến hóa,
            // nhưng chúng đã MẤT vai trò mở khóa — và hiện chưa có hiệu ứng nào thay thế.
            // Đó là món nợ D13 để lại, chờ Khoa đặt hiệu ứng mới (xem docs/decisions.md D13).
            TestGame g = TestGame.New().AtTurn(3);
            GameState state = g.State;
            state.Stage = 2;
            state.Neuron = 500;

            NeuronNetwork network = Build(state);
            TurnSystem turn = new TurnSystem(
                g.Config, state, g.Random, g.Factory,
                new Climate(g.Config), new Difficulty(g.Config),
                network, g.Combat);

            Assert.IsTrue(turn.Plan.IsUnlocked(ActivityKind.Build), "đã mở sẵn từ lượt 3");
            Assert.IsTrue(turn.Plan.IsUnlocked(ActivityKind.Hunt));

            network.Buy("s2_stone_tools");
            network.Buy("s2_scavenge_meat");

            Assert.IsTrue(turn.Plan.IsUnlocked(ActivityKind.Build), "mua nút không đổi gì");
            Assert.IsTrue(turn.Plan.IsUnlocked(ActivityKind.Hunt));
            Assert.IsFalse(network.HasAllGates(2), "mới 2 trong 3 nút cổng");
        }

        // ---------- cờ applyNodeEffects ----------

        [Test]
        public void TatCo_ThiKhongHieuUngSoNao()
        {
            GameState state = StateAt(1, 1000);
            NeuronNetwork network = Build(state, GameMode.Mvp, applyEffects: false);
            BuyAll(network, "s1_climbing", "s1_short_upright", "s1_young_leaves", "s1_dig_roots");

            Assert.AreSame(NodeEffects.None, network.Effects);
            Assert.AreEqual(0, network.Effects.GatherYieldBonus);
            Assert.AreEqual(0.0, network.Effects.SpeedBonus, 1e-12);
        }

        [Test]
        public void BatCo_ThiCongDonHieuUng()
        {
            GameState state = StateAt(1, 1000);
            NeuronNetwork network = Build(state, GameMode.Mvp, applyEffects: true);

            BuyAll(network, "s1_climbing", "s1_short_upright", "s1_strong_jaw");
            Assert.AreEqual(0.5, network.Effects.GatherFlatPerMember, 1e-12);
            Assert.AreEqual(0.05, network.Effects.SpeedBonus, 1e-12);
            Assert.AreEqual(0.1, network.Effects.StrengthBonus, 1e-12);

            BuyAll(network, "s1_young_leaves", "s1_dig_roots", "s1_insects_eggs");
            Assert.AreEqual(1, network.Effects.GatherYieldBonus);
            Assert.AreEqual(1, network.Effects.DryPenaltyReduction);

            BuyAll(network, "s1_grooming", "s1_alarm_call", "s1_carry_young");
            Assert.AreEqual(1, network.Effects.MoraleActivityBonus);
            Assert.AreEqual(0.05, network.Effects.DefenceOddsBonus, 1e-12);
            Assert.AreEqual(3, network.Effects.BirthMoraleCostOverride);
        }

        [Test]
        public void HaiNutCungCongSpeed_ThiCongDon()
        {
            // "Đứng thẳng ngắn" (giai đoạn 1) và "Đi bằng hai chân" (giai đoạn 2) đều +0.05.
            GameState state = StateAt(1, 2000);
            state.CombatWins = 1;
            NeuronNetwork network = Build(state, GameMode.Mvp, applyEffects: true);

            BuyMandatoryGatesOfStage1(network);
            Assert.AreEqual(0.05, network.Effects.SpeedBonus, 1e-12);

            network.Buy("s1_evolve");
            network.Buy("s2_bipedal");
            Assert.AreEqual(0.10, network.Effects.SpeedBonus, 1e-12);
        }

        [Test]
        public void HieuUngChuaApDung_DuocLietKeRaChuKhongImLang()
        {
            GameState state = StateAt(1, 2000);
            NeuronNetwork network = Build(state, GameMode.Mvp, applyEffects: true);

            network.Buy("s1_young_leaves");       // bad_event_food_reduction, cần events.json ở I2
            IReadOnlyList<string> pending = network.Effects.NotYetApplied;

            Assert.AreEqual(1, pending.Count);
            StringAssert.Contains("bad_event_food_reduction", pending[0]);
        }

        // ---------- khôi phục từ ván đã lưu (chuẩn bị cho C7) ----------

        [Test]
        public void DungLaiTuDanhSachNutDaMuaTrongGameState()
        {
            GameState state = StateAt(2, 100);
            state.PurchasedNodeIds.Add("s2_stone_tools");
            state.PurchasedNodeIds.Add("s2_scavenge_meat");

            NeuronNetwork network = Build(state, GameMode.Mvp, applyEffects: true);

            Assert.IsTrue(network.Has("s2_stone_tools"));
            Assert.IsTrue(network.Has("s2_scavenge_meat"));
            Assert.AreEqual(PurchaseError.AlreadyPurchased, network.Buy("s2_stone_tools").Error);
        }

        [Test]
        public void VanDaLuuCoNutKhongTonTai_ThiNoNgay()
        {
            GameState state = StateAt(1, 0);
            state.PurchasedNodeIds.Add("s1_biet_bay");

            Assert.Throws<NeuronDataException>(() => Build(state));
        }

        private static void BuyAll(NeuronNetwork network, params string[] ids)
        {
            foreach (string id in ids)
            {
                PurchaseResult result = network.Buy(id);
                Assert.IsTrue(result.Succeeded, id + ": " + result.Message);
            }
        }

        private static void BuyMandatoryGatesOfStage1(NeuronNetwork network)
        {
            BuyAll(network,
                "s1_climbing", "s1_short_upright",
                "s1_young_leaves", "s1_dig_roots",
                "s1_grooming", "s1_alarm_call");
        }

        private static void BuyMandatoryGatesOfStage2(NeuronNetwork network)
        {
            BuyAll(network,
                "s2_bipedal", "s2_arched_foot",
                "s2_stone_tools", "s2_scavenge_meat",
                "s2_share_food", "s2_watch_rotation");
        }
    }
}
