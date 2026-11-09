using System.Collections.Generic;
using System.IO;
using BeforeUs.Core;
using NUnit.Framework;

namespace BeforeUs.Combat.Tests
{
    /// <summary>
    /// Chạy trên đúng nội dung Assets/Data/beasts.json, không phải trên dữ liệu dựng sẵn:
    /// ai sửa file đó mà làm sai danh sách §9 của Khoa thì test này đổ.
    /// </summary>
    [TestFixture]
    public sealed class BeastCatalogTests
    {
        private BeastCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _catalog = new BeastCatalog(BeastCatalog.LoadData(File.ReadAllText(TestBattles.BeastsJsonPath)));
        }

        [Test]
        public void DungBaLoaiVaBaConMoi_CuaDesignMd_KhongThemLoaiNao()
        {
            // design.md §9 [ĐỀ XUẤT] của Khoa. Agent không được bịa thêm loài (HANDOFF.md §6).
            List<string> ids = new List<string>();
            foreach (BeastSpecies species in _catalog.Species)
            {
                ids.Add(species.Id);
            }

            Assert.That(ids, Is.EquivalentTo(new[]
            {
                "leopard", "hyena_pack", "sabertooth",
                "small_antelope", "boar_piglet", "hare"
            }));
        }

        [Test]
        public void KieuTanCong_DungNhuBangDesignMd()
        {
            Assert.That(_catalog.Find("leopard").AttackStyle, Is.EqualTo(BeastAttackStyle.HuntWeakest));
            Assert.That(_catalog.Find("hyena_pack").AttackStyle, Is.EqualTo(BeastAttackStyle.Random));
            Assert.That(_catalog.Find("sabertooth").AttackStyle, Is.EqualTo(BeastAttackStyle.ChallengeStrongest));
        }

        [Test]
        public void Vung_DungNhuBangDesignMd()
        {
            Assert.That(_catalog.Find("leopard").LivesIn(0), Is.True, "Báo hoa mai: rừng nhiệt đới");
            Assert.That(_catalog.Find("hyena_pack").LivesIn(1), Is.True, "Linh cẩu: savan");
            Assert.That(_catalog.Find("sabertooth").LivesIn(1), Is.True, "Hổ răng kiếm: savan");
        }

        [Test]
        public void ConMoiGiaiDoan2_ChiDungChoSanBat()
        {
            foreach (string id in new[] { "small_antelope", "boar_piglet", "hare" })
            {
                BeastSpecies prey = _catalog.Find(id);
                Assert.That(prey.PlaysRole(CombatKind.Hunt), Is.True, id);
                Assert.That(prey.PlaysRole(CombatKind.Defence), Is.False, id + " không tấn công hang");
            }
        }

        [Test]
        public void HoRangKiem_ChiXuatHienOCoHoiCombat()
        {
            // design.md §9: "Nguy hiểm nhất, cơ hội combat thưởng lớn".
            BeastSpecies tiger = _catalog.Find("sabertooth");

            Assert.That(tiger.PlaysRole(CombatKind.Opportunity), Is.True);
            Assert.That(tiger.PlaysRole(CombatKind.Defence), Is.False);
            Assert.That(tiger.PlaysRole(CombatKind.Hunt), Is.False);
        }

        [Test]
        public void ChonLoai_SanBat_RaConMoi_KhongRaQuaiThu()
        {
            IRandom random = TestBattles.Random();
            for (int i = 0; i < 50; i++)
            {
                BeastSpecies picked = _catalog.Choose(CombatKind.Hunt, 0, random);
                Assert.That(picked.PlaysRole(CombatKind.Hunt), Is.True);
                Assert.That(
                    picked.Id,
                    Is.EqualTo("small_antelope").Or.EqualTo("boar_piglet").Or.EqualTo("hare"));
            }
        }

        [Test]
        public void ChonLoai_PhongThuOVung0_RaBaoHoaMai()
        {
            Assert.That(
                _catalog.Choose(CombatKind.Defence, 0, TestBattles.Random()).Id,
                Is.EqualTo("leopard"));
        }

        [Test]
        public void ChonLoai_KhamPha_DoiMatThuCuaVungSAPTOI()
        {
            // [ĐỀ XUẤT]: Khám phá là "chiếm vùng mới" (design.md §5), nên kẻ cản đường
            // là chủ của vùng mới, không phải vùng đang ở.
            Assert.That(
                _catalog.Choose(CombatKind.Explore, 0, TestBattles.Random()).Id,
                Is.EqualTo("hyena_pack"),
                "Từ rừng nhiệt đới đi sang savan thì gặp thú savan");
        }

        [Test]
        public void ChonLoai_KhamPhaOVungCuoi_ThiLuiVeVungHienTai()
        {
            // MVP tối đa 2 vùng; vùng 2 không có loài nào. Không được nổ.
            Assert.That(
                _catalog.Choose(CombatKind.Explore, 1, TestBattles.Random()).Id,
                Is.EqualTo("hyena_pack"));
        }

        [Test]
        public void ChonLoai_TranHuongDan_ThiNoNgay()
        {
            // design.md §8: trận hướng dẫn luôn thắng, không vào sân.
            Assert.Throws<BeastDataException>(
                () => _catalog.TemplateFor(CombatKind.Tutorial));
        }

        [Test]
        public void MauTran_DungKieuTranTheoBangDesignMd()
        {
            Assert.That(_catalog.TemplateFor(CombatKind.Hunt).BattleKind, Is.EqualTo(BattleKindName.Lure));
            Assert.That(_catalog.TemplateFor(CombatKind.Opportunity).BattleKind, Is.EqualTo(BattleKindName.Lure));
            Assert.That(_catalog.TemplateFor(CombatKind.Defence).BattleKind, Is.EqualTo(BattleKindName.Prevent));
            Assert.That(_catalog.TemplateFor(CombatKind.Explore).BattleKind, Is.EqualTo(BattleKindName.Prevent));
        }

        [Test]
        public void ThoiLuongTran_NamTrongDai20Den40Giay()
        {
            foreach (CombatKind kind in new[]
            {
                CombatKind.Hunt, CombatKind.Explore, CombatKind.Defence, CombatKind.Opportunity
            })
            {
                double seconds = _catalog.TemplateFor(kind).TimeLimitSeconds;
                Assert.That(seconds, Is.InRange(20.0, 40.0), kind + ": design.md §9 cho dải 20–40 giây");
            }
        }

        [Test]
        public void MoiLoai_DeuCoChuHienChoNguoiChoi()
        {
            foreach (BeastSpecies species in _catalog.Species)
            {
                Assert.That(species.Data.Name, Is.Not.Empty, species.Id);
                Assert.That(species.Data.Description, Is.Not.Empty, species.Id);
                Assert.That(species.Data.AttackStyleText, Is.Not.Empty, species.Id);
            }
        }

        [Test]
        public void ChuHienChoNguoiChoi_KhongChepTenBien()
        {
            // Bài học từ neurons.json: 20 câu phải viết lại vì chúng chép nguyên tên biến
            // ("gather_yield +1") ra màn hình người chơi.
            string[] codeSmells =
            {
                "_", "hp", "attack_", "move_speed", "pack_size", "hunt_weakest",
                "challenge_strongest", "attack_style", "CombatKind", "null"
            };

            foreach (BeastSpecies species in _catalog.Species)
            {
                foreach (string text in new[]
                {
                    species.Data.Name, species.Data.Description, species.Data.AttackStyleText
                })
                {
                    foreach (string smell in codeSmells)
                    {
                        Assert.That(text.ToLowerInvariant(), Does.Not.Contain(smell),
                            species.Id + ": chữ hiện cho người chơi lọt tên biến \"" + smell + "\" — \"" + text + "\"");
                    }
                }
            }
        }

        [Test]
        public void KieuTanCongLa_ThiNapHongNgay()
        {
            string json = File.ReadAllText(TestBattles.BeastsJsonPath)
                .Replace("\"hunt_weakest\"", "\"an_thit_nguoi\"");

            Assert.Throws<BeastDataException>(() => new BeastCatalog(BeastCatalog.LoadData(json)));
        }

        [Test]
        public void ThieuTruong_ThiNapHongNgay()
        {
            Assert.Throws<BeastDataException>(
                () => BeastCatalog.LoadData("{ \"species\": [] }"));
        }

        [Test]
        public void ThuaTruongLa_ThiNapHongNgay()
        {
            string json = File.ReadAllText(TestBattles.BeastsJsonPath)
                .Replace("\"species\": [", "\"mon_an_vat\": 3, \"species\": [");

            Assert.Throws<BeastDataException>(() => BeastCatalog.LoadData(json));
        }
    }
}
