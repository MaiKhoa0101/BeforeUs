# Kế hoạch bộ asset 21 hình — CHƯA SINH GÌ

Trạng thái: **CHẶN** — plugin SpriteCook đã cài nhưng chưa được cấp quyền, nên không
có công cụ sinh ảnh nào gọi được. Ghi ngày 09/11/2026.

Đây là kế hoạch, không phải manifest. Manifest thật (`manifest.json`) do
`tools/art/make_placeholder_art.py` sinh ra và **bị ghi đè mỗi lần chạy** — đừng sửa tay.

## 1. Tám hình hiện có — GIỮ NGUYÊN, không hình nào nghỉ hưu

| File | Khung | Đường chân | Ai dùng (đã grep) |
| --- | --- | --- | --- |
| `member_stage1_{male,female}_{stand,sit}` | 96×128 | 122 | `PlaceholderArtKeys.ForMemberPose` |
| `object_food_nest` | 96×96 | 88 | màn cộng đồng + thẻ Gather màn lượt |
| `object_club` | 96×96 | 88 | màn cộng đồng + thẻ Hunt màn lượt |
| `object_rock_pile` | 96×96 | 88 | **thẻ Build màn lượt — CÒN SỐNG** |
| `object_branch` | 96×96 | 88 | **thẻ Explore màn lượt — CÒN SỐNG** |

**Đính chính:** `object_rock_pile` và `object_branch` KHÔNG hết người dùng.
`PlaceholderArtKeys.ForActivity()` (`PlaceholderArtKeys.cs:46-47`) vẫn trả về đúng hai tên
đó, và `TurnScreenModel.cs:403` gọi nó để vẽ thẻ hoạt động Build/Explore của **màn lượt**.
Thứ đổi là **màn cộng đồng**: nó chuyển sang `site.ArtKey` cho ô công trình.
Hai hình này chuyển màn, không chết. Không đánh dấu nghỉ hưu, không xoá.

## 2. Một hình mới — lối mòn

| Tên đề xuất | Khung | Đường chân |
| --- | --- | --- |
| `object_trail` | 96×96 | 88 |

Theo đúng quy ước của hai anh em cùng hàng: `CommunityScreenModel.cs:217-223` dựng
FoodNest bằng `"object_food_nest"`, Club bằng `"object_club"`, còn Trail truyền **`null`**.

**Phụ thuộc chéo — không phải việc của Art:** sinh xong PNG vẫn chưa hiện. Phải có người
đổi `null` ở `CommunityScreenModel.cs:223` thành `"object_trail"`. File đó nằm trong
`Assets/Scripts/**` — ngoài quyền ghi của tôi. Cần Lead UI/Game làm một dòng.

## 3. Mười hai hình công trình — khoá lấy từ `Assets/Data/sites.json`

Khung **96×96**, đường chân **88**, giống nhóm vật thể.

### Vùng 0 — 4 hình
| art_key | loại | giai đoạn | sức cần |
| --- | --- | --- | --- |
| `site_nhanh_cay_gay` | wood | 1 | 1.8 |
| `site_bai_da_nho` | stone | 1 | 2.2 |
| `site_hoc_da_tru_mua` | stone | 1 | 3.0 |
| `site_tang_da_lon` | stone | 2 | 3.6 |

### Vùng 1 — 7 hình
| art_key | loại | giai đoạn | sức cần |
| --- | --- | --- | --- |
| `site_to_moi_bo` | earth | 1 | 2.4 |
| `site_bui_co_voi` | wood | 1 | 2.8 |
| `site_go_dat_nut` | earth | 1 | 3.2 |
| `site_xac_cay_kho` | wood | 2 | 3.4 |
| `site_bui_gai_ran` | wood | 2 | 3.8 |
| `site_vach_da_vo` | stone | 2 | 4.2 |
| `site_dong_da_cuoi` | stone | 2 | 4.8 |

### Ổ thức ăn — 1 hình
| art_key | ghi chú |
| --- | --- |
| `site_o_dung_thuc_an` | Dùng thật, qua `HubMenuModel.cs:115`. Màn cộng đồng bỏ qua ô này (có ô riêng dùng `object_food_nest`), nhưng menu hub liệt kê mọi `VisibleSites` nên vẫn cần hình. |

Tổng: 8 cũ + 1 lối mòn + 12 công trình = **21**.

## 4. Ràng buộc kỹ thuật phải tôn trọng

- RGBA 8 bit, nền trong suốt. `verify_art.py` ném lỗi nếu khác (`depth != 8 or color_type != 6`).
- Đường chân: thành viên đáy ở `y=121`, vật thể/công trình đáy ở `y=87`
  (verify kiểm `abs(bottom - (ground-1)) <= 1`, và mọi hình cùng nhóm phải lệch nhau 0 px).
- `verify_art.py` đang **ghi cứng 8 tên** trong `groups` và sẽ nổ khi thiếu file.
  Phải nới nó (bỏ qua file chưa sinh + thêm nhóm `site_*`) trước khi kiểm bộ 21.
  Chưa làm — chờ nhìn output thật của hình đầu tiên rồi mới sửa một lần.

## 5. Rủi ro đã thấy: 12 hình công trình hiển thị rất nhỏ

`MainScreen.uss`: `.community-site .community-object-art` = **56×56**, `.hub-item-art` = **40×40**
(`scale-to-fit`, nên nguồn 96×96 vẫn đúng — chỉ là bị thu nhỏ).

Ở 40 px, **5 hình đá** (`bai_da_nho`, `hoc_da_tru_mua`, `tang_da_lon`, `vach_da_vo`,
`dong_da_cuoi`) và **4 hình gỗ** (`nhanh_cay_gay`, `bui_co_voi`, `xac_cay_kho`, `bui_gai_ran`)
sẽ gần như không phân biệt được nếu chỉ khác chi tiết bề mặt. Phải phân biệt bằng
**bóng dáng tổng thể và bố cục**, không bằng hoạ tiết. Đây là lý do nữa để duyệt mẫu trước.

## 6. Thứ tự làm khi plugin mở khoá

1. Sinh **1 hình** (đực đứng). Chạy nguyên quy trình: tải về → cắt → canh đường chân → verify.
2. Nếu bước canh không tự động hoá được, dừng và báo — 21 hình chỉnh tay là bài toán khác.
3. Tự động hoá xong mới sinh mẫu thứ 2 (ổ thức ăn), rồi **bàn giao cho Khoa duyệt phong cách**.
4. Khoa chốt xong mới sinh 19 hình còn lại.
