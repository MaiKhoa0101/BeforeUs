# Before Us — Sinh vật, lịch sử và combat có trạng thái (bestiary v1)

> **Trạng thái:** đặc tả để agent đọc và thiết kế. Viết ngày 2026-11-09 theo yêu cầu của Khoa.
> Quyết định của Khoa ghi ở §0 và ở `docs/decisions.md` D21.
> **Khoa duyệt toàn bộ đề xuất ở §9 (09/11/2026, 01:07): "theo đề xuất của bạn hết".** Danh sách loài ở §2.2 đã chốt.
> Vẫn là **[ĐỀ XUẤT]**: mọi con số (B3 hiệu chỉnh) và từng câu chữ trong codex (giọng văn đã được giữ, câu cụ thể vẫn là nháp).
> Sự thật lịch sử chỉ lấy từ nguồn ở §10. Agent **không tự thêm** sự thật không có nguồn.
> File này bổ sung `docs/design.md` §9 (Combat mini), không thay nó. Nếu hai file mâu thuẫn: **dừng và hỏi** (theo `AGENTS.md`).

---

## 0. Khoa đã quyết gì (09/11/2026)

Nguyên văn:

> "rất thú vị, bổ sung thêm nhiều sinh vật và thông tin nữa, tôi muốn những sinh vật này sau khi giao chiến thì sẽ hiện thành tựu và chi tiết về sự kiện trong lịch sử cho người chơi tìm hiểu. cơ chế tranh mồi thì để vào mục phát triển, chưa vội tham, cơ chế bắt kẻ lẻ loi thì hợp lí, cho vào luôn phát triển hiện tại. trạng thái của quái thú rất thú vị, giảm nhàm chán cho combat, xem xét đưa vào luôn. Tất cả viết vào tài liệu cho tôi để agent đọc và thiết kế"

| Nội dung | Khoa quyết | Phạm vi |
| --- | --- | --- |
| Thêm nhiều sinh vật và thông tin lịch sử | Có | Dữ liệu cho cả 5 giai đoạn (§2). Trong MVP, chỉ loài ở §2.2 có combat. |
| Sau combat hiện thành tựu và chi tiết sự kiện lịch sử | Có | **MVP** (§5) |
| Kiểu tấn công "Bắt kẻ lẻ loi" | Đưa vào phát triển hiện tại | **MVP** (§3) |
| Trạng thái quái thú | "Xem xét đưa vào luôn" | **MVP** theo bậc ưu tiên ở §4.5; được cắt theo thứ tự ở §7 |
| Tranh mồi | Để vào mục phát triển, chưa làm | **Ngoài MVP** (§8) |

Quyết định này **không** dời hai mốc đang có:
- 15/11: màn hình cộng đồng phải chơi được (D15).
- 28/11: phát hành (`HANDOFF.md` §1: trễ thì cắt nội dung, không lùi lịch).

Việc trong file này xếp sau hai mốc đó. Nếu thiếu thời gian, cắt theo §7.

---

## 1. Đọc nhanh cho agent

- **Ba hệ mới:**
  1. Kiểu nhắm mục tiêu thứ tư: `isolated` (§3).
  2. Trạng thái trong trận (§4).
  3. Hồ sơ lịch sử (codex) và thành tựu, hiện sau mỗi trận (§5).
- **Không thêm loài** ngoài bảng §2.2. Các loài khác ở §2 là dữ liệu cho bản đầy đủ. Agent không dựng combat cho chúng trong MVP.
- **File dữ liệu:**
  - `Assets/Data/beasts.json`: thêm `attack_style: "isolated"`, khối `states`, trường `behaviors` và `codex_id` cho mỗi loài (§4.6).
  - `Assets/Data/codex.vi.json` (**mới**): chữ lịch sử hiện cho người chơi (§5.4).
  - `Assets/Data/achievements.json` (**mới**): điều kiện thành tựu (§5.3).
- **Lưu ý loader:** loader của `beasts.json` báo lỗi khi gặp trường lạ (`MissingMemberHandling.Error`, `Required.Always`). Vì vậy phải sửa `BeastData.cs` trước, rồi mới thêm trường vào JSON.
- **Không đổi** `balance.json` và **không đổi** hợp đồng `CombatResult { Won; DeadMemberIds }` của Core. Thông tin thêm cho thành tựu đi qua `BattleReport` của nhánh combat (§5.3).
- **Hiệu chỉnh lại:** mọi trạng thái và kiểu nhắm mới đều làm đổi tỉ lệ thắng thật. Phải chạy lại công cụ hiệu chỉnh B3, và báo Khoa nếu lệch quá 10 điểm % so với công thức (`design.md` §9).
- **Tắt được:** mỗi trạng thái có cờ `enabled` trong JSON, để tắt bằng dữ liệu nếu không hiệu chỉnh kịp.
- **Ngẫu nhiên:** chỉ qua `IRandom`. Mọi bộ đếm thời gian của trạng thái phải tất định với cùng seed.
- **Chữ hiện cho người chơi** chỉ là bản nháp. Khoa duyệt giọng văn.

---

## 2. Sinh vật theo giai đoạn

### 2.1 Giai đoạn trong game và mốc lịch sử

| GĐ | Tên trong game | Loài người tương ứng | Khoảng thời gian (làm tròn) | Vùng trong game |
| --- | --- | --- | --- | --- |
| 1 | Vượn người | *Sahelanthropus* (Toumaï, Chad), *Ardipithecus* | ~7 – 4,4 triệu năm trước | Rừng nhiệt đới |
| 2 | Người vượn phương Nam | *Australopithecus*, *Paranthropus* | ~4 – 2 triệu năm trước | Savan |
| 3 | Người khéo léo | *Homo habilis* | ~2,4 – 1,5 triệu năm trước | Savan ven hồ (Olduvai) — ngoài MVP |
| 4 | Người đứng thẳng | *Homo erectus* (và *H. heidelbergensis*) | ~1,9 triệu – vài trăm nghìn năm trước | Châu Phi, ra Âu-Á — ngoài MVP |
| 5 | Người hiện đại | *Homo sapiens* (cùng thời Neanderthal) | ~300 nghìn năm trước tới nay | Âu-Á băng hà, Úc — ngoài MVP |

MVP kết thúc khi bộ tộc đạt giai đoạn 3. Vì vậy, trong MVP người chơi chỉ gặp sinh vật của giai đoạn 1–2, ở hai vùng: rừng và savan.

**Mức bằng chứng.** Mỗi sinh vật có một mức. Văn bản trong codex phải nói đúng mức đó:

| Mức | Nghĩa | Cách viết trong codex |
| --- | --- | --- |
| **A** | Có dấu vết trực tiếp: răng hoặc móng trên xương vượn người / người, hoặc dấu xẻ thịt trên xương con vật. | Được viết khẳng định, kèm "dấu vết cho thấy…". |
| **B** | Cùng thời, cùng nơi (hóa thạch nằm chung lớp). Chuyện hai bên gặp nhau là suy luận hợp lý. | "Sống cùng thời và cùng vùng với…", "nhiều khả năng đã chạm trán…" |
| **C** | Giả thuyết, hoặc chỉ là bối cảnh. | "Một số nhà khoa học cho rằng…", và luôn nêu là còn tranh luận. |

### 2.2 Danh sách cho MVP (Khoa duyệt 09/11; chỉ số vẫn [ĐỀ XUẤT])

| id | Tên | Vùng | Kiểu nhắm | Trạng thái, hành vi | Vai trò (`roles`) | Bậc | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `leopard` | Báo hoa mai | Rừng (0) | `hunt_weakest` | Rình, Bỏ chạy, Choáng; Vồ và tha (MVP-2) | defence, explore, opportunity | Đã có | Xem ghi chú niên đại ở §2.3 |
| `python` | Trăn đá châu Phi | Rừng (0) | **`isolated`** | Rình, Bỏ chạy, Choáng; Siết (MVP-2) | defence, explore | **Mới, MVP-1** | Khoa đã vẽ mẫu art trăn. Rừng hiện chỉ có một loài. Trăn phục kích kẻ đi lẻ, rất hợp kiểu nhắm này. |
| `hyena_pack` | Linh cẩu (bầy) | Savan (1) | `random` | Bỏ chạy theo bầy, Choáng | defence, explore | Đã có | |
| `sabertooth` | Hổ răng kiếm (*Megantereon*) | Savan (1) | `challenge_strongest` | Rình, Bỏ chạy, Choáng; Cuồng nộ (MVP-2) | opportunity | Đã có | |
| `eagle` | Đại bàng lớn | Savan (1) | **`isolated`** | Rình (lượn vòng), Bổ nhào, Bỏ chạy | defence | **Mới, MVP-2** | Câu chuyện đứa trẻ Taung. Chỉ làm khi trăn đã xong. |
| `small_antelope` | Linh dương nhỏ | Rừng, Savan | `random` | Hoảng loạn (MVP-2) | hunt | Đã có | |
| `boar_piglet` | Lợn rừng con | Rừng, Savan | `random` | Hoảng loạn (MVP-2) | hunt | Đã có | |
| `hare` | Thỏ rừng | Rừng, Savan | `random` | Hoảng loạn (MVP-2) | hunt | Đã có | |

Câu hỏi còn mở trước đây, "trăn hay *Dinofelis* ở rừng": đề xuất **trăn** cho MVP. *Dinofelis* để lại cho giai đoạn 2–3 của bản đầy đủ.

Lý do:
- Bằng chứng về *Dinofelis* săn vượn người yếu, vì phân tích đồng vị lại mâu thuẫn với giả thuyết đó.
- Trăn thì đã có art.

### 2.3 Hồ sơ từng sinh vật

Mỗi mục ghi đủ dữ liệu để viết codex. Câu trong ngoặc kép là **bản nháp chữ hiện cho người chơi**. "Nguồn" là số thứ tự ở §10.

#### Giai đoạn 1 — Rừng nhiệt đới (~7 – 4,4 triệu năm trước)

**Báo hoa mai** — *Panthera pardus* · `leopard` · MVP · Mức **A**
- Sự kiện lịch sử: ở hang Swartkrans (Nam Phi) có mảnh sọ SK 54 của một *Paranthropus* trẻ, khoảng 1,8 triệu năm tuổi. Trên sọ có hai lỗ, khớp với hai răng nanh hàm dưới của báo hoa mai. Nhiều khả năng con báo đã ngoạm con mồi và tha vào hang. Báo hoa mai ngày nay vẫn hay tha con mồi lên cây hoặc vào hang, rồi quay lại ăn dần. [Nguồn 1]
- Hành vi trong game: Rình trên cao → Vồ → Tha về phía rừng.
- **Ghi chú niên đại:** báo hoa mai xuất hiện muộn hơn giai đoạn 1, khoảng 2 triệu năm trước. Trong game, nó đại diện cho các loài mèo rình mồi trong rừng. Codex phải nói rõ điều này. Khoa chọn **giữ báo hoa mai** và ghi chú trong codex (§9, câu 3).
- Nháp lớp 1: "Kẻ săn mồi kiên nhẫn nhất của rừng. Nó chờ trên cành cây, chọn con mồi yếu nhất, rồi đổ xuống không một tiếng động."
- Nháp lớp 2: "Ở hang Swartkrans (Nam Phi), người ta tìm thấy mảnh sọ của một đứa trẻ người vượn sống cách đây khoảng 1,8 triệu năm. Trên sọ có hai lỗ tròn, khớp đúng hai răng nanh của báo hoa mai."
- Nháp lớp 3: "Mảnh sọ SK 54 làm đổi cách ta nhìn tổ tiên mình. Trước đó, đống xương trong hang từng được xem là dấu vết của người tiền sử đi săn. Nhưng ít nhất một phần trong số đó lại là thức ăn thừa của báo. Tổ tiên ta là kẻ săn, nhưng cũng là con mồi."

**Trăn đá châu Phi** — *Python sebae* · `python` · MVP-1 · Mức **C** cho thời tiền sử, **A** cho thời nay
- Bối cảnh: rắn đã là mối đe dọa của loài linh trưởng từ rất lâu. Theo giả thuyết "phát hiện rắn" của Lynne Isbell, nhu cầu nhận ra rắn có thể đã thúc đẩy thị lực của linh trưởng tiến hóa. Giả thuyết này vẫn còn tranh luận. [Nguồn 2]
- Bằng chứng thời nay: theo nghiên cứu của Headland và Greene (PNAS, 2011), khoảng 26% đàn ông Agta (Philippines) từng bị trăn gấm tấn công. Đây là bằng chứng hiếm hoi cho thấy loài trăn lớn săn được cả người. [Nguồn 2]
- Hành vi trong game: phục kích kẻ đứng tách khỏi nhóm, rồi Siết (MVP-2).
- Nháp lớp 1: "Nó nằm im giữa đám lá mục, đợi một kẻ đi lạc khỏi nhóm."
- Nháp lớp 2: "Ở Philippines, khoảng một phần tư số đàn ông bộ tộc săn bắt hái lượm Agta từng bị trăn khổng lồ tấn công. Với tổ tiên sống trong rừng, rắn lớn là nỗi sợ có thật."
- Nháp lớp 3: "Có giả thuyết cho rằng đôi mắt tinh tường của loài linh trưởng tiến hóa một phần để kịp nhận ra rắn. Giả thuyết này vẫn còn được tranh luận."

**Hổ răng kiếm *Machairodus kabir*** · `machairodus` · Bản đầy đủ · Mức **B**
- Tìm thấy ở Toros-Menalla (Chad), cùng nơi với hộp sọ Toumaï (*Sahelanthropus*), khoảng 7 triệu năm trước. To hơn một chút so với hổ Amur lớn nhất. Có lẽ hạ được cả hà mã và voi non. Môi trường quanh đó có nước quanh năm, gần rừng và đồng cỏ. [Nguồn 3]
- Hành vi gợi ý: `challenge_strongest`, Rình, Cuồng nộ.

#### Giai đoạn 2 — Savan (~4 – 2 triệu năm trước)

**Hổ răng kiếm** — *Megantereon* · `sabertooth` · MVP · Mức **B**
- Cỡ báo hoa mai hoặc báo đốm; loài lớn nhất nặng khoảng 120 kg. Là thú săn rình: dùng chân trước khỏe ghì con mồi, rồi cắn xuyên cổ bằng cặp răng nanh dài. Ở châu Phi, dấu vết cổ nhất khoảng 3–3,5 triệu năm (Nam Turkwel, Kenya); loài này tuyệt chủng ở châu Phi khoảng 1,3 triệu năm trước. [Nguồn 4]
- Một nghiên cứu đồng vị năm 2000 ở Swartkrans cho rằng nó săn *Paranthropus*, người *Homo* sớm và khỉ đầu chó. Ở Dmanisi (Gruzia), một hộp sọ *Homo erectus* có vết có thể do nó cắn, nhưng nhiều tác giả cho rằng vết đó do linh cẩu gây ra. [Nguồn 4]
- Nháp lớp 1: "Hai chiếc răng nanh dài hơn bàn tay. Nó không rượt đuổi: nó chờ, ghì chặt, rồi cắn một nhát."
- Nháp lớp 2: "Phân tích hóa học trên xương ở hang Swartkrans (Nam Phi) cho thấy hổ răng kiếm *Megantereon* có thể đã săn cả người vượn lẫn những người *Homo* đầu tiên."
- Nháp lớp 3: "Có một giả thuyết, còn tranh luận, rằng xác thừa do nó bỏ lại từng là nguồn thịt cho tổ tiên ta, những kẻ nhặt nhạnh chứ chưa phải thợ săn."

**Linh cẩu (bầy)** · `hyena_pack` · MVP · Mức **B** (codex dùng *Chasmaporthetes*)
- *Chasmaporthetes* được gọi là "linh cẩu săn mồi". Chân dài và mảnh như báo săn, nhiều khả năng săn ban ngày ở vùng trống, đôi khi theo nhóm. Sống từ khoảng 4,9 triệu tới 780 nghìn năm trước, ở châu Phi, Âu-Á và cả Bắc Mỹ. [Nguồn 5]
- Chưa có bằng chứng trực tiếp rằng nó săn vượn người. Codex phải viết ở mức B.
- Nháp lớp 1: "Từng con thì không đáng sợ. Nhưng chúng không bao giờ đi một mình."
- Nháp lớp 2: "Khác với linh cẩu ngày nay, *Chasmaporthetes* có đôi chân dài như báo săn. Nó là một thợ săn đường dài thực thụ, đã đi từ châu Phi sang tận châu Mỹ."

**Đại bàng lớn (chuyện đứa trẻ Taung)** · `eagle` · MVP-2 · Mức **A** (có diễn giải)
- Đứa trẻ Taung (*Australopithecus africanus*), tìm thấy năm 1924 ở Nam Phi, sống khoảng 2,8 triệu năm trước (một ước tính khác là 2,58 triệu), chết khi 3–4 tuổi. Năm 2006, Lee Berger chỉ ra vết móng vuốt trong hốc mắt và vết lõm trên sọ, giống với vết trên những con khỉ bị đại bàng ngày nay giết. Từ đó ông kết luận đứa trẻ nhiều khả năng bị một con chim săn mồi lớn bắt. [Nguồn 6]
- Hành vi gợi ý: Lượn vòng (Rình) → Bổ nhào vào người đứng tách nhóm → Nhấc đi (Vồ và tha).
- Nháp lớp 1: "Một cái bóng lượn vòng trên đầu. Nó chờ một đứa trẻ đi xa khỏi người lớn."
- Nháp lớp 2: "Hộp sọ người vượn đầu tiên được tìm thấy ở châu Phi là của một đứa trẻ 3 tuổi. Năm 2006, các nhà khoa học nhận ra vết móng vuốt trong hốc mắt nó: thủ phạm là một con đại bàng."

***Dinofelis* (báo răng kiếm giả)** · Bản đầy đủ · Mức **C**
- Sống khoảng 5 – 1,2 triệu năm trước; to cỡ báo đốm lớn. Ở vài hang Nam Phi, xương của nó nằm lẫn với hóa thạch vượn người, và có sọ *Paranthropus* mang lỗ răng nanh. Nhưng phân tích đồng vị cho thấy nó thích con mồi ăn cỏ, nên việc săn vượn người (nếu có) có lẽ là hiếm. [Nguồn 7]

***Deinotherium* (khủng tượng ngà cong xuống)** · Bản đầy đủ · Mức **B**
- Cao 3,6–4 m ở vai, nặng khoảng 9–12 tấn. Ngà mọc từ hàm dưới, cong xuống và ra sau, có lẽ dùng để bẻ cành ăn. Sống ở châu Phi tới khoảng 1 triệu năm trước; có hóa thạch ở Olduvai. [Nguồn 8]
- Không phải thú săn mồi. Hợp làm chướng ngại có Húc càn trong trận Khám phá, hoặc một sự kiện "gặp gỡ".

**Con mồi nhỏ (linh dương nhỏ, lợn rừng con, thỏ rừng)** · MVP · Mức **A** (cho linh dương nhỏ)
- Ở Kanjera South (Kenya, bờ hồ Victoria), khoảng 2 triệu năm trước, tổ tiên ta đã chủ động săn linh dương nhỏ và mang nguyên con về. Với linh dương cỡ lớn thì họ chỉ nhặt phần đầu bị thú khác bỏ lại, đập sọ lấy óc. [Nguồn 9]
- Nháp (linh dương nhỏ, lớp 2): "Khoảng 2 triệu năm trước, bên bờ hồ Victoria, tổ tiên ta đã biết săn linh dương nhỏ rồi mang cả con về chia nhau. Đây là một trong những dấu vết cổ nhất của việc ăn thịt thường xuyên."
- Lợn rừng con, thỏ rừng: chỉ có lớp 1 (mô tả), chưa có sự kiện lịch sử có nguồn. Agent không tự viết lớp 2 cho hai loài này.

#### Giai đoạn 3 — Người khéo léo, savan ven hồ (~2,4 – 1,5 triệu năm trước) · bản đầy đủ

**Cá sấu sừng** — *Crocodylus anthropophagus* · Mức **A**
- Khoảng 2 triệu năm trước ở Olduvai (Tanzania). Có gờ "sừng" trên mắt; có thể dài tới khoảng 5 m. Xương vượn người cùng thời ở Olduvai mang vết cắn của cá sấu. Tên loài có nghĩa là "ăn thịt người". [Nguồn 10]
- Hành vi gợi ý: `isolated` (kẻ ra mép nước), Rình dưới nước, Vồ và kéo.

**Trâu sừng cong** — *Pelorovis oldowayensis* · Mức **B**
- Mỗi lõi sừng dài khoảng 1 m; khi còn lớp sừng bọc ngoài, cặp sừng có thể dài gấp đôi, cong thành nửa vòng tròn. Cỡ trâu rừng châu Phi nhưng chân dài hơn. Hóa thạch tốt nhất ở Olduvai; tuyệt chủng khoảng 800 nghìn năm trước. [Nguồn 11]
- Hành vi gợi ý: con mồi lớn có Húc càn, gây Ngã.

**Hà mã khổng lồ** — *Hippopotamus gorgops* · Mức **A**
- To hơn hà mã ngày nay nhiều (có ước tính gấp đôi, nhưng còn tranh cãi). Mắt và mũi nằm cao hơn trên sọ. Có dấu xẻ thịt ở El-Kherba (Algeria, ~1,8 triệu năm), Kilombe (~1,76 triệu năm), Buia (~1 triệu năm); ở Olduvai, xương của nó còn được làm công cụ (~1,5 triệu năm). [Nguồn 12]

#### Giai đoạn 4 — Người đứng thẳng, châu Phi ra Âu-Á · bản đầy đủ

**Voi *Palaeoloxodon recki*** · Mức **A**
- Ở điểm EAK, Olduvai, khoảng 1,8 triệu năm trước: bộ xương một con voi nằm cùng công cụ đá Oldowan, xương dài bị đập gãy khi còn tươi. Đây là dấu vết xẻ thịt voi cổ nhất, có lẽ của *Homo erectus* (Domínguez-Rodrigo, 2026). [Nguồn 13]
- Dùng cho Tranh mồi (§8).

**Khỉ đầu chó khổng lồ** — *Theropithecus oswaldi* · Mức **A**
- Rất lớn so với khỉ Cựu Thế giới; một mẫu ước khoảng 72 kg. Sống ở Đông Phi, Nam Phi, Bắc Phi, Tây Ban Nha và Ấn Độ; tuyệt chủng ở Nam Phi khoảng 1 triệu năm trước. Một di chỉ đồ đá Acheulean có nhiều con non bị xẻ thịt (công bố 1981). [Nguồn 14]
- Hành vi gợi ý: bầy, `random`.

**Linh cẩu khổng lồ** — *Pachycrocuta brevirostris* · Mức **A**
- Cao 90–100 cm ở vai, nặng khoảng 100–110 kg, là một trong những loài linh cẩu lớn nhất từng có. Ở Chu Khẩu Điếm (Trung Quốc, ~500 nghìn năm trước), xương "Người Bắc Kinh" (*Homo erectus*) có dấu bị nó ăn: sọ bị cắn vỡ phần mặt để lấy óc. Chưa rõ nó săn hay chỉ ăn xác. Nó còn dùng thân hình lớn để giành xác từ hổ răng kiếm. [Nguồn 15]
- Dùng cho Tranh mồi (§8).

**Vượn khổng lồ** — *Gigantopithecus blacki* · Mức **B** · **có liên hệ với Việt Nam**
- Cao khoảng 3 m, nặng 200–300 kg (ước từ răng). Là loài vượn lớn nhất từng có. Ăn thực vật: ban đầu chủ yếu ăn quả, về sau phải ăn vỏ cây và cành. Tuyệt chủng khoảng 295–215 nghìn năm trước, khi khí hậu đổi mùa rõ hơn. [Nguồn 16]
- Ở hang Thẩm Khuyên (miền Bắc Việt Nam) có hóa thạch *Gigantopithecus* nằm cùng *Homo erectus* (nghiên cứu năm 1996). [Nguồn 17]
- Không săn người. Hợp làm sự kiện "gặp gỡ" không combat ở vùng Đông Nam Á.

**Hổ răng mã tấu** — *Homotherium latidens* · Mức **B**
- Ở Schöningen (Đức), khoảng 300 nghìn năm trước: hóa thạch của nó nằm chung lớp với những ngọn giáo gỗ dài hơn 2 m của *Homo heidelbergensis*. Cao hơn 90 cm ở vai, nặng hơn 180 kg, răng nanh trên dài hơn 10 cm. [Nguồn 18]

**Đảo Flores: cò khổng lồ, rồng Komodo, voi lùn *Stegodon*** · Mức **A**/**B**
- Cò già *Leptoptilos robustus* cao khoảng 1,8 m, nặng khoảng 16 kg, bay được. Sống ở hang Liang Bua cùng "người hobbit" *Homo floresiensis* hơn 50 nghìn năm trước, và tranh xác voi lùn *Stegodon* với họ (mức B). [Nguồn 19]
- Nghiên cứu năm 2026: rồng Komodo thường được ăn xác *Stegodon* trước, rồi *H. floresiensis* mới tới nhặt phần còn lại (mức A). [Nguồn 20]
- Dùng cho Tranh mồi (§8).

#### Giai đoạn 5 — Người hiện đại · bản đầy đủ

**Sư tử hang** — *Panthera spelaea* · Mức **A**
- Ở Đức, xương một con sư tử hang khoảng 48 nghìn năm tuổi mang vết thương do giáo gỗ, loại vũ khí người Neanderthal dùng. [Nguồn 21]

**Linh cẩu hang** — *Crocuta crocuta spelaea* · Mức **A**
- Nặng khoảng 88 kg, hơn linh cẩu đốm ngày nay khoảng 60%. Tranh hang với người Neanderthal và người hiện đại: có lúc hang thuộc về người, có lúc thuộc về linh cẩu. Nó cướp con mồi của người Neanderthal, và có xương Neanderthal mang dấu linh cẩu gặm. Ở châu Âu, nó tuyệt chủng khoảng 31 nghìn năm trước. [Nguồn 22]
- Hành vi gợi ý: trận Phòng thủ "tranh hang".

**Voi ma mút lông xoăn** — *Mammuthus primigenius* · Mức **A**
- Ở Kostenki 11 (Nga) có một vòng tròn rộng khoảng 9 m, dựng từ xương của ít nhất 64 sọ và 51 hàm dưới ma mút, hơn 20 nghìn năm tuổi. Công dụng của nó vẫn còn tranh luận. Ở Ukraine và đồng bằng phía tây nước Nga, người ta biết khoảng 70 công trình như vậy. [Nguồn 23]

**Thằn lằn khổng lồ** — *Varanus priscus* (Megalania) · Mức **B**
- Dài tới khoảng 6 m, nặng khoảng 500 kg. Một mẩu xương khoảng 50 nghìn năm tuổi ở hang Capricorn (Queensland, Úc) cho thấy nó còn sống khi những người đầu tiên tới Úc. [Nguồn 24]

---

## 3. Kiểu tấn công thứ tư: "Bắt kẻ lẻ loi" (MVP)

### 3.1 Bảng kiểu tấn công sau khi thêm

| Kiểu | Giá trị JSON | Nhắm vào |
| --- | --- | --- |
| Săn kẻ yếu | `hunt_weakest` | Thành viên có máu thấp nhất |
| Ngẫu nhiên | `random` | Bất kỳ ai |
| Thách đấu kẻ mạnh | `challenge_strongest` | Thành viên có `strength` cao nhất |
| **Bắt kẻ lẻ loi** | **`isolated`** | **Thành viên đứng xa đồng đội nhất** |

### 3.2 Định nghĩa chính xác [ĐỀ XUẤT]

- **Trên sân** (`TargetSelection.PickOnField`):
  - Với mỗi thành viên còn sống trên sân, tính `d` = khoảng cách tới **đồng đội còn sống gần nhất** trên sân.
  - Chọn người có `d` lớn nhất.
  - Nếu chỉ có 1 người trên sân: chọn người đó.
  - Nếu hòa: bốc ngẫu nhiên qua `IRandom`, giống ba kiểu cũ.
- **Ngoài sân** (`PickFromTribe`, khi phải lấy thêm người cho đủ tổn thất của luật): người ở nhà không có vị trí, nên **dùng như `random`**.
- **Khóa mục tiêu:** giữ đúng cách khóa mục tiêu mà B2 đang dùng cho ba kiểu cũ. Kiểu mới **không** chọn lại mục tiêu mỗi khung hình, nếu không con thú sẽ đổi hướng liên tục. Nếu có trạng thái Rình (§4), mục tiêu được chọn ở cuối pha Rình.

### 3.3 Cảm giác và cách người chơi đối phó

- Cách đối phó: thả người sát nhau.
- Đổi lại, sau này nhóm đứng sát dễ bị các đòn Húc càn trúng nhiều người cùng lúc (§8). Kiểu `isolated` và Húc càn tạo ra hai áp lực ngược nhau. Đây là chủ ý.
- Gợi ý khi gặp lần đầu (`design.md` §13; chữ đặt trong file chuỗi): "Nó rình kẻ đứng tách khỏi nhóm. Thả người sát nhau."
- Nháp `attack_style_text` cho trăn: "Nó chờ một kẻ đi lạc khỏi nhóm, rồi chỉ nhắm vào kẻ đó."

### 3.4 Test bắt buộc

- Ba người đứng sát nhau, một người đứng xa → chọn người đứng xa.
- Hai người cách đều nhau → mỗi người được chọn khoảng 50% trên 10.000 lần rút, với seed cố định.
- Chỉ một người trên sân → không tiêu lần rút ngẫu nhiên nào. Lý do: rút thừa làm lệch chuỗi ngẫu nhiên của cả ván, như chú thích sẵn có trong `TargetSelection.cs`.
- `PickFromTribe` với kiểu `isolated` cho kết quả phân bố giống `random`.

---

## 4. Trạng thái trong trận (MVP, theo bậc)

### 4.1 Nguyên tắc

1. **Mọi trạng thái phải có tín hiệu nhìn thấy được:** biểu tượng trên đầu, đổi màu, hoạt ảnh hoặc âm thanh. Combat tự đánh, nên người chơi chỉ còn cách *đọc* trận để quyết định thả người vào đâu. Trạng thái nào không có tín hiệu thì người chơi chỉ thấy khó hiểu, không thấy hay.
2. Trạng thái tạo ra **một khoảnh khắc để người chơi phản ứng**. Ví dụ: thú đang Rình ai thì thả người tới đứng cạnh người đó; thú đang Choáng thì dồn người vào đánh.
3. Mọi thời gian, ngưỡng và hệ số nằm trong `beasts.json`. Không viết cứng.
4. Không trạng thái nào được xóa sổ cả bộ tộc, và không được làm sai luật "phòng thủ thua thì thú ăn đúng `quota` người rồi rút" (`design.md` §8).

### 4.2 Trạng thái của quái thú

| Trạng thái | Vào khi | Làm gì | Ra khi | Tín hiệu | Số [ĐỀ XUẤT] |
| --- | --- | --- | --- | --- | --- |
| **Rình** (`stalk`) | Đầu trận Phòng thủ hoặc Khám phá, với loài có hành vi phục kích (báo, trăn, *Megantereon*, đại bàng) | Đi chậm sát mép; ở cuối pha thì chọn mục tiêu theo kiểu nhắm và **đánh dấu mục tiêu** | Hết giờ Rình, hoặc bị đánh trúng (chuyển ngay sang Tấn công) | Biểu tượng con mắt trên đầu người bị nhắm; thú thấp người, hơi mờ | `seconds` 2.5; `move_speed_mult` 0.4 |
| **Tấn công** | Mặc định, sau Rình | Như hiện nay: đuổi mục tiêu đã khóa và đánh | Gặp điều kiện của trạng thái khác | — | — |
| **Choáng** (`stagger`) | Bị trúng ≥ `hits` đòn trong `window_seconds` | Đứng im, không đánh | Hết `seconds`; sau đó **miễn Choáng** trong `immune_seconds` | Vòng sao xoay trên đầu, tiếng "bộp" | `hits` 3; `window_seconds` 1.5; `seconds` 1.2; `immune_seconds` 5.0 |
| **Bỏ chạy** (`flee`) — trận Phòng thủ / Khám phá | Máu ≤ `hp_ratio` của máu tối đa. Với bầy: khi số con gục ≥ `pack_down_ratio` của bầy, cả bầy bỏ chạy | Quay đầu chạy về mép xuất phát, nhanh hơn | Ra khỏi mép sân → **trận kết thúc, người chơi thắng** (lý do kết thúc mới: `BeastRepelled`) | Thú quay lưng, cụp tai, có tiếng kêu | `hp_ratio` 0.25; `move_speed_mult` 1.15; `pack_down_ratio` 0.67 |
| **Hoảng loạn** (`panic`) — trận Dụ và hạ | Máu ≤ `hp_ratio` | Chạy nhanh hơn trong `burst_seconds`, sau đó **mệt** (chậm hơn) | Hết pha mệt thì về tốc độ thường | Bụi tung, rồi thú thở dốc | `hp_ratio` 0.35; `burst_mult` 1.2; `burst_seconds` 2.0; `tired_mult` 0.8; `tired_seconds` 2.0 |
| **Vồ và tha** (`grab_drag`) | Đòn trúng **đầu tiên** sau Rình (báo; đại bàng thì nhấc lên) | Mục tiêu chuyển sang trạng thái *Bị tha*; thú kéo người đó về mép xuất phát | (a) Đồng đội gây đủ `release_damage_ratio` × máu tối đa của thú → thú **nhả** người, người đó *Ngã*, thú Choáng ngắn. (b) Thú ra tới mép → người bị tha chết, thú rời trận (xem ghi chú ở 4.4) | Dây kéo / vệt máu nhạt; thanh "giải cứu" trên đầu thú | `drag_speed_mult` 0.6; `release_damage_ratio` 0.2; `release_stagger_seconds` 0.8 |
| **Siết** (`grab_squeeze`) | Như Vồ và tha, cho trăn | Thú **đứng yên** siết; người bị siết mất máu theo thời gian | Như (a) ở trên, hoặc người bị siết hết máu | Thân trăn quấn quanh người; thanh giải cứu | `squeeze_damage_per_second` 0.6; `release_damage_ratio` 0.2 |
| **Cuồng nộ** (`enrage`) | Máu ≤ `hp_ratio`, một lần mỗi trận (*Megantereon*) | Đánh nhanh hơn, chạy nhanh hơn | Hết trận | Ánh đỏ, tiếng gầm | `hp_ratio` 0.5; `attack_interval_mult` 0.75; `move_speed_mult` 1.1 |

### 4.3 Trạng thái của thành viên

| Trạng thái | Vào khi | Làm gì | Ra khi | Tín hiệu |
| --- | --- | --- | --- | --- |
| **Bị tha / Bị siết** | Thú Vồ hoặc Siết trúng | Không di chuyển, không đánh. Đồng đội tự ưu tiên đánh con thú đang giữ người | Được nhả hoặc chết | Biểu tượng tay giơ lên, tiếng kêu cứu |
| **Ngã** | Được thú nhả ra; về sau còn do Húc càn (§8) | Không hành động trong `down_seconds` | Hết thời gian | Nằm nghiêng, ngôi sao nhỏ |

Số [ĐỀ XUẤT]: `down_seconds` 1.0.

Khi có người *Bị tha* hoặc *Bị siết*, đồng đội ưu tiên đánh con thú đang giữ người. Đây là hành vi tự động, không cần người chơi ra lệnh, vì combat là tự đánh.

### 4.4 Ghi chú kỹ thuật cho Vồ và tha

- Trong trận **Phòng thủ**: thú tha được một người ra khỏi sân thì tính là thua. Người bị tha là nạn nhân **đầu tiên** của `quota`; chính sách tổn thất (`DesignTollCasualtyPolicy`) lấy thêm người, nếu cần, cho đủ `quota`. Như vậy luật §8 không đổi.
- Trong trận **Khám phá** hoặc **Cơ hội** (`opportunity`): thú tha được người ra khỏi sân thì thú rời trận, tính là **thua**, và người bị tha được tính vào số chết của luật thua (2 người).
- Agent phải viết test cho cả hai trường hợp. Nếu chính sách tổn thất hiện tại không cho đặt "nạn nhân đầu tiên" thì **dừng và hỏi**, không tự đổi luật.

### 4.5 Bậc ưu tiên

| Bậc | Gồm | Lý do |
| --- | --- | --- |
| **MVP-1** (làm trước) | Rình, Choáng, Bỏ chạy (Phòng thủ), lý do kết thúc `BeastRepelled`, biểu tượng mục tiêu | Rẻ, dễ đọc. Rình hiện mục tiêu nên làm cho cả `isolated` lẫn `hunt_weakest` dễ hiểu. Bỏ chạy làm trận Phòng thủ kết thúc gọn và giống thật: thú săn mồi tránh bị thương. |
| **MVP-2** (nếu kịp) | Vồ và tha, Siết, trạng thái thành viên Bị tha và Ngã, Cuồng nộ, Hoảng loạn | Kịch tính nhất (giải cứu đồng đội), nhưng đụng vào chính sách tổn thất và cần thêm hoạt ảnh. |
| **Sau MVP** | Húc càn, Bổ nhào có vùng ảnh hưởng, Tranh mồi | Cần loài và kiểu trận chưa có trong MVP (§8). |

**Rủi ro cân bằng:** Choáng thưởng cho việc dồn nhiều người đánh cùng lúc, nên làm đường cong "tỉ lệ thắng theo số người `k`" dốc thêm. Ghi chú `levers` trong `beasts.json` đã nói đường cong này vốn quá dốc. B3 phải đo trước và sau khi bật Choáng. Nếu lệch quá 10 điểm %, tắt Choáng bằng `enabled: false` và báo Khoa; không tự chỉnh `balance.json`.

### 4.6 Dữ liệu trong `beasts.json` [ĐỀ XUẤT]

Thêm một khối ở cấp trên cùng:

```json
"states": {
  "_comment": "Trạng thái trong trận, docs/bestiary.md §4. Mọi số là [ĐỀ XUẤT]. enabled=false thì trạng thái không bao giờ kích hoạt.",
  "stalk":        { "enabled": true,  "seconds": 2.5, "move_speed_mult": 0.4 },
  "stagger":      { "enabled": true,  "hits": 3, "window_seconds": 1.5, "seconds": 1.2, "immune_seconds": 5.0 },
  "flee":         { "enabled": true,  "hp_ratio": 0.25, "move_speed_mult": 1.15, "pack_down_ratio": 0.67 },
  "panic":        { "enabled": false, "hp_ratio": 0.35, "burst_mult": 1.2, "burst_seconds": 2.0, "tired_mult": 0.8, "tired_seconds": 2.0 },
  "grab_drag":    { "enabled": false, "drag_speed_mult": 0.6, "release_damage_ratio": 0.2, "release_stagger_seconds": 0.8 },
  "grab_squeeze": { "enabled": false, "squeeze_damage_per_second": 0.6, "release_damage_ratio": 0.2 },
  "enrage":       { "enabled": false, "hp_ratio": 0.5, "attack_interval_mult": 0.75, "move_speed_mult": 1.1 },
  "member_down":  { "down_seconds": 1.0 }
}
```

Thêm hai trường cho mỗi loài trong `species[]`:
- `behaviors`: danh sách trạng thái loài đó *được phép* dùng. Một trạng thái chạy khi nó vừa có `enabled: true` ở khối chung, vừa có tên trong `behaviors` của loài.
- `codex_id`: id của hồ sơ trong `codex.vi.json`.

| Loài | `attack_style` | `behaviors` | `codex_id` |
| --- | --- | --- | --- |
| `leopard` | `hunt_weakest` | `["stalk", "stagger", "flee", "grab_drag"]` | `leopard` |
| `python` | `isolated` | `["stalk", "stagger", "flee", "grab_squeeze"]` | `python` |
| `hyena_pack` | `random` | `["stagger", "flee"]` | `chasmaporthetes` |
| `sabertooth` | `challenge_strongest` | `["stalk", "stagger", "flee", "enrage"]` | `megantereon` |
| `eagle` | `isolated` | `["stalk", "flee", "grab_drag"]` | `taung_eagle` |
| `small_antelope`, `boar_piglet`, `hare` | `random` | `["stagger", "panic"]` | trùng id loài |

**Cách đặt chỉ số [ĐỀ XUẤT] cho hai loài mới** (B3 hiệu chỉnh sau):
- `python`: máu cao, chậm, đánh ít nhưng đau. Bắt đầu từ `{ "hp": 24.0, "move_speed": 1.8, "attack_damage": 1.6, "attack_interval": 1.8, "attack_range": 1.2 }`.
- `eagle`: máu thấp, rất nhanh. Bắt đầu từ `{ "hp": 9.0, "move_speed": 3.4, "attack_damage": 1.2, "attack_interval": 1.4, "attack_range": 1.0 }`.

---

## 5. Hồ sơ lịch sử (codex) và thành tựu (MVP)

### 5.1 Người chơi thấy gì

**Sau mỗi trận** (thắng hay thua), màn kết quả hiện như bây giờ. Thêm tối đa **2 thẻ** bên dưới, chạm để bỏ qua:
1. **Thẻ hồ sơ**:
   - Hiện khi vừa mở một lớp mới của hồ sơ loài vừa đánh.
   - Gồm: hình loài, tên tiếng Việt, tên khoa học, mốc thời gian, 2–3 câu.
   - Có nút "Đọc thêm" mở ra Bảo tàng.
2. **Thẻ thành tựu**: hiện khi trận vừa rồi mở được thành tựu. Gồm tên, một câu lịch sử, và một dòng "vì sao bạn nhận được".

Nếu trận mở nhiều hơn 2 thẻ, phần còn lại vào Bảo tàng, đánh dấu "mới". Màn kết quả không được dài hơn hiện nay quá vài giây, vì nhịp game nhanh là mục tiêu cảm xúc ở `design.md` §1.

**Mỗi hồ sơ có 3 lớp, mở dần** [ĐỀ XUẤT]:

| Lớp | Mở khi | Nội dung |
| --- | --- | --- |
| 1 — Hồ sơ | Gặp loài lần đầu (thắng hay thua) | Tên, thời gian, nơi sống, mô tả ngắn, **kiểu tấn công** (thay cho gợi ý "lần đầu gặp một loài" ở `design.md` §13) |
| 2 — Sự kiện lịch sử | Thắng loài đó lần đầu | Câu chuyện bằng chứng (ví dụ SK 54, đứa trẻ Taung) |
| 3 — Tranh luận | Thắng loài đó 3 lần, hoặc mở thành tựu gắn với loài đó | Điều gì còn tranh cãi, mức bằng chứng, nguồn |

**Bảo tàng** (từ menu chính, và từ nút tạm dừng trong ván):
- Lưới các loài. Loài chưa gặp hiện bóng đen và "???".
- Mỗi trang hồ sơ gồm: hình, các lớp đã mở, dòng nguồn (tên bài, năm), và nhãn mức bằng chứng A/B/C viết thành chữ ("Có dấu vết trực tiếp" / "Sống cùng thời" / "Giả thuyết").
- Có một tab Thành tựu.

**Lưu xuyên ván** [ĐỀ XUẤT]:
- Hồ sơ và thành tựu lưu ở **hồ sơ người chơi**, tách khỏi file lưu ván (C7). Thua ván không mất hồ sơ.
- Hồ sơ và thành tựu **không cho thưởng gameplay** nào. Như vậy cân bằng 30–40% không đổi, và không ai phải "cày" để mạnh lên.

### 5.2 Thành tựu cho MVP [ĐỀ XUẤT]

| id | Tên | Điều kiện | Mở / kể chuyện gì |
| --- | --- | --- | --- |
| `first_blood` | Lần đầu cầm cự | Thắng trận combat đầu tiên (kể cả trận hướng dẫn lượt 3) | Một câu: tổ tiên ta vừa là con mồi vừa là kẻ săn |
| `two_holes` | Hai lỗ trên sọ | Thắng báo hoa mai lần đầu | Lớp 2 của `leopard` (SK 54) |
| `no_one_left_behind` | Không bỏ ai lại | Thắng một loài kiểu `isolated` mà không ai chết | Lớp 3 của `python` (giả thuyết phát hiện rắn) |
| `saber_dance` | Đối mặt răng kiếm | Thắng *Megantereon* mà người có `strength` cao nhất còn sống | Lớp 3 của `megantereon` |
| `pack_breaker` | Bẻ gãy bầy | Làm cả bầy linh cẩu Bỏ chạy | Lớp 2 của `chasmaporthetes` |
| `driven_off` | Đuổi đi | Thắng một trận Phòng thủ bằng `BeastRepelled` | Thú săn mồi tránh bị thương, nên đuổi được thì không cần giết |
| `dizzy` | Dồn cho choáng | Làm quái thú Choáng 3 lần trong một trận | (không có lớp lịch sử; thành tựu kỹ năng) |
| `first_hunt` | Bữa thịt đầu tiên | Săn thành công lần đầu | Lớp 2 của `small_antelope` (Kanjera South) |
| `out_of_the_forest` | Rời khỏi rừng | Di cư sang savan | Giả thuyết "chọn lọc theo biến động khí hậu" của Rick Potts (`design.md` §10) |
| `rescuer` | Giật lại từ nanh vuốt | Giải cứu một người đang Bị tha hoặc Bị siết (**MVP-2**) | Lớp 3 của `leopard` |
| `shadow_above` | Cái bóng trên đầu | Thắng đại bàng lần đầu (**MVP-2**) | Lớp 2 của `taung_eagle` |
| `collector_forest` / `collector_savanna` | Nhà sưu tầm | Mở đủ lớp 1 của mọi loài ở một vùng | — |

**Đã có nguồn cho lớp lịch sử:**
- `first_hunt` → Kanjera South [Nguồn 9].
- `out_of_the_forest` → Rick Potts, đã nêu trong `design.md` §10. Agent cần một nguồn có thể dẫn trước khi đưa chữ vào game; nếu chưa có, chỉ để tên thành tựu, không kèm câu lịch sử.

### 5.3 Kiến trúc [ĐỀ XUẤT]

- **Không đổi** `CombatResult` của Core.
- Nhánh combat đã có `BattleReport`. Thêm vào đó các con số thống kê thành tựu cần:
  - id loài
  - lý do kết thúc (gồm `BeastRepelled`)
  - số lần Choáng
  - số lần giải cứu
  - người `strength` cao nhất còn sống hay không
  - số người chết
- Tầng `Game` đọc `BattleReport` và các sự kiện vòng lượt (săn thành công, di cư; telemetry ở I3 đã ghi đúng những sự kiện này). Từ đó tầng `Game` tính thành tựu và cập nhật hồ sơ người chơi.
- Core không biết gì về codex.
- `Assets/Data/achievements.json`: mỗi thành tựu gồm `id`, `condition` và `codex_unlock`.
  - `condition` có dạng `{ "type": ..., ...tham số }`, và `type` chỉ được là một trong: `win_first_battle`, `win_vs_species`, `win_vs_style_no_deaths`, `win_vs_species_strongest_alive`, `pack_fled`, `win_by_repel`, `stagger_count_in_battle`, `first_hunt`, `migrated_to_region`, `rescue`, `codex_complete_region`.
  - `codex_unlock` có dạng `{ "entry": ..., "layer": ... }`, hoặc `null`.
  - Tên và chữ hiện cho người chơi nằm trong `codex.vi.json`, không nằm trong `achievements.json`.

### 5.4 `Assets/Data/codex.vi.json` [ĐỀ XUẤT]

```json
{
  "_comment": "Hồ sơ lịch sử, docs/bestiary.md §2.3 và §5. Chữ hiện cho người chơi: bản nháp chờ Khoa duyệt. Mỗi câu phải khớp mức bằng chứng. Không thêm sự thật không có trong sources.",
  "entries": [
    {
      "id": "leopard",
      "name": "Báo hoa mai",
      "scientific_name": "Panthera pardus",
      "era_text": "Khoảng 2 triệu năm trước tới nay",
      "place_text": "Châu Phi và châu Á",
      "evidence_level": "A",
      "layers": [
        { "unlock": "encounter",  "title": "Kẻ rình trên cành", "text": "…" },
        { "unlock": "first_win",  "title": "Hai lỗ trên sọ",    "text": "…" },
        { "unlock": "wins:3",     "title": "Kẻ săn hay con mồi?", "text": "…" }
      ],
      "accuracy_note": "Xuất hiện muộn hơn giai đoạn 1; trong game đại diện cho các loài mèo rình rập trong rừng.",
      "sources": [ { "title": "How Leopard Kills Rewrite Our Prehistory", "publisher": "Discover", "url": "https://www.discovermagazine.com/how-leopard-kills-rewrite-our-prehistory-42902" } ]
    }
  ],
  "achievements": [
    { "id": "two_holes", "name": "Hai lỗ trên sọ", "reason_text": "Thắng báo hoa mai lần đầu." }
  ]
}
```

Các giá trị `unlock` hợp lệ: `encounter`, `first_win`, `wins:N`, `achievement:<id>`.

**Hình (Khoa chốt 09/11, xem `docs/prompt.md` §2.7):**
- Mỗi hồ sơ có **một** file tranh ngang `codex_<id>.png` (hồ sơ đầy đủ). Con vật nằm ở nửa phải, chữ đặt ở nửa trái.
- Hình hồ sơ (thẻ sau trận, ô trong lưới Bảo tàng) **không phải file riêng**. UI cắt phần phải của chính file đó (`scale-and-crop` + `background-position-x`).
- Thêm trường tùy chọn `card_focus_x` (0–1, mặc định 1) cho tranh nào có con vật lệch bố cục.

Phần `"…"` lấy từ các câu nháp ở §2.3.

### 5.5 Văn phong

- Mỗi lớp tối đa 3 câu. Số làm tròn ("khoảng 1,8 triệu năm"), không ghi khoảng sai số.
- Viết đúng mức bằng chứng (§2.1). Không được biến giả thuyết thành sự thật.
- Lớp 3 luôn có một câu "điều gì còn tranh luận".
- Không dùng ảnh chụp người thật hay ảnh hóa thạch tải từ mạng. Hình trong Bảo tàng là art của game.

---

## 6. Hành vi tấn công (danh mục)

Hành vi là cách một loài dùng các trạng thái ở §4. Danh mục này dùng cho cả MVP lẫn bản đầy đủ.

| Hành vi | Gồm các trạng thái | Loài |
| --- | --- | --- |
| Phục kích | Rình → Tấn công | Báo, trăn, *Megantereon*, *Machairodus*, cá sấu |
| Vồ và tha | Rình → Vồ và tha → (Bị tha) | Báo, đại bàng (nhấc lên), cá sấu (kéo xuống nước) |
| Siết | Rình → Siết → (Bị siết) | Trăn |
| Bổ nhào | Lượn (Rình) → lao nhanh theo đường thẳng tới mục tiêu `isolated` | Đại bàng |
| Bầy truy đuổi | Nhiều con, `random`; Bỏ chạy theo bầy | Linh cẩu, *Chasmaporthetes*, linh cẩu hang |
| Cuồng nộ | Tấn công → Cuồng nộ khi máu thấp | *Megantereon*, *Homotherium*, sư tử hang |
| Húc càn (sau MVP) | Lấy đà → lao thẳng, trúng mọi người trên đường → Ngã | *Deinotherium*, *Pelorovis*, hà mã *gorgops*, ma mút |
| Tranh mồi (sau MVP) | Xem §8 | *Pachycrocuta*, Komodo, cò Flores, linh cẩu hang |

---

## 7. Việc cho agent, thứ tự làm và thứ tự cắt

Mã việc theo kiểu `HANDOFF.md` (K = Khoa, A = agent). Supervisor xếp lịch, sau mốc 15/11 (D15) hoặc song song nếu nhánh combat rảnh.

| Mã | Ai | Việc | Cần trước | Nghiệm thu |
| --- | --- | --- | --- | --- |
| BX0 | K | Duyệt §2.2 (danh sách loài), §9 (câu hỏi), giọng văn codex | — | **Xong 09/11:** Khoa theo toàn bộ đề xuất |
| BX1 | A | Kiểu nhắm `isolated`: enum `BeastAttackStyle.Isolated`, `BeastData` nhận `"isolated"`, `TargetSelection` (§3.2), test (§3.4); thêm dòng vào bảng `design.md` §9 | — | Test xanh; B3 chạy được với kiểu mới |
| BX2 | A | Thêm loài `python` vào `beasts.json` với chỉ số [ĐỀ XUẤT] (§4.6); hiệu chỉnh B3 | BX0, BX1 | Bảng B3 cho rừng lệch ≤ 10 điểm % so với công thức, hoặc đã báo Khoa |
| BX3 | A | Trạng thái MVP-1: khối `states`, trường `behaviors`, Rình, Choáng, Bỏ chạy, `BattleEndReason.BeastRepelled`; test tất định theo seed; B3 đo trước và sau | BX1 | Test xanh; báo cáo B3 trước/sau cho từng trạng thái |
| BX4 | A | Tín hiệu hình và âm cho trạng thái MVP-1 (biểu tượng tạm được), chữ vào file chuỗi | BX3 | Người xem trận nhận ra thú đang Rình ai, đang Choáng, đang Bỏ chạy |
| BX5 | A | `codex.vi.json` + loader + hồ sơ người chơi lưu xuyên ván + thẻ hồ sơ trên màn kết quả | BX0 | Đánh xong một loài lần đầu thì thấy thẻ lớp 1; thắng lần đầu thì thấy lớp 2; tắt game mở lại vẫn còn |
| BX6 | A | `achievements.json` + bộ tính thành tựu ở tầng `Game` (§5.3) + thẻ thành tựu | BX3, BX5 | Test từng `type` điều kiện; không đổi `CombatResult` |
| BX7 | A | Màn Bảo tàng (UI Toolkit): lưới loài, trang hồ sơ, tab thành tựu | BX5, BX6 | Mở được từ menu chính và từ nút tạm dừng |
| BX8 | K | Art trăn (Khoa đã có mẫu), đại bàng nếu làm; duyệt chữ codex | BX0 | — |
| BX9 | A | MVP-2: Vồ và tha, Siết, Bị tha, Ngã, Cuồng nộ, Hoảng loạn; loài `eagle` | BX3; Khoa đồng ý ở BX0 | Test cả hai trường hợp ở §4.4; B3 lệch ≤ 10 điểm % hoặc đã báo |

**Không bao giờ cắt** (Khoa đã quyết):
- BX1 (kẻ lẻ loi)
- Thẻ hồ sơ sau trận (phần lõi của BX5)

**Thiếu thời gian thì cắt theo thứ tự này:**
1. BX9 (toàn bộ MVP-2, gồm đại bàng)
2. BX7 (Bảo tàng; vẫn giữ thẻ sau trận)
3. Choáng trong BX3 (đặt `enabled: false`)
4. BX6 (thành tựu; vẫn giữ hồ sơ)

---

## 8. Phát triển sau (ngoài MVP)

### 8.1 Tranh mồi (Khoa: để sau, chưa làm)

- **Ý tưởng:**
  - Một kiểu trận mới, tạm gọi là `contest`.
  - Giữa sân có một cái xác lớn (voi *Palaeoloxodon recki*, voi lùn *Stegodon*, hà mã).
  - Bộ tộc và kẻ ăn xác cùng giành phần.
  - Thức ăn thu được tính theo số giây người của bộ tộc đứng xẻ thịt mà không bị đuổi đi.
  - Kẻ ăn xác mạnh hơn thì tới trước và chiếm phần ngon. Bộ tộc phải chọn: đánh đuổi, hay chờ nhặt phần còn lại.
- **Cơ sở lịch sử:**
  - Ở Liang Bua, rồng Komodo được ăn xác *Stegodon* trước, người *H. floresiensis* tới sau và chỉ xẻ các phần ít thịt [Nguồn 20].
  - *Pachycrocuta* dùng thân hình lớn để giành xác từ hổ răng kiếm [Nguồn 15].
  - Ở Kanjera South, tổ tiên ta nhặt phần đầu linh dương lớn sau khi thú khác ăn xong [Nguồn 9].
- **Cần thiết kế thêm:**
  - Liên hệ với luật "thức ăn không để dành".
  - Có người chết hay không khi bị đuổi khỏi xác.
  - Tỉ lệ thắng nằm ở đâu trong công thức §5.2.

### 8.2 Húc càn và trạng thái Ngã cho thú lớn

- Thú ăn cỏ khổng lồ (*Deinotherium*, *Pelorovis*, hà mã *gorgops*, ma mút) lấy đà rồi lao thẳng. Ai đứng trên đường lao đều Ngã.
- Đây là áp lực ngược với kiểu `isolated`: một bên buộc phải đứng sát nhau, một bên phạt việc đứng sát nhau.

### 8.3 Vùng đất cho giai đoạn 3–5 [ĐỀ XUẤT, câu hỏi cho Khoa]

| Vùng | Giai đoạn | Sinh vật tiêu biểu |
| --- | --- | --- |
| Savan ven hồ (Olduvai) | 3 | Cá sấu sừng, *Pelorovis*, hà mã *gorgops*, *Dinofelis* |
| Sa mạc, đồng bằng sông Nile (đã có tên trong `balance.json`) | 3–4 | Cần nghiên cứu thêm |
| Âu-Á (Dmanisi, Chu Khẩu Điếm, Schöningen) | 4 | *Megantereon*, *Pachycrocuta*, *Homotherium* |
| Đông Nam Á (miền Bắc Việt Nam, Quảng Tây) | 4 | *Gigantopithecus* (gặp gỡ, không combat) |
| Đảo Flores | 4 (nhánh phụ) | Cò khổng lồ, Komodo, *Stegodon* — Tranh mồi |
| Âu-Á băng hà | 5 | Sư tử hang, linh cẩu hang (tranh hang), ma mút |
| Úc | 5 | Megalania |

### 8.4 Cần nghiên cứu thêm trước khi viết hồ sơ

- *Smilodon* và gấu mặt ngắn *Arctodus* ở châu Mỹ.
- Gấu hang châu Âu.
- Hệ động vật Aramis (*Ardipithecus*, giai đoạn 1).
- Thú săn mồi ở Dmanisi.
- Sa mạc và sông Nile.

Chưa có nguồn đã kiểm cho các mục này. Agent **không** tự viết hồ sơ cho chúng.

---

## 9. Câu hỏi cho Khoa

> **Đã trả lời 09/11/2026: "theo đề xuất của bạn hết".** Kết quả:
> 1. Trăn vào MVP, mang kiểu `isolated`.
> 2. Đại bàng vào MVP-2; cắt đầu tiên nếu trễ.
> 3. Giữ báo hoa mai ở rừng; codex ghi rõ sai niên đại.
> 4. Thú Bỏ chạy khỏi trận Phòng thủ = người chơi thắng (`BeastRepelled`). B3 phải đo mức tăng tỉ lệ thắng.
> 5. Hồ sơ và thành tựu lưu xuyên ván, không cho thưởng gameplay.
> 6. Bản đầy đủ đi theo bảng vùng ở §8.3, gồm Âu-Á và Đông Nam Á / Việt Nam. Ngoài MVP.
> 7. Giữ giọng văn các câu nháp ở §2.3.

Câu hỏi gốc:

1. Đưa **trăn** vào MVP và cho nó mang kiểu "Bắt kẻ lẻ loi"? *(Đề xuất: có.)*
2. **Đại bàng** (chuyện Taung) vào MVP-2 hay để sau? *(Đề xuất: MVP-2, cắt đầu tiên nếu trễ.)*
3. Báo hoa mai ở rừng giai đoạn 1 sai niên đại khoảng 5 triệu năm. Giữ và ghi chú trong codex, hay đổi sang *Machairodus kabir* cho đúng thời Toumaï? *(Đề xuất: giữ, vì art và chỉ số đã có, và codex sẽ nói rõ.)*
4. Thú **Bỏ chạy** khỏi trận Phòng thủ thì tính là người chơi thắng? *(Đề xuất: có. Việc này làm tăng tỉ lệ thắng Phòng thủ, B3 sẽ đo.)*
5. Hồ sơ và thành tựu **lưu xuyên ván** và **không cho thưởng gameplay**? *(Đề xuất: có.)*
6. Bản đầy đủ có đi sang **Âu-Á** ở giai đoạn 4–5, và có một vùng **Đông Nam Á / Việt Nam** với *Gigantopithecus*?
7. Giọng văn các câu nháp ở §2.3: giữ, hay muốn kể chuyện hơn hoặc khoa học hơn?

---

## 10. Nguồn

1. Discover — *How Leopard Kills Rewrite Our Prehistory*: https://www.discovermagazine.com/how-leopard-kills-rewrite-our-prehistory-42902
2. National Geographic — *Meet the Agta, a tribe where a quarter of men have been attacked by giant snakes* (Headland & Greene, PNAS 2011): https://www.nationalgeographic.com/science/article/meet-the-agta-a-tribe-where-a-quarter-of-men-have-been-attacked-by-giant-snakes · bài gốc: https://pmc.ncbi.nlm.nih.gov/articles/PMC3248510
3. National Geographic — *Toumai and the Sabercats*: https://www.nationalgeographic.com/science/article/toumai-and-the-sabercats
4. Wikipedia — *Megantereon*: https://en.wikipedia.org/wiki/Megantereon
5. Wikipedia — *Chasmaporthetes*: https://en.wikipedia.org/wiki/Chasmaporthetes
6. Wikipedia — *Taung Child*: https://en.wikipedia.org/wiki/Taung_Child · NBC News — *Were human ancestors hunted by birds?*: https://www.nbcnews.com/id/wbna10819471
7. Wikipedia — *Dinofelis*: https://en.wikipedia.org/wiki/Dinofelis
8. Wikipedia — *Deinotherium*: https://en.wikipedia.org/wiki/Deinotherium
9. ScienceDaily — Kanjera South, *Earliest Archaeological Evidence of Persistent Hominin Carnivory* (PLoS ONE 2013): https://www.sciencedaily.com/releases/2013/05/130510124441.htm
10. Newswise — *Extinct horned crocodiles ate early hominids* (Brochu, PLoS ONE 2010): https://www.newswise.com/articles/extinct-horned-crocodiles-ate-early-hominids
11. Wikipedia — *Pelorovis*: https://en.wikipedia-on-ipfs.org/wiki/Pelorovis
12. Wikipedia — *Hippopotamus gorgops*: https://en.wikipedia.org/wiki/Hippopotamus_gorgops
13. Live Science — *Human ancestors butchered and ate elephants 1.8 million years ago* (2026): https://livescience.com/archaeology/human-evolution/human-ancestors-butchered-and-ate-elephants-18-million-years-ago-helping-to-fuel-their-large-brains
14. Wikipedia — *Theropithecus oswaldi*: https://en.wikipedia.org/wiki/Theropithecus_oswaldi
15. Wikipedia — *Pachycrocuta*: https://en.wikipedia.org/wiki/Pachycrocuta
16. CNN — *Largest ape Gigantopithecus blacki extinction* (2024): https://amp.cnn.com/cnn/2024/01/10/world/largest-ape-gigantopithecus-blacki-extinction-scn/index.html
17. Priroda (2022) — *Pleistocene of Vietnam: Gigantopithecus and Other Animals* (trang tóm tắt; Thẩm Khuyên lấy từ danh mục tham khảo, chưa đọc bài gốc 1996): https://journals.eco-vector.com/0032-874X/article/view/627718
18. IFLScience — *Did humans coexist with saber cats?* (Schöningen): https://www.iflscience.com/did-humans-coexist-saber-cats-24147
19. Sci.News — *Giant Marabou Storks Lived alongside Homo floresiensis*: https://www.sci.news/paleontology/leptoptilos-robustus-11070.html
20. Sci.News — *Homo floresiensis Probably Didn't Hunt Stegodons or Use Fire* (Veatch et al., Science Advances 2026): https://www.sci.news/othersciences/anthropology/homo-floresiensis-hunting-fire-use-14891.html
21. Archaeology (AIA) — *Neanderthal cave lion* (2023): https://www.archaeology.org/news/11833-231013-neanderthal-cave-lion
22. Wikipedia — *Cave hyena*: https://en.wikipedia.org/wiki/Cave_hyena
23. ScienceDaily — Kostenki 11 mammoth bone circle (2020): https://www.sciencedaily.com/releases/2020/03/200316204633.htm
24. Newsweek — *First humans in Australia lived alongside giant predator lizards* (Price, Quaternary Science Reviews): https://newsweek.com/first-humans-australia-lived-alongside-giant-predator-lizards-2-599753

> **Lưu ý cho người viết codex:** nguồn 4, 5, 7, 8, 11, 12, 14, 15, 22 là Wikipedia. Trước khi phát hành, nên dẫn thêm bài gốc mà Wikipedia trích, đặc biệt cho các câu ở lớp 3.
