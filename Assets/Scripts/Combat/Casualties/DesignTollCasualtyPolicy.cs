using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Bản DÙNG THẬT. Thua thì giữ đúng mức tổn thất của luật: người ngã trong sân tính trước,
    /// thiếu thì bù bằng người ở nhà, chọn theo kiểu tấn công của quái thú (design.md §9).
    ///
    /// Hai con số tổn thất KHÔNG được tính lại ở đây:
    ///   • thua Săn bắt / Khám phá / Cơ hội combat → `combat_results.loss_deaths` đọc từ balance.json;
    ///   • thua Phòng thủ → gọi thẳng <see cref="ProbabilisticCombatResolver.RaidQuota"/> của Core.
    /// Công thức quota (design.md §8) chỉ có đúng một bản cài, và bản đó thuộc Core.
    ///
    /// Thắng thì sân là sự thật: không bù thêm ai. design.md §5.2 ghi thắng vẫn có thể mất 1 người
    /// (trung bình 50%) — con số đó phải TỰ MỌC RA từ trận đánh, và B3 đo xem nó có mọc ra thật không.
    ///
    /// Thứ tự rút số ngẫu nhiên: mỗi nạn nhân bù thêm rút tối đa hai lần (một để so chỉ số nếu phải
    /// phá hoà, một nếu kiểu tấn công là ngẫu nhiên) — xem <see cref="TargetSelection"/>.
    /// </summary>
    public sealed class DesignTollCasualtyPolicy : ICasualtyPolicy
    {
        private readonly BalanceConfig _config;
        private readonly ProbabilisticCombatResolver _odds;
        private readonly IMemberSource _members;
        private readonly IRandom _random;

        /// <param name="odds">
        /// Nguồn DUY NHẤT của quota phòng thủ. Nhận kiểu cụ thể chứ không phải một interface mỏng,
        /// để ai đọc mã cũng thấy ngay con số đến từ Core.
        /// </param>
        public DesignTollCasualtyPolicy(
            BalanceConfig config,
            ProbabilisticCombatResolver odds,
            IMemberSource members,
            IRandom random)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (odds == null) throw new ArgumentNullException(nameof(odds));
            if (members == null) throw new ArgumentNullException(nameof(members));
            if (random == null) throw new ArgumentNullException(nameof(random));

            _config = config;
            _odds = odds;
            _members = members;
            _random = random;
        }

        public CombatResult Apply(CombatRequest request, BattleReport report)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (report == null) throw new ArgumentNullException(nameof(report));

            List<int> dead = new List<int>(report.DeadMemberIds);

            if (!report.Won)
            {
                int toll = request.Kind == CombatKind.Defence
                    ? _odds.RaidQuota(request)
                    : _config.Data.CombatResults.LossDeaths;

                TopUp(dead, request, report.AttackStyle, toll);
            }

            ApplyLastSurvivorFloor(dead, request);

            return new CombatResult(report.Won, dead.ToArray());
        }

        /// <summary>
        /// Bù cho đủ <paramref name="toll"/> người bằng những ai chưa chết, lấy trong CẢ bộ tộc.
        /// Sân giết nhiều hơn mức luật thì giữ nguyên số của sân — họ chết thật rồi.
        /// </summary>
        private void TopUp(List<int> dead, CombatRequest request, BeastAttackStyle style, int toll)
        {
            if (dead.Count >= toll)
            {
                return;
            }

            List<int> pool = new List<int>();
            IReadOnlyList<int> all = request.AllMemberIds;
            for (int i = 0; i < all.Count; i++)
            {
                if (!dead.Contains(all[i]))
                {
                    pool.Add(all[i]);
                }
            }

            while (dead.Count < toll && pool.Count > 0)
            {
                int? victim = TargetSelection.PickFromTribe(style, pool, _members, _random);
                if (victim == null)
                {
                    break;
                }

                dead.Add(victim.Value);
                pool.Remove(victim.Value);
            }
        }

        /// <summary>
        /// D11 (docs/decisions.md): trận Phòng thủ bắt buộc không bao giờ xóa sổ cả bầy —
        /// design.md §8 "ăn đúng quota người rồi rút". Core giữ luật này trong
        /// <c>ProbabilisticCombatResolver.PickVictims</c>; đây KHÔNG phải bản cài thứ hai của luật,
        /// mà là chỗ kẹp kết quả của mô phỏng cho khỏi mâu thuẫn với nó.
        ///
        /// Phải kẹp cả khi người ta ngã trong sân: nếu trận phòng thủ giết sạch đội hình thì
        /// không kẹp sẽ ra một cú thua tức thì không tránh nổi, đúng cái D11 bác bỏ.
        /// Cắt từ cuối danh sách, tức bỏ người ngã sau cùng trước.
        ///
        /// Chỉ áp cho Phòng thủ. Săn bắt, Khám phá và Cơ hội combat là tùy chọn — người chơi
        /// tự chọn đánh thì chịu, chúng vẫn xóa sổ được cả bầy.
        ///
        /// B3 báo tỉ lệ trận bị kẹp ở đây; con số đó cao nghĩa là mô phỏng đang đánh nặng tay
        /// hơn luật rất nhiều, và Khoa cần biết.
        /// </summary>
        private static void ApplyLastSurvivorFloor(List<int> dead, CombatRequest request)
        {
            if (request.Kind != CombatKind.Defence)
            {
                return;
            }

            int cap = Math.Max(0, request.AllMemberIds.Count - 1);
            while (dead.Count > cap)
            {
                dead.RemoveAt(dead.Count - 1);
            }
        }
    }
}
