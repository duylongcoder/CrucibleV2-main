# Plan: Vòng lượt — Chiếu chỉ → Bàn luyện ×3 → Dâng (F-009, phần 1 "run-loop")

> **Trạng thái**: Done (D1–D6 duyệt 2026-11-04; 486/486 EditMode xanh) · **Cập nhật**: 2026-11-04
> **GDD**: `design/run-flow.md` §Chiếu chỉ, §Bàn luyện, §Trạng thái & chuyển trạng thái (hàng RunStart → Offering),
> Edge Cases "đóng tab… Decree đã rút thì không rút lại"; `design/brewing.md` §Trạng thái một lượt Bàn luyện, AC-10;
> `design/economy.md` §Chợ (hàng làm mới đầu lượt), AC-2b (phần Bỏ qua), AC-2c; GDD §6.1 AC-2/AC-3 ·
> **ADR**: ADR-001 (Command/Reducer), ADR-004 (RNG stream) · **Loại**: Logic (Core)
> Xây trên F-000…F-006 (430/430 EditMode xanh).

## 1. Bối cảnh

F-006 đã có `MarketRules.RollSeasonPrice` / `RefreshShop` nhưng production chưa gọi → Chợ rỗng, S luôn 100; `Brew`
khóa lưới ở `ResultPending` mà không có lệnh nào mở lại; không có khái niệm *lượt*. Hai fuzz (`MarketInvariantTest`,
`BagCapacityInvariantTest`) đang giả lượt mới bằng tay. Plan này nối **khung vòng lượt** trong một Mùa: vào Chiếu
chỉ (rút S) → `AcceptDecree` → 3 lượt Bàn luyện (mỗi lượt làm mới Chợ; kết thúc bằng `AckBrewResult` sau Nung hoặc
`SkipTurn`) → vào **Dâng**. Phần Dâng → Rèn & Trận → Chiến lợi phẩm → Mùa kế / RunEnd thuộc F-010/F-011/F-017
(F-009 phần 2 nối lại sau). Pillar P3 (nhịp 3 lượt/Mùa là áp lực tài nguyên), P2 (tất định).

**Yêu cầu** (đánh số để truy vết):
- R1 — **Vào Chiếu chỉ** (`SeasonRules.EnterDecree`): đặt `Phase = Decree`, `Turn = 0`, rồi rút hệ số giá Mùa
  (`MarketRules.RollSeasonPrice`, stream `Market`, đúng 1 lần). Rút **lúc vào** Decree, không lúc nhận chiếu →
  Decree đã rút thì đọc lại save không rút lại. `StartRun` gọi `EnterDecree` cho Mùa 1 (thay "S = 100" của F-006 R2).
  Điểm nối cho F-010 (rút loại trang bị, stream `Forge`) và F-011 (modifier Mùa sau, stream `Season`) ghi trong XML doc.
- R2 — `AcceptDecree`: chỉ ở `Decree` → `Phase = Workbench`, bắt đầu lượt 1 (R3). Phát `DecreeAccepted(season)` +
  `TurnStarted(season, 1)`. Từ chối: chưa có run (`RunNotActive`), sai phase (`WrongPhase`).
- R3 — **Bắt đầu lượt k** (private `StartTurn`): `Turn = k`, `WorkbenchStep = Editing`, `MarketRules.RefreshShop`
  (stream `Shop`; F-007 kích `OnShopRefresh` ngay sau). Lưới luôn trống khi bắt đầu lượt (Nung đã tiêu hao; Bỏ qua đã
  trả về Túi). Không rút stream nào khác.
- R4 — `SkipTurn`: chỉ ở Workbench·Editing (`ValidateEditable`: `RunNotActive` / `WrongPhase` / `GridLocked`). Luôn
  hợp lệ ở Editing kể cả lưới trống (brewing AC-10). Vật phẩm trên lưới về **cuối Túi theo thứ tự ô 0→8**, lưới trống;
  rồi kết thúc lượt (R6). Phát `TurnSkipped(season, turn)` (+ `GridChanged` nếu lưới có vật phẩm) + sự kiện của R6.
  Không rút RNG ngoài R3/R1 của bước kế.
- R5 — `AckBrewResult`: chỉ ở Workbench·`ResultPending` (sai phase → `WrongPhase`; Editing → `NoPendingResult` mới)
  → kết thúc lượt (R6). Phát sự kiện của R6. `LastBrew` / `LastBrewResultUid` **giữ nguyên** (D4).
- R6 — **Kết thúc lượt** (private `EndTurn`): `Turn < RunData.TurnsPerSeason` → bắt đầu lượt `Turn + 1` (R3);
  ngược lại → `Phase = Offering`, `Turn = 0`, `WorkbenchStep = Editing`, xóa hàng Chợ (`Slots` rỗng, `RerollCount = 0`)
  — D3; phát `OfferingStarted(season)`. Lượt Bỏ qua vẫn tính (run-flow §Bàn luyện).
- R7 — `RunState.Turn`: 1…`TurnsPerSeason` khi `Phase == Workbench`, 0 ở mọi phase khác. `StartRun` đặt 0.
- R8 — `RunData.TurnsPerSeason` (tham số ctor tùy chọn cuối, mặc định 3); `ContentDb.Create` ném `ArgumentException`
  khi `TurnsPerSeason < 1`.
- R9 — Bất biến qua mọi chuỗi Command hợp lệ/không hợp lệ: Command bị từ chối không đổi state hash; `Turn` đúng R7;
  Túi + lưới ≤ sức chứa (AC-2b cả nhánh Bỏ qua); mỗi Mùa đúng `TurnsPerSeason` lần `TurnStarted` = số lần
  `RefreshShop` (AC-2c phần đếm — trigger thật ở F-007) bất kể số reroll; mọi run tới Offering Mùa 1.
- R10 — Tất định: cùng seed + cùng chuỗi lệnh → cùng hash sau mỗi lệnh; cùng seed → cùng S Mùa 1 và cùng hàng Chợ
  lượt 1 (GDD §6.1 AC-2, phần Chợ/giá); thêm reroll không đổi kết quả `Magic` của cùng lượt nung (GDD §6.1 AC-3, phần
  ma thuật).
- R11 — `StateHasher` ghi `Turn` (sau `WorkbenchStep`).

**Ngoài phạm vi**:
- Rút loại trang bị ở Decree (stream `Forge`), `Decree` state, GDD §6.1 AC-2 phần "loại trang bị" → **F-010**.
- Modifier Mùa sau (stream `Season`) + hiển thị → **F-011**.
- Dâng (`PutOnTray`/`ClearTray`/`Offer`/`DismissCourtier`, tự Thua khi không có gì dâng), Trận, `AckBattle`, Uy tín,
  Spoils, chuyển Mùa + RunEnd → **F-010/F-011/F-017**, rồi **F-009 phần 2** (nối Resolution → Decree Mùa + 1 /
  RunEnd; AC-1 RandomAgent 1.000 run trọn).
- `OnShopRefresh` + Effect → **F-007** (gọi trong `StartTurn` sau `RefreshShop`).
- Kính soi ở Offering → **F-008**. Popup Chiếu chỉ, nút Tiếp tục / Bỏ qua lượt (+ xác nhận khi lưới có đồ) → **F-014**.
  DTO `Turn` → **F-013**. SO `RunDef.TurnsPerSeason` + `ContentBaker` (Content chưa có code).

**TBD**: không (số lượt/Mùa = 3 đã chốt ở run-flow Tuning Knobs; khoảng an toàn 2–4 chỉ là tuning).

**Quyết định (đề xuất, chờ duyệt)**:
- D1: Rút ngẫu nhiên của Chiếu chỉ ở **lúc vào Decree** (`StartRun` / F-009 phần 2), không ở `AcceptDecree` — vì
  popup cần hiện loại trang bị (F-010) trước khi nhận, và "đã rút thì không rút lại". Hệ quả: sau `StartRun` S ∈
  [70, 130] (không còn 100); sửa test F-006 `Apply_StartRun_SetsNeutralSeasonPriceAndEmptyShop`.
- D2: Logic vòng lượt đặt trong class mới `SeasonRules` (TECH §3.3 đã liệt kê tên này) — không nhét vào
  `BrewingRules`/`MarketRules`. `EnterDecree` là `public static` để `RunStartRules` và F-009 phần 2 gọi.
- D3: Vào Offering → **xóa hàng Chợ** (Dâng không mua bán; tránh hàng cũ lưu trong save/hash). S giữ nguyên tới
  Decree kế (bán ở Dâng bị cấm nên không ảnh hưởng).
- D4: `AckBrewResult` **không** xóa `LastBrew`/`LastBrewResultUid` (view chỉ phát lại khi `ResultPending`; giữ để
  màn Dâng có thể nhắc "mẻ vừa rồi" — F-014). Lần Nung kế ghi đè.
- D5: `SkipTurn` khi `ResultPending` → từ chối `GridLocked` (theo bảng trạng thái brewing: Bỏ qua chỉ từ Editing;
  sau Nung phải "Tiếp tục"). `AckBrewResult` khi Editing → `RejectReason.NoPendingResult` (mới, = 20).
- D6: Không đổi hai fuzz F-005/F-006 (vẫn giả lượt bằng tay để giữ phạm vi lệnh Chợ/lưới dày); thêm fuzz mới
  `RunLoopInvariantTest` chạy vòng thật.

## 2. Thiết kế triển khai

| Class / file (dưới `Assets/_Project/Scripts/`) | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Commands/AcceptDecree.cs` | Command rỗng | Core | Mới |
| `Core/Commands/SkipTurn.cs` | Command rỗng | Core | Mới |
| `Core/Commands/AckBrewResult.cs` | Command rỗng | Core | Mới |
| `Core/Commands/DecreeAccepted.cs` | sự kiện `(int Season)` | Core | Mới |
| `Core/Commands/TurnStarted.cs` | sự kiện `(int Season, int Turn)` — view đọc lại hàng Chợ | Core | Mới |
| `Core/Commands/TurnSkipped.cs` | sự kiện `(int Season, int Turn)` | Core | Mới |
| `Core/Commands/OfferingStarted.cs` | sự kiện `(int Season)` | Core | Mới |
| `Core/Commands/RejectReason.cs` | + `NoPendingResult = 20` | Core | Sửa |
| `Core/Content/RunData.cs` | + `int TurnsPerSeason` (ctor `turnsPerSeason = 3`, cuối) | Core | Sửa |
| `Core/Content/ContentDb.cs` | validate R8 (cạnh `ValidateBag`) | Core | Sửa |
| `Core/State/RunState.cs` | + `int Turn` | Core | Sửa |
| `Core/State/StateHasher.cs` | ghi `Turn` sau `WorkbenchStep` (R11) | Core | Sửa |
| `Core/Rules/SeasonRules.cs` | `EnterDecree`, `AcceptDecree`, `SkipTurn`, `AckBrewResult`; private `StartTurn`, `EndTurn` | Core | Mới |
| `Core/Rules/RunStartRules.cs` | `Turn = 0`; thay `SeasonPricePercent = Neutral` bằng `SeasonRules.EnterDecree` (cuối mutate, sau `Rng`) | Core | Sửa |
| `Core/Rules/GameRules.cs` | dispatch 3 Command mới | Core | Sửa |
| `Core/Rules/MarketRules.cs` | sửa XML doc (người gọi = `SeasonRules`) | Core | Sửa |

**Public API**
```csharp
namespace Crucible.Core.Rules {
    public static class SeasonRules {
        /// Vào Chiếu chỉ của run.Season: Phase = Decree, Turn = 0, rút S (Market). F-010: + loại trang bị (Forge);
        /// F-011: + modifier Mùa sau (Season). Không validate — người gọi đảm bảo run đang active.
        public static void EnterDecree(RunState run, ContentDb content);
        public static CommandResult AcceptDecree(RunState run, ContentDb content, AcceptDecree command);
        public static CommandResult SkipTurn(RunState run, ContentDb content, SkipTurn command);
        public static CommandResult AckBrewResult(RunState run, ContentDb content, AckBrewResult command);
    }
}
// RunData(int startingReputation, int startingBagCapacity, int maxBagCapacity, int seasonCount, int turnsPerSeason = 3)
```

**Luồng** (validate trước, mutate sau; sự kiện gom vào `List<GameEvent>` cục bộ — Command không ở vòng nóng):
```
StartRun ── mutate nền ──▶ EnterDecree ─▶ [Decree] ──AcceptDecree──▶ StartTurn(1) ─▶ [Workbench·Editing, Turn 1]
[Editing] ──Brew──▶ [ResultPending] ──AckBrewResult──▶ EndTurn
[Editing] ──SkipTurn (lưới → Túi)──────────────────▶ EndTurn
EndTurn: Turn < N ? StartTurn(Turn+1) : [Offering] (Turn 0, Chợ xóa)
```

**Interface phụ thuộc**: không (luật static; RNG qua `run.Rng`).
**Dữ liệu**: `RunData.TurnsPerSeason` (≥ 1; mặc định 3; an toàn 2–4). SO chưa có (Content chưa có code).
**Scene / prefab bị chạm**: không. `find_guid_refs` không cần (chỉ script Core, không MonoBehaviour).
**Lý do thiết kế**: một class luật cho máy trạng thái trong Mùa, tách khỏi luật từng hệ thống; `EndTurn` là điểm duy
nhất quyết định lượt kế / Dâng → F-009 phần 2 chỉ thêm `EnterDecree` sau Resolution. Sự kiện cụ thể (`TurnStarted`,
`OfferingStarted`) thay cho một `PhaseChanged` chung để view không phải so state cũ/mới.

## 3. Test cases

Mẫu phong cách: `GameRulesBuyTest`, `GameRulesRerollTest`, `MarketInvariantTest`. Dựng state bằng `StateFactory`
(+ helper mới `StateFactory.CreateDecreeRun(content)`: run active ở Decree, Turn 0, Túi bộ khởi đầu). Đối tượng test
qua seam production `GameRules.Apply` (trừ `EnterDecree` gọi trực tiếp — public, F-009 phần 2 sẽ gọi).

### Editor tests
(none)

### Unit tests

#### GameRulesAcceptDecreeTest (`Assets/Tests/EditMode/Core/Rules/GameRulesAcceptDecreeTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Apply_AcceptDecreeInDecree_EntersWorkbenchTurnOne` | Accepted; `Phase` Workbench; `Turn` 1; `WorkbenchStep` Editing |
| `Apply_AcceptDecree_StocksShop` | 5 ô, cả 5 `IsAvailable`; `RerollCount` 0 |
| `Apply_AcceptDecree_EmitsDecreeAcceptedThenTurnStarted` | đúng 2 sự kiện theo thứ tự; `Season` 1, `Turn` 1 |
| `Apply_AcceptDecree_OnlyAdvancesShopStream` | 6 stream khác không đổi; `Shop` đổi |
| `Apply_AcceptDecree_KeepsSeasonPrice` | `SeasonPricePercent` bằng trước lệnh |
| `Apply_AcceptDecreeWithoutRun_RejectsRunNotActive` | Rejected `RunNotActive`; hash không đổi |
| `Apply_AcceptDecreeOutsideDecree_RejectsWrongPhase` (phase: Workbench, Offering, Battle, Spoils, Ended) | Rejected `WrongPhase`; hash không đổi |

#### SeasonRulesEnterDecreeTest (`.../Core/Rules/SeasonRulesEnterDecreeTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `EnterDecree_FromOffering_SetsDecreeAndTurnZero` | `Phase` Decree; `Turn` 0 |
| `EnterDecree_Default_RollsSeasonPriceInRange` (500 seed) | 70 ≤ S ≤ 130 |
| `EnterDecree_OnlyAdvancesMarketStream` | 6 stream khác không đổi; `Market` đổi |
| `EnterDecree_SameSeed_SameSeasonPrice` | hai run cùng seed → cùng S |

#### GameRulesSkipTurnTest (`.../Core/Rules/GameRulesSkipTurnTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Apply_SkipTurnWithEmptyGrid_Accepted` (brewing AC-10) | Accepted; `Turn` 2 |
| `Apply_SkipTurnWithGridItems_ReturnsItemsToBagEndInCellOrder` (ô 0, 4, 8) | lưới trống; 3 uid cuối Túi theo thứ tự ô 0→4→8; Túi cũ giữ thứ tự |
| `Apply_SkipTurnWithGridItems_EmitsGridChanged` | có `GridChanged` + `TurnSkipped(1, 1)` + `TurnStarted(1, 2)` |
| `Apply_SkipTurnWithEmptyGrid_EmitsNoGridChanged` | không có `GridChanged` |
| `Apply_SkipTurnWithGridItems_KeepsUsedSlots` (AC-2b nhánh Bỏ qua; Túi + lưới = sức chứa) | `UsedSlots` không đổi, ≤ `BagCapacity` |
| `Apply_SkipTurnBeforeLastTurn_RefreshesShopAndResetsReroll` (đã reroll 2 lần) | `RerollCount` 0; hàng = hàng `RefreshShop` kỳ vọng từ cùng trạng thái stream |
| `Apply_SkipTurnOnLastTurn_EntersOffering` | `Phase` Offering; `Turn` 0; `OfferingStarted(1)`; không `TurnStarted` |
| `Apply_SkipTurnOnLastTurn_ClearsShop` (D3) | `Slots` rỗng; `RerollCount` 0 |
| `Apply_SkipTurnOnLastTurn_DoesNotAdvanceAnyStream` | 7 stream không đổi |
| `Apply_SkipTurnWhenResultPending_RejectsGridLocked` (D5) | Rejected `GridLocked`; hash không đổi |
| `Apply_SkipTurnOutsideWorkbench_RejectsWrongPhase` (phase: Decree, Offering) | Rejected `WrongPhase`; hash không đổi |
| `Apply_SkipTurnWithoutRun_RejectsRunNotActive` | Rejected `RunNotActive` |

#### GameRulesAckBrewResultTest (`.../Core/Rules/GameRulesAckBrewResultTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Apply_AckAfterBrewOnTurnOne_StartsTurnTwoEditing` | Accepted; `Turn` 2; `WorkbenchStep` Editing |
| `Apply_AckAfterBrew_ReopensGridCommands` | `PlaceItem` sau Ack được Accepted |
| `Apply_AckAfterBrew_KeepsBrewResultInBag` | uid `LastBrewResultUid` vẫn ở Túi |
| `Apply_AckAfterBrew_KeepsLastBrew` (D4) | `LastBrew` cùng tham chiếu; `LastBrewResultUid` không đổi |
| `Apply_AckOnLastTurn_EntersOffering` | `Phase` Offering; `Turn` 0; sự kiện `OfferingStarted(1)` |
| `Apply_AckWhileEditing_RejectsNoPendingResult` | Rejected `NoPendingResult`; hash không đổi |
| `Apply_AckOutsideWorkbench_RejectsWrongPhase` (phase: Decree, Offering) | Rejected `WrongPhase` |
| `Apply_AckWithoutRun_RejectsRunNotActive` | Rejected `RunNotActive` |

#### GameRulesStartRunTest (sửa)
| Test method | Kiểm tra |
|---|---|
| `Apply_StartRun_SetsNeutralSeasonPriceAndEmptyShop` → **đổi thành** `Apply_StartRun_RollsSeasonPriceAndLeavesShopEmpty` (D1) | S ∈ [70, 130] và = S của `EnterDecree` trên run cùng seed; `Slots` rỗng; `RerollCount` 0 |
| `Apply_ValidStartRun_EntersSeasonOneDecree` (+ 1 assert) | `Turn` 0 |

#### ContentDbTest (sửa — `.../Core/Content/ContentDbTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Create_TurnsPerSeasonBelowOne_Throws` (0, −1) | `ArgumentException` |
| `Create_TurnsPerSeasonOne_Succeeds` | `Run.TurnsPerSeason` = 1 |

#### StateHasherTest (sửa)
| Test method | Kiểm tra |
|---|---|
| `Hash_StatesDifferInOneField_ReturnsDifferentValue` (+ case `"Turn"`) | hash khác |

### Integration tests (Core, nhiều Command qua `GameRules.Apply`)

#### RunLoopTest (`.../Core/Rules/RunLoopTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Apply_StartRunThenFullSeasonOfSkips_ReachesOfferingAfterConfiguredTurns` (`TurnsPerSeason`: 1, 3, 4) | Offering sau đúng N `SkipTurn`; mọi lượt trước đó `Turn` = 1…N |
| `Apply_MixedBrewAndSkip_ThreeTurnStartedPerSeason` (Nung–Bỏ qua–Nung + reroll 0/2/5 lần) | đúng 3 `TurnStarted`, `Turn` 1, 2, 3 (AC-2c phần đếm) |
| `Apply_SameSeed_SameSeasonPriceAndTurnOneShop` (GDD §6.1 AC-2) | hai run cùng seed: cùng S, cùng dãy `ItemId` 5 ô lượt 1 |
| `Apply_ExtraRerollBeforeBrew_SameMagicResult` (GDD §6.1 AC-3, phần ma thuật; 200 seed; content `MagicChance` 50%) | `Magic` thành phẩm run có reroll = run không reroll |
| `Apply_ExtraRerollInTurnOne_SameSeasonPrice` | S không đổi (stream `Market` độc lập) |

#### RunLoopInvariantTest (`.../Core/Rules/RunLoopInvariantTest.cs`) — fuzz
| Test method | Kiểm tra |
|---|---|
| `Apply_RandomCommandsFromStartRun_KeepsLoopInvariants` (seed 1–5 × tới khi Offering hoặc 3.000 lệnh; lệnh ngẫu nhiên gồm `AcceptDecree`/`SkipTurn`/`AckBrewResult`/`Brew`/`Buy`/`Sell`/`Reroll`/`PlaceItem`/`RemoveFromGrid`, tham số có giá trị sai) | sau mỗi lệnh: bị từ chối → hash không đổi; `Turn` đúng R7; `UsedSlots ≤ BagCapacity`; vàng ≥ 0; số `TurnStarted` ≤ N |
| `Apply_RandomCommandsFromStartRun_AlwaysReachesOffering` (100 seed; lệnh vòng chiếm ≥ 1/4) | mọi seed tới Offering trong 3.000 lệnh; tổng `TurnStarted` = N |
| `Apply_SameSeedSameCommands_SameHashEveryStep` (seed 7, 500 lệnh) | hash hai run bằng nhau **sau mỗi lệnh** (GDD §6.1 AC-1, Editor) |

### Visual verification tests
(none — chưa có view; F-014)

### Manual tests
(none)

**Truy vết**: R1 → `SeasonRulesEnterDecreeTest.*`, `Apply_StartRun_RollsSeasonPriceAndLeavesShopEmpty` · R2 →
`GameRulesAcceptDecreeTest.*` · R3 → `Apply_AcceptDecree_StocksShop`, `Apply_AcceptDecree_OnlyAdvancesShopStream`,
`Apply_SkipTurnBeforeLastTurn_RefreshesShopAndResetsReroll` · R4 → `GameRulesSkipTurnTest.*` · R5 →
`GameRulesAckBrewResultTest.*` · R6 → `Apply_SkipTurnOnLastTurn_*`, `Apply_AckOnLastTurn_EntersOffering`,
`Apply_StartRunThenFullSeasonOfSkips_*` · R7 → `Apply_ValidStartRun_EntersSeasonOneDecree`, `EnterDecree_FromOffering_*`,
fuzz · R8 → `Create_TurnsPerSeason*` · R9 → `RunLoopInvariantTest.*`, `Apply_MixedBrewAndSkip_*`,
`Apply_SkipTurnWithGridItems_KeepsUsedSlots` · R10 → `Apply_SameSeed_*`, `Apply_ExtraReroll*`,
`Apply_SameSeedSameCommands_SameHashEveryStep` · R11 → `StateHasherTest` case `Turn`.
**AC**: brewing AC-10 (phần Bỏ qua luôn hợp lệ) · economy AC-2b (nhánh Bỏ qua) · economy AC-2c (đếm lượt; trigger →
F-007) · GDD §6.1 AC-2 (S + hàng lượt 1; loại trang bị → F-010), AC-3 (ma thuật; trang bị/chiến lợi phẩm → F-010/F-011)
· run-flow AC-1/AC-3 → F-009 phần 2.
**Testability**: **PASS** — luật static thuần, state dựng trực tiếp, RNG có seed; mỗi chuyển trạng thái có một
Command quan sát được qua sự kiện + state.

## 4. Các bước thực hiện

> `CLAUDE.md` §Git: Commit tự động **không**, test-first **không** → chỉ đề xuất message; gộp test vào commit xanh.
> Lệnh test (Editor đóng): `"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode
> -testResults Logs/editmode.xml -logFile -` (Linux: `UNITY="$HOME/Unity/Hub/Editor/6000.6.7f1/Editor/Unity"`).

- [x] **Bước 1 — Skeleton**: 3 Command, 4 sự kiện, `NoPendingResult`, `RunData.TurnsPerSeason`, `RunState.Turn`,
  `SeasonRules` (thân trả `Reject(UnknownCommand)` / rỗng), dispatch trong `GameRules`, `StateFactory.CreateDecreeRun`.
  Kiểm chứng: compile sạch; **430/430** xanh (chưa đổi hành vi).
- [x] **Bước 2 — Test first**: viết test §3; sửa 3 test cũ (StartRun ×2, StateHasher case). Kiểm chứng: test mới fail
  đúng lý do (`UnknownCommand`, `Turn` sai, S = 100…); ghi số pass/fail dự đoán trước (các test `Reject*` có thể
  pass sớm nếu skeleton trả đúng — sửa skeleton trả `UnknownCommand` để chúng fail).
- [x] **Bước 3 — Implement**: thứ tự `RunData` + validate → `StateHasher` → `EnterDecree` + `StartRun` → `AcceptDecree`
  + `StartTurn` → `EndTurn` → `SkipTurn` → `AckBrewResult`. Kiểm chứng: toàn bộ EditMode xanh; 0 warning CS mới;
  `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .` 0 lỗi.
- [x] **Bước 4 — Refactor**: gộp test `Reject*` tham số hóa theo phase; dùng chung helper "chạy tới lượt k" trong
  `RunLoopTest`/fuzz. Test vẫn xanh.
- [x] **Bước 5 — Tài liệu**: PLAN F-009 → **In Progress** (phần 1 xong; phần 2 chờ F-010/F-011/F-017) + ghi chú;
  TECH §3 (State + `Turn`, Command & luật + `SeasonRules`, §3.3 bỏ "F-009 gọi"); F-006 R2 ghi "thay bởi F-009 D1";
  F-013 backlog + `Turn`; brewing/economy/run-flow tick AC đã đạt; LOG + CHECKPOINT.
  Commit đề xuất: `feat(run-loop): add decree→workbench×3→offering turn loop (AcceptDecree, SkipTurn, AckBrewResult)`.

## 5. Rủi ro & đánh đổi

- **D1 đổi hành vi `StartRun`** (S không còn 100): chỉ 1 test F-006 dựa vào; save chưa có (F-013) nên không cần migrate.
- **Run dừng ở Offering**: cho tới F-010, sau lượt 3 không còn Command nào hợp lệ ngoài `SetTimescale` → chơi thử
  greybox kẹt ở Dâng. Chấp nhận (Core-only; F-014 chưa có view).
- **Fuzz "luôn tới Offering"** phụ thuộc tỉ lệ lệnh vòng trong bộ sinh lệnh — đặt cố định ≥ 1/4 để không flaky; nếu fail
  là lỗi luật (vd `AckBrewResult` không mở khóa), không tăng số lệnh.
- `OnShopRefresh` (F-007) phải chèn **trong** `StartTurn` ngay sau `RefreshShop`, trước khi phát `TurnStarted` — ghi
  XML doc để F-007 không gọi ở `Reroll`/`AcceptDecree` riêng.
- Thứ tự stream trong `EnterDecree` (Market, rồi F-010 Forge, F-011 Season) không ảnh hưởng tất định vì stream độc
  lập; vẫn cố định thứ tự trong code để hash dễ đọc.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - `StateFactory.CreateWorkbenchRun` đặt `Turn = 1` (run Bàn luyện hợp lệ theo R7); thêm `ContentFactory.TurnsPerSeason`
    + tham số `Create(turnsPerSeason)`.
  - `EnterDecree` đặt cả `Phase` + `Turn` → `RunStartRules` bỏ dòng `Phase = Decree` / `Turn = 0` riêng (Refactor).
  - Sự kiện gom vào `List<GameEvent>` cục bộ (Command không ở vòng nóng).
- **Deviations**:
  - Bỏ `Apply_ExtraRerollInTurnOne_SameSeasonPrice`: S rút ở Decree trước mọi reroll → luôn bằng nhau, test không kiểm
    được gì cho tới khi có Mùa 2 (F-009 phần 2 sẽ thêm "reroll Mùa 1 không đổi S Mùa 2").
  - `Apply_AckOutsideWorkbench_*` không có case ResultPending ở phase khác — không tới được qua luật.
- **Tradeoffs**: hai fuzz F-005/F-006 giữ cơ chế giả lượt (D6) — vẫn đúng vì gọi trực tiếp `RefreshShop` / đặt `Editing`.
- **Test changes** (sau Bước 2): `Apply_SkipTurnWithGridItems_EmitsGridChanged` đưa assert `Count` lên đầu (skeleton
  fail bằng exception thay vì assertion); thêm assert `Accepted` cho `Apply_SkipTurnWithGridItems_KeepsUsedSlots`,
  `Apply_AcceptDecree_KeepsSeasonPrice`, `Apply_AckAfterBrew_KeepsBrewResultInBag/KeepsLastBrew` (pass sai ở skeleton).
- **Open questions**: - None
