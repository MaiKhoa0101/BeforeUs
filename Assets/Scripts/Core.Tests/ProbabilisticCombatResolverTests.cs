using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C6 — ProbabilisticCombatResolver (design.md §5.2, §8). Test xác suất trên 10.000 lần rút.</summary>
    [TestFixture]
    public class ProbabilisticCombatResolverTests
    {
        private const int Draws = 10000;

        private BalanceConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
        }

        private ProbabilisticCombatResolver Resolver(int seed = 1, IPurchasedNodes nodes = null)
        {
            return new ProbabilisticCombatResolver(_config, new XorShift128Random(seed), nodes);
        }

        private static CombatRequest Request(
            CombatKind kind, int party = 3, int stage = 2, int region = 0,
            int buildLevel = 0, int level = 0, int crew = 5)
        {
            int[] all = new int[crew];
            for (int i = 0; i < crew; i++)
            {
                all[i] = i + 1;
            }

            int[] members = new int[party];
            for (int i = 0; i < party; i++)
            {
                members[i] = i + 1;
            }

            return new CombatRequest(kind, members, stage, region, buildLevel, level, crew, all);
        }

        // ---------- công thức, design.md §5.2 ----------

        [Test]
        public void SanBat_KhopCongThuc()
        {
            ProbabilisticCombatResolver r = Resolver();

            Assert.AreEqual(0.50 + 0.08 * 3, r.WinChance(Request(CombatKind.Hunt, party: 3)), 1e-12);
            Assert.AreEqual(0.50 + 0.08 * 1, r.WinChance(Request(CombatKind.Hunt, party: 1)), 1e-12);
            Assert.AreEqual(
                0.50 + 0.08 * 2 + 0.05 * 3,
                r.WinChance(Request(CombatKind.Hunt, party: 2, buildLevel: 3)), 1e-12);
            Assert.AreEqual(
                0.50 + 0.08 * 2 - 0.02 * 4,
                r.WinChance(Request(CombatKind.Hunt, party: 2, level: 4)), 1e-12);
        }

        [Test]
        public void KhamPha_ThapHonSanBat0Phay1()
        {
            ProbabilisticCombatResolver r = Resolver();
            Assert.AreEqual(0.40 + 0.08 * 3, r.WinChance(Request(CombatKind.Explore, party: 3)), 1e-12);
        }

        [Test]
        public void ClampTren0Phay92VaDuoi0Phay05()
        {
            ProbabilisticCombatResolver r = Resolver();

            Assert.AreEqual(0.92, r.WinChance(Request(CombatKind.Hunt, party: 20)), 1e-12);
            Assert.AreEqual(0.05, r.WinChance(Request(CombatKind.Explore, party: 1, level: 50)), 1e-12);
        }

        // ---------- công thức, design.md §8 ----------

        [Test]
        public void PhongThu_KhopCongThuc()
        {
            ProbabilisticCombatResolver r = Resolver();

            Assert.AreEqual(
                0.35 + 0.05 * 5 + 0.04 * 2,
                r.WinChance(Request(CombatKind.Defence, crew: 5, stage: 2)), 1e-12);

            Assert.AreEqual(
                0.35 + 0.05 * 6 + 0.06 * 2 + 0.04 * 1 - 0.05 * 1 - 0.02 * 3,
                r.WinChance(Request(CombatKind.Defence, crew: 6, stage: 1, region: 1, buildLevel: 2, level: 3)),
                1e-12);
        }

        [Test]
        public void CoHoiCombat_DungChungCongThucPhongThu()
        {
            // before_us_sim.py:110-112 dùng cùng biến p cho cả phòng thủ và cơ hội combat.
            ProbabilisticCombatResolver r = Resolver();

            CombatRequest defence = Request(CombatKind.Defence, crew: 6, stage: 2, region: 1, buildLevel: 1, level: 2);
            CombatRequest opportunity = Request(CombatKind.Opportunity, crew: 6, stage: 2, region: 1, buildLevel: 1, level: 2);

            Assert.AreEqual(r.WinChance(defence), r.WinChance(opportunity), 1e-12);
        }

        [Test]
        public void PhongThu_ClampTren0Phay95()
        {
            ProbabilisticCombatResolver r = Resolver();
            Assert.AreEqual(0.95, r.WinChance(Request(CombatKind.Defence, crew: 30)), 1e-12);
            Assert.AreEqual(0.05, r.WinChance(Request(CombatKind.Defence, crew: 1, level: 40)), 1e-12);
        }

        [Test]
        public void KhopVoiCongThucCuaMoPhong()
        {
            // Quét thẳng công thức before_us_sim.py:55 và :101-102.
            ProbabilisticCombatResolver r = Resolver();

            for (int k = 1; k <= 6; k++)
            {
                for (int build = 0; build <= 3; build++)
                {
                    for (int level = 0; level <= 4; level++)
                    {
                        double hunt = Clamp(0.50 - 0.02 * level + 0.08 * k + 0.05 * build, 0.05, 0.92);
                        double explore = Clamp(0.40 - 0.02 * level + 0.08 * k + 0.05 * build, 0.05, 0.92);

                        Assert.AreEqual(hunt, r.WinChance(Request(CombatKind.Hunt, party: k, buildLevel: build, level: level)), 1e-12);
                        Assert.AreEqual(explore, r.WinChance(Request(CombatKind.Explore, party: k, buildLevel: build, level: level)), 1e-12);
                    }
                }
            }

            for (int crew = 1; crew <= 8; crew++)
            {
                for (int stage = 1; stage <= 3; stage++)
                {
                    for (int region = 0; region <= 1; region++)
                    {
                        double defence = Clamp(
                            0.35 + 0.05 * crew + 0.06 * 0 + 0.04 * stage - 0.05 * region, 0.05, 0.95);
                        Assert.AreEqual(
                            defence,
                            r.WinChance(Request(CombatKind.Defence, crew: crew, stage: stage, region: region)),
                            1e-12);
                    }
                }
            }
        }

        private static double Clamp(double x, double a, double b)
        {
            return System.Math.Max(a, System.Math.Min(x, b));
        }

        // ---------- tần suất thực tế trên 10.000 lần rút ----------

        [Test]
        public void TanSuatThangThucTe_KhopCongThuc()
        {
            ProbabilisticCombatResolver r = Resolver(2024);

            foreach (CombatKind kind in new[] { CombatKind.Hunt, CombatKind.Explore, CombatKind.Defence })
            {
                CombatRequest request = Request(kind, party: 3, crew: 5, stage: 2);
                double expected = r.WinChance(request);

                int wins = 0;
                for (int i = 0; i < Draws; i++)
                {
                    if (r.Resolve(request).Won)
                    {
                        wins++;
                    }
                }

                double actual = (double)wins / Draws;

                // Sai số chuẩn ở n = 10.000 với p ≈ 0.5 là 0.5 điểm %; 4 sai số chuẩn là dư an toàn.
                Assert.That(actual, Is.EqualTo(expected).Within(0.02), kind + ": thực tế " + actual + " vs công thức " + expected);
            }
        }

        [Test]
        public void TanSuatChetKhiThang_KhopWinDeathChanceAvg()
        {
            ProbabilisticCombatResolver r = Resolver(7);
            CombatRequest request = Request(CombatKind.Hunt, party: 6, crew: 8);

            int wins = 0, deathsOnWin = 0;
            for (int i = 0; i < Draws; i++)
            {
                CombatResult result = r.Resolve(request);
                if (!result.Won)
                {
                    continue;
                }

                wins++;
                if (result.DeadMemberIds.Count > 0)
                {
                    deathsOnWin++;
                }
            }

            Assert.Greater(wins, 1000);
            Assert.That((double)deathsOnWin / wins, Is.EqualTo(0.5).Within(0.03));
        }

        [Test]
        public void ThangThiChetToiDaMotNguoi()
        {
            ProbabilisticCombatResolver r = Resolver(11);
            CombatRequest request = Request(CombatKind.Hunt, party: 6, crew: 8);

            for (int i = 0; i < Draws; i++)
            {
                CombatResult result = r.Resolve(request);
                if (result.Won)
                {
                    Assert.LessOrEqual(result.DeadMemberIds.Count, 1);
                }
            }
        }

        [Test]
        public void ThuaSanHoacKhamPha_MatDungLossDeaths()
        {
            ProbabilisticCombatResolver r = Resolver(13);
            CombatRequest request = Request(CombatKind.Hunt, party: 1, crew: 8, level: 20);   // tỉ lệ thắng kịch sàn

            int losses = 0;
            for (int i = 0; i < 2000; i++)
            {
                CombatResult result = r.Resolve(request);
                if (!result.Won)
                {
                    losses++;
                    Assert.AreEqual(2, result.DeadMemberIds.Count);
                }
            }

            Assert.Greater(losses, 1500);
        }

        // ---------- quota phòng thủ, design.md §8 ----------

        [Test]
        public void QuotaTheoVungVaDoKho()
        {
            ProbabilisticCombatResolver r = Resolver();

            Assert.AreEqual(1, r.RaidQuota(Request(CombatKind.Defence, region: 0, level: 0)));
            Assert.AreEqual(2, r.RaidQuota(Request(CombatKind.Defence, region: 1, level: 0)));
            Assert.AreEqual(4, r.RaidQuota(Request(CombatKind.Defence, region: 1, level: 2)), "quota 2 + 1 mỗi bậc");
        }

        [Test]
        public void ThuaPhongThu_MatDungQuotaNguoi()
        {
            ProbabilisticCombatResolver r = Resolver(17);
            CombatRequest request = Request(CombatKind.Defence, crew: 8, region: 1, level: 2, stage: 1);

            int losses = 0;
            for (int i = 0; i < 5000; i++)
            {
                CombatResult result = r.Resolve(request);
                if (!result.Won)
                {
                    losses++;
                    Assert.AreEqual(4, result.DeadMemberIds.Count);
                }
            }

            Assert.Greater(losses, 100);
        }

        [Test]
        public void D11_ThuaPhongThuKhongBaoGioLamThuaVanNgayLapTuc()
        {
            // D11 / design.md §8: "ăn đúng quota người rồi rút, không bao giờ xóa sổ cả bầy".
            // Số người chừa lại suy ra từ ngưỡng thua, không viết cứng: sau góp ý 8 của Khoa
            // (playtest 01, thua khi còn <= 1 người) thì phải chừa lại 2, không còn là 1.
            Assert.AreEqual(2, _config.MinSurvivorsAfterForcedCombat);

            ProbabilisticCombatResolver r = Resolver(19);
            CombatRequest request = Request(CombatKind.Defence, crew: 4, party: 4, region: 1, level: 5, stage: 1);

            Assert.AreEqual(7, r.RaidQuota(request), "quota danh nghĩa vượt xa sĩ số");

            int losses = 0;
            for (int i = 0; i < 2000; i++)
            {
                CombatResult result = r.Resolve(request);
                if (!result.Won)
                {
                    losses++;
                    Assert.AreEqual(2, result.DeadMemberIds.Count, "4 người thì mất 2, còn lại 2 — chưa thua");
                    Assert.Greater(
                        4 - result.DeadMemberIds.Count, _config.LoseWhenCrewAtOrBelow,
                        "sĩ số còn lại phải TRÊN ngưỡng thua, nếu không D11 vô nghĩa");
                }
            }

            Assert.Greater(losses, 100);
        }

        [Test]
        public void D11_SiSoDaONguongThua_ThiPhongThuThuaKhongGietThem()
        {
            ProbabilisticCombatResolver r = Resolver(37);

            foreach (int crew in new[] { 1, 2 })
            {
                CombatRequest request = Request(CombatKind.Defence, crew: crew, party: crew, region: 1, level: 5, stage: 1);
                for (int i = 0; i < 500; i++)
                {
                    CombatResult result = r.Resolve(request);
                    if (!result.Won)
                    {
                        Assert.AreEqual(0, result.DeadMemberIds.Count, "sĩ số " + crew);
                    }
                }
            }
        }

        [Test]
        public void D11_ChiApChoPhongThu_CacTranTuyChonVanXoaSoDuocCaBay()
        {
            // Đối xứng của design.md §8: Cơ hội combat là tùy chọn, tự chọn đánh thì chịu.
            ProbabilisticCombatResolver r = Resolver(41);
            CombatRequest opportunity = Request(CombatKind.Opportunity, crew: 2, party: 2, level: 30, stage: 1);

            bool sawWipeOut = false;
            for (int i = 0; i < 2000 && !sawWipeOut; i++)
            {
                CombatResult result = r.Resolve(opportunity);
                if (!result.Won && result.DeadMemberIds.Count == 2)
                {
                    sawWipeOut = true;
                }
            }

            Assert.IsTrue(sawWipeOut, "thua Cơ hội combat vẫn phải giết được cả bầy");
        }

        // ---------- nạn nhân ----------

        [Test]
        public void NanNhanKhongTrungNhau_VaNamTrongBoToc()
        {
            ProbabilisticCombatResolver r = Resolver(23);
            CombatRequest request = Request(CombatKind.Defence, crew: 6, party: 6, region: 1, level: 1, stage: 1);

            for (int i = 0; i < 2000; i++)
            {
                CombatResult result = r.Resolve(request);
                HashSet<int> seen = new HashSet<int>();
                foreach (int id in result.DeadMemberIds)
                {
                    Assert.IsTrue(seen.Add(id), "nạn nhân trùng");
                    CollectionAssert.Contains(request.AllMemberIds, id);
                }
            }
        }

        [Test]
        public void NanNhanCoTheLaNguoiKhongDiTran()
        {
            // design.md §5.2: "Thua: mất 2 người" — không kèm điều kiện tham gia.
            ProbabilisticCombatResolver r = Resolver(29);
            CombatRequest request = Request(CombatKind.Hunt, party: 1, crew: 6, level: 20);

            bool sawStayHome = false;
            for (int i = 0; i < 500 && !sawStayHome; i++)
            {
                CombatResult result = r.Resolve(request);
                foreach (int id in result.DeadMemberIds)
                {
                    if (id != 1)
                    {
                        sawStayHome = true;
                    }
                }
            }

            Assert.IsTrue(sawStayHome);
        }

        // ---------- trận hướng dẫn, design.md §8 ----------

        [Test]
        public void TranHuongDan_LuonThangVaKhongAiChet()
        {
            ProbabilisticCombatResolver r = Resolver(31);
            CombatRequest request = Request(CombatKind.Tutorial, party: 2, crew: 5, level: 10);

            Assert.AreEqual(1.0, r.WinChance(request), 1e-12);

            for (int i = 0; i < 1000; i++)
            {
                CombatResult result = r.Resolve(request);
                Assert.IsTrue(result.Won);
                Assert.AreEqual(0, result.DeadMemberIds.Count);
            }
        }

        // ---------- hiệu ứng nút ----------

        [Test]
        public void HieuUngNut_CongVaoTiLeThang()
        {
            BalanceConfig config = _config;
            GameState state = new GameState { Turn = 1, Stage = 2, Neuron = 1000 };
            Neuron.NeuronNetwork network = new Neuron.NeuronNetwork(
                Neuron.NeuronNetwork.LoadData(TestPaths.ReadNeuronsJson()), config, state, applyNodeEffects: true);

            ProbabilisticCombatResolver without = Resolver();
            ProbabilisticCombatResolver with = Resolver(1, network);

            CombatRequest explore = Request(CombatKind.Explore, party: 3);
            Assert.AreEqual(without.WinChance(explore), with.WinChance(explore), 1e-12);

            network.Buy("s2_bipedal");
            network.Buy("s2_arched_foot");   // Khám phá +0.05
            Assert.AreEqual(without.WinChance(explore) + 0.05, with.WinChance(explore), 1e-12);
        }

        [Test]
        public void TatCoApNodeEffects_ThiTiLeThangKhongDoi()
        {
            GameState state = new GameState { Turn = 1, Stage = 2, Neuron = 1000 };
            Neuron.NeuronNetwork network = new Neuron.NeuronNetwork(
                Neuron.NeuronNetwork.LoadData(TestPaths.ReadNeuronsJson()), _config, state, applyNodeEffects: false);

            network.Buy("s2_bipedal");
            network.Buy("s2_arched_foot");

            CombatRequest explore = Request(CombatKind.Explore, party: 3);
            Assert.AreEqual(Resolver().WinChance(explore), Resolver(1, network).WinChance(explore), 1e-12);
        }
    }
}
