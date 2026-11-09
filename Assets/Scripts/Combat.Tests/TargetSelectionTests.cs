using System.Collections.Generic;
using BeforeUs.Core;
using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    /// <summary>
    /// design.md §9: kiểu tấn công của quái thú quyết định AI CHẾT.
    /// B2 bổ sung bộ test chạy trên đúng số liệu Assets/Data/beasts.json.
    /// </summary>
    [TestFixture]
    public sealed class TargetSelectionTests
    {
        private static List<Combatant> Field(params CombatantSpec[] specs)
        {
            List<Combatant> list = new List<Combatant>();
            for (int i = 0; i < specs.Length; i++)
            {
                BattleSimulation battle = new BattleSimulation(
                    new BattleSetup(
                        BattleKind.Lure,
                        TestBattles.LureField(),
                        new[] { specs[i] },
                        new[] { TestBattles.Beast(0, x: 5.0) },
                        30.0),
                    TestBattles.Random());

                list.Add(battle.Tribe[0]);
            }

            return list;
        }

        private static CombatantSpec MemberSpec(int id, double strength, int hp)
        {
            return CombatantSpec.ForMember(TestBattles.Member(id, 1.0, strength, hp), TestBattles.Profile());
        }

        [Test]
        public void SanKeYeu_NhamNguoiMauThapNhat()
        {
            List<Combatant> candidates = Field(
                MemberSpec(1, 1.0, 7),
                MemberSpec(2, 1.0, 3),
                MemberSpec(3, 1.0, 5));

            Combatant picked = TargetSelection.PickOnField(
                BeastAttackStyle.HuntWeakest, candidates, TestBattles.Random());

            Assert.That(picked.Id, Is.EqualTo(2));
        }

        [Test]
        public void SanKeYeu_DocMauCONLAI_ChuKhongPhaiHpGoc()
        {
            // [ĐỀ XUẤT] diễn giải: trong sân thì "kẻ yếu" là người đang bị thương nặng nhất.
            List<Combatant> candidates = Field(
                MemberSpec(1, 1.0, 7),
                MemberSpec(2, 1.0, 5));

            candidates[0].Hp = 1.0;   // người hp gốc cao nhất nhưng đang thoi thóp

            Combatant picked = TargetSelection.PickOnField(
                BeastAttackStyle.HuntWeakest, candidates, TestBattles.Random());

            Assert.That(picked.Id, Is.EqualTo(1));
        }

        [Test]
        public void ThachDauKeManh_NhamNguoiStrengthCaoNhat()
        {
            List<Combatant> candidates = Field(
                MemberSpec(1, 0.9, 5),
                MemberSpec(2, 1.2, 5),
                MemberSpec(3, 1.0, 5));

            Combatant picked = TargetSelection.PickOnField(
                BeastAttackStyle.ChallengeStrongest, candidates, TestBattles.Random());

            Assert.That(picked.Id, Is.EqualTo(2));
        }

        [Test]
        public void NgauNhien_ChonDuocMoiNguoi()
        {
            List<Combatant> candidates = Field(
                MemberSpec(1, 1.0, 5),
                MemberSpec(2, 1.0, 5),
                MemberSpec(3, 1.0, 5));

            IRandom random = TestBattles.Random();
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < 300; i++)
            {
                seen.Add(TargetSelection.PickOnField(BeastAttackStyle.Random, candidates, random).Id);
            }

            Assert.That(seen, Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void HoaNhau_ThiBocNgauNhien_KhongThienVIIdNho()
        {
            // Thiên vị theo id là loại lệch không ai nhìn ra cho tới khi có người để ý
            // nhân vật #1 chết nhiều bất thường.
            List<Combatant> candidates = Field(
                MemberSpec(1, 1.0, 5),
                MemberSpec(2, 1.0, 5),
                MemberSpec(3, 1.0, 5));

            IRandom random = TestBattles.Random();
            int firstId = 0;
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < 300; i++)
            {
                int id = TargetSelection.PickOnField(BeastAttackStyle.HuntWeakest, candidates, random).Id;
                seen.Add(id);
                if (i == 0)
                {
                    firstId = id;
                }
            }

            Assert.That(seen.Count, Is.GreaterThan(1), "Hoà nhau mà luôn ra một người là thiên vị theo id");
            Assert.That(firstId, Is.EqualTo(1).Or.EqualTo(2).Or.EqualTo(3));
        }

        [Test]
        public void MotUngVien_ThiKhongRutSoNgauNhien()
        {
            List<Combatant> candidates = Field(MemberSpec(4, 1.0, 5));
            IRandom random = TestBattles.Random();
            string before = random.SaveState();

            Combatant picked = TargetSelection.PickOnField(BeastAttackStyle.Random, candidates, random);

            Assert.That(picked.Id, Is.EqualTo(4));
            Assert.That(random.SaveState(), Is.EqualTo(before),
                "Chỉ có một lựa chọn thì không được tiêu một lần rút — nếu không, ván lưu lại sẽ lệch");
        }

        [Test]
        public void KhongCoUngVien_ThiTraVeNull()
        {
            Assert.That(
                TargetSelection.PickOnField(BeastAttackStyle.Random, new List<Combatant>(), TestBattles.Random()),
                Is.Null);
        }

        // --- Chọn nạn nhân ngoài sân (người ở nhà) ----------------------------------------

        [Test]
        public void ChonNgoaiSan_SanKeYeu_DocHpGOC()
        {
            // Người ở nhà chưa hề bị gì, nên máu còn lại của họ đúng bằng chỉ số gốc.
            MemberList members = new MemberList(new[]
            {
                TestBattles.Member(1, hp: 7),
                TestBattles.Member(2, hp: 3),
                TestBattles.Member(3, hp: 5)
            });

            int? picked = TargetSelection.PickFromTribe(
                BeastAttackStyle.HuntWeakest, new[] { 1, 2, 3 }, members, TestBattles.Random());

            Assert.That(picked, Is.EqualTo(2));
        }

        [Test]
        public void ChonNgoaiSan_ThachDauKeManh_DocStrength()
        {
            MemberList members = new MemberList(new[]
            {
                TestBattles.Member(1, strength: 0.8),
                TestBattles.Member(2, strength: 1.2),
                TestBattles.Member(3, strength: 1.0)
            });

            int? picked = TargetSelection.PickFromTribe(
                BeastAttackStyle.ChallengeStrongest, new[] { 1, 2, 3 }, members, TestBattles.Random());

            Assert.That(picked, Is.EqualTo(2));
        }

        [Test]
        public void ChonNgoaiSan_DanhSachRong_ThiTraVeNull()
        {
            Assert.That(
                TargetSelection.PickFromTribe(
                    BeastAttackStyle.Random, new int[0], new MemberList(new Member[0]), TestBattles.Random()),
                Is.Null);
        }
    }
}
