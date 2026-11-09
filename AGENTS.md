# Before Us — Quy tắc cho mọi agent

Đọc file này trước mỗi việc. Chủ dự án và người thiết kế: **Khoa**. Supervisor điều phối việc theo `HANDOFF.md`.

## Nguồn sự thật (thứ tự ưu tiên)

1. `docs/design.md`: luật chơi và con số. Mục **[ĐỀ XUẤT]** dùng được nhưng phải báo lại.
2. `Assets/Data/*.json`: con số cân bằng, khớp `docs/design.md`.
3. `tools/sim/before_us_sim.py`: mô phỏng tham chiếu; Core phải cho kết quả tương đương.
4. `HANDOFF.md`: kế hoạch, việc, tiêu chí nghiệm thu.

Nếu các nguồn mâu thuẫn nhau hoặc mâu thuẫn với việc được giao: **dừng và hỏi**, không tự đoán.

## Kiến trúc bắt buộc

- `Assets/Scripts/Core/`: luật chơi bằng C# thuần. **Không** tham chiếu `UnityEngine` (asmdef `BeforeUs.Core` đặt `noEngineReferences: true`). Không dùng API C# mới hơn C# 9.
- `Assets/Scripts/Core.Tests/`: test NUnit cho Core. Core cũng phải build và test được ngoài Unity bằng `dotnet test tools/CoreTests/` để chạy nhanh.
- `Assets/Scripts/Game/`: cầu nối Core với Unity (vòng lượt, lưu ván, gọi combat).
- `Assets/Scripts/UI/` + `Assets/UI/` (UXML, USS): giao diện bằng UI Toolkit. UI chỉ đọc trạng thái và gọi Core, không tự tính luật.
- `Assets/Scripts/Combat/`: scene combat thời gian thực. Trả về Core `CombatResult { bool Won; IReadOnlyList<int> DeadMemberIds; }` qua `ICombatResolver`.
- Mọi con số cân bằng đọc từ `Assets/Data/*.json`. Không viết cứng.
- Ngẫu nhiên chỉ qua `IRandom` có seed và lưu được trạng thái, để test lặp lại và lưu ván chính xác.

## Cách làm việc

- Chỉ làm đúng việc được giao (một mã việc trong `HANDOFF.md`), trên một nhánh git riêng `task/<mã-việc>`.
- Không thêm tính năng ngoài phạm vi MVP. Ý tưởng hay thì ghi vào báo cáo, không làm.
- Thay đổi Core phải có test. Chạy toàn bộ test trước khi báo xong; test cân bằng (`BalanceTests`) phải qua.
- Không sửa file `.unity`, `.prefab`, `.asset` trừ khi việc được giao nói rõ. Ưu tiên tạo đối tượng bằng code hoặc UXML.
- Không đổi con số trong `balance.json` hay luật trong `docs/design.md`. Cần đổi thì đề xuất trong báo cáo, kèm kết quả mô phỏng.
- Không thêm package hay thư viện mới nếu chưa được duyệt (đã duyệt: `com.unity.nuget.newtonsoft-json`, NUnit, UI Toolkit, Input System, Cinemachine).
- Văn bản hiện cho người chơi: tiếng Việt, đặt trong file chuỗi (`Assets/Data/strings.vi.json`), không viết cứng trong code.
- Đặt tên code bằng tiếng Anh; chú thích ngắn.

## Báo cáo khi xong việc

```
Mã việc:
Đã làm:
File thay đổi:
Test đã chạy và kết quả:
Giá trị [ĐỀ XUẤT] đã dùng hoặc tự đặt:
Câu hỏi cho Khoa / rủi ro:
```
