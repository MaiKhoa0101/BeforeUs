# Core API — Before Us

> **Phạm vi file này: chữ ký API của `BeforeUs.Core`.** Không phải luật chơi.
> Luật và con số: `docs/design.md` + `Assets/Data/balance.json`. Nếu file này mâu thuẫn với chúng, chúng đúng.
> Quyết định vận hành: `docs/decisions.md`.
>
> Viết cho Lead UI (A1–A4) và Lead combat (B1–B4) để khỏi phải đọc ngược mã nguồn Core.
> Cập nhật: 2026-11-08, sau playtest 01. Trạng thái: **C1–C8 xong.** Luật đã đổi theo D13 và D14 — xem `docs/decisions.md` và `docs/playtest-01.md`.

## 0. Cái gì CHƯA có — đọc trước khi lập kế hoạch

| Chưa có | Mã việc | Hệ quả cho bạn |
| --- | --- | --- |
| 4 bot và test cân bằng | C8 | — (không ảnh hưởng UI) |

**Vừa xong 2026-11-08, dùng được ngay:**

- **`NeuronNetwork`** + `Assets/Data/neurons.json` (C5) — mua nút, nút cổng, chiến tích, tiến hóa. **A3 dựng được rồi.** Xem §11.
- **Điều kiện thắng** — `TurnOutcome.Won`, `Evolved`, `StageBefore/After`. **A4 dựng được rồi.** Vòng chơi đã khép kín.
- **`SaveGame`** (C7) — lưu và tải ván. **Nút "Tiếp tục" (A4) dựng được rồi.** Xem §12.
- **`EventSystem`** (C6) — sự kiện §8 đầy đủ: ân hạn, trận hướng dẫn lượt 3, ba loại sự kiện, quota phòng thủ. **A2 nhịp 2 dựng được rồi.** Xem §7.
- **`ProbabilisticCombatResolver`** (C6, phần combat) — xem §6. Nếu bạn đang dùng test double cho `ICombatResolver`, **đổi sang bản thật ngay**: một dòng khởi tạo.

### ⚠ Đừng bao giờ cài lại công thức luật ở ngoài Core

Quy tắc này vẫn áp dụng cho mọi thứ Core chưa làm xong. Khi bạn cần một `ICombatResolver`, một nguồn sự kiện, hay bất cứ mảnh luật nào chưa có, bản tạm phải là **test double kịch bản hóa** — kết quả do **bên gọi bơm vào**, lớp đó không tự tính gì:

```csharp
// ĐÚNG: không chứa một mẩu luật nào.
sealed class ScriptedResolver : ICombatResolver
{
    public Queue<CombatResult> Scripted = new Queue<CombatResult>();
    public CombatResult Fallback = new CombatResult(false, null);

    public CombatResult Resolve(CombatRequest request)
        => Scripted.Count > 0 ? Scripted.Dequeue() : Fallback;
}
```

**Tuyệt đối không** đọc `combat_odds`, `base_win`, `defence_odds` rồi tự tính `0.50 + 0.08 × k + …`. Công thức `design.md` §5.2 và §8 chỉ được cài **đúng một lần, bên trong Core** — nay là `ProbabilisticCombatResolver`.

Vì sao nghiêm khắc chỗ này: hai bản cài cùng một công thức sẽ trôi khỏi nhau — một bên sửa, bên kia không, và không ai phát hiện cho tới khi số liệu cân bằng sai. `HANDOFF.md` §8 xếp **"agent âm thầm đổi luật" là rủi ro số 1 của dự án**, và cách nó xảy ra trong thực tế gần như luôn là: ai đó cần một con số, thấy công thức trong tài liệu, gõ lại vào file của mình.

Áp dụng cho **mọi** công thức, không riêng combat: hái lượm (§5.1), độ khó (§11 design.md), khí hậu (§10 design.md), sức chứa hang (§4 design.md), giá nút neuron (§7). Core đã phơi ra hết dưới dạng hàm — gọi chúng, đừng gõ lại.

---

Đã có và **ổn định**, cứ dựa vào: `BalanceConfig`, `IRandom`, `Member`, `GameState`, `Climate`, `Difficulty`, `TurnPlan`, `TurnSystem`, `TurnOutcome`, `ICombatResolver`.

Core là C# thuần, **không tham chiếu `UnityEngine`** (asmdef `BeforeUs.Core`, `noEngineReferences: true`). Nếu bạn thấy mình muốn thêm `UnityEngine` vào Core thì thiết kế đã lệch — hỏi trước.

---

## 1. Một lượt trọn vẹn

Đoạn này là mã thật, có một test trong `Core.Tests` chạy đúng nó để nó không mục theo thời gian.

```csharp
using BeforeUs.Core;
using BeforeUs.Core.Config;

// --- dựng ván mới (một lần mỗi ván) ---
string json = /* nội dung Assets/Data/balance.json, tầng Unity tự nạp */;
BalanceConfig config = BalanceConfig.FromJson(json);          // chế độ lấy từ trường "mode" trong JSON
                                                              // hoặc: BalanceConfig.FromJson(json, GameMode.Mvp)

IRandom random = new XorShift128Random(seed: 12345);
MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(config));
GameState state = GameState.NewGame(config, random, factory);

NeuronData neuronData = NeuronNetwork.LoadData(neuronsJson);  // nội dung Assets/Data/neurons.json
NeuronNetwork nodes = new NeuronNetwork(neuronData, config, state, applyNodeEffects: true);

ICombatResolver combat = new ProbabilisticCombatResolver(config, random, nodes);

TurnSystem turn = new TurnSystem(
    config, state, random, factory,
    new Climate(config), new Difficulty(config),
    nodes, combat, events: null);                             // events: C6

// --- mỗi lượt ---
turn.BeginTurn();                                             // áp dụng sự kiện đầu lượt (design.md §2 bước 1)

// người chơi phân công, sửa thoải mái bao nhiêu lần cũng được
PlanResult r = turn.Plan.Assign(ActivityKind.Gather, 1, 2, 3);
if (!r.Succeeded)
{
    ShowError(r.Error, r.Message);                            // xem bảng PlanError ở §3
}

turn.Plan.Assign(ActivityKind.Morale, 4, 5);
turn.Plan.Clear(ActivityKind.Morale);                         // đổi ý: bỏ hẳn hoạt động
turn.Plan.Assign(ActivityKind.Gather, 1, 2, 3, 4, 5);         // gán lại = THAY THẾ, không cộng dồn

// dự báo "cuối lượt cần X, đang có Y" (design.md §2)
int needed = turn.FoodNeeded;
int have   = turn.ForecastFood;

// --- kết thúc lượt: đây là nơi DUY NHẤT mọi thứ được tính ---
TurnOutcome outcome = turn.EndTurn();
```

Sau `EndTurn()`: `state.Turn` đã tăng, `turn.Plan` đã xóa sạch, `outcome` là báo cáo của lượt vừa xong.

---

## 2. D2 — luật quan trọng nhất với UI

`docs/decisions.md` D2, nhắc lại vì nó quyết định cách bạn dựng màn chính:

- Trong lượt, phân công **chỉ là dự định**. `TurnPlan` không có đường nào chạm `GameState` — gọi bao nhiêu lần cũng không đổi tài nguyên, không đổi sĩ số, không đổi lượt.
- **Không gì được tính** cho tới khi gọi `EndTurn()`.
- Một hoạt động xuất hiện **tối đa một lần** mỗi lượt và trả năng lượng **đúng một lần**, không phụ thuộc số người. Cử 1 người hay 5 người đi Kiếm ăn đều tốn 1 năng lượng.
- **Mỗi thành viên tối đa 1 hoạt động** mỗi lượt.

Nghĩa là UI được tự do: cho người chơi kéo thả, undo, đổi ý bao nhiêu lần tùy thích, không cần lớp "dự định" riêng — `TurnPlan` đã là lớp đó.

---

## 3. `TurnPlan` — phân công trong lượt

```csharp
PlanResult Assign(ActivityKind activity, params int[] memberIds);
PlanResult Assign(ActivityKind activity, IReadOnlyList<int> memberIds);
void Clear(ActivityKind activity);
void ClearAll();

IReadOnlyList<int> MembersOn(ActivityKind activity);
int  PartySizeOn(ActivityKind activity);
bool IsBusy(int memberId);
bool IsUnlocked(ActivityKind activity);

int EnergyMax;            // theo giai đoạn hiện tại (design.md §4)
int EnergySpent;
int EnergyRemaining;
int AssignedMemberCount;
int IdleMemberCount;

bool CanAfford(ActivityKind activity);        // còn đủ năng lượng để THÊM hoạt động này?
int  EnergyCostToAdd(ActivityKind activity);  // phải trả thêm bao nhiêu
```

**Làm mờ thẻ hoạt động — dùng hai truy vấn, không tự tính:**

```csharp
bool unlocked  = turn.Plan.IsUnlocked(activity);
bool affordable = turn.Plan.CanAfford(activity);

card.SetEnabled(unlocked && affordable);
card.tooltip = !unlocked  ? "Chưa mở khóa"
             : !affordable ? $"Cần {turn.Plan.EnergyCostToAdd(activity)} năng lượng, còn {turn.Plan.EnergyRemaining}"
             : null;
```

Hai truy vấn tách bạch **có chủ đích**, để UI nói được lý do: Săn bắt chưa mua nút thì `IsUnlocked` sai nhưng `CanAfford` vẫn đúng.

**Ca biên quan trọng:** hoạt động **đã có người** thì `EnergyCostToAdd` bằng **0** và `CanAfford` luôn **đúng** — kể cả khi năng lượng đã cạn sạch. Một hoạt động trả năng lượng đúng một lần mỗi lượt (D2), nên cử thêm người vào hoạt động đang có là miễn phí. Nếu UI tự tính `EnergySpent + cost <= EnergyMax` thì sẽ làm mờ nhầm đúng ca này.

Cả hai là **truy vấn thuần, không có tác dụng phụ** — gọi mỗi khung hình cũng được. Bất biến được test khóa: `CanAfford(a)` đúng khi và chỉ khi `Assign(a, …)` không trả `NotEnoughEnergy`.

`Assign` với danh sách rỗng ⇔ `Clear`. **Thất bại thì không đổi gì** — dự định cũ giữ nguyên, nên UI có thể gọi thẳng rồi hiện lỗi, không cần kiểm tra trước.

`PlanResult`: `.Succeeded`, `.Error`, `.Message` (tiếng Việt, dùng tạm được để hiện; khi cần bản chuẩn thì ánh xạ `.Error` sang `strings.vi.json`).

| `PlanError` | Khi nào |
| --- | --- |
| `None` | thành công |
| `ActivityLocked` | hoạt động chưa mở khóa ở lượt này |
| `UnknownMember` | id không có trong bộ tộc |
| `MemberAlreadyAssigned` | người đó đã được cử cho hoạt động khác |
| `DuplicateMember` | cùng một người hai lần trong danh sách |
| `NotEnoughEnergy` | vượt ngân sách năng lượng |

`ActivityKind`: `Gather`, `Morale`, `Build`, `Hunt`, `Explore`. Danh sách đủ: `ActivityRules.All`.

**Mở khóa** (`IsUnlocked`) — dùng để làm mờ nút, đừng tự tính lại:
Kiếm ăn và Nâng tinh thần mở từ đầu · Khám phá từ lượt 6 **và** còn vùng chưa đến · Xây dựng và Săn bắt cần **nút neuron đã mua** (`s2_stone_tools`, `s2_scavenge_meat`), **không phải** cứ lên giai đoạn 2 là có (`decisions.md` D1).

---

## 4. `TurnSystem`

```csharp
TurnPlan Plan;
SiteSystem Sites;                // slot công trình (v7 §B) — xem §14
ClimatePhase Phase;              // Wet | Dry
int DifficultyLevel;
double FindChanceMultiplier;     // hệ số bào mòn của độ khó (v7 §A.7)
double SpeedThresholdRelief;     // Ổ đựng thức ăn nới ngưỡng bao nhiêu (v7 §B.4)
int FoodNeeded;                  // "cuối lượt cần X"
int ForecastFood;                // "đang có Y" — chỉ phần CHẮC CHẮN
GatherForecast ForecastGather(); // { int Guaranteed; double Expected; }
void BeginTurn();
TurnOutcome EndTurn();           // NÉM nếu chưa đạt sàn Kiếm ăn hoặc chưa chọn slot — xem §13
```

Constructor nay nhận thêm hai danh mục. **Thiếu cái nào thì hoạt động tương ứng NÉM, chứ không âm thầm bỏ qua:**

```csharp
new TurnSystem(config, state, random, factory, climate, difficulty, nodes, combat,
               events, odds,
               plants: PlantCatalog.Load(<Assets/Data/plants.json>),
               sites:  SiteCatalog.Load(<Assets/Data/sites.json>));
```

`ForecastFood` chỉ đếm thức ăn **chắc chắn**: hái lượm theo dự định hiện tại, cộng thức ăn sự kiện đã nhận. **Không** tính thức ăn từ Săn bắt, vì trận có thể thua. Nếu UI muốn hiện khoảng "tệ nhất–tốt nhất" thì tự cộng thêm `k × hunt_food[stage]`; Core cố ý không đoán hộ.

> **⚠ CẢNH BÁO CŨ ĐÃ GỠ (2026-11-08).** Chỗ này trước đây cảnh báo *"thức ăn không bằng `k × yield` vì mỗi người có `speed` riêng; ba người mang về 3+2+4 chứ không phải 3+3+3"*. **Cảnh báo đó nay SAI.** D4 của Khoa (playtest 01) bỏ `speed` khỏi công thức lượng thức ăn, và `GatherYieldPerMember` / `GatherRules.FoodFromMember` **không còn tồn tại**.

**Cạm bẫy mới, nặng hơn cái cũ.** Từ v7, thức ăn hái lượm là **tổng của những lần rút trúng** — mỗi người rút riêng với mỗi loài thực vật đã khám phá. Nó **không suy ra được từ `k` bằng BẤT KỲ phép nhân nào**, và hai lượt giống hệt nhau vẫn cho hai con số khác nhau.

```csharp
GatherForecast f = turn.ForecastGather();
ShowGuaranteed(f.Guaranteed);   // CHẮC CHẮN có — chỉ từ khung đảm bảo theo speed
ShowExpected(f.Expected);       // kỳ vọng dài hạn, KHÔNG phải con số sẽ xảy ra
```

`turn.ForecastFood` giữ đúng nghĩa cũ ("chắc chắn có"): nay bằng `ForecastGather().Guaranteed` cộng thức ăn sự kiện.

Sau khi lượt kết thúc, `outcome.Gather` cho chi tiết: ai tìm được loài gì, và có khám phá được loài mới không.

`speed` nay làm đúng hai việc trong hái lượm:

1. **theo từng người** — tăng cơ hội rút trúng **loài hiếm**;
2. **theo TỔNG cả nhóm** — đạt `guaranteed_total_speed` của một loài thì nhóm **chắc chắn tìm thấy loài đó MỘT lần** (D16 của Khoa).

Nó **không** còn nhân vào lượng thức ăn.

> ⚠ Điểm 2 là nơi dễ đọc nhầm nhất của cả §A. Ngưỡng là **tổng của nhóm**, không phải chỉ số từng người; và nó cho **một** lần tìm thấy cho **cả nhóm**, không phải mỗi người một lần. Bản cài đầu tiên của chính Core đã đọc nhầm cả hai chỗ — xem `decisions.md` D16.

### Xem trước một trận (góp ý 3 của Khoa, playtest 01)

```csharp
CombatPreview PreviewCombat(ActivityKind activity);   // Hunt hoặc Explore
```

```csharp
CombatPreview preview = turn.PreviewCombat(ActivityKind.Hunt);
if (preview.HasParty)
{
    ShowFood(preview.FoodIfWon);                      // thức ăn NẾU thắng
    if (preview.WinChance.HasValue) ShowOdds(preview.WinChance.Value);
    else ShowUnknown();                               // KHÔNG được tự tính lấy
}
```

`CombatPreview`: `HasParty`, `PartySize`, `FoodIfWon`, `double? WinChance`.

- **Truy vấn thuần, không tác dụng phụ** — gọi mỗi khung hình cũng được, không gọi tới `ICombatResolver`.
- `FoodIfWon` = `k × hunt_food_per_member[stage]`. Khám phá không ra thức ăn nên luôn 0. **Giai đoạn 1 hiện là 0** — đúng dữ liệu, Khoa sẽ quyết sau khi chơi.
- `WinChance` lấy từ `ICombatOdds`, **cùng một công thức với lúc giải quyết thật**. `null` khi `TurnSystem` không có nguồn ước lượng (ví dụ đang dùng test double) — lúc đó hiện "chưa rõ", **đừng tự tính**.
- Core trả **dữ liệu thô**, không quy ra "dễ / vừa / khó". Cách diễn đạt là quyết định thiết kế của Khoa.

Nguồn ước lượng lấy tự động nếu `ICombatResolver` đã hiện thực `ICombatOdds` (`ProbabilisticCombatResolver` có). Nếu I1 thay bằng scene combat thật thì truyền riêng: `new TurnSystem(…, events, odds: probabilisticResolver)`.

Hệ số bào mòn của độ khó chốt theo trạng thái đầu lượt, nên di cư giữa lượt không đổi kết quả hái lượm của chính lượt đó.

Thứ tự giải quyết cố định, công khai qua `TurnSystem.ResolveOrder`:
`Gather → Morale → Build → Hunt → Explore`. Dùng nó nếu bạn muốn hoạt ảnh kết quả theo đúng thứ tự Core đã tính.

---

## 5. `TurnOutcome` — UI đọc gì sau mỗi lượt

```csharp
int Turn;                        // lượt VỪA kết thúc
ClimatePhase Phase;
int DifficultyLevel;
GatherOutcome Gather;            // chi tiết hái lượm: ai tìm được gì, có khám phá loài mới không

List<ActivityOutcome> Activities;
ActivityOutcome OutcomeOf(ActivityKind kind);   // null nếu không cử ai

int FoodFromActivities, FoodFromEvents, FoodTotal, FoodNeeded;
bool Starved;  int StarvationDeaths;

int NeuronGained;                // chỉ phần từ hoạt động, không gồm sự kiện
int MoraleDelta;                 // thay đổi ròng cả lượt, sau khi đã chặn ở 0

int BuildLevelBefore, BuildLevelAfter;  bool BuildLevelIncreased;
int RegionBefore,     RegionAfter;      bool Migrated;
int StageBefore,      StageAfter;       bool Evolved;
int CrewAfter;                           // sĩ số sau khi lượt đã giải quyết xong
int MoraleFromDeaths;                    // phần tinh thần mất vì có người chết; luôn <= 0

List<int> DeadMemberIds;         // mọi nguyên nhân: combat, đói, sự kiện
List<int> BornMemberIds;         // tối đa 1 mỗi lượt (design.md §6)
bool Lost;                       // THUA — dùng cái này (design.md §1)
bool TribeWipedOut;              // hết sạch người; hẹp hơn Lost, chỉ là thông tin
bool Won;                        // đã đạt giai đoạn thắng của chế độ (MVP: giai đoạn 3)
```

**Vòng chơi khép kín ở đây (A4):** `Won` → màn thắng, **`Lost`** → màn thua. Cả hai do `EndTurn()` đặt. `Evolved` → màn ăn mừng tiến hóa (design.md §7: đổi ngoại hình toàn bộ bộ tộc).

> **⚠ Dùng `Lost`, đừng dùng `TribeWipedOut` làm điều kiện thua.** Theo góp ý 8 của Khoa ở playtest 01, thua là khi sĩ số **còn từ 1 trở xuống**, không còn là "hết sạch người". `TribeWipedOut` vẫn đúng nghĩa của nó (`CrewAfter == 0`) nhưng **không** còn là điều kiện thua. Ngưỡng đọc từ `config.LoseWhenCrewAtOrBelow`.

> **Mất tinh thần khi có người chết** (playtest 01, góp ý 1): mọi cái chết — đói, thua combat, quái thú ăn — đều trừ `morale.on_member_death`. UI đọc `MoraleFromDeaths` nếu muốn tách riêng phần đó khi kể lại lượt. Giá trị hiện tại là **[ĐỀ XUẤT] −2**, chờ Khoa duyệt.

`ActivityOutcome`: `Kind`, `PartySize`, `Food`, `Morale`, `Neuron`, `bool? CombatWon` (chỉ có nghĩa với `Hunt`/`Explore`), `DeadMemberIds`.

**Banner đổi pha** (design.md §10) — đừng tự đếm lượt:

```csharp
Climate climate = new Climate(config);
if (climate.IsPhaseChangeImminent(state.Turn))        // mặc định báo trước 2 lượt
{
    ShowBanner(climate.PhaseAt(state.Turn), climate.TurnsUntilPhaseChange(state.Turn));
}
```
Thêm: `climate.PhaseAt(turn)`, `climate.IsDry(turn)`, `climate.PhaseLength`, hằng `Climate.DefaultWarningTurns`.

---

## 6. `ICombatResolver` — ranh giới Core ↔ nhánh combat

Nguyên văn, khớp `AGENTS.md`:

```csharp
public interface ICombatResolver
{
    CombatResult Resolve(CombatRequest request);
}

public sealed class CombatRequest
{
    CombatKind Kind;                    // Hunt | Explore | Defence | Opportunity | Tutorial
    IReadOnlyList<int> MemberIds;       // những người được cử
    IReadOnlyList<int> AllMemberIds;    // TOÀN BỘ bộ tộc, kể cả người ở nhà
    int PartySize;                      // = MemberIds.Count, tức k của design.md §5.2
    int Stage, Region, BuildLevel, DifficultyLevel, Crew;
}

public sealed class CombatResult
{
    bool Won;
    IReadOnlyList<int> DeadMemberIds;
    static CombatResult Win();                              // thắng, không ai chết
    static CombatResult Loss(IReadOnlyList<int> dead);
}
```

**Ai gọi ai:** `TurnSystem` gọi `Resolve` trong `EndTurn()` khi có người được cử đi Săn bắt hoặc Khám phá. C6 sẽ gọi thêm cho Phòng thủ và Cơ hội combat.

**Resolver quyết ai chết, không phải Core.** Đây là ranh giới quan trọng nhất của mục này: `TurnSystem` chỉ xóa đúng những id trong `DeadMemberIds` và không tự thêm ai. Kiểu tấn công của quái thú (săn kẻ yếu / ngẫu nhiên / thách đấu kẻ mạnh, design.md §9) sống hoàn toàn bên nhánh combat.

Hai hệ quả:
- `DeadMemberIds` **được phép** chứa người không tham gia trận. Thua Săn bắt mất 2 người dù chỉ cử 1 là hợp lệ (design.md §5.2).
- Thắng vẫn có thể chết. `CombatResult.Win()` nghĩa là thắng *và* không ai chết, không phải "thắng nên không ai chết".

Chữ ký **đồng bộ**. I1 sẽ bọc lớp bất đồng bộ bên ngoài khi nối scene combat thật; C4–C8 không phải sửa, nên cứ dựng theo chữ ký này.

### `ProbabilisticCombatResolver` — đã có, dùng ngay

Bản thật đã xong. **Bỏ test double, đổi một dòng khởi tạo:**

```csharp
ICombatResolver combat = new ProbabilisticCombatResolver(config, random, nodes);
//                                                                       ^^^^^ tùy chọn:
//   IPurchasedNodes để lấy hiệu ứng nút "Bàn chân vòm" (+0.05 Khám phá) và
//   "Tiếng gọi báo động" (+0.05 Phòng thủ). Bỏ qua thì không cộng gì.
```

Dùng chung `IRandom` với `TurnSystem` — một thể hiện cho cả ván, nếu không lưu ván (C7) sẽ không khôi phục đúng.

Hai hàm công khai thêm, **dành riêng cho B3** (công cụ hiệu chỉnh combat):

```csharp
double WinChance(CombatRequest request);   // tỉ lệ thắng theo công thức, đã clamp
int    RaidQuota(CombatRequest request);   // số người quái thú ăn khi phòng thủ thua
```

`WinChance` tồn tại chính xác để B3 so tỉ lệ thắng của scene combat thật với **con số Core đang dùng**, chứ không phải với một bản chép tay của công thức. HANDOFF B3 yêu cầu "báo bảng lệch theo k, giai đoạn, độ khó" — gọi `WinChance` cho từng tổ hợp là ra bảng đó.

Trận hướng dẫn: `CombatKind.Tutorial` luôn thắng, không ai chết, bỏ qua mọi công thức (design.md §8).

---

## 7. `EventSystem` — sự kiện (A2)

```csharp
IEventDecisions decisions = /* của bạn: A2 hiện hộp thoại, bot ở C8 tự cài */;
EventSystem events = new EventSystem(decisions, odds: probabilisticResolver);

TurnSystem turn = new TurnSystem(config, state, random, factory,
    climate, difficulty, nodes, combat, events);
```

`odds` là tùy chọn (`ICombatOdds`, `ProbabilisticCombatResolver` hiện thực sẵn). Thiếu nó thì `EstimatedWinChance` luôn bằng 0 và người chơi phải quyết định mù.

Truyền `events: null` thì vòng lượt vẫn chạy, chỉ là không có sự kiện nào.

### Core hỏi, bạn trả lời

```csharp
public interface IEventDecisions
{
    bool ChooseGoodOutcome(EventDecisionContext context);   // sự kiện không combat
    bool AttackOpportunity(EventDecisionContext context);   // cơ hội combat: đánh hay rút lui
}
```

`EventDecisionContext`: `Config`, `State`, `Random`, `Phase`, `DifficultyLevel`, và `EstimatedWinChance` (chỉ có nghĩa với cơ hội combat).

`design.md` §8 ghi "người chơi cân nhắc được kết quả tốt khoảng 60%" — **60% là thuộc tính của người chơi, không phải của luật**, nên Core không viết cứng nó. Bot ở C8 cài 0.6; A2 cài bằng lựa chọn thật của người chơi.

> **`ChooseGoodOutcome` là bản giản lược của MVP.** Nội dung sự kiện thật chỉ có ở `events.json` (I2), nên hiện một sự kiện không combat chỉ có hai kết quả tốt/xấu, đúng như `before_us_sim.py`. Khi I2 xong, interface này sẽ nhận thêm lựa chọn cụ thể.

### Đọc kết quả để hiện hộp sự kiện

```csharp
TurnOutcome outcome = turn.EndTurn();
TurnEventRecord ev = events.LastEvent;      // sự kiện của lượt vừa rồi
```

`TurnEventRecord`: `Kind`, `Turn`, `CombatWon` (`bool?`), `Declined` (rút lui), `Food`, `Morale`, `Neuron`, `DeadMemberIds`.

`TurnEventKind`: `None`, `Tutorial`, `NonCombatGood`, `NonCombatBad`, `Raid`, `Opportunity`.

Luật §8 đã cài sẵn: lượt 1–5 không sự kiện; **lượt 3 là trận hướng dẫn** (luôn thắng, không ai chết, +2 tinh thần +3 neuron +1 chiến tích) và lượt đó **không** rút sự kiện nào khác; từ lượt 6 mỗi lượt máy có 70% khả năng xảy ra sự kiện, chia 40% không combat / 20% Ẩm hoặc 25% Khô thú tấn công hang / phần còn lại là cơ hội combat.

### NỢ-I1: quyết định sự kiện hiện là ĐỒNG BỘ

`RunMachineTurn` chạy bên trong `EndTurn()`, nên `IEventDecisions` **không chờ được hộp thoại bất đồng bộ**. Cùng đúng một vấn đề với combat thật.

**`HANDOFF.md` đã xếp việc này vào I1** ("Nối combat thật vào vòng lượt qua `ICombatResolver` (bất đồng bộ)", tuần 2). **Không ai giải nó hôm nay** — hình dạng API bất đồng bộ phải do **một** Lead duy nhất quyết, nếu không dự án sẽ có hai control flow song song.

- **A2 bây giờ:** dựng hộp sự kiện ở dạng **chỉ hiển thị** — đọc `events.LastEvent` để kể lại chuyện vừa xảy ra, **không hỏi người chơi**. Tiêu chí "xong khi" của A2 trong `HANDOFF.md` là *"Gợi ý hiện đúng lượt, tắt được"*, đạt được trọn vẹn mà không cần đụng tới bất đồng bộ.
- **Đừng tự dựng vòng lặp bất đồng bộ quanh Core.** Thấy cần thì báo Supervisor.
- **Người làm I1:** đây là món nợ của bạn. `IEventDecisions` và `ICombatResolver` cùng cần một lời giải, hãy giải chúng một lần.

### NỢ-I2: sự kiện "ngẫu nhiên" chưa hoãn sang đầu lượt sau

`design.md` §2 bước 1 và §8 nói sự kiện không combat loại "không cần chọn" phải hiện và áp dụng ở **đầu lượt người chơi kế tiếp**. **Chưa cài** — §8 không cho biết tỉ lệ giữa loại "lựa chọn" và loại "ngẫu nhiên" trong nhóm 40%, nên mọi con số đều là bịa. Hiện mọi sự kiện không combat giải quyết ngay ở lượt máy.

`ITurnEvents.ApplyPendingAtTurnStart` có sẵn chỗ cắm, thân rỗng **có chủ đích**. Gỡ nợ ở I2 khi `events.json` có nội dung thật và mỗi sự kiện tự khai loại của nó. Chi tiết: `docs/decisions.md` D12.

---

## 8. Đọc trạng thái để hiện lên màn hình

```csharp
int    Turn, Stage, Region, Neuron, Morale;
int    BuildLevel;                           // SUY RA, không có setter — xem §14
List<string> ClearedSiteIds;                 // slot đã dọn trong ván này (v7 §B)
int    FoodNestTier;                         // bậc Ổ đựng thức ăn, 0 = ổ đất trống
int    SitesClearedAtStart;                  // slot dọn sẵn của vùng (v7 §F.2)
int    CombatWins, Hunts, MigratedEver;     // chiến tích (design.md §7.1)
int    Crew;                                 // = Members.Count
List<Member> Members;
Member FindMember(int id);

int LairCapacity(BalanceConfig config);      // 6 + 2 × build_level
int FoodNeeded(BalanceConfig config);
int EnergyMax(BalanceConfig config);
```

> 🚫 **`GameState.BuildPoints` đã chết** (D18). Nó còn đó như một thuộc tính chỉ-đọc **luôn trả 0**, chỉ để nhánh UI còn biên dịch được trong lúc gỡ. `TurnScreenModel` đang in `build=<cấp>/<điểm>` — phần `<điểm>` nay vô nghĩa, xin gỡ.

`Member` **bất biến**: `Id`, `Speed`, `Strength`, `Hp`, `Traits` (`IReadOnlyList<string>` dạng `"nhóm:lựa chọn"`). Chỉ số cố định suốt đời, không có cơ chế bị thương.

`Traits` hiện **luôn rỗng** — bộ đặc điểm sẽ được định nghĩa ở F1 theo đúng art Khoa vẽ được. Cơ chế rút đã sẵn, chỉ chờ dữ liệu. Đừng viết cứng tên đặc điểm trong UI.

Con số cân bằng đọc qua `BalanceConfig`, đừng viết cứng:
`config.EnergyMaxForStage(stage)`, `UpkeepForStage`, `HuntFoodForStage`, `DryPenaltyForStage`, `RequiredNeuronForStage`, `FeatsForStage`, `RaidQuotaForRegion`, `EnergyCostOf(activity)`, `NeuronPerMemberOf(activity)`, `UnlockOf(activity)`, `WinStage`, `RegionMaxIndex`, `DifficultyPeriodTurns`. Số thô: `config.Data`.

---

## 9. Ngẫu nhiên

```csharp
public interface IRandom
{
    double NextDouble();                       // [0, 1)
    int    NextInt(int minInclusive, int maxExclusive);
    string SaveState();                        // "xs128:xxxxxxxx,…"
    void   LoadState(string state);
}
```

Bản cài: `XorShift128Random(int seed)`. **Dùng chung một thể hiện cho cả ván** — Core, sự kiện và combat cùng rút từ đó, nếu không thì lưu ván (C7) sẽ không khôi phục đúng.

Đừng dùng `System.Random` hay `UnityEngine.Random` ở bất cứ đâu ảnh hưởng tới luật: chúng không lưu được trạng thái, và `System.Random` còn cho chuỗi khác nhau giữa Mono và CoreCLR nên test và bản build sẽ lệch nhau.

---

## 10. Chạy test Core

```
dotnet test tools/CoreTests
```
Không cần mở Unity. Core cũng build trong Unity qua asmdef `BeforeUs.Core` / `BeforeUs.Core.Tests`.

---

## 11. `NeuronNetwork` — mạng lưới neuron (A3)

```csharp
NeuronData data = NeuronNetwork.LoadData(neuronsJson);     // Assets/Data/neurons.json
NeuronNetwork network = new NeuronNetwork(data, config, state, applyNodeEffects: true);
```

`NeuronNetwork` hiện thực `IPurchasedNodes`, nên **truyền thẳng nó vào `TurnSystem`** và vào `ProbabilisticCombatResolver`. Mua nút xong thì Xây dựng / Săn bắt mở khóa ngay, không cần làm gì thêm.

```csharp
IReadOnlyList<NeuronNode> NodesOfStage(int stage);   // dựng màn A3 từ đây
NeuronNode Node(string nodeId);
bool Has(string nodeId);

PurchaseResult CanBuy(string nodeId);   // hỏi trước, để làm mờ nút
PurchaseResult Buy(string nodeId);      // mua thật; mở nút Tiến hóa = lên giai đoạn luôn

bool CanEvolve;                         // nút Tiến hóa của giai đoạn hiện tại có mở được không
bool HasAllGates(int stage);
bool HasFeats(int stage);
NeuronNode EvolveNodeOfCurrentStage();

NodeEffects Effects;                    // tổng hiệu ứng số của các nút đã mua
IReadOnlyCollection<string> PurchasedIds;
```

`NeuronNode`: `Id`, `Name`, `Branch`, `Requires`, `Stage`, `Cost` (đã chốt theo chế độ), `Type` (`Root`/`Gate`/`Optional`/`Evolve`), `Data.EffectText` (mô tả hiệu ứng, nguyên văn design.md).

| `PurchaseError` | Khi nào |
| --- | --- |
| `UnknownNode` | id không có trong neurons.json |
| `AlreadyPurchased` | đã mở rồi |
| `WrongStage` | nút của giai đoạn khác |
| `MissingPrerequisite` | chưa mở nút gốc của nhánh |
| `NotEnoughNeuron` | thiếu neuron |
| `MissingGates` | nút Tiến hóa: chưa đủ 3 nút cổng |
| `MissingFeats` | nút Tiến hóa: chưa đủ chiến tích |

`.Message` của `MissingFeats` đã liệt kê sẵn phần còn thiếu dạng `"thắng trận 0/1, săn thành công 1/2"` — hiện thẳng lên UI được.

**Dựng màn A3:** `NodesOfStage(state.Stage)` cho 10 nút, nhóm theo `Branch` ra 3 nhánh. Trạng thái mỗi nút: đã mua (`Has`), mua được (`CanBuy(...).Succeeded`), hay khóa (xem `.Error` để biết vì sao). Chiến tích đọc từ `state.CombatWins`, `state.Hunts`, `state.MigratedEver`; yêu cầu đọc từ `config.FeatsForStage(state.Stage)`.

**Cờ `applyNodeEffects`.** Tắt thì nút vẫn mở khóa hoạt động và vẫn tiến hóa được, nhưng **mọi hiệu ứng số bị bỏ qua** (`Effects` trả `NodeEffects.None`). Cờ này tồn tại cho Cổng 2: mô phỏng tham chiếu không tính hiệu ứng nút, nên C8 chạy hai lần để đo riêng phần đóng góp của chúng. Trong game thật thì **bật**.

Hai hiệu ứng Core chưa áp dụng được: `bad_event_food_reduction` (cần `events.json` ở I2) và `raid_warning` (chỉ là thông tin cho UI — A2 đọc `Effects.RaidWarning` để quyết có hiện cảnh báo trước hay không). `Effects.NotYetApplied` liệt kê chúng ra thay vì im lặng.

---

## 12. `SaveGame` — lưu và tải ván (A4)

```csharp
using BeforeUs.Core.Save;

// lưu
string json = SaveGame.Write(state, random, config.Mode);
// … tầng Unity tự cất chuỗi này đi (PlayerPrefs, IndexedDB, file — tùy nền tảng)

// tải
LoadedGame loaded = SaveGame.Read(json, config);
// loaded.State, loaded.Random, loaded.Mode
```

`SaveGame` **không chạm hệ thống file** — nó chỉ đổi giữa trạng thái ván và một chuỗi JSON. Nơi cất là việc của tầng Unity, và phải thế thì WebGL mới chạy được (ở đó `System.IO` gần như vô dụng).

### Dựng lại ván sau khi tải

```csharp
LoadedGame loaded = SaveGame.Read(json, config);

MemberFactory factory = new MemberFactory(MemberStatRanges.FromConfig(config));
NeuronNetwork nodes = new NeuronNetwork(neuronData, config, loaded.State, applyNodeEffects: true);
ICombatResolver combat = new ProbabilisticCombatResolver(config, loaded.Random, nodes);
EventSystem events = new EventSystem(decisions, combat);

TurnSystem turn = new TurnSystem(
    config, loaded.State, loaded.Random, factory,
    new Climate(config), new Difficulty(config),
    nodes, combat, events, odds: combat,
    plants: PlantCatalog.Load(plantsJson),
    sites:  SiteCatalog.Load(sitesJson));
```

Ba điểm phải làm đúng, nếu không ván tải lại sẽ trôi khỏi ván gốc:

1. **Dùng chung `loaded.Random`** cho `TurnSystem`, `ProbabilisticCombatResolver` và `EventSystem`. Một thể hiện duy nhất.
2. **`NeuronNetwork` phải nhận `loaded.State`** — nó tự đọc lại các nút đã mua từ `State.PurchasedNodeIds`.
3. **`MemberFactory` không cần chỉnh tay**: `TurnSystem` tự gọi `ResumeFrom(State.NextMemberId)` trong constructor.

### ⚠ Chỉ lưu GIỮA HAI LƯỢT

Gọi `SaveGame.Write` **sau khi `EndTurn()` trả về**, đừng gọi giữa lúc người chơi đang phân công. Dự định trong lượt (`TurnPlan`) **cố ý không được lưu**: theo D2 nó chưa phải là gì cả cho tới khi người chơi bấm kết thúc lượt. Nếu A4 cần "lưu rồi thoát" ngay giữa lượt thì cách đúng là **bỏ dự định** và lưu trạng thái đầu lượt — người chơi phân công lại khi quay vào, không mất gì thật.

### Ván hỏng thì nổ, không im lặng

`SaveGame.Read` ném `SaveGameException` khi: sai phiên bản · sai chế độ (ván MVP nạp bằng số liệu bản đầy đủ) · thiếu trường · JSON hỏng · trạng thái ngẫu nhiên không đọc được · id thành viên trùng · `next_member_id` nhỏ hơn id đang sống. Hiện thông báo cho người chơi rồi quay về menu, **đừng nuốt lỗi và chơi tiếp** — ván sai còn tệ hơn ván mất.

File lưu hiện là **`version: 3`**. Đổi hình dạng file theo cách không đọc ngược được thì tăng `SaveGame.CurrentVersion`.

| Phiên bản | Đổi gì |
| --- | --- |
| 1 | bản đầu |
| 2 | thêm `discovered_plants` (v7 §A.4) |
| 3 | bỏ `build_level` và `build_points`; thêm `cleared_sites`, `food_nest_tier`, `sites_cleared_at_start` (D18) |

`build_level` **không còn được lưu** — nó là số suy ra từ ba trường mới (§14). Lưu một số suy ra là cách chắc chắn nhất để hai nguồn lệch nhau sau một lần sửa luật.

---

## 13. Sàn bắt buộc đi Kiếm ăn (v7 §A.6, D6 của Khoa)

> *"Ít nhất 1/3 số người phải đi tìm thức ăn để hạn chế việc người chơi phát triển quá nhanh."*

```csharp
int  GatherFloor;                 // ceil(crew × 1/3)
bool MeetsGatherFloor;
bool CanPossiblyMeetGatherFloor;  // cửa thoát — xem dưới
PlanResult ValidateForEndTurn();  // gọi trước khi bật nút "Kết thúc lượt"
```

`EndTurn()` **ném `InvalidOperationException`** nếu `ValidateForEndTurn()` không đạt. UI nên gọi nó trước và làm mờ nút, chứ đừng để người chơi bấm rồi nhận ngoại lệ.

Hai mã lỗi `ValidateForEndTurn` trả về:

| `PlanError` | Nghĩa |
| --- | --- |
| `GatherFloorNotMet` | chưa cử đủ người đi Kiếm ăn |
| `SiteNotChosen` | có người đi Xây dựng mà chưa chọn slot (§14) |

**Cửa thoát.** Sàn chặn khi người chơi **có thể** đạt mà **chọn không**. Nó **không** chặn khi họ **không thể** — ví dụ Kiếm ăn đang bị khoá, hoặc năng lượng tối đa của giai đoạn còn ít hơn chi phí của chính hoạt động Kiếm ăn. `CanPossiblyMeetGatherFloor` là chỗ đọc điều đó; `ValidateForEndTurn` đã tự xét, UI không phải tự kiểm lần nữa.

---

## 14. `SiteSystem` — slot công trình (v7 §B)

Thay hẳn cơ chế điểm xây của `design.md` §5. **Không còn `build_points`.**

```csharp
SiteSystem sites = turn.Sites;                         // null nếu ván dựng không có sites.json

List<SiteView> VisibleSites(int stage, IPurchasedNodes nodes);
SiteView       Find(string siteId);                    // null nếu không có
int            ClearedCount;                           // = state.BuildLevel
double         StrengthOf(IReadOnlyList<int> memberIds);
bool           WouldClear(string siteId, IReadOnlyList<int> memberIds);   // truy vấn thuần
bool           IsCleared(string siteId);
double         SpeedThresholdRelief;                   // bậc Ổ hiện tại nới bao nhiêu
SiteCatalog    Catalog;
```

`SiteView` có đúng những trường nhánh UI đã khai trong `ISiteSystem`: `Id`, `Kind`, `StrengthRequired`, `RequiresNodeId`, `Stage`, `Cleared`, `Tier`, `MaxTier`, `ArtKey`. **Không có trường tên và mô tả** — nội dung hiện cho người chơi nằm ở `strings.vi.json` theo khóa `site.<id>.name`.

> ⚠ **`SiteView` là BẢN CHỤP, không phải tay cầm.** Mỗi lần gọi `Find` hay `VisibleSites` là một vật mới. Gán `view.Tier = 2` không đổi gì cả — trạng thái thật nằm ở `GameState.FoodNestTier` và `GameState.ClearedSiteIds`, và chỉ `SiteSystem.Resolve` mới đổi được chúng.

### Một lượt Xây dựng

```csharp
turn.Plan.Assign(ActivityKind.Build, memberIds);
PlanResult r = turn.Plan.ChooseSite("hoc_da_tru_mua");   // SiteNotAvailable nếu chưa hiện ra

bool se = turn.Plan.WouldClearSite;      // hiện "sẽ dọn xong" hay "chưa đủ sức"
turn.EndTurn();                          // NÉM nếu có người đi Xây mà chưa chọn slot
```

Sau lượt, `outcome.Site` là `SiteResolution { Attempted, Cleared, SiteId, Strength, Required, TierAfter }`.

### Ba luật dễ sai

1. **Ăn cả ngã về không** (D7 của Khoa). Đủ **tổng sức mạnh** của nhóm thì dọn xong ngay; thiếu thì lượt đó phí, **không tích luỹ gì**. Không có thanh tiến trình, và hai lượt thiếu không cộng lại thành một lượt đủ.
2. **Xây dựng giải quyết CUỐI lượt** (D19). Slot vừa dọn **không** nâng tỉ lệ thắng trận của chính lượt đó, cũng **không** nới sức chứa hang kịp cho lần sinh cuối lượt. Hiệu lực bắt đầu từ lượt sau.
3. **`build_level` là số suy ra** (D18): `SitesClearedAtStart + ClearedSiteIds.Count + FoodNestTier`. Đừng tìm setter, không có. Mỗi bậc Ổ đựng thức ăn cũng tính là +1.

### Ổ đựng thức ăn

Là một `SiteView` như mọi slot khác, nhưng **nâng cấp nhiều bậc**: `Tier` là bậc hiện tại, `MaxTier` là bậc cuối, và `StrengthRequired` là ngưỡng của **bậc tiếp theo**. Hết bậc thì `Cleared = true` và nó biến mất khỏi `VisibleSites`.

Mỗi bậc nới ngưỡng `guaranteed_total_speed` của **mọi loài thực vật** (D17). Core cài bằng cách cộng vào tổng nhanh nhẹn của nhóm; UI chỉ cần hiện `turn.SpeedThresholdRelief`.

### "Slot" là CHỖ CHO NGƯỜI, không phải ô đất đặt công trình

Đây là chỗ dễ hiểu nhầm nhất của cả §B. Khoa, 2026-11-09: *"vùng 1 có tối đa 8 chỗ, khởi đầu là 4 chỗ, cần phải thực hiện dọn chỗ cho 4 chỗ còn lại"*.

- **Slot = sức chứa hang.** Mỗi vùng có một trần và một số chỗ mở sẵn.
- **Dọn một công trình mở thêm đúng một chỗ** ([ĐỀ XUẤT]), tới trần của vùng.
- Nên số công trình của một vùng = `capacity_max − capacity_open_at_start`. `SiteCatalog` **nổ lúc nạp** nếu hai số đó không khớp.

```csharp
int cap = state.LairCapacity(config);   // = RegionCapacityOpen + ClearedSiteIds.Count, kẹp ở RegionCapacityMax
```

Công thức `6 + 2 × build_level` của `design.md` §4 **đã chết**. `balance.json` không còn khối `lair`.

---

## 15. Di cư (v7 §F.2) — thắng một trận có thể giết ván

Khoa, 2026-11-09, ba câu nguyên văn:

> *"người ở lại mất luôn, đây là cơ chế giảm sức mạnh người chơi ở khu vực cũ"*
> *"Miễn khu vực mới có đủ slot cho số người di cư thì bao nhiêu người trong bộ tộc đều được lựa chọn đi được"*
> *"cộng đồng ở vùng đất mới chính là cộng đồng chính, nó là lối đi 1 chiều không có đường trở lại"*

**Thắng trận Khám phá LÀ di cư.** Không có bước xác nhận riêng, không có đường quay lại, và **người không được chọn mất hẳn**.

### Truy vấn xem trước — bắt buộc dùng

```csharp
MigrationPreview p = turn.PreviewMigration();   // truy vấn thuần, KHÔNG tác dụng phụ

p.HasNextRegion;   // false = hết vùng, thắng Khám phá chỉ là một trận thắng bình thường
p.NextRegion;
p.CapacityOpen;    // số chỗ vùng mới mở sẵn = trần số người mang theo được
p.Crew;
p.Chosen;          // số người đã chọn; 0 = chưa chọn
p.LeftBehind;      // số người SẼ MẤT HẲN nếu thắng
```

Cũng đọc được qua `turn.Plan.MigrationCapacity` và `turn.Plan.MembersLeftBehindIfMigrating`.

> Vì sao Core phơi con số này thay vì để UI tự tính: nó là **luật của Khoa**, không phải cách diễn đạt. Người chơi cử 3 người đi khám phá, thắng, rồi phát hiện phải bỏ lại 4 người và còn đúng 3 — giao với ngưỡng thua `≤ 1` thì ván gần như xong. Luật đó hợp lệ, nhưng nó phải **nhìn thấy được trước khi bấm**; nếu không người chơi sẽ gọi nó là lỗi chứ không gọi là luật.

### Chọn đoàn — bắt buộc, nếu không thì không kết thúc lượt được

```csharp
turn.Plan.Assign(ActivityKind.Explore, memberIds);
turn.Plan.ChooseMigrationParty(idsToTakeAlong);   // ≤ p.CapacityOpen người
```

Chưa chọn thì `ValidateForEndTurn()` trả `PlanError.MigrationPartyNotChosen` và `EndTurn()` ném. Chọn quá số chỗ thì `MigrationPartyTooLarge`.

Core **không** bắt đoàn di cư phải chứa nhóm đi Khám phá — ai đi ai ở là quyết định của người chơi ([ĐỀ XUẤT], chờ Khoa bác nếu muốn chặt hơn).

### Chuyện gì xảy ra ở cuối lượt

Di cư là **việc cuối cùng** của lượt, sau cả ăn lẫn sinh. Cả bộ tộc — kể cả những người sắp bị bỏ lại — vẫn ăn phần của lượt đó, và công trình dọn trong lượt vẫn thuộc về vùng cũ.

```csharp
outcome.Migrated;                // = RegionAfter > RegionBefore
outcome.LeftBehindMemberIds;     // KHÔNG nằm trong DeadMemberIds
```

Ba điều dễ sai:

1. **Người bị bỏ lại KHÔNG phải người chết.** Không trừ tinh thần theo `morale.on_member_death`, không vào `DeadMemberIds`. UI nên nói về họ bằng giọng khác với giọng nói về người chết.
2. **`build_level` tụt về 0** khi di cư (trừ bậc Ổ đựng thức ăn): công trình thuộc về vùng cũ. Đó là chủ ý — Khoa gọi di cư là *"cơ chế giảm sức mạnh người chơi"*. Sức chứa hang cũng nhận số của vùng mới.
3. **Người sinh ra trong chính lượt đó được đi theo** nếu còn chỗ, dù người chơi không thể chọn nó — lúc chọn thì nó chưa tồn tại.
