using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// Luật mới từ playtest 01 (docs/playtest-01.md, docs/decisions.md D14):
    /// mất tinh thần khi có người chết, thua khi còn 1 người, và truy vấn xem trước trận.
    /// </summary>
    [TestFixture]
    public class PlaytestRulesTests
    {
        // ---------- D14a: mất tinh thần khi có thành viên chết (góp ý 1) ----------

        [Test]
        public void ChetVIDoi_ThiMatTinhThan()
        {
            // Tinh thần đặt 3 có chủ đích: dưới birth.morale_cost (4) nên KHÔNG sinh ai cuối lượt,
            // để phép trừ vì cái chết đứng một mình. Đặt 10 thì 10 − 2 = 8 vẫn đủ sinh,
            // và kết quả sẽ là 4 — đúng luật, nhưng không còn cô lập được thứ đang đo.
            // Giai đoạn 3 (khẩu phần 2/người): sàn 2 người mang về 6, cả bộ tộc cần 10 -> đói.
            TestGame g = TestGame.New().WithStage(3).MeetGatherFloor();
            g.State.Morale = 3;

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.IsTrue(outcome.Starved);
            Assert.AreEqual(1, outcome.StarvationDeaths);
            Assert.AreEqual(-2, outcome.MoraleFromDeaths);
            Assert.AreEqual(0, outcome.BornMemberIds.Count, "2 tinh thần chưa đủ sinh");
            Assert.AreEqual(2, g.State.Morale, "3 + 1 (kiếm ăn) − 2 (chết) = 2");
        }

        [Test]
        public void ChetTrongCombat_ThiMatTinhThan()
        {
            TestGame g = TestGame.New().WithStage(2).AtTurn(3);
            g.State.Morale = 10;
            int[] all = g.AllMemberIds();
            g.Combat.ThenLose(all[0], all[1]);

            g.Plan.Assign(ActivityKind.Hunt, all[0]);
            g.Plan.Assign(ActivityKind.Gather, new[] { all[2], all[3], all[4] });
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(-4, outcome.MoraleFromDeaths, "hai người chết, mỗi người −2");
        }

        [Test]
        public void ChetVISuKien_ThiMatTinhThan()
        {
            TestGame g = TestGame.New();
            g.State.Morale = 10;
            int victim = g.MemberIds(1)[0];
            g.Events.OnMachineTurn = ctx => ctx.KillMember(victim);

            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(-2, outcome.MoraleFromDeaths);
        }

        [Test]
        public void KhongAiChet_ThiKhongMatTinhThan()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());

            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(0, outcome.MoraleFromDeaths);
        }

        [Test]
        public void MatTinhThanVIChet_VanChanODuoi0()
        {
            // design.md §4: tinh thần không âm.
            TestGame g = TestGame.New().WithStage(3).MeetGatherFloor();
            g.State.Morale = 0;

            g.Turn.EndTurn();

            Assert.AreEqual(0, g.State.Morale);
        }

        // ---------- D14b: thua khi còn 1 người (góp ý 8) ----------

        [Test]
        public void NguongThuaDocTuBalanceJson()
        {
            TestGame g = TestGame.New();
            Assert.AreEqual(1, g.Config.LoseWhenCrewAtOrBelow);
        }

        [Test]
        public void ConMotNguoi_ThiThua()
        {
            TestGame g = TestGame.New().WithStage(2).AtTurn(3);
            int[] all = g.AllMemberIds();
            g.Combat.ThenLose(all[0], all[1], all[2], all[3]);   // còn 1 người

            g.Plan.Assign(ActivityKind.Hunt, all[0]);
            g.Plan.Assign(ActivityKind.Gather, new[] { all[3], all[4] });   // đạt sàn, và người sống sót đủ ăn
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(1, outcome.CrewAfter);
            Assert.IsTrue(outcome.Lost, "còn 1 người là thua");
            Assert.IsFalse(outcome.TribeWipedOut, "nhưng chưa phải hết sạch người");
        }

        [Test]
        public void ConHaiNguoi_ThiChuaThua()
        {
            TestGame g = TestGame.New().WithStage(2).AtTurn(3);
            int[] all = g.AllMemberIds();
            g.Combat.ThenLose(all[0], all[1], all[2]);   // còn 2 người

            g.Plan.Assign(ActivityKind.Hunt, all[0]);
            g.Plan.Assign(ActivityKind.Gather, new[] { all[3], all[4] });
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(2, outcome.CrewAfter);
            Assert.IsFalse(outcome.Lost);
        }

        [Test]
        public void HetSachNguoi_VanLaThua()
        {
            TestGame g = TestGame.New().WithStage(2).AtTurn(3);
            g.Combat.ThenLose(g.AllMemberIds());

            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(1));
            g.MeetGatherFloor();
            TurnOutcome outcome = g.Turn.EndTurn();

            Assert.AreEqual(0, outcome.CrewAfter);
            Assert.IsTrue(outcome.Lost);
            Assert.IsTrue(outcome.TribeWipedOut);
        }

        // ---------- D14c: D11 phải theo ngưỡng thua ----------

        [Test]
        public void SoNguoiPhongThuChuaLai_SuyRaTuNguongThua_KhongVietCung()
        {
            TestGame g = TestGame.New();
            Assert.AreEqual(
                g.Config.LoseWhenCrewAtOrBelow + 1,
                g.Config.MinSurvivorsAfterForcedCombat,
                "một trận không né được không bao giờ được thành cú thua tức thì");
        }

        // ---------- góp ý 3: xem trước trận ----------

        [Test]
        public void XemTruocTran_ChuaCuAi_ThiRong()
        {
            TestGame g = TestGame.New().WithStage(2).AtTurn(3);

            CombatPreview preview = g.Turn.PreviewCombat(ActivityKind.Hunt);
            Assert.IsFalse(preview.HasParty);
            Assert.AreEqual(0, preview.FoodIfWon);
        }

        [Test]
        public void XemTruocTran_ThucAnNeuThang()
        {
            // design.md §5.2: giai đoạn 2 cho 5 thức ăn mỗi người.
            TestGame g = TestGame.New().WithStage(2).AtTurn(3);
            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(3));

            CombatPreview preview = g.Turn.PreviewCombat(ActivityKind.Hunt);

            Assert.AreEqual(3, preview.PartySize);
            Assert.AreEqual(15, preview.FoodIfWon);
        }

        [Test]
        public void XemTruocTran_GiaiDoan1_ThucAnBang0()
        {
            // hunt_food_per_member[0] = 0 — đúng dữ liệu hiện tại, Khoa sẽ quyết sau khi chơi.
            TestGame g = TestGame.New().AtTurn(3);
            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(3));

            Assert.AreEqual(0, g.Turn.PreviewCombat(ActivityKind.Hunt).FoodIfWon);
        }

        [Test]
        public void XemTruocTran_KhongCoNguonUocLuong_ThiWinChanceLaNull()
        {
            // TestGame dùng ScriptedCombatResolver, vốn không biết tính xác suất.
            // UI phải hiện "chưa rõ" chứ KHÔNG được tự tính lấy.
            TestGame g = TestGame.New().WithStage(2).AtTurn(3);
            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(2));

            Assert.IsNull(g.Turn.PreviewCombat(ActivityKind.Hunt).WinChance);
        }

        [Test]
        public void XemTruocTran_TiLeThangLayTuCungMotCongThucVoiLucGiaiQuyet()
        {
            BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
            XorShift128Random random = new XorShift128Random(5);
            MemberFactory factory = new MemberFactory(MemberStatRanges.AverageFromConfig(config));
            GameState state = GameState.NewGame(config, random, factory);
            state.Stage = 2;
            state.Turn = 3;

            ProbabilisticCombatResolver combat = new ProbabilisticCombatResolver(config, random);
            TurnSystem turn = new TurnSystem(
                config, state, random, factory,
                new Climate(config), new Difficulty(config),
                NoPurchasedNodes.Instance, combat);

            turn.Plan.Assign(ActivityKind.Hunt, 1, 2, 3);
            CombatPreview preview = turn.PreviewCombat(ActivityKind.Hunt);

            Assert.IsNotNull(preview.WinChance);

            // design.md §5.2: 0.50 + 0.08 × 3 với build 0 và độ khó 0.
            Assert.AreEqual(0.50 + 0.08 * 3, preview.WinChance.Value, 1e-12);
        }

        [Test]
        public void XemTruocTran_KhongCoTacDungPhu()
        {
            TestGame g = TestGame.New().WithStage(2).AtTurn(3);
            g.Plan.Assign(ActivityKind.Hunt, g.MemberIds(2));

            int moraleBefore = g.State.Morale;
            int crewBefore = g.State.Crew;
            int turnBefore = g.State.Turn;

            for (int i = 0; i < 50; i++)
            {
                g.Turn.PreviewCombat(ActivityKind.Hunt);
                g.Turn.PreviewCombat(ActivityKind.Explore);
                g.Turn.PreviewCombat(ActivityKind.Gather);
            }

            Assert.AreEqual(moraleBefore, g.State.Morale);
            Assert.AreEqual(crewBefore, g.State.Crew);
            Assert.AreEqual(turnBefore, g.State.Turn);
            Assert.AreEqual(0, g.Combat.Requests.Count, "xem trước thì không được gọi resolver");
        }

        [Test]
        public void XemTruocTran_HoatDongKhongMoCombat_ThiRong()
        {
            TestGame g = TestGame.New();
            g.Plan.Assign(ActivityKind.Gather, g.MemberIds(3));

            Assert.IsFalse(g.Turn.PreviewCombat(ActivityKind.Gather).HasParty);
            Assert.IsFalse(g.Turn.PreviewCombat(ActivityKind.Morale).HasParty);
            Assert.IsFalse(g.Turn.PreviewCombat(ActivityKind.Build).HasParty);
        }
    }
}
