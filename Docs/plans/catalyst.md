# Plan: Xúc tác & Effect primitive (F-007)

> **Trạng thái**: Done (D1–D12 duyệt 2026-11-04; 997/997 EditMode xanh) · **Cập nhật**: 2026-11-04
> **GDD**: `design/economy.md` §Xúc tác & Effect primitive, Edge Cases (6 Xúc tác, thứ tự), AC-2c, AC-6; `design/brewing.md`
> §Khớp công thức bước 4, §Formulas (`Quality`), AC-1; GDD §6.1 Edge Cases (mọi `Chance` luôn tung), §6.2 (`AddStat`);
> `design/ui-ban-luyen.md` §6 Kệ Xúc tác (nháy "đã kích hoạt" lúc hiện kết quả); TECH §3.3 (Chợ, vòng lượt), §3.4 (thứ tự
> rút `Chance` bước 3) · **ADR**: ADR-001, ADR-004 (số nguyên), ADR-005 (content trong Core), ADR-007 (chống rò rỉ)
> · **Loại**: Logic/Data (Core)
> Xây trên F-000…F-006, F-008, F-009 phần 1, F-010, F-011 (890/890 EditMode xanh, commit `033f516`).

## 1. Bối cảnh

Xúc tác đã mua/nhận được (F-006/F-011), chiếm suất `CatalystSlots`, sắp thứ tự được (`ReorderCatalyst`, F-005) — nhưng
chưa làm gì. `BrewingRules` tính `Quality = BaseEssence` với 0 Potency / không hệ số; `SeasonRules.StartTurn` để sẵn chỗ
cho `OnShopRefresh`. Plan này thêm **Effect primitive** (Trigger + Condition + Action từ whitelist) dạng data trên Xúc
tác, bộ chạy `CatalystRunner`, và nối vào Nung + đầu lượt. Pillar P3 (xây bộ Xúc tác "độc quyền", quyết định thứ tự),
P2 (tất định, đọc được).

**Yêu cầu** (đánh số để truy vết):
- R1 — **Content Effect**: `EffectData(trigger, conditions, action)`; Xúc tác (`Kind = Catalyst`) mang 0…3 Effect
  (`ItemData.effects`, tùy chọn). Loại khác mang Effect → lỗi content (D11).
  - Trigger: `OnBrewStart`, `OnRecipeMatch`, `OnBrewFail`, `OnBrewEnd`, `OnShopRefresh`.
  - Condition: `IngredientIs(item)` (lưới có ≥ 1 vật phẩm `item`), `RecipeIs(product)` (công thức khớp ra `product`),
    `AdditiveCount(n)` (số phụ gia trên lưới ≥ n), `Chance(%)`. Danh sách 0…3, mỗi loại tối đa 1 lần, nối **AND** (D2).
  - Action (đúng 1 / Effect): `AddEssence(n)`, `AddPotency(n)`, `MulPotency(%)`, `AddStat(stat, n)`, `GainGold(n)`,
    `CreateIngredient(item)`. **Không** có `AddMagicChance` dạng Effect (D1).
- R2 — **Validator** (`ContentDb.Create`): `AddEssence/AddPotency/MulPotency/AddStat` chỉ với `OnRecipeMatch`;
  `OnShopRefresh` chỉ điều kiện `Chance`; `RecipeIs` chỉ với `OnRecipeMatch`/`OnBrewEnd` và `product` phải có `Recipe`;
  `IngredientIs` chỉ khoáng/phụ gia; `CreateIngredient` chỉ nguyên liệu gốc (khoáng/phụ gia, không `Recipe`, không phế
  phẩm); id không tồn tại → lỗi; số nằm trong khoảng `EffectLimits` (D8).
- R3 — **Thứ tự kích hoạt** (D3): pha `OnBrewStart` → (`OnRecipeMatch` nếu khớp | `OnBrewFail` nếu phế phẩm) →
  `OnBrewEnd`; trong mỗi pha: Xúc tác theo thứ tự Túi; trong mỗi Xúc tác: Effect theo thứ tự khai báo. Effect "kích hoạt"
  khi trigger thuộc pha và **mọi** điều kiện đúng.
- R4 — **RNG** (D4): khi trigger của Effect cháy, điều kiện `Chance` của nó **luôn** được tung đúng 1 `Roll(%)` — kể cả
  khi điều kiện khác sai hoặc action sẽ bị chặn. Nung: stream `Chance`, **sau** ma thuật + dòng thuộc tính (TECH §3.4 bước
  3). `OnShopRefresh`: stream `Shop`, **sau** `RefreshShop`. Không Effect nào cháy → không rút thêm.
- R5 — **`Quality`** (D5): `Essence = BaseEssence + Σ AddEssence`, `Potency = Σ AddPotency`, hệ số = mọi `MulPotency`
  → `QualityFormula.Compute` (đã sắp tăng dần) ⇒ kết quả không phụ thuộc thứ tự Xúc tác (brewing AC-1: 18, +3, ×150 → 108).
- R6 — **`AddStat`** (D6): cộng vào dòng cùng thuộc tính nếu có, chưa có → thêm dòng mới cuối `Lines`; giá trị kẹp
  `≤ BattleData.MaxStatValue`. Dòng do Xúc tác tạo/sửa vẫn ẩn (`RevealedMask` = 0 như cũ).
- R7 — **Action ngoài `Quality`** (D7) áp **sau** khi kết quả vào Túi, theo thứ tự kích hoạt: `GainGold` → `Gold += n`;
  `CreateIngredient` → 1 vật phẩm cuối Túi (`NextUid++`) nếu `BagRules.HasRoom(run, 1)`, không thì **bị chặn** (không tạo,
  ghi `Blocked`). Không giới hạn số lần ngoài "mỗi trigger cháy tối đa 1 lần / lượt".
- R8 — **`OnShopRefresh`**: trong `SeasonRules.StartTurn`, ngay sau `RefreshShop`, trước `TurnStarted` → đúng 1 lần mỗi
  lượt Bàn luyện; `Reroll` / mua / bán **không** kích hoạt (economy AC-2c: 3 lần / Mùa với `TurnsPerSeason` = 3).
- R9 — **Ghi nhận** (D10): `BrewLog.Activations` (`CatalystActivation`: `CatalystUid`, `EffectIndex`, `Blocked`) theo thứ
  tự kích hoạt (lưu được → mở lại vào Result vẫn nháy đúng); event `CatalystActivated(catalystUid, effectIndex, trigger,
  blocked)` sau `BrewCompleted` / trước `TurnStarted`. `StateHasher` ghi `Activations` trong `LastBrew`.
- R10 — **Không rò rỉ**: `BrewLog` / `CatalystActivated` không chứa `Recipe`/`Pattern`/khoảng cách/dòng thuộc tính (test
  reflection hiện có mở rộng). Trận, Chợ, giá bán không đổi.

**Ngoài phạm vi**: view kệ + nháy kích hoạt (F-014); DTO save mang `Effects` id / `Activations` (F-013 — ghi chú);
`OnSeasonStart` (sau MVP); Thuốc thử (sau MVP); content Xúc tác thật + giá + `UtilitySlotWeights` (`balance`); fuzz
1.000 run có Xúc tác (F-009 phần 2 — chỉ cần content factory có Xúc tác).
**TBD (không làm)**: danh sách Xúc tác MVP (GDD §5 "TBD"), giá bán Xúc tác đặc biệt (economy Open Questions — giữ mặc định).

### Quyết định cần duyệt (đề xuất)

| # | Quyết định | Lý do / phương án khác |
|---|---|---|
| D1 | Phụ gia giữ field `ItemData.MagicChance` = chính là `AddMagicChance` (trigger ngầm: nằm trong khoảng trống của lượt khớp). Không tạo Effect object cho phụ gia; Xúc tác không có `AddMagicChance`. Đóng câu hỏi mở "field vs Effect". | Effect cho phụ gia chỉ đổi chỗ gom (`MagicChanceFormula.ForBrew`) mà thêm content/validator; field đã validate 0–50. Ghi GDD: "phụ gia chỉ dùng `AddMagicChance` = field `MagicChance`". |
| D2 | 1 action / Effect; 0…3 Effect / Xúc tác; 0…3 điều kiện / Effect, mỗi loại ≤ 1, AND. 0 Effect hợp lệ. | 0 Effect hợp lệ để không đổi content test hiện có (`ContentFactory` có Xúc tác trống); content thật soát ở `balance`. OR / nhiều action → content mới cần primitive mới (economy: "đề xuất, không tự thêm"). |
| D3 | Thứ tự: pha-trước (Start → Match/Fail → End), rồi Túi, rồi khai báo. | Phương án "Túi-trước" (Xúc tác 1 chạy cả 3 pha rồi tới Xúc tác 2) làm `OnBrewStart` của Xúc tác 2 chạy sau `OnBrewEnd` của Xúc tác 1 — trái nghĩa tên trigger. |
| D4 | `Chance` nung → stream `Chance` sau dòng thuộc tính; `OnShopRefresh` → stream `Shop`. Luôn tung khi trigger cháy. | Đúng TECH §3.4 + economy §Dependencies. Lượt rút chỉ phụ thuộc (lưới, kết quả khớp, bộ Xúc tác) — không phụ thuộc kết quả tung khác. |
| D5 | Gom mọi action `Quality` trước, tính một lần bằng `QualityFormula`. | Bất biến thứ tự (brewing §Formulas: Add trước, Mul sau). |
| D6 | `AddStat` trên dòng chưa có → **thêm dòng**; kẹp `MaxStatValue`; không tự lộ. | Phương án "chỉ cộng dòng có sẵn" làm Xúc tác vô dụng khi dòng không xuất hiện (và lộ gián tiếp dòng có/không qua kết quả Trận). |
| D7 | Action vàng / tạo nguyên liệu áp sau khi kết quả vào Túi, theo thứ tự kích hoạt; `CreateIngredient` tạo đúng 1; "giới hạn số lần" = mỗi trigger ≤ 1 lần / lượt (≤ 3 nung + 3 làm mới / Mùa). | Kết quả nung luôn vừa Túi (brewing) giữ nguyên; thứ tự có nghĩa: `CreateIngredient` trước lấp chỗ cuối → cái sau bị chặn (economy "thứ tự Xúc tác"). Bộ đếm theo run cần thêm state + save → để `balance` quyết nếu cần. |
| D8 | Khoảng (`EffectLimits`): `AddEssence` 1–1 000; `AddPotency` 1–100; `MulPotency` 101–500 (`QualityFormula.MaxMulPercent`); `AddStat` 1–`MaxStatValue`; `GainGold` 1–1 000; `AdditiveCount` 1–8; `Chance` 1–100. | Chặn content sai/tràn; số thật ở `balance`. 100 % `MulPotency` vô nghĩa → loại. |
| D9 | `RecipeIs` chỉ ở `OnRecipeMatch`/`OnBrewEnd` (ở `OnBrewEnd` phế phẩm → sai); `IngredientIs`/`AdditiveCount` đọc lưới **trước** tiêu hao. | `RecipeIs` ở `OnBrewStart`/`OnBrewFail` luôn vô nghĩa/sai → lỗi content sớm. |
| D10 | Ghi nhận cả trong `BrewLog` (lưu, hash) và event; `Blocked` cho `CreateIngredient` bị chặn (UI: "Túi đầy"). Kích hoạt `OnShopRefresh` chỉ có event (nháy ngay khi lượt mở, không cần lưu). | `BrewLog` là nguồn duy nhất còn sống khi mở lại vào Result (GDD §6.4). |
| D11 | Effect khai báo trên `ItemData` (chuỗi id) → `ContentDb` resolve thành `CatalystEffect` (`ItemId` đã tra) — `ContentDb.EffectsOf(ItemId)` (rỗng nếu không có). | Theo mẫu `RecipeData → Recipe`, `RewardData → Reward`. Không cần `EffectId` (không lưu Effect vào save; save lưu `ItemId` Xúc tác). |
| D12 | Không Command mới, không `RejectReason` mới. Bán / sắp xếp / suất 5 Xúc tác giữ như F-005/F-006/F-011 (chỉ thêm test khóa AC-6 nếu thiếu). | Effect chạy bên trong `Brew` và `StartTurn`. |

## 2. Thiết kế triển khai

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Content/EffectTrigger.cs` | enum 5 trigger | Core | Mới |
| `Core/Content/EffectConditionKind.cs` | enum `IngredientIs`, `RecipeIs`, `AdditiveCount`, `Chance` | Core | Mới |
| `Core/Content/EffectActionKind.cs` | enum 6 action | Core | Mới |
| `Core/Content/EffectConditionData.cs` | record: `Kind`, `ItemId` (chuỗi, cho Ingredient/Recipe), `Value` (n / %) | Core | Mới |
| `Core/Content/EffectActionData.cs` | record: `Kind`, `Amount`, `Stat` (AddStat), `ItemId` (chuỗi, CreateIngredient) | Core | Mới |
| `Core/Content/EffectData.cs` | record: `Trigger`, `Conditions`, `Action` | Core | Mới |
| `Core/Content/EffectLimits.cs` | hằng số khoảng D2/D8 | Core | Mới |
| `Core/Content/CatalystEffect.cs` | Effect đã resolve (`ItemId` thật, mảng điều kiện bất biến) | Core | Mới |
| `Core/Content/ItemData.cs` | + tham số tùy chọn `effects` (mặc định rỗng) | Core | Sửa |
| `Core/Content/ContentDb.cs` | `ResolveEffects` + validate R2 sau khi có `itemIds`; `EffectsOf(ItemId)` | Core | Sửa |
| `Core/Rules/CatalystRunner.cs` | `EvaluateBrew` (R3, R4 — chỉ rút RNG, không sửa state) → `CatalystBrewOutcome`; `ApplyBrewOutcome` (R7); `RunShopRefresh` (R8) | Core | Mới |
| `Core/Rules/CatalystBrewOutcome.cs` | kết quả gom: `EssenceBonus`, `PotencyBonus`, `MulPercents`, `StatBonuses`, danh sách kích hoạt (+ action chờ áp) | Core | Mới |
| `Core/Rules/StatLineRules.cs` | + `ApplyBonuses(StatLine[] rolled, IReadOnlyList<StatLine> bonuses, int max)` (R6) | Core | Sửa |
| `Core/Rules/BrewingRules.cs` | gọi `EvaluateBrew` sau dòng thuộc tính (scrap: sau match); `Quality` từ outcome; `ApplyBrewOutcome` sau `Bag.Add(result)`; `BrewLog` kèm activations; event | Core | Sửa |
| `Core/Rules/SeasonRules.cs` | `StartTurn` gọi `CatalystRunner.RunShopRefresh` sau `RefreshShop` | Core | Sửa |
| `Core/Logs/CatalystActivation.cs` | `readonly struct`: `CatalystUid`, `EffectIndex`, `Blocked` | Core | Mới |
| `Core/Logs/BrewLog.cs` | + `Activations` (tham số `Create` tùy chọn, mặc định rỗng) | Core | Sửa |
| `Core/Commands/CatalystActivated.cs` | event | Core | Mới |
| `Core/State/StateHasher.cs` | ghi `LastBrew.Activations` | Core | Sửa |
| `Tests/EditMode/TestUtils/ContentFactory.cs` | + `CreateWithCatalysts(params ItemData[] catalysts)`, helper `Catalyst(id, params EffectData[])` | Tests | Sửa |

**API**
```csharp
public enum EffectTrigger { OnBrewStart = 0, OnRecipeMatch = 1, OnBrewFail = 2, OnBrewEnd = 3, OnShopRefresh = 4 }
public enum EffectConditionKind { IngredientIs = 0, RecipeIs = 1, AdditiveCount = 2, Chance = 3 }
public enum EffectActionKind { AddEssence = 0, AddPotency = 1, MulPotency = 2, AddStat = 3, GainGold = 4, CreateIngredient = 5 }

public sealed class EffectConditionData { public EffectConditionData(EffectConditionKind kind, int value = 0, string itemId = null); }
public sealed class EffectActionData { public EffectActionData(EffectActionKind kind, int amount = 0, StatKind stat = default, string itemId = null); }
public sealed class EffectData { public EffectData(EffectTrigger trigger, EffectActionData action, IReadOnlyList<EffectConditionData> conditions = null); }

public sealed class ItemData { public ItemData(..., int revealLines = 0, IReadOnlyList<EffectData> effects = null); public IReadOnlyList<EffectData> Effects { get; } }
public sealed class ContentDb { public IReadOnlyList<CatalystEffect> EffectsOf(ItemId id); }   // rỗng nếu không phải Xúc tác / không Effect

public static class CatalystRunner
{
    // Lưới chưa tiêu hao; recipe null = phế phẩm. Chỉ rút `chance`; không sửa run.
    public static CatalystBrewOutcome EvaluateBrew(IReadOnlyList<ItemInstance> bag, IReadOnlyList<ItemInstance> grid,
                                                   Recipe recipe, ContentDb content, ref Pcg32 chance);
    // Sau khi kết quả đã vào Túi + lưới đã trống: GainGold / CreateIngredient theo thứ tự; trả activations (đã có Blocked).
    public static IReadOnlyList<CatalystActivation> ApplyBrewOutcome(RunState run, ContentDb content, CatalystBrewOutcome outcome);
    // StartTurn: sau RefreshShop. Rút stream Shop; áp ngay; thêm CatalystActivated vào events.
    public static void RunShopRefresh(RunState run, ContentDb content, List<GameEvent> events);
}
public sealed class CatalystActivated : GameEvent { int CatalystUid; int EffectIndex; EffectTrigger Trigger; bool Blocked; }
```

**Luồng `Brew` mới** (khớp): match → `Roll(Magic)` → `StatLineRules.Roll` → `EvaluateBrew` (Chance Xúc tác) →
`Quality` + `ApplyBonuses` → tạo kết quả → chụp lưới cho `BrewLog` → xóa lưới → `Bag.Add(result)` → `ApplyBrewOutcome`
→ `BrewLog.Create(..., activations)` → events `BrewCompleted`, rồi `CatalystActivated`×n. Phế phẩm: match → `EvaluateBrew`
(pha Start/Fail/End) → như trên. Lưới chụp bằng mảng tạm 9 ô trước khi xóa (1 alloc / lượt Nung — không phải vòng nóng).

**Interface phụ thuộc**: không — `Pcg32` truyền `ref`, `RunState`/`ContentDb` dựng trong test.
**Dữ liệu**: không SO mới (Content asmdef / baker chưa có — Effect là `*Data` trong Core, ADR-005).
**Scene / prefab bị chạm**: không. `scripts/find_guid_refs.py`: không áp dụng (không sửa MonoBehaviour).
**Lý do thiết kế**: tách *đánh giá* (pure + RNG) khỏi *áp dụng* (mutate) để giữ validate-trước-mutate-sau và test được
từng nửa; `CatalystRunner` là tên đã có trong TECH §3.3.

## 3. Test cases

### Editor tests
(none)

### Unit tests
#### `ContentDbEffectValidationTest` (mới — Content)
| Test method | Kiểm tra |
|---|---|
| `Create_CatalystWithValidEffects_ResolvesEffectsInDeclaredOrder` | `EffectsOf` trả đúng số, đúng thứ tự, `ItemId` đã resolve |
| `Create_NonCatalystWithEffects_Throws` (kind: Mineral, Additive, Lens, Scrap) | `ArgumentException` |
| `Create_CatalystWithMoreThanMaxEffects_Throws` | `ArgumentException` |
| `Create_EffectWithDuplicateConditionKind_Throws` | `ArgumentException` |
| `Create_EffectWithMoreThanMaxConditions_Throws` | `ArgumentException` |
| `Create_QualityActionOutsideOnRecipeMatch_Throws` (action × trigger không hợp lệ) | `ArgumentException` |
| `Create_ShopRefreshWithNonChanceCondition_Throws` (IngredientIs, RecipeIs, AdditiveCount) | `ArgumentException` |
| `Create_RecipeIsOnStartOrFail_Throws` (OnBrewStart, OnBrewFail) | `ArgumentException` |
| `Create_RecipeIsTargetWithoutRecipe_Throws` | `ArgumentException` |
| `Create_IngredientIsNonIngredient_Throws` (Lens, Catalyst, Scrap) | `ArgumentException` |
| `Create_CreateIngredientNonBaseItem_Throws` (thành phẩm, Kính, Xúc tác, phế phẩm) | `ArgumentException` |
| `Create_EffectUnknownItemId_Throws` | `ArgumentException` |
| `Create_AmountOutOfRange_Throws` (mỗi action: min − 1, max + 1) | `ArgumentException` |
| `Create_AmountAtBounds_Succeeds` (mỗi action: min, max) | không ném |
| `Create_ConditionValueOutOfRange_Throws` (AdditiveCount 0/9, Chance 0/101) | `ArgumentException` |
| `Create_CatalystWithoutEffects_Succeeds` | `EffectsOf` rỗng |
| `EffectsOf_NonCatalyst_ReturnsEmpty` | rỗng |

#### `CatalystRunnerTest` (mới — Rules, `EvaluateBrew` + `RunShopRefresh` trực tiếp)
| Test method | Kiểm tra |
|---|---|
| `EvaluateBrew_NoCatalysts_ReturnsNeutralAndDrawsNothing` | bonus 0, không kích hoạt, state RNG không đổi |
| `EvaluateBrew_MatchedRecipe_FiresStartMatchEndOnly` | chỉ Effect Start/Match/End kích hoạt, Fail không |
| `EvaluateBrew_Scrap_FiresStartFailEndOnly` | chỉ Effect Start/Fail/End kích hoạt |
| `EvaluateBrew_TwoCatalysts_OrdersByPhaseThenBagThenDeclaration` | thứ tự activation đúng D3 |
| `EvaluateBrew_IngredientIsPresent_Activates` / `_Absent_DoesNotActivate` | có / không kích hoạt |
| `EvaluateBrew_RecipeIsMatchingProduct_Activates` / `_OtherProduct_DoesNotActivate` | có / không |
| `EvaluateBrew_RecipeIsOnBrewEndWithScrap_DoesNotActivate` | không kích hoạt |
| `EvaluateBrew_AdditiveCount` (số phụ gia n−1, n, n+1) | false, true, true |
| `EvaluateBrew_ChanceConditionWithFailingOtherCondition_StillDrawsOnce` | RNG tiến đúng 1 `Roll`, không kích hoạt |
| `EvaluateBrew_ChanceOutcome_MatchesSameSeedRoll` | kích hoạt ⇔ `Roll(%)` của bản sao RNG |
| `EvaluateBrew_ConditionsAreAnded` | 1 điều kiện sai → không kích hoạt |
| `EvaluateBrew_QualityActions_SummedAndCollected` | `EssenceBonus`, `PotencyBonus`, `MulPercents` đúng |
| `EvaluateBrew_NonCatalystBagItems_Ignored` | vật phẩm khác trong Túi không đổi thứ tự / kết quả |
| `RunShopRefresh_GainGold_AddsGoldAndEvent` | vàng +n, 1 `CatalystActivated(trigger OnShopRefresh)` |
| `RunShopRefresh_ChanceDrawsFromShopStream` | stream `Shop` tiến 1 `Roll`, `Chance` không đổi |
| `RunShopRefresh_CreateIngredientNoRoom_Blocked` | không tạo, `Blocked = true`, `NextUid` không đổi |
| `RunShopRefresh_BrewTriggerEffects_Ignored` | không kích hoạt, không rút |

#### `StatLineRulesTest` (sửa — thêm)
| Test method | Kiểm tra |
|---|---|
| `ApplyBonuses_ExistingStat_AddsValue` | giá trị dòng cộng đúng, vị trí giữ |
| `ApplyBonuses_MissingStat_AppendsLine` | dòng mới cuối danh sách |
| `ApplyBonuses_SumAboveMax_ClampsToMax` | = `MaxStatValue` |
| `ApplyBonuses_NoBonuses_ReturnsSameLines` | không đổi |

### Integration tests (qua `GameRules.Apply`)
#### `GameRulesBrewCatalystTest` (mới)
| Test method | Kiểm tra |
|---|---|
| `Apply_BrewWithQualityCatalysts_ComputesExampleQuality` (AC-1: Essence 18 = base 15 + Add 3, +3 Potency, ×150) | `Quality` = 108 |
| `Apply_BrewWithReorderedQualityCatalysts_SameQuality` (AC-1, economy Edge Case) | `Quality` bằng nhau 2 thứ tự |
| `Apply_BrewMulPotencyAboveCap_ClampsToQualityCap` | = `QualityCap` |
| `Apply_BrewScrapWithQualityCatalysts_ScrapUnchanged` | phế phẩm `Quality` 0, không dòng |
| `Apply_BrewAddStat_ProductHasBoostedLine` | dòng thuộc tính đúng giá trị; `RevealedMask` 0 |
| `Apply_BrewGainGold` (OnBrewStart, OnRecipeMatch, OnBrewEnd) | vàng +n |
| `Apply_BrewFailGainGold_OnlyOnScrap` | vàng +n khi hỏng, không đổi khi khớp |
| `Apply_BrewCreateIngredient_AddsItemAfterResult` | Túi: kết quả rồi nguyên liệu mới; uid tăng; `LastBrewResultUid` = kết quả |
| `Apply_BrewCreateIngredientBagFull_BlockedAndLogged` | không tạo; `Activations[i].Blocked`; Túi + lưới ≤ sức chứa |
| `Apply_BrewTwoCreateIngredientOneRoom_FirstInBagOrderWins` (economy: thứ tự có nghĩa) | Xúc tác đứng trước tạo, sau bị chặn |
| `Apply_BrewWithCatalysts_LogsActivationsAndEvents` | `LastBrew.Activations` = event `CatalystActivated` cùng thứ tự, sau `BrewCompleted` |
| `Apply_BrewChanceOrder_CatalystDrawsAfterMagicAndLines` | kết quả = mô phỏng tay `Roll(magic)` → dòng → `Roll(cat)` cùng seed |
| `Apply_BrewWithCatalysts_SameStateSameResultAndHash` | 2 run cùng seed → cùng hash |
| `Apply_BrewWithoutCatalysts_ResultUnchangedFromBefore` (hồi quy) | `Quality` = `BaseEssence`, RNG như cũ |

#### `GameRulesShopRefreshCatalystTest` (mới)
| Test method | Kiểm tra |
|---|---|
| `Apply_AcceptDecree_FiresShopRefreshOnceBeforeTurnStarted` | 1 `CatalystActivated` trước `TurnStarted` |
| `Apply_SeasonWithRerollsAndTrades_ShopRefreshFiresThreeTimes` (AC-2c; reroll 0/2/5 lần mỗi lượt) | đếm kích hoạt = 3 |
| `Apply_Reroll_DoesNotFireShopRefresh` | không `CatalystActivated` |
| `Apply_SkipTurnAndAckBrewResult_EachFireShopRefresh` | mỗi lượt mới +1 |
| `Apply_OfferingStart_DoesNotFireShopRefresh` | hết lượt 3 → không kích hoạt |

#### Mở rộng test có sẵn
| File · test | Kiểm tra |
|---|---|
| `BrewLogTest` · test reflection không rò rỉ | thêm `CatalystActivation`/`CatalystActivated`: không field `Recipe`/`Pattern`/`Lines` |
| `StateHasherTest` · `Hash_DifferentActivations_DifferentHash` | hash khác khi `Activations` khác |
| `GameRulesBuyTest` / `GameRulesChooseRewardTest` · Xúc tác thứ 6 (AC-6) | đã có `CatalystSlotsFull` → chỉ xác nhận, thêm nếu thiếu |

### Visual verification tests
(none — không UI trong F-007)

### Manual tests
(none)

**Truy vết**: R1 → `Create_CatalystWithValidEffects_*`, `Create_NonCatalystWithEffects_Throws`; R2 → các `Create_*_Throws`
trong `ContentDbEffectValidationTest`; R3 → `EvaluateBrew_MatchedRecipe_*`, `_Scrap_*`, `_TwoCatalysts_Orders*`; R4 →
`EvaluateBrew_Chance*`, `RunShopRefresh_ChanceDrawsFromShopStream`, `Apply_BrewChanceOrder_*`; R5 →
`Apply_BrewWithQualityCatalysts_*`, `Apply_BrewWithReorderedQualityCatalysts_SameQuality`; R6 → `ApplyBonuses_*`,
`Apply_BrewAddStat_*`; R7 → `Apply_BrewGainGold`, `Apply_BrewCreateIngredient*`, `Apply_BrewTwoCreateIngredient*`; R8 →
`GameRulesShopRefreshCatalystTest`; R9 → `Apply_BrewWithCatalysts_LogsActivationsAndEvents`, `Hash_DifferentActivations_*`;
R10 → reflection test mở rộng, `Apply_BrewWithoutCatalysts_ResultUnchangedFromBefore`.
**Testability**: PASS — luật static thuần C#, RNG seed + truyền `ref`, `RunState`/`ContentDb` dựng bằng factory; đánh giá
tách khỏi áp dụng nên mỗi nửa test riêng. Ước lượng ~75 test mới.

## 4. Các bước thực hiện

> Commit theo `CLAUDE.md` §Git (Commit tự động: không; test-first: không) → chỉ đề xuất message ở cuối.
> Lệnh chạy test (Editor đóng): `"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml -logFile -`

- [x] **Bước 1 — Skeleton**: 3 enum, 3 `*Data`, `EffectLimits`, `CatalystEffect`, `CatalystActivation`,
  `CatalystActivated`, `CatalystBrewOutcome`, `CatalystRunner` (trả outcome trung tính / không làm gì),
  `StatLineRules.ApplyBonuses` (trả nguyên), `ItemData.effects`, `ContentDb.EffectsOf` (rỗng), `BrewLog.Activations`
  (rỗng); `ContentFactory` helper. Kiểm chứng: EditMode chạy → **890/890 xanh** (không đổi hành vi), 0 warning CS.
- [x] **Bước 2 — Test first**: viết test §3; chạy → test mới fail đúng lý do (validator không ném, không kích hoạt, `Quality`
  = base); test cũ xanh. Ghi vào §6 các test dự kiến pass sẵn ở skeleton (`EvaluateBrew_NoCatalysts_*`,
  `Create_CatalystWithoutEffects_Succeeds`, `EffectsOf_NonCatalyst_ReturnsEmpty`, `_DoesNotActivate`, `Apply_Reroll_*`,
  `Apply_OfferingStart_*`, `Apply_BrewWithoutCatalysts_*`, `ApplyBonuses_NoBonuses_*`).
- [x] **Bước 3 — Implement**: thứ tự validate content → resolve `EffectsOf` → `EvaluateBrew` → `ApplyBonuses` → nối
  `BrewingRules` → `ApplyBrewOutcome` + `BrewLog` + event → `RunShopRefresh` trong `StartTurn` → `StateHasher`. Kiểm chứng:
  EditMode **toàn bộ xanh**, 0 warning CS; `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .` 0 lỗi.
- [x] **Bước 4 — Refactor**: gộp test tham số hóa cùng phân vùng (khoảng amount), gọn validator Effect (một helper
  khoảng); EditMode vẫn xanh.
- [x] **Bước 5 — Bằng chứng & tài liệu**: PLAN F-007 → Done; TECH §3.1 (`EffectData` thật), §3.3 (đoạn **Xúc tác**:
  pha, thứ tự, áp sau kết quả), §3.4 bước 3 chi tiết + `OnShopRefresh` trên `Shop`; economy.md: D1 (phụ gia =
  field), D6, D7, AC-2c tick; brewing AC-1 tick; GDD Systems Index #7 → Implemented; LOG + CHECKPOINT; đề xuất commit
  `feat(catalyst): add effect primitive and catalyst runner for brew and shop refresh`.

## 5. Rủi ro & đánh đổi

- Thêm lần rút `Chance`/`Shop` chỉ xảy ra khi có Xúc tác có `Chance` → mọi test/replay cũ (không Xúc tác) giữ nguyên chuỗi
  RNG. Content thật có Xúc tác sẽ đổi chuỗi — chưa có save thật, chấp nhận.
- `OnShopRefresh` dùng stream `Shop` → kết quả tung phụ thuộc số lần reroll lượt **trước** (GDD §6.1 đã chấp nhận reroll
  làm lệch `Shop`). AC-3 §6.1 không bị ảnh hưởng (trang bị / ma thuật / chiến lợi phẩm ở stream khác).
- Xúc tác `CreateIngredient` + `OnShopRefresh` = nguyên liệu miễn phí 3 lần / Mùa; `GainGold` + `OnBrewFail` thưởng cho
  việc nung hỏng → cần GreedyAgent kiểm ở `balance` (AC-8 economy). Khoảng D8 chỉ chặn tràn, không cân bằng.
- `AddStat` thêm dòng mới (D6) → `Lines` có thể dài hơn số dòng khởi đầu của công thức, nhưng vẫn mỗi `StatKind` ≤ 1
  dòng (cộng dồn vào dòng có sẵn) ⇒ ≤ 1 + 3 dòng, `RevealedMask` (int) dư chỗ.
- Kích hoạt `OnShopRefresh` không lưu trong state (chỉ event, D10) → mở lại save giữa lượt sẽ không nháy lại; chấp nhận
  (thuần hiển thị).
- 0 Effect hợp lệ (D2) có thể để lọt Xúc tác "trống" vào content thật → checklist content ở `balance`.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - Skeleton + test-first gộp 1 lần chạy (như F-008/F-011): 997 test → 81 fail đúng lý do, 890 cũ xanh; 28 test mới pass
    sẵn ở skeleton — đều là phân vùng phủ định/trung tính (biên hợp lệ `AmountAtBounds` ×10, `CatalystWithoutEffects`,
    `EffectsOf_NonCatalyst`, các `…_0` không kích hoạt, `ConditionsAreAnded`, `NoCatalysts`, `RecipeIsOnBrewEndWithScrap`,
    `BrewTriggerEffects_Ignored`, `Reroll_DoesNotFire`, `SkipLastTurn_Offering…`, `ScrapWithQualityCatalysts`,
    `SameStateSameHash`, `EffectlessCatalyst`, `NoBonuses`, 2 test reflection mở rộng). Dự đoán được, giữ nguyên.
  - `CatalystEffect` lưu điều kiện phẳng (mỗi loại ≤ 1 — D2) thay vì mảng → `EvaluateBrew` không alloc theo điều kiện.
- **Deviations** (lệch so với plan và lý do):
  - `CatalystActivation` mang thêm `Trigger` (plan: chỉ `CatalystUid`, `EffectIndex`, `Blocked`) — event dựng thẳng từ
    activation, UI biết pha mà không tra content.
  - Helper test là `ContentFactory.CreateWithEffects(bellows, mortar, tongs, steelStatLines, qualityCap)` + `Effect(...)`
    (plan: `CreateWithCatalysts(params ItemData[])`) — giữ Id/`ItemId` của 3 Xúc tác có sẵn; `Create` thêm `extraItems`,
    `qualityCap`.
  - Ví dụ AC-1 dùng Thép (`BaseEssence` 18) + `AddPotency` 3 + `MulPotency` 150 (plan: base 15 + `AddEssence` 3) — đúng
    số của brewing AC-1; `AddEssence` có test riêng.
  - Thêm test ngoài plan: `Create_CreateIngredientEffect_ResolvesActionItem`, `Create_EffectWithNullAction_Throws`,
    `Create_CreateIngredientWithAmount_Throws`, `RunShopRefresh_CreateIngredient_AddsItemToBagEnd`,
    `Apply_BrewWithAddEssence_AddsToBaseEssence`. Gộp `Apply_SkipTurnAndAckBrewResult_*` → `Apply_AckBrewResult_*` (SkipTurn
    đã phủ ở test đếm 3 lần / Mùa).
- **Tradeoffs**:
  - `Brew` clone lưới 9 ô (1 alloc / lượt Nung) để tạo `BrewLog` sau khi biết action nào bị chặn — không phải vòng nóng.
  - `CatalystBrewOutcome` là class (5 `List` nhỏ) cấp mỗi lượt Nung kể cả khi không có Xúc tác — đơn giản hơn pool; đo lại
    ở `optimize` nếu cần.
  - Implement xanh lần đầu (997/997); refactor: `ApplyBonuses` bỏ closure `FindIndex` → vòng lặp; 997/997, 0 warning CS,
    `check_unity_project.py` 0 lỗi.
- **Test changes** (sửa test đã commit ở Bước 2 và lý do): - None
- **Open questions**:
  - Danh sách Xúc tác MVP + giá + `UtilitySlotWeights` → `balance` (GreedyAgent kiểm `OnBrewFail`+`GainGold`, `OnShopRefresh`+`CreateIngredient`).
  - F-013: DTO save mang `BrewLog.Activations`; F-014: kệ nháy theo `CatalystActivated` / `LastBrew.Activations`.
