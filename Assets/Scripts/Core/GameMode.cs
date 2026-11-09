using System;

namespace BeforeUs.Core
{
    /// <summary>Chế độ số liệu. balance.json có vài trường tách theo chế độ (win_stage, required_neuron, period_turns, max_index).</summary>
    public enum GameMode
    {
        /// <summary>Bản đầy đủ: thắng ở giai đoạn 5.</summary>
        Full,

        /// <summary>Bản đầu tiên (vertical slice): thắng ở giai đoạn 3.</summary>
        Mvp
    }

    public static class GameModeParser
    {
        public static GameMode Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "full": return GameMode.Full;
                case "mvp": return GameMode.Mvp;
                default: throw new ArgumentException("mode phải là \"full\" hoặc \"mvp\", nhận được: " + value, nameof(value));
            }
        }
    }
}
