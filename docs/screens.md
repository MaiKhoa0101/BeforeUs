# Before Us — Màn hình, component và asset

> **File này dùng để làm gì:** phân tích và mô tả từng màn hình của game, các component giao diện dùng chung, và danh sách asset cần làm. Agent làm giao diện đọc file này để biết màn hình gồm những gì, nối với dữ liệu nào, và còn thiếu asset gì.
> **Nguồn:** hai mockup sơ bộ của Khoa (09/11/2026): màn **Cộng đồng** và màn **Mini combat**, đọc đối chiếu với code đang có (`Assets/UI/MainScreen.uxml`, `CommunityScreenModel.cs`, `Assets/Scripts/Combat/HUONG-DAN-NOI-SCENE.md`), `bestiary.md` và `prompt.md`.
> Mọi kích thước, vị trí, tên gọi và lựa chọn kỹ thuật ở đây là **[ĐỀ XUẤT]** cho tới khi Khoa duyệt. Mục §7 liệt kê các chỗ mockup và code đang khác nhau, cần Khoa chốt.

## Mục lục

1. Nguyên tắc chung
2. Màn Cộng đồng
3. Màn Mini combat
4. Màn kết quả trận (hồ sơ, thành tựu)
5. Component dùng chung
6. Danh sách asset
7. Chỗ mockup và code khác nhau, câu hỏi cho Khoa
8. Việc cho agent

---

## 1. Nguyên tắc chung

### 1.1 Khung hình

- Màn hình **ngang cố định** (D8). Độ phân giải tham chiếu **1920 × 1080** (16:9). UI Toolkit đặt `PanelSettings` ở chế độ *Scale With Screen Size*, khớp theo chiều cao.
- Chừa **vùng an toàn** 3% mỗi cạnh cho nút bấm (máy có tai thỏ, viền cong khi lên mobile sau này).
- Không đặt nút quan trọng ở 4 góc sát mép.

### 1.2 Ngôn ngữ hình ảnh rút ra từ mockup

Cả hai mockup cùng một ý: **giao diện làm từ vật liệu thời tiền sử**. Giữ ý này cho mọi màn:

| Vật liệu | Dùng cho | Thấy trong mockup |
| --- | --- | --- |
| **Bia đá** (phiến đá xám có vết nứt) | Thẻ thành viên, bảng thông tin, ô chọn | Thẻ "Tên / Thông tin / Chỉ số" |
| **Xương** | Thanh cuộn, vạch chia, khung | Thanh cuộn khúc xương, khúc xương dựng đứng bên phải |
| **Hóa thạch** | Nút hành động lớn | Nút "End turn" là hóa thạch cá |
| **Gỗ, lá** | Nút menu, biển chỉ dẫn | Biển "Menu" bằng gỗ có dây lá |
| **Lớp đất cắt ngang** | Nền vùng điều khiển | Dải đất có rễ, xương, sỏi dưới màn Cộng đồng |

Quy tắc giữ nhất quán:
- Mỗi loại vật liệu chỉ mang **một vai trò**. Bia đá luôn là "một người / một mục"; hóa thạch luôn là "nút hành động chính".
- Chữ trên vật liệu dùng màu kem sáng có viền tối mỏng để đọc được trên cả nền đá lẫn nền đất.
- Phong cách vẽ asset giao diện đi cùng phong cách vector phẳng của sinh vật (`prompt.md` §2.4): mảng phẳng, đổ bóng cắt cạnh, viền nâu sẫm mảnh.

### 1.3 Chữ

- **Mọi chữ hiện cho người chơi là tiếng Việt**, lấy từ `Assets/Data/strings.vi.json`. Mockup đang có chữ tiếng Anh ("Food", "Hunt", "Main home", "Slot need to clean", "Exit choice", "End turn", "Menu"); đây chỉ là nhãn tạm (bảng đổi tên ở §2.3).
- Phông chữ **phải đủ dấu tiếng Việt**. Phông trong mockup là kiểu sans tròn, đậm. Đề xuất một phông giấy phép SIL OFL có hỗ trợ tiếng Việt, ví dụ *Be Vietnam Pro* hoặc *Baloo 2*. Agent kiểm lại bảng ký tự trước khi dùng, vì phông thiếu dấu sẽ vỡ chữ "ổ", "ượ".
- Cỡ chữ tối thiểu 22 px ở 1080p cho chữ thường, 28 px cho nhãn nút.

### 1.4 Cách dựng: cảnh và giao diện tách lớp

Mỗi màn có hai lớp:
- **Lớp cảnh**: nền vẽ, vật thể bấm được, nhân vật có hoạt ảnh.
- **Lớp giao diện** (UI Toolkit): thanh thẻ thành viên, nút, chỉ số, gợi ý.

**[ĐỀ XUẤT] cho MVP:**
- **Màn Cộng đồng:** dựng toàn bộ bằng UI Toolkit như hiện nay (`CommunityScreenView`). Nền là ảnh, vật thể là nút có hình đặt theo **tỉ lệ phần trăm** trên ảnh nền, thành viên là hình tĩnh có chuyển động nhẹ (nhún, lật hướng). Không đưa hình gắn xương vào màn này trong MVP.
- **Màn Combat:** là **scene Unity thật** (`BattleView`, xem `HUONG-DAN-NOI-SCENE.md`), vì sinh vật gắn xương (D24) là GameObject. Giao diện của combat (thẻ thả quân, đồng hồ) phủ lên bằng UI Toolkit.
- Sau MVP, màn Cộng đồng có thể chuyển phần cảnh sang scene để thành viên có hoạt ảnh xương; model không đổi.

### 1.5 Chất liệu mockup: không dùng thẳng

- Mockup có chữ chìm **"Canva"** và **"Tribalster"**, nên phần lớn hình là tài nguyên mẫu của Canva hoặc ảnh mẫu.
- Hình vượn trong mockup là hình minh họa có sẵn, chưa theo phong cách đã chốt.
- **Không đưa các hình này vào bản phát hành.** Dùng chúng làm bố cục tham khảo, rồi làm lại asset theo §6. Nếu muốn giữ nguyên một tài nguyên Canva nào, phải đọc giấy phép của tài nguyên đó trước.
- Bốn bộ nền `Assets/Art/game_background_1…4` (3840×2160, có tách lớp) đã có trong dự án. Agent cần ghi rõ nguồn và giấy phép của chúng vào `Assets/Art/README` trước khi dùng cho bản phát hành.

---

## 2. Màn Cộng đồng

### 2.1 Mục đích

Nơi người chơi **nhìn thấy bộ tộc của mình và giao việc** cho từng lượt. Thay cho màn danh sách hoạt động cũ (playtest 01, góp ý 6–7, D15).

### 2.2 Bố cục theo mockup

```
┌──────────────────────────────────────────────────────────────── 1920 ┐
│ [Trạng thái: lượt, giai đoạn, năng lượng, tinh thần, khí hậu]   [Menu]│  ← dải trên (thiếu trong mockup)
│                                                                       │
│        (Ổ thức ăn)            (Khu săn)                               │
│   (Chỗ cần dọn)       vượn      [Hang / Main home]     vượn  (Chỗ cần dọn)
│              vượn                                  (mảng đất đã dọn)  │  ← LỚP CẢNH ≈ 62% chiều cao
│ ─────────────────────── mép cỏ / mặt đất ─────────────────────────── │
│ [Bỏ chọn] [thẻ][thẻ][thẻ][thẻ] ...cuộn ngang...  ┃xương┃ [Kết thúc lượt]│  ← DẢI ĐẤT ≈ 38%
│            ═════ thanh cuộn khúc xương ═════                          │
└───────────────────────────────────────────────────────────────── 1080 ┘
```

| Vùng | Vị trí (theo % màn hình) | Nội dung |
| --- | --- | --- |
| Dải trạng thái | trên cùng, cao 8% | Lượt, giai đoạn, năng lượng, tinh thần, banner khí hậu, nút mạng lưới neuron. **Mockup chưa có**, xem §2.5. |
| Nút Menu | góc trên phải, 12% × 9% | Biển gỗ có lá. Mở menu tạm dừng (lưu, Bảo tàng, cài đặt, thoát). |
| Lớp cảnh | 0–62% chiều cao | Nền rừng theo vùng, vật thể bấm được, thành viên đứng ở các chỗ. |
| Dải đất | 62–100% | Thanh thẻ thành viên, nút Bỏ chọn bên trái, nút Kết thúc lượt bên phải, dự báo thức ăn. |

### 2.3 Vật thể bấm được trong cảnh

Nối với `CommunityTargetKind` trong `CommunityScreenModel.cs`:

| Mockup ghi | Tên tiếng Việt đề xuất | Model | Hoạt động | Hình |
| --- | --- | --- | --- | --- |
| Food | **Ổ thức ăn** | `FoodNest` | Kiếm ăn | Đống quả trên phiến đá; nâng cấp theo bậc ổ thức ăn (`sites.json` → `food_nest`) |
| Hunt | **Khu săn** | `Club` | Đi săn | Đống đá với gậy, mũi đá, xương nhọn |
| Main home | **Hang** | `Hub` | Nâng tinh thần, sinh sản | Khúc gỗ, vũng nước, chỗ ngồi quây quần |
| Slot need to clean | **Chỗ cần dọn** | `Site` | Xây dựng (dọn chỗ) | Đống đá phủ rêu; mỗi `site` có `art_key` riêng (`sites.json`) |
| (mảng đất nâu bên phải) | **Chỗ đã dọn** | `Site` đã dọn | — | Mảng đất sạch, có thể có ổ lá để nằm |
| **(không có)** | **Lối mòn** | `Trail` | Khám phá, di cư | **Mockup thiếu.** Đề xuất: lối mòn đi ra mép phải cảnh, có cột đá hoặc dấu chân. |

Mỗi vật thể là một `ObjectButton` (§5.3), gồm hình, nhãn tên, huy hiệu số người đã cử, biểu tượng khóa kèm lý do khi chưa mở.

**Vị trí vật thể** đặt theo phần trăm trên ảnh nền, cất trong dữ liệu theo vùng (đề xuất thêm `anchor_x`, `anchor_y` vào `sites.json` và một khối `community_layout` cho ổ thức ăn, khu săn, hang, lối mòn). Đổi vùng là đổi nền và đổi bộ vị trí, không sửa code.

### 2.4 Thành viên trong cảnh

- Code hiện đặt thành viên vào **chỗ cố định** (`MemberSlotModel`, dáng đứng hoặc ngồi theo chỗ, đếm từ 1). Mockup vẽ vượn **đi lại tự do**.
- **[ĐỀ XUẤT]:** giữ chỗ cố định làm **điểm neo**. Thành viên đứng quanh điểm neo, thỉnh thoảng đi vài bước rồi quay về (chuyển động tạm bằng tween, chưa cần xương). Như vậy cảnh trông sống động mà luật sức chứa vẫn đúng.
- Trên đầu mỗi người có tên (góp ý của Khoa ở playtest màn cộng đồng) và một biểu tượng nhỏ cho việc đang được giao.
- Người ở chỗ vượt sức chứa (`OverCapacity`) đứng mờ, có biểu tượng cảnh báo.
- Bấm vào thành viên trong cảnh cũng chọn thẻ của người đó ở dải dưới.

### 2.5 Thông tin bắt buộc mà mockup chưa có

Các phần sau đang có trong `MainScreen.uxml` và là luật của game, nên phải có chỗ trên màn:

| Thông tin | Vì sao bắt buộc | Đề xuất chỗ đặt |
| --- | --- | --- |
| **Năng lượng** (chấm) | Mỗi hoạt động trừ năng lượng | Dải trạng thái, trái |
| **Tinh thần** (thanh) | Đủ thì sinh thêm người | Dải trạng thái, cạnh năng lượng |
| **Lượt, giai đoạn** | Nhịp ván, độ khó tăng theo lượt | Dải trạng thái, giữa |
| **Banner khí hậu** (Ẩm/Khô, báo trước 2 lượt) | `design.md` §10 | Dải mỏng chạy ngang dưới dải trạng thái khi sắp đổi pha |
| **Dự báo "cuối lượt cần X, đang có Y"** | Gợi ý lượt 1 (§13) | Dải đất, ngay trên nút Kết thúc lượt |
| **Nút mạng lưới neuron** | Tiến hóa | Cạnh nút Menu: biểu tượng não bằng đá |
| **Gợi ý hướng dẫn** | §13, tắt được | Bong bóng neo vào vật thể hoặc thẻ liên quan |

Khi vào pha Khô: nền ngả vàng úa, đom đóm ít đi; dự báo đổi màu cảnh báo nếu thiếu thức ăn.

### 2.6 Dải đất: thanh thẻ thành viên

| Phần | Mô tả |
| --- | --- |
| **Nút Bỏ chọn** (trái, phiến đá có hai xương bắt chéo) | Thoát chế độ giao việc: đóng bảng giao việc đang mở, bỏ chọn thẻ. Mockup ghi "Exit choice". |
| **Thẻ thành viên** (bia đá) | Chứa **mọi** thành viên, kể cả người rảnh (`CommunityScreenModel` đã làm vậy). Nội dung chi tiết ở §5.1. |
| **Thanh cuộn khúc xương** | Cuộn ngang khi nhiều người hơn số thẻ vừa màn (4–5 thẻ ở 1080p). Kéo hoặc cuộn chuột. |
| **Khúc xương dựng** (bên phải) | Vạch ngăn giữa thẻ và nút Kết thúc lượt. Chỉ để trang trí, không bấm được. |
| **Nút Kết thúc lượt** (hóa thạch cá) | Nút hành động chính. Khi còn năng lượng hoặc còn người rảnh, hỏi lại một lần ("Còn 2 người chưa có việc. Vẫn kết thúc?"). |

### 2.7 Luồng thao tác

1. Bấm một **vật thể** → vật thể sáng viền, **bảng giao việc** mở (`c-task`): tên hoạt động, chi phí năng lượng, số người cần hoặc khuyên dùng, tỉ lệ thắng nếu là trận (`PreviewCombat`).
2. Bấm **thẻ thành viên** để thêm hoặc bớt người. Thẻ đổi sang trạng thái *đã giao*, có biểu tượng hoạt động. Người trong cảnh đi về phía vật thể.
3. **Bỏ chọn** hoặc bấm ra ngoài → đóng bảng.
4. **Khám phá** mở thêm bảng chọn người di cư (`c-migrate`, design-v7 §F.2).
5. **Kết thúc lượt** → kết quả lượt; Săn và Khám phá mở màn Combat.

Bàn phím và chuột: `Esc` = Bỏ chọn, `Enter` = Kết thúc lượt, cuộn chuột trên dải thẻ = cuộn ngang.

### 2.8 Trạng thái của màn

| Trạng thái | Hiện gì |
| --- | --- |
| Bình thường | Như mockup |
| Đang giao việc | Vật thể được chọn sáng viền; các vật thể khác mờ 40%; bảng giao việc mở |
| Vật thể bị khóa | Hình xám, ổ khóa đá, chạm vào thì hiện lý do (lấy từ Core, không tự suy) |
| Hết năng lượng | Mọi vật thể tốn năng lượng bị mờ; dự báo nháy nhẹ |
| Pha Khô | Bảng màu nền ngả vàng; banner khí hậu |
| Sự kiện đầu lượt | Hộp sự kiện phủ giữa màn, cảnh mờ phía sau |

---

## 3. Màn Mini combat

### 3.1 Mục đích

Trận thời gian thực 20–40 giây. Người chơi **chạm vào sân để thả từng thành viên đã được cử**; người và thú tự đánh theo chỉ số (`design.md` §9). Có hai kiểu trận: **Dụ và hạ** (thú tìm đường thoát) và **Phòng thủ** (thú tiến vào hang).

### 3.2 Bố cục theo mockup

```
┌───────────────────────────────────────────────────────────── 1920 ┐
│ [thẻ][thẻ][thẻ][thẻ][thẻ][thẻ][thẻ]   ← người được cử, chạm để chọn rồi thả │ ≈ 30%
│ ═════ thanh cuộn khúc xương ═════                                   │
│ [Kiểu trận + mục tiêu]        [ĐỒNG HỒ 0:30]              [Tạm dừng] │  ← thiếu trong mockup
│┌──────────┐┌──────────────────────────────────┐┌──────────┐        │
││ Vùng thả ││   vượn  -4.55 [▬▬ thanh máu ▬▬] -5  vượn      ││ Vùng thả │ │ ≈ 70%
││          ││            (báo)                            ││          │ │
│└──────────┘└──────────────────────────────────┘└──────────┘        │
└──────────────────────────────────────────────────────────────── 1080 ┘
```

| Vùng | Vị trí | Nội dung |
| --- | --- | --- |
| Dải thẻ thả quân | trên cùng, 0–28% | Thẻ của những người được cử vào trận (bản thu gọn, §5.1). Nền đất có xương, hóa thạch. |
| Dải thông tin trận | 28–34% | **Mockup chưa có.** Kiểu trận và mục tiêu, đồng hồ, nút tạm dừng. |
| Sân | 34–100% | Sân cỏ theo vùng; vùng thả; thú; thành viên; số sát thương; thanh máu. |

**Hướng nhìn:** mockup là sân nhìn hơi từ trên xuống, nhân vật nhìn ngang. Cách này khớp mô phỏng: trục X của sân (rộng 60) là chiều ngang màn hình, trục Y (cao 12) là chiều sâu, hiện thành chiều dọc của dải sân. Nhân vật ở Y lớn hơn (xa hơn) vẽ phía trên và nhỏ hơn một chút (khoảng 90%), để có cảm giác chiều sâu.

### 3.3 Vùng thả: mockup khác code

- **Mockup:** **hai** vùng thả ở hai bên, thú ở giữa.
- **Code hiện tại** (`beasts.json` → `battles`): **một** dải thả liền, `deploy_min_x`–`deploy_max_x`. Dụ và hạ là 4–52 trên sân rộng 60, Phòng thủ là 6–50. Đích của thú ở một mép: thoát ra phải (Dụ và hạ, `goal_x` = 60), hoặc cửa hang bên trái (Phòng thủ, `goal_x` = 0).
- Mockup cũng **chưa vẽ đích của thú**. Người chơi phải nhìn thấy thú đang chạy đi đâu.

**[ĐỀ XUẤT]**, chờ Khoa chọn (§7):
- **Cách A — giữ luật hiện tại**, chỉ vẽ cho rõ:
  - Vùng thả là một dải mờ đúng theo `DeployMinX`–`DeployMaxX`.
  - Dụ và hạ: **lối thoát** ở mép phải (bụi rậm, khe đá, có mũi tên).
  - Phòng thủ: **cửa hang** ở mép trái, có bộ tộc đứng sau.
  - Không cần đổi code, không cần hiệu chỉnh lại.
- **Cách B — làm đúng mockup** (hai vùng thả hai bên, thú ở giữa):
  - Phải sửa `BattleField` để hỗ trợ nhiều vùng thả, thêm dữ liệu vùng thả vào `beasts.json`.
  - **Phải chạy lại hiệu chỉnh B3**, vì vị trí thả đổi thì tỉ lệ thắng đổi.
  - Lối thoát hay cửa hang vẫn phải có ở một mép, nên chỉ một trong hai vùng thả gần đích.

### 3.4 Thành phần trên sân

| Thành phần | Mô tả | Nguồn dữ liệu |
| --- | --- | --- |
| **Thú** | Hình gắn xương (`prompt.md`), lật hướng theo kiểu trận | `Combatant.Position`, `Setup.Kind` |
| **Thanh máu của thú** | Thanh đỏ trên đầu thú, như mockup. Thêm tên loài nhỏ phía trên lần đầu gặp | `Hp / MaxHp` |
| **Biểu tượng trạng thái** | Mắt (Rình), sao xoay (Choáng), mũi tên quay lui (Bỏ chạy), lửa đỏ (Cuồng nộ) | `bestiary.md` §4 |
| **Dấu mục tiêu** | Vòng đỏ dưới chân người đang bị thú nhắm | `TargetId` |
| **Thành viên** | Hình vượn theo giai đoạn; thanh máu nhỏ chỉ hiện khi đã mất máu | `Hp / MaxHp` |
| **Người ngã** | Ngã xuống, mờ dần, để lại dấu nhỏ trên sân đến hết trận | `DiedAtSeconds` |
| **Số sát thương** | Bay lên rồi mờ trong 0,8 giây. **Làm tròn tới số nguyên**; mockup đang ghi "-4.55", quá rối khi trận nhanh. Màu trắng vàng cho đòn trúng thú, màu đỏ cho đòn trúng người | sự kiện đánh trúng |
| **Vùng thả** | Dải mờ, sáng lên khi đang chọn một thẻ; chạm ngoài vùng thì rung nhẹ và báo "Không thả được ở đây" | `DeployMinX`, `DeployMaxX` |
| **Đích của thú** | Lối thoát (Dụ và hạ) hoặc cửa hang (Phòng thủ) | `GoalX` |
| **Đồng hồ** | Đếm ngược, chuyển đỏ ở 5 giây cuối | `TimeRemainingSeconds` |

### 3.5 Luồng thao tác

1. Vào trận: tên kiểu trận và mục tiêu hiện giữa màn 1,5 giây ("Dụ và hạ — hạ con báo trước khi nó chạy thoát"). Lần đầu gặp một loài thì hiện thêm kiểu tấn công của nó (§13).
2. Chạm một **thẻ** để chọn người → vùng thả sáng lên → chạm vào sân để thả. Người đã thả thì thẻ chuyển trạng thái *trên sân*, kèm thanh máu nhỏ.
3. Chạm thẳng vào sân khi chưa chọn thẻ → thả người kế tiếp theo thứ tự thẻ. Cách này nhanh cho người chơi quen tay. Hành vi này là [ĐỀ XUẤT], Khoa chỉnh ở B4.
4. Hết người, hết giờ, thú gục, thú thoát hoặc thú tới hang → màn kết quả (§4).

---

## 4. Màn kết quả trận (hồ sơ, thành tựu)

Màn mới theo `bestiary.md` §5, phủ lên màn combat khi trận kết thúc.

| Phần | Nội dung |
| --- | --- |
| Tiêu đề | THẮNG / THUA, kèm lý do bằng chữ (`BattleEndReason` → `strings.vi.json`) |
| Tổn thất | Chân dung những người chết, kể cả người ở nhà bị lấy thêm theo luật (`DesignTollCasualtyPolicy`) |
| Phần thưởng | Thức ăn, tinh thần, neuron, chiến tích |
| **Thẻ hồ sơ** (tối đa 1) | Ô vuông cắt từ tranh hồ sơ đầy đủ (`prompt.md` §2.7), tên loài, mốc thời gian, 2–3 câu; nút "Đọc thêm" mở Bảo tàng |
| **Thẻ thành tựu** (tối đa 1) | Tên, một câu lịch sử, lý do nhận |
| Nút | "Tiếp tục" quay về màn Cộng đồng |

Chạm bất kỳ đâu để bỏ qua thẻ. Toàn màn không được dài hơn vài giây nếu người chơi bấm tiếp.

---

## 5. Component dùng chung

### 5.1 `MemberCard` — thẻ thành viên (bia đá)

| Phần | Nội dung | Nguồn |
| --- | --- | --- |
| Chân dung | Mặt người, ghép từ đặc điểm (F1) | `Member.Traits` |
| Tên | Một dòng, tối đa 12 ký tự rồi cắt "…" | `Member.Name` |
| Thông tin | Việc đang làm, hoặc "Rảnh" | `TurnPlan` qua `TurnScreenModel` |
| Chỉ số | Ba biểu tượng nhỏ kèm số: nhanh nhẹn, sức mạnh, máu | `Member.Speed`, `Strength`, `Hp` |

**Hai biến thể:**
- **Đầy đủ** (màn Cộng đồng): 200 × 260 px ở 1080p, có đủ ba dòng.
- **Thu gọn** (màn Combat): 150 × 190 px, chỉ có chân dung, tên và thanh máu.

**Trạng thái:**

| Trạng thái | Hình |
| --- | --- |
| Rảnh | Bia đá xám bình thường |
| Đang chọn | Viền sáng màu hổ phách, nhô lên 8 px |
| Đã giao việc | Góc trên có biểu tượng hoạt động |
| Trên sân (combat) | Mờ 50%, thanh máu nhỏ hiện |
| Đã chết | Đá nứt đôi, chân dung xám, không bấm được |
| Không cử được | Mờ, bấm vào thì hiện lý do |

**Kỹ thuật:** nền bia đá là ảnh **9-slice** (`-unity-slice-*`), để thẻ co giãn mà vết nứt ở góc không bị kéo méo.

### 5.2 Nút vật liệu

| Component | Hình | Dùng ở |
| --- | --- | --- |
| `StoneButton` | Phiến đá, có biểu tượng | Bỏ chọn, Tạm dừng |
| `FossilButton` | Hóa thạch trên đá | Kết thúc lượt, Tiếp tục |
| `WoodSign` | Biển gỗ có lá | Menu |

Mỗi nút có 4 trạng thái: thường, rê chuột (sáng 10%), nhấn (lún 4 px, tối 10%), khóa (xám, mờ 50%).

### 5.3 `ObjectButton` — vật thể trong cảnh

- Hình vật thể kèm nhãn tên phía trên (chữ kem, viền tối, như "Food" và "Hunt" trong mockup).
- Huy hiệu tròn góc phải: số người đã cử.
- Ổ khóa đá khi chưa mở.
- Khi chọn: viền sáng chạy quanh hình. Viền này vẽ bằng USS, không cần ảnh riêng.

### 5.4 Các component khác

| Component | Mô tả |
| --- | --- |
| `BoneScrollbar` | Thanh cuộn ngang: rãnh là khúc xương dài, núm kéo là khớp xương |
| `ResourceBar` | Năng lượng (chấm đá), tinh thần (thanh lửa hoặc thanh lá) |
| `ClimateBanner` | Dải ngang có biểu tượng mưa hoặc nắng hạn, chữ báo trước |
| `ForecastBox` | "Cần X / có Y", đổi màu khi thiếu |
| `TaskPanel` | Bảng giao việc, đã có (`c-task`); đổi da sang bia đá lớn |
| `HintBubble` | Bong bóng gợi ý neo vào phần tử, có nút tắt |
| `HPBar` | Thanh máu: đỏ cho thú, xanh lá cho người |
| `DamageNumber` | Số bay lên, làm tròn, hai màu |
| `StateIcon` | Biểu tượng trạng thái thú (§3.4) |
| `TargetMarker` | Vòng đỏ dưới chân |
| `DeployZone` | Dải mờ, sáng khi chọn thẻ |
| `BattleTimer` | Đồng hồ đếm ngược |
| `EventDialog` | Hộp sự kiện: tranh, chữ, 1–2 lựa chọn |
| `ResultPanel`, `CodexCard`, `AchievementCard` | Màn kết quả (§4) |

---

## 6. Danh sách asset

Cột "Ai làm": **K** = Khoa vẽ; **AI** = sinh bằng prompt (`prompt.md`) rồi Khoa duyệt; **A** = agent làm bằng code hoặc USS.
Ưu tiên: **1** = cần cho bản MVP chơi được; **2** = nên có trước 28/11; **3** = sau MVP.

### 6.1 Màn Cộng đồng

| Asset | Kích thước | Biến thể | Ai làm | Ưu tiên |
| --- | --- | --- | --- | --- |
| Nền cảnh cộng đồng | 3840 × 2160, tách lớp (trời, rừng xa, rừng gần, mặt đất) | vùng 0 rừng, vùng 1 savan; mỗi vùng thêm bản pha Khô | K/AI | 1 (vùng 0), 2 (còn lại) |
| Ổ thức ăn | 256 × 256 | theo bậc ổ (0–3) | K/AI | 1 |
| Khu săn | 256 × 256 | 1 | K/AI | 1 |
| Hang (trung tâm) | 384 × 256 | 1 mỗi vùng | K/AI | 1 |
| Lối mòn | 256 × 256 | 1 mỗi vùng | K/AI | 1 |
| Chỗ cần dọn / đã dọn | 256 × 256 | 12 `art_key` trong `sites.json`, mỗi cái 2 trạng thái | K/AI | 1 (vùng 0), 2 |
| Thành viên trong cảnh | 256 × 340 | giai đoạn 1–2, nam/nữ, đứng/ngồi | K | 1 |
| Đom đóm, sương | hạt nhỏ | — | A (particle hoặc USS) | 3 |

Hình đang dùng tạm ở `Assets/Art/Placeholder` có khung 96 × 128 và 96 × 96. Asset thật vẽ lớn hơn (256 trở lên) rồi để UI thu nhỏ: màn mới hiện vật thể lớn hơn màn cũ.

### 6.2 Màn Combat

| Asset | Kích thước | Biến thể | Ai làm | Ưu tiên |
| --- | --- | --- | --- | --- |
| Nền sân | 3840 × 2160, tách lớp | theo vùng; xem lại 4 bộ `game_background_*` sẵn có | K/AI | 1 |
| Lối thoát (Dụ và hạ) | 256 × 512 | theo vùng | K/AI | 1 |
| Cửa hang (Phòng thủ) | 384 × 512 | theo vùng | K/AI | 1 |
| Thú gắn xương | theo `prompt.md` | 5 thú + 3 con mồi | AI + K cắt mảnh | 1 |
| Thành viên gắn xương | khung người, chưa có prompt | giai đoạn 1–2 | K/AI | 1 |
| Biểu tượng trạng thái | 64 × 64 | mắt, sao, quay lui, lửa | AI/K | 1 |
| Vòng mục tiêu, dải vùng thả | — | — | A (USS hoặc shader) | 1 |
| Hiệu ứng trúng đòn, bụi | sprite sheet nhỏ | — | AI/A | 2 |

### 6.3 Giao diện chung

| Asset | Kích thước | Ghi chú | Ai làm | Ưu tiên |
| --- | --- | --- | --- | --- |
| Bia đá thẻ | 256 × 320, 9-slice | 3 trạng thái màu viền làm bằng USS | K/AI | 1 |
| Phiến đá nút | 256 × 192, 9-slice | | K/AI | 1 |
| Hóa thạch cá (Kết thúc lượt) | 320 × 192 | | K/AI | 1 |
| Biển gỗ Menu | 256 × 128 | | K/AI | 2 |
| Dải đất cắt ngang | 3840 × 820, lặp ngang được | có rễ, xương, sỏi | K/AI | 1 |
| Khúc xương thanh cuộn, núm | 1024 × 48, 64 × 64 | | K/AI | 2 |
| Bộ biểu tượng | 64 × 64 | năng lượng, tinh thần, nhanh nhẹn, sức mạnh, máu, Kiếm ăn, Săn, Xây, Khám phá, Nâng tinh thần, khóa, neuron | K/AI | 1 |
| Phông chữ | — | đủ dấu tiếng Việt, giấy phép OFL | A | 1 |
| Mặt nạ viền thẻ hồ sơ | 512 × 512 | `prompt.md` §2.7 | K/AI | 2 |

Prompt cho các asset giao diện chưa viết. Khi viết thì thêm thành một hạng mục mới trong `prompt.md` §5, theo cùng phong cách vector phẳng.

---

## 7. Chỗ mockup và code khác nhau, câu hỏi cho Khoa

| # | Chỗ khác | Đề xuất |
| --- | --- | --- |
| 1 | Combat: mockup có **hai vùng thả** hai bên, code có **một dải thả** và đích ở một mép (§3.3) | **Cách A** (giữ luật, vẽ rõ vùng thả và đích) cho MVP; cách B để sau khi có thời gian hiệu chỉnh lại |
| 2 | Mockup combat chưa có **đích của thú**, **đồng hồ**, **kiểu trận và mục tiêu** | Thêm như §3.2 và §3.4 |
| 3 | Cộng đồng: mockup chưa có **lối mòn Khám phá**, **năng lượng, tinh thần, dự báo, khí hậu, nút neuron** | Thêm như §2.3 và §2.5 |
| 4 | Thành viên **đi tự do** (mockup) hay **đứng theo chỗ** (code) | Giữ chỗ làm điểm neo, cho đi lại quanh đó (§2.4) |
| 5 | Số sát thương có 2 chữ số lẻ ("-4.55") | Làm tròn tới số nguyên |
| 6 | Nhãn tiếng Anh trên mockup | Đổi sang tiếng Việt qua `strings.vi.json` (§2.3) |
| 7 | Hình mockup có chữ chìm Canva, Tribalster | Chỉ dùng làm bố cục, vẽ lại asset (§1.5) |
| 8 | Nền dải thẻ combat có **sọ khủng long ba sừng** hóa thạch | Đổi sang xương thú có vú chung chung, tránh nhầm thời đại và tránh dùng hình có sẵn |
| 9 | Màn Cộng đồng dùng UI Toolkit hay scene | UI Toolkit cho MVP (§1.4) |

---

## 8. Việc cho agent

| Mã | Ai | Việc | Cần trước | Nghiệm thu |
| --- | --- | --- | --- | --- |
| UX0 | K | Duyệt §7, chọn cách A/B cho vùng thả | — | Khoa trả lời §7 |
| UX1 | A | Đổi da màn Cộng đồng theo §2: lớp cảnh có nền ảnh, vật thể đặt theo % từ dữ liệu, dải đất, thanh thẻ bia đá, nút Bỏ chọn và Kết thúc lượt; giữ nguyên `CommunityScreenModel` | UX0 | Chơi trọn một lượt chỉ bằng chuột; mọi thông tin ở §2.5 có trên màn |
| UX2 | A | `MemberCard` hai biến thể, 6 trạng thái, 9-slice | — | Ảnh chụp đủ trạng thái |
| UX3 | A | Thêm `anchor_x`, `anchor_y` và `community_layout` vào dữ liệu; loader kiểm chặt như các file khác | UX0 | Đổi vùng là đổi vị trí vật thể, không sửa code |
| UX4 | A | Giao diện combat theo §3: dải thẻ thu gọn, dải thông tin trận, vùng thả, đích, đồng hồ, số sát thương làm tròn, biểu tượng trạng thái | UX0, combat B1 | Một trận Dụ và hạ, một trận Phòng thủ chạy trọn, nhìn rõ mục tiêu |
| UX5 | A | Màn kết quả §4 (thẻ hồ sơ cắt từ tranh đầy đủ) | BX5 trong `bestiary.md` | Thấy thẻ hồ sơ lần đầu gặp một loài |
| UX6 | A | Chọn và nhập phông đủ dấu tiếng Việt; kiểm mọi chuỗi trong `strings.vi.json` hiển thị không vỡ | — | Ảnh chụp chuỗi có "ổ", "ượ", "ữ" |
| UX7 | K | Vẽ hoặc duyệt asset ưu tiên 1 ở §6 | — | — |

Mốc 15/11 của D15 (màn Cộng đồng chơi được) vẫn giữ nguyên. UX1–UX3 nằm trong mốc đó; UX4–UX6 theo sau.
