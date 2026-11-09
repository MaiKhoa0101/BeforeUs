using System;

namespace BeforeUs.Combat
{
    /// <summary>
    /// ⚠ ĐÒN BẨY THỬ NGHIỆM, MẶC ĐỊNH TẮT. Không phải lựa chọn thiết kế đã chốt.
    ///
    /// Vì sao tồn tại: §5.2 cho tỉ lệ thắng tăng +8 điểm mỗi người, nhưng trận mô phỏng tăng
    /// +20 tới +25. Nguyên nhân là cấu trúc chứ không phải chọn số — sát thương cộng tuyến tính
    /// rồi so với một ngưỡng máu, nên xác suất thắng nhảy từ gần 0 lên gần 1 trong một quãng k
    /// rất hẹp. Chỉnh beasts.json DỜI được đường cong; làm THOẢI nó thì phải đổi thiết kế trận.
    ///
    /// Hai đòn bẩy dưới đây là cách tiêu chuẩn để làm thoải, và cả hai đều chạm vào CẢM GIÁC
    /// trận đánh — đất của Khoa ở B4. Nên chúng được cài, được đo, và được báo cáo;
    /// không cái nào được tự ý bật làm mặc định.
    ///
    /// Cùng kỷ luật với cờ `applyNodeEffects` của Core và <see cref="SceneOnlyCasualtyPolicy"/>.
    /// </summary>
    public sealed class BattleOptions
    {
        /// <summary>Mặc định: không đòn bẩy nào bật. Đây là thứ game thật dùng.</summary>
        public static readonly BattleOptions Default = new BattleOptions(0);

        public BattleOptions(int maxAttackersPerBeast)
        {
            if (maxAttackersPerBeast < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAttackersPerBeast));
            }

            MaxAttackersPerBeast = maxAttackersPerBeast;
        }

        /// <summary>
        /// ĐÒN BẨY (A) — sân có bề ngang hữu hạn, người thứ n phải chờ chỗ.
        /// 0 nghĩa là không giới hạn (mặc định, và là hành vi của toàn bộ B1–B2).
        ///
        /// Khi bật: mỗi bước, chỉ <c>MaxAttackersPerBeast</c> người GẦN NHẤT quanh một con thú
        /// được ra đòn; những người còn lại đứng chờ. Người thêm vào đóng góp giảm dần, nên
        /// đường cong theo k thoải ra — và vị trí thả quân thành quan trọng hơn số lượng.
        /// </summary>
        public int MaxAttackersPerBeast { get; }

        public bool LimitsAttackers
        {
            get { return MaxAttackersPerBeast > 0; }
        }
    }
}
