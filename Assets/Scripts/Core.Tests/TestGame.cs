using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Sites;

namespace BeforeUs.Core.Tests
{
    /// <summary>Bộ giải quyết combat lập trình sẵn, để test luật lượt không phụ thuộc xác suất.</summary>
    public sealed class ScriptedCombatResolver : ICombatResolver
    {
        private readonly Queue<CombatResult> _scripted = new Queue<CombatResult>();

        public List<CombatRequest> Requests { get; } = new List<CombatRequest>();

        /// <summary>Dùng khi hết kịch bản. Mặc định: thua, không ai chết.</summary>
        public CombatResult Fallback { get; set; } = new CombatResult(false, null);

        public ScriptedCombatResolver Then(CombatResult result)
        {
            _scripted.Enqueue(result);
            return this;
        }

        public ScriptedCombatResolver ThenWin(params int[] deadMemberIds)
        {
            return Then(new CombatResult(true, deadMemberIds));
        }

        public ScriptedCombatResolver ThenLose(params int[] deadMemberIds)
        {
            return Then(new CombatResult(false, deadMemberIds));
        }

        public CombatResult Resolve(CombatRequest request)
        {
            Requests.Add(request);
            return _scripted.Count > 0 ? _scripted.Dequeue() : Fallback;
        }
    }

    /// <summary>Hệ sự kiện lập trình sẵn, để test điểm nối của C4 mà không cần C6.</summary>
    public sealed class ScriptedTurnEvents : ITurnEvents
    {
        public Action<TurnEventContext> OnTurnStart { get; set; }
        public Action<TurnEventContext> OnMachineTurn { get; set; }

        public void ApplyPendingAtTurnStart(TurnEventContext context)
        {
            if (OnTurnStart != null)
            {
                OnTurnStart(context);
            }
        }

        public void RunMachineTurn(TurnEventContext context)
        {
            if (OnMachineTurn != null)
            {
                OnMachineTurn(context);
            }
        }
    }

    /// <summary>Một ván dựng sẵn cho test. Chỉ số thành viên dùng chế độ trung bình nên số học thức ăn là chính xác.</summary>
    public sealed class TestGame
    {
        private TestGame(BalanceConfig config, GameState state, XorShift128Random random, MemberFactory factory,
            PurchasedNodeSet nodes, ScriptedCombatResolver combat, ScriptedTurnEvents events, TurnSystem turn)
        {
            Config = config;
            State = state;
            Random = random;
            Factory = factory;
            Nodes = nodes;
            Combat = combat;
            Events = events;
            Turn = turn;
        }

        public BalanceConfig Config { get; }
        public GameState State { get; }
        public XorShift128Random Random { get; }
        public MemberFactory Factory { get; }
        public PurchasedNodeSet Nodes { get; }
        public ScriptedCombatResolver Combat { get; }
        public ScriptedTurnEvents Events { get; }
        public TurnSystem Turn { get; }

        public TurnPlan Plan
        {
            get { return Turn.Plan; }
        }

        /// <summary>Sĩ số của ván test. Xem FixtureCapacityOpen để biết vì sao nó được ghim.</summary>
        public const int FixtureCrew = 5;

        /// <summary>
        /// Số chỗ ở của ván test, ghim bằng 6 — đúng bằng `lair.capacity_base` của v6.
        ///
        /// VÌ SAO GHIM: sĩ số khởi đầu và sức chứa hang là HAI CON SỐ CÂN BẰNG, và từ 2026-11-09
        /// Khoa đã ủy quyền cho chúng tôi chỉnh chúng theo dải Cổng 2. Nếu ~280 test luật lượt
        /// treo vào hai con số đó thì mỗi vòng chỉnh cân bằng sẽ làm đỏ hàng chục test KHÔNG
        /// liên quan gì tới cân bằng — và sức ép sửa assertion hàng loạt chính là cái bẫy đã bắt
        /// được chúng tôi một lần rồi (xem decisions.md, mẫu K1 và ghi chú về 14 assertion).
        ///
        /// Test NÀO thật sự nói về sĩ số hoặc sức chứa thì tự đặt số của nó, đừng dùng ván này.
        /// </summary>
        public const int FixtureCapacityOpen = 6;

        public static TestGame New(int seed = 1, GameMode mode = GameMode.Mvp, params string[] purchasedNodes)
        {
            BalanceConfig config = BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), mode);
            XorShift128Random random = new XorShift128Random(seed);

            // Chế độ trung bình: speed = 1, strength = 1, nên số học của test là chính xác.
            MemberFactory factory = new MemberFactory(MemberStatRanges.AverageFromConfig(config));
            GameState state = GameState.NewGame(config, random, factory);

            while (state.Members.Count < FixtureCrew)
            {
                state.Members.Add(factory.Create(random));
            }

            while (state.Members.Count > FixtureCrew)
            {
                state.Members.RemoveAt(state.Members.Count - 1);
            }

            state.NextMemberId = factory.NextId;

            PurchasedNodeSet nodes = new PurchasedNodeSet(purchasedNodes);
            ScriptedCombatResolver combat = new ScriptedCombatResolver();
            ScriptedTurnEvents events = new ScriptedTurnEvents();

            // Danh mục TỐI GIẢN: một loài, dinh dưỡng 1, không khám phá được gì.
            // Nhờ vậy thức ăn hái lượm luôn bằng ĐÚNG số người đi kiếm ăn, và test luật lượt
            // không phải chiến đấu với sự ngẫu nhiên của cơ chế hái lượm v7.
            TurnSystem turn = new TurnSystem(
                config, state, random, factory,
                new Climate(config), new Difficulty(config),
                nodes, combat, events, odds: null,
                plants: TestOverrides.SinglePlant, sites: TestOverrides.StandardSites);

            // Ghim sau khi dựng TurnSystem: constructor của nó vừa chép số thật của vùng 0 vào.
            state.RegionCapacityOpen = FixtureCapacityOpen;

            return new TestGame(config, state, random, factory, nodes, combat, events, turn);
        }

        /// <summary>Id của toàn bộ thành viên còn sống.</summary>
        public int[] AllMemberIds()
        {
            int[] ids = new int[State.Members.Count];
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = State.Members[i].Id;
            }

            return ids;
        }

        /// <summary>Id của n thành viên đầu tiên còn sống.</summary>
        public int[] MemberIds(int count)
        {
            int[] ids = new int[count];
            for (int i = 0; i < count; i++)
            {
                ids[i] = State.Members[i].Id;
            }

            return ids;
        }

        /// <summary>
        /// Cử người đi Xây dựng và chọn slot trong một bước. Trả về slot đã chọn.
        /// Dùng ở test luật lượt: slot nào không quan trọng, miễn là có chọn.
        /// </summary>
        public SiteView AssignBuild(string siteId, params int[] memberIds)
        {
            Plan.Assign(ActivityKind.Build, memberIds);
            PlanResult result = Plan.ChooseSite(siteId);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Không chọn được slot " + siteId + ": " + result.Message);
            }

            return Turn.Sites.Find(siteId);
        }

        /// <summary>Nhảy tới một lượt bất kỳ mà không giải quyết gì — để test luật phụ thuộc số lượt.</summary>
        public TestGame AtTurn(int turn)
        {
            State.Turn = turn;
            return this;
        }

        public TestGame WithStage(int stage)
        {
            State.Stage = stage;
            return this;
        }

        /// <summary>
        /// Chọn đoàn di cư cho lượt này (design-v7 §F.2). Mặc định mang theo cả bộ tộc —
        /// dùng ở test không quan tâm tới di cư nhưng có cử người đi Khám phá.
        /// </summary>
        public TestGame TakeEveryoneIfMigrating()
        {
            if (Plan.PartySizeOn(ActivityKind.Explore) == 0 || Plan.MigrationCapacity <= 0)
            {
                return this;
            }

            List<int> party = new List<int>();
            for (int i = 0; i < State.Members.Count && party.Count < Plan.MigrationCapacity; i++)
            {
                party.Add(State.Members[i].Id);
            }

            Plan.ChooseMigrationParty(party);
            return this;
        }

        /// <summary>
        /// Cử đủ người đi Kiếm ăn để đạt sàn bắt buộc (design-v7 §A.6), lấy từ CUỐI danh sách
        /// để không giành người với các test đang dùng MemberIds(n) từ đầu danh sách.
        /// Dùng ở những test không quan tâm tới hái lượm nhưng vẫn phải kết thúc lượt được.
        /// </summary>
        public TestGame MeetGatherFloor()
        {
            int floor = Plan.GatherFloor;
            if (floor <= 0)
            {
                return this;
            }

            List<int> chosen = new List<int>();
            for (int i = State.Members.Count - 1; i >= 0 && chosen.Count < floor; i--)
            {
                int id = State.Members[i].Id;
                if (!Plan.IsBusy(id))
                {
                    chosen.Add(id);
                }
            }

            chosen.AddRange(Plan.MembersOn(ActivityKind.Gather));
            Plan.Assign(ActivityKind.Gather, chosen);
            return this;
        }
    }
}
