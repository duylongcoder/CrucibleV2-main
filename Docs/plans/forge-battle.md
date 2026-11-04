# Plan: Rèn, Trận, Uy tín — Dâng → mô phỏng Trận → Resolution (F-010)

> **Trạng thái**: Done (D1–D11 duyệt 2026-11-04; 665/665 EditMode xanh) · **Cập nhật**: 2026-11-04
> **GDD**: `design/run-flow.md` §Chiếu chỉ (loại trang bị), §Dâng, §Rèn & Trận, §Uy tín, §Kết thúc run, §Trạng thái
> & chuyển trạng thái (hàng Offering → Resolution → …), Edge Cases; `design/battle.md` §Luật cốt lõi, §Formulas, Edge
> Cases, AC-1/2/4/5/7; `design/ui-season.md` §B (Dâng), §E (Kết quả); GDD §6.1 AC-2/AC-3 (phần loại trang bị) ·
> **ADR**: ADR-001 (Command/Reducer), ADR-004 (số nguyên + RNG stream), ADR-007 (chống rò rỉ) · **Loại**: Logic (Core)
> Xây trên F-000…F-006 + F-009 phần 1 (486/486 EditMode xanh, commit `f136032`).

## 1. Bối cảnh

Vòng lượt hiện dừng ở `Offering` — không lệnh nào đi tiếp, `Reputation` không bao giờ đổi, Mùa không bao giờ tăng.
`EnterDecree` mới rút S (stream `Market`), chưa rút **loại trang bị** (stream `Forge`) nên Chiếu chỉ chưa có nội dung
chính. Plan này nối nửa sau của một Mùa: Chiếu chỉ rút loại trang bị → **Dâng** 1 thành phẩm đạt chuẩn (hoặc để cận
thần ra về) → **mô phỏng Trận** tất định (lõi `BattleSimulator`, quyết định người dùng 2026-11-04) → `AckBattle` →
**Resolution** đủ bảng chuyển trạng thái (Uy tín, Trục xuất, Thắng run, Mùa kế, vào Spoils). Pillar P3 (Uy tín là áp
lực sống còn; chọn khoáng dâng có hệ quả), P2 (luật trận đọc được, tất định), P1 (đối thủ cố định theo Mùa).

**Yêu cầu** (đánh số để truy vết):
- R1 — **Content Trận/Rèn** (`BattleData`, tham số mới của `ContentDb.Create`): hằng số trận (`HeroBaseHp` 100,
  `HeroBaseAtk` 10, `OffAffinityPercent` 50, `AbsorbCapPercent` 75, `ReflectCapPercent` 50, `MaxRounds` 10), danh
  sách `EquipmentData` (Id, `AtkPerQ`, `HpPerQ`, thuộc tính hợp), `OpponentData` (Id, ★, loại trận, HP, ATK, SB, PD,
  HT), `SeasonData` mỗi Mùa (đối thủ cố định + bể trang bị có trọng số). `DifficultyData.OpponentStatPercent` (mặc
  định 100). `ContentDb` validate (§2 Validate) và tra cứu `GetEquipment` / `TryFindEquipment` / `Season(n)`.
- R2 — **Rút loại trang bị lúc vào Chiếu chỉ**: `SeasonRules.EnterDecree` gọi `ForgeRules.RollEquipment` **sau**
  `RollSeasonPrice` (thứ tự Market → Forge; F-011 thêm Season sau). Đúng **1** lần `NextInt(Σ trọng số)` trên stream
  `Forge`, kể cả bể 1 lựa chọn (D4); chọn theo trọng số tích lũy theo thứ tự khai báo bể; lựa chọn trọng số 0 không
  bao giờ ra. Kết quả lưu `RunState.DecreeEquipment`. Đối thủ của Mùa = `content.Season(run.Season).Opponent` (không
  random, không lưu).
- R3 — **Đạt chuẩn**: `OfferingRules.IsOfferable(ItemData)` = `Kind == Mineral` **và** `Recipe != null` (thành phẩm).
  Nguyên liệu gốc, phế phẩm, phụ gia, Kính, Xúc tác → không (ui-season §B).
- R4 — `Offer(uid)`: chỉ ở `Offering`. Từ chối theo thứ tự: `RunNotActive` → `WrongPhase` → `ItemNotFound` (uid
  không trong Túi) → `ItemNotOfferable` (mới). Hợp lệ: gỡ vật phẩm khỏi Túi (tiêu hao), dựng chỉ số (R6), mô phỏng
  (R7), `LastBattle = log`, `Phase = Battle`. Phát `ItemOffered(season, uid)` + `BattleStarted(season, log)`.
  Không rút RNG.
- R5 — `DismissCourtier`: chỉ ở `Offering` **và** Túi không có món đạt chuẩn (còn → `HasOfferableItem`, mới — D3).
  Hợp lệ: `LastBattle = BattleLog.Forfeit(...)` (Thua, không hiệp), `Phase = Battle`. Phát `CourtierDismissed(season)`
  + `BattleStarted(season, log)`. (run-flow AC-7.)
- R6 — **Chỉ số** (`BattleStats`, battle.md §Formulas): ta `ATK = HeroBaseAtk + ⌊Q × AtkPerQ / 100⌋`, `HP = HeroBaseHp
  + ⌊Q × HpPerQ / 100⌋`, SB/PD/HT = `⌊v × Aff / 100⌋` (Aff 100 nếu thuộc tính nằm trong danh sách hợp của trang bị,
  ngược lại `OffAffinityPercent`). F-010 truyền `v = 0` cho cả 3 (khoáng chưa có dòng — F-008 cấp, D1). Đối thủ: mọi
  chỉ số × `OpponentStatPercent / 100` (floor), HP sau scale ≥ 1 (D10).
- R7 — **Mô phỏng** (`BattleSimulator.Simulate`, không RNG): mỗi hiệp ta đánh trước: `gốc = ⌊ATK × (100 + SB) / 100⌋`,
  `nhận = max(1, ⌊gốc × (100 − min(HTCap, HT_đt)) / 100⌋)`, dội ngược `⌊nhận × min(PDCap, PD_đt) / 100⌋` vào người
  đánh (không bị Hấp thụ, không dội tiếp). Sau đòn ta: có người gục → hết trận (D5); cả hai còn đứng → đối thủ đánh,
  cùng công thức đổi vai. Cả hai gục cùng hiệp → **ta thắng**. Hết `MaxRounds` cả hai đứng → Σ sát thương ta gây (đòn
  + dội ngược) ≥ Σ đối thủ gây → ta thắng. Phép nhân qua `BattleMath.MulDivFloor` (D9, không tràn `long`).
- R8 — **`BattleLog`** (bất biến, `Core/Logs`): `Season`, `Equipment`, `OfferedItem` + `OfferedMagic` (không có khi
  forfeit), chỉ số đầu trận hai bên (`Combatant`), `Rounds` (`BattleRound`: sát thương ta gây, dội ngược ta nhận, đối
  thủ có đánh không, sát thương đối thủ gây, dội ngược đối thủ nhận, HP sau mỗi bên — chặn dưới 0), `HeroTotalDealt`,
  `OpponentTotalDealt`, `EndReason` (`Knockout` / `RoundLimit` / `Forfeit`), `HeroWon`. Không chứa `Quality` hay dòng
  ẩn (ADR-007).
- R9 — `AckBattle`: chỉ ở `Battle` (else `WrongPhase`). Resolution theo run-flow §Trạng thái, **xét Uy tín trước Mùa**:
  1. Thua → `Reputation −1`, phát `ReputationLost(remaining)`; về 0 → `Phase = Ended`, `Outcome = Exiled`,
     `RunEnded(Exiled)`.
  2. Còn Uy tín, `Season == SeasonCount` → `Ended`, `Outcome = Won`, `RunEnded(Won)` (kể cả thua Đại chiến).
  3. Thắng, `Season < SeasonCount` → `Phase = Spoils`, `SpoilsStarted(season)` (lệnh Spoils ở F-011 — D7).
  4. Thua, còn Uy tín, `Season < SeasonCount` → `Season + 1`, `EnterDecree`, `SeasonStarted(season + 1)`.
  `LastBattle` giữ nguyên tới lần Dâng kế (D11).
- R10 — `RunState`: + `DecreeEquipment` (`EquipmentId`), `LastBattle` (`BattleLog`, null), `Outcome` (`RunOutcome`:
  `None` / `Won` / `Exiled`). `StartRun` đặt `LastBattle = null`, `Outcome = None` (trước `EnterDecree`).
- R11 — `StateHasher` ghi `DecreeEquipment`, `Outcome`, `LastBattle` (đủ field R8) sau `Shop`.
- R12 — Bất biến & tất định: lệnh bị từ chối không đổi hash; Trận không đổi trạng thái stream RNG nào (battle AC-1);
  `Reputation` không bao giờ tăng (run-flow AC-5); mỗi Mùa đúng 1 lần `BattleStarted` và 1 `AckBattle` (AC-3);
  `BattleLog.Equipment == DecreeEquipment` cùng Mùa (AC-2); cùng seed → cùng loại trang bị Mùa 1, thêm reroll không đổi
  loại trang bị Mùa kế (GDD §6.1 AC-2/AC-3 phần trang bị).

**Ngoài phạm vi**:
- Dòng thuộc tính SB/PD/HT trên khoáng, Kính soi ở Dâng → **F-008** (chỉ cần truyền `v` vào `BattleStats.ForHero`).
- Lộ thuộc tính trong trận + ghi Sổ (battle AC-6), biểu cảm thợ rèn (`SmithReactionBands`), timeline phát lại 6 nhịp
  (AC-3), lộ ★/loại trận ở nhịp 1, view cutscene/Trận → **F-017** / **F-014**.
- Lệnh Spoils (`ChooseReward`/`SkipReward`), `SpoilsGold`, modifier Mùa (`OpponentStatScale`, trọng số bể) → **F-011**.
- Quy đổi Danh vọng + mở bậc khó khi RunEnd → **F-012**. Xóa save run, tạo run mới sau `Ended`, DTO `DecreeEquipment` /
  `LastBattle` / `Outcome` → **F-013**. Popup Chiếu chỉ / Dâng / Kết quả → **F-014**.
- RandomAgent 1.000 run **tới RunEnd** (run-flow AC-1) → **F-009 phần 2** sau F-011 (Spoils hiện là ngõ cụt — D7).
- `RunViewBuilder` ẩn đối thủ tới `Phase == Battle` (ADR-007) → khi có `RunViewBuilder` (F-014). Core giữ đối thủ dưới
  dạng tra cứu content, không phát qua sự kiện trước `BattleStarted`.

**TBD (không làm)**: chỉ số đối thủ theo Mùa × bậc, ★, `SpoilsGold`, bể trang bị từng Mùa — số liệu ở `balance`; plan
chỉ dùng số tạm trong `ContentFactory` (test).

**Quyết định** (D1–D3 chốt 2026-11-04; D4–D11 duyệt 2026-11-04):
- D1 ✅: `BattleSimulator` lõi làm ở F-010 (F-017 còn lộ thuộc tính, biểu cảm, timeline 6 nhịp, view). SB/PD/HT của ta
  = 0 tới F-008; `BattleStats.ForHero` nhận sẵn giá trị thô để F-008 chỉ cần cấp số.
- D2 ✅: Khay là state view. Core chỉ có `Offer(uid)` + `DismissCourtier` + `AckBattle`; **bỏ** `PutOnTray`/`ClearTray`/
  `OfferingTrayUid` và tham số `nowUnix` của `Offer` (chưa có gì ghi meta theo thời gian). Sửa TECH §3.2/§3.3.
- D3 ✅ (theo "đủ bảng chuyển trạng thái"): `AckBattle` làm đủ Resolution; F-009 phần 2 còn lại fuzz 1.000 run tới RunEnd.
- D4: `RollEquipment` luôn rút đúng 1 `NextInt` kể cả bể 1 lựa chọn — số lần rút không phụ thuộc content (cùng tinh thần
  "tung ma thuật kể cả 0%").
- D5: Ta gục vì dội ngược ở đòn của mình mà đối thủ còn đứng → hết trận, **ta thua**, đối thủ không đánh nữa (battle.md ghi
  "đối thủ còn sống → đánh", không nói khi ta đã gục — làm rõ thành "cả hai còn đứng"). Sửa câu chữ battle.md.
- D6: Run kết thúc giữ `IsActive = true`, `Phase = Ended` + `Outcome`; mọi lệnh run sau đó → `WrongPhase`. Tạo run mới
  là việc của `GameSession` (F-013) bằng `RunState` mới.
- D7: Thắng Mùa < 8 → `Phase = Spoils` chưa có lệnh (ngõ cụt tới F-011). Không cộng vàng (`SpoilsGold` TBD).
- D8: `HeroBaseHp/Atk` đặt trong `BattleData` (Core); SO vẫn có thể để ở `RunDef` — `ContentBaker` gom (battle.md ghi
  "`RunDef`" là nơi designer chỉnh, không ràng buộc record Core).
- D9: Mọi phép `⌊x × n / d⌋` qua `BattleMath.MulDivFloor` = `(x / d) × n + (x % d) × n / d` (chính xác với x, n ≥ 0,
  không tràn khi `Quality ≤ MaxQualityCap`). Giá trị SB/PD/HT (thô và sau scale) chặn validate `[0, MaxStatValue =
  10 000]`.
- D10: Đối thủ sau scale: HP ≥ 1 (tránh "gục từ đầu"), ATK có thể 0 (mỗi đòn vẫn ≥ 1).
- D11: `AckBattle` không xóa `LastBattle` (màn Kết thúc run / Spoils còn dùng); lần Dâng kế ghi đè.

## 2. Thiết kế triển khai

| Class / file (dưới `Assets/_Project/Scripts/`) | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Core/Content/StatKind.cs` | enum `Sharpness = 0, Reflect = 1, Absorb = 2` (F-008 dùng lại cho dòng) | Core | Mới |
| `Core/Content/BattleKind.cs` | enum `Tournament = 0, War = 1, GrandWar = 2` | Core | Mới |
| `Core/Content/EquipmentId.cs` | `readonly struct` bọc chỉ số (như `ItemId`), gán theo Id sắp ordinal | Core | Mới |
| `Core/Content/EquipmentData.cs` | Id, `AtkPerQ`, `HpPerQ`, `Affinities` + `HasAffinity(StatKind)` | Core | Mới |
| `Core/Content/OpponentData.cs` | Id, `Stars`, `Kind`, `Hp`, `Atk`, `Sharpness`, `Reflect`, `Absorb` | Core | Mới |
| `Core/Content/ForgeOptionData.cs` | `(string EquipmentId, int Weight)` | Core | Mới |
| `Core/Content/SeasonData.cs` | `(string OpponentId, IReadOnlyList<ForgeOptionData> ForgePool)` | Core | Mới |
| `Core/Content/ForgeOption.cs` | đã resolve: `(EquipmentId Equipment, int Weight)` | Core | Mới |
| `Core/Content/SeasonContent.cs` | đã resolve: `OpponentData Opponent`, `IReadOnlyList<ForgeOption> ForgePool`, `TotalWeight` | Core | Mới |
| `Core/Content/BattleData.cs` | hằng số trận + `Equipment`, `Opponents`, `Seasons` (dạng chuỗi Id) | Core | Mới |
| `Core/Content/DifficultyData.cs` | + `OpponentStatPercent` (ctor `opponentStatPercent = 100`) | Core | Sửa |
| `Core/Content/ContentDb.cs` | + tham số `BattleData battle`; validate; `Battle`, `GetEquipment`, `TryFindEquipment`, `Season(int)` | Core | Sửa |
| `Core/State/RunOutcome.cs` | enum `None = 0, Won = 1, Exiled = 2` | Core | Mới |
| `Core/State/RunState.cs` | + `DecreeEquipment`, `LastBattle`, `Outcome` | Core | Sửa |
| `Core/State/StateHasher.cs` | ghi 3 field mới + `WriteBattleLog` (R11) | Core | Sửa |
| `Core/Logs/Combatant.cs` | `readonly struct`: `Hp`, `Atk` (long), `Sharpness`, `Reflect`, `Absorb` (int) | Core | Mới |
| `Core/Logs/BattleRound.cs` | `readonly struct` một hiệp (R8) | Core | Mới |
| `Core/Logs/BattleEndReason.cs` | enum `Knockout = 0, RoundLimit = 1, Forfeit = 2` | Core | Mới |
| `Core/Logs/BattleLog.cs` | bất biến (R8); factory `Forfeit(season, equipment, hero, opponent)`; ctor internal cho simulator | Core | Mới |
| `Core/Rules/BattleMath.cs` | `MulDivFloor(long x, long num, long den)` (D9) | Core | Mới |
| `Core/Rules/BattleStats.cs` | `ForHero(...)`, `ForOpponent(...)` (R6) | Core | Mới |
| `Core/Rules/BattleSimulator.cs` | `Simulate(...)` (R7) — thuần, không state, không RNG | Core | Mới |
| `Core/Rules/ForgeRules.cs` | `RollEquipment(run, content)` (R2) | Core | Mới |
| `Core/Rules/OfferingRules.cs` | `IsOfferable`, `HasOfferable`, `Offer`, `DismissCourtier` (R3–R5) | Core | Mới |
| `Core/Rules/SeasonRules.cs` | `EnterDecree` gọi `ForgeRules.RollEquipment`; + `AckBattle` (R9); doc | Core | Sửa |
| `Core/Rules/RunStartRules.cs` | reset `LastBattle`, `Outcome` (R10) | Core | Sửa |
| `Core/Rules/GameRules.cs` | dispatch `Offer`, `DismissCourtier`, `AckBattle` | Core | Sửa |
| `Core/Commands/Offer.cs`, `DismissCourtier.cs`, `AckBattle.cs` | Command | Core | Mới |
| `Core/Commands/ItemOffered.cs`, `CourtierDismissed.cs`, `BattleStarted.cs`, `ReputationLost.cs`, `RunEnded.cs`, `SpoilsStarted.cs`, `SeasonStarted.cs` | sự kiện (R4, R5, R9) | Core | Mới |
| `Core/Commands/RejectReason.cs` | + `ItemNotOfferable = 21`, `HasOfferableItem = 22` | Core | Sửa |
| `Tests/EditMode/TestUtils/ContentFactory.cs` | + `CreateBattle(...)` (4 trang bị, 8 đối thủ, 8 Mùa), tham số `battle` | Tests | Sửa |
| `Tests/EditMode/TestUtils/StateFactory.cs` | + `CreateOfferingRun(content, bagItemIds)`, `CreateBattleRun(...)` | Tests | Sửa |

**Public / internal API**
```csharp
namespace Crucible.Core.Content {
    public sealed class EquipmentData {
        public EquipmentData(string id, int atkPerQ, int hpPerQ, IReadOnlyList<StatKind> affinities);
        public bool HasAffinity(StatKind stat);
    }
    public sealed class OpponentData {
        public OpponentData(string id, int stars, BattleKind kind, int hp, int atk, int sharpness, int reflect, int absorb);
    }
    public sealed class SeasonData { public SeasonData(string opponentId, IReadOnlyList<ForgeOptionData> forgePool); }
    public sealed class BattleData {
        public const int MaxStatValue = 10_000;
        public BattleData(int heroBaseHp, int heroBaseAtk, int offAffinityPercent, int absorbCapPercent,
                          int reflectCapPercent, int maxRounds, IReadOnlyList<EquipmentData> equipment,
                          IReadOnlyList<OpponentData> opponents, IReadOnlyList<SeasonData> seasons);
    }
    // ContentDb.Create(items, difficulties, kits, run, brewing, market, battle)
    public BattleData Battle { get; }
    public EquipmentData GetEquipment(EquipmentId id);
    public bool TryFindEquipment(string id, out EquipmentId equipmentId);
    public SeasonContent Season(int season);   // 1…SeasonCount; ngoài khoảng → ArgumentOutOfRangeException
}
namespace Crucible.Core.Rules {
    public static class ForgeRules   { public static void RollEquipment(RunState run, ContentDb content); }
    public static class BattleStats {
        public static Combatant ForHero(long quality, int sharpness, int reflect, int absorb, EquipmentData equipment, BattleData data);
        public static Combatant ForOpponent(OpponentData opponent, int statPercent);
    }
    public static class BattleSimulator {
        public static BattleLog Simulate(int season, EquipmentId equipment, ItemInstance offered,
                                         Combatant hero, Combatant opponent, BattleData data);
    }
    public static class OfferingRules {
        public static bool IsOfferable(ItemData item);
        public static bool HasOfferable(RunState run, ContentDb content);
        public static CommandResult Offer(RunState run, ContentDb content, Offer command);
        public static CommandResult DismissCourtier(RunState run, ContentDb content, DismissCourtier command);
    }
    // SeasonRules + public static CommandResult AckBattle(RunState run, ContentDb content, AckBattle command);
}
// Commands: Offer(int uid) · DismissCourtier · AckBattle
```

**Luồng**
```
EnterDecree: Phase=Decree, Turn=0, RollSeasonPrice (Market) → RollEquipment (Forge) → [F-011: Season]
[Offering] ──Offer(uid)──────▶ Túi −uid; ForHero(Q, 0,0,0) × ForOpponent(Season(s).Opponent, diff%) → Simulate → [Battle]
[Offering] ──DismissCourtier─▶ (không món đạt chuẩn) BattleLog.Forfeit → [Battle]
[Battle]   ──AckBattle───────▶ thua? Rep−1 → Rep 0 ? [Ended·Exiled]
                               : s == N ? [Ended·Won] : thắng ? [Spoils] : Season+1, EnterDecree → [Decree]
```

**Validate (`ContentDb.Create`, ném `ArgumentException`/`ArgumentNullException`)**: `battle` null; `HeroBaseHp`,
`HeroBaseAtk` ≥ 1; `OffAffinityPercent`, `AbsorbCapPercent`, `ReflectCapPercent` ∈ [0, 100]; `MaxRounds` ≥ 1; trang bị:
Id rỗng/trùng, `AtkPerQ`/`HpPerQ` ∈ [0, 100], `Affinities` null hoặc trùng; đối thủ: Id rỗng/trùng, ★ ∈ [1, 5], HP ≥ 1,
ATK ≥ 0, SB/PD/HT ∈ [0, `MaxStatValue`]; `Seasons.Count == Run.SeasonCount`; mỗi Mùa: đối thủ có thật, bể ≥ 1 lựa chọn,
trang bị có thật và không trùng trong bể, trọng số ≥ 0, Σ ∈ [1, `int.MaxValue`]; `DifficultyData.OpponentStatPercent` ∈
[1, 1000]. Tra cứu Equipment trong `Create` dùng `Dictionary` chỉ để resolve (không lặp — TECH §3.3).

**Interface phụ thuộc**: không (luật static; RNG qua `run.Rng.Get(RngStream.Forge)`; Trận không nhận RNG).
**Dữ liệu**: chưa có SO/`ContentBaker` (Content trống) — `EquipmentDef`/`OpponentDef`/`RunDef.Seasons` + baker để sau;
test dùng `ContentFactory.CreateBattle`.
**Scene / prefab bị chạm**: không.
**Lý do thiết kế**: tách `BattleStats` (quy đổi) khỏi `BattleSimulator` (luật hiệp) để test ví dụ battle.md bằng chỉ số
trực tiếp, độc lập với F-008; `OfferingRules` riêng (Dâng là màn riêng), `AckBattle` ở `SeasonRules` vì nó điều khiển
chuyển Mùa (cùng chỗ `EnterDecree`); một đường Resolution duy nhất cho cả Dâng lẫn "ra về" nhờ `BattleLog.Forfeit`.

## 3. Test cases

### Editor tests
(none)

### Unit tests

#### `BattleMathTest`
| Test method | Kiểm tra |
|---|---|
| `MulDivFloor_SmallValues_MatchesFloorOfProduct` (x, n, d: {(40,50,100), (37,33,100), (0,80,100), (99,1,100)}) | = ⌊x × n / d⌋ |
| `MulDivFloor_QualityAtMaxCap_DoesNotOverflow` | kết quả > 0 và = `(x/100)×n + (x%100)×n/100` |

#### `BattleStatsTest`
| Test method | Kiểm tra |
|---|---|
| `ForHero_SwordQuality40_ReturnsFormulaExample` | ATK 30, HP 108 (battle.md ví dụ) |
| `ForHero_QualityZero_ReturnsBaseStats` | ATK = `HeroBaseAtk`, HP = `HeroBaseHp` |
| `ForHero_StatMatchesAffinity_KeepsFullValue` | SB 20 → 20 với Kiếm |
| `ForHero_StatOffAffinity_AppliesOffAffinityPercent` (v: {20, 21}) | ⌊v × 50 / 100⌋ (10, 10) |
| `ForHero_ZeroStatValues_ReturnsZeroSecondaryStats` | SB = PD = HT = 0 |
| `ForOpponent_Percent100_ReturnsDefStats` | đúng 5 chỉ số của def |
| `ForOpponent_Percent150_ScalesAllStatsWithFloor` | mỗi chỉ số = ⌊v × 150 / 100⌋ |
| `ForOpponent_ScaledHpBelowOne_ClampsHpToOne` | HP = 1 |

#### `BattleSimulatorTest`
| Test method | Kiểm tra |
|---|---|
| `Simulate_FormulaExample_HeroWinsInRoundFive` | `HeroWon`, `Knockout`, 5 hiệp, đòn ta 32, đòn đối thủ 20 (battle AC-2) |
| `Simulate_FormulaExample_RecordsHpAfterEachRound` | HP sau hiệp 4: ta 28, đối thủ 22 |
| `Simulate_HeroKnocksOutInRoundOne_OpponentDoesNotAttack` | 1 hiệp, `OpponentAttacked` false |
| `Simulate_OpponentKnocksOutHero_HeroLoses` | `HeroWon` false, `Knockout` |
| `Simulate_BothFallFromReflectInOneRound_HeroWins` | `HeroWon` (battle AC-5) |
| `Simulate_HeroFallsFromReflectOpponentStands_HeroLosesWithoutCounter` | thua, hiệp cuối `OpponentAttacked` false (D5) |
| `Simulate_RoundLimitHeroDealtMore_HeroWins` | `RoundLimit`, `MaxRounds` hiệp, thắng |
| `Simulate_RoundLimitOpponentDealtMore_HeroLoses` | `RoundLimit`, thua |
| `Simulate_RoundLimitEqualTotals_HeroWins` | thắng (battle AC-4) |
| `Simulate_TotalsIncludeReflectDamage` | `HeroTotalDealt` = Σ đòn ta + Σ dội ngược vào đối thủ |
| `Simulate_AbsorbAboveCap_ClampedToCap` | sát thương = ⌊gốc × 25 / 100⌋ khi HT 90 (battle AC-7) |
| `Simulate_ReflectAboveCap_ClampedToCap` | dội ngược = ⌊nhận × 50 / 100⌋ khi PD 80 |
| `Simulate_DamageWouldRoundToZero_DealsOne` (ATK: {0, 1}) | mỗi đòn = 1 |
| `Simulate_SharpnessIncreasesBaseHit` | đòn = ⌊ATK × (100 + SB) / 100⌋ |
| `Simulate_ReflectDamageIsNotAbsorbedNorReflected` | dội ngược không đổi theo HT người đánh; không có dội lần hai |
| `Simulate_HpAfterIsNeverNegative` | mọi `HpAfter` ≥ 0 |
| `Simulate_SameInputs_ReturnEqualLogs` (1.000 bộ chỉ số `System.Random` cố định seed) | hai lần chạy cùng chuỗi hiệp + kết quả (battle AC-1) |

#### `BattleLogTest`
| Test method | Kiểm tra |
|---|---|
| `Forfeit_Always_IsLossWithNoRounds` | `HeroWon` false, `Forfeit`, 0 hiệp, không có vật phẩm dâng |
| `BattleLog_HasNoQualityOrHiddenLineMembers` (reflection) | không property tên chứa `Quality`/`Line`/`Recipe` (ADR-007) |

#### `ContentDbBattleTest`
| Test method | Kiểm tra |
|---|---|
| `Create_ValidBattle_ResolvesSeasonsInOrder` | `Season(1..8).Opponent.Id` đúng thứ tự khai báo |
| `Create_ValidBattle_EquipmentIdsSortedOrdinal` | `TryFindEquipment` → chỉ số theo thứ tự sắp |
| `Season_OutOfRange_Throws` (s: {0, 9}) | `ArgumentOutOfRangeException` |
| `Create_NullBattle_Throws` | `ArgumentNullException` |
| `Create_InvalidBattleConstant_Throws` (case: HeroBaseHp 0, HeroBaseAtk 0, OffAffinity 101, AbsorbCap −1, ReflectCap 101, MaxRounds 0) | `ArgumentException` |
| `Create_InvalidEquipment_Throws` (case: Id rỗng, trùng Id, AtkPerQ 101, HpPerQ −1, Affinities null, Affinity trùng) | `ArgumentException` |
| `Create_InvalidOpponent_Throws` (case: trùng Id, Stars 0, Stars 6, Hp 0, Atk −1, Sharpness 10 001) | `ArgumentException` |
| `Create_SeasonCountMismatch_Throws` (count: {7, 9}) | `ArgumentException` |
| `Create_InvalidSeason_Throws` (case: đối thủ lạ, bể rỗng, trang bị lạ, trang bị trùng trong bể, trọng số âm, Σ = 0) | `ArgumentException` |
| `Create_DifficultyStatPercentOutOfRange_Throws` (p: {0, 1001}) | `ArgumentException` |
| `Create_BoundaryValues_Succeeds` (OffAffinity 0/100, caps 0/100, MaxRounds 1, Stars 1/5, Atk 0, stat 10 000, percent 1/1000) | không ném |

#### `ForgeRulesTest`
| Test method | Kiểm tra |
|---|---|
| `RollEquipment_SingleOptionPool_PicksIt` | `DecreeEquipment` = lựa chọn duy nhất |
| `RollEquipment_SingleOptionPool_StillAdvancesForgeOnce` | trạng thái `Forge` = sau đúng 1 `NextInt` (D4) |
| `RollEquipment_ZeroWeightOption_NeverPicked` (seed 0–999) | không lần nào ra lựa chọn trọng số 0 |
| `RollEquipment_Weights_PickByCumulativeWeight` | lấy `NextInt` dự đoán từ bản sao stream → đúng lựa chọn tích lũy |
| `RollEquipment_DoesNotTouchOtherStreams` | 6 stream khác giữ nguyên |
| `RollEquipment_UsesPoolOfCurrentSeason` | Mùa 2 với bể khác Mùa 1 → kết quả thuộc bể Mùa 2 |

#### `OfferingRulesTest`
| Test method | Kiểm tra |
|---|---|
| `IsOfferable_ProductMineral_ReturnsTrue` (Steel, CastIron, Starsteel) | true |
| `IsOfferable_NonProduct_ReturnsFalse` (Iron, Silver, Ash, Lens, Bellows, Scrap) | false |
| `HasOfferable_BagWithProduct_ReturnsTrue` | true |
| `HasOfferable_BagWithoutProduct_ReturnsFalse` | false |

### Integration tests (qua `GameRules.Apply`)

#### `GameRulesOfferTest`
| Test method | Kiểm tra |
|---|---|
| `Apply_OfferProduct_RemovesItemFromBag` | uid không còn trong Túi, Túi −1 |
| `Apply_OfferProduct_EntersBattleWithLog` | `Phase = Battle`, `LastBattle` ≠ null, `EndReason` ≠ `Forfeit` |
| `Apply_OfferProduct_LogUsesDecreeEquipment` | `LastBattle.Equipment == DecreeEquipment` (run-flow AC-2) |
| `Apply_OfferProduct_LogMatchesDirectSimulation` | log trùng `Simulate(ForHero(Q,0,0,0), ForOpponent(đối thủ Mùa, diff%))` |
| `Apply_OfferProduct_EmitsItemOfferedThenBattleStarted` | 2 sự kiện, đúng thứ tự, đúng season/uid |
| `Apply_OfferProduct_DoesNotAdvanceAnyRngStream` | 7 stream không đổi (battle AC-1) |
| `Apply_OfferMagicProduct_LogCarriesMagicFlag` | `OfferedMagic` true |
| `Apply_OfferHarderDifficulty_ScalesOpponent` | bậc `OpponentStatPercent` 200 → HP đối thủ đầu trận gấp đôi |
| `Apply_Offer_RejectsWhenNotActive` | `RunNotActive`, hash không đổi |
| `Apply_Offer_RejectsOutsideOffering` (phase: Decree, Workbench, Battle, Spoils, Ended) | `WrongPhase`, hash không đổi |
| `Apply_OfferUnknownUid_RejectsItemNotFound` | `ItemNotFound`, hash không đổi |
| `Apply_OfferNonProduct_RejectsItemNotOfferable` (Iron, Scrap, Lens, Bellows) | `ItemNotOfferable`, hash không đổi |

#### `GameRulesDismissCourtierTest`
| Test method | Kiểm tra |
|---|---|
| `Apply_DismissWithNoProduct_EntersBattleWithForfeitLog` | `Phase = Battle`, `Forfeit`, `HeroWon` false (run-flow AC-7) |
| `Apply_DismissWithNoProduct_KeepsBag` | Túi không đổi |
| `Apply_DismissWithNoProduct_EmitsCourtierDismissedThenBattleStarted` | 2 sự kiện đúng thứ tự |
| `Apply_DismissWithProduct_RejectsHasOfferableItem` | `HasOfferableItem`, hash không đổi |
| `Apply_Dismiss_RejectsOutsideOffering` (phase: Decree, Workbench, Battle) | `WrongPhase` |

#### `GameRulesAckBattleTest`
| Test method | Kiểm tra |
|---|---|
| `Apply_AckLossWithReputationLeft_LosesOneReputation` | `Reputation` 3 → 2, `ReputationLost(2)` |
| `Apply_AckLossWithReputationLeft_EntersNextSeasonDecree` | `Season` + 1, `Phase = Decree`, `Turn` 0, `SeasonStarted(s+1)` |
| `Apply_AckLossWithReputationLeft_RollsNextSeasonDecree` | `SeasonPricePercent` và `DecreeEquipment` = rút từ bản sao stream (Market rồi Forge) |
| `Apply_AckLossAtReputationOne_ExilesRun` | `Ended`, `Exiled`, `Reputation` 0, `RunEnded(Exiled)` (run-flow AC-5) |
| `Apply_AckLossInLastSeasonAtReputationOne_Exiles` | `Exiled`, không `Won` (Edge Case Mùa 8) |
| `Apply_AckLossInLastSeasonWithReputationLeft_WinsRun` | `Ended`, `Won` |
| `Apply_AckWinInLastSeason_WinsRun` | `Ended`, `Won`, `Reputation` không đổi |
| `Apply_AckWinBeforeLastSeason_EntersSpoils` | `Phase = Spoils`, `Season` không đổi, `SpoilsStarted(s)` |
| `Apply_AckWin_KeepsReputation` | `Reputation` không đổi, không `ReputationLost` |
| `Apply_AckForfeit_CountsAsLoss` | `Reputation` −1 |
| `Apply_Ack_KeepsLastBattle` | `LastBattle` cùng tham chiếu (D11) |
| `Apply_Ack_RejectsOutsideBattle` (phase: Decree, Workbench, Offering, Spoils, Ended) | `WrongPhase`, hash không đổi |
| `Apply_AnyRunCommandAfterEnded_RejectsWrongPhase` (AcceptDecree, SkipTurn, Offer, DismissCourtier, AckBattle) | `WrongPhase` (D6) |

#### Sửa test có sẵn
| Test | Sửa |
|---|---|
| `SeasonRulesEnterDecreeTest` | + `EnterDecree_Always_RollsEquipmentAfterSeasonPrice` (Market rồi Forge, từ bản sao stream); + `EnterDecree_DoesNotTouchShopChanceInspectSeasonReward` |
| `GameRulesStartRunTest` | + `Apply_StartRun_RollsSeasonOneEquipment`; + `Apply_StartRun_ResetsLastBattleAndOutcome` |
| `StateHasherTest.Hash_StatesDifferInOneField_*` | + case `DecreeEquipment`, `Outcome`, `LastBattle` null/khác, một field `BattleRound` khác, `EndReason` khác |
| `ContentDb*Test` (mọi chỗ gọi `ContentDb.Create`) | đi qua `ContentFactory` / truyền `CreateBattle()` |

#### `RunLoopTest` (mở rộng — chạy lệnh thật từ `StartRun`)
| Test method | Kiểm tra |
|---|---|
| `Apply_SameSeed_SameSeasonOneEquipment` | hai run cùng seed → cùng `DecreeEquipment` (GDD §6.1 AC-2) |
| `Apply_ExtraRerollInSeasonOne_SameSeasonTwoEquipment` | thêm 1 `Reroll` ở Mùa 1, cùng chuỗi còn lại, thua → Mùa 2 cùng `DecreeEquipment` (GDD §6.1 AC-3) |
| `Apply_FullSeasonWithOffer_ReachesSeasonTwoOrSpoils` | Decree → 3 lượt (Nung Thép lượt 1) → Offer Thép → Ack → Mùa 2 Decree (thua) hoặc Spoils (thắng) theo `LastBattle.HeroWon` |
| `Apply_SkipAllTurnsAndDismissThreeSeasons_ExilesInSeasonThree` | Bỏ qua + ra về × 3 Mùa → `Exiled` ở Mùa 3 |

#### `RunLoopInvariantTest` (mở rộng fuzz — sinh thêm `Offer(uid ngẫu nhiên)`, `DismissCourtier`, `AckBattle`)
| Test method | Kiểm tra |
|---|---|
| `Apply_RandomCommandsFromStartRun_KeepsLoopInvariants` (seed 1–5, chạy tới `Spoils`/`Ended`/hết bước) | lệnh bị từ chối không đổi hash; `Reputation` không tăng; ≤ 1 `BattleStarted` và ≤ 1 `AckBattle` mỗi Mùa; `BattleLog.Equipment == DecreeEquipment`; `Turn` đúng R7 run-loop |
| `Apply_RandomCommandsFromStartRun_AlwaysReachesSpoilsOrEnded` (seed 0–99) | mọi run tới `Spoils` hoặc `Ended` (thay "AlwaysReachesOffering") |

### Visual verification tests
(none — không có view)

### Manual tests
(none)

**Truy vết**:
R1 → `ContentDbBattleTest`; R2 → `ForgeRulesTest`, `SeasonRulesEnterDecreeTest`, `GameRulesStartRunTest`,
`Apply_AckLossWithReputationLeft_RollsNextSeasonDecree`; R3 → `OfferingRulesTest`; R4 → `GameRulesOfferTest`;
R5 → `GameRulesDismissCourtierTest`; R6 → `BattleStatsTest`, `Apply_OfferHarderDifficulty_ScalesOpponent`;
R7 → `BattleSimulatorTest`, `BattleMathTest`; R8 → `BattleLogTest`, `Simulate_FormulaExample_*`,
`Apply_OfferProduct_LogMatchesDirectSimulation`; R9 → `GameRulesAckBattleTest`, `RunLoopTest`; R10 →
`GameRulesStartRunTest`, `Apply_Ack_KeepsLastBattle`; R11 → `StateHasherTest`; R12 → `RunLoopInvariantTest`,
`RunLoopTest` (2 test seed), `Apply_OfferProduct_DoesNotAdvanceAnyRngStream`, `Simulate_SameInputs_ReturnEqualLogs`.
Run-flow AC-2/3/4/5/7 và battle AC-1/2/4/5/7 đều có test; battle AC-3/6/8, run-flow AC-1/6* ngoài phạm vi (§1).

**Testability**: **PASS** — luật static thuần, chỉ số trận nhập trực tiếp (`Combatant`) không cần F-008; RNG dự đoán
được bằng bản sao `Pcg32`; mọi chuyển trạng thái tới được bằng `StateFactory` dựng thẳng `Phase` + `LastBattle`.
**WARN nhẹ**: `ContentDb.Create` thêm tham số → mọi test dựng content phải đi qua `ContentFactory` (đã gần như vậy;
Bước 1 sửa chỗ còn gọi thẳng).

## 4. Các bước thực hiện

> Commit theo `CLAUDE.md` §Git (tự động: không — chỉ đề xuất message; test-first: không — gộp test vào commit xanh).
> Lệnh test (Editor đóng): `"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode
> -testResults Logs/editmode.xml -logFile -` (Linux: `UNITY="$HOME/Unity/Hub/Editor/6000.6.7f1/Editor/Unity"`).

- [x] **Bước 1 — Skeleton**: mọi type §2 (thân rỗng / trả mặc định; `Simulate` trả `BattleLog.Forfeit`), `ContentDb.Create`
  thêm `battle` (chỉ null-check), `ContentFactory.CreateBattle` + `StateFactory` helper, sửa mọi chỗ gọi `Create`.
  Kiểm chứng: compile sạch, **486/486** xanh (không đổi hành vi).
- [x] **Bước 2 — Test first**: viết test §3 + sửa 4 nhóm test cũ. Kiểm chứng: test mới fail **đúng lý do** (assert,
  không exception ngoài dự kiến); dự đoán pass sẵn: test `Reject*` của `WrongPhase` cho `Offer`/`AckBattle` nếu skeleton
  đã trả từ chối mặc định — ghi lại test nào pass và vì sao (§6).
- [x] **Bước 3 — Implement** theo thứ tự: `BattleMath` → `ContentDb` validate/resolve → `BattleStats` → `BattleSimulator` +
  `BattleLog` → `ForgeRules` + `EnterDecree` → `StateHasher` → `OfferingRules` → `AckBattle` → dispatch `GameRules` →
  fuzz. Kiểm chứng: toàn bộ EditMode xanh; 0 warning CS trong log; `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .` 0 lỗi.
- [x] **Bước 4 — Refactor**: gộp `Reject*` theo phase thành `[TestCase]`; helper "chạy tới Offering Mùa s" dùng chung
  `RunLoopTest`/fuzz; kiểm `BattleSimulator` không alloc thừa (một `BattleRound[]` theo số hiệp thật). Kiểm chứng: vẫn xanh.
- [x] **Bước 5 — Tài liệu**: PLAN F-010 → Done (+ ghi chú F-017 còn gì), F-009 phần 2 thu hẹp (fuzz 1.000 run sau F-011),
  F-013 DTO thêm `DecreeEquipment`/`LastBattle`/`Outcome`; TECH §3.1 (`BattleData`…), §3.2 (bỏ `OfferingTrayUid`, thêm
  `Outcome`), §3.3 (bỏ `PutOnTray`/`ClearTray`, `Offer(uid)`), §3.5 (`BattleLog` thực tế); battle.md câu D5; run-flow
  §Dâng ghi khay là view; LOG checkpoint + session.

## 5. Rủi ro & đánh đổi

- **Spoils ngõ cụt** (D7): run thắng trận trước Mùa 8 kẹt ở `Spoils` tới F-011 → fuzz chỉ kiểm tới `Spoils`/`Ended`;
  AC-1 1.000 run trọn chờ F-009 phần 2.
- **SB/PD/HT của ta = 0** tới F-008: kết quả Trận hiện chỉ phụ thuộc `Quality` × loại trang bị → mọi số cân bằng đo bây
  giờ là tạm; không chạy `balance` Trận trước F-008.
- **Thay đổi chữ ký `ContentDb.Create`**: chạm nhiều test (Bước 1 xử lý tập trung qua `ContentFactory`).
- **Tràn số**: dựa vào `MaxQualityCap` + `MaxStatValue`; nếu F-008/F-007 cho SB vượt 10 000 phải nới `MulDivFloor` hoặc trần.
- **Lệch tài liệu**: TECH liệt kê `PutOnTray`/`ClearTray`/`OfferingTrayUid`/`Offer(nowUnix)` — sửa ở Bước 5 theo D2.
- **Thứ tự rút RNG ở Decree** (Market → Forge) là hợp đồng replay; F-011 phải chèn `Season` **sau** Forge.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - `ContentFactory.CreateBattle` (test): 4 trang bị theo bảng battle.md; đối thủ Mùa s = `opp_s` (HP 40 + 20s, ATK 5 + 2s,
    Mùa 4 & 8 thêm SB/PD/HT 5); bể Mùa s = 4 trang bị trọng số 40/30/20/10 xoay vòng. `ContentFactory.Create` thêm tham số
    `battle`, `opponentStatPercent`.
  - `DismissCourtier` ghi `BattleLog.Forfeit` với chỉ số đầu trận = ta `ForHero(Q 0)` + đối thủ đã scale (view Kết quả vẫn
    có tên/chỉ số đối thủ nếu cần).
  - Bậc khó không tìm thấy (`DifficultyId` lạ) → hệ số 100 (`StartRun` đã chặn Id lạ, nên chỉ là phòng thủ).
  - `AckBattle` coi `LastBattle == null` là Thua (không tới được qua luật).
  - `ContentDb`: Id trang bị/đối thủ rỗng hoặc trùng dùng chung `AddUnique`; phần tử null trong danh sách → `ArgumentException`.
  - `BattleSimulator` gom hiệp vào `List` rồi `ToArray` (mảng đúng số hiệp thật; Command không ở vòng nóng).
- **Deviations**:
  - Giữ hai fuzz cũ của `RunLoopInvariantTest` (vẫn đúng: dừng ở Offering) và **thêm** `Apply_RandomCommandsThroughBattle_*`
    + `Apply_RandomCommandsFromStartRun_AlwaysReachesSpoilsOrEnded` (plan ghi "thay" `AlwaysReachesOffering`).
  - Thêm ngoài plan: `BattleMathTest` case (150,150,100); `BattleStatsTest.ForHero_QualityAtMaxCap_DoesNotOverflow`;
    `BattleSimulatorTest.Simulate_Always_RecordsInputsInLog`; `BattleLogTest.Forfeit_Always_KeepsSeasonAndEquipment`;
    `ContentDbBattleTest` `ResolvesForgePoolInDeclaredOrder`, `TryFindEquipment_UnknownId_ReturnsFalse`,
    `Create_ZeroWeightOptionAlongsidePositive_Succeeds`, case `null pool`/`total overflow`; `GameRulesDismissCourtierTest`
    `Apply_DismissWithEmptyBag_IsAccepted`, `Apply_Dismiss_RejectsWhenNotActive`; `GameRulesAckBattleTest.Apply_Ack_RejectsWhenNotActive`.
  - `StateHasherTest`: thêm `Hash_LastBattleDiffersInOneField_*` (6 case qua `Simulate`) thay cho "một field `BattleRound`
    khác / `EndReason` khác" — `BattleLog` ctor là `internal`, chỉ dựng được qua `Simulate`/`Forfeit`.
  - Bước 4: `Reject*` đã viết dạng `[TestCase]` từ Bước 2; không cần helper "chạy tới Offering Mùa s" riêng (dùng
    `StateFactory.CreateOfferingRun`) — chỉ gộp `RunLoopTest.StartAndAccept` dùng `StartRun`.
- **Tradeoffs**: `RunState.IsActive` giữ `true` khi `Ended` (D6) → `StartRun` trên cùng `RunState` bị `RunAlreadyActive`;
  F-013 `GameSession` phải tạo `RunState` mới.
- **Test changes** (Bước 2, trước khi có commit — chính sách test-first: không):
  - Skeleton: 35 test fail bằng `NullReferenceException` vì skeleton cố ý trả `null` (`BattleLog.Forfeit`, `ContentDb.Season`)
    — không phải lỗi setup; còn lại fail bằng assertion.
  - Pass ở skeleton (đã dự đoán): biên `MulDivFloor` = 0, `ForHero_ZeroStatValues`, test reflection ADR-007, `Create_NullBattle_Throws`
    (null-check có ở skeleton), 2 test "hợp lệ không ném", `TryFindEquipment` lạ, `IsOfferable`/`HasOfferable` = false.
  - Pass sai → đã sửa: `RollEquipment_SingleOptionPool_PicksIt` (Giáp = chỉ số 0 trùng mặc định → đổi sang Kiếm, so
    `EquipmentId(3)`), `RollEquipment_UsesPoolOfCurrentSeason` (so `EquipmentId(1)` thay vì tra cứu skeleton);
    thêm assert `Accepted` cho `Apply_OfferProduct_DoesNotAdvanceAnyRngStream`, `Apply_DismissWithNoProduct_KeepsBag`,
    `Apply_AckWin_KeepsReputation`, `Apply_Ack_KeepsLastBattle` (lệnh bị từ chối làm chúng pass).
  - Sửa test cũ: `SeasonRulesEnterDecreeTest.EnterDecree_OnlyAdvancesMarketStream` → `EnterDecree_OnlyAdvancesMarketAndForgeStreams`
    (R2 cố ý rút thêm `Forge`); 7 chỗ gọi thẳng `ContentDb.Create` thêm `CreateBattle()`.
- **Open questions**: - None
