using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Game;
using UnityEngine.UIElements;

namespace BeforeUs.UI
{
    /// <summary>
    /// Phần hiển thị của màn hình cộng đồng (góp ý 6–7 của Khoa).
    ///
    /// Tách khỏi MainScreenController vì hai màn là hai bố cục khác hẳn nhau; gộp vào một lớp
    /// thì cả hai cùng khó đọc. Lớp này KHÔNG giữ trạng thái ván — nó nhận CommunityScreenModel
    /// rồi đổ vào UXML, và gọi ngược ra ngoài bằng hai callback khi người chơi bấm.
    /// </summary>
    public sealed class CommunityScreenView
    {
        /// <summary>Phần thân của màn cộng đồng. Ẩn/hiện cả cụm này.</summary>
        private readonly VisualElement _root;

        /// <summary>
        /// Gốc tài liệu. Lớp phủ (menu trung tâm, bảng kết quả…) nằm ở MỨC GỐC chứ không trong
        /// màn nào, vì hai màn dùng chung chúng — nằm trong một màn thì ẩn màn đó là ẩn luôn
        /// lớp phủ, và người chơi bấm Kết thúc lượt mà không thấy bảng kết quả nào.
        /// </summary>
        private readonly VisualElement _document;
        private readonly Action _changed;      // gọi khi dự định đổi: vẽ lại cả màn
        private readonly Action _endTurn;
        private readonly Action _classic;
        private readonly Action _openNetwork;
        private readonly Action _openCatalog;

        private GameSession _session;

        private readonly VisualElement _header;
        private readonly VisualElement _topHints;
        private readonly VisualElement _climate;
        private readonly Label _climateText;
        private readonly VisualElement _climateHint;

        private readonly Label _energy;
        private readonly VisualElement _energyPips;
        private readonly Label _moraleLabel;
        private readonly VisualElement _moraleFill;
        private readonly Label _moraleHint;
        private readonly Label _screenHint;
        private readonly Button _classicButton;
        private readonly Button _hintsToggle;
        private readonly Button _hintCatalog;

        private readonly Label _memberBarTitle;
        private readonly VisualElement _memberCards;
        private readonly Label _forecastExpected;

        private readonly Label _title;
        private readonly VisualElement _rowBack;
        private readonly VisualElement _rowFront;
        private readonly Button _hubButton;
        private readonly VisualElement _hubHintSlot;
        private readonly VisualElement _hubOverlay;
        private readonly Label _hubTitle;
        private readonly VisualElement _hubSections;
        private readonly Button _hubClose;
        private readonly Button _foodNest;
        private readonly Button _club;
        private readonly Button _trail;
        private readonly Label _sitesTitle;
        private readonly VisualElement _sites;

        private readonly VisualElement _task;
        private readonly Label _taskTitle;
        private readonly Label _taskParty;
        private readonly Label _taskDetail;
        private readonly VisualElement _taskLines;
        private readonly Label _taskMessage;
        private readonly VisualElement _migrate;
        private readonly Label _migrateTitle;
        private readonly Label _migrateLeft;
        private readonly VisualElement _migrateList;
        private readonly Label _migrateChosen;
        private readonly Button _migrateAll;
        private readonly Button _taskClear;
        private readonly Button _taskClose;

        private readonly Label _forecastNeed;
        private readonly Label _forecastHave;
        private readonly Label _forecastStatus;
        private readonly VisualElement _forecast;
        private readonly Button _endTurnButton;

        public CommunityScreenView(
            VisualElement root, Action changed, Action endTurn, Action classic, Action hub, Action openCatalog)
        {
            _document = root;
            _root = root.Q<VisualElement>("community-screen");
            _changed = changed;
            _endTurn = endTurn;
            _classic = classic;
            _openNetwork = hub;
            _openCatalog = openCatalog;

            _header = _root.Q<VisualElement>("c-header");
            _topHints = _root.Q<VisualElement>("c-top-hints");
            _climate = _root.Q<VisualElement>("c-climate");
            _climateText = _root.Q<Label>("c-climate-text");
            _climateHint = _root.Q<VisualElement>("c-climate-hint");

            _energy = _root.Q<Label>("c-energy");
            _energyPips = _root.Q<VisualElement>("c-energy-pips");
            _moraleLabel = _root.Q<Label>("c-morale-label");
            _moraleFill = _root.Q<VisualElement>("c-morale-fill");
            _moraleHint = _root.Q<Label>("c-morale-hint");
            _screenHint = _root.Q<Label>("c-screen-hint");

            _classicButton = _root.Q<Button>("c-classic");
            _classicButton.clicked += () => _classic();

            // §13 đòi gợi ý "tắt được" và "xem lại được". Hai nút này có ở màn A1 nhưng rơi mất
            // khi chuyển sang màn cộng đồng, nên tiêu chí A2 gãy trên chính màn mặc định.
            _hintsToggle = _root.Q<Button>("c-hints-toggle");
            _hintsToggle.clicked += () =>
            {
                _session.Hints.Enabled = !_session.Hints.Enabled;
                _changed();
            };

            _hintCatalog = _root.Q<Button>("c-hint-catalog");
            _hintCatalog.clicked += () => _openCatalog();

            _memberBarTitle = _root.Q<Label>("c-member-bar-title");
            _memberCards = _root.Q<VisualElement>("c-member-cards");
            _forecastExpected = _root.Q<Label>("c-forecast-expected");

            _title = _root.Q<Label>("c-title");
            _rowBack = _root.Q<VisualElement>("c-row-back");
            _rowFront = _root.Q<VisualElement>("c-row-front");

            _hubButton = _root.Q<Button>("c-hub");
            _hubButton.clicked += OpenHub;
            _hubHintSlot = _root.Q<VisualElement>("c-hub-hint");

            _hubOverlay = _document.Q<VisualElement>("hub-overlay");
            _hubTitle = _document.Q<Label>("hub-title");
            _hubSections = _document.Q<VisualElement>("hub-sections");
            _hubClose = _document.Q<Button>("hub-close");
            _hubClose.clicked += () => Show(_hubOverlay, false);

            _foodNest = _root.Q<Button>("c-food-nest");
            _foodNest.clicked += () => OpenTarget(CommunityTargetKind.FoodNest);
            _club = _root.Q<Button>("c-club");
            _club.clicked += () => OpenTarget(CommunityTargetKind.Club);
            _trail = _root.Q<Button>("c-trail");
            _trail.clicked += () => OpenTarget(CommunityTargetKind.Trail);

            _sitesTitle = _root.Q<Label>("c-sites-title");
            _sites = _root.Q<VisualElement>("c-sites");

            _task = _root.Q<VisualElement>("c-task");
            _taskTitle = _root.Q<Label>("c-task-title");
            _taskParty = _root.Q<Label>("c-task-party");
            _taskDetail = _root.Q<Label>("c-task-detail");
            _taskLines = _root.Q<VisualElement>("c-task-lines");
            _taskMessage = _root.Q<Label>("c-task-message");

            _migrate = _root.Q<VisualElement>("c-migrate");
            _migrateTitle = _root.Q<Label>("c-migrate-title");
            _migrateLeft = _root.Q<Label>("c-migrate-left");
            _migrateList = _root.Q<VisualElement>("c-migrate-list");
            _migrateChosen = _root.Q<Label>("c-migrate-chosen");
            _migrateAll = _root.Q<Button>("c-migrate-all");
            _migrateAll.clicked += () =>
            {
                _session.Editor.ChooseWholeTribeForMigration(_session.State.Members);
                _changed();
            };

            _taskClear = _root.Q<Button>("c-task-clear");
            _taskClear.clicked += ClearCurrent;
            _taskClose = _root.Q<Button>("c-task-close");
            _taskClose.clicked += CloseTask;

            _forecast = _root.Q<VisualElement>("c-forecast");
            _forecastNeed = _root.Q<Label>("c-forecast-need");
            _forecastHave = _root.Q<Label>("c-forecast-have");
            _forecastStatus = _root.Q<Label>("c-forecast-status");

            _endTurnButton = _root.Q<Button>("c-end-turn");
            _endTurnButton.clicked += () => _endTurn();
        }

        public void SetVisible(bool visible)
        {
            _root.EnableInClassList("hidden", !visible);
            if (!visible)
            {
                Show(_hubOverlay, false);
            }
        }

        private static void Show(VisualElement element, bool visible)
        {
            element.EnableInClassList("hidden", !visible);
        }

        // --- menu trung tâm (góp ý 7) --------------------------------------------------------

        private void OpenHub()
        {
            HubMenuModel model = HubMenuModel.Build(_session);
            _hubTitle.text = model.Title;
            _hubClose.text = model.CloseText;

            _hubSections.Clear();
            for (int i = 0; i < model.Sections.Count; i++)
            {
                _hubSections.Add(BuildHubSection(model.Sections[i]));
            }

            Show(_hubOverlay, true);
        }

        private VisualElement BuildHubSection(HubSectionModel section)
        {
            VisualElement block = new VisualElement();
            block.AddToClassList("hub-section");
            block.Add(Text(section.Title, "hub-section-title"));

            if (section.Items.Count == 0)
            {
                block.Add(Text(section.EmptyText, "hub-section-empty"));
                return block;
            }

            for (int i = 0; i < section.Items.Count; i++)
            {
                block.Add(BuildHubItem(section.Items[i]));
            }

            return block;
        }

        private VisualElement BuildHubItem(HubItemModel item)
        {
            Button button = new Button();
            button.AddToClassList("hub-item");
            button.EnableInClassList("hub-item-ready", item.Highlight);
            button.EnableInClassList("hub-item-disabled", !item.Enabled);

            VisualElement art = PlaceholderArt.Thumbnail(item.ArtKey, "hub-item-art");
            if (art != null)
            {
                button.Add(art);
            }

            VisualElement text = new VisualElement();
            text.AddToClassList("hub-item-text");
            text.Add(Text(item.Name, "hub-item-name"));
            text.Add(Text(item.StatusText, "hub-item-status"));
            if (item.DisabledReason != null)
            {
                text.Add(Text(item.DisabledReason, "hub-item-reason"));
            }

            button.Add(text);

            if (item.Highlight)
            {
                button.Add(Text(_session.Strings.Get("hub.ready"), "hub-item-badge"));
            }

            if (item.Enabled)
            {
                HubAction action = item.Action;
                string siteId = item.SiteId;
                button.clicked += () => RunHubAction(action, siteId);
            }

            return button;
        }

        /// <summary>
        /// Menu trung tâm chỉ là ĐƯỜNG ĐI: mỗi mục dẫn về đúng thao tác đã có ở chỗ khác,
        /// không có cơ chế riêng. Nhờ vậy bấm bãi đá quanh hang hay bấm nó trong menu đều
        /// mở cùng một bảng, và không có hai đường code phải giữ cho khớp nhau.
        /// </summary>
        private void RunHubAction(HubAction action, string siteId)
        {
            Show(_hubOverlay, false);

            switch (action)
            {
                case HubAction.OpenSite:
                    _session.Editor.SelectSite(siteId);
                    break;

                case HubAction.OpenMorale:
                    _session.Editor.SelectActivity(ActivityKind.Morale);
                    break;

                default:
                    _openNetwork();
                    return;
            }

            _changed();
        }

        public void Refresh(GameSession session)
        {
            _session = session;
            CommunityScreenModel model = CommunityScreenModel.Build(session);
            TurnScreenModel shared = model.Shared;

            _header.Clear();
            for (int i = 0; i < model.HeaderChips.Count; i++)
            {
                Label chip = new Label(model.HeaderChips[i].Text);
                chip.AddToClassList("chip");
                _header.Add(chip);
            }

            _energy.text = shared.EnergyText;
            _energyPips.Clear();
            for (int i = 0; i < shared.EnergyMax; i++)
            {
                VisualElement pip = new VisualElement();
                pip.AddToClassList("pip");
                if (i < shared.EnergySpent)
                {
                    pip.AddToClassList("pip-spent");
                }

                _energyPips.Add(pip);
            }

            _moraleLabel.text = shared.MoraleText;
            _moraleHint.text = shared.MoraleHint;
            _moraleFill.style.width = Length.Percent(shared.MoraleFill * 100f);
            _screenHint.text = session.Strings.Get("community.screen.hint");
            _classicButton.text = session.Strings.Get("community.classic");
            _hintsToggle.text = model.HintsToggleText;
            _hintCatalog.text = model.HintCatalogText;
            _memberBarTitle.text = model.MemberBarTitle + " — " + model.MemberBarHint;
            _forecastExpected.text = model.ForecastExpectedText;
            BuildMemberCards(model);

            _climate.EnableInClassList("hidden", shared.ClimateBannerText == null);
            _climateText.text = shared.ClimateBannerText ?? string.Empty;
            FillHints(_climateHint, shared.ClimateHint);
            FillHintList(_topHints, shared.TopHints);

            _title.text = model.CommunityTitle;
            BuildSlots(model);

            BuildObject(_foodNest, model.FoodNest);
            BuildObject(_club, model.Club);
            BuildObject(_trail, model.Trail);
            BuildSiteList(model);
            _hubButton.text = model.Hub.Name + "\n" + model.Hub.StatusText;

            BuildTask(model.Task);

            _forecastNeed.text = shared.ForecastNeedText;
            _forecastHave.text = shared.ForecastHaveText;
            _forecastStatus.text = shared.ForecastStatusText + "   " + shared.GatherFloorText;
            _forecast.EnableInClassList("forecast-short", shared.ForecastShort);

            _endTurnButton.SetEnabled(shared.CanEndTurn);
            _endTurnButton.text = shared.CanEndTurn
                ? model.EndTurnText
                : model.EndTurnText + " — " + shared.EndTurnBlockedReason;
        }

        // --- cụm cộng đồng ----------------------------------------------------------------

        private void BuildSlots(CommunityScreenModel model)
        {
            _rowFront.Clear();
            _rowBack.Clear();

            for (int i = 0; i < model.Slots.Count; i++)
            {
                MemberSlotModel slot = model.Slots[i];
                VisualElement row = slot.Pose == SlotPose.Sit ? _rowFront : _rowBack;
                row.Add(BuildSlot(slot));
            }
        }

        private VisualElement BuildSlot(MemberSlotModel slot)
        {
            Button element = new Button();
            element.AddToClassList("slot");
            element.tooltip = slot.Tooltip;

            if (!slot.Occupied)
            {
                element.AddToClassList("slot-empty");
                element.SetEnabled(false);
                return element;
            }

            MemberRowModel member = slot.Member;
            element.EnableInClassList("slot-assigned", member.Assigned && !member.InSelectedActivity);
            element.EnableInClassList("slot-selected", member.InSelectedActivity);
            element.EnableInClassList("slot-focused", slot.Focused);

            // Tên NẰM TRÊN ĐẦU, đúng yêu cầu của Khoa — thêm trước ảnh nên nó ở trên.
            element.Add(Text(slot.NameText, "slot-name"));

            VisualElement art = PlaceholderArt.Thumbnail(slot.ArtKey, "slot-art");
            if (art != null)
            {
                element.Add(art);
            }

            element.Add(Text(slot.StatusText, "slot-status"));

            int memberId = member.Id;
            element.clicked += () => ClickMember(memberId);

            return element;
        }

        /// <summary>Slot công trình quanh hang. Bấm vào một slot là mở bảng dọn slot đó.</summary>
        private void BuildSiteList(CommunityScreenModel model)
        {
            _sitesTitle.text = model.SitesTitle + " — " + model.ClearedCountText;
            _sites.Clear();

            for (int i = 0; i < model.Sites.Count; i++)
            {
                CommunityObjectModel site = model.Sites[i];
                string siteId = site.SiteId;

                Button button = new Button();
                button.AddToClassList("community-object");
                button.AddToClassList("community-site");
                BuildObject(button, site);

                if (site.Enabled)
                {
                    button.clicked += () => OpenSite(siteId);
                }

                _sites.Add(button);
            }
        }

        private void OpenSite(string siteId)
        {
            if (_session.Editor.SelectedSiteId == siteId)
            {
                _session.Editor.Deselect();
            }
            else
            {
                _session.Editor.SelectSite(siteId);
            }

            _changed();
        }

        /// <summary>
        /// Bấm một người: vừa cử/rút khỏi việc đang mở, vừa soi họ. Lưới slot và thanh thẻ gọi
        /// CÙNG hàm này, nên phát sáng ở hai nơi luôn khớp mà không cần đồng bộ gì.
        /// </summary>
        private void ClickMember(int memberId)
        {
            _session.Editor.FocusMember(memberId);
            if (_session.Editor.SelectedActivity != null)
            {
                _session.Editor.ToggleMember(memberId);
            }

            _changed();
        }

        private void BuildMemberCards(CommunityScreenModel model)
        {
            _memberCards.Clear();

            for (int i = 0; i < model.MemberCards.Count; i++)
            {
                MemberRowModel row = model.MemberCards[i];
                int id = row.Id;

                Button card = new Button();
                card.AddToClassList("member-card");
                card.EnableInClassList("member-card-assigned", row.Assigned);
                card.EnableInClassList("member-card-focused", row.Focused);

                card.Add(Text(row.Name, "member-card-name"));
                card.Add(Text(row.SpeedText, "member-card-stat"));
                card.Add(Text(row.StrengthText, "member-card-stat"));
                card.Add(Text(row.HpText, "member-card-stat"));
                card.Add(Text(row.StatusText, "member-card-status"));

                // Chỉ giải thích chỉ số khi đang soi: luôn hiện thì thẻ nào cũng dày đặc chữ.
                if (row.Focused)
                {
                    card.Add(Text(row.SpeedHelp, "member-card-help"));
                    card.Add(Text(row.StrengthHelp, "member-card-help"));
                    card.Add(Text(row.HpHelp, "member-card-help"));
                }

                card.clicked += () => ClickMember(id);
                _memberCards.Add(card);
            }
        }

        private void BuildObject(Button button, CommunityObjectModel model)
        {
            button.Clear();
            button.EnableInClassList("community-object-selected", model.Selected);
            button.EnableInClassList("community-object-disabled", !model.Enabled);

            VisualElement art = PlaceholderArt.Thumbnail(model.ArtKey, "community-object-art");
            if (art != null)
            {
                button.Add(art);
            }

            button.Add(Text(model.Name, "community-object-name"));
            button.Add(Text(model.StatusText, "community-object-status"));

            if (!string.IsNullOrEmpty(model.SubText))
            {
                button.Add(Text(model.SubText, "community-object-sub"));
            }

            if (model.DisabledReason != null)
            {
                button.Add(Text(model.DisabledReason, "community-object-reason"));
            }

            if (model.Hint != null)
            {
                button.Add(BuildHintBox(model.Hint));
            }
        }

        // --- bảng giao việc ---------------------------------------------------------------

        private void BuildTask(TaskPanelModel task)
        {
            _task.EnableInClassList("hidden", !task.Open);
            if (!task.Open)
            {
                _screenHint.text = task.Title;
                return;
            }

            _taskTitle.text = task.Title;
            _taskParty.text = task.PartyText;
            _taskDetail.text = task.Detail;
            _taskClear.text = task.ClearText;
            _taskClose.text = task.CloseText;

            _taskLines.Clear();
            for (int i = 0; i < task.Lines.Count; i++)
            {
                _taskLines.Add(Text(task.Lines[i], "task-line"));
            }

            BuildMigration(task.Migration);

            _taskMessage.text = task.Message ?? string.Empty;
            _taskMessage.EnableInClassList("hidden", task.Message == null);
        }

        /// <summary>
        /// Bảng chọn người di cư. Chỉ hiện khi đang cử người đi Khám phá — Core chặn kết thúc
        /// lượt cho tới khi chọn xong, nên thiếu bảng này là người chơi kẹt cứng không hiểu vì sao.
        /// </summary>
        private void BuildMigration(MigrationPanelModel migration)
        {
            bool show = migration != null && migration.Required;
            _migrate.EnableInClassList("hidden", !show);
            if (!show)
            {
                return;
            }

            _migrateTitle.text = migration.Title;
            _migrateLeft.text = migration.LeftBehindText;
            _migrateChosen.text = migration.ChosenText;
            _migrateAll.text = migration.ChooseAllText;
            _migrateAll.SetEnabled(migration.CanChooseAll);

            _migrateList.Clear();
            for (int i = 0; i < migration.Members.Count; i++)
            {
                MigrationMemberModel row = migration.Members[i];
                int id = row.Id;

                Button chip = new Button();
                chip.text = row.Name;
                chip.AddToClassList("migrate-chip");
                chip.EnableInClassList("migrate-chip-chosen", row.Chosen);
                chip.clicked += () =>
                {
                    _session.Editor.ToggleMigrationMember(id);
                    _changed();
                };

                _migrateList.Add(chip);
            }
        }

        private void OpenTarget(CommunityTargetKind kind)
        {
            ActivityKind? activity = CommunityScreenModel.ActivityOf(kind);
            if (activity == null)
            {
                return;
            }

            // Bấm lại đúng thứ đang mở = đóng lại. Cùng cử chỉ, hai chiều.
            if (_session.Editor.SelectedActivity == activity.Value)
            {
                _session.Editor.Deselect();
            }
            else
            {
                _session.Editor.SelectActivity(activity.Value);
            }

            _changed();
        }

        private void ClearCurrent()
        {
            ActivityKind? activity = _session.Editor.SelectedActivity;
            if (activity != null)
            {
                _session.Editor.ClearActivity(activity.Value);
                _changed();
            }
        }

        private void CloseTask()
        {
            _session.Editor.Deselect();
            _changed();
        }

        // --- tiện ích ----------------------------------------------------------------------

        private void FillHints(VisualElement slot, HintModel hint)
        {
            slot.Clear();
            if (hint != null)
            {
                slot.Add(BuildHintBox(hint));
            }
        }

        private void FillHintList(VisualElement slot, List<HintModel> hints)
        {
            slot.Clear();
            for (int i = 0; i < hints.Count; i++)
            {
                slot.Add(BuildHintBox(hints[i]));
            }
        }

        private VisualElement BuildHintBox(HintModel hint)
        {
            string id = hint.Id;

            VisualElement box = new VisualElement();
            box.AddToClassList("hint-box");
            box.Add(Text(hint.Text, "hint-text"));

            Button dismiss = new Button();
            dismiss.text = hint.DismissText;
            dismiss.AddToClassList("hint-dismiss-button");
            dismiss.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                _session.Hints.Dismiss(id);
                _changed();
            });
            box.Add(dismiss);

            return box;
        }

        private static Label Text(string value, string className)
        {
            Label label = new Label(value);
            label.AddToClassList(className);
            return label;
        }
    }
}
