# Before Us — Nhật ký quyết định

> Nơi ghi các quyết định của **Khoa** (chủ dự án, người thiết kế) làm rõ chỗ tài liệu còn mơ hồ.
> File này **không** ghi đè `docs/design.md`, `AGENTS.md` hay `HANDOFF.md`. Khi mâu thuẫn: `design.md` là nguồn sự thật về luật chơi, file này là nguồn sự thật về *quyết định vận hành*.
> Thứ tự ưu tiên nguồn sự thật vẫn theo `AGENTS.md`.

## Quy ước đọc file này

1. **Số hiệu không bao giờ được dùng lại.** D14 đã dùng thì quyết định sau là D15, kể cả khi ai đó bảo ghi là D14.
2. **Quyết định bị thay thì ĐÁNH DẤU, không xoá.** Lịch sử quyết định là thứ giải thích vì sao luật hiện tại trông như vậy — xoá D1 thì không ai hiểu nổi vì sao `s2_stone_tools` rỗng ruột.
3. **Đọc bảng dưới TRƯỚC khi đọc chi tiết.** Một mục đánh dấu 🚫 thì không được áp dụng, dù nó viết rõ ràng tới đâu.

| Số | Nội dung | Trạng thái | Ai quyết |
| --- | --- | --- | --- |
| D1 | Săn bắt và Xây dựng mở khóa bằng nút neuron | 🚫 **ĐÃ BỊ THAY bởi D13** | Khoa |
| D2 | Một hoạt động tối đa một lần mỗi lượt; chỉ tính khi `EndTurn()` | ✅ Còn hiệu lực · ✏️ đã đính chính | Khoa |
| D3 | Prototype cũ dời sang `Assets/~Prototype/` | ✅ Còn hiệu lực | Khoa |
| D4 | Không dùng git; đình chỉ quy tắc nhánh `task/<mã>` | ✅ Còn hiệu lực | Khoa |
| D5 | Lịch; ngày phát hành 28/11/2026 | ✅ Còn hiệu lực | Khoa |
| D6 | Giữ dự án Unity hiện tại, không dựng mới | ✅ Còn hiệu lực | Khoa |
| D7 | Mobile không vào MVP | ✅ Còn hiệu lực | Khoa |
| D8 | Hướng màn hình ngang cố định | ✅ Còn hiệu lực · ✏️ đã đính chính cách thực thi | Khoa |
| D9 | Giữ Built-in Render Pipeline, không URP | ✅ Còn hiệu lực | Khoa |
| D10 | Chuẩn hóa trên Unity 6000.6.7f1 | ✅ Còn hiệu lực | Khoa |
| D11 | Thua Phòng thủ luôn chừa lại người sống sót | ✅ Còn hiệu lực · ✏️ đính chính bởi D14c (chừa 2, không phải 1) | **Supervisor** |
| D12 | HOÃN: sự kiện "ngẫu nhiên" chưa hoãn sang đầu lượt sau | ⏳ **Món nợ**, gỡ ở I2 | **Supervisor** |
| D13 | Săn bắt và Xây dựng mở từ lượt 3, bỏ điều kiện nút | ✅ Còn hiệu lực · **thay D1** | Khoa |
| D14 | Ba thay đổi luật từ playtest 01 (a, b, c) | ✅ Còn hiệu lực · ⏳ `−2` chờ Khoa duyệt | Khoa |
| D15 | Cả ba hệ thống mới của playtest 01 vào MVP; mốc 15/11 | ✅ Còn hiệu lực | Khoa |
| D16 | Khung đảm bảo hái lượm tính theo TỔNG nhanh nhẹn cả nhóm, mỗi loài một lần | ✅ Còn hiệu lực · **sửa lỗi đọc của bản cài Phần A đầu** | Khoa |
| D17 | Ổ đựng thức ăn nâng cấp thì HẠ ngưỡng khung đảm bảo, không giảm nhân lực/năng lượng | ✅ Còn hiệu lực · **thay hai đề xuất cũ** | Khoa |
| D18 | `build_level` thành số suy ra; `build_points` và `points_to_level_factor` chết | ✅ Còn hiệu lực | **Agent (Core)**, theo D7/B.2 |
| D19 | Xây dựng giải quyết CUỐI lượt, không còn trước combat | ✅ Còn hiệu lực | **Agent (Core)**, theo D7 |
| D20 | "Slot" là CHỖ CHO NGƯỜI (sức chứa hang) theo vùng; dọn công trình mở thêm chỗ | ✅ Còn hiệu lực · **thay công thức §4** | Khoa |
| D21 | Sinh vật lịch sử · kiểu nhắm "Bắt kẻ lẻ loi" · trạng thái trong trận · hồ sơ và thành tựu | ✅ Còn hiệu lực · đặc tả ở `bestiary.md` | Khoa |
| D22 | Khoa ủy quyền chỉnh cân bằng cho Supervisor; mọi lần chỉnh phải ghi `balance-log.md` | ✅ Còn hiệu lực | **Khoa ủy quyền cho Supervisor** |
| D23 | Tiêu chí "không chết sớm" ngang hàng dải Cổng 2: balanced thua trước lượt 10 ≤ 10% | ✅ Còn hiệu lực | **Supervisor** |
| D24 | Hoạt ảnh sinh vật bằng gắn xương (2D skeletal rigging), không sinh ảnh từng động tác | ✅ Còn hiệu lực · đặc tả ở `prompt.md` | Khoa |
| D25 | Di cư: người ở lại mất hẳn · trần là số chỗ vùng mới mở sẵn · một chiều | ✅ Còn hiệu lực | Khoa |
| D26 | C7 dùng chung bot của C8; bỏ hẳn driver TestPlayer | ✅ Còn hiệu lực | **Supervisor** |
| D27 | Dừng trước D5 catalog có chủ đích; nhánh Core bàn giao ở `docs/handover-core.md` | ✅ Còn hiệu lực | **Supervisor** |

**Đang chờ Khoa, chưa thành quyết định:** bản nháp đặc tả v7 (`docs/design-v7-draft.md`, 9 câu hỏi ở Phần D) · giá trị `morale.on_member_death = −2` · `hunt_food_per_member[0] = 0` · sáu nút neuron rỗng (hai từ D13, bốn nữa nếu v7 được duyệt).

## 2026-11-08

### D1 — Mở khóa Săn bắt và Xây dựng theo đúng luật, chấp nhận lệch với mô phỏng
> 🚫 **ĐÃ BỊ THAY bởi D13 (2026-11-08) — ĐỪNG ÁP DỤNG MỤC NÀY.** Săn bắt và Xây dựng nay mở từ lượt 3, không còn điều kiện nút neuron nào.
> Giữ lại vì nó là thứ **duy nhất** giải thích vì sao `s2_stone_tools` và `s2_scavenge_meat` hiện rỗng ruột, và vì sao số liệu C8 trông như vậy.
Core làm đúng `design.md` §5 và `balance.json`: phải mua nút `s2_stone_tools` (28 neuron) mới mở Xây dựng, nút `s2_scavenge_meat` (42 neuron) mới mở Săn bắt.
**Lý do:** mô phỏng tham chiếu `tools/sim/before_us_sim.py:56` dùng `build=s>=2, hunt=s>=2`, tức bỏ qua điều kiện nút. Khoa đã xem và chấp nhận sai lệch này **tạm thời**; việc hợp lý hóa để sang giai đoạn sau. Không sửa `sim.py`, không sửa số trong `balance.json`, không sửa Core cho khớp sim. Chỉ đo và báo.

### D2 — Một hoạt động chỉ xuất hiện tối đa một lần trong một lượt
> ✅ **Còn hiệu lực.** ✏️ Đã đính chính 2026-11-08 phần công thức thức ăn Kiếm ăn — xem khối trích dẫn cuối mục.
Trong lượt của mình, người chơi được tự do thay đổi phân công bao nhiêu lần tùy ý; mọi thứ chỉ là dự định. Không có gì được tính cho tới khi người chơi bấm kết thúc lượt.
**Hệ quả bắt buộc cho Core:** trạng thái phân công là map `hoạt động → tập memberId`, sửa tự do, xác thực ngân sách năng lượng tại chỗ; mỗi hoạt động trả năng lượng đúng một lần mỗi lượt, không phụ thuộc số người; Kiếm ăn là **một** hoạt động với `k` = tổng số người được cử, tinh thần **+1 một lần**; mỗi thành viên tối đa 1 hoạt động mỗi lượt; chỉ giải quyết kết quả trong một bước `EndTurn()`.
**Thức ăn Kiếm ăn tính theo TỪNG NGƯỜI, không nhân gộp.** `design.md` §5.1: "thực nhận của mỗi người = `gather_yield × speed` (làm tròn)". Nhân `speed` rồi làm tròn cho từng người, sau đó mới cộng lại — `GatherRules.FoodFromMember` trong Core làm đúng vậy. Vì `speed` nằm trong dải 0.8–1.2, ba người cùng đi có thể mang về 3+2+4 chứ không phải 3+3+3. Đẳng thức `k × gather_yield` **chỉ đúng khi `speed = 1`**, tức chỉ ở chế độ trung bình (`MemberStatRanges.AverageFromConfig`) dùng cho BalanceTests ở C8.
**Lý do:** `sim.py:72` và `sim.py:80` gọi `add('gather', …)` hai lần trong cùng một lượt, trả 2 năng lượng và +2 tinh thần. Dưới D2 việc đó không hợp lệ. Core làm theo D2; sai lệch được ghi rõ trong báo cáo C8.
> **Đính chính 2026-11-08 (Supervisor, không phải Khoa đổi ý).** Bản D2 đầu tiên chuyển cho Lead Core ghi thức ăn Kiếm ăn là `k × gather_yield`. Vế đó sai: nó gộp nhầm công thức của chế độ trung bình thành luật chung, trong khi `design.md` §5.1 luôn nhân và làm tròn theo từng người. **Luật không hề thay đổi** — chỉ lời diễn giải của Supervisor được sửa cho khớp `design.md`. Core từ đầu đã cài đúng theo §5.1.

### D3 — Prototype cũ chuyển sang `Assets/~Prototype/`, không xoá
6 file `.cs` (`M1`, `M2`, `F1`, `Enemy`, `SpawnMobs`, `PopulationManagement`) cùng `.meta` đã được di dời khỏi `Assets/Script/`. Thư mục rỗng `Assets/Script/` và `Assets/Script.meta` đã xoá sau khi xác nhận nội dung an toàn.
**Lý do:** prototype không thuộc kiến trúc MVP. Unity bỏ qua thư mục kết thúc bằng `~`, nên chúng ra khỏi đường biên dịch mà vẫn còn để tham khảo. Phải làm sớm vì `Enemy.cs:9` có `using UnityEditor.U2D.Animation;` trong script runtime, sẽ làm gãy bản build WebGL ở I4. Hệ quả đã biết: `Assets/Scenes/SampleScene.unity` hiện "Missing (Mono Script)".
**Tuyệt đối không đụng `Assets/Sprite/**`** — art Khoa tự vẽ, dùng lại ở F1–F2.

### D4 — Dự án hiện không dùng git
Quy tắc "mỗi việc một nhánh `task/<mã-việc>`" trong `AGENTS.md` **tạm thời đình chỉ**.
**Lý do:** không có repo git cho thư mục dự án, nên không có đường lùi tự động. Đổi lại: cẩn trọng gấp đôi, chỉ tạo file trong phạm vi được cấp, di dời thay vì xoá, không sửa file ngoài phạm vi.

### D5 — Lịch
Bắt đầu 08/11/2026 (Chủ nhật). Tuần 1: 08–14/11. Tuần 2: 15–21/11. Tuần 3: 22–28/11. **Ngày phát hành: 28/11/2026.**
**Lý do:** theo `HANDOFF.md`, S2–S3 thuộc tuần 1 ngày 1 và C1–C8 thuộc tuần 1 ngày 2–3. Nhánh UI và combat chờ C4, nên C4 là đường găng. Nếu trễ: giữ ngày phát hành, cắt hình ảnh và nội dung trước.

### D6 — Giữ dự án Unity hiện tại, không dựng dự án mới
Khoa có cân nhắc dựng một dự án mới để làm được cả PC và mobile.
**Lý do bác:** Unity **không cần dự án mới để đa nền tảng** — một dự án build được WebGL / Windows / Android / iOS chỉ bằng đổi Build Target; thứ quyết định là module cài trong Unity Hub và thiết kế input/UI, không phải cấu trúc dự án. Chi phí dựng mới là nửa ngày đến một ngày import lại asset, trên quỹ 21 ngày có ngày phát hành cố định (D5).
**Ghi thêm:** nhờ `noEngineReferences: true` trên asmdef `BeforeUs.Core`, toàn bộ lõi luật chơi chép sang dự án khác gần như không mất công. Nên nếu về sau thật sự cần đổi dự án, cái giá vẫn thấp — quyết định này rẻ để đảo ngược.

### D7 — Mobile không vào MVP
Chuẩn bị sẵn để làm sau 28/11/2026, nhưng không nằm trong phạm vi bản phát hành.
**Lý do:** `HANDOFF.md` §3 xếp "điện thoại" vào cột *Không có (để sau)*; `design.md` §1 ghi "PC và trình duyệt (WebGL) trước, điện thoại sau". Thêm nền tảng khi lịch đã cố định là đi ngược chính sách của chính `HANDOFF.md` §1: *"giữ ngày phát hành, cắt bớt hình ảnh và nội dung"*.
**Cách dung hòa đã chọn:** không chặn đường tới mobile — chốt hướng màn hình (D8), giữ UI co giãn, cài sẵn Android module — nhưng chỉ phát hành WebGL đúng hạn. Không cần đổi kiến trúc: `design.md` §9 vốn đã viết thao tác combat là "chạm hoặc nhấp".

### D8 — Hướng màn hình ngang cố định
> ✅ **Còn hiệu lực.** ✏️ Đã đính chính 2026-11-08: thực thi bằng `PanelSettings.referenceResolution`, KHÔNG phải Player Settings.
Ràng buộc bắt buộc cho Lead UI ở A1–A4.
**Lý do:** hợp với sân combat 2D nhìn ngang — sprite của Khoa đi qua lại theo trục x, và prototype cũ cũng chỉ chặn biên theo trục x, không có biên trục y (`Assets/~Prototype/M1.cs:49` giới hạn x trong −6…5; `M2.cs:46` trong −6…6). Khớp sẵn với `ProjectSettings.asset`: `defaultScreenWidth/Height` 1920×1080 và `defaultScreenWidth/HeightWeb` 960×600. Hướng dọc sẽ phải thiết kế lại sân combat; co giãn cả hai hướng thì Lead UI phải làm và test hai layout cho mỗi màn, trong 3 tuần.
**Thực thi bằng `PanelSettings.referenceResolution = 1920×1080` trong scene**, không phải bằng Player Settings.
> **Đính chính 2026-11-08 (Supervisor, không phải Khoa đổi ý).** Bản D8 đầu tiên dặn Khoa đặt `Default Orientation → Landscape Left` trong Player Settings, và viện dẫn bốn cờ `allowedAutorotate*` trong `ProjectSettings.asset` đang ở mặc định để kết luận "D8 chưa có hiệu lực". **Hướng dẫn đó sai.** `Default Orientation` và các cờ `allowedAutorotate*` chỉ hiện khi tab nền tảng là **Android hoặc iOS**; với Windows và WebGL, mục Resolution and Presentation có các trường khác hẳn. Bốn cờ đó là cờ của bản mobile, mà mobile theo **D7** nằm sau 28/11/2026 — nên trạng thái mặc định của chúng không nói gì về D8 cả.
> Việc đặt `Default Orientation` thuộc đợt mobile sau phát hành. Ràng buộc ngang cố định của MVP được thực thi ở tầng UI Toolkit, không ở tầng project.

### D9 — Giữ Built-in Render Pipeline, không chuyển URP
**Lý do:** MVP dùng sprite phẳng và hình CC0 của Kenney; `design.md` không yêu cầu ánh sáng động ở đâu cả. URP chỉ cần nếu muốn 2D Lights hoặc Shader Graph, mà URP lại nặng hơn trên WebGL — nền tảng phát hành. Quyết ngay hôm nay vì pipeline là thứ đắt nhất nếu đổi muộn (phải làm lại toàn bộ material và shader); nhưng kết luận là **không đổi**.
**Trạng thái trên đĩa:** đã đúng, không cần ai làm gì — `ProjectSettings/GraphicsSettings.asset:42` có `m_CustomRenderPipeline: {fileID: 0}`, tức Built-in.

### D10 — Chuẩn hóa trên Unity 6000.6.7f1
Dự án đã được mở bằng Unity 6000.6.7f1 và Unity đã tự migrate toàn bộ gói. **Không quay về 2022.3.13f1.** Mọi tài liệu còn ghi "Unity 2022.3.13f1" phải đọc là 6000.6.7f1.
**Lý do:** `Packages/manifest.json` và `Library/` đã migrate xong, quay lại là gỡ bằng tay một mớ dễ sai; Unity 6 build WebGL tốt hơn, mà WebGL là nền tảng phát hành.
**Hệ quả đã kiểm chứng trên đĩa:**
- `timeline` 1.7.6→6.6.0, `ugui` 1.0.0→2.6.0, `test-framework` 1.1.33→1.8.0, `ai.navigation` 1.1.5→2.0.16; `com.unity.textmeshpro@3.0.6` bị gỡ (TMP nay nằm trong `ugui` 2.x); `newtonsoft-json` 3.2.1→3.2.2 (cùng dòng 13.0.x, không ảnh hưởng).
- 45 lỗi `error CS` (21× CS0029, 24× CS0619) đều đến từ `Assets/TextMesh Pro/Examples & Extras/` — script mẫu TMP 3.x dùng API đã bỏ. Thư mục đó đã được xoá sau khi xác nhận không `.cs`, `.unity`, `.prefab` hay `.asset` nào bên ngoài tham chiếu tới. Lấy lại được qua Package Manager (samples của `com.unity.ugui`).
- **Trần ngôn ngữ không đổi:** Unity 6000.6.7f1 tự sinh `Assembly-CSharp.csproj` với `<LangVersion>9.0</LangVersion>` và `<TargetFramework>netstandard2.1</TargetFramework>`. Vì vậy `tools/CoreTests/CoreLib/BeforeUs.Core.csproj` giữ nguyên `netstandard2.1` + `LangVersion 9.0` là đúng; S2–S3 không phải làm lại.

### D11 — Phòng thủ thua thì luôn chừa lại ít nhất 1 người
> ✅ **Còn hiệu lực, nhưng con số đã đổi.** ✏️ D14c (2026-11-08) nâng số người chừa lại từ **1** lên **2**, và biểu diễn nó thành `lose_when_crew_at_or_below + 1` thay vì một hằng số. Tiêu đề mục này giữ nguyên chữ "1 người" theo quy ước không sửa lịch sử — đọc phần đính chính ở cuối mục.
Khi thua trận Phòng thủ bắt buộc, quái thú ăn `quota` người nhưng **không bao giờ giết người cuối cùng**. Thua Săn bắt, Khám phá hay Cơ hội combat thì vẫn có thể xóa sổ cả bầy.
**Lý do:**
1. **Tiền lệ D1.** `design.md` §8 viết "ăn đúng `quota` người rồi rút, không bao giờ xóa sổ cả bầy"; `before_us_sim.py:108` làm `crew -= min(crew, quota + …)`, tức sĩ số có thể về 0. D1 đã dựng khuôn cho đúng loại xung đột này: Core theo `design.md`, sai lệch với sim được chấp nhận, đo ở C8, không sửa sim. Xử khác đi thì Core có hai chính sách cho cùng một kiểu xung đột.
2. **Thứ tự nguồn sự thật.** `AGENTS.md` xếp `design.md` trên `tools/sim/before_us_sim.py`. Chọn sim ở đây là lật ngược thứ tự ưu tiên của chính dự án, việc chỉ làm khi Khoa ra lệnh minh thị.
3. **Câu đó của §8 có chủ đích.** Bảng §8 có một đối xứng rõ: Cơ hội combat là **tùy chọn** nên giết được cả bầy (tự chọn đánh thì chịu); Phòng thủ là **bắt buộc, không né được** nên không được thành cú thua tức thì không tránh nổi. `sim.py:108` chỉ là giản lược, giống cách sim giản lược chỉ số thành viên và bỏ qua nút tùy chọn.
4. **Không mở cửa thoát.** Chừa 1 người không làm ván bất tử: độ khó kéo hái lượm về 0 (`gather.floor = 0`, design.md §5.1) thì người sống sót chết đói ngay lượt sau. §11 "độ khó tăng dần tự kết thúc những ván ở lì" vẫn nguyên hiệu lực — chỉ đổi cách ván kết thúc, không đổi việc nó kết thúc.
**Phải đo ở C8:** thua Phòng thủ khi `quota >= sĩ số` xảy ra bao nhiêu phần trăm số ván, và ước lượng nó dịch tỉ lệ thắng bao nhiêu điểm.
> **Nguồn: Supervisor áp dụng tiền lệ D1, không phải Khoa ra quyết định mới.** Khoa có thể lật bất cứ lúc nào — khi đó là lật từ bản đúng-luật sang bản khớp-sim, chứ không phải ngược lại.
> **Đính chính 2026-11-08 sau playtest 01 (do góp ý 8 của Khoa, Khoa bác được).** Số người chừa lại đổi từ **1** thành **2**, và không còn viết cứng — nó bằng `lose_when_crew_at_or_below + 1`, phơi ra ở `BalanceConfig.MinSurvivorsAfterForcedCombat`.
> **Vì sao buộc phải đổi:** góp ý 8 hạ ngưỡng thua xuống "còn 1 người". Giữ nguyên "chừa 1" thì chừa lại đúng 1 người **chính là** thua — tức một trận *không né được* lại thành cú thua tức thì, đúng thứ D11 sinh ra để ngăn. Ý định của §8 không đổi; chỉ con số phải theo ngưỡng thua. Tính ra thay vì viết cứng nên nếu Khoa đổi ngưỡng lần nữa thì D11 tự theo.

### D12 — HOÃN: sự kiện không combat loại "ngẫu nhiên" chưa được hoãn sang đầu lượt sau
> ⏳ **Đây là món nợ, không phải việc đã xong.** `design.md` §2 bước 1 CHƯA được thực thi. Gỡ ở I2.
**Đây không phải quyết định đã chốt. Đây là quyết định được HOÃN tới I2, vì `design.md` §8 thiếu dữ liệu để quyết.**

`design.md` §8 mô tả hai loại sự kiện không combat: loại **lựa chọn** hiện ngay ở lượt máy, loại **ngẫu nhiên (không cần chọn)** phải "được hiện và áp dụng ở **đầu lượt người chơi kế tiếp**". Nhưng §8 chỉ cho một con số chung là **40%** cho cả nhóm, **không cho biết trong 40% đó bao nhiêu phần là loại nào**. Không có tỉ lệ thì không cài được mà không tự bịa một con số.

**Hiện trạng:** mọi sự kiện không combat giải quyết ngay ở lượt máy, đúng như `before_us_sim.py:103`.

**⚠ Món nợ còn treo:** `design.md` §2 bước 1 — *"Đầu lượt người chơi: áp dụng sự kiện ngẫu nhiên (không cần chọn) đã được rút ở lượt máy trước, nếu có"* — **CHƯA ĐƯỢC THỰC THI**. `ITurnEvents.ApplyPendingAtTurnStart` đã có sẵn chỗ cắm nhưng thân rỗng có chủ đích. Đừng đọc nhầm là đã xong.

**Gỡ nợ ở I2:** khi `events.json` có nội dung thật, mỗi sự kiện tự khai loại của nó. Tỉ lệ trở thành **hệ quả của nội dung Khoa viết**, không ai phải bịa ra con số nào. Lúc đó mới cài thân cho `ApplyPendingAtTurnStart`.

**Vì sao xử khác D1 và D11.** D1 và D11 là ca *`design.md` có quy tắc rõ, xung đột với `sim.py`* ⇒ theo `design.md`. D12 là ca *`design.md` thiếu một con số* ⇒ không có gì để mà theo. Áp máy móc tiền lệ D1 vào đây sẽ thành tự đặt ra một tỉ lệ rồi gọi nó là "theo luật". Hai loại xung đột khác hẳn nhau và phải xử khác nhau.
> **Nguồn: Supervisor, không phải Khoa ra quyết định mới.** Khoa có thể chốt một tỉ lệ bất cứ lúc nào để cài trước I2.

### D13 — Săn bắt và Xây dựng mở từ lượt 3, bỏ hoàn toàn điều kiện nút neuron
**Khoa quyết**, nguyên văn: *"ý tưởng đó nên đổi lại, nó mặc định có ở lượt 3 luôn"*.

Khác D11 và D12 (vốn là Supervisor áp dụng tiền lệ hoặc hoãn quyết định), **D13 là Khoa đổi luật**. Nó thay thế D1.

**Lý do thiết kế:** lượt 3 cũng là lượt trận combat hướng dẫn (design.md §8) — người chơi học đánh rồi được đi săn ngay.

**Lý do kỹ thuật, đo được (C8, trước D13):** dưới D1, bot balanced thắng **0,0%** trên 2.000 ván, **100% thua vì đói**. Tắt riêng D1 thì lên **31,1% ± 1,0**. D1 một mình chịu trách nhiệm ~31 điểm %. Cơ chế: hang chứa tối đa 6 người, nâng hang cần Xây dựng, mà Xây dựng khóa sau `s2_stone_tools`; bộ tộc kẹt 6 người suốt giai đoạn 1 rồi chết đói khi độ khó kéo hái lượm về 0.

**Đã áp vào file nào:**
- `Assets/Data/balance.json` — `activities.build.unlock` và `activities.hunt.unlock`: `"node:s2_stone_tools"` / `"node:s2_scavenge_meat"` → **`"turn:3"`**. Đúng hai chuỗi, không khóa nào khác đổi.
- `docs/design.md` §5 — đúng hai ô cột "Mở khóa" + một dòng ghi chú dưới bảng. Ngoại lệ hẹp với quy tắc "không agent nào sửa design.md", để design.md không mâu thuẫn balance.json.
- `Assets/Data/neurons.json` — `effect_text` của hai nút đã sửa; câu cũ ("Mở khóa hoạt động Xây dựng") sau D13 là lời nói dối hiện trên màn hình người chơi.

**⚠ MÓN NỢ D13 ĐỂ LẠI — hai nút cổng mất việc.** `s2_stone_tools` (28 neuron) và `s2_scavenge_meat` (42 neuron) **vẫn là nút cổng bắt buộc** tính vào 250 neuron của giai đoạn 2, nhưng **hiện không làm gì cả**. Người chơi trả 70 neuron cho hai nút rỗng. Chờ Khoa đặt hiệu ứng mới.

**⚠ Hệ quả thứ hai, chưa quyết.** `combat_results.hunt_food_per_member` là `[0, 5, 7, 10]` — giai đoạn 1 cho **0 thức ăn**. Số đó đặt khi săn còn khóa ở giai đoạn 2 nên ô đầu chưa bao giờ dùng tới. Dưới D13, suốt giai đoạn 1 đi săn **thắng cũng không được miếng nào**, chỉ được 6 neuron/người đổi lấy 2 năng lượng và rủi ro chết. Khoa sẽ tự chơi rồi quyết; **chưa sửa**.

**Số liệu C8 hiện có đã lỗi thời.** Toàn bộ bảng trong báo cáo C8 đo phiên bản luật *trước* D13. Giữ lại để tham khảo, nhưng đừng dùng chúng để kết luận về luật hiện tại.

### D14 — Ba thay đổi luật từ playtest 01
> ✅ **Còn hiệu lực.** ⚠ Giá trị `morale.on_member_death = −2` là **[ĐỀ XUẤT] của Supervisor**, chờ Khoa duyệt.
**Khoa chơi thật rồi góp ý**, 2026-11-08. Nguyên văn 8 góp ý: `docs/playtest-01.md`. Ba mục dưới đây là phần đã thi hành; góp ý 5, 6, 7 là hệ thống mới và **Khoa đang quyết phạm vi**.

**D14a — Mất tinh thần khi có thành viên chết.** Góp ý 1. Thêm khóa `morale.on_member_death` vào `balance.json`. Áp cho **mọi** cái chết: đói, thua combat, quái thú ăn. Cài ở `TurnSystem.RemoveMember` có chủ đích — mọi cái chết đều đi qua đúng hàm đó, nên không đường nào lọt.
> ⚠ Giá trị **−2 là [ĐỀ XUẤT] của Supervisor, KHÔNG phải số của Khoa.** Chọn cùng độ lớn với `combat_results.morale_on_loss` đang có để không đặt ra một thang đo mới. **Khoa phải duyệt.**

**D14b — Thua khi còn 1 người, không còn là "hết sạch người".** Góp ý 8, nguyên văn: *"khi cộng đồng chỉ còn 1 người thì kết thúc game"*. Thêm khóa `lose_when_crew_at_or_below: 1`. `design.md` §1 đã sửa đúng câu điều kiện thua. `TurnOutcome` có `Lost` (điều kiện thua thật) và giữ `TribeWipedOut` (hết sạch người) như một thông tin hẹp hơn.
> Khóa `lose_when_crew_at_or_below` là **phán đoán của Lead Core**: Supervisor chỉ yêu cầu đổi điều kiện, không nói cất số ở đâu. Viết cứng số 1 trong Core sẽ vi phạm `AGENTS.md` ("mọi con số cân bằng đọc từ JSON"), nên nó vào `balance.json`.

**D14c — D11 chừa lại 2 người thay vì 1.** Hệ quả bắt buộc của D14b. Chi tiết ở phần đính chính của D11 ngay trên.

**Phơi dữ liệu cho nhánh UI (góp ý 3), không đổi luật:** `TurnSystem.PreviewCombat(activity)` trả `CombatPreview { PartySize, FoodIfWon, WinChance }` — truy vấn thuần, không tác dụng phụ. `WinChance` lấy từ `ICombatOdds` đã có, **không có bản cài công thức thứ hai**. Core trả dữ liệu thô, không quy ra thang "dễ / vừa / khó" — cách diễn đạt là quyền của Khoa.

**Không đổi:** `hunt_food_per_member[0]` vẫn là **0** (săn giai đoạn 1 thắng cũng không ra thức ăn). Khoa sẽ tự chơi rồi quyết.

### D15 — Cả ba hệ thống mới của playtest 01 vào MVP
**Khoa quyết**, 2026-11-08. Góp ý 5 (hái lượm có khám phá thực vật), 6 (màn hình cộng đồng với công trình theo slot) và 7 (thao tác bằng cách bấm vào vật thể) đều **vào phạm vi MVP**, không hoãn sau 28/11.

> **Ghi chú đánh số:** Supervisor yêu cầu ghi mục này là "D14", nhưng **D14 đã được dùng** ở cùng ngày cho ba thay đổi luật từ playtest (D14a/b/c). Để tránh hai mục cùng số, mục này là **D15**. Nội dung quyết định không đổi.

**⚠ Mốc ràng buộc: 15/11/2026 — màn hình cộng đồng phải chơi được.**
Lý do: playtest P1 với 5 người là **16–18/11** (`HANDOFF.md` tuần 2). Không thể để người lạ chơi thử một màn hình đang dở.

**Hệ quả lên lịch (D5).** Ba hệ thống này thay §5, §5.1 và toàn bộ A1 — tức thay phần lớn thứ đã dựng xong ở C4 và A1. Quỹ thời gian từ 08/11 tới 15/11 là **7 ngày**, trong đó phải: Khoa duyệt đặc tả, viết lại luật hái lượm và Xây dựng trong Core, dựng lại màn chính, và làm art cho các vật thể tương tác. `HANDOFF.md` §1 nói rõ nếu trễ thì **giữ ngày phát hành, cắt hình ảnh và nội dung** — không lùi lịch.

**Việc đầu tiên là đặc tả, không phải code.** Góp ý 5 và 6 hiện là văn xuôi; code thẳng từ đó buộc agent phải tự bịa vài chục con số và tên gọi, đúng thứ `AGENTS.md` cấm và `HANDOFF.md` §6 dành riêng cho Khoa. Bản nháp đặc tả: `docs/design-v7-draft.md` — **đề xuất để Khoa sửa rồi duyệt**, chưa phải luật.

### K1 — GHI NHẬN KHUÔN MẪU (không phải quyết định): phần thưởng của một hệ thống không được nằm sau cánh cửa mà chính hệ thống đó phải mở

Dự án đã vấp **hai lần** vào cùng một hình dạng lỗi. Ghi lại để lần thứ ba được nhận ra trong năm phút thay vì sau một vòng đo.

**Lần 1 — D1 (bỏ bởi D13).** Hoạt động thu nhập cao nhất (Săn bắt, 6 neuron/người) bị khoá sau một khoản phí 70 neuron chỉ trả được bằng hoạt động thu nhập thấp nhất (Kiếm ăn, 1 neuron/người). Đo được: bot balanced thắng **0,0%** trên 2.000 ván, 100% thua vì đói.

**Lần 2 — hái lượm v7, 2026-11-09.** Hệ khám phá thực vật chỉ trả thưởng khi đã khám phá được nhiều loài, nhưng loài khởi đầu (`la_non`, dinh dưỡng 1) nghèo tới mức cả bộ tộc phải đi hái lượm từ lượt 1 — nên không bao giờ sống đủ lâu để khám phá. Đo được: ván chết quanh **lượt 19**, chỉ 4/10 ván lên nổi giai đoạn 2. Sửa bằng `la_non.nutrition` 1 → 3 (khôi phục đúng mức v6): lên 8/10 và lượt 23.

**Cùng một hình dạng:** *phần thưởng của hệ thống nằm sau một cánh cửa mà chính hệ thống đó phải mở.*

**Phép kiểm bắt buộc khi thêm bất kỳ hệ tiến triển nào:** trạng thái KHỞI ĐẦU phải sống sót được **mà không cần tới hệ đó**. Nếu người chơi phải dùng hệ thống để mở được hệ thống, thì nó đã hỏng — và nó sẽ chỉ lộ ra sau một vòng đo tốn kém, vì mọi test đơn lẻ vẫn xanh.

> Nguồn: Supervisor, 2026-11-09. Đây là ghi nhận kỹ thuật, không phải luật chơi, nên không mang số hiệu D.
---

*Ghi chú nguồn: cả D1–D10 nay đều có diễn giải đầy đủ từ Khoa. D6–D9 ban đầu tới tay Lead Core dưới dạng tóm tắt một dòng qua Supervisor và đã được bổ sung câu chữ gốc ngày 2026-11-08; phần "Lý do" của bốn mục đó hiện là bản đầy đủ, không phải bản Lead Core tự suy.*

### D16 — Khung đảm bảo hái lượm tính theo TỔNG nhanh nhẹn của cả nhóm

**Ai quyết:** Khoa, 2026-11-09, trả lời câu E2 của bản nháp v7.

> *"Giảm yêu cầu của các cây có thể tìm thấy, ví dụ cây yêu cầu có tổng nhanh nhẹn là 6 thì mới chắc chắn tìm thấy thì giảm còn 5.8 hoặc 5.5 kiểu kiểu vậy"*

Mỗi loài thực vật có một ngưỡng `guaranteed_total_speed`. Nếu TỔNG nhanh nhẹn của nhóm được cử đi đạt ngưỡng đó thì nhóm **chắc chắn tìm thấy loài đó ít nhất MỘT lần** trong lượt.

**Đây là lời đính chính một lỗi đọc của chính tôi.** Bản cài Phần A đầu tiên đặt một ngưỡng duy nhất trong `balance.json` (`guaranteed_find_speed = 1.00`) và áp cho TỪNG NGƯỜI, nên mỗi người đủ nhanh đều mang về một lần tìm thấy. Theo cách đọc đúng thì sàn thức ăn thấp hơn hẳn: `n` người không còn cho `n` lần đảm bảo, mà cho đúng một lần mỗi loài.

Đã sửa: `balance.json` bỏ `guaranteed_find_speed`; `plants.json` thêm `guaranteed_total_speed` cho cả 8 loài (toàn bộ [ĐỀ XUẤT]).

### D17 — Ổ đựng thức ăn nâng cấp thì HẠ ngưỡng khung đảm bảo

**Ai quyết:** Khoa, 2026-11-09, cùng câu trả lời E2.

Bản nháp v7 §B.4 từng đề xuất hai cách đọc "giảm chi phí đi hái lượm" và cài **cả hai** ở hai bậc khác nhau: bậc 1 giảm nhân lực (sàn 1/3 → 1/4), bậc 2 giảm năng lượng (1 → 0). **Cả hai đều bị bỏ**, không giữ làm tuỳ chọn. Thang bậc mới, toàn bộ [ĐỀ XUẤT], nằm ở `Assets/Data/sites.json`:

| Bậc | Sức mạnh cần | Nới ngưỡng |
| --- | --- | --- |
| 1 | 2.0 | +0.2 |
| 2 | 4.0 | +0.5 |
| 3 | 7.0 | +1.0 |

Cài bằng cách **cộng vào tổng nhanh nhẹn của nhóm** thay vì trừ vào ngưỡng của từng loài. Cùng một hiệu lực, nhưng chỉ có một chỗ để sai.

### D18 — `build_level` thành số suy ra, `build_points` chết

**Ai quyết:** Agent (Core), theo D7 của Khoa và §B.2 của bản nháp v7. **Khoa có thể lật.**

`build_level` = số slot dọn sẵn của vùng + số slot đã dọn + bậc Ổ đựng thức ăn. `GameState.BuildLevel` **không còn setter**.

Ba công thức ngoài Xây dựng giữ nguyên từng chữ: sức chứa hang `6 + 2 × build_level` (§4), tỉ lệ thắng Săn/Khám phá `+0.05 × build_level` (§5.2), Phòng thủ `+0.06 × build_level` (§8).

Chết theo: `balance.json` khóa `activities.build.points_to_level_factor`, `GameState.BuildPoints` (còn lại một thuộc tính chỉ-đọc trả 0 để nhánh UI chưa gỡ kịp vẫn biên dịch được), trường `build_level` và `build_points` trong file lưu ván. **File lưu lên phiên bản 3.**

### D19 — Xây dựng giải quyết CUỐI lượt

**Ai quyết:** Agent (Core), theo D7 của Khoa. **Khoa có thể lật.**

D7 nói *"lượt sau slot đó sẽ được dọn"*. `TurnSystem.ResolveOrder` đổi từ `{Kiếm ăn, Tinh thần, Xây dựng, Săn, Khám phá}` thành `{Kiếm ăn, Tinh thần, Săn, Khám phá, Xây dựng}`.

Hệ quả: slot vừa dọn **không** nâng tỉ lệ thắng trận của chính lượt đó, cũng **không** nới sức chứa hang kịp cho lần sinh cuối lượt. Đây là đảo ngược một lựa chọn cũ của Core — lý do cũ ("cấp công trình vừa lên có hiệu lực ngay") đã bị D7 xoá.

### D20 (BẢN ĐẦU — 🚫 ĐÃ BỊ THAY) — Slot thuộc về vùng, nhưng chưa nối vào di cư

> 🚫 **Mục này đã bị thay bởi "D20 (viết lại)" ở cuối file.** Bản đầu hiểu "slot" là ô đất đặt
> công trình và hoãn phần di cư; cả hai đều sai. Giữ lại để thấy cái gì đã bị thay, không phải
> để đọc như luật hiện hành.

**Ai quyết:** Khoa quyết phần luật, 2026-11-09; Agent quyết phần hoãn.

> *"Mỗi vùng sẽ có giới hạn số slot, ví dụ khu vực 1 chỉ có 8 slot, khu vực 2 sẽ là 14 slot, khi thực hiện khám phá vùng mới, sẽ phải chọn ra tầm 4-6 người mang theo qua khu vực mới, khu vực mới sẽ reset lại số slot và có số slot đã mở khởi đầu khác nhau."*

`sites.json` đã theo đúng cách đó: mỗi vùng một danh sách slot riêng, một `slot_cap` và một `cleared_at_start`. Nhưng **Core chưa reset slot khi đổi vùng**, vì ba câu hỏi còn mở (xem `design-v7-draft.md` Phần G): ai ở lại khi chỉ mang 4–6 người, bộ tộc dưới 4 người có di cư được không, và chia đôi bộ tộc thì điều kiện thua tính theo nhóm nào.

Hệ quả tạm thời, ghi rõ để không ai tưởng là lỗi: di cư **không** làm `build_level` tụt, và `cleared_at_start` của vùng 1 (= 2) **chưa có tác dụng**.

### D21 — Sinh vật lịch sử, kiểu nhắm "Bắt kẻ lẻ loi", trạng thái trong trận, hồ sơ lịch sử và thành tựu

> ✅ **Trùng số đã gỡ, 2026-11-09.** Mục kia ("Di cư: một chiều…") đã đổi thành **D25**. Số D21 nay chỉ thuộc về mục này, và `bestiary.md`, `design.md` §9, `HANDOFF.md` §7 vẫn trỏ đúng — không file nào phải sửa.

**Ai quyết:** Khoa, 2026-11-09. Nguyên văn:

> *"rất thú vị, bổ sung thêm nhiều sinh vật và thông tin nữa, tôi muốn những sinh vật này sau khi giao chiến thì sẽ hiện thành tựu và chi tiết về sự kiện trong lịch sử cho người chơi tìm hiểu. cơ chế tranh mồi thì để vào mục phát triển, chưa vội tham, cơ chế bắt kẻ lẻ loi thì hợp lí, cho vào luôn phát triển hiện tại. trạng thái của quái thú rất thú vị, giảm nhàm chán cho combat, xem xét đưa vào luôn. Tất cả viết vào tài liệu cho tôi để agent đọc và thiết kế"*

Khoa quyết:
- Kiểu tấn công thứ tư **"Bắt kẻ lẻ loi"** (`isolated`) vào MVP.
- **Sau mỗi trận hiện hồ sơ lịch sử và thành tựu** vào MVP.
- **Trạng thái quái thú** được xem xét đưa vào MVP, theo bậc ưu tiên.
- **Tranh mồi** để sau MVP.

Đặc tả đầy đủ nằm ở `docs/bestiary.md`. Mọi con số, danh sách loài cho MVP (thêm trăn; đại bàng nếu kịp) và chữ hiện cho người chơi đều là **[ĐỀ XUẤT]**, chờ Khoa trả lời các câu hỏi ở `bestiary.md` §9.

**Không đổi:** `balance.json`, hợp đồng `CombatResult`, mốc 15/11 của D15, ngày phát hành 28/11. Nếu thiếu thời gian, cắt theo thứ tự ghi ở `bestiary.md` §7. Không được cắt kiểu nhắm `isolated` và thẻ hồ sơ sau trận.

> Nguồn: ghi bởi Claude (phiên thiết kế với Khoa), 2026-11-09. Số hiệu D21 vì D16–D20 đã được dùng trong cùng ngày.

**D21a — Khoa duyệt toàn bộ đề xuất của `bestiary.md` §9.** 2026-11-09, 01:07. Nguyên văn: *"theo đề xuất của bạn hết"*.

Kết quả:
- Thêm trăn (rừng, kiểu `isolated`) và đại bàng (savan, MVP-2, cắt đầu tiên nếu trễ).
- Giữ báo hoa mai ở rừng; codex ghi rõ sai niên đại.
- Thú Bỏ chạy khỏi trận Phòng thủ tính là người chơi thắng.
- Hồ sơ và thành tựu lưu xuyên ván, không cho thưởng gameplay.
- Bản đầy đủ đi theo bảng vùng ở `bestiary.md` §8.3.
- Giữ giọng văn codex.

Chỉ số của các loài và câu chữ cụ thể vẫn là **[ĐỀ XUẤT]**.

### D20 (viết lại) — "Slot" là CHỖ CHO NGƯỜI, không phải ô đất đặt công trình

**Ai quyết:** Khoa, 2026-11-09.

> *"vùng 1 có tối đa 8 chỗ, khởi đầu là 4 chỗ, cần phải thực hiện dọn chỗ cho 4 chỗ còn lại, tương tự với các vùng khác"*

Mỗi vùng có một **trần chỗ ở** và một **số chỗ mở sẵn**. Dọn một công trình mở thêm **đúng một chỗ** ([ĐỀ XUẤT]), tới trần của vùng. Nên số công trình của một vùng bằng đúng `trần − mở sẵn`; `SiteCatalog` nổ lúc nạp nếu lệch.

Công thức `sức chứa = 6 + 2 × build_level` của `design.md` §4 **chết**; khối `lair` biến khỏi `balance.json`, thay bằng bảng theo vùng trong `sites.json`.

`build_level` vẫn tồn tại cho §5.2 và §8, và vẫn là số suy ra (D18) — nhưng nay **chỉ đếm công trình của vùng hiện tại**, nên di cư làm nó tụt về 0.

> Bản D20 đầu tiên (viết sáng 2026-11-09) hiểu "slot" là ô đất đặt công trình và hoãn phần di cư. Cả hai đều sai; mục này thay hẳn nó.

### D25 — Di cư: một chiều, và người ở lại mất hẳn

> ✏️ **Đổi số D21 → D25 ngày 2026-11-09.** Mục này từng mang số D21 và trùng với
> "D21 — Sinh vật lịch sử…" của nhánh Combat, ghi cùng ngày. Đổi số MỤC NÀY vì mục kia
> đã được `bestiary.md`, `design.md` §9 và `HANDOFF.md` §7 tham chiếu từ bên ngoài;
> đổi nó sẽ làm hỏng bốn chỗ trỏ tới. Số D21 **không** được dùng lại cho mục này nữa.

**Ai quyết:** Khoa, 2026-11-09, ba câu trả lời.

> *"người ở lại mất luôn, đây là cơ chế giảm sức mạnh người chơi ở khu vực cũ, qua khu vực mới thì họ phải bị giảm nhân lực để đối mặt với các khó khăn mới."*
> *"Miễn khu vực mới có đủ slot cho số người di cư thì bao nhiêu người trong bộ tộc đều được lựa chọn đi được."*
> *"người chơi chỉ quan tâm đến cộng đồng chính, cộng đồng khi di cư bị chia ra thì cộng đồng ở vùng đất mới chính là cộng đồng chính, nó là lối đi 1 chiều không có đường trở lại"*

Năm hệ quả đã cài:

1. `region` **chỉ tăng**. Không có đường về, và không để dành chỗ cho nó trong thiết kế.
2. Người ở lại bị **xoá khỏi `GameState`** — không lưu, không theo dõi. Họ **không phải người chết**: không trừ tinh thần theo `morale.on_member_death`, không vào `DeadMemberIds`.
3. **Không có giới hạn 4–6.** Con số đó trong tin nhắn của Khoa là ví dụ. Ràng buộc thật là **số chỗ vùng mới mở sẵn**.
4. Trạng thái chỗ của vùng (`region_capacity_open`, `region_capacity_max`) **vào file lưu**, cùng `cleared_sites` và `food_nest_tier`. File lưu lên **phiên bản 3**.
5. Giao với ngưỡng thua `≤ 1` (D14b): mang 2 người đi, để lại 5 — bộ tộc còn 2, một cái chết nữa là thua. **Di cư là nước đi nguy hiểm thật, và không được làm mềm nó.** Thay vào đó Core phơi `TurnSystem.PreviewMigration()` để người chơi thấy trước khi bấm.

### D22 — Khoa ủy quyền chỉnh cân bằng

**Ai quyết:** Khoa ủy quyền cho Supervisor, 2026-11-09.

> *"cái này bạn tùy ý chỉnh sửa nha, cân bằng sao cũng được, khởi đầu 4 người cũng được chẳng sao"*

**Được chỉnh:** con số cân bằng. **Không được chỉnh:** luật và nội dung — cơ chế, cách một hệ thống vận hành, tên loài, câu mô tả hiện cho người chơi.

Điều kiện kèm theo, do Supervisor đặt: **mọi lần chỉnh phải có một dòng trong `docs/balance-log.md`** với tên khóa, giá trị cũ → mới, lý do, và phép đo biện minh. `HANDOFF.md` §8 gọi rủi ro số một là *"agent âm thầm đổi luật"*; thứ duy nhất phân biệt "được ủy quyền chỉnh" với "âm thầm đổi" là có nhìn thấy được hay không.

> ⚠ Quy tắc này **đã bị vi phạm ngay ngày đầu**: `starvation_deaths` và `find_chance` của cả 8 loài được đổi rồi mới ghi, và Supervisor phát hiện bằng cách tự đọc file. Quy tắc sửa lại: **ghi dòng log trong cùng lượt với lúc sửa số.**

### D23 — Tiêu chí "không chết sớm", ngang hàng với dải Cổng 2

**Ai quyết:** Supervisor, 2026-11-09. Ngưỡng là [ĐỀ XUẤT].

Bot balanced **thua trước lượt 10 không quá 10%** số ván.

> Dải Cổng 2 là điều kiện **cần**, không phải điều kiện **đủ**. Một cấu hình thắng 35% mà một phần ba số ván chết trước lượt 5 thì vẫn hỏng: người chơi thua trước khi kịp hiểu luật, trong khi `design.md` §1 đặt cảm xúc mục tiêu là *"thua thì bực, muốn chơi lại"*.

Kéo theo một luật về **hướng chỉnh**: kéo tỉ lệ thắng xuống bằng những đòn **cắn về sau** (bào mòn độ khó, khẩu phần giai đoạn 2+, chi phí tiến hóa, trần sức chứa, quota phòng thủ), **không** bằng cách tăng sát thương của những cú trừng phạt sớm.

Phát hiện ra tiêu chí này nhờ Lead Combat/UI chạy hạt 12345: bộ tộc tuyệt diệt ở lượt 3 với `starvation_deaths = 2`. C8 nay in hai cột `<5`, `<10` và một khối nguyên nhân thua tách riêng cho 10 lượt đầu.

### D24 — Hoạt ảnh sinh vật làm bằng gắn xương (2D skeletal rigging), không sinh ảnh theo từng động tác

**Ai quyết:** Khoa, 2026-11-09, 01:38. Nguyên văn:

> *"hoạt ảnh trên game nên để gắn xương cho rigging chứ không nên tạo nhiều ảnh chỉ cho 1 thao tác, gắn xuong thì lúc đó muốn thao tác hoạt ảnh gì cũng được"*

Cách làm cho mỗi sinh vật:
- Sinh **một hình gốc** ở tư thế để rig, và **một bộ mảnh cơ thể**.
- Ghép các mảnh thành file `.psb`, rồi gắn xương trong Unity.
- Mọi trạng thái trong trận (`bestiary.md` §4) là **đoạn hoạt ảnh bằng xương**.

Quy trình, khung xương mẫu và prompt nằm ở `docs/prompt.md` §2.

**Không thêm package.** Dự án đã có `com.unity.feature.2d`, bộ này kéo theo `com.unity.2d.animation` 16.0.1 và `com.unity.2d.psdimporter` 15.0.1 (xem `Packages/packages-lock.json`).

**Không đổi:** art và hoạt ảnh theo khung hình Khoa đã làm trong `Assets/Sprite/**` (D3).

Ngoại lệ đã biết: động tác Siết của trăn cần thêm **một mảnh vòng quấn**, vì một lưới hình không vẽ được chỗ thân đè lên chính nó.

### D26 — C7 dùng chung bot của C8, bỏ hẳn `TestPlayer`

> ✏️ **Đổi số D24 → D26 ngày 2026-11-09.** Mục này từng mang số D24 và trùng với
> "D24 — Hoạt ảnh sinh vật bằng gắn xương" của Khoa. Cùng quy tắc đã dùng cho D21/D25:
> **giữ số của Khoa, đổi số của mình** — mục kia được `prompt.md` trỏ tới từ bên ngoài.
> Số D24 **không** được dùng lại cho mục này nữa.

**Ai quyết:** Supervisor, 2026-11-09, sau khi tôi tự khai món nợ.

C7 (lưu/tải ván) từng có driver riêng, `TestPlayer`. Sau khi v7 đổi sức chứa hang thành "chỗ theo vùng", chính sách của nó hỏng: chết quanh lượt 16, **0/10 ván di cư được** — trong khi bot balanced của C8 thắng 33,2% với trung vị 51 lượt.

**Hệ quả nguy hiểm, và là lý do thật của quyết định này:** 0/10 ván di cư nghĩa là **lưu–tải qua một lần di cư chưa từng được kiểm**. Mà di cư (D25) là thao tác **một chiều, phá huỷ, xoá người khỏi bộ tộc vĩnh viễn**. Một lỗi ở đó không làm game văng — nó làm người chơi tải ván lên và thấy một bộ tộc sai, hoặc thấy lại những người lẽ ra đã mất.

> *"Đừng vá chính sách của driver cũ — một driver mà chính sách khác hẳn bot thật thì mọi thứ nó kiểm đều không đại diện cho ván thật."*

Đã làm: `BotGame.Resume(config, options, state, random)` + `PlayTurns` + `Fingerprint`; `TestPlayer.cs` **xoá hẳn**; thêm bộ `SaveMigrationTests` kiểm chuỗi **lưu → tải → di cư → lưu → tải**.

Hồ sơ driver C7, trước và sau:

| | TestPlayer (cũ) | BotGame balanced (nay) |
| --- | --- | --- |
| tới giai đoạn 2 | 0/10 | **8/10** |
| di cư | **0/10** | **8/10** |
| nút mua được TB | 3,5 | **9,4** |
| công trình dọn TB | — | **1,7** |
| số lượt TB | 12,7 | **35,1** |

Một bẫy đếm đã gặp khi làm: `GameState.ClearedSiteIds` **reset mỗi lần di cư**, nên đếm ở cuối ván thì một ván dọn 3 công trình rồi di cư trông y như ván chưa dọn gì. `BotGame.SitesClearedEver` đếm cả ván.

### D27 — Dừng trước D5 catalog, có chủ đích

**Ai quyết:** Supervisor, 2026-11-09, sau khi agent hỏi ngược lại bằng chính lập luận cũ của Supervisor.

D5 (catalog thực vật xuyên ván) là mục áp chót trong thứ tự thi hành đã khoá. Nó **không được dựng**, và đó là quyết định chứ không phải bỏ sót.

Hai lý do:

1. **Catalog là hệ thống duy nhất nằm ngoài ván chơi.** Khoa: *"tôi cần thấy sản phẩm và tôi tự chơi thử"*. Một bộ sưu tập xuyên ván không đóng góp gì cho câu đó, và P1 còn 7 ngày.
2. **Hai quyết định thiết kế đang treo đều nằm TRONG ván chơi**, đều đã có số, đều chờ Khoa: ổ thức ăn bậc 2–3 bị trội hoàn toàn, và hiệu ứng nút đáng +16 điểm %. Dựng catalog trong lúc đó là làm việc dễ thay vì việc đúng.

Thứ tự thi hành ban đầu đặt D5 áp chót và được chạy theo quán tính cho tới mục cuối. Câu hỏi "thứ tự ấy còn đúng không" là thứ người đặt ra nó khó tự hỏi mình — nên nó được hỏi từ phía thi hành.

**Bàn giao đầy đủ của nhánh Core: `docs/handover-core.md`.** Trạng thái, món nợ, ba thứ chờ Khoa, năm cạm bẫy có tên, và hai bài học về phương pháp.
