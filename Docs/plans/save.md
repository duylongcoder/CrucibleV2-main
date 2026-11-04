# Plan: Lưu & tiếp tục (F-013)

> **Trạng thái**: Draft (chờ duyệt D1–D12) · **Cập nhật**: 2026-11-04
> **GDD**: §6.4 (Rules, Edge Cases, AC-1…AC-4), §7 (menu "Tiếp tục") · **ADR**: ADR-001, ADR-003, ADR-004, ADR-005, ADR-007
> · **Loại**: Integration (C# thuần, EditMode)
> Xây trên F-000…F-011 + F-009 phần 2 (1024/1024 EditMode xanh). Newtonsoft `3.2.1` **đã có** trong `Packages/manifest.json`
> (dll `isExplicitlyReferenced: 0` → `Crucible.Runtime` tự tham chiếu, không cần sửa asmdef).

## 1. Bối cảnh

"Dừng là dừng" (P4): mọi hành động hợp lệ phải được lưu ngay, mở lại vào đúng trạng thái, cùng `StateHash`. Toàn bộ state
đã nằm trong `RunState` / `MetaState` thuần C# (ADR-001), RNG chụp/khôi phục được (`RngStreams.Capture/Restore`),
`StateHasher` đã phủ mọi field. Còn thiếu: DTO + JSON + version/migrate, kho lưu atomic có backup meta, và lớp Application
`GameSession` (Execute → lưu → event; Load khi khởi động; run mới / run kết thúc).

**Yêu cầu** (đánh số để truy vết):
- R1 — **Round-trip run**: `RunState` → `RunSaveDto` → JSON → `RunSaveDto` → `RunState` giữ nguyên `StateHasher.Hash` ở mọi
  phase/bước (Decree, Workbench·Editing có lưới, Workbench·ResultPending, Offering, Battle, Spoils, sau khi `Inspect`, có
  modifier). DTO mang đủ field TECH §3.2: seed, 7 trạng thái stream, bậc khó, bộ khởi đầu, `StartedAtUnix`, Mùa, phase,
  `Turn`, `WorkbenchStep`, vàng, Uy tín, sức chứa, `NextUid`, Túi (thứ tự giữ), lưới 9 ô, vật phẩm (`Uid`, item, `Magic`,
  `Quality`, `Lines`, `RevealedMask`), `LastBrewResultUid`, `LastBrew` (+ `Activations`), `SeasonPricePercent`, `Shop`
  (ô + `RerollCount`), `DecreeEquipment`, `LastBattle`, `Outcome`, `SpoilsOptions`, `ActiveModifier`, `NextModifier`.
- R2 — **Content bằng `Id` chuỗi** (TECH §3.1): DTO không chứa chỉ số `ItemId/EquipmentId/RewardId/ModifierId`; `Id` không
  resolve được trong `ContentDb` hiện tại → save hỏng (R7).
- R3 — **Round-trip meta**: `MetaState` (`Renown`, `Timescale`) ↔ `MetaSaveDto` giữ `StateHash` phần meta. (Sổ công thức,
  bậc khó / bộ khởi đầu đã mở → F-012 thêm field vào `MetaSaveDto`.)
- R4 — **Lưu sau mỗi Command được nhận** (GDD §6.4): `GameSession.Execute` Accepted → ghi; Rejected → **không ghi gì**.
  Meta ghi **trước** run (AC-3: đóng tab giữa hai lần ghi không làm mất mở khóa); meta chỉ ghi khi nội dung JSON khác lần
  ghi trước.
- R5 — **Mở lại** (AC-1, phần Editor): session mới đọc cùng kho → cùng `StateHash`, `HasResumableRun` = true, và chơi tiếp
  cùng chuỗi lệnh → cùng hash cuối như không ngắt. Đóng giữa lúc phát lại lượt nung → mở lại ở `ResultPending` với
  `LastBrew` nguyên vẹn (kết quả đã tính khi Nung).
- R6 — **Run kết thúc** (AC-2): lệnh đưa run vào `Ended` → xóa save run, meta giữ nguyên; `HasResumableRun` = false (cả
  trong session hiện tại lẫn sau khi mở lại).
- R7 — **Save run hỏng** (AC-4): JSON lỗi / thiếu field / field lạ / enum lạ / `Id` không có / version không hỗ trợ / vi phạm
  ràng buộc cấu trúc (D10) → `Load` báo `RunLoadStatus.Corrupt` + lý do, **xóa** save run, meta không đổi, không ném lỗi.
- R8 — **Meta hỏng**: dùng backup tốt gần nhất (`MetaLoadStatus.RestoredFromBackup`); backup cũng hỏng / không có →
  `MetaState` mới (`MetaLoadStatus.Reset`). Không ảnh hưởng run.
- R9 — **Ghi atomic + backup meta**: ghi file tạm → thay thế; không bao giờ để lại file nửa vời; ghi meta giữ bản trước ở
  `meta.bak.json`. Save run **không** có backup (không save-scum).
- R10 — **Version & migrate**: mọi file có `version`; chuỗi migrate `vN → vN+1` trên `JObject` trước khi map. Hiện
  `CurrentVersion = 1`, chưa có bước; `version` thiếu / > hiện tại / không có đường → run: hỏng (R7); meta: như meta hỏng (R8).
- R11 — **Run mới**: `StartNewRun(difficultyId, kitId)` bỏ run hiện có (dở hoặc đã `Ended`) — UI tự xác nhận ghi đè —
  tạo `RunState` mới, seed từ `ISeedSource`, thời điểm từ `IClock` (Core không đọc đồng hồ).
- R12 — **Fuzz tất định qua save**: trên content trọn run (F-009 phần 2), round-trip sau **mỗi** lệnh giữ hash; ngắt ở bước
  ngẫu nhiên → load → chơi tiếp → cùng hash cuối.

**Ngoài phạm vi**:
- `GameHost` (MonoBehaviour), scene Boot, `ContentBaker` SO → `ContentDb` (chưa có SO content) → feature Boot/F-014.
  Vì vậy AC-1 **trên trình duyệt** (đóng tab thật) chưa kiểm được — chỉ phần Editor (R5).
- `RunView` / `MetaView` trong `GameSession` (`RunViewBuilder` chưa có) → F-014. F-013 chỉ lộ `HasResumableRun`,
  `StateHash`, `LastLoad`.
- Sổ công thức / mở khóa trong meta (AC-3 phần "Sổ đã có thành phẩm") → F-012; F-013 chỉ đảm bảo thứ tự ghi meta-trước.
- Màn báo lỗi "Không thể tiếp tục run" / "meta mới" → F-014/F-015 đọc `LoadReport`.

**TBD (không làm)**: hai tab cùng mở (GDD §6.4); cơ chế đồng bộ IndexedDB (`FS.syncfs` / auto-sync) của 6000.6 WebGL →
lần build WebGL đầu (`setup`/`build`).

### Quyết định cần duyệt (đề xuất)

| # | Quyết định | Lý do / phương án bỏ |
|---|---|---|
| D1 | Phạm vi = `GameSession` + kho + DTO + mapper + migrator, **không** `GameHost`/Boot | Chưa có content SO để Boot nạp; làm GameHost rỗng là code chết |
| D2 | Core thêm `BrewLog.Restore(...)` + `BattleLog.Restore(...)` **public**, có validate (ném `ArgumentException`) | Ctor đang `private`/`internal`; bỏ `InternalsVisibleTo("Crucible.Runtime")` vì mở toàn bộ internal Core cho Runtime |
| D3 | JSON Newtonsoft: `TypeNameHandling.None`, `MissingMemberHandling.Error`, enum dạng **chuỗi** (`StringEnumConverter`, cấm số), `ulong` dạng số | Field lạ / enum số tay → hỏng thay vì âm thầm sai; chuỗi enum bền khi đổi thứ tự enum |
| D4 | Lưu sau mỗi Accepted: meta (nếu khác) **rồi** run; `Ended` → xóa run thay vì ghi | GDD AC-2/AC-3; crash giữa hai lần ghi → chơi lại lệnh cùng kết quả (tất định), mở khóa không mất |
| D5 | Lỗi đọc/map run → xóa run + `Corrupt` (kèm `Reason` chuỗi cho log); lỗi **ghi** (IO) không bắt — ném lên caller | Không nuốt lỗi (CLAUDE.md); ghi hỏng là lỗi môi trường, GameHost (sau) log + báo |
| D6 | Backup meta thuộc **hợp đồng `ISaveStore`**: `Write(Meta)` giữ bản cũ ở `MetaBackup`. `FileSaveStore`: `File.Replace(tmp, dst, bak)` (lần đầu `File.Move`) | Một chỗ thực thi, test hợp đồng chạy trên cả `InMemorySaveStore` và `FileSaveStore` |
| D7 | `StartNewRun` tạo `RunState` mới (không thêm lệnh Core "AbandonRun") | `RunStartRules` từ chối khi `IsActive`; bỏ run là việc của Application, không phải luật game |
| D8 | Seed tự sinh qua `ISeedSource` (`SystemSeedSource`: `Guid.NewGuid()` → 8 byte); test dùng seed cố định | MVP không nhập seed (GDD §9); không `System.Random` |
| D9 | `SaveMigrator(currentVersion, steps)` — `steps[i]`: `JObject` v(i+1) → v(i+2); production `(1, rỗng)` | Khung migrate có test bằng bước giả; chưa cần bước thật |
| D10 | Validate khi load = **cấu trúc**, không phải luật: field bắt buộc khác null; `IsActive`; Mùa ∈ [1, `SeasonCount`]; `Turn` khớp phase; lưới 9 ô; Uid duy nhất và < `NextUid`; 7 stream, `Inc` lẻ; ≤ `MaxShopSlots` ô Chợ; ≤ `MaxSpoilsOptions`; Timescale 1–3; mọi `Id` resolve | Đủ chặn sửa tay / content đổi; validate trọn luật (I1–I7) để fuzz lo |
| D11 | Fuzz: 100 run (50 seed × 2 hồ sơ) round-trip sau mỗi lệnh + 50 run ngắt ở bước ngẫu nhiên rồi chơi tiếp; ngân sách ≤ 5 s | 1.000 run × mỗi bước quá chậm với JSON; tất định 1.000 run đã có ở F-009 |
| D12 | File `run.json`, `meta.json`, `meta.bak.json` trong `Application.persistentDataPath/save/` — đường dẫn do caller (GameHost sau này) truyền vào `FileSaveStore(rootDir)` | Test dùng thư mục tạm; Runtime C# thuần không gọi `Application` |

## 2. Thiết kế triển khai

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Logs/BrewLog.cs` | + `static BrewLog Restore(int firstCell, IReadOnlyList<BrewTick>, BrewLogResult, IReadOnlyList<CatalystActivation>)` — validate `0 ≤ firstCell`, `Ticks[i].Cell == firstCell + i`, ô cuối ≤ 8, ≥ 1 tick, tick đầu/cuối có vật phẩm | Core | Sửa |
| `Core/Logs/BattleLog.cs` | + `static BattleLog Restore(...)` cùng tham số ctor; validate `Forfeit` ⇔ `!HasOfferedItem` ⇔ 0 hiệp, HP ≥ 0 | Core | Sửa |
| `Runtime/Save/SaveSlot.cs` | `enum SaveSlot { Run, Meta, MetaBackup }` | Runtime | Mới |
| `Runtime/Save/ISaveStore.cs` | `bool TryRead(SaveSlot, out string)`, `void Write(SaveSlot, string)` (atomic; `Meta` giữ bản cũ ở `MetaBackup`), `void Delete(SaveSlot)` | Runtime | Mới |
| `Runtime/Save/InMemorySaveStore.cs` | Dictionary; đếm/ghi nhật ký thao tác (`Operations`) cho test thứ tự | Runtime | Mới |
| `Runtime/Save/FileSaveStore.cs` | `FileSaveStore(string rootDir)`; ghi `<file>.tmp` → flush → `File.Replace`/`File.Move` | Runtime | Mới |
| `Runtime/Save/RunSaveDto.cs` + `ItemDto`, `StatLineDto`, `RngStateDto`, `BrewLogDto`, `BrewTickDto`, `CatalystActivationDto`, `BattleLogDto`, `BattleRoundDto`, `CombatantDto` | POCO public field, không logic; `Id` chuỗi; ô trống lưới / Chợ = `null` | Runtime | Mới (10 file) |
| `Runtime/Save/MetaSaveDto.cs` | `Version`, `Renown`, `Timescale` | Runtime | Mới |
| `Runtime/Save/SaveMapper.cs` | `RunSaveDto ToDto(RunState, ContentDb)`; `bool TryToRunState(RunSaveDto, ContentDb, out RunState, out string reason)`; tương tự meta | Runtime | Mới |
| `Runtime/Save/SaveSerializer.cs` | settings D3; `string Serialize(object)`; `bool TryDeserialize<T>(string json, SaveMigrator, out T, out string reason)` (parse `JObject` → migrate → `ToObject<T>`) | Runtime | Mới |
| `Runtime/Save/SaveMigrator.cs` | D9; `bool TryMigrate(JObject, out string reason)` | Runtime | Mới |
| `Runtime/Save/SaveVersion.cs` | `const int Current = 1` | Runtime | Mới |
| `Runtime/App/IClock.cs`, `SystemClock.cs` | `long NowUnix` (`DateTimeOffset.UtcNow`) | Runtime | Mới |
| `Runtime/App/ISeedSource.cs`, `SystemSeedSource.cs` | `ulong NextSeed()` (D8) | Runtime | Mới |
| `Runtime/App/GameSession.cs` | Application: giữ `RunState`, `MetaState`; Execute → lưu → event; Load; StartNewRun | Runtime | Mới |
| `Runtime/App/LoadReport.cs`, `RunLoadStatus.cs`, `MetaLoadStatus.cs` | Kết quả `Load` | Runtime | Mới |

**Public API**
```csharp
namespace Crucible.Runtime.App {
public sealed class GameSession {
    public GameSession(ContentDb content, ISaveStore store, IClock clock, ISeedSource seeds);
    public bool HasResumableRun { get; }          // run.IsActive && Phase != Ended
    public ulong StateHash { get; }               // StateHasher.Hash(run, meta) — debug + test
    public long NowUnix { get; }                  // cho Command cần thời điểm (Brew)
    public LoadReport LastLoad { get; }
    public event Action<CommandResult> CommandApplied;   // chỉ phát khi Accepted, sau khi lưu
    public LoadReport Load();                     // gọi 1 lần lúc khởi động
    public CommandResult StartNewRun(string difficultyId, string starterKitId);
    public CommandResult Execute(ICommand command);      // StartRun qua Execute → Reject(RunAlreadyActive) nếu đang có run
}
public enum RunLoadStatus  { NoRun, Resumed, Corrupt }
public enum MetaLoadStatus { New, Loaded, RestoredFromBackup, Reset }
public readonly struct LoadReport { RunLoadStatus Run; MetaLoadStatus Meta; string RunError; string MetaError; }
}
namespace Crucible.Runtime.Save {
public static class SaveMapper {
    public static RunSaveDto ToDto(RunState run, ContentDb content);
    public static bool TryToRunState(RunSaveDto dto, ContentDb content, out RunState run, out string reason);
    public static MetaSaveDto ToDto(MetaState meta);
    public static bool TryToMetaState(MetaSaveDto dto, out MetaState meta, out string reason);
}
}
```

**Luồng `Execute`**: `GameRules.Apply` → Rejected: trả về (không ghi, không event). Accepted: `metaJson = Serialize(meta)`;
khác `_lastMetaJson` → `store.Write(Meta)`; run `Ended` → `store.Delete(Run)`, ngược lại `store.Write(Run, ...)`; phát
`CommandApplied`.
**Luồng `Load`**: meta: `Meta` → (lỗi) `MetaBackup` → (lỗi) `new MetaState()` + `Reset`, sau `RestoredFromBackup`/`Reset`
ghi lại `Meta` ngay. Run: không có → `NoRun`; lỗi → `Delete(Run)` + `Corrupt`; ok → `Resumed`. Run đọc được nhưng
`Phase == Ended` (crash trước khi xóa) → xóa, `NoRun`.

**Interface phụ thuộc**: `ISaveStore`, `IClock`, `ISeedSource` (constructor). Test: `InMemorySaveStore`, `FixedClock` +
`FixedSeedSource` (TestUtils).
**Dữ liệu**: không SO mới. **Scene / prefab bị chạm**: không. **asmdef**: không đổi (Newtonsoft tự tham chiếu); `link.xml`
giữ `Crucible.Runtime.Save` DTO → làm ở lần build WebGL đầu (ghi vào Rủi ro).
**Lý do thiết kế**: DTO tách Core (ADR-003); mapper tĩnh không state; hợp đồng backup trong store để test một lần cho cả
hai cài đặt; `GameSession` C# thuần → test EditMode toàn bộ luồng lưu/mở lại mà không cần scene.

## 3. Test cases

### Editor tests
(none)

### Unit tests
#### `BrewLogRestoreTest` (mới — Core/Logs)
| Test method | Kiểm tra |
|---|---|
| `Restore_FieldsFromCreatedLog_HashEqualsOriginal` | hash `LastBrew` bằng bản gốc (có Activations) |
| `Restore_TickCellsNotContiguous_Throws` | `ArgumentException` |
| `Restore_FirstOrLastTickEmpty_Throws` | `ArgumentException` |
| `Restore_LastCellBeyondGrid_Throws` | `ArgumentException` |

#### `BattleLogRestoreTest` (mới — Core/Logs)
| Test method | Kiểm tra |
|---|---|
| `Restore_FieldsFromSimulatedLog_HashEqualsOriginal` | hash bằng bản gốc |
| `Restore_ForfeitWithRounds_Throws` | `ArgumentException` |
| `Restore_NegativeHp_Throws` | `ArgumentException` |

#### `SaveMapperRunTest` (mới — Runtime/Save)
| Test method | Kiểm tra |
|---|---|
| `RoundTrip_PhaseFixture_HashEqual` (Decree, WorkbenchEditingWithGrid, ResultPending, Offering, Battle, Spoils, EndedWon) | `StateHash` bằng nhau |
| `RoundTrip_InspectedProductWithLines_HashEqual` | `Lines` + `RevealedMask` giữ nguyên (hash) |
| `RoundTrip_ShopWithSoldSlot_HashEqual` | ô `None` giữ vị trí |
| `RoundTrip_ModifiersNullAndSet_HashEqual` | hash bằng nhau |
| `RoundTrip_ContinuePlaying_SameRngDraws` | `Reroll` sau load cho cùng hàng Chợ như không load |
| `ToDto_AnyRun_ContainsNoContentIndex` | JSON không có khóa `Index` / chỉ chứa `Id` chuỗi của content |
| `TryToRunState_UnknownItemId_ReturnsFalse` | false, lý do nêu Id |
| `TryToRunState_UnknownEquipmentRewardOrModifierId_ReturnsFalse` (3 case) | false |
| `TryToRunState_DuplicateUid_ReturnsFalse` | false |
| `TryToRunState_UidNotBelowNextUid_ReturnsFalse` | false |
| `TryToRunState_GridLengthNotNine_ReturnsFalse` | false |
| `TryToRunState_RngStreamCountWrong_ReturnsFalse` | false |
| `TryToRunState_EvenRngIncrement_ReturnsFalse` | false |
| `TryToRunState_SeasonOutOfRange_ReturnsFalse` (0, SeasonCount + 1) | false |
| `TryToRunState_TurnInconsistentWithPhase_ReturnsFalse` (Workbench·0, Decree·1) | false |
| `TryToRunState_TooManyShopSlotsOrSpoilsOptions_ReturnsFalse` (2 case) | false |
| `TryToRunState_RequiredFieldNull_ReturnsFalse` (Bag, Grid, Rng, Shop) | false, không ném |
| `TryToRunState_InactiveRun_ReturnsFalse` | false |

#### `SaveMapperMetaTest` (mới)
| Test method | Kiểm tra |
|---|---|
| `RoundTrip_Meta_HashEqual` | hash phần meta bằng nhau |
| `TryToMetaState_TimescaleOutOfRange_ReturnsFalse` (0, 4) | false |

#### `SaveSerializerTest` (mới)
| Test method | Kiểm tra |
|---|---|
| `SerializeDeserialize_RunDto_HashEqualAfterMapping` | hash bằng nhau |
| `Serialize_RunDto_HasVersionAndNoTypeMetadata` | có `"version": 1`, không có `$type` |
| `Serialize_EnumField_WrittenAsName` | `"phase": "Workbench"` |
| `TryDeserialize_MalformedJson_ReturnsFalse` | false, không ném |
| `TryDeserialize_UnknownMember_ReturnsFalse` | false |
| `TryDeserialize_NumericOrUnknownEnum_ReturnsFalse` (`3`, `"Lunch"`) | false |
| `TryDeserialize_EmptyString_ReturnsFalse` | false |

#### `SaveMigratorTest` (mới)
| Test method | Kiểm tra |
|---|---|
| `TryMigrate_CurrentVersion_Unchanged` | JObject giữ nguyên |
| `TryMigrate_OlderVersionWithSteps_AppliesStepsInOrder` (current 3, từ 1) | 2 bước chạy theo thứ tự, version = 3 |
| `TryMigrate_VersionAboveCurrent_ReturnsFalse` | false |
| `TryMigrate_VersionMissingOrNotInteger_ReturnsFalse` (2 case) | false |

#### `SaveStoreContractTest` (mới — abstract, chạy cho `InMemorySaveStore` và `FileSaveStore` thư mục tạm)
| Test method | Kiểm tra |
|---|---|
| `TryRead_NothingWritten_ReturnsFalse` | false |
| `Write_ThenTryRead_ReturnsSameText` | cùng chuỗi (Unicode tiếng Việt) |
| `Write_Twice_ReturnsLatest` | chuỗi lần 2 |
| `Delete_AfterWrite_TryReadReturnsFalse` | false |
| `WriteMeta_Second_PreviousKeptInBackup` | `MetaBackup` = lần 1 |
| `WriteMeta_First_NoBackup` | `MetaBackup` không có |
| `WriteRun_Twice_NoBackupCreated` | `MetaBackup` không có |

#### `FileSaveStoreTest` (mới — riêng file)
| Test method | Kiểm tra |
|---|---|
| `Write_Completed_NoTempFileLeft` | thư mục chỉ còn file đích |
| `TryRead_LeftoverTempFromCrash_IgnoresTemp` | đọc bản đích cũ |
| `Ctor_MissingDirectory_CreatesOnFirstWrite` | ghi thành công |

### Integration tests (`GameSession` + `InMemorySaveStore`)
#### `GameSessionSaveTest` (mới)
| Test method | Kiểm tra |
|---|---|
| `Execute_Accepted_RunWritten` | kho có `Run`, mở lại cùng hash |
| `Execute_Rejected_NothingWritten` | `Operations` rỗng, event không phát |
| `Execute_MetaUnchanged_MetaNotWritten` | chỉ 1 thao tác `Write(Run)` |
| `Execute_SetTimescale_MetaWrittenBeforeRun` | thứ tự `Write(Meta)` → `Write(Run)` |
| `Execute_CommandApplied_RaisedAfterSave` | trong handler, kho đã có state mới |
| `Execute_RunEnds_RunDeletedMetaKept` (Won, Exiled) | `Run` không còn, `Meta` còn, `HasResumableRun` false |
| `StartNewRun_NoRun_UsesSeedAndClock` | seed = `FixedSeedSource`, `StartedAtUnix` = `FixedClock` |
| `StartNewRun_ActiveRun_ReplacesRun` | seed mới, Mùa 1, kho chứa run mới |
| `StartNewRun_EndedRun_StartsFresh` | Accepted |
| `Execute_StartRunWhileActive_Rejected` | `RunAlreadyActive` |

#### `GameSessionLoadTest` (mới)
| Test method | Kiểm tra |
|---|---|
| `Load_EmptyStore_NoRunAndNewMeta` | `NoRun`, `New`, `HasResumableRun` false |
| `Load_AfterPhase_SameHashAndResumable` (Decree, Workbench, ResultPending, Offering, Battle, Spoils) | hash bằng session cũ, `Resumed` (AC-1 Editor) |
| `Load_ClosedDuringBrewReplay_ResultPendingWithLog` | `ResultPending`, hash `LastBrew` bằng nhau (GDD §6.4) |
| `Load_ThenContinue_SameFinalHashAsUninterrupted` | hash cuối bằng nhau |
| `Load_CorruptRun_ReportsCorruptDeletesRunKeepsMeta` (rác, `Id` lạ, version 99, field lạ) | `Corrupt` + lý do, `Run` bị xóa, meta hash không đổi (AC-4) |
| `Load_EndedRunLeftOver_DeletedAsNoRun` | `NoRun`, `Run` bị xóa |
| `Load_CorruptMetaWithBackup_RestoredFromBackup` | `RestoredFromBackup`, `Meta` = backup |
| `Load_CorruptMetaAndBackup_ResetAndRewritten` | `Reset`, meta mặc định đã ghi |
| `Load_CorruptMeta_RunStillResumed` | `Resumed` |

#### `SaveRoundTripFuzzTest` (mới — content `CreateFullRun`, `RandomAgent`, `RunRecorder.Play` + `onStep`)
| Test method | Kiểm tra |
|---|---|
| `EveryStep_RoundTripThroughJson_HashEqual` (100 run) | hash sau round-trip = hash state mỗi bước |
| `InterruptAtRandomStep_ContinueFromLoad_SameFinalHash` (50 run) | hash cuối bằng run không ngắt |

### Visual verification tests
(none — chưa có UI)

### Manual tests
| Đối tượng | Góc nhìn kiểm tra / cách kiểm |
|---|---|
| AC-1/AC-4 trên WebGL | Hoãn tới khi có `GameHost` + build WebGL: đóng tab ở 5 màn, sửa `run.json` trong IndexedDB |

**Truy vết**: R1 → `SaveMapperRunTest.RoundTrip_*`, `SaveSerializerTest.SerializeDeserialize_*`, `BrewLogRestoreTest`,
`BattleLogRestoreTest`; R2 → `ToDto_AnyRun_ContainsNoContentIndex`, `TryToRunState_Unknown*`; R3 → `SaveMapperMetaTest`;
R4 → `GameSessionSaveTest.Execute_*`; R5 → `GameSessionLoadTest.Load_AfterPhase_*`, `Load_ClosedDuringBrewReplay_*`,
`Load_ThenContinue_*`; R6 → `Execute_RunEnds_*`, `Load_EndedRunLeftOver_*`; R7 → `Load_CorruptRun_*`,
`SaveSerializerTest.TryDeserialize_*`, `SaveMapperRunTest.TryToRunState_*`; R8 → `Load_CorruptMeta*`; R9 →
`SaveStoreContractTest`, `FileSaveStoreTest`; R10 → `SaveMigratorTest`, `Load_CorruptRun_*(version 99)`; R11 →
`StartNewRun_*`; R12 → `SaveRoundTripFuzzTest`.
**Testability**: PASS — mọi phụ thuộc inject qua constructor; không Unity API; file I/O cô lập trong thư mục tạm.
WARN nhỏ: `File.Replace` trên IndexedDB WebGL chỉ kiểm được khi build (Rủi ro).

## 4. Các bước thực hiện

> Commit theo `CLAUDE.md` §Git: tự động **không**, test-first **không** → gộp test vào commit cùng phần sửa khi xanh.

- [ ] **Bước 1 — Skeleton**: `BrewLog.Restore`/`BattleLog.Restore` (ném `NotImplementedException`), toàn bộ type §2 (thân
  rỗng / trả mặc định), `TestUtils/{FixedClock,FixedSeedSource,SaveFixtures}` (dựng run ở từng phase từ `CreateFullRun` +
  `RandomAgent`). Kiểm: batch EditMode compile sạch, 1024 test cũ xanh.
- [ ] **Bước 2 — Test first**: viết test §3; chạy EditMode, xác nhận test mới fail đúng lý do (không fail vì lỗi arrange),
  1024 cũ xanh. Ghi số fail/pass vào §6.
- [ ] **Bước 3 — Implement** theo thứ tự: Restore (Core) → serializer + migrator → mapper → stores → `GameSession`. Kiểm:
  toàn bộ EditMode xanh; fuzz ≤ 5 s (đọc thời gian trong `Logs/editmode.xml`); 0 warning CS mới;
  `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .` 0 lỗi.
- [ ] **Bước 4 — Refactor**: gộp test theo `TestCase`, tách helper lặp ở mapper; test vẫn xanh.
- [ ] **Bước 5 — Tài liệu**: TECH §3 (Phiên chơi → Implemented, file Runtime/Save + App), §3.6 API thật, §4 (D3/D6/D9),
  §8 nợ (`link.xml`, IndexedDB); GDD §6.4 tick AC-2, AC-4 (Editor), ghi AC-1/AC-3 còn lại; PLAN F-013 → Done; TEST.md;
  LOG + CHECKPOINT.

Lệnh test (Linux, Editor đóng):
```bash
UNITY="$HOME/Unity/Hub/Editor/6000.6.7f1/Editor/Unity"
"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml -logFile -
```

## 5. Rủi ro & đánh đổi

- **IL2CPP stripping**: Newtonsoft + DTO cần `link.xml` (TECH §5) — chỉ lộ khi build WebGL; ghi nợ kỹ thuật, làm ở build đầu.
- **`File.Replace` trên WebGL/IndexedDB** chưa kiểm; fallback dự kiến: ghi tmp → `Delete` → `Move` (mất tính atomic ở một
  cửa sổ nhỏ) — quyết định khi build. Đồng bộ IndexedDB vẫn TBD.
- **Ghi mỗi lệnh**: run JSON ước ~10–30 KB; ngân sách `Execute + lưu ≤ 5 ms` (TECH §6) đo khi có Profiler trên WebGL.
- **Đổi content giữa phiên bản** (xóa/đổi `Id`) làm save run thành hỏng → run mất. Chấp nhận (ADR-005 cấm đổi `Id` đã phát hành).
- **Validate cấu trúc, không phải luật** (D10): save sửa tay "hợp lệ cấu trúc nhưng sai luật" (vd vàng 99999) vẫn được nhận
  — chấp nhận, không có chống gian lận ở game single-player.
- `GameSession` lộ `StateHash` thay vì `RunView` tới khi F-014 — Presentation chưa tồn tại nên chưa vi phạm ADR-007.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**: - None
- **Deviations**: - None
- **Tradeoffs**: - None
- **Test changes**: - None
- **Open questions**: - None
