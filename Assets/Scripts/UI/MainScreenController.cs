using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Neuron;
using BeforeUs.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace BeforeUs.UI
{
    /// <summary>
    /// Màn chính vòng lượt (A1). Chơi trọn một lượt chỉ bằng chuột: nhấp một thẻ hoạt động để chọn,
    /// nhấp từng thành viên để cử đi hoặc rút về, rồi bấm Kết thúc lượt.
    ///
    /// Lớp này KHÔNG tính luật và không đọc GameState. Nó dựng TurnScreenModel rồi đổ vào UXML;
    /// mọi con số và mọi câu chữ đều từ đó mà ra (AGENTS.md: "UI chỉ đọc trạng thái và gọi Core").
    ///
    /// SCENE: Assets/Scenes/Game.unity đã dựng sẵn và nối đủ — GameObject "UI" có sẵn ba thành
    /// phần này, UIDocument đã trỏ MainPanelSettings + MainScreen.uxml, GameBootstrap đã trỏ ba
    /// file JSON. Khoa chỉ cần mở scene đó rồi bấm Play.
    /// Hướng dẫn nối tay 6 bước cũ đã bỏ: nó mô tả việc nay đã làm xong trong scene.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(GameBootstrap))]
    public sealed class MainScreenController : MonoBehaviour
    {
        private GameBootstrap _bootstrap;
        private GameSession _session;
        private VisualElement _root;

        private VisualElement _classicScreen;
        private CommunityScreenView _community;

        private VisualElement _header;
        private Label _energyLabel;
        private VisualElement _energyPips;
        private Label _planRuleLabel;
        private Label _moraleLabel;
        private VisualElement _moraleFill;
        private Label _moraleHint;

        private VisualElement _forecast;
        private Label _forecastTitle;
        private Label _forecastNeed;
        private Label _forecastHave;
        private Label _forecastStatus;
        private Label _forecastNote;

        private VisualElement _climateBanner;
        private Label _climateBannerText;
        private VisualElement _climateHintSlot;
        private VisualElement _topHintSlot;
        private Button _hintsToggle;
        private Button _hintCatalogButton;

        private VisualElement _hintCatalogOverlay;
        private Label _hintCatalogTitle;
        private VisualElement _hintCatalogLines;
        private Button _hintCatalogReset;
        private Button _hintCatalogClose;

        private Label _hintLabel;
        private VisualElement _activityList;
        private Label _membersTitle;
        private VisualElement _memberList;
        private Button _endTurnButton;

        private VisualElement _resultOverlay;
        private Label _resultTitle;
        private VisualElement _resultLines;
        private Button _resultContinue;

        private Button _screenToggle;
        private Button _networkButton;
        private VisualElement _neuronOverlay;
        private Label _neuronTitle;
        private Label _neuronStage;
        private Label _neuronHave;
        private Label _neuronFeats;
        private Label _neuronMessage;
        private Label _neuronNoNodes;
        private VisualElement _neuronBranches;
        private VisualElement _neuronEvolve;
        private Button _neuronClose;

        /// <summary>Kết quả lần bấm mua gần nhất, để hiện lỗi. Xóa khi đóng màn.</summary>
        private PurchaseResult _lastPurchase;

        private VisualElement _menuOverlay;
        private Label _menuTitle;
        private Label _menuBody;
        private Button _menuNewGame;

        private VisualElement _evolveOverlay;
        private Label _evolveTitle;
        private Label _evolveBody;
        private VisualElement _evolveLines;
        private Button _evolveContinue;

        private VisualElement _endOverlay;
        private Label _endTitle;
        private Label _endBody;
        private VisualElement _endLines;
        private Button _endButton;

        private void OnEnable()
        {
            _bootstrap = GetComponent<GameBootstrap>();
            _root = GetComponent<UIDocument>().rootVisualElement;

            if (_root == null)
            {
                Debug.LogError("MainScreenController: UIDocument chưa có Source Asset. Trỏ nó vào Assets/UI/MainScreen.uxml.");
                return;
            }

            if (_root.styleSheets.count == 0)
            {
                Debug.LogWarning("MainScreenController: MainScreen.uxml không nạp được MainScreen.uss. Màn hình sẽ chạy nhưng không có định dạng.");
            }

            CacheElements();

            _classicScreen = _root.Q<VisualElement>("screen");
            _community = new CommunityScreenView(
                _root,
                Refresh,
                OnEndTurn,
                ToggleScreen,
                OnOpenNetwork,
                OnOpenHintCatalog);

            _session = _bootstrap.Session;
            Refresh();

            if (!_bootstrap.SkipStartMenu)
            {
                ShowMenu();
            }
        }

        private void CacheElements()
        {
            _header = _root.Q<VisualElement>("header");
            _energyLabel = _root.Q<Label>("energy-label");
            _energyPips = _root.Q<VisualElement>("energy-pips");
            _planRuleLabel = _root.Q<Label>("plan-rule-label");
            _moraleLabel = _root.Q<Label>("morale-label");
            _moraleFill = _root.Q<VisualElement>("morale-fill");
            _moraleHint = _root.Q<Label>("morale-hint");

            _forecast = _root.Q<VisualElement>("forecast");
            _forecastTitle = _root.Q<Label>("forecast-title");
            _forecastNeed = _root.Q<Label>("forecast-need");
            _forecastHave = _root.Q<Label>("forecast-have");
            _forecastStatus = _root.Q<Label>("forecast-status");
            _forecastNote = _root.Q<Label>("forecast-note");

            _climateBanner = _root.Q<VisualElement>("climate-banner");
            _climateBannerText = _root.Q<Label>("climate-banner-text");
            _climateHintSlot = _root.Q<VisualElement>("climate-hint");
            _topHintSlot = _root.Q<VisualElement>("top-hints");

            _screenToggle = _root.Q<Button>("screen-toggle");
            _screenToggle.clicked += ToggleScreen;

            _networkButton = _root.Q<Button>("network-button");
            _networkButton.clicked += OnOpenNetwork;

            _neuronOverlay = _root.Q<VisualElement>("neuron-overlay");
            _neuronTitle = _root.Q<Label>("neuron-title");
            _neuronStage = _root.Q<Label>("neuron-stage");
            _neuronHave = _root.Q<Label>("neuron-have");
            _neuronFeats = _root.Q<Label>("neuron-feats");
            _neuronMessage = _root.Q<Label>("neuron-message");
            _neuronNoNodes = _root.Q<Label>("neuron-nonodes");
            _neuronBranches = _root.Q<VisualElement>("neuron-branches");
            _neuronEvolve = _root.Q<VisualElement>("neuron-evolve");
            _neuronClose = _root.Q<Button>("neuron-close");
            _neuronClose.clicked += OnCloseNetwork;

            _hintsToggle = _root.Q<Button>("hints-toggle");
            _hintsToggle.clicked += OnToggleHints;
            _hintCatalogButton = _root.Q<Button>("hint-catalog-button");
            _hintCatalogButton.clicked += OnOpenHintCatalog;

            _hintCatalogOverlay = _root.Q<VisualElement>("hint-catalog-overlay");
            _hintCatalogTitle = _root.Q<Label>("hint-catalog-title");
            _hintCatalogLines = _root.Q<VisualElement>("hint-catalog-lines");
            _hintCatalogReset = _root.Q<Button>("hint-catalog-reset");
            _hintCatalogReset.clicked += OnResetHints;
            _hintCatalogClose = _root.Q<Button>("hint-catalog-close");
            _hintCatalogClose.clicked += () => Show(_hintCatalogOverlay, false);

            _hintLabel = _root.Q<Label>("hint-label");
            _activityList = _root.Q<VisualElement>("activity-list");
            _membersTitle = _root.Q<Label>("members-title");
            _memberList = _root.Q<VisualElement>("member-list");

            _endTurnButton = _root.Q<Button>("end-turn-button");
            _endTurnButton.clicked += OnEndTurn;

            _resultOverlay = _root.Q<VisualElement>("result-overlay");
            _resultTitle = _root.Q<Label>("result-title");
            _resultLines = _root.Q<VisualElement>("result-lines");
            _resultContinue = _root.Q<Button>("result-continue");
            _resultContinue.clicked += OnResultContinue;

            _menuOverlay = _root.Q<VisualElement>("menu-overlay");
            _menuTitle = _root.Q<Label>("menu-title");
            _menuBody = _root.Q<Label>("menu-body");
            _menuNewGame = _root.Q<Button>("menu-newgame");
            _menuNewGame.clicked += OnNewGameFromMenu;

            _evolveOverlay = _root.Q<VisualElement>("evolve-overlay");
            _evolveTitle = _root.Q<Label>("evolve-title");
            _evolveBody = _root.Q<Label>("evolve-body");
            _evolveLines = _root.Q<VisualElement>("evolve-lines");
            _evolveContinue = _root.Q<Button>("evolve-continue");
            _evolveContinue.clicked += () => Show(_evolveOverlay, false);

            _endOverlay = _root.Q<VisualElement>("end-overlay");
            _endTitle = _root.Q<Label>("end-title");
            _endBody = _root.Q<Label>("end-body");
            _endLines = _root.Q<VisualElement>("end-lines");
            _endButton = _root.Q<Button>("end-button");
            _endButton.clicked += OnRestart;
        }

        // --- vẽ lại toàn bộ màn hình ---------------------------------------------------------

        /// <summary>
        /// Vẽ lại màn đang bật. Hai màn dùng chung một GameSession và chung TurnPlanEditor,
        /// nên chuyển qua lại giữa chúng không làm mất dự định đang xếp.
        /// </summary>
        private void Refresh()
        {
            bool community = _bootstrap.Screen == ScreenMode.Community;
            Show(_classicScreen, !community);
            _community.SetVisible(community);

            if (community)
            {
                _community.Refresh(_session);
                return;
            }

            RefreshClassic();
        }

        private void ToggleScreen()
        {
            _bootstrap.Screen = _bootstrap.Screen == ScreenMode.Community
                ? ScreenMode.Classic
                : ScreenMode.Community;
            Refresh();
        }

        private void RefreshClassic()
        {
            TurnScreenModel model = TurnScreenModel.Build(_session);

            _header.Clear();
            for (int i = 0; i < model.HeaderChips.Count; i++)
            {
                Label chip = new Label(model.HeaderChips[i].Text);
                chip.AddToClassList("chip");
                _header.Add(chip);
            }

            _energyLabel.text = model.EnergyText;
            _energyPips.Clear();
            for (int i = 0; i < model.EnergyMax; i++)
            {
                VisualElement pip = new VisualElement();
                pip.AddToClassList("pip");
                if (i < model.EnergySpent)
                {
                    pip.AddToClassList("pip-spent");
                }

                _energyPips.Add(pip);
            }

            _planRuleLabel.text = model.PlanRuleText;

            _moraleLabel.text = model.MoraleText;
            _moraleHint.text = model.MoraleHint;
            _moraleFill.style.width = Length.Percent(model.MoraleFill * 100f);
            _hintsToggle.text = model.HintsToggleText;
            _networkButton.text = model.NetworkButtonText;
            _screenToggle.text = _session.Strings.Get("community.back");
            _hintCatalogButton.text = model.HintCatalogButtonText;

            Show(_climateBanner, model.ClimateBannerText != null);
            _climateBannerText.text = model.ClimateBannerText ?? string.Empty;
            FillHintSlot(_climateHintSlot, model.ClimateHint);
            FillHintSlots(_topHintSlot, model.TopHints);

            _forecastTitle.text = model.ForecastTitle;
            _forecastNeed.text = model.ForecastNeedText;
            _forecastHave.text = model.ForecastHaveText;
            _forecastStatus.text = model.ForecastStatusText;
            _forecastNote.text = model.ForecastNote;
            _forecast.EnableInClassList("forecast-short", model.ForecastShort);

            _hintLabel.text = model.HintText;

            // Core NÉM nếu EndTurn gọi khi dự định chưa hợp lệ (ví dụ chưa đủ sàn Kiếm ăn).
            // Nút phải khoá, không chỉ báo — bấm được mà sập là lỗi tệ nhất trong buổi chơi thử.
            _endTurnButton.SetEnabled(model.CanEndTurn);
            _endTurnButton.text = model.CanEndTurn
                ? model.EndTurnText
                : model.EndTurnText + " — " + model.EndTurnBlockedReason;

            BuildActivityCards(model.Activities);

            _membersTitle.text = model.MembersTitle;
            BuildMemberRows(model.Members);

        }

        private void BuildActivityCards(List<ActivityCardModel> cards)
        {
            _activityList.Clear();

            for (int i = 0; i < cards.Count; i++)
            {
                ActivityCardModel card = cards[i];
                ActivityKind kind = card.Kind;

                Button element = new Button();
                element.AddToClassList("activity-card");
                element.EnableInClassList("activity-card-selected", card.Selected);
                element.EnableInClassList("activity-card-locked", !card.Unlocked);
                element.EnableInClassList("activity-card-unaffordable", card.Unlocked && !card.Affordable);

                VisualElement head = new VisualElement();
                head.AddToClassList("activity-card-head");
                VisualElement icon = PlaceholderArt.Thumbnail(card.ArtKey, "art-thumb-activity");
                if (icon != null)
                {
                    head.Add(icon);
                }

                head.Add(Text(card.Name, "activity-name"));
                element.Add(head);
                element.Add(Text(card.CostText, "activity-cost"));
                element.Add(Text(card.EffectText, "activity-effect"));

                Label party = Text(card.PartyText, "activity-party");
                party.EnableInClassList("activity-party-empty", card.PartySize == 0);
                element.Add(party);

                if (card.LockReason != null)
                {
                    element.Add(Text(card.LockReason, "activity-lock"));
                }

                for (int j = 0; j < card.PreviewLines.Count; j++)
                {
                    element.Add(Text(card.PreviewLines[j], "activity-preview"));
                }

                // "Chưa mở khóa" và "hết năng lượng" là hai lý do khác nhau, nói riêng từng cái.
                if (card.Unlocked && card.UnaffordableReason != null)
                {
                    element.Add(Text(card.UnaffordableReason, "activity-unaffordable-note"));
                }

                if (card.Message != null)
                {
                    element.Add(Text(card.Message, "activity-message"));
                }

                if (card.Hint != null)
                {
                    element.Add(BuildHintBox(card.Hint));
                }

                if (card.PartySize > 0)
                {
                    Button clear = new Button();
                    clear.text = _session.Strings.Get("activity.clear");
                    clear.AddToClassList("activity-clear-button");
                    clear.RegisterCallback<ClickEvent>(evt =>
                    {
                        evt.StopPropagation();
                        _session.Editor.ClearActivity(kind);
                        Refresh();
                    });
                    element.Add(clear);
                }

                if (card.Unlocked)
                {
                    element.clicked += () =>
                    {
                        _session.Editor.SelectActivity(kind);
                        Refresh();
                    };
                }

                _activityList.Add(element);
            }
        }

        private void BuildMemberRows(List<MemberRowModel> rows)
        {
            _memberList.Clear();

            for (int i = 0; i < rows.Count; i++)
            {
                MemberRowModel row = rows[i];
                int memberId = row.Id;

                Button element = new Button();
                element.AddToClassList("member-row");
                element.EnableInClassList("member-row-selected", row.InSelectedActivity);
                element.EnableInClassList("member-row-assigned", row.Assigned && !row.InSelectedActivity);

                VisualElement portrait = PlaceholderArt.Thumbnail(row.ArtKey, "art-thumb-member");
                if (portrait != null)
                {
                    element.Add(portrait);
                }

                VisualElement text = new VisualElement();
                text.AddToClassList("member-text");
                text.Add(Text(row.Name, "member-name"));
                text.Add(Text(row.StatsText, "member-stats"));
                text.Add(Text(row.StatusText, "member-status"));
                element.Add(text);

                element.clicked += () =>
                {
                    _session.Editor.ToggleMember(memberId);
                    Refresh();
                };

                _memberList.Add(element);
            }
        }

        // --- gợi ý hướng dẫn (design.md §13) --------------------------------------------------

        private void FillHintSlot(VisualElement slot, HintModel hint)
        {
            slot.Clear();
            if (hint != null)
            {
                slot.Add(BuildHintBox(hint));
            }
        }

        private void FillHintSlots(VisualElement slot, List<HintModel> hints)
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
                // Thẻ hoạt động cũng bắt nhấp; đừng để tắt gợi ý thành chọn luôn hoạt động đó.
                evt.StopPropagation();
                _session.Hints.Dismiss(id);
                Refresh();
            });
            box.Add(dismiss);

            return box;
        }

        private void OnToggleHints()
        {
            _session.Hints.Enabled = !_session.Hints.Enabled;
            Refresh();
        }

        private void OnOpenHintCatalog()
        {
            List<HintCatalogEntry> entries = HintRules.Catalog(_session);

            _hintCatalogTitle.text = _session.Strings.Get("hint.catalog.title");
            _hintCatalogReset.text = _session.Strings.Get("hint.catalog.reset");
            _hintCatalogClose.text = _session.Strings.Get("hint.catalog.close");

            _hintCatalogLines.Clear();
            for (int i = 0; i < entries.Count; i++)
            {
                HintCatalogEntry entry = entries[i];

                VisualElement block = new VisualElement();
                block.AddToClassList("catalog-entry");

                string when = entry.Dismissed
                    ? entry.When + " " + _session.Strings.Get("hint.catalog.dismissed")
                    : entry.When;
                block.Add(Text(when, "catalog-when"));
                block.Add(Text(entry.Text, "catalog-text"));

                _hintCatalogLines.Add(block);
            }

            Show(_hintCatalogOverlay, true);
        }

        private void OnResetHints()
        {
            _session.Hints.ShowAllAgain();
            Refresh();
            OnOpenHintCatalog();
        }

        // --- màn mạng lưới neuron (A3, design.md §7) ------------------------------------------

        private void OnOpenNetwork()
        {
            Show(_neuronOverlay, true);
            RefreshNetwork();
        }

        private void OnCloseNetwork()
        {
            _lastPurchase = null;
            Show(_neuronOverlay, false);

            // Mua nút có thể mở khóa hoạt động hoặc đổi giai đoạn, nên màn chính phải vẽ lại.
            Refresh();
        }

        private void RefreshNetwork()
        {
            NeuronScreenModel model = NeuronScreenModel.Build(_session, _lastPurchase);

            _neuronTitle.text = model.Title;
            _neuronStage.text = model.StageText;
            _neuronHave.text = model.NeuronText;
            _neuronFeats.text = model.FeatsSummary;
            _neuronClose.text = model.CloseText;

            _neuronMessage.text = model.Message ?? string.Empty;
            Show(_neuronMessage, model.Message != null);

            _neuronNoNodes.text = model.StageHasNoNodesText;
            Show(_neuronNoNodes, model.StageHasNoNodes);

            BuildBranches(model);
            BuildEvolveBox(model.Evolve);
        }

        private void BuildBranches(NeuronScreenModel model)
        {
            _neuronBranches.Clear();

            for (int i = 0; i < model.Branches.Count; i++)
            {
                NeuronBranchModel branch = model.Branches[i];

                VisualElement column = new VisualElement();
                column.AddToClassList("neuron-branch");
                column.Add(Text(branch.Name, "neuron-branch-title"));

                for (int j = 0; j < branch.Nodes.Count; j++)
                {
                    column.Add(BuildNodeCard(branch.Nodes[j]));
                }

                _neuronBranches.Add(column);
            }
        }

        private VisualElement BuildNodeCard(NeuronNodeModel node)
        {
            string id = node.Id;

            Button card = new Button();
            card.AddToClassList("neuron-node");
            card.EnableInClassList("neuron-node-purchased", node.Purchased);
            card.EnableInClassList("neuron-node-buyable", node.Buyable);
            card.EnableInClassList("neuron-node-blocked", !node.Purchased && !node.Buyable);

            card.Add(Text(node.Name, "neuron-node-name"));
            card.Add(Text(node.TypeText + " · " + node.CostText, "neuron-node-meta"));
            card.Add(Text(node.EffectText, "neuron-node-effect"));

            if (node.Purchased)
            {
                card.Add(Text(_session.Strings.Get("neuron.purchased"), "neuron-node-state"));
            }
            else if (node.BlockReason != null)
            {
                card.Add(Text(node.BlockReason, "neuron-node-block"));
            }

            if (node.Buyable)
            {
                card.Add(Text(_session.Strings.Get("neuron.buy"), "neuron-node-state"));
                card.clicked += () => Buy(id);
            }

            return card;
        }

        private void BuildEvolveBox(EvolveModel evolve)
        {
            _neuronEvolve.Clear();
            if (!evolve.Exists)
            {
                return;
            }

            _neuronEvolve.Add(Text(evolve.Name + " · " + evolve.CostText, "neuron-evolve-title"));

            // design.md §7: ba điều kiện độc lập, mỗi cái một dòng. Người chơi thiếu cổng thì đi
            // mua nút, thiếu chiến tích thì đi đánh trận — hai đường khác hẳn nhau.
            AddCondition(evolve.GatesText, evolve.GatesDone);
            AddCondition(evolve.FeatsText, evolve.FeatsDone);
            AddCondition(evolve.NeuronText, evolve.NeuronEnough);

            Button button = new Button();
            button.text = _session.Strings.Get("neuron.evolve.button");
            button.AddToClassList("neuron-evolve-button");
            button.SetEnabled(evolve.CanEvolve);

            NeuronNode node = _session.Network.EvolveNodeOfCurrentStage();
            if (evolve.CanEvolve && node != null)
            {
                string id = node.Id;
                button.clicked += () => Buy(id);
            }

            _neuronEvolve.Add(button);
        }

        private void AddCondition(string text, bool done)
        {
            Label label = Text(text, "neuron-condition");
            label.AddToClassList(done ? "neuron-condition-done" : "neuron-condition-missing");
            _neuronEvolve.Add(label);
        }

        private void Buy(string nodeId)
        {
            NeuronNode node = _session.Network.Node(nodeId);
            bool isEvolveNode = node != null && node.Type == NodeType.Evolve;

            _lastPurchase = _session.Network.Buy(nodeId);
            RefreshNetwork();
            Refresh();

            // design.md §7: mỗi lần tiến hóa có màn ăn mừng riêng. Biết đây là nút Tiến hóa vì
            // chính mình vừa bấm nó, và Core đã xác nhận mua thành công — không tự suy từ giai đoạn.
            if (isEvolveNode && _lastPurchase.Succeeded)
            {
                Show(_neuronOverlay, false);
                ShowEvolutionScreen(EndScreenModel.StageName(_session, _session.State.Stage));
            }
        }

        // --- vòng lượt ------------------------------------------------------------------------

        private void OnEndTurn()
        {
            // Chốt chặn thứ hai. Cả hai màn đều đã tắt nút khi Core nói chưa kết thúc lượt được,
            // nhưng Core NÉM chứ không trả lỗi — mà một ngoại lệ không bắt giữa buổi chơi thử là
            // mất cả phiên. Nút tắt là lớp một; dòng này là lớp hai.
            PlanResult check = _session.Turn.Plan.ValidateForEndTurn();
            if (!check.Succeeded)
            {
                Debug.LogWarning("Bấm Kết thúc lượt khi chưa hợp lệ: " + check.Message);
                Refresh();
                return;
            }

            TurnOutcome outcome = _session.EndTurn();
            TurnResultModel result = TurnResultModel.Build(_session, outcome);

            _resultTitle.text = result.Title;
            _resultContinue.text = result.ContinueText;
            _resultLines.Clear();
            for (int i = 0; i < result.Lines.Count; i++)
            {
                _resultLines.Add(Text(result.Lines[i], "result-line"));
            }

            Refresh();
            Show(_resultOverlay, true);
        }

        private void OnResultContinue()
        {
            Show(_resultOverlay, false);
            if (_session.IsOver)
            {
                ShowEndScreen();
            }
        }

        // --- menu, màn thắng, màn thua, màn ăn mừng (A4) ---------------------------------------

        private void ShowMenu()
        {
            EndScreenModel menu = EndScreenModel.Menu(_session.Strings);
            _menuTitle.text = menu.Title;
            _menuBody.text = menu.Body;
            _menuNewGame.text = menu.ButtonText;
            Show(_menuOverlay, true);
        }

        private void OnNewGameFromMenu()
        {
            Show(_menuOverlay, false);
            StartFreshGame();
        }

        /// <summary>
        /// Thắng hay thua đều là một màn: chữ khác nhau, thống kê giống nhau.
        /// Phán quyết lấy từ GameSession, tức từ TurnOutcome của Core.
        /// </summary>
        private void ShowEndScreen()
        {
            EndScreenModel model = _session.IsWon
                ? EndScreenModel.Win(_session)
                : EndScreenModel.Lose(_session);

            _endTitle.text = model.Title;
            _endBody.text = model.Body;
            _endButton.text = model.ButtonText;

            _endLines.Clear();
            for (int i = 0; i < model.Lines.Count; i++)
            {
                _endLines.Add(Text(model.Lines[i], "result-line"));
            }

            Show(_endOverlay, true);
        }

        private void ShowEvolutionScreen(string newStageName)
        {
            EndScreenModel model = EndScreenModel.Evolution(_session, newStageName);

            _evolveTitle.text = model.Title;
            _evolveBody.text = model.Body;
            _evolveContinue.text = model.ButtonText;

            _evolveLines.Clear();
            for (int i = 0; i < model.Lines.Count; i++)
            {
                _evolveLines.Add(Text(model.Lines[i], "result-line"));
            }

            Show(_evolveOverlay, true);
        }

        private void StartFreshGame()
        {
            bool hintsEnabled = _session.Hints.Enabled;

            _session = _bootstrap.NewGame();
            _session.Hints.Enabled = hintsEnabled;

            _lastPurchase = null;
            Show(_endOverlay, false);
            Show(_resultOverlay, false);
            Show(_hintCatalogOverlay, false);
            Show(_neuronOverlay, false);
            Show(_evolveOverlay, false);
            Refresh();
        }

        /// <summary>Ván mới thì gợi ý hiện lại từ đầu, nhưng ai đã tắt hẳn gợi ý thì vẫn tắt.</summary>
        private void OnRestart()
        {
            StartFreshGame();
        }

        // --- tiện ích -------------------------------------------------------------------------

        private static Label Text(string value, string className)
        {
            Label label = new Label(value);
            label.AddToClassList(className);
            return label;
        }

        private static void Show(VisualElement element, bool visible)
        {
            element.EnableInClassList("hidden", !visible);
        }
    }
}
