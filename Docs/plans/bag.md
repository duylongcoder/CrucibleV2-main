# Plan: Túi (F-005)

> **Trạng thái**: Done (D1–D3, R5 duyệt 2026-11-04; 312/312 EditMode xanh) · **Cập nhật**: 2026-11-04
> **GDD**: `design/economy.md` §Túi, §Xúc tác (thứ tự trong Túi), §Edge Cases, AC-2/AC-2b; `design/brewing.md` §Lưới
> (lưới tính vào sức chứa); `design/run-flow.md` §Chiến lợi phẩm (Nới Túi, "Túi ở trần"); `design/ui-ban-luyen.md`
> §7 Túi, §6 Kệ Xúc tác · **ADR**: ADR-001 (Command/Reducer), ADR-005 (content trong Core) · **Loại**: Logic (Core)
> Xây trên F-000…F-004 (255/255 EditMode xanh).

## 1. Bối cảnh

Túi là kho duy nhất của run; mọi hệ thống đưa vật phẩm vào (Chợ F-006, Xúc tác `CreateIngredient` F-007, Chiến lợi
phẩm F-011) đều phải hỏi "còn chỗ không?" theo **cùng một luật**: Túi và lưới dùng chung sức chứa. Hiện state đã có
`RunState.Bag` (thứ tự list = thứ tự Xúc tác) và `BagCapacity`, nhưng chưa có luật đếm chỗ, chưa có trần sức chứa,
chưa sắp xếp được Xúc tác. Plan này gom luật Túi vào một chỗ (`BagRules`) để F-006/F-007/F-011 chỉ việc gọi. P3.

**Yêu cầu** (đánh số để truy vết):
- R1: Chỗ đã dùng = số vật phẩm trong Túi + số vật phẩm trên lưới; mỗi vật phẩm chiếm đúng 1 chỗ (không gộp chồng,
  kể cả cùng `ItemId`). Chỗ trống = sức chứa − chỗ đã dùng, không âm.
- R2: `HasRoom(n)` = còn ≥ n chỗ trống — hợp đồng duy nhất cho mọi luật thêm vật phẩm vào Túi (Mua, `CreateIngredient`,
  chiến lợi phẩm). n < 1 là lỗi lập trình (exception).
- R3: Đặt / gỡ / di chuyển trên lưới và Nung **không bao giờ** làm chỗ đã dùng vượt sức chứa (vật phẩm chỉ đổi vùng;
  Nung tiêu hao ≥ 1, thêm đúng 1). Bất biến kiểm bằng fuzz.
- R4: Sức chứa có trần `MaxBagCapacity` (content `RunData`, mặc định GDD 24). Nới Túi `Expand(amount)` cộng tối đa tới
  trần, trả số chỗ thực sự thêm; `CanExpand` = chưa ở trần (run-flow: lựa chọn Nới Túi vô hiệu khi Túi ở trần).
  amount < 1 là lỗi lập trình.
- R5: Content hợp lệ: `1 ≤ StartingBagCapacity ≤ MaxBagCapacity`; mọi bộ khởi đầu có số vật phẩm ≤
  `StartingBagCapacity` → vi phạm = `ArgumentException` khi tạo `ContentDb` (giải câu hỏi mở "bộ khởi đầu > sức chứa
  Túi?" — **đề xuất**: lỗi content, không cắt bớt).
- R6: Command `ReorderCatalyst(uid, toIndex)` đổi vị trí một Xúc tác trong **thứ tự Xúc tác** (vị trí trên kệ,
  0 = kích hoạt đầu tiên); vật phẩm không phải Xúc tác giữ nguyên chỗ trong list `Bag`; số vật phẩm và sức chứa không
  đổi; phát `CatalystsReordered`; không rút random.
- R7: `ReorderCatalyst` bị từ chối, state không đổi, khi: chưa có run / sai phase / không ở bước Editing (cùng
  `GridRules.ValidateEditable` — kệ không nhận input khi phát lại); uid không có trong Túi (`ItemNotFound`); vật phẩm
  không phải Xúc tác (`ItemNotCatalyst`); `toIndex` ngoài `[0, số Xúc tác)` hoặc bằng vị trí hiện tại
  (`InvalidCatalystIndex` — cùng cách `MoveCell` từ chối ô trùng).

**Ngoài phạm vi**: `Buy`/`Sell` + từ chối `BagFull` + AC-2/AC-2b đầy đủ (F-006 — dùng `HasRoom`); `CatalystSlots` (5)
và chặn Xúc tác thứ 6 (F-006/F-007); chạy Effect theo thứ tự Xúc tác (F-007); phần thưởng Nới Túi + lọc lựa chọn
(F-011 — dùng `CanExpand`/`Expand`); `SkipTurn` trả lưới về Túi (F-009); SO `RunDef` + `ContentBaker` (Content
chưa có code); view Túi 24 ô / kệ / header "x/sức chứa · n trên lò" (F-014 — đọc `UsedSlots`, `GridRules.CountItems`,
`MaxBagCapacity`); save DTO (F-013 — không có field state mới).
**TBD (không làm)**: không có.

**Quyết định (người dùng duyệt 2026-11-04)**:
- D1: **Chỉ Xúc tác sắp được thứ tự** (`ReorderCatalyst` như TECH §3.3, kệ ui-ban-luyen §6). economy.md §UI ghi "Túi:
  kéo để sắp thứ tự" — thứ tự các vật phẩm khác không ảnh hưởng luật → để view tự sắp (không lưu, không hash).
- D2: `ReorderCatalyst` chỉ trong Workbench · Editing (giống lưới). Thứ tự cho `OnShopRefresh` đầu lượt kế = thứ tự
  chốt ở lượt trước.
- D3: Không thêm `RejectReason.BagFull` ở F-005 (chưa có Command nào dùng) — F-006 thêm cùng `Buy`.

## 2. Thiết kế triển khai

| Class / file (dưới `Assets/_Project/Scripts/`) | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Rules/BagRules.cs` | đếm chỗ (`UsedSlots`, `FreeSlots`, `HasRoom`), trần (`CanExpand`, `Expand`), luật `ReorderCatalyst` | Core | Mới |
| `Core/Commands/ReorderCatalyst.cs` | Command `(int Uid, int ToIndex)` | Core | Mới |
| `Core/Commands/CatalystsReordered.cs` | sự kiện (không payload — view đọc lại Túi) | Core | Mới |
| `Core/Commands/RejectReason.cs` | + `ItemNotCatalyst = 13`, `InvalidCatalystIndex = 14` | Core | Sửa |
| `Core/Rules/GridRules.cs` | + `public static int CountItems(RunState run)` (số ô có vật phẩm) | Core | Sửa |
| `Core/Rules/GameRules.cs` | dispatch `ReorderCatalyst` → `BagRules.ReorderCatalyst` | Core | Sửa |
| `Core/Content/RunData.cs` | + `MaxBagCapacity` (ctor: `startingReputation, startingBagCapacity, maxBagCapacity, seasonCount`) | Core | Sửa |
| `Core/Content/ContentDb.cs` | validate R5 trong `Create` | Core | Sửa |
| `Core/Rules/BrewingRules.cs` | (Bước 4) `IsGridEmpty` → `GridRules.CountItems(run) == 0` | Core | Sửa |

**Public API**
```csharp
public static class BagRules
{
    public static int UsedSlots(RunState run);                 // run.Bag.Count + GridRules.CountItems(run)
    public static int FreeSlots(RunState run);                 // max(0, run.BagCapacity − UsedSlots)
    public static bool HasRoom(RunState run, int count = 1);   // count < 1 → ArgumentOutOfRangeException
    public static bool CanExpand(RunState run, ContentDb content);           // BagCapacity < Run.MaxBagCapacity
    public static int Expand(RunState run, ContentDb content, int amount);   // mutate; trả số chỗ thêm (0 khi ở trần); amount < 1 → ArgumentOutOfRangeException
    public static CommandResult ReorderCatalyst(RunState run, ContentDb content, ReorderCatalyst command);
}
public sealed class ReorderCatalyst : ICommand { public ReorderCatalyst(int uid, int toIndex); int Uid; int ToIndex; }
```

**Thuật toán `ReorderCatalyst`** (validate trước, mutate sau; không alloc theo frame — chỉ chạy khi có input):
1. `ValidateEditable` → tìm `bagIndex` theo uid → kiểm `Kind == Catalyst` → đếm Xúc tác `n` và vị trí hiện tại `from`
   (thứ tự xuất hiện trong `Bag`) → kiểm `0 ≤ toIndex < n` và `toIndex ≠ from`.
2. Ghi lại các chỉ số list đang chứa Xúc tác `p[0..n)` (tăng dần) và dãy Xúc tác `c[0..n)`; chuyển `c[from]` tới
   `toIndex` (dịch các phần tử giữa); ghi `c[k]` vào `Bag[p[k]]`. → vật phẩm khác không đổi chỉ số.

**Interface phụ thuộc**: không (luật static thuần, không RNG).
**Dữ liệu**: `RunData.MaxBagCapacity` (content; SO `RunDef` sau). Mặc định GDD: khởi đầu 14 / trần 24. `ContentFactory`
test: 8 / 12 (giữ 8 cho test hiện có).
**State / hash**: không field mới — `BagCapacity` và thứ tự `Bag` đã được `StateHasher` ghi.
**Scene / prefab bị chạm**: không.
**Lý do thiết kế**: một nguồn sự thật cho "còn chỗ" (tránh mỗi luật tự đếm và quên lưới — đúng lỗi R1 của review
2026-11-03); `HasRoom` chỉ là truy vấn để luật gọi giữ kỷ luật validate-trước-mutate-sau (không có `TryAdd` mutate
lẫn validate). Sắp theo vị trí trên kệ (không theo chỉ số `Bag`) vì UI chỉ biết kệ 5 ô.

## 3. Test cases

### Editor tests
(none)

### Unit tests
#### BagRulesTest (`Assets/Tests/EditMode/Core/Rules/BagRulesTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `UsedSlots_BagAndGridHaveItems_CountsBoth` (bag 3, lưới 2) | 5 |
| `UsedSlots_SameItemIdTwice_CountsEachInstance` | 2 |
| `UsedSlots_EmptyBagAndGrid_ReturnsZero` | 0 |
| `FreeSlots_BelowCapacity_ReturnsCapacityMinusUsed` | 8 − 5 = 3 |
| `FreeSlots_AtCapacity_ReturnsZero` | 0 |
| `FreeSlots_UsedAboveCapacity_ReturnsZero` (state dựng tay vượt) | 0 (không âm) |
| `HasRoom_FreeSlotsEqualCount_ReturnsTrue` (count: {1, 3}) | true |
| `HasRoom_FreeSlotsBelowCount_ReturnsFalse` (count: {1, 4}, free = count − 1) | false |
| `HasRoom_GridFillsRemainingCapacity_ReturnsFalse` | false (lưới tính vào sức chứa) |
| `HasRoom_CountBelowOne_Throws` (count: {0, −1}) | `ArgumentOutOfRangeException` |
| `CanExpand_BelowMax_ReturnsTrue` | true |
| `CanExpand_AtMax_ReturnsFalse` | false |
| `Expand_WithinMax_AddsFullAmount` (8 + 2, trần 12) | trả 2; `BagCapacity` = 10 |
| `Expand_PastMax_ClampsToMax` (11 + 3) | trả 1; `BagCapacity` = 12 |
| `Expand_AtMax_ReturnsZero` | trả 0; `BagCapacity` = 12 |
| `Expand_AmountBelowOne_Throws` (amount: {0, −1}) | `ArgumentOutOfRangeException`; `BagCapacity` không đổi |

#### GameRulesReorderCatalystTest (`.../Core/Rules/GameRulesReorderCatalystTest.cs`)
Túi mẫu: `[Sắt, C1, Than, C2, C3]` (uid 1–5; C1–C3 = 3 Xúc tác).

| Test method | Kiểm tra |
|---|---|
| `Apply_MoveCatalystForward_ReordersCatalystsOnly` (C1 → 2) | `Bag` uid = `[1, 4, 3, 5, 2]`; Accepted; sự kiện `CatalystsReordered` |
| `Apply_MoveCatalystBackward_ReordersCatalystsOnly` (C3 → 0) | `Bag` uid = `[1, 5, 3, 2, 4]` |
| `Apply_MoveCatalystToAdjacent_SwapsPair` (C2 → 0) | `Bag` uid = `[1, 4, 3, 2, 5]` |
| `Apply_ReorderCatalyst_KeepsNonCatalystIndices` | Sắt ở chỉ số 0, Than ở chỉ số 2 |
| `Apply_ReorderCatalyst_KeepsCountsAndCapacity` | `Bag.Count`, `BagCapacity`, lưới không đổi |
| `Apply_ReorderCatalyst_DoesNotDrawRandom` | trạng thái 7 stream RNG không đổi |
| `Apply_ReorderCatalystSameIndex_RejectsWithInvalidCatalystIndex` | Rejected `InvalidCatalystIndex`; hash không đổi |
| `Apply_ReorderCatalystIndexOutOfRange_RejectsWithInvalidCatalystIndex` (toIndex: {−1, 3}) | Rejected `InvalidCatalystIndex`; hash không đổi |
| `Apply_ReorderCatalystNotCatalyst_RejectsWithItemNotCatalyst` (uid Sắt) | Rejected `ItemNotCatalyst`; hash không đổi |
| `Apply_ReorderCatalystUidOnGrid_RejectsWithItemNotFound` | Rejected `ItemNotFound` |
| `Apply_ReorderCatalystUnknownUid_RejectsWithItemNotFound` | Rejected `ItemNotFound` |
| `Apply_ReorderCatalystWhenResultPending_RejectsWithGridLocked` | Rejected `GridLocked`; hash không đổi |
| `Apply_ReorderCatalystOutsideWorkbench_RejectsWithWrongPhase` | Rejected `WrongPhase` |
| `Apply_ReorderCatalystWithoutRun_RejectsWithRunNotActive` | Rejected `RunNotActive` |

#### ContentDbBagTest (`.../Core/Content/ContentDbBagTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Create_StartingCapacityEqualsMax_Succeeds` | không ném; `Run.MaxBagCapacity` đọc đúng |
| `Create_StartingCapacityAboveMax_Throws` | `ArgumentException` |
| `Create_StartingCapacityBelowOne_Throws` (capacity: {0, −1}) | `ArgumentException` |
| `Create_StarterKitFillsCapacity_Succeeds` (kit = capacity) | không ném |
| `Create_StarterKitExceedsCapacity_Throws` (kit = capacity + 1) | `ArgumentException`, message chứa Id bộ khởi đầu |

#### GridRules (bổ sung vào `GameRulesGridTest` hoặc `GridRulesCountTest`)
| Test method | Kiểm tra |
|---|---|
| `CountItems_GridWithGaps_CountsOccupiedCells` (ô 0, 4, 8) | 3 |
| `CountItems_EmptyGrid_ReturnsZero` | 0 |

### Integration tests
#### BagCapacityInvariantTest (`.../Core/Rules/BagCapacityInvariantTest.cs`) — qua `GameRules.Apply`
| Test method | Kiểm tra |
|---|---|
| `Apply_RandomGridAndBrewCommands_NeverExceedCapacity` (5 seed × 2.000 lệnh; Túi + lưới = sức chứa lúc đầu) | sau mỗi lệnh: `UsedSlots ≤ BagCapacity` |
| `Apply_RandomGridCommands_ConservesItems` (cùng fuzz, không Nung) | tập uid Túi ∪ lưới không đổi, không trùng |
| `Apply_PlaceAndRemoveAtFullCapacity_KeepsUsedSlots` | `UsedSlots` trước = sau |
| `Apply_BrewAtFullCapacity_LeavesRoomForResult` (lưới 2 vật phẩm) | `UsedSlots` = sức chứa − 1; kết quả trong Túi |
| `Apply_ReorderCatalystThenBrew_KeepsCatalystOrder` | thứ tự Xúc tác sau Nung = thứ tự đã sắp (kết quả thêm ở cuối) |

Fuzz: `System.Random(seed)` chọn `PlaceItem`/`RemoveFromGrid`/`MoveCell`/`Brew`/`ReorderCatalyst` với tham số ngẫu nhiên
(cả uid/ô sai để có lệnh bị từ chối); sau `Brew` test tự đặt `WorkbenchStep = Editing` (giả `AckBrewResult` — F-009,
ghi chú trong test).

#### GameRulesStartRunTest (bổ sung)
| Test method | Kiểm tra |
|---|---|
| `Apply_StartRun_BagWithinCapacity` | `UsedSlots ≤ BagCapacity`; `BagCapacity ≤ MaxBagCapacity` |

### Visual verification tests
(none — chưa có view; F-014)

### Manual tests
(none)

**Truy vết**: R1 → `UsedSlots_*`, `FreeSlots_*`, `CountItems_*` · R2 → `HasRoom_*` · R3 → `BagCapacityInvariantTest.*`
· R4 → `CanExpand_*`, `Expand_*`, `Apply_StartRun_BagWithinCapacity` · R5 → `ContentDbBagTest.*` · R6 →
`Apply_MoveCatalyst*`, `Apply_ReorderCatalyst_Keeps*`, `Apply_ReorderCatalyst_DoesNotDrawRandom`,
`Apply_ReorderCatalystThenBrew_KeepsCatalystOrder` · R7 → `Apply_ReorderCatalyst*_RejectsWith*`
**Testability**: **PASS** — luật static thuần, state dựng bằng `StateFactory`, không RNG trong `BagRules`; fuzz dùng
seed cố định.

## 4. Các bước thực hiện

> Commit theo `CLAUDE.md` §Git: Commit tự động **không**, test-first **không** → đề xuất message, gộp test vào commit
> khi xanh. Lệnh test (Editor đóng):
> `"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml -logFile -`

- [x] **Bước 1 — Skeleton**: `BagRules` (thân trả mặc định / `CommandResult.Reject(UnknownCommand)`), `ReorderCatalyst`,
  `CatalystsReordered`, 2 `RejectReason`, `GridRules.CountItems` (trả 0), dispatch trong `GameRules`,
  `RunData.MaxBagCapacity` + sửa mọi chỗ gọi ctor (`ContentFactory`, `ContentDbTest`, `ContentDbMagicTest`,
  `ContentDbRecipeTest`, `GameRulesBrewMagicTest`). `ContentFactory`: + `MaxBagCapacity = 12`, + 2 Xúc tác
  (`cat_mortar`, `cat_tongs`). **Kiểm**: compile sạch, 255/255 cũ xanh (thêm item đổi chỉ số `ItemId` — test dùng
  `ItemId` cứng phải vẫn xanh; nếu đỏ thì sửa test dùng `IdOf`).
- [x] **Bước 2 — Test first**: viết test §3; chạy → test mới **fail đúng lý do** (giá trị mặc định / Reject sai lý
  do). Dự đoán pass tầm thường: `UsedSlots_EmptyBagAndGrid_ReturnsZero`, `CountItems_EmptyGrid_ReturnsZero`,
  `FreeSlots_AtCapacity/UsedAboveCapacity_ReturnsZero`, `HasRoom_*ReturnsFalse`, `CanExpand_AtMax`,
  `Expand_AtMax_ReturnsZero`, `*_Succeeds`, invariant tests (luật lưới đã đúng — chúng là regression guard) → ghi rõ
  trong báo cáo.
- [x] **Bước 3 — Implement**: `CountItems`, `BagRules`, validate `ContentDb`, `ReorderCatalyst`. **Kiểm**: toàn bộ
  EditMode xanh, 0 warning CS mới, `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .` 0 lỗi.
- [x] **Bước 4 — Refactor**: `BrewingRules.IsGridEmpty` → `GridRules.CountItems`; gộp test tham số hóa cùng phân vùng.
  **Kiểm**: EditMode xanh.
- [x] **Bước 5 — Tài liệu**: PLAN F-005 → Done; TECH §2 bảng (BagRules) + §3.3 (`ReorderCatalyst` đã có, ghi thứ tự
  theo kệ); economy.md §Túi ghi D1 (chỉ Xúc tác sắp thứ tự) + R5 vào Edge Cases; GDD Systems Index #5; CHECKPOINT.
  Đề xuất commit `feat(bag): add shared bag capacity rules and ReorderCatalyst`.

## 5. Rủi ro & đánh đổi

- Thêm 2 Xúc tác vào `ContentFactory` dịch `ItemId` của item xếp sau `cat_*` (ordinal) → test dùng chỉ số cứng có thể
  đổi nghĩa (`StateFactory.CreateActiveRun` dùng `ItemId(0..2)` — vẫn hợp lệ). Kiểm ở Bước 1.
- `Expand` / `CanExpand` / `HasRoom` chưa có caller production tới F-006/F-011 — chấp nhận vì là luật Túi theo GDD và
  được test trực tiếp; nếu F-011 cần ngữ nghĩa khác (vd Nới Túi bị từ chối thay vì cắt tại trần) thì sửa ở đó.
- Ctor `RunData` toàn `int` theo vị trí → dễ truyền nhầm thứ tự; chèn tham số giữa làm mọi chỗ gọi cũ lỗi compile (3
  → 4 tham số) nên không lọt im lặng.
- UI layout cố định 24 ô (ui-ban-luyen L5): Core không chặn `MaxBagCapacity > 24` — kiểm ở F-014 / validator content.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - `BagRules.IndexOfUid` là `public` (không `internal`): Core không có `InternalsVisibleTo`, và `GridRules.Place` +
    `ReorderCatalyst` (và sau này `Sell`, `Inspect`) cùng cần tìm uid trong Túi.
  - `ReorderCatalyst` alloc một mảng `positions` (≤ số Xúc tác) mỗi lệnh — chỉ chạy theo input, không phải vòng lặp nóng.
  - `ContentDb` validate bộ khởi đầu theo **số Id** (`ItemIds.Count`), trước khi resolve Id — Id lạ vẫn để `StartRun`
    từ chối `UnknownContent` như cũ.
- **Deviations**:
  - Bước 4 thêm việc ngoài plan (trong phạm vi Túi): gộp `GridRules.IndexOfUid` (private) vào `BagRules.IndexOfUid`.
  - `GridRulesTest.cs` là file mới (plan để ngỏ "bổ sung vào `GameRulesGridTest` hoặc file riêng").
  - `Apply_StartRun_BagWithinCapacity` kiểm `UsedSlots ∈ [1, BagCapacity]` (plan: `≤`) để test fail được ở skeleton.
- **Tradeoffs**: `Expand`/`CanExpand`/`HasRoom` chưa có caller production (F-006/F-011) — đã test trực tiếp.
- **Test changes** (trước khi implement, chưa commit): sau lần chạy test-first, 4 test pass ở skeleton vì
  `ReorderCatalyst` no-op giữ nguyên state → thêm điều kiện `result.Accepted` cho `KeepsNonCatalystIndices`,
  `KeepsCountsAndCapacity`, `DoesNotDrawRandom`; `ReorderCatalystThenBrew_KeepsCatalystOrder` so với thứ tự cứng
  `[6, 2, 4]` thay vì thứ tự đọc lại sau khi sắp. Không chạy lại riêng để xác nhận fail (suy luận: skeleton trả
  `Rejected(UnknownCommand)` và không đổi thứ tự); sau implement cả 4 xanh.
  Pass ở skeleton như dự đoán: fuzz bất biến (10), `PlaceAndRemoveAtFullCapacity`, `UsedSlots_Empty`, `CountItems_Empty`,
  `FreeSlots_AtCapacity/UsedAboveCapacity`, `HasRoom_*ReturnsFalse` (3), `CanExpand_AtMax`, `Expand_AtMax`, `*_Succeeds` (2).
- **Open questions**: - None
