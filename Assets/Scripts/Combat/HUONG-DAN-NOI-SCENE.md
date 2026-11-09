# Nối scene combat — hướng dẫn cho Khoa

Phần logic (B1-logic, B2, B3) đã xong và chạy được không cần Unity. Phần hình và cảm giác thả
quân là B1-hình và B4, thuộc về Khoa. File này là chỗ nối giữa hai phần.

Agent **không được sửa** `.unity`, `.prefab`, `.asset` (`HANDOFF.md` §8: "Scene Unity hỏng do
agent"), nên mọi thao tác trong Editor dưới đây Khoa tự làm.

---

## 0. Việc đầu tiên: mở Editor một lần

Agent không tạo file `.meta`. Mở Unity một lần, Unity sẽ tự sinh `.meta` cho:

- `Assets/Scripts/Combat/` và `Assets/Scripts/Combat.Tests/`
- `Assets/Data/beasts.json`

Sau đó mở **Window → General → Test Runner → EditMode**: phải thấy nhóm `BeforeUs.Combat.Tests`
và toàn bộ xanh. Nếu không thấy, kiểm tra `BeforeUs.Combat.asmdef` có được Unity nhận chưa.

Chạy ngoài Unity nhanh hơn nhiều, không cần mở Editor:

```
dotnet test tools/CombatTests
dotnet run --project tools/CombatTests/Calibrate -- 500
```

---

## 1. Ý chính: logic không biết gì về Unity

`BeforeUs.Combat` đặt `noEngineReferences: true`, giống `BeforeUs.Core`. Nghĩa là trong toàn bộ
thư mục này **không thể** gọi `UnityEngine` — compiler chặn.

Hệ quả cho Khoa: mô phỏng trận không vẽ gì cả. Nó giữ một danh sách đơn vị, mỗi đơn vị có toạ độ
và máu. Việc của scene là mỗi khung hình đọc danh sách đó rồi đặt sprite đúng chỗ. Khoa sửa hình
thoải mái mà không chạm được vào luật, và ngược lại.

---

## 2. Một MonoBehaviour tối thiểu

Tạo một GameObject rỗng trong scene combat, gắn script này:

```csharp
using UnityEngine;
using BeforeUs.Combat;

public sealed class BattleView : MonoBehaviour
{
    [SerializeField] private GameObject memberPrefab;
    [SerializeField] private GameObject beastPrefab;

    // Tỉ lệ quy đổi: 1 đơn vị sân = bao nhiêu đơn vị Unity.
    // Sân rộng 60 đơn vị (beasts.json), màn hình ngang ~17 đơn vị Unity → thử 0.28.
    [SerializeField] private float scale = 0.28f;

    private BattleSimulation _battle;
    private readonly Dictionary<int, Transform> _memberViews = new Dictionary<int, Transform>();
    private readonly Dictionary<int, Transform> _beastViews = new Dictionary<int, Transform>();

    /// <summary>Tầng Game gọi hàm này khi vào trận (I1).</summary>
    public void Begin(BattleSimulation battle)
    {
        _battle = battle;

        foreach (Combatant m in battle.Tribe)
        {
            _memberViews[m.Id] = Instantiate(memberPrefab, transform).transform;
            _memberViews[m.Id].gameObject.SetActive(false);   // chưa thả thì chưa hiện
        }

        foreach (Combatant b in battle.Beasts)
        {
            _beastViews[b.Id] = Instantiate(beastPrefab, transform).transform;
        }

        battle.Start();
    }

    private void Update()
    {
        if (_battle == null || _battle.IsFinished) return;

        _battle.Step(Time.deltaTime);     // logic tự tiến, tự chia thành bước cố định

        foreach (Combatant m in _battle.Tribe)
        {
            Transform view = _memberViews[m.Id];
            view.gameObject.SetActive(m.IsDeployed && m.IsAlive);
            view.position = ToWorld(m.Position);
        }

        foreach (Combatant b in _battle.Beasts)
        {
            Transform view = _beastViews[b.Id];
            view.gameObject.SetActive(b.IsAlive);
            view.position = ToWorld(b.Position);
        }

        if (_battle.IsFinished) ShowKetQua(_battle.Report);
    }

    private Vector3 ToWorld(CombatVector p)
    {
        return new Vector3((float)p.X * scale, (float)p.Y * scale, 0f);
    }
}
```

**Ba điều đáng nhớ:**

1. `Step` nhận **giây thật**; bên trong nó tự chia thành bước cố định 1/30 giây. Máy 30 fps hay
   144 fps đều cho cùng một trận. Đừng tự gọi `StepFixed()` trong `Update` — đó là lối dành cho
   test và cho công cụ hiệu chỉnh.
2. Lớp hiển thị **chỉ đọc**. Không có hàm nào cho nó đổi máu, giết ai, hay tuyên thắng thua.
3. `Begin` chỉ gọi `battle.Start()` khi Khoa muốn đồng hồ chạy. Trước đó trạng thái là `Ready` —
   muốn làm màn đếm ngược "3, 2, 1" thì cứ giữ ở đó bao lâu cũng được.

---

## 3. Thả quân khi người chơi chạm

```csharp
private void OnClick(Vector3 screenPosition)
{
    Vector3 world = Camera.main.ScreenToWorldPoint(screenPosition);
    CombatVector target = new CombatVector(world.x / scale, world.y / scale);

    DeployResult result = _battle.TryDeploy(_memberDangCam, target);

    switch (result)
    {
        case DeployResult.Ok:               /* chạy hiệu ứng thả */ break;
        case DeployResult.OutsideDeployZone: ShowLoi("Không thả được ở đây"); break;
        case DeployResult.AlreadyDeployed:   ShowLoi("Người này đã xuống rồi"); break;
    }
}
```

`TryDeploy` **thất bại thì không đổi gì**, nên cứ gọi thẳng rồi hiện lỗi; không cần kiểm tra trước.

Vùng thả hợp lệ đọc từ `_battle.Setup.Field`: `DeployMinX`, `DeployMaxX`, `Height`. Nên vẽ nó ra
màn hình (một dải mờ) — người chơi cần nhìn thấy mình được thả ở đâu.

---

## 4. Vẽ sân cho đúng

Đọc từ `_battle.Setup.Field`, đừng viết cứng:

| Thứ | Lấy ở đâu | Ý nghĩa trên màn hình |
| --- | --- | --- |
| Bề ngang sân | `Field.Width` (60) | toàn bộ chiều ngang |
| Bề dọc sân | `Field.Height` (12) | chiều cao khu vực đi lại |
| `Field.GoalX` | 60 với Dụ-và-hạ, 0 với Phòng thủ | **vạch thoát** của thú, hoặc **cửa hang** |
| Vùng thả | `DeployMinX`–`DeployMaxX` | dải người chơi được chạm vào |

Hướng chạy của thú khác nhau giữa hai kiểu trận: **Dụ và hạ** thú chạy sang phải (thoát),
**Phòng thủ** kẻ địch tiến sang trái (về hang). Đọc `_battle.Setup.Kind` để lật sprite.

---

## 5. Hoạt ảnh lấy từ đâu

| Muốn vẽ gì | Đọc trường nào |
| --- | --- |
| Thanh máu | `Hp / MaxHp` |
| Đang vung đòn | `LastAttackAtSeconds` — vừa đổi nghĩa là vừa đánh |
| Sắp ra đòn | `TimeUntilNextAttack` đếm ngược về 0 |
| Đang nhắm ai | `TargetId` |
| Vừa ngã xuống | `DiedAtSeconds` khác −1 |
| Đồng hồ trận | `_battle.TimeRemainingSeconds` |

---

## 6. Trận kết thúc

```csharp
BattleReport report = _battle.Report;   // null cho tới khi xong

report.Won            // thắng hay thua
report.Reason         // vì sao: AllBeastsDown, BeastEscaped, TargetReached, TimeLimit, TribeDown
report.DeadMemberIds  // ai ngã TRONG SÂN
```

**`report` chưa phải kết quả cuối** gửi về vòng lượt. Mức tổn thất theo luật (`design.md` §5.2
"thua mất 2 người", §8 quota phòng thủ) do `DesignTollCasualtyPolicy` áp lên sau — nó có thể thêm
cả người đang ở nhà. Tầng Game làm việc đó ở I1, scene không phải lo.

Chữ hiện cho người chơi theo từng `Reason` nên đặt ở `strings.vi.json` (đất của Lead UI), không
viết cứng trong scene.

---

## 7. Chỉnh cảm giác ở B4 — chỉnh cái gì

Mọi con số trận nằm trong `Assets/Data/beasts.json`. Sửa file, chạy lại công cụ hiệu chỉnh, xem
tỉ lệ thắng đổi bao nhiêu. **Không bao giờ sửa `balance.json`** — đó là luật chơi, và đổi nó là
quyết định riêng.

| Muốn gì | Sửa gì |
| --- | --- |
| Trận dài/ngắn hơn | `battles.*.time_limit_seconds` (dải 20–40 của §9) |
| Người chạy nhanh hơn | `tribe_in_battle.move_speed` |
| Đánh dày đòn hơn | `tribe_in_battle.attack_interval` (nhỏ hơn = nhanh hơn) |
| Một loài khó hơn | `species[].stats.hp` hoặc `attack_damage` |
| Bầy linh cẩu đông hơn | `species[].pack_size` |

**Cảnh báo một chỗ nhạy:** chênh lệch giữa `tribe_in_battle.move_speed` và `species[].stats.move_speed`
rất nhạy. Thú chạy nhanh hơn người thì người chỉ đánh được nó lúc nó chạy ngang qua; thú chạy
chậm hơn thì người bám theo đánh suốt trận. Đổi tốc độ thú từ 2,6 xuống 2,3 (người là 2,4) lật hẳn
trận từ khó sang dễ. Đổi từng chút một và chạy lại công cụ đo.

**Khối `levers` đang tắt hết và nên để yên** cho tới khi Khoa đọc bảng B3 — xem báo cáo.

---

## 8. Những chỗ agent cố ý không làm

- Không có scene, prefab, sprite nào. Toàn bộ hình là của Khoa.
- Không có âm thanh (F3).
- Chưa nối vào vòng lượt. `ICombatResolver` hiện là bản đồng bộ chạy headless; nối scene thật vào
  vòng lượt là **I1**, và nó cần một lớp bất đồng bộ mà `docs/core-api.md` §7 ghi rõ phải do **một**
  Lead duy nhất thiết kế.
- Chưa có màn "lần đầu gặp một loài" (`design.md` §13). Chữ đã sẵn trong `beasts.json`
  (`attack_style_text`), chỉ còn việc hiện ra.
