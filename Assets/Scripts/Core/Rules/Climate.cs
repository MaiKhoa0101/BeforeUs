using System;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    public enum ClimatePhase
    {
        /// <summary>Pha Ẩm.</summary>
        Wet,

        /// <summary>Pha Khô: hái lượm giảm, thú tấn công hang nhiều hơn.</summary>
        Dry
    }

    /// <summary>
    /// Khí hậu xen kẽ Ẩm/Khô (design.md §10). Hàm thuần theo số lượt: không giữ trạng thái,
    /// nên tải ván giữa chừng không bao giờ lệch pha.
    /// </summary>
    public sealed class Climate
    {
        /// <summary>design.md §10: báo trước 2 lượt khi sắp đổi pha.</summary>
        public const int DefaultWarningTurns = 2;

        private readonly int _phaseLength;
        private readonly ClimatePhase _firstPhase;

        public Climate(BalanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _phaseLength = config.Data.Climate.PhaseLength;
            _firstPhase = config.Data.Climate.FirstPhase == "dry" ? ClimatePhase.Dry : ClimatePhase.Wet;
        }

        public int PhaseLength
        {
            get { return _phaseLength; }
        }

        public ClimatePhase FirstPhase
        {
            get { return _firstPhase; }
        }

        /// <summary>Pha của lượt. Lượt 1–10 Ẩm, 11–20 Khô, 21–30 Ẩm… khi phase_length = 10.</summary>
        public ClimatePhase PhaseAt(int turn)
        {
            RequireTurn(turn);
            bool flipped = ((turn - 1) / _phaseLength) % 2 == 1;
            if (!flipped)
            {
                return _firstPhase;
            }

            return _firstPhase == ClimatePhase.Wet ? ClimatePhase.Dry : ClimatePhase.Wet;
        }

        public bool IsDry(int turn)
        {
            return PhaseAt(turn) == ClimatePhase.Dry;
        }

        /// <summary>Còn bao nhiêu lượt nữa thì đổi pha. Lượt cuối của một pha trả về 1.</summary>
        public int TurnsUntilPhaseChange(int turn)
        {
            RequireTurn(turn);
            return _phaseLength - ((turn - 1) % _phaseLength);
        }

        /// <summary>Có nên báo trước "sắp đổi pha" ở lượt này không (design.md §10).</summary>
        public bool IsPhaseChangeImminent(int turn, int warningTurns = DefaultWarningTurns)
        {
            if (warningTurns < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(warningTurns));
            }

            return TurnsUntilPhaseChange(turn) <= warningTurns;
        }

        private static void RequireTurn(int turn)
        {
            if (turn < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(turn), "lượt đếm từ 1");
            }
        }
    }
}
