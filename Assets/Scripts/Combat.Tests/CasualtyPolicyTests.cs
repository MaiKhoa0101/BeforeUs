using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    /// <summary>
    /// Mức tổn thất của luật: design.md §5.2 "Thua: mất 2 người", §8 "ăn đúng quota người",
    /// docs/decisions.md D11 "phòng thủ thua luôn chừa lại ít nhất 1 người".
    /// </summary>
    [TestFixture]
    public sealed class CasualtyPolicyTests
    {
        private BalanceConfig _config;
        private IRandom _random;
        private ProbabilisticCombatResolver _odds;

        [SetUp]
        public void SetUp()
        {
            _config = TestBattles.Config();
            _random = TestBattles.Random();
            _odds = new ProbabilisticCombatResolver(_config, _random);
        }

        private DesignTollCasualtyPolicy Policy(params Member[] members)
        {
            return new DesignTollCasualtyPolicy(_config, _odds, new MemberList(members), _random);
        }

        /// <summary>
        /// Dựng thẳng một <see cref="BattleReport"/> thay vì dàn dựng cả một trận chỉ để ép ra đúng
        /// danh sách người ngã. Dùng được nhờ InternalsVisibleTo, giống cách Core.Tests dựng
        /// TurnEventContext.
        /// </summary>
        private static BattleReport Report(
            bool won,
            IReadOnlyList<int> deadInBattle,
            BeastAttackStyle style = BeastAttackStyle.HuntWeakest,
            BattleKind kind = BattleKind.Lure)
        {
            return new BattleReport(
                won,
                won ? BattleEndReason.AllBeastsDown : BattleEndReason.BeastEscaped,
                12.0,
                deadInBattle,
                won ? 1 : 0,
                1,
                deadInBattle.Count,
                deadInBattle.Count,
                style,
                kind);
        }

        [Test]
        public void ThuaSanBat_ChetTrongSanItHonMucLuat_ThiBuChoDu()
        {
            // §5.2: thua thì mất 2 người, kể cả khi trận chỉ giết 1.
            Member[] tribe =
            {
                TestBattles.Member(1, hp: 5),
                TestBattles.Member(2, hp: 3),
                TestBattles.Member(3, hp: 7),
                TestBattles.Member(4, hp: 6)
            };

            CombatRequest request = TestBattles.Request(CombatKind.Hunt, new[] { 1, 2 }, new[] { 1, 2, 3, 4 });
            CombatResult result = Policy(tribe).Apply(request, Report(false, new[] { 1 }));

            Assert.That(result.Won, Is.False);
            Assert.That(result.DeadMemberIds.Count, Is.EqualTo(_config.Data.CombatResults.LossDeaths));
            Assert.That(result.DeadMemberIds, Contains.Item(1), "Người ngã trong sân phải nằm trong danh sách");
        }

        [Test]
        public void ThuaSanBat_NguoiBuThem_LayCaNguoiOnHA_VaTheoKieuTanCong()
        {
            // §5.2 không kèm điều kiện tham gia; CombatRequest.AllMemberIds tồn tại chính vì chuyện này.
            // "Săn kẻ yếu" → người bù thêm phải là người hp GỐC thấp nhất trong số còn lại.
            Member[] tribe =
            {
                TestBattles.Member(1, hp: 5),
                TestBattles.Member(2, hp: 7),
                TestBattles.Member(3, hp: 3)
            };

            CombatRequest request = TestBattles.Request(CombatKind.Hunt, new[] { 1 }, new[] { 1, 2, 3 });
            CombatResult result = Policy(tribe).Apply(request, Report(false, new[] { 1 }));

            Assert.That(result.DeadMemberIds, Is.EquivalentTo(new[] { 1, 3 }));
        }

        [Test]
        public void ThuaSanBat_SanGietNhieuHonMucLuat_ThiGiuNguyenSoCuaSan()
        {
            Member[] tribe =
            {
                TestBattles.Member(1), TestBattles.Member(2), TestBattles.Member(3), TestBattles.Member(4)
            };

            CombatRequest request = TestBattles.Request(CombatKind.Hunt, new[] { 1, 2, 3 }, new[] { 1, 2, 3, 4 });
            CombatResult result = Policy(tribe).Apply(request, Report(false, new[] { 1, 2, 3 }));

            Assert.That(result.DeadMemberIds, Is.EquivalentTo(new[] { 1, 2, 3 }),
                "Họ chết thật trong sân rồi, không ai sống lại vì luật chỉ đòi 2");
        }

        [Test]
        public void Thang_ThiKhongBuThemAi()
        {
            // §5.2 ghi thắng vẫn có thể mất 1 người (trung bình 50%) — con số đó phải TỰ MỌC RA
            // từ trận đánh, không phải do chính sách này nhét vào.
            Member[] tribe = { TestBattles.Member(1), TestBattles.Member(2), TestBattles.Member(3) };

            CombatRequest request = TestBattles.Request(CombatKind.Hunt, new[] { 1, 2 }, new[] { 1, 2, 3 });
            CombatResult result = Policy(tribe).Apply(request, Report(true, new int[0]));

            Assert.That(result.Won, Is.True);
            Assert.That(result.DeadMemberIds, Is.Empty);
        }

        [Test]
        public void ThuaPhongThu_DungDungQuotaCuaCore()
        {
            Member[] tribe =
            {
                TestBattles.Member(1), TestBattles.Member(2), TestBattles.Member(3),
                TestBattles.Member(4), TestBattles.Member(5)
            };

            CombatRequest request = TestBattles.Request(
                CombatKind.Defence, new[] { 1, 2, 3, 4, 5 }, new[] { 1, 2, 3, 4, 5 }, region: 0, difficultyLevel: 1);

            int quota = _odds.RaidQuota(request);
            CombatResult result = Policy(tribe).Apply(
                request, Report(false, new int[0], BeastAttackStyle.Random, BattleKind.Prevent));

            Assert.That(quota, Is.GreaterThan(0), "Kịch bản test phải có quota dương");
            Assert.That(result.DeadMemberIds.Count, Is.EqualTo(quota));
        }

        [Test]
        public void ThuaPhongThu_D11_LuonChuaLaiItNhat1Nguoi()
        {
            // Độ khó cao kéo quota vượt sĩ số: quota = raid_quota_by_region[0] + 1 × level.
            Member[] tribe = { TestBattles.Member(1), TestBattles.Member(2), TestBattles.Member(3) };

            CombatRequest request = TestBattles.Request(
                CombatKind.Defence, new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, region: 0, difficultyLevel: 6);

            Assert.That(_odds.RaidQuota(request), Is.GreaterThanOrEqualTo(3), "Kịch bản phải có quota >= sĩ số");

            CombatResult result = Policy(tribe).Apply(
                request, Report(false, new int[0], BeastAttackStyle.Random, BattleKind.Prevent));

            Assert.That(result.DeadMemberIds.Count, Is.EqualTo(2));
        }

        [Test]
        public void ThuaPhongThu_SanGietSachDoiHinh_VanPhaiChuaLai1Nguoi()
        {
            // Ca này D11 chỉ chặn được nếu kẹp cả người ngã TRONG SÂN. Không kẹp thì trận phòng thủ
            // bắt buộc thành cú thua tức thì không tránh nổi — đúng cái D11 bác bỏ.
            Member[] tribe = { TestBattles.Member(1), TestBattles.Member(2), TestBattles.Member(3) };

            CombatRequest request = TestBattles.Request(
                CombatKind.Defence, new[] { 1, 2, 3 }, new[] { 1, 2, 3 });

            CombatResult result = Policy(tribe).Apply(
                request,
                Report(false, new[] { 1, 2, 3 }, BeastAttackStyle.Random, BattleKind.Prevent));

            Assert.That(result.DeadMemberIds.Count, Is.EqualTo(2));
            Assert.That(result.DeadMemberIds, Is.EquivalentTo(new[] { 1, 2 }), "Cắt người ngã sau cùng trước");
        }

        [Test]
        public void ThuaCoHoiCombat_VanXoaSoDuocCaBay()
        {
            // D11 chỉ che trận Phòng thủ BẮT BUỘC. Cơ hội combat là tùy chọn: tự chọn đánh thì chịu.
            Member[] tribe = { TestBattles.Member(1), TestBattles.Member(2) };

            CombatRequest request = TestBattles.Request(CombatKind.Opportunity, new[] { 1, 2 }, new[] { 1, 2 });
            CombatResult result = Policy(tribe).Apply(request, Report(false, new[] { 1 }));

            Assert.That(result.DeadMemberIds.Count, Is.EqualTo(2));
        }

        [Test]
        public void BoToc_ItHonMucTonThat_ThiChiMatNhungAiCon()
        {
            Member[] tribe = { TestBattles.Member(1) };

            CombatRequest request = TestBattles.Request(CombatKind.Hunt, new[] { 1 }, new[] { 1 });
            CombatResult result = Policy(tribe).Apply(request, Report(false, new int[0]));

            Assert.That(result.DeadMemberIds.Count, Is.EqualTo(1));
        }

        [Test]
        public void ChinhSachChiTinhSan_KhongBuThemAi()
        {
            // Dụng cụ đo, không phải lựa chọn thiết kế.
            CombatRequest request = TestBattles.Request(CombatKind.Hunt, new[] { 1, 2 }, new[] { 1, 2, 3 });
            CombatResult result = new SceneOnlyCasualtyPolicy().Apply(request, Report(false, new[] { 1 }));

            Assert.That(result.DeadMemberIds, Is.EquivalentTo(new[] { 1 }));
        }

    }
}
