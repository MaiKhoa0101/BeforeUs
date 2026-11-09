using BeforeUs.Core;

namespace BeforeUs.Game
{
    /// <summary>
    /// Tên ảnh tạm cho từng thứ trên màn hình. Thuần C#: chỗ này chỉ chọn TÊN, việc nạp ảnh
    /// thật là của tầng giao diện (xem BeforeUs.UI.PlaceholderArt).
    ///
    /// Tách ra để khi Khoa thay bằng art thật, phần duy nhất phải động tới là thư mục ảnh —
    /// không đụng tới logic chọn dáng hay chọn biến thể.
    /// </summary>
    public static class PlaceholderArtKeys
    {
        public const string Folder = "BeforeUsPlaceholder/";

        /// <summary>
        /// Dáng theo việc: đang được cử đi thì ĐỨNG, đang rảnh thì NGỒI.
        ///
        /// Biến thể nam/nữ rút từ id cho ổn định giữa các lần vẽ lại. Đây thuần là hình thức:
        /// Core không có khái niệm giới tính, và Member.Traits còn rỗng tới F1 nên chưa có
        /// dữ liệu thật nào để dựa vào. Khi F1 có đặc điểm, đổi đúng hàm này.
        /// </summary>
        public static string ForMember(int memberId, bool assigned)
        {
            return ForMemberPose(memberId, assigned);
        }

        /// <summary>
        /// Dáng do bên gọi quyết. Màn cộng đồng lấy dáng từ SLOT chứ không từ việc đang làm
        /// (Khoa: "slot 1 ngồi, slot 2 đứng"), nên hai màn hỏi cùng một hàm với ý nghĩa khác nhau.
        /// </summary>
        public static string ForMemberPose(int memberId, bool standing)
        {
            string who = (memberId % 2 == 0) ? "female" : "male";
            string pose = standing ? "stand" : "sit";
            return "member_stage1_" + who + "_" + pose;
        }

        /// <summary>Vật thể gắn với hoạt động (góp ý 6–7 của Khoa). Null là chưa có hình.</summary>
        public static string ForActivity(ActivityKind kind)
        {
            switch (kind)
            {
                case ActivityKind.Gather: return "object_food_nest";
                case ActivityKind.Hunt: return "object_club";
                case ActivityKind.Build: return "object_rock_pile";
                case ActivityKind.Explore: return "object_branch";
                default: return null;   // Nâng tinh thần chưa có vật thể riêng
            }
        }
    }
}
