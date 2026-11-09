# Before Us — Đặc tả v7, BẢN NHÁP ĐỀ XUẤT

> # 🚫 BẢN NHÁP NÀY ĐÃ ĐƯỢC HẤP THỤ — 09/11/2026
>
> Phần **luật chơi** của file này đã được gộp vào `docs/design.md` (nay là **v7**):
>
> | Phần của bản nháp | Nay đọc ở |
> | --- | --- |
> | A — Hái lượm có khám phá thực vật | `design.md` §5.1 |
> | B — Công trình theo slot, Ổ đựng thức ăn | `design.md` §5.3 |
> | F.2 — Chỗ ở và di cư theo vùng | `design.md` §5.3 và §12 |
> | A.7 — Bào mòn độ khó | `design.md` §11 |
>
> **Luật hiện hành đọc ở `design.md`, không đọc ở đây.** File này giữ lại vì nó là thứ `design.md`
> không chứa: **lịch sử của quyết định** — nguyên văn chín câu trả lời của Khoa (Phần D), các cách đọc
> đã bị bỏ (F.1), ba câu hỏi đã đóng và những câu **còn mở** (Phần E, G). Xoá nó đi là xoá lý do
> vì sao luật hiện hành lại như vậy.
>
> Còn mở, chưa ai trả lời: **E1** (slot có cần ngưỡng số người riêng không), **E5** (catalog xuyên ván
> có vào mốc 15/11 không), **G1–G3** đã được Khoa trả lời ngày 09/11 và đã thành `decisions.md` D25.


> **Đây KHÔNG phải luật.** `docs/design.md` v6 vẫn là nguồn sự thật cho tới khi Khoa duyệt bản này.
> Soạn theo góp ý 5 và 6 của Khoa ở `docs/playtest-01.md`. Phạm vi đã chốt ở `docs/decisions.md` D15.
>
> **Bản 2 — 2026-11-08, sau khi Khoa trả lời chín câu hỏi.** Câu trả lời nguyên văn của Khoa đặt trong khối trích dẫn ở từng mục liên quan. Khoa nói sẽ *"làm việc và bổ sung thiết kế cơ chế"*, nên đây vẫn là một file để Khoa sửa, chưa đẩy sang `design.md`.
>
> **Mọi con số trong file này đều mang dấu [ĐỀ XUẤT]** — không con số nào là của Khoa, kể cả những con số nằm trong mục Khoa đã duyệt hướng.
> Chỗ nào vẫn chưa có cơ sở để đoán thì nằm ở mục cuối, **không** bị đoán bừa chỉ vì Khoa vừa trả lời chín câu khác.
>
> Mốc: **15/11 màn hình cộng đồng phải chơi được** (P1 là 16–18/11).

---

## Phần A — Hái lượm có khám phá thực vật (thay §5.1)

> Nguyên văn Khoa, `playtest-01.md` mục 5: *"khi chọn kiếm ăn nó sẽ hiển thị các loại thực vật đã phát hiện, tỉ lệ khám phá ra hoa quả khác cũng sẽ ngẫu nhiên khi lựa chọn kiếm ăn, mỗi hoa quả nó sẽ có tỉ lệ tìm kiếm thấy khác nhau, dinh dưỡng cũng khác nhau, nhưng sẽ có khung đảm bảo nếu cử người có chỉ số tốc độ là bao nhiêu đó sẽ tìm thấy ít nhất 1. Khi mùa khô xảy ra thì tỉ lệ tìm thấy hoa quả giảm xuống."*

### A.1 Một loài thực vật

Dữ liệu mới, đề xuất đặt ở `Assets/Data/plants.json`:

| Trường | Kiểu | Nghĩa |
| --- | --- | --- |
| `id` | chuỗi | Định danh, không hiện cho người chơi. |
| `name` | chuỗi | Tên hiện cho người chơi. |
| `description` | chuỗi | Một câu mô tả, hiện ở catalog (D5). |
| `find_chance` | số thực | Tỉ lệ **một người** tìm thấy nó trong một lần Kiếm ăn. |
| `nutrition` | số nguyên | Thức ăn nhận được **mỗi lần tìm thấy**. |
| `regions` | mảng số | Những vùng có loài này. |
| `dry_multiplier` | số thực | Nhân vào `find_chance` trong pha Khô. |
| `discover_weight` | số nguyên | Trọng số khi rút loài **mới**. 0 = biết sẵn từ đầu ván. |

**Nội dung đề xuất (D9).** Khoa nói: *"Soạn nội dung vào file thiết kế, tôi sẽ làm việc và bổ sung thiết kế cơ chế cho"*. Dưới đây là nội dung để Khoa **sửa**, không phải để dùng nguyên. **Toàn bộ [ĐỀ XUẤT].**

| `id` | Tên | Mô tả | `find_chance` | `nutrition` | `regions` | `dry_mult` | `discover_weight` |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `la_non` | Lá non | Thứ dễ kiếm nhất, nhạt và ít no, nhưng chưa bao giờ hết. | 0.80 | 1 | 0, 1 | 0.70 | 0 (biết sẵn) |
| `qua_sung` | Quả sung | Chín rộ theo mùa. Mùa khô thì cây trút sạch quả. | 0.55 | 2 | 0 | 0.50 | 5 |
| `hat_co` | Hạt cỏ | Phải nhặt rất lâu mới đủ một nắm. | 0.50 | 2 | 1 | 0.60 | 4 |
| `cu_mai` | Củ mài | Nằm sâu dưới đất nên nắng hạn không với tới được. | 0.40 | 3 | 0, 1 | 0.80 | 4 |
| `nam_go` | Nấm gỗ | Mọc trên thân cây mục. Phải biết cây nào mới có. | 0.35 | 3 | 0 | 0.45 | 3 |
| `qua_bao_bap` | Quả bao báp | Vỏ cứng như đá, ruột chua và nhiều nước. | 0.25 | 5 | 1 | 0.90 | 2 |
| `trung_chim` | Trứng chim | Phải trèo, và phải nhanh tay trước khi chim mẹ về. | 0.20 | 5 | 0, 1 | 0.85 | 2 |
| `to_ong` | Tổ ong | Ngọt hơn mọi thứ bộ tộc từng biết. Đổi lại là bị đốt. | 0.15 | 7 | 0, 1 | 1.00 | 1 |

**Nguyên tắc đặt số [ĐỀ XUẤT]: dễ tìm thì ít no, hiếm thì nhiều.** Để người chơi có lý do đi tìm loài mới thay vì hái mãi loài đầu tiên.

`la_non` biết sẵn từ đầu ván, để lượt 1 không rơi vào cảnh không biết hái gì. Nó cũng là cái tên đã có sẵn trong `design.md` §7.3 ("Nhai lá non").

### A.2 `speed` làm gì — Khoa đã chốt

> **D4, nguyên văn Khoa:** *"Chỉ số speed là yếu tố tăng khả năng tìm thấy hoa quả hiếm chứ không đại diện cho sức hái lượm (số lượng thực phẩm mang về)."*

**`speed` KHÔNG còn nhân vào lượng thức ăn.** §5.1 của v6 ghi *"thực nhận của mỗi người = `gather_yield × speed`"* — câu đó **bị xoá**.

`speed` nay có đúng hai tác dụng trong Kiếm ăn:

1. **Tăng khả năng tìm thấy loài HIẾM.** Càng hiếm thì `speed` càng giúp nhiều:
   ```
   bonus = speed_rare_bonus × (speed − 1) × (1 − find_chance)
   ```
   [ĐỀ XUẤT] `speed_rare_bonus = 0.30`.
   Người có `speed` 1.2 tìm tổ ong (`find_chance` 0.15): `0.30 × 0.2 × 0.85 = +0.051`, tức 0.15 → 0.20, hơn một phần ba cơ hội. Cũng người đó tìm lá non (0.80): `+0.012`, gần như không đổi. Đúng câu "tăng khả năng tìm thấy hoa quả hiếm".

2. **Ngưỡng của khung đảm bảo** (D8, Khoa duyệt theo đề xuất): `speed >= 1.00` thì không bao giờ về tay không.

> **Hai tác dụng này không mâu thuẫn nhau.** Một cái là *chất* (tìm được thứ quý), một cái là *sàn* (không về tay không). `speed` giờ là chỉ số của người **đi tìm giỏi**, không phải người **mang vác khỏe**.

### A.3 Giải quyết một lần Kiếm ăn

Kiếm ăn vẫn là **một** hoạt động với `k` người (D2 của `decisions.md` không đổi).

```
0. SÀN BẮT BUỘC (xem A.6): k phải >= ceil(crew × gather_floor_fraction)

1. KHÁM PHÁ — một lần cho CẢ NHÓM, không phải mỗi người:
     nếu còn loài chưa biết ở vùng hiện tại:
         rút một lần với tỉ lệ discover_chance(k)
         nếu trúng: rút một loài chưa biết theo discover_weight, đánh dấu ĐÃ BIẾT
                    (loài vừa khám phá tham gia luôn bước 2 của chính lượt này)

2. TÌM KIẾM — mỗi người, mỗi loài một lần rút:
     với mỗi người tham gia:
         với mỗi loài ĐÃ BIẾT có ở vùng hiện tại:
             p = find_chance × (pha Khô ? dry_multiplier : 1)
                             × hệ_số_độ_khó
                 + speed_rare_bonus × (speed − 1) × (1 − find_chance)
             p = clamp(p, 0, 1)
             nếu rút trúng: thức ăn += nutrition
         nếu người này KHÔNG tìm thấy gì VÀ speed >= guaranteed_find_speed:
             tìm thấy đúng 1 lần loài có find_chance CAO NHẤT trong vùng đã biết

3. Tổng thức ăn của hoạt động = tổng của mọi người.
```

**Vì sao khám phá là một lần cho cả nhóm:** cử 6 người mà khám phá được 6 loài trong một lượt là vô lý, và chỉ sau hai lượt là hết thứ để tìm — giết luôn cảm giác tò mò mà cơ chế này sinh ra.

[ĐỀ XUẤT] `discover_chance(k) = min(0.60, 0.15 + 0.05 × k)` · `guaranteed_find_speed = 1.00` · `speed_rare_bonus = 0.30`.

`guaranteed_find_speed = 1.00` chọn vì `design.md` §3 nói trung bình của hệ số là 1.0, nên khoảng một nửa bộ tộc đạt ngưỡng. Thấp hơn thì ai cũng đạt và ngưỡng mất nghĩa; cao hơn thì khung đảm bảo gần như không bao giờ bật.

### A.4 Khám phá: "mới" khác "đã biết" thế nào

> **D5, nguyên văn Khoa:** *"Ngoài menu chính sẽ có 1 bộ catalog lưu những sinh vật người chơi đã khám phá. Còn vào ván thì không nhớ phải tìm lại."*

**Hai bộ nhớ tách rời, đây là điểm dễ cài sai nhất của Phần A:**

| | Trong ván | Catalog ngoài menu chính |
| --- | --- | --- |
| Nhớ gì | Loài đã khám phá **trong ván này** | Mọi loài người chơi **từng** khám phá, qua mọi ván |
| Ảnh hưởng lối chơi | Có — chỉ loài đã biết mới tìm thấy được | **Không** — thuần tra cứu |
| Ván mới | **Xoá sạch, tìm lại từ đầu** | Giữ nguyên, chỉ thêm vào |
| Lưu ở đâu | `GameState.DiscoveredPlantIds`, trong file lưu ván | **Ngoài file lưu ván** — xem A.5 |

**Catalog không được ảnh hưởng lối chơi.** Nếu nó cho người chơi bắt đầu ván mới với loài đã biết sẵn thì ván thứ mười sẽ dễ hơn hẳn ván đầu, và tỉ lệ thắng mục tiêu 30–40% (`design.md` §1) mất nghĩa. Khoa đã nói rõ: *"vào ván thì không nhớ phải tìm lại"*.

Trong ván:
- **Chưa biết** = không bao giờ tìm thấy, **không hiện tên** — chỉ hiện "còn N loài chưa biết ở vùng này" [ĐỀ XUẤT], để người chơi biết còn thứ để tìm.
- **Di cư sang vùng mới**: loài chỉ có ở vùng mới trở thành khám phá được. Loài đã biết mà vùng mới không có thì không tìm thấy nữa, nhưng vẫn nhớ (phòng khi quay lại).

### A.5 Catalog xuyên ván — hệ thống MỚI

Đây là **phạm vi chưa từng có trong `HANDOFF.md`**. Nó cần ba thứ chưa tồn tại:

1. **Bộ nhớ ngoài file lưu ván.** File lưu hiện tại (`SaveGame`, C7) chỉ chứa một ván. Catalog sống qua mọi ván nên phải là một file riêng [ĐỀ XUẤT] `catalog.json`, ghi bởi tầng Unity (WebGL thì là IndexedDB qua `PlayerPrefs`), **không** đi qua `SaveGame`.
2. **Một màn hình mới ngoài menu chính** — không nằm trong A1–A4 của `HANDOFF.md`.
3. **Điểm ghi**: mỗi khi khám phá một loài trong ván, ghi vào catalog ngay, kể cả khi ván đó sau này thua.

[ĐỀ XUẤT] catalog ghi: `id`, `name`, `description`, lần đầu khám phá ở vùng nào, đã gặp bao nhiêu lần. Không ghi `find_chance` hay `nutrition` cho tới khi người chơi đã gặp loài đó **[ĐỀ XUẤT] 3 lần** — để catalog cũng là một thứ đáng lấp đầy.

### A.6 Sàn bắt buộc đi kiếm ăn

> **D6, nguyên văn Khoa:** *"Nên để ít nhất 1/3 số người phải đi tìm thức ăn để hạn chế việc người chơi phát triển quá nhanh."*

**Luật:** mỗi lượt, số người được cử đi Kiếm ăn phải `>= ceil(crew × gather_floor_fraction)`. [ĐỀ XUẤT] `gather_floor_fraction = 1/3`.

Chưa đủ sàn thì **không kết thúc lượt được** — `TurnPlan` báo lỗi mới `GatherFloorNotMet`, cùng kiểu với `NotEnoughEnergy` đang có. Chọn chặn cứng vì Khoa viết *"phải đi tìm thức ăn"*, không phải "nên".

| `crew` | Sàn |
| --- | --- |
| 2 | 1 |
| 3 | 1 |
| 6 | 2 |
| 8 | 3 |
| 12 | 4 |

Sàn này hạ được bằng nâng cấp Ổ đựng thức ăn (Phần B.4).

### A.7 Vì sao ván vẫn kết thúc — **câu khẳng định cần Khoa xác nhận**

> **Khoa trả lời một câu khác với câu được hỏi, và chỗ hở đó phải được viết thành luật.** Câu hỏi là *"sau khi bỏ `gather_yield`, cái gì kết thúc ván?"*. Câu trả lời của Khoa (sàn 1/3) là một cái **hãm tốc độ phát triển**, không phải một cái **kết thúc**. Một cái hãm tự nó không làm ai thua.

**Khẳng định đề xuất — nếu Khoa không phản đối thì đây là luật:**

> **Ván kết thúc vì độ khó bào mòn nguồn thức ăn nhanh hơn mức sàn 1/3 bù lại được.**
> Mỗi bậc độ khó nhân mọi tỉ lệ tìm thấy với `hệ_số_độ_khó = max(0, 1 − 0.10 × difficulty_level)` [ĐỀ XUẤT]. Người chơi phải dồn thêm người sang Kiếm ăn, bóp nghẹt Săn bắt, Xây dựng và Nâng tinh thần, cho tới khi cả bộ tộc đi kiếm ăn mà vẫn thiếu. **Đói vẫn là thứ kết thúc ván**, chỉ là qua một đường khác.

Cơ chế này đã hoạt động trong thực tế: C8 đo được **100% số ván thua là vì đói**. v7 chỉ cần giữ nó sống qua cuộc thay máu.

**⚠ Con số `0.10` gần như chắc chắn sai và phải đo lại.** Tính thô với bộ loài ở A.1, vùng 0: một người kiếm ăn mang về trung bình ~4.1 thức ăn ở bậc 0. Ở bậc 5 (lượt 60 trong MVP) còn ~2.0; bậc 10 (lượt 120) về 0. **Tức ván kéo tới quanh lượt 120**, trong khi v6 kết thúc quanh lượt 40 và dải Cổng 2 đặt trung vị 42–52. `0.10` nhiều khả năng quá nhẹ. Đây đúng là con số mà C8 phải đo lại đầu tiên sau khi v7 xong.

`difficulty.per_level.gather = -1` của `balance.json` gắn vào công thức sắp bị xoá, nên nó **phải** được thay bằng `hệ_số_độ_khó` ở trên, không được bỏ không.

### A.8 Mùa khô

Thay `dry_penalty` theo giai đoạn bằng `dry_multiplier` theo **loài**. Khoa nói *"tỉ lệ tìm thấy hoa quả giảm xuống"* — tác động lên **tỉ lệ**, không phải một con số trừ thẳng.

| v6 (§5.1) | v7 |
| --- | --- |
| `dry_penalty[stage]` = 1/1/2/2, trừ thẳng vào `gather_yield` | mỗi loài một `dry_multiplier`, nhân vào `find_chance` |
| `dry_min` = 1, sàn cho pha Khô | **không còn** — sàn giờ là khung đảm bảo theo `speed` |

Hệ quả: mùa khô không còn là hình phạt đều tay mà là **đổi loài nào đáng đi tìm**. `to_ong` (`dry_multiplier` 1.00) không giảm chút nào, nên mùa khô đẩy người chơi sang tìm tổ ong. Ý định thiết kế [ĐỀ XUẤT].

### A.9 Công thức `gather_yield` cũ — bị thay hẳn

| Khóa `balance.json` | Số hiện tại | Sau v7 |
| --- | --- | --- |
| `gather.base` | 3 | chết |
| `gather.per_region` | 1 | chết — vùng nay quyết định **loài nào có** |
| `gather.dry_penalty` | [1,1,2,2] | chết — thay bằng `dry_multiplier` theo loài |
| `gather.dry_min` | 1 | chết |
| `gather.floor` | 0 | chết |
| `gather.fire_bonus_from_stage` / `fire_bonus` | 4 / 1 | **giữ chỗ** cho bếp lửa giai đoạn 4, ngoài MVP |
| `difficulty.per_level.gather` | −1 | **phải thay** bằng `hệ_số_độ_khó` (A.7) |

---

## Phần B — Công trình theo slot (thay cơ chế Xây dựng ở §5)

> Nguyên văn Khoa, `playtest-01.md` mục 6: *"Ở màn hình chính này sẽ có các slot nhỏ như ổ đựng thức ăn, hoặc các bãi đá/nhánh cây chưa được dọn — để dọn thì cần số người và số sức mạnh cụ thể; việc nâng cấp công trình tức là chọn các công trình trên và thực hiện nó. Ở các giai đoạn tiến hóa sau thì thêm các slot công trình khác như nghĩa địa hay bếp lửa."*

### B.1 Cách dọn — Khoa đã chốt

> **D7, nguyên văn Khoa:** *"Làm phát dọn luôn, chỉ cần đủ số sức mạnh khi cho lượng người vào thực hiện công trình thì lượt sau slot đó sẽ được dọn."*

**Ăn cả ngã về không, không có thanh tiến trình.** Cử người vào một slot; nếu **tổng sức mạnh** của nhóm đạt ngưỡng thì slot được dọn. Thiếu thì lượt đó phí, không tích luỹ gì.

**Hiệu lực từ LƯỢT SAU.** Khoa nói rõ *"lượt sau slot đó sẽ được dọn"*. Nghĩa là: cử người ở lượt N → giải quyết ở cuối lượt N → `build_level` tăng và slot biến mất **từ lượt N+1**.

> ⚠ Điều này **lật một quyết định cũ của Core.** `TurnSystem.ResolveOrder` hiện đặt Xây dựng **trước** combat, với lý do "cấp công trình vừa lên có hiệu lực ngay trong trận cùng lượt". Dưới D7, lý do đó biến mất — phần thưởng tới ở lượt sau, nên thứ tự Xây dựng so với combat không còn quan trọng. Ghi lại để không ai đọc chú thích cũ trong mã rồi tưởng nó vẫn đúng.

**Số người có còn là điều kiện riêng không?** Khoa viết *"chỉ cần đủ số sức mạnh khi cho lượng người vào"* — đọc sát thì **sức mạnh là điều kiện duy nhất**, số người chỉ là cách đạt được nó. [ĐỀ XUẤT] bỏ ngưỡng số người, giữ mỗi ngưỡng tổng sức mạnh. Đơn giản hơn một bậc, và vẫn đúng ý "cần số người cụ thể" vì muốn đủ sức mạnh thì phải cử đủ người. Xem câu hỏi **E1**.

### B.2 `build_level` giữ lại — Khoa duyệt theo đề xuất (D2)

`build_level` = **số slot đã dọn**. Không còn `build_points`, không còn `points_to_level_factor`.

Ba công thức ngoài Xây dựng **giữ nguyên từng chữ**:
- §4 sức chứa hang = `6 + 2 × build_level`
- §5.2 tỉ lệ thắng Săn/Khám phá `+0.05 × build_level`
- §8 tỉ lệ thắng Phòng thủ `+0.06 × build_level`

| Khóa | Sau v7 |
| --- | --- |
| `activities.build.points_to_level_factor` = 4 | chết |
| `lair.capacity_base` = 6 | **giữ** |
| `lair.capacity_per_build_level` = 2 | **giữ** — nay là "mỗi slot dọn xong cho +2 sức chứa" |

### B.3 Nút neuron mở ra loại slot — Khoa duyệt theo đề xuất (D1)

Slot có trường `requires_node`. **`s2_stone_tools` ("Dùng đá đập") mở mọi slot loại đá.** Muốn dọn đá thì phải biết đập đá.

Điều này **trả một trong hai món nợ D13**: `s2_stone_tools` có việc trở lại mà không phải phát minh cơ chế nào — nó dùng lại đúng `UnlockRule` kiểu `node:` đã viết và đã có test.

**`s2_scavenge_meat` KHÔNG được gán vào đây.** Nó là về *ăn xác thịt*, không về dọn dẹp; ép nó mở một loại slot sẽ là gán ghép. Nút đó vẫn rỗng, và nên quyết cùng lúc với `hunt_food_per_member[0] = 0` — cả hai đều là chuyện săn bắt. Xem mục "còn treo".

### B.4 Ổ đựng thức ăn — công trình NÂNG CẤP ĐƯỢC, không phải slot dọn một lần

> **D3, nguyên văn Khoa:** *"Ổ đựng thức ăn chỉ là tượng trưng, nó là lối tắt để người chơi truy cập vào hệ thống thực phẩm đã khám phá và giao nhiệm vụ cho các nhân vật khác, đồng thời việc nâng cấp nó sẽ giảm chi phí đi hái lượm, và những hiệu ứng bổ trợ khác về thực phẩm."*

Câu trả lời này **gỡ mâu thuẫn với §4** và **mở ra một hệ thống mới**. Hai mặt, ghi rõ cả hai:

**Mặt 1 — không mâu thuẫn §4 nữa.** Ổ **không chứa thức ăn**. Nó là **lối tắt giao diện**: bấm vào ổ để mở danh sách thực vật đã khám phá trong ván và giao người đi Kiếm ăn. `design.md` §4 *"thức ăn không để dành"* **giữ nguyên, không sửa một chữ**.

**Mặt 2 — đây là hệ nâng cấp nhiều bậc, không phải một slot dọn một lần.** Khác mọi slot khác ở Phần B.

> 🚫 **THANG BẬC DƯỚI ĐÂY ĐÃ BỊ THAY** bởi câu trả lời E2 của Khoa ngày 2026-11-09 — xem F.1 và `decisions.md` D17. Giữ lại để thấy cái gì đã bị bỏ, **không** phải mô tả luật hiện hành.
>
> | Bậc | Tên | Sức mạnh cần | Hiệu ứng (ĐÃ BỎ) |
> | --- | --- | --- | --- |
> | 1 | Ổ lót lá | 2.0 | ~~Sàn Kiếm ăn 1/3 → 1/4~~ |
> | 2 | Ổ quây đá | 4.0 | ~~Kiếm ăn tốn 0 năng lượng~~ |
> | 3 | Ổ có mái che | 7.0 | ~~Mùa khô: `dry_multiplier` +0.15~~ |

Thang bậc **hiện hành** (D17), toàn bộ [ĐỀ XUẤT], dữ liệu thật ở `Assets/Data/sites.json`:

| Bậc | Tên | Sức mạnh cần | Hiệu ứng |
| --- | --- | --- | --- |
| 0 | Ổ đất trống | — | Có sẵn từ đầu ván. Lối tắt giao diện. Không hiệu ứng số. |
| 1 | Ổ lót lá | 2.0 | Nới mọi ngưỡng `guaranteed_total_speed` đi **0.2** |
| 2 | Ổ quây đá | 4.0 | Nới **0.5** |
| 3 | Ổ có mái che | 7.0 | Nới **1.0** |

**Mỗi bậc cũng tính là +1 `build_level`** như các slot khác, nên sức chứa hang và tỉ lệ thắng combat vẫn lên theo.

> ~~**"Giảm chi phí đi hái lượm" có hai cách đọc**, và tôi cài cả hai vào thang trên.~~ 🚫 Khoa đã trả lời: không phải cách đọc nào trong hai cách đó. Xem F.1.
>
> *"Những hiệu ứng bổ trợ khác về thực phẩm"* — bậc 3 là một đề xuất cho chỗ đó. Đây là chỗ Khoa nói sẽ *"bổ sung thiết kế cơ chế"*, nên tôi để đúng một bậc làm mẫu chứ không lấp đầy.

### B.5 Danh sách slot — nội dung đề xuất (D9)

Dữ liệu mới, đề xuất đặt ở `Assets/Data/sites.json`. **Toàn bộ số là [ĐỀ XUẤT].**

Ngưỡng sức mạnh đặt theo mốc: sức mạnh trung bình một người là **1.0** (`design.md` §3), nên "3.0" nghĩa là **ba người trung bình vừa đủ** — cử người khỏe thì bớt được một người.

**Giai đoạn 1 — Vượn người**

| `id` | Tên | Sức mạnh cần | Loại | `requires_node` | Mô tả hiện cho người chơi |
| --- | --- | --- | --- | --- | --- |
| `nhanh_cay_gay` | Nhánh cây gãy | 1.8 | gỗ | — | Một nhánh lớn đổ chắn ngang chỗ nằm. Đẩy được nếu đủ người. |
| `bai_da_nho` | Bãi đá nhỏ | 2.2 | đá | — | Sỏi và đá vụn rải khắp nền hang. Nhặt sạch thì nằm êm hơn. |
| `hoc_da_tru_mua` | Hốc đá trú mưa | 3.0 | đá | — | Một hốc sâu trong vách. Dọn xong thì cả bộ tộc có chỗ tránh mưa. |

**Giai đoạn 2 — Người vượn phương Nam**

| `id` | Tên | Sức mạnh cần | Loại | `requires_node` | Mô tả hiện cho người chơi |
| --- | --- | --- | --- | --- | --- |
| `tang_da_lon` | Tảng đá lớn | 3.6 | đá | `s2_stone_tools` | Tảng đá chắn nửa cửa hang. Phải biết đập đá mới vỡ nổi. |
| `bui_gai_ran` | Bụi gai rậm | 3.0 | gỗ | — | Gai chằng chịt quanh hang. Phát quang thì nhìn ra xa hơn. |
| `vung_nuoc_dong` | Vũng nước đọng | 2.6 | đất | — | Nước tù bốc mùi sau mưa. Khơi cho chảy đi thì bớt ruồi muỗi. |

**Giai đoạn 3+ — chỉ chừa chỗ, ngoài MVP.** Khoa nhắc **nghĩa địa** và **bếp lửa**. Bếp lửa nối thẳng với `gather.fire_bonus_from_stage = 4` đã có sẵn trong `balance.json`. Nghĩa địa chưa có móc nối nào trong luật hiện tại — có thể là chỗ dùng lại `morale.on_member_death` (D14a). Không đặc tả ở đây.

---

## Phần C — Ảnh hưởng dây chuyền: cái giá phải trả

### C.1 Khóa `balance.json` thành vô nghĩa

`gather.base` · `gather.per_region` · `gather.dry_penalty` · `gather.dry_min` · `gather.floor` · `activities.build.points_to_level_factor` · `difficulty.per_level.gather` *(phải thay, không được bỏ không — A.7)*

### C.2 Hệ quả của D4 — `speed` rời khỏi công thức thức ăn

Khoa bỏ `speed` khỏi lượng hái lượm. Ba thứ đang tồn tại bị gỡ theo:

**1. Cái bẫy "3+2+4 chứ không phải 3+3+3" biến mất.** `docs/core-api.md` §4 có hẳn một mục **"Cạm bẫy"** cảnh báo Lead UI đừng nhân `k × yield`, vì mỗi người có `speed` riêng. Sau v7 cảnh báo đó **sai** và phải gỡ — thay bằng cảnh báo mới: thức ăn nay là tổng của những lần rút trúng, không suy ra được từ `k` bằng bất kỳ phép nhân nào.

**2. `GatherRules.FoodFromMember(yield, member, speedBonus)` thành vô nghĩa**, cùng với `NodeEffects.SpeedBonus` ở nhánh hái lượm. `speedBonus` sinh ra để cộng vào `speed` **trước khi nhân** — không còn phép nhân thì không còn chỗ cộng. `speed_bonus` của hai nút "Đứng thẳng ngắn" và "Đi bằng hai chân" nay chỉ còn tác dụng qua đường **tìm loài hiếm** (A.2) và qua combat.

**3. `MemberStatRanges` và chế độ trung bình của C8 đổi vai.** Chế độ trung bình (`AverageFromConfig`, mọi `speed` = 1.0) sinh ra để C8 khớp `before_us_sim.py`, vốn dùng `speed = 1`. Sau v7, `speed = 1` nghĩa là **đúng ngưỡng khung đảm bảo** và **không có bonus tìm loài hiếm** — tức chế độ trung bình nay là một lựa chọn có ý nghĩa cân bằng hẳn hoi, không còn là một phép giản lược vô hại. Bot C8 chạy ở chế độ đó sẽ không bao giờ thấy phần thưởng của `speed`.

### C.3 Hệ thống hoàn toàn mới, không có trong `HANDOFF.md`

| Hệ thống | Từ đâu | Vì sao là phạm vi mới |
| --- | --- | --- |
| **Catalog xuyên ván** | D5 | Bộ nhớ **ngoài** file lưu ván + một màn hình mới ngoài menu chính. `HANDOFF.md` A1–A4 không có màn nào cho nó. |
| **Ổ thức ăn nâng cấp nhiều bậc** | D3 | Khác hẳn slot dọn một lần: có thang bậc, mỗi bậc một hiệu ứng khác loại. |
| **Sàn bắt buộc Kiếm ăn** | D6 | Luật xác thực mới ở `TurnPlan` (`GatherFloorNotMet`), chặn kết thúc lượt. |

### C.4 Bốn hiệu ứng nút neuron thành vô nghĩa

| Nút | Hiệu ứng hiện tại | Sau v7 |
| --- | --- | --- |
| Leo trèo khéo (gốc, GĐ1) | `gather_flat_per_member` +0.5 | **vô nghĩa** |
| **Đào rễ củ (CỔNG, GĐ1)** | `gather_yield_bonus` +1 | **vô nghĩa** |
| Ăn côn trùng, trứng (tùy chọn, GĐ1) | `dry_penalty_reduction` 1 | **vô nghĩa** |
| Gậy đào (tùy chọn, GĐ2) | `gather_yield_bonus_wet` +1 | **vô nghĩa** |

Cộng với `s2_scavenge_meat` còn rỗng từ D13: **5 nút trên 20 không làm gì.** Một trong số đó — "Đào rễ củ" — là nút **cổng bắt buộc** để tiến hóa khỏi giai đoạn 1.

Thang mới của v7 cho chỗ gắn tự nhiên: `find_chance` theo loài hoặc theo nhóm loài, `discover_chance`, `dry_multiplier`, `guaranteed_find_speed`. Ví dụ "Đào rễ củ" thành **+0.15 `find_chance` cho `cu_mai`** [ĐỀ XUẤT]. Nhưng đó là thiết kế nội dung, thuộc Khoa.

### C.5 Mã Core phải viết lại

| Chỗ | Việc |
| --- | --- |
| `GatherRules` | **viết lại hoàn toàn** |
| `TurnSystem.ResolveGather` / `ResolveBuild` | viết lại theo A.3 và B.1 |
| `TurnPlan` | Xây dựng cần **chọn slot nào**; thêm xác thực sàn Kiếm ăn |
| `TurnSystem.ResolveOrder` | xem lại — lý do đặt Xây dựng trước combat không còn (B.1) |
| `GameState` | thêm `DiscoveredPlantIds`, `ClearedSiteIds`, `FoodNestLevel` |
| `SaveGame` | thêm ba trường, **tăng `CurrentVersion` 1 → 2**; ván đã lưu của Khoa sẽ không đọc được |
| `NodeEffects` | bỏ 4 trường, thêm trường của thang mới |
| mới | `PlantCatalog` + `plants.json` · `SiteCatalog` + `sites.json` · bộ nhớ catalog xuyên ván |

### C.6 Test sẽ đỏ

| Nhóm | Số test | Vì sao |
| --- | --- | --- |
| `GatherRulesTests` | 10 | toàn bộ công thức bị xoá |
| `TurnSystemTests` | ~10 | mọi test Kiếm ăn và Xây dựng |
| `PlaytestRulesTests` | ~3 | sàn Kiếm ăn đổi điều kiện kết thúc lượt |
| `BalanceConfigTests` | 2–3 | các khóa đã chết |
| `NeuronNetworkTests` | 2–3 | hiệu ứng nút đổi thang |
| `SaveGameTests` | 2–3 | đổi phiên bản file lưu |
| `CoreApiExampleTests` | 1–2 | ví dụ trong tài liệu đổi |
| `BalanceTests` (C8) | **toàn bộ** | bot không còn biết chơi |

Khoảng **35 test** phải viết lại. **Không nới lỏng** — viết lại cho khớp luật mới.

### C.7 Vì sao mọi phép đo cân bằng phải làm lại

Hái lượm là nguồn thức ăn chính, và C8 đo được **đói là nguyên nhân 100% số ván thua**. Thay hẳn cách sinh thức ăn tức là thay thứ quyết định ván sống hay chết.

Mất hiệu lực ngay khi v7 được duyệt:
- Dải Cổng 2 (balanced 30–45%, trung vị 42–52 lượt).
- **`tools/sim/before_us_sim.py` không còn là mô phỏng tham chiếu của luật hiện hành.** `AGENTS.md` xếp nó là nguồn sự thật thứ 3; sau v7 nó mô tả một trò chơi khác. Hoặc Khoa cho sửa sim, hoặc hạ cấp nó trong `AGENTS.md` — **không được để nguyên và giả vờ nó vẫn đúng**.
- Bốn bot C8 phải viết lại chiến lược (thêm: phải tuân sàn 1/3).
- Phép kiểm chéo mạnh nhất dự án đang có — Core tắt D1/D2/D11 tái tạo sim lệch 0,1 điểm % — **không còn chạy được**.

### C.8 Ước lượng công — ĐÃ CẬP NHẬT

Ước lượng cũ (1,5–2 ngày) chỉ tính Phần A và Phần B như bản nháp đầu. D3, D5 và D6 thêm phạm vi chưa được tính.

| Việc | Ước lượng [ĐỀ XUẤT] |
| --- | --- |
| Phần A — hái lượm, khám phá, `plants.json` | 0,75 ngày |
| Phần B — slot, `sites.json`, `build_level` dẫn xuất | 0,5 ngày |
| **D6** — sàn Kiếm ăn + `hệ_số_độ_khó` thay `difficulty.gather` | 0,25 ngày |
| **D3** — ổ thức ăn nâng cấp nhiều bậc | 0,5 ngày |
| **D5** — catalog xuyên ván, phần Core + lưu trữ | 0,25 ngày |
| Viết lại ~35 test | 0,75 ngày |
| **Tổng, chỉ riêng Core** | **≈ 3 ngày** |

Chưa tính: màn hình catalog (D5) và màn hình cộng đồng (A1 dựng lại) thuộc nhánh UI; art cho vật thể tương tác (góp ý 7); và thời gian Khoa bổ sung thiết kế như đã nói ở D9.

> **⚠ Rủi ro lịch, nói thẳng.** Quỹ tới mốc 15/11 là **7 ngày**. Riêng Core đã ăn 3. `HANDOFF.md` §1 nói nếu trễ thì **giữ ngày phát hành, cắt hình ảnh và nội dung**. Hai ứng viên cắt được mà ít đau nhất, theo thứ tự: **màn hình catalog (D5)** — cơ chế trong ván không phụ thuộc nó, cắt đi thì chỉ mất phần tra cứu ngoài menu; và **bậc 3 của ổ thức ăn (B.4)**. Đây là nhận định của Lead Core, **quyết định cắt là của Khoa**.

---

## Phần D — Chín câu hỏi, Khoa đã trả lời

| Câu | Khoa chốt | Nằm ở mục |
| --- | --- | --- |
| D1 — nút neuron mở loại slot? | **Theo đề xuất**: `s2_stone_tools` mở slot loại đá | B.3 |
| D2 — `build_level` còn không? | **Theo đề xuất**: giữ, = số slot đã dọn | B.2 |
| D3 — ổ thức ăn vs §4 | Ổ là **tượng trưng**, lối tắt giao diện; nâng cấp giảm chi phí hái lượm | B.4 |
| D4 — `speed` còn nhân vào lượng không? | **Không.** `speed` = khả năng tìm **loài hiếm** | A.2, C.2 |
| D5 — nhớ qua ván? | **Catalog ngoài menu chính** nhớ; **trong ván thì không**, phải tìm lại | A.4, A.5 |
| D6 — cái gì thay `difficulty.gather`? | Sàn **1/3 bộ tộc** phải đi kiếm ăn | A.6, A.7 |
| D7 — dọn slot ăn cả ngã về không? | **Có**, và slot dọn xong **từ lượt sau** | B.1 |
| D8 — `guaranteed_find_speed` = 1.00? | **Theo đề xuất** | A.2, A.3 |
| D9 — bốn nút mất hiệu ứng | **Soạn nội dung vào file**, Khoa sẽ bổ sung cơ chế | A.1, B.5, C.4 |

---

## Phần E — Câu hỏi MỚI nảy ra từ chính các câu trả lời

**E1. Slot có cần ngưỡng SỐ NGƯỜI riêng, hay chỉ cần tổng sức mạnh?**
Khoa viết *"chỉ cần đủ số sức mạnh khi cho lượng người vào"*. Đọc sát thì sức mạnh là điều kiện duy nhất.
→ **Đề xuất: chỉ ngưỡng sức mạnh.** Đơn giản hơn một bậc, và vẫn đúng ý "cần số người cụ thể" vì muốn đủ sức mạnh thì phải cử đủ người. Cũng làm `strength` thành chỉ số có việc rõ ràng ngoài combat.
→ B: giữ cả hai ngưỡng. Cho phép ra những slot kiểu "cần đông người dù không cần khỏe".

**E2. "Giảm chi phí đi hái lượm" là chi phí nào?**
Hai cách đọc: chi phí **nhân lực** (sàn 1/3) hay chi phí **năng lượng** (1 điểm).
→ **Đề xuất: cả hai, ở hai bậc khác nhau** (B.4 bậc 1 và bậc 2). Nếu Khoa chỉ muốn một thì bỏ bậc kia.

**E3. Khẳng định ở A.7 có đúng ý Khoa không?**
*"Ván kết thúc vì độ khó bào mòn nguồn thức ăn nhanh hơn mức sàn 1/3 bù lại được."*
→ **Không trả lời tức là đồng ý** — đây là luật, và Core sẽ cài theo. Nếu Khoa có cơ chế kết thúc khác trong đầu thì nói sớm, vì nó đổi cả cách đặt số của Phần A.

**E4. `hệ_số_độ_khó = 1 − 0.10 × bậc` cho ván kéo tới quanh lượt 120, trong khi v6 kết thúc quanh lượt 40.**
→ **Đề xuất: cứ cài 0.10, rồi để C8 đo và đề xuất lại.** Không đoán thêm ở đây — đây đúng là thứ C8 sinh ra để trả lời.

**E5. Catalog xuyên ván có nằm trong phạm vi 15/11 không?**
→ **Đề xuất: KHÔNG.** Cắt khỏi mốc 15/11, làm sau P1. Cơ chế trong ván không phụ thuộc nó; cắt đi chỉ mất phần tra cứu ngoài menu, mà 5 người chơi thử sẽ chơi chứ không tra cứu. **Quyết định cắt là của Khoa.**

---

---

## Phần F — Trả lời đợt 2 của Khoa (2026-11-09) và ÁNH XẠ LẠI

Khoa gửi ba câu trả lời, đánh số "E2" và "E4". Ánh xạ thật sự:

| Khoa đánh số | Thực ra trả lời câu nào | Trạng thái |
| --- | --- | --- |
| "E2" | **đúng là E2** (chi phí hái lượm) | ✅ đã trả lời — nhưng bằng một cách đọc THỨ BA |
| "E4" | **KHÔNG phải E4.** Trả lời câu mở *"Slot có đổi theo vùng không"* | ⏳ **E4 vẫn còn mở** |
| câu thứ ba (bộ lọc) | Thuộc **A.5 catalog xuyên ván**, không phải thứ tự hiển thị trong lượt | ✅ ghi nhận |

### F.1 E2 — cách đọc thứ ba, thay hẳn hai cách đọc cũ

Nguyên văn: *"Giảm yêu cầu của các cây có thể tìm thấy, ví dụ cây yêu cầu có tổng nhanh nhẹn là 6 thì mới chắc chắn tìm thấy thì giảm còn 5.8 hoặc 5.5 kiểu kiểu vậy"*

Hai cách đọc cũ (giảm nhân lực 1/3 → 1/4 ở bậc 1; giảm năng lượng 1 → 0 ở bậc 2) **bị bỏ, không giữ làm tuỳ chọn**. Ổ đựng thức ăn nâng cấp thì **hạ ngưỡng `guaranteed_total_speed`** của các loài.

Kéo theo một sửa lỗi đọc nữa, lớn hơn: **ngưỡng khung đảm bảo là TỔNG nhanh nhẹn của cả nhóm được cử đi, cho TỪNG loài** — không phải chỉ số của từng người. Bản cài Phần A đầu tiên đọc sai thành từng người; đã sửa (`plants.json` thêm `guaranteed_total_speed`, `balance.json` bỏ `guaranteed_find_speed`).

Hệ quả số học của cách đọc đúng: khung đảm bảo nay chỉ bảo đảm **một lần tìm thấy cho cả nhóm, mỗi loài**, chứ không phải mỗi người một lần. Sàn thức ăn thấp hơn hẳn bản cài sai — phải đo lại ở C8.

### F.2 Trả lời "E4" — slot theo vùng, và ba luật MỚI

Nguyên văn: *"Mỗi vùng sẽ có giới hạn số slot, ví dụ khu vực 1 chỉ có 8 slot, khu vực 2 sẽ là 14 slot, khi thực hiện khám phá vùng mới, sẽ phải chọn ra tầm 4-6 người mang theo qua khu vực mới, khu vực mới sẽ reset lại số slot và có số slot đã mở khởi đầu khác nhau."*

Ba luật rút ra, đều là MỚI:
1. **Slot thuộc về vùng**, không thuộc về bộ tộc. Mỗi vùng có trần slot riêng (vùng 1: 8; vùng 2: 14 — [ĐỀ XUẤT] giữ nguyên số Khoa nêu) và số slot mở sẵn khi tới khác nhau.
2. **Di cư là mang theo 4–6 người**, không phải cả bộ tộc.
3. **Sang vùng mới thì số slot đã dọn reset** về mức khởi đầu của vùng đó.

Luật 1 trả lời luôn câu mở *"Slot có đổi theo vùng không"*. Luật 2 và 3 thì đẻ ra ba câu hỏi mới ở Phần G.

### F.3 Câu thứ ba — bộ lọc catalog

Nguyên văn: *"Hiện loài theo tên, nhóm loài, thời đại (bộ lọc)."*

Thuộc **A.5 (catalog xuyên ván)**, tức phần D5 — không phải thứ tự hiển thị trong một lượt chơi (câu đó vẫn mở). Kéo theo **hai trường mới cho mỗi loài** trong `plants.json`: `nhóm loài` và `thời đại`. Ghi nhận, **chưa cài**, vì D5 nằm gần cuối thứ tự thi hành và là ứng viên cắt đầu tiên nếu hết thời gian (E5).

### F.4 E4 THẬT vẫn chưa được trả lời

Câu E4 của tôi là **hệ số bào mòn độ khó** `1 − 0.10 × bậc`. Tính ra ván kéo tới quanh lượt 120, trong khi Cổng 2 của `HANDOFF.md` nhắm 42–52 lượt. Khoa chưa chạm tới câu này. **Nó vẫn mở.** Đề xuất cũ giữ nguyên: cứ cài 0.10, để C8 đo rồi đề xuất lại bằng số thật.

---

## Phần G — Câu hỏi mới nảy ra từ đợt trả lời thứ hai

**G1. Những người KHÔNG được mang theo khi di cư thì sao?**
Chết? Ở lại thành một nhóm riêng? Biến mất khỏi ván? Luật 2 của F.2 nói "chọn ra tầm 4–6 người mang theo", nhưng bộ tộc lúc đó có thể đang 10 người. **Không đoán.**

**G2. Bộ tộc dưới 4 người có di cư được không?**
Nếu ngưỡng dưới là 4 mà bộ tộc còn 3, thì Khám phá thành nút chết đúng lúc người chơi cần nó nhất. Đây là đúng **mẫu K1** (phần thưởng nằm sau cánh cửa mà chính nó phải mở) — cần Khoa chốt rõ.

**G3. Chia đôi bộ tộc thì điều kiện THUA tính theo nhóm nào?**
`lose_when_crew_at_or_below: 1` hiện tính trên tổng số người. Nếu 4 người sang vùng mới và 6 người ở lại thì luật thua áp cho nhóm nào — nhóm đang chơi, cả hai, hay tổng? Ảnh hưởng thẳng tới `TurnOutcome.Lost`.

Ba câu này **chặn việc cài di cư theo luật mới**. Phần B (công trình theo slot) làm được ngay vì không phụ thuộc chúng; riêng phần "slot thuộc về vùng" sẽ cài ở mức dữ liệu (mỗi vùng một bộ slot) nhưng **chưa nối vào Khám phá**.

## Những thứ bản nháp này VẪN CỐ Ý không trả lời

Chín câu trả lời của Khoa không chạm tới những chỗ này, và tôi không đoán nốt chỉ vì vừa có chín câu trả lời khác.

- **Slot đã dọn có mọc lại không**, hay dọn xong là hết vĩnh viễn. Ảnh hưởng thẳng tới việc Xây dựng còn là hoạt động dùng được ở cuối ván hay không — nếu dọn hết 6 slot thì Xây dựng thành nút chết.
- ~~**Slot có đổi theo vùng không**~~ — ✅ Khoa trả lời 2026-11-09, xem F.2 luật 1: slot thuộc về vùng, mỗi vùng một trần riêng.
- **Thứ tự ưu tiên hiển thị** khi một người tìm thấy nhiều loài trong cùng một lượt.
- **`tools/sim/before_us_sim.py` sẽ ra sao** sau v7 (C.7) — sửa, bỏ, hay hạ cấp trong `AGENTS.md`. Quyết định về quy trình, không phải về luật chơi.
- **`s2_scavenge_meat` nhận hiệu ứng gì**, và `hunt_food_per_member[0]` có còn là 0 không. Hai việc này thuộc về nhau và nên quyết cùng lúc.
