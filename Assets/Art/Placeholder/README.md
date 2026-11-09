# Hình tạm — do agent vẽ bằng code

**Toàn bộ hình trong thư mục này là hàng tạm, không phải art của Khoa.** Chúng được sinh ra
bằng script, bằng hình học đơn giản, để có thứ nhìn thấy được trong game trong lúc chờ bản vẽ thật.

- Sinh ngày: **08/11/2026**
- Sinh bởi: `tools/art/make_placeholder_art.py` (Python, chỉ thư viện chuẩn)
- Kiểm bởi: `tools/art/verify_art.py`
- Danh sách đầy đủ: `manifest.json` cạnh file này

## Cách thay bằng bản vẽ của Khoa

Thả PNG **cùng tên, cùng kích thước** vào `Resources/BeforeUsPlaceholder/`, ghi đè file cũ.
Không phải sửa code, không phải sửa scene.

Hai điều kiện để thay xong là chạy đúng ngay:

| | |
| --- | --- |
| **Cùng khung** | Thành viên **96 × 128**, vật thể **96 × 96** |
| **Cùng đường chân** | Thành viên đứng/ngồi trên **y = 122**, vật thể trên **y = 88** (tính từ mép trên) |

Nền trong suốt, PNG RGBA 8 bit.

Khi đã thay hết, xoá thư mục này và `tools/art/` — không còn gì tham chiếu tới chúng ngoài
`PlaceholderArt.cs` ở tầng giao diện.

## Sinh lại

```
python tools/art/make_placeholder_art.py     # vẽ lại cả 8 hình
python tools/art/verify_art.py               # giải mã lại, in bóng dáng bằng chữ, kiểm đường chân
```

Muốn đổi bảng màu, tỉ lệ hay dáng: **sửa script rồi chạy lại.** Đừng sửa PNG bằng tay — lần chạy
sau sẽ ghi đè. Đây là lý do bộ hình này làm bằng code chứ không vẽ tay từng file.

## Phong cách đã chọn

Bóng phẳng, bảng màu hạn chế (15 màu), không viền nét, không chuyển sắc. Chọn vậy vì đọc được ở
kích thước nhỏ và trông có chủ đích thay vì vụng. Dự án đang có sẵn vài ngôn ngữ thị giác khác
nhau; bộ này cố ý đồng nhất trong chính nó để nhìn liền mạch cho tới khi Khoa thay cả bộ.

## Không đụng tới

`Assets/Sprite/**` là art Khoa tự vẽ. Agent không chạm một byte nào trong đó.
