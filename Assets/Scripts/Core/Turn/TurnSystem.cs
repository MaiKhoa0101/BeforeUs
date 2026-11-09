using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;
using BeforeUs.Core.Plants;
using BeforeUs.Core.Sites;

namespace BeforeUs.Core
{
    /// <summary>
    /// Vòng lượt (design.md §2–§6).
    ///
    /// D2 là luật trung tâm của lớp này: trong lượt, phân công chỉ là dự định (xem <see cref="Plan"/>)
    /// và KHÔNG gì được tính; toàn bộ kết quả giải quyết trong đúng một bước <see cref="EndTurn"/>.
    /// Mỗi hoạt động xuất hiện tối đa một lần và trả năng lượng đúng một lần, không phụ thuộc số người.
    ///
    /// Sai lệch đã biết và đã được Khoa chấp nhận: before_us_sim.py:72 và :80 cử người Kiếm ăn HAI lần
    /// trong cùng một lượt, trả 2 năng lượng và +2 tinh thần. Dưới D2 việc đó không hợp lệ.
    /// Core làm theo D2; chênh lệch sẽ được đo và báo ở C8, không "sửa" cho khớp sim.
    /// </summary>
    public sealed class TurnSystem
    {
        /// <summary>
        /// Thứ tự giải quyết hoạt động, cố định để ván luôn tái lập được.
        /// Hái lượm và Nâng tinh thần trước (tất định), rồi hai hoạt động mở combat, rồi Xây dựng.
        ///
        /// ⚠ XÂY DỰNG ĐÃ CHUYỂN TỪ VỊ TRÍ THỨ BA XUỐNG CUỐI. Lý do cũ — "cấp công trình vừa lên
        /// có hiệu lực ngay trong trận cùng lượt" — bị D7 của Khoa lật: "lượt sau slot đó sẽ được
        /// dọn". Đặt Xây dựng cuối cùng là cách rẻ nhất để lời hứa đó thành luật: build_level mới
        /// không kịp chạm vào trận nào, cũng không chạm vào sức chứa hang lúc xét sinh sản.
        /// design.md không quy định thứ tự, nên đây là lựa chọn của Core.
        /// </summary>
        public static readonly ActivityKind[] ResolveOrder =
        {
            ActivityKind.Gather,
            ActivityKind.Morale,
            ActivityKind.Hunt,
            ActivityKind.Explore,
            ActivityKind.Build
        };

        private readonly BalanceConfig _config;
        private readonly GameState _state;
        private readonly IRandom _random;
        private readonly MemberFactory _memberFactory;
        private readonly Climate _climate;
        private readonly Difficulty _difficulty;
        private readonly IPurchasedNodes _nodes;
        private readonly ICombatResolver _combatResolver;
        private readonly ITurnEvents _events;

        private TurnEventContext _eventContext;
        private readonly List<int> _eventDeaths = new List<int>();
        private readonly ICombatOdds _odds;
        private readonly PlantCatalog _plants;
        private readonly SiteSystem _sites;
        private int _moraleFromDeaths;
        private bool _pendingMigration;
        private bool _turnBegun;
        private int _moraleAtTurnStart;
        private int _eventFood;

        public TurnSystem(
            BalanceConfig config,
            GameState state,
            IRandom random,
            MemberFactory memberFactory,
            Climate climate,
            Difficulty difficulty,
            IPurchasedNodes nodes,
            ICombatResolver combatResolver,
            ITurnEvents events = null,
            ICombatOdds odds = null,
            PlantCatalog plants = null,
            SiteCatalog sites = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (memberFactory == null) throw new ArgumentNullException(nameof(memberFactory));
            if (climate == null) throw new ArgumentNullException(nameof(climate));
            if (difficulty == null) throw new ArgumentNullException(nameof(difficulty));
            if (combatResolver == null) throw new ArgumentNullException(nameof(combatResolver));

            _config = config;
            _state = state;
            _random = random;
            _memberFactory = memberFactory;
            _climate = climate;
            _difficulty = difficulty;
            _nodes = nodes ?? NoPurchasedNodes.Instance;
            _combatResolver = combatResolver;
            _events = events;

            // Nguồn ước lượng tỉ lệ thắng cho PreviewCombat. Nếu bộ giải quyết đã biết tính
            // xác suất thì dùng luôn nó — không có bản cài công thức thứ hai nào ở đây.
            _odds = odds ?? (combatResolver as ICombatOdds);

            // Giữ bộ cấp id đồng bộ với ván, kể cả khi ván vừa được tải lại (C7).
            if (_state.NextMemberId > 0)
            {
                _memberFactory.ResumeFrom(_state.NextMemberId);
            }

            _plants = plants;

            // v7: loài biết sẵn từ đầu ván (plants.json discover_weight = 0). Gieo ở đây để mọi
            // đường vào — ván mới, ván tải lại — đều đi qua đúng một chỗ.
            if (_plants != null && _state.DiscoveredPlantIds.Count == 0)
            {
                _state.DiscoveredPlantIds.AddRange(_plants.KnownFromStartIds());
            }

            // design-v7 §B: slot công trình. Không có catalog thì Xây dựng không dùng được —
            // nổ lúc giải quyết chứ không âm thầm cộng điểm như cơ chế cũ.
            _sites = sites == null ? null : new SiteSystem(sites, _state);

            // Ván MỚI chưa biết trần chỗ ở của vùng khởi đầu, nên lấy từ sites.json một lần ở đây.
            // Ván TẢI LẠI thì đã có sẵn trong file lưu — tra lại sẽ xoá mất công trình đã dọn.
            if (_sites != null && _state.RegionCapacityMax == 0)
            {
                _sites.EnterRegion(_state.Region);
            }

            Plan = new TurnPlan(_config, _state, _nodes, _sites);
        }

        /// <summary>Hệ slot công trình của ván này (design-v7 §B). null nếu ván dựng không có sites.json.</summary>
        public SiteSystem Sites
        {
            get { return _sites; }
        }

        /// <summary>Dự định của lượt hiện tại (D2). Tự xóa sạch sau mỗi <see cref="EndTurn"/>.</summary>
        public TurnPlan Plan { get; }

        public ClimatePhase Phase
        {
            get { return _climate.PhaseAt(_state.Turn); }
        }

        public int DifficultyLevel
        {
            get { return _difficulty.LevelAt(_state.Turn); }
        }

        /// <summary>
        /// Số cộng vào tổng nhanh nhẹn của nhóm hái lượm, do bậc Ổ đựng thức ăn (design-v7 §B.4, E2).
        /// 0 khi ván chưa có sites.json hoặc ổ còn ở bậc 0.
        /// </summary>
        public double SpeedThresholdRelief
        {
            get { return _sites == null ? 0.0 : _sites.SpeedThresholdRelief; }
        }

        /// <summary>Hệ số bào mòn của độ khó lên mọi tỉ lệ tìm thấy (design-v7 §A.7).</summary>
        public double FindChanceMultiplier
        {
            get { return _difficulty.FindChanceMultiplier(DifficultyLevel); }
        }

        /// <summary>
        /// Dự báo hái lượm với dự định hiện tại (design-v7 §A.3). Truy vấn thuần, không rút số.
        /// `Guaranteed` là thứ duy nhất chắc chắn có; `Expected` chỉ là trung bình dài hạn.
        /// </summary>
        public GatherForecast ForecastGather()
        {
            if (_plants == null)
            {
                return new GatherForecast(0, 0);
            }

            return GatherRules.Forecast(
                _config, _plants, _state, LivingParty(Plan.MembersOn(ActivityKind.Gather)),
                Phase, FindChanceMultiplier, SpeedThresholdRelief);
        }

        /// <summary>"Cuối lượt cần X" của design.md §2.</summary>
        public int FoodNeeded
        {
            get { return _state.FoodNeeded(_config); }
        }

        /// <summary>
        /// "Đang có Y" của design.md §2: thức ăn CHẮC CHẮN có với dự định hiện tại —
        /// hái lượm cộng thức ăn sự kiện đã nhận. Không tính thức ăn từ Săn bắt, vì trận có thể thua.
        /// </summary>
        public int ForecastFood
        {
            get
            {
                // v7: thức ăn hái lượm là kết quả của những lần rút, nên "chắc chắn có" nay
                // chỉ còn phần khung đảm bảo theo speed. Muốn con số kỳ vọng thì gọi ForecastGather().
                return _eventFood + ForecastGather().Guaranteed;
            }
        }

        /// <summary>
        /// Xem trước hậu quả của việc THẮNG trận Khám phá (design-v7 §F.2).
        /// Truy vấn thuần, KHÔNG tác dụng phụ — cùng kỷ luật với PreviewCombat và CanAfford.
        ///
        /// Vì sao phải có: thắng một trận có thể giết ván. Người chơi cử 3 người đi khám phá,
        /// thắng, rồi phát hiện phải bỏ lại 4 người và còn đúng 3. Đó là luật, nhưng nó phải
        /// NHÌN THẤY ĐƯỢC trước khi bấm — nếu không họ sẽ gọi nó là lỗi chứ không gọi là luật.
        /// </summary>
        public MigrationPreview PreviewMigration()
        {
            if (_sites == null || !_sites.HasNextRegion(_config.RegionMaxIndex))
            {
                return MigrationPreview.None;
            }

            return new MigrationPreview(
                _state.Region + 1,
                _sites.NextRegionCapacityOpen(_config.RegionMaxIndex),
                _state.Crew,
                Plan.MigrationParty.Count,
                Plan.MembersLeftBehindIfMigrating);
        }

        /// <summary>
        /// Thông tin để người chơi cân nhắc trước khi bấm kết thúc lượt (góp ý 3 của Khoa, playtest 01).
        /// Truy vấn thuần, KHÔNG tác dụng phụ — cùng kỷ luật với <see cref="TurnPlan.CanAfford"/>.
        ///
        /// Trả DỮ LIỆU THÔ. Core cố ý không quy ra thang "dễ / vừa / khó": cách diễn đạt là quyền
        /// của UI và của Khoa, còn con số thì chỉ được tính ở đúng một nơi.
        /// </summary>
        public CombatPreview PreviewCombat(ActivityKind activity)
        {
            if (activity != ActivityKind.Hunt && activity != ActivityKind.Explore)
            {
                return CombatPreview.None;
            }

            List<Member> party = LivingParty(Plan.MembersOn(activity));
            if (party.Count == 0)
            {
                return CombatPreview.None;
            }

            CombatRequest request = BuildCombatRequest(activity, party);

            int foodIfWon = activity == ActivityKind.Hunt
                ? _config.HuntFoodForStage(_state.Stage) * party.Count
                : 0;

            return new CombatPreview(
                party.Count,
                foodIfWon,
                _odds != null ? _odds.WinChance(request) : (double?)null);
        }

        /// <summary>
        /// design.md §2 bước 1. Gọi được nhiều lần không sao: lần thứ hai trở đi không làm gì.
        /// <see cref="EndTurn"/> tự gọi nếu tầng trên quên.
        /// </summary>
        public void BeginTurn()
        {
            if (_turnBegun)
            {
                return;
            }

            _turnBegun = true;
            _moraleAtTurnStart = _state.Morale;
            _moraleFromDeaths = 0;
            _pendingMigration = false;
            _eventFood = 0;
            _eventDeaths.Clear();
            _eventContext = new TurnEventContext(_config, _state, _random, _combatResolver, Phase, DifficultyLevel);

            if (_events != null)
            {
                _events.ApplyPendingAtTurnStart(_eventContext);
                DrainEventContext();
            }
        }

        /// <summary>design.md §2 bước 2 đến 4. Đây là nơi DUY NHẤT dự định biến thành kết quả (D2).</summary>
        public TurnOutcome EndTurn()
        {
            BeginTurn();

            // design-v7 §A.6 (D6 của Khoa): ít nhất 1/3 bộ tộc phải đi tìm thức ăn.
            // Chặn cứng vì Khoa viết "phải đi tìm thức ăn", không phải "nên".
            PlanResult floor = Plan.ValidateForEndTurn();
            if (!floor.Succeeded)
            {
                throw new InvalidOperationException(floor.Message);
            }

            TurnOutcome outcome = new TurnOutcome
            {
                Turn = _state.Turn,
                Phase = Phase,
                DifficultyLevel = DifficultyLevel,
                BuildLevelBefore = _state.BuildLevel,
                RegionBefore = _state.Region,
                StageBefore = _state.Stage
            };

            ResolveActivities(outcome);
            RunMachineTurn();
            ResolveEndOfTurn(outcome);

            outcome.BuildLevelAfter = _state.BuildLevel;
            outcome.RegionAfter = _state.Region;
            outcome.StageAfter = _state.Stage;
            outcome.MoraleDelta = _state.Morale - _moraleAtTurnStart;
            outcome.MoraleFromDeaths = _moraleFromDeaths;
            outcome.CrewAfter = _state.Crew;

            // design.md §1, sửa theo góp ý 8 của Khoa ở playtest 01: thua khi sĩ số <= ngưỡng.
            outcome.Lost = _state.Crew <= _config.LoseWhenCrewAtOrBelow;

            // design.md §2 bước 4: kiểm tra thắng/thua. Tiến hóa xảy ra lúc mở nút Tiến hóa
            // (design.md §7, bất cứ lúc nào trong lượt), nên ở đây chỉ còn việc đối chiếu giai đoạn.
            outcome.Won = _state.Stage >= _config.WinStage;

            _state.Turn++;
            Plan.ClearAll();
            _turnBegun = false;

            // design.md §4: thức ăn không để dành. Xóa ngay để dự báo của lượt sau không mượn
            // nhầm thức ăn sự kiện của lượt này, kể cả khi tầng trên chưa gọi BeginTurn.
            _eventFood = 0;
            _eventDeaths.Clear();

            return outcome;
        }

        private void ResolveActivities(TurnOutcome outcome)
        {
            // Hệ số bào mòn chốt một lần từ đầu lượt, TRƯỚC khi Khám phá có thể đổi vùng.
            double findMultiplier = FindChanceMultiplier;

            List<ActivityKind> active = Plan.ActiveActivitiesInResolveOrder();
            for (int i = 0; i < active.Count; i++)
            {
                ActivityKind kind = active[i];

                // Người đã chết ở một hoạt động trước đó thì không tham gia nữa.
                List<Member> party = LivingParty(Plan.MembersOn(kind));
                if (party.Count == 0)
                {
                    continue;
                }

                ActivityOutcome result = new ActivityOutcome(kind, party.Count);
                outcome.Activities.Add(result);

                // design.md §5: neuron = neuron_per_member × k, nhận cả khi thất bại.
                result.Neuron = _config.NeuronPerMemberOf(kind) * party.Count;
                _state.Neuron += result.Neuron;
                outcome.NeuronGained += result.Neuron;

                switch (kind)
                {
                    case ActivityKind.Gather:
                        ResolveGather(result, party, outcome, findMultiplier);
                        break;
                    case ActivityKind.Morale:
                        ResolveMorale(result, party);
                        break;
                    case ActivityKind.Build:
                        ResolveBuild(result, party, outcome);
                        break;
                    case ActivityKind.Hunt:
                    case ActivityKind.Explore:
                        ResolveCombatActivity(result, party, kind);
                        break;
                }

                outcome.FoodFromActivities += result.Food;
                for (int d = 0; d < result.DeadMemberIds.Count; d++)
                {
                    outcome.DeadMemberIds.Add(result.DeadMemberIds[d]);
                }
            }
        }

        private void ResolveGather(ActivityOutcome result, List<Member> party, TurnOutcome outcome, double findMultiplier)
        {
            if (_plants == null)
            {
                throw new InvalidOperationException(
                    "Từ v7, TurnSystem cần PlantCatalog để giải quyết Kiếm ăn. "
                    + "Truyền plants: PlantCatalog.Load(<nội dung Assets/Data/plants.json>) vào constructor.");
            }

            // D2: Kiếm ăn là MỘT hoạt động với k = tổng số người được cử, tinh thần +1 MỘT lần.
            // v7: thức ăn là tổng những lần rút trúng — KHÔNG suy ra được từ k bằng phép nhân nào.
            GatherOutcome gather = GatherRules.Resolve(
                _config, _plants, _state, party, outcome.Phase, findMultiplier, _random, SpeedThresholdRelief);

            outcome.Gather = gather;
            result.Food = gather.Food;
            AddMorale(result, _config.Data.Activities.Gather.MoraleOnSuccess);
        }

        private void ResolveMorale(ActivityOutcome result, List<Member> party)
        {
            // design.md §5: Nâng tinh thần là hoạt động duy nhất cộng theo đầu người.
            // Nút "Chải lông cho nhau" cộng thêm 1 MỖI LẦN LÀM, không theo đầu người (design.md §7.3).
            AddMorale(
                result,
                _config.Data.Activities.Morale.MoralePerMember * party.Count + _nodes.Effects.MoraleActivityBonus);
        }

        private void ResolveBuild(ActivityOutcome result, List<Member> party, TurnOutcome outcome)
        {
            if (_sites == null)
            {
                throw new InvalidOperationException(
                    "Từ v7, TurnSystem cần SiteCatalog để giải quyết Xây dựng. "
                    + "Truyền sites: SiteCatalog.Load(<nội dung Assets/Data/sites.json>) vào constructor.");
            }

            // Chọn slot là điều kiện bắt buộc; TurnPlan.ValidateForEndTurn đã chặn trước khi tới đây.
            string siteId = Plan.BuildTargetSiteId;

            int[] ids = new int[party.Count];
            for (int i = 0; i < party.Count; i++)
            {
                ids[i] = party[i].Id;
            }

            // design-v7 §B.1 (D7 của Khoa): ăn cả ngã về không. Thiếu sức mạnh thì lượt đó phí.
            SiteResolution site = _sites.Resolve(siteId, ids);
            outcome.Site = site;

            // Tinh thần vẫn cộng dù dọn trượt: họ đã bỏ cả lượt ra làm. design.md §5 không
            // gắn morale_on_success với thành công, nó là phần thưởng của chính việc làm.
            AddMorale(result, _config.Data.Activities.Build.MoraleOnSuccess);
        }

        private void ResolveCombatActivity(ActivityOutcome result, List<Member> party, ActivityKind kind)
        {
            CombatRequest request = BuildCombatRequest(kind, party);
            CombatResult combat = _combatResolver.Resolve(request);
            if (combat == null)
            {
                throw new InvalidOperationException("ICombatResolver trả về null cho " + kind);
            }

            result.CombatWon = combat.Won;
            CombatResultsData results = _config.Data.CombatResults;

            if (combat.Won)
            {
                AddMorale(result, results.MoraleOnWin);

                // design.md §7.1: thắng trận bất kỳ đều tính chiến tích "thắng trận".
                _state.CombatWins++;

                if (kind == ActivityKind.Hunt)
                {
                    result.Food = _config.HuntFoodForStage(_state.Stage) * party.Count;
                    _state.Hunts++;
                }
                else if (_sites != null && _sites.HasNextRegion(_config.RegionMaxIndex))
                {
                    // KHÔNG đổi vùng ngay tại đây. Di cư là việc cuối cùng của lượt (§F.2),
                    // để Xây dựng của chính lượt này còn thuộc về vùng cũ và để cả bộ tộc —
                    // kể cả những người sắp bị bỏ lại — vẫn ăn phần của lượt này.
                    _pendingMigration = true;
                }
                else if (_sites == null)
                {
                    // Ván dựng không có sites.json: giữ hành vi cũ của design.md §12.
                    _state.Region = Math.Min(_state.Region + 1, _config.RegionMaxIndex);
                    _state.MigratedEver++;
                }
            }
            else
            {
                AddMorale(result, results.MoraleOnLoss);
            }

            // design.md §9: người chết trong trận là chết thật, kể cả khi thắng.
            // Ai chết là quyết định của bộ giải quyết combat (kiểu tấn công của quái thú), không phải của C4.
            ApplyCombatDeaths(result, combat.DeadMemberIds);
        }

        private void ApplyCombatDeaths(ActivityOutcome result, IReadOnlyList<int> deadMemberIds)
        {
            for (int i = 0; i < deadMemberIds.Count; i++)
            {
                int id = deadMemberIds[i];
                if (RemoveMember(id))
                {
                    result.DeadMemberIds.Add(id);
                }
            }
        }

        private void RunMachineTurn()
        {
            if (_events == null)
            {
                return;
            }

            _events.RunMachineTurn(_eventContext);
            DrainEventContext();
        }

        private void ResolveEndOfTurn(TurnOutcome outcome)
        {
            for (int i = 0; i < _eventDeaths.Count; i++)
            {
                outcome.DeadMemberIds.Add(_eventDeaths[i]);
            }

            outcome.FoodFromEvents = _eventFood;
            outcome.FoodTotal = outcome.FoodFromActivities + _eventFood;
            outcome.FoodNeeded = FoodNeeded;

            ResolveHunger(outcome);

            // design.md §4: tinh thần không âm. Chặn một lần ở cuối lượt, như before_us_sim.py:115.
            if (_state.Morale < 0)
            {
                _state.Morale = 0;
            }

            ResolveBirth(outcome);

            // design.md §4: thức ăn không để dành — nên nó chưa từng được cất vào GameState.

            // §F.2: di cư là việc CUỐI CÙNG của lượt. Cả bộ tộc đã ăn xong, đã sinh xong,
            // công trình của lượt này đã thuộc về vùng cũ — rồi đoàn mới lên đường.
            ResolveMigration(outcome);
        }

        /// <summary>
        /// Di cư sang vùng kế tiếp (design-v7 §F.2, Khoa 2026-11-09).
        ///
        /// Ba luật, đều do Khoa nói thẳng:
        ///   - "người ở lại mất luôn" — xoá khỏi GameState, không lưu, không theo dõi;
        ///   - "miễn khu vực mới có đủ slot cho số người di cư" — trần là số chỗ mở sẵn của vùng mới;
        ///   - "lối đi 1 chiều không có đường trở lại" — region chỉ tăng.
        /// </summary>
        private void ResolveMigration(TurnOutcome outcome)
        {
            if (!_pendingMigration || _sites == null)
            {
                return;
            }

            _pendingMigration = false;

            int capacity = _sites.NextRegionCapacityOpen(_config.RegionMaxIndex);
            if (capacity <= 0)
            {
                return;
            }

            // Người trong đoàn đã chết trong lượt thì rơi khỏi đoàn.
            List<int> travellers = new List<int>();
            IReadOnlyList<int> chosen = Plan.MigrationParty;
            for (int i = 0; i < chosen.Count && travellers.Count < capacity; i++)
            {
                if (_state.FindMember(chosen[i]) != null)
                {
                    travellers.Add(chosen[i]);
                }
            }

            // Người sinh ra TRONG lượt này không thể có trong đoàn — lúc người chơi chọn thì nó
            // chưa tồn tại. Bỏ lại một đứa trẻ vì thứ tự thao tác là lỗi, không phải luật:
            // cho nó theo nếu còn chỗ.
            for (int i = 0; i < outcome.BornMemberIds.Count && travellers.Count < capacity; i++)
            {
                int id = outcome.BornMemberIds[i];
                if (_state.FindMember(id) != null && !travellers.Contains(id))
                {
                    travellers.Add(id);
                }
            }

            // Cả đoàn chết sạch trong chính lượt đó thì không ai đi được — ở lại vùng cũ.
            if (travellers.Count == 0)
            {
                return;
            }

            HashSet<int> going = new HashSet<int>(travellers);
            for (int i = _state.Members.Count - 1; i >= 0; i--)
            {
                int id = _state.Members[i].Id;
                if (!going.Contains(id))
                {
                    // Người ở lại KHÔNG phải người chết: không trừ tinh thần theo
                    // morale.on_member_death (D14a), và không vào DeadMemberIds.
                    _state.Members.RemoveAt(i);
                    outcome.LeftBehindMemberIds.Add(id);
                }
            }

            _sites.EnterRegion(_state.Region + 1);
            _state.MigratedEver++;

            // outcome.Migrated là thuộc tính suy ra (RegionAfter > RegionBefore); RegionAfter
            // được chốt sau khi ResolveEndOfTurn chạy xong, nên không phải gán gì ở đây.
        }

        private void ResolveHunger(TurnOutcome outcome)
        {
            if (_state.Crew == 0 || outcome.FoodTotal >= outcome.FoodNeeded)
            {
                return;
            }

            // Nút "Chia sẻ thức ăn": thiếu ĐÚNG 1 phần ăn thì không mất người (design.md §7.4).
            if (_nodes.Effects.StarvationForgiveOne && outcome.FoodNeeded - outcome.FoodTotal == 1)
            {
                return;
            }

            outcome.Starved = true;

            // design.md §4 [ĐỀ XUẤT]: mất đúng 1 thành viên, chọn ngẫu nhiên.
            int deaths = Math.Min(_config.Data.StarvationDeaths, _state.Crew);
            for (int i = 0; i < deaths; i++)
            {
                int index = _random.NextInt(0, _state.Members.Count);
                int id = _state.Members[index].Id;
                if (RemoveMember(id))
                {
                    outcome.StarvationDeaths++;
                    outcome.DeadMemberIds.Add(id);
                }
            }
        }

        private void ResolveBirth(TurnOutcome outcome)
        {
            BirthData birth = _config.Data.Birth;

            // Nút "Cõng con trên lưng" hạ chi phí xuống 3 (design.md §7.3).
            int moraleCost = _nodes.Effects.BirthMoraleCost(birth.MoraleCost);

            for (int i = 0; i < birth.MaxPerTurn; i++)
            {
                // design.md §6: cuối lượt sau khi ăn, nếu tinh thần đủ, còn thành viên
                // và số thành viên < sức chứa hang thì sinh 1 người và trừ tinh thần.
                if (_state.Crew == 0
                    || _state.Morale < moraleCost
                    || _state.Crew >= _state.LairCapacity(_config))
                {
                    return;
                }

                // Nút "Dạy trẻ" chỉ cộng cho người mới sinh (design.md §7.4).
                Member born = _memberFactory.Create(_random, _nodes.Effects.NewbornStatBonus);
                _state.Members.Add(born);
                _state.NextMemberId = _memberFactory.NextId;
                _state.Morale -= moraleCost;
                outcome.BornMemberIds.Add(born.Id);
            }
        }

        private void DrainEventContext()
        {
            _eventFood += _eventContext.Food;
            _state.Morale += _eventContext.Morale;
            _state.Neuron += _eventContext.Neuron;
            _state.CombatWins += _eventContext.CombatWins;

            IReadOnlyList<int> dead = _eventContext.DeadMemberIds;
            for (int i = 0; i < dead.Count; i++)
            {
                if (RemoveMember(dead[i]))
                {
                    _eventDeaths.Add(dead[i]);
                }
            }

            _eventContext.Reset();
        }

        /// <summary>Dựng yêu cầu combat. Dùng chung cho PreviewCombat và lúc giải quyết thật,
        /// nên con số người chơi thấy trước chính là con số trận sẽ dùng.</summary>
        private CombatRequest BuildCombatRequest(ActivityKind kind, List<Member> party)
        {
            int[] memberIds = new int[party.Count];
            for (int i = 0; i < party.Count; i++)
            {
                memberIds[i] = party[i].Id;
            }

            return new CombatRequest(
                kind == ActivityKind.Hunt ? CombatKind.Hunt : CombatKind.Explore,
                memberIds,
                _state.Stage,
                _state.Region,
                _state.BuildLevel,
                DifficultyLevel,
                _state.Crew,
                AllMemberIds());
        }

        private int[] AllMemberIds()
        {
            int[] ids = new int[_state.Members.Count];
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = _state.Members[i].Id;
            }

            return ids;
        }

        private List<Member> LivingParty(IReadOnlyList<int> memberIds)
        {
            List<Member> party = new List<Member>(memberIds.Count);
            for (int i = 0; i < memberIds.Count; i++)
            {
                Member member = _state.FindMember(memberIds[i]);
                if (member != null)
                {
                    party.Add(member);
                }
            }

            return party;
        }

        private bool RemoveMember(int id)
        {
            for (int i = 0; i < _state.Members.Count; i++)
            {
                if (_state.Members[i].Id == id)
                {
                    _state.Members.RemoveAt(i);

                    // Góp ý 1 của Khoa ở playtest 01: mất thành viên thì mất tinh thần.
                    // Đặt ở đây có chủ đích — mọi cái chết đều đi qua hàm này (combat, đói, sự kiện),
                    // nên không có đường nào lọt.
                    int delta = _config.Data.Morale.OnMemberDeath;
                    _state.Morale += delta;
                    _moraleFromDeaths += delta;
                    return true;
                }
            }

            return false;
        }

        private void AddMorale(ActivityOutcome result, int amount)
        {
            result.Morale += amount;
            _state.Morale += amount;
        }
    }
}
