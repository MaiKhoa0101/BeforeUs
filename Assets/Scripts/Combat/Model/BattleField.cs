using System;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Hình học sân trận. Trục x là trục chính: thú đi về phía <see cref="GoalX"/> — vạch thoát với
    /// Dụ và hạ, cửa hang với Phòng thủ. Trục y chỉ để người chơi dàn đội hình rộng hay hẹp.
    ///
    /// Hai chiều chứ không phải một, nhưng mọi phán quyết thắng/thua chỉ đọc x: nhờ vậy lớp hiển thị
    /// vẽ được cả kiểu nhìn ngang (D8) lẫn kiểu nhìn từ trên xuống mà không đổi một dòng luật nào.
    /// </summary>
    public sealed class BattleField
    {
        public BattleField(
            double width,
            double height,
            double goalX,
            double goalRadius,
            double deployMinX,
            double deployMaxX)
        {
            if (width <= 0.0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0.0) throw new ArgumentOutOfRangeException(nameof(height));
            if (goalRadius < 0.0) throw new ArgumentOutOfRangeException(nameof(goalRadius));
            if (deployMinX > deployMaxX) throw new ArgumentException("deployMinX lớn hơn deployMaxX");

            Width = width;
            Height = height;
            GoalX = goalX;
            GoalRadius = goalRadius;
            DeployMinX = deployMinX;
            DeployMaxX = deployMaxX;
        }

        public double Width { get; }

        public double Height { get; }

        /// <summary>Dụ và hạ: vạch thoát. Phòng thủ: cửa hang hoặc mục tiêu cần giữ.</summary>
        public double GoalX { get; }

        /// <summary>Sai số coi là đã chạm đích.</summary>
        public double GoalRadius { get; }

        public double DeployMinX { get; }

        public double DeployMaxX { get; }

        /// <summary>Người chơi có được thả quân ở đây không (design.md §9: chạm vào bản đồ trận để thả).</summary>
        public bool IsInsideDeployZone(CombatVector position)
        {
            return position.X >= DeployMinX
                   && position.X <= DeployMaxX
                   && position.Y >= 0.0
                   && position.Y <= Height;
        }
    }
}
