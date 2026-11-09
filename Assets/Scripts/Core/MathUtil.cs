using System;

namespace BeforeUs.Core
{
    /// <summary>Phép toán dùng chung cho các công thức trong docs/design.md.</summary>
    public static class MathUtil
    {
        /// <summary>clamp(x, a, b) của design.md §5.2 và §8.</summary>
        public static double Clamp(double value, double min, double max)
        {
            if (min > max)
            {
                throw new ArgumentException("min phải <= max", nameof(min));
            }

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>Bản số nguyên, dùng cho sức chứa, quota, chỉ số vùng.</summary>
        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentException("min phải <= max", nameof(min));
            }

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
