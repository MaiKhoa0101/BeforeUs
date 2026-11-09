using System;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    /// <summary>Điều kiện mở khóa năm hoạt động (design.md §5).</summary>
    public static class ActivityRules
    {
        public static readonly ActivityKind[] All =
        {
            ActivityKind.Gather,
            ActivityKind.Morale,
            ActivityKind.Build,
            ActivityKind.Hunt,
            ActivityKind.Explore
        };

        /// <summary>
        /// Hoạt động này có mở ở lượt hiện tại không.
        ///
        /// D1: Xây dựng và Săn bắt mở bằng NÚT (`node:s2_stone_tools`, `node:s2_scavenge_meat`),
        /// đúng design.md §5 và balance.json — KHÔNG phải bằng giai đoạn như before_us_sim.py:56 làm.
        /// Sai lệch này đã được Khoa chấp nhận tạm thời; chỉ đo và báo ở C8, không sửa.
        /// </summary>
        public static bool IsUnlocked(BalanceConfig config, GameState state, IPurchasedNodes nodes, ActivityKind activity)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (!MeetsUnlockRule(config.UnlockOf(activity), state, nodes))
            {
                return false;
            }

            // design.md §5: Khám phá còn cần "còn vùng chưa đến".
            if (activity == ActivityKind.Explore && state.Region >= config.RegionMaxIndex)
            {
                return false;
            }

            return true;
        }

        private static bool MeetsUnlockRule(UnlockRule rule, GameState state, IPurchasedNodes nodes)
        {
            switch (rule.Kind)
            {
                case UnlockKind.Start:
                    return true;

                case UnlockKind.Turn:
                    return state.Turn >= rule.FromTurn;

                case UnlockKind.Node:
                    return nodes != null && nodes.Has(rule.NodeId);

                default:
                    throw new ArgumentOutOfRangeException(nameof(rule));
            }
        }
    }
}
