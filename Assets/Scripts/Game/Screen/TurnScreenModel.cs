using System;
using System.Collections.Generic;
using System.Globalization;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Neuron;

namespace BeforeUs.Game
{
    /// <summary>Một thẻ hoạt động trên màn chính.</summary>
    public sealed class ActivityCardModel
    {
        public ActivityKind Kind { get; set; }
        public string Name { get; set; }
        public string CostText { get; set; }
        public string EffectText { get; set; }

        /// <summary>Lấy thẳng từ Core (TurnPlan.IsUnlocked). Tầng này không tự suy điều kiện mở khóa.</summary>
        public bool Unlocked { get; set; }

        /// <summary>Vì sao khóa, bằng tiếng người. Null khi đã mở.</summary>
        public string LockReason { get; set; }

        /// <summary>
        /// Ngân sách năng lượng còn đủ để bật hoạt động này không — TurnPlan.CanAfford, không tự tính.
        /// Dùng để làm mờ thẻ TRƯỚC khi người chơi bấm, vì ngân sách năng lượng là quyết định
        /// trung tâm của mỗi lượt.
        ///
        /// Tách hẳn khỏi <see cref="Unlocked"/>: Săn bắt chưa mua nút thì khóa nhưng vẫn "đủ năng
        /// lượng", nên hai lý do làm mờ phải nói được riêng chứ không gộp một.
        ///
        /// Ca biên đừng tự tính lại: hoạt động ĐÃ có người thì thêm người nữa không tốn thêm năng
        /// lượng, nên CanAfford vẫn đúng kể cả khi năng lượng đã cạn (TurnPlan.EnergyCostToAdd = 0).
        /// </summary>
        public bool Affordable { get; set; }

        /// <summary>Vì sao thiếu năng lượng: "Cần 3 năng lượng, còn 2". Null khi đủ.</summary>
        public string UnaffordableReason { get; set; }

        public int PartySize { get; set; }
        public string PartyText { get; set; }
        public bool Selected { get; set; }

        /// <summary>Lỗi của thao tác gần nhất, nếu nó nhắm vào đúng hoạt động này.</summary>
        public string Message { get; set; }

        /// <summary>Gợi ý §13 neo vào đúng thẻ này, hoặc null.</summary>
        public HintModel Hint { get; set; }

        /// <summary>Khoá ảnh tạm của hoạt động, hoặc null nếu chưa có hình.</summary>
        public string ArtKey { get; set; }

        /// <summary>
        /// Những gì biết được TRƯỚC khi bấm kết thúc lượt, với Săn bắt và Khám phá (góp ý 3 của Khoa).
        /// Mỗi dòng đã tra chuỗi xong. Rỗng khi chưa cử ai hoặc hoạt động không mở trận.
        /// Con số do TurnSystem.PreviewCombat đưa ra; chỗ này chỉ đổi sang tiếng người.
        /// </summary>
        public List<string> PreviewLines { get; } = new List<string>();
    }

    /// <summary>Một ô thông tin trên thanh đầu màn hình. Có khoá để màn khác lọc bớt được.</summary>
    public sealed class HeaderChip
    {
        public HeaderChip(string key, string text)
        {
            Key = key;
            Text = text;
        }

        public string Key { get; }
        public string Text { get; }
    }

    /// <summary>Một dòng thành viên trong danh sách bộ tộc.</summary>
    public sealed class MemberRowModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>Một dòng gọn: "nhanh 1,08 · khỏe 1,02 · máu 4".</summary>
        public string StatsText { get; set; }

        /// <summary>Ba chỉ số tách riêng, để thẻ xếp thành cột được.</summary>
        public string SpeedText { get; set; }

        public string StrengthText { get; set; }
        public string HpText { get; set; }

        /// <summary>Chỉ số này DÙNG để làm gì (nghĩa v7, không phải nghĩa cũ của speed).</summary>
        public string SpeedHelp { get; set; }

        public string StrengthHelp { get; set; }
        public string HpHelp { get; set; }

        public string StatusText { get; set; }
        public bool Assigned { get; set; }

        /// <summary>Đang được soi. Lưới slot và thanh thẻ cùng đọc một nguồn.</summary>
        public bool Focused { get; set; }

        /// <summary>Người này có đang nằm trong hoạt động đang được chọn không.</summary>
        public bool InSelectedActivity { get; set; }

        /// <summary>Khoá ảnh tạm. Tầng này chỉ nói TÊN ảnh; nạp ảnh là việc của tầng giao diện.</summary>
        public string ArtKey { get; set; }
    }

    /// <summary>
    /// Toàn bộ nội dung màn chính, đã tra chuỗi xong, dưới dạng dữ liệu thuần.
    /// Lớp hiển thị chỉ việc đổ vào UXML — nó không đọc GameState và không tính gì.
    ///
    /// Mọi con số ở đây đều do Core trả về. Hái lượm v7: thức ăn KHÔNG suy ra được từ số người
    /// bằng bất kỳ phép nhân nào — `TurnSystem.ForecastGather()` trả phần chắc chắn và phần kỳ vọng,
    /// chỗ này chỉ diễn đạt lại. `speed` không còn nhân vào lượng (D4 playtest 01).
    /// </summary>
    public sealed class TurnScreenModel
    {
        private readonly List<HeaderChip> _headerChips = new List<HeaderChip>();
        private readonly List<ActivityCardModel> _activities = new List<ActivityCardModel>();
        private readonly List<MemberRowModel> _members = new List<MemberRowModel>();
        private readonly List<HintModel> _topHints = new List<HintModel>();

        public string Title { get; set; }

        /// <summary>Các ô thông tin trên thanh đầu màn hình, đã định dạng xong.</summary>
        public List<HeaderChip> HeaderChips
        {
            get { return _headerChips; }
        }

        public string EnergyText { get; set; }

        /// <summary>
        /// Nhuệ khí dạng thanh giá trị (góp ý 2 của Khoa). Ngưỡng hỏi Core
        /// (NodeEffects.BirthMoraleCost) chứ không đọc thẳng balance.json: nút "Cõng con trên lưng"
        /// hạ ngưỡng xuống 3, đọc thẳng là hiện sai ngay sau khi người chơi mua nút đó.
        ///
        /// design.md §6 vốn đã GIỮ phần dư — sinh xong chỉ trừ đi đúng ngưỡng. Nên đây thuần là
        /// chuyện hiển thị, không có luật nào đổi.
        /// </summary>
        public int MoraleValue { get; set; }

        public int MoraleThreshold { get; set; }
        public string MoraleText { get; set; }
        public string MoraleHint { get; set; }

        /// <summary>Phần đầy của thanh, 0..1. Tràn ngưỡng thì chặn ở 1.</summary>
        public float MoraleFill { get; set; }

        /// <summary>Đủ nhuệ khí nhưng hang đã đầy nên chưa sinh được (design.md §6).</summary>
        public bool MoraleBlockedByLair { get; set; }
        public int EnergyMax { get; set; }
        public int EnergySpent { get; set; }

        public string ForecastTitle { get; set; }
        public string ForecastNeedText { get; set; }
        public string ForecastHaveText { get; set; }

        /// <summary>Kỳ vọng toán học của hái lượm (v7). KHÔNG phải con số sẽ xảy ra.</summary>
        public string ForecastExpectedText { get; set; }

        /// <summary>Sàn Kiếm ăn (§A.6) và lý do chưa kết thúc lượt được. Phán quyết của Core.</summary>
        public string GatherFloorText { get; set; }

        public bool CanEndTurn { get; set; }
        public string EndTurnBlockedReason { get; set; }
        public string ForecastStatusText { get; set; }
        public string ForecastNote { get; set; }
        public bool ForecastShort { get; set; }

        public List<ActivityCardModel> Activities
        {
            get { return _activities; }
        }

        public string MembersTitle { get; set; }

        public List<MemberRowModel> Members
        {
            get { return _members; }
        }

        public string HintText { get; set; }
        public string PlanRuleText { get; set; }
        public string EndTurnText { get; set; }

        /// <summary>Banner báo trước đổi pha khí hậu (design.md §10). Null khi chưa tới lúc báo.</summary>
        public string ClimateBannerText { get; set; }

        /// <summary>Gợi ý §13 neo vào banner khí hậu, hoặc null.</summary>
        public HintModel ClimateHint { get; set; }

        /// <summary>Gợi ý §13 không neo vào thẻ nào: hiện thành một thanh dưới thanh thông tin.</summary>
        public List<HintModel> TopHints
        {
            get { return _topHints; }
        }

        /// <summary>Nút mở màn mạng lưới neuron, kèm dấu khi đang có nút mở được.</summary>
        public string NetworkButtonText { get; set; }

        public bool HintsEnabled { get; set; }
        public string HintsToggleText { get; set; }
        public string HintCatalogButtonText { get; set; }

        public static TurnScreenModel Build(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            BalanceConfig config = session.Config;
            GameState state = session.State;
            TurnSystem turn = session.Turn;
            TurnPlan plan = turn.Plan;
            StringTable s = session.Strings;
            TurnPlanEditor editor = session.Editor;

            TurnScreenModel model = new TurnScreenModel();
            model.Title = s.Get("app.title");

            model.HeaderChips.Add(new HeaderChip("turn", s.Format("top.turn", state.Turn)));
            model.HeaderChips.Add(new HeaderChip("stage", s.Format("top.stage", state.Stage, StageName(s, state.Stage))));
            model.HeaderChips.Add(new HeaderChip("region", s.Format("top.region", RegionName(config, state.Region))));
            model.HeaderChips.Add(new HeaderChip("climate",
                s.Get(turn.Phase == ClimatePhase.Dry ? "top.climate.dry" : "top.climate.wet")));
            model.HeaderChips.Add(new HeaderChip("neuron", s.Format("top.neuron", state.Neuron)));
            model.HeaderChips.Add(new HeaderChip("morale", s.Format("top.morale", state.Morale)));

            // Khoá "build": màn cộng đồng lọc bỏ ô này. build_level của Core vẫn là cơ chế v6
            // (điểm xây), còn màn cộng đồng đếm slot đã dọn — hiện cả hai là bày ra hai con số
            // mâu thuẫn. Một con số sai còn đỡ hơn hai con số cãi nhau.
            model.HeaderChips.Add(new HeaderChip("build", s.Format("top.build", state.BuildLevel, state.BuildPoints)));
            model.HeaderChips.Add(new HeaderChip("lair", s.Format("top.lair", state.Crew, state.LairCapacity(config))));
            model.HeaderChips.Add(new HeaderChip("difficulty", s.Format("top.difficulty", turn.DifficultyLevel)));

            model.EnergyMax = plan.EnergyMax;
            model.EnergySpent = plan.EnergySpent;
            model.EnergyText = s.Format("top.energy", plan.EnergySpent, plan.EnergyMax);

            BuildMoraleBar(model, session);

            List<HintModel> hints = HintRules.ActiveHints(session);
            model.HintsEnabled = session.Hints.Enabled;
            model.HintsToggleText = s.Get(session.Hints.Enabled ? "hint.toggle.on" : "hint.toggle.off");
            model.HintCatalogButtonText = s.Get("hint.catalog.open");
            model.NetworkButtonText = s.Get("neuron.open")
                + (HasBuyableNode(session) ? " · " + s.Get("neuron.badge.canbuy") : string.Empty);
            model.ClimateHint = HintAt(hints, HintAnchor.Climate);
            for (int i = 0; i < hints.Count; i++)
            {
                if (hints[i].Anchor == HintAnchor.Top)
                {
                    model.TopHints.Add(hints[i]);
                }
            }

            model.ClimateBannerText = ClimateBanner(session);

            BuildForecast(model, session);
            BuildActivities(model, session, hints);
            BuildMembers(model, session);

            model.HintText = editor.SelectedActivity == null
                ? s.Get("hint.select.activity")
                : s.Format("hint.selected.activity", ActivityName(s, editor.SelectedActivity.Value));
            model.PlanRuleText = s.Get("hint.plan.free");
            model.EndTurnText = s.Get("button.endturn");

            // Sàn Kiếm ăn: cả con số lẫn phán quyết "đã đủ chưa" đều của Core, kể cả ca suy biến
            // (không đủ người hoặc Kiếm ăn bị khóa thì Core cố ý KHÔNG chặn).
            model.GatherFloorText = plan.MeetsGatherFloor
                ? s.Format("gather.floor.met", plan.GatherFloor)
                : s.Format("gather.floor.short", plan.GatherFloor, plan.PartySizeOn(ActivityKind.Gather));

            PlanResult endTurn = plan.ValidateForEndTurn();
            model.CanEndTurn = endTurn.Succeeded;
            model.EndTurnBlockedReason = endTurn.Succeeded ? null : ExplainPlanError(s, endTurn.Error);

            return model;
        }

        private static void BuildMoraleBar(TurnScreenModel model, GameSession session)
        {
            StringTable s = session.Strings;
            GameState state = session.State;

            int threshold = Effects(session).BirthMoraleCost(session.Config.Data.Birth.MoraleCost);
            model.MoraleValue = state.Morale;
            model.MoraleThreshold = threshold;
            model.MoraleText = s.Format("morale.bar", state.Morale, threshold);
            model.MoraleFill = threshold <= 0
                ? 1f
                : Math.Min(1f, (float)state.Morale / threshold);

            model.MoraleBlockedByLair = state.Crew >= state.LairCapacity(session.Config);
            model.MoraleHint = model.MoraleBlockedByLair
                ? s.Get("morale.bar.lairfull")
                : s.Format("morale.bar.hint", threshold);
        }

        /// <summary>Hiệu ứng nút đang có hiệu lực. NodeEffects.None khi chưa có mạng lưới.</summary>
        private static NodeEffects Effects(GameSession session)
        {
            return session.Network != null ? session.Network.Effects : NodeEffects.None;
        }

        private static void BuildForecast(TurnScreenModel model, GameSession session)
        {
            StringTable s = session.Strings;
            TurnSystem turn = session.Turn;

            int needed = turn.FoodNeeded;
            int have = turn.ForecastFood;

            model.ForecastTitle = s.Get("forecast.title");
            model.ForecastNeedText = s.Format("forecast.need", needed);
            model.ForecastHaveText = s.Format("forecast.have", have);
            model.ForecastExpectedText = s.Format(
                "forecast.expected", Formatting.Amount(turn.ForecastGather().Expected));
            model.ForecastNote = s.Get("forecast.note");
            model.ForecastShort = have < needed;
            model.ForecastStatusText = model.ForecastShort
                ? s.Format("forecast.short", needed - have, session.Config.Data.StarvationDeaths)
                : s.Get("forecast.ok");
        }

        /// <summary>
        /// Banner "sắp đổi pha" của design.md §10. Lượt nào đáng báo, còn mấy lượt, và pha sắp tới
        /// là gì — cả ba đều hỏi Climate, không đếm lượt lấy.
        /// </summary>
        private static string ClimateBanner(GameSession session)
        {
            Climate climate = session.Climate;
            int turn = session.State.Turn;
            if (!climate.IsPhaseChangeImminent(turn))
            {
                return null;
            }

            int turnsLeft = climate.TurnsUntilPhaseChange(turn);
            ClimatePhase next = climate.PhaseAt(turn + turnsLeft);
            return session.Strings.Format(
                next == ClimatePhase.Dry ? "climate.banner.to.dry" : "climate.banner.to.wet",
                turnsLeft);
        }

        /// <summary>
        /// Có nút nào mở được ngay không — hỏi NeuronNetwork.CanBuy từng nút, không so neuron với giá.
        /// Dùng để gắn dấu lên nút mở màn mạng lưới, để người chơi không phải mở ra dò.
        /// </summary>
        private static bool HasBuyableNode(GameSession session)
        {
            NeuronNetwork network = session.Network;
            if (network == null)
            {
                return false;
            }

            IReadOnlyList<NeuronNode> nodes = network.NodesOfStage(session.State.Stage);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (network.CanBuy(nodes[i].Id).Succeeded)
                {
                    return true;
                }
            }

            return false;
        }

        private static HintModel HintAt(List<HintModel> hints, HintAnchor anchor)
        {
            for (int i = 0; i < hints.Count; i++)
            {
                if (hints[i].Anchor == anchor)
                {
                    return hints[i];
                }
            }

            return null;
        }

        private static HintModel HintFor(List<HintModel> hints, ActivityKind kind)
        {
            switch (kind)
            {
                case ActivityKind.Gather: return HintAt(hints, HintAnchor.Gather);
                case ActivityKind.Morale: return HintAt(hints, HintAnchor.Morale);
                case ActivityKind.Explore: return HintAt(hints, HintAnchor.Explore);
                default: return null;
            }
        }

        private static void BuildActivities(TurnScreenModel model, GameSession session, List<HintModel> hints)
        {
            BalanceConfig config = session.Config;
            TurnPlan plan = session.Turn.Plan;
            StringTable s = session.Strings;
            TurnPlanEditor editor = session.Editor;

            for (int i = 0; i < ActivityRules.All.Length; i++)
            {
                ActivityKind kind = ActivityRules.All[i];

                ActivityCardModel card = new ActivityCardModel();
                card.Kind = kind;
                card.Name = ActivityName(s, kind);
                card.CostText = s.Format("activity.cost", config.EnergyCostOf(kind), config.NeuronPerMemberOf(kind));
                card.Unlocked = plan.IsUnlocked(kind);
                card.LockReason = card.Unlocked ? null : ExplainLockOf(session, kind);
                card.PartySize = plan.PartySizeOn(kind);
                card.PartyText = card.PartySize == 0
                    ? s.Get("activity.party.none")
                    : s.Format("activity.party.some", card.PartySize);
                card.Selected = editor.SelectedActivity == kind;
                card.EffectText = EffectText(session, kind, card);
                card.Hint = HintFor(hints, kind);
                card.ArtKey = PlaceholderArtKeys.ForActivity(kind);
                BuildPreview(card, session, kind);

                card.Affordable = plan.CanAfford(kind);
                card.UnaffordableReason = card.Affordable
                    ? null
                    : s.Format("activity.unaffordable", plan.EnergyCostToAdd(kind), plan.EnergyRemaining);

                if (editor.LastResult != null && !editor.LastResult.Succeeded && editor.LastResultActivity == kind)
                {
                    card.Message = ExplainPlanError(s, editor.LastResult.Error);
                }

                model.Activities.Add(card);
            }
        }

        /// <summary>
        /// Xem trước trận. Core trả dữ liệu thô và cố ý KHÔNG quy ra thang "dễ / vừa / khó";
        /// việc đổi sang chữ là của chỗ này, việc tính con số là của Core.
        ///
        /// Giai đoạn 1 thắng săn vẫn được 0 thức ăn (hunt_food_per_member[0] = 0). Hiện thẳng,
        /// không giấu và không làm tròn lên — đó là một cái bẫy thật trong luật, Khoa cần thấy.
        /// </summary>
        private static void BuildPreview(ActivityCardModel card, GameSession session, ActivityKind kind)
        {
            CombatPreview preview = session.Turn.PreviewCombat(kind);
            if (!preview.HasParty)
            {
                return;
            }

            StringTable s = session.Strings;
            card.PreviewLines.Add(s.Format("activity.preview.party", preview.PartySize));

            if (kind == ActivityKind.Hunt)
            {
                card.PreviewLines.Add(preview.FoodIfWon == 0
                    ? s.Get("activity.preview.food.zero")
                    : s.Format("activity.preview.food", preview.FoodIfWon));
            }

            card.PreviewLines.Add(preview.WinChance == null
                ? s.Get("activity.preview.winchance.unknown")
                : s.Format("activity.preview.winchance", Formatting.Percent(preview.WinChance.Value)));
        }

        private static void BuildMembers(TurnScreenModel model, GameSession session)
        {
            GameState state = session.State;
            StringTable s = session.Strings;
            TurnPlanEditor editor = session.Editor;
            model.MembersTitle = s.Format("members.title", state.Crew);

            for (int i = 0; i < state.Members.Count; i++)
            {
                Member member = state.Members[i];
                ActivityKind? activity = editor.FindActivityOf(member.Id);

                MemberRowModel row = new MemberRowModel();
                row.Id = member.Id;
                row.Name = s.Format("member.name", member.Id);
                row.SpeedText = s.Format("member.stat.speed", Formatting.Stat(member.Speed));
                row.StrengthText = s.Format("member.stat.strength", Formatting.Stat(member.Strength));
                row.HpText = s.Format("member.stat.hp", member.Hp);
                row.SpeedHelp = s.Get("member.stat.speed.help");
                row.StrengthHelp = s.Get("member.stat.strength.help");
                row.HpHelp = s.Get("member.stat.hp.help");
                row.StatsText = row.SpeedText + " · " + row.StrengthText + " · " + row.HpText;
                row.Focused = editor.FocusedMemberId == member.Id;
                row.Assigned = activity != null;
                row.ArtKey = PlaceholderArtKeys.ForMember(member.Id, activity != null);
                row.InSelectedActivity = activity != null && activity == editor.SelectedActivity;

                if (activity == null)
                {
                    row.StatusText = s.Get("member.idle");
                }
                else
                {
                    row.StatusText = s.Format("member.busy", ActivityName(s, activity.Value));
                }

                model.Members.Add(row);
            }
        }

        private static string EffectText(GameSession session, ActivityKind kind, ActivityCardModel card)
        {
            StringTable s = session.Strings;
            BalanceConfig config = session.Config;
            TurnSystem turn = session.Turn;

            switch (kind)
            {
                case ActivityKind.Gather:
                {
                    // v7 §A (D4): thức ăn KHÔNG suy ra được từ số người bằng bất kỳ phép nhân nào.
                    // Nó là tổng của những lần rút riêng của từng người với từng loài đã biết.
                    // Core trả hai con số qua ForecastGather; chỗ này chỉ diễn đạt lại.
                    string text = s.Format("activity.effect.gather", turn.Plan.GatherFloor);
                    if (turn.Plan.PartySizeOn(ActivityKind.Gather) == 0)
                    {
                        return text;
                    }

                    GatherForecast forecast = turn.ForecastGather();
                    return text + " " + s.Format(
                        "activity.effect.gather.forecast",
                        forecast.Guaranteed,
                        Formatting.Amount(forecast.Expected));
                }

                case ActivityKind.Morale:
                    return s.Format("activity.effect.morale", config.Data.Activities.Morale.MoralePerMember);

                case ActivityKind.Build:
                    return s.Get("activity.effect.build");

                case ActivityKind.Hunt:
                    return s.Get("activity.effect.hunt");

                default:
                    return s.Get("activity.effect.explore");
            }
        }

        /// <summary>
        /// Vì sao một hoạt động đang khóa. Trạng thái khóa là của Core; chỗ này chỉ đọc điều kiện
        /// đã ghi trong balance.json rồi nói lại bằng tiếng người (D1: người chơi phải hiểu vì sao,
        /// không thì tưởng game hỏng).
        /// </summary>
        public static string ExplainLockOf(GameSession session, ActivityKind kind)
        {
            StringTable s = session.Strings;
            UnlockRule rule = session.Config.UnlockOf(kind);

            if (rule.Kind == UnlockKind.Node)
            {
                // Tên nút là nội dung của Khoa và đã nằm trong neurons.json; hỏi mạng lưới chứ
                // đừng giữ bản sao thứ hai trong strings.vi.json.
                NeuronNode node = session.Network == null ? null : session.Network.Node(rule.NodeId);
                return node != null
                    ? s.Format("lock.node", node.Name)
                    : s.Format("lock.node.unnamed", rule.NodeId);
            }

            if (rule.Kind == UnlockKind.Turn && session.State.Turn < rule.FromTurn)
            {
                return s.Format("lock.turn", rule.FromTurn);
            }

            // Điều kiện trong balance.json đã đạt mà Core vẫn báo khóa: lý do còn lại duy nhất
            // được design.md §5 ghi là Khám phá đã hết vùng để đi.
            if (kind == ActivityKind.Explore && session.State.Region >= session.Config.RegionMaxIndex)
            {
                return s.Get("lock.explore.noregion");
            }

            return s.Get("lock.unknown");
        }

        public static string ExplainPlanError(StringTable s, PlanError error)
        {
            string key = "plan.error." + error;
            return s.Has(key) ? s.Get(key) : s.Get("plan.error.unknown");
        }

        public static string ActivityName(StringTable s, ActivityKind kind)
        {
            return s.Get("activity." + ActivityKey(kind) + ".name");
        }

        public static string ActivityKey(ActivityKind kind)
        {
            switch (kind)
            {
                case ActivityKind.Gather: return "gather";
                case ActivityKind.Morale: return "morale";
                case ActivityKind.Build: return "build";
                case ActivityKind.Hunt: return "hunt";
                default: return "explore";
            }
        }

        public static string StageName(StringTable s, int stage)
        {
            string key = "stage." + stage.ToString(CultureInfo.InvariantCulture) + ".name";
            return s.Has(key) ? s.Get(key) : s.Get("stage.unknown");
        }

        public static string RegionName(BalanceConfig config, int region)
        {
            string[] names = config.Data.Regions.Names;
            if (names != null && region >= 0 && region < names.Length)
            {
                return names[region];
            }

            return region.ToString(CultureInfo.InvariantCulture);
        }

    }
}
