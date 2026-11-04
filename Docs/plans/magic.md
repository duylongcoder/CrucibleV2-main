# Plan: Kích hoạt ma thuật (F-003)

> **Trạng thái**: Implemented (duyệt khi gọi `build magic`) · **Cập nhật**: 2026-11-04
> **GDD**: `design/brewing.md` §Kích hoạt ma thuật, §Formulas (`MagicChance%`), §Edge Cases; GDD §6.1 (stream `Chance`),
> §6.2 (`Magic` công khai) · **ADR**: ADR-004 · **Loại**: Logic
> Xây trên `plans/brewing.md` (F-002 — xanh 160/160, **chưa commit** lúc viết plan).

## 1. Bối cảnh

Ma thuật là cửa vào nhánh Khoáng Tối thượng (P1): mỗi lượt nung khớp công thức tung một lần xem thành phẩm có `Magic`.
Xác suất đến từ nguyên liệu đã ma thuật (truyền thừa) và phụ gia trong khoảng trống. F-002 đã có `ItemInstance.Magic`,
slot `RequiresMagic` và `Brew` luôn cho `Magic = false`; plan này thêm công thức và lần tung.
**Chốt với người dùng 2026-11-04**: công thức tham số hóa bằng 3 bảng theo tier (`Inherit`, `Cap`, `Retain`) trong
`BrewingData`; mặc định = công thức GDD gốc; cách cân bằng theo tier (Open Question brewing.md) chốt ở `balance` chỉ
bằng đổi số.

**Yêu cầu** (đánh số để truy vết):
- R1: `MagicChanceFormula.Compute(magicIngredientTiers, additiveChances, productTier, brewing)` =
  `min(Cap[tierSP], ⌊(Σ Inherit[tier] + Σ additiveChance) × Retain[tierSP] / 100⌋)` — số nguyên phần trăm.
  Không có cả hai → 0 (AC-6: ví dụ = 50; tổng > trần → trần).
- R2: `MagicChanceFormula.ForBrew(grid, recipe, content)`: gom tier của mọi **nguyên liệu (`Mineral`) trên lưới có
  `Magic`** + `MagicChance` của mọi **phụ gia trên lưới** (lưới đã khớp ⇒ phụ gia chỉ nằm ở `Gap`), tier thành phẩm =
  `Tier` của `recipe.Product`. Thêm một phụ gia hợp lệ → tăng đúng `MagicChance` của phụ gia đó (AC-4).
- R3: `Brew` khớp công thức → rút **đúng 1 lần** stream `Chance` (`Roll(MagicChance%)`, kể cả 0%) và đặt
  `Magic` của thành phẩm = kết quả. Không rút stream nào khác.
- R4: `Brew` không khớp (phế phẩm) → **không** rút random; phế phẩm `Magic = false`. Command bị từ chối → không rút
  (giữ từ F-002).
- R5: Cùng state + cùng lưới → cùng `Magic` (tất định).
- R6: `BrewingData` có 3 bảng 10 phần tử (chỉ số = tier − 1): `MagicInheritByTier` (0–100), `MagicCapByTier` (0–100),
  `MagicRetainByTier` (0–100). Sai độ dài / ngoài khoảng → `ArgumentException` ở `ContentDb.Create`.
- R7: `ItemData.MagicChance` (phụ gia, 0–50); vật phẩm không phải phụ gia có `MagicChance ≠ 0` → `ArgumentException`.
  `Mineral` / `Additive` phải có `Tier` 1–10 → ngoài khoảng `ArgumentException` (bảng tra theo tier).

**Ngoài phạm vi**: dòng thuộc tính sinh bằng `Chance` (F-008 — sẽ rút **sau** lần tung ma thuật); Effect Xúc tác,
`AddMagicChance` dạng Effect (F-007 — xem §5); hiệu ứng ma thuật trên sprite / UI (F-014); số liệu cân bằng theo tier
(`balance`).
**TBD (không làm)**: số liệu thật của 3 bảng (dùng mặc định = công thức gốc).

## 2. Thiết kế triển khai

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Content/BrewingData.cs` | + `TierCount = 10`, `MaxPercent = 100`, `MaxAdditiveMagicChance = 50`; 3 bảng; ctor mới | Core | Sửa |
| `Core/Content/ItemData.cs` | + `int MagicChance` (tham số tùy chọn) | Core | Sửa |
| `Core/Content/ContentDb.cs` | validate R6, R7 | Core | Sửa |
| `Core/Rules/MagicChanceFormula.cs` | `Compute` (R1), `ForBrew` (R2) | Core | Mới |
| `Core/Rules/BrewingRules.cs` | khớp → `ForBrew` → `Rng.Get(Chance).Roll(...)` → `Magic` (R3, R4) | Core | Sửa |

**Public API**
```csharp
public sealed class BrewingData {
    public const int TierCount = 10;  public const int MaxPercent = 100;  public const int MaxAdditiveMagicChance = 50;
    public BrewingData(long qualityCap, IReadOnlyList<int> magicInheritByTier, IReadOnlyList<int> magicCapByTier,
                       IReadOnlyList<int> magicRetainByTier);
    public IReadOnlyList<int> MagicInheritByTier { get; }  public IReadOnlyList<int> MagicCapByTier { get; }
    public IReadOnlyList<int> MagicRetainByTier { get; }
}
public sealed class ItemData { public ItemData(string id, ItemKind kind, int tier, RecipeData recipe = null, int magicChance = 0);
                               public int MagicChance { get; } }
public static class MagicChanceFormula {
    public static int Compute(IReadOnlyList<int> magicIngredientTiers, IReadOnlyList<int> additiveChances,
                              int productTier, BrewingData brewing);
    public static int ForBrew(IReadOnlyList<ItemInstance> grid, Recipe recipe, ContentDb content);
}
```

**Interface phụ thuộc**: không — RNG là `ref Pcg32` từ `run.Rng`, content truyền tham số.
**Dữ liệu**: chưa có SO; mặc định (`Inherit[t] = 5t`, `Cap = 90`, `Retain = 100`) nằm trong `ContentFactory` của test và
sẽ nằm trong SO config khi có `ContentBaker` — không hard-code trong Core.
**Scene / prefab bị chạm**: không.
**Lý do thiết kế**:
- `Compute` nhận danh sách số (tier, chance) thay vì state → test công thức (AC-6) không cần dựng lưới; `ForBrew` là
  phần gom dữ liệu, test riêng (AC-4).
- `Retain` áp **trước** `Cap` và floor một lần: thứ tự cố định, số nguyên (ADR-004).
- Tung ma thuật là lần rút `Chance` **đầu tiên** của một lượt nung khớp; F-008 (dòng thuộc tính) và F-007 (điều kiện
  `Chance` của Xúc tác) rút sau, theo thứ tự cố định — ghi vào TECH §3.4 khi build.
- `BrewingData` ctor bắt buộc đủ 4 tham số (không overload mặc định) → không có giá trị gameplay ẩn trong Core. Đổi chữ
  ký chạm `ContentFactory`, `ContentDbTest`, `ContentDbRecipeTest` → gom về `ContentFactory.CreateBrewing(cap)`.
- Phụ gia `MagicChance` khoảng 0–50 (GDD ghi 1–50): cho phép 0 để content thử nghiệm / phụ gia "trơ" không bị chặn.

## 3. Test cases

Tất cả EditMode. `ContentFactory` thêm: `add_ash` `MagicChance` 10 (tier 1), `add_bone` (Additive tier 2, `MagicChance`
20, được phép ở `Gap` của Gang) — `add_sand` giữ 0; `CreateBrewing(cap)` dựng 3 bảng mặc định. Một content riêng cho
test 100% (`Cap` = `Retain` = 100, `Inherit` lớn) dựng trong test.

### Editor tests
(none)

### Unit tests
#### MagicChanceFormulaTest
| Test method | Kiểm tra |
|---|---|
| `Compute_GddExample_Returns50` | 50 (ngl ma thuật tier 4 + phụ gia 20 + 10, tierSP 5) |
| `Compute_NoMagicIngredientNoAdditive_ReturnsZero` | 0 |
| `Compute_SumAboveCap_ReturnsCap` | 90 |
| `Compute_InheritTableByIngredientTier_UsesTableValue` | bảng `Inherit` tùy chỉnh → giá trị tra đúng tier |
| `Compute_CapByProductTier_UsesProductTierCap` | `Cap[tierSP]` khác nhau theo tier → trần đúng tier thành phẩm |
| `Compute_RetainByProductTier_FloorsScaledSum` | tổng 45, `Retain` 50 → 22 |
| `Compute_TierOutsideOneToTen_ThrowsArgumentOutOfRangeException (case: ingredient 0, ingredient 11, product 0, product 11)` | ném |
| `ForBrew_AllowedAdditiveAdded_IncreasesByAdditiveChance` | có Tro − không Tro = 10 |
| `ForBrew_MagicIngredient_AddsInheritOfItsTier` | thép ma thuật (tier 2) → 10 |
| `ForBrew_NonMagicIngredients_ReturnsZero` | 0 |

#### ContentDbMagicTest
| Test method | Kiểm tra |
|---|---|
| `Create_MagicTableWrongLength_ThrowsArgumentException (table: inherit, cap, retain)` | ném |
| `Create_MagicTableValueOutsideZeroToHundred_ThrowsArgumentException (table, value: inherit/cap/retain × −1/101)` | ném |
| `Create_AdditiveMagicChanceOutsideRange_ThrowsArgumentException (chance: −1, 51)` | ném |
| `Create_NonAdditiveWithMagicChance_ThrowsArgumentException` | ném |
| `Create_MineralOrAdditiveTierOutsideOneToTen_ThrowsArgumentException (kind, tier: Mineral 0, Mineral 11, Additive 0)` | ném |

### Integration tests (Core, qua `GameRules.Apply`)
#### GameRulesBrewMagicTest
| Test method | Kiểm tra |
|---|---|
| `Apply_BrewMatching_MagicEqualsChanceRollAtComputedPercent` | `Magic` = `Roll(ForBrew(...))` trên bản sao stream `Chance` trước khi Nung |
| `Apply_BrewMatching_AdvancesChanceStreamByOneDraw` | trạng thái `Chance` sau = trạng thái trước + một `NextUInt` |
| `Apply_BrewMatching_LeavesOtherStreamsUnchanged` | 6 stream khác không đổi |
| `Apply_BrewMatchingZeroPercent_StillAdvancesChanceByOneDraw` | như trên với lưới 0% |
| `Apply_BrewMatchingHundredPercent_ProducesMagicProduct` | `Magic` true (content Cap/Retain 100) |
| `Apply_BrewNonMatching_LeavesAllStreamsUnchanged` | `Rng.Capture()` trước = sau |
| `Apply_BrewNonMatching_ScrapHasNoMagic` | `Magic` false |
| `Apply_SameStateSameGrid_ProducesSameMagic` | hai state dựng giống nhau → cùng `Magic`, cùng hash |

### Visual verification tests
(none)

### Manual tests
(none)

**Truy vết**: R1 → `Compute_*`; R2 → `ForBrew_*`; R3 → `Apply_BrewMatching_*` (4) + `*HundredPercent_*`; R4 →
`Apply_BrewNonMatching_*` (2) + `GameRulesGridTest.Apply_RejectedGridCommand_*` (F-002); R5 → `Apply_SameStateSameGrid_*`;
R6 → `Create_MagicTable*`; R7 → `Create_AdditiveMagicChance*`, `Create_NonAdditiveWithMagicChance_*`,
`Create_MineralOrAdditiveTier*`. AC brewing.md: AC-4 (MagicChance) → `ForBrew_AllowedAdditiveAdded_*`; AC-5 → F-002
(`Match_RequiresMagic*`) + `Apply_BrewMatchingHundredPercent_*` (thành phẩm ma thuật dùng lại được); AC-6 →
`Compute_GddExample_*`, `Compute_SumAboveCap_*`. GDD §6.1 AC-3 (vế ma thuật) → `Apply_BrewMatching_LeavesOtherStreams*`
+ `RngStreamsTest.Get_DrawFromShop_*` (F-001).
**Testability**: PASS — công thức thuần; RNG kiểm bằng bản sao trạng thái stream, không cần thống kê.

## 4. Các bước thực hiện

> `CLAUDE.md` §Git: commit tự động **không**, commit test-first **không**. **Commit F-002 trước khi bắt đầu.**
> Lệnh test: `CLAUDE.md` §Lệnh (`$UNITY` Linux), **Editor phải đóng**.

- [x] **Bước 1 — Skeleton**: sửa `BrewingData` / `ItemData`, tạo `MagicChanceFormula` (trả 0); `ContentFactory.CreateBrewing`
  + sửa `ContentDbTest`, `ContentDbRecipeTest` dùng nó; 160 test cũ vẫn xanh.
- [x] **Bước 2 — Test first**: viết §3; chạy EditMode → test mới fail đúng lý do (giá trị 0 / không ném / stream không
  đổi); test cũ xanh. Dự kiến pass sẵn trên stub: `Compute_NoMagic…_ReturnsZero`, `ForBrew_NonMagic…_ReturnsZero`,
  `Apply_BrewNonMatching_*` (2), `Apply_SameStateSameGrid_*` — ghi §6.
- [x] **Bước 3 — Implement**: validate content → `Compute` → `ForBrew` → tung trong `BrewingRules`; EditMode xanh, 0 warning CS.
- [x] **Bước 4 — Refactor**: gọn validate bảng (một helper cho 3 bảng); EditMode xanh; `check_unity_project.py` 0 lỗi.
- [x] **Bước 5 — Bằng chứng & tài liệu**: LOG; PLAN F-003 → Done; TECH §3.4 thứ tự rút `Chance` trong một lượt nung;
  GDD Systems Index #3 → Implemented; đề xuất commit `feat(brewing): add magic activation roll and tiered magic chance`.

## 5. Rủi ro & đánh đổi

- Thêm lần rút `Chance` làm đổi mọi chuỗi RNG sau một lượt nung khớp so với F-002 — chưa có save/replay thật, chấp nhận.
- Phụ gia mang `MagicChance` dạng field; economy.md mô tả phụ gia "chỉ dùng `AddMagicChance`" (Effect primitive). Khi làm
  F-007, nếu chuyển sang Effect thì `ForBrew` đọc từ Effect thay vì field — chỉ đổi chỗ gom, công thức giữ nguyên.
- Thứ tự rút `Chance` (ma thuật trước, dòng thuộc tính / Xúc tác sau) là hợp đồng giữa F-003/F-007/F-008 — đổi thứ tự
  về sau làm lệch replay; ghi TECH để giữ.
- Mặc định bảng = công thức gốc, vốn đi ngược yêu cầu "tier cao khó giữ ma thuật" — **cố ý**, chờ `balance` đổi số.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - Plan ở trạng thái Draft khi gọi `build` → coi lệnh `build magic` là duyệt (gồm các điểm tự quyết §2).
  - Bước 1 + 2 chạy chung một lần batchmode (như F-002): 160 test cũ xanh với chữ ký `BrewingData` mới, test mới fail đúng.
  - Content thử nghiệm thêm `add_bone` (Additive tier 2, 20%, được phép ở `Gap` của Gang); `add_ash` có 10%. Test tung
    dùng hằng `BoneMagicChance` (không gọi `ForBrew` để tính kỳ vọng — tránh test tự xác nhận chính SUT).
  - `ForBrew` phân biệt theo `Kind`: phụ gia góp `MagicChance` (kể cả không có `Magic`), chỉ `Mineral` có `Magic` góp
    `Inherit`. Cấp phát 2 `List<int>` mỗi lượt Nung — không phải vòng lặp nóng.
  - `ContentDb.ValidateItem` chạy cho mọi item trong vòng gom Scrap/Recipe (một vòng duy nhất).
- **Deviations**: - None
- **Tradeoffs**:
  - 6 test pass ngay trên skeleton: 5 đã dự đoán ở plan (`Compute_NoMagic…`, `ForBrew_NonMagic…`, `Apply_BrewNonMatching_*`
    ×2, `Apply_SameStateSameGrid_*`) + `Apply_BrewMatching_LeavesOtherStreamsUnchanged` (stub không rút stream nào — test
    có nghĩa khi đi cặp với `Apply_BrewMatching_AdvancesChanceStreamByOneDraw`, fail trên stub).
  - Với seed của `StateFactory`, lần tung 20% ra `true` → `Apply_BrewMatching_MagicEqualsChanceRollAtComputedPercent`
    fail trên stub (không tầm thường). Đổi seed mặc định có thể làm test này pass tầm thường trên stub — không ảnh hưởng
    tính đúng khi code đã cài.
  - Bước 4: không cần refactor — validate 3 bảng đã dùng chung `ValidateTierTable` từ lúc implement.
- **Test changes**:
  - `ContentDbTest`, `ContentDbRecipeTest`: `new BrewingData(cap)` → `ContentFactory.CreateBrewing(cap)` (chữ ký mới, dự
    kiến trong plan). `ContentFactory`: Gang cho phép thêm `add_bone`; `add_ash` có `MagicChance` 10 — test F-002 vẫn xanh.
- **Open questions**:
  - Số liệu thật 3 bảng theo tier → `balance`. Phụ gia field vs Effect `AddMagicChance` → xem lại ở F-007.
