# Before Us — Bàn giao cho Supervisor (Paseo)

Bạn là supervisor điều phối các coding agent làm game **Before Us** cho Khoa. File này cho bạn đủ ngữ cảnh để lập và giao việc. Đọc kèm: `AGENTS.md` (quy tắc cho agent), `docs/design.md` (luật chơi), `Assets/Data/balance.json` (số liệu), `tools/sim/before_us_sim.py` (mô phỏng tham chiếu).

## 1. Mục tiêu

Một bản demo chơi được trên trình duyệt (WebGL), phát hành trên itch.io **trong 3 tuần**, kể từ ngày bắt đầu (tuần 1, ngày 1).

- Tuần 1: lõi luật chơi + prototype (vòng lượt và combat mini).
- Tuần 2: ghép, playtest với 5 người, hoàn thiện hình ảnh và âm thanh.
- Tuần 3: test thêm, sửa lỗi, phát hành.

Khoa muốn đi nhanh để giữ cảm hứng. Nếu bị trễ: **giữ ngày phát hành, cắt bớt hình ảnh và nội dung**, không lùi lịch. Không bao giờ cắt: tính đúng của luật chơi, lưu ván, test cân bằng.

## 2. Về Khoa và cách làm việc với Khoa

- Khoa là người thiết kế và chủ dự án, làm một mình cùng agent. Kỹ sư phần mềm (Kotlin/Android, Python, TypeScript); C# và Unity đang học. Máy Windows, đã có Unity.
- Khoa quyết định: mọi thay đổi luật và con số, cảm giác combat, phong cách hình ảnh, nội dung sự kiện, và có đi tiếp qua mỗi cổng hay không.
- Khoa tự làm: dựng và chỉnh scene trong Unity Editor khi cần, cảm giác thả quân trong combat, vẽ chân dung, tìm người chơi thử.
- Gửi Khoa mỗi ngày một bản tóm tắt ngắn: việc xong, việc đang làm, câu hỏi cần Khoa trả lời (đánh số, mỗi câu kèm phương án đề xuất để Khoa chỉ cần chọn).

## 3. Phạm vi MVP

| Có | Không có (để sau) |
| --- | --- |
| Giai đoạn tiến hóa 1–2; thắng khi lên giai đoạn 3 | Giai đoạn 3–5 |
| Mạng lưới neuron giai đoạn 1–2 (design.md §7.3–7.4) | Nút giai đoạn 3–4, phong cách đánh tầm xa |
| 2 vùng: rừng nhiệt đới, savan | Các vùng khác |
| 3 loài quái thú + con mồi thú nhỏ | Thêm loài |
| Combat 2 kiểu (Dụ và hạ, Phòng thủ), 3 kiểu tấn công | — |
| Khoảng 12 sự kiện, đủ các loại | Thêm sự kiện |
| Khí hậu Ẩm/Khô, độ khó tăng dần, hướng dẫn từng bước, lưu ván | Quảng cáo, điện thoại |
| Chân dung ghép đặc điểm ngẫu nhiên cho giai đoạn 1–2; còn lại dùng hình CC0 của Kenney | Hoạt ảnh đầy đủ |

Số liệu MVP dùng `mode: "mvp"` trong `balance.json` (chi phí neuron 150/250, độ khó tăng mỗi 12 lượt, tối đa 2 vùng).

## 4. Kiến trúc

Xem `AGENTS.md`. Tóm tắt: Core là C# thuần, không dùng UnityEngine, test được bằng `dotnet test` ngoài Unity; UI bằng UI Toolkit (UXML/USS là file chữ, agent sửa được); combat là scene riêng, trả kết quả gọn qua `ICombatResolver`; mọi con số trong JSON.

```
BeforeUs/
  AGENTS.md  HANDOFF.md
  docs/design.md
  tools/sim/before_us_sim.py        mô phỏng tham chiếu (python before_us_sim.py mvp)
  tools/CoreTests/                  project dotnet test, biên dịch chung mã nguồn Core
  Assets/
    Data/balance.json  neurons.json  events.json  beasts.json  regions.json  strings.vi.json
    Scripts/Core/  Core.Tests/  Game/  UI/  Combat/
    UI/                             UXML, USS
```

## 5. Việc, thứ tự và tiêu chí nghiệm thu

Ký hiệu: **[K]** Khoa làm, **[A]** agent làm, **[K+A]** agent dựng, Khoa chỉnh và quyết. Các việc không phụ thuộc nhau chạy song song.

### Tuần 1, ngày 1: Chuẩn bị

| Mã | Ai | Việc | Phụ thuộc | Xong khi |
| --- | --- | --- | --- | --- |
| S1 | K | Tạo dự án Unity 2D (bản LTS đã cài, module WebGL), repo GitHub, bật Git LFS; chép bộ file này vào gốc repo | — | Repo có dự án Unity mở được |
| S2 | A | `.gitignore` Unity, `.gitattributes` LFS, cấu trúc thư mục, asmdef `BeforeUs.Core` (noEngineReferences), `BeforeUs.Core.Tests`, package Newtonsoft JSON | S1 | Unity mở không lỗi; asmdef chặn được UnityEngine trong Core |
| S3 | A | `tools/CoreTests/` (project dotnet, NUnit) biên dịch mã nguồn Core; một test mẫu | S2 | `dotnet test tools/CoreTests` chạy xanh |
| S4 | K | Hẹn 5 người chơi thử cho tuần 2, ngày 2–4 | — | Có 5 người đồng ý |

**Cổng 1:** S1–S3 xong; agent đọc được docs và chạy được test.

### Tuần 1, ngày 2–3: Lõi luật chơi

| Mã | Ai | Việc | Phụ thuộc | Xong khi |
| --- | --- | --- | --- | --- |
| C1 | A | Đọc `balance.json` vào `BalanceConfig` (chọn mode full/mvp) | S3 | Test đọc đúng mọi trường |
| C2 | A | `IRandom` có seed và lưu/khôi phục trạng thái; `Member` (speed, strength, hp, traits); `GameState` | S3 | Test: cùng seed cho cùng chuỗi kết quả |
| C3 | A | `Climate` (Ẩm/Khô 10 lượt), `Difficulty` (bậc theo lượt) | C1 | Test theo design.md §10–11 |
| C4 | A | `TurnSystem`: 5 hoạt động, mở khóa, năng lượng, hái lượm, thức ăn không để dành, đói, sinh sản, sức chứa, tinh thần | C1, C2, C3 | Test từng quy tắc design.md §2–6 |
| C5 | A | `neurons.json` từ design.md §7.3–7.4; `NeuronNetwork`: mua nút, nút cổng, chiến tích, tiến hóa; cờ `applyNodeEffects` | C1, C2 | Test: không tiến hóa được khi thiếu cổng hoặc chiến tích |
| C6 | A | `EventSystem` (§8): thời gian ân hạn, trận hướng dẫn lượt 3, 3 loại sự kiện, quota phòng thủ; `ProbabilisticCombatResolver` (§5.2, §8) | C4 | Test xác suất trên 10.000 lần rút |
| C7 | A | Lưu và tải ván (JSON, gồm trạng thái `IRandom`) | C4, C5, C6 | Lưu rồi tải giữa ván cho kết quả y hệt chạy liền |
| C8 | A | 4 bot (balanced, safe, reckless, careless) chép logic từ `before_us_sim.py`; `BalanceTests` chạy 2.000 ván mỗi bot | C4–C6 | Xem Cổng 2 |

**Cổng 2** (với `applyNodeEffects = false`, mode mvp, để so với mô phỏng):

- Toàn bộ unit test xanh.
- Bot balanced thắng 30–45%, trung vị 42–52 lượt. Bot safe thắng ≤ 2%. Bot reckless và careless thắng ≤ 20%.
- Sau đó chạy lại với `applyNodeEffects = true` và **báo Khoa** tỉ lệ thắng mới; nếu vượt 45%, đề xuất chỉnh hiệu ứng nút (không tự sửa).

### Tuần 1, ngày 2–7: Prototype (hai nhánh song song)

Nhánh A, vòng lượt, agent làm gần hết (bắt đầu khi C4 xong; dùng resolver xác suất):

| Mã | Ai | Việc | Xong khi |
| --- | --- | --- | --- |
| A1 | A | Màn chính UI Toolkit: tài nguyên, giai đoạn, pha khí hậu, dự báo "cuối lượt cần X, đang có Y", danh sách thành viên chọn theo hoạt động, nút kết thúc lượt | Chơi được nhiều lượt chỉ bằng chuột |
| A2 | A | Hộp sự kiện (lựa chọn và ngẫu nhiên), banner đổi pha, gợi ý hướng dẫn theo design.md §13 | Gợi ý hiện đúng lượt, tắt được |
| A3 | A | Màn mạng lưới neuron: 3 nhánh, trạng thái nút, chiến tích, nút Tiến hóa | Mua nút và tiến hóa được từ UI |
| A4 | A | Menu: chơi mới, tiếp tục (lưu ván), màn thắng, màn thua (nút chơi lại) | Vòng chơi khép kín |

Nhánh B, combat mini, Khoa dẫn dắt:

| Mã | Ai | Việc | Xong khi |
| --- | --- | --- | --- |
| B1 | K+A | Scene combat 2D: chạm/nhấp để thả thành viên đã cử, tự đánh theo speed/strength/hp; kiểu trận Dụ và hạ, Phòng thủ | Một trận chạy trọn và trả `CombatResult` |
| B2 | A | 3 kiểu tấn công (săn kẻ yếu, ngẫu nhiên, thách đấu kẻ mạnh); `beasts.json` theo design.md §9 | Test chọn mục tiêu đúng kiểu |
| B3 | A | Công cụ hiệu chỉnh: tự chơi combat 500 trận với cách thả ngẫu nhiên, đo tỉ lệ thắng so với công thức §5.2/§8 | Báo bảng lệch theo k, giai đoạn, độ khó |
| B4 | K | Chơi và chỉnh cảm giác thả quân | Khoa thấy "đã tay" |

**Cổng 3:** chơi trọn một ván bằng giao diện nút bấm; combat chạy độc lập, 3 người thử thấy "đã tay"; tỉ lệ thắng combat lệch công thức ≤ 10 điểm %.

### Tuần 2, ngày 1–4: Ghép và playtest

| Mã | Ai | Việc | Xong khi |
| --- | --- | --- | --- |
| I1 | A | Nối combat thật vào vòng lượt qua `ICombatResolver` (bất đồng bộ), thay resolver xác suất | Săn, khám phá, phòng thủ, cơ hội combat đều vào scene combat |
| I2 | A | `events.json` khoảng 12 sự kiện (bản nháp theo các loại ở design.md §8) | Khoa duyệt nội dung |
| I3 | A | Telemetry: ghi sự kiện chơi ra file JSON Lines (bắt đầu ván, mỗi lượt, combat, chết, tiến hóa, kết thúc); notebook Python phân tích | Có biểu đồ tỉ lệ thắng, lượt chết, nguyên nhân chết |
| I4 | A | Bản build WebGL; trang itch.io riêng tư (Khoa tạo tài khoản và đăng) | 5 người mở được trên trình duyệt |
| P1 | K | Playtest với 5 người, quan sát, thu file telemetry | Có ghi chú và dữ liệu |

**Cổng 4:** người chơi có tính toán thắng khoảng 25–45%; ít nhất 3/5 người tự chơi lại sau khi thua. Không đạt thì đề xuất chỉnh số (kèm mô phỏng), Khoa quyết.

### Tuần 2, ngày 4 đến hết tuần 3: Hoàn thiện và phát hành

| Mã | Ai | Việc |
| --- | --- | --- |
| F1 | K+A | Hệ ghép chân dung từ đặc điểm ngẫu nhiên (code: agent; hình: Khoa, thiếu thì dùng hình tạm Kenney) |
| F2 | K+A | Hình nền 2 vùng, quái thú, màn tiến hóa (giữ đơn giản) |
| F3 | A | Khoảng 10 hiệu ứng âm thanh (jsfxr) và nhạc nền CC0; cân âm lượng |
| F4 | A | Sửa lỗi theo playtest; hiệu năng WebGL |
| F5 | K | Trang itch.io công khai: ảnh, GIF, mô tả; devlog |

**Cổng 5:** demo công khai, có người lạ chơi và để lại nhận xét.

## 6. Phải hỏi Khoa trước khi làm

- Bất kỳ thay đổi luật hoặc con số nào; mọi giá trị **[ĐỀ XUẤT]** muốn đổi.
- Nội dung sự kiện, tên và kiểu tấn công của quái thú.
- Phong cách hình ảnh, âm thanh.
- Thêm bất cứ thứ gì ngoài phạm vi MVP.
- Sửa scene hoặc prefab mà việc được giao không nói rõ.

## 7. Câu hỏi còn mở (đã có giá trị đề xuất để chạy trước)

1. Chỉ số thành viên: speed, strength 0.8–1.2; hp 3–7.
2. Khi đói, người chết chọn ngẫu nhiên.
3. Hiệu ứng số của các nút neuron (design.md §7.3–7.4) chưa được kiểm chứng bằng mô phỏng.
4. Tên 3 loài quái thú: báo hoa mai, linh cẩu, hổ răng kiếm. *(09/11, D21/D21a — Khoa đã duyệt: thêm trăn, và đại bàng nếu kịp; các việc BX0–BX9 cho kiểu nhắm "Bắt kẻ lẻ loi", trạng thái trong trận, hồ sơ lịch sử và thành tựu ở `docs/bestiary.md` §7, kèm thứ tự cắt khi thiếu thời gian.)*
5. Thời lượng trận combat 20–40 giây; số kẻ địch mỗi trận.

## 8. Rủi ro chính

| Rủi ro | Cách xử lý |
| --- | --- |
| Agent âm thầm đổi luật | Test cân bằng chạy trước mỗi lần ghép nhánh; số chỉ nằm trong JSON |
| Code chạy nhanh hơn playtest | Không vượt cổng khi chưa có người chơi thử |
| Combat không "đã tay" | Bắt đầu nhánh B ngay ngày 2; ưu tiên thời gian của Khoa cho nó |
| Scene Unity hỏng do agent | Scene mỏng, UI bằng UXML, Khoa tự sửa scene |
| Trễ lịch | Giữ ngày phát hành, cắt hình ảnh trước |
