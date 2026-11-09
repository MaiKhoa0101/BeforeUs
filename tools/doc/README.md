# tools/doc — tài liệu docx cho Before Us

Chuyển tài liệu thiết kế sang **.docx có mục lục** để Khoa sửa trong Word, mà **không**
làm mất khả năng đọc của agent.

Chỉ dùng thư viện chuẩn của Python. Không cài gì (mạng bị chặn). Cần Python 3.8 trở lên.

---

## Ba lệnh

### 1. Sinh docx từ Markdown

```
python tools/doc/docx_write.py docs/design.md docs/design.docx --sync docs/design.sync.json
```

Nếu tài liệu có liên kết Markdown, lệnh này in cảnh báo (chữ vẫn giữ nguyên — xem
"Giới hạn đã biết"). Thêm `--strict-links` để biến cảnh báo đó thành lỗi dừng hẳn.

Mở file trong Word: nếu được hỏi **"cập nhật mục lục?"** thì bấm **Có**. Mục lục do Word
tự dựng từ các tiêu đề, không phải do công cụ dựng sẵn — nên nó luôn đúng với nội dung
hiện tại, kể cả sau khi Khoa sửa và thêm mục.

### 2. Sinh lại bản chiếu Markdown từ docx

```
python tools/doc/docx_read.py docs/design.docx docs/design.md --sync docs/design.sync.json
```

Chạy lệnh này **mỗi khi sửa docx trong Word**. Nó cũng tự cập nhật băm trong file đồng bộ.

### 3. Kiểm băm trước khi tin bản chiếu

```
python tools/doc/docx_read.py --check docs/design.docx
```

`KHỚP` (mã thoát 0) thì bản `.md` tin được. `LỆCH` (mã thoát 2) thì **đừng đọc bản `.md`** —
chạy lệnh 2 trước.

---

## Mô hình: docx là bản chính, .md là bản chiếu

```
  docs/design.docx        ← BẢN CHÍNH. Khoa sửa ở đây, trong Word.
  docs/design.md          ← bản chiếu, SINH TỰ ĐỘNG. Đừng sửa tay.
  docs/design.sync.json   ← sha256 của docx lúc bản chiếu được sinh.
```

`AGENTS.md` xếp `design.md` là **nguồn sự thật số 1** và cả ba Lead đọc nó liên tục.
docx là file nhị phân: không `grep` được, và dự án không có git (`decisions.md` D4) nên
cũng không `diff` được để xác minh "đúng hai dòng đổi, không dòng nào khác". Bản chiếu
dạng chữ giữ lại cả hai khả năng đó.

## Quy trình cho agent

1. Chạy lệnh 3 (`--check`).
2. `KHỚP` → đọc `docs/design.md` như trước.
3. `LỆCH` → chạy lệnh 2 để sinh lại, **rồi mới** đọc.

**Đừng bỏ bước 1.** Nó không phải thủ tục cho đẹp. Một bản chiếu cũ trông y hệt một bản
chiếu mới — không có dấu hiệu nào trong chính file cho biết nó đã lỗi thời. Trong một
ngày làm việc đã có hai lần agent lập luận từ tài liệu lỗi thời (Lead UI dùng D1 khi D13
đã thay nó; Supervisor đánh số trùng D14). Lần này thứ lỗi thời sẽ là **luật chơi**, và
cái sai sẽ đi thẳng vào mã nguồn.

Mã thoát của `--check` là 0 hoặc 2, nên gắn vào script được:

```
python tools/doc/docx_read.py --check docs/design.docx || echo "PHẢI sinh lại bản chiếu"
```

---

## Kiểm công cụ

```
python tools/doc/test_roundtrip.py                 # 18 trường hợp dựng sẵn + docs/design.md
python tools/doc/test_roundtrip.py <file.md>       # kiểm một file bất kỳ
```

Bài kiểm làm **ba** phép so, theo thứ tự này, và hai phép đầu mới là phép quan trọng:

1. **Không rụng chữ.** Bỏ hết ký tự cú pháp Markdown khỏi cả hai bên rồi so từng ký tự.
2. **Không rụng dấu định dạng.** Đếm `**`, `` ` `` và `*` đơn trên chuỗi **thô**.
3. **Khớp cấu trúc.** So dạng chuẩn hoá: bỏ khác biệt về khoảng trắng và cách căn bảng.

Phép (3) chạy `normalise()` trên **cả hai** bên, nên một lỗi trong hàm dựng sẽ tự che
chính nó. Phép (1) và (2) không đi qua parser, nên chúng bắt được thứ (3) bỏ sót.

Đây không phải lo xa. Trong lúc làm, phép (2) đã bắt được **hai lỗi thật** mà (1) và (3)
đều để lọt:

- Hàm gộp dấu chạy tới dấu nghiêng `*` thì `'*' + '*'` chính là `'**'`, nên nó **xoá sạch
  64 dấu đậm** của `docs/design.md` — gồm mọi nhãn **[ĐỀ XUẤT]** và **[MVP]**. Phép (1)
  mù vì nó bóc dấu cú pháp; phép (3) mù vì cả hai bên chạy cùng hàm hỏng.
- Ô tiêu đề bảng bị ép đậm ở từng run, nên bên đọc phải bóc đậm khỏi mọi ô tiêu đề và
  làm mất chữ đậm thật trong ô `| Giai đoạn | Bản đầy đủ | **[MVP]** |`. Nay đậm trang
  trí nằm ở style đoạn `TableHeaderCell`, tách hẳn khỏi đậm do tác giả viết.

Kết quả trên `docs/design.md` hiện tại: 64 khối (22 tiêu đề, 14 bảng, 11 danh sách,
11 đoạn, 4 khối mã, 2 trích dẫn) — **10189 ký tự chữ khớp từng ký tự, 64/64 dấu đậm,
110/110 dấu mã, cấu trúc khớp**. 18 trường hợp dựng sẵn đều xanh.

---

## Những gì giữ được qua khứ hồi

| Markdown | docx | Về lại Markdown |
| --- | --- | --- |
| `#` … `######` | style Heading1–6, có `outlineLvl` | ✔ |
| Đoạn văn | Normal | ✔ |
| **Bảng** | `w:tbl`, viền, tiêu đề đậm có nền, lặp đầu trang | ✔ kể cả căn lề và ô rỗng |
| `- ` / `1. `, lồng 3 mức | `numPr`, numId 1 (chấm) và 2 (số) | ✔ |
| `**đậm**`, `*nghiêng*`, `` `mã` `` | `w:b`, `w:i`, style ký tự InlineCode | ✔ kể cả lồng nhau |
| ```` ``` ```` | style SourceCode, nền xám, Consolas | ✔ |
| ` ```python ` | thêm một run **ẩn** `⟦lang:python⟧` | ✔ |
| `> ` nhiều dòng liền | một đoạn Quote có ngắt dòng mềm | ✔ đúng số dòng |
| `---` | đoạn có đường kẻ dưới | ✔ |

`outlineLvl` trong style tiêu đề là thứ trường TOC của Word dùng để gom mục lục. Thiếu nó
thì mục lục dựng ra **rỗng** dù tiêu đề trông vẫn đúng cỡ chữ — một lỗi im lặng khó đoán.

---

## Giới hạn đã biết

- **Liên kết `[chữ](địa-chỉ)`**: chưa dựng thành siêu liên kết Word.

  **Chữ và định dạng KHÔNG bị mất** — kể cả `[**chữ đậm**](...)`. Ba trường hợp trong bài
  kiểm khoá điều này. Nhưng trong Word chúng hiện ra dạng cú pháp thô và không bấm được.

  `docx_write.py` **báo rõ** khi gặp liên kết: in cảnh báo ra stderr kèm số dòng và từng
  liên kết. Muốn chặn hẳn thì thêm `--strict-links` — tài liệu có liên kết thì dừng với
  mã thoát 3.

  > **Lưu ý cho Lead Core khi gộp v7.** `design.md` **hiện tại** không có liên kết nào —
  > tôi đếm trước khi cắt phạm vi, không phải bỏ sót. Nhưng bản gộp **có thể** sinh ra
  > liên kết (trỏ sang `decisions.md` chẳng hạn). Nếu cảnh báo nổ, đó không phải lỗi và
  > chữ không mất; chỉ là tài liệu sẽ trông thô ở chỗ đó. Cần siêu liên kết thật thì báo,
  > phần việc là thêm relationship cho mỗi liên kết — làm được, chỉ là chưa cần.
- **Ảnh**: không hỗ trợ.
- **Danh sách đánh số tự đếm lại** từ 1 trong mỗi khối. `1. 1. 1.` sẽ về thành `1. 2. 3.`.
- **Ngôn ngữ khối mã** cất trong chữ ẩn. Nếu Khoa bật "hiện chữ ẩn" trong Word và xoá nó
  thì khứ hồi mất dòng ` ```python `, không mất mã.
- **Lồng quá 3 mức** danh sách bị kẹp về mức 3.
- Công cụ **không gộp nội dung**. Nó chỉ chuyển định dạng.

## Tất định

Cùng một Markdown vào thì ra cùng một chuỗi byte (thời gian trong ZIP được ghim, thứ tự
phần cố định). Điều này bắt buộc: nếu không, băm trong file đồng bộ sẽ lệch sau mỗi lần
sinh lại và cảnh báo chống trôi thành tiếng ồn — rồi người ta sẽ bỏ qua nó.
