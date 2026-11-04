# Plan: Core foundation + Seed & RNG (F-000, F-001)

> **Trạng thái**: Implemented (duyệt khi gọi `build core-foundation`) · **Cập nhật**: 2026-11-04
> **GDD**: `GDD.md` §6.1, `design/run-flow.md` §Khởi đầu run · **ADR**: ADR-001, ADR-004, ADR-005 · **Loại**: Logic
> **TECH**: §3.1–3.5 (phần nền). Brewing (F-002) là plan kế tiếp, xây trên nền này.

## 1. Bối cảnh

Mọi hệ thống gameplay chạy trong lõi C# thuần (`Crucible.Core`) qua Command → `GameRules.Apply` (ADR-001), tất định
bằng PCG32 + 7 stream và số nguyên (ADR-004). Plan này dựng **nền móng** để các plan sau (Brewing, Market, Battle…)
chỉ thêm luật: RNG, kiểu content tối thiểu, state, pipeline Command, state hash, và command đầu tiên `StartRun`.
Pillar: P2 (luật tất định, đọc được).

**Yêu cầu** (đánh số để truy vết):
- R1: `Pcg32` cho chuỗi giống hệt PCG32 tham chiếu (seed 42, seq 54).
- R2: `NextInt(max)` trả giá trị trong `[0, max)`, phân phối đều (không lệch modulo); `max ≤ 0` → `ArgumentOutOfRangeException`.
- R3: `Roll(percent)`: 0 → luôn false, 100 → luôn true; **luôn tiêu thụ đúng 1 lần rút** ("mọi điều kiện `Chance` luôn
  được tung" — economy.md); `percent` ngoài `[0, 100]` → `ArgumentOutOfRangeException`.
- R4: `RngStreams` có 7 stream (`Shop, Chance, Inspect, Forge, Market, Season, Reward`) seed bằng SplitMix64 từ seed run;
  cùng seed → cùng chuỗi; rút ở một stream **không** đổi stream khác (nền của GDD §6.1 AC-3); seed 0 hợp lệ.
- R5: Trạng thái RNG chụp / khôi phục được (cho save) và tiếp tục y hệt.
- R6: `ContentDb`: `ItemId` gán theo thứ tự sắp **ordinal** của `Id` chuỗi (không phụ thuộc thứ tự đầu vào); tra `Id`
  chuỗi; `Id` trùng → `ArgumentException`.
- R7: `GameRules.Apply(StartRun)` hợp lệ → Mùa 1, phase `Decree`, vàng 0, Uy tín và sức chứa Túi từ `RunData`, Túi =
  vật phẩm bộ khởi đầu theo thứ tự, `Uid` tăng dần (run-flow §Khởi đầu run).
- R8: Command bị từ chối → state (gồm RNG) không đổi; lý do: `UnknownContent`, `RunAlreadyActive`, `UnknownCommand`.
- R9: `StateHasher`: state bằng nhau → cùng hash; khác bất kỳ field gameplay nào (kể cả thứ tự Túi, trạng thái RNG) →
  khác hash; không phụ thuộc chi tiết bộ nhớ (capacity của `List`). Cùng seed + cùng Command → cùng hash.
- R10: `Crucible.Core` không tham chiếu UnityEngine; type state không có field `float`/`double` (ADR-004).

**Ngoài phạm vi**: Brewing/RecipeMatcher (F-002), Market (F-006), Inspect (F-008), Season/Decree rút ngẫu nhiên (F-009),
Battle (F-017), `RunView` (dựng ở F-002 khi có vật phẩm/dòng thuộc tính), `GameSession` + save JSON (F-013),
`ContentBaker` SO (làm khi có content thật — test dựng `ContentDb` trực tiếp).
**TBD (không làm)**: không có.

## 2. Thiết kế triển khai

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Random/SplitMix64.cs` | `static ulong Next(ref ulong state)` | Core | Mới |
| `Core/Random/Pcg32.cs` | struct PRNG: `NextUInt`, `NextInt`, `Roll`, `State` | Core | Mới |
| `Core/Random/Pcg32State.cs` | struct `{ ulong State; ulong Inc; }` (save) | Core | Mới |
| `Core/Random/RngStream.cs` | enum 7 stream | Core | Mới |
| `Core/Random/RngStreams.cs` | giữ 7 `Pcg32`; `Create(seed)`, `Get(stream)` (ref), `Capture()`, `Restore()` | Core | Mới |
| `Core/Content/ItemId.cs`, `ItemKind.cs`, `ItemData.cs` | id + dữ liệu tối thiểu (Id, Kind, Tier) | Core | Mới |
| `Core/Content/DifficultyData.cs`, `StarterKitData.cs`, `RunData.cs` | dữ liệu khởi đầu run | Core | Mới |
| `Core/Content/ContentDb.cs` | `Create(...)`, `GetItem`, `TryFindItem`, `TryFindDifficulty`, `TryFindStarterKit`, `Run` | Core | Mới |
| `Core/State/RunState.cs`, `RunPhase.cs`, `ItemInstance.cs`, `MetaState.cs` | state run / meta (field nền) | Core | Mới |
| `Core/State/CanonicalWriter.cs`, `StateHasher.cs` | ghi nhị phân chuẩn hóa + FNV-1a 64 | Core | Mới |
| `Core/Commands/ICommand.cs`, `StartRun.cs`, `CommandResult.cs`, `RejectReason.cs` | pipeline Command | Core | Mới |
| `Core/Commands/GameEvent.cs`, `RunStarted.cs` | sự kiện | Core | Mới |
| `Core/Rules/GameRules.cs` | `Apply` dispatch theo type, validate-trước-mutate-sau | Core | Mới |
| `Core/Rules/RunStartRules.cs` | luật `StartRun` | Core | Mới |

**Public API**
```csharp
public static class SplitMix64 { public static ulong Next(ref ulong state); }
public struct Pcg32 {
    public Pcg32(ulong seed, ulong sequence);
    public uint NextUInt();
    public int NextInt(int maxExclusive);          // Lemire, không lệch
    public bool Roll(int percent);                 // luôn rút đúng 1 lần
    public Pcg32State State { get; set; }
}
public sealed class RngStreams {
    public static RngStreams Create(ulong seed);
    public ref Pcg32 Get(RngStream stream);
    public Pcg32State[] Capture();                 // theo thứ tự enum
    public void Restore(Pcg32State[] states);
}
public sealed class ContentDb {
    public static ContentDb Create(IReadOnlyList<ItemData> items, IReadOnlyList<DifficultyData> difficulties,
                                   IReadOnlyList<StarterKitData> kits, RunData run);
    public ItemData GetItem(ItemId id);
    public bool TryFindItem(string id, out ItemId itemId);
    public bool TryFindDifficulty(string id, out DifficultyData data);
    public bool TryFindStarterKit(string id, out StarterKitData data);
    public RunData Run { get; }
}
public sealed class StartRun : ICommand {
    public StartRun(string difficultyId, string starterKitId, ulong seed, long nowUnix);
}
public readonly struct CommandResult {
    public bool Accepted { get; }  public RejectReason Reason { get; }  public IReadOnlyList<GameEvent> Events { get; }
}
public static class GameRules {
    public static CommandResult Apply(RunState run, MetaState meta, ContentDb content, ICommand command);
}
public static class StateHasher { public static ulong Hash(RunState run, MetaState meta); }
```

**Interface phụ thuộc**: không cần interface — PRNG là struct có seed, thời gian đi vào Command (`nowUnix`), content
truyền tham số. Test dựng `ContentDb` bằng creation method.
**Dữ liệu**: chưa có SO (ContentBaker ngoài phạm vi). `RunData` mặc định trong test: `StartingReputation`,
`StartingBagCapacity`, `SeasonCount` — luôn truyền tường minh.
**Scene / prefab bị chạm**: không.
**Lý do thiết kế**:
- `sealed class` + property chỉ-đọc thay `record`: `record` / `init` cần `IsExternalInit` (không có trong .NET Standard
  2.1 của Unity) — tránh thêm shim.
- `RngStreams.Get` trả `ref Pcg32` để luật rút trực tiếp trên stream của mình mà không copy struct.
- `RunState.IsActive` phân biệt "chưa có run" với run đang chơi (cho `RunAlreadyActive`).
- Mọi type public trong Core (thư viện luật), không cần `InternalsVisibleTo`.

## 3. Test cases

Tất cả EditMode (`Assets/Tests/EditMode/Core/...`), namespace theo SUT.

### Editor tests
#### CoreAssemblyConstraintsTest
| Test method | Kiểm tra |
|---|---|
| `CoreAssembly_ReferencedAssemblies_ExcludeUnityEngine` | không assembly tham chiếu nào có tên bắt đầu `UnityEngine` |
| `CoreAssembly_StateTypes_HaveNoFloatingPointFields` | không field `float`/`double` trong type thuộc namespace `Crucible.Core.State` |

### Unit tests
#### SplitMix64Test
| Test method | Kiểm tra |
|---|---|
| `Next_ReferenceSeed_ReturnsReferenceSequence` | 5 giá trị đầu từ seed 1234567 khớp vector tham chiếu SplitMix64 |

#### Pcg32Test
| Test method | Kiểm tra |
|---|---|
| `NextUInt_Seed42Sequence54_ReturnsReferenceSequence` | 6 giá trị đầu = `0xa15c02b7, 0x7b47f409, 0xba1d3330, 0x83d2f293, 0xbfa4784b, 0xcbed606e` |
| `NextInt_PositiveMax_ReturnsValueInRange (seed: {0, 1, 42}, max: {1, 2, 7, int.MaxValue})` | 0 ≤ kết quả < max |
| `NextInt_NonPositiveMax_ThrowsArgumentOutOfRangeException (max: {0, -1})` | ném `ArgumentOutOfRangeException` |
| `NextInt_SixtyThousandSamplesMaxSix_EachBucketWithinFivePercent` | mỗi giá trị 0–5 xuất hiện 10.000 ± 500 lần (seed cố định) |
| `Roll_PercentZero_ReturnsFalse (seed: {0, 1, 42})` | false |
| `Roll_PercentHundred_ReturnsTrue (seed: {0, 1, 42})` | true |
| `Roll_AnyValidPercent_AdvancesStateByOneDraw (percent: {0, 50, 100})` | `State` sau `Roll` = `State` sau một `NextUInt` từ cùng điểm |
| `Roll_PercentOutsideZeroToHundred_ThrowsArgumentOutOfRangeException (percent: {-1, 101})` | ném `ArgumentOutOfRangeException` |
| `State_SetFromEarlierCapture_ReplaysSameSequence` | 3 giá trị sau khôi phục = 3 giá trị ban đầu |

#### RngStreamsTest
| Test method | Kiểm tra |
|---|---|
| `Create_SameSeed_ProducesIdenticalSequences (stream: (RngStream))` | 3 giá trị đầu giống nhau giữa hai instance |
| `Create_SameSeed_StreamsProduceDistinctFirstValues` | 7 stream có 7 giá trị đầu khác nhau |
| `Get_DrawFromShop_LeavesOtherStreamsUnchanged (other: Chance, Inspect, Forge, Market, Season, Reward)` | stream khác vẫn cho giá trị như instance chưa rút |
| `Create_SeedZero_StreamsProduceDistinctFirstValues` | 7 giá trị đầu khác nhau, không toàn 0 |
| `Restore_FromCapture_ContinuesIdentically` | sau khôi phục, mỗi stream cho cùng giá trị kế tiếp như bản gốc |

#### ContentDbTest
| Test method | Kiểm tra |
|---|---|
| `Create_ItemsInReverseOrder_AssignsIdsByOrdinalSort` | `ItemId` của "a_item" < "b_item" < "c_item" |
| `TryFindItem_KnownId_ReturnsMatchingItem` | true + `GetItem(id).Id` = id chuỗi |
| `TryFindItem_UnknownId_ReturnsFalse` | false |
| `Create_DuplicateItemId_ThrowsArgumentException` | ném `ArgumentException` |

#### GameRulesStartRunTest
| Test method | Kiểm tra |
|---|---|
| `Apply_ValidStartRun_EntersSeasonOneDecree` | Accepted; `Season` = 1; `Phase` = `Decree`; `Gold` = 0; `Reputation`, `BagCapacity` = giá trị `RunData`; `Seed` = seed command |
| `Apply_ValidStartRun_FillsBagWithStarterKitInOrder` | `ItemId` trong Túi = danh sách bộ khởi đầu, đúng thứ tự |
| `Apply_ValidStartRun_AssignsStrictlyIncreasingUids` | `Uid` Túi tăng ngặt; `NextUid` > `Uid` lớn nhất |
| `Apply_ValidStartRun_RaisesRunStarted` | một sự kiện `RunStarted` |
| `Apply_UnknownDifficulty_RejectsWithUnknownContent` | Rejected, `UnknownContent` |
| `Apply_UnknownStarterKit_RejectsWithUnknownContent` | Rejected, `UnknownContent` |
| `Apply_StartRunWhileRunActive_RejectsWithRunAlreadyActive` | Rejected, `RunAlreadyActive` |
| `Apply_UnregisteredCommandType_RejectsWithUnknownCommand` | Rejected, `UnknownCommand` |
| `Apply_RejectedCommand_LeavesStateHashUnchanged` | hash trước = hash sau |
| `Apply_SameSeedOnFreshStates_ProducesSameStateHash` | hai state mới + cùng `StartRun` → cùng hash |

#### StateHasherTest
| Test method | Kiểm tra |
|---|---|
| `Hash_TwoIdenticallyBuiltStates_ReturnsSameValue` | hai hash bằng nhau |
| `Hash_StatesDifferInOneField_ReturnsDifferentValue (mutation: gold, season, phase, bag order, item uid, rng draw)` | hai hash khác nhau |
| `Hash_BagListCapacityDiffers_ReturnsSameValue` | hai hash bằng nhau |

### Integration tests
(none)

### Visual verification tests
(none)

### Manual tests
(none)

**Truy vết**: R1 → `Pcg32Test.NextUInt_Seed42…`; R2 → `NextInt_*` (3); R3 → `Roll_*` (4); R4 → `RngStreamsTest`
(`Create_*`, `Get_*`) + `SplitMix64Test`; R5 → `Pcg32Test.State_*`, `RngStreamsTest.Restore_*`; R6 → `ContentDbTest`;
R7 → `GameRulesStartRunTest.Apply_ValidStartRun_*`; R8 → `Apply_Unknown*`, `Apply_StartRunWhileRunActive_*`,
`Apply_RejectedCommand_*`; R9 → `StateHasherTest` + `Apply_SameSeedOnFreshStates_*`; R10 → `CoreAssemblyConstraintsTest`.
**Testability**: PASS — logic thuần, không phụ thuộc Unity, không I/O, PRNG có seed.

## 4. Các bước thực hiện

> `CLAUDE.md` §Git: commit tự động **không**, commit test-first **không** → đề xuất message, test + code gộp một commit
> khi xanh. Lệnh test: `CLAUDE.md` §Lệnh (`$UNITY` Linux = `~/Unity/Hub/Editor/6000.6.7f1/Editor/Unity`), **Editor phải đóng**.

- [x] **Bước 1 — Skeleton**: tạo mọi type §2, thân method ném `NotImplementedException` / trả mặc định; compile sạch
  (`"$UNITY" -batchmode -nographics -projectPath . -quit -logFile Logs/compile.log`, grep `error CS` = 0).
- [x] **Bước 2 — Test first**: viết toàn bộ test §3 (+ `TestUtils/ContentFactory` creation method); chạy EditMode →
  **fail đúng lý do** (`NotImplementedException` / giá trị mặc định), test ràng buộc assembly (R10) xanh ngay.
- [x] **Bước 3 — Implement**: SplitMix64 → Pcg32 → RngStreams → ContentDb → state + CanonicalWriter/StateHasher →
  GameRules/StartRun; chạy EditMode tới khi xanh (`Logs/editmode.xml`, đọc `result="Passed"`, 0 failed).
- [x] **Bước 4 — Refactor**: gộp test cùng phân vùng, gọn code; EditMode vẫn xanh. Chạy `check_unity_project.py` (0 lỗi).
- [x] **Bước 5 — Bằng chứng & tài liệu**: ghi số test/kết quả vào LOG; PLAN F-000/F-001 → Done; TECH §3 trạng thái →
  Implemented (phần nền); đề xuất commit `feat(core): add RNG streams, content db, state hash and StartRun`.

## 5. Rủi ro & đánh đổi

- Vector tham chiếu SplitMix64 / PCG32 phải đối chiếu bản cài đặt tham chiếu C khi viết test (không chép từ trí nhớ
  mà không kiểm) — nếu lệch, kiểm thuật toán trước khi sửa vector.
- `CanonicalWriter` phải ghi **mọi** field gameplay; thêm field mới ở plan sau mà quên ghi → hash không bắt được khác
  biệt. Giảm rủi ro: mỗi plan thêm field phải thêm một case vào `Hash_StatesDifferInOneField_*`.
- Chạy batchmode khóa project: phải đóng Editor đang mở.
- Test phân phối (R2) dùng seed cố định → tất định, không flaky.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - Plan ở trạng thái Draft khi gọi `build` → coi lệnh `build core-foundation` là duyệt.
  - Vector tham chiếu SplitMix64 (seed 1234567) và PCG32 (42, 54) đối chiếu bằng bản Python độc lập theo
    `pcg32_srandom_r` / `splitmix64.c` trước khi viết test — khớp.
  - Dẫn seed stream: `mix = seed ^ ((i + 1) × 0x9E3779B97F4A7C15)`, rồi hai lần `SplitMix64.Next(ref mix)` → `(seed,
    sequence)` của `Pcg32` stream `i`. Salt khác 0 nên seed 0 vẫn cho 7 stream khác nhau. **Đổi công thức này = đổi
    mọi run đã lưu** (save lưu trạng thái RNG nên run dở không vỡ, nhưng seed cũ cho kết quả khác).
  - `Roll(percent)` = `(NextUInt() × 100) >> 32 < percent` — đúng một lần rút; không dùng `NextInt(100)` vì vòng
    loại bỏ của Lemire có thể rút thêm. Độ lệch ≤ 100/2³², chấp nhận.
  - `ItemId` chỉ có `Index` + so bằng (không `IComparable`); test thứ tự so `Index`. Constructor `ItemId(int)` public
    (test dựng state trực tiếp).
  - `StarterKitData.ItemIds` là Id chuỗi; `StartRun` phân giải trong bước validate — item lạ → `UnknownContent`.
  - `RunState` thêm `StartedAtUnix` (lưu `nowUnix` của `StartRun`) và được hash; `MetaState` phần nền chỉ có `Renown`.
  - `CanonicalWriter` băm FNV-1a thẳng khi ghi (không buffer, không alloc); string = độ dài (−1 nếu null) + UTF-16.
  - `ContentDb.Create`: Id trùng / rỗng ở item, bậc khó, bộ khởi đầu đều → `ArgumentException` (plan chỉ test item).
- **Deviations**: - None
- **Tradeoffs**:
  - 5 test so bằng / từ chối (`Hash_TwoIdenticallyBuiltStates_*`, `Hash_BagListCapacityDiffers_*`,
    `Apply_RejectedCommand_*`, `Apply_SameSeedOnFreshStates_*`, `TryFindItem_UnknownId_*`) pass ngay trên skeleton vì
    stub trả hash 0 / kết quả mặc định — đã dự đoán, không phải tính năng có sẵn; các test anh em
    (`Hash_StatesDifferInOneField_*`, `Apply_Unknown*`, `TryFindItem_KnownId_*`) fail trên stub nên cặp test có nghĩa.
  - Bước 4: không refactor — `Roll_PercentZero/Hundred` giữ riêng (khác phân vùng & assertion).
- **Test changes**:
  - `Apply_ValidStartRun_RaisesRunStarted`: `Has.Count.EqualTo(1)` → `result.Events.Count` — NUnit tìm property
    `Count` công khai, mảng `GameEvent[]` chỉ có `Count` tường minh (`ArgumentException: Property Count was not found`).
    Chưa commit, sửa trước khi xanh.
- **Open questions**:
  - Bộ khởi đầu nhiều hơn `StartingBagCapacity` — chưa có luật (hiện nạp hết). Chốt ở `gdd`/economy §Túi hoặc
    validate trong `ContentBaker`.
  - `ProjectSettings.asset`: batchmode bỏ define `SENTIS_ANALYTICS_ENABLED` khỏi Standalone — không thuộc feature,
    không commit; người dùng kiểm.
