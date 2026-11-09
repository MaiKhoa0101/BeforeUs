# Playtest 01 — Khoa tự chơi, 2026-11-08

> Phản hồi playtest **đầu tiên** của dự án, từ chính người thiết kế sau khi chơi bản prototype.
> Phần trích dẫn là **nguyên văn của Khoa**. Phần phân loại bên dưới mỗi mục là của Supervisor.
> File này ghi lại, **không** phải là quyết định — quyết định nằm ở `docs/decisions.md`.

| # | Loại | Chủ sở hữu | Trạng thái |
| --- | --- | --- | --- |
| 1 | Luật | Lead Core | **Đã làm** (D14a) |
| 2 | Giao diện | Lead UI | Chờ — không đổi luật |
| 3 | Giao diện + API | Lead UI (Core đã phơi dữ liệu) | **Core đã làm** phần của mình |
| 4 | Hệ thống (xác nhận hàng rào) | I1 | Hoãn có chủ đích |
| 5 | Hệ thống mới | Chưa giao | **Khoa đang quyết phạm vi** |
| 6 | Hệ thống mới | Chưa giao | **Khoa đang quyết phạm vi** |
| 7 | Hệ thống mới | Chưa giao | **Khoa đang quyết phạm vi** |
| 8 | Luật | Lead Core | **Đã làm** (D14b, D14c) |

---

### 1. Mất nhuệ khí khi có thành viên chết

> Thêm mất nhuệ khí khi có thành viên chết

**Loại:** luật. **Chủ sở hữu:** Lead Core. **Trạng thái: đã làm.**

Thêm khóa `morale.on_member_death` vào `balance.json`, áp cho **mọi** cái chết: đói, thua combat, quái thú ăn. Giá trị **−2 là [ĐỀ XUẤT] của Supervisor**, không phải của Khoa — chọn cùng độ lớn với `combat_results.morale_on_loss` đang có để không đặt ra một thang đo mới. Chi tiết: `decisions.md` D14.

---

### 2. Nhuệ khí là một thanh, đầy thì sinh, reset và giữ phần dư

> Nhuệ khí sẽ là 1 thanh giá trị, khi đạt ngưỡng sẽ sinh ra thành viên mới, rồi thanh đó sẽ reset về 0 và + thêm số nhuệ khí còn dư ở lượt trước.

**Loại:** giao diện. **Chủ sở hữu:** Lead UI. **Không đổi luật.**

**Nhận định của Supervisor: đây không phải đổi luật.** `design.md` §6 đã là *"trừ 4 tinh thần"*, tức phần dư **đã** được giữ sang lượt sau. "Reset về 0 rồi cộng phần dư" là **cùng một phép tính**, chỉ khác cách kể. Core không cần sửa gì.

Việc còn lại thuần hiển thị: vẽ tinh thần thành thanh tiến trình `morale / birth.morale_cost` thay vì một con số trần. Lead UI đọc `state.Morale` và `config.Data.Birth.MoraleCost`.

---

### 3. Thêm thông tin cho cuộc đi săn

> Nên thêm thông tin cho cuộc đi săn, ví dụ cuộc đi săn lượt này sẽ đem về bao nhiêu thức ăn, ước lượng sức mạnh của con mồi/đối thủ.

**Loại:** giao diện + API. **Chủ sở hữu:** Lead UI; Core phải phơi dữ liệu trước. **Trạng thái: Core đã làm phần của mình.**

Core thêm truy vấn **không tác dụng phụ** `TurnSystem.PreviewCombat(activity)` → `FoodIfWon` và `WinChance`. Xem `docs/core-api.md` §4.

Hai ràng buộc cho Lead UI:
- **Đừng tự tính lại** `k × hunt_food_per_member[stage]` hay công thức §5.2. Gọi `PreviewCombat`.
- Core trả **dữ liệu thô** (`double WinChance`, `int FoodIfWon`), **không** trả thang đo kiểu "dễ / vừa / khó". Cách diễn đạt là quyền của UI và của Khoa.

---

### 4. Tạm bỏ qua các bảng phải hiện ra

> Hiện tại là prototype nên tôi tạm bỏ qua vấn đề về các bảng, ví dụ sự kiện xảy ra ở lượt máy cần chọn hoặc mini combat cần phải hiện ra.

**Loại:** hệ thống. **Chủ sở hữu:** I1. **Hoãn có chủ đích.**

**Nhận định của Supervisor: góp ý này xác nhận hàng rào NỢ-I1 đã dựng đúng chỗ.** `IEventDecisions` và `ICombatResolver` hiện **đồng bộ**, chạy bên trong `EndTurn()`, nên không chờ được hộp thoại. `HANDOFF.md` xếp việc này vào I1 (tuần 2). A2 hiện dựng hộp sự kiện ở dạng **chỉ hiển thị** — kể lại chuyện vừa xảy ra, không hỏi người chơi.

Khoa tự nhận ra đúng chỗ hệ thống còn thiếu, và nó trùng khít với chỗ đã được khoanh. Không ai phải làm gì thêm bây giờ.

---

### 5. Kiếm ăn có khám phá thực vật

> Cơ chế kiếm ăn cũng sẽ có thêm yêu cầu, khi chọn kiếm ăn nó sẽ hiển thị các loại thực vật đã phát hiện, tỉ lệ khám phá ra hoa quả khác cũng sẽ ngẫu nhiên khi lựa chọn kiếm ăn, mỗi hoa quả nó sẽ có tỉ lệ tìm kiếm thấy khác nhau, dinh dưỡng cũng khác nhau, nhưng sẽ có khung đảm bảo nếu cử người có chỉ số tốc độ là bao nhiêu đó sẽ tìm thấy ít nhất 1. Khi mùa khô xảy ra thì tỉ lệ tìm thấy hoa quả giảm xuống.

**Loại:** hệ thống mới. **Chủ sở hữu:** chưa giao. **Khoa đang quyết phạm vi — không ai động vào.**

**Còn thiếu để làm được:** danh mục thực vật (tên, tỉ lệ tìm thấy, dinh dưỡng) — chưa tồn tại, sẽ cần một file dữ liệu mới; ngưỡng `speed` của "khung đảm bảo tìm thấy ít nhất 1"; mức giảm tỉ lệ ở pha Khô.

**Va chạm:** thay hẳn `design.md` §5.1 (`gather_yield = 3 + region …`) và `balance.json` khối `gather`. Kéo theo D2 (thức ăn Kiếm ăn tính theo từng người) phải diễn đạt lại. Mọi số cân bằng của C8 sẽ phải đo lại từ đầu.

---

### 6. Màn hình chính là khu vực cộng đồng, công trình theo slot

> Bản chất người chơi sẽ nhìn thấy khu vực cộng đồng như là màn hình chính, trên đó hiển thị các thành viên đang ngồi hoặc hoạt động sinh hoạt ngẫu nhiên (set mặc định theo slot trên màn hình). Ở màn hình chính này sẽ có các slot nhỏ như ổ đựng thức ăn, hoặc các bãi đá/nhánh cây chưa được dọn — để dọn thì cần số người và số sức mạnh cụ thể; việc nâng cấp công trình tức là chọn các công trình trên và thực hiện nó. Ở các giai đoạn tiến hóa sau thì thêm các slot công trình khác như nghĩa địa hay bếp lửa.

**Loại:** hệ thống mới. **Chủ sở hữu:** chưa giao. **Khoa đang quyết phạm vi — không ai động vào.**

**Còn thiếu để làm được:** danh mục slot công trình (loại, yêu cầu số người, yêu cầu tổng `strength`, mở ở giai đoạn nào) — chưa tồn tại.

**Va chạm:** thay hẳn luật Xây dựng ở `design.md` §5 (hiện là "mỗi người +1 điểm xây, đủ 4 × (build_level+1) thì lên cấp") bằng mô hình chọn-công-trình-cụ-thể. Ảnh hưởng `lair.capacity_per_build_level` và toàn bộ A1. Đây cũng là chỗ **chiến lợi phẩm của D13 có thể về**: hai nút cổng `s2_stone_tools` / `s2_scavenge_meat` đang rỗng, có thể gắn vào hệ slot này.

---

### 7. Thao tác bằng cách bấm vào vật thể trên màn hình

> Màn hình chính thấy một cụm cộng đồng đang ngồi đứng hoạt động khác nhau. Kiếm ăn thì bấm vào ổ thức ăn; đi săn thì bấm vào cây gậy to (đổi hình theo giai đoạn tiến hóa: lưỡi dao đá, cây gỗ vuốt nhọn…); bấm vào trung tâm ra lựa chọn nâng cấp công trình / nâng cấp tiến hóa / nâng nhuệ khí.

**Loại:** hệ thống mới (giao diện). **Chủ sở hữu:** chưa giao. **Khoa đang quyết phạm vi — không ai động vào.**

**Còn thiếu để làm được:** art cho từng vật thể tương tác theo từng giai đoạn tiến hóa.

**Va chạm:** thay hẳn A1 (màn chính dạng danh sách + nút). **Không đổi luật** — `TurnPlan` đã cho phép phân công tự do bao nhiêu lần tùy ý (D2), nên mô hình bấm-vào-vật-thể dựng được trên đúng API hiện có. Đây là thay đổi đắt nhất về art và về A1, nhưng **rẻ nhất về Core**: Core không phải sửa gì.

---

### 8. Thua khi cộng đồng chỉ còn 1 người

> Đổi lại điều kiện: khi cộng đồng chỉ còn 1 người thì kết thúc game.

**Loại:** luật. **Chủ sở hữu:** Lead Core. **Trạng thái: đã làm.**

**⚠ Nhận định của Supervisor: góp ý này va chạm D11.** D11 nói thua trận Phòng thủ bắt buộc thì quái thú luôn **chừa lại ít nhất 1 người** — để một trận *không né được* không bao giờ thành cú thua tức thì. Nếu ngưỡng thua là "còn 1 người", thì chừa lại đúng 1 người **chính là** thua, và D11 trở thành vô nghĩa.

Đã sửa cùng lúc: D11 nay chừa lại ít nhất **2** người, suy ra từ ngưỡng thua chứ không phải viết cứng. Chi tiết: `decisions.md` D11 (đính chính) và D14.

---

## Việc còn treo sau playtest này

| Việc | Ai |
| --- | --- |
| Quyết phạm vi góp ý 5, 6, 7 | **Khoa** |
| Thanh tinh thần (góp ý 2) | Lead UI |
| Hiện thông tin đi săn từ `PreviewCombat` (góp ý 3) | Lead UI |
| Hộp thoại sự kiện và mini combat hiện ra (góp ý 4) | I1 |
| Duyệt giá trị [ĐỀ XUẤT] `morale.on_member_death = −2` | **Khoa** |
| Hai nút cổng `s2_stone_tools` / `s2_scavenge_meat` đang rỗng (nợ D13) | **Khoa** — có thể gắn vào góp ý 6 |
| `hunt_food_per_member[0] = 0` (săn giai đoạn 1 không ra thức ăn) | **Khoa** |
