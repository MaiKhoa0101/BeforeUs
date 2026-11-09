# Before Us — Prompt tạo sinh asset

> **File này dùng để làm gì:** chứa mọi prompt dùng để tạo sinh asset (hình, và về sau cả âm thanh) cho Before Us bằng công cụ AI như SpriteCook hay CellCog.
> Mỗi hạng mục có một mục riêng. Hạng mục mới thì thêm vào cuối file, không tạo file khác.
> Asset sinh ra là **bản nháp chờ Khoa duyệt**, không phải art chính thức.
> Ngày tạo: 09/11/2026. Hạng mục đầu tiên: sinh vật (theo `docs/bestiary.md`).
>
> **Cập nhật 09/11, Khoa quyết (D24 trong `docs/decisions.md`):**
> - Hoạt ảnh làm bằng **gắn xương (rigging)** trong Unity, **không** sinh nhiều ảnh cho từng động tác.
> - Mỗi sinh vật chỉ cần **một hình gốc ở tư thế để rig** và **một bộ mảnh cơ thể**.
> - Mọi động tác (rình, vồ, choáng, bỏ chạy…) làm bằng xương trong Unity.
>
> **Cập nhật 09/11 (v2), theo góp ý của Khoa sau lần thử Gemini:** phong cách bám sát hai mẫu hổ răng kiếm và trăn (khối màu phẳng, đổ bóng cắt cạnh, cơ bắp góc cạnh, viền sáng mảnh); tranh hồ sơ có **dáng theo bản năng** (rình, gầm, phô nanh, xòe cánh…) và bố cục vuông của hai mẫu. **Hình gốc để rig vẫn giữ dáng trung tính** cho dễ cắt mảnh (Khoa chốt 09/11).
>
> **Cập nhật 09/11 (v4), Khoa chốt 4 hình cho mỗi loài:** (1) hình gốc, (2) phân mảnh từ hình gốc, (3) hình hồ sơ, (4) hồ sơ đầy đủ. Hình 4 là tranh ngang, nền đầy đủ, con vật nằm nửa phải để ghép chữ bên trái. Hình 3 **không sinh**: game cắt từ hình 4 (§2.7).
>
> **Cập nhật 09/11 (v5), Khoa chốt:** nền hồ sơ **theo vùng của từng loài**, không dùng chung savan. Có 10 khối vùng, mỗi khối gồm cảnh, bảng màu và danh sách chặn (§2.8). Báo hoa mai và lợn rừng con vẽ trong rừng nhiệt đới, đúng vùng của chúng trong game.
>
> **Cập nhật 09/11 (v6), sau lần thử Gemini thứ hai:**
> - Hồ sơ vẽ theo tranh mẫu báo nền vàng (`docs/art-reference/style_codex_leopard.png`): vector phẳng kiểu áp phích, nền nhiều lớp trong tông của một màu. Màu chủ đạo lấy theo vùng.
> - Mỗi loài có dòng giải phẫu chính xác, và câu "NOT …" để không bị vẽ thành loài hiện đại.
> - Bộ mảnh ghi đúng số mảnh, tên từng mảnh và chỗ khớp; cấm chốt, đinh, viền, chữ.
> - Thêm §1.1 Quản Gemini: khung luật đầu cuộc trò chuyện, bảng kiểm loại hình, câu bắt làm lại.
> - Nền xóa được dùng hồng sen #FF00FF.
>
> **Cập nhật 09/11 (v7), sau lần thử thứ ba** (Gemini vẽ hình gốc thành cảnh báo trên cành savan):
> - Prompt ngắn, ràng buộc quan trọng đặt đầu.
> - Thứ tự làm: hồ sơ đầy đủ → hình gốc → bộ mảnh.
> - Hình gốc đính kèm hồ sơ của chính loài đó, **không** đính kèm ảnh mẫu phong cách.
> - Màu chủ đạo vùng chỉ áp cho nền, con vật giữ màu thật.

## Mục lục

1. Quy tắc chung (mọi hạng mục) — 1.1 Quản Gemini
2. Sinh vật — quy trình rig, khối prompt dùng chung, khung xương mẫu, bốn hình của mỗi loài (§2.7), vùng đất và nền hồ sơ (§2.8)
3. Sinh vật trong MVP (prompt đã ghép đủ)
4. Sinh vật cho bản đầy đủ (prompt đã ghép đủ)
5. Các hạng mục khác (chưa viết)

---

## 1. Quy tắc chung (mọi hạng mục)

- **Không ghi vào `Assets/Sprite/**`.** Thư mục đó là art Khoa tự vẽ (D3); các hoạt ảnh theo khung hình có sẵn trong đó giữ nguyên. Hình sinh ra để ở `Assets/Art/Generated/<hạng mục>/`. Khoa duyệt xong mới đưa vào dùng.
- **Làm 1 mẫu trước.** Sinh một con, rig thử, đưa Khoa duyệt, rồi mới làm cả loạt.
- **Phong cách:** theo đúng hai mẫu hổ răng kiếm và trăn của Khoa: minh họa game 2D, khối màu phẳng, đổ bóng cắt cạnh thành mảng góc cạnh, cơ bắp phóng đại, màu con vật rực và ấm, viền sáng mảnh trên mép. Nền **không** thuộc phong cách mà thuộc vùng (§2.8). Hồ sơ đầy đủ (hình 4): con vật ở **dáng theo bản năng**, dữ và ngầu. Hình gốc để rig: dáng trung tính, dễ cắt mảnh.
  - Khi dùng SpriteCook, tải hai mẫu đó lên làm `style_asset_ids`. Ảnh mẫu quan trọng hơn chữ.
  - Khối STYLE ở §2.4 chỉ hỗ trợ. Sau khi Khoa duyệt mẫu đầu tiên, chỉnh khối đó một lần cho khớp, rồi giữ nguyên cho mọi hình sau.
- **Luôn đính kèm ảnh mẫu phong cách** `docs/art-reference/style_codex_leopard.png` (tranh báo nền vàng Khoa chọn ngày 09/11) mỗi lần sinh. Prompt dặn chỉ lấy *cách vẽ*, không lấy màu vàng, con báo hay cảnh savan. Hai mẫu cũ của Khoa (`sample_sabertooth.png`, `sample_python.png`) cùng thư mục, dùng thêm khi cần.
- **Nền theo vùng, không bao giờ dùng chung một nền.** Game có nhiều vùng (rừng nhiệt đới, savan, ven hồ, Âu-Á, Đông Nam Á, Flores, băng hà, Úc, sa mạc, sông Nile). Mỗi vùng có loài đặc trưng, cảnh vật, bảng màu và ánh sáng riêng (§2.8). Hồ sơ của loài nào vẽ nền vùng của loài đó. Mỗi vùng còn có danh sách **chặn** (ví dụ vùng rừng: không đồng cỏ, không cây keo), để công cụ không kéo mọi tranh về savan. Khối phong cách **không** được chứa màu nền hay địa hình cụ thể.
- **Mỗi lần chỉ dán một prompt, ra một hình.** Đừng gộp hình gốc, bộ mảnh và tranh hồ sơ vào một yêu cầu: công cụ sẽ ghép thành một ảnh nhiều ô có chữ (lỗi đã gặp với Gemini ngày 09/11).
- **Lưu `asset_id`** của mọi hình vào `Assets/Art/Generated/spritecook-assets.json` (`asset_id`, `sha12`, `label`).
- **Không máu me, không cảnh giết chóc.** Không người thật, không thương hiệu, không nhân vật có bản quyền, không chữ trong hình.
- Prompt viết bằng tiếng Anh. Ghi chú viết bằng tiếng Việt.

### 1.1 Quản Gemini (bắt buộc khi dùng Gemini hoặc chatbot tạo ảnh)

Gemini đã mắc các lỗi sau trong lần thử ngày 09/11: gộp nhiều hình vào một ảnh có tiêu đề, thêm chữ, viền trắng quanh con vật, vẽ ô carô giả làm nền trong suốt, cắt mảnh tùy ý (thân cắt 5 khúc, có đinh chốt), và đổi loài tuyệt chủng thành loài hiện đại. Quy trình dưới đây để chặn các lỗi đó.

**Bước 1. Mở một cuộc trò chuyện mới cho mỗi loài**, dán khung luật này làm tin nhắn đầu tiên (chưa xin hình):
```
You are generating game assets for me. Rules for this whole conversation, no exceptions: 1) Each of my messages asks for exactly ONE image. Never combine several assets in one image, never make panels, sheets with titles, collages, before/after or comparison layouts. 2) Never draw any text, letters, titles, labels, numbers, logos or UI frames. 3) Never add a white outline, sticker border, glow or drop shadow around the subject. 4) When I ask for a plain magenta background, fill the whole background with flat #FF00FF and nothing else; never draw a checkerboard. 5) Follow the anatomy I describe exactly; do not replace an extinct animal with the modern animal it resembles. 6) For a parts sheet, draw exactly the number and list of pieces I give, nothing more; no pins, rivets, bolts or joint markers. 7) If you cannot follow a rule, tell me instead of drawing.
```

**Bước 2. Làm theo thứ tự 4 → 1 → 2, mỗi tin nhắn đính kèm đúng ảnh ghi ở prompt và dán đúng MỘT prompt:**
1. **Hồ sơ đầy đủ (hình 4)**: đính kèm ảnh mẫu phong cách `docs/art-reference/style_codex_leopard.png`. Riêng **báo hoa mai** thì đính kèm `sample_sabertooth.png` thay vào, vì ảnh mẫu cũng là con báo trên cành, Gemini sẽ chép nguyên cảnh.
2. **Hình gốc (hình 1)**: đính kèm **hình 4 vừa duyệt**, KHÔNG đính kèm ảnh mẫu phong cách. Gemini giữ thiết kế con vật từ hình 4, chỉ đổi dáng và bỏ nền.
3. **Bộ mảnh (hình 2)**: đính kèm **chỉ hình 1**.

Lý do (lần thử thứ ba, 09/11): khi đính kèm tranh báo trên cành để xin hình gốc con báo, Gemini vẽ lại đúng cảnh savan trên cành, bỏ qua yêu cầu nhìn ngang và nền hồng sen. Ảnh đính kèm luôn mạnh hơn chữ, nên chỉ đính kèm ảnh có **đúng thứ mình muốn giữ**.

**Prompt ngắn có chủ đích:** các ràng buộc quan trọng nhất (một con, nhìn ngang, nền hồng sen, không cảnh vật) nằm ở **đầu** prompt. Đừng thêm mô tả phong cách dài vào hình gốc: prompt càng dài, Gemini càng bỏ sót đoạn cuối.

**Bước 3. Kiểm trước khi nhận.** Sai bất kỳ dòng nào là **loại**, không sửa tay:

| Kiểm | Hình gốc | Bộ mảnh | Hồ sơ đầy đủ |
| --- | --- | --- | --- |
| Chỉ một hình, không ô, không tiêu đề | ✔ | ✔ | ✔ |
| Không chữ, số, nhãn nào | ✔ | ✔ | ✔ |
| Không viền trắng, không bóng đổ quanh con vật | ✔ | ✔ | ✔ |
| Nền hồng sen phẳng (#FF00FF), không ô carô | ✔ | ✔ | — |
| Nhìn ngang, dáng trung tính, miệng ngậm | ✔ | — | — |
| Đúng số mảnh và đúng tên mảnh theo danh sách "Kiểm mảnh"; thân là MỘT mảnh; không đinh, chốt | — | ✔ | — |
| Đúng giải phẫu, không giống loài hiện đại (đọc dòng `NOT ...` trong prompt) | ✔ | ✔ | ✔ |
| Nền đúng vùng và đúng màu chủ đạo của vùng (§2.8), con vật trong nửa phải, nửa trái thoáng | — | — | ✔ |

**Bước 4. Khi loại, nhắn đúng câu này** (thay `<lỗi>` bằng dòng sai), không giải thích thêm:
```
This image breaks the rule: <lỗi>. Generate it again from my previous prompt, following every rule. One image only.
```

**Bước 5. Sai 2 lần liên tiếp thì đổi cách:**
- Hình gốc và hồ sơ: chuyển sang SpriteCook (`generate_game_art`, `style_asset_ids`).
- Bộ mảnh: **cắt tay từ hình gốc** bằng Krita, GIMP hoặc Photoshop theo danh sách "Kiểm mảnh" (đường cắt đi qua khớp), rồi vẽ bù phần bị che ở đầu khớp. Đây là cách chắc chắn nhất; bộ mảnh do AI vẽ chỉ là đường tắt.

**Xóa nền hồng sen:** SpriteCook `remove_background`, hoặc Magic Wand chọn màu #FF00FF trong Krita, GIMP, Photoshop. Gemini không xuất được nền trong suốt thật nên mọi prompt dùng nền hồng sen.

---

## 2. Sinh vật — quy trình rig và khối dùng chung

### 2.1 Công cụ trong dự án (đã có, không cần thêm package)

`Packages/manifest.json` có sẵn `com.unity.feature.2d`. Bộ này kéo theo:
- `com.unity.2d.animation` 16.0.1: xương, Skinning Editor, IK, Sprite Library.
- `com.unity.2d.psdimporter` 15.0.1: nhập file nhiều lớp, mỗi lớp thành một mảnh.

### 2.2 Quy trình cho một sinh vật

| Bước | Làm gì | Công cụ | Đầu ra |
| --- | --- | --- | --- |
| 1 | Sinh **hình gốc ở tư thế để rig** (§2.3). Đây là hình chuẩn để so mọi mảnh. | SpriteCook `generate_game_art` + `style_asset_ids` | `beast_<id>_rigpose.png` |
| 2 | Sinh **bộ mảnh cơ thể** theo khung xương mẫu (§2.6), dùng `reference_asset_id` = hình bước 1. | SpriteCook | `beast_<id>_parts.png` |
| 3 | Kiểm mảnh: cùng tỉ lệ, cùng màu, cùng hướng sáng với hình gốc; khớp nối có phần **thừa che khớp** (§2.3). Mảnh nào lệch thì **cắt thẳng từ hình gốc** bằng Krita, GIMP hoặc Photoshop, rồi vẽ bù phần bị che, hoặc dùng SpriteCook `edit_asset_id` để nối dài phần bị che. | Krita / GIMP / Photoshop + SpriteCook | Các mảnh đã sửa |
| 4 | Ghép mảnh thành **file nhiều lớp**: mỗi mảnh một lớp, đặt đúng vị trí như hình gốc, đặt tên lớp theo §2.6, xếp thứ tự trước/sau theo §2.6. | Krita / Photoshop | `beast_<id>.psb` (hoặc `.psd`, chọn importer là PSD Importer) |
| 5 | Trong Unity: Skinning Editor → tạo xương theo §2.6 → Auto Geometry → Auto Weights → chỉnh tay chỗ khớp; gắn IK: Limb cho chân, chuỗi CCD hoặc FABRIK cho đuôi, cổ, thân rắn. | Unity 2D Animation | Prefab đã rig |
| 6 | Làm các đoạn hoạt ảnh trong danh sách của từng loài bằng xương, đặt tên đoạn khớp tên trạng thái ở `bestiary.md` §4. | Unity Animation | `beast_<id>_<trạng thái>.anim` |
| 7 | (Nếu cần biến thể) Đổi lớp lông, đốm bằng Sprite Library, dùng chung bộ xương. | Sprite Library | Biến thể |

**Vì sao vẫn cần bộ mảnh:** gắn xương lên **một ảnh liền** chỉ làm được khi các phần cơ thể không che nhau (ví dụ thân rắn duỗi thẳng). Thú bốn chân nhìn ngang thì chân xa bị thân che. Khi chân đó cử động sẽ lộ ra chỗ trống, nên chân xa phải là mảnh riêng, có phần thừa ở khớp.

### 2.3 Yêu cầu chung cho hình gốc (tư thế để rig)

- Nhìn ngang hoàn toàn, quay mặt sang phải. Không phối cảnh xiên, không co rút.
- Tư thế trung tính: đứng thẳng, miệng ngậm, đầu ngang vai. Đây là hình để cắt mảnh và gắn xương, nên ưu tiên dễ cắt. Vẻ dữ, ngầu đến từ phong cách vẽ và các đoạn hoạt ảnh bằng xương; còn **dáng bản năng chỉ dùng cho tranh hồ sơ**.
- Bốn chân đều thấy rõ, chân gần và chân xa **so le, không chồng lên nhau**.
- Đuôi duỗi ra sau, không cuộn.
- Chi phía xa tối hơn chi phía gần một bậc màu, để Unity xếp lớp dễ đọc.
- Nền **hồng sen phẳng #FF00FF**, rồi xóa nền (SpriteCook `remove_background` hoặc chọn màu trong Krita/GIMP/Photoshop). Không yêu cầu "transparent": Gemini sẽ vẽ ô carô giả.
- Không đổ bóng, không cảnh vật, không viền trắng quanh con vật.

Hướng sprite: mọi hình quay mặt sang phải. Trận Phòng thủ có kẻ địch đi sang trái; scene tự lật hình theo `_battle.Setup.Kind` (`Assets/Scripts/Combat/HUONG-DAN-NOI-SCENE.md`).

### 2.4 Khối prompt dùng chung

Mọi prompt ở §3–§4 **đã ghép sẵn**. Sửa thì sửa trong `tools/art/gen_prompts.py` rồi chạy lại.

**v7 (đang dùng): prompt ngắn, ràng buộc đặt đầu.**
```
Hồ sơ đầy đủ (làm đầu tiên): Wide 16:9 + <cảnh> + Attitude + Composition (con vật nửa PHẢI) + Anatomy + NOT-modern + Style (chỉ chép cách vẽ của ảnh mẫu) + Colors (màu chủ đạo vùng) + Place (bóng vùng) + <chặn vùng> + No text…
Hình gốc (thứ hai)        : Game sprite, một con, nhìn ngang hướng phải + Neutral pose + giữ thiết kế của hình 4 đính kèm, bỏ nền + Anatomy + NOT-modern + STYLE_SHORT + NO_SCENE
Bộ mảnh (thứ ba)          : Parts sheet của con vật trong hình 1 đính kèm + đúng N mảnh có tên + chỉ cắt ở khớp, thân một mảnh + xếp theo số + cấm chốt/viền/chữ + nền hồng sen
Hình hồ sơ                : không prompt, game cắt từ hồ sơ đầy đủ (§2.7)
```

**STYLE_SHORT**
```
Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients
```

**NO_SCENE**
```
Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

Các khối dài bên dưới là bản v6, giữ lại để tham khảo cách tả phong cách; prompt v7 không ghép chúng nữa.

**ONE** (mở đầu mọi prompt)
```
ONE IMAGE ONLY, one single asset, no panels.
```

**STYLE_REF** (đi kèm ảnh mẫu phong cách)
```
Use the attached style reference only for the drawing style, not for its colors, animal or setting
```

**STYLE** (cách vẽ con vật, giống tranh mẫu báo nền vàng)
```
Flat vector game illustration: the animal is drawn with clean crisp shapes and hard-edged cel shading in three to four tones per color (a base tone, one shadow tone, one dark accent tone and a bright highlight along the top edges), thin dark brown contour lines, coat patterns such as spots, stripes and scales drawn as clean flat shapes, exaggerated powerful anatomy with clearly defined muscle masses, bold readable silhouette, no texture noise, no gradients on the animal, no painterly brush strokes
```

**BG_REMOVE** (nền để xóa; không dùng 'transparent' vì Gemini vẽ ô carô giả)
```
plain flat solid magenta background (#FF00FF) filling the whole image, nothing else in the background
```

**TECH_RIG** (hình gốc)
```
single animal, full body, strict flat side view facing right, no perspective, no foreshortening, neutral relaxed pose exactly as described above, mouth closed, no snarl, plain flat solid magenta background (#FF00FF) filling the whole image, nothing else in the background, no ground, no grass, no cast shadow, no glow, no white outline or sticker border around the animal; this is a clean character reference that will be cut into parts for 2D skeletal animation
```

**MOOD_CODEX** (chỉ cho hồ sơ)
```
fierce, primal and dramatic mood, the animal caught in a moment of pure instinct
```

**CODEX_BG** (nền hồ sơ: các lớp bóng phẳng trong tông của một màu chủ đạo, màu lấy theo vùng)
```
background in the same flat vector poster style: four to five flat overlapping silhouette layers (foreground plants, middle trees, hills, far mountains, sky) built only from tones of the region's dominant hue, darkest and most saturated in the foreground, lighter and paler toward the horizon, a large soft glowing light source in the sky, simplified iconic silhouettes of the region's plants and landforms
```

**COMPOSITION** (bố cục hồ sơ 16:9)
```
wide 16:9 landscape illustration with the background reaching every edge, no border; the animal large and fully inside the right half of the frame in a dramatic three-quarter view, standing on a strong dark foreground element of its region (branch, rock, root, bank or ice ledge); the left half of the frame is calm and open (glowing sky and distant layers only) leaving clean empty space for text
```

**AVOID** (mọi hình)
```
no text, no letters, no labels, no titles, no numbers, no watermark, no panels, no collage, no frame, no white outline or sticker border, no drop shadow, no blood, no gore, no wounds, no modern humans, no modern objects, not photorealistic, not a 3D render, not pixel art, no cartoon or chibi proportions, no extra limbs, no duplicated parts
```

**AVOID_CODEX** (chỉ cho hồ sơ)
```
not cute, no calm, sleepy or stiff standing pose
```

**Tranh mẫu phong cách hồ sơ:** `docs/art-reference/style_codex_leopard.png`. Đặc điểm cần giữ:
- Con vật: khối phẳng sắc nét, đổ bóng cắt cạnh 3–4 tông, viền nâu sẫm mảnh, điểm sáng dọc mép trên, đốm vẽ thành mảng phẳng gọn.
- Nền: 4–5 lớp bóng phẳng chồng nhau (cây gần, cây giữa, đồi, núi xa, trời), **tất cả trong các tông của một màu**, đậm ở gần, nhạt dần về chân trời, một vầng sáng lớn trên trời.
- Cành cây hoặc mỏm đá đậm màu ở tiền cảnh làm chỗ đứng cho con vật.
- Khác tranh mẫu ở hai điểm: **màu chủ đạo theo vùng** (rừng xanh ngọc, băng hà xanh băng…), và **khổ 16:9, con vật ở nửa phải** để đặt chữ bên trái.

**Thiết lập SpriteCook (nếu dùng):** `pixel: false`; hình gốc và bộ mảnh `resolution: "2K"`, `smart_crop: false` cho bộ mảnh, rồi `remove_background`; hồ sơ `aspect_ratio: "16:9"`, `bg_mode: "include"`.

### 2.5 Ví dụ đã ghép đủ (báo hoa mai, hình gốc)

```
Game sprite: one leopard alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: compact powerful body, relatively short legs, tail about as long as the body, broad head with small rounded ears, golden coat covered in rosettes that are rings with open centers, pale belly. realistic wild leopard proportions, not a jaguar, not a cheetah, no lion features. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

### 2.6 Khung xương mẫu và danh sách mảnh

Mỗi khung có: dáng để rig, **danh sách mảnh cố định** (mỗi mảnh là một đoạn nằm giữa hai khớp), tên lớp trong file `.psb`, và xương.
Chỉ cắt ở **khớp thật**: hàm, cổ, vai, khuỷu, cổ tay, hông, gối, cổ chân, gốc đuôi và các đốt đuôi, gốc cánh, cổ tay cánh, các đốt vòi. Không cắt giữa một khối cơ (ví dụ cắt ngang thân).
Mỗi mảnh kéo dài và bo tròn thêm khoảng 15% ở đầu khớp, để khi xoay không hở. Chi phía xa tối hơn một bậc màu.

#### Khung A — thú bốn chân

Dùng cho: mèo lớn, linh cẩu, con mồi nhỏ, trâu, hà mã, khỉ đầu chó (đi bốn chân).

Dáng để rig:
```
standing in a neutral pose, all four legs clearly visible, near legs and far legs staggered so they do not overlap, head level with the shoulders, tail extended straight back and slightly down, far-side legs one shade darker than near-side legs
```

Mảnh (19 mảnh mặc định; loài có thể thêm hoặc bớt, xem dòng "Kiểm mảnh" của từng loài):

1. head (skull with upper jaw and ears) — joins the neck behind the skull
2. lower jaw — hinges under the ear
3. neck — joins the torso in front of the shoulders
4. torso, chest to hips in ONE piece — carries the neck, legs and tail
5. tail base — joins the hips
6. tail middle — joins the tail base
7. tail tip — joins the tail middle
8. near front leg upper part (shoulder to elbow) — joins the torso at the shoulder
9. far front leg upper part (shoulder to elbow) — joins the torso at the shoulder
10. near front leg lower part (elbow to wrist) — joins the upper part at the elbow
11. far front leg lower part (elbow to wrist) — joins the upper part at the elbow
12. near front paw — joins at the wrist
13. far front paw — joins at the wrist
14. near hind leg upper part (hip to knee) — joins the torso at the hip
15. far hind leg upper part (hip to knee) — joins the torso at the hip
16. near hind leg lower part (knee to ankle) — joins the upper part at the knee
17. far hind leg lower part (knee to ankle) — joins the upper part at the knee
18. near hind paw — joins at the ankle
19. far hind paw — joins at the ankle

Tên lớp (sau → trước): `leg_hind_far_upper, leg_hind_far_lower, foot_hind_far, leg_front_far_upper, leg_front_far_lower, foot_front_far, ear_far, tail_3, tail_2, tail_1, torso, neck, jaw, head, ear_near, leg_hind_near_upper, leg_hind_near_lower, foot_hind_near, leg_front_near_upper, leg_front_near_lower, foot_front_near`

Xương:
- Gốc `hips`. Từ đó: `spine` → `chest` → `neck` → `head` → `jaw`.
- `tail_1` → `tail_2` → `tail_3`, nối từ `hips`.
- Mỗi chân 3 xương (`upper`, `lower`, `foot`). Chân trước nối từ `chest`, chân sau nối từ `hips`.
- IK: Limb cho mỗi chân; CCD cho đuôi.
- Tai (`ear`) chỉ có ở loài cần cụp tai: báo, linh cẩu, thỏ.

#### Khung B — rắn

Dùng cho: trăn.

Dáng để rig:
```
body stretched out almost straight along the ground with only a very gentle wave, head slightly raised at the front, the whole body visible with no part crossing over another
```

Mảnh (3 mảnh mặc định; loài có thể thêm hoặc bớt, xem dòng "Kiểm mảnh" của từng loài):

1. head (skull with upper jaw) — joins the front end of the body
2. lower jaw — hinges at the back of the head
3. body, from neck to tail tip in ONE long continuous piece, stretched straight — bones are added inside it later

Tên lớp (sau → trước): `body, jaw, head`

Xương:
- Chuỗi 12–16 xương dọc thân (`body_01` … `body_16`), rồi `head`, `jaw`.
- Gắn xương thẳng lên **một mảnh thân liền**.
- IK: FABRIK cho cả chuỗi.
- Ngoại lệ: động tác Siết dùng thêm **một mảnh vòng quấn** (§3.2), đổi bằng Sprite Library, vì một lưới hình không vẽ được thân tự đè lên chính nó.

#### Khung C — bò sát thân dài

Dùng cho: cá sấu, rồng Komodo, Megalania.

Dáng để rig:
```
low sprawling stance with the belly close to the ground, legs bent out to the side and all four visible without overlapping, long tail extended straight back on the ground
```

Mảnh (16 mảnh mặc định; loài có thể thêm hoặc bớt, xem dòng "Kiểm mảnh" của từng loài):

1. head (skull with upper jaw) — joins the neck
2. lower jaw — hinges at the back of the head
3. neck — joins the torso
4. torso in ONE piece — carries neck, legs and tail
5. tail piece 1 — joins the hips
6. tail piece 2 — joins tail piece 1
7. tail piece 3 — joins tail piece 2
8. tail piece 4, the tip — joins tail piece 3
9. near front leg upper part — joins the torso
10. near hind leg upper part — joins the torso
11. far front leg upper part — joins the torso
12. far hind leg upper part — joins the torso
13. near front leg lower part with foot and claws — joins at the elbow or knee
14. near hind leg lower part with foot and claws — joins at the elbow or knee
15. far front leg lower part with foot and claws — joins at the elbow or knee
16. far hind leg lower part with foot and claws — joins at the elbow or knee

Tên lớp (sau → trước): Như khung A, nhưng đuôi 4 đoạn (`tail_1` … `tail_4`), chân 2 đoạn (`upper`, `lower`), không có tai.

Xương:
- Như khung A, đuôi 4 xương, mỗi chân 2 xương.

#### Khung D — chim

Dùng cho: đại bàng, cò khổng lồ (cò đứng: chân dài 2 đoạn).

Dáng để rig:
```
side view with both wings raised and spread in a gliding position, near wing in front of the body and far wing behind, legs tucked under the body with talons visible, tail fanned slightly
```

Mảnh (8 mảnh mặc định; loài có thể thêm hoặc bớt, xem dòng "Kiểm mảnh" của từng loài):

1. head with beak — joins the body at the neck
2. body in ONE piece — carries everything
3. near wing inner part (arm) — joins the body at the shoulder
4. near wing outer part (long flight feathers) — joins the inner part at the wrist
5. far wing in ONE piece, one shade darker — joins the body behind the shoulder
6. tail fan — joins the rear of the body
7. near leg with talons — joins the belly
8. far leg with talons, one shade darker — joins the belly

Tên lớp (sau → trước): `wing_far, leg_far, tail, body, head, leg_near, wing_near_inner, wing_near_outer`

Xương:
- Gốc `body` → `neck` → `head`.
- `wing_near_inner` → `wing_near_outer`.
- `wing_far`, `tail`, `leg_near`, `leg_far`.
- IK: không cần.

#### Khung E — voi

Dùng cho: *Deinotherium*, voi *Palaeoloxodon*, *Stegodon*, ma mút.

Dáng để rig:
```
standing in a neutral pose, all four pillar-like legs visible and staggered, trunk hanging straight down and slightly forward, tusks fully visible, tail hanging down
```

Mảnh (16 mảnh mặc định; loài có thể thêm hoặc bớt, xem dòng "Kiểm mảnh" của từng loài):

1. head (skull without trunk, without tusks) — joins the torso at the neck
2. trunk piece 1, the base — joins the face
3. trunk piece 2 — joins trunk piece 1
4. trunk piece 3, the tip — joins trunk piece 2
5. tusks as ONE piece — fixed to the face
6. ear — fixed to the side of the head
7. torso in ONE piece — carries head, legs and tail
8. tail — joins the hips
9. near front leg upper part — joins the torso
10. near hind leg upper part — joins the torso
11. far front leg upper part — joins the torso
12. far hind leg upper part — joins the torso
13. near front leg lower part with foot — joins at the elbow or knee
14. near hind leg lower part with foot — joins at the elbow or knee
15. far front leg lower part with foot — joins at the elbow or knee
16. far hind leg lower part with foot — joins at the elbow or knee

Tên lớp (sau → trước): Như khung A, thêm `ear`, `tusks`, `trunk_1`, `trunk_2`, `trunk_3` nằm trên `head`. Không có `jaw` (trừ *Deinotherium*: `jaw` mang ngà).

Xương:
- Như khung A, thêm chuỗi `trunk_1` → `trunk_3` (IK: CCD) và `ear`.

#### Khung F — vượn lớn

Dùng cho: *Gigantopithecus*.

Dáng để rig:
```
standing on all four limbs in a knuckle-walking pose, long arms and shorter legs all visible and staggered, head level
```

Mảnh (15 mảnh mặc định; loài có thể thêm hoặc bớt, xem dòng "Kiểm mảnh" của từng loài):

1. head (skull with upper jaw) — joins the torso
2. lower jaw — hinges under the ear
3. torso in ONE piece — carries head and limbs
4. near arm upper part — joins at the shoulder
5. far arm upper part — joins at the shoulder
6. near forearm — joins at the elbow
7. far forearm — joins at the elbow
8. near hand — joins at the wrist
9. far hand — joins at the wrist
10. near thigh — joins at the hip
11. far thigh — joins at the hip
12. near shin — joins at the knee
13. far shin — joins at the knee
14. near foot — joins at the ankle
15. far foot — joins at the ankle

Tên lớp (sau → trước): Như khung A, không có đuôi; chân trước thành `arm_upper`, `forearm`, `hand`.

Xương:
- Như khung A, không có đuôi.
### 2.7 Bốn hình của mỗi sinh vật, và hình hồ sơ cắt trong game

| Số | Hình | Dùng ở đâu | Cách làm |
| --- | --- | --- | --- |
| 1 | Hình gốc | Chuẩn để cắt mảnh | Prompt, dáng trung tính, nhìn ngang |
| 2 | Phân mảnh từ hình gốc | Gắn xương, hoạt ảnh trong trận | Prompt (`reference_asset_id` = hình 1), rồi sửa tay nếu lệch |
| 3 | Hình hồ sơ | Thẻ sau trận, ô trong lưới Bảo tàng | **Không prompt.** Game cắt từ hình 4 |
| 4 | Hồ sơ đầy đủ | Trang hồ sơ trong Bảo tàng, có chữ đè bên trái | Prompt, tranh ngang 16:9, nền đầy đủ |

**Vì sao hình 3 không cần sinh:** con vật trong hình 4 luôn nằm trọn trong nửa phải, nên chỉ cần **hiện phần bên phải của chính file hình 4**.
- Không tốn lượt sinh ảnh, không thêm file.
- Hai hình luôn khớp nhau. Sinh riêng thì con vật ở hai hình sẽ khác nhau.

**Cách làm trong UI Toolkit** (một texture, hai cách hiện):

```css
/* Hình hồ sơ: ô vuông, chỉ thấy nửa phải của hồ sơ đầy đủ */
.codex-card-art {
    width: 160px;
    height: 160px;
    -unity-background-scale-mode: scale-and-crop;  /* phủ kín ô, phần thừa bị cắt */
    background-position-x: right;                  /* neo vào mép phải, nơi có con vật */
    background-position-y: center;
}
/* Muốn viền tan trắng giống hai mẫu: đặt thêm một lớp con phủ lên trên,
   dùng một ảnh mặt nạ vignette (trong suốt ở giữa, trắng ở mép) cho mọi thẻ. */
.codex-card-vignette {
    position: absolute; left: 0; top: 0; right: 0; bottom: 0;
    background-image: url("project://database/Assets/UI/Codex/card_vignette.png");
}
/* Hồ sơ đầy đủ: hiện cả tranh, chữ đặt trong nửa trái */
.codex-full-art { -unity-background-scale-mode: scale-and-crop; }
.codex-full-text { position: absolute; left: 4%; top: 8%; width: 44%; }
```

**Khi một tranh lệch bố cục** (con vật lấn sang trái): đừng sinh lại vội. Thêm trường `card_focus_x` (0 = mép trái, 1 = mép phải; mặc định 1) cho hồ sơ đó trong `codex.vi.json`. Code UI đặt `background-position-x` theo giá trị này. Chỉ sinh lại khi con vật quá to, không cắt vuông được.

Ảnh `card_vignette.png` là một ảnh dùng chung cho mọi thẻ, làm một lần.

### 2.8 Vùng đất và nền hồ sơ

Khoa chốt 09/11: *"game là ở các vùng khác nhau, mỗi vùng sẽ có các con vật đặc trưng của vùng đó"*. Nền hồ sơ dùng **màu chủ đạo và bóng cây, địa hình của vùng**, vẽ theo phong cách tranh mẫu (các lớp bóng phẳng trong tông của một màu). Không bao giờ dùng chung nền savan.

Vùng 0 và 1 là hai vùng của MVP (`balance.json` `regions`). Các vùng còn lại là đề xuất cho bản đầy đủ (`bestiary.md` §8.3).

| Mã vùng | Tên | Trong game | Màu chủ đạo | Loài |
| --- | --- | --- | --- | --- |
| `rainforest` | Rừng nhiệt đới | vùng 0 (MVP) | deep emerald green | Báo hoa mai, Trăn đá châu Phi, Lợn rừng con (con mồi) |
| `savanna` | Savan | vùng 1 (MVP) | warm golden amber | Linh cẩu (bầy, theo *Chasmaporthetes*), Hổ răng kiếm, Linh dương nhỏ (con mồi), Thỏ rừng (con mồi), Đại bàng lớn (chuyện đứa trẻ Taung, MVP-2), Báo răng kiếm giả, Khủng tượng ngà cong, Khỉ đầu chó khổng lồ |
| `lakeshore` | Savan ven hồ | bản đầy đủ [ĐỀ XUẤT] | muted teal and olive | Hổ răng kiếm khổng lồ, Cá sấu sừng, Trâu sừng cong, Hà mã khổng lồ, Voi ngà thẳng |
| `eurasia_temperate` | Âu-Á ôn đới | bản đầy đủ [ĐỀ XUẤT] | cool slate blue with rust-orange accents | Linh cẩu khổng lồ, Hổ răng mã tấu |
| `southeast_asia` | Đông Nam Á núi đá vôi | bản đầy đủ [ĐỀ XUẤT] | misty jade green | Vượn khổng lồ |
| `flores` | Đảo Flores | bản đầy đủ [ĐỀ XUẤT] | warm sand gold with turquoise sea accents | Cò già khổng lồ, Rồng Komodo, Voi lùn Stegodon |
| `ice_age_eurasia` | Âu-Á băng hà | bản đầy đủ [ĐỀ XUẤT] | icy blue lit by pale pink-gold low sunlight | Sư tử hang, Linh cẩu hang, Voi ma mút lông xoăn |
| `australia` | Úc | bản đầy đủ [ĐỀ XUẤT] | red ochre | Thằn lằn khổng lồ |
| `desert` | Sa mạc | bản đầy đủ [ĐỀ XUẤT] — chưa có loài | terracotta and sand | — (chưa có loài, cần nghiên cứu) |
| `nile` | Đồng bằng sông Nile | bản đầy đủ [ĐỀ XUẤT] — chưa có loài | papyrus green with river blue | — (chưa có loài, cần nghiên cứu) |

#### `rainforest` — Rừng nhiệt đới

Màu chủ đạo:
```
deep emerald green, from dark emerald in the foreground to pale jade mist at the back
```
Bóng cây, địa hình:
```
giant trunks with buttress roots, hanging lianas, broad leaves and ferns, layered canopy, shafts of light
```
Chặn:
```
not savanna, no open grassland, no golden dry grass, no acacia trees, no snow, no desert
```

#### `savanna` — Savan

Màu chủ đạo:
```
warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon
```
Bóng cây, địa hình:
```
flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills
```
Chặn:
```
no rainforest, no dense jungle, no snow, no pine forest
```

#### `lakeshore` — Savan ven hồ

Màu chủ đạo:
```
muted teal and olive, from dark olive in the foreground to pale silver-teal water and sky
```
Bóng cây, địa hình:
```
reed beds, still lake water with reflections, gallery trees along the shore, volcanic hills
```
Chặn:
```
no rainforest, no snow, no desert dunes
```

#### `eurasia_temperate` — Âu-Á ôn đới

Màu chủ đạo:
```
cool slate blue with rust-orange accents, from dark slate in the foreground to pale grey-blue sky
```
Bóng cây, địa hình:
```
birch and pine trunks, limestone hills with cave mouths, cold lakes with reeds
```
Chặn:
```
no savanna, no acacia trees, no tropical plants, no palms
```

#### `southeast_asia` — Đông Nam Á núi đá vôi

Màu chủ đạo:
```
misty jade green, from dark jade in the foreground to pale white-green mist
```
Bóng cây, địa hình:
```
tall steep karst limestone towers, bamboo clumps, bands of drifting cloud, a winding river
```
Chặn:
```
no savanna, no acacia trees, no snow, no desert
```

#### `flores` — Đảo Flores

Màu chủ đạo:
```
warm sand gold with turquoise sea accents, from dark ochre in the foreground to pale turquoise sky
```
Bóng cây, địa hình:
```
lontar palms, a large limestone cave mouth, dry island hills, the sea on the horizon
```
Chặn:
```
no African savanna animals, no snow, no pine forest
```

#### `ice_age_eurasia` — Âu-Á băng hà

Màu chủ đạo:
```
icy blue lit by pale pink-gold low sunlight, from dark steel blue in the foreground to almost white sky
```
Bóng cây, địa hình:
```
snowy steppe, frozen river, limestone cliffs and rock shelters, sparse wind-bent trees, falling snow
```
Chặn:
```
no savanna, no acacia trees, no tropical plants, no green jungle
```

#### `australia` — Úc

Màu chủ đạo:
```
red ochre, from dark rust in the foreground to pale hot orange sky
```
Bóng cây, địa hình:
```
red rock outcrops, grey-green eucalyptus trees, spinifex grass clumps
```
Chặn:
```
no African savanna animals, no snow, no jungle
```

#### `desert` — Sa mạc

Màu chủ đạo:
```
terracotta and sand, from dark terracotta in the foreground to pale hot sky
```
Bóng cây, địa hình:
```
rock plateaus, wind-carved stones, sand dunes, rare thorn bushes
```
Chặn:
```
no rainforest, no snow, no lush grass
```

#### `nile` — Đồng bằng sông Nile

Màu chủ đạo:
```
papyrus green with river blue, from dark green in the foreground to pale warm sky
```
Bóng cây, địa hình:
```
papyrus reeds, palm groves, a wide slow river, desert edge far away
```
Chặn:
```
no snow, no pine forest, no jungle
```

**Thêm loài:** gán `region` trong dữ liệu loài. **Thêm vùng:** thêm một mục vào `REGIONS`. Chạy lại script.

---

## 3. Sinh vật trong MVP

Mỗi loài có **4 hình**: (1) hình gốc dáng trung tính, (2) phân mảnh từ hình gốc, (3) hình hồ sơ — game tự cắt từ hình 4, không cần prompt, (4) hồ sơ đầy đủ — tranh ngang, nền theo vùng (§2.8), con vật nằm nửa phải.
Mỗi prompt đã ghép đủ, dán thẳng được. **Thứ tự làm: hình 4 → hình 1 → hình 2.** **Dùng Gemini thì làm đúng §1.1.**

Thứ tự làm: báo hoa mai (mẫu để duyệt cả quy trình) → trăn → linh cẩu → hổ răng kiếm → 3 con mồi → đại bàng (MVP-2).

### 3.1 `leopard` — Báo hoa mai (*Panthera pardus*) — khung A (thú bốn chân)

**Vùng:** Rừng nhiệt đới (`rainforest`, vùng 0 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Rình rồi vồ: tai cụp, mắt khóa mục tiêu, thân nén như lò xo.

- Thân dài, chân ngắn hơn sư tử, đuôi dài; lông vàng, đốm hoa thị.
- Thêm lớp và xương `ear` để cụp tai khi bỏ chạy.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_leopard_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one leopard alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: compact powerful body, relatively short legs, tail about as long as the body, broad head with small rounded ears, golden coat covered in rosettes that are rings with open centers, pale belly. realistic wild leopard proportions, not a jaguar, not a cheetah, no lion features. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_leopard_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 21 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the leopard in the attached picture. Draw exactly 21 separate pieces of this same leopard, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw, (20) near ear, (21) far ear. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw; 20. near ear; 21. far ear.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_leopard.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Trên cành rêu giữa rừng mưa, sắp đổ xuống (vùng 0). Chuyện SK 54 kể bằng chữ, không vẽ.
```
Wide 16:9 illustration. A leopard crouched along a thick mossy branch high in the rainforest, ears flat, staring straight down at its prey, one paw reaching forward, about to drop. Attitude: ears flattened, eyes locked on prey, lips pulled back over long canines, body coiled like a spring about to pounce. Composition: the leopard is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: compact powerful body, relatively short legs, tail about as long as the body, broad head with small rounded ears, golden coat covered in rosettes that are rings with open centers, pale belly. realistic wild leopard proportions, not a jaguar, not a cheetah, no lion features. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of deep emerald green, from dark emerald in the foreground to pale jade mist at the back; the leopard keeps its own natural colors and stands out bright against it. Place: giant trunks with buttress roots, hanging lianas, broad leaves and ferns, layered canopy, shafts of light. Not savanna, no open grassland, no golden dry grass, no acacia trees, no snow, no desert. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): `idle` (thở, vẫy đuôi); `walk`; `run`; `stalk` (hạ thấp người, bò sát đất); `attack` (vồ); `stagger` (lắc đầu, chân loạng choạng); `flee` (quay đầu chạy, cụp tai); `grab_drag` (lùi, cổ ghì xuống); `panic` (chạy nước rút).

### 3.2 `python` — Trăn đá châu Phi (*Python sebae*) — khung B (rắn)

**Vùng:** Rừng nhiệt đới (`rainforest`, vùng 0 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Phục kích: đầu rụt về sau hình chữ S, há miệng, sắp phóng.

- Thân to, dài; đầu hình tam giác; vằn ô-liu và nâu hạt dẻ có viền sẫm, bụng nhạt.
- Dùng khung 16:9.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_python_rigpose.png` · `aspect_ratio: "16:9"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one python alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: body stretched out almost straight in a gentle wave, head slightly raised, no part of the body crossing another, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: very thick heavy body, triangular head with a dark arrow-shaped mark on the crown and a dark stripe through the eye, irregular olive and chestnut blotches edged in black along the back, pale cream belly, round pupils. not a cobra, no hood, no venom fangs. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_python_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 3 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the python in the attached picture. Draw exactly 3 separate pieces of this same python, same colors, same outlines, same scale: (1) head (skull with upper jaw), (2) lower jaw, (3) body. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw); 2. lower jaw; 3. body, from neck to tail tip in ONE long continuous piece, stretched straight.

**2b. Mảnh vòng quấn cho trạng thái Siết (ngoại lệ của khung B)** → `beast_python_coil.png` · đính kèm: hình 1
```
Using the python in the attached picture: three tight coils of the same python body wrapped upright around an empty cylindrical space, seen from the side, the coils overlapping naturally, no head, cut cleanly at both ends so it can join the rest of the body, same scale and colors as the reference image. same colors and outlines. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_python.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Rễ bạnh khổng lồ dưới tán rừng mưa, thế phóng.
```
Wide 16:9 illustration. A huge African rock python rising from its coils among giant buttress roots on the rainforest floor, head drawn back in a tight S, jaws wide open showing curved teeth, about to strike. Attitude: cold unblinking yellow eyes, jaws open showing curved teeth, front of the body pulled back ready to strike. Composition: the python is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: very thick heavy body, triangular head with a dark arrow-shaped mark on the crown and a dark stripe through the eye, irregular olive and chestnut blotches edged in black along the back, pale cream belly, round pupils. not a cobra, no hood, no venom fangs. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of deep emerald green, from dark emerald in the foreground to pale jade mist at the back; the python keeps its own natural colors and stands out bright against it. Place: giant trunks with buttress roots, hanging lianas, broad leaves and ferns, layered canopy, shafts of light. Not savanna, no open grassland, no golden dry grass, no acacia trees, no snow, no desert. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): `idle` (thân lượn rất nhẹ); `slither`; `stalk` (nằm sát đất, đầu đứng im); `attack` (phóng thẳng nửa thân trước); `stagger` (co giật, đầu rụt lại); `flee` (trườn nhanh, sóng thân dài); `grab_squeeze` (đổi sang mảnh vòng quấn, siết theo nhịp).

### 3.3 `hyena_pack` — Linh cẩu (bầy, theo *Chasmaporthetes*) — khung A (thú bốn chân)

**Vùng:** Savan (`savanna`, vùng 1 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Săn theo bầy: lông gáy dựng, đầu thấp, ánh mắt không buông.

- Đây **không** phải linh cẩu đốm ngày nay. *Chasmaporthetes* có chân dài, mảnh như báo săn.
- Bầy có 3 con dùng chung một bộ xương. Khác biệt chỉ là đổi lớp `torso` và `head` bằng Sprite Library (2 biến thể đốm, prompt bên dưới).

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_hyena_pack_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one hunting hyena alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Chasmaporthetes: long slender legs of nearly equal length so the back is almost level, light build like a cheetah, smaller narrow head with cat-like slicing teeth, short sandy coat with sparse small dark spots, small rounded ears, thin tail. NOT a modern spotted hyena: no strongly sloping back, no heavy bone-crushing head, no shaggy mane. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_hyena_pack_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 21 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the hunting hyena in the attached picture. Draw exactly 21 separate pieces of this same hunting hyena, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw, (20) near ear, (21) far ear. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw; 20. near ear; 21. far ear.

**2b. Biến thể đốm (dùng `edit_asset_id` trên mảnh `torso` và mảnh `head`, chạy 2 lần)** → `beast_hyena_pack_var1.png / _var2.png` · đính kèm: hình 1
```
Using the hunting hyena in the attached picture: the same body part with a slightly different dark spot pattern and a slightly darker muzzle, identical shape, size and outline. same colors and outlines. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_chasmaporthetes.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Cả bầy đang lao qua đồng cỏ, bụi tung.
```
Wide 16:9 illustration. Three long-legged hunting hyenas running flat out in a tight pack across the savanna grass, the leader in front with jaws open, dust kicked up behind them. Attitude: hackles raised along the spine, head low, lips curled, a relentless hunting stare. Composition: the hunting hyena is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Chasmaporthetes: long slender legs of nearly equal length so the back is almost level, light build like a cheetah, smaller narrow head with cat-like slicing teeth, short sandy coat with sparse small dark spots, small rounded ears, thin tail. NOT a modern spotted hyena: no strongly sloping back, no heavy bone-crushing head, no shaggy mane. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon; the hunting hyena keeps its own natural colors and stands out bright against it. Place: flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills. No rainforest, no dense jungle, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): `idle`; `run` (chạy sải dài); `attack` (lao tới, há miệng); `stagger`; `flee` (cụp đuôi, cúi đầu).

### 3.4 `sabertooth` — Hổ răng kiếm (*Megantereon*) — khung A (thú bốn chân)

**Vùng:** Savan (`savanna`, vùng 1 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Gầm, phô răng kiếm, chân trước khổng lồ sẵn sàng ghì.

- Cỡ báo đốm, **không** to bằng sư tử. Chân trước rất khỏe; đuôi ngắn, chỉ cần 2 đoạn.
- Nanh trên dài, dẹt, chìa xuống dưới cằm. Nanh **gắn vào mảnh `head`**, không gắn vào `jaw`.
- Khoa đã vẽ một mẫu hổ răng kiếm: dùng mẫu đó làm `reference_asset_id` khi sinh hình gốc.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_sabertooth_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one sabertooth cat alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Megantereon: jaguar-sized, very robust heavily muscled forelimbs and thick neck, short face, long flat blade-like upper canines reaching below the chin and resting against a bony flange on the front of the lower jaw, short tail, plain tawny coat with faint pale markings. NOT Smilodon (smaller and lighter), NOT a modern tiger or lion: no stripes, no mane. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_sabertooth_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 18 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the sabertooth cat in the attached picture. Draw exactly 18 separate pieces of this same sabertooth cat, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail tip, (7) near front leg upper part (shoulder to elbow), (8) far front leg upper part (shoulder to elbow), (9) near front leg lower part (elbow to wrist), (10) far front leg lower part (elbow to wrist), (11) near front paw, (12) far front paw, (13) near hind leg upper part (hip to knee), (14) far hind leg upper part (hip to knee), (15) near hind leg lower part (knee to ankle), (16) far hind leg lower part (knee to ankle), (17) near hind paw, (18) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail tip; 7. near front leg upper part (shoulder to elbow); 8. far front leg upper part (shoulder to elbow); 9. near front leg lower part (elbow to wrist); 10. far front leg lower part (elbow to wrist); 11. near front paw; 12. far front paw; 13. near hind leg upper part (hip to knee); 14. far hind leg upper part (hip to knee); 15. near hind leg lower part (knee to ankle); 16. far hind leg lower part (knee to ankle); 17. near hind paw; 18. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_megantereon.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Phóng ra từ cỏ khô cao cạnh cây keo.
```
Wide 16:9 illustration. A Megantereon sabertooth bursting out of tall dry savanna grass beside an acacia, snarling with its long sabers bared, massive forelimbs reaching forward. Attitude: snarling with the long sabers fully bared, shoulders rolling, massive forelimbs ready to grab, menacing glare. Composition: the sabertooth cat is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Megantereon: jaguar-sized, very robust heavily muscled forelimbs and thick neck, short face, long flat blade-like upper canines reaching below the chin and resting against a bony flange on the front of the lower jaw, short tail, plain tawny coat with faint pale markings. NOT Smilodon (smaller and lighter), NOT a modern tiger or lion: no stripes, no mane. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon; the sabertooth cat keeps its own natural colors and stands out bright against it. Place: flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills. No rainforest, no dense jungle, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): `idle`; `walk`; `attack` (chồm lên, hai chân trước vươn ra, há miệng); `stagger`; `enrage` (gầm, lưng cong, cụp tai); `panic`.

### 3.5 `small_antelope` — Linh dương nhỏ (con mồi) — khung A (thú bốn chân)

**Vùng:** Savan (`savanna`, vùng 1 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Cảnh giác: mũi phồng, tai dựng, sẵn sàng phóng đi.

- Sừng gắn vào mảnh `head`.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_small_antelope_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one small antelope alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: small duiker-like antelope with an arched back, hindquarters higher than the shoulders, short straight spike horns, reddish-brown coat, large dark eyes, thin legs. not an impala, no lyre-shaped horns. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_small_antelope_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 19 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the small antelope in the attached picture. Draw exactly 19 separate pieces of this same small antelope, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_small_antelope.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Bờ hồ giữa savan lúc bình minh (chuyện Kanjera South).
```
Wide 16:9 illustration. A small antelope in a high startled leap along the shore of a savanna lake at dawn, legs tucked, ears back. Attitude: nostrils flared, eyes wide, ready to bolt at any sound. Composition: the small antelope is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: small duiker-like antelope with an arched back, hindquarters higher than the shoulders, short straight spike horns, reddish-brown coat, large dark eyes, thin legs. not an impala, no lyre-shaped horns. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon; the small antelope keeps its own natural colors and stands out bright against it. Place: flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills. No rainforest, no dense jungle, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): `idle`; `run` (nhảy); `stagger`; `panic` (nhảy zig-zag).

### 3.6 `boar_piglet` — Lợn rừng con (con mồi) — khung A (thú bốn chân)

**Vùng:** Rừng nhiệt đới (`rainforest`, vùng 0 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Liều lĩnh: lông gáy dựng, cúi đầu, không chịu lùi.

- Đuôi chỉ cần 1 đoạn. Không có tranh hồ sơ riêng có nguồn lịch sử (xem `bestiary.md` §2.3); tranh dưới đây chỉ là hình minh họa lớp 1.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_boar_piglet_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one wild piglet alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: young wild pig, stocky round body, light brown coat with pale horizontal stripes along the body, short snout, tiny tusks just showing. not a domestic pink pig. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_boar_piglet_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 17 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the wild piglet in the attached picture. Draw exactly 17 separate pieces of this same wild piglet, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) near front leg upper part (shoulder to elbow), (7) far front leg upper part (shoulder to elbow), (8) near front leg lower part (elbow to wrist), (9) far front leg lower part (elbow to wrist), (10) near front paw, (11) far front paw, (12) near hind leg upper part (hip to knee), (13) far hind leg upper part (hip to knee), (14) near hind leg lower part (knee to ankle), (15) far hind leg lower part (knee to ankle), (16) near hind paw, (17) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. near front leg upper part (shoulder to elbow); 7. far front leg upper part (shoulder to elbow); 8. near front leg lower part (elbow to wrist); 9. far front leg lower part (elbow to wrist); 10. near front paw; 11. far front paw; 12. near hind leg upper part (hip to knee); 13. far hind leg upper part (hip to knee); 14. near hind leg lower part (knee to ankle); 15. far hind leg lower part (knee to ankle); 16. near hind paw; 17. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_boar_piglet.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Ven suối giữa dương xỉ rừng mưa, thế ương ngạnh.
```
Wide 16:9 illustration. A wild boar piglet squealing defiantly with bristles raised and front legs planted at the edge of a rainforest stream among ferns. Attitude: bristles raised along the back, head lowered, stubborn and fearless. Composition: the wild piglet is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: young wild pig, stocky round body, light brown coat with pale horizontal stripes along the body, short snout, tiny tusks just showing. not a domestic pink pig. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of deep emerald green, from dark emerald in the foreground to pale jade mist at the back; the wild piglet keeps its own natural colors and stands out bright against it. Place: giant trunks with buttress roots, hanging lianas, broad leaves and ferns, layered canopy, shafts of light. Not savanna, no open grassland, no golden dry grass, no acacia trees, no snow, no desert. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): `idle`; `run`; `stagger`; `panic`.

### 3.7 `hare` — Thỏ rừng (con mồi) — khung A (thú bốn chân)

**Vùng:** Savan (`savanna`, vùng 1 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Bật nhảy: chân sau nén, tai dựng.

- Thêm lớp và xương `ear`; chân sau dài. Không có nguồn lịch sử riêng; tranh chỉ minh họa lớp 1.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_hare_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one hare alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: wild hare with very long black-tipped ears, long powerful hind legs, brown-grey coat, large side eyes. not a domestic rabbit. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_hare_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 21 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the hare in the attached picture. Draw exactly 21 separate pieces of this same hare, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw, (20) near long ear, (21) far long ear. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw; 20. near long ear; 21. far long ear.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_hare.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Đồi savan lộng gió, cú bật ngang.
```
Wide 16:9 illustration. A wild hare in an explosive sideways leap across a windy savanna hilltop, ears streaming back, fast clouds above. Attitude: eyes wide, hind legs coiled for an explosive jump. Composition: the hare is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: wild hare with very long black-tipped ears, long powerful hind legs, brown-grey coat, large side eyes. not a domestic rabbit. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon; the hare keeps its own natural colors and stands out bright against it. Place: flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills. No rainforest, no dense jungle, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): `idle`; `run`; `stagger`; `panic` (nhảy ngang).

### 3.8 `eagle` — Đại bàng lớn (chuyện đứa trẻ Taung, MVP-2) — khung D (chim)

**Vùng:** Savan (`savanna`, vùng 1 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Bổ nhào: mào dựng, mỏ há, móng xòe.

- Lấy dáng đại bàng mào châu Phi: mào lông trên đầu, ngực sọc, móng rất to.
- Không biết chính xác loài chim nào đã bắt đứa trẻ Taung, nên không ghi tên loài cụ thể trong game.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_eagle_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one eagle alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: wings spread wide in a glide, near wing in front of the body, far wing behind, talons tucked under the body, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: large African forest eagle like a crowned eagle: short broad rounded wings, long barred tail, double crest on the head, dark grey-brown back, pale chest with heavy dark bars, massive yellow legs and huge talons. not a bald eagle, no white head. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_eagle_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 8 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the eagle in the attached picture. Draw exactly 8 separate pieces of this same eagle, same colors, same outlines, same scale: (1) head with beak, (2) body in ONE piece, (3) near wing inner part (arm), (4) near wing outer part (long flight feathers), (5) far wing in ONE piece, (6) tail fan, (7) near leg with talons, (8) far leg with talons. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head with beak; 2. body in ONE piece; 3. near wing inner part (arm); 4. near wing outer part (long flight feathers); 5. far wing in ONE piece, one shade darker; 6. tail fan; 7. near leg with talons; 8. far leg with talons, one shade darker.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_taung_eagle.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Từ trời lao xuống mỏm đá savan, người vượn chỉ là bóng nhỏ.
```
Wide 16:9 illustration. A huge eagle diving out of the sky with wings half folded and enormous talons thrust forward, screaming, above a rocky limestone kopje on the savanna, far below a tiny group of early hominins as small silhouettes near a lone acacia. Attitude: crest raised, hooked beak open in a scream, piercing yellow eyes, huge talons spread wide. Composition: the eagle is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: large African forest eagle like a crowned eagle: short broad rounded wings, long barred tail, double crest on the head, dark grey-brown back, pale chest with heavy dark bars, massive yellow legs and huge talons. not a bald eagle, no white head. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon; the eagle keeps its own natural colors and stands out bright against it. Place: flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills. No rainforest, no dense jungle, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): `glide`; `flap`; `stalk` (lượn vòng, đầu nhìn xuống); `attack` (bổ nhào, cụp cánh, móng đưa ra trước); `grab_drag` (vỗ cánh mạnh bay lên, móng quặp); `flee`.

---

## 4. Sinh vật cho bản đầy đủ

Cấu trúc giống §3. Hoạt ảnh chỉ là dự kiến.
Mỗi tranh hồ sơ phải khác các tranh khác ở ít nhất hai điểm: nơi chốn, thời điểm hoặc thời tiết, góc nhìn, số con hoặc hành động (dòng "Điểm riêng").

### 4.1 `machairodus` — Hổ răng kiếm khổng lồ (*Machairodus kabir*) — khung A (thú bốn chân)

**Vùng:** Savan ven hồ (`lakeshore`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Bá chủ hồ nước: to hơn hổ, gầm trầm.

Giai đoạn 1.
Phải đúng: To hơn hổ; nanh dài, dẹt, gắn vào mảnh `head`.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_machairodus_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one sabertooth cat alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Machairodus kabir: bigger than the largest tiger, long legs, long flat serrated saber canines, relatively short tail, plain tawny coat. NOT a modern tiger: no stripes; NOT Smilodon. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_machairodus_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 19 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the sabertooth cat in the attached picture. Draw exactly 19 separate pieces of this same sabertooth cat, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_machairodus.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Bờ hồ Chad có sương sớm, thú khác ở xa.
```
Wide 16:9 illustration. A giant Machairodus sabertooth striding along a misty lakeshore at early morning in the Late Miocene Chad basin with its head low and sabers bared, long-necked giraffe-like animals and small three-toed horses far away near the water. Attitude: tiger-sized power, sabers bared, deep growl, heavy shoulders rolling forward. Composition: the sabertooth cat is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Machairodus kabir: bigger than the largest tiger, long legs, long flat serrated saber canines, relatively short tail, plain tawny coat. NOT a modern tiger: no stripes; NOT Smilodon. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of muted teal and olive, from dark olive in the foreground to pale silver-teal water and sky; the sabertooth cat keeps its own natural colors and stands out bright against it. Place: reed beds, still lake water with reflections, gallery trees along the shore, volcanic hills. No rainforest, no snow, no desert dunes. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, run, attack, stagger, enrage.

### 4.2 `dinofelis` — Báo răng kiếm giả (*Dinofelis*) — khung A (thú bốn chân)

**Vùng:** Savan (`savanna`, vùng 1 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Kẻ rình đêm: trườn thấp, mắt sáng.

Giai đoạn 2.
Phải đúng: Nanh **ngắn** hơn răng kiếm thật.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_dinofelis_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one false sabertooth cat alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Dinofelis: jaguar-like build with robust forelimbs, short flattened canines only slightly longer than a leopard's, faint spots on a tawny coat. NOT a leopard, NOT a true sabertooth with long sabers. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_dinofelis_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 19 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the false sabertooth cat in the attached picture. Draw exactly 19 separate pieces of this same false sabertooth cat, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_dinofelis.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Ban đêm, trăng tròn, cửa hang trên đồi savan.
```
Wide 16:9 illustration. A Dinofelis slinking out of a dark limestone cave mouth in a rocky savanna hill at night under a full moon, head low, eyes catching the moonlight, fangs bared. Attitude: slinking low, eyes glowing, short fangs bared. Composition: the false sabertooth cat is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Dinofelis: jaguar-like build with robust forelimbs, short flattened canines only slightly longer than a leopard's, faint spots on a tawny coat. NOT a leopard, NOT a true sabertooth with long sabers. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon; the false sabertooth cat keeps its own natural colors and stands out bright against it. Place: flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills. No rainforest, no dense jungle, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, stalk, attack, stagger, flee.

### 4.3 `deinotherium` — Khủng tượng ngà cong (*Deinotherium*) — khung E (voi)

**Vùng:** Savan (`savanna`, vùng 1 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Khổng lồ: ngẩng đầu, ngà hàm dưới vung xuống.

Giai đoạn 2.
Phải đúng: Ngà mọc ở **hàm dưới**, cong xuống và ra sau; cao ~4 m. Khung E nhưng thêm mảnh `jaw` mang ngà (không có mảnh `tusks` riêng).

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_deinotherium_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one Deinotherium alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and staggered, trunk hanging down, tusks fully visible, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: elephant relative, much taller and longer-legged than an elephant, small head for the body, short trunk, NO upper tusks, one pair of tusks growing from the LOWER jaw and curving down and backward like hooks. NOT a modern elephant. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_deinotherium_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 16 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the Deinotherium in the attached picture. Draw exactly 16 separate pieces of this same Deinotherium, same colors, same outlines, same scale: (1) head (skull without trunk, (2) trunk piece 1, (3) trunk piece 2, (4) trunk piece 3, (5) ear, (6) torso in ONE piece, (7) tail, (8) near front leg upper part, (9) near hind leg upper part, (10) far front leg upper part, (11) far hind leg upper part, (12) near front leg lower part with foot, (13) near hind leg lower part with foot, (14) far front leg lower part with foot, (15) far hind leg lower part with foot, (16) lower jaw carrying both downward-curving tusks. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull without trunk, without tusks); 2. trunk piece 1, the base; 3. trunk piece 2; 4. trunk piece 3, the tip; 5. ear; 6. torso in ONE piece; 7. tail; 8. near front leg upper part; 9. near hind leg upper part; 10. far front leg upper part; 11. far hind leg upper part; 12. near front leg lower part with foot; 13. near hind leg lower part with foot; 14. far front leg lower part with foot; 15. far hind leg lower part with foot; 16. lower jaw carrying both downward-curving tusks.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_deinotherium.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Góc thấp nhìn lên, rừng thưa savan ban trưa.
```
Wide 16:9 illustration. A towering Deinotherium rearing its head and trumpeting among savanna woodland trees in midday heat haze, downward-curving tusks swinging, one huge foot raised, seen from a low angle so it dwarfs the trees. Attitude: towering, head raised, downward tusks swinging, trumpeting. Composition: the Deinotherium is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: elephant relative, much taller and longer-legged than an elephant, small head for the body, short trunk, NO upper tusks, one pair of tusks growing from the LOWER jaw and curving down and backward like hooks. NOT a modern elephant. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon; the Deinotherium keeps its own natural colors and stands out bright against it. Place: flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills. No rainforest, no dense jungle, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, charge, stagger.

### 4.4 `crocodile_horned` — Cá sấu sừng (*Crocodylus anthropophagus*) — khung C (bò sát thân dài)

**Vùng:** Savan ven hồ (`lakeshore`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Kẻ phục kích dưới nước: há hàm, sừng trên mắt.

Giai đoạn 3.
Phải đúng: Gờ "sừng" trên mắt; dài ~5 m.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_crocodile_horned_rigpose.png` · `aspect_ratio: "16:9"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one horned crocodile alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: low stance, all four legs visible and not overlapping, tail lying straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Crocodylus anthropophagus: heavy crocodile body with prominent horn-like bony bosses rising at the back of the skull above the eyes, deep strong snout, armored back. NOT a modern Nile crocodile without horns. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_crocodile_horned_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 16 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the horned crocodile in the attached picture. Draw exactly 16 separate pieces of this same horned crocodile, same colors, same outlines, same scale: (1) head (skull with upper jaw), (2) lower jaw, (3) neck, (4) torso in ONE piece, (5) tail piece 1, (6) tail piece 2, (7) tail piece 3, (8) tail piece 4, (9) near front leg upper part, (10) near hind leg upper part, (11) far front leg upper part, (12) far hind leg upper part, (13) near front leg lower part with foot and claws, (14) near hind leg lower part with foot and claws, (15) far front leg lower part with foot and claws, (16) far hind leg lower part with foot and claws. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw); 2. lower jaw; 3. neck; 4. torso in ONE piece; 5. tail piece 1; 6. tail piece 2; 7. tail piece 3; 8. tail piece 4, the tip; 9. near front leg upper part; 10. near hind leg upper part; 11. far front leg upper part; 12. far hind leg upper part; 13. near front leg lower part with foot and claws; 14. near hind leg lower part with foot and claws; 15. far front leg lower part with foot and claws; 16. far hind leg lower part with foot and claws.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_crocodile_horned.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Trời giông, lao ra khỏi hồ Olduvai.
```
Wide 16:9 illustration. A horned crocodile lunging out of muddy lake water among reeds with jaws gaping wide, water splashing off its armored head, under a stormy sky over the lakes of Olduvai. Attitude: jaws gaping, horn ridges above the eyes, armored body tense. Composition: the horned crocodile is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Crocodylus anthropophagus: heavy crocodile body with prominent horn-like bony bosses rising at the back of the skull above the eyes, deep strong snout, armored back. NOT a modern Nile crocodile without horns. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of muted teal and olive, from dark olive in the foreground to pale silver-teal water and sky; the horned crocodile keeps its own natural colors and stands out bright against it. Place: reed beds, still lake water with reflections, gallery trees along the shore, volcanic hills. No rainforest, no snow, no desert dunes. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, crawl, stalk, attack, grab_drag, stagger, flee.

### 4.5 `pelorovis` — Trâu sừng cong (*Pelorovis oldowayensis*) — khung A (thú bốn chân)

**Vùng:** Savan ven hồ (`lakeshore`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Húc càn: cúi đầu, cặp sừng khổng lồ chĩa tới.

Giai đoạn 3.
Phải đúng: Sừng cong nửa vòng tròn, rất dài; gắn vào mảnh `head`.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_pelorovis_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one giant buffalo alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Pelorovis: long-legged wild cattle with enormous horns spreading sideways from the head and curving forward and up in a wide half circle, dark brown coat. NOT a modern cape buffalo, NOT a water buffalo. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_pelorovis_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 19 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the giant buffalo in the attached picture. Draw exactly 19 separate pieces of this same giant buffalo, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_pelorovis.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Mùa mưa ven hồ, bình minh, đang lao tới.
```
Wide 16:9 illustration. A Pelorovis bull charging head-down across a green rainy-season lakeside plain at sunrise with its enormous half-circle horns aimed forward, dust and spray exploding behind it, the herd behind. Attitude: snorting, enormous horns aimed forward. Composition: the giant buffalo is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Pelorovis: long-legged wild cattle with enormous horns spreading sideways from the head and curving forward and up in a wide half circle, dark brown coat. NOT a modern cape buffalo, NOT a water buffalo. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of muted teal and olive, from dark olive in the foreground to pale silver-teal water and sky; the giant buffalo keeps its own natural colors and stands out bright against it. Place: reed beds, still lake water with reflections, gallery trees along the shore, volcanic hills. No rainforest, no snow, no desert dunes. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, charge, stagger, panic.

### 4.6 `hippo_gorgops` — Hà mã khổng lồ (*Hippopotamus gorgops*) — khung A (thú bốn chân)

**Vùng:** Savan ven hồ (`lakeshore`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Đe dọa: há miệng khoe nanh.

Giai đoạn 3.
Phải đúng: Mắt và mũi nằm cao trên sọ.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_hippo_gorgops_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one giant hippo alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Hippopotamus gorgops: larger than any modern hippo, eye sockets raised high on top of the skull like periscopes, nostrils high on the snout, massive barrel body, short legs. NOT a modern hippo. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_hippo_gorgops_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 17 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the giant hippo in the attached picture. Draw exactly 17 separate pieces of this same giant hippo, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) near front leg upper part (shoulder to elbow), (7) far front leg upper part (shoulder to elbow), (8) near front leg lower part (elbow to wrist), (9) far front leg lower part (elbow to wrist), (10) near front paw, (11) far front paw, (12) near hind leg upper part (hip to knee), (13) far hind leg upper part (hip to knee), (14) near hind leg lower part (knee to ankle), (15) far hind leg lower part (knee to ankle), (16) near hind paw, (17) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. near front leg upper part (shoulder to elbow); 7. far front leg upper part (shoulder to elbow); 8. near front leg lower part (elbow to wrist); 9. far front leg lower part (elbow to wrist); 10. near front paw; 11. far front paw; 12. near hind leg upper part (hip to knee); 13. far hind leg upper part (hip to knee); 14. near hind leg lower part (knee to ankle); 15. far hind leg lower part (knee to ankle); 16. near hind paw; 17. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_hippo_gorgops.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Chiều tối, trồi lên khỏi hồ, cò bay tán loạn.
```
Wide 16:9 illustration. A giant hippo surging out of a shallow reedy lake in a threat yawn at evening, mouth gaping to show huge tusks, water streaming off its body, white egrets scattering. Attitude: mouth gaping in a threat yawn showing huge tusks. Composition: the giant hippo is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Hippopotamus gorgops: larger than any modern hippo, eye sockets raised high on top of the skull like periscopes, nostrils high on the snout, massive barrel body, short legs. NOT a modern hippo. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of muted teal and olive, from dark olive in the foreground to pale silver-teal water and sky; the giant hippo keeps its own natural colors and stands out bright against it. Place: reed beds, still lake water with reflections, gallery trees along the shore, volcanic hills. No rainforest, no snow, no desert dunes. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, charge, stagger.

### 4.7 `palaeoloxodon_recki` — Voi ngà thẳng (*Palaeoloxodon recki*) — khung E (voi)

**Vùng:** Savan ven hồ (`lakeshore`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Uy nghi: tai xòe, vòi giơ cao.

Giai đoạn 4.
Phải đúng: Ngà gần thẳng.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_palaeoloxodon_recki_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one straight-tusked elephant alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and staggered, trunk hanging down, tusks fully visible, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Palaeoloxodon recki: very tall elephant with a high domed head and a pronounced forehead ridge, long nearly straight tusks, long legs. NOT a modern African elephant. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_palaeoloxodon_recki_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 16 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the straight-tusked elephant in the attached picture. Draw exactly 16 separate pieces of this same straight-tusked elephant, same colors, same outlines, same scale: (1) head (skull without trunk, (2) trunk piece 1, (3) trunk piece 2, (4) trunk piece 3, (5) tusks as ONE piece, (6) ear, (7) torso in ONE piece, (8) tail, (9) near front leg upper part, (10) near hind leg upper part, (11) far front leg upper part, (12) far hind leg upper part, (13) near front leg lower part with foot, (14) near hind leg lower part with foot, (15) far front leg lower part with foot, (16) far hind leg lower part with foot. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull without trunk, without tusks); 2. trunk piece 1, the base; 3. trunk piece 2; 4. trunk piece 3, the tip; 5. tusks as ONE piece; 6. ear; 7. torso in ONE piece; 8. tail; 9. near front leg upper part; 10. near hind leg upper part; 11. far front leg upper part; 12. far hind leg upper part; 13. near front leg lower part with foot; 14. near hind leg lower part with foot; 15. far front leg lower part with foot; 16. far hind leg lower part with foot.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_palaeoloxodon_recki.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Bãi bùn khô nứt mùa khô ven hồ, một con đơn độc.
```
Wide 16:9 illustration. A lone straight-tusked elephant trumpeting with its trunk raised and ears flared on cracked dry mudflats beside a shrinking lake in the dry season, dust swirling. Attitude: ears flared, trunk raised, long straight tusks forward. Composition: the straight-tusked elephant is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Palaeoloxodon recki: very tall elephant with a high domed head and a pronounced forehead ridge, long nearly straight tusks, long legs. NOT a modern African elephant. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of muted teal and olive, from dark olive in the foreground to pale silver-teal water and sky; the straight-tusked elephant keeps its own natural colors and stands out bright against it. Place: reed beds, still lake water with reflections, gallery trees along the shore, volcanic hills. No rainforest, no snow, no desert dunes. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, charge, stagger.

### 4.8 `theropithecus_oswaldi` — Khỉ đầu chó khổng lồ (*Theropithecus oswaldi*) — khung A (thú bốn chân)

**Vùng:** Savan (`savanna`, vùng 1 (MVP)). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Đầu đàn: lật môi khoe nanh, bờm dựng.

Giai đoạn 4.
Phải đúng: Lớn, dáng giống khỉ gelada; đi bốn chân nên dùng khung A.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_theropithecus_oswaldi_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one giant baboon alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Theropithecus oswaldi: gelada-like giant monkey much bigger than any modern baboon, long heavy mane over the shoulders, bare hourglass-shaped chest patch, deep jaw, long face with upturned nostrils. NOT a modern baboon or mandrill, no colorful face. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_theropithecus_oswaldi_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 19 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the giant baboon in the attached picture. Draw exactly 19 separate pieces of this same giant baboon, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_theropithecus_oswaldi.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Sườn cao nguyên sương mù phía trên savan.
```
Wide 16:9 illustration. A giant male baboon on a rock baring huge canines in a lip-flip threat, mane flared, the troop behind him on a foggy grassy highland slope above the savanna. Attitude: lips flipped up to bare huge canines, mane bristling. Composition: the giant baboon is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Theropithecus oswaldi: gelada-like giant monkey much bigger than any modern baboon, long heavy mane over the shoulders, bare hourglass-shaped chest patch, deep jaw, long face with upturned nostrils. NOT a modern baboon or mandrill, no colorful face. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon; the giant baboon keeps its own natural colors and stands out bright against it. Place: flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills. No rainforest, no dense jungle, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, run, attack, stagger, flee.

### 4.9 `pachycrocuta` — Linh cẩu khổng lồ (*Pachycrocuta brevirostris*) — khung A (thú bốn chân)

**Vùng:** Âu-Á ôn đới (`eurasia_temperate`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Kẻ nghiền xương: hàm khổng lồ, lưng dốc dựng lông.

Giai đoạn 4.
Phải đúng: To, nặng, mặt ngắn. Trong tranh **không** có xương người.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_pachycrocuta_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one giant hyena alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Pachycrocuta: hyena the size of a lion or bigger, short broad muzzle, massive bone-crushing jaws, short sturdy legs, strongly sloping back, coarse spotted coat. NOT a modern spotted hyena: much heavier, shorter face. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_pachycrocuta_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 19 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the giant hyena in the attached picture. Draw exactly 19 separate pieces of this same giant hyena, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_pachycrocuta.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Rừng bạch dương tuyết, ngậm xương thú.
```
Wide 16:9 illustration. A giant short-faced hyena striding out of a snowy birch forest in northern China with a huge animal leg bone clamped in its jaws, hackles raised, breath steaming. Attitude: massive jaws open, sloping back bristling, menacing heavy head. Composition: the giant hyena is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Pachycrocuta: hyena the size of a lion or bigger, short broad muzzle, massive bone-crushing jaws, short sturdy legs, strongly sloping back, coarse spotted coat. NOT a modern spotted hyena: much heavier, shorter face. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of cool slate blue with rust-orange accents, from dark slate in the foreground to pale grey-blue sky; the giant hyena keeps its own natural colors and stands out bright against it. Place: birch and pine trunks, limestone hills with cave mouths, cold lakes with reeds. No savanna, no acacia trees, no tropical plants, no palms. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, run, attack, stagger, flee.

### 4.10 `gigantopithecus` — Vượn khổng lồ (*Gigantopithecus blacki*) — khung F (vượn lớn)

**Vùng:** Đông Nam Á núi đá vôi (`southeast_asia`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Người khổng lồ của rừng đá vôi: to lớn, uy hiếp.

Giai đoạn 4.
Phải đúng: Màu lông là **phỏng đoán**. Cảnh núi đá vôi kiểu miền Bắc Việt Nam và Quảng Tây.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_gigantopithecus_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one giant ape alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing on all fours, long arms and legs visible and staggered, head level, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Gigantopithecus: only teeth and jaws are known, so this is a reconstruction: a giant orangutan-like ape about three meters tall, very heavy body, long arms, thick reddish-brown fur, large heavy-browed face. NOT King Kong, NOT a gorilla, no human-like upright posture. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_gigantopithecus_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 15 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the giant ape in the attached picture. Draw exactly 15 separate pieces of this same giant ape, same colors, same outlines, same scale: (1) head (skull with upper jaw), (2) lower jaw, (3) torso in ONE piece, (4) near arm upper part, (5) far arm upper part, (6) near forearm, (7) far forearm, (8) near hand, (9) far hand, (10) near thigh, (11) far thigh, (12) near shin, (13) far shin, (14) near foot, (15) far foot. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw); 2. lower jaw; 3. torso in ONE piece; 4. near arm upper part; 5. far arm upper part; 6. near forearm; 7. far forearm; 8. near hand; 9. far hand; 10. near thigh; 11. far thigh; 12. near shin; 13. far shin; 14. near foot; 15. far foot.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_gigantopithecus.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Núi đá vôi, mưa phùn, đứng dậy gầm (dáng là tưởng tượng).
```
Wide 16:9 illustration. A colossal ape rising up on its hind legs among tall bamboo in a misty karst valley, arms spread wide, mouth open in a roar, steep limestone towers and drizzle behind. Attitude: colossal, chest puffed out, mouth open showing large teeth. Composition: the giant ape is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Gigantopithecus: only teeth and jaws are known, so this is a reconstruction: a giant orangutan-like ape about three meters tall, very heavy body, long arms, thick reddish-brown fur, large heavy-browed face. NOT King Kong, NOT a gorilla, no human-like upright posture. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of misty jade green, from dark jade in the foreground to pale white-green mist; the giant ape keeps its own natural colors and stands out bright against it. Place: tall steep karst limestone towers, bamboo clumps, bands of drifting cloud, a winding river. No savanna, no acacia trees, no snow, no desert. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, eat, stagger.

### 4.11 `homotherium` — Hổ răng mã tấu (*Homotherium latidens*) — khung A (thú bốn chân)

**Vùng:** Âu-Á ôn đới (`eurasia_temperate`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Kẻ săn bền bỉ: lưng dốc, sải chân dài.

Giai đoạn 4.
Phải đúng: Lưng dốc (chân trước dài hơn chân sau); nanh ngắn hơn *Smilodon*, gắn vào `head`.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_homotherium_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one scimitar-toothed cat alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Homotherium: long front legs and shorter hind legs giving a sloping back like a hyena, short broad serrated scimitar canines that only just show below the lips, short bobbed tail, lean body, pale coat. NOT Smilodon, NOT a lion, no mane. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_homotherium_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 18 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the scimitar-toothed cat in the attached picture. Draw exactly 18 separate pieces of this same scimitar-toothed cat, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail tip, (7) near front leg upper part (shoulder to elbow), (8) far front leg upper part (shoulder to elbow), (9) near front leg lower part (elbow to wrist), (10) far front leg lower part (elbow to wrist), (11) near front paw, (12) far front paw, (13) near hind leg upper part (hip to knee), (14) far hind leg upper part (hip to knee), (15) near hind leg lower part (knee to ankle), (16) far hind leg lower part (knee to ankle), (17) near hind paw, (18) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail tip; 7. near front leg upper part (shoulder to elbow); 8. far front leg upper part (shoulder to elbow); 9. near front leg lower part (elbow to wrist); 10. far front leg lower part (elbow to wrist); 11. near front paw; 12. far front paw; 13. near hind leg upper part (hip to knee); 14. far hind leg upper part (hip to knee); 15. near hind leg lower part (knee to ankle); 16. far hind leg lower part (knee to ankle); 17. near hind paw; 18. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_homotherium.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Lau sậy mùa thu, giáo gỗ bỏ lại (Schöningen).
```
Wide 16:9 illustration. A Homotherium prowling through tall golden autumn reeds on the shore of a cold shallow lake in Ice Age Germany, head low, serrated fangs bared, stepping past a wooden spear lying in the reeds. Attitude: long front legs striding, sloping back, serrated fangs bared. Composition: the scimitar-toothed cat is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Homotherium: long front legs and shorter hind legs giving a sloping back like a hyena, short broad serrated scimitar canines that only just show below the lips, short bobbed tail, lean body, pale coat. NOT Smilodon, NOT a lion, no mane. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of cool slate blue with rust-orange accents, from dark slate in the foreground to pale grey-blue sky; the scimitar-toothed cat keeps its own natural colors and stands out bright against it. Place: birch and pine trunks, limestone hills with cave mouths, cold lakes with reeds. No savanna, no acacia trees, no tropical plants, no palms. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, run, attack, stagger, enrage.

### 4.12 `leptoptilos_robustus` — Cò già khổng lồ (*Leptoptilos robustus*) — khung D (chim)

**Vùng:** Đảo Flores (`flores`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Kẻ tranh xác: xòe cánh, há mỏ.

Giai đoạn 4.
Phải đúng: Cao ~1,8 m; **bay được**. Khung D, nhưng hình gốc là dáng **đứng** cánh khép, chân dài 2 đoạn.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_leptoptilos_robustus_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one giant stork alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still on two long legs slightly apart, wings folded, bill pointing forward, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Leptoptilos robustus: marabou-like stork about 1.8 m tall, bald pinkish head and neck, huge heavy wedge-shaped bill, long thick legs, dark wings and pale body. NOT a modern marabou: much taller and heavier. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_leptoptilos_robustus_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 10 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the giant stork in the attached picture. Draw exactly 10 separate pieces of this same giant stork, same colors, same outlines, same scale: (1) head with beak, (2) body in ONE piece, (3) near wing inner part (arm), (4) near wing outer part (long flight feathers), (5) far wing in ONE piece, (6) tail fan, (7) near long leg upper part, (8) near long leg lower part with foot, (9) far long leg upper part, (10) far long leg lower part with foot. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head with beak; 2. body in ONE piece; 3. near wing inner part (arm); 4. near wing outer part (long flight feathers); 5. far wing in ONE piece, one shade darker; 6. tail fan; 7. near long leg upper part; 8. near long leg lower part with foot; 9. far long leg upper part, one shade darker; 10. far long leg lower part with foot, one shade darker.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_leptoptilos_robustus.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Nhìn từ trong hang ra, ngược sáng (Liang Bua).
```
Wide 16:9 illustration. A giant marabou stork with wings flared wide and huge bill open, standing guard on a rock ledge in the mouth of a great limestone cave on Flores, dark against the bright monsoon forest outside. Attitude: wings half spread, huge bill open, menacing glare. Composition: the giant stork is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Leptoptilos robustus: marabou-like stork about 1.8 m tall, bald pinkish head and neck, huge heavy wedge-shaped bill, long thick legs, dark wings and pale body. NOT a modern marabou: much taller and heavier. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm sand gold with turquoise sea accents, from dark ochre in the foreground to pale turquoise sky; the giant stork keeps its own natural colors and stands out bright against it. Place: lontar palms, a large limestone cave mouth, dry island hills, the sea on the horizon. No african savanna animals, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, peck, flap, stagger.

### 4.13 `komodo` — Rồng Komodo (*Varanus komodoensis*) — khung C (bò sát thân dài)

**Vùng:** Đảo Flores (`flores`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Kiên nhẫn và chết người: lưỡi chẻ thè ra.

Giai đoạn 4.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_komodo_rigpose.png` · `aspect_ratio: "16:9"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one Komodo dragon alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: low stance, all four legs visible and not overlapping, tail lying straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Komodo dragon: heavy low body, thick muscular tail, long forked yellow tongue, grey-brown beaded scales, strong curved claws. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_komodo_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 16 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the Komodo dragon in the attached picture. Draw exactly 16 separate pieces of this same Komodo dragon, same colors, same outlines, same scale: (1) head (skull with upper jaw), (2) lower jaw, (3) neck, (4) torso in ONE piece, (5) tail piece 1, (6) tail piece 2, (7) tail piece 3, (8) tail piece 4, (9) near front leg upper part, (10) near hind leg upper part, (11) far front leg upper part, (12) far hind leg upper part, (13) near front leg lower part with foot and claws, (14) near hind leg lower part with foot and claws, (15) far front leg lower part with foot and claws, (16) far hind leg lower part with foot and claws. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw); 2. lower jaw; 3. neck; 4. torso in ONE piece; 5. tail piece 1; 6. tail piece 2; 7. tail piece 3; 8. tail piece 4, the tip; 9. near front leg upper part; 10. near hind leg upper part; 11. far front leg upper part; 12. far hind leg upper part; 13. near front leg lower part with foot and claws; 14. near hind leg lower part with foot and claws; 15. far front leg lower part with foot and claws; 16. far hind leg lower part with foot and claws.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_komodo.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Góc sát đất, bò thẳng về phía người xem.
```
Wide 16:9 illustration. A Komodo dragon crawling straight toward the viewer at ground level through dry gold island grassland with scattered lontar palms, forked tongue flicking, heavy claws gripping dry earth. Attitude: forked tongue out, head low, powerful and patient. Composition: the Komodo dragon is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Komodo dragon: heavy low body, thick muscular tail, long forked yellow tongue, grey-brown beaded scales, strong curved claws. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm sand gold with turquoise sea accents, from dark ochre in the foreground to pale turquoise sky; the Komodo dragon keeps its own natural colors and stands out bright against it. Place: lontar palms, a large limestone cave mouth, dry island hills, the sea on the horizon. No african savanna animals, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, crawl, attack, stagger, flee.

### 4.14 `stegodon_dwarf` — Voi lùn Stegodon (*Stegodon florensis insularis*) — khung E (voi)

**Vùng:** Đảo Flores (`flores`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Mẹ bảo vệ con: tai xòe, vòi giơ.

Giai đoạn 4.
Phải đúng: Hai ngà sát nhau, gần chạm.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_stegodon_dwarf_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one dwarf Stegodon alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and staggered, trunk hanging down, tusks fully visible, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: dwarf Stegodon florensis insularis: small elephant relative, low flat skull, long tusks set very close together and nearly touching so the trunk hangs beside them, short trunk. NOT a modern elephant. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_stegodon_dwarf_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 16 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the dwarf Stegodon in the attached picture. Draw exactly 16 separate pieces of this same dwarf Stegodon, same colors, same outlines, same scale: (1) head (skull without trunk, (2) trunk piece 1, (3) trunk piece 2, (4) trunk piece 3, (5) tusks as ONE piece, (6) ear, (7) torso in ONE piece, (8) tail, (9) near front leg upper part, (10) near hind leg upper part, (11) far front leg upper part, (12) far hind leg upper part, (13) near front leg lower part with foot, (14) near hind leg lower part with foot, (15) far front leg lower part with foot, (16) far hind leg lower part with foot. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull without trunk, without tusks); 2. trunk piece 1, the base; 3. trunk piece 2; 4. trunk piece 3, the tip; 5. tusks as ONE piece; 6. ear; 7. torso in ONE piece; 8. tail; 9. near front leg upper part; 10. near hind leg upper part; 11. far front leg upper part; 12. far hind leg upper part; 13. near front leg lower part with foot; 14. near hind leg lower part with foot; 15. far front leg lower part with foot; 16. far hind leg lower part with foot.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_stegodon_dwarf.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Lội suối, mẹ che con.
```
Wide 16:9 illustration. A dwarf Stegodon mother trumpeting with ears flared, standing in front of her calf while wading through a clear shallow river in an island monsoon forest. Attitude: ears flared, trunk raised, protective. Composition: the dwarf Stegodon is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: dwarf Stegodon florensis insularis: small elephant relative, low flat skull, long tusks set very close together and nearly touching so the trunk hangs beside them, short trunk. NOT a modern elephant. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of warm sand gold with turquoise sea accents, from dark ochre in the foreground to pale turquoise sky; the dwarf Stegodon keeps its own natural colors and stands out bright against it. Place: lontar palms, a large limestone cave mouth, dry island hills, the sea on the horizon. No african savanna animals, no snow, no pine forest. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, stagger, panic.

### 4.15 `cave_lion` — Sư tử hang (*Panthera spelaea*) — khung A (thú bốn chân)

**Vùng:** Âu-Á băng hà (`ice_age_eurasia`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Vua băng giá: gầm vang.

Giai đoạn 5.
Phải đúng: Không có bờm lớn.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_cave_lion_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one cave lion alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Panthera spelaea: bigger than a modern lion, no mane or only a very short ruff, thick pale winter coat, faint stripes on the flanks. NOT a modern maned African lion. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_cave_lion_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 19 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the cave lion in the attached picture. Draw exactly 19 separate pieces of this same cave lion, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_cave_lion.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Sông băng lúc bình minh, một cặp.
```
Wide 16:9 illustration. A cave lion roaring on the bank of a frozen river at sunrise, its mate behind it, breath steaming in the cold. Attitude: roaring, maneless and powerful. Composition: the cave lion is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Panthera spelaea: bigger than a modern lion, no mane or only a very short ruff, thick pale winter coat, faint stripes on the flanks. NOT a modern maned African lion. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of icy blue lit by pale pink-gold low sunlight, from dark steel blue in the foreground to almost white sky; the cave lion keeps its own natural colors and stands out bright against it. Place: snowy steppe, frozen river, limestone cliffs and rock shelters, sparse wind-bent trees, falling snow. No savanna, no acacia trees, no tropical plants, no green jungle. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, run, attack, stagger, enrage.

### 4.16 `cave_hyena` — Linh cẩu hang (*Crocuta crocuta spelaea*) — khung A (thú bốn chân)

**Vùng:** Âu-Á băng hà (`ice_age_eurasia`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Giữ hang: chồm tới, há hàm.

Giai đoạn 5.
Phải đúng: To hơn linh cẩu đốm.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_cave_hyena_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one cave hyena alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: cave hyena: spotted hyena form but much larger and heavier, thick winter coat with dark spots, big head, sloping back. NOT the size of a modern spotted hyena. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_cave_hyena_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 19 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the cave hyena in the attached picture. Draw exactly 19 separate pieces of this same cave hyena, same colors, same outlines, same scale: (1) head (skull with upper jaw and ears), (2) lower jaw, (3) neck, (4) torso, (5) tail base, (6) tail middle, (7) tail tip, (8) near front leg upper part (shoulder to elbow), (9) far front leg upper part (shoulder to elbow), (10) near front leg lower part (elbow to wrist), (11) far front leg lower part (elbow to wrist), (12) near front paw, (13) far front paw, (14) near hind leg upper part (hip to knee), (15) far hind leg upper part (hip to knee), (16) near hind leg lower part (knee to ankle), (17) far hind leg lower part (knee to ankle), (18) near hind paw, (19) far hind paw. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw and ears); 2. lower jaw; 3. neck; 4. torso, chest to hips in ONE piece; 5. tail base; 6. tail middle; 7. tail tip; 8. near front leg upper part (shoulder to elbow); 9. far front leg upper part (shoulder to elbow); 10. near front leg lower part (elbow to wrist); 11. far front leg lower part (elbow to wrist); 12. near front paw; 13. far front paw; 14. near hind leg upper part (hip to knee); 15. far hind leg upper part (hip to knee); 16. near hind leg lower part (knee to ankle); 17. far hind leg lower part (knee to ankle); 18. near hind paw; 19. far hind paw.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_cave_hyena.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Từ trong mái đá nhìn ra (chuyện tranh hang).
```
Wide 16:9 illustration. Two cave hyenas defending the entrance of a rock shelter, one stepping forward with jaws open and hackles raised, seen from inside the shelter looking out at a snowy valley. Attitude: hunched, jaws open, hackles raised. Composition: the cave hyena is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: cave hyena: spotted hyena form but much larger and heavier, thick winter coat with dark spots, big head, sloping back. NOT the size of a modern spotted hyena. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of icy blue lit by pale pink-gold low sunlight, from dark steel blue in the foreground to almost white sky; the cave hyena keeps its own natural colors and stands out bright against it. Place: snowy steppe, frozen river, limestone cliffs and rock shelters, sparse wind-bent trees, falling snow. No savanna, no acacia trees, no tropical plants, no green jungle. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, run, attack, stagger, flee.

### 4.17 `mammoth` — Voi ma mút lông xoăn (*Mammuthus primigenius*) — khung E (voi)

**Vùng:** Âu-Á băng hà (`ice_age_eurasia`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Xé bão tuyết: vòi giơ, ngà chĩa tới.

Giai đoạn 5.
Phải đúng: Tai nhỏ, đầu gồ cao, lưng dốc.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_mammoth_rigpose.png` · `aspect_ratio: "1:1"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one woolly mammoth alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: standing still, all four legs visible and staggered, trunk hanging down, tusks fully visible, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Mammuthus primigenius: high domed head, small ears, strongly sloping back with a high shoulder hump, long shaggy brown hair over dense underfur, very long tusks curving out, up and inward in a spiral, short tail. NOT a modern elephant. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_mammoth_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 16 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the woolly mammoth in the attached picture. Draw exactly 16 separate pieces of this same woolly mammoth, same colors, same outlines, same scale: (1) head (skull without trunk, (2) trunk piece 1, (3) trunk piece 2, (4) trunk piece 3, (5) tusks as ONE piece, (6) ear, (7) torso in ONE piece, (8) tail, (9) near front leg upper part, (10) near hind leg upper part, (11) far front leg upper part, (12) far hind leg upper part, (13) near front leg lower part with foot, (14) near hind leg lower part with foot, (15) far front leg lower part with foot, (16) far hind leg lower part with foot. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull without trunk, without tusks); 2. trunk piece 1, the base; 3. trunk piece 2; 4. trunk piece 3, the tip; 5. tusks as ONE piece; 6. ear; 7. torso in ONE piece; 8. tail; 9. near front leg upper part; 10. near hind leg upper part; 11. far front leg upper part; 12. far hind leg upper part; 13. near front leg lower part with foot; 14. near hind leg lower part with foot; 15. far front leg lower part with foot; 16. far hind leg lower part with foot.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_mammoth.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Bão tuyết, con đực đầu đàn lao tới.
```
Wide 16:9 illustration. A woolly mammoth bull charging through a blizzard on the snowy steppe with its trunk raised and long curved tusks forward, snow flying off its shaggy fur, the herd fading into the distance. Attitude: trunk raised, long curved tusks forward, snow flying off its fur. Composition: the woolly mammoth is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Mammuthus primigenius: high domed head, small ears, strongly sloping back with a high shoulder hump, long shaggy brown hair over dense underfur, very long tusks curving out, up and inward in a spiral, short tail. NOT a modern elephant. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of icy blue lit by pale pink-gold low sunlight, from dark steel blue in the foreground to almost white sky; the woolly mammoth keeps its own natural colors and stands out bright against it. Place: snowy steppe, frozen river, limestone cliffs and rock shelters, sparse wind-bent trees, falling snow. No savanna, no acacia trees, no tropical plants, no green jungle. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, walk, charge, stagger.

### 4.18 `megalania` — Thằn lằn khổng lồ (*Varanus priscus*) — khung C (bò sát thân dài)

**Vùng:** Úc (`australia`, bản đầy đủ [ĐỀ XUẤT]). Nền hồ sơ theo vùng này (§2.8).

**Bản năng / dáng (chỉ cho hồ sơ):** Rồng của nước Úc: hàm há, lưỡi chẻ.

Giai đoạn 5.
Phải đúng: Dài ~6 m.

**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.

**1. Hình gốc** → `beast_megalania_rigpose.png` · `aspect_ratio: "16:9"` · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách
```
Game sprite: one giant monitor lizard alone, full body, seen exactly from the side (profile view), facing right. Neutral pose: low stance, all four legs visible and not overlapping, tail lying straight back, mouth closed. Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background. Anatomy: Varanus priscus (Megalania): monitor lizard about six meters long, far bulkier than a Komodo dragon, heavy limbs, deep powerful jaws, mottled brown beaded scales. NOT a Komodo dragon: far larger and heavier. Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients. Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text
```

**2. Phân mảnh từ hình gốc** → `beast_megalania_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng 16 mảnh**
```
Cut-out parts sheet for 2D skeletal animation of the giant monitor lizard in the attached picture. Draw exactly 16 separate pieces of this same giant monitor lizard, same colors, same outlines, same scale: (1) head (skull with upper jaw), (2) lower jaw, (3) neck, (4) torso in ONE piece, (5) tail piece 1, (6) tail piece 2, (7) tail piece 3, (8) tail piece 4, (9) near front leg upper part, (10) near hind leg upper part, (11) far front leg upper part, (12) far hind leg upper part, (13) near front leg lower part with foot and claws, (14) near hind leg lower part with foot and claws, (15) far front leg lower part with foot and claws, (16) far hind leg lower part with foot and claws. Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated. Spread the pieces apart in rows in the numbered order, side view, with wide gaps. No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text. Background: solid flat magenta #FF00FF only
```

Kiểm mảnh (đếm trước khi nhận): 1. head (skull with upper jaw); 2. lower jaw; 3. neck; 4. torso in ONE piece; 5. tail piece 1; 6. tail piece 2; 7. tail piece 3; 8. tail piece 4, the tip; 9. near front leg upper part; 10. near hind leg upper part; 11. far front leg upper part; 12. far hind leg upper part; 13. near front leg lower part with foot and claws; 14. near hind leg lower part with foot and claws; 15. far front leg lower part with foot and claws; 16. far hind leg lower part with foot and claws.

**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).

**4. Hồ sơ đầy đủ** → `codex_megalania.png` · `aspect_ratio: "16:9"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách
Điểm riêng của tranh này: Mỏm đá đỏ giữa bụi rậm Úc, ngẩng đầu.
```
Wide 16:9 illustration. A gigantic monitor lizard rearing its head up with jaws open and forked tongue out on top of a red rocky outcrop in the Australian bush at noon. Attitude: jaws open, forked tongue out, massive body. Composition: the giant monitor lizard is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text. Anatomy: Varanus priscus (Megalania): monitor lizard about six meters long, far bulkier than a Komodo dragon, heavy limbs, deep powerful jaws, mottled brown beaded scales. NOT a Komodo dragon: far larger and heavier. Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes. Background colors: only shades of red ochre, from dark rust in the foreground to pale hot orange sky; the giant monitor lizard keeps its own natural colors and stands out bright against it. Place: red rock outcrops, grey-green eucalyptus trees, spinifex grass clumps. No african savanna animals, no snow, no jungle. No text, no border, no frame, no blood, not cute, not a calm pose
```

**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): idle, crawl, attack, stagger, flee.

---

_§1.1, §2.4–§2.6, §2.8, §3 và §4 được sinh bằng `tools/art/gen_prompts.py`. Sửa script rồi chạy lại, dán đè các mục đó._

## 5. Các hạng mục khác (chưa viết)

Khi thêm hạng mục mới, giữ cấu trúc: **quy tắc riêng → khối dùng chung → prompt từng thứ → tên file**. Nhân vật có hoạt ảnh thì theo cách rig ở §2.

- **Biểu tượng trạng thái trong trận:** mắt Rình, sao Choáng, Bỏ chạy, giải cứu (`bestiary.md` §4).
- **Thành viên bộ tộc theo giai đoạn:** rig theo dáng người. Khung 96×128, đường chân y = 122 (`Assets/Art/Placeholder/README.md`). Cần khung xương người riêng.
- **Công trình và vật thể:** khung 96×96, đường chân y = 88 (`PLAN-bo-asset-21.md`).
- **Nền vùng:** rừng nhiệt đới, savan.
- **Màn Bảo tàng và thẻ thành tựu.**
- **Âm thanh và nhạc** (nếu dùng công cụ tạo sinh).

---

## Phụ lục — Quy ước tên file

| Loại | Tên file |
| --- | --- |
| Hình gốc tư thế rig | `beast_<id>_rigpose.png` |
| Bộ mảnh sinh ra | `beast_<id>_parts.png` |
| File nhiều lớp đã ghép | `beast_<id>.psb` |
| Mảnh đổi thêm (vòng quấn, biến thể đốm) | `beast_<id>_<tên>.png`, ví dụ `beast_python_coil.png` |
| Đoạn hoạt ảnh | `beast_<id>_<trạng thái>.anim` |
| Hồ sơ đầy đủ (hình 4) | `codex_<codex_id>.png` |
| Hình hồ sơ (hình 3) | không có file; game cắt từ `codex_<codex_id>.png` |
| Mặt nạ viền thẻ (dùng chung) | `Assets/UI/Codex/card_vignette.png` |

- `<id>` khớp `species[].id` trong `beasts.json`.
- `<codex_id>` khớp `codex.vi.json` (`bestiary.md` §4.6).
