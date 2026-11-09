using System.Collections.Generic;
using System.IO;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    /// <summary>
    /// Đường đi trọn vẹn: CombatRequest của Core → dựng sân từ beasts.json → chạy trận →
    /// CombatResult trả về Core.
    /// </summary>
    [TestFixture]
    public sealed class SimulatedCombatResolverTests
    {
        private BalanceConfig _config;
        private BeastCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _config = TestBattles.Config();
            _catalog = new BeastCatalog(BeastCatalog.LoadData(File.ReadAllText(TestBattles.BeastsJsonPath)));
        }

        private SimulatedCombatResolver Resolver(IReadOnlyList<Member> tribe, IRandom random)
        {
            MemberList members = new MemberList(tribe);
            return new SimulatedCombatResolver(
                new BattleSetupFactory(_catalog, members),
                new DesignTollCasualtyPolicy(
                    _config, new ProbabilisticCombatResolver(_config, random), members, random),
                new RandomDeploymentAutoPlayer(random),
                random);
        }

        private static List<Member> Tribe(params int[] hps)
        {
            List<Member> tribe = new List<Member>();
            for (int i = 0; i < hps.Length; i++)
            {
                tribe.Add(TestBattles.Member(i + 1, hp: hps[i]));
            }

            return tribe;
        }

        private static int[] Ids(IReadOnlyList<Member> tribe)
        {
            int[] ids = new int[tribe.Count];
            for (int i = 0; i < tribe.Count; i++)
            {
                ids[i] = tribe[i].Id;
            }

            return ids;
        }

        [Test]
        public void TranHuongDan_LuonThang_KhongAiChet_VaKhongVaoSan()
        {
            List<Member> tribe = Tribe(5, 5, 5);
            SimulatedCombatResolver resolver = Resolver(tribe, TestBattles.Random());

            CombatResult result = resolver.Resolve(
                TestBattles.Request(CombatKind.Tutorial, Ids(tribe)));

            Assert.That(result.Won, Is.True);
            Assert.That(result.DeadMemberIds, Is.Empty);
            Assert.That(resolver.LastReport, Is.Null, "design.md §8: trận hướng dẫn không vào sân");
        }

        [Test]
        public void SanBat_ChayTronMotTran_VaTraVeCombatResult()
        {
            List<Member> tribe = Tribe(5, 4, 6);
            SimulatedCombatResolver resolver = Resolver(tribe, TestBattles.Random());

            CombatResult result = resolver.Resolve(
                TestBattles.Request(CombatKind.Hunt, new[] { 1, 2 }, Ids(tribe)));

            Assert.That(resolver.LastReport, Is.Not.Null);
            Assert.That(resolver.LastReport.Kind, Is.EqualTo(BattleKind.Lure), "Săn bắt là Dụ và hạ");
            Assert.That(resolver.LastReport.ElapsedSeconds, Is.GreaterThan(0.0));

            foreach (int id in result.DeadMemberIds)
            {
                Assert.That(Ids(tribe), Contains.Item(id), "Chỉ được giết người có thật");
            }
        }

        [Test]
        public void NguoiChet_KhongBaoGioTrungNhau()
        {
            List<Member> tribe = Tribe(3, 3, 3, 3, 3);
            SimulatedCombatResolver resolver = Resolver(tribe, TestBattles.Random(4242));

            for (int i = 0; i < 200; i++)
            {
                CombatResult result = resolver.Resolve(
                    TestBattles.Request(CombatKind.Defence, Ids(tribe), Ids(tribe), difficultyLevel: 3));

                Assert.That(result.DeadMemberIds, Is.Unique);
            }
        }

        [Test]
        public void CungSeed_ThiCungKetQua()
        {
            List<Member> tribe = Tribe(5, 4, 6);

            CombatResult a = Resolver(tribe, TestBattles.Random(999))
                .Resolve(TestBattles.Request(CombatKind.Hunt, new[] { 1, 2 }, Ids(tribe)));
            CombatResult b = Resolver(tribe, TestBattles.Random(999))
                .Resolve(TestBattles.Request(CombatKind.Hunt, new[] { 1, 2 }, Ids(tribe)));

            Assert.That(b.Won, Is.EqualTo(a.Won));
            Assert.That(b.DeadMemberIds, Is.EqualTo(a.DeadMemberIds));
        }

        [Test]
        public void KhamPha_TuRungNhietDoi_GapCaBayLinhCau()
        {
            List<Member> tribe = Tribe(5, 5, 5);
            SimulatedCombatResolver resolver = Resolver(tribe, TestBattles.Random());

            resolver.Resolve(TestBattles.Request(CombatKind.Explore, Ids(tribe), Ids(tribe), region: 0));

            Assert.That(resolver.LastReport.Kind, Is.EqualTo(BattleKind.Prevent), "Khám phá là Phòng thủ");
            Assert.That(resolver.LastReport.BeastCount, Is.EqualTo(_catalog.Find("hyena_pack").Data.PackSize));
        }

        [Test]
        public void ThuaPhongThu_VanChuaLaiItNhat1Nguoi_KeCaODoKhoCao()
        {
            List<Member> tribe = Tribe(3, 3, 3);
            SimulatedCombatResolver resolver = Resolver(tribe, TestBattles.Random(31337));

            for (int i = 0; i < 300; i++)
            {
                CombatResult result = resolver.Resolve(
                    TestBattles.Request(CombatKind.Defence, Ids(tribe), Ids(tribe), difficultyLevel: 8));

                Assert.That(result.DeadMemberIds.Count, Is.LessThanOrEqualTo(tribe.Count - 1),
                    "D11: phòng thủ không bao giờ xóa sổ cả bầy");
            }
        }

        // --- Kiểu tấn công, đo trên đường đi trọn vẹn -------------------------------------

        /// <summary>Đếm số lần mỗi người chết qua nhiều trận cùng một kịch bản.</summary>
        private Dictionary<int, int> DeathTally(
            string speciesId, CombatKind kind, IReadOnlyList<Member> tribe, int battles, int seed)
        {
            IRandom random = TestBattles.Random(seed);
            MemberList members = new MemberList(tribe);
            BattleSetupFactory factory = new BattleSetupFactory(_catalog, members);
            BeastSpecies species = _catalog.Find(speciesId);

            SimulatedCombatResolver resolver = new SimulatedCombatResolver(
                factory,
                new DesignTollCasualtyPolicy(
                    _config, new ProbabilisticCombatResolver(_config, random), members, random),
                new RandomDeploymentAutoPlayer(random),
                random);

            Dictionary<int, int> tally = new Dictionary<int, int>();
            for (int i = 0; i < tribe.Count; i++)
            {
                tally[tribe[i].Id] = 0;
            }

            CombatRequest request = TestBattles.Request(kind, Ids(tribe), Ids(tribe), region: 0);

            for (int i = 0; i < battles; i++)
            {
                BattleReport report = resolver.Play(factory.Build(request, species, random));
                foreach (int id in report.DeadMemberIds)
                {
                    tally[id]++;
                }
            }

            return tally;
        }

        private const int StyleBattles = 3000;

        /// <summary>
        /// Tổng số người ngã phải đủ lớn thì mới nói được gì về thiên hướng.
        ///
        /// Có lý do cụ thể: bản đầu của ba test này chạy 400 trận với chỉ số thú lúc ấy, và khi
        /// B3 hiệu chỉnh hổ răng kiếm yếu đi thì tổng số người chết tụt xuống 23 — tỉ số 13/10
        /// không chứng minh được gì, nhưng test vẫn "đỏ" như thể kiểu tấn công hỏng.
        /// Một test đổi màu theo con số cân bằng là một test nói dối.
        /// </summary>
        private static int Total(Dictionary<int, int> tally)
        {
            int total = 0;
            foreach (KeyValuePair<int, int> entry in tally)
            {
                total += entry.Value;
            }

            Assert.That(total, Is.GreaterThan(150),
                "Kịch bản chỉ có " + total + " người ngã — quá ít để đo thiên hướng. "
                + "Sửa kịch bản test, đừng nới ngưỡng.");

            return total;
        }

        /// <summary>
        /// Kiểm định độ lệch bằng SAI SỐ CHUẨN, không bằng một tỉ lệ tự đặt.
        ///
        /// Lý do: thiên hướng đo trên đường đi trọn vẹn YẾU hơn nhiều so với ở hàm chọn mục tiêu,
        /// vì thú chỉ đánh được ai ĐANG TRONG TẦM — mà thả quân ngẫu nhiên thì người khỏe nhất
        /// (hay yếu nhất) thường không ở đó. Một ngưỡng kiểu "phải gấp 1,3 lần" chỉ là con số tôi
        /// bịa ra và sẽ đỏ/xanh theo từng lần hiệu chỉnh cân bằng.
        ///
        /// Chênh lệch giữa hai bộ đếm có độ lệch chuẩn ≈ √(a+b); đòi 2,5 lần độ lệch chuẩn là
        /// đòi một tín hiệu thật, và nó tự co giãn theo cỡ mẫu.
        /// </summary>
        private static void AssertBiasTowards(
            Dictionary<int, int> tally, int favoured, int against, string style)
        {
            int a = tally[favoured];
            int b = tally[against];
            double sigma = System.Math.Sqrt(a + b);

            Assert.That(
                a - b,
                Is.GreaterThan(2.5 * sigma),
                style + ": #" + favoured + " chết " + a + " lần, #" + against + " chết " + b
                + " lần. Chênh " + (a - b) + ", cần hơn " + (2.5 * sigma).ToString("F0")
                + " mới vượt được nhiễu.");
        }

        [Test]
        public void BaoHoaMai_SanKeYeu_GietNguoiMauThapNhieuHonHan()
        {
            // #1 hp 3 (yếu nhất) ... #4 hp 7 (dày nhất). Kiểu "săn kẻ yếu" phải lệch rõ về #1.
            Dictionary<int, int> tally = DeathTally(
                "leopard", CombatKind.Defence, Tribe(3, 5, 6, 7), StyleBattles, seed: 20261108);

            Total(tally);
            AssertBiasTowards(tally, favoured: 1, against: 4, style: "Báo hoa mai săn kẻ yếu");
        }

        [Test]
        public void HoRangKiem_ThachDauKeManh_GietNguoiStrengthCaoNhieuHonHan()
        {
            // Máu thấp nhất của dải §3 để con thú đã được hiệu chỉnh yếu đi vẫn giết đủ người
            // mà đo được. Thiên hướng phải đến từ `strength`, nên hp cho bằng nhau hết.
            List<Member> tribe = new List<Member>
            {
                TestBattles.Member(1, strength: 0.8, hp: 3),
                TestBattles.Member(2, strength: 0.9, hp: 3),
                TestBattles.Member(3, strength: 1.1, hp: 3),
                TestBattles.Member(4, strength: 1.2, hp: 3)
            };

            Dictionary<int, int> tally = DeathTally(
                "sabertooth", CombatKind.Opportunity, tribe, StyleBattles, seed: 20261108);

            Total(tally);
            AssertBiasTowards(tally, favoured: 4, against: 1, style: "Hổ răng kiếm thách đấu kẻ mạnh");
        }

        [Test]
        public void LinhCau_NgauNhien_KhongLechVeAiCa()
        {
            // Mọi người cùng chỉ số: kiểu "ngẫu nhiên" không được dồn vào một ai.
            Dictionary<int, int> tally = DeathTally(
                "hyena_pack", CombatKind.Defence, Tribe(3, 3, 3, 3), StyleBattles, seed: 20261108);

            int total = Total(tally);
            double expected = total / 4.0;

            foreach (KeyValuePair<int, int> entry in tally)
            {
                Assert.That(entry.Value, Is.EqualTo(expected).Within(expected * 0.35),
                    "Người #" + entry.Key + " chết " + entry.Value + "/" + total
                    + " lần — kiểu ngẫu nhiên mà lệch thế này là có thiên vị");
            }
        }
    }
}
