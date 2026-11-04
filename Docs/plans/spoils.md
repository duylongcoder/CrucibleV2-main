# Plan: Chiến lợi phẩm & modifier Mùa (F-011)

> **Trạng thái**: In Progress — **Phần A Done** (741/741 EditMode xanh), Phần B chưa làm (D1–D12 duyệt 2026-11-04; D4 sửa: Bỏ qua chủ động) · **Cập nhật**: 2026-11-04
> **GDD**: `design/run-flow.md` §Chiến lợi phẩm, §Modifier Mùa, §Chiếu chỉ (modifier Mùa sau), §Trạng thái (hàng Spoils),
> Edge Cases (cả 3 không hợp lệ), Tuning Knobs (`SpoilsGold`), AC-6/AC-6b; `design/economy.md` §Túi (Nới Túi), §Formulas
> (M modifier), Edge Cases (chiến lợi phẩm khi Túi đầy, 6 Xúc tác); `design/battle.md` Tuning (`OpponentStatScale`);
> `design/ui-season.md` §A (dòng modifier), §E (Chiến lợi phẩm) · **ADR**: ADR-001 (Command/Reducer), ADR-004 (số nguyên +
> RNG stream), ADR-005 (Content trong Core) · **Loại**: Logic/Data (Core)
> Xây trên F-000…F-006, F-009 phần 1, F-010 (665/665 EditMode xanh, **chưa commit** lúc viết plan).

## 1. Bối cảnh

Sau F-010, Thắng trận Mùa < 8 đưa run vào `Phase = Spoils` nhưng **không lệnh nào rời Spoils** → ngõ cụt, chặn
F-009 phần 2 (RandomAgent 1.000 run tới RunEnd). Đồng thời `MarketRules`/`ForgeRules`/`OfferingRules` đang dùng hệ số
trung tính 100 ở mọi chỗ GDD dành cho **modifier Mùa** (stream `Season` chưa ai rút). Plan này làm cả hai, chia thành
**2 lát build độc lập** (Phần A gỡ ngõ cụt trước — đủ cho F-009 phần 2; Phần B sau):

- **Phần A — Chiến lợi phẩm**: vàng `SpoilsGold` + chọn 1/3 từ bể Mùa (stream `Reward`) + `ChooseReward` / `SkipReward`.
- **Phần B — Modifier Mùa**: rút modifier Mùa sau ở Chiếu chỉ (stream `Season`), áp vào giá mua/bán/Kính soi/reroll,
  reroll miễn phí, chỉ số đối thủ, trọng số bể trang bị.

Pillar: P3 (vàng khan hiếm, chọn 1/3 có đánh đổi, Túi/Xúc tác có trần), P1 (modifier biết trước 1 Mùa → lên kế hoạch),
onboarding GDD §3 (Mùa 2–3 đảm bảo có Xúc tác).

**Yêu cầu** (đánh số để truy vết):

*Phần A — Chiến lợi phẩm*
- R1 — **Content phần thưởng**: `RewardData` (Id, `RewardKind` `Item` | `ExpandBag`, `ItemId` khi Item, `Amount` khi
  ExpandBag) — danh sách toàn cục, tham số mới của `ContentDb.Create`. `SeasonData` thêm: `SpoilsGold` (≥ 0), bể
  `RewardPool` (`RewardOptionData(rewardId, weight ≥ 0)`, thứ tự khai báo), `GuaranteeCatalyst` (bool). Validate §2.
- R2 — **Vào Spoils** (trong `AckBattle`, nhánh Thắng & Mùa < `SeasonCount`): `Gold += SpoilsGold(Mùa)` **đúng 1 lần**,
  rút lựa chọn (R3) vào `RunState.SpoilsOptions`, `Phase = Spoils`, phát `SpoilsStarted(season, gold, options)`.
  Thua / Mùa cuối: không vàng, không rút `Reward` (AC-6).
- R3 — **Rút lựa chọn** (stream `Reward`): ứng viên = mục bể có trọng số > 0 **và nhận được ngay lúc này** (R4). Nếu
  `GuaranteeCatalyst` và có ứng viên Xúc tác → lượt rút đầu chỉ trong tập ứng viên Xúc tác. Các lượt sau rút trong
  ứng viên còn lại, **không lặp** (`RewardId` đã chọn bị loại). Mỗi lượt đúng 1 `NextInt(Σ trọng số ứng viên)`, kể cả
  khi chỉ còn 1 ứng viên (như forge-battle D4). Số lựa chọn = `min(3, số ứng viên)`. Thứ tự hiển thị = thứ tự khai báo
  bể (D3). (run-flow §Điều kiện xuất hiện; AC-6b.)
- R4 — **Nhận được** (`SpoilsRules.IsClaimable`): Item → `BagRules.HasRoom(run)` và (không phải Xúc tác hoặc
  `CountCatalysts < CatalystSlots`); ExpandBag → `BagRules.CanExpand`. (economy Edge Cases; run-flow Edge Cases.)
- R5 — `ChooseReward(index)`: chỉ ở `Spoils`. Từ chối theo thứ tự: `RunNotActive` → `WrongPhase` → `InvalidRewardIndex`
  (mới; ngoài `[0, SpoilsOptions.Count)`) → lý do không nhận được (`BagFull` / `CatalystSlotsFull` / `BagAtMaxCapacity`
  mới — phòng thủ, bình thường không xảy ra vì state không đổi trong Spoils). Hợp lệ: Item → `ItemInstance(NextUid++,
  item)` thêm cuối Túi (`Magic = false`, `Quality = 0`); ExpandBag → `BagRules.Expand(amount)`. Phát
  `RewardChosen(index, rewardId, itemUid | 0, bagAdded)` rồi rời Spoils (R7). Không rút RNG.
- R6 — `SkipReward`: hợp lệ ở `Spoils` **bất kể còn lựa chọn hay không** — người chơi có thể không muốn phần thưởng nào
  (D4, người dùng chốt 2026-11-04; đóng Open Question run-flow). Vàng `SpoilsGold` đã cộng lúc vào Spoils vẫn giữ. Phát
  `RewardSkipped(season)` rồi rời Spoils (R7). Từ chối: `RunNotActive` → `WrongPhase`.
- R7 — **Rời Spoils**: xóa `SpoilsOptions`, `Season + 1`, `EnterDecree`, `SeasonStarted(season + 1)` — dùng chung với
  nhánh Thua-còn-Uy-tín của `AckBattle` (`SeasonRules.StartNextSeason`).
- R8 — `RunState.SpoilsOptions: List<RewardId>` (rỗng ngoài Spoils); `StateHasher` ghi (sau `LastBattle`).

*Phần B — Modifier Mùa*
- R9 — **Content modifier**: `ModifierData` (Id, danh sách `ModifierParamData(param, value, equipmentId?)`), danh sách
  toàn cục, tham số mới của `ContentDb.Create`. `ModifierParam`: `BuyPrice`, `SellPrice`, `LensPrice`, `RerollPrice`,
  `OpponentStats`, `ForgeWeight` (cần `equipmentId`) — **Scale %**; `FreeRerolls` — **Offset** 0–2. `SeasonData` thêm bể
  `ModifierPool` (`ModifierOptionData(modifierId, weight ≥ 0)`) + `NoModifierWeight` (≥ 0, mặc định 1). Validate §2.
- R10 — **Rút modifier Mùa sau ở Chiếu chỉ**: `EnterDecree` Mùa S: `ActiveModifier = NextModifier` (modifier đã công bố ở
  Chiếu chỉ Mùa S−1), `NextModifier = null`; rút S (`Market`) → loại trang bị (`Forge`, có áp `ForgeWeight` của
  `ActiveModifier`) → nếu S < `SeasonCount`: đúng 1 `NextInt(NoModifierWeight + Σ trọng số)` trên stream `Season` từ bể
  của **Mùa S+1** → `NextModifier` (null nếu trúng phần "không modifier"). Mùa cuối: không rút `Season`. Thứ tự Market →
  Forge → Season (hợp đồng replay, TECH §3.3). Mùa 1 luôn không modifier (D6).
- R11 — **Giá** (economy §Formulas, M = modifier %): mua `max(1, ⌊MarketPrice × S × M_mua / 10000⌋)` — Kính soi dùng
  `M_kính` **thay** `M_mua` (D7); bán `⌊Giá mua × SellRate × M_bán / 10000⌋` (phế phẩm vẫn giá cố định, không modifier);
  reroll lần k (k từ 0): `k < FreeRerolls` → 0, ngược lại `⌊(RerollBase + (k − FreeRerolls) × RerollStep) × M_reroll / 100⌋`
  (D8). Modifier không có tham số → 100 % / 0 reroll miễn phí.
- R12 — **Bất biến mua-bán lỗ** (economy Edge Cases, AC-3): validator đảm bảo `SellRatePercent × M_bán < 10000` với mọi
  modifier → mua rồi bán cùng Mùa luôn lỗ.
- R13 — **Chỉ số đối thủ**: `BattleStats.ForOpponent` nhân `OpponentStatPercent` (bậc khó) × `M_đối thủ` (modifier đang
  áp): `⌊v × diff × mod / 10000⌋`, HP sau scale ≥ 1 (giữ D10 của F-010).
- R14 — **Trọng số bể trang bị**: `ForgeRules.RollEquipment` dùng `w' = ⌊w × M_forge(trang bị) / 100⌋`; vẫn đúng 1 lần
  `NextInt(Σ w')`. Validator: với mọi Mùa S ≥ 2 và mọi modifier trong bể của S, `Σ w' ≥ 1`.
- R15 — `RunState.ActiveModifier`, `NextModifier` (`ModifierId?`); `StateHasher` ghi. Chiếu chỉ hiển thị `NextModifier`
  (view F-014+ đọc state — không có event mới).

**Ngoài phạm vi**: UI màn Chiến lợi phẩm / dòng modifier ở Chiếu chỉ (F-014/F-015 view), Thuốc thử (sau MVP), modifier
đổi luật (#17), Danh vọng (F-012), lưu save (F-013 — chỉ ghi chú DTO), RandomAgent 1.000 run (F-009 phần 2).
**TBD (không làm, chỉ để field/dữ liệu test)**: số `SpoilsGold` từng Mùa, nội dung bể phần thưởng thật, trần tier khoáng
hiếm theo Mùa (do content quyết qua bể từng Mùa), danh sách modifier thật + khoảng an toàn — chốt ở `balance`.

## 2. Thiết kế triển khai

### Phần A

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Content/RewardKind.cs` | enum `Item = 0, ExpandBag = 1` | Core | Mới |
| `Core/Content/RewardData.cs` | record content: `Id`, `Kind`, `ItemId` (string), `Amount` | Core | Mới |
| `Core/Content/RewardOptionData.cs` | `(string RewardId, int Weight)` trong bể Mùa | Core | Mới |
| `Core/Content/RewardId.cs` | struct bọc chỉ số (như `EquipmentId`), gán theo thứ tự sắp ordinal của Id | Core | Mới |
| `Core/Content/Reward.cs` | đã resolve: `Id` (string, cho save), `Kind`, `Item` (`ItemId`), `Amount`, `IsCatalyst` | Core | Mới |
| `Core/Content/RewardOption.cs` | struct `(RewardId Reward, int Weight)` | Core | Mới |
| `Core/Content/SeasonData.cs` | + `SpoilsGold`, `RewardPool`, `GuaranteeCatalyst` (tham số tùy chọn, mặc định 0 / rỗng / false) | Core | Sửa |
| `Core/Content/SeasonContent.cs` | + `SpoilsGold`, `RewardPool: IReadOnlyList<RewardOption>`, `GuaranteeCatalyst` | Core | Sửa |
| `Core/Content/ContentDb.cs` | tham số `rewards`; resolve + validate; `GetReward(RewardId)`, `TryFindReward(string, out RewardId)`; hằng `MaxSpoilsOptions = 3`, `MaxSpoilsGold = 1_000_000` | Core | Sửa |
| `Core/Rules/SpoilsRules.cs` | `Enter`, `DrawOptions`, `IsClaimable`, `ChooseReward`, `SkipReward` | Core | Mới |
| `Core/Rules/SeasonRules.cs` | `AckBattle` nhánh Thắng gọi `SpoilsRules.Enter`; tách `StartNextSeason` (public) | Core | Sửa |
| `Core/Rules/GameRules.cs` | dispatch `ChooseReward`, `SkipReward` | Core | Sửa |
| `Core/Commands/ChooseReward.cs`, `SkipReward.cs` | Command | Core | Mới |
| `Core/Commands/RewardChosen.cs`, `RewardSkipped.cs` | Event | Core | Mới |
| `Core/Commands/SpoilsStarted.cs` | + `Gold`, `Options` (`IReadOnlyList<RewardId>`, bản sao) | Core | Sửa |
| `Core/Commands/RejectReason.cs` | + `InvalidRewardIndex = 23`, `BagAtMaxCapacity = 24` | Core | Sửa |
| `Core/State/RunState.cs` | + `SpoilsOptions: List<RewardId>` | Core | Sửa |
| `Core/State/StateHasher.cs` | ghi `SpoilsOptions` (count + index) | Core | Sửa |

**API Phần A**
```csharp
public static class SpoilsRules
{
    // SeasonRules.AckBattle gọi khi Thắng & Season < SeasonCount; thêm SpoilsStarted vào events.
    public static void Enter(RunState run, ContentDb content, List<GameEvent> events);
    public static bool IsClaimable(RunState run, ContentDb content, Reward reward);
    public static CommandResult ChooseReward(RunState run, ContentDb content, ChooseReward command);
    public static CommandResult SkipReward(RunState run, ContentDb content, SkipReward command);
    // private: DrawOptions(run, content, season, List<RewardId> into) — R3.
}
public static class SeasonRules
{
    // Season + 1, EnterDecree, SeasonStarted — AckBattle (Thua còn Uy tín) và SpoilsRules dùng chung.
    public static void StartNextSeason(RunState run, ContentDb content, List<GameEvent> events);
}
public sealed class ChooseReward : ICommand { public ChooseReward(int index); public int Index { get; } }
public sealed class SkipReward : ICommand { }
```

**Thuật toán rút (R3)** — chỉ lặp mảng/List, buffer tạm cấp phát mỗi lần vào Spoils (1 lần/Mùa, không phải vòng nóng):
1. `candidates` = các `RewardOption` của bể Mùa theo thứ tự khai báo, `Weight > 0` và `IsClaimable`.
2. Nếu `GuaranteeCatalyst` và có ứng viên `IsCatalyst`: `pick = WeightedPick(rng, candidates where IsCatalyst)`, chuyển
   vào `chosen`, xóa khỏi `candidates`.
3. Trong khi `chosen.Count < MaxSpoilsOptions` và `candidates` không rỗng: `WeightedPick(rng, candidates)` → `chosen`.
4. `SpoilsOptions` = `chosen` sắp theo vị trí khai báo trong bể (D3).

**Validate content (thêm vào `ContentDb.Create`)**
- Reward: Id không rỗng/không trùng; `Item` → `ItemId` tồn tại, `Recipe == null` (nguyên liệu gốc — D2), `Kind ≠ Scrap`,
  `Amount == 0`; `ExpandBag` → `ItemId == null`, `Amount ≥ 1`.
- Mỗi Mùa: `SpoilsGold ∈ [0, MaxSpoilsGold]`; bể không null phần tử, `RewardId` tồn tại, không trùng trong bể, trọng số
  ≥ 0, Σ ≤ `int.MaxValue`; `GuaranteeCatalyst` → bể có ≥ 1 phần thưởng Xúc tác trọng số > 0.
- **Mùa cuối** (`SeasonCount`): `SpoilsGold == 0`, bể rỗng, `GuaranteeCatalyst == false` (không có Spoils — D5).

### Phần B

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Content/ModifierParam.cs` | enum `BuyPrice, SellPrice, LensPrice, RerollPrice, OpponentStats, ForgeWeight, FreeRerolls` | Core | Mới |
| `Core/Content/ModifierParamData.cs` | `(ModifierParam Param, int Value, string EquipmentId = null)` | Core | Mới |
| `Core/Content/ModifierData.cs` | `(string Id, IReadOnlyList<ModifierParamData> Params)` | Core | Mới |
| `Core/Content/ModifierOptionData.cs` | `(string ModifierId, int Weight)` | Core | Mới |
| `Core/Content/ModifierId.cs` | struct bọc chỉ số | Core | Mới |
| `Core/Content/SeasonModifier.cs` | đã resolve, tra O(1): `Id`, `BuyPercent`, `SellPercent`, `LensPercent`, `RerollPercent`, `OpponentStatPercent`, `FreeRerolls`, `ForgeWeightPercent(EquipmentId)`; `static Neutral` | Core | Mới |
| `Core/Content/ModifierOption.cs` | struct `(ModifierId Modifier, int Weight)` | Core | Mới |
| `Core/Content/SeasonData.cs` / `SeasonContent.cs` | + `ModifierPool`, `NoModifierWeight` (+ `ModifierTotalWeight` ở bản resolve) | Core | Sửa |
| `Core/Content/ContentDb.cs` | tham số `modifiers`; resolve + validate; `GetModifier(ModifierId)`, `TryFindModifier`; hằng `MinModifierPercent = 1`, `MaxModifierPercent = 1000`, `MaxFreeRerolls = 2` | Core | Sửa |
| `Core/Rules/ModifierRules.cs` | `Active(run, content) → SeasonModifier` (Neutral nếu null); `RollNext(run, content)` | Core | Mới |
| `Core/Rules/SeasonRules.cs` | `EnterDecree`: shift → Market → Forge → `ModifierRules.RollNext` | Core | Sửa |
| `Core/Rules/MarketRules.cs` | `BuyPrice`/`SellPrice`/`RerollPrice` dùng modifier (R11); bỏ `NeutralPercent` khỏi chỗ M | Core | Sửa |
| `Core/Rules/PriceFormula.cs` | `Sell(buyPrice, sellRatePercent, modifierPercent)`; `Reroll(base, step, count, freeRerolls, modifierPercent)` | Core | Sửa |
| `Core/Rules/ForgeRules.cs` | trọng số × `ForgeWeightPercent` (R14) | Core | Sửa |
| `Core/Rules/BattleStats.cs` | `ForOpponent(opponent, difficultyPercent, modifierPercent)` | Core | Sửa |
| `Core/Rules/OfferingRules.cs` | truyền `ModifierRules.Active(...).OpponentStatPercent` | Core | Sửa |
| `Core/State/RunState.cs` | + `ActiveModifier`, `NextModifier` (`ModifierId?`) | Core | Sửa |
| `Core/State/StateHasher.cs` | ghi 2 field (HasValue + Index) | Core | Sửa |

**API Phần B**
```csharp
public static class ModifierRules
{
    public static SeasonModifier Active(RunState run, ContentDb content);
    // Chỉ gọi khi run.Season < SeasonCount: 1 NextInt trên stream Season, bể của Mùa run.Season + 1.
    public static void RollNext(RunState run, ContentDb content);
}
public static class PriceFormula
{
    public static int Buy(int marketPrice, int seasonPercent, int modifierPercent);            // không đổi
    public static int Sell(int buyPrice, int sellRatePercent, int modifierPercent);             // ⌊buy × rate × mod / 10000⌋
    public static int Reroll(int rerollBase, int rerollStep, int rerollCount, int freeRerolls, int modifierPercent);
}
public static class BattleStats
{
    public static Combatant ForOpponent(OpponentData opponent, int difficultyPercent, int modifierPercent);
}
```

**Validate content (Phần B)**
- Modifier: Id không rỗng/không trùng; mỗi `ModifierParam` tối đa 1 lần, riêng `ForgeWeight` tối đa 1 lần **mỗi trang bị**;
  Scale ∈ `[1, 1000]`; `FreeRerolls` ∈ `[0, 2]`; `ForgeWeight` cần `EquipmentId` tồn tại, tham số khác phải `EquipmentId == null`;
  `SellRatePercent × SellPercent < 10000` (R12).
- Mỗi Mùa: `NoModifierWeight ≥ 0`; bể modifier: Id tồn tại, không trùng, trọng số ≥ 0; Mùa 2…N: `NoModifierWeight + Σ ∈ [1,
  int.MaxValue]`; **Mùa 1**: bể rỗng (D6); với mọi modifier trọng số > 0 trong bể Mùa S: Σ trọng số trang bị sau scale ≥ 1 (R14).

**Interface phụ thuộc**: không mới — RNG qua `run.Rng.Get(RngStream.Reward / Season)`.
**Dữ liệu**: chỉ record content trong Core; `RewardDef`/`SeasonModifierDef` SO + `ContentBaker` thuộc việc baker chung (chưa có).
**Scene / prefab bị chạm**: không.
**Ghi chú F-013 (save DTO)**: phải mang `SpoilsOptions` (Id chuỗi phần thưởng), `ActiveModifier`, `NextModifier` (Id chuỗi).

### Quyết định cần duyệt

- **D1 — Chia 2 lát build**: Phần A (Spoils) build + commit trước, Phần B (modifier) sau. A đủ để mở F-009 phần 2.
- **D2 — Phần thưởng vật phẩm chỉ là nguyên liệu gốc** (`Recipe == null`; khoáng/phụ gia tier cao, Xúc tác, Kính):
  tránh câu hỏi "`Quality` của khoáng thành phẩm từ phần thưởng" và tránh dâng thẳng phần thưởng. Khớp chú thích
  `ItemData.Recipe` ("nguyên liệu gốc chỉ từ Chợ / bộ khởi đầu / chiến lợi phẩm").
- **D3 — Thứ tự thẻ = thứ tự khai báo bể**, không phải thứ tự rút: không lộ thẻ nào là "bắt buộc", ổn định cho test/UI.
- **D4 — `SkipReward` luôn dùng được ở Spoils** (người dùng chốt 2026-11-04: có thể không muốn nhận phần thưởng nào).
  Lựa chọn lọc trước khi rút theo run-flow (thẻ hiện ra luôn nhận được; bể còn < 3 thì hiện ít hơn); sửa `ui-season.md`
  §E (bỏ thẻ mờ, nút Bỏ qua luôn có) và run-flow Open Questions ở Bước A5.
- **D5 — Mùa cuối không khai báo Spoils** (validator chặn `SpoilsGold ≠ 0` / bể ≠ rỗng): ui-season §E "Thắng Mùa 8 +
  vàng (nếu có)" bỏ phần vàng — vàng mất khi run kết thúc, không có nghĩa.
- **D6 — Mùa 1 không modifier**: modifier "biết trước 1 Mùa" mà Mùa 1 không có Chiếu chỉ trước → không công bố được;
  cũng giữ Mùa 1 đơn giản (onboarding). Validator chặn bể modifier Mùa 1.
- **D7 — Kính soi dùng `LensPrice` thay `BuyPrice`** (không nhân chồng): mỗi tham số điều khiển đúng một nhóm hàng;
  muốn "mọi thứ đắt" thì modifier khai báo cả hai.
- **D8 — Reroll miễn phí không tính vào bậc tăng giá**: lần trả phí đầu tiên luôn giá `RerollBase` (× M). `RerollCount`
  vẫn đếm mọi lần (cả miễn phí).
- **D9 — `SpoilsGold` ở `SeasonData`** (theo Mùa) thay vì curve trong `RunData` như Tuning Knobs ghi (`RunDef`): mọi thứ
  "theo Mùa" đã nằm ở `SeasonData`; baker vẫn có thể đọc từ curve của `RunDef`.
- **D10 — `Reward`/`Modifier` là danh sách toàn cục, Mùa tham chiếu theo Id** (như đối thủ): Id chuỗi ổn định cho save.
- **D11 — `GuaranteeCatalyst: bool`** thay cho "lựa chọn bắt buộc" tổng quát: GDD chỉ có một ví dụ (Xúc tác Mùa 2–3);
  tổng quát hóa khi có ca thứ hai.
- **D12 — Rút `Reward` cả khi chỉ còn 1 ứng viên; rút `Season` cả khi bể chỉ có "không modifier"** (như D4 của F-010):
  số lần rút chỉ phụ thuộc state + content, hợp đồng replay đơn giản.

## 3. Test cases

### Editor tests
(none)

### Unit tests — Phần A

#### ContentDbRewardTest (`Tests/EditMode/Core/Content/`)
| Test method | Kiểm tra |
|---|---|
| `Create_ValidRewards_ResolvesKindItemAndAmount` | `GetReward` trả đúng Kind / Item / Amount |
| `TryFindReward_KnownId_ReturnsSortedIndex` | `RewardId` theo thứ tự sắp ordinal |
| `Create_DuplicateRewardId_Throws` | `ArgumentException` |
| `Create_ItemRewardUnknownItem_Throws` | `ArgumentException` |
| `Create_ItemRewardWithRecipe_Throws` | `ArgumentException` |
| `Create_ItemRewardScrap_Throws` | `ArgumentException` |
| `Create_ExpandBagAmountBelowOne_Throws` (amount: {0, −1}) | `ArgumentException` |
| `Create_SeasonPoolUnknownReward_Throws` | `ArgumentException` |
| `Create_SeasonPoolDuplicateReward_Throws` | `ArgumentException` |
| `Create_SeasonPoolNegativeWeight_Throws` | `ArgumentException` |
| `Create_SpoilsGoldOutOfRange_Throws` (gold: {−1, MaxSpoilsGold + 1}) | `ArgumentException` |
| `Create_GuaranteeCatalystWithoutCatalystReward_Throws` | `ArgumentException` |
| `Create_LastSeasonDeclaresSpoils_Throws` (gold / pool / guarantee) | `ArgumentException` |

#### SpoilsRulesTest (`Tests/EditMode/Core/Rules/`) — gọi qua `GameRules.Apply(AckBattle)` trên run Thắng
| Test method | Kiểm tra |
|---|---|
| `Enter_Win_AddsSpoilsGoldOnce` | `Gold` tăng đúng `SpoilsGold` Mùa đó |
| `Enter_PoolOfFiveClaimable_OffersThreeDistinct` | 3 `RewardId` khác nhau, đều thuộc bể |
| `Enter_PoolOfTwoClaimable_OffersTwo` | 2 lựa chọn |
| `Enter_EmptyPool_OffersNone` | `SpoilsOptions` rỗng, `Phase = Spoils` |
| `Enter_ZeroWeightOption_NeverOffered` (200 seed) | Id trọng số 0 không xuất hiện |
| `Enter_BagFull_ExcludesItemRewards` | chỉ còn ExpandBag |
| `Enter_CatalystSlotsFull_ExcludesCatalystRewards` | không có phần thưởng Xúc tác |
| `Enter_BagAtMaxCapacity_ExcludesExpandBag` | không có ExpandBag |
| `Enter_GuaranteeCatalystWithRoom_AlwaysOffersCatalyst` (500 seed) | mọi lần có ≥ 1 Xúc tác (AC-6b) |
| `Enter_GuaranteeCatalystButSlotsFull_OffersWithoutCatalyst` | không lỗi, 0 Xúc tác |
| `Enter_Options_OrderedByPoolDeclaration` | thứ tự `SpoilsOptions` tăng theo vị trí trong bể |
| `Enter_SameSeed_SameOptions` | hai run cùng seed → cùng lựa chọn |
| `Enter_OnlyRewardStreamAdvances` | 6 stream còn lại giữ nguyên state |
| `Enter_DrawsOncePerOption` | state `Reward` = sau đúng `min(3, n)` lần `NextInt` (so với `Pcg32` tự rút) |
| `Enter_EmitsSpoilsStartedWithGoldAndOptions` | event mang gold + danh sách = `SpoilsOptions` |

#### GameRulesChooseRewardTest
| Test method | Kiểm tra |
|---|---|
| `Apply_ChooseItemReward_AddsItemToBagEnd` | món cuối Túi đúng `ItemId`, `Uid = NextUid` cũ |
| `Apply_ChooseItemReward_IncrementsNextUid` | `NextUid + 1` |
| `Apply_ChooseExpandBag_IncreasesCapacityByAmount` | `BagCapacity + Amount` |
| `Apply_ChooseExpandBagNearCap_ClampsToMax` | `BagCapacity = MaxBagCapacity` |
| `Apply_Choose_EntersNextSeasonDecree` | `Season + 1`, `Phase = Decree`, `SpoilsOptions` rỗng |
| `Apply_Choose_EmitsRewardChosenThenSeasonStarted` | thứ tự event |
| `Apply_ChooseInvalidIndex_Rejects` (index: {−1, Count}) | `InvalidRewardIndex`, state hash không đổi |
| `Apply_ChooseWhenBagFull_RejectsBagFull` (state dựng tay) | `BagFull`, hash không đổi |
| `Apply_ChooseCatalystWhenSlotsFull_RejectsCatalystSlotsFull` | `CatalystSlotsFull` |
| `Apply_ChooseExpandAtMax_RejectsBagAtMaxCapacity` | `BagAtMaxCapacity` |
| `Apply_ChooseOutsideSpoils_RejectsWrongPhase` (phase: mọi phase ≠ Spoils) | `WrongPhase` |
| `Apply_ChooseWhenNotActive_RejectsRunNotActive` | `RunNotActive` |

#### GameRulesSkipRewardTest
| Test method | Kiểm tra |
|---|---|
| `Apply_SkipWithNoOptions_EntersNextSeasonDecree` | `Season + 1`, `Phase = Decree` |
| `Apply_SkipWithNoOptions_EmitsRewardSkipped` | event `RewardSkipped(season)` |
| `Apply_SkipWithOptions_EntersNextSeasonWithoutReward` | `Season + 1`, Túi / `BagCapacity` không đổi |
| `Apply_Skip_KeepsSpoilsGold` | `Gold` giữ phần vàng đã cộng |
| `Apply_SkipWhenNotActive_RejectsRunNotActive` | `RunNotActive` |
| `Apply_SkipOutsideSpoils_RejectsWrongPhase` | `WrongPhase` |

#### GameRulesAckBattleTest (sửa)
| Test method | Kiểm tra |
|---|---|
| `Apply_AckWinBeforeLastSeason_EntersSpoils` (giữ) | `Phase = Spoils` |
| `Apply_AckLoss_GivesNoSpoilsGold` | `Gold` không đổi |
| `Apply_AckWinInLastSeason_DoesNotDrawReward` | state stream `Reward` không đổi |

### Unit tests — Phần B

#### ContentDbModifierTest
| Test method | Kiểm tra |
|---|---|
| `Create_ModifierWithoutParams_IsNeutral` | mọi % = 100, `FreeRerolls = 0`, `ForgeWeightPercent` = 100 |
| `Create_ModifierParams_Resolved` | từng tham số đọc đúng giá trị |
| `Create_DuplicateModifierId_Throws` | `ArgumentException` |
| `Create_ScaleOutOfRange_Throws` (value: {0, 1001}) | `ArgumentException` |
| `Create_FreeRerollsOutOfRange_Throws` (value: {−1, 3}) | `ArgumentException` |
| `Create_DuplicateParam_Throws` | `ArgumentException` |
| `Create_ForgeWeightUnknownEquipment_Throws` | `ArgumentException` |
| `Create_ForgeWeightWithoutEquipment_Throws` | `ArgumentException` |
| `Create_NonForgeParamWithEquipment_Throws` | `ArgumentException` |
| `Create_SellModifierAllowsProfit_Throws` | `ArgumentException` (rate × M_bán ≥ 10000) |
| `Create_FirstSeasonModifierPool_Throws` | `ArgumentException` |
| `Create_SeasonModifierWeightsSumZero_Throws` | `ArgumentException` |
| `Create_ModifierZeroesForgePool_Throws` | `ArgumentException` (Σ w' = 0) |

#### ModifierRulesTest
| Test method | Kiểm tra |
|---|---|
| `Active_NoModifier_ReturnsNeutral` | `SeasonModifier.Neutral` |
| `RollNext_OnlyNoModifierWeight_LeavesNextNull` | `NextModifier == null` |
| `RollNext_OnlyOneModifier_SetsNext` | `NextModifier` = modifier đó |
| `RollNext_ZeroWeightModifier_NeverChosen` (200 seed) | không xuất hiện |
| `RollNext_UsesPoolOfFollowingSeason` | modifier thuộc bể Mùa S+1 |
| `RollNext_OnlySeasonStreamAdvances` | stream khác giữ nguyên |

#### SeasonRulesEnterDecreeTest (sửa / thêm)
| Test method | Kiểm tra |
|---|---|
| `EnterDecree_ShiftsNextModifierToActive` | `ActiveModifier` = `NextModifier` cũ |
| `EnterDecree_FirstSeason_HasNoActiveModifier` | `ActiveModifier == null` |
| `EnterDecree_LastSeason_DoesNotDrawSeasonStream` | state `Season` không đổi, `NextModifier == null` |
| `EnterDecree_DrawsMarketThenForgeThenSeason` | state 3 stream = rút thủ công theo thứ tự |
| `EnterDecree_ForgeRollUsesActiveModifierWeights` | modifier đưa trọng số trang bị A về… chỉ còn B → luôn B |

#### PriceFormulaTest / MarketRulesPriceTest (sửa / thêm)
| Test method | Kiểm tra |
|---|---|
| `Sell_WithModifier_FloorsProduct` (buy, rate, mod: {(12, 75, 100) → 9, (12, 75, 50) → 4}) | giá bán |
| `Reroll_WithinFreeRerolls_ReturnsZero` (count: {0, 1}, free 2) | 0 |
| `Reroll_AfterFreeRerolls_StartsAtBase` (count 2, free 2) | `RerollBase × M / 100` |
| `Reroll_WithModifier_ScalesPrice` | `⌊(base + k × step) × M / 100⌋` |
| `BuyPrice_ActiveBuyModifier_Applies` | `⌊MarketPrice × S × M_mua / 10000⌋` |
| `BuyPrice_Lens_UsesLensModifierNotBuy` | Kính chỉ nhân `M_kính` |
| `SellPrice_ActiveSellModifier_Applies` | công thức R11 |
| `SellPrice_Scrap_IgnoresModifier` | `ScrapSellPrice` |
| `RerollPrice_ActiveFreeRerolls_FirstIsFree` | 0 |
| `Apply_RerollFree_KeepsGold` (qua `GameRules`) | `Gold` không đổi, `RerollCount + 1` |

#### BattleStatsTest / GameRulesOfferTest (sửa / thêm)
| Test method | Kiểm tra |
|---|---|
| `ForOpponent_DifficultyAndModifier_MultiplyFloor` | `⌊v × diff × mod / 10000⌋` từng chỉ số |
| `ForOpponent_ScaledHpBelowOne_ClampsToOne` (giữ, chữ ký mới) | HP = 1 |
| `Apply_OfferWithOpponentModifier_ScalesOpponentInLog` | `LastBattle` chỉ số đối thủ đã nhân modifier |

#### StateHasherTest (thêm case vào `Hash_StatesDifferInOneField_*`)
| Case | Kiểm tra |
|---|---|
| `SpoilsOptions` khác (A); `ActiveModifier`, `NextModifier` khác (B) | hash khác |

### Integration tests (EditMode, nhiều luật qua `GameRules.Apply`)

#### RunLoopTest / RunLoopInvariantTest (thêm)
| Test method | Kiểm tra |
|---|---|
| `Spoils_EveryOfferedOptionIsClaimable` (1.000 seed, Túi đầy/không, Xúc tác đủ/không) | `ChooseReward(i)` chấp nhận với mọi i < `SpoilsOptions.Count` |
| `RunLoop_WinEverySeason_ReachesRunEndWon` (agent thắng chắc: content đối thủ yếu) | `Phase = Ended`, `Outcome = Won`, đi qua 7 lần Spoils |
| `RunLoop_SameSeedSameCommands_SameHash` (có Spoils + modifier) | hash cuối bằng nhau |
| `Market_BuyThenSellSameSeason_AlwaysLoses_WithAnyModifier` (MarketInvariantTest, Phần B) | `Gold` sau < trước |

### Visual verification tests
(none — chưa có view; UI Chiến lợi phẩm / dòng modifier thuộc F-014/F-015)

### Manual tests
(none)

**Truy vết**: R1 → ContentDbRewardTest; R2 → `Enter_Win_AddsSpoilsGoldOnce`, `Apply_AckLoss_GivesNoSpoilsGold`,
`Apply_AckWinInLastSeason_DoesNotDrawReward`, `Enter_EmitsSpoilsStarted…`; R3 → SpoilsRulesTest `Enter_*` (số lượng,
không lặp, trọng số 0, guarantee, thứ tự, số lần rút); R4 → `Enter_BagFull_*`, `Enter_CatalystSlotsFull_*`,
`Enter_BagAtMaxCapacity_*`; R5 → GameRulesChooseRewardTest; R6 → GameRulesSkipRewardTest; R7 → `Apply_Choose_EntersNextSeasonDecree`,
`Apply_SkipWithNoOptions_Enters…`, `Apply_SkipWithOptions_*`, `RunLoop_WinEverySeason_*`, `Spoils_EveryOfferedOptionIsClaimable`; R8 → StateHasherTest; R9 →
ContentDbModifierTest; R10 → ModifierRulesTest + SeasonRulesEnterDecreeTest; R11 → PriceFormulaTest + MarketRulesPriceTest;
R12 → `Create_SellModifierAllowsProfit_Throws`, `Market_BuyThenSell…_WithAnyModifier`; R13 → BattleStatsTest +
`Apply_OfferWithOpponentModifier_*`; R14 → `EnterDecree_ForgeRollUsesActiveModifierWeights`, `Create_ModifierZeroesForgePool_Throws`;
R15 → StateHasherTest + `EnterDecree_ShiftsNextModifierToActive`.

**Testability**: PASS — luật static thuần, RNG qua stream có seed, state dựng bằng `StateFactory` (thêm
`CreateSpoilsRun(content, options, …)`), so sánh state qua `StateHasher`. Không có state ẩn hay phụ thuộc thời gian.

## 4. Các bước thực hiện

> Commit theo `CLAUDE.md` §Git (không tự commit; không commit test-first). Mỗi phần đi đủ 5 bước rồi mới sang phần kế.
> Kiểm chứng chung: `"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults
> Logs/editmode.xml -logFile -` (Editor đóng) — đọc `Logs/editmode.xml` (`result="Passed"`, total/failed).

### Phần A — Chiến lợi phẩm
- [x] **A1 — Skeleton**: `RewardKind`, `RewardData`, `RewardOptionData`, `RewardId`, `Reward`, `RewardOption`; mở rộng
  `SeasonData`/`SeasonContent` (tham số tùy chọn); `ContentDb.Create(..., battle, rewards)` + `GetReward`/`TryFindReward`
  (thân tối thiểu); `RunState.SpoilsOptions`; Commands/Events; `RejectReason` +3; `SpoilsRules` thân rỗng; `ContentFactory`
  thêm bể phần thưởng test (`reward_mithril`, `reward_bellows`, `reward_mortar`, `reward_lens`, `reward_bag_2`) + tham số;
  `StateFactory.CreateSpoilsRun`. Kiểm chứng: compile sạch, **665 test cũ vẫn xanh**.
- [x] **A2 — Test first**: viết test Phần A §3; chạy → test mới fail đúng lý do (assert, không exception compile).
- [x] **A3 — Implement**: validate content, `SpoilsRules`, `SeasonRules.StartNextSeason` + nhánh Thắng của `AckBattle`,
  dispatch `GameRules`, `StateHasher`. Kiểm chứng: toàn bộ EditMode xanh.
- [x] **A4 — Refactor**: gộp test tham số hóa cùng phân vùng; bỏ trùng giữa `AckBattle` và `StartNextSeason`. Test xanh.
- [x] **A5 — Tài liệu**: PLAN.md F-011 (Phần A Done), F-009 phần 2 bỏ ghi chú "ngõ cụt"; TECH §3.2/§3.3 (Spoils,
  `ChooseReward`/`SkipReward`, thứ tự từ chối, stream `Reward`); `ui-season.md` §E theo D4/D5; run-flow AC-6/AC-6b
  tick phần logic; LOG + CHECKPOINT. Đề xuất commit `feat(spoils): add spoils gold, reward draw and ChooseReward/SkipReward`.

### Phần B — Modifier Mùa
- [ ] **B1 — Skeleton**: `ModifierParam`, `ModifierParamData`, `ModifierData`, `ModifierOptionData`, `ModifierId`,
  `SeasonModifier` (+ `Neutral`), `ModifierOption`; `SeasonData`/`SeasonContent` + bể modifier; `ContentDb.Create(...,
  modifiers)`; `RunState.ActiveModifier/NextModifier`; `ModifierRules` thân trả Neutral; đổi chữ ký `PriceFormula.Sell/
  Reroll`, `BattleStats.ForOpponent` (gọi cũ truyền 100 / 0 — hành vi không đổi). Kiểm chứng: compile, test cũ xanh
  (sửa chỗ gọi chữ ký trong test cũ, không đổi kỳ vọng).
- [ ] **B2 — Test first**: test Phần B §3; xác nhận fail đúng lý do. Lưu ý: test so sánh state RNG sau `EnterDecree`
  (`SeasonRulesEnterDecreeTest`, `GameRulesStartRunTest`, `RunLoopTest`) sẽ đổi vì thêm lần rút `Season` — sửa kỳ vọng
  ở bước này và ghi vào §6 *Test changes*.
- [ ] **B3 — Implement**: validate, `ModifierRules`, `EnterDecree` (shift → Market → Forge → Season), giá, reroll miễn phí,
  forge weight, đối thủ, `StateHasher`. Kiểm chứng: toàn bộ EditMode xanh.
- [ ] **B4 — Refactor**: bỏ `MarketRules.NeutralPercent` nếu không còn dùng; `OfferingRules.NeutralStatPercent` gom về
  `SeasonModifier.Neutral`. Test xanh.
- [ ] **B5 — Tài liệu**: PLAN.md F-011 Done (+ ghi chú F-013 DTO); TECH §3.3/§3.4 (thứ tự Market → Forge → Season, công
  thức giá có modifier); economy.md §Formulas (bán × M_bán, reroll miễn phí — D7/D8); GDD Systems Index #11 Implemented;
  LOG + CHECKPOINT. Đề xuất commit `feat(season): add season modifiers for prices, rerolls, opponents and forge pool`.

## 5. Rủi ro & đánh đổi

- **Đổi chữ ký public** (`ContentDb.Create` +2 tham số, `PriceFormula.Sell/Reroll`, `BattleStats.ForOpponent`,
  `SpoilsStarted`): chỉ có test gọi (chưa có Runtime/baker) — sửa `ContentFactory` là chính. Chấp nhận.
- **Replay lệch vì thêm lần rút `Season`**: mọi kỳ vọng state RNG sau `EnterDecree` đổi một lần ở B2; chưa có save thật
  nên không phá dữ liệu người chơi.
- **ui-season §E lệch run-flow** (thẻ mờ vs lọc trước) — chốt theo run-flow + D4; sửa ui-season ở A5.
- **Tràn số đối thủ**: `v × diff × mod` ≤ 10 000 × 1 000 × 1 000 = 10¹⁰ — trong `long` qua `MulDivFloor`; kết quả ≤
  10⁶ vừa `int`. Mô phỏng đã tính trong `long` (D9 F-010) — giữ nguyên, thêm 1 case biên ở BattleStatsTest.
- **Cân bằng**: `SpoilsGold` lớn làm mất khan hiếm vàng (P3); modifier giá cực đoan — validator chỉ chặn lỗi logic
  (lãi mua-bán, bể trang bị rỗng), khoảng an toàn thật chốt ở `balance`.
- **Testability**: PASS.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - (A) `ContentDb.Create(..., battle, rewards = null)` — tham số **tùy chọn** (null = không có phần thưởng) để 8 test content
    gọi `ContentDb.Create` trực tiếp không phải sửa; `ContentFactory` luôn truyền `CreateRewards()`.
  - (A) `SeasonData` thêm 3 tham số tùy chọn (mặc định: 0 vàng, bể rỗng, không đảm bảo) → `CreateSeasons()` mặc định
    không có Chiến lợi phẩm; test cần Spoils dùng `ContentFactory.CreateWithSpoils()` / `CreateSpoilsSeasons()`.
  - (A) `ClaimReason` theo thứ tự của `Buy`: `CatalystSlotsFull` trước `BagFull`.
  - (A) `Pick` không rút RNG khi không có ứng viên phù hợp (guarantee mà không còn Xúc tác nhận được) — D12 chỉ áp cho lượt có ≥ 1 ứng viên.
  - (A) Phần thưởng Item tạo `ItemInstance(NextUid++, item)` với `Magic = false`, `Quality = 0` (nguyên liệu gốc — D2).
- **Deviations** (lệch so với plan và lý do):
  - (A) Gộp chạy test của Bước 1 vào Bước 2 (một lần batchmode cho skeleton + test mới) — mỗi lần batchmode ~1 phút; skeleton
    compile sạch, 665 test cũ xanh trong cùng lần chạy.
  - (A) Test integration `Spoils_EveryOfferedOptionIsClaimable`, `Apply_WinEverySeason_ReachesRunEndWonThroughSpoils`,
    `Apply_SameSeedThroughSpoils_SameHash` đặt ở `RunLoopInvariantTest` / `RunLoopTest` như plan; `WinSeason` thêm tay Sắt + Than
    vào Túi mỗi Mùa rồi nung Thép thật (đối thủ HP 1 / ATK 0).
  - (A) `RunLoopTest.Record` giờ assert lệnh được chấp nhận (chặt hơn; test cũ vẫn xanh).
- **Tradeoffs**:
  - (A) Ở skeleton có 11 test phủ định pass "trống" (không bày lựa chọn nào → không vi phạm): `Enter_EmptyPool_*`,
    `Enter_ZeroWeightOption_*`, `Enter_BagAtMaxCapacity_*`, `Enter_WithoutGuarantee_*`, `Spoils_EveryOfferedOptionIsClaimable`,
    `Apply_AckLoss_GivesNoSpoilsGold`, `IsClaimable_ItemWhenBagFull_*`… — chấp nhận vì sau implement chúng kiểm trên lựa chọn thật
    (các test dương cùng nhóm fail ở skeleton).
- **Test changes** (sửa test sau Bước 2 và lý do):
  - (A) `ContentFactory.CreateSpoilsSeasons(pool)`: bể tùy chỉnh **không** bật đảm bảo Xúc tác (trước: luôn bật Mùa 2–3 →
    validator mới từ chối đúng vì bể tùy chỉnh không có Xúc tác) — 4 test `SpoilsRulesTest` lỗi arrange.
  - (A) `Enter_BagFull_OffersOnlyExpandBag`: Túi 8/8 thay vì 14/14 (14 > trần 12 → Nới Túi không nhận được — lỗi arrange).
  - (A) Bỏ case `"guarantee"` của `Create_LastSeasonDeclaresSpoils_Throws` trước khi chạy (case này fail vì 2 lý do cùng lúc, không phân biệt được).
- **Open questions**: - None
