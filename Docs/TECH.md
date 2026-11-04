# Technical Design — CrucibleV2 (tên tạm)

> Chủ sở hữu: Developer · **Cập nhật**: 2026-11-04
> Kiến trúc tổng, quy ước code, các quyết định kỹ thuật (ADR), ngân sách hiệu năng.

## 1. Tổng quan kiến trúc

- **Mẫu chính**: **lõi mô phỏng C# thuần + Command/Reducer** (ADR-001). Unity chỉ lo hiển thị + input.
- **Luồng khởi động**: scene **Boot** (nạp content, đọc save, tạo `GameSession`) → scene **Game** (ADR-002).
- **Quản lý scene**: một scene Game chứa root Menu / Xưởng / Lò rèn / Nơi giao đấu + các Canvas; `ScreenRouter` bật
  root theo phase của state. Không load scene giữa các màn.
- **Giao tiếp**: Presentation gửi `Command` cho `GameSession`; nghe C# event (`Action<T>`) + đọc `RunView`. UI không giữ
  state game, không tự tính luật.
- **Dữ liệu**: SO `*Def` ở `Assets/_Project/Data` → `ContentDb` bất biến lúc Boot (ADR-005).
- **Lưu game**: snapshot JSON (Newtonsoft) sau mỗi Command được nhận — §4, ADR-003.

```
Presentation  (Crucible.Runtime: view MonoBehaviour, uGUI, phát lại BrewLog/BattleLog, audio, input adapter)
     │ Command                             ▲ C# event + RunView (đã lọc thông tin ẩn — ADR-007)
Application   (GameSession — C# thuần, do GameHost MonoBehaviour giữ: áp Command → lưu → phát event)
     │                                     ▲
Core          (Crucible.Core, noEngineReferences: RunState, MetaState, Command, Rules (Brewing, Market,
               Inspect, Season, Battle), RngStreams, ContentDb, BrewLog/BattleLog, StateHash)
Platform      (SaveService — Newtonsoft JSON; Input System; ContentBaker SO → ContentDb)
```

**Luồng Command**: `GameSession.Execute(cmd)` → Core **validate trước, mutate sau** → `CommandResult { Accepted |
Rejected(reason), Events[], Logs }`. Rejected → state không đổi, **không tiêu thụ RNG** (GDD §6.1). Accepted → lưu
ngay → phát event.

**Tất định** (ADR-004): Core chỉ số nguyên (`long` cho trung gian), không float/double; PRNG tự viết **PCG32**, 7
stream (`Shop`, `Chance`, `Inspect`, `Forge`, `Market`, `Season`, `Reward`) seed bằng SplitMix64(seed, streamId);
trạng thái stream nằm trong save. Không `System.Random` / `UnityEngine.Random`. `StateHash` (FNV-1a 64 trên dạng
chuẩn hóa của state) cho test GDD §6.1 AC-1.

**Chống rò rỉ** (ADR-007): Presentation chỉ nhận `RunView` do Core dựng — **không** chứa dòng thuộc tính chưa lộ,
`Pattern` / khoảng cách công thức. Ngoại lệ có chủ đích (biểu cảm thợ rèn, thuộc tính lộ trong Trận) đi qua log
riêng sau khi dâng.

**Presentation**: phát lại chỉ đọc `BrewLog` / `BattleLog`; timescale thuần hiển thị; chuỗi animation `async
Awaitable` + `destroyCancellationToken`; event đăng ký `OnEnable` ↔ hủy `OnDisable`.

**Đồ họa**: ánh sáng = sprite overlay additive/multiply (ADR-006, đóng ART A4). Trang bị đổi màu: shader Shader
Graph URP 2D map 4 chỉ số xám → 4 màu `EquipmentRamp`, truyền bằng `MaterialPropertyBlock` (đóng ART A6).

## 2. Assembly definitions

| Assembly | Đường dẫn | Tham chiếu | Nền tảng | Ghi chú |
|---|---|---|---|---|
| Unity.2D.Welcome | Assets/Welcome | — | Any | **Hiện có** — của template, không phải code game |
| Crucible.Core | Assets/_Project/Scripts/Core | — | Any | `noEngineReferences: true`; luật, state, RNG, log — **đã tạo 2026-11-04** |
| Crucible.Content | Assets/_Project/Scripts/Content | Core | Any | SO `*Def` + `ContentBaker` |
| Crucible.Runtime | Assets/_Project/Scripts/Runtime | Core, Content, Unity.InputSystem, Newtonsoft.Json | Any | GameHost, GameSession, SaveService, view, UI |
| Crucible.Editor | Assets/_Project/Scripts/Editor | Content, Runtime | Editor | validator content, tool |
| Crucible.Tests.EditMode | Assets/Tests/EditMode | Core, Content, Runtime | Editor | luật, công thức, tất định, state hash, round-trip DTO, `GameSession` |
| Crucible.Tests.PlayMode | Assets/Tests/PlayMode | Runtime | Any | tích hợp màn, layout UI |

## 3. Hệ thống kỹ thuật

| Hệ thống | Class chính | Assembly | Phụ thuộc | GDD | Trạng thái |
|---|---|---|---|---|---|
| Content trong Core | `ContentDb`, `*Data`, `ItemId` | Core | — | ADR-005 | Implemented (Item/Difficulty/StarterKit/Run + `RecipeData`→`Recipe` có validate, `BrewingData` (+ 3 bảng ma thuật theo tier), `ItemData.MagicChance`, `ItemKind.Scrap`) |
| State | `RunState`, `MetaState`, `ItemInstance` | Core | ContentDb | GDD §6, design/* | Implemented (nền + `Grid[9]`, `WorkbenchStep`, `LastBrewResultUid`; `ItemInstance.Magic/Quality` (long), `Lines` + `RevealedMask` (F-008)) |
| Command & luật | `GameRules`, `ICommand`, `CommandResult`, `*Rules`, `RecipeMatcher`, `BattleSimulator` | Core | State, RNG | brewing, economy, run-flow, battle | In Progress (`StartRun`, `PlaceItem/RemoveFromGrid/MoveCell` → `GridRules`, `Brew` → `BrewingRules` + `RecipeMatcher` + `QualityFormula` + `MagicChanceFormula`, `SetTimescale` → `SettingsRules`, `ReorderCatalyst` + chỗ chung Túi/lưới → `BagRules`, Effect Xúc tác → `CatalystRunner`, `Buy`/`Sell`/`Reroll` + bày hàng → `MarketRules` + `PriceFormula`, `AcceptDecree`/`SkipTurn`/`AckBrewResult`/`AckBattle` → `SeasonRules`, `ChooseReward`/`SkipReward` → `SpoilsRules`, rút trang bị → `ForgeRules`, `Offer`/`DismissCourtier` → `OfferingRules` + `BattleStats` + `BattleSimulator` + `BattleMath`) |
| RNG | `Pcg32`, `RngStreams` | Core | — | GDD §6.1 | Implemented |
| Log / View / Hash | `BrewLog`, `BattleLog`, `RunViewBuilder`, `StateHasher` | Core | State | GDD §7, ADR-007 | In Progress (`StateHasher`, `BrewLog` + `RunState.LastBrew`, `BattleLog` + `RunState.LastBattle`) |
| Phát lại (Presentation) | `BrewReplayTimeline` (C# thuần) | Runtime | Core | brewing.md §Phát lại | Implemented (đồng hồ; view ở F-014) |
| Phiên chơi | `GameSession`, `GameHost`, `ISaveStore`, `FileSaveStore`, `SaveMapper`, `IClock` | Runtime | Core | GDD §6.4 | Designed |

### 3.1 Content trong Core — `ContentDb`
- **Trách nhiệm**: dữ liệu content bất biến cho luật.
- **Dữ liệu**: record `ItemData`, `RecipeData`, `EquipmentData`, `OpponentData`, `DifficultyData`, `RunData`,
  `RewardData`, `ModifierData`, `StarterKitData`, `EffectData` (do `ContentBaker` ở Content tạo từ SO).
  **Effect** (F-007): `ItemData.Effects` (chỉ Xúc tác, 0…3) = `EffectData(Trigger, Action, Conditions ≤ 3 khác loại, AND)` →
  `ContentDb.EffectsOf(ItemId) → CatalystEffect` (điều kiện lưu phẳng). Validator: action `Quality` (`AddEssence/AddPotency/
  MulPotency/AddStat`) chỉ `OnRecipeMatch`; `OnShopRefresh` chỉ `Chance`; `RecipeIs` chỉ `OnRecipeMatch`/`OnBrewEnd` + thành
  phẩm có `Recipe`; `IngredientIs` khoáng/phụ gia; `CreateIngredient` nguyên liệu gốc; khoảng `EffectLimits`. Phụ gia không
  có Effect — `ItemData.MagicChance` chính là `AddMagicChance` (catalyst D1).
  **Trận** (F-010): `BattleData` (tham số `ContentDb.Create`) gồm hằng số trận (`HeroBaseHp/Atk`, `OffAffinityPercent`,
  `AbsorbCapPercent`, `ReflectCapPercent`, `MaxRounds`, `MaxStatValue` 10 000) + `EquipmentData` (`AtkPerQ`, `HpPerQ`,
  `Affinities: StatKind[]`) + `OpponentData` (★, `BattleKind`, HP/ATK/SB/PD/HT) + `SeasonData` (đối thủ cố định + bể
  `ForgeOptionData` có trọng số, đúng `SeasonCount` Mùa); `DifficultyData.OpponentStatPercent` (1–1000). Tra cứu
  `GetEquipment`/`TryFindEquipment`/`Season(n) → SeasonContent` (đã resolve).
  `ItemId` / `RecipeId`… là `struct` bọc chỉ số `int`, gán theo thứ tự sắp của `Id` chuỗi (ổn định). **Save lưu
  `Id` chuỗi**, không lưu chỉ số.
- Dữ liệu chỉ để hiển thị (sprite, `EquipmentRamp`, text) ở lại SO — Core không biết.

### 3.2 State
- **`RunState`** (class mutable, chỉ Core sửa): seed, `RngStreams`, độ khó, bộ khởi đầu; `Season` 1–8;
  `Phase` (`Decree · Workbench · Offering · Battle · Spoils · Ended`); `Turn` (1…`RunData.TurnsPerSeason` ở Workbench, 0 ở phase khác); `WorkbenchStep` (`Editing ·
  ResultPending`); vàng, Uy tín, sức chứa Túi; `Bag: List<ItemInstance>` (thứ tự list = thứ tự Xúc tác); `Grid:
  ItemInstance?[9]`; `SeasonPricePercent` (S, 100 tới khi rút ở Decree); `Shop` (`ShopState`: `List<ShopSlot>` 4 nguyên liệu + 1
  tiện ích, `ShopSlot.None` = rỗng/đã bán; `RerollCount`); `DecreeEquipment` (`EquipmentId`, rút ở Decree); `ActiveModifier`/`NextModifier` (`ModifierId?`); `LastBrew`, `LastBattle` (giữ sau `AckBattle`), `SpoilsOptions` (`List<RewardId>`, thứ tự khai báo bể, rỗng ngoài Spoils); `Outcome` (`None · Won · Exiled`; run `Ended` giữ `IsActive = true`);
  `NextUid`.
- **`ItemInstance`**: `Uid` (tăng dần, không tái dùng), `ItemId`, `Magic`, `Quality`, `Lines: StatLine[]`,
  `RevealedMask`.
- **`MetaState`**: theo khoáng `Unlocked(atUnix)`, `RecipeUnlocked`, `DiscoveredStatsMask`; Danh vọng; bậc khó /
  bộ khởi đầu đã mở; timescale.
- Mutable thay immutable: tránh copy + GC mỗi Command; nguyên tử nhờ kỷ luật validate-trước-mutate-sau.

### 3.3 Command & reducer
```csharp
public interface ICommand { }                       // record: PlaceItem(int uid, int cell) …
public readonly struct CommandResult { bool Accepted; RejectReason Reason; IReadOnlyList<GameEvent> Events; }
public static class GameRules {
    public static CommandResult Apply(RunState run, MetaState meta, ContentDb content, ICommand cmd);
}
```
- **Command**: `StartRun(diff, kit, seed, nowUnix)` · `AcceptDecree` · `PlaceItem` · `MoveCell` · `RemoveFromGrid`
  · `ReorderCatalyst(uid, toIndex)` (vị trí trên kệ, vật phẩm khác giữ chỉ số; chỉ Workbench·Editing) · `Buy(slot, cell?)` · `Sell` · `Reroll` · `Inspect(target, lens)` · `Brew(nowUnix)` ·
  `SkipTurn` · `AckBrewResult` · `Offer(uid)` (khay cận thần là state view — F-010 D2) · `DismissCourtier` · `AckBattle` ·
  `ChooseReward(i)` · `SkipReward` · `SetTimescale(n)` (meta, 1–3, nhận ở mọi thời điểm). Thời điểm thật do Runtime truyền vào.
  **Tua phát lại (`SkipReplay`) không là Command**: kết quả đã tính + lưu lúc `Brew`, tua chỉ là hiển thị
  (`BrewReplayTimeline.Skip`) → không lưu, không hash, không thể lệch replay (F-004, 2026-11-04).
- **Sức chứa Túi** (F-005): mọi luật thêm vật phẩm vào Túi hỏi `BagRules.HasRoom(run, n)` (Túi + lưới ≤
  `BagCapacity`) **trước** khi mutate; Nới Túi qua `BagRules.Expand` (cắt tại `RunData.MaxBagCapacity`). `ContentDb`
  chặn sức chứa khởi đầu ngoài `[1, trần]` và bộ khởi đầu vượt sức chứa.
- **Chợ** (F-006): `Buy(slot, cell?)` từ chối theo thứ tự `ValidateEditable` → `InvalidShopSlot` → `ShopSlotEmpty` →
  `InvalidCell`/`ItemNotPlaceable` (khi có `cell`) → `CatalystSlotsFull` → `BagFull` → `NotEnoughGold`; `Sell(uid)` từ Túi
  hoặc lưới; `Reroll` chỉ rút `Shop`. Giá tính lúc cần từ `ItemData.MarketPrice` × S (không lưu). `MarketRules.RefreshShop`
  (đặt lại reroll + bày hàng, stream `Shop`) và `RollSeasonPrice` (stream `Market`, 1 lần `NextInt`) **không là Command** —
  `SeasonRules` gọi; F-007 kích `OnShopRefresh` sau `RefreshShop` (trong `SeasonRules.StartTurn`).
- **Vòng lượt** (F-009 phần 1): `SeasonRules.EnterDecree` (public, `StartRun` + F-009 phần 2 gọi) đặt Decree, `Turn = 0`,
  rút S **lúc vào** Decree (F-010 thêm `Forge`, F-011 thêm `Season`, thứ tự Market → Forge → Season). `AcceptDecree` →
  lượt 1. Mỗi lượt: Editing + `RefreshShop` + `TurnStarted`. Kết thúc lượt: `AckBrewResult` (chỉ `ResultPending`, else
  `NoPendingResult`; giữ `LastBrew`) hoặc `SkipTurn` (chỉ Editing; lưới → cuối Túi ô 0→8). Hết lượt cuối → `Offering`,
  `Turn = 0`, xóa hàng Chợ, `OfferingStarted`. `StateHasher` ghi `Turn`.
- **Dâng → Trận → Resolution** (F-010): `EnterDecree` rút S rồi `ForgeRules.RollEquipment` (1 `NextInt(Σ trọng số)` stream
  `Forge`, kể cả bể 1 lựa chọn). `Offer(uid)` từ chối `RunNotActive` → `WrongPhase` → `ItemNotFound` → `ItemNotOfferable`
  (đạt chuẩn = khoáng có `Recipe`); hợp lệ: tiêu hao, `BattleStats.ForHero(Q, SB, PD, HT = 0 tới F-008)` ×
  `ForOpponent(đối thủ Mùa, % bậc khó)` → `BattleSimulator.Simulate` (không RNG) → `LastBattle`, `Phase = Battle`,
  `ItemOffered` + `BattleStarted`. `DismissCourtier` chỉ khi Túi không có món đạt chuẩn (`HasOfferableItem`) →
  `BattleLog.Forfeit`. `AckBattle` (chỉ `Battle`): Thua → Uy tín −1 (`ReputationLost`), 0 → `Ended·Exiled`; Mùa cuối →
  `Ended·Won`; Thắng → `SpoilsRules.Enter`; Thua còn Uy tín → `SeasonRules.StartNextSeason` (`Season + 1`, `EnterDecree`,
  `SeasonStarted`). `StateHasher` ghi `DecreeEquipment`, `Outcome`, `LastBattle`.
- **Chiến lợi phẩm** (F-011 Phần A): content `RewardData` (`Item` nguyên liệu gốc không phế phẩm | `ExpandBag` `Amount ≥ 1`) — danh
  sách toàn cục (`ContentDb.Create(..., rewards)`, `GetReward`/`TryFindReward`); `SeasonData.SpoilsGold` (0…1 000 000),
  `RewardPool` (`RewardOptionData` có trọng số), `GuaranteeCatalyst`; Mùa cuối không khai báo được. `SpoilsRules.Enter`:
  `Gold += SpoilsGold`, ứng viên = trọng số > 0 **và** nhận được (Item: chỗ Túi + suất Xúc tác; ExpandBag: chưa trần), đảm
  bảo → lượt đầu chỉ Xúc tác; mỗi lượt 1 `NextInt(Σ)` stream `Reward`, không lặp, tối đa `MaxSpoilsOptions = 3`; kết quả theo
  thứ tự khai báo bể; `SpoilsStarted(season, gold, options)`. `ChooseReward(i)` từ chối `RunNotActive` → `WrongPhase` →
  `InvalidRewardIndex` → `CatalystSlotsFull`/`BagFull`/`BagAtMaxCapacity`; Item → cuối Túi (`NextUid++`), ExpandBag →
  `BagRules.Expand`; `RewardChosen`. `SkipReward` luôn hợp lệ ở Spoils (`RewardSkipped`). Cả hai → `StartNextSeason`.
  `StateHasher` ghi `SpoilsOptions`.
- **Modifier Mùa** (F-011 Phần B): content `ModifierData` (`ModifierParamData`: `BuyPrice`/`SellPrice`/`LensPrice`/`RerollPrice`/
  `OpponentStats` Scale 1–1000 %, `ForgeWeight` 0–1000 % theo trang bị, `FreeRerolls` 0–2) → `SeasonModifier` (tra O(1),
  `Neutral`); `SeasonData.ModifierPool` + `NoModifierWeight` (bể của chính Mùa đó; Mùa 1 rỗng). Validator: `SellRate × M_bán
  < 10000`, mọi modifier trọng số > 0 giữ Σ trọng số bể trang bị sau scale ≥ 1. `RunState.ActiveModifier` / `NextModifier`
  (`ModifierId?`). `EnterDecree`: `Active = Next`, `Next = null` → Market → Forge (trọng số `⌊w × ForgeWeight% / 100⌋`) →
  `ModifierRules.RollNext` (Mùa < cuối: 1 `NextInt(NoModifierWeight + Σ)` stream `Season`, "không modifier" đứng trước).
  Áp: `MarketRules` (mua / Kính / bán / reroll + miễn phí), `BattleStats.ForOpponent(opp, diff%, mod%)` = `⌊v × diff × mod /
  10000⌋`. `StateHasher` ghi `ActiveModifier`, `NextModifier`.
- **Kính soi** (F-008): `Inspect(targetUid, lensUid)` — cả hai trong Túi; Workbench·Editing hoặc Offering. Từ chối
  `RunNotActive` → `WrongPhase` → `GridLocked` → `ItemNotFound` (target) → `ItemNotInspectable` (không có `Recipe`) →
  `ItemNotFound` (Kính) → `ItemNotLens` → `NothingToReveal` (Kính giữ lại). Dòng ẩn = `Quality` (bit 0) + `Lines` (bit i + 1);
  ẩn ≤ `RevealLines` → lộ hết, không rút; ngược lại mỗi dòng 1 `NextInt(còn lại)` stream `Inspect`. Kính tiêu hao,
  `ItemInspected(target, lens, bit mới)`. `Offer` truyền `StatLineRules.ValueOf` (Sắc bén/Phản đòn/Hấp thụ, kể cả chưa lộ)
  vào `ForHero`. `StateHasher` ghi `Lines` + `RevealedMask` mỗi vật phẩm.
- **Xúc tác** (F-007): `CatalystRunner.EvaluateBrew(bag, grid, recipe?, content, ref chance)` xét pha `OnBrewStart` →
  `OnRecipeMatch` | `OnBrewFail` → `OnBrewEnd`, trong pha theo thứ tự Túi rồi thứ tự khai báo; `Chance` tung trước và luôn
  tung khi trigger cháy; chỉ rút RNG → `CatalystBrewOutcome` (Essence/Potency/Mul/StatBonuses + kích hoạt). `Brew`: `Quality
  = QualityFormula(Base + ΣAddEssence, ΣAddPotency, Mul)`, `StatLineRules.ApplyBonuses` (cộng / thêm dòng, kẹp
  `MaxStatValue`, không tự lộ); sau `Bag.Add(result)`: `ApplyBrewOutcome` (`GainGold`, `CreateIngredient` 1 vật phẩm cuối
  Túi nếu `HasRoom`, không thì `Blocked`) → `BrewLog.Activations` + event `CatalystActivated` sau `BrewCompleted`.
  `SeasonRules.StartTurn`: `RefreshShop` → `CatalystRunner.RunShopRefresh` (stream `Shop`, áp ngay, event) → `TurnStarted`.
  Không Command / `RejectReason` mới. `StateHasher` ghi `LastBrew.Activations`.
- Handler gọi luật thuần (static, không state): `RecipeMatcher`, `BrewingRules`, `CatalystRunner`, `MarketRules`,
  `InspectRules`, `SeasonRules`, `BattleSimulator`, `SpoilsRules`, `RenownRules`.
- **Luật**: validate mọi điều kiện **trước** khi chạm state / RNG; bị từ chối → `Rejected(reason)`, không đổi gì.
  Chỉ lặp `List` / mảng — **cấm lặp `Dictionary` / `HashSet`** trong luật.

### 3.4 RNG
- `Pcg32` (struct `ulong state, inc`): `uint NextUInt()`, `int NextInt(int max)` (Lemire, không lệch),
  `bool Roll(int percent)`.
- `RngStreams`: 7 stream, mỗi stream `SplitMix64(seed ^ streamSalt)`. Mỗi lớp luật chỉ nhận stream của mình (`ref`).
- **Thứ tự rút `Chance` trong một lượt Nung** (hợp đồng giữa F-003/F-007/F-008 — đổi thứ tự = lệch replay):
  1. tung ma thuật — đúng 1 lần `Roll(MagicChance%)` khi khớp công thức, kể cả 0% (F-003, `BrewingRules`);
  2. dòng thuộc tính thành phẩm (F-008, `StatLineRules.Roll`: mỗi dòng khởi đầu theo thứ tự 1 `Roll(%)`, có → 1
  `NextInt(max − min + 1)`); 3. điều kiện `Chance` của Xúc tác (F-007: pha → thứ tự Túi → khai báo, mỗi Effect có trigger cháy 1 `Roll`). `OnShopRefresh` tung `Chance` trên stream `Shop` ngay sau `RefreshShop`.
  Phế phẩm: không rút ma thuật / thuộc tính. Command bị từ chối: không rút.

### 3.5 Log, View, Hash
- `BrewLog` (`Core/Logs`, bất biến): `FirstCell` + `Ticks` (`BrewTick`: `Cell`, `HasItem`, `ItemId`, `Magic`) từ vật
  phẩm đầu → cuối (ô trống ở giữa = "chờ") + `Result` (`BrewLogResult`: `IsScrap`, `ItemId`, `Magic`). **Không** chứa
  `Recipe`/`Pattern` (test reflection). `Brew` chụp trước khi tiêu hao → `RunState.LastBrew` + `BrewCompleted.Log`;
  `StateHasher` ghi `LastBrew` và `MetaState.Timescale`.
- `BattleLog` (`Core/Logs`, bất biến, ctor `internal`; F-010): `Season`, `Equipment`, `OfferedItem` + `OfferedMagic`
  (`HasOfferedItem` false khi forfeit), `Hero`/`Opponent` (`Combatant` đầu trận), `Rounds` (`BattleRound`: đòn ta, dội
  ngược ta nhận, đối thủ có đánh, đòn đối thủ, dội ngược đối thủ nhận, HP sau ≥ 0), tổng sát thương hai bên,
  `EndReason` (`Knockout · RoundLimit · Forfeit`), `HeroWon`. Không chứa `Quality`/dòng ẩn. Thuộc tính lộ + biểu cảm thợ
  rèn → F-017. Phép nhân trận qua `BattleMath.MulDivFloor` (không tràn `long`).
- `RunViewBuilder.Build(run, content) → RunView`: vật phẩm chỉ có `RevealedLines`; đối thủ chỉ có tên tới khi
  `Phase == Battle` (ADR-007).
- `StateHasher.Hash(run, meta) → ulong`: FNV-1a 64 trên `CanonicalWriter` nhị phân (thứ tự field cố định).

### 3.6 `GameSession` (Runtime, C# thuần)
```csharp
public sealed class GameSession {
    public GameSession(ContentDb content, ISaveStore store, IClock clock);
    public RunView Run { get; }  public MetaView Meta { get; }  public bool HasRun { get; }
    public event Action<CommandResult> CommandApplied;
    public CommandResult Execute(ICommand cmd);   // Apply → Accepted: lưu run/meta → dựng view → event
    public void Load();                            // đọc run.json + meta.json, migrate
}
```
- **Inject**: `ISaveStore` (`FileSaveStore` Newtonsoft + ghi atomic / `InMemorySaveStore` cho test), `IClock`
  (`SystemClock`).
- **Vòng đời**: `GameHost : MonoBehaviour` ở Boot tạo `GameSession`, `DontDestroyOnLoad`, rồi vào scene Game. View
  lấy session qua `GameHost` ở `Awake`; đăng ký `OnEnable` ↔ hủy `OnDisable`.
- DTO (`RunSaveDto`, `MetaSaveDto`) + `SaveMapper` ở Runtime; Core không phụ thuộc JSON.

### 3.7 Test & vị trí file
- **EditMode**: `Pcg32` vector chuẩn · độc lập stream (GDD §6.1 AC-3) · bảng `RecipeMatcher` · Command bị từ chối
  không đổi RNG · ví dụ trọn trận battle · 1.000 run lặp lại cùng `StateHash` · `RunView` không có field dòng ẩn /
  `Pattern` (reflection) · round-trip save cùng `StateHash`.
- **Fuzz trọn run** (F-009 phần 2, test-only): `TestUtils/RandomAgent` (lệnh theo phase + 10 % nhiễu + gợi ý công thức có
  mua/bán), `TestUtils/RunRecorder` (`Play` → `RunRecord` + `StepContext` mỗi bước), `ContentFactory.CreateFullRun(opp%)` (Spoils +
  modifier + 3 Xúc tác có Effect). `FullRunInvariantTest`: 500 seed × 2 hồ sơ, bất biến I1–I7 / Mùa / run, phủ nhánh; ~2 s.
- **File**: `Scripts/Core/{Content,State,Commands,Rules,Random,Logs,View}` ·
  `Scripts/Runtime/App/{GameHost,GameSession,ISaveStore,FileSaveStore,SaveDto,SaveMapper,SystemClock}`.

## 4. Lưu game

- Snapshot `RunState` (+ `MetaState` khi đổi) → DTO → JSON (Newtonsoft, ADR-003). Lưu **dữ liệu** (giá trị, seed,
  trạng thái stream RNG, log đang phát), không lưu object/scene.
- Trường `version`; chuỗi migrate cũ → mới khi load; validate; giữ backup tốt gần nhất (save run hỏng → xóa run,
  giữ meta; meta hỏng → backup — GDD §6.4).
- Ghi atomic: file tạm → flush → rename đè. Hai file: `run.json`, `meta.json`.
- Vị trí: `Application.persistentDataPath/save/`. WebGL = IndexedDB — ⚠ kiểm cơ chế đồng bộ (`FS.syncfs`) của
  Unity 6000.6 ở lần build WebGL đầu (`setup`/`build`).
- Thời điểm thật (vd "mở lúc" trong Sổ) do Runtime truyền vào Command — Core không đọc đồng hồ.

## 5. Quy ước code (bổ sung cho CLAUDE.md)

- **Forbidden trong Core**: `UnityEngine.*`, `float`/`double`, `DateTime.Now`, `System.Random`, static state.
- **Forbidden mọi nơi**: `Resources.Load` cho nội dung, `SendMessage`, singleton giữ state, `UnityEngine.Random`
  cho gameplay, Presentation tự tính luật.
- **Allowed libraries**: Newtonsoft JSON (`com.unity.nuget.newtonsoft-json`, ADR-003) — cần `link.xml` cho IL2CPP.
- **Define symbols**: `UNITY_INCLUDE_TESTS` cho seam test.

## 6. Ngân sách hiệu năng

| Hạng mục | Ngân sách | Đo bằng | Hiện tại | Ngày đo |
|---|---|---|---|---|
| FPS mục tiêu | 60 → 16.67 ms/frame (WebGL desktop; mobile chốt sau) | Profiler (Development build) | | |
| Gameplay scripts | ~3 ms | CPU module | | |
| Rendering (CPU submit) | ~4 ms | | | |
| UI / khác | ~3 ms | | | |
| GC Alloc mỗi frame (ổn định) | 0 B | `GC Allocated In Frame` | | |
| Draw calls màn Bàn luyện | ≤ 30 (ART §8.6) | Frame Debugger | | |
| Texture runtime | ≤ 32 MB (ART §8.6) | Memory Profiler | | |
| `Execute(cmd)` + lưu | ≤ 5 ms | ProfilerMarker | | |
| Build WebGL (nén) | ≤ 30 MB | Build report | | |
| Load lần đầu (WebGL) | ≤ 10 s | đo tay | | |

## 7. Architecture Decision Records

| ADR | Tiêu đề | Trạng thái | Ngày |
|---|---|---|---|
| ADR-001 | Lõi mô phỏng C# thuần + Command/Reducer | Accepted | 2026-11-04 |
| ADR-002 | Scene: Boot + một scene Game | Accepted | 2026-11-04 |
| ADR-003 | Lưu game: snapshot JSON (Newtonsoft) + version/migrate + ghi atomic | Accepted | 2026-11-04 |
| ADR-004 | Tất định: PCG32 + 7 stream SplitMix64; Core chỉ số nguyên | Accepted | 2026-11-04 |
| ADR-005 | Content: SO `*Def` → `ContentDb` bake lúc Boot, `Id` chuỗi | Accepted | 2026-11-04 |
| ADR-006 | Ánh sáng: sprite overlay (không URP 2D Light) | Accepted | 2026-11-04 |
| ADR-007 | `RunView` lọc thông tin ẩn trước khi tới Presentation | Accepted | 2026-11-04 |

### ADR-001: Lõi mô phỏng C# thuần + Command/Reducer
- **Bối cảnh**: GDD yêu cầu tất định tuyệt đối trên Editor/Standalone/WebGL, lưu sau mỗi hành động, test được toàn
  bộ luật; giả định lúc `init` là SO + event channel.
- **Các phương án**: A — Core C# thuần (`noEngineReferences`) + Command → reducer: test EditMode toàn bộ, lưu =
  snapshot, state hash dễ; nhược: phải map content SO → data. B — MonoBehaviour + SO event channel: quen thuộc;
  nhược: state rải nhiều object, khó lưu nguyên tử và chứng minh tất định.
- **Quyết định**: A. Thay giả định lúc `init`.
- **Hệ quả**: mọi luật nằm ở Core; Runtime mỏng; cần `ContentBaker` (ADR-005).
- **Luật cho code**: Required — mọi thay đổi state đi qua `GameSession.Execute`; Command validate trước mutate sau.
  Forbidden — MonoBehaviour sửa state; Core tham chiếu UnityEngine.
- **Tương thích Unity**: asmdef `noEngineReferences` (có từ 2019.2).

### ADR-002: Scene Boot + một scene Game
- **Bối cảnh**: WebGL load scene tốn thời gian; luồng màn ngắn, chuyển liên tục (≈ 40 lần/run).
- **Các phương án**: A — Boot + 1 scene Game, root bật/tắt; B — scene additive theo địa điểm (tách asset tốt hơn,
  nhiều lần load).
- **Quyết định**: A.
- **Hệ quả**: bộ nhớ chứa mọi màn cùng lúc (trong ngân sách §6); `ScreenRouter` điều phối root.
- **Luật cho code**: Forbidden — `SceneManager.LoadScene` ngoài Boot → Game.
- **Tương thích Unity**: —

### ADR-003: Lưu game bằng snapshot JSON (Newtonsoft)
- **Bối cảnh**: lưu sau mỗi hành động; state có map/danh sách lồng; cần migrate giữa phiên bản.
- **Các phương án**: A — `JsonUtility` + DTO mảng (không thêm package; không Dictionary/polymorphism); B — Newtonsoft
  JSON (Dictionary, polymorphism, migrate dễ; thêm package, cần `link.xml`, build lớn hơn).
- **Quyết định**: B — người dùng chọn, đồng ý thêm `com.unity.nuget.newtonsoft-json` (cài ở `setup`/`build`).
- **Hệ quả**: DTO tách khỏi Core state (Core không phụ thuộc JSON); test round-trip DTO; `link.xml` giữ type DTO.
- **Luật cho code**: Required — trường `version`, ghi atomic, backup. Forbidden — serialize trực tiếp object Core có
  logic; `TypeNameHandling` khác `None`.
- **Tương thích Unity**: kiểm phiên bản package tương thích 6000.6 khi cài; IL2CPP stripping.

### ADR-004: Tất định — PCG32, 7 stream, số nguyên
- **Bối cảnh**: GDD §6.1 (7 stream, cùng seed → cùng state hash mọi nền tảng); Trận số nguyên.
- **Các phương án**: A — PRNG tự viết (PCG32) + SplitMix64 chia stream, Core số nguyên; B — `System.Random` (thuật
  toán không được đảm bảo ổn định giữa runtime) + float (sai khác nền tảng).
- **Quyết định**: A.
- **Hệ quả**: trạng thái stream lưu trong save; test state hash 1.000 lần trên Editor + WebGL.
- **Luật cho code**: Forbidden — float/double, `System.Random`, `UnityEngine.Random` trong gameplay. Required — mỗi
  hệ thống chỉ dùng stream của mình; Command bị từ chối không rút random.
- **Tương thích Unity**: —

### ADR-005: Content SO → ContentDb
- **Bối cảnh**: designer chỉnh content trong Inspector; Core không được tham chiếu UnityEngine.
- **Các phương án**: A — SO `*Def` + `ContentBaker` → record bất biến; B — JSON content (dễ diff, mất Inspector).
- **Quyết định**: A; `Id` chuỗi ổn định (không dùng tên asset / GUID làm khóa gameplay).
- **Hệ quả**: editor validator: `Id` trùng, `Pattern` trùng, thứ tự biến thể trong Sổ không tương quan khoảng cách
  (ui-meta I2), `PossibleStats`, `EquipmentRamp` thuộc palette khóa.
- **Luật cho code**: Forbidden — Core đọc SO trực tiếp; đổi `Id` đã phát hành.
- **Tương thích Unity**: —

### ADR-006: Ánh sáng bằng sprite overlay
- **Bối cảnh**: ART A4 — ánh lò, spotlight Dâng, tint Thua; WebGL GPU yếu; chất pixel (dải bậc).
- **Các phương án**: A — sprite overlay additive/multiply theo trạng thái ART §2; B — URP 2D Light (mềm, tốn GPU,
  gradient mượt lệch chất pixel).
- **Quyết định**: A.
- **Hệ quả**: không dùng Light2D; overlay là asset pixel (ART §8); tắt theo "Giảm chuyển động" khi là hiệu ứng nhấp nháy.
- **Luật cho code**: Forbidden — `Light2D` trong scene game.
- **Tương thích Unity**: —

### ADR-007: RunView lọc thông tin ẩn
- **Bối cảnh**: GDD §7 "không rò rỉ"; UI dễ vô tình lộ dòng ẩn / khoảng cách.
- **Các phương án**: A — Core dựng `RunView` không chứa dữ liệu ẩn; B — Presentation đọc `RunState`, kiểm bằng test
  quét text.
- **Quyết định**: A (kèm test quét text như lớp bảo vệ thứ hai).
- **Hệ quả**: dữ liệu ẩn chỉ tồn tại trong Core + save; ngoại lệ sau khi dâng đi qua `BattleLog` / sự kiện riêng.
- **Luật cho code**: Forbidden — Presentation tham chiếu `RunState` / `Recipe.Pattern`.
- **Tương thích Unity**: —

## 8. Nợ kỹ thuật

| ID | Mô tả | Vị trí | Ảnh hưởng | Kế hoạch |
|---|---|---|---|---|
