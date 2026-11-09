using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Sites;

namespace BeforeUs.Game
{
    /// <summary>Dáng mặc định của một slot. Thuộc về SLOT, không thuộc về người đứng trong đó.</summary>
    public enum SlotPose
    {
        Sit,
        Stand
    }

    /// <summary>Một chỗ đứng/ngồi cố định trong cụm cộng đồng.</summary>
    public sealed class MemberSlotModel
    {
        /// <summary>Đếm từ 1, đúng như Khoa nói "slot 1 ngồi, slot 2 đứng".</summary>
        public int SlotIndex { get; set; }

        public SlotPose Pose { get; set; }
        public bool Occupied { get; set; }

        /// <summary>Thông tin người đang ở slot này, hoặc null khi slot trống.</summary>
        public MemberRowModel Member { get; set; }

        public string ArtKey { get; set; }
        public string Tooltip { get; set; }

        /// <summary>Tên hiện NGAY TRÊN ĐẦU slot (góp ý của Khoa, playtest màn cộng đồng).</summary>
        public string NameText { get; set; }

        /// <summary>Dòng trạng thái gọn dưới tên: đang làm gì.</summary>
        public string StatusText { get; set; }

        public bool Focused { get; set; }

        /// <summary>Chỗ này nằm ngoài sức chứa của vùng: có người nhưng hang không đủ chỗ.</summary>
        public bool OverCapacity { get; set; }
    }

    /// <summary>Thứ bấm được trong cụm cộng đồng.</summary>
    public enum CommunityTargetKind
    {
        None,
        FoodNest,
        Club,

        /// <summary>Lối mòn ra vùng mới: Khám phá. Thắng là di cư, và di cư là đi hẳn.</summary>
        Trail,

        Hub,

        /// <summary>Một slot công trình quanh hang (design-v7 Phần B).</summary>
        Site
    }

    /// <summary>Một vật thể bấm được: ổ thức ăn, cây gậy, trung tâm.</summary>
    public sealed class CommunityObjectModel
    {
        public CommunityTargetKind Kind { get; set; }
        public string Name { get; set; }
        public string ArtKey { get; set; }
        public string StatusText { get; set; }

        /// <summary>Bấm được không. Lý do khóa lấy từ Core, không tự suy.</summary>
        public bool Enabled { get; set; }

        public string DisabledReason { get; set; }
        public bool Selected { get; set; }

        /// <summary>Id slot công trình, hoặc null với ổ thức ăn / cây gậy / trung tâm.</summary>
        public string SiteId { get; set; }

        /// <summary>Dòng phụ: ngưỡng sức mạnh, bậc ổ thức ăn… Rỗng nếu không có.</summary>
        public string SubText { get; set; }

        /// <summary>
        /// Gợi ý §13 neo vào chính vật thể này. Màn A1 neo gợi ý vào thẻ hoạt động; màn cộng đồng
        /// không có thẻ, nên gợi ý phải chuyển sang vật thể tương ứng — nếu không chúng biến mất
        /// khỏi màn mặc định mà không ai thấy.
        /// </summary>
        public HintModel Hint { get; set; }
    }

    /// <summary>Bảng giao việc mở ra khi bấm một vật thể.</summary>
    public sealed class TaskPanelModel
    {
        private readonly List<string> _lines = new List<string>();

        public bool Open { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }

        public List<string> Lines
        {
            get { return _lines; }
        }

        public string PartyText { get; set; }
        public string ClearText { get; set; }
        public string CloseText { get; set; }
        public string Message { get; set; }

        /// <summary>Bảng chọn người di cư, chỉ có nghĩa với Khám phá.</summary>
        public MigrationPanelModel Migration { get; set; }
    }

    /// <summary>Một người trong bảng chọn di cư.</summary>
    public sealed class MigrationMemberModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Chosen { get; set; }
    }

    /// <summary>Bảng chọn ai theo sang vùng mới (design-v7 §F.2).</summary>
    public sealed class MigrationPanelModel
    {
        private readonly List<MigrationMemberModel> _members = new List<MigrationMemberModel>();

        /// <summary>Có đang cử người đi Khám phá và vùng mới có chỗ không.</summary>
        public bool Required { get; set; }

        public int Capacity { get; set; }
        public string Title { get; set; }
        public string ChosenText { get; set; }
        public string LeftBehindText { get; set; }
        public string ChooseAllText { get; set; }
        public bool CanChooseAll { get; set; }

        public List<MigrationMemberModel> Members
        {
            get { return _members; }
        }
    }

    /// <summary>
    /// Màn hình cộng đồng (góp ý 6–7 của Khoa, playtest 01).
    ///
    /// Thành viên đứng/ngồi theo SLOT cố định; bấm vật thể để giao việc. Mọi phán quyết vẫn của
    /// Core: số slot lấy GameState.LairCapacity, mở khóa lấy TurnPlan.IsUnlocked, ngân sách lấy
    /// CanAfford, xem trước trận lấy PreviewCombat.
    ///
    /// Dùng lại <see cref="TurnScreenModel"/> cho phần đầu màn hình và dự báo thay vì chép lại —
    /// hai màn chia nhau một nguồn, nên sửa một chỗ là cả hai đúng.
    /// </summary>
    public sealed class CommunityScreenModel
    {
        private readonly List<MemberSlotModel> _slots = new List<MemberSlotModel>();
        private readonly List<HeaderChip> _headerChips = new List<HeaderChip>();
        private readonly List<CommunityObjectModel> _sites = new List<CommunityObjectModel>();

        /// <summary>Phần dùng chung với màn A1: thanh đầu, năng lượng, nhuệ khí, dự báo, gợi ý.</summary>
        public TurnScreenModel Shared { get; set; }

        /// <summary>
        /// Ô thông tin của RIÊNG màn này: đã bỏ ô "Công trình" của v6.
        ///
        /// build_level của Core vẫn chạy theo điểm xây v6, còn màn cộng đồng sẽ đếm slot đã dọn
        /// (v7 §B.2) — hiện cả hai là bày ra hai con số cãi nhau trước mặt người chơi.
        /// Quyết định "hiện gì" thuộc về model để bài kiểm với tới được, không nằm trong view.
        /// </summary>
        public List<HeaderChip> HeaderChips
        {
            get { return _headerChips; }
        }

        public List<MemberSlotModel> Slots
        {
            get { return _slots; }
        }

        public CommunityObjectModel FoodNest { get; set; }
        public CommunityObjectModel Club { get; set; }

        /// <summary>Lối vào Khám phá. Thiếu nó thì màn cộng đồng không đi khám phá được.</summary>
        public CommunityObjectModel Trail { get; set; }
        public CommunityObjectModel Hub { get; set; }

        /// <summary>Các slot công trình đang hiện ra quanh hang. Nguồn: ISiteSystem.</summary>
        public List<CommunityObjectModel> Sites
        {
            get { return _sites; }
        }

        public string SitesTitle { get; set; }
        public string ClearedCountText { get; set; }

        public TaskPanelModel Task { get; set; }

        /// <summary>
        /// Thanh thẻ thành viên ở dưới màn hình (góp ý của Khoa).
        ///
        /// Chứa MỌI thành viên, không chỉ người đã được cử: phải nhìn chỉ số mới quyết được
        /// cử ai, nên người đang rảnh cũng phải có mặt. Trạng thái làm việc lấy từ TurnPlan
        /// qua TurnScreenModel — không suy từ trạng thái giao diện.
        /// </summary>
        public List<MemberRowModel> MemberCards
        {
            get { return Shared.Members; }
        }

        public string MemberBarTitle { get; set; }
        public string MemberBarHint { get; set; }

        /// <summary>Hai nút của §13 bị rơi khi chuyển màn: tắt gợi ý và tra cứu gợi ý.</summary>
        public string HintsToggleText { get; set; }

        public string HintCatalogText { get; set; }

        /// <summary>Hai dòng dự báo v7 mà cả hai màn đều chưa từng vẽ.</summary>
        public string ForecastExpectedText { get; set; }

        public string ForecastNote { get; set; }

        public string CommunityTitle { get; set; }
        public string EndTurnText { get; set; }

        public static CommunityScreenModel Build(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            StringTable s = session.Strings;

            CommunityScreenModel model = new CommunityScreenModel();
            model.Shared = TurnScreenModel.Build(session);
            for (int i = 0; i < model.Shared.HeaderChips.Count; i++)
            {
                HeaderChip chip = model.Shared.HeaderChips[i];
                if (chip.Key != "build")
                {
                    model.HeaderChips.Add(chip);
                }
            }

            int crew = session.State.Crew;
            int capacity = session.State.LairCapacity(session.Config);
            model.CommunityTitle = crew > capacity
                ? s.Format("community.title.over", crew, capacity)
                : s.Format("community.title", crew, capacity);
            model.EndTurnText = s.Get("button.endturn");
            model.MemberBarTitle = s.Get("members.bar.title");
            model.MemberBarHint = s.Get("members.bar.hint");
            model.HintsToggleText = model.Shared.HintsToggleText;
            model.HintCatalogText = model.Shared.HintCatalogButtonText;
            model.ForecastExpectedText = model.Shared.ForecastExpectedText;
            model.ForecastNote = model.Shared.ForecastNote;

            BuildSlots(model, session);

            model.FoodNest = BuildObject(session, CommunityTargetKind.FoodNest, ActivityKind.Gather,
                "community.nest.name", "object_food_nest");
            model.FoodNest.SubText = NestTierText(session);

            model.Club = BuildObject(session, CommunityTargetKind.Club, ActivityKind.Hunt,
                "community.club.name", "object_club");
            model.Trail = BuildObject(session, CommunityTargetKind.Trail, ActivityKind.Explore,
                "community.trail.name", "object_trail");
            model.Hub = BuildHub(session);

            // Chuyển gợi ý §13 từ thẻ hoạt động sang vật thể tương ứng. Đặt SAU khi đã dựng
            // đủ bốn vật thể — gán vào thứ chưa tồn tại là cách màn hình sập ngay khi mở.
            model.FoodNest.Hint = HintOnCard(model.Shared, ActivityKind.Gather);
            model.Club.Hint = HintOnCard(model.Shared, ActivityKind.Hunt);
            model.Trail.Hint = HintOnCard(model.Shared, ActivityKind.Explore);
            model.Hub.Hint = HintOnCard(model.Shared, ActivityKind.Morale);
            BuildSites(model, session);

            model.Task = BuildTask(session, model.Shared);
            return model;
        }

        /// <summary>
        /// Slot cố định: số lượng = sức chứa hang của Core. Dáng xen kẽ theo chỉ số slot,
        /// đúng nguyên văn Khoa "slot 1 ngồi, slot 2 đứng" — không hoạt ảnh sinh hoạt ngẫu nhiên.
        /// Người xếp vào slot theo thứ tự id nên vị trí ổn định giữa các lượt.
        /// </summary>
        private static void BuildSlots(CommunityScreenModel model, GameSession session)
        {
            StringTable s = session.Strings;
            int capacity = session.State.LairCapacity(session.Config);
            List<MemberRowModel> rows = model.Shared.Members;

            // Sĩ số CÓ THỂ vượt sức chứa: vùng 0 mở sẵn 4 chỗ mà ván bắt đầu với 5 người.
            // Sức chứa chỉ chặn SINH THÊM, không giết ai. Vẽ đúng số chỗ rồi bỏ rơi người thứ 5
            // là nói dối người chơi về bộ tộc của họ — vẽ thêm chỗ và ghi rõ là quá tải.
            int slots = capacity > rows.Count ? capacity : rows.Count;

            for (int i = 0; i < slots; i++)
            {
                MemberSlotModel slot = new MemberSlotModel();
                slot.SlotIndex = i + 1;
                slot.Pose = (i % 2 == 0) ? SlotPose.Sit : SlotPose.Stand;
                slot.Occupied = i < rows.Count;
                slot.OverCapacity = i >= capacity;

                if (slot.Occupied)
                {
                    MemberRowModel row = rows[i];
                    slot.Member = row;
                    slot.ArtKey = PlaceholderArtKeys.ForMemberPose(row.Id, slot.Pose == SlotPose.Stand);
                    slot.NameText = row.Name;
                    slot.StatusText = row.StatusText;
                    slot.Focused = row.Focused;
                    slot.Tooltip = row.Name + " — " + row.StatusText + " · " + row.StatsText;
                }
                else
                {
                    slot.Tooltip = s.Format("community.slot.empty", slot.SlotIndex);
                }

                if (slot.OverCapacity && slot.Occupied)
                {
                    slot.Tooltip = slot.Tooltip + " — " + s.Get("community.slot.over");
                }

                model.Slots.Add(slot);
            }
        }

        private static CommunityObjectModel BuildObject(
            GameSession session, CommunityTargetKind kind, ActivityKind activity, string nameKey, string artKey)
        {
            StringTable s = session.Strings;
            TurnPlan plan = session.Turn.Plan;

            CommunityObjectModel obj = new CommunityObjectModel();
            obj.Kind = kind;
            obj.Name = s.Get(nameKey);
            obj.ArtKey = artKey;
            obj.Enabled = plan.IsUnlocked(activity);
            obj.Selected = session.Editor.SelectedActivity == activity;

            int party = plan.PartySizeOn(activity);
            obj.StatusText = party == 0
                ? s.Get("activity.party.none")
                : s.Format("activity.party.some", party);

            if (!obj.Enabled)
            {
                obj.DisabledReason = TurnScreenModel.ExplainLockOf(session, activity);
            }
            else if (!plan.CanAfford(activity))
            {
                obj.DisabledReason = s.Format(
                    "activity.unaffordable", plan.EnergyCostToAdd(activity), plan.EnergyRemaining);
            }

            return obj;
        }

        /// <summary>
        /// Slot công trình. Danh sách, ngưỡng và trạng thái đều hỏi ISiteSystem — màn hình không
        /// tự quyết slot nào hiện ra, và không tự so sức mạnh với ngưỡng.
        /// </summary>
        private static void BuildSites(CommunityScreenModel model, GameSession session)
        {
            StringTable s = session.Strings;
            model.SitesTitle = s.Get("site.title");
            model.ClearedCountText = s.Format("site.cleared.count", session.Sites.ClearedCount);

            IReadOnlyList<SiteView> sites = session.Sites.VisibleSites(session.State.Stage, session.Network);
            IReadOnlyList<int> builders = session.Turn.Plan.MembersOn(ActivityKind.Build);
            bool buildUnlocked = session.Turn.Plan.IsUnlocked(ActivityKind.Build);

            for (int i = 0; i < sites.Count; i++)
            {
                SiteView site = sites[i];
                if (site.Kind == SiteKind.FoodNest)
                {
                    continue; // Ổ thức ăn có ô riêng bên trái.
                }

                CommunityObjectModel obj = new CommunityObjectModel();
                obj.Kind = CommunityTargetKind.Site;
                obj.SiteId = site.Id;
                obj.Name = s.Get("site." + site.Id + ".name");
                obj.ArtKey = site.ArtKey;
                obj.Enabled = buildUnlocked;
                obj.Selected = session.Editor.SelectedSiteId == site.Id;

                double needed = site.StrengthRequired;
                obj.StatusText = s.Format("site.strength", Formatting.Amount(needed));

                if (obj.Selected)
                {
                    double have = session.Sites.StrengthOf(builders);
                    obj.SubText = s.Format("site.strength.now", Formatting.Amount(have));
                }
                else
                {
                    obj.SubText = s.Get("site." + site.Id + ".desc");
                }

                if (!buildUnlocked)
                {
                    obj.DisabledReason = TurnScreenModel.ExplainLockOf(session, ActivityKind.Build);
                }

                model.Sites.Add(obj);
            }
        }

        /// <summary>Bậc hiện tại của Ổ đựng thức ăn (§B.4). Tên bậc là nội dung, nằm trong file chuỗi.</summary>
        private static string NestTierText(GameSession session)
        {
            StringTable s = session.Strings;
            SiteView nest = session.Sites.Find(session.Sites.Catalog.FoodNestId);
            if (nest == null)
            {
                return string.Empty;
            }

            string tier = s.Format(
                "site.nest.tier", nest.Tier, nest.MaxTier, s.Get("site.nest.tier." + nest.Tier));

            // Hết bậc thì Core bỏ ổ khỏi danh sách nâng cấp ở menu trung tâm; ô bên trái vẫn còn
            // vì nó là lối tắt Kiếm ăn, nên phải tự nói rằng không nâng được nữa.
            return nest.Tier >= nest.MaxTier ? tier + " · " + s.Get("site.nest.max") : tier;
        }

        private static HintModel HintOnCard(TurnScreenModel shared, ActivityKind kind)
        {
            for (int i = 0; i < shared.Activities.Count; i++)
            {
                if (shared.Activities[i].Kind == kind)
                {
                    return shared.Activities[i].Hint;
                }
            }

            return null;
        }

        private static CommunityObjectModel BuildHub(GameSession session)
        {
            StringTable s = session.Strings;

            CommunityObjectModel hub = new CommunityObjectModel();
            hub.Kind = CommunityTargetKind.Hub;
            hub.Name = s.Get("community.hub.name");
            hub.StatusText = s.Get("community.hub.hint");
            hub.Enabled = true;
            return hub;
        }

        private static TaskPanelModel BuildTask(GameSession session, TurnScreenModel shared)
        {
            StringTable s = session.Strings;
            TaskPanelModel task = new TaskPanelModel();
            task.CloseText = s.Get("community.task.close");
            task.ClearText = s.Get("activity.clear");

            ActivityKind? activity = session.Editor.SelectedActivity;
            if (activity == null)
            {
                task.Open = false;
                task.Title = s.Get("community.task.none");
                return task;
            }

            // Dùng lại thẻ đã dựng trong Shared: dựng lại lần hai vừa phí vừa có nguy cơ
            // hai bản lệch nhau nếu sau này một trong hai đổi.
            ActivityCardModel card = null;
            for (int i = 0; i < shared.Activities.Count; i++)
            {
                if (shared.Activities[i].Kind == activity.Value)
                {
                    card = shared.Activities[i];
                }
            }

            task.Open = true;
            task.Title = TurnScreenModel.ActivityName(s, activity.Value);

            // Dọn slot: tiêu đề là TÊN SLOT chứ không phải chữ "Xây dựng" — người chơi bấm vào
            // bãi đá thì họ đang nghĩ về bãi đá đó, không nghĩ về một hoạt động trừu tượng.
            string siteId = session.Editor.SelectedSiteId;
            if (activity.Value == ActivityKind.Build && siteId != null)
            {
                SiteView site = session.Sites.Find(siteId);
                if (site != null)
                {
                    IReadOnlyList<int> builders = session.Turn.Plan.MembersOn(ActivityKind.Build);
                    double needed = site.StrengthRequired;
                    double have = session.Sites.StrengthOf(builders);
                    bool ready = session.Sites.WouldClear(siteId, builders);

                    task.Title = s.Get("site." + siteId + ".name");
                    task.Detail = s.Get("site." + siteId + ".desc");
                    task.Lines.Add(s.Format("site.strength", Formatting.Amount(needed))
                        + " · " + s.Format("site.strength.now", Formatting.Amount(have)));
                    task.Lines.Add(ready ? s.Get("site.ready") : s.Get("site.short"));
                    task.Lines.Add(s.Get("site.allornothing"));
                    task.PartyText = builders.Count == 0
                        ? s.Get("activity.party.none")
                        : s.Format("activity.party.some", builders.Count);
                    task.Message = card != null ? card.Message : null;
                    return task;
                }
            }

            task.Detail = card != null ? card.EffectText : string.Empty;
            task.PartyText = card != null ? card.PartyText : string.Empty;
            task.Message = card != null ? card.Message : null;

            if (card != null)
            {
                task.Lines.Add(card.CostText);
                for (int i = 0; i < card.PreviewLines.Count; i++)
                {
                    task.Lines.Add(card.PreviewLines[i]);
                }

                if (activity.Value == ActivityKind.Explore)
                {
                    task.Migration = BuildMigration(session);
                }

                if (card.UnaffordableReason != null)
                {
                    task.Lines.Add(card.UnaffordableReason);
                }
            }

            return task;
        }

        /// <summary>
        /// Bảng chọn người theo sang vùng mới (design-v7 §F.2).
        ///
        /// Mọi con số đều của Core: sức chứa lấy TurnPlan.MigrationCapacity, số người bỏ lại lấy
        /// MembersLeftBehindIfMigrating. Trước đây chỗ này tự tính "sĩ số trừ số chỗ" vì Core chưa
        /// phơi ra; nay Core sở hữu nên phép tính đó đã bị xoá, không để lại bản sao.
        ///
        /// Vì sao bắt buộc: thắng một trận Khám phá có thể giết ván — người không được chọn mất
        /// hẳn. Core chặn kết thúc lượt cho tới khi người chơi quyết; giao diện phải cho họ chỗ
        /// để quyết, nếu không cử người đi Khám phá là kẹt cứng.
        /// </summary>
        private static MigrationPanelModel BuildMigration(GameSession session)
        {
            TurnPlan plan = session.Turn.Plan;
            StringTable s = session.Strings;

            MigrationPanelModel panel = new MigrationPanelModel();
            panel.Capacity = plan.MigrationCapacity;
            panel.Required = plan.PartySizeOn(ActivityKind.Explore) > 0 && panel.Capacity > 0;
            if (!panel.Required)
            {
                return panel;
            }

            panel.Title = s.Format("migrate.title", panel.Capacity);
            panel.LeftBehindText = s.Format("migrate.leftbehind", plan.MembersLeftBehindIfMigrating);
            panel.ChooseAllText = s.Get("migrate.chooseall");
            panel.ChosenText = s.Format("migrate.chosen", plan.MigrationParty.Count, panel.Capacity);
            panel.CanChooseAll = session.State.Crew <= panel.Capacity;

            IReadOnlyList<int> chosen = plan.MigrationParty;
            for (int i = 0; i < session.State.Members.Count; i++)
            {
                Member member = session.State.Members[i];
                MigrationMemberModel row = new MigrationMemberModel();
                row.Id = member.Id;
                row.Name = s.Format("member.name", member.Id);
                row.Chosen = IndexOf(chosen, member.Id) >= 0;
                panel.Members.Add(row);
            }

            return panel;
        }

        /// <summary>IReadOnlyList không có Contains ở C# 9 / netstandard2.1.</summary>
        private static int IndexOf(IReadOnlyList<int> list, int value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Hoạt động gắn với một vật thể. Dùng chung cho cả lúc vẽ và lúc bấm.</summary>
        public static ActivityKind? ActivityOf(CommunityTargetKind kind)
        {
            switch (kind)
            {
                case CommunityTargetKind.FoodNest: return ActivityKind.Gather;
                case CommunityTargetKind.Club: return ActivityKind.Hunt;
                case CommunityTargetKind.Trail: return ActivityKind.Explore;
                case CommunityTargetKind.Site: return ActivityKind.Build;
                default: return null;
            }
        }
    }
}
