# Plan: Chợ & kinh tế (F-006)

> **Trạng thái**: Done (D1–D6 duyệt 2026-11-04; 430/430 EditMode xanh) · **Cập nhật**: 2026-11-04
> **GDD**: `design/economy.md` §Chợ, §Formulas, §Edge Cases, §Tuning Knobs, AC-1…AC-7; GDD §6.1 (stream `Shop`,
> `Market`, AC-2/AC-3); `design/run-flow.md` §Chiếu chỉ (hệ số giá rút ở Decree), §Bàn luyện (hàng làm mới đầu lượt),
> §Modifier Mùa; `design/ui-ban-luyen.md` §2 Chợ, §4 (kéo-mua lên lưới, vùng bán) · **ADR**: ADR-001 (Command/Reducer),
> ADR-004 (RNG stream), ADR-005 (content trong Core) · **Loại**: Logic (Core)
> Xây trên F-000…F-005 (312/312 EditMode xanh).

## 1. Bối cảnh

Vàng chỉ đến từ bán (+ thưởng trận, F-011); Chợ là nơi duy nhất biến vàng thành nguyên liệu gốc tier ≤ 3. Đây là
vòng áp lực P3 "bán gì để mua gì". Hiện Core chưa có giá, chưa có hàng Chợ, chưa có hệ số giá Mùa. Plan này thêm
**luật Chợ** (`MarketRules`, `PriceFormula`), dữ liệu (`MarketData`, `ItemData.MarketPrice`), state (`ShopState`,
`SeasonPricePercent`) và 3 Command `Buy` / `Sell` / `Reroll`. Việc **gọi** làm mới hàng đầu lượt và rút hệ số giá ở
Chiếu chỉ thuộc vòng Mùa (F-009) — F-006 cung cấp hai hàm luật để F-009 gọi.

**Yêu cầu** (đánh số để truy vết):
- R1 — Giá mua `max(1, ⌊MarketPrice × S × M / 10000⌋)`; giá bán `max(0, ⌊giá mua × SellRate / 100⌋)`, phế phẩm =
  `ScrapSellPrice` cố định; reroll lần k (k từ 0) = `RerollBase + k × RerollStep`. S = `RunState.SeasonPricePercent`,
  M = modifier % (MVP: 100 trung tính, F-011 thay). Giá **không** phụ thuộc `Quality`/`Magic`/thuộc tính. Tính trung
  gian bằng `long`.
- R2 — `RollSeasonPrice`: rút S đều trong `[SeasonPriceMin, SeasonPriceMax]` (70–130) từ stream `Market`, đúng 1 lần
  `NextInt`. `StartRun` đặt S = 100 và xóa hàng Chợ (giá trị trung tính tới khi F-009 rút ở Decree Mùa 1).
  *(2026-11-04: thay bởi F-009 run-loop D1 — `StartRun` gọi `SeasonRules.EnterDecree`, S rút ngay ở Decree Mùa 1.)*
- R3 — `RefreshShop`: đặt lại số reroll = 0 rồi bày hàng mới (R4) từ stream `Shop`. Không rút stream nào khác.
- R4 — Bày hàng: đúng `IngredientSlotCount` (4) ô nguyên liệu + 1 ô tiện ích, theo thứ tự ô.
  - Ô nguyên liệu: chỉ khoáng / phụ gia **gốc** (`Recipe == null`), tier `1…ShopMaxTier` (3). Rút tier theo
    `TierWeights` (70/25/5) **chỉ trên các tier có hàng** (tier rỗng bị bỏ, trọng số chia lại), rồi rút đều một món
    trong tier đó. Được trùng món. Không tier nào có hàng → ô rỗng.
  - Ô tiện ích: Kính soi hoặc Xúc tác theo `UtilityLensWeight` / `UtilityCatalystWeight`; số Xúc tác trong Túi ≥
    `CatalystSlots` (5) → chỉ Kính soi (không rút loại). Loại còn lại không có content → loại kia; cả hai không có →
    ô rỗng. Rút đều một món trong loại.
  - Không bao giờ có item tier > `ShopMaxTier` hoặc có `Recipe` (AC-1).
- R5 — `Buy(slot, cell?)`: trừ vàng = giá mua, ô hàng thành đã bán, vật phẩm mới (`Uid = NextUid++`, `Magic = false`,
  `Quality = 0`) vào **cuối Túi**; có `cell` → đặt thẳng lên ô đó (vật phẩm cũ trong ô về cuối Túi, như `PlaceItem`).
  Phát `ItemBought` (+ `GridChanged` khi có `cell`). Không rút RNG.
- R6 — `Buy` bị từ chối, state không đổi, theo thứ tự: chưa có run / sai phase / không Editing (`ValidateEditable`)
  → slot ngoài `[0, số ô)` (`InvalidShopSlot`) → ô đã bán / rỗng (`ShopSlotEmpty`) → `cell` sai (`InvalidCell`) /
  món không đặt được lên lưới (`ItemNotPlaceable`) → Xúc tác khi đã đủ `CatalystSlots` (`CatalystSlotsFull`) →
  `!BagRules.HasRoom(run)` (`BagFull`) → vàng < giá (`NotEnoughGold`).
- R7 — `Sell(uid)`: vật phẩm trong Túi **hoặc** trên lưới, mọi loại (kể cả thành phẩm tier cao, Xúc tác, Kính soi,
  phế phẩm); bỏ vật phẩm, cộng vàng = giá bán. Phát `ItemSold` (+ `GridChanged` nếu từ lưới). Không rút RNG.
  Từ chối: `ValidateEditable`; uid không có (`ItemNotFound`).
- R8 — `Reroll`: trừ vàng = giá reroll hiện tại, số reroll +1, bày lại cả 5 ô (R4, kể cả ô đã bán). Phát
  `ShopRerolled`. Từ chối: `ValidateEditable`; vàng < giá (`NotEnoughGold`). Chỉ rút stream `Shop`.
- R9 — Bất biến sau mọi Command: vàng ≥ 0; Túi + lưới ≤ sức chứa; số Xúc tác ≤ `CatalystSlots` (khi bắt đầu ≤);
  mua rồi bán ngay cùng món → vàng giảm (không vòng sinh lời trong một lượt).
- R10 — Content hợp lệ (`ContentDb.Create` ném `ArgumentException`): `MarketPrice ≥ 0`; `1 ≤ ShopMaxTier ≤ 10`;
  `1 ≤ IngredientSlotCount ≤ MaxShopSlots − 1` (layout ≤ 6 ô); `TierWeights` đủ `ShopMaxTier` phần tử, ≥ 0, tổng > 0;
  trọng số tiện ích ≥ 0, tổng > 0; `1 ≤ SeasonPriceMin ≤ SeasonPriceMax ≤ 1000`; `0 ≤ SellRate ≤ 99`;
  `RerollBase, RerollStep, ScrapSellPrice ≥ 0`; `CatalystSlots ≥ 1`.
- R11 — State hash ghi `SeasonPricePercent`, hàng Chợ (từng ô: item + đã bán) và số reroll.

**Ngoài phạm vi**:
- Gọi `RollSeasonPrice` ở Decree, `RefreshShop` đầu mỗi lượt, `SkipTurn`/`AckBrewResult`, AC-2b phần Bỏ qua, AC-2c
  (3 lần/Mùa) → **F-009**. GDD §6.1 AC-2 (cùng seed → cùng hàng lượt 1) kiểm đầy đủ ở F-009; F-006 kiểm tất định
  ở mức hàm.
- Trigger `OnShopRefresh` + chạy Effect (`GainGold`, `CreateIngredient`) → **F-007** (gọi sau `RefreshShop`; `Reroll`
  không kích hoạt — ghi trong XML doc).
- Modifier Mùa (`ScaleParam` giá mua/bán/reroll, `OffsetParam` reroll miễn phí), vàng thưởng trận → **F-011**.
- Danh vọng mở nguyên liệu vào bể Chợ (lọc bể) → **F-012**. Kính soi `Inspect` → **F-008**.
- View Chợ / vùng bán / modal xác nhận / `SellConfirmTier` → **F-014**. Save DTO cho `Shop`, `SeasonPricePercent` →
  **F-013** (ghi vào backlog F-013).
- SO `RunDef`/`ItemDef.MarketPrice` + `ContentBaker` (Content chưa có code). GreedyAgent AC-8 → `balance`.

**TBD**: số liệu `UtilitySlotWeights` và `MarketPrice` từng item — chỉ là **số** (chốt ở `balance`); cơ chế đã rõ
nên code đọc từ content, test dùng số của `ContentFactory` (D2).

**Quyết định (người dùng duyệt 2026-11-04, giữ nguyên đề xuất)**:
- D1: F-006 **không** nối vào vòng lượt — chỉ cung cấp `RefreshShop` / `RollSeasonPrice`; F-009 gọi. `StartRun` chỉ
  đặt S = 100 + xóa hàng. (Phương án khác: rút S + bày hàng ngay trong `StartRun` — trộn trách nhiệm Decree vào F-006.)
- D2: `UtilitySlotWeights` là dữ liệu `MarketData` ngay từ bây giờ (mặc định test 50/50), số thật chốt ở `balance` —
  không bỏ ô tiện ích.
- D3: Xúc tác đang có trong Túi **vẫn có thể** xuất hiện ở Chợ (GDD không cấm; Xúc tác không gộp chồng nên sở hữu 2
  bản cùng loại là hợp lệ). Nếu muốn loại trừ → ghi vào economy.md trước khi build.
- D4: Có `Buy(slot, cell)` mua thẳng lên lưới (TECH §3.3, ui-ban-luyen §4 kéo Chợ → ô lưới) — làm luôn để view không
  phải gửi 2 lệnh không nguyên tử.
- D5: Phế phẩm bán giá cố định `ScrapSellPrice`, **không** nhân hệ số Mùa. Kính soi/Xúc tác bán theo công thức chung.
- D6: Bể nguyên liệu gộp khoáng + phụ gia trong cùng tier (rút đều trên tập gộp). GDD §3 ghi "phụ gia tier 1 xuất
  hiện ở Chợ Mùa 3–4" — coi là nhịp onboarding do F-012/`balance` lo (bể theo Danh vọng / content), không chặn F-006.

## 2. Thiết kế triển khai

| Class / file (dưới `Assets/_Project/Scripts/`) | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Content/MarketData.cs` | tuning Chợ (R10); `const MaxShopSlots = 6` | Core | Mới |
| `Core/Content/ItemData.cs` | + `int MarketPrice` (tham số ctor tùy chọn `marketPrice = 0`, cuối danh sách) | Core | Sửa |
| `Core/Content/ContentDb.cs` | + tham số `MarketData market` (bắt buộc, cuối); validate R10; dựng sẵn bể `ShopPool(tier)`, `Lenses`, `Catalysts` (mảng `ItemId` tăng dần) | Core | Sửa |
| `Core/State/ShopSlot.cs` | `readonly struct (ItemId ItemId, bool IsAvailable)`; `ShopSlot.None` = ô rỗng/đã bán | Core | Mới |
| `Core/State/ShopState.cs` | `List<ShopSlot> Slots` (rỗng = chưa bày), `int RerollCount` | Core | Mới |
| `Core/State/RunState.cs` | + `int SeasonPricePercent`, `ShopState Shop { get; } = new ShopState()` | Core | Sửa |
| `Core/State/StateHasher.cs` | ghi R11 (sau `LastBrew`, trước RNG) | Core | Sửa |
| `Core/Rules/PriceFormula.cs` | 3 công thức thuần (R1) | Core | Mới |
| `Core/Rules/MarketRules.cs` | giá theo state, `RollSeasonPrice`, `RefreshShop`, `Buy`, `Sell`, `Reroll` | Core | Mới |
| `Core/Rules/BagRules.cs` | + `public static int CountCatalysts(RunState run, ContentDb content)` (dùng lại trong `ReorderCatalyst`) | Core | Sửa |
| `Core/Rules/GridRules.cs` | `IsPlaceable` → `public` (Buy-lên-lưới dùng chung luật) | Core | Sửa |
| `Core/Rules/RunStartRules.cs` | `SeasonPricePercent = MarketRules.NeutralPercent`; `Shop.Slots.Clear()`, `RerollCount = 0` | Core | Sửa |
| `Core/Rules/GameRules.cs` | dispatch `Buy` / `Sell` / `Reroll` | Core | Sửa |
| `Core/Commands/Buy.cs`, `Sell.cs`, `Reroll.cs` | Command | Core | Mới |
| `Core/Commands/ItemBought.cs`, `ItemSold.cs`, `ShopRerolled.cs` | sự kiện | Core | Mới |
| `Core/Commands/RejectReason.cs` | + `InvalidShopSlot = 15`, `ShopSlotEmpty = 16`, `NotEnoughGold = 17`, `BagFull = 18`, `CatalystSlotsFull = 19` | Core | Sửa |

**Public API**
```csharp
public sealed class MarketData
{
    public const int MaxShopSlots = 6;   // layout Chợ 3×2 (ui-ban-luyen §6)
    public MarketData(int shopMaxTier, int ingredientSlotCount, IReadOnlyList<int> tierWeights,
                      int utilityLensWeight, int utilityCatalystWeight, int seasonPriceMin, int seasonPriceMax,
                      int sellRatePercent, int rerollBase, int rerollStep, int catalystSlots, int scrapSellPrice);
    // + property cùng tên; TierWeights[i] = trọng số tier i + 1
}

public static class PriceFormula
{
    public static int Buy(int marketPrice, int seasonPercent, int modifierPercent);  // tham số < 0 → ArgumentOutOfRange
    public static int Sell(int buyPrice, int sellRatePercent);
    public static int Reroll(int rerollBase, int rerollStep, int rerollCount);
}

public static class MarketRules
{
    public const int NeutralPercent = 100;
    public static int BuyPrice(RunState run, ContentDb content, ItemId item);
    public static int SellPrice(RunState run, ContentDb content, ItemId item);       // phế phẩm → ScrapSellPrice
    public static int RerollPrice(RunState run, ContentDb content);
    public static void RollSeasonPrice(RunState run, ContentDb content);             // F-009 gọi ở Decree
    public static void RefreshShop(RunState run, ContentDb content);                 // F-009 gọi đầu lượt; F-007 kích OnShopRefresh sau đó
    public static CommandResult Buy(RunState run, ContentDb content, Buy command);
    public static CommandResult Sell(RunState run, ContentDb content, Sell command);
    public static CommandResult Reroll(RunState run, ContentDb content, Reroll command);
}

public sealed class Buy : ICommand { public Buy(int slot, int? cell = null); int Slot; int? Cell; }
public sealed class Sell : ICommand { public Sell(int uid); int Uid; }
public sealed class Reroll : ICommand { }
public sealed class ItemBought : GameEvent { int Slot; int Uid; int Price; }
public sealed class ItemSold : GameEvent { int Uid; int Price; }
public sealed class ShopRerolled : GameEvent { int Price; }
```

**Thuật toán bày hàng** (`FillSlots`, private, dùng chung cho `RefreshShop` và `Reroll`; `ref Pcg32 shop =
run.Rng.Get(RngStream.Shop)`):
1. `Slots.Clear()`. Với mỗi ô nguyên liệu `s = 0…IngredientSlotCount−1`: `total = Σ TierWeights[t]` trên tier có
   `ShopPool(t).Count > 0`; `total == 0` → `ShopSlot.None`; ngược lại `r = shop.NextInt(total)` → tier đầu tiên mà
   tổng cộng dồn > r → `pool[shop.NextInt(pool.Count)]`.
2. Ô tiện ích: `lensOk = Lenses.Count > 0 && LensWeight > 0`; `catOk = Catalysts.Count > 0 && CatalystWeight > 0 &&
   CountCatalysts < CatalystSlots`. Cả hai → `shop.NextInt(lw + cw) < lw ? Lens : Catalyst`; một → loại đó (không rút
   loại); không → `None`. Rồi `pool[shop.NextInt(pool.Count)]`.
3. Không alloc ngoài `List` có sẵn (capacity 6) — chạy theo lượt/lệnh, không theo frame.

**Interface phụ thuộc**: không (luật static; RNG qua `run.Rng`).
**Dữ liệu**: `MarketData` (từ `RunDef` sau — knob economy.md §Tuning Knobs), `ItemData.MarketPrice` (`ItemDef`).
`ContentFactory`: `CreateMarket()` = 3 / 4 / [70, 25, 5] / 50 / 50 / 70–130 / 75 / 2 / 1 / 5 / 1; giá item tạm (Sắt 4,
Đồng 5, Than 3, Thép 12…); thêm `ore_silver` (tier 3 gốc) và `ore_mithril` (tier 4 gốc — phải không bao giờ lên kệ).
**State / hash**: `SeasonPricePercent`, `Shop.Slots`, `Shop.RerollCount` (R11).
**Scene / prefab bị chạm**: không.
**Lý do thiết kế**: giá **tính** từ `MarketPrice × S` mỗi lần (không lưu trong ô) → không lệch khi S/modifier đổi và
không phải hash giá; một hàm `FillSlots` cho cả làm mới và reroll để hai đường không thể lệch luật; bể dựng sẵn trong
`ContentDb` theo thứ tự `ItemId` (mảng, không `Dictionary` — TECH §3.3) → tất định và không lọc lại mỗi lần bày.
`Buy` lên lưới gộp trong một Command để nguyên tử (D4).

## 3. Test cases

### Editor tests
(none)

### Unit tests
#### PriceFormulaTest (`Assets/Tests/EditMode/Core/Rules/PriceFormulaTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Buy_GddExample_ReturnsTwelve` (10, 120, 100) | 12 |
| `Buy_FractionalResult_Floors` (7, 130, 100) | 9 |
| `Buy_ResultBelowOne_ReturnsOne` (marketPrice: {0, 1}, S 70) | 1 |
| `Buy_NegativeInput_Throws` (từng tham số −1) | `ArgumentOutOfRangeException` |
| `Sell_GddExample_ReturnsNine` (12, 75) | 9 |
| `Sell_BuyPriceOne_ReturnsZero` | 0 |
| `Sell_AnyBuyPrice_BelowBuyPrice` (buy 1…500, rate 75) | `Sell < Buy` với mọi giá trị |
| `Reroll_CountZeroToTwo_ReturnsTwoThreeFour` (k: {0, 1, 2}) | 2, 3, 4 |
| `Reroll_NegativeCount_Throws` | `ArgumentOutOfRangeException` |

#### MarketRulesPriceTest (`.../Core/Rules/MarketRulesPriceTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `BuyPrice_SeasonPercent_ScalesMarketPrice` (Sắt 4, S 130) | 5 |
| `SellPrice_Mineral_IsSellRateOfBuyPrice` (Thép 12, S 100) | 9 |
| `SellPrice_Scrap_ReturnsScrapSellPriceAtAnySeason` (S: {70, 130}) | `ScrapSellPrice` |
| `SellPrice_SameItemDifferentQualityAndMagic_SamePrice` (AC-3) | hai giá bằng nhau |
| `RerollPrice_AfterRerolls_IncreasesByStep` (RerollCount 0 → 2) | 2 → 4 |

#### MarketRulesShopTest (`.../Core/Rules/MarketRulesShopTest.cs`) — `RefreshShop`, `RollSeasonPrice`
| Test method | Kiểm tra |
|---|---|
| `RefreshShop_Default_FillsFourIngredientsAndOneUtility` (AC-6) | 5 ô, cả 5 `IsAvailable`; ô 0–3 là khoáng/phụ gia, ô 4 là Kính soi hoặc Xúc tác |
| `RefreshShop_ManySeeds_NeverOffersTierAboveMaxOrRecipe` (AC-1: 2.000 seed × 5 lần = 10.000 lượt) | không ô nào là `ore_mithril`, Thép, Gang, Sao thép |
| `RefreshShop_ManyDraws_TierShareMatchesWeights` (10.000 ô, seed cố định) | tỉ lệ tier 1/2/3 trong ±3 điểm % của 70/25/5 |
| `RefreshShop_TierWithoutItems_RedistributesWeight` (content không có tier 3 gốc) | mọi ô nguyên liệu có tier 1–2; ô không rỗng |
| `RefreshShop_NoIngredientPool_LeavesIngredientSlotsEmpty` | ô 0–3 `!IsAvailable`; ô 4 vẫn có hàng |
| `RefreshShop_CatalystSlotsFull_UtilityIsAlwaysLens` (5 Xúc tác trong Túi, 500 seed) | ô 4 luôn là Kính soi |
| `RefreshShop_CatalystWeightZero_UtilityIsAlwaysLens` | ô 4 luôn là Kính soi |
| `RefreshShop_NoUtilityContent_LeavesUtilitySlotEmpty` | ô 4 `!IsAvailable` |
| `RefreshShop_AfterRerolls_ResetsRerollCount` (AC-5 "lượt sau về 2") | `RerollCount` = 0; `RerollPrice` = 2 |
| `RefreshShop_SameSeed_SameSlots` | hai run cùng seed → cùng dãy `ItemId` |
| `RefreshShop_OnlyAdvancesShopStream` | 6 stream khác không đổi; `Shop` đổi |
| `RollSeasonPrice_ManySeeds_StaysWithinRange` (5.000 seed) | 70 ≤ S ≤ 130; gặp cả 70 và 130 |
| `RollSeasonPrice_OnlyAdvancesMarketStream` | 6 stream khác không đổi |

#### GameRulesBuyTest (`.../Core/Rules/GameRulesBuyTest.cs`)
Hàng mẫu dựng tay: `[Sắt, Than, Đồng, Tro, Xúc tác Bễ]`, S = 100, vàng 20.

| Test method | Kiểm tra |
|---|---|
| `Apply_BuyIngredient_AddsItemToBagEnd` | món cuối Túi có `ItemId` Sắt, `Uid` = `NextUid` cũ, `Magic` false, `Quality` 0 |
| `Apply_BuyIngredient_DeductsBuyPrice` | vàng = 20 − 4 |
| `Apply_BuyIngredient_MarksSlotUnavailable` | ô 0 `!IsAvailable`; ô khác không đổi |
| `Apply_BuyIngredient_EmitsItemBought` | 1 sự kiện `ItemBought(0, uid, 4)` |
| `Apply_BuyOntoEmptyCell_PlacesItemOnGrid` | `Grid[4]` = món mới; Túi không thêm; sự kiện có `GridChanged` |
| `Apply_BuyOntoOccupiedCell_ReturnsOccupantToBagEnd` | `Grid[4]` = món mới; vật phẩm cũ ở cuối Túi |
| `Apply_BuyCatalyst_AddsToBag` (4 Xúc tác sẵn) | 5 Xúc tác trong Túi |
| `Apply_Buy_DoesNotDrawRandom` | 7 stream không đổi |
| `Apply_BuySlotOutOfRange_RejectsWithInvalidShopSlot` (slot: {−1, 5}) | Rejected `InvalidShopSlot`; hash không đổi |
| `Apply_BuyBeforeShopStocked_RejectsWithInvalidShopSlot` | Rejected `InvalidShopSlot` |
| `Apply_BuySoldSlot_RejectsWithShopSlotEmpty` | Rejected `ShopSlotEmpty`; hash không đổi |
| `Apply_BuyOntoInvalidCell_RejectsWithInvalidCell` (cell: {−1, 9}) | Rejected `InvalidCell`; hash không đổi |
| `Apply_BuyCatalystOntoCell_RejectsWithItemNotPlaceable` | Rejected `ItemNotPlaceable`; hash không đổi |
| `Apply_BuyCatalystWhenSlotsFull_RejectsWithCatalystSlotsFull` (AC-6b) | Rejected `CatalystSlotsFull`; hash không đổi |
| `Apply_BuyWhenBagAndGridFull_RejectsWithBagFull` (AC-2: Túi 5 + lưới 3 = 8) | Rejected `BagFull`; vàng không đổi |
| `Apply_BuyOntoOccupiedCellWhenFull_RejectsWithBagFull` | Rejected `BagFull` (đổi chỗ vẫn +1) |
| `Apply_BuyWithGoldBelowPrice_RejectsWithNotEnoughGold` (vàng 3, giá 4) | Rejected `NotEnoughGold`; hash không đổi |
| `Apply_BuyWithGoldEqualPrice_LeavesZeroGold` | Accepted; vàng 0 |
| `Apply_BuyWhenResultPending_RejectsWithGridLocked` | Rejected `GridLocked` |
| `Apply_BuyOutsideWorkbench_RejectsWithWrongPhase` | Rejected `WrongPhase` |
| `Apply_BuyWithoutRun_RejectsWithRunNotActive` | Rejected `RunNotActive` |

#### GameRulesSellTest (`.../Core/Rules/GameRulesSellTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Apply_SellFromBag_RemovesItemAndAddsSellPrice` (Thép, S 100) | Thép không còn; vàng +9; `ItemSold(uid, 9)` |
| `Apply_SellFromGrid_ClearsCellAndAddsSellPrice` | ô trống; vàng + giá; có `GridChanged` |
| `Apply_SellCatalyst_AddsSellPrice` | Xúc tác không còn; vàng + giá |
| `Apply_SellScrap_AddsScrapSellPrice` | vàng + `ScrapSellPrice` |
| `Apply_SellHighTierProduct_UsesItsMarketPrice` (Sao thép tier 3 có `Recipe`) | vàng + `⌊MarketPrice × 75 / 100⌋` |
| `Apply_Sell_KeepsOtherBagOrder` | thứ tự uid còn lại không đổi |
| `Apply_Sell_DoesNotDrawRandom` | 7 stream không đổi |
| `Apply_SellUnknownUid_RejectsWithItemNotFound` | Rejected `ItemNotFound`; hash không đổi |
| `Apply_SellWhenResultPending_RejectsWithGridLocked` | Rejected `GridLocked`; hash không đổi |
| `Apply_SellOutsideWorkbench_RejectsWithWrongPhase` | Rejected `WrongPhase` |
| `Apply_SellWithoutRun_RejectsWithRunNotActive` | Rejected `RunNotActive` |

#### GameRulesRerollTest (`.../Core/Rules/GameRulesRerollTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Apply_RerollThreeTimes_ChargesTwoThreeFour` (AC-5) | vàng giảm 2, rồi 3, rồi 4 |
| `Apply_Reroll_RestocksSoldSlots` | sau khi mua ô 0 rồi reroll: 5 ô `IsAvailable` |
| `Apply_Reroll_EmitsShopRerolledWithPrice` | `ShopRerolled(2)` |
| `Apply_Reroll_OnlyAdvancesShopStream` (GDD §6.1 AC-3) | 6 stream khác không đổi |
| `Apply_RerollWithGoldBelowPrice_RejectsWithNotEnoughGold` | Rejected `NotEnoughGold`; hash không đổi |
| `Apply_RerollWhenResultPending_RejectsWithGridLocked` | Rejected `GridLocked` |
| `Apply_RerollOutsideWorkbench_RejectsWithWrongPhase` | Rejected `WrongPhase` |
| `Apply_RerollWithoutRun_RejectsWithRunNotActive` | Rejected `RunNotActive` |

#### ContentDbMarketTest (`.../Core/Content/ContentDbMarketTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Create_ValidMarket_BuildsPoolsInItemIdOrder` | `ShopPool(1)` = gốc tier 1 tăng dần `ItemId`; không chứa item có `Recipe` / tier 4 |
| `Create_NegativeMarketPrice_Throws` | `ArgumentException`, message chứa Id item |
| `Create_ShopMaxTierOutOfRange_Throws` (tier: {0, 11}) | `ArgumentException` |
| `Create_IngredientSlotCountOutOfRange_Throws` (count: {0, 6}) | `ArgumentException` |
| `Create_TierWeightsWrongLength_Throws` | `ArgumentException` |
| `Create_TierWeightsNegativeOrAllZero_Throws` ({[-1, 25, 5], [0, 0, 0]}) | `ArgumentException` |
| `Create_UtilityWeightsInvalid_Throws` ({(−1, 50), (0, 0)}) | `ArgumentException` |
| `Create_SeasonPriceRangeInvalid_Throws` ({(0, 130), (131, 130), (70, 1001)}) | `ArgumentException` |
| `Create_SellRateOutOfRange_Throws` (rate: {−1, 100}) | `ArgumentException` |
| `Create_NegativeRerollOrScrapPrice_Throws` | `ArgumentException` |
| `Create_CatalystSlotsBelowOne_Throws` | `ArgumentException` |

#### StateHasherTest (bổ sung case vào `Hash_StatesDifferInOneField_*`)
| Test method | Kiểm tra |
|---|---|
| `Hash_StatesDifferInOneField_*` + 4 case: `SeasonPricePercent`, `ItemId` một ô, `IsAvailable` một ô, `RerollCount` | hash khác nhau |

#### GameRulesStartRunTest (bổ sung)
| Test method | Kiểm tra |
|---|---|
| `Apply_StartRun_SetsNeutralSeasonPriceAndEmptyShop` (run cũ có hàng + S 130) | S = 100; `Shop.Slots` rỗng; `RerollCount` 0 |

### Integration tests
#### MarketInvariantTest (`.../Core/Rules/MarketInvariantTest.cs`) — qua `GameRules.Apply`
| Test method | Kiểm tra |
|---|---|
| `Apply_RandomMarketAndGridCommands_KeepsInvariants` (5 seed × 2.000 lệnh; vàng đầu 30; mỗi 25 lệnh test gọi `RefreshShop` giả lượt mới — F-009) | sau mỗi lệnh: vàng ≥ 0, `UsedSlots ≤ BagCapacity`, số Xúc tác ≤ 5 (AC-2b phần Nung, AC-4, R9) |
| `Apply_BuyToFullThenBrew_StaysWithinCapacity` (AC-2b) | sau Mua tới đầy và Nung: `UsedSlots ≤ BagCapacity` |
| `Apply_BuyThenSellSameItem_LosesGold` (AC-7: mọi ô hàng × S {70, 100, 130}) | vàng sau < vàng trước |
| `Apply_RejectedMarketCommand_KeepsHash` (gộp trong fuzz) | Command bị từ chối → hash trước = sau |
| `Apply_SameSeedSameCommands_SameHash` | hai run cùng seed + cùng chuỗi lệnh (có `Reroll`) → cùng hash |

Fuzz: `System.Random(seed)` chọn `Buy` (slot/cell ngẫu nhiên, có giá trị sai) / `Sell` / `Reroll` / `PlaceItem` /
`RemoveFromGrid` / `Brew`; sau `Brew` test đặt `WorkbenchStep = Editing` (giả `AckBrewResult` — F-009, ghi chú trong
test như `BagCapacityInvariantTest`).

### Visual verification tests
(none — chưa có view; F-014)

### Manual tests
(none)

**Truy vết**: R1 → `PriceFormulaTest.*`, `MarketRulesPriceTest.*` · R2 → `RollSeasonPrice_*`,
`Apply_StartRun_SetsNeutralSeasonPriceAndEmptyShop` · R3 → `RefreshShop_AfterRerolls_ResetsRerollCount`,
`RefreshShop_OnlyAdvancesShopStream` · R4 → `RefreshShop_*` · R5 → `Apply_Buy*` (Accepted) · R6 →
`Apply_Buy*_RejectsWith*` · R7 → `Apply_Sell*` · R8 → `Apply_Reroll*` · R9 → `MarketInvariantTest.*`,
`Sell_AnyBuyPrice_BelowBuyPrice` · R10 → `ContentDbMarketTest.*` · R11 → `StateHasherTest` (4 case),
`Apply_SameSeedSameCommands_SameHash`
**AC economy.md**: AC-1 `RefreshShop_ManySeeds_NeverOffersTierAboveMaxOrRecipe` · AC-2 `Apply_BuyWhenBagAndGridFull_*`
· AC-2b `Apply_BuyToFullThenBrew_*` + fuzz (phần Bỏ qua → F-009) · AC-2c → F-007/F-009 · AC-3
`SellPrice_SameItemDifferentQualityAndMagic_SamePrice` · AC-4 fuzz + `Buy_ResultBelowOne_ReturnsOne` · AC-5
`Apply_RerollThreeTimes_*` + `RefreshShop_AfterRerolls_*` · AC-6 `RefreshShop_Default_*`, `RefreshShop_CatalystSlotsFull_*`
· AC-6b `Apply_BuyCatalystWhenSlotsFull_*` · AC-7 `Apply_BuyThenSellSameItem_LosesGold` · AC-8 → `balance`.
**Testability**: **PASS** — luật static thuần, RNG qua stream có seed, state dựng bằng `StateFactory` (+ helper
`StockShop(run, content, params string[] ids)` dựng hàng tay), không I/O. Test thống kê dùng seed cố định → tất định,
không flaky.

## 4. Các bước thực hiện

> Commit theo `CLAUDE.md` §Git: Commit tự động **không**, test-first **không** → đề xuất message, gộp test vào commit
> khi xanh. Lệnh test (Editor đóng):
> `"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml -logFile -`

- [x] **Bước 1 — Skeleton**: `MarketData`, `ItemData.MarketPrice`, tham số `market` của `ContentDb.Create` + sửa mọi
  chỗ gọi (`ContentFactory` + 5 file test `ContentDb*Test`, `GameRulesBrewMagicTest` — 28 lời gọi, dùng
  `ContentFactory.CreateMarket()`), `ShopSlot`, `ShopState`, field `RunState`, 3 Command + 3 sự kiện, 5
  `RejectReason`, `PriceFormula` / `MarketRules` (thân trả 0 / no-op / `Reject(UnknownCommand)`), dispatch, `ContentDb`
  bể rỗng. `ContentFactory`: `CreateMarket()`, giá item, + `ore_silver`, `ore_mithril`; `StateFactory.StockShop`.
  **Kiểm**: compile sạch, 312/312 cũ xanh (thêm item dịch `ItemId` — test dùng `new ItemId(n)` cứng (13 chỗ) phải vẫn
  xanh, đỏ thì đổi sang `IdOf`).
- [x] **Bước 2 — Test first**: viết test §3; chạy → test mới **fail đúng lý do**. Dự đoán pass tầm thường ở skeleton:
  `*_DoesNotDrawRandom`, `*_OnlyAdvances*Stream` (no-op không rút gì — thêm điều kiện "stream mục tiêu đổi" /
  `Accepted` để chúng fail), `RefreshShop_NoIngredientPool/NoUtilityContent_*` (ô rỗng), invariant fuzz (không lệnh
  nào được nhận) → ghi rõ trong báo cáo.
- [x] **Bước 3 — Implement**: `PriceFormula` → validate `ContentDb` + bể → `RollSeasonPrice` / `RefreshShop` →
  `Buy` / `Sell` / `Reroll` → `StateHasher` → `StartRun`. **Kiểm**: toàn bộ EditMode xanh, 0 warning CS mới,
  `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .` 0 lỗi.
- [x] **Bước 4 — Refactor**: `BagRules.ReorderCatalyst` dùng `CountCatalysts`; gộp test tham số hóa cùng phân vùng;
  rà alloc (`FillSlots` dùng lại `List`). **Kiểm**: EditMode xanh.
- [x] **Bước 5 — Tài liệu**: PLAN F-006 → Done (+ ghi chú phần chuyển F-007/F-009/F-011/F-013); TECH §2 (MarketRules,
  PriceFormula), §3.2 (`SeasonPricePercent`, `Shop`), §3.3 (`Buy(slot, cell?)`, `Sell(uid)`, `Reroll`, thứ tự lý do
  từ chối); economy.md ghi D3/D5/D6 + giá tạm; GDD Systems Index #6; F-013 ghi DTO cần `Shop` + `SeasonPricePercent`;
  CHECKPOINT. Đề xuất commit `feat(market): add shop stocking, Buy/Sell/Reroll and price formulas`.

## 5. Rủi ro & đánh đổi

- **Thêm tham số bắt buộc cho `ContentDb.Create`** → 28 chỗ gọi trong test phải sửa (cơ học, lỗi compile nên không
  lọt). Đổi lại không có giá trị mặc định ẩn cho knob kinh tế.
- Thêm 2 item vào `ContentFactory` dịch `ItemId` (`ore_mithril`, `ore_silver` xếp giữa `ore_iron` và `ore_starsteel`)
  — 13 chỗ `new ItemId(n)` trong test; kiểm ở Bước 1.
- Production chưa gọi `RefreshShop`/`RollSeasonPrice` cho tới F-009 → Chợ rỗng nếu chơi thử trước F-009. Chấp nhận
  (D1); F-009 là feature kế tiếp.
- Số lần rút `Shop` mỗi lần bày không cố định (rút loại tiện ích chỉ khi cả hai loại hợp lệ) → hợp lệ vì `Shop` chỉ
  dùng cho Chợ (GDD §6.1 cho phép reroll đổi hàng sau đó); tất định theo state.
- Test thống kê tỉ lệ tier (±3 điểm %) phụ thuộc seed cố định → nếu đổi thuật toán rút, có thể phải đổi seed — không
  phải flaky, nhưng ghi chú trong test.
- Modifier M cố định 100 (`MarketRules.NeutralPercent`) tới F-011 — F-011 thay bằng modifier Mùa, chữ ký
  `PriceFormula.Buy` đã sẵn tham số.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - Ô đã bán = `ShopSlot.None` (cùng giá trị với ô không có hàng) — luật và hash không cần phân biệt; view hiện "ô trống".
  - `PriceFormula` tính bằng `long`, kẹp kết quả ≤ `int.MaxValue`; `MarketData.MaxSeasonPricePercent = 1000` (R10) để
    giá trung gian không tràn.
  - `ContentDb` validate tổng trọng số tier / tiện ích ≤ `int.MaxValue` (tránh tràn khi `NextInt(total)`).
  - `DrawUtility`: loại có trọng số 0 coi như không có content → `CatalystWeight = 0` ⇒ luôn Kính soi; cả hai không
    hợp lệ ⇒ ô rỗng.
  - Thứ tự hash: sau `LastBrew` — `SeasonPricePercent`, `RerollCount`, số ô, từng ô (`IsAvailable`, `ItemId`).
- **Deviations**:
  - Bước 4 **không** đổi `BagRules.ReorderCatalyst` sang `CountCatalysts`: vòng lặp hiện tại vừa đếm vừa tìm vị trí
    `from` trong một lượt — tách ra thành 2 vòng không gọn hơn.
  - StateHasherTest: "item một ô" / "available một ô" viết thành 2 test so sánh hai state cùng số ô
    (`Hash_ShopSlotsDifferOnlyIn*`) thay vì case trong `Mutations` (baseline không có ô nên case sẽ chỉ đo số ô);
    `Mutations` thêm `season price`, `shop slot count`, `shop reroll count`.
  - Thêm test ngoài plan: `RefreshShop_BothUtilityKinds_OffersEachKind`, `Sell_NegativeInput_Throws`,
    `Reroll_NegativeInput_Throws` (3 tham số).
  - `StateFactory.CreateActiveRun` đặt `SeasonPricePercent = 100` (state test mặc định trung tính).
- **Tradeoffs**: `RefreshShop`/`RollSeasonPrice` chưa có caller production (F-009) — đã test trực tiếp; mỗi Command
  Chợ alloc mảng sự kiện (như luật khác — chạy theo input, không theo frame).
- **Test changes** (trước khi implement, chưa commit):
  - Sau lần chạy test-first (430: 312 cũ xanh, 116 mới fail đúng lý do, 2 pass): `Sell_BuyPriceOne_ReturnsZero` pass
    ở skeleton như dự đoán (biên, skeleton trả 0); `Sell_AnyBuyPrice_BelowBuyPrice` yếu → đổi thành
    `Sell_AnyBuyPrice_PositiveAndBelowBuyPrice` (buy 2…500, kết quả ∈ [1, buy − 1]). Không chạy lại riêng để xác nhận
    fail (suy luận: skeleton trả 0 < 1).
  - 2 test fail ở skeleton bằng exception (`Sequence contains no elements` — `.Single()` trên danh sách sự kiện rỗng)
    thay vì assertion: `Apply_BuyIngredient_EmitsItemBought`, `Apply_Reroll_EmitsShopRerolledWithPrice` — chấp nhận.
  - Sau implement, `MarketInvariantTest.Apply_RandomMarketAndGridCommands_KeepsInvariants` fail ở điều kiện độ phủ
    (chỉ 45–79/2000 lệnh được nhận — vàng cạn); bất biến vẫn đúng. Sửa harness: mỗi lượt giả +20 vàng (`TurnGold`, giả
    vàng thưởng F-011) → xanh. Không sửa luật.
- **Open questions**:
  - Tier 4+ có recipe dùng `MarketPrice` riêng để bán — số chốt ở `balance` (phải > tổng giá nguyên liệu, economy.md
    Edge Cases).
