using System;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using UnityEngine;

namespace BeforeUs.Game
{
    /// <summary>
    /// Nguồn kết quả combat.
    ///
    /// <see cref="Probabilistic"/> là bản thật của Core (design.md §5.2, §8) — mặc định khi chơi.
    /// Ba lựa chọn còn lại là kịch bản test: chúng không đọc combat_odds, không đọc base_win,
    /// không tính xác suất; kết quả do kịch bản quy định sẵn. Để gỡ lỗi giao diện: ép thắng,
    /// ép thua, xem bảng kết quả và màn thua mà không phải chơi đi chơi lại chờ may rủi.
    /// </summary>
    public enum CombatSource
    {
        Probabilistic,
        ScriptedAlwaysWin,
        ScriptedAlwaysLose,
        ScriptedAlternating
    }

    /// <summary>
    /// Màn hình nào đang chạy.
    ///
    /// <see cref="Classic"/> là màn A1–A4 đã nghiệm thu, giữ lại làm bản dự phòng chơi được cho
    /// tới hết P1: nếu màn cộng đồng hỏng sát ngày thử thì gạt về đây là có ngay bản chạy được.
    /// Đừng dọn nó đi dù màn mới chạy tốt.
    /// </summary>
    public enum ScreenMode
    {
        // Giá trị GHIM TƯỜNG MINH. Scene lưu enum thành SỐ, nên đổi thứ tự khai báo ở đây
        // sẽ âm thầm lật màn hình mặc định của mọi scene đã lưu — không lỗi, không cảnh báo.
        // Viết "screen: 0" trong scene là chưa đủ; con số đó chỉ có nghĩa nhờ dòng dưới đây.
        Community = 0,
        Classic = 1
    }

    /// <summary>
    /// Cầu nối duy nhất giữa Unity và lõi luật chơi: nhận nội dung hai file JSON dưới dạng TextAsset
    /// rồi dựng GameSession. Đây là file duy nhất trong Assets/Scripts/Game/ được phép dùng UnityEngine.
    ///
    /// Vì sao TextAsset chứ không Resources.Load: Assets/Data/ không nằm trong thư mục Resources,
    /// và di chuyển balance.json nằm ngoài phạm vi ghi của nhánh giao diện. Kéo thả vào ô Inspector
    /// cũng chạy đúng trên bản build WebGL.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Dữ liệu (kéo từ Assets/Data vào)")]
        [SerializeField] private TextAsset balanceJson;
        [SerializeField] private TextAsset stringsJson;
        [SerializeField] private TextAsset neuronsJson;
        [SerializeField] private TextAsset plantsJson;
        [SerializeField] private TextAsset sitesJson;

        [Header("Ván chơi")]
        [Tooltip("0 = mỗi lần chạy một hạt khác nhau.")]
        [SerializeField] private int seed;

        [Tooltip("Bật: hiệu ứng số của nút neuron có tác dụng. Tắt: nút chỉ còn tác dụng mở khóa (cờ Cổng 2).")]
        [SerializeField] private bool applyNodeEffects = true;

        [Header("Combat")]
        [Tooltip("Để Probabilistic khi chơi. Ba lựa chọn Scripted chỉ dùng để gỡ lỗi giao diện.")]
        [SerializeField] private CombatSource combatSource = CombatSource.Probabilistic;

        [Tooltip("Số người chết khi kịch bản gỡ lỗi cho thua. Tham số kịch bản, không phải luật chơi.")]
        [SerializeField] private int scriptedDeathsOnLoss = 2;

        [Header("Màn hình")]
        [Tooltip("Community là màn cộng đồng mới. Classic là màn A1-A4 cũ, giữ làm bản dự phòng.")]
        [SerializeField] private ScreenMode screen = ScreenMode.Community;

        [Header("Đồ nghề phát triển — để tắt khi có người chơi thử")]
        [Tooltip("Bật: bấm Play là vào thẳng ván, bỏ qua menu đầu ván. Menu vẫn còn, chỉ không hiện.")]
        [SerializeField] private bool skipStartMenu;

        private GameSession _session;

        /// <summary>
        /// Vào thẳng ván, không qua menu đầu ván. Chỉ để đỡ ma sát khi bấm Play nhiều lần lúc
        /// dựng scene và chuẩn bị playtest. Mặc định TẮT, nên người chơi thử vẫn thấy menu thật.
        /// </summary>
        public bool SkipStartMenu
        {
            get { return skipStartMenu; }
        }

        /// <summary>Màn hình đang chạy. Đổi trong Inspector, hoặc bằng nút "Màn cũ" lúc chạy.</summary>
        public ScreenMode Screen
        {
            get { return screen; }
            set { screen = value; }
        }

        /// <summary>Ván đang chơi. Dựng ở lần hỏi đầu tiên.</summary>
        public GameSession Session
        {
            get
            {
                if (_session == null)
                {
                    _session = CreateSession(seed);
                }

                return _session;
            }
        }

        /// <summary>Ván mới hoàn toàn, hạt mới. Dùng cho nút Chơi lại.</summary>
        public GameSession NewGame()
        {
            _session = CreateSession(0);
            return _session;
        }

        private GameSession CreateSession(int requestedSeed)
        {
            if (balanceJson == null)
            {
                throw new InvalidOperationException(
                    "GameBootstrap: chưa gán balanceJson. Kéo Assets/Data/balance.json vào ô Balance Json.");
            }

            if (stringsJson == null)
            {
                throw new InvalidOperationException(
                    "GameBootstrap: chưa gán stringsJson. Kéo Assets/Data/strings.vi.json vào ô Strings Json.");
            }

            if (neuronsJson == null)
            {
                throw new InvalidOperationException(
                    "GameBootstrap: chưa gán neuronsJson. Kéo Assets/Data/neurons.json vào ô Neurons Json.");
            }

            if (plantsJson == null)
            {
                throw new InvalidOperationException(
                    "GameBootstrap: chưa gán plantsJson. Kéo Assets/Data/plants.json vào ô Plants Json. "
                    + "Thiếu nó thì hái lượm luôn ra 0 thức ăn — thà dừng ngay còn hơn chạy một ván hỏng.");
            }

            if (sitesJson == null)
            {
                throw new InvalidOperationException(
                    "GameBootstrap: chưa gán sitesJson. Kéo Assets/Data/sites.json vào ô Sites Json. "
                    + "Thiếu nó thì không có slot công trình và không có sức chứa hang.");
            }

            int actualSeed = requestedSeed != 0 ? requestedSeed : Environment.TickCount;

            // Nhà máy chứ không phải thể hiện: bộ giải quyết thật phải rút từ đúng IRandom
            // mà GameSession dựng cho ván này.
            return GameSession.NewGame(
                balanceJson.text,
                stringsJson.text,
                neuronsJson.text,
                plantsJson.text,
                sitesJson.text,
                actualSeed,
                CreateResolver,
                applyNodeEffects);
        }

        private ICombatResolver CreateResolver(BalanceConfig config, IRandom random, IPurchasedNodes nodes)
        {
            switch (combatSource)
            {
                case CombatSource.ScriptedAlwaysWin:
                    return ScriptedCombatResolver.AlwaysWin();

                case CombatSource.ScriptedAlwaysLose:
                    return ScriptedCombatResolver.AlwaysLose(scriptedDeathsOnLoss);

                case CombatSource.ScriptedAlternating:
                    return ScriptedCombatResolver.Alternating(scriptedDeathsOnLoss);

                default:
                    return new ProbabilisticCombatResolver(config, random, nodes);
            }
        }
    }
}
