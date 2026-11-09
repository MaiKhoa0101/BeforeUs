using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Sites;
using BeforeUs.Core.Neuron;

namespace BeforeUs.Core.Tests
{
    public enum BotStyle
    {
        /// <summary>Người chơi biết cân nhắc.</summary>
        Balanced,

        /// <summary>Không bao giờ nhận trận tự chọn.</summary>
        Safe,

        /// <summary>Đánh mọi thứ.</summary>
        Reckless,

        /// <summary>Chọn bừa.</summary>
        Careless
    }

    /// <summary>
    /// Bốn bot của C8, chép logic từ before_us_sim.py:65-80 (phân công), :104 (sự kiện lựa chọn)
    /// và :110 (cơ hội combat).
    ///
    /// KHÁC BIỆT CÓ CHỦ ĐÍCH với sim, đã được Khoa chấp nhận — xem docs/decisions.md:
    ///  - D1: Xây dựng và Săn bắt mở bằng NÚT đã mua, không phải bằng giai đoạn (sim:56 dùng s>=2).
    ///  - D2: Kiếm ăn là MỘT hoạt động mỗi lượt (sim:72 và :80 cử người kiếm ăn hai lần).
    ///  - D11: thua Phòng thủ luôn chừa lại ít nhất 1 người (sim:108 cho sĩ số về 0).
    /// Các cờ trong <see cref="BotOptions"/> cho phép TẮT từng khác biệt để đo riêng phần đóng góp.
    /// Không cờ nào sửa Core: chúng chỉ đổi cách bot chơi hoặc bọc thêm một lớp quanh resolver.
    /// </summary>
    public sealed class BotOptions
    {
        public BotStyle Style { get; set; }
        public bool ApplyNodeEffects { get; set; }

        /// <summary>
        /// Tắt D1: coi như hai nút mở khóa đã mua sẵn từ khi lên giai đoạn 2, đúng như sim:56.
        /// Bot vẫn phải mua chúng để tiến hóa, chỉ là không phải đợi mới được Xây dựng / Săn bắt.
        /// </summary>
        public bool DisableD1 { get; set; }

        /// <summary>
        /// Tắt D2: mô phỏng việc sim cử người Kiếm ăn hai lần — tốn thêm 1 năng lượng và được thêm
        /// 1 tinh thần mỗi lượt. Cùng số người đi kiếm ăn nên thức ăn không đổi.
        /// </summary>
        public bool DisableD2 { get; set; }

        /// <summary>Tắt D11: thua Phòng thủ có thể xóa sổ cả bầy, đúng như sim:108.</summary>
        public bool DisableD11 { get; set; }

        /// <summary>Chỉ Xây dựng mở theo giai đoạn, Săn bắt vẫn cần nút (phương án A của câu 12).</summary>
        public bool UnlockBuildByStage { get; set; }

        /// <summary>Chỉ Săn bắt mở theo giai đoạn, Xây dựng vẫn cần nút (phương án A′ của câu 12).</summary>
        public bool UnlockHuntByStage { get; set; }

        /// <summary>
        /// Danh mục thực vật dùng cho ván. null = plants.json thật.
        /// Có mặt để vòng chỉnh cân bằng quét được số của từng loài mà KHÔNG sửa file trên đĩa.
        /// </summary>
        public Plants.PlantCatalog Plants { get; set; }

        /// <summary>Dữ liệu nút thay thế, dùng để đo phương án. Null thì dùng neurons.json thật.</summary>
        public Neuron.NeuronData Neurons { get; set; }
    }

    /// <summary>Một ván do bot chơi, từ đầu tới khi thắng, thua hoặc hết lượt.</summary>
    public sealed class BotGame
    {
        public const int MaxTurns = 150;

        private static readonly string[] Stage1Order =
        {
            "s1_climbing", "s1_young_leaves", "s1_grooming",
            "s1_short_upright", "s1_dig_roots", "s1_alarm_call",
            "s1_evolve"
        };

        // Nhánh công cụ trước: s2_stone_tools mở Xây dựng, s2_scavenge_meat mở Săn bắt,
        // mà săn là chiến tích bắt buộc để rời giai đoạn 2. Đây là cách chơi hợp lý nhất dưới D1.
        private static readonly string[] Stage2Order =
        {
            "s2_stone_tools", "s2_scavenge_meat",
            "s2_bipedal", "s2_share_food",
            "s2_arched_foot", "s2_watch_rotation",
            "s2_evolve"
        };

        private readonly BalanceConfig _config;
        private readonly BotOptions _options;
        private readonly GameState _state;
        private readonly XorShift128Random _random;
        private readonly NeuronNetwork _network;
        private readonly IPurchasedNodes _unlocks;
        private readonly Plants.PlantCatalog _plants;
        private readonly EventSystem _events;
        private readonly TurnSystem _turn;
        private readonly ProbabilisticCombatResolver _odds;
        private bool _pendingD2Morale;

        public BotGame(BalanceConfig config, BotOptions options, int seed)
            : this(config, options, new XorShift128Random(seed), null)
        {
        }

        /// <summary>
        /// Dựng lại ván từ trạng thái đã TẢI. Đây chính là đường C7 phải đi, và nó dùng đúng
        /// con bot của C8 — không còn driver riêng với chính sách khác.
        ///
        /// Vì sao quan trọng: driver cũ (TestPlayer) chơi khác bot thật tới mức 0/10 ván của nó
        /// di cư được, nghĩa là LƯU–TẢI QUA MỘT LẦN DI CƯ chưa từng được kiểm. Mà di cư là thao
        /// tác một chiều, phá huỷ, xoá người khỏi bộ tộc vĩnh viễn — hỏng ở đó không làm game
        /// văng, nó làm người chơi tải ván lên và thấy một bộ tộc sai.
        /// </summary>
        public static BotGame Resume(
            BalanceConfig config, BotOptions options, GameState state, XorShift128Random random)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (random == null) throw new ArgumentNullException(nameof(random));

            return new BotGame(config, options, random, state);
        }

        private BotGame(BalanceConfig config, BotOptions options, XorShift128Random random, GameState resumed)
        {
            _config = config;
            _options = options;
            _random = random;

            MemberFactory factory = new MemberFactory(MemberStatRanges.AverageFromConfig(config));
            _state = resumed ?? GameState.NewGame(config, _random, factory);

            _network = new NeuronNetwork(
                options.Neurons ?? TestOverrides.StandardNeurons, config, _state, options.ApplyNodeEffects);

            bool buildByStage = options.DisableD1 || options.UnlockBuildByStage;
            bool huntByStage = options.DisableD1 || options.UnlockHuntByStage;
            _plants = options.Plants ?? TestOverrides.StandardPlants;
            _unlocks = buildByStage || huntByStage
                ? (IPurchasedNodes)new SimUnlockNodes(_network, _state, buildByStage, huntByStage)
                : _network;

            _odds = new ProbabilisticCombatResolver(config, _random, _network);
            ICombatResolver combat = options.DisableD11
                ? (ICombatResolver)new NoD11Resolver(_odds)
                : _odds;

            _events = new EventSystem(new BotDecisions(options.Style), _odds);

            ITurnEvents events = options.DisableD2
                ? (ITurnEvents)new D2MoraleEvents(_events, () => TakePendingD2Morale())
                : _events;

            _turn = new TurnSystem(
                config, _state, _random, factory,
                new Climate(config), new Difficulty(config),
                _unlocks, combat, events, odds: _odds, plants: _plants, sites: TestOverrides.StandardSites);
        }

        public GameState State
        {
            get { return _state; }
        }

        public XorShift128Random Random
        {
            get { return _random; }
        }

        public TurnSystem Turn
        {
            get { return _turn; }
        }

        /// <summary>Chơi tối đa ngần này lượt, dừng sớm khi thắng hoặc thua. Trả số lượt đã chơi.</summary>
        public int PlayTurns(int turns)
        {
            for (int i = 0; i < turns; i++)
            {
                if (_state.Crew <= _config.LoseWhenCrewAtOrBelow || _state.Stage >= _config.WinStage)
                {
                    return i;
                }

                _turn.BeginTurn();
                BuyNodes();
                Assign();

                TurnOutcome outcome = _turn.EndTurn();
                if (outcome.Site != null && outcome.Site.Cleared)
                {
                    SitesClearedEver++;
                }

                BuyNodes();
            }

            return turns;
        }

        /// <summary>
        /// Vân tay của ván: mọi thứ C7 phải giữ nguyên qua một vòng lưu–tải.
        /// Gồm cả trạng thái bộ sinh số ngẫu nhiên, vì ván trôi lệch thì nó lệch trước tiên.
        /// </summary>
        public string Fingerprint()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("turn=").Append(_state.Turn)
              .Append(" stage=").Append(_state.Stage)
              .Append(" region=").Append(_state.Region)
              .Append(" cap=").Append(_state.RegionCapacityOpen).Append('/').Append(_state.RegionCapacityMax)
              .Append(" neuron=").Append(_state.Neuron)
              .Append(" morale=").Append(_state.Morale)
              .Append(" build=").Append(_state.BuildLevel)
              .Append(" nest=").Append(_state.FoodNestTier)
              .Append(" feats=").Append(_state.CombatWins).Append('/').Append(_state.Hunts).Append('/').Append(_state.MigratedEver)
              .Append(" nextId=").Append(_state.NextMemberId)
              .Append(" rng=").Append(_random.SaveState());

            sb.Append(" sites=[").Append(string.Join(",", _state.ClearedSiteIds)).Append(']');
            sb.Append(" plants=[").Append(string.Join(",", _state.DiscoveredPlantIds)).Append(']');
            sb.Append(" nodes=[").Append(string.Join(",", _state.PurchasedNodeIds)).Append(']');

            sb.Append(" members=[");
            for (int i = 0; i < _state.Members.Count; i++)
            {
                Member m = _state.Members[i];
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append(m.Id).Append(':')
                  .Append(m.Speed.ToString("F4", CultureInfo.InvariantCulture)).Append('/')
                  .Append(m.Strength.ToString("F4", CultureInfo.InvariantCulture)).Append('/')
                  .Append(m.Hp);
            }

            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>Số lần thua Phòng thủ mà quota danh nghĩa >= sĩ số (chỗ D11 có tác dụng).</summary>
        public int D11Saves { get; private set; }

        /// <summary>
        /// Số công trình đã dọn TRONG CẢ VÁN. Khác GameState.ClearedSiteIds.Count, vì danh sách
        /// đó reset mỗi lần di cư — đếm ở cuối ván thì một ván dọn 3 công trình rồi di cư
        /// trông y như một ván chưa dọn gì.
        /// </summary>
        public int SitesClearedEver { get; private set; }

        public GameResult Play()
        {
            for (int turn = 1; turn <= MaxTurns; turn++)
            {
                _turn.BeginTurn();
                BuyNodes();
                Assign();

                int crewBefore = _state.Crew;
                TurnOutcome outcome = _turn.EndTurn();
                if (outcome.Site != null && outcome.Site.Cleared)
                {
                    SitesClearedEver++;
                }

                BuyNodes();

                CountD11(outcome, crewBefore);

                if (_state.Stage >= _config.WinStage)
                {
                    return GameResult.Win(outcome.Turn);
                }

                if (_state.Crew == 0)
                {
                    return GameResult.Loss(outcome.Turn, _state.Stage, ClassifyDeath(outcome));
                }
            }

            return GameResult.Timeout(MaxTurns, _state.Stage);
        }

        private void CountD11(TurnOutcome outcome, int crewBefore)
        {
            if (_events.LastEvent.Kind != TurnEventKind.Raid
                || _events.LastEvent.CombatWon != false)
            {
                return;
            }

            // Quota danh nghĩa tính trên sĩ số lúc trận xảy ra; xấp xỉ bằng sĩ số đầu lượt.
            int quota = _config.RaidQuotaForRegion(_state.Region)
                        + _config.Data.Difficulty.PerLevel.RaidQuota * outcome.DifficultyLevel;

            if (quota >= crewBefore)
            {
                D11Saves++;
            }
        }

        /// <summary>
        /// Nguyên nhân sĩ số về 0. Thứ tự trong lượt là: hoạt động → sự kiện → đói,
        /// nên soi ngược lại để tìm thứ cuối cùng giết người.
        /// </summary>
        private DeathCause ClassifyDeath(TurnOutcome outcome)
        {
            if (outcome.StarvationDeaths > 0)
            {
                return DeathCause.Starvation;
            }

            TurnEventRecord last = _events.LastEvent;
            if (last.Turn == outcome.Turn && last.DeadMemberIds.Count > 0)
            {
                return last.Kind == TurnEventKind.Raid ? DeathCause.Raid : DeathCause.Opportunity;
            }

            for (int i = 0; i < outcome.Activities.Count; i++)
            {
                ActivityOutcome activity = outcome.Activities[i];
                if (activity.DeadMemberIds.Count > 0)
                {
                    return activity.Kind == ActivityKind.Hunt ? DeathCause.Hunt : DeathCause.Explore;
                }
            }

            return DeathCause.Unknown;
        }

        private void BuyNodes()
        {
            // Chỉ mua nút BẮT BUỘC (3 gốc + 3 cổng + Tiến hóa). Tổng đúng bằng required_neuron,
            // nên chi tiêu neuron của bot khớp chi phí phẳng của sim. Nút tùy chọn thì sim
            // cũng không mua (xem docstring của before_us_sim.py).
            string[] order = _state.Stage == 1 ? Stage1Order : Stage2Order;

            bool bought = true;
            while (bought)
            {
                bought = false;
                for (int i = 0; i < order.Length; i++)
                {
                    if (_network.Buy(order[i]).Succeeded)
                    {
                        bought = true;
                        order = _state.Stage == 1 ? Stage1Order : Stage2Order;
                        break;
                    }
                }
            }
        }

        private void Assign()
        {
            List<int> free = new List<int>(_state.Crew);
            for (int i = 0; i < _state.Members.Count; i++)
            {
                free.Add(_state.Members[i].Id);
            }

            int energyBudget = _turn.Plan.EnergyMax;

            if (_options.Style == BotStyle.Careless)
            {
                AssignCareless(free, energyBudget);
                EnsureGatherFloor();
                ChooseMigrationPartyIfExploring();
                return;
            }

            int crew = _state.Crew;
            int cap = _state.LairCapacity(_config);
            FeatData feats = _config.FeatsForStage(_state.Stage);

            bool reckless = _options.Style == BotStyle.Reckless;
            bool fight = _options.Style != BotStyle.Safe;

            // sim:72 — nhóm kiếm ăn đầu tiên, vừa đủ no.
            //
            // QUAN TRỌNG: sim TRẢ NĂNG LƯỢNG cho Kiếm ăn ngay ở dòng 72, TRƯỚC Khám phá và Săn bắt.
            // Phải giữ đúng thứ tự đó, nếu không ở lượt chật năng lượng (Khám phá 3 + Nâng tinh thần 1
            // = 4 = tối đa của giai đoạn 1) thì Kiếm ăn bị rớt và cả bộ tộc chết đói.
            // D2 chỉ gộp HAI LẦN CỬ NGƯỜI thành một hoạt động — nó không đổi thứ tự đặt năng lượng.
            // v7: không còn gather_yield. Ước lượng từ kỳ vọng của một người, và không bao giờ
            // thấp hơn sàn bắt buộc (§A.6). Bot KHÔNG còn là bản chép trung thành của
            // before_us_sim.py — sim đã mô tả một trò chơi khác (xem design-v7-draft.md §C.7).
            int gatherCost = _config.EnergyCostOf(ActivityKind.Gather);
            double perGatherer = _state.Members.Count == 0 ? 0 : GatherRules.Forecast(
                _config, _plants, _state,
                new[] { _state.Members[0] }, _turn.Phase, _turn.FindChanceMultiplier).Expected;
            int demand = perGatherer > 0
                ? (int)Math.Ceiling(_turn.FoodNeeded / perGatherer)
                : free.Count;
            int firstGather = Math.Max(_turn.Plan.GatherFloor, demand);
            List<int> gatherers = new List<int>();
            bool gatherReserved = false;

            if (firstGather > 0 && free.Count > 0 && energyBudget >= gatherCost)
            {
                gatherers = Take(free, firstGather);
                energyBudget -= gatherCost;
                gatherReserved = true;
            }

            // sim:74-75 — khám phá.
            bool dry = _turn.Phase == ClimatePhase.Dry;
            bool wantExplore = _state.MigratedEver < feats.MigratedEver
                               || (dry && _state.Region < _config.RegionMaxIndex)
                               || reckless;
            if (fight && wantExplore && (crew >= 5 || reckless))
            {
                TryAssign(ActivityKind.Explore, free, 3, ref energyBudget);
            }

            // sim:76 — xây khi sắp chật hang.
            if (crew >= cap - 1)
            {
                TryAssign(ActivityKind.Build, free, 2, ref energyBudget);
                ChooseBuildSite(free, ref energyBudget);
            }

            // sim:77-78 — săn.
            bool wantHunt = _state.Hunts < feats.Hunts
                            || _state.CombatWins < feats.CombatWins
                            || (crew >= 7 && _options.Style != BotStyle.Balanced)
                            || reckless;
            if (fight && wantHunt && (crew >= 3 || reckless))
            {
                int k = reckless ? 3 : Math.Min(3, crew - 1);
                TryAssign(ActivityKind.Hunt, free, k, ref energyBudget);
            }

            // sim:79 — nâng tinh thần khi chưa chật hang.
            if (crew < cap)
            {
                TryAssign(ActivityKind.Morale, free, 2, ref energyBudget);
            }

            // sim:80 — phần còn lại đi kiếm ăn. D2: gộp vào CÙNG một hoạt động với nhóm đầu,
            // nên không trả năng lượng lần thứ hai (đó chính là chỗ Core khác sim).
            if (gatherReserved && free.Count > 0 && _options.DisableD2)
            {
                // Tắt D2: tái hiện đúng sim:80 — lần cử người Kiếm ăn THỨ HAI tốn thêm 1 năng lượng
                // và được thêm 1 tinh thần. Chỉ xảy ra KHI CÒN NGƯỜI DƯ; nếu hết năng lượng thì
                // sim cũng bỏ qua lần thứ hai và những người đó ngồi không.
                if (energyBudget >= gatherCost)
                {
                    energyBudget -= gatherCost;
                    _pendingD2Morale = true;
                    gatherers.AddRange(free);
                    free.Clear();
                }
            }
            else if (gatherReserved)
            {
                gatherers.AddRange(free);
                free.Clear();
            }
            else if (free.Count > 0 && energyBudget >= gatherCost)
            {
                // Nhóm đầu không đặt được (hết năng lượng từ trước), nhưng cuối lượt vẫn còn chỗ.
                gatherers.AddRange(free);
                free.Clear();
                energyBudget -= gatherCost;
                gatherReserved = true;
            }

            if (gatherReserved && gatherers.Count > 0)
            {
                _turn.Plan.Assign(ActivityKind.Gather, gatherers);
            }

            EnsureGatherFloor();
            ChooseMigrationPartyIfExploring();
        }

        private void AssignCareless(List<int> free, int energyBudget)
        {
            // sim:65-70 — chọn bừa trong các hoạt động đang mở.
            List<ActivityKind> options = new List<ActivityKind>();
            for (int i = 0; i < ActivityRules.All.Length; i++)
            {
                if (_turn.Plan.IsUnlocked(ActivityRules.All[i]))
                {
                    options.Add(ActivityRules.All[i]);
                }
            }

            if (options.Count == 0)
            {
                return;
            }

            while (free.Count > 0 && energyBudget >= 1)
            {
                ActivityKind activity = options[_random.NextInt(0, options.Count)];
                int cost = _config.EnergyCostOf(activity);
                if (cost > energyBudget)
                {
                    break;
                }

                int k = _random.NextInt(1, free.Count + 1);

                // Dưới D2 một hoạt động chỉ xuất hiện một lần: rút trúng hoạt động đã có người
                // thì gộp thêm vào, không trả năng lượng lần nữa.
                List<int> party = new List<int>(_turn.Plan.MembersOn(activity));
                bool alreadyActive = party.Count > 0;
                party.AddRange(Take(free, k));

                if (!_turn.Plan.Assign(activity, party).Succeeded)
                {
                    continue;
                }

                if (!alreadyActive)
                {
                    energyBudget -= cost;
                }
                else if (_options.DisableD2)
                {
                    // Tắt D2: sim cho cùng một hoạt động xuất hiện nhiều lần trong acts, mỗi lần
                    // trả năng lượng riêng (sim:64) và nhận thưởng riêng (sim:84).
                    energyBudget -= cost;
                    if (activity == ActivityKind.Gather)
                    {
                        _pendingD2Morale = true;
                    }
                }

                if (activity == ActivityKind.Build)
                {
                    ChooseBuildSite(free, ref energyBudget);
                }
            }

        }

        /// <summary>
        /// Sàn bắt buộc của v7 §A.6 chặn kết thúc lượt. Bot nào không tự đạt thì nhường chỗ cho
        /// Kiếm ăn: trả người về, và BỎ HẲN hoạt động khác nếu không còn đủ năng lượng.
        /// Thà mất một hoạt động còn hơn không kết thúc được lượt.
        /// </summary>
        private void EnsureGatherFloor()
        {
            if (_turn.Plan.MeetsGatherFloor || !_turn.Plan.CanPossiblyMeetGatherFloor)
            {
                return;
            }


            int floor = _turn.Plan.GatherFloor;

            for (int attempt = 0; attempt <= ActivityRules.All.Length; attempt++)
            {
                List<int> gatherers = new List<int>(_turn.Plan.MembersOn(ActivityKind.Gather));

                for (int i = 0; i < _state.Members.Count && gatherers.Count < floor; i++)
                {
                    int id = _state.Members[i].Id;
                    if (gatherers.Contains(id))
                    {
                        continue;
                    }

                    ReleaseFromOtherActivities(id);
                    gatherers.Add(id);
                }

                if (_turn.Plan.Assign(ActivityKind.Gather, gatherers).Succeeded)
                {
                    return;
                }

                // Không đặt được thì gần như chắc chắn vì hết năng lượng: bỏ hoạt động đắt nhất
                // đang chiếm chỗ rồi thử lại.
                if (!DropMostExpensiveOtherActivity())
                {
                    return;
                }
            }
        }

        private void ReleaseFromOtherActivities(int memberId)
        {
            for (int i = 0; i < ActivityRules.All.Length; i++)
            {
                ActivityKind other = ActivityRules.All[i];
                if (other == ActivityKind.Gather)
                {
                    continue;
                }

                List<int> party = new List<int>(_turn.Plan.MembersOn(other));
                if (party.Remove(memberId))
                {
                    _turn.Plan.Assign(other, party);
                    return;
                }
            }
        }

        private bool DropMostExpensiveOtherActivity()
        {
            ActivityKind worst = ActivityKind.Gather;
            int worstCost = 0;

            for (int i = 0; i < ActivityRules.All.Length; i++)
            {
                ActivityKind activity = ActivityRules.All[i];
                if (activity == ActivityKind.Gather || _turn.Plan.PartySizeOn(activity) == 0)
                {
                    continue;
                }

                int cost = _config.EnergyCostOf(activity);
                if (cost > worstCost)
                {
                    worst = activity;
                    worstCost = cost;
                }
            }

            if (worstCost == 0)
            {
                return false;
            }

            _turn.Plan.Clear(worst);
            return true;
        }

        /// <summary>
        /// Chọn slot công trình cho nhóm đang được cử đi Xây dựng (design-v7 §B.1).
        /// Chiến thuật: slot RẺ NHẤT mà nhóm này dọn nổi; không nổi slot nào thì bỏ hẳn hoạt động
        /// Xây dựng — bot không đốt 2 năng lượng để chắc chắn trượt.
        /// </summary>
        private void ChooseBuildSite(List<int> free, ref int energyBudget)
        {
            IReadOnlyList<int> party = _turn.Plan.MembersOn(ActivityKind.Build);
            if (party.Count == 0 || _turn.Sites == null)
            {
                return;
            }

            List<SiteView> visible = _turn.Sites.VisibleSites(_state.Stage, _unlocks);
            SiteView best = null;
            SiteView cheapest = null;

            for (int i = 0; i < visible.Count; i++)
            {
                SiteView site = visible[i];

                if (cheapest == null || site.StrengthRequired < cheapest.StrengthRequired)
                {
                    cheapest = site;
                }

                if (!_turn.Sites.WouldClear(site.Id, party))
                {
                    continue;
                }

                if (best == null || site.StrengthRequired < best.StrengthRequired)
                {
                    best = site;
                }
            }

            // Bot cẩu thả cứ nhắm slot rẻ nhất dù biết sẽ trượt — đó đúng là chỗ nó cẩu thả,
            // và là cách duy nhất để đo được cái giá của luật "ăn cả ngã về không".
            if (best == null && _options.Style == BotStyle.Careless)
            {
                best = cheapest;
            }

            if (best != null && _turn.Plan.ChooseSite(best.Id).Succeeded)
            {
                return;
            }

            // Không dọn nổi gì: trả người và trả lại năng lượng.
            List<int> released = new List<int>(party);
            _turn.Plan.Clear(ActivityKind.Build);
            energyBudget += _config.EnergyCostOf(ActivityKind.Build);
            free.InsertRange(0, released);
        }

        /// <summary>
        /// Chọn đoàn di cư khi có người đi Khám phá (design-v7 §F.2). Thắng là di cư, và
        /// người không được chọn mất hẳn — nên không chọn thì không kết thúc lượt được.
        ///
        /// Chính sách của bot: mang theo TỐI ĐA số chỗ vùng mới mở sẵn, ưu tiên nhóm đi khám phá
        /// (họ là người tìm ra chỗ đó), rồi tới những người còn lại theo thứ tự trong bộ tộc.
        /// </summary>
        private void ChooseMigrationPartyIfExploring()
        {
            if (_turn.Plan.PartySizeOn(ActivityKind.Explore) == 0)
            {
                return;
            }

            int capacity = _turn.Plan.MigrationCapacity;
            if (capacity <= 0)
            {
                return;
            }

            List<int> party = new List<int>();
            IReadOnlyList<int> explorers = _turn.Plan.MembersOn(ActivityKind.Explore);
            for (int i = 0; i < explorers.Count && party.Count < capacity; i++)
            {
                party.Add(explorers[i]);
            }

            for (int i = 0; i < _state.Members.Count && party.Count < capacity; i++)
            {
                int id = _state.Members[i].Id;
                if (!party.Contains(id))
                {
                    party.Add(id);
                }
            }

            _turn.Plan.ChooseMigrationParty(party);
        }

        private void TryAssign(ActivityKind activity, List<int> free, int count, ref int energyBudget)
        {
            if (count <= 0 || free.Count == 0 || !_turn.Plan.IsUnlocked(activity))
            {
                return;
            }

            int cost = _config.EnergyCostOf(activity);
            if (cost > energyBudget)
            {
                return;
            }

            List<int> party = Take(free, count);
            if (party.Count == 0)
            {
                return;
            }

            if (_turn.Plan.Assign(activity, party).Succeeded)
            {
                energyBudget -= cost;
            }
            else
            {
                free.InsertRange(0, party);
            }
        }

        private static List<int> Take(List<int> free, int count)
        {
            int take = Math.Min(count, free.Count);
            List<int> taken = free.GetRange(0, take);
            free.RemoveRange(0, take);
            return taken;
        }

        private bool TakePendingD2Morale()
        {
            bool pending = _pendingD2Morale;
            _pendingD2Morale = false;
            return pending;
        }

        /// <summary>
        /// Tắt D2: cộng thêm 1 tinh thần ở những lượt mà sim sẽ cử người Kiếm ăn lần thứ hai
        /// (morale_success của sim:84 được cộng mỗi lần add('gather', …)).
        /// Bọc ngoài EventSystem thật chứ không thay thế nó.
        /// </summary>
        private sealed class D2MoraleEvents : ITurnEvents
        {
            private readonly ITurnEvents _inner;
            private readonly System.Func<bool> _takePending;

            public D2MoraleEvents(ITurnEvents inner, System.Func<bool> takePending)
            {
                _inner = inner;
                _takePending = takePending;
            }

            public void ApplyPendingAtTurnStart(TurnEventContext context)
            {
                _inner.ApplyPendingAtTurnStart(context);
            }

            public void RunMachineTurn(TurnEventContext context)
            {
                if (_takePending())
                {
                    context.AddMorale(1);
                }

                _inner.RunMachineTurn(context);
            }
        }

        /// <summary>Quyết định sự kiện, chép từ before_us_sim.py:104 và :110.</summary>
        private sealed class BotDecisions : IEventDecisions
        {
            private readonly BotStyle _style;

            public BotDecisions(BotStyle style)
            {
                _style = style;
            }

            public bool ChooseGoodOutcome(EventDecisionContext context)
            {
                double skill = _style == BotStyle.Careless ? 0.5 : 0.6;
                return context.Random.NextDouble() < skill;
            }

            public bool AttackOpportunity(EventDecisionContext context)
            {
                if (_style == BotStyle.Careless || _style == BotStyle.Reckless)
                {
                    return true;
                }

                return _style == BotStyle.Balanced
                       && context.EstimatedWinChance > 0.6
                       && context.State.Crew >= 7;
            }
        }

        /// <summary>
        /// Tắt D1: hai nút mở khóa coi như đã có từ giai đoạn 2, đúng như before_us_sim.py:56.
        /// Chỉ ảnh hưởng điều kiện mở khóa; bot vẫn phải mua nút thật để tiến hóa.
        /// </summary>
        private sealed class SimUnlockNodes : IPurchasedNodes
        {
            private readonly IPurchasedNodes _inner;
            private readonly GameState _state;
            private readonly bool _build;
            private readonly bool _hunt;

            public SimUnlockNodes(IPurchasedNodes inner, GameState state, bool build, bool hunt)
            {
                _inner = inner;
                _state = state;
                _build = build;
                _hunt = hunt;
            }

            public bool Has(string nodeId)
            {
                if (_state.Stage >= 2)
                {
                    if (_build && nodeId == "s2_stone_tools") return true;
                    if (_hunt && nodeId == "s2_scavenge_meat") return true;
                }

                return _inner.Has(nodeId);
            }

            public NodeEffects Effects
            {
                get { return _inner.Effects; }
            }
        }

        /// <summary>
        /// Tắt D11: trả lại hành vi của before_us_sim.py:108 — thua Phòng thủ thì quái thú ăn
        /// min(sĩ số, quota) người, tức có thể xóa sổ cả bầy.
        /// </summary>
        private sealed class NoD11Resolver : ICombatResolver
        {
            private readonly ProbabilisticCombatResolver _inner;

            public NoD11Resolver(ProbabilisticCombatResolver inner)
            {
                _inner = inner;
            }

            public CombatResult Resolve(CombatRequest request)
            {
                CombatResult result = _inner.Resolve(request);
                if (request.Kind != CombatKind.Defence || result.Won)
                {
                    return result;
                }

                int wanted = Math.Min(_inner.RaidQuota(request), request.AllMemberIds.Count);
                if (result.DeadMemberIds.Count >= wanted)
                {
                    return result;
                }

                // D11 chặn ở sĩ số − 1, nên chênh lệch luôn đúng bằng 1 người: người còn sống sót.
                List<int> dead = new List<int>(result.DeadMemberIds);
                for (int i = 0; i < request.AllMemberIds.Count && dead.Count < wanted; i++)
                {
                    int id = request.AllMemberIds[i];
                    if (!dead.Contains(id))
                    {
                        dead.Add(id);
                    }
                }

                return new CombatResult(false, dead);
            }
        }
    }
}
