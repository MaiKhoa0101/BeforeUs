using System;
using BeforeUs.Core;

namespace BeforeUs.Combat
{
    /// <summary>
    /// ⚠ DỤNG CỤ ĐO, KHÔNG PHẢI LỰA CHỌN THIẾT KẾ. Đừng dùng trong game thật.
    ///
    /// Chỉ những ai ngã trong sân mới chết; không bù cho đủ mức tổn thất của luật
    /// (design.md §5.2 "mất 2 người", §8 quota). Tồn tại đúng một lý do: để B3 chạy được cùng
    /// một bộ 500 trận theo hai cách rồi đặt hai bảng cạnh nhau, cho Khoa chốt bằng số thật
    /// thay vì đoán trước khi nhìn số.
    ///
    /// Cùng kỷ luật với cờ `applyNodeEffects` của Core (docs/core-api.md §11): một công tắc đổi luật
    /// mà không được gắn nhãn là một công tắc sẽ lên bản phát hành ở sai vị trí. Mặc định ở mọi chỗ
    /// nối dây phải là <see cref="DesignTollCasualtyPolicy"/>.
    /// </summary>
    public sealed class SceneOnlyCasualtyPolicy : ICasualtyPolicy
    {
        public CombatResult Apply(CombatRequest request, BattleReport report)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (report == null) throw new ArgumentNullException(nameof(report));

            return new CombatResult(report.Won, report.DeadMemberIds);
        }
    }
}
