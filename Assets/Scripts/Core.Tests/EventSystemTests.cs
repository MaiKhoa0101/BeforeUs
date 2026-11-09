using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using NUnit.Framework;

namespace BeforeUs.Core.Tests
{
    /// <summary>C6 — EventSystem (design.md §8).</summary>
    [TestFixture]
    public class EventSystemTests
    {
        private BalanceConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
        }

        private GameState StateAt(int turn, int crew = 5, int stage = 1, int region = 0)
        {
            GameState state = new GameState { Turn = turn, Stage = stage, Region = region };
            MemberFactory factory = new MemberFactory(MemberStatRanges.AverageFromConfig(_config));
            IRandom seedRandom = new XorShift128Random(1);
            for (int i = 0; i < crew; i++)
            {
                state.Members.Add(factory.Create(seedRandom));
            }

            state.NextMemberId = factory.NextId;
            return state;
        }

        private TurnEventContext Context(
            GameState state, IRandom random, ICombatResolver resolver,
            ClimatePhase phase = ClimatePhase.Wet, int level = 0)
        {
            return new TurnEventContext(_config, state, random, resolver, phase, level);
        }

        private static EventSystem System(
            Func<EventDecisionContext, bool> chooseGood = null,
            Func<EventDecisionContext, bool> attack = null,
            ICombatOdds odds = null)
        {
            return new EventSystem(new DelegateEventDecisions(chooseGood, attack), odds);
        }

        // ---------- thời gian ân hạn và trận hướng dẫn ----------

        [Test]
        public void Luot1Den5_KhongCoSuKienNgauNhien()
        {
            EventSystem events = System();

            foreach (int turn in new[] { 1, 2, 4, 5 })
            {
                GameState state = StateAt(turn);
                TurnEventContext ctx = Context(state, new XorShift128Random(turn), new ScriptedCombatResolver());

                events.RunMachineTurn(ctx);

                Assert.AreEqual(TurnEventKind.None, events.LastEvent.Kind, "lượt " + turn);
                Assert.AreEqual(0, ctx.Food);
                Assert.AreEqual(0, ctx.Morale);
            }
        }

        [Test]
        public void Luot3_TranHuongDan_LuonThangKhongAiChet()
        {
            // design.md §8: +2 tinh thần, +3 neuron, tính 1 chiến tích thắng trận.
            GameState state = StateAt(3);
            EventSystem events = System();
            TurnEventContext ctx = Context(
                state, new XorShift128Random(1),
                new ProbabilisticCombatResolver(_config, new XorShift128Random(1)));

            events.RunMachineTurn(ctx);

            Assert.AreEqual(TurnEventKind.Tutorial, events.LastEvent.Kind);
            Assert.IsTrue(events.LastEvent.CombatWon.Value);
            Assert.AreEqual(2, ctx.Morale);
            Assert.AreEqual(3, ctx.Neuron);
            Assert.AreEqual(1, ctx.CombatWins);
            Assert.AreEqual(0, ctx.DeadMemberIds.Count);
        }

        [Test]
        public void Luot3_KhongRutSuKienNaoKhac()
        {
            // before_us_sim.py:97-99 dùng if/elif: lượt hướng dẫn thay thế hẳn việc rút sự kiện.
            for (int seed = 1; seed <= 20; seed++)
            {
                GameState state = StateAt(3);
                EventSystem events = System();
                events.RunMachineTurn(Context(
                    state, new XorShift128Random(seed),
                    new ProbabilisticCombatResolver(_config, new XorShift128Random(seed))));

                Assert.AreEqual(TurnEventKind.Tutorial, events.LastEvent.Kind, "seed " + seed);
            }
        }

        // ---------- tần suất và phân loại, design.md §8 ----------

        [Test]
        public void TuLuot6_TanSuatCoSuKienLa70PhanTram()
        {
            const int draws = 20000;
            EventSystem events = System();
            IRandom random = new XorShift128Random(2024);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();

            int happened = 0;
            for (int i = 0; i < draws; i++)
            {
                GameState state = StateAt(10);
                events.RunMachineTurn(Context(state, random, resolver));
                if (events.LastEvent.Kind != TurnEventKind.None)
                {
                    happened++;
                }
            }

            Assert.That((double)happened / draws, Is.EqualTo(0.7).Within(0.015));
        }

        [Test]
        public void PhanLoaiSuKien_PhaAm_40_20_40()
        {
            Dictionary<TurnEventKind, int> counts = DrawTypes(ClimatePhase.Wet, 30000, 7);
            int total = Total(counts);

            Assert.That(Share(counts, TurnEventKind.NonCombatGood, TurnEventKind.NonCombatBad, total),
                Is.EqualTo(0.40).Within(0.02), "không combat");
            Assert.That(Share(counts, TurnEventKind.Raid, total),
                Is.EqualTo(0.20).Within(0.02), "thú tấn công hang, pha Ẩm");
            Assert.That(Share(counts, TurnEventKind.Opportunity, total),
                Is.EqualTo(0.40).Within(0.02), "cơ hội combat");
        }

        [Test]
        public void PhanLoaiSuKien_PhaKho_ThuTanCongNhieuHon()
        {
            // design.md §8: 20% pha Ẩm, 25% pha Khô.
            Dictionary<TurnEventKind, int> counts = DrawTypes(ClimatePhase.Dry, 30000, 11);
            int total = Total(counts);

            Assert.That(Share(counts, TurnEventKind.Raid, total), Is.EqualTo(0.25).Within(0.02));
            Assert.That(Share(counts, TurnEventKind.Opportunity, total), Is.EqualTo(0.35).Within(0.02));
        }

        private Dictionary<TurnEventKind, int> DrawTypes(ClimatePhase phase, int draws, int seed)
        {
            EventSystem events = System(chooseGood: c => true, attack: c => true);
            IRandom random = new XorShift128Random(seed);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(true, null);

            Dictionary<TurnEventKind, int> counts = new Dictionary<TurnEventKind, int>();
            for (int i = 0; i < draws; i++)
            {
                GameState state = StateAt(10);
                events.RunMachineTurn(Context(state, random, resolver, phase));

                TurnEventKind kind = events.LastEvent.Kind;
                if (kind == TurnEventKind.None)
                {
                    continue;
                }

                counts.TryGetValue(kind, out int current);
                counts[kind] = current + 1;
            }

            return counts;
        }

        private static int Total(Dictionary<TurnEventKind, int> counts)
        {
            int total = 0;
            foreach (int value in counts.Values)
            {
                total += value;
            }

            return total;
        }

        private static double Share(Dictionary<TurnEventKind, int> counts, TurnEventKind kind, int total)
        {
            counts.TryGetValue(kind, out int value);
            return (double)value / total;
        }

        private static double Share(
            Dictionary<TurnEventKind, int> counts, TurnEventKind a, TurnEventKind b, int total)
        {
            counts.TryGetValue(a, out int first);
            counts.TryGetValue(b, out int second);
            return (double)(first + second) / total;
        }

        // ---------- sự kiện không combat ----------

        [Test]
        public void KhongCombat_KetQuaTot()
        {
            GameState state = StateAt(10);
            EventSystem events = System(chooseGood: c => true);

            RunUntil(events, state, TurnEventKind.NonCombatGood, out TurnEventContext ctx);

            Assert.AreEqual(4, ctx.Food);
            Assert.AreEqual(2, ctx.Morale);
        }

        [Test]
        public void KhongCombat_KetQuaXau_NangThemTheoDoKho()
        {
            // design.md §8 + §11: xấu = −(3 + level) thức ăn, −(2 + level) tinh thần.
            GameState state = StateAt(10);
            EventSystem events = System(chooseGood: c => false);

            RunUntil(events, state, TurnEventKind.NonCombatBad, out TurnEventContext ctx, level: 0);
            Assert.AreEqual(-3, ctx.Food);
            Assert.AreEqual(-2, ctx.Morale);

            GameState harder = StateAt(40);
            EventSystem events2 = System(chooseGood: c => false);
            RunUntil(events2, harder, TurnEventKind.NonCombatBad, out TurnEventContext ctx2, level: 3);
            Assert.AreEqual(-6, ctx2.Food, "−3 − 3 bậc độ khó");
            Assert.AreEqual(-5, ctx2.Morale, "−2 − 3 bậc độ khó");
        }

        [Test]
        public void KhongCombat_QuyetDinhDoBenNgoaiCungCap_KhongPhaiCore()
        {
            // design.md §8 nói "người chơi cân nhắc được kết quả tốt khoảng 60%", nhưng 60% là thuộc tính
            // của NGƯỜI CHƠI. Core không được viết cứng nó; bot ở C8 và A2 tự cài.
            int asked = 0;
            GameState state = StateAt(10);
            EventSystem events = System(chooseGood: c =>
            {
                asked++;
                return c.State.Crew >= 5;
            });

            RunUntil(events, state, TurnEventKind.NonCombatGood, out TurnEventContext _);
            Assert.Greater(asked, 0, "Core phải hỏi, không tự quyết");
        }

        // ---------- thú tấn công hang ----------

        [Test]
        public void ThuTanCongHang_ThangThiDuocThuong()
        {
            GameState state = StateAt(10);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(true, null);

            EventSystem events = System();
            TurnEventContext ctx = RunUntilWith(events, state, resolver, TurnEventKind.Raid);

            Assert.IsTrue(events.LastEvent.CombatWon.Value);
            Assert.AreEqual(2, ctx.Morale);
            Assert.AreEqual(3, ctx.Neuron);
            Assert.AreEqual(1, ctx.CombatWins);
        }

        [Test]
        public void ThuTanCongHang_LaBatBuoc_KhongHoiNguoiChoi()
        {
            // design.md §8: "phòng thủ bắt buộc" — khác Cơ hội combat.
            bool asked = false;
            GameState state = StateAt(10);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(true, null);

            EventSystem events = System(attack: c =>
            {
                asked = true;
                return false;
            });

            RunUntilWith(events, state, resolver, TurnEventKind.Raid);
            Assert.IsFalse(asked, "không được hỏi người chơi có đánh hay không");
        }

        [Test]
        public void ThuTanCongHang_CaBoTocThamGia()
        {
            GameState state = StateAt(10, crew: 6);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(true, null);

            EventSystem events = System();
            RunUntilWith(events, state, resolver, TurnEventKind.Raid);

            CombatRequest request = LastRequestOfKind(resolver, CombatKind.Defence);
            Assert.AreEqual(6, request.PartySize, "cả 6 người phòng thủ");
            Assert.AreEqual(6, request.Crew);
        }

        [Test]
        public void ThuTanCongHang_ThuaThiMatNguoi()
        {
            GameState state = StateAt(10, crew: 6);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(false, new[] { 1, 2 });

            EventSystem events = System();
            TurnEventContext ctx = RunUntilWith(events, state, resolver, TurnEventKind.Raid);

            Assert.IsFalse(events.LastEvent.CombatWon.Value);
            Assert.AreEqual(2, ctx.DeadMemberIds.Count);
            Assert.AreEqual(0, ctx.Morale, "thua phòng thủ không trừ tinh thần (design.md §8)");
        }

        // ---------- cơ hội combat ----------

        [Test]
        public void CoHoiCombat_RutLuiThiKhongGiXayRa()
        {
            GameState state = StateAt(10);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(false, new[] { 1, 2 });

            EventSystem events = System(attack: c => false);
            TurnEventContext ctx = RunUntilWith(events, state, resolver, TurnEventKind.Opportunity);

            Assert.IsTrue(events.LastEvent.Declined);
            Assert.IsNull(events.LastEvent.CombatWon);
            Assert.AreEqual(0, ctx.Food);
            Assert.AreEqual(0, ctx.Morale);
            Assert.AreEqual(0, ctx.DeadMemberIds.Count);
            Assert.AreEqual(0, resolver.Requests.Count, "rút lui thì không gọi resolver");
        }

        [Test]
        public void CoHoiCombat_ThangThiThuongLon()
        {
            GameState state = StateAt(10);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(true, null);

            EventSystem events = System(attack: c => true);
            TurnEventContext ctx = RunUntilWith(events, state, resolver, TurnEventKind.Opportunity);

            Assert.AreEqual(6, ctx.Food);
            Assert.AreEqual(3, ctx.Morale);
            Assert.AreEqual(4, ctx.Neuron);
            Assert.AreEqual(1, ctx.CombatWins);
        }

        [Test]
        public void CoHoiCombat_ThuaThiMatHaiNguoi()
        {
            GameState state = StateAt(10, crew: 6);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(false, new[] { 1, 2 });

            EventSystem events = System(attack: c => true);
            TurnEventContext ctx = RunUntilWith(events, state, resolver, TurnEventKind.Opportunity);

            Assert.AreEqual(2, ctx.DeadMemberIds.Count);
        }

        [Test]
        public void CoHoiCombat_NguoiChoiDuocBietTiLeThangUocLuong()
        {
            GameState state = StateAt(10, crew: 7, stage: 2);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(true, null);

            ProbabilisticCombatResolver odds = new ProbabilisticCombatResolver(_config, new XorShift128Random(1));
            double seen = -1;

            EventSystem events = new EventSystem(
                new DelegateEventDecisions(null, c =>
                {
                    seen = c.EstimatedWinChance;
                    return false;
                }),
                odds);

            RunUntilWith(events, state, resolver, TurnEventKind.Opportunity);

            // design.md §8: p = 0.35 + 0.05·crew + 0.04·stage, các hạng khác bằng 0.
            Assert.AreEqual(0.35 + 0.05 * 7 + 0.04 * 2, seen, 1e-12);
        }

        [Test]
        public void KhongCoNguonUocLuong_ThiTiLeThangBang0()
        {
            GameState state = StateAt(10);
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            double seen = -1;

            EventSystem events = System(attack: c =>
            {
                seen = c.EstimatedWinChance;
                return false;
            });

            RunUntilWith(events, state, resolver, TurnEventKind.Opportunity);
            Assert.AreEqual(0.0, seen, 1e-12);
        }

        // ---------- nối vào vòng lượt ----------

        [Test]
        public void NoiVaoTurnSystem_ThucAnSuKienVaoLuotDangTinh()
        {
            TestGame g = TestGame.New(5);
            EventSystem events = System(chooseGood: c => true, attack: c => false);

            TurnSystem turn = new TurnSystem(
                g.Config, g.State, g.Random, g.Factory,
                new Climate(g.Config), new Difficulty(g.Config),
                g.Nodes, g.Combat, events, odds: null, plants: TestOverrides.SinglePlant);

            g.State.Turn = 3;   // trận hướng dẫn
            turn.Plan.Assign(ActivityKind.Gather, g.AllMemberIds());
            TurnOutcome outcome = turn.EndTurn();

            Assert.AreEqual(TurnEventKind.Tutorial, events.LastEvent.Kind);
            Assert.AreEqual(1, g.State.CombatWins, "chiến tích từ trận hướng dẫn vào GameState");
            Assert.AreEqual(15, outcome.FoodFromActivities);
        }

        // ---------- tiện ích ----------

        private void RunUntil(
            EventSystem events, GameState state, TurnEventKind wanted,
            out TurnEventContext context, int level = 0)
        {
            ScriptedCombatResolver resolver = new ScriptedCombatResolver();
            resolver.Fallback = new CombatResult(true, null);
            context = RunUntilWith(events, state, resolver, wanted, level);
        }

        private TurnEventContext RunUntilWith(
            EventSystem events, GameState state, ScriptedCombatResolver resolver,
            TurnEventKind wanted, int level = 0)
        {
            for (int seed = 1; seed <= 400; seed++)
            {
                resolver.Requests.Clear();
                TurnEventContext ctx = Context(state, new XorShift128Random(seed), resolver, ClimatePhase.Wet, level);
                events.RunMachineTurn(ctx);

                if (events.LastEvent.Kind == wanted)
                {
                    return ctx;
                }
            }

            Assert.Fail("Không rút được sự kiện loại " + wanted + " trong 400 seed");
            return null;
        }

        private static CombatRequest LastRequestOfKind(ScriptedCombatResolver resolver, CombatKind kind)
        {
            for (int i = resolver.Requests.Count - 1; i >= 0; i--)
            {
                if (resolver.Requests[i].Kind == kind)
                {
                    return resolver.Requests[i];
                }
            }

            Assert.Fail("Không có yêu cầu combat loại " + kind);
            return null;
        }
    }
}
