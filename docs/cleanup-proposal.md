# Đề xuất tái cấu trúc `Assets/` — BẢN ĐỀ XUẤT, CHƯA THỰC HIỆN

> Soạn 09/11/2026 bởi Lead UI theo yêu cầu "xem xét" của Khoa. **Chưa dời file nào.**
> Mọi mục dưới đây cần Khoa duyệt trước khi ai đó chạm vào cây thư mục.

---

## 1. Cấu trúc hiện tại

```
Assets/
  Art/Placeholder/       82 file   hình tạm do agent sinh (Lead Art sở hữu)
  Button/               744 file   pack icon bên thứ ba
  Data/                  12 file   balance · strings.vi · neurons · plants · sites · beasts
  Rock/                 377 file   pack pixel art đá, bên thứ ba
  Scenes/                10 file   Game.unity · SampleScene.unity · 3 ảnh nền rời
  Scripts/              265 file   Core · Core.Tests · Combat · Combat.Tests · Game · UI
  Sprite/                40 file   art Khoa tự vẽ
  TextMesh Pro/          92 file   tài nguyên TMP
  UI/                     6 file   MainScreen.uxml · .uss · MainPanelSettings.asset
  ~Prototype/             6 file   prototype cũ, Unity bỏ qua (D3)
```

Hai nhóm lẫn vào nhau ở cùng một cấp: **tài sản của dự án** (`Scripts`, `Data`, `UI`, `Sprite`,
`Art`) và **pack bên thứ ba** (`Button`, `Rock`, `TextMesh Pro`). Nhóm thứ hai chiếm
**1.213 / 1.773 file** — hơn hai phần ba cây thư mục — nhưng không ai trong nhóm sửa chúng.

---

## 2. Cấu trúc đề xuất

```
Assets/
  ThirdParty/
    Button/            ← dời
    Rock/              ← dời
    TextMesh Pro/      ← dời
  Art/        Data/    Scenes/    Scripts/    Sprite/    UI/    ~Prototype/
```

Thêm `Assets/Backgrounds/` cho ba ảnh nền hiện nằm lẫn trong `Assets/Scenes/`.

**KHÔNG dời `Assets/Sprite/`.** Art của Khoa. GUID sống sót qua việc dời, nhưng thói quen và
trí nhớ đường dẫn của Khoa thì không — và lợi ích gần bằng không.

---

## 3. Cái gì gãy với mỗi lần dời

### 3.1 Dời `Button`, `Rock`, `TextMesh Pro` vào `ThirdParty/`

| Nguy cơ | Thực tế đo được 09/11 |
| --- | --- |
| Tham chiếu trong scene/prefab | **An toàn.** Unity tham chiếu bằng GUID trong `.meta`, không bằng đường dẫn. Dời bằng Unity Editor (hoặc dời cả file lẫn `.meta`) thì mọi tham chiếu giữ nguyên |
| 7 file `.csproj` trong `tools/` | **Không ảnh hưởng.** Chúng chỉ trỏ tới `Assets/Scripts/Core/`, `Core.Tests/`, `Combat/`, `Combat.Tests/` — không file nào nhắc tới ba pack này |
| `tools/art/*`, `tools/doc/*`, `tools/merge_v7.py` | **Không ảnh hưởng.** `tools/art/make_placeholder_art.py` chỉ ghi vào `Assets/Art/Placeholder/`; không script nào đọc ba pack |
| `AGENTS.md`, `docs/core-api.md`, `docs/handover-core.md` | **Không ảnh hưởng.** Không file nào nhắc tới ba pack |
| `docs/decisions.md` | **Một dòng lỗi thời**, không phải gãy: D10 nhắc `Assets/TextMesh Pro/Examples & Extras/` — thư mục đó đã xoá từ trước. Dòng đó là ghi chép lịch sử, không phải đường dẫn đang dùng |
| `Assets/Art/Placeholder/README.md` + `manifest.json` | **Không ảnh hưởng** |

**Rủi ro thật sự duy nhất:** nếu dời bằng lệnh hệ thống mà **quên dời `.meta` đi kèm**, Unity
sẽ cấp GUID mới và mọi tham chiếu đứt. Dời bằng Unity Editor thì nó tự lo. Nếu dời ngoài
Editor thì phải dời `file` và `file.meta` cùng nhau, kể cả `.meta` của thư mục.

### 3.2 Tách ba ảnh nền khỏi `Assets/Scenes/`

| File | Kích thước | Sửa lần cuối |
| --- | --- | --- |
| `749.jpg` | 6.136 KB | 26/08 |
| `back ground 2.png` | 628 KB | 05/10 |
| `savanna-landscape-...-vector.jpg` | 128 KB | 18/07 |

Cả ba **không được `.unity` nào tham chiếu** tính tới 09/11 — nhưng chúng là của Khoa, có thể
đang để dành cho F2. **Đề xuất: dời sang `Assets/Backgrounds/`, không xoá.** Cần Khoa xác nhận.

---

## 4. Việc PHẢI làm trước, không liên quan dọn dẹp

> ⚠ Phát hiện trong lúc kiểm, nghiêm trọng hơn mọi mục tái cấu trúc ở trên.

`ProjectSettings/EditorBuildSettings.asset` hiện liệt kê **đúng một scene**:

```yaml
m_Scenes:
- enabled: 1
  path: Assets/Scenes/SampleScene.unity
```

**`Game.unity` KHÔNG có trong danh sách build.** Nghĩa là bản build WebGL ở I4 sẽ đóng gói
`SampleScene` — scene cũ đang treo "Missing (Mono Script)" từ D3 — chứ không phải game.

Sửa: mở `Game.unity`, **File → Build Settings → Add Open Scenes**, rồi bỏ `SampleScene` khỏi
danh sách (hoặc kéo `Game.unity` lên đầu). `ProjectSettings/**` ngoài phạm vi ghi của Lead UI.

Đây cũng là lý do **không được xoá `SampleScene.unity`** cho tới khi việc trên xong.

---

## 5. Thứ tự đề xuất, nếu Khoa duyệt

1. Khoa sửa Build Settings (mục 4). Không phụ thuộc gì khác, và nó đang là lỗi thật.
2. Dời ba pack vào `ThirdParty/` **bằng Unity Editor**, đóng mọi thứ khác trước.
3. Chạy lại cả ba bộ test: **299 / 81 / 177**. Lệch là có `.meta` bị bỏ sót.
4. Mở `Game.unity`, bấm Play, xác nhận màn cộng đồng còn nguyên.
5. Ảnh nền: hỏi Khoa trước, rồi mới dời.

Mỗi bước dừng được. Không bước nào cần làm cùng lúc với bước khác.
