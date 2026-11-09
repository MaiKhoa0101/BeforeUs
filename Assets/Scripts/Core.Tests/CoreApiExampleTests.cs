using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Neuron;
using BeforeUs.Core.Plants;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// Khóa ví dụ "một lượt trọn vẹn" ở docs/core-api.md §1.
    /// Nếu ai đó đổi chữ ký Core mà quên cập nhật tài liệu, test này gãy trước khi Lead UI đọc nhầm.
    /// Sửa test và tài liệu cùng lúc, đừng sửa riêng một bên.
    /// </summary>
    [TestFixture]
    public class CoreApiExampleTests
    {
        /// <summary>Bản giả của ICombatResolver đúng như docs/core-api.md §0 bảo nhánh UI tự viết tạm.</summary>
        private sealed class AlwaysWinsResolver : ICombatResolver
        {
            public CombatResult Resolve(CombatRequest request)
            {
                return CombatResult.Win();
            }
        }

        [Test]
        public void ViDuMotLuotTronVen_ChayDung()
        {
            // --- dựng ván mới (một lần mỗi ván) ---
            string json = TestPaths.ReadBalanceJson();
            BalanceConfig config = BalanceConfig.FromJson(json);

            IRandom random = new XorShift128Random(seed: 12345);
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(config));
            GameState state = GameState.NewGame(config, random, factory);

            NeuronData neuronData = NeuronNetwork.LoadData(TestPaths.ReadNeuronsJson());
            NeuronNetwork nodes = new NeuronNetwork(neuronData, config, state, applyNodeEffects: true);

            PlantCatalog plantCatalog = PlantCatalog.Load(TestPaths.ReadPlantsJson());
            ICombatResolver combat = new ProbabilisticCombatResolver(config, random, nodes);

            TurnSystem turn = new TurnSystem(
                config, state, random, factory,
                new Climate(config), new Difficulty(config),
                nodes, combat, events: null, odds: null, plants: plantCatalog);

            // --- mỗi lượt ---
            turn.BeginTurn();

            PlanResult r = turn.Plan.Assign(ActivityKind.Gather, 1, 2, 3);
            Assert.IsTrue(r.Succeeded, r.Message);

            turn.Plan.Assign(ActivityKind.Morale, 4);
            turn.Plan.Clear(ActivityKind.Morale);
            turn.Plan.Assign(ActivityKind.Gather, 1, 2, 3, 4);

            // Ván này dùng start.crew thật của balance.json, nên nó đi theo mỗi lần chỉnh cân bằng.
            Assert.AreEqual(state.Crew, turn.Plan.PartySizeOn(ActivityKind.Gather), "gán lại là thay thế, không cộng dồn");
            Assert.AreEqual(1, turn.Plan.EnergySpent, "một hoạt động, một năng lượng");

            int needed = turn.FoodNeeded;
            Assert.AreEqual(state.Crew, needed, "mỗi người × khẩu phần 1 ở giai đoạn 1");

            // v7: thức ăn hái lượm KHÔNG suy ra được từ k bằng phép nhân nào — nó là tổng
            // những lần rút trúng. "Chắc chắn có" chỉ còn là phần khung đảm bảo theo speed.
            GatherForecast forecast = turn.ForecastGather();
            Assert.AreEqual(forecast.Guaranteed, turn.ForecastFood,
                "ForecastFood = phần chắc chắn của hái lượm, cộng thức ăn sự kiện");
            Assert.GreaterOrEqual(forecast.Expected, forecast.Guaranteed,
                "kỳ vọng không bao giờ thấp hơn phần chắc chắn");

            TurnOutcome outcome = turn.EndTurn();

            Assert.AreEqual(1, outcome.Turn, "báo cáo là của lượt vừa kết thúc");
            Assert.AreEqual(2, state.Turn, "state.Turn đã tăng");
            Assert.AreEqual(0, turn.Plan.AssignedMemberCount, "dự định đã xóa sạch");

            // v7: thức ăn hái lượm là NGẪU NHIÊN, nên ví dụ này không khẳng định được "không đói".
            // Khẳng định cái chắc chắn đúng: thức ăn của lượt đến từ đúng lần Kiếm ăn đó.
            Assert.AreEqual(outcome.Gather.Food, outcome.FoodFromActivities);
            Assert.AreEqual(outcome.FoodTotal < outcome.FoodNeeded, outcome.Starved);
        }

        [Test]
        public void MoKhoaDungNhuTaiLieuMoTa()
        {
            // docs/core-api.md §3.
            BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson());
            IRandom random = new XorShift128Random(1);
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(config));
            GameState state = GameState.NewGame(config, random, factory);
            PurchasedNodeSet nodes = new PurchasedNodeSet();

            TurnSystem turn = new TurnSystem(
                config, state, random, factory,
                new Climate(config), new Difficulty(config),
                nodes, new AlwaysWinsResolver());

            Assert.IsTrue(turn.Plan.IsUnlocked(ActivityKind.Gather));
            Assert.IsTrue(turn.Plan.IsUnlocked(ActivityKind.Morale));
            Assert.IsFalse(turn.Plan.IsUnlocked(ActivityKind.Explore), "lượt 1 chưa tới lượt 6");
            Assert.IsFalse(turn.Plan.IsUnlocked(ActivityKind.Build), "lượt 1 chưa tới lượt 3 (D13)");
            Assert.IsFalse(turn.Plan.IsUnlocked(ActivityKind.Hunt), "lượt 1 chưa tới lượt 3 (D13)");

            state.Turn = 3;
            Assert.IsTrue(turn.Plan.IsUnlocked(ActivityKind.Build), "D13: mở từ lượt 3, không cần nút");
            Assert.IsTrue(turn.Plan.IsUnlocked(ActivityKind.Hunt));

            CollectionAssert.AreEqual(
                new[]
                {
                    ActivityKind.Gather, ActivityKind.Morale, ActivityKind.Build,
                    ActivityKind.Hunt, ActivityKind.Explore
                },
                ActivityRules.All);
        }

        [Test]
        public void BannerDoiPha_DungNhuTaiLieuMoTa()
        {
            // docs/core-api.md §5.
            BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson());
            Climate climate = new Climate(config);

            Assert.AreEqual(2, Climate.DefaultWarningTurns);
            Assert.IsFalse(climate.IsPhaseChangeImminent(8));
            Assert.IsTrue(climate.IsPhaseChangeImminent(9));
            Assert.AreEqual(2, climate.TurnsUntilPhaseChange(9));
            Assert.AreEqual(ClimatePhase.Wet, climate.PhaseAt(9));
        }

        [Test]
        public void ResolverQuyetAiChet_KeCaNguoiKhongDiTran()
        {
            // docs/core-api.md §6 — hai hệ quả được nêu trong tài liệu.
            TestGame g = TestGame.New(1, GameMode.Mvp).WithStage(2).AtTurn(3);   // D13: Săn bắt mở từ lượt 3
            int[] all = g.AllMemberIds();

            // Thắng nhưng vẫn chết một người, và người chết không nằm trong nhóm đi săn.
            g.Combat.ThenWin(all[4]);

            g.Plan.Assign(ActivityKind.Hunt, all[0], all[1]);
            g.Plan.Assign(ActivityKind.Gather, all[2], all[3]);
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.IsTrue(outcome.OutcomeOf(ActivityKind.Hunt).CombatWon.Value);
            Assert.IsNull(g.State.FindMember(all[4]), "người ở nhà vẫn chết theo quyết định của resolver");
            CollectionAssert.Contains(outcome.DeadMemberIds, all[4]);

            // Tinh thần: +3 thắng trận, +1 kiếm ăn, −2 vì có người chết (playtest 01, góp ý 1) = 2,
            // chưa đủ 4 để sinh. Trước khi có luật mất tinh thần khi chết thì lượt này sinh được một
            // người và sĩ số giữ nguyên 5; giờ thì không.
            Assert.AreEqual(-2, outcome.MoraleFromDeaths);
            Assert.AreEqual(2, g.State.Morale);
            Assert.AreEqual(0, outcome.BornMemberIds.Count);
            Assert.AreEqual(4, g.State.Crew);
        }

        [Test]
        public void TraitsHienLuonRong()
        {
            // docs/core-api.md §8 — Lead UI không được viết cứng tên đặc điểm.
            BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson());
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(config));
            GameState state = GameState.NewGame(config, new XorShift128Random(1), factory);

            foreach (Member member in state.Members)
            {
                Assert.AreEqual(0, member.Traits.Count);
            }
        }
    }
}
