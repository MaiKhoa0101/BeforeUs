# Before Us — Đặc tả luật chơi (v7)




> Nguồn sự thật cho luật chơi.
> 
> **v7 (09/11/2026)** gộp `docs/design-v7-draft.md` vào đây: hái lượm có khám phá thực vật (§5.1), công trình và chỗ ở theo vùng (§5.3), di cư một chiều (§12). Bản nháp đó nay **đã được hấp thụ** — nó còn giữ phần hỏi–đáp với Khoa và các câu còn mở, nhưng luật hiện hành đọc ở file này.
> 
> ⚠ **`tools/sim/before_us_sim.py` nay mô tả một trò chơi KHÁC.** Nó dựng theo v6 và không biết thực vật, công trình theo vùng hay di cư có chọn người. Số của nó chỉ còn giá trị lịch sử; nguồn cân bằng hiện hành là C8 trong `Assets/Scripts/Core.Tests/BalanceTests.cs` (xem §15 và `docs/balance-log.md`).
> Mục đánh dấu **[ĐỀ XUẤT]** là giá trị chưa được người thiết kế (Khoa) chốt: được dùng để chạy, nhưng phải liệt kê trong báo cáo để Khoa duyệt. Từ 09/11, các con số cân bằng Khoa đã **ủy quyền** cho Supervisor chỉnh (`docs/decisions.md` D22) vẫn mang nhãn này — chúng được quyết bằng phép đo, không phải bằng chữ ký của Khoa, và Khoa lật lại lúc nào cũng được. Mọi lần chỉnh ghi ở `docs/balance-log.md`.
> Mục đánh dấu **[MVP]** là giá trị riêng của bản đầu tiên (vertical slice).

## 1. Tổng quan

- Thể loại: chiến thuật sinh tồn theo lượt, có combat mini thời gian thực.
- Người chơi dẫn dắt một bộ tộc tiền sử tiến hóa qua 5 giai đoạn, từ vượn người đến người hiện đại. Mỗi lượt tượng trưng cho hàng trăm năm.
- Thắng: đạt giai đoạn 5 (**[MVP]**: đạt giai đoạn 3). Thua: số thành viên còn lại **từ 1 trở xuống** (`lose_when_crew_at_or_below`). *(Đổi theo góp ý 8 của Khoa ở playtest 01, 2026-11-08; trước đó là "về 0". Xem `docs/decisions.md` D14.)*
- Một người chơi, đấu với game. Nền tảng: PC và trình duyệt (WebGL) trước, điện thoại sau. Engine: Unity, C#.
- Cảm xúc mục tiêu: thử thách (thua thì bực, muốn chơi lại); phụ: tò mò khám phá, nhịp nhanh phấn khích trong combat, bất ngờ từ sự kiện.
- Tỉ lệ thắng mục tiêu: 30–40% với người chơi biết cân nhắc. Né mọi combat tự chọn phải thua.

## 2. Vòng lượt

Mỗi lượt theo thứ tự:

1. **Đầu lượt người chơi**: áp dụng sự kiện ngẫu nhiên (không cần chọn) đã được rút ở lượt máy trước, nếu có. Hiện dự báo "cuối lượt cần X thức ăn, đang có Y".
2. **Lượt người chơi**: người chơi chọn hoạt động, tự chọn từng thành viên tham gia (mỗi thành viên tối đa 1 hoạt động mỗi lượt), mỗi hoạt động trả năng lượng một lần (không phụ thuộc số người). Có thể mở nút trong mạng lưới neuron bất cứ lúc nào trong lượt. Kết quả hoạt động được giải quyết khi người chơi kết thúc lượt; Săn bắt và Khám phá mở combat mini.
3. **Lượt máy**: rút sự kiện (mục 8).
4. **Cuối lượt**: ăn → sinh thành viên → kiểm tra tiến hóa → kiểm tra thắng/thua.

Lượt bắt đầu từ 1. Không có giới hạn số lượt; độ khó tăng dần tự kết thúc những ván ở lì.

## 3. Thành viên

- Bắt đầu ván với **4** thành viên **[ĐỀ XUẤT]** — đúng bằng số chỗ ở mở sẵn của vùng 0 (§5.3). Trước 09/11 là 5, đổi vì 5 người / 4 chỗ là hai luật loại trừ nhau; xem `docs/balance-log.md` #3.
- Mỗi thành viên sinh ra với chỉ số ngẫu nhiên, cố định suốt đời (không có cơ chế bị thương):
  - `speed` **[ĐỀ XUẤT]** hệ số 0.8–1.2: tốc độ đánh trong combat, và hai việc trong hái lượm (§5.1) — tăng cơ hội tìm thấy **loài hiếm**, và góp vào **tổng nhanh nhẹn của nhóm** để đạt khung đảm bảo. **Không còn nhân vào lượng thức ăn** (D4 của Khoa, playtest 01).
  - `strength` **[ĐỀ XUẤT]** hệ số 0.8–1.2: nhân vào sát thương trong combat.
  - `hp` **[ĐỀ XUẤT]** số nguyên 3–7: máu trong combat; quái thú kiểu "săn kẻ yếu" nhắm người có `hp` thấp nhất.
  - Ngoại hình: tổ hợp ngẫu nhiên các đặc điểm (khuôn mặt, tóc, màu da, dấu vết…), theo bộ hình của giai đoạn hiện tại.
- Giá trị trung bình của các hệ số là 1.0, để cân bằng khớp với mô phỏng (mô phỏng dùng giá trị trung bình).

## 4. Tài nguyên

| Tài nguyên | Quy tắc |
|---|---|
| Thức ăn | **Không để dành.** Thức ăn kiếm được trong lượt chỉ dùng cho lượt đó; cuối lượt phần dư bị bỏ. |
| Khẩu phần | Mỗi thành viên cần `upkeep[stage]` = 1 / **2** / 2 / 3 (giai đoạn 1/2/3/4) thức ăn mỗi lượt. **[ĐỀ XUẤT]** giai đoạn 2 nâng từ 1 lên 2 ngày 09/11 (`balance-log.md` #9). |
| Đói | Cuối lượt nếu thức ăn < tổng khẩu phần: mất đúng **1** thành viên (người chơi chọn ai hay ngẫu nhiên: **[ĐỀ XUẤT]** ngẫu nhiên). |
| Năng lượng | Hồi đầy đầu mỗi lượt. Tối đa `energy_max[stage]` = 4 / 5 / 6 / 7. |
| Neuron | Kiếm từ hoạt động (theo số người tham gia), tiêu để mở nút mạng lưới neuron. |
| Tinh thần | Tích lũy; đạt ngưỡng thì sinh thành viên. Không âm (chặn ở 0). |
| Công trình | Cấp công trình `build_level` = số công trình đã dọn ở vùng hiện tại + bậc Ổ đựng thức ăn. **Số suy ra, không có bộ đặt.** Dùng cho §5.2 và §8. |
| Chỗ ở | Sức chứa hang = số chỗ vùng mở sẵn + số công trình đã dọn, chặn ở trần của vùng (§5.3). **Công thức `6 + 2 × build_level` của v6 đã bị thay** (D20). |
| Vùng đất | Chỉ số vùng `region`, bắt đầu 0 (rừng nhiệt đới). Tối đa 5 (**[MVP]** tối đa 1: rừng nhiệt đới, savan). |

## 5. Năm hoạt động

`k` = số thành viên được cử. Neuron nhận được = `neuron[hoạt động] × k`, nhận cả khi thất bại.

| Hoạt động | Năng lượng | Neuron mỗi người | Mở khóa | Kết quả |
|---|---|---|---|---|
| Kiếm ăn (`gather`) | 1 | 1 | Từ đầu | Mỗi người rút thử từng loài thực vật đã biết; thức ăn là tổng những lần rút trúng (mục 5.1). **Không còn `gather_yield`.** +1 tinh thần. |
| Nâng tinh thần (`morale`) | 1 | 1 | Từ đầu | +2 tinh thần mỗi người. Giai đoạn 1–3: chải lông, chơi đùa; từ giai đoạn 4: quây quần bên lửa (chỉ đổi hình ảnh). |
| Xây dựng (`build`) | 2 | 2 | Từ lượt 3 | Chọn **một** công trình và cử người dọn. Đủ **tổng sức mạnh** thì dọn xong ngay, thiếu thì lượt đó phí — ăn cả ngã về không, **không có điểm xây tích lũy** (mục 5.3). +1 tinh thần. |
| Săn bắt (`hunt`) | 2 | 6 | Từ lượt 3 | Mở combat mini kiểu Dụ và hạ (mục 9). |
| Khám phá (`explore`) | 3 | 6 | Từ lượt 6; còn vùng chưa đến | Mở combat mini kiểu Phòng thủ (mục 9). **Thắng là di cư** — một chiều, và người không được mang theo mất hẳn (mục 12). |

> **D13 (Khoa quyết 2026-11-08).** Ô "Mở khóa" của Xây dựng và Săn bắt đổi từ "Giai đoạn 2 + nút neuron" thành **"Từ lượt 3"** — cùng lượt với trận hướng dẫn ở mục 8: người chơi học đánh rồi được đi săn ngay. Chi tiết và hệ quả còn treo: `docs/decisions.md` D13. Đây là thay đổi duy nhất của file này ngoài bản v6 gốc.

### 5.1 Hái lượm: khám phá thực vật (thay hẳn `gather_yield` của v6)

> **Khác v6 ở chỗ cơ bản nhất:** thức ăn **không suy ra được từ số người bằng bất kỳ phép nhân nào**.
> Nó là tổng của những lần rút trúng, mỗi người rút riêng với mỗi loài **đã khám phá**.

Dữ liệu loài nằm ở `Assets/Data/plants.json`. **Toàn bộ số của bảng dưới là [ĐỀ XUẤT].**

| Loài | `find_chance` | Dinh dưỡng | Vùng | Mùa khô (×) | Tổng nhanh nhẹn để chắc chắn | Trọng số khám phá |
|---|---|---|---|---|---|---|
| Lá non | 0.64 | 3 | 0, 1 | 0.70 | 2.0 | biết sẵn |
| Quả sung | 0.44 | 2 | 0 | 0.50 | 3.0 | 5 |
| Hạt cỏ | 0.40 | 2 | 1 | 0.60 | 3.0 | 4 |
| Củ mài | 0.32 | 3 | 0, 1 | 0.80 | 4.0 | 4 |
| Nấm gỗ | 0.28 | 3 | 0 | 0.45 | 4.5 | 3 |
| Quả bao báp | 0.20 | 5 | 1 | 0.90 | 5.5 | 2 |
| Trứng chim | 0.16 | 5 | 0, 1 | 0.85 | 6.0 | 2 |
| Tổ ong | 0.12 | 7 | 0, 1 | 1.00 | 7.0 | 1 |

Giải quyết một lần Kiếm ăn, theo đúng thứ tự này để ván tái lập được.

**Bước 1 — Khám phá.** Một lần rút cho cả nhóm. Tỉ lệ = `min(0.60, 0.15 + 0.05 × số người)` **[ĐỀ XUẤT]**. Trúng thì rút một loài chưa biết của vùng theo trọng số; loài vừa khám phá **tham gia luôn bước 2 của chính lượt này**.

**Bước 2 — Tìm kiếm.** Mỗi người, với mỗi loài đã biết, một lần rút:

```
p = find_chance × (pha Khô ? dry_multiplier : 1) × hệ_số_độ_khó
p = p + 0.30 × (speed − 1) × (1 − find_chance)      [ĐỀ XUẤT] speed_rare_bonus = 0.30
p = clamp(p, 0, 1)
```

Phần cộng tỉ lệ với `(1 − find_chance)`, nên `speed` là chỉ số của **người đi tìm giỏi**, không phải người mang vác khỏe.

**Bước 3 — Khung đảm bảo.** Với mỗi loài, nếu **tổng nhanh nhẹn của cả nhóm** đạt ngưỡng của loài đó mà chưa ai tìm được nó, nhóm vẫn chắc chắn tìm được **một lần**. Bước này không tiêu lần rút nào.

> ⚠ Ngưỡng là **tổng của nhóm**, không phải chỉ số từng người; và nó cho **một** lần tìm thấy cho **cả nhóm**,
> không phải mỗi người một lần. Đây là chỗ dễ đọc nhầm nhất của cả mục — bản cài đầu tiên của Core đã sai
> cả hai chỗ (`docs/decisions.md` D16).

**Sàn bắt buộc.** Mỗi lượt phải cử ít nhất **⌈1/3 sĩ số⌉** người đi Kiếm ăn (D6 của Khoa: *"để hạn chế việc người chơi phát triển quá nhanh"*). Sàn chặn khi người chơi **có thể** đạt mà **chọn không**; nó không chặn khi họ **không thể**.

**Loài biết sẵn.** Đầu ván chỉ biết lá non. Loài đã khám phá **không nhớ qua ván** (D5); danh mục xuyên ván nằm ngoài menu chính và không ảnh hưởng lối chơi.

### 5.2 Tỉ lệ thắng combat (dùng cho kết quả giản lược và để chỉnh độ khó combat thật)

```
p_hunt    = clamp(0.50 + 0.08 × k + 0.05 × build_level - 0.02 × difficulty_level, 0.05, 0.92)
p_explore = clamp(0.40 + 0.08 × k + 0.05 × build_level - 0.02 × difficulty_level, 0.05, 0.92)
```

| Kết quả | Săn bắt | Khám phá |
|---|---|---|
| Thắng | Thức ăn = k × `hunt_food[stage]` (giai đoạn 2/3/4: 5 / 7 / 10); +3 tinh thần; chiến tích săn +1 | `region` +1 (di cư sang vùng mới); +3 tinh thần; chiến tích di cư +1 |
| Thắng: thương vong | Có thể mất 1 người tùy kiểu tấn công của quái thú (trung bình 50%) | Như Săn bắt |
| Thua | −2 tinh thần; mất 2 người | Như Săn bắt |

### 5.3 Công trình và chỗ ở theo vùng

> **D20, nguyên văn Khoa (09/11):** *"vùng 1 có tối đa 8 chỗ, khởi đầu là 4 chỗ, cần phải thực hiện dọn chỗ
> cho 4 chỗ còn lại, tương tự với các vùng khác"*

**"Chỗ" là chỗ cho THÀNH VIÊN**, tức sức chứa hang — không phải ô đất đặt công trình.

- Mỗi vùng có một **trần chỗ ở** và một **số chỗ mở sẵn** khi bộ tộc vừa tới.
- **Dọn một công trình mở thêm đúng một chỗ** **[ĐỀ XUẤT]**, tới trần của vùng.
- Nên số công trình của một vùng bằng đúng `trần − mở sẵn`. Dữ liệu ở `Assets/Data/sites.json`; nạp sai thì nổ ngay.

| Vùng | Trần chỗ | Mở sẵn | Số công trình |
|---|---|---|---|
| 0 — Rừng nhiệt đới | 8 | 4 | 4 |
| 1 — Savan | 14 | 7 **[ĐỀ XUẤT]** | 7 |

**Cách dọn (D7 của Khoa).** Chọn một công trình, cử người vào. Đủ **tổng sức mạnh** thì dọn xong; thiếu thì lượt đó phí và **không tích lũy gì** — hai lượt thiếu không cộng lại thành một lượt đủ. Không có thanh tiến trình.

**Hiệu lực từ lượt sau.** Xây dựng giải quyết **cuối lượt**, sau cả hai hoạt động mở combat: công trình vừa dọn không nâng tỉ lệ thắng trận của chính lượt đó, cũng không nới sức chứa kịp cho lần sinh cuối lượt (D19).

**Nút neuron mở loại công trình.** `s2_stone_tools` ("Dùng đá đập") mở mọi công trình loại đá (D20/B.3).

Ngưỡng sức mạnh của 11 công trình đều là **[ĐỀ XUẤT]**, từ 1.8 tới 4.8; mốc đặt số là sức mạnh trung bình một người = 1.0, nên "3.0" nghĩa là ba người trung bình vừa đủ.

**Ổ đựng thức ăn** là công trình **nâng cấp nhiều bậc**, không phải dọn một lần. Nó là lối tắt giao diện để xem thực vật đã khám phá và giao người đi Kiếm ăn — **không chứa thức ăn**, nên §4 *"thức ăn không để dành"* giữ nguyên. Mỗi bậc **hạ ngưỡng khung đảm bảo của mọi loài** (E2 của Khoa). Thang bậc **[ĐỀ XUẤT]**:

| Bậc | Sức mạnh cần | Nới ngưỡng |
|---|---|---|
| 1 | 2.0 | 0.2 |
| 2 | 4.0 | 0.5 |
| 3 | 7.0 | 1.0 |

> ⚠ **Vấn đề thiết kế chưa giải, cần Khoa quyết.** Đo 500 ván: bot đạt bậc 1 ở **93,2%** số ván, bậc 2 và 3
> ở **0,0%**. Lý do có tính cấu trúc: bậc 2 cần 4.0 sức mạnh trong khi **luôn còn công trình rẻ hơn**, và dọn
> công trình cho **+1 chỗ ở lẫn +1 `build_level`** còn nâng ổ chỉ cho `build_level`. Nâng ổ là lựa chọn
> **bị trội hoàn toàn**. Ba hướng gỡ: cho ổ mở chỗ như công trình; hạ ngưỡng bậc 2–3 xuống dưới công trình
> rẻ nhất; hoặc cắt bậc 2–3 khỏi MVP.

## 6. Sinh sản

Cuối lượt, sau khi ăn: nếu tinh thần ≥ **4**, còn thành viên và số thành viên < sức chứa hang thì sinh 1 thành viên, trừ 4 tinh thần. Tối đa 1 thành viên mỗi lượt.

## 7. Tiến hóa và mạng lưới neuron

- Mỗi giai đoạn có một mạng lưới riêng gồm 3 nhánh. Mỗi nhánh: 1 nút gốc → 1 nút cổng (bắt buộc), cộng 1 nút tùy chọn nối từ nút gốc.
- Nút "Tiến hóa" của giai đoạn mở được khi đủ 3 nút cổng **và** đủ chiến tích của giai đoạn. Mở nút Tiến hóa thì lên giai đoạn kế; bộ đếm chiến tích săn và thắng trận về 0 (riêng "đã từng di cư" giữ nguyên).
- Neuron tiêu khi mở nút. Neuron dư được giữ sang giai đoạn sau.
- Mỗi lần tiến hóa: đổi ngoại hình toàn bộ bộ tộc (màn ăn mừng riêng), tăng năng lượng tối đa và khẩu phần theo bảng mục 4.

### 7.1 Chiến tích cần để tiến hóa

| Tiến hóa từ | Chiến tích |
|---|---|
| Giai đoạn 1 | Thắng 1 trận bất kỳ (trận hướng dẫn ở lượt 3 được tính) |
| Giai đoạn 2 | Săn thành công 2 lần |
| Giai đoạn 3 | Săn thành công 2 lần, đã từng di cư ít nhất 1 lần |
| Giai đoạn 4 | Săn thành công 3 lần |

### 7.2 Tổng neuron bắt buộc mỗi giai đoạn

| Giai đoạn | Bản đầy đủ | **[MVP]** |
|---|---|---|
| 1 | 85 | 150 |
| 2 | 135 | 250 |
| 3 | 190 | — |
| 4 | 240 | — |

### 7.3 Nút giai đoạn 1: Vượn người

Giá ghi theo **[MVP]**; bản đầy đủ ghi trong ngoặc. Hiệu ứng có số là **[ĐỀ XUẤT]**: mô phỏng chưa tính hiệu ứng nút, nên sau khi thêm phải chạy lại test cân bằng.

| Nhánh | Nút | Loại | Neuron | Hiệu ứng |
|---|---|---|---|---|
| Thân thể | Leo trèo khéo | Gốc | 18 (10) | Kiếm ăn +0.5 thức ăn mỗi người, làm tròn xuống trên tổng |
| Thân thể | Đứng thẳng ngắn | Cổng | 27 (15) | `speed` mọi thành viên +0.05 |
| Thân thể | Hàm khỏe | Tùy chọn | 27 (15) | `strength` mọi thành viên +0.1 |
| Ăn uống | Nhai lá non | Gốc | 18 (10) | Sự kiện "mất mùa trái cây" mất ít hơn 1 thức ăn |
| Ăn uống | Đào rễ củ | Cổng | 27 (15) | `gather_yield` +1 |
| Ăn uống | Ăn côn trùng, trứng | Tùy chọn | 27 (15) | Pha Khô trừ hái lượm ít hơn 1 |
| Xã hội | Chải lông cho nhau | Gốc | 18 (10) | Nâng tinh thần +1 tinh thần mỗi lần làm |
| Xã hội | Tiếng gọi báo động | Cổng | 27 (15) | Tỉ lệ thắng phòng thủ +0.05 |
| Xã hội | Cõng con trên lưng | Tùy chọn | 27 (15) | Sinh thành viên cần 3 tinh thần thay vì 4 |
| — | Tiến hóa: Người vượn phương Nam | Cần 3 cổng + chiến tích | 15 (10) | Lên giai đoạn 2 |

### 7.4 Nút giai đoạn 2: Người vượn phương Nam

| Nhánh | Nút | Loại | Neuron | Hiệu ứng |
|---|---|---|---|---|
| Thân thể | Đi bằng hai chân | Gốc | 28 (15) | `speed` +0.05; đánh nhanh hơn trong combat |
| Thân thể | Bàn chân vòm | Cổng | 42 (25) | Tỉ lệ thắng Khám phá +0.05 |
| Thân thể | Ném đá | Tùy chọn | 37 (20) | Mở phong cách đánh tầm xa trong combat (để sau MVP: chỉ cộng `strength` +0.1) |
| Công cụ, ăn uống | Dùng đá đập | Gốc | 28 (15) | Mở Xây dựng |
| Công cụ, ăn uống | Ăn xác thịt, tủy xương | Cổng | 42 (25) | Mở Săn bắt thú nhỏ |
| Công cụ, ăn uống | Gậy đào | Tùy chọn | 37 (20) | `gather_yield` +1 trong pha Ẩm |
| Xã hội | Chia sẻ thức ăn | Gốc | 28 (15) | Cuối lượt thiếu đúng 1 phần ăn thì không mất người |
| Xã hội | Canh gác luân phiên | Cổng | 42 (25) | Báo trước thú tấn công hang 1 lượt |
| Xã hội | Dạy trẻ | Tùy chọn | 37 (20) | Thành viên mới sinh có `speed`, `strength` +0.05 |
| — | Tiến hóa: Người khéo léo | Cần 3 cổng + chiến tích | 40 (15) | Lên giai đoạn 3 (**[MVP]**: thắng ván) |

Giai đoạn 3 (Người khéo léo: săn thú vừa, công cụ đá tốt hơn) và giai đoạn 4 (Người đứng thẳng: lửa, nấu chín, săn thú lớn theo nhóm) chưa thiết kế nút; ngoài phạm vi MVP.

## 8. Sự kiện

- Lượt 1–5: không có sự kiện ngẫu nhiên. **Lượt 3**: trận combat hướng dẫn với một con vật dễ, luôn thắng, không ai chết: +2 tinh thần, +3 neuron, tính 1 chiến tích thắng trận.
- Từ lượt 6, mỗi lượt máy có **70%** khả năng xảy ra một sự kiện. Rút loại:

| Loại | Xác suất (khi có sự kiện) | Quy tắc |
|---|---|---|
| Không combat: lựa chọn hoặc ngẫu nhiên có lợi/hại | 40% | Kết quả tốt: +4 thức ăn, +2 tinh thần. Xấu: −(3 + `difficulty_level`) thức ăn, −(2 + `difficulty_level`) tinh thần. Người chơi cân nhắc được kết quả tốt khoảng 60%. Sự kiện ngẫu nhiên (không cần chọn) được hiện và áp dụng ở **đầu lượt người chơi kế tiếp**; sự kiện lựa chọn hiện ngay ở lượt máy. |
| Thú tấn công hang (phòng thủ bắt buộc) | 20% pha Ẩm, 25% pha Khô | Mở combat Phòng thủ. Thắng: +2 tinh thần, +3 neuron, +1 chiến tích thắng trận, có thể mất 1 người. Thua: quái thú ăn đúng `quota` người rồi rút, không bao giờ xóa sổ cả bầy. |
| Cơ hội combat (tùy chọn) | Phần còn lại (40% / 35%) | Người chơi chọn tấn công hoặc rút lui. Thắng: +6 thức ăn, +3 tinh thần, +4 neuron, +1 chiến tích thắng trận, có thể mất 1 người. Thua: mất 2 người. Rút lui: không gì xảy ra. |

```
p_defence = clamp(0.35 + 0.05 × crew + 0.06 × build_level + 0.04 × stage - 0.05 × region - 0.02 × difficulty_level, 0.05, 0.95)
quota     = [1, 2, 2, 3, 3, 3][region] + difficulty_level
```

- Thức ăn từ sự kiện cộng vào thức ăn của lượt đang tính (không để dành).
- Sự kiện công bằng với mọi bộ tộc: không ưu ái bộ tộc yếu, không làm khó bộ tộc mạnh.

## 9. Combat mini

- Thời gian thực, khoảng 20–40 giây mỗi trận. Thao tác: chạm hoặc nhấp vào bản đồ trận để thả từng thành viên đã được cử; thành viên và kẻ địch tự đánh theo chỉ số.
- Hai kiểu trận:

| Kiểu | Mục tiêu người chơi | Thua khi | Dùng cho |
|---|---|---|---|
| Dụ và hạ (Lure) | Hạ con thú trước khi nó chạy thoát | Thú chạy ra khỏi khu vực | Săn bắt, cơ hội combat |
| Phòng thủ (Prevent) | Chặn kẻ địch trước khi tới hang hoặc mục tiêu | Kẻ địch chạm mục tiêu | Thú tấn công hang, Khám phá |

- Kiểu tấn công của quái thú quyết định ai chết. Người chơi chỉ biết kiểu của một loài sau lần đầu gặp nó.

| Kiểu tấn công | Nhắm vào |
|---|---|
| Săn kẻ yếu | Thành viên `hp` thấp nhất |
| Ngẫu nhiên | Bất kỳ ai |
| Thách đấu kẻ mạnh | Thành viên `strength` cao nhất |
| Bắt kẻ lẻ loi | Thành viên đứng xa đồng đội nhất (*thêm theo D21, Khoa 09/11; định nghĩa chính xác ở `docs/bestiary.md` §3*) |

- Người chết trong trận là chết thật, kể cả khi thắng. Phòng thủ thua: quái thú ăn đủ `quota` rồi rút.
- Combat phải được hiệu chỉnh sao cho tỉ lệ thắng thực tế xấp xỉ công thức ở mục 5.2 và 8; độ lệch quá 10 điểm % so với công thức phải báo Khoa.
- Đầu ra của combat cho Core: `{ won: bool, deadMemberIds: [...] }`. Trước khi có combat thật, Core dùng bộ giải quyết xác suất theo công thức (`ICombatResolver`).

**Loài quái thú cho MVP** (3 loài gốc là [ĐỀ XUẤT]; trăn và đại bàng thêm theo D21, Khoa duyệt 09/11):

| Loài | Vùng | Kiểu tấn công | Ghi chú |
|---|---|---|---|
| Báo hoa mai | Rừng nhiệt đới | Săn kẻ yếu | Rình rập, tấn công hang |
| Linh cẩu (bầy) | Savan | Ngẫu nhiên | Nhiều con nhỏ |
| Hổ răng kiếm (Megantereon) | Savan | Thách đấu kẻ mạnh | Nguy hiểm nhất, cơ hội combat thưởng lớn |
| Trăn đá châu Phi | Rừng nhiệt đới | Bắt kẻ lẻ loi | Phục kích, tấn công hang (D21) |
| Đại bàng lớn | Savan | Bắt kẻ lẻ loi | Tấn công hang. MVP-2: cắt đầu tiên nếu trễ (D21) |

Con mồi săn bắt giai đoạn 2 (thú nhỏ, không đánh trả mạnh): **[ĐỀ XUẤT]** linh dương nhỏ, lợn rừng con, thỏ rừng.

> **Bổ sung 09/11 (D21).** Ba hệ mới của combat được đặc tả trong `docs/bestiary.md`:
> - Kiểu nhắm "Bắt kẻ lẻ loi" (§3).
> - Trạng thái trong trận: Rình, Choáng, Bỏ chạy…; xem §4.
> - Hồ sơ lịch sử và thành tựu hiện sau mỗi trận, cùng màn Bảo tàng (§5).
> 
> Khoa đã duyệt toàn bộ đề xuất của file đó ngày 09/11 (`bestiary.md` §9); trăn đá và đại bàng đã vào bảng loài ở trên.

## 10. Khí hậu

- Pha Ẩm và pha Khô xen kẽ, mỗi pha **10** lượt: lượt 1–10 Ẩm, 11–20 Khô, 21–30 Ẩm…
- Pha Khô: hái lượm giảm (mục 5.1); thú tấn công hang nhiều hơn (mục 8).
- Báo trước 2 lượt khi sắp đổi pha (cây cỏ úa dần, bầu trời đổi màu).
- Cơ sở thiết kế: giả thuyết "variability selection" của Rick Potts (khí hậu Đông Phi dao động giữa mưa nhiều và hạn hán).

## 11. Độ khó tăng dần

```
difficulty_level = floor(turn / 20)        [MVP]: floor(turn / 12)
```

Mỗi bậc: tỉ lệ thắng mọi combat −2 điểm %; quái thú tấn công hang ăn thêm 1 người; sự kiện xấu thêm −1 thức ăn và −1 tinh thần. Độ khó tăng theo thời gian, không theo sức mạnh thực tế của bộ tộc.

**Hái lượm:** v6 trừ thẳng −1 thức ăn mỗi người. v7 **nhân** vào mọi tỉ lệ tìm thấy:

```
hệ_số_độ_khó = max(0, 1 − 0.18 × difficulty_level)      [ĐỀ XUẤT] balance-log.md #10
```

Đây là cơ chế duy nhất kết thúc những ván ở lì. Hệ số −0.18 được chọn **bằng phép đo**, không phải bằng cảm nhận: quét −0.13 / −0.15 / −0.16 / −0.18 cho tỉ lệ thắng 45,4 / 41,6 / 42,6 / 35,2%.

## 12. Vùng đất

| region | Vùng | Ghi chú |
|---|---|---|
| 0 | Rừng nhiệt đới | Khởi đầu |
| 1 | Savan | **[MVP]** vùng cuối |
| 2+ | Sa mạc, đồng bằng sông Nile, … | Ngoài MVP |

Mỗi vùng: hệ sinh thái, quái thú, lợi và bất lợi riêng (ngoài các con số chung ở trên, chi tiết từng vùng ngoài MVP), cùng trần chỗ ở và bộ công trình riêng (§5.3).

**Di cư = thắng Khám phá.** Không có bước xác nhận riêng. Ba luật của Khoa (09/11, `decisions.md` D25):

> *"người ở lại mất luôn, đây là cơ chế giảm sức mạnh người chơi ở khu vực cũ"*
> *"Miễn khu vực mới có đủ slot cho số người di cư thì bao nhiêu người trong bộ tộc đều được lựa chọn đi được"*
> *"cộng đồng ở vùng đất mới chính là cộng đồng chính, nó là lối đi 1 chiều không có đường trở lại"*

- **Một chiều.** `region` chỉ tăng; không có đường về.
- **Trần đoàn đi = số chỗ vùng mới mở sẵn.** Không có giới hạn "4–6 người" — con số đó là ví dụ, không phải luật.
- **Người không được mang theo mất hẳn.** Họ **không phải người chết**: không trừ tinh thần theo `morale.on_member_death`, không có tang. Người sinh ra trong chính lượt di cư được đi theo nếu còn chỗ.
- **Công trình thuộc về vùng cũ**, nên `build_level` tụt về 0 (trừ bậc Ổ) và sức chứa nhận số của vùng mới.
- Di cư là **việc cuối cùng** của lượt: cả bộ tộc — kể cả những người sắp bị bỏ lại — vẫn ăn phần của lượt đó.

> ⚠ **Thắng một trận có thể giết ván.** Cử 3 người đi khám phá, thắng, rồi phải bỏ lại 4 người và còn đúng 3 — giao với ngưỡng thua ≤ 1 thì ván gần như xong. Đó là luật, nhưng nó **phải nhìn thấy được trước khi bấm**: Core phơi số người sẽ bị bỏ lại thành một truy vấn không tác dụng phụ.

## 13. Hướng dẫn trong game

Không có màn hướng dẫn dài. Mỗi gợi ý tối đa 2 câu, hiện cạnh thứ nó nói tới, tắt được, xem lại trong mục tra cứu.

| Lúc | Gợi ý |
|---|---|
| Lượt 1 | Cử người đi Kiếm ăn; đọc dự báo "cuối lượt cần X, đang có Y" |
| Lượt 2 | Nâng tinh thần để sinh thêm thành viên |
| Lượt 3 | Trận combat hướng dẫn: chạm để thả người |
| Lần đầu đủ neuron | Mạng lưới neuron; nút cổng và chiến tích |
| Lượt 6 | Khám phá mở khóa |
| Lượt 11 (pha Khô đầu tiên) | Hạn hán: hái lượm giảm, nên đi săn hoặc di cư |
| Lần đầu gặp một loài | Kiểu tấn công của nó |

## 14. Khác

- Lưu ván: có (trạng thái ván + trạng thái bộ sinh số ngẫu nhiên).
- Quảng cáo: không có trong MVP. Về sau chỉ dùng quảng cáo có thưởng do người chơi tự chọn (hồi sinh tối đa 1 lần mỗi ván, nhân đôi thưởng sau combat thắng); không quảng cáo bắt buộc, không giới hạn số ván.

## 15. Cân bằng tham chiếu

Đo bằng C8 (`BalanceTests`), 2.000 ván mỗi bot, chế độ **[MVP]**, `applyNodeEffects = false`, sai số chuẩn ≈ 1,1 điểm %. Số ngày 09/11/2026:

| Bot | Thắng | Trung vị | p10 | p90 | Thua trước lượt 5 | Trước lượt 10 |
|---|---|---|---|---|---|---|
| Cân nhắc (balanced) | **33,2%** | 51 | 48 | 59 | 2,1% | 5,6% |
| Né combat (safe) | 0,0% | — | — | — | 0,0% | 0,1% |
| Liều (reckless) | 0,4% | 25 | 22 | 32 | 4,2% | 62,4% |
| Chọn bừa (careless) | 7,1% | 33 | 28 | 40 | 2,1% | 34,6% |

Hai tiêu chí **ngang hàng** (`HANDOFF.md` Cổng 2 và `decisions.md` D23): balanced thắng 30–45% với trung vị 42–52 lượt; safe ≤ 2%; reckless và careless ≤ 20%; **và** balanced thua trước lượt 10 ≤ 10%. Cả sáu đều đạt.

> ⚠ Bật hiệu ứng nút neuron thì balanced thắng **52,1%**, trên mốc 45%. Dải Cổng 2 đo ở `false` nên đây không phải chỉ tiêu trượt, nhưng hiệu ứng nút đang đáng **+16 điểm %** — con số cần Khoa xem.

> Số cũ của `before_us_sim.py` (bot cân nhắc ~38%, ~47 lượt) là của **v6** và giữ lại ở đây chỉ để đối chiếu lịch sử. Mô phỏng không biết thực vật, công trình theo vùng hay di cư có chọn người.
