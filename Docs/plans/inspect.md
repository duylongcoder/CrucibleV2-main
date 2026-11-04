# Plan: Thuộc tính ẩn & Kính soi (F-008)

> **Trạng thái**: Done (D1–D10 duyệt 2026-11-04; 890/890 EditMode xanh) · **Cập nhật**: 2026-11-04
> **GDD**: GDD §6.2 (Detailed Rules, Edge Cases, AC-1…AC-5); `design/brewing.md` §Công thức ("các dòng thuộc tính khởi
> đầu"); `design/battle.md` §Luật cốt lõi (SB/PD/HT từ khoáng đã dâng); `design/ui-ban-luyen.md` §3 Bàn soi; TECH §3.2
> (`ItemInstance.Lines`, `RevealedMask`), §3.4 (thứ tự rút `Chance`) · **ADR**: ADR-001, ADR-004, ADR-007 (chống rò rỉ)
> · **Loại**: Logic (Core)
> Xây trên F-000…F-006, F-009 phần 1, F-010, F-011 (820/820 EditMode xanh, commit `ec22c60`).

## 1. Bối cảnh

Thành phẩm hiện chỉ có `Quality` + `Magic`; `OfferingRules.Offer` truyền SB/PD/HT = 0 vào `BattleStats.ForHero` nên
loại trang bị "hợp" thuộc tính nào cũng như nhau, và Kính soi chỉ là món hàng vô dụng. Plan này thêm **dòng thuộc tính
ẩn** cho thành phẩm (sinh lúc nung), lệnh **`Inspect`** (Kính soi lộ dòng), và nối dòng vào Trận. Pillar P2 (đọc được,
tất định), P3 (Kính soi là quyết định kinh tế: soi để chọn đúng khoáng dâng cho loại trang bị của Chiếu chỉ).

**Quyết định người dùng (2026-11-04)**: dòng khởi đầu của công thức = danh sách **(thuộc tính, % xuất hiện, min, max)**;
mỗi dòng tung 1 lần `Roll(%)`, có thì rút giá trị đều trong `[min, max]` (stream `Chance`).

**Yêu cầu** (đánh số để truy vết):
- R1 — **Content dòng**: `StatLineData(stat, chancePercent, min, max)`; `RecipeData` thêm danh sách dòng khởi đầu (tùy
  chọn, mặc định rỗng). Validate: `chancePercent ∈ [0, 100]`, `1 ≤ min ≤ max ≤ BattleData.MaxStatValue` (D5), mỗi
  `StatKind` tối đa 1 dòng / công thức (D4).
- R2 — **Content Kính**: `ItemData.RevealLines` — Kính (`Kind = Lens`) phải ≥ 1; loại khác phải 0 (GDD Tuning: 1 / 3).
- R3 — **Dòng của thành phẩm**: `ItemInstance` thêm `Lines` (`IReadOnlyList<StatLine>`, `StatLine(stat, value)`, theo thứ
  tự khai báo công thức, chỉ các dòng phụ có mặt) và `RevealedMask` (int: bit 0 = `Quality`, bit i+1 = `Lines[i]`).
  "Dòng ẩn" của thành phẩm = `Quality` + `Lines` → số dòng = `1 + Lines.Count`. `Magic` **không** là dòng (công khai,
  AC-5). Nguyên liệu gốc / phế phẩm / Kính / Xúc tác: không có dòng (`Lines` rỗng, mask 0).
- R4 — **Sinh dòng lúc nung** (khớp công thức): sau lần `Roll(MagicChance)` (TECH §3.4 bước 1), với từng dòng khởi đầu
  theo thứ tự khai báo: đúng 1 `Roll(chancePercent)` (kể cả 0 / 100); có → đúng 1 `NextInt(max − min + 1)`, giá trị =
  `min + roll`. Stream `Chance`. Phế phẩm: không rút (giữ nguyên). Mặc định không lộ dòng nào (`RevealedMask = 0`).
- R5 — `Inspect(targetUid, lensUid)`: ở `Workbench·Editing` hoặc `Offering`. Từ chối theo thứ tự: `RunNotActive` →
  `WrongPhase` (phase khác) → `GridLocked` (Workbench·ResultPending — "đang phát lại") → `ItemNotFound` (target không
  trong Túi) → `ItemNotInspectable` (mới; target không phải thành phẩm — `Recipe == null`, gồm phế phẩm) →
  `ItemNotFound` (lens không trong Túi) → `ItemNotLens` (mới) → `NothingToReveal` (mới; mọi dòng đã lộ — **không** tiêu
  hao Kính, AC-2). Target chỉ trong Túi (ui-ban-luyen §3: vật phẩm trên lưới chỉ có "Gỡ về Túi").
- R6 — **Lộ dòng**: `unrevealed` = các chỉ số dòng chưa lộ theo thứ tự (0 = `Quality`, rồi `Lines`). Nếu
  `unrevealed.Count ≤ RevealLines` → lộ hết, **không rút RNG** (D6). Ngược lại lộ `RevealLines` dòng chọn không lặp:
  mỗi lượt 1 `NextInt(số còn lại)` trên stream `Inspect` chọn trong danh sách còn lại. Kính bị gỡ khỏi Túi (tiêu hao).
  Phát `ItemInspected(targetUid, lensUid, newlyRevealedMask)`. Mask giữ suốt run trên instance (mất khi tiêu hao).
- R7 — **Trận đọc mọi dòng** (kể cả chưa lộ): `Offer` truyền `SB/PD/HT` = giá trị dòng tương ứng của khoáng đã dâng
  (0 nếu không có) vào `BattleStats.ForHero`. `DismissCourtier` giữ 0.
- R8 — **Giá bán không phụ thuộc dòng** (AC-3) — `SellPrice` chỉ theo `ItemId` (đã đúng, thêm test khóa).
- R9 — `StateHasher` ghi `Lines` (count + stat + value) và `RevealedMask` của mọi `ItemInstance` (Túi + lưới).
- R10 — **Không rò rỉ qua log hiện có** (AC-4 phần Core): `BrewLog` / `BrewCompleted` không mang `Lines` (test reflection
  như `Recipe`). `RunView` lọc dòng chưa lộ thuộc F-014 (ADR-007).

**Ngoài phạm vi**: Xúc tác `AddStat` (F-007 — chạy sau dòng, TECH §3.4 bước 3); `RunViewBuilder` / UI Bàn soi (F-014);
lộ thuộc tính khi kích hoạt trong Trận + Sổ "đã khám phá" (F-017); Kính tốt trong content thật + giá (balance);
`MetaState.DiscoveredStatsMask` (F-012/F-017); DTO save mang `Lines` + `RevealedMask` (F-013 — ghi chú).
**TBD (không làm)**: bảng dòng khởi đầu thật của từng công thức, giá Kính — `balance`.

## 2. Thiết kế triển khai

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Content/StatLineData.cs` | record content: `Stat`, `ChancePercent`, `Min`, `Max` | Core | Mới |
| `Core/Content/RecipeData.cs` | + tham số tùy chọn `statLines` (mặc định rỗng) | Core | Sửa |
| `Core/Content/Recipe.cs` | + `StatLines: IReadOnlyList<StatLineData>` (đã validate) | Core | Sửa |
| `Core/Content/ItemData.cs` | + `RevealLines` (tham số tùy chọn, mặc định 0) | Core | Sửa |
| `Core/Content/ContentDb.cs` | validate R1/R2 trong `ResolveRecipe` / `ValidateItem` | Core | Sửa |
| `Core/State/StatLine.cs` | `readonly struct StatLine(StatKind Stat, int Value)` | Core | Mới |
| `Core/State/ItemInstance.cs` | + `Lines` (ctor tùy chọn, bản sao bất biến), `RevealedMask { get; set; }`, `LineCount`/`IsRevealed(i)` helper | Core | Sửa |
| `Core/Rules/StatLineRules.cs` | `Roll(ref Pcg32, Recipe) → StatLine[]` (R4); `ValueOf(item, StatKind)` (R7) | Core | Mới |
| `Core/Rules/InspectRules.cs` | `IsInspectable(ItemData)`, `Inspect(run, content, Inspect)` (R5, R6) | Core | Mới |
| `Core/Rules/BrewingRules.cs` | gọi `StatLineRules.Roll` sau tung ma thuật | Core | Sửa |
| `Core/Rules/OfferingRules.cs` | truyền SB/PD/HT từ dòng (R7) | Core | Sửa |
| `Core/Rules/GameRules.cs` | dispatch `Inspect` | Core | Sửa |
| `Core/Commands/Inspect.cs`, `ItemInspected.cs` | Command / Event | Core | Mới |
| `Core/Commands/RejectReason.cs` | + `ItemNotLens = 25`, `ItemNotInspectable = 26`, `NothingToReveal = 27` | Core | Sửa |
| `Core/State/StateHasher.cs` | `WriteItem` ghi `Lines` + `RevealedMask` | Core | Sửa |

**API**
```csharp
public sealed class StatLineData { public StatLineData(StatKind stat, int chancePercent, int min, int max); }
public readonly struct StatLine { public StatLine(StatKind stat, int value); public StatKind Stat { get; } public int Value { get; } }
public sealed class ItemInstance
{
    public ItemInstance(int uid, ItemId itemId, bool magic = false, long quality = 0, IReadOnlyList<StatLine> lines = null);
    public IReadOnlyList<StatLine> Lines { get; }      // rỗng nếu không có
    public int RevealedMask { get; set; }              // bit 0 = Quality, bit i+1 = Lines[i]
}
public static class StatLineRules
{
    public static StatLine[] Roll(ref Pcg32 chance, Recipe recipe);   // R4
    public static int ValueOf(ItemInstance item, StatKind stat);      // 0 nếu không có dòng
}
public static class InspectRules
{
    public static bool IsInspectable(ItemData item);                  // Recipe != null
    public static int LineCount(ItemInstance item);                   // 1 + Lines.Count
    public static CommandResult Inspect(RunState run, ContentDb content, Inspect command);
}
public sealed class Inspect : ICommand { public Inspect(int targetUid, int lensUid); }
public sealed class ItemInspected : GameEvent { int TargetUid; int LensUid; int RevealedMask /* chỉ bit mới lộ */; }
```

**Interface phụ thuộc**: không mới — RNG `Chance` (nung) và `Inspect` (soi) qua `run.Rng.Get`.
**Dữ liệu**: chỉ record content; `RecipeDef.StatLines`, `ItemDef.RevealLines` ở SO + `ContentBaker` (chưa có baker).
**Scene / prefab bị chạm**: không.
**Ghi chú F-013**: DTO `ItemInstance` phải mang `Lines` (stat + value) và `RevealedMask`.

### Quyết định cần duyệt

- **D1 — Mô hình dòng**: `Quality` là field riêng (đã có) nhưng tính là **dòng ẩn số 0** cho Kính soi; `Lines` chỉ chứa
  dòng phụ. Không gói `Quality` vào `Lines` để khỏi đụng `QualityFormula`/Trận/`BrewLog`.
- **D2 — `RevealedMask` là `int` mutable** trên `ItemInstance` (state mutable như `RunState`, TECH §3.2); `Lines` bất
  biến sau khi tạo. Tối đa 4 dòng (Quality + 3 `StatKind`) nên `int` dư.
- **D3 — Target chỉ trong Túi** (ui-ban-luyen §3); Kính luôn trong Túi (không đặt lên lưới được).
- **D4 — Mỗi `StatKind` tối đa 1 dòng / công thức**: Trận đọc "giá trị thuộc tính" — 1 dòng / thuộc tính tránh câu hỏi
  cộng dồn; Xúc tác `AddStat` (F-007) sẽ cộng vào dòng sẵn có hoặc thêm dòng.
- **D5 — `min ≥ 1`**: dòng giá trị 0 vừa vô nghĩa trong Trận (battle: "thuộc tính bằng 0 → không lộ") vừa phí Kính.
- **D6 — Lộ hết thì không rút RNG `Inspect`**: số lần rút vẫn chỉ phụ thuộc state (tất định); khỏi lãng phí stream.
- **D7 — Chọn dòng không lặp bằng `NextInt(còn lại)` trên danh sách chưa lộ** (thứ tự: Quality, rồi `Lines`) — đơn
  giản, đều, mỗi lần chọn đúng 1 lần rút.
- **D8 — Thứ tự từ chối R5**: lỗi về target trước lỗi về Kính, `NothingToReveal` cuối cùng (sau khi xác nhận Kính hợp
  lệ) — người chơi chỉ nhận "Không còn gì để soi" khi thật sự có Kính.
- **D9 — Rút giá trị kể cả khi `min == max`** (`NextInt(1)`): số lần rút `Chance` = 1 + Σ(dòng: 1 + có?1:0), không phụ
  thuộc khoảng giá trị — content đổi `max` không làm lệch chuỗi RNG của dòng sau.
- **D10 — Thành phẩm dùng làm nguyên liệu mất dòng** (tiêu hao như mọi nguyên liệu); thành phẩm mới tự sinh dòng theo
  công thức của nó. Không kế thừa dòng (GDD không mô tả kế thừa; chỉ `Magic` có kế thừa qua `MagicInherit`).

## 3. Test cases

### Editor tests
(none)

### Unit tests

#### ContentDbStatLineTest (`Tests/EditMode/Core/Content/`)
| Test method | Kiểm tra |
|---|---|
| `Create_RecipeStatLines_ResolvedInDeclarationOrder` | `Recipe.StatLines` đúng thứ tự + giá trị |
| `Create_RecipeWithoutStatLines_HasEmptyList` | rỗng |
| `Create_StatLineChanceOutOfRange_Throws` (chance: {−1, 101}) | `ArgumentException` |
| `Create_StatLineMinBelowOne_Throws` | `ArgumentException` |
| `Create_StatLineMinAboveMax_Throws` | `ArgumentException` |
| `Create_StatLineMaxAboveMaxStatValue_Throws` | `ArgumentException` |
| `Create_DuplicateStatInRecipe_Throws` | `ArgumentException` |
| `Create_LensWithoutRevealLines_Throws` | `ArgumentException` |
| `Create_NonLensWithRevealLines_Throws` | `ArgumentException` |

#### StatLineRulesTest
| Test method | Kiểm tra |
|---|---|
| `Roll_NoStatLines_ReturnsEmptyWithoutDrawing` | rỗng, state `Chance` không đổi |
| `Roll_Chance100_AlwaysHasLineInRange` (500 seed) | có dòng, giá trị ∈ [min, max] |
| `Roll_Chance0_NeverHasLine` (500 seed) | không dòng |
| `Roll_MinEqualsMax_ValueIsMin` | giá trị = min |
| `Roll_DrawCount_OneRollPlusOneValuePerPresentLine` | state `Chance` = bản sao sau `Roll` ×2 + `NextInt` ×(số dòng có) |
| `Roll_MultipleLines_KeepDeclarationOrder` | thứ tự `Lines` theo khai báo |
| `Roll_ValueRange_ReachesBothEnds` (2.000 seed) | thấy cả min và max |
| `ValueOf_StatPresent_ReturnsValue` / `ValueOf_StatAbsent_ReturnsZero` | giá trị / 0 |

#### GameRulesBrewTest / GameRulesBrewMagicTest (thêm)
| Test method | Kiểm tra |
|---|---|
| `Apply_BrewRecipeWithStatLines_ProductHasLines` (chance 100) | `Lines` = 1 dòng đúng stat, giá trị trong khoảng |
| `Apply_BrewProduct_StartsFullyHidden` | `RevealedMask = 0` |
| `Apply_BrewStatLinesDrawAfterMagic` | `Magic` giống hệt content không có dòng (cùng seed) — tung ma thuật vẫn là lần rút đầu |
| `Apply_BrewScrap_NoLinesAndNoChanceDraw` | phế phẩm `Lines` rỗng, `Chance` không đổi (giữ test cũ) |
| `Apply_BrewLog_HasNoLinesField` (reflection `BrewLog`/`BrewLogResult`/`BrewTick`/`BrewCompleted`) | không field/property kiểu `StatLine` |

#### GameRulesInspectTest
| Test method | Kiểm tra |
|---|---|
| `Apply_InspectFourLinesWithLens1_RevealsExactlyOne` (AC-1) | mask có đúng 1 bit |
| `Apply_InspectTwice_RevealsDifferentLine` (AC-1) | lần 2 thêm đúng 1 bit mới, khác bit lần 1 |
| `Apply_InspectUnrevealedAtMostRevealLines_RevealsAll` (Kính 3, 3 dòng ẩn) | mask đủ mọi dòng |
| `Apply_InspectRevealAll_DoesNotDrawInspectStream` | state `Inspect` không đổi |
| `Apply_InspectPartial_DrawsOncePerRevealedLine` | state `Inspect` = bản sao sau `NextInt(4)`, `NextInt(3)` (Kính 2, 4 dòng) |
| `Apply_InspectOneHiddenLineWithLens3_RevealsOneAndConsumesLens` (Edge Case) | 1 bit mới, Kính mất |
| `Apply_InspectQualityOnlyProduct_RevealsQuality` (Edge Case "chỉ có Quality") | mask = 1 |
| `Apply_Inspect_ConsumesLens` | Kính khỏi Túi, Túi −1 |
| `Apply_Inspect_EmitsItemInspectedWithNewBitsOnly` | event mask = bit mới |
| `Apply_InspectMagicProduct_MagicNotCountedAsLine` (AC-5) | 1 dòng (Quality), Kính 1 lộ hết, `Magic` không đổi |
| `Apply_InspectInOffering_Accepted` | chấp nhận ở Dâng |
| `Apply_InspectFullyRevealed_RejectsNothingToReveal` (AC-2) | lý do + Kính còn, hash không đổi |
| `Apply_InspectRawIngredient_RejectsItemNotInspectable` / `Apply_InspectScrap_…` | lý do, hash không đổi |
| `Apply_InspectTargetNotInBag_RejectsItemNotFound` (uid trên lưới / không tồn tại) | lý do |
| `Apply_InspectLensNotInBag_RejectsItemNotFound` | lý do |
| `Apply_InspectWithNonLens_RejectsItemNotLens` | lý do |
| `Apply_InspectWhileResultPending_RejectsGridLocked` | lý do |
| `Apply_InspectOutsideWorkbenchAndOffering_RejectsWrongPhase` (Decree, Battle, Spoils, Ended) | lý do |
| `Apply_InspectWhenNotActive_RejectsRunNotActive` | lý do |
| `Apply_InspectTargetErrorBeforeLensError_RejectsItemNotInspectable` (target gốc + lens sai) | thứ tự D8 |

#### GameRulesOfferTest / BattleStatsTest (thêm)
| Test method | Kiểm tra |
|---|---|
| `Apply_OfferProductWithLines_HeroGetsLineValuesWithAffinity` | `LastBattle.Hero` SB/PD/HT = dòng × độ hợp (Kiếm: SB 100 %, PD 50 %) |
| `Apply_OfferHiddenLines_StillCountInBattle` | mask 0 vẫn tính (battle §Dependencies) |
| `Apply_OfferProductWithoutLines_HeroSecondaryStatsZero` | 0 |

#### MarketRulesPriceTest (thêm)
| Test method | Kiểm tra |
|---|---|
| `SellPrice_SameProductDifferentLines_SamePrice` (AC-3) | bằng nhau |

#### StateHasherTest (thêm case)
| Case | Kiểm tra |
|---|---|
| `item lines` (thêm dòng), `item line value`, `revealed mask` | hash khác |

### Integration tests
| Test method | Kiểm tra |
|---|---|
| `RunLoopTest.Apply_BrewInspectOffer_LinesFlowIntoBattle` | Nung thành phẩm có dòng → Soi → Dâng: `LastBattle.Hero.Sharpness` = giá trị dòng × độ hợp |
| `RunLoopInvariantTest` — `NextCommand` thêm `Inspect` ngẫu nhiên | bất biến cũ giữ; từ chối không đổi hash; `RevealedMask` chỉ tăng (không mất bit) |

### Visual verification tests
(none — chưa có view; Bàn soi thuộc F-014)

### Manual tests
(none)

**Truy vết**: R1 → ContentDbStatLineTest; R2 → `Create_Lens*`; R3 → `Apply_BrewRecipeWithStatLines_*`,
`Apply_BrewProduct_StartsFullyHidden`, `Apply_InspectMagicProduct_*`; R4 → StatLineRulesTest + `Apply_BrewStatLinesDrawAfterMagic`,
`Apply_BrewScrap_*`; R5 → `Apply_Inspect*_Rejects*` + `Apply_InspectInOffering_Accepted`; R6 → `Apply_InspectFourLines*`,
`*Twice*`, `*RevealsAll`, `*DrawsOncePerRevealedLine`, `*DoesNotDrawInspectStream`, `*ConsumesLens`, `*EmitsItemInspected*`;
R7 → `Apply_Offer*Lines*` + `RunLoopTest.Apply_BrewInspectOffer_*`; R8 → `SellPrice_SameProductDifferentLines_SamePrice`;
R9 → StateHasherTest; R10 → `Apply_BrewLog_HasNoLinesField`.

**Testability**: PASS — luật static thuần, RNG có seed, state dựng tay (`StateFactory` thêm helper tạo thành phẩm có
dòng + Kính trong Túi). Content test: `ContentFactory.Create` thêm tham số `steelStatLines` (mặc định rỗng — không đổi
chuỗi `Chance` của test cũ) và `lensRevealLines` (mặc định 1 — không thêm item mới để khỏi dời `ItemId`).

## 4. Các bước thực hiện

> Commit theo `CLAUDE.md` §Git (không tự commit). Kiểm chứng: `"$UNITY" -batchmode -nographics -projectPath . -runTests
> -testPlatform EditMode -testResults Logs/editmode.xml -logFile -` (Editor đóng), đọc `Logs/editmode.xml`.

- [x] **Bước 1 — Skeleton**: `StatLineData`, `StatLine`, `RecipeData`/`Recipe` + `StatLines`, `ItemData.RevealLines`,
  `ItemInstance.Lines/RevealedMask`, `StatLineRules`/`InspectRules` thân rỗng, Command/Event, `RejectReason` +3,
  dispatch; `ContentFactory` (Lens `RevealLines` 1, tham số `steelStatLines`/`lensRevealLines`), `StateFactory` helper.
  Kiểm chứng: compile, 820 test cũ xanh.
- [x] **Bước 2 — Test first**: test §3; xác nhận fail đúng lý do.
- [x] **Bước 3 — Implement**: validate content, `StatLineRules`, `BrewingRules`, `InspectRules`, `OfferingRules`,
  `StateHasher`. Kiểm chứng: toàn bộ EditMode xanh.
- [x] **Bước 4 — Refactor**: gộp test tham số hóa; dọn helper. Test xanh.
- [x] **Bước 5 — Tài liệu**: PLAN.md F-008 Done (+ ghi chú F-013 DTO, F-014 view lọc dòng); TECH §3.2/§3.3/§3.4
  (`Inspect`, thứ tự từ chối, thứ tự `Chance` bước 2 đã cụ thể); GDD §6.2 AC-1/2/3/5 (logic) + Systems Index #8;
  brewing.md §Công thức (dòng khởi đầu = chance/min/max); LOG + CHECKPOINT. Đề xuất commit
  `feat(inspect): add hidden stat lines, lens Inspect command and feed lines into battle`.

## 5. Rủi ro & đánh đổi

- **Rò rỉ dòng ẩn**: `RunState`/`ItemInstance` chứa dòng chưa lộ — chấp nhận ở Core (Trận cần); chặn ở `RunView`
  (F-014, ADR-007). `BattleLog.Hero` mang SB/PD/HT **sau khi dâng** — GDD cho phép lộ trong Trận, nhưng UI chỉ nên hiện
  thuộc tính khi kích hoạt (F-017) → ghi chú cho F-017.
- **Từ chối `NothingToReveal` tự nó lộ "hết dòng ẩn"** — GDD Edge Cases chấp nhận (người chơi đã trả Kính cho các dòng).
- **Đổi chữ ký**: `ItemInstance`, `RecipeData`, `ItemData` thêm tham số **tùy chọn** → 41 chỗ `new ItemInstance(` trong
  test không phải sửa.
- **Chuỗi RNG**: thêm lần rút `Chance` chỉ khi công thức có dòng — content test mặc định không có dòng nên test cũ giữ
  nguyên kỳ vọng.
- **Testability**: PASS.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - `ItemInstance.Lines` lưu bản sao (`StatLine[]`), rỗng dùng `Array.Empty` — tránh alias với mảng người gọi.
  - Kiểm phase của `Inspect`: Workbench → `GridLocked` nếu không phải Editing; phase khác ngoài Offering → `WrongPhase`
    (cùng ý nghĩa với `ValidateEditable` nhưng thêm Offering).
  - `PickLines` cấp phát 1 `List<int>` mỗi lần soi (≤ 4 phần tử, theo lệnh người chơi — không phải vòng nóng).
  - Thêm `StateFactory.AddProduct` / `AddItem` (helper test) và tham số `ContentFactory.Create(steelStatLines, lensRevealLines = 1)`.
- **Deviations** (lệch so với plan và lý do):
  - Gộp chạy Bước 1 vào Bước 2 (một lần batchmode skeleton + test mới), như F-011.
  - Test random `Apply_RandomCommandsWithInspect_RevealedLinesOnlyGrow` viết thành test mới trong `RunLoopInvariantTest` thay vì
    sửa `NextCommand` chung — giữ nguyên chuỗi lệnh ngẫu nhiên của các test bất biến cũ.
  - Bước 4: không refactor — helper `BitCount` (5 dòng) lặp ở `GameRulesInspectTest` và `RunLoopTest`, giữ để mỗi lớp test tự đủ.
- **Tradeoffs**:
  - 6 test ở skeleton pass "trống" (ca trung tính/phủ định: `Create_RecipeWithoutStatLines_*`, `Create_StatLineAtBounds_*`,
    `Create_Lens_ExposesRevealLines`, `Roll_Chance0_*`, `Roll_NoStatLines_*`, `ValueOf_StatAbsent_*`) + `Apply_BrewStatLines_DrawAfterMagic…`
    (kỳ vọng tính qua chính `StatLineRules.Roll`) + test random Inspect (mọi Inspect bị từ chối) — các test dương cùng nhóm fail ở skeleton.
  - Test random Inspect chọn uid ngẫu nhiên nên ít khi soi thành công; hành vi soi đã khóa bằng `GameRulesInspectTest` tất định —
    test random chỉ canh bất biến "mask chỉ tăng, không vượt số dòng, từ chối không đổi hash".
- **Test changes** (sửa test sau Bước 2 và lý do):
  - `MarketRulesShopTest` (2 test dựng item Kính riêng): thêm `revealLines: 1` — validator R2 mới từ chối Kính `RevealLines = 0` (đúng thiết kế).
- **Open questions**: - None
