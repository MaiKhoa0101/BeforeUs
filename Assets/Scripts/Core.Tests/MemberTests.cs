using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C2. Member, MemberFactory và GameState.</summary>
    [TestFixture]
    public class MemberTests
    {
        private BalanceConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
        }

        [Test]
        public void ChiSoNamTrongDaiDeXuat()
        {
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(_config));
            IRandom rng = new XorShift128Random(11);

            for (int i = 0; i < 5000; i++)
            {
                Member m = factory.Create(rng);
                Assert.That(m.Speed, Is.InRange(0.8, 1.2));
                Assert.That(m.Strength, Is.InRange(0.8, 1.2));
                Assert.That(m.Hp, Is.InRange(3, 7));
            }
        }

        [Test]
        public void IdTangDan_KhongTrung()
        {
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(_config));
            IRandom rng = new XorShift128Random(1);
            HashSet<int> ids = new HashSet<int>();

            for (int i = 0; i < 200; i++)
            {
                Assert.IsTrue(ids.Add(factory.Create(rng).Id), "id trùng ở lần thứ " + i);
            }

            Assert.AreEqual(201, factory.NextId);
        }

        [Test]
        public void CheDoTrungBinh_MoiChiSoBang1_VaKhongTieuSoNgauNhien()
        {
            // design.md §3: "mô phỏng dùng giá trị trung bình". C8 cần đúng điều này.
            MemberFactory factory = new MemberFactory(MemberStatRanges.AverageFromConfig(_config));
            XorShift128Random rng = new XorShift128Random(7);
            string before = rng.SaveState();

            for (int i = 0; i < 10; i++)
            {
                Member m = factory.Create(rng);
                Assert.AreEqual(1.0, m.Speed, 1e-12);
                Assert.AreEqual(1.0, m.Strength, 1e-12);
                Assert.AreEqual(5, m.Hp);
            }

            Assert.AreEqual(before, rng.SaveState(),
                "dải một điểm thì không được rút số: rút để lấy về hằng số là việc thừa");
        }

        [Test]
        public void DacDiemNgoaiHinh_RutTuBoDuocCapVao()
        {
            TraitCategory[] pool =
            {
                new TraitCategory("toc", new[] { "thang", "xoan" }),
                new TraitCategory("da", new[] { "sang", "toi" })
            };

            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(_config), pool);
            IRandom rng = new XorShift128Random(3);

            Member m = factory.Create(rng);
            Assert.AreEqual(2, m.Traits.Count);
            StringAssert.StartsWith("toc:", m.Traits[0]);
            StringAssert.StartsWith("da:", m.Traits[1]);
        }

        [Test]
        public void KhongCapBoDacDiem_ThiDanhSachRong()
        {
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(_config));
            Member m = factory.Create(new XorShift128Random(1));
            Assert.AreEqual(0, m.Traits.Count);
        }

        [Test]
        public void CungSeed_ChoCungBoThanhVien()
        {
            MemberFactory a = new MemberFactory(MemberStatRanges.FromConfig(_config));
            MemberFactory b = new MemberFactory(MemberStatRanges.FromConfig(_config));
            IRandom ra = new XorShift128Random(321);
            IRandom rb = new XorShift128Random(321);

            for (int i = 0; i < 50; i++)
            {
                Member ma = a.Create(ra);
                Member mb = b.Create(rb);
                Assert.AreEqual(ma.Id, mb.Id);
                Assert.AreEqual(ma.Speed, mb.Speed, 0.0);
                Assert.AreEqual(ma.Strength, mb.Strength, 0.0);
                Assert.AreEqual(ma.Hp, mb.Hp);
            }
        }

        [Test]
        public void VanMoi_DungSoLieuKhoiDau()
        {
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(_config));
            GameState state = GameState.NewGame(_config, new XorShift128Random(9), factory);

            Assert.AreEqual(1, state.Turn);
            Assert.AreEqual(1, state.Stage);
            Assert.AreEqual(0, state.Region);
            Assert.AreEqual(4, state.Crew);
            Assert.AreEqual(0, state.Neuron);
            Assert.AreEqual(0, state.Morale);
            Assert.AreEqual(0, state.BuildLevel);
            Assert.AreEqual(0, state.ClearedSiteIds.Count);
            Assert.AreEqual(0, state.FoodNestTier);
            Assert.AreEqual(0, state.CombatWins);
            Assert.AreEqual(0, state.Hunts);
            Assert.AreEqual(0, state.MigratedEver);
            Assert.AreEqual(5, state.NextMemberId);
        }

        [Test]
        public void SucChuaHangVaKhauPhan_TheoCongThucDesign()
        {
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(_config));
            GameState state = GameState.NewGame(_config, new XorShift128Random(9), factory);

            // v7 §F.2: sức chứa nay theo VÙNG, không còn 6 + 2 × build_level.
            // GameState.NewGame chưa biết sites.json nên hai số này còn 0; TurnSystem điền khi dựng ván.
            state.RegionCapacityOpen = 4;
            state.RegionCapacityMax = 8;
            Assert.AreEqual(4, state.LairCapacity(_config), "chỗ mở sẵn của vùng 0");

            state.ClearedSiteIds.Add("nhanh_cay_gay");
            Assert.AreEqual(5, state.LairCapacity(_config), "mỗi công trình dọn xong mở thêm một chỗ");

            for (int i = 0; i < 10; i++)
            {
                state.ClearedSiteIds.Add("gia_" + i);
            }

            Assert.AreEqual(8, state.LairCapacity(_config), "không bao giờ vượt trần của vùng");

            // design.md §4: khẩu phần giai đoạn 1 là 1 mỗi người. start.crew nay là 4.
            Assert.AreEqual(4, state.FoodNeeded(_config));
            state.Stage = 3;
            Assert.AreEqual(8, state.FoodNeeded(_config));

            Assert.AreEqual(6, state.EnergyMax(_config));
        }

        [Test]
        public void TimThanhVienTheoId()
        {
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(_config));
            GameState state = GameState.NewGame(_config, new XorShift128Random(9), factory);

            Assert.IsNotNull(state.FindMember(1));
            Assert.AreEqual(1, state.FindMember(1).Id);
            Assert.IsNull(state.FindMember(999));
        }

        [Test]
        public void ChiSoKhongHopLe_ThiBaoLoi()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Member(0, 1, 1, 5, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Member(1, 1, 1, 0, null));
            Assert.Throws<ArgumentException>(() => new MemberStatRanges(1.2, 0.8, 1, 1, 3, 7));
            Assert.Throws<ArgumentException>(() => new TraitCategory("toc", new string[0]));
        }
    }
}
