# -*- coding: utf-8 -*-
"""Gop design-v7-draft.md vao design.md bang phau thuat: chi thay nhung muc v7 that su thay the.
Moi phep thay deu assert count == 1, nen khong co gi doi ngoai y muon."""
import io, hashlib, sys

PATH = "docs/design.md"
EXPECTED = "075af8cda001aa5ce7f3037c5f51c5c850c263bafab3d31913e2e361a88cbbd0"

raw = io.open(PATH, "rb").read()
actual = hashlib.sha256(raw).hexdigest()
if actual != EXPECTED:
    sys.exit("DUNG: design.md da doi tu luc kiem. mong doi %s, gap %s" % (EXPECTED, actual))

s = raw.decode("utf-8")
edits = []


def sub(label, old, new):
    global s
    if s.count(old) != 1:
        sys.exit("DUNG: '%s' khong khop duy nhat (gap %d)" % (label, s.count(old)))
    s = s.replace(old, new)
    edits.append(label)


# ---------- 0. Tieu de va chu dan ----------
sub("tiêu đề → v7",
    "# Before Us — Đặc tả luật chơi (v6)\n\n"
    "> Nguồn sự thật cho luật chơi. Mọi con số khớp với `tools/sim/before_us_sim.py` (biến `P` và `P_MVP`).",

    "# Before Us — Đặc tả luật chơi (v7)\n\n"
    "> Nguồn sự thật cho luật chơi.\n"
    ">\n"
    "> **v7 (09/11/2026)** gộp `docs/design-v7-draft.md` vào đây: hái lượm có khám phá thực vật (§5.1), "
    "công trình và chỗ ở theo vùng (§5.3), di cư một chiều (§12). Bản nháp đó nay **đã được hấp thụ** — "
    "nó còn giữ phần hỏi–đáp với Khoa và các câu còn mở, nhưng luật hiện hành đọc ở file này.\n"
    ">\n"
    "> ⚠ **`tools/sim/before_us_sim.py` nay mô tả một trò chơi KHÁC.** Nó dựng theo v6 và không biết "
    "thực vật, công trình theo vùng hay di cư có chọn người. Số của nó chỉ còn giá trị lịch sử; "
    "nguồn cân bằng hiện hành là C8 trong `Assets/Scripts/Core.Tests/BalanceTests.cs` "
    "(xem §15 và `docs/balance-log.md`).")

sub("chú dẫn nhãn",
    "> Mục đánh dấu **[ĐỀ XUẤT]** là giá trị chưa được người thiết kế (Khoa) chốt: được dùng để chạy, "
    "nhưng phải liệt kê trong báo cáo để Khoa duyệt.",

    "> Mục đánh dấu **[ĐỀ XUẤT]** là giá trị chưa được người thiết kế (Khoa) chốt: được dùng để chạy, "
    "nhưng phải liệt kê trong báo cáo để Khoa duyệt. Từ 09/11, các con số cân bằng Khoa đã **ủy quyền** "
    "cho Supervisor chỉnh (`docs/decisions.md` D22) vẫn mang nhãn này — chúng được quyết bằng phép đo, "
    "không phải bằng chữ ký của Khoa, và Khoa lật lại lúc nào cũng được. Mọi lần chỉnh ghi ở "
    "`docs/balance-log.md`.")

# ---------- 1. §3 si so khoi dau ----------
sub("§3 sĩ số 5 → 4",
    "- Bắt đầu ván với **5** thành viên.",
    "- Bắt đầu ván với **4** thành viên **[ĐỀ XUẤT]** — đúng bằng số chỗ ở mở sẵn của vùng 0 (§5.3). "
    "Trước 09/11 là 5, đổi vì 5 người / 4 chỗ là hai luật loại trừ nhau; xem `docs/balance-log.md` #3.")

# ---------- 2. §3 speed khong con nhan vao hai luom ----------
sub("§3 speed",
    "  - `speed` **[ĐỀ XUẤT]** hệ số 0.8–1.2: nhân vào lượng hái lượm của người đó và tốc độ đánh trong combat.",
    "  - `speed` **[ĐỀ XUẤT]** hệ số 0.8–1.2: tốc độ đánh trong combat, và hai việc trong hái lượm (§5.1) — "
    "tăng cơ hội tìm thấy **loài hiếm**, và góp vào **tổng nhanh nhẹn của nhóm** để đạt khung đảm bảo. "
    "**Không còn nhân vào lượng thức ăn** (D4 của Khoa, playtest 01).")

# ---------- 3. §4 khau phan ----------
sub("§4 khẩu phần",
    "| Khẩu phần | Mỗi thành viên cần `upkeep[stage]` = 1 / 1 / 2 / 3 (giai đoạn 1/2/3/4) thức ăn mỗi lượt. |",
    "| Khẩu phần | Mỗi thành viên cần `upkeep[stage]` = 1 / **2** / 2 / 3 (giai đoạn 1/2/3/4) thức ăn mỗi lượt. "
    "**[ĐỀ XUẤT]** giai đoạn 2 nâng từ 1 lên 2 ngày 09/11 (`balance-log.md` #9). |")

# ---------- 4. §4 suc chua hang ----------
sub("§4 sức chứa hang",
    "| Công trình | Cấp công trình `build_level`, bắt đầu 0. Sức chứa hang = 6 + 2 × `build_level`. |",
    "| Công trình | Cấp công trình `build_level` = số công trình đã dọn ở vùng hiện tại + bậc Ổ đựng thức ăn. "
    "**Số suy ra, không có bộ đặt.** Dùng cho §5.2 và §8. |\n"
    "| Chỗ ở | Sức chứa hang = số chỗ vùng mở sẵn + số công trình đã dọn, chặn ở trần của vùng (§5.3). "
    "**Công thức `6 + 2 × build_level` của v6 đã bị thay** (D20). |")

# ---------- 5. §5 ba o "Ket qua" ----------
sub("§5 ô Kiếm ăn",
    "| Kiếm ăn (`gather`) | 1 | 1 | Từ đầu | Luôn thành công. Mỗi người được `gather_yield` thức ăn (mục 5.1). +1 tinh thần. |",
    "| Kiếm ăn (`gather`) | 1 | 1 | Từ đầu | Mỗi người rút thử từng loài thực vật đã biết; thức ăn là tổng "
    "những lần rút trúng (mục 5.1). **Không còn `gather_yield`.** +1 tinh thần. |")

sub("§5 ô Xây dựng",
    "| Xây dựng (`build`) | 2 | 2 | Từ lượt 3 | Mỗi người +1 điểm xây. Đủ 4 × (`build_level` + 1) điểm thì `build_level` +1 và điểm về 0. +1 tinh thần. |",
    "| Xây dựng (`build`) | 2 | 2 | Từ lượt 3 | Chọn **một** công trình và cử người dọn. Đủ **tổng sức mạnh** "
    "thì dọn xong ngay, thiếu thì lượt đó phí — ăn cả ngã về không, **không có điểm xây tích lũy** (mục 5.3). "
    "+1 tinh thần. |")

sub("§5 ô Khám phá",
    "| Khám phá (`explore`) | 3 | 6 | Từ lượt 6; còn vùng chưa đến | Mở combat mini kiểu Phòng thủ để chiếm vùng mới (mục 9). |",
    "| Khám phá (`explore`) | 3 | 6 | Từ lượt 6; còn vùng chưa đến | Mở combat mini kiểu Phòng thủ (mục 9). "
    "**Thắng là di cư** — một chiều, và người không được mang theo mất hẳn (mục 12). |")

# ---------- 6. §5.1 thay ca muc, va them §5.3 ----------
OLD_51 = """### 5.1 Lượng hái lượm mỗi người

```
gather_yield = 3 + region (+1 nếu stage >= 4, nhờ lửa)
nếu pha Khô:  gather_yield = max(1, gather_yield - dry_penalty[stage])   dry_penalty = 1 / 1 / 2 / 2
gather_yield = max(0, gather_yield - difficulty_level)
thực nhận của mỗi người = gather_yield × speed (làm tròn; mô phỏng dùng speed = 1)
```
"""

NEW_51 = """### 5.1 Hái lượm: khám phá thực vật (thay hẳn `gather_yield` của v6)

> **Khác v6 ở chỗ cơ bản nhất:** thức ăn **không suy ra được từ số người bằng bất kỳ phép nhân nào**.
> Nó là tổng của những lần rút trúng, mỗi người rút riêng với mỗi loài **đã khám phá**.

Dữ liệu loài nằm ở `Assets/Data/plants.json`. **Toàn bộ số của bảng dưới là [ĐỀ XUẤT].**

| Loài | `find_chance` | Dinh dưỡng | Vùng | Mùa khô (×) | Tổng nhanh nhẹn để chắc chắn | Trọng số khám phá |
| --- | --- | --- | --- | --- | --- | --- |
| Lá non | 0.64 | 3 | 0, 1 | 0.70 | 2.0 | biết sẵn |
| Quả sung | 0.44 | 2 | 0 | 0.50 | 3.0 | 5 |
| Hạt cỏ | 0.40 | 2 | 1 | 0.60 | 3.0 | 4 |
| Củ mài | 0.32 | 3 | 0, 1 | 0.80 | 4.0 | 4 |
| Nấm gỗ | 0.28 | 3 | 0 | 0.45 | 4.5 | 3 |
| Quả bao báp | 0.20 | 5 | 1 | 0.90 | 5.5 | 2 |
| Trứng chim | 0.16 | 5 | 0, 1 | 0.85 | 6.0 | 2 |
| Tổ ong | 0.12 | 7 | 0, 1 | 1.00 | 7.0 | 1 |

Giải quyết một lần Kiếm ăn, theo đúng thứ tự này để ván tái lập được:

1. **Khám phá** — một lần rút cho cả nhóm. Tỉ lệ = `min(0.60, 0.15 + 0.05 × số người)` **[ĐỀ XUẤT]**.
   Trúng thì rút một loài chưa biết của vùng theo trọng số; loài vừa khám phá **tham gia luôn bước 2 của chính lượt này**.
2. **Tìm kiếm** — mỗi người, với mỗi loài đã biết, một lần rút:

```
p = find_chance × (pha Khô ? dry_multiplier : 1) × hệ_số_độ_khó
p = p + 0.30 × (speed − 1) × (1 − find_chance)      [ĐỀ XUẤT] speed_rare_bonus = 0.30
p = clamp(p, 0, 1)
```

   Phần cộng tỉ lệ với `(1 − find_chance)`, nên `speed` là chỉ số của **người đi tìm giỏi**, không phải người mang vác khỏe.

3. **Khung đảm bảo** — với mỗi loài, nếu **tổng nhanh nhẹn của cả nhóm** đạt ngưỡng của loài đó mà chưa ai
   tìm được nó, nhóm vẫn chắc chắn tìm được **một lần**. Bước này không tiêu lần rút nào.

> ⚠ Ngưỡng là **tổng của nhóm**, không phải chỉ số từng người; và nó cho **một** lần tìm thấy cho **cả nhóm**,
> không phải mỗi người một lần. Đây là chỗ dễ đọc nhầm nhất của cả mục — bản cài đầu tiên của Core đã sai
> cả hai chỗ (`docs/decisions.md` D16).

**Sàn bắt buộc.** Mỗi lượt phải cử ít nhất **⌈1/3 sĩ số⌉** người đi Kiếm ăn (D6 của Khoa: *"để hạn chế việc
người chơi phát triển quá nhanh"*). Sàn chặn khi người chơi **có thể** đạt mà **chọn không**; nó không chặn
khi họ **không thể**.

**Loài biết sẵn.** Đầu ván chỉ biết lá non. Loài đã khám phá **không nhớ qua ván** (D5); danh mục xuyên ván
nằm ngoài menu chính và không ảnh hưởng lối chơi.
"""

sub("§5.1 thay cả mục", OLD_51, NEW_51)

# ---------- 7. them §5.3 sau §5.2 ----------
ANCHOR_53 = """| Thua | −2 tinh thần; mất 2 người | Như Săn bắt |

## 6. Sinh sản"""

NEW_53 = """| Thua | −2 tinh thần; mất 2 người | Như Săn bắt |

### 5.3 Công trình và chỗ ở theo vùng

> **D20, nguyên văn Khoa (09/11):** *"vùng 1 có tối đa 8 chỗ, khởi đầu là 4 chỗ, cần phải thực hiện dọn chỗ
> cho 4 chỗ còn lại, tương tự với các vùng khác"*

**"Chỗ" là chỗ cho THÀNH VIÊN**, tức sức chứa hang — không phải ô đất đặt công trình.

- Mỗi vùng có một **trần chỗ ở** và một **số chỗ mở sẵn** khi bộ tộc vừa tới.
- **Dọn một công trình mở thêm đúng một chỗ** **[ĐỀ XUẤT]**, tới trần của vùng.
- Nên số công trình của một vùng bằng đúng `trần − mở sẵn`. Dữ liệu ở `Assets/Data/sites.json`; nạp sai thì nổ ngay.

| Vùng | Trần chỗ | Mở sẵn | Số công trình |
| --- | --- | --- | --- |
| 0 — Rừng nhiệt đới | 8 | 4 | 4 |
| 1 — Savan | 14 | 7 **[ĐỀ XUẤT]** | 7 |

**Cách dọn (D7 của Khoa).** Chọn một công trình, cử người vào. Đủ **tổng sức mạnh** thì dọn xong; thiếu thì
lượt đó phí và **không tích lũy gì** — hai lượt thiếu không cộng lại thành một lượt đủ. Không có thanh tiến trình.

**Hiệu lực từ lượt sau.** Xây dựng giải quyết **cuối lượt**, sau cả hai hoạt động mở combat: công trình vừa
dọn không nâng tỉ lệ thắng trận của chính lượt đó, cũng không nới sức chứa kịp cho lần sinh cuối lượt (D19).

**Nút neuron mở loại công trình.** `s2_stone_tools` ("Dùng đá đập") mở mọi công trình loại đá (D20/B.3).

Ngưỡng sức mạnh của 11 công trình đều là **[ĐỀ XUẤT]**, từ 1.8 tới 4.8; mốc đặt số là sức mạnh trung bình
một người = 1.0, nên "3.0" nghĩa là ba người trung bình vừa đủ.

**Ổ đựng thức ăn** là công trình **nâng cấp nhiều bậc**, không phải dọn một lần. Nó là lối tắt giao diện để
xem thực vật đã khám phá và giao người đi Kiếm ăn — **không chứa thức ăn**, nên §4 *"thức ăn không để dành"*
giữ nguyên. Mỗi bậc **hạ ngưỡng khung đảm bảo của mọi loài** (E2 của Khoa). Thang bậc **[ĐỀ XUẤT]**:

| Bậc | Sức mạnh cần | Nới ngưỡng |
| --- | --- | --- |
| 1 | 2.0 | 0.2 |
| 2 | 4.0 | 0.5 |
| 3 | 7.0 | 1.0 |

> ⚠ **Vấn đề thiết kế chưa giải, cần Khoa quyết.** Đo 500 ván: bot đạt bậc 1 ở **93,2%** số ván, bậc 2 và 3
> ở **0,0%**. Lý do có tính cấu trúc: bậc 2 cần 4.0 sức mạnh trong khi **luôn còn công trình rẻ hơn**, và dọn
> công trình cho **+1 chỗ ở lẫn +1 `build_level`** còn nâng ổ chỉ cho `build_level`. Nâng ổ là lựa chọn
> **bị trội hoàn toàn**. Ba hướng gỡ: cho ổ mở chỗ như công trình; hạ ngưỡng bậc 2–3 xuống dưới công trình
> rẻ nhất; hoặc cắt bậc 2–3 khỏi MVP.

## 6. Sinh sản"""

sub("§5.3 thêm mới", ANCHOR_53, NEW_53)

# ---------- 8. §11 bao mon do kho ----------
sub("§11 bào mòn",
    "Mỗi bậc: tỉ lệ thắng mọi combat −2 điểm %; quái thú tấn công hang ăn thêm 1 người; sự kiện xấu thêm "
    "−1 thức ăn và −1 tinh thần; hái lượm −1 thức ăn mỗi người (có thể về 0). Độ khó tăng theo thời gian, "
    "không theo sức mạnh thực tế của bộ tộc.",

    "Mỗi bậc: tỉ lệ thắng mọi combat −2 điểm %; quái thú tấn công hang ăn thêm 1 người; sự kiện xấu thêm "
    "−1 thức ăn và −1 tinh thần. Độ khó tăng theo thời gian, không theo sức mạnh thực tế của bộ tộc.\n\n"
    "**Hái lượm:** v6 trừ thẳng −1 thức ăn mỗi người. v7 **nhân** vào mọi tỉ lệ tìm thấy:\n\n"
    "```\nhệ_số_độ_khó = max(0, 1 − 0.18 × difficulty_level)      [ĐỀ XUẤT] balance-log.md #10\n```\n\n"
    "Đây là cơ chế duy nhất kết thúc những ván ở lì. Hệ số −0.18 được chọn **bằng phép đo**, không phải "
    "bằng cảm nhận: quét −0.13 / −0.15 / −0.16 / −0.18 cho tỉ lệ thắng 45,4 / 41,6 / 42,6 / 35,2%.")

# ---------- 9. §12 di cu ----------
sub("§12 di cư",
    "Mỗi vùng: hệ sinh thái, quái thú, lợi và bất lợi riêng (ngoài các con số chung ở trên, chi tiết từng vùng "
    "ngoài MVP). Di cư = thắng Khám phá; bộ tộc chuyển hẳn sang vùng mới.",

    "Mỗi vùng: hệ sinh thái, quái thú, lợi và bất lợi riêng (ngoài các con số chung ở trên, chi tiết từng vùng "
    "ngoài MVP), cùng trần chỗ ở và bộ công trình riêng (§5.3).\n\n"
    "**Di cư = thắng Khám phá.** Không có bước xác nhận riêng. Ba luật của Khoa (09/11, `decisions.md` D25):\n\n"
    "> *\"người ở lại mất luôn, đây là cơ chế giảm sức mạnh người chơi ở khu vực cũ\"*\n"
    "> *\"Miễn khu vực mới có đủ slot cho số người di cư thì bao nhiêu người trong bộ tộc đều được lựa chọn đi được\"*\n"
    "> *\"cộng đồng ở vùng đất mới chính là cộng đồng chính, nó là lối đi 1 chiều không có đường trở lại\"*\n\n"
    "- **Một chiều.** `region` chỉ tăng; không có đường về.\n"
    "- **Trần đoàn đi = số chỗ vùng mới mở sẵn.** Không có giới hạn \"4–6 người\" — con số đó là ví dụ, không phải luật.\n"
    "- **Người không được mang theo mất hẳn.** Họ **không phải người chết**: không trừ tinh thần theo "
    "`morale.on_member_death`, không có tang. Người sinh ra trong chính lượt di cư được đi theo nếu còn chỗ.\n"
    "- **Công trình thuộc về vùng cũ**, nên `build_level` tụt về 0 (trừ bậc Ổ) và sức chứa nhận số của vùng mới.\n"
    "- Di cư là **việc cuối cùng** của lượt: cả bộ tộc — kể cả những người sắp bị bỏ lại — vẫn ăn phần của lượt đó.\n\n"
    "> ⚠ **Thắng một trận có thể giết ván.** Cử 3 người đi khám phá, thắng, rồi phải bỏ lại 4 người và còn "
    "đúng 3 — giao với ngưỡng thua ≤ 1 thì ván gần như xong. Đó là luật, nhưng nó **phải nhìn thấy được trước "
    "khi bấm**: Core phơi số người sẽ bị bỏ lại thành một truy vấn không tác dụng phụ.")

# ---------- 10. §15 can bang tham chieu ----------
sub("§15 cân bằng",
    "## 15. Cân bằng tham chiếu (từ mô phỏng)\n\n"
    "| Chế độ | Bot cân nhắc | Bot né combat | Bot liều | Bot chọn bừa |\n"
    "| --- | --- | --- | --- | --- |\n"
    "| **[MVP]** thắng ở giai đoạn 3 | ~38%, ~47 lượt (44–52) | 0% | ~12% | ~8% |\n"
    "| Bản đầy đủ | ~40%, ~50 lượt (46–58) | 0% | ~1% | ~0.5% |",

    "## 15. Cân bằng tham chiếu\n\n"
    "Đo bằng C8 (`BalanceTests`), 2.000 ván mỗi bot, chế độ **[MVP]**, `applyNodeEffects = false`, "
    "sai số chuẩn ≈ 1,1 điểm %. Số ngày 09/11/2026:\n\n"
    "| Bot | Thắng | Trung vị | p10 | p90 | Thua trước lượt 5 | Trước lượt 10 |\n"
    "| --- | --- | --- | --- | --- | --- | --- |\n"
    "| Cân nhắc (balanced) | **33,2%** | 51 | 48 | 59 | 2,1% | 5,6% |\n"
    "| Né combat (safe) | 0,0% | — | — | — | 0,0% | 0,1% |\n"
    "| Liều (reckless) | 0,4% | 25 | 22 | 32 | 4,2% | 62,4% |\n"
    "| Chọn bừa (careless) | 7,1% | 33 | 28 | 40 | 2,1% | 34,6% |\n\n"
    "Hai tiêu chí **ngang hàng** (`HANDOFF.md` Cổng 2 và `decisions.md` D23): balanced thắng 30–45% với "
    "trung vị 42–52 lượt; safe ≤ 2%; reckless và careless ≤ 20%; **và** balanced thua trước lượt 10 ≤ 10%. "
    "Cả sáu đều đạt.\n\n"
    "> ⚠ Bật hiệu ứng nút neuron thì balanced thắng **52,1%**, trên mốc 45%. Dải Cổng 2 đo ở `false` nên đây "
    "không phải chỉ tiêu trượt, nhưng hiệu ứng nút đang đáng **+16 điểm %** — con số cần Khoa xem.\n\n"
    "> Số cũ của `before_us_sim.py` (bot cân nhắc ~38%, ~47 lượt) là của **v6** và giữ lại ở đây chỉ để đối "
    "chiếu lịch sử. Mô phỏng không biết thực vật, công trình theo vùng hay di cư có chọn người.")

io.open(PATH, "w", encoding="utf-8", newline="\n").write(s)
print("Da ap dung %d phep thay:" % len(edits))
for e in edits:
    print("  -", e)
