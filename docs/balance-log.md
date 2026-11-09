# Nhật ký chỉnh cân bằng — Before Us

Mỗi dòng là **một con số** đã đổi trong game của Khoa. Cột "phép đo" là thứ biện minh cho việc đổi; không có phép đo thì ghi thẳng **"chưa có"**, không dựng lý do hồi tố.

## Vì sao có file này

Ngày 2026-11-09 Khoa ủy quyền chỉnh cân bằng: *"cái này bạn tùy ý chỉnh sửa nha, cân bằng sao cũng được, khởi đầu 4 người cũng được chẳng sao"*.

`HANDOFF.md` §8 gọi rủi ro số một của dự án là **"agent âm thầm đổi luật"**. Thứ duy nhất phân biệt "được ủy quyền chỉnh" với "âm thầm đổi" là **có nhìn thấy được hay không**. File này là chỗ nhìn thấy.

> ⚠ **Nó đã trượt ngay ngày đầu.** Hai con số (`starvation_deaths`, `find_chance` của cả 8 loài) được đổi rồi mới ghi, và Supervisor phát hiện bằng cách tự đọc file chứ không phải qua báo cáo. Quy tắc sửa lại: **ghi dòng log trong cùng lượt với lúc sửa số**, không để sang bước sau.

## Ranh giới — Supervisor vạch ngày 2026-11-09

**ĐƯỢC chỉnh (con số cân bằng):** `start.crew`, dinh dưỡng và tỉ lệ tìm thấy từng loài, ngưỡng sức mạnh dọn công trình, số chỗ mỗi vùng, hệ số bào mòn độ khó, giá nút neuron, `hunt_food_per_member`, `morale.on_member_death`, `upkeep_per_member`, `starvation_deaths`.

**KHÔNG được chỉnh (luật và nội dung):** cơ chế mới, cách một hệ thống vận hành, tên loài, câu mô tả hiện cho người chơi.

Gặp thứ **trông như con số nhưng thực ra là luật** — ví dụ "một công trình mở mấy chỗ", "số loài biết sẵn lúc bắt đầu" — thì **hỏi Supervisor**.

## Đích — hai tiêu chí NGANG HÀNG

**1. Dải Cổng 2 (`HANDOFF.md`):** balanced thắng **30–45%**, trung vị **42–52 lượt**; safe ≤ 2%; reckless ≤ 20%; careless ≤ 20%. Đo ở `applyNodeEffects = false`, 2.000 ván/bot, SE ≈ 1,1 điểm %.

**2. Không được chết sớm [ĐỀ XUẤT], Supervisor đặt 2026-11-09:** bot balanced thua **trước lượt 10 không quá 10%** số ván.

> Dải Cổng 2 là điều kiện **cần**, không phải điều kiện **đủ**. Một cấu hình thắng 35% mà một phần ba số ván chết trước lượt 5 thì vẫn hỏng: người chơi thua trước khi kịp hiểu luật, trong khi `design.md` §1 đặt cảm xúc mục tiêu là *"thua thì bực, muốn chơi lại"*, không phải *"chết trước khi thấy được gì"*.
>
> **Hướng chỉnh bắt buộc:** kéo tỉ lệ thắng xuống bằng những đòn **cắn về sau** (bào mòn độ khó, khẩu phần giai đoạn 2+, chi phí tiến hóa, trần sức chứa, quota phòng thủ). **Không** kéo bằng cách tăng sát thương của những cú trừng phạt sớm.

C8 nay in hai cột `<5` và `<10` cùng một khối "nguyên nhân thua TRONG 10 LƯỢT ĐẦU" tách riêng, vì gộp chung với cả ván thì một mở màn chí mạng bị pha loãng mất — đúng cái đã xảy ra ở dòng #7.

Quy trình bắt buộc: **đo trước · chỉnh · đo lại · ghi log cùng lượt**.

---

## Nhật ký

| # | Ngày | Khóa | Cũ → Mới | Ai | Lý do | Phép đo biện minh |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 2026-11-08 | `morale.on_member_death` | (không có) → **−2** | Supervisor | Góp ý 1 của Khoa ở playtest 01: chết người phải có sức nặng. Khoa **chưa duyệt** con số. | **Chưa có.** Đặt theo cảm nhận thiết kế. |
| 2 | 2026-11-09 | `plants.la_non.nutrition` | 1 → **3** | Supervisor | Số 1 là của tôi và nó sai: cả bộ tộc phải hái lượm từ lượt 1 và vẫn chết quanh lượt 19 (mẫu K1). | Driver 10 ván: stage2 **4/10 → 8/10**, di cư 7/10 → 8/10, nút 6 → 9, lượt 19 → 23. ⚠ **Lý lẽ kèm theo đã lỗi thời:** lúc viết tôi biện minh bằng `0.80 × 3 = 3.0` = `gather_yield` v6. Sau dòng #8, `la_non.find_chance` là **0.64**, nên đẳng thức đó không còn đúng (`0.64 × 3 + 0.36 × 3` vẫn = 3.0 nhờ khung đảm bảo, nhưng chỉ khi tổng nhanh nhẹn ≥ 2.0). Con số 3 giữ nguyên; **lý lẽ thì phải đọc kèm dòng này**. |
| 3 | 2026-11-09 | `start.crew` | 5 → **4** | Supervisor, Khoa chuẩn thuận | Khoa chốt vùng 0 **mở sẵn 4 chỗ**. 5 người / 4 chỗ là hai luật loại trừ nhau. Khoa sau đó: *"khởi đầu 4 người cũng được chẳng sao"*. | C8 (cùng lúc với #4): balanced **98,6% → 82,3%**, trung vị **33 → 51**, reckless 44,5% → 2,4%, careless 49,4% → 15,4%. |
| 4 | 2026-11-09 | `lair.capacity_base`, `lair.capacity_per_build_level` | 6, 2 → **xoá khỏi `balance.json`** | Khoa (luật) | Sức chứa tính **theo vùng**; bảng mới ở `sites.json`. Đổi luật của Khoa, không phải tôi chỉnh số. | (xem #3) |
| 5 | 2026-11-09 | `activities.build.points_to_level_factor` | 4 → **xoá** | Khoa (luật) | D7 thay cơ chế điểm xây bằng "ăn cả ngã về không". | — |
| 6 | 2026-11-09 | `gather.guaranteed_find_speed` | 1.00 → **xoá**, thay bằng `plants[].guaranteed_total_speed` | Khoa (luật) | E2: ngưỡng là **tổng nhanh nhẹn cả nhóm**, theo từng loài. Xem `decisions.md` D16. | — |
| 7 | 2026-11-09 | `starvation_deaths` | 1 → **2** → **1 (đã hoàn nguyên)** | Agent (Core) | **GHI MUỘN, và đã bị hoàn nguyên.** Tôi đổi để kéo balanced từ 82,3% về dải. Nó có tác dụng (82,3% → 35,9%) nhưng kéo bằng cách **làm mở màn chí mạng**. | Đo khi đổi: balanced 35,9%, trung vị 51 — nhưng **thua trước lượt 10 = 11,5%**, vượt ngưỡng 10%. Lead UI tìm ra bằng hạt 12345: tuyệt diệt ở lượt 3. Hoàn nguyên về 1, và cùng lúc đó <10 về **5,6%**. |
| 8 | 2026-11-09 | `plants[*].find_chance` (cả 8 loài) | **× 0,80** <br>`la_non` 0.80→0.64 · `qua_sung` 0.55→0.44 · `hat_co` 0.50→0.40 · `cu_mai` 0.40→0.32 · `nam_go` 0.35→0.28 · `qua_bao_bap` 0.25→0.20 · `trung_chim` 0.20→0.16 · `to_ong` 0.15→0.12 | Agent (Core) | **GHI MUỘN.** v7 cho quá nhiều thức ăn: một người hái lượm biết hết loài của vùng 0 mang về kỳ vọng **7,8**, trong khi v6 cho đúng **3**. Nhân chung 0,80 lấy lại một phần và **giữ nguyên thứ tự độ hiếm** giữa các loài, để Khoa vẫn chỉnh tiếp được. | Quét 500 ván/ô: ×1,00 → 85,4% · **×0,80 → 69,2%** · ×0,60 → 58,6% · ×0,50 → 49,6% (trung vị trôi lên 54, ra ngoài dải). |
| 9 | 2026-11-09 | `upkeep_per_member` | [1, 1, 2, 3] → **[1, 2, 2, 3]** | Agent (Core) | Đòn **cắn về sau** đúng nghĩa: giai đoạn 1 giữ nguyên 1, chỉ từ giai đoạn 2 mới nặng lên. Mở màn không đổi một chút nào. | Quét 500 ván/ô, `starvation_deaths = 1`: [1,1,2,3] → 69,2% · **[1,2,2,3] → 54,4%** · [1,3,3,4] → 31,0%. Cột `<5`/`<10` **không nhúc nhích** (2,0% / 5,0%) ở cả bốn ô — bằng chứng rằng khóa này không đụng vào mở màn. Chỉ số ở vị trí giai đoạn 2 có tác dụng; [1,2,2,3] và [1,2,3,3] cho số y hệt vì ván thắng ở giai đoạn 3. |
| 10 | 2026-11-09 | `difficulty.per_level.find_chance_multiplier` | −0.10 → **−0.18** | Agent (Core) | Khóa `upkeep` giai đoạn 2 là **số nguyên** và nhảy quá thô (2 → 54,4%, 3 → 31,0%, không có gì ở giữa). Cần một khóa mịn hơn, cũng cắn về sau, để hạ cánh vào giữa dải. Đây cũng là **câu trả lời cho E4**, câu hỏi mở lâu nhất của bản nháp v7 — nay đo được thay vì đoán. | Quét 500 ván/ô trên nền `upkeep [1,2,2,3]`: −0,13 → 45,4% · −0,15 → 41,6% · −0,16 → 42,6% · **−0,18 → 35,2%**. Cột `<5`/`<10` giữ nguyên 2,0% / 5,0% ở cả bốn ô. |

### Kết quả sau dòng #7–#10 (C8 đầy đủ, 2.000 ván/bot, `applyNodeEffects = false`)

| chỉ tiêu | dải | đo được | |
| --- | --- | --- | --- |
| balanced thắng | 30–45% | **33,2% ± 1,1** | ĐẠT |
| balanced trung vị | 42–52 lượt | **51,0** (p10 48, p90 59) | ĐẠT |
| safe thắng | ≤ 2% | **0,0%** | ĐẠT |
| reckless thắng | ≤ 20% | **0,4% ± 0,1** | ĐẠT |
| careless thắng | ≤ 20% | **7,1% ± 0,6** | ĐẠT |
| balanced thua trước lượt 10 | ≤ 10% | **5,6%** (trước lượt 5: 2,1%) | ĐẠT |

Nguyên nhân thua của balanced trong 10 lượt đầu: **đói 5,6%**, không có nguyên nhân nào khác.

> ⚠ **Còn một cảnh báo chưa xử:** bật `applyNodeEffects = true` thì balanced thắng **52,1%** (trước vòng chỉnh này là 94,3%), vẫn trên mốc 45%. Dải Cổng 2 của `HANDOFF.md` đo ở `false`, nên đây không phải chỉ tiêu trượt — nhưng hiệu ứng nút đang đáng giá **+16 điểm %**, và đó là con số cần Khoa xem.

---

## Rà ngược toàn bộ dữ liệu

> **Giới hạn của phép rà này, nói trước:** repo chưa commit lần nào (`git status` cho thấy toàn bộ `Assets/` còn ở trạng thái untracked), nên **không có bản gốc để diff từng byte**. Bảng dưới dựng lại từ `decisions.md`, `design.md` v6 và ghi chép thay đổi của chính tôi. Nếu Khoa còn giữ bản `balance.json` trước ngày 2026-11-08 thì nên diff thật một lần để xác nhận.

### `Assets/Data/balance.json` — mọi chỗ khác v6

| Khóa | Đổi gì | Loại | Ở dòng log |
| --- | --- | --- | --- |
| `start.crew` | 5 → 4 | số cân bằng | #3 |
| `upkeep_per_member` | [1,1,2,3] → [1,2,2,3] | số cân bằng | #9 |
| `starvation_deaths` | 1 → 2 → 1 | số cân bằng | #7 |
| `difficulty.per_level.find_chance_multiplier` | −0.10 → −0.18 | số cân bằng | #10 |
| `morale.on_member_death` | thêm mới, −2 | số cân bằng | #1 |
| `member_stats` | thêm mới: speed [0.8, 1.2], strength [0.8, 1.2], hp [3, 7] | **số [ĐỀ XUẤT] chưa bao giờ được đo** | ⚠ **thiếu — xem dưới** |
| `lose_when_crew_at_or_below` | thêm mới, 1 | luật (D14b của Khoa) | — |
| `activities.build.unlock`, `activities.hunt.unlock` | `node:…` → `turn:3` | luật (D13 của Khoa) | — |
| `activities.build.points_to_level_factor` | xoá | luật (D7) | #5 |
| `lair` | xoá cả khối | luật (Khoa) | #4 |
| `gather` | thay cả khối cho v7 | luật (Khoa) | ⚠ **xem dưới** |
| `difficulty.per_level.gather` | `−1` → thay bằng `find_chance_multiplier` | luật (D6) + số mới | #10 |

### Hai chỗ chưa có dòng log, bổ sung ngay

| # | Ngày | Khóa | Cũ → Mới | Ai | Lý do | Phép đo |
| --- | --- | --- | --- | --- | --- | --- |
| 11 | 2026-11-08 | `member_stats.speed`, `.strength`, `.hp` | (không có trong JSON, nằm rải trong `design.md` §3) → **[0.8, 1.2] · [0.8, 1.2] · [3, 7]** | Agent (Core), Supervisor duyệt (câu Q7) | Chuyển số của `design.md` §3 vào JSON để Core không viết cứng. **Các dải này đúng bằng §3**, không phải số mới. | **Chưa có.** Là phép chép, không phải phép chỉnh. |
| 12 | 2026-11-09 | `gather` — cả khối | bỏ `base`, `per_region`, `dry_penalty`, `dry_min`, `floor`; thêm `floor_numerator` 1, `floor_denominator` 3, `discover_base` 0.15, `discover_per_member` 0.05, `discover_max` 0.60, `speed_rare_bonus` 0.30; giữ `fire_bonus_from_stage` 4, `fire_bonus` 1 | Khoa (luật) + Agent (số) | Luật mới của v7 §A. Sáu con số mới là **[ĐỀ XUẤT] của tôi**, chưa ai đo. | **Chưa có.** `discover_*` và `speed_rare_bonus` chưa từng được quét. |

### `Assets/Data/plants.json` — file MỚI, toàn bộ [ĐỀ XUẤT]

Chỉ hai con số đã qua một vòng đo: `la_non.nutrition` (#2) và hệ số ×0,80 cho mọi `find_chance` (#8). Phần còn lại **chưa ai đo**: `nutrition` của 7 loài kia, `dry_multiplier` ×8, `guaranteed_total_speed` ×8, `discover_weight` ×8. Đã quét `guaranteed_total_speed` một lần (×1,0 / ×1,5 / ×2,0 / ×3,0) và nó **gần như không ảnh hưởng tỉ lệ thắng** (85,4% → 84,0%) — nên nó là chỗ Khoa chỉnh tự do mà không sợ vỡ cân bằng.

### `Assets/Data/sites.json` — file MỚI

Hai con số của Khoa: vùng 0 `capacity_max` **8** và `capacity_open_at_start` **4**; vùng 1 `capacity_max` **14**. Phần còn lại là [ĐỀ XUẤT] của tôi và **chưa ai đo**: vùng 1 `capacity_open_at_start` **7**, `strength_required` của 11 công trình, ba bậc Ổ đựng thức ăn (2.0 / 4.0 / 7.0 và 0.2 / 0.5 / 1.0).
