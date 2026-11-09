using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Neuron;
using BeforeUs.Core.Plants;
using BeforeUs.Core.Sites;

namespace BeforeUs.Game
{
    /// <summary>
    /// Một ván đang chơi: gom đúng những thứ Core cần để chạy vòng lượt, cộng bảng chuỗi.
    /// Thuần C#, không tham chiếu UnityEngine — tầng Unity chỉ đưa vào nội dung hai file JSON.
    ///
    /// Lớp này KHÔNG tính luật. Mọi thay đổi trạng thái đều đi qua TurnSystem của Core.
    /// </summary>
    public sealed class GameSession
    {
        private GameSession(
            BalanceConfig config,
            GameState state,
            IRandom random,
            TurnSystem turn,
            Climate climate,
            EventSystem events,
            NeuronNetwork network,
            NeuronData neuronData,
            PlantCatalog plants,
            StringTable strings,
            int seed)
        {
            Config = config;
            State = state;
            Random = random;
            Turn = turn;
            Climate = climate;
            Events = events;
            Network = network;
            NeuronData = neuronData;
            Plants = plants;
            Strings = strings;
            Seed = seed;
            Editor = new TurnPlanEditor(turn);
            Hints = new HintState();
        }

        public BalanceConfig Config { get; }
        public GameState State { get; }
        public IRandom Random { get; }
        public TurnSystem Turn { get; }

        /// <summary>Cùng thể hiện mà TurnSystem dùng, để banner đổi pha hỏi đúng nguồn (design.md §10).</summary>
        public Climate Climate { get; }

        /// <summary>Hệ sự kiện của C6. Đọc Events.LastEvent sau EndTurn để kể lại chuyện vừa xảy ra.</summary>
        public EventSystem Events { get; }

        /// <summary>Mạng lưới neuron của C5. Cũng chính là IPurchasedNodes mà TurnSystem dùng để mở khóa.</summary>
        public NeuronNetwork Network { get; }

        /// <summary>Dữ liệu thô của neurons.json: tên nhánh, tên giai đoạn, lời mô tả hiệu ứng.
        /// Đây là NỘI DUNG của Khoa, nên màn hình đọc thẳng từ đây chứ không chép lại vào strings.vi.json.</summary>
        public NeuronData NeuronData { get; }

        /// <summary>Bộ loài thực vật của v7 (plants.json). Hái lượm không chạy được nếu thiếu.</summary>
        public PlantCatalog Plants { get; }

        public StringTable Strings { get; }
        public TurnPlanEditor Editor { get; }
        public HintState Hints { get; }

        /// <summary>
        /// Hệ slot công trình của Core (design-v7 Phần B). Stub ISiteSystem của nhánh giao diện
        /// đã bị xoá khi Core land Phần B — Core nhận đúng chữ ký đó, nên đổi chỗ này là xong.
        /// </summary>
        public SiteSystem Sites
        {
            get { return Turn.Sites; }
        }
        public int Seed { get; }

        /// <summary>Báo cáo của lượt vừa kết thúc, hoặc null khi chưa kết thúc lượt nào.</summary>
        public TurnOutcome LastOutcome { get; private set; }

        /// <summary>Bộ tộc đã tuyệt diệt — thua (design.md §1). Phán quyết của Core.</summary>
        public bool IsLost { get; private set; }

        /// <summary>
        /// Đã đạt giai đoạn thắng — thắng ván (design.md §1). Lấy thẳng TurnOutcome.Won của Core;
        /// tầng này KHÔNG so state.Stage với config.WinStage, để luật thắng chỉ có một bản cài.
        ///
        /// Hệ quả nhìn thấy được: Core chốt cờ này trong EndTurn(), nên nếu người chơi mở nút
        /// Tiến hóa giữa lượt thì màn ăn mừng hiện ngay, còn màn thắng đợi tới khi bấm Kết thúc lượt.
        /// </summary>
        public bool IsWon { get; private set; }

        /// <summary>Ván đã kết thúc, thắng hay thua.</summary>
        public bool IsOver
        {
            get { return IsLost || IsWon; }
        }

        /// <summary>
        /// Bộ giải quyết combat thật của Core (design.md §5.2, §8). Nhận qua nhà máy nên nó chắc chắn
        /// dùng chung một IRandom với TurnSystem — điều kiện bắt buộc để ván tái lập được và để
        /// lưu ván (C7) khôi phục đúng.
        /// </summary>
        public static readonly Func<BalanceConfig, IRandom, IPurchasedNodes, ICombatResolver> Probabilistic =
            (config, random, nodes) => new ProbabilisticCombatResolver(config, random, nodes);

        /// <summary>
        /// Dựng ván mới. <paramref name="balanceJson"/> là nội dung Assets/Data/balance.json,
        /// <paramref name="stringsJson"/> là nội dung Assets/Data/strings.vi.json.
        ///
        /// Combat nhận qua nhà máy chứ không nhận thể hiện dựng sẵn: IRandom chỉ ra đời bên trong
        /// hàm này, và bộ giải quyết BẮT BUỘC phải rút từ đúng thể hiện đó.
        /// </summary>
        public static GameSession NewGame(
            string balanceJson,
            string stringsJson,
            string neuronsJson,
            string plantsJson,
            string sitesJson,
            int seed,
            Func<BalanceConfig, IRandom, IPurchasedNodes, ICombatResolver> combatResolverFactory,
            bool applyNodeEffects = true,
            IEventDecisions eventDecisions = null)
        {
            if (combatResolverFactory == null)
            {
                throw new ArgumentNullException(nameof(combatResolverFactory));
            }

            BalanceConfig config = BalanceConfig.FromJson(balanceJson);
            StringTable strings = StringTable.FromJson(stringsJson);

            IRandom random = new XorShift128Random(seed);
            MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(config));
            GameState state = GameState.NewGame(config, random, factory);

            // Mạng lưới neuron của C5. Cùng một thể hiện vừa là màn A3 vừa là IPurchasedNodes mà
            // ActivityRules hỏi để mở khóa Xây dựng và Săn bắt (D1) — một nguồn sự thật, không hai.
            PlantCatalog plants = PlantCatalog.Load(plantsJson);
            SiteCatalog siteCatalog = SiteCatalog.Load(sitesJson);
            NeuronData neuronData = NeuronNetwork.LoadData(neuronsJson);
            NeuronNetwork network = new NeuronNetwork(neuronData, config, state, applyNodeEffects);
            IPurchasedNodes nodes = network;

            Climate climate = new Climate(config);
            ICombatResolver resolver = combatResolverFactory(config, random, nodes);

            // Sự kiện của C6. IEventDecisions là quyết định của NGƯỜI CHƠI; tới I1 mới có đường
            // hỏi thật (EndTurn gọi đồng bộ, không chờ được hộp thoại), nên tạm dùng bản thay thế
            // do tầng trên chỉ định. Xem câu hỏi cho Khoa trong báo cáo A2.
            EventSystem events = new EventSystem(
                eventDecisions ?? new NeverAttackDecisions(),
                resolver as ICombatOdds);

            TurnSystem turn = new TurnSystem(
                config,
                state,
                random,
                factory,
                climate,
                new Difficulty(config),
                nodes,
                resolver,
                events,
                null,
                plants,
                siteCatalog);

            GameSession session = new GameSession(config, state, random, turn, climate, events, network, neuronData, plants, strings, seed);
            turn.BeginTurn();
            return session;
        }

        /// <summary>
        /// Dựng ván với một bộ giải quyết combat đã có sẵn. Dành cho test double
        /// (<see cref="ScriptedCombatResolver"/>) — nó không rút số ngẫu nhiên nên không cần
        /// dùng chung IRandom. Khi chạy thật thì dùng <see cref="Probabilistic"/>.
        /// </summary>
        public static GameSession NewGame(
            string balanceJson,
            string stringsJson,
            string neuronsJson,
            string plantsJson,
            string sitesJson,
            int seed,
            ICombatResolver combatResolver,
            bool applyNodeEffects = true,
            IEventDecisions eventDecisions = null)
        {
            if (combatResolver == null)
            {
                throw new ArgumentNullException(nameof(combatResolver));
            }

            return NewGame(
                balanceJson,
                stringsJson,
                neuronsJson,
                plantsJson,
                sitesJson,
                seed,
                (config, random, nodes) => combatResolver,
                applyNodeEffects,
                eventDecisions);
        }

        /// <summary>
        /// Kết thúc lượt: chỗ DUY NHẤT dự định biến thành kết quả (D2). Sau đó tự mở lượt kế,
        /// trừ khi bộ tộc đã tuyệt diệt.
        /// </summary>
        public TurnOutcome EndTurn()
        {
            if (IsOver)
            {
                return LastOutcome;
            }

            TurnOutcome outcome = Turn.EndTurn();

            LastOutcome = outcome;
            IsLost = outcome.TribeWipedOut;
            IsWon = outcome.Won;
            Editor.Reset();

            if (!IsOver)
            {
                Turn.BeginTurn();
            }

            return outcome;
        }
    }
}
