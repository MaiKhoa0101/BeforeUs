# Bàn giao — nhánh Lõi luật chơi (Core)

**Ngày:** 2026-11-09 · **Phạm vi:** S2, S3, C1–C8 · **Trạng thái:** hoàn thành, dừng có chủ đích trước D5.

Đọc file này là đủ để tiếp việc. Không cần đọc lại lịch sử hội thoại.

---

## 1. Đọc gì, theo thứ tự nào

| Thứ tự | File | Vì sao |
| --- | --- | --- |
| 1 | `docs/design.md` | Luật chơi, **v7**. Nguồn sự thật. ⚠ Chạy `python tools/doc/docx_read.py --check docs/design.docx` **trước** khi tin nó. |
| 2 | `docs/decisions.md` | 26 quyết định D1–D26. Bảng tra ở đầu file cho biết cái nào còn hiệu lực, cái nào bị thay. |
| 3 | `docs/core-api.md` | Hợp đồng API cho ba nhánh kia. 15 mục. |
| 4 | `docs/balance-log.md` | Mọi con số cân bằng đã đổi, kèm phép đo biện minh. |
| 5 | `docs/design-v7-draft.md` | **Đã hấp thụ** — không phải luật hiện hành. Giữ vì nó chứa *lý do*: nguyên văn trả lời của Khoa, các cách đọc đã bị bỏ, câu còn mở. |

`docs/bestiary.md`, `docs/prompt.md`, `HANDOFF.md`, `AGENTS.md`, `WORKSPACE_PROTOCOL.md`, `tools/sim/` **không thuộc nhánh này**.

---

## 2. Trạng thái

**C1–C8 xong. Cổng 2 đạt cả sáu chỉ tiêu.**

| Chỉ tiêu | Dải | Đo được | |
| --- | --- | --- | --- |
| balanced thắng | 30–45% | **33,2% ± 1,1** | ĐẠT |
| balanced trung vị | 42–52 lượt | **51** (p10 48, p90 59) | ĐẠT |
| safe thắng | ≤ 2% | **0,0%** | ĐẠT |
| reckless thắng | ≤ 20% | **0,4% ± 0,1** | ĐẠT |
| careless thắng | ≤ 20% | **7,1% ± 0,6** | ĐẠT |
| balanced thua trước lượt 10 | ≤ 10% | **5,6%** (trước lượt 5: 2,1%) | ĐẠT |

2.000 ván mỗi bot, chế độ MVP, `applyNodeEffects = false`, SE ≈ 1,1 điểm %.

**Không tuyên bố qua Cổng 2** — chỉ Khoa quyết qua cổng. Đây là số đo, không phải phán quyết.

### Kiểm thử

```
dotnet test tools/CoreTests                      → 299/299 xanh
dotnet build tools/CoreTests/UnityApiGate        → sạch (cổng API Unity, NUnit 3.5 biên dịch-only)
python tools/doc/test_roundtrip.py               → 18 trường hợp + design.md, xanh
python tools/doc/docx_read.py --check docs/design.docx  → KHỚP
```

Năm test trong `BalanceSweepTests` mang `[Explicit]` — công cụ **đo**, không phải chốt chặn, mỗi cái chạy hàng nghìn ván. Gọi đích danh khi cần một vòng chỉnh cân bằng.

### Hiện vật

38 file `Assets/Scripts/Core/`, 25 file `Assets/Scripts/Core.Tests/`.

| File | Bytes | sha256 (16 đầu) |
| --- | --- | --- |
| `docs/design.md` | 29925 | `6b702ecc7b87fbca` |
| `docs/design.docx` | 18397 | `a7906ac8542fcaa3` |
| `docs/design.sync.json` | 614 | `eb2be290ad0f2dcf` |
| `docs/decisions.md` | 47284 | `b2df582f72d9fae7` |
| `docs/core-api.md` | 45016 | `a374e8f63a7bcae5` |
| `docs/balance-log.md` | 12898 | `d12317b7302a5277` |
| `Assets/Data/balance.json` | 3809 | `67af3ff841754633` |
| `Assets/Data/plants.json` | 3832 | `a05ea47d0ff2d39e` |
| `Assets/Data/sites.json` | 3304 | `aa452229a662d801` |
| `Assets/Data/neurons.json` | 9084 | `f0942666af46f720` |

> Dự án **chưa commit git lần nào** (`decisions.md` D4). Bảng băm trên là thứ gần nhất với một mốc kiểm lại được. Sau P1, `git init` nên là việc đầu tiên.

---

## 3. Còn nợ

### 3.1 D5 — catalog xuyên ván: **chưa dựng, CÓ CHỦ ĐÍCH**

Không phải quên. Lý do dừng:

- Catalog là **hệ thống duy nhất nằm ngoài ván chơi**. Khoa nói *"tôi cần thấy sản phẩm và tôi tự chơi thử"*; một bộ sưu tập xuyên ván không đóng góp gì cho câu đó.
- P1 còn 7 ngày (16–18/11), và **hai quyết định thiết kế đang treo đều nằm TRONG ván chơi** (§4). Dựng catalog trong lúc đó là làm việc dễ thay vì việc đúng.

Thứ đã sẵn nếu quyết dựng: Khoa đã chốt bộ lọc **tên · nhóm loài · thời đại**, kéo theo hai trường mới cho mỗi loài trong `plants.json` (`nhóm loài`, `thời đại`) — **ghi nhận, chưa cài**.

### 3.2 Câu còn mở, chưa ai trả lời

| Mã | Câu | Chặn gì |
| --- | --- | --- |
| **E1** | Công trình có cần ngưỡng **số người** riêng, hay chỉ tổng sức mạnh là đủ? Hiện chỉ tổng sức mạnh (`design.md` §5.3). | Không chặn gì. |
| **E5** | Catalog xuyên ván có vào mốc 15/11 không? | Chặn việc *quyết có dựng hay không*, không chặn việc dựng. |
| — | `hunt_food_per_member[0] = 0`: săn ở giai đoạn 1 cho **0 thức ăn**, chỉ cho neuron, tinh thần, chiến tích. Săn mở từ lượt 3 (D13) tức là ngay giai đoạn 1. Có chủ ý không? | Không chặn, nhưng nó làm Săn bắt thành hoạt động lạ ở giai đoạn 1. |
| — | Thứ tự ưu tiên **hiển thị** khi một người tìm thấy nhiều loài trong cùng một lượt. | Nhánh UI. |
| — | `tools/sim/before_us_sim.py` sẽ ra sao sau v7 — sửa, bỏ, hay hạ cấp trong `AGENTS.md`. | Quy trình, không phải luật. |

### 3.3 Hai nút cổng rỗng ruột

D13 bỏ điều kiện nút cho Săn bắt và Xây dựng, nên hiệu ứng `unlock_activity` của hai nút này **trở thành vô hiệu**:

| Nút | Hiệu ứng khai trong `neurons.json` | Thực tế |
| --- | --- | --- |
| `s2_stone_tools` | `unlock_activity: build` | **Vô hiệu**, nhưng nút vẫn có việc: nó mở các công trình **loại đá** qua `requires_node` trong `sites.json` (D20/B.3). `effect_text` đang nói thiếu việc đó. |
| `s2_scavenge_meat` | `unlock_activity: hunt` | **Vô hiệu, và không có việc nào khác.** Thật sự rỗng. |

Khoa đã nói sẽ bổ sung cơ chế (D9). `s2_scavenge_meat` nên quyết **cùng lúc** với câu `hunt_food_per_member[0]` ở trên — cả hai đều là chuyện săn bắt.

---

## 4. Chờ Khoa — ba quyết định, đều đã có số

### 4.1 Ổ đựng thức ăn bậc 2–3 bị trội hoàn toàn

Đo 500 ván, bot balanced: **bậc 1 đạt ở 93,2% số ván; bậc 2 và 3 ở 0,0%.**

Lý do có tính cấu trúc, không phải chính sách bot kém. Cùng một khoản sức mạnh bỏ ra:

- dọn một công trình → **+1 chỗ ở** *và* +1 `build_level`;
- nâng một bậc ổ → chỉ +1 `build_level`, **không có chỗ ở**.

Bậc 2 cần 4,0 sức mạnh, trong khi **luôn còn công trình rẻ hơn** (vùng 0: 1,8 / 2,2 / 3,0 / 3,6; vùng 1: 2,4 / 2,8 / 3,2 / 3,4 / 3,8 / 4,2 / 4,8). Một người chơi biết cân nhắc sẽ không bao giờ chọn nó.

Ba hướng gỡ, **không tự chọn**: cho ổ mở chỗ như công trình · hạ ngưỡng bậc 2–3 xuống dưới công trình rẻ nhất · cắt bậc 2–3 khỏi MVP.

### 4.2 Hiệu ứng nút đáng +16 điểm %

| | balanced thắng |
| --- | --- |
| `applyNodeEffects = false` | 33,2% |
| `applyNodeEffects = true` | **52,1%** |

Dải Cổng 2 đo ở `false` nên đây **không phải chỉ tiêu trượt**. Nhưng `HANDOFF.md` nói rõ vượt 45% thì **đề xuất chỉnh, không tự sửa**.

### 4.3 E5 — catalog có vào mốc 15/11 không

Xem §3.1.

---

## 5. Cạm bẫy cho người sau

Năm cái dưới đây đều đã cắn ít nhất một lần trong ngày hôm nay. Bốn cái đầu có test canh; cái cuối thì không.

### 5.1 `SiteView` là bản CHỤP, không phải tay cầm

Mỗi lần gọi `Find` hay `VisibleSites` trả về một vật mới. Gán `view.Tier = 2` **không đổi gì**. Trạng thái thật ở `GameState.FoodNestTier` và `GameState.ClearedSiteIds`; chỉ `SiteSystem.Resolve` đổi được chúng. *(Nhánh UI mất một vòng mới nhận ra.)*

### 5.2 `ClearedSiteIds` **reset khi di cư** — đừng đếm ở cuối ván

Một ván dọn 3 công trình rồi di cư trông **y hệt** ván chưa dọn gì. Dùng `BotGame.SitesClearedEver` nếu cần đếm cả ván. *(Suýt dẫn tới kết luận "bot không biết xây".)*

### 5.3 Constructor `TurnSystem` chép số chỗ của vùng khi `RegionCapacityMax == 0`

Đúng cho ván **mới**, sai cho ván **đã tải**. Nếu nó chép cả với ván tải thì ván ở vùng 1 bị kéo về số của vùng hiện tại và **xoá sạch công trình đã dọn** — không ngoại lệ, không thông báo, chỉ là tiến độ biến mất.

Test canh: `SaveMigrationTests.TaiVanDaDiCu_KhongBiGhiDeSoChoCuaVung`.

### 5.4 Khung đảm bảo hái lượm: ngưỡng là **TỔNG nhóm**, cho **một** lần tìm thấy

Không phải chỉ số từng người, và không phải mỗi người một lần. Bản cài đầu tiên của Core sai **cả hai chỗ** (D16). Test canh: `GatherRulesTests.KhungDamBao_ChoCaNhomMotLan_ChuKhongPhaiMoiNguoiMotLan`.

### 5.5 ⚠ Danh sách đánh số có khối mã chen giữa **không sống sót qua docx** — không có test canh

`1. … 2. ``` khối mã ``` … 3. …` → khứ hồi trả về `1. 1. 1.`.

Trong một đặc tả **luật chơi**, "bước 2" thành "bước 1" là **sai nội dung**, không phải sai trình bày: ai đó sẽ cài sai thứ tự giải quyết và không truy ra được vì sao.

Đây là **giới hạn đã ghi** của `tools/doc` (README, mục "Giới hạn đã biết"), không phải lỗi công cụ. Cách tránh: viết các bước thành đoạn `**Bước 1 — …**` thay vì danh sách đánh số. `design.md` §5.1 đã viết lại theo cách đó.

**Luôn chạy `python tools/doc/test_roundtrip.py docs/design.md` trước khi sinh docx.** Phép "không rụng chữ" của nó bắt được lỗi này; phép so cấu trúc **để lọt**.

---

## 6. Hai bài học về phương pháp, đắt hơn bất kỳ dòng mã nào

**Nguồn sai, không phải suy luận sai.** Ba lần trong ngày một con số *trông* sai nhưng cái sai nằm ở **phép đo**, không ở hệ thống: bot thắng 0% vì phép đo đặt năng lượng sai thứ tự; bot "không biết xây" vì đếm ở cuối ván; và một lần tôi đọc dấu vết trên đĩa rồi **suy ra tác giả** của `design.md`, trong khi `list_agents` trả lời thẳng câu đó. Khi một con số trông sai, nghi phép đo trước khi nghi hệ thống.

**Bất biến trải trên cả file không kiểm được bằng cách nhìn chỗ mình vừa sửa.** Hai va chạm số (D21, D24) lọt qua mắt vì tôi chỉ soi chỗ mình vừa ghi. Script quét cả file bắt được cả hai trong một lần chạy. Áp cho mọi bàn giao: sau khi sửa, quét cả file tìm thứ **không** nên đổi mà đã đổi.

Một hệ quả cụ thể: **đếm đúng 8 nhãn vẫn có thể là mất hai nhãn cũ và thêm hai nhãn mới.** Kiểm từng nhãn bằng chuỗi neo, đừng kiểm bằng phép đếm.

---

## 7. Quy tắc còn hiệu lực với người kế nhiệm

- **Số cân bằng được chỉnh** (Khoa ủy quyền, D22) — nhưng **mỗi lần chỉnh phải ghi một dòng `balance-log.md` TRONG CÙNG LƯỢT**, đủ bốn cột, kể cả cột phép đo. Không có phép đo thì ghi thẳng "chưa có". Quy tắc này **đã bị vi phạm một lần** ngày đầu tiên.
- **Luật và nội dung không được chỉnh**: cơ chế, cách một hệ thống vận hành, tên loài, câu mô tả hiện cho người chơi. Gặp thứ *trông như con số nhưng thực ra là luật* thì **hỏi**, đừng tự xếp loại.
- **Đo trước · chỉnh · đo lại.** Không chỉnh một con số vì nó "trông sai".
- **Hai tiêu chí ngang hàng** (D23): dải Cổng 2 **và** "balanced thua trước lượt 10 ≤ 10%". Kéo tỉ lệ thắng xuống bằng đòn **cắn về sau**, không bằng cách tăng sát thương của trừng phạt sớm.
- **Khi phải sửa đến assertion thứ ba theo cùng một hướng, dừng lại và nghi dữ liệu thử** — không phải nghi tiếp assertion thứ tư.
- Không có git: **sao lưu ra ngoài repo trước khi ghi đè bất cứ thứ gì Khoa viết tay**, và băm lại ngay trước khi ghi.
