using System;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// C1. Đối chiếu từng trường với Assets/Data/balance.json. Mục đích không phải "config nạp chạy được"
    /// mà là chốt số: nếu ai đó đổi một con số trong balance.json mà không qua Khoa, test này gãy.
    /// </summary>
    [TestFixture]
    public class BalanceConfigTests
    {
        private string _json;

        [SetUp]
        public void SetUp()
        {
            _json = TestPaths.ReadBalanceJson();
        }

        [Test]
        public void NapTheoModeGhiTrongFile_LaMvp()
        {
            BalanceConfig config = BalanceConfig.FromJson(_json);
            Assert.AreEqual("mvp", config.Data.Mode);
            Assert.AreEqual(GameMode.Mvp, config.Mode);
        }

        [Test]
        public void DocDungMoiTruong_CapCaoNhat()
        {
            BalanceData d = BalanceConfig.FromJson(_json).Data;

            Assert.AreEqual(4, d.Start.Crew);   // 5 -> 4, xem docs/balance-log.md
            Assert.AreEqual(0, d.Start.Region);
            Assert.AreEqual(1, d.Start.Stage);

            // design.md §3 [ĐỀ XUẤT], chuyển vào JSON ngày 2026-11-08.
            CollectionAssert.AreEqual(new[] { 0.8, 1.2 }, d.MemberStats.Speed);
            CollectionAssert.AreEqual(new[] { 0.8, 1.2 }, d.MemberStats.Strength);
            CollectionAssert.AreEqual(new[] { 3, 7 }, d.MemberStats.Hp);

            Assert.AreEqual(5, d.WinStage.Full);
            Assert.AreEqual(3, d.WinStage.Mvp);

            CollectionAssert.AreEqual(new[] { 4, 5, 6, 7 }, d.EnergyMax);
            CollectionAssert.AreEqual(new[] { 1, 2, 2, 3 }, d.UpkeepPerMember);
            Assert.AreEqual(1, d.StarvationDeaths);
            Assert.IsFalse(d.FoodCarriesOver);
            CollectionAssert.AreEqual(new[] { 1, 2, 2, 3, 3, 3 }, d.RaidQuotaByRegion);
        }

        [Test]
        public void DocDungMoiTruong_NamHoatDong()
        {
            ActivitiesData a = BalanceConfig.FromJson(_json).Data.Activities;

            Assert.AreEqual(1, a.Gather.Energy);
            Assert.AreEqual(1, a.Gather.NeuronPerMember);
            Assert.AreEqual("start", a.Gather.Unlock);
            Assert.AreEqual(1, a.Gather.MoraleOnSuccess);

            Assert.AreEqual(1, a.Morale.Energy);
            Assert.AreEqual(1, a.Morale.NeuronPerMember);
            Assert.AreEqual("start", a.Morale.Unlock);
            Assert.AreEqual(2, a.Morale.MoralePerMember);

            Assert.AreEqual(2, a.Build.Energy);
            Assert.AreEqual(2, a.Build.NeuronPerMember);
            Assert.AreEqual("turn:3", a.Build.Unlock);   // D13
            Assert.AreEqual(1, a.Build.MoraleOnSuccess);

            Assert.AreEqual(2, a.Hunt.Energy);
            Assert.AreEqual(6, a.Hunt.NeuronPerMember);
            Assert.AreEqual("turn:3", a.Hunt.Unlock);   // D13
            Assert.AreEqual(0.50, a.Hunt.BaseWin, 1e-12);

            Assert.AreEqual(3, a.Explore.Energy);
            Assert.AreEqual(6, a.Explore.NeuronPerMember);
            Assert.AreEqual("turn:6", a.Explore.Unlock);
            Assert.AreEqual(0.40, a.Explore.BaseWin, 1e-12);
        }

        [Test]
        public void DocDungMoiTruong_HaiLuomVaCombat()
        {
            BalanceData d = BalanceConfig.FromJson(_json).Data;

            // v7: công thức gather_yield đã bị xoá; số liệu hái lượm nay nằm ở plants.json.
            Assert.AreEqual(4, d.Gather.FireBonusFromStage, "giữ chỗ cho bếp lửa giai đoạn 4");
            Assert.AreEqual(1, d.Gather.FireBonus);
            Assert.AreEqual(1, d.Gather.FloorNumerator, "D6: sàn 1/3 bộ tộc phải đi kiếm ăn");
            Assert.AreEqual(3, d.Gather.FloorDenominator);
            Assert.AreEqual(0.15, d.Gather.DiscoverBase, 1e-12);
            Assert.AreEqual(0.05, d.Gather.DiscoverPerMember, 1e-12);
            Assert.AreEqual(0.60, d.Gather.DiscoverMax, 1e-12);
            Assert.AreEqual(0.30, d.Gather.SpeedRareBonus, 1e-12);

            Assert.AreEqual(0.08, d.CombatOdds.PerMember, 1e-12);
            Assert.AreEqual(0.05, d.CombatOdds.PerBuildLevel, 1e-12);
            Assert.AreEqual(0.05, d.CombatOdds.Min, 1e-12);
            Assert.AreEqual(0.92, d.CombatOdds.Max, 1e-12);

            CollectionAssert.AreEqual(new[] { 0, 5, 7, 10 }, d.CombatResults.HuntFoodPerMember);
            Assert.AreEqual(3, d.CombatResults.MoraleOnWin);
            Assert.AreEqual(-2, d.CombatResults.MoraleOnLoss);
            Assert.AreEqual(0.5, d.CombatResults.WinDeathChanceAvg, 1e-12);
            Assert.AreEqual(2, d.CombatResults.LossDeaths);

            Assert.AreEqual(0.35, d.DefenceOdds.Base, 1e-12);
            Assert.AreEqual(0.05, d.DefenceOdds.PerMember, 1e-12);
            Assert.AreEqual(0.06, d.DefenceOdds.PerBuildLevel, 1e-12);
            Assert.AreEqual(0.04, d.DefenceOdds.PerStage, 1e-12);
            Assert.AreEqual(-0.05, d.DefenceOdds.PerRegion, 1e-12);
            Assert.AreEqual(0.05, d.DefenceOdds.Min, 1e-12);
            Assert.AreEqual(0.95, d.DefenceOdds.Max, 1e-12);
        }

        [Test]
        public void DocDungMoiTruong_SinhSanTienHoaSuKien()
        {
            BalanceData d = BalanceConfig.FromJson(_json).Data;

            Assert.AreEqual(4, d.Birth.MoraleCost);
            Assert.AreEqual(1, d.Birth.MaxPerTurn);

            CollectionAssert.AreEqual(new[] { 85, 135, 190, 240 }, d.Evolution.RequiredNeuron.Full);
            CollectionAssert.AreEqual(new[] { 150, 250, 190, 240 }, d.Evolution.RequiredNeuron.Mvp);

            Assert.AreEqual(4, d.Evolution.Feats.Length);
            AssertFeat(d.Evolution.Feats[0], 1, 1, 0, 0);
            AssertFeat(d.Evolution.Feats[1], 2, 0, 2, 0);
            AssertFeat(d.Evolution.Feats[2], 3, 0, 2, 1);
            AssertFeat(d.Evolution.Feats[3], 4, 0, 3, 0);

            Assert.AreEqual(5, d.Events.GraceTurns);
            Assert.AreEqual(3, d.Events.TutorialTurn);
            Assert.AreEqual(2, d.Events.TutorialReward.Morale);
            Assert.AreEqual(3, d.Events.TutorialReward.Neuron);
            Assert.AreEqual(1, d.Events.TutorialReward.CombatWins);
            Assert.AreEqual(0.7, d.Events.ChancePerTurn, 1e-12);
            Assert.AreEqual(0.4, d.Events.ShareNoncombat, 1e-12);
            Assert.AreEqual(0.20, d.Events.ShareRaid.Wet, 1e-12);
            Assert.AreEqual(0.25, d.Events.ShareRaid.Dry, 1e-12);
            Assert.AreEqual(4, d.Events.NoncombatGood.Food);
            Assert.AreEqual(2, d.Events.NoncombatGood.Morale);
            Assert.AreEqual(-3, d.Events.NoncombatBad.Food);
            Assert.AreEqual(-2, d.Events.NoncombatBad.Morale);
            Assert.AreEqual(6, d.Events.OpportunityWin.Food);
            Assert.AreEqual(3, d.Events.OpportunityWin.Morale);
            Assert.AreEqual(4, d.Events.OpportunityWin.Neuron);
            Assert.AreEqual(2, d.Events.RaidWin.Morale);
            Assert.AreEqual(3, d.Events.RaidWin.Neuron);
        }

        [Test]
        public void DocDungMoiTruong_KhiHauDoKhoVung()
        {
            BalanceData d = BalanceConfig.FromJson(_json).Data;

            Assert.AreEqual(10, d.Climate.PhaseLength);
            Assert.AreEqual("wet", d.Climate.FirstPhase);

            Assert.AreEqual(20, d.Difficulty.PeriodTurns.Full);
            Assert.AreEqual(12, d.Difficulty.PeriodTurns.Mvp);
            Assert.AreEqual(-0.02, d.Difficulty.PerLevel.CombatOdds, 1e-12);
            Assert.AreEqual(1, d.Difficulty.PerLevel.RaidQuota);
            Assert.AreEqual(-1, d.Difficulty.PerLevel.BadEventFood);
            Assert.AreEqual(-1, d.Difficulty.PerLevel.BadEventMorale);
            Assert.AreEqual(-0.18, d.Difficulty.PerLevel.FindChanceMultiplier, 1e-12);   // xem balance-log.md #10

            Assert.AreEqual(5, d.Regions.MaxIndex.Full);
            Assert.AreEqual(1, d.Regions.MaxIndex.Mvp);
            CollectionAssert.AreEqual(
                new[] { "Rừng nhiệt đới", "Savan", "Sa mạc", "Đồng bằng sông Nile" },
                d.Regions.Names);
        }

        [Test]
        public void ChonMode_DoiDungBonTruongTachTheoMode()
        {
            BalanceConfig mvp = BalanceConfig.FromJson(_json, GameMode.Mvp);
            BalanceConfig full = BalanceConfig.FromJson(_json, GameMode.Full);

            Assert.AreEqual(3, mvp.WinStage);
            Assert.AreEqual(5, full.WinStage);

            CollectionAssert.AreEqual(new[] { 150, 250, 190, 240 }, mvp.RequiredNeuron);
            CollectionAssert.AreEqual(new[] { 85, 135, 190, 240 }, full.RequiredNeuron);

            Assert.AreEqual(12, mvp.DifficultyPeriodTurns);
            Assert.AreEqual(20, full.DifficultyPeriodTurns);

            Assert.AreEqual(1, mvp.RegionMaxIndex);
            Assert.AreEqual(5, full.RegionMaxIndex);
        }

        [Test]
        public void TongGiaNutBatBuoc_KhopRequiredNeuron()
        {
            // design.md §7.3–7.4: mỗi giai đoạn có 3 nút gốc + 3 nút cổng + nút Tiến hóa.
            // Tổng giá đúng bằng required_neuron, nên mô hình mua-từng-nút của C5 tái tạo được
            // chi phí phẳng của before_us_sim.py mà không cần giả định thêm.
            BalanceConfig mvp = BalanceConfig.FromJson(_json, GameMode.Mvp);
            BalanceConfig full = BalanceConfig.FromJson(_json, GameMode.Full);

            Assert.AreEqual(3 * 18 + 3 * 27 + 15, mvp.RequiredNeuronForStage(1), "giai đoạn 1 MVP");
            Assert.AreEqual(3 * 28 + 3 * 42 + 40, mvp.RequiredNeuronForStage(2), "giai đoạn 2 MVP");
            Assert.AreEqual(3 * 10 + 3 * 15 + 10, full.RequiredNeuronForStage(1), "giai đoạn 1 bản đầy đủ");
            Assert.AreEqual(3 * 15 + 3 * 25 + 15, full.RequiredNeuronForStage(2), "giai đoạn 2 bản đầy đủ");
        }

        [Test]
        public void TraCuuTheoGiaiDoan()
        {
            BalanceConfig c = BalanceConfig.FromJson(_json, GameMode.Mvp);

            Assert.AreEqual(4, c.EnergyMaxForStage(1));
            Assert.AreEqual(7, c.EnergyMaxForStage(4));
            Assert.AreEqual(1, c.UpkeepForStage(1));
            Assert.AreEqual(3, c.UpkeepForStage(4));
            Assert.AreEqual(5, c.HuntFoodForStage(2));
            Assert.AreEqual(10, c.HuntFoodForStage(4));

            Assert.AreEqual(2, c.FeatsForStage(2).Hunts);
            Assert.AreEqual(1, c.FeatsForStage(3).MigratedEver);

            Assert.Throws<ArgumentOutOfRangeException>(() => c.EnergyMaxForStage(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => c.EnergyMaxForStage(5));
        }

        [Test]
        public void QuotaVungVuotBang_DungGiaTriCuoi()
        {
            BalanceConfig c = BalanceConfig.FromJson(_json, GameMode.Full);
            Assert.AreEqual(1, c.RaidQuotaForRegion(0));
            Assert.AreEqual(2, c.RaidQuotaForRegion(1));
            Assert.AreEqual(3, c.RaidQuotaForRegion(5));
            Assert.AreEqual(3, c.RaidQuotaForRegion(9), "vùng ngoài bảng dùng giá trị cuối");
        }

        [Test]
        public void MoKhoaHoatDong_PhanTichDungChuoi()
        {
            BalanceConfig c = BalanceConfig.FromJson(_json);

            Assert.AreEqual(UnlockKind.Start, c.UnlockOf(ActivityKind.Gather).Kind);
            Assert.AreEqual(UnlockKind.Start, c.UnlockOf(ActivityKind.Morale).Kind);

            // D13 (Khoa quyết 2026-11-08): Xây dựng và Săn bắt mở từ lượt 3, bỏ hẳn điều kiện nút.
            // Thay cho D1 cũ, vốn bắt phải mua s2_stone_tools và s2_scavenge_meat trước.
            UnlockRule build = c.UnlockOf(ActivityKind.Build);
            Assert.AreEqual(UnlockKind.Turn, build.Kind);
            Assert.AreEqual(3, build.FromTurn);

            UnlockRule hunt = c.UnlockOf(ActivityKind.Hunt);
            Assert.AreEqual(UnlockKind.Turn, hunt.Kind);
            Assert.AreEqual(3, hunt.FromTurn);

            UnlockRule explore = c.UnlockOf(ActivityKind.Explore);
            Assert.AreEqual(UnlockKind.Turn, explore.Kind);
            Assert.AreEqual(6, explore.FromTurn);
        }

        [Test]
        public void NangLuongVaNeuron_TraCuuTheoHoatDong()
        {
            BalanceConfig c = BalanceConfig.FromJson(_json);

            Assert.AreEqual(1, c.EnergyCostOf(ActivityKind.Gather));
            Assert.AreEqual(2, c.EnergyCostOf(ActivityKind.Build));
            Assert.AreEqual(3, c.EnergyCostOf(ActivityKind.Explore));

            Assert.AreEqual(1, c.NeuronPerMemberOf(ActivityKind.Morale));
            Assert.AreEqual(6, c.NeuronPerMemberOf(ActivityKind.Hunt));
        }

        [Test]
        public void ThemKhoaLaDaVaoJson_ThiNapHong()
        {
            // Bảo hiểm chống trôi số: nếu balance.json mọc thêm một khóa mà DTO chưa biết,
            // Core phải nổ chứ không được âm thầm bỏ qua.
            string tampered = _json.Replace("\"mode\": \"mvp\"", "\"mode\": \"mvp\",\n  \"khoa_la\": 1");
            Assert.AreNotEqual(_json, tampered, "chuỗi mẫu phải thật sự đổi");
            Assert.Throws<BalanceConfigException>(() => BalanceConfig.FromJson(tampered));
        }

        [Test]
        public void ThieuKhoa_ThiNapHong()
        {
            string tampered = _json.Replace("\"starvation_deaths\": 1,", string.Empty);
            Assert.AreNotEqual(_json, tampered, "chuỗi mẫu phải thật sự đổi");
            Assert.Throws<BalanceConfigException>(() => BalanceConfig.FromJson(tampered));
        }

        [Test]
        public void JsonRong_ThiNapHong()
        {
            Assert.Throws<BalanceConfigException>(() => BalanceConfig.FromJson(string.Empty));
            Assert.Throws<BalanceConfigException>(() => BalanceConfig.FromJson("{ khong phai json"));
        }

        [Test]
        public void ModeKhongHopLe_ThiNapHong()
        {
            string tampered = _json.Replace("\"mode\": \"mvp\"", "\"mode\": \"sieu-de\"");
            Assert.Throws<ArgumentException>(() => BalanceConfig.FromJson(tampered));
        }

        private static void AssertFeat(FeatData feat, int fromStage, int wins, int hunts, int migrated)
        {
            Assert.AreEqual(fromStage, feat.FromStage, "from_stage");
            Assert.AreEqual(wins, feat.CombatWins, "combat_wins giai đoạn " + fromStage);
            Assert.AreEqual(hunts, feat.Hunts, "hunts giai đoạn " + fromStage);
            Assert.AreEqual(migrated, feat.MigratedEver, "migrated_ever giai đoạn " + fromStage);
        }
    }
}
