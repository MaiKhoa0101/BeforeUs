using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    /// <summary>
    /// Bộ giải quyết combat theo công thức (design.md §5.2 và §8).
    ///
    /// Đây là bản cài DUY NHẤT của các công thức đó trong toàn dự án. Nhánh combat thật (B1–B4)
    /// KHÔNG được chép lại công thức; nó gọi <see cref="WinChance"/> để hiệu chỉnh (B3), còn khi
    /// scene combat thật chạy thì nó thay thế lớp này qua ICombatResolver chứ không chạy song song.
    /// Hai bản cài cùng một công thức chắc chắn sẽ trôi khỏi nhau (HANDOFF.md §8, rủi ro số 1).
    ///
    /// Thứ tự rút số ngẫu nhiên, cố định để ván tái lập được:
    ///   1) một lần rút quyết thắng/thua;
    ///   2) nếu thắng: một lần rút quyết có mất người hay không;
    ///   3) chọn nạn nhân: mỗi người một lần rút.
    /// </summary>
    public sealed class ProbabilisticCombatResolver : ICombatResolver, ICombatOdds
    {
        private readonly BalanceConfig _config;
        private readonly IRandom _random;
        private readonly IPurchasedNodes _nodes;

        public ProbabilisticCombatResolver(BalanceConfig config, IRandom random, IPurchasedNodes nodes = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (random == null) throw new ArgumentNullException(nameof(random));

            _config = config;
            _random = random;
            _nodes = nodes ?? NoPurchasedNodes.Instance;
        }

        /// <summary>
        /// Tỉ lệ thắng theo công thức, đã clamp. Công khai để B3 hiệu chỉnh combat thật
        /// so với chính con số Core dùng, thay vì so với một bản chép tay.
        ///
        /// design.md §5.2:
        ///   p_hunt    = clamp(0.50 + 0.08·k + 0.05·build − 0.02·level, 0.05, 0.92)
        ///   p_explore = clamp(0.40 + 0.08·k + 0.05·build − 0.02·level, 0.05, 0.92)
        /// design.md §8:
        ///   p_defence = clamp(0.35 + 0.05·crew + 0.06·build + 0.04·stage − 0.05·region − 0.02·level, 0.05, 0.95)
        ///
        /// Cơ hội combat dùng chung công thức phòng thủ (before_us_sim.py:110-112).
        /// </summary>
        public double WinChance(CombatRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Kind == CombatKind.Tutorial)
            {
                return 1.0;
            }

            // Các hệ số trong balance.json đã mang sẵn dấu (per_region −0.05, difficulty −0.02): luôn CỘNG.
            double difficultyDelta = _config.Data.Difficulty.PerLevel.CombatOdds * request.DifficultyLevel;

            if (request.Kind == CombatKind.Hunt || request.Kind == CombatKind.Explore)
            {
                CombatOddsData odds = _config.Data.CombatOdds;
                bool hunt = request.Kind == CombatKind.Hunt;

                double p = (hunt ? _config.Data.Activities.Hunt.BaseWin : _config.Data.Activities.Explore.BaseWin)
                           + odds.PerMember * request.PartySize
                           + odds.PerBuildLevel * request.BuildLevel
                           + difficultyDelta;

                if (!hunt)
                {
                    p += _nodes.Effects.ExploreOddsBonus;   // nút "Bàn chân vòm"
                }

                return MathUtil.Clamp(p, odds.Min, odds.Max);
            }

            DefenceOddsData defence = _config.Data.DefenceOdds;
            double q = defence.Base
                       + defence.PerMember * request.Crew
                       + defence.PerBuildLevel * request.BuildLevel
                       + defence.PerStage * request.Stage
                       + defence.PerRegion * request.Region
                       + difficultyDelta
                       + _nodes.Effects.DefenceOddsBonus;   // nút "Tiếng gọi báo động"

            return MathUtil.Clamp(q, defence.Min, defence.Max);
        }

        /// <summary>
        /// Số người quái thú ăn khi phòng thủ thua (design.md §8):
        /// quota = raid_quota_by_region[region] + raid_quota × level. Không bao giờ xóa sổ cả bầy.
        /// </summary>
        public int RaidQuota(CombatRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            int quota = _config.RaidQuotaForRegion(request.Region)
                        + _config.Data.Difficulty.PerLevel.RaidQuota * request.DifficultyLevel;

            return Math.Max(0, quota);
        }

        public CombatResult Resolve(CombatRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // design.md §8: trận hướng dẫn ở lượt 3 luôn thắng và không ai chết.
            if (request.Kind == CombatKind.Tutorial)
            {
                return CombatResult.Win();
            }

            CombatResultsData results = _config.Data.CombatResults;
            bool won = _random.NextDouble() < WinChance(request);

            int deaths;
            if (won)
            {
                // design.md §5.2: thắng vẫn có thể mất 1 người, trung bình 50%.
                // Bản xác suất gộp mọi kiểu tấn công của quái thú thành một con số; kiểu tấn công thật
                // (design.md §9) là việc của scene combat ở nhánh B.
                deaths = _random.NextDouble() < results.WinDeathChanceAvg ? 1 : 0;
            }
            else if (request.Kind == CombatKind.Defence)
            {
                deaths = RaidQuota(request);
            }
            else
            {
                deaths = results.LossDeaths;
            }

            return new CombatResult(won, PickVictims(request, deaths));
        }

        /// <summary>
        /// Chọn nạn nhân ngẫu nhiên trong TOÀN BỘ bộ tộc, không riêng người đi trận:
        /// design.md §5.2 ghi "Thua: mất 2 người" không kèm điều kiện tham gia, và
        /// before_us_sim.py:95 cũng trừ thẳng vào sĩ số.
        /// </summary>
        private int[] PickVictims(CombatRequest request, int deaths)
        {
            if (deaths <= 0)
            {
                return null;
            }

            List<int> pool = new List<int>(request.AllMemberIds);

            // D11: thua Phòng thủ BẮT BUỘC thì quái thú "ăn đúng quota người rồi rút, không bao giờ
            // xóa sổ cả bầy" (design.md §8) — một trận KHÔNG NÉ ĐƯỢC không bao giờ được thành
            // cú thua tức thì.
            //
            // Số người phải chừa lại SUY RA TỪ NGƯỠNG THUA, không viết cứng: sau góp ý 8 của Khoa
            // (playtest 01) ngưỡng thua là "còn <= 1 người", nên chừa đúng 1 người chính là thua và
            // D11 sẽ thành vô nghĩa. MinSurvivorsAfterForcedCombat = ngưỡng thua + 1 giữ đúng ý định
            // ban đầu, và tự theo nếu Khoa đổi ngưỡng lần nữa.
            //
            // Vẫn CHỈ áp cho Phòng thủ. Săn bắt, Khám phá và Cơ hội combat là TÙY CHỌN —
            // người chơi tự chọn đánh thì chịu, chúng vẫn xóa sổ được cả bầy.
            int cap = request.Kind == CombatKind.Defence
                ? Math.Max(0, pool.Count - _config.MinSurvivorsAfterForcedCombat)
                : pool.Count;

            int count = Math.Min(deaths, cap);
            int[] victims = new int[count];

            for (int i = 0; i < count; i++)
            {
                int index = _random.NextInt(0, pool.Count);
                victims[i] = pool[index];
                pool.RemoveAt(index);
            }

            return victims;
        }
    }
}
