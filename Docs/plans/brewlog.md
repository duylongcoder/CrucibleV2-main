# Plan: BrewLog + phát lại, timescale, chạm để tua (F-004)

> **Trạng thái**: Draft (chờ duyệt) · **Cập nhật**: 2026-11-04
> **GDD**: `design/brewing.md` §Nhật ký lượt nung & phát lại, §Formulas (Thời lượng phát lại), AC-7, AC-11; GDD §6.3
> (meta: timescale ưa thích); `design/ui-ban-luyen.md` §5, §4 "Khi phát lại" · **ADR**: ADR-001, ADR-004, ADR-007 ·
> **Loại**: Logic (Core) + Logic thuần Presentation (Runtime, không MonoBehaviour)
> Xây trên F-002 + F-003 (196/196 EditMode xanh, **chưa commit** lúc viết plan).

## 1. Bối cảnh

Lượt Nung tính xong ngay; phần hiển thị phải phát lại từng nhịp để người chơi "nghe" chuỗi mình xếp (P1), và tua
được để tôn trọng thời gian (P4). Hiện `BrewingRules.Brew` xóa lưới và chỉ để lại `LastBrewResultUid` — mất thông tin
cần cho phát lại (vật phẩm đã tiêu hao). Plan này thêm `BrewLog` (bản chụp chuỗi nạp + kết quả, không rò rỉ công
thức), timescale lưu trong meta, và mô hình thời gian phát lại thuần C# cho Presentation.

**Phạm vi (đề xuất — người dùng xác nhận khi duyệt)**: Runtime hiện **trống** (chưa `GameSession`, `GameHost`, scene,
màn Bàn luyện). View phát lại (highlight ô, lò bùng, chạm vùng lò để tua, nút 1×/2×/3×, phím tắt) cần màn Bàn luyện
→ **chuyển sang F-014** (greybox UI); F-004 làm phần dữ liệu + luật + đồng hồ phát lại để view chỉ việc đọc.

**Yêu cầu** (đánh số để truy vết):
- R1: `Brew` được chấp nhận → tạo `BrewLog` từ lưới **trước khi tiêu hao**: chỉ các tick từ ô có vật phẩm đầu tiên
  tới ô có vật phẩm cuối cùng (`FirstCell`, `Ticks`); mỗi tick = "nạp X" (`ItemId`, `Magic`) hoặc "chờ" (ô trống ở
  giữa). Ô trống trước vật phẩm đầu / sau vật phẩm cuối không có trong log.
- R2: `BrewLog` có **một** kết quả cuối: thành phẩm (`ItemId`, `Magic`) hoặc phế phẩm (`IsScrap`).
- R3: `BrewLog` **không** chứa `Recipe` / `Pattern` / slot / khoảng cách công thức (ADR-007); phần tick chỉ phụ thuộc
  lưới → cùng lưới thì khớp và hỏng có `Ticks` trùng nhau (AC-7).
- R4: `RunState.LastBrew` giữ log của lượt Nung gần nhất (lưu cùng save — TECH §4 "log đang phát"); `BrewCompleted`
  mang cùng log; `StartRun` đặt `null`; Command bị từ chối không đổi `LastBrew`. `StateHasher` ghi `LastBrew`.
- R5: `MetaState.Timescale` ∈ {1, 2, 3}, mặc định 1 (GDD §6.3 "timescale ưa thích"). Command `SetTimescale(n)`:
  n ngoài 1–3 → `Rejected(InvalidTimescale)`, không đổi gì; hợp lệ → lưu + sự kiện `TimescaleChanged`. Chấp nhận ở
  **mọi** thời điểm (kể cả chưa có run, đang `ResultPending`/phát lại — ui-ban-luyen §5 "luôn chọn được"); không rút
  random. `StateHasher` ghi `Timescale`.
- R6: `BrewReplayTimeline` (Runtime, thuần C#): phát **mọi** tick của log theo thứ tự, mỗi tick
  `BrewTickSeconds / timescale` giây; không dừng sớm; `IsFinished` (= được hiện kết quả) chỉ khi tick cuối đã chạy
  hết. Thời lượng = `TickCount × BrewTickSeconds / timescale` (brewing.md §Formulas: lưới đầy 27 s ở 1×, 9 s ở 3×).
- R7: Đổi timescale **giữa** phát lại → giữ tiến độ, đổi tốc độ từ lúc đó.
- R8: Chạm để tua = `Skip()` → `IsFinished` ngay. Tua là hành động **chỉ Presentation**, không phải Command: kết quả
  đã tính + lưu lúc Nung, nên state sau tua ≡ state sau xem hết (AC-11).

**Ngoài phạm vi**: view MonoBehaviour phát lại, input chạm vùng lò / phím Space / 1-2-3, SO `BrewTickSeconds`
(knob "config Presentation"), âm nạp theo tick, "Giảm chuyển động", overlay kết quả → **F-014**; `AckBrewResult`
(Result → TurnEnd, F-009); "Đã ghi vào Sổ công thức" (F-012); `GameSession`/save DTO cho `LastBrew` + `Timescale`
(F-013 — DTO phải mang 2 field này); `MeltTicks` (chỉ hiển thị, F-014).
**TBD (không làm)**: không có.

**Lệch so với ui-ban-luyen §2** (tên lệnh tạm, "chốt ở `arch`"): `SkipReplay` **không** là Command Core (R8) —
ghi vào TECH §3.3 ở Bước 5.

## 2. Thiết kế triển khai

| Class / file (dưới `Assets/_Project/Scripts/`) | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Logs/BrewLog.cs` | bản ghi bất biến: `FirstCell`, `Ticks`, `Result`; factory `Create(grid, result, isScrap)` chụp lưới | Core | Mới |
| `Core/Logs/BrewTick.cs` | `readonly struct`: `Cell`, `HasItem`, `ItemId`, `Magic` | Core | Mới |
| `Core/Logs/BrewLogResult.cs` | `readonly struct`: `IsScrap`, `ItemId`, `Magic` | Core | Mới |
| `Core/State/RunState.cs` | + `BrewLog LastBrew` (null = chưa có) | Core | Sửa |
| `Core/State/MetaState.cs` | + `int Timescale` (khởi tạo 1) | Core | Sửa |
| `Core/State/StateHasher.cs` | ghi `LastBrew` (sau `LastBrewResultUid`), `Timescale` (sau `Renown`) | Core | Sửa |
| `Core/Rules/BrewingRules.cs` | tạo log **trước** `Array.Clear(Grid)`; gán `run.LastBrew`; đưa log vào `BrewCompleted` | Core | Sửa |
| `Core/Rules/RunStartRules.cs` | `run.LastBrew = null` | Core | Sửa |
| `Core/Commands/BrewCompleted.cs` | + `BrewLog Log` | Core | Sửa |
| `Core/Commands/SetTimescale.cs` | Command `SetTimescale(int timescale)` | Core | Mới |
| `Core/Commands/TimescaleChanged.cs` | sự kiện (`Timescale`) | Core | Mới |
| `Core/Commands/RejectReason.cs` | + `InvalidTimescale = 12` | Core | Sửa |
| `Core/Rules/SettingsRules.cs` | `MinTimescale = 1`, `MaxTimescale = 3`; `SetTimescale(meta, cmd)` | Core | Mới |
| `Core/Rules/GameRules.cs` | dispatch `SetTimescale` | Core | Sửa |
| `Runtime/Replay/BrewReplayTimeline.cs` | đồng hồ phát lại thuần C# (float giây — Presentation, ngoài ràng buộc Core) | Runtime | Mới |

**Public / internal API**
```csharp
namespace Crucible.Core.Logs {
  public readonly struct BrewTick { public int Cell { get; } public bool HasItem { get; } public ItemId ItemId { get; } public bool Magic { get; } }
  public readonly struct BrewLogResult { public bool IsScrap { get; } public ItemId ItemId { get; } public bool Magic { get; } }
  public sealed class BrewLog {
    public int FirstCell { get; }
    public IReadOnlyList<BrewTick> Ticks { get; }          // Ticks[i].Cell == FirstCell + i
    public BrewLogResult Result { get; }
    /// grid: 9 ô (null = trống), ≥ 1 vật phẩm, nếu không ArgumentException. Chụp giá trị — sửa grid sau đó không đổi log.
    public static BrewLog Create(IReadOnlyList<ItemInstance> grid, ItemInstance result, bool isScrap);
  }
}
namespace Crucible.Core.Commands { public sealed class SetTimescale : ICommand { public SetTimescale(int timescale); public int Timescale { get; } } }
namespace Crucible.Core.Rules {
  public static class SettingsRules {
    public const int MinTimescale = 1, MaxTimescale = 3;
    public static CommandResult SetTimescale(MetaState meta, SetTimescale command);
  }
}
namespace Crucible.Runtime.Replay {
  public sealed class BrewReplayTimeline {
    public BrewReplayTimeline(int tickCount, float tickSeconds, int timescale);   // tickCount ≥ 1, tickSeconds > 0, timescale 1–3
    public int TickCount { get; }
    public int CurrentTick { get; }        // 0…TickCount−1 (giữ TickCount−1 khi xong)
    public bool IsFinished { get; }        // được hiện kết quả
    public int Timescale { get; }
    public void SetTimescale(int timescale);   // giữ tiến độ (R7)
    public void Advance(float deltaSeconds);   // delta < 0 → ArgumentOutOfRangeException
    public void Skip();                        // R8
    public static float DurationSeconds(int tickCount, float tickSeconds, int timescale);
  }
}
```
- Tiến độ của timeline lưu theo **đơn vị tick** (`_progressTicks += delta × timescale / tickSeconds`) → đổi timescale
  giữa chừng không nhảy tick. `CurrentTick = min(⌊progress⌋, TickCount − 1)`; `IsFinished = progress ≥ TickCount`.
- `BrewingRules.Brew`: `isScrap = recipe == null`; log tạo sau khi có `result`, trước khi xóa lưới.
- `BrewCompleted(resultUid, log)`; `LastBrewResultUid` **giữ nguyên** (không gộp vào log trong feature này — tách
  refactor; cân nhắc khi làm F-009 `AckBrewResult`).

**Interface phụ thuộc**: không có mới — Core vẫn static; timeline nhận `deltaSeconds` từ view (không đọc `Time`).
**Dữ liệu**: không SO mới. Timescale 1–3 là hằng thiết kế (brewing.md Tuning: "1×/2×/3× — khoảng an toàn —"),
`BrewTickSeconds` (3 s) là tham số constructor; SO giữ nó tạo cùng view ở F-014.
**Scene / prefab bị chạm**: không. Runtime asmdef có script đầu tiên → lần compile đầu của `Crucible.Runtime`.
**Lý do thiết kế**:
- Log trong `RunState` (không chỉ trong event) để save/resume và `StateHash` thấy được; resume giữa phát lại vào
  Result (brewing.md Edge Cases) nhưng overlay kết quả vẫn cần log.
- Log chỉ chứa phạm vi được phát (đầu → cuối) → view không cần tự cắt, và không lưu gì ngoài thứ người chơi tự xếp.
- Tua không là Command → không lưu, không hash, không thể làm lệch replay; khớp TECH "timescale thuần hiển thị".
- Timescale là Command trên `MetaState` (không phải biến view) vì phải nhớ qua phiên (GDD §6.3) và đi qua save.

## 3. Test cases

Vị trí: `Assets/Tests/EditMode/Core/Logs/`, `.../Core/Rules/`, `.../Runtime/Replay/` — assembly
`Crucible.Tests.EditMode` (đã tham chiếu Runtime). Lưới trong test dựng bằng `StateFactory.CreateWorkbenchRun`
(null = ô trống).

### Editor tests
(none)

### Unit tests
#### BrewLogTest (`Core/Logs/BrewLogTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Create_ItemsWithLeadingAndTrailingEmptyCells_TicksSpanFirstToLastItem` | `FirstCell` = ô vật phẩm đầu; số tick = cuối − đầu + 1 |
| `Create_EmptyCellBetweenItems_RecordsWaitTick` | tick đó `HasItem` false |
| `Create_ItemCell_RecordsItemIdAndMagic` | tick có `ItemId`, `Magic` của vật phẩm |
| `Create_TicksInOrder_CellsAreConsecutive` | `Ticks[i].Cell` = `FirstCell + i` |
| `Create_SingleItem_HasOneTick` (cell: {0, 4, 8}) | 1 tick, `FirstCell` = ô đó |
| `Create_FullGrid_HasNineTicks` | 9 tick, `FirstCell` 0 |
| `Create_ProductResult_RecordsProductAndMagic` | `IsScrap` false; `ItemId`, `Magic` của thành phẩm |
| `Create_ScrapResult_MarksScrap` | `IsScrap` true |
| `Create_GridModifiedAfterCreate_LogUnchanged` | tick giữ giá trị lúc tạo |
| `Create_EmptyGrid_ThrowsArgumentException` | ném `ArgumentException` |
| `Create_GridNotNineCells_ThrowsArgumentException` | ném `ArgumentException` |
| `LogTypes_PublicMembers_ExposeNoRecipeTypes` | không member nào của `BrewLog`/`BrewTick`/`BrewLogResult` có kiểu `Recipe`/`RecipeSlot` (hoặc tập hợp của chúng) |

#### SettingsRules — qua `GameRules.Apply` (`Core/Rules/GameRulesSetTimescaleTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `NewMetaState_Timescale_IsOne` | 1 |
| `Apply_SetTimescaleInRange_StoresTimescale` (1, 2, 3) | `meta.Timescale` = giá trị |
| `Apply_SetTimescaleInRange_EmitsTimescaleChanged` | 1 sự kiện `TimescaleChanged` mang giá trị mới |
| `Apply_SetTimescaleOutOfRange_RejectsWithInvalidTimescale` (0, 4, −1) | `Rejected(InvalidTimescale)` |
| `Apply_SetTimescaleOutOfRange_StateHashUnchanged` | `StateHash` trước = sau |
| `Apply_SetTimescaleWithoutActiveRun_Accepted` | chấp nhận |
| `Apply_SetTimescaleWhileResultPending_Accepted` | chấp nhận; lưới / `WorkbenchStep` không đổi |
| `Apply_SetTimescale_RunRngUnchanged` | trạng thái 7 stream không đổi |

#### BrewReplayTimelineTest (`Runtime/Replay/BrewReplayTimelineTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `DurationSeconds_FullGrid_MatchesFormula` (timescale 1 → 27, 2 → 13.5, 3 → 9) | thời lượng (s) |
| `New_Timeline_AtFirstTickNotFinished` | `CurrentTick` 0; chưa xong |
| `Advance_LessThanOneTick_StaysOnFirstTick` | `CurrentTick` 0 |
| `Advance_OneTickAtATime_VisitsEveryTickInOrder` | dãy `CurrentTick` = 0, 1, …, n−1 |
| `Advance_JustBeforeEnd_NotFinished` | chưa xong; `CurrentTick` = n−1 |
| `Advance_ExactlyFullDuration_Finished` | xong |
| `Advance_PastEnd_StaysFinishedOnLastTick` | xong; `CurrentTick` = n−1 |
| `Advance_Timescale3_FinishesInOneThirdOfDuration` | xong sau `Duration(1×)/3` |
| `SetTimescale_MidReplay_KeepsTickAndChangesRate` | tick không đổi lúc đổi; thời gian còn lại đúng tốc độ mới |
| `Skip_MidReplay_FinishedImmediately` | xong |
| `Skip_BeforeAdvance_Finished` | xong |
| `Constructor_InvalidArguments_Throws` (tickCount 0 · tickSeconds 0 · timescale 0 · timescale 4) | ném `ArgumentOutOfRangeException` |
| `Advance_NegativeDelta_Throws` | ném `ArgumentOutOfRangeException` |
| `SetTimescale_OutOfRange_Throws` (0, 4) | ném `ArgumentOutOfRangeException` |

### Integration tests (Core, qua `GameRules.Apply`)
#### GameRulesBrewLogTest (`Core/Rules/GameRulesBrewLogTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Apply_BrewMatchingGrid_StoresLogWithProductResult` | `LastBrew` ≠ null; kết quả = thành phẩm (`IsScrap` false, đúng `ItemId`, `Magic` trùng `ItemInstance` trong Túi) |
| `Apply_BrewNonMatchingGrid_StoresLogWithScrapResult` | `IsScrap` true |
| `Apply_BrewGridWithGap_LogTicksMatchGridBeforeBrew` | `FirstCell`, tick nạp/chờ đúng lưới đã xếp (`[_, Sắt, _, Than]` → 3 tick: Sắt, chờ, Than) |
| `Apply_BrewSameGridMatchedAndScrap_TicksIdentical` (AC-7) | `Ticks` + `FirstCell` trùng nhau; chỉ `Result` khác |
| `Apply_Brew_EventCarriesStoredLog` | `BrewCompleted.Log` là cùng đối tượng `LastBrew` |
| `Apply_BrewRejected_LastBrewUnchanged` (lưới trống · đang `ResultPending`) | `LastBrew` giữ giá trị cũ |
| `Apply_BrewTwice_LastBrewIsLatest` | `LastBrew` là log của lượt sau |

AC-7 dựng "cùng lưới, khớp vs hỏng" bằng 2 `ContentDb` có **cùng tập Id** (để `ItemId` trùng chỉ số): content mặc
định và biến thể Thép **không** có công thức → thêm tham số `ContentFactory.Create(bool steelHasRecipe = true)`.

#### Sửa test hiện có
| Test | Thay đổi |
|---|---|
| `GameRulesStartRunTest` (test reset bước Bàn luyện, dòng ~82) | đặt `_run.LastBrew` khác null trước → sau `StartRun` là `null` |
| `StateHasherTest` | + mutation `"last brew log"` vào bảng `Mutations` (chỉ cho `RunState`); + test mới `Hash_MetaTimescaleDiffers_ReturnsDifferentValue` (meta chưa có bảng mutation) |

### Visual verification tests
(none — view phát lại ở F-014; ảnh `Docs/evidence/ui-ban-luyen/` phát lại theo ui-ban-luyen §8)

### Manual tests
(none trong feature này — feel nhịp 3 s, độ đọc của highlight, tua: kiểm ở F-014 / playtest AC-13)

**Truy vết**: R1 → `BrewLogTest.Create_ItemsWithLeading…`, `…RecordsWaitTick`, `…RecordsItemIdAndMagic`,
`…CellsAreConsecutive`, `…SingleItem…`, `…FullGrid…`, `GameRulesBrewLogTest.Apply_BrewGridWithGap_…`; R2 →
`Create_ProductResult_…`, `Create_ScrapResult_…`, `Apply_BrewMatchingGrid_…`, `Apply_BrewNonMatchingGrid_…`; R3 →
`LogTypes_PublicMembers_ExposeNoRecipeTypes`, `Apply_BrewSameGridMatchedAndScrap_TicksIdentical`; R4 →
`Apply_Brew_EventCarriesStoredLog`, `Apply_BrewRejected_…`, `Apply_BrewTwice_…`, `GameRulesStartRunTest` (sửa),
`StateHasherTest` (sửa), `Create_GridModifiedAfterCreate_…`; R5 → toàn bộ `GameRulesSetTimescaleTest`, `StateHasherTest`
(Timescale); R6 → `DurationSeconds_…`, `New_…`, `Advance_*`; R7 → `SetTimescale_MidReplay_…`; R8 → `Skip_*`
(+ thiết kế: tua không có Command nên không thể đổi state).
**Testability**: **PASS** — mọi thứ là C# thuần (static rules / class không MonoBehaviour), input inject được (lưới,
`deltaSeconds`), không đồng hồ thật, không random mới. Lưu ý nhỏ: so sánh float trong timeline dùng
`Is.EqualTo(x).Within(1e-4f)`; test "đúng biên" dùng delta là bội số chính xác nhị phân (vd `tickSeconds = 2`, delta 2/1/0.5)
để tránh sai số cộng dồn.

**Ước lượng số test**: ~12 `BrewLogTest` + ~13 `GameRulesSetTimescaleTest` (gồm case tham số) + ~19
`BrewReplayTimelineTest` + ~8 `GameRulesBrewLogTest` + 2–3 sửa → khoảng **50 test mới**; tổng dự kiến ~246.

## 4. Các bước thực hiện

> Commit theo chính sách `CLAUDE.md` §Git (**không** tự commit; test-first: không → test gộp vào commit `feat`).
> Lệnh test (Linux, Editor đóng):
> `"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml -logFile -`
> Một class: thêm `-testFilter Crucible.Tests.Core.Logs.BrewLogTest` (v.v.).

- [ ] **Bước 0 — Tiền điều kiện**: người dùng commit F-002/F-003 đang dở (`git status` sạch phần Core) để diff F-004 tách biệt.
- [ ] **Bước 1 — Skeleton**: tạo `BrewLog`, `BrewTick`, `BrewLogResult`, `SetTimescale`, `TimescaleChanged`,
  `SettingsRules`, `BrewReplayTimeline` (thân rỗng / trả mặc định / `throw new NotImplementedException()` cho
  `BrewLog.Create`), thêm field `LastBrew`, `Timescale` (chưa khởi tạo 1), `InvalidTimescale`, tham số
  `BrewCompleted(resultUid, log)` (truyền `null`), dispatch `SetTimescale` → gọi skeleton. **Kiểm chứng**: chạy EditMode
  → compile sạch (cả `Crucible.Runtime` lần đầu có script), 196/196 cũ vẫn xanh.
- [ ] **Bước 2 — Test first**: viết test §3 + sửa 2 test hiện có + tham số `ContentFactory.Create(steelHasRecipe)`.
  **Kiểm chứng**: chạy EditMode → test mới **fail đúng lý do** (log null / `NotImplementedException` / giá trị mặc
  định / timeline không tiến). Dự đoán pass tầm thường ở skeleton: `Apply_SetTimescaleWithoutActiveRun_Accepted` (nếu
  skeleton trả Accept) — ghi lại thực tế.
- [ ] **Bước 3 — Implement**: `BrewLog.Create` (validate → chụp tick → kết quả), `BrewingRules` (log trước
  `Array.Clear`), `RunStartRules`, `StateHasher` (2 field, thứ tự cố định), `SettingsRules`, `MetaState.Timescale = 1`,
  `BrewReplayTimeline`. **Kiểm chứng**: EditMode toàn bộ xanh; 0 warning CS mới (`grep "warning CS" Logs/…` trên log
  batchmode).
- [ ] **Bước 4 — Refactor**: gộp test cùng phân vùng thành `[TestCase]`, bỏ trùng helper dựng lưới; code không đổi
  hành vi. **Kiểm chứng**: EditMode xanh, cùng số test.
- [ ] **Bước 5 — Bằng chứng & tài liệu**: `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .`
  0 lỗi (`.meta` mới do Unity sinh); TECH §3 (bảng: `BrewLog` Implemented, `Timeline` ở Runtime), §3.3 (+ `SetTimescale`,
  `SkipReplay` không là Command), §3.5 (cấu trúc `BrewLog`); brewing.md AC-7, AC-11 tick (phần Core); PLAN F-004 →
  Done (Core) + ghi chú view → F-014; F-013 ghi chú DTO mang `LastBrew` + `Timescale`; LOG.

Đề xuất commit sau Bước 3/4 xanh: `feat(brewing): add BrewLog, timescale setting and replay timeline`.

## 5. Rủi ro & đánh đổi

- **View tách sang F-014**: F-004 "Done" chưa có gì nhìn thấy được; AC-11 phần "hiện kết quả ngay" chỉ kiểm được khi có
  view. Chấp nhận: đồng hồ phát lại đã test ở mức logic, view chỉ đọc `CurrentTick`/`IsFinished`.
- **Float trong Runtime timeline**: không ảnh hưởng tất định (không vào state/hash/save); chỉ quyết định khung hình nào
  đổi tick.
- **`LastBrew` vs `LastBrewResultUid` trùng một phần** (uid kết quả suy ra được từ Túi): giữ cả hai để không trộn refactor
  vào feature; xem lại ở F-009.
- **Kích thước save**: log ≤ 9 tick + 1 kết quả → không đáng kể.
- **Runtime asmdef compile lần đầu** (tham chiếu Input System / uGUI / TMP): nếu thiếu package sẽ lộ ở Bước 1 — sửa
  asmdef thuộc `setup`, báo người dùng trước.
- **Rò rỉ qua `Result.ItemId`**: kết quả là thứ người chơi được thấy (thành phẩm/phế phẩm), không lộ khoảng cách; vẫn
  giữ test reflection R3 làm hàng rào.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**: - None
- **Deviations** (lệch so với plan và lý do): - None
- **Tradeoffs**: - None
- **Test changes** (sửa test đã commit ở Bước 2 và lý do): - None
- **Open questions**: - None
