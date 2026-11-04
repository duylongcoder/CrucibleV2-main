# Plan: Lò nung — lưới, chuỗi nạp, khớp công thức, `Quality` (F-002)

> **Trạng thái**: Implemented (duyệt khi gọi `build brewing`) · **Cập nhật**: 2026-11-04
> **GDD**: `design/brewing.md` §Luật cốt lõi, §Formulas, §Edge Cases · **ADR**: ADR-001, ADR-004, ADR-005 · **Loại**: Logic
> **TECH**: §3.1 (Recipe trong ContentDb), §3.2 (`Grid`, `WorkbenchStep`), §3.3 (`PlaceItem · MoveCell · RemoveFromGrid ·
> Brew`). Xây trên `plans/core-foundation.md` (F-000/F-001 — đã xanh, chưa commit).

## 1. Bối cảnh

Lò nung là cơ chế trung tâm (P1 khám phá, P2 luật tất định đọc được): người chơi xếp vật phẩm lên lưới 3×3 — một dòng
thời gian 9 nhịp — rồi Nung; chuỗi nhịp khớp đúng một công thức (nguyên liệu, thứ tự, **khoảng cách**) → thành phẩm,
không khớp → phế phẩm. Plan này thêm lưới, 4 Command, `RecipeMatcher`, `QualityFormula` và công thức trong `ContentDb`.
Các lớp sau (F-003 ma thuật, F-004 BrewLog, F-007 Xúc tác, F-009 lượt/Mùa) chỉ cắm thêm vào `BrewingRules`.

**Yêu cầu** (đánh số để truy vết):
- R1: `RunState.Grid` 9 ô (null = trống). `PlaceItem(uid, cell)`: vật phẩm rời Túi vào ô; ô đã có vật phẩm → vật phẩm cũ
  về **cuối** Túi (thay). Chỉ đặt được `Mineral` / `Additive`; `Scrap`, `Lens`, `Catalyst` → `ItemNotPlaceable`;
  `uid` không có trong Túi → `ItemNotFound`.
- R2: `RemoveFromGrid(cell)`: vật phẩm về cuối Túi; ô trống → `CellEmpty`.
- R3: `MoveCell(from, to)`: `to` trống → chuyển; `to` có vật phẩm → đổi chỗ; `from` trống → `CellEmpty`; `from == to`
  → `InvalidCell`.
- R4: Lệnh lưới và `Brew` cần run đang chơi (`RunNotActive`), `Phase == Workbench` (`WrongPhase`), `WorkbenchStep ==
  Editing` (`GridLocked` — AC-8); chỉ số ô ngoài `[0, 8]` → `InvalidCell`. Thứ tự kiểm: run → phase → step → tham số.
- R5: `FeedSequence` = ô 0…8 theo hàng (trái → phải, trên → dưới).
- R6: `RecipeMatcher` theo brewing.md §Khớp công thức: bỏ ô trống đầu; slot nguyên liệu đúng item (và `Magic` nếu
  `RequiresMagic`); slot `Gap` trống hoặc phụ gia thuộc `AllowedAdditives`; mọi ô sau pattern trống; pattern vượt ô 8 →
  không khớp (AC-2, AC-3, AC-3b, AC-5; Edge Cases).
- R7: `Brew` lưới trống → `EmptyGrid` (AC-10). Lưới ≥ 1 vật phẩm → **mọi** vật phẩm trên lưới bị tiêu hao, đúng **1**
  kết quả vào cuối Túi với `Uid = NextUid` (AC-9); khớp → thành phẩm `Quality = QualityFormula(BaseEssence, 0, ∅)`,
  `Magic = false`; không khớp → phế phẩm `Quality = 0`. Sau đó `WorkbenchStep = ResultPending`, `LastBrewResultUid` =
  uid kết quả, sự kiện `BrewCompleted(resultUid)`. Phụ gia hợp lệ trong khoảng trống → cùng thành phẩm, cùng `Quality`
  (AC-4 phần thành phẩm).
- R8: `QualityFormula.Compute(essence, addPotency, mulPercents, cap)` số nguyên (`long`, chốt 2026-11-04):
  `Q = essence × (1 + addPotency)`, rồi mỗi `mul%` sắp tăng dần `Q = ⌊Q × mul / 100⌋`, mỗi bước `min(cap, Q)`; không
  phụ thuộc thứ tự đầu vào (AC-1 = 108). `essence < 0`, `addPotency < 0`, `mul ∉ [100, 500]` → `ArgumentOutOfRangeException`.
- R9: `ContentDb` phân giải công thức (`ItemData.Recipe`) → `Recipe` (ItemId), xếp theo `ItemId` thành phẩm. Content
  sai → `ArgumentException`: pattern rỗng / > 9 slot; slot đầu/cuối là `Gap`; nguyên liệu lạ hoặc không phải `Mineral`;
  phụ gia lạ hoặc không phải `Additive`; hai công thức **cùng hình** (cùng item ở cùng vị trí, cùng vị trí `Gap`, bỏ qua
  `RequiresMagic` — "không chuỗi nào khớp 2 công thức"); > 1 item `Scrap`. `BrewingData.QualityCap` ∈ `[0, long.MaxValue / 500]`.
- R10: Command bị từ chối → state không đổi (hash). `StateHasher` bắt khác biệt ở `Grid`, `WorkbenchStep`,
  `LastBrewResultUid`, `ItemInstance.Magic`, `ItemInstance.Quality`.

**Ngoài phạm vi**: tung ma thuật + `MagicChance` (F-003 — `Brew` ở plan này **không** rút RNG); dòng thuộc tính
`Lines` / `RevealedMask` (F-008); `BrewLog`, phát lại, tua (F-004 — AC-7, AC-11); Effect Xúc tác + Add/MulPotency thật
(F-007 — `Brew` gọi `QualityFormula` với 0 / ∅); `SkipTurn`, `AckBrewResult`, đếm lượt, `TurnEnd` (F-009 — AC-10 vế
"Bỏ qua luôn hợp lệ"); soi / mua bán bị khóa trong Brewing (AC-8 vế soi/mua — test ở F-006/F-008); mở khóa Sổ công thức
(F-012); `ContentBaker` SO; Túi đầy (lưới chỉ rút/trả, kết quả luôn vừa — kiểm sức chứa ở Chợ F-006).
**TBD (không làm)**: không có. Open Question brewing.md "Nung khi lưới chỉ có phụ gia" → theo Edge Cases: **cho phép**,
ra phế phẩm.

## 2. Thiết kế triển khai

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Content/ItemKind.cs` | + `Scrap` | Core | Sửa |
| `Core/Content/ItemData.cs` | + `RecipeData Recipe` (null = nguyên liệu gốc), ctor tham số tùy chọn | Core | Sửa |
| `Core/Content/RecipeData.cs`, `RecipeSlotData.cs` | công thức dạng Id chuỗi (đầu vào content) | Core | Mới |
| `Core/Content/Recipe.cs`, `RecipeSlot.cs` | công thức đã phân giải (`ItemId`), `IsAllowedAdditive` | Core | Mới |
| `Core/Content/BrewingData.cs` | `QualityCap` (config toàn game) | Core | Mới |
| `Core/Content/ContentDb.cs` | + tham số `brewing`; `Recipes`, `Brewing`, `HasScrap`, `ScrapItemId`; validate R9 | Core | Sửa |
| `Core/State/ItemInstance.cs` | + `Magic`, `Quality` (ctor tùy chọn) | Core | Sửa |
| `Core/State/WorkbenchStep.cs` | enum `Editing · ResultPending` | Core | Mới |
| `Core/State/RunState.cs` | + `Grid` (`ItemInstance[9]`), `WorkbenchStep`, `LastBrewResultUid` | Core | Sửa |
| `Core/State/StateHasher.cs` | ghi field mới (R10) | Core | Sửa |
| `Core/Commands/PlaceItem.cs`, `RemoveFromGrid.cs`, `MoveCell.cs`, `Brew.cs` | Command | Core | Mới |
| `Core/Commands/GridChanged.cs`, `BrewCompleted.cs` | sự kiện | Core | Mới |
| `Core/Commands/RejectReason.cs` | + `RunNotActive, WrongPhase, GridLocked, InvalidCell, ItemNotFound, ItemNotPlaceable, CellEmpty, EmptyGrid` | Core | Sửa |
| `Core/Rules/GridRules.cs` | `CellCount = 9`; Place / Remove / Move; `ValidateEditable` dùng chung với Brew | Core | Mới |
| `Core/Rules/RecipeMatcher.cs` | `Match(grid, content) → Recipe` (null = không khớp) | Core | Mới |
| `Core/Rules/QualityFormula.cs` | R8 | Core | Mới |
| `Core/Rules/BrewingRules.cs` | luật `Brew` (R7) | Core | Mới |
| `Core/Rules/GameRules.cs` | dispatch 4 Command mới | Core | Sửa |
| `Core/Rules/RunStartRules.cs` | đặt `Grid` trống, `WorkbenchStep = Editing`, `LastBrewResultUid = 0` | Core | Sửa |

**Public API**
```csharp
public enum ItemKind { Mineral, Additive, Lens, Catalyst, Scrap }
public sealed class ItemData { public ItemData(string id, ItemKind kind, int tier, RecipeData recipe = null); public RecipeData Recipe { get; } }
public sealed class RecipeSlotData {
    public static RecipeSlotData Gap { get; }
    public static RecipeSlotData Ingredient(string itemId, bool requiresMagic = false);
    public bool IsGap { get; }  public string ItemId { get; }  public bool RequiresMagic { get; }
}
public sealed class RecipeData {
    public RecipeData(IReadOnlyList<RecipeSlotData> pattern, IReadOnlyList<string> allowedAdditives, long baseEssence);
}
public readonly struct RecipeSlot { public bool IsGap { get; } public ItemId Item { get; } public bool RequiresMagic { get; } }
public sealed class Recipe {
    public ItemId Product { get; }  public IReadOnlyList<RecipeSlot> Pattern { get; }  public long BaseEssence { get; }
    public bool IsAllowedAdditive(ItemId item);
}
public sealed class BrewingData { public BrewingData(long qualityCap); public long QualityCap { get; } }
public sealed class ContentDb {
    public static ContentDb Create(IReadOnlyList<ItemData> items, IReadOnlyList<DifficultyData> difficulties,
                                   IReadOnlyList<StarterKitData> kits, RunData run, BrewingData brewing);   // + brewing
    public IReadOnlyList<Recipe> Recipes { get; }  public BrewingData Brewing { get; }
    public bool HasScrap { get; }  public ItemId ScrapItemId { get; }
}
public sealed class ItemInstance { public ItemInstance(int uid, ItemId itemId, bool magic = false, long quality = 0); public bool Magic { get; } public long Quality { get; } }
public sealed class RunState { public ItemInstance[] Grid { get; }  public WorkbenchStep WorkbenchStep { get; set; }  public int LastBrewResultUid { get; set; } }
public sealed class PlaceItem : ICommand { public PlaceItem(int uid, int cell); }
public sealed class RemoveFromGrid : ICommand { public RemoveFromGrid(int cell); }
public sealed class MoveCell : ICommand { public MoveCell(int fromCell, int toCell); }
public sealed class Brew : ICommand { public Brew(long nowUnix); }
public sealed class GridChanged : GameEvent { }
public sealed class BrewCompleted : GameEvent { public BrewCompleted(int resultUid); public int ResultUid { get; } }
public static class RecipeMatcher { public static Recipe Match(IReadOnlyList<ItemInstance> grid, ContentDb content); }
public static class QualityFormula { public static long Compute(long essence, long addPotency, IReadOnlyList<int> mulPercents, long cap); }
```

**Interface phụ thuộc**: không — luật static, content/state truyền tham số; `Brew` không rút RNG ở plan này.
**Dữ liệu**: `BrewingData.QualityCap` mặc định 10¹⁵ (brewing.md §Formulas). Kích thước lưới 9 là hằng `GridRules.CellCount`
(brewing.md: "cố định MVP", không phải knob tuning). Chưa có SO.
**Scene / prefab bị chạm**: không.
**Lý do thiết kế**:
- `BrewingData` riêng thay vì nhét vào `RunData`: F-003 sẽ thêm trần ma thuật / truyền thừa vào cùng chỗ. Đổi chữ ký
  `ContentDb.Create` → cập nhật `ContentFactory` + `ContentDbTest.CreateWithItems` (test chưa commit; ghi §6 *Test changes*).
- `Brew(nowUnix)` giữ đúng chữ ký TECH §3.3 dù F-002 chưa dùng `nowUnix` (F-012 mở khóa Sổ cần nó).
- Nguyên liệu trong slot phải là `Mineral` → slot nguyên liệu và slot `Gap` không bao giờ nhận cùng một item, nên "khớp 2
  công thức" ⇔ "cùng hình" — validator đơn giản, đúng.
- Phế phẩm thiếu trong content khi cần → `InvalidOperationException` ở bước validate (lỗi content, không phải lệnh
  người chơi), trước khi mutate.
- `Quality` vật phẩm gốc = 0 (chỉ thành phẩm có `Quality` ý nghĩa; nguyên liệu gốc không dâng được — run-flow §Dâng).
- `MoveCell(from == to)` → `InvalidCell` thay vì chấp nhận no-op: không phát `GridChanged` thừa.
- `RecipeMatcher` lặp `content.Recipes` (mảng, thứ tự `ItemId`) — tất định, không lặp `Dictionary` (TECH §3.3).

## 3. Test cases

Tất cả EditMode (`Assets/Tests/EditMode/Core/...`), namespace theo thư mục. `ContentFactory` thêm content thử
nghiệm: `ore_iron`, `ore_coal` (Mineral), `add_ash` (Additive, được phép ở `Gap` của Gang), `add_sand` (Additive, không
được phép), `lens_basic`, `cat_bellows` (Catalyst), `scrap`; thành phẩm `ore_steel` = `[iron, coal]` (BaseEssence 18),
`ore_cast_iron` = `[iron, Gap, coal]` (BaseEssence 12, cho phép `add_ash`), `ore_starsteel` = `[steel(RequiresMagic), coal]`.
`StateFactory.CreateWorkbenchRun(content, params string[] bagItemIds)` dựng run `Phase = Workbench`, `Editing`.

### Editor tests
(none — `CoreAssemblyConstraintsTest` có sẵn phủ field mới)

### Unit tests
#### QualityFormulaTest
| Test method | Kiểm tra |
|---|---|
| `Compute_GddExample_Returns108` | 108 |
| `Compute_NoPotency_ReturnsEssence` | = essence |
| `Compute_MultipliersInDifferentOrder_ReturnsSameValue (order: [150,110], [110,150])` | hai kết quả bằng nhau |
| `Compute_MultipliersFlooredAscending_ReturnsFlooredValue` | essence 7, `[150, 110]` → 10 |
| `Compute_ResultAboveCap_ReturnsCap` | = cap |
| `Compute_EssenceTimesAddOverflowsLong_ReturnsCap` | = cap, không ném |
| `Compute_MultiplierOutsideRange_ThrowsArgumentOutOfRangeException (mul: 99, 501)` | ném |
| `Compute_NegativeEssenceOrAdd_ThrowsArgumentOutOfRangeException (essence, add: (-1, 0), (0, -1))` | ném |

#### RecipeMatcherTest
| Test method | Kiểm tra |
|---|---|
| `Match_PatternAtFirstCell_ReturnsRecipe` | Thép |
| `Match_LeadingEmptyCells_ReturnsRecipe` | Thép |
| `Match_PatternCrossesRowBoundary_ReturnsRecipe` | Thép (sắt ô 2, than ô 3) |
| `Match_IronCoalFamilyByGap_ReturnsVariant (gap: 0 → steel, 1 → cast_iron)` | đúng thành phẩm theo khoảng cách |
| `Match_IronCoalTwoGaps_ReturnsNull` | null |
| `Match_WrongIngredientOrder_ReturnsNull` | null |
| `Match_AllowedAdditiveInGap_ReturnsSameRecipe` | Gang |
| `Match_DisallowedAdditiveInGap_ReturnsNull` | null |
| `Match_AdditiveInIngredientSlot_ReturnsNull` | null |
| `Match_ItemAfterPattern_ReturnsNull` | null |
| `Match_PatternRunsPastLastCell_ReturnsNull` | null |
| `Match_OnlyAdditives_ReturnsNull` | null |
| `Match_RequiresMagicIngredientWithoutMagic_ReturnsNull` | null |
| `Match_RequiresMagicIngredientWithMagic_ReturnsRecipe` | Starsteel |

#### ContentDbRecipeTest
| Test method | Kiểm tra |
|---|---|
| `Create_ItemWithRecipe_ExposesResolvedRecipe` | `Product`, `Pattern` (item + `Gap`), `BaseEssence` khớp dữ liệu |
| `Create_InvalidRecipe_ThrowsArgumentException (case: empty, ten slots, leading gap, trailing gap, unknown ingredient, additive as ingredient, unknown additive, mineral as additive)` | ném `ArgumentException` |
| `Create_TwoRecipesSameShape_ThrowsArgumentException (requiresMagic differs: false, true)` | ném `ArgumentException` |
| `Create_TwoScrapItems_ThrowsArgumentException` | ném `ArgumentException` |
| `Create_OneScrapItem_ExposesScrapItemId` | `HasScrap` true, `GetItem(ScrapItemId).Kind` = `Scrap` |
| `Create_QualityCapOutsideRange_ThrowsArgumentOutOfRangeException (cap: -1, long.MaxValue / 500 + 1)` | ném |

#### StateHasherTest (sửa)
| Test method | Kiểm tra |
|---|---|
| `Hash_StatesDifferInOneField_ReturnsDifferentValue (mutation: + grid cell, workbench step, last brew uid, item magic, item quality)` | hai hash khác nhau |

### Integration tests (Core, qua `GameRules.Apply`)
#### GameRulesGridTest
| Test method | Kiểm tra |
|---|---|
| `Apply_PlaceItemOnEmptyCell_MovesItemFromBagToCell` | Accepted; ô chứa vật phẩm; Túi không còn uid đó; sự kiện `GridChanged` |
| `Apply_PlaceItemOnOccupiedCell_ReturnsOccupantToBagEnd` | ô chứa vật phẩm mới; vật phẩm cũ ở cuối Túi |
| `Apply_PlaceItemNotPlaceable_RejectsWithItemNotPlaceable (kind: scrap, lens, catalyst)` | Rejected, `ItemNotPlaceable` |
| `Apply_PlaceItemUnknownUid_RejectsWithItemNotFound` | Rejected, `ItemNotFound` |
| `Apply_GridCommandCellOutOfRange_RejectsWithInvalidCell (command: place -1, place 9, remove 9, move →9)` | Rejected, `InvalidCell` |
| `Apply_RemoveFromGrid_AppendsItemToBagEnd` | ô trống; vật phẩm ở cuối Túi |
| `Apply_RemoveFromEmptyCell_RejectsWithCellEmpty` | Rejected, `CellEmpty` |
| `Apply_MoveCellToEmptyCell_MovesItem` | ô đích có vật phẩm, ô nguồn trống |
| `Apply_MoveCellToOccupiedCell_SwapsItems` | hai vật phẩm đổi ô |
| `Apply_MoveCellFromEmptyCell_RejectsWithCellEmpty` | Rejected, `CellEmpty` |
| `Apply_MoveCellToSameCell_RejectsWithInvalidCell` | Rejected, `InvalidCell` |
| `Apply_GridCommandOutsideWorkbench_RejectsWithWrongPhase (command: place, remove, move, brew)` | Rejected, `WrongPhase` |
| `Apply_GridCommandWithoutActiveRun_RejectsWithRunNotActive (command: place, remove, move, brew)` | Rejected, `RunNotActive` |
| `Apply_GridCommandAfterBrew_RejectsWithGridLocked (command: place, remove, move, brew)` | Rejected, `GridLocked` |
| `Apply_RejectedGridCommand_LeavesStateHashUnchanged (command: place unknown uid, move from empty, brew empty)` | hash trước = sau |

#### GameRulesBrewTest
| Test method | Kiểm tra |
|---|---|
| `Apply_BrewEmptyGrid_RejectsWithEmptyGrid` | Rejected, `EmptyGrid` |
| `Apply_BrewMatchingGrid_AddsProductWithBaseEssenceQuality` | vật phẩm cuối Túi = Thép, `Quality` 18, `Magic` false |
| `Apply_BrewNonMatchingGrid_AddsScrapWithZeroQuality` | vật phẩm cuối Túi = `scrap`, `Quality` 0 |
| `Apply_BrewOnlyAdditives_AddsScrap` | vật phẩm cuối Túi = `scrap` |
| `Apply_Brew_EmptiesGrid` | 9 ô null |
| `Apply_Brew_RemovesConsumedItemsFromRun` | uid trên lưới không còn trong Túi lẫn lưới |
| `Apply_Brew_AddsExactlyOneItemToBag` | `Bag.Count` = trước + 1 |
| `Apply_Brew_AssignsNextUidToResult` | uid kết quả = `NextUid` cũ; `NextUid` tăng 1 |
| `Apply_Brew_EntersResultPendingWithResultUid` | `ResultPending`; `LastBrewResultUid` = uid kết quả |
| `Apply_Brew_RaisesBrewCompletedWithResultUid` | một `BrewCompleted`, `ResultUid` = uid kết quả |
| `Apply_BrewIronCoalFamilyByGap_ProducesVariant (gap: 0 → steel, 1 → cast_iron, 2 → scrap)` | đúng item kết quả |
| `Apply_BrewWithAllowedAdditiveInGap_SameProductAndQualityAsWithout` | cùng `ItemId`, cùng `Quality` |

#### GameRulesStartRunTest (sửa)
| Test method | Kiểm tra |
|---|---|
| `Apply_ValidStartRun_StartsWithEmptyGridInEditing` | 9 ô null; `Editing`; `LastBrewResultUid` 0 |

### Visual verification tests
(none)

### Manual tests
(none)

**Truy vết**: R1 → `Apply_PlaceItem*` (4) ; R2 → `Apply_RemoveFrom*` (2); R3 → `Apply_MoveCell*` (4); R4 →
`Apply_GridCommandOutsideWorkbench_*`, `*WithoutActiveRun_*`, `*AfterBrew_*`, `*CellOutOfRange_*`; R5 →
`Match_PatternCrossesRowBoundary_*`, `Match_LeadingEmptyCells_*`; R6 → `RecipeMatcherTest`; R7 → `GameRulesBrewTest`;
R8 → `QualityFormulaTest`; R9 → `ContentDbRecipeTest`; R10 → `StateHasherTest` (sửa) + `Apply_RejectedGridCommand_*`.
AC brewing.md: AC-1 → `Compute_GddExample_*`, `Compute_MultipliersInDifferentOrder_*`; AC-2/AC-3/AC-3b →
`Match_IronCoalFamilyByGap_*`, `Match_IronCoalTwoGaps_*`, `Apply_BrewIronCoalFamilyByGap_*`; AC-4 (thành phẩm) →
`Match_AllowedAdditiveInGap_*`, `Apply_BrewWithAllowedAdditiveInGap_*`; AC-5 → `Match_RequiresMagic*`; AC-8 (lưới) →
`Apply_GridCommandAfterBrew_*`; AC-9 → `Apply_Brew_RemovesConsumed*`, `Apply_Brew_AddsExactlyOne*`; AC-10 (lưới trống) →
`Apply_BrewEmptyGrid_*`. Còn lại: AC-4 (MagicChance), AC-6 → F-003; AC-7, AC-11 → F-004; AC-8 (soi/mua) → F-006/F-008;
AC-10 (Bỏ qua) → F-009.
**Testability**: PASS — logic thuần, state dựng trực tiếp, không RNG, không I/O.

## 4. Các bước thực hiện

> `CLAUDE.md` §Git: commit tự động **không**, commit test-first **không** → đề xuất message, test + code gộp một commit
> khi xanh. **Nên commit F-000/F-001 trước khi bắt đầu** (đang chưa commit). Lệnh test: `CLAUDE.md` §Lệnh
> (`$UNITY` Linux = `~/Unity/Hub/Editor/6000.6.7f1/Editor/Unity`), **Editor phải đóng**.

- [x] **Bước 1 — Skeleton**: thêm/sửa mọi type §2 (method mới trả mặc định / ném `NotImplementedException`); cập nhật
  `ContentFactory`, `ContentDbTest.CreateWithItems` theo chữ ký `ContentDb.Create` mới; 69 test cũ vẫn xanh
  (`-runTests -testPlatform EditMode`, đọc `Logs/editmode.xml`).
- [x] **Bước 2 — Test first**: viết test §3 (+ `StateFactory.CreateWorkbenchRun`, content thử nghiệm); chạy EditMode →
  test mới **fail đúng lý do**; test cũ vẫn xanh. Test "Rejected → không đổi" có thể pass sẵn trên stub — ghi §6.
- [x] **Bước 3 — Implement**: `QualityFormula` → content (`RecipeData`/`Recipe`/validate) → `RecipeMatcher` → state + hash
  → `GridRules` → `BrewingRules` → dispatch `GameRules`; EditMode xanh (0 failed), không warning CS mới.
- [x] **Bước 4 — Refactor**: gộp test cùng phân vùng; gọn validate dùng chung lưới/Brew; EditMode xanh;
  `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .` 0 lỗi.
- [x] **Bước 5 — Bằng chứng & tài liệu**: LOG (số test); PLAN F-002 → Done (AC lưới/khớp/Quality) hoặc In Test; TECH
  §3.1–3.3 cập nhật API thật; GDD Systems Index #2 → Implemented; đề xuất commit
  `feat(brewing): add grid commands, recipe matcher and quality formula`.

## 5. Rủi ro & đánh đổi

- `Brew` không rút RNG ở F-002; F-003 sẽ thêm đúng 1 lần rút `Chance` khi khớp → mọi hash/replay ghi trước F-003 đổi
  (chưa có save thật nên chấp nhận).
- Floor từng bước theo Mul tăng dần lệch chút so với tích số thực (≤ 1 đơn vị mỗi Mul) — đổi lấy tất định + không phụ
  thuộc thứ tự. Đã chốt với người dùng 2026-11-04.
- Đổi chữ ký `ContentDb.Create` chạm test của F-000 (chưa commit) — làm ở bước 1, chạy lại toàn bộ trước khi viết test mới.
- `CanonicalWriter`: 5 field mới phải được ghi — đã có case mutation tương ứng.
- Thứ tự kiểm lý do từ chối (run → phase → step → tham số) cố định trong test; đổi thứ tự sau này phải sửa test.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - Plan ở trạng thái Draft khi gọi `build` → coi lệnh `build brewing` là duyệt (gồm các điểm tự quyết ở §2).
  - Bước 1 + 2 chạy chung một lần batchmode (skeleton + test) để tiết kiệm lượt chạy Editor; kết quả vẫn tách được:
    69 test cũ xanh (chữ ký `ContentDb.Create` mới không phá gì), test mới fail đúng lý do.
  - `BrewingData.MaxQualityCap = long.MaxValue / 500` là nơi duy nhất giữ giới hạn trần; `QualityFormula` và
    `ContentDb` cùng đọc nó (Bước 4: bỏ phụ thuộc ngược `Content → Rules`).
  - `QualityFormula`: `essence × (1 + add)` so với `cap / (1 + add)` trước khi nhân (không tràn); `addPotency =
    long.MaxValue` → hệ số giữ `long.MaxValue`.
  - `ContentDb` validate thêm `BaseEssence < 0` → `ArgumentException` (ngoài bảng test, cùng nhóm R9).
  - `RecipeMatcher` kiểm phụ gia trong `Gap` bằng **cả** `Kind == Additive` và `AllowedAdditives` (validator đã bảo đảm
    danh sách chỉ chứa Additive — kiểm kép, không tốn).
  - `GridChanged` dùng chung một mảng sự kiện tĩnh (không alloc mỗi lệnh lưới); `Brew` cấp phát 1 `BrewCompleted`.
  - `StateHasher` ghi lưới theo ô (cờ có/không + item) và `Magic`/`Quality` cho mọi item ở Túi lẫn lưới.
- **Deviations**: - None
- **Tradeoffs**:
  - 11 test pass ngay trên skeleton vì stub trả null / từ chối mặc định: 8 `RecipeMatcherTest.Match_*_ReturnsNull` và 3
    `Apply_RejectedGridCommand_LeavesStateHashUnchanged`. Đã dự đoán (plan §4 bước 2); các test dương tương ứng
    (`Match_*_ReturnsRecipe/Variant`, `Apply_*_Rejects*`) fail trên stub nên cặp test có nghĩa.
  - Bước 4: không gộp test — `Compute_MultipliersInDifferentOrder_*` và `Compute_MultipliersFlooredAscending_*` khác
    assertion; test khớp ở `RecipeMatcherTest` và `GameRulesBrewTest` khác lớp (unit / qua `GameRules`).
- **Test changes**:
  - `ContentFactory` / `ContentDbTest.CreateWithItems`: thêm tham số `BrewingData` theo chữ ký mới (dự kiến trong plan).
  - `GameRulesBrewTest.Apply_Brew_RemovesConsumedItemsFromRun`: `Has.None.AnyOf(...)` không có trong NUnit 3.5 của Unity
    (CS1061) → `remaining.Intersect(...)` + `Is.Empty`. Sửa trước lần chạy đầu, chưa commit.
- **Open questions**:
  - Bộ khởi đầu > sức chứa Túi (từ F-000) — vẫn mở.
