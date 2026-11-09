using System.Globalization;

namespace BeforeUs.Game
{
    /// <summary>
    /// Chỗ DUY NHẤT định dạng số cho người chơi đọc. Đừng gọi ToString ở nơi khác: rải ra là
    /// sớm muộn có chỗ quên, rồi màn hình có chỗ "0,91" chỗ "0.91".
    ///
    /// Dấu thập phân là dấu phẩy theo quy ước Việt. Không dùng CultureInfo("vi-VN") vì bản build
    /// WebGL có thể chạy ở chế độ invariant globalization, khi đó tra culture theo tên sẽ im lặng
    /// rơi về invariant và dấu phẩy biến mất. Sửa thẳng NumberFormatInfo thì không phụ thuộc ICU.
    /// </summary>
    public static class Formatting
    {
        private static readonly NumberFormatInfo Vietnamese = CreateVietnamese();

        /// <summary>Hệ số nhanh nhẹn / khỏe: hai chữ số thập phân, dấu phẩy.</summary>
        public static string Stat(double value)
        {
            return value.ToString("0.00", Vietnamese);
        }

        public static string Int(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Số có dấu, dùng cho mức thay đổi: +3, −2.</summary>
        public static string Signed(int value)
        {
            return value > 0 ? "+" + Int(value) : Int(value);
        }

        /// <summary>Số lượng có một chữ số thập phân: 4.25 → "4,3". Dùng cho con số kỳ vọng.</summary>
        public static string Amount(double value)
        {
            return value.ToString("0.0", Vietnamese);
        }

        /// <summary>Tỉ lệ 0..1 thành phần trăm làm tròn: 0.655 → "66%".</summary>
        public static string Percent(double ratio)
        {
            return Int((int)System.Math.Round(ratio * 100.0, System.MidpointRounding.AwayFromZero)) + "%";
        }

        private static NumberFormatInfo CreateVietnamese()
        {
            NumberFormatInfo format = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            format.NumberDecimalSeparator = ",";
            format.NumberGroupSeparator = ".";
            return format;
        }
    }
}
