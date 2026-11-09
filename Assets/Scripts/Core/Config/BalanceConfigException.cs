using System;

namespace BeforeUs.Core.Config
{
    /// <summary>Số liệu cân bằng không đọc được hoặc không hợp lệ. Luôn nổ to, không bao giờ im lặng dùng giá trị mặc định.</summary>
    public class BalanceConfigException : Exception
    {
        public BalanceConfigException(string message) : base(message)
        {
        }

        public BalanceConfigException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
