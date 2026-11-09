using System;
using System.Collections.Generic;
using BeforeUs.Core;

namespace BeforeUs.Game
{
    /// <summary>
    /// Test double để kiểm giao diện, KHÔNG phải bản mô phỏng của combat. Khi chơi thật thì dùng
    /// ProbabilisticCombatResolver của Core (mặc định của GameBootstrap).
    ///
    /// Nó cố ý không chứa một dòng luật chơi nào: không đọc combat_odds, không đọc base_win,
    /// không tính xác suất. Kết quả trận do bên gọi quy định sẵn. Công thức §5.2 và §8 chỉ được
    /// phép có đúng một bản cài, và bản đó thuộc Core (AGENTS.md "UI chỉ đọc trạng thái và gọi
    /// Core"; HANDOFF.md §8 rủi ro "agent âm thầm đổi luật").
    ///
    /// Giá trị của nó là ép được đúng trường hợp cần xem: thắng, thua, chết đúng người — thay vì
    /// chơi đi chơi lại chờ xác suất ra điều mình muốn kiểm.
    /// </summary>
    public sealed class ScriptedCombatResolver : ICombatResolver
    {
        private readonly Func<CombatRequest, CombatResult> _script;
        private readonly List<CombatRequest> _requests = new List<CombatRequest>();

        public ScriptedCombatResolver(Func<CombatRequest, CombatResult> script)
        {
            if (script == null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            _script = script;
        }

        /// <summary>Những trận đã được hỏi tới. Dùng để kiểm chứng vòng lượt, không hiện cho người chơi.</summary>
        public IReadOnlyList<CombatRequest> Requests
        {
            get { return _requests; }
        }

        public CombatResult Resolve(CombatRequest request)
        {
            _requests.Add(request);
            CombatResult result = _script(request);
            return result ?? CombatResult.Win();
        }

        /// <summary>Mọi trận đều thắng, không ai chết.</summary>
        public static ScriptedCombatResolver AlwaysWin()
        {
            return new ScriptedCombatResolver(request => CombatResult.Win());
        }

        /// <summary>
        /// Mọi trận đều thua. <paramref name="deathCount"/> là THAM SỐ KỊCH BẢN do người gọi chọn,
        /// không phải con số luật chơi: người chết lấy từ đầu danh sách được cử đi.
        /// </summary>
        public static ScriptedCombatResolver AlwaysLose(int deathCount)
        {
            return new ScriptedCombatResolver(request => CombatResult.Loss(FirstMembers(request, deathCount)));
        }

        /// <summary>Thắng, thua, thắng, thua… theo đúng thứ tự trận được hỏi tới.</summary>
        public static ScriptedCombatResolver Alternating(int deathCountOnLoss)
        {
            int seen = 0;
            return new ScriptedCombatResolver(request =>
            {
                bool win = seen % 2 == 0;
                seen++;
                return win ? CombatResult.Win() : CombatResult.Loss(FirstMembers(request, deathCountOnLoss));
            });
        }

        /// <summary>Hàng đợi kết quả đã viết sẵn; hết hàng đợi thì mọi trận sau đều thắng.</summary>
        public static ScriptedCombatResolver Queued(params CombatResult[] results)
        {
            Queue<CombatResult> queue = new Queue<CombatResult>(results ?? new CombatResult[0]);
            return new ScriptedCombatResolver(request => queue.Count > 0 ? queue.Dequeue() : CombatResult.Win());
        }

        private static List<int> FirstMembers(CombatRequest request, int count)
        {
            List<int> dead = new List<int>();
            IReadOnlyList<int> ids = request.MemberIds;
            for (int i = 0; i < ids.Count && dead.Count < count; i++)
            {
                dead.Add(ids[i]);
            }

            return dead;
        }
    }
}
