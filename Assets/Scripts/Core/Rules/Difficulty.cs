using System;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    /// <summary>
    /// Độ khó tăng dần theo thời gian, không theo sức mạnh thực tế của bộ tộc (design.md §11).
    ///
    /// Mọi giá trị trả về là SỐ CỘNG THÊM đã mang sẵn dấu, đúng như balance.json ghi
    /// (combat_odds −0.02, gather −1, bad_event_food −1, bad_event_morale −1, raid_quota +1).
    /// Nơi gọi cứ cộng thẳng, không tự đảo dấu — đó là chỗ dễ sai nhất của mục này.
    /// </summary>
    public sealed class Difficulty
    {
        private readonly int _periodTurns;
        private readonly DifficultyPerLevelData _perLevel;

        public Difficulty(BalanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _periodTurns = config.DifficultyPeriodTurns;
            _perLevel = config.Data.Difficulty.PerLevel;
        }

        /// <summary>Số lượt cho mỗi bậc: 20 ở bản đầy đủ, 12 ở MVP.</summary>
        public int PeriodTurns
        {
            get { return _periodTurns; }
        }

        /// <summary>difficulty_level = floor(turn / period_turns) (design.md §11).</summary>
        public int LevelAt(int turn)
        {
            if (turn < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(turn), "lượt đếm từ 1");
            }

            return turn / _periodTurns;
        }

        /// <summary>Cộng vào tỉ lệ thắng mọi combat trước khi clamp (design.md §5.2, §8, §11).</summary>
        public double CombatOddsDelta(int level)
        {
            RequireLevel(level);
            return _perLevel.CombatOdds * level;
        }

        /// <summary>Cộng vào số người quái thú ăn khi phòng thủ thua (design.md §8, §11).</summary>
        public int RaidQuotaDelta(int level)
        {
            RequireLevel(level);
            return _perLevel.RaidQuota * level;
        }

        /// <summary>Cộng vào thức ăn của sự kiện xấu (design.md §8, §11).</summary>
        public int BadEventFoodDelta(int level)
        {
            RequireLevel(level);
            return _perLevel.BadEventFood * level;
        }

        /// <summary>Cộng vào tinh thần của sự kiện xấu (design.md §8, §11).</summary>
        public int BadEventMoraleDelta(int level)
        {
            RequireLevel(level);
            return _perLevel.BadEventMorale * level;
        }

        /// <summary>
        /// Hệ số NHÂN vào mọi tỉ lệ tìm thấy thực vật (design-v7 §A.7). 1.0 ở bậc 0, giảm dần, chặn ở 0.
        ///
        /// Đây là cơ chế duy nhất kết thúc những ván ở lì (design.md §11). C8 đo được 100% số ván
        /// thua là vì đói; bỏ cơ chế này thì ván không bao giờ kết thúc.
        /// </summary>
        public double FindChanceMultiplier(int level)
        {
            RequireLevel(level);
            return Math.Max(0.0, 1.0 + _perLevel.FindChanceMultiplier * level);
        }

        private static void RequireLevel(int level)
        {
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "bậc độ khó không âm");
            }
        }
    }
}
