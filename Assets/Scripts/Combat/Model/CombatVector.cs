using System;
using System.Globalization;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Một điểm trên sân trận. Đơn vị do lớp hiển thị quy đổi sang đơn vị Unity.
    ///
    /// Là struct thuần, cố ý KHÔNG dùng UnityEngine.Vector2: assembly này đặt noEngineReferences,
    /// nên cùng một phép tính chạy y hệt trong `dotnet test` (B3 headless) lẫn trong Editor.
    /// </summary>
    public readonly struct CombatVector : IEquatable<CombatVector>
    {
        public CombatVector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }

        public double Y { get; }

        public static readonly CombatVector Zero = new CombatVector(0.0, 0.0);

        public double DistanceTo(CombatVector other)
        {
            double dx = other.X - X;
            double dy = other.Y - Y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        /// <summary>
        /// Điểm mới sau khi đi về phía <paramref name="target"/> tối đa <paramref name="maxDistance"/>.
        /// Không bao giờ đi quá đích: tới nơi thì dừng đúng ở đích.
        /// </summary>
        public CombatVector MovedToward(CombatVector target, double maxDistance)
        {
            if (maxDistance <= 0.0)
            {
                return this;
            }

            double dx = target.X - X;
            double dy = target.Y - Y;
            double distance = Math.Sqrt((dx * dx) + (dy * dy));

            if (distance <= maxDistance || distance <= 0.0)
            {
                return target;
            }

            double scale = maxDistance / distance;
            return new CombatVector(X + (dx * scale), Y + (dy * scale));
        }

        public bool Equals(CombatVector other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is CombatVector other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static bool operator ==(CombatVector left, CombatVector right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CombatVector left, CombatVector right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return "(" + X.ToString("0.##", CultureInfo.InvariantCulture)
                   + ", " + Y.ToString("0.##", CultureInfo.InvariantCulture) + ")";
        }
    }
}
