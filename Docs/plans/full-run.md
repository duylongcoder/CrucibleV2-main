# Plan: Trọn run — RandomAgent 1.000 run tới RunEnd (F-009, phần 2 "full-run")

> **Trạng thái**: Done (D1–D6 duyệt khi gọi `build` 2026-11-04; 1024/1024 EditMode xanh) · **Cập nhật**: 2026-11-04
> **GDD**: `design/run-flow.md` AC-1, AC-2, AC-3, AC-5, AC-7, §Trạng thái & chuyển trạng thái; GDD §6.1 AC-1 (Editor),
> AC-3 (phần loại trang bị + chiến lợi phẩm); `design/economy.md` AC-1, AC-4 · **ADR**: ADR-001 (Command/Reducer),
> ADR-004 (RNG stream) · **Loại**: Logic (test-only)
> Xây trên F-000…F-008, F-009 phần 1, F-010, F-011 (997/997 EditMode xanh, commit `b5fe0fd`).

## 1. Bối cảnh

Toàn bộ máy trạng thái run đã có luật: Decree → Workbench ×3 → Offering → Battle → (Spoils) → Decree Mùa kế / Ended
(F-009 phần 1, F-010, F-011). Nhưng chưa có kiểm chứng **trọn một run**: fuzz hiện có (`RunLoopInvariantTest`) dừng ở
Offering hoặc ở Spoils/Ended của **Mùa 1**, sinh lệnh mù (phần lớn bị từ chối) nên không đi xa được, và chạy trên content
không có Chiến lợi phẩm + modifier + Xúc tác cùng lúc. Plan này thêm **RandomAgent** (người chơi ngẫu nhiên biết luật ở
mức "lệnh có khả năng hợp lệ") và fuzz 1.000 run trên content đủ hệ thống, kiểm bất biến sau mỗi lệnh. Pillar P2 (tất
định), P3 (áp lực tài nguyên không được tạo ngõ cụt). Dự kiến **không đổi code production**; lỗi tìm ra → `bug`.

**Yêu cầu** (đánh số để truy vết):
- R1 — **RandomAgent** (test util): `Next(run, content)` trả 1 `ICommand` theo phase/bước hiện tại, **không đổi** `run`;
  tất định theo seed của agent. Trọng số mỗi phase có ≥ 1 lệnh tiến (vd Brew/SkipTurn ở Editing) để run luôn tiến.
  Có tỉ lệ nhiễu `NoisePercent` (mặc định 10) sinh lệnh bất kỳ với tham số sai để phủ nhánh từ chối.
- R2 — **Gợi ý công thức** (`RecipeHintPercent`, mặc định 25): ở Editing lưới trống, nếu Túi đủ nguyên liệu cho một
  `Recipe` (kể cả `RequiresMagic`) → xếp lần lượt từ ô 0 theo `Pattern` (Gap = để trống) rồi Nung. Không có gợi ý thì
  gần như không bao giờ làm ra khoáng đạt chuẩn → mọi run tự Thua và nhánh Thắng / Spoils / Mùa 8 không được phủ.
- R3 — **Content trọn run** (`ContentFactory.CreateFullRun(opponentStatPercent)`): 8 Mùa, Chiến lợi phẩm Mùa 1–7 (bể mặc
  định + đảm bảo Xúc tác Mùa 2–3), modifier Mùa 2–8 (5 modifier mặc định, trọng số 1 + "không modifier" 1), 3 Xúc tác có
  Effect phủ các trigger (`OnRecipeMatch`, `OnShopRefresh` + `Chance`, `OnBrewFail` + `CreateIngredient`), dòng thuộc
  tính Thép. Hai hồ sơ: **chuẩn** (đối thủ 100 %) và **dễ** (đối thủ 1 % → phủ nhánh Thắng tới Mùa 8).
- R4 — **AC-1 run-flow**: 1.000 run (500 seed × 2 hồ sơ), mọi run tới `Phase == Ended` trong `MaxCommandsPerRun`
  (5.000) lệnh.
- R5 — **Bất biến sau mỗi lệnh** (cả lệnh nhiễu):
  - I1 bị từ chối → `StateHasher` không đổi;
  - I2 `Turn` ∈ [1, `TurnsPerSeason`] khi Workbench, 0 nơi khác;
  - I3 `UsedSlots ≤ BagCapacity ≤ MaxBagCapacity`; số Xúc tác ≤ `CatalystSlots`;
  - I4 `Gold ≥ 0`; mọi ô Chợ còn hàng: giá mua ≥ 1, tier ≤ `ShopMaxTier`, không có `Recipe`; mọi vật phẩm Túi: giá bán
    ≥ 0 (economy AC-1, AC-4);
  - I5 `Reputation` không tăng (AC-5), ∈ [0, `StartingReputation`]; `Reputation == 0` ⇒ `Ended·Exiled` **ngay trong cùng
    lệnh** (AC-5);
  - I6 `Season` không giảm, mỗi lệnh tăng tối đa 1, ∈ [1, `SeasonCount`]; tăng chỉ qua `SeasonStarted`;
  - I7 `Phase == Spoils` ⇒ `LastBattle.HeroWon` và `Season < SeasonCount`.
- R6 — **Bất biến theo Mùa** (xét khi Mùa đóng = `SeasonStarted` hoặc `RunEnded`):
  - đúng 1 `BattleStarted` và đúng 1 `AckBattle` được nhận (AC-3); ≤ 1 `ItemOffered`, và uid dâng biến khỏi Túi (0–1
    khoáng tiêu hao — AC-3);
  - `BattleStarted.Log.Equipment` = `DecreeEquipment` đã rút lúc vào Decree Mùa đó (AC-2), `DecreeEquipment` không đổi
    trong Mùa;
  - `DismissCourtier` được nhận ⇒ trận Mùa đó Thua (AC-7);
  - đúng `TurnsPerSeason` lần `TurnStarted`.
- R7 — **Bất biến theo run**: `Outcome == Exiled` ⇔ `Reputation == 0`; `Outcome == Won` ⇔ `Season == SeasonCount` và
  `Reputation ≥ 1`; số Mùa có trận = `Season` cuối. Sau `Ended`, mọi lệnh (trừ `SetTimescale`) bị từ chối, state không đổi.
- R8 — **Phủ nhánh** (chống pass rỗng): qua 1.000 run, mỗi nhánh sau xuất hiện ≥ 1 lần: `Ended·Exiled`, `Ended·Won`,
  run tới Mùa 8, Thua ở Mùa 8 còn Uy tín (→ Won), `ChooseReward` nhận, `SkipReward` nhận, `DismissCourtier` nhận, thành
  phẩm khác phế phẩm, phế phẩm, `CatalystActivated`, `ItemInspected`, `ActiveModifier != null`, `Expand` Túi (Nới Túi).
- R9 — **Tất định (GDD §6.1 AC-1, Editor)**: 1.000 run (hồ sơ chuẩn) — chơi lại cùng seed + cùng chuỗi lệnh agent đã
  ghi → cùng `StateHash` **sau mỗi lệnh**.
- R10 — **Độc lập stream (GDD §6.1 AC-3, phần còn lại)**: thêm 1 `Reroll` ở Mùa 1 **không đổi** S Mùa 2, loại trang bị
  Mùa 2, `NextModifier`, và lựa chọn Chiến lợi phẩm Mùa 1 (bù cho test đã bỏ ở phần 1 — Implementation Notes run-loop).

**Ngoài phạm vi**:
- GreedyAgent / số liệu cân bằng (tỉ lệ thắng, vàng trung bình) → M3 `balance`. Agent chỉ phục vụ bất biến.
- Phần đa nền tảng của GDD §6.1 AC-1/AC-2 (Standalone, WebGL) → build WebGL đầu tiên (`setup`/`build`).
- Lưu/tiếp tục giữa run (`Load` giữa chừng — F-013); có thể mở rộng fuzz này thêm "save → load → hash" sau.
- run-flow AC-4 (Hòa → Thắng, đã có ở battle), AC-6c/6d (cutscene, view — F-014/F-017), AC-6/6b (đã xong F-011).
- economy AC-1 nói "fuzz 10.000 lượt": 1.000 run × ~10–24 lượt có hàng Chợ + reroll đạt mốc; nếu số lượt thực tế < 10.000
  thì ghi số thật vào Implementation Notes và giữ AC-1 chưa tick.

**TBD**: không.

**Quyết định (đề xuất, chờ duyệt)**:
- D1: `RandomAgent` đặt ở **test assembly** (`Assets/Tests/EditMode/TestUtils/`), không vào Core — chưa có code production
  nào cần agent; GreedyAgent (M3) có thể chuyển nó sang `Crucible.Core.Simulation` hoặc Editor tool khi cần chạy ngoài test.
- D2: Agent "ngẫu nhiên biết luật" (lệnh sinh từ state: uid có thật, ô trống, slot còn hàng…) + 10 % nhiễu, **không** chỉ
  lệnh mù như `RunLoopInvariantTest` — lệnh mù không qua nổi Mùa 1 (≈ 20.000 lệnh/Mùa). Nhiễu giữ phủ nhánh từ chối.
- D3: Gợi ý công thức 25 % (R2) là núm của agent, không phải luật. Hồ sơ **dễ** (đối thủ 1 %) để phủ Thắng/Mùa 8 thay vì
  tăng gợi ý lên 100 % (vẫn giữ người chơi "dở" ở hồ sơ chuẩn → phủ Trục xuất).
- D4: 500 seed × 2 hồ sơ = 1.000 run cho R4–R8; R9 1.000 run riêng (hồ sơ chuẩn). Ngân sách: cả class mới ≤ 15 s Editor
  batchmode; vượt → giảm R9 xuống 500 run và ghi lại (không giảm R4).
- D5: Giữ nguyên `RunLoopInvariantTest` (lệnh mù, mật độ từ chối cao — vẫn có giá trị). Không refactor chung helper ở
  feature này.
- D6: Fuzz tìm ra vi phạm → **dừng**, chuyển `/unity-game-studio bug` (test tái hiện tối giản từ seed + chuỗi lệnh), không
  sửa luật trong commit của feature này.

## 2. Thiết kế triển khai

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `Assets/Tests/EditMode/TestUtils/RandomAgent.cs` | sinh lệnh theo phase (R1, R2), hàng đợi xếp công thức | `Crucible.Tests.EditMode` | Mới |
| `Assets/Tests/EditMode/TestUtils/RunRecorder.cs` | chạy 1 run bằng agent tới Ended / giới hạn; ghi chuỗi lệnh + sự kiện; gọi callback bất biến mỗi bước | `Crucible.Tests.EditMode` | Mới |
| `Assets/Tests/EditMode/TestUtils/ContentFactory.cs` | + `CreateFullRun(int opponentStatPercent = 100)`, `CreateFullRunSeasons()`, `FullRunEffects` | `Crucible.Tests.EditMode` | Sửa |
| `Assets/Tests/EditMode/TestUtils/RandomAgentTest.cs` | test agent (không mutate, tất định, lệnh không nhiễu hợp lệ ở phase đơn) | `Crucible.Tests.EditMode` | Mới |
| `Assets/Tests/EditMode/Core/Rules/FullRunInvariantTest.cs` | R4–R9 | `Crucible.Tests.EditMode` | Mới |
| `Assets/Tests/EditMode/Core/Rules/RunStreamIndependenceTest.cs` | R10 | `Crucible.Tests.EditMode` | Mới |

Không sửa `Assets/_Project/**`. `.meta` của file mới do Unity sinh khi import (batchmode lần chạy test đầu).

**API (test util)**
```csharp
namespace Crucible.Tests.TestUtils {
    public sealed class RandomAgent {
        public RandomAgent(int seed, int noisePercent = 10, int recipeHintPercent = 25);
        /// Lệnh kế cho state hiện tại; không đổi run. Hàng đợi xếp công thức bị xóa khi (Season, Turn, WorkbenchStep) đổi.
        public ICommand Next(RunState run, ContentDb content);
    }

    public sealed class RunRecord {                     // kết quả một run
        public IReadOnlyList<ICommand> Commands { get; }
        public int Steps { get; }  public bool Ended { get; }
    }

    public static class RunRecorder {
        /// StartRun(seed) → lặp agent.Next → GameRules.Apply tới Ended hoặc maxCommands; onStep(step, command, result, before, run).
        public static RunRecord Play(ContentDb content, MetaState meta, ulong seed, RandomAgent agent, int maxCommands,
                                     Action<StepContext> onStep = null);
    }
    // StepContext: Step, Command, Result, HashBefore, ReputationBefore, SeasonBefore, Run
}
```

**Trọng số lệnh (không nhiễu)**:

| Phase / bước | Lệnh (trọng số) |
|---|---|
| Decree | `AcceptDecree` (1) |
| Workbench·Editing | gợi ý công thức (R2, khi lưới trống); `PlaceItem(uid Túi, ô trống)` 4 · `Brew` 2 (lưới có đồ) · `Buy(slot còn hàng, cell? 1/3)` 2 · `RemoveFromGrid(ô có đồ)` 1 · `MoveCell(có đồ → trống)` 1 · `Sell(uid Túi/lưới)` 1 · `Reroll` 1 · `Inspect(khoáng có Recipe, Kính)` 1 · `ReorderCatalyst(uid Xúc tác, chỉ số)` 1 · `SkipTurn` 1 |
| Workbench·ResultPending | `AckBrewResult` (1) |
| Offering | `Offer(uid đạt chuẩn)` 3 · `Inspect` 1 · `DismissCourtier` 1 |
| Battle | `AckBattle` 4 · `SetTimescale(1–3)` 1 |
| Spoils | `ChooseReward(0…n−1)` 2 · `SkipReward` 1 |
| Nhiễu (10 %, mọi phase) | lệnh bất kỳ trong 19 loại, tham số ngẫu nhiên gồm giá trị sai (uid −1…NextUid+1, ô −1…9, slot −1…6, index −1…3, `StartRun` thứ hai) |

Thiếu điều kiện (vd không có Kính) → lệnh đó trọng số 0 trong lượt rút.

**Content `CreateFullRun`**: `Create(opponentStatPercent, steelStatLines: {Sắc bén 70 % 1–9, Hấp thụ 50 % 1–9},
bellowsEffects: {OnRecipeMatch → AddEssence 5}, mortarEffects: {OnShopRefresh + Chance 50 → GainGold 1}, tongsEffects:
{OnBrewFail → CreateIngredient Sắt}, battle: CreateBattle(CreateFullRunSeasons()))`; `CreateFullRunSeasons` = như
`CreateSpoilsSeasons()` + `modifierPool` 5 modifier × 1, `noModifierWeight` 1 cho Mùa 2–8.

**Interface phụ thuộc**: không (agent dùng `System.Random(seed)` riêng, không chạm `run.Rng`).
**Dữ liệu**: chỉ content test. **Scene / prefab bị chạm**: không.
**Lý do thiết kế**: tách agent (sinh lệnh) khỏi recorder (vòng lặp + ghi) khỏi test (bất biến) → R9 dùng lại chuỗi lệnh đã
ghi để chơi lại, không phụ thuộc agent tất định qua hai lần chạy; test bất biến đọc được như danh sách I1–I7.

## 3. Test cases

Mẫu phong cách: `RunLoopInvariantTest`, `GameRulesAckBattleTest`. Đối tượng qua seam production `GameRules.Apply`.

### Editor tests
(none)

### Unit tests

#### RandomAgentTest (`Assets/Tests/EditMode/TestUtils/RandomAgentTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Next_AnyPhase_DoesNotChangeState` (Decree, Editing, ResultPending, Offering, Battle, Spoils, Ended) | hash trước = sau 100 lần `Next` |
| `Next_SameSeed_SameCommandSequence` | 200 lệnh của hai agent cùng seed trên hai run cùng seed giống nhau (kiểu + tham số) |
| `Next_NoNoiseInSinglePhase_ReturnsAcceptedCommand` (Decree, ResultPending, Battle, Spoils) | lệnh trả về được `Accepted` |
| `Next_RecipeHintWithIngredientsInBag_BrewsProduct` (hint 100 %, Túi Sắt + Than, Editing lưới trống) | sau ≤ 3 lệnh agent: `BrewCompleted` không phế phẩm (Thép) |

### Integration tests (Core, nhiều Command qua `GameRules.Apply`)

#### FullRunInvariantTest (`Assets/Tests/EditMode/Core/Rules/FullRunInvariantTest.cs`)
| Test method | Kiểm tra |
|---|---|
| `Agent_FiveHundredRuns_EveryRunReachesEnded` (hồ sơ: chuẩn, dễ) — R4 | mọi run `Phase == Ended` trong 5.000 lệnh |
| `Agent_FiveHundredRuns_StepInvariantsHold` (hồ sơ: chuẩn, dễ) — R5 | I1–I7 đúng sau mọi lệnh (thông điệp: seed, step, lệnh) |
| `Agent_FiveHundredRuns_SeasonInvariantsHold` (hồ sơ: chuẩn, dễ) — R6 | mỗi Mùa: 1 trận, 1 Ack, ≤ 1 Dâng tiêu hao đúng uid, trang bị = Chiếu chỉ, ra về ⇒ Thua, N `TurnStarted` |
| `Agent_FiveHundredRuns_OutcomeMatchesReputationAndSeason` (hồ sơ: chuẩn, dễ) — R7 | Exiled ⇔ Uy tín 0; Won ⇔ Mùa cuối + Uy tín ≥ 1; số Mùa có trận = `Season` |
| `Agent_EndedRun_RejectsEveryRunCommand` — R7 | 18 loại lệnh (trừ `SetTimescale`) bị từ chối, hash không đổi |
| `Agent_ThousandRuns_CoversEveryBranch` — R8 | mỗi nhánh R8 đếm ≥ 1 (assert từng nhánh, tên nhánh trong thông điệp) |
| `Agent_ThousandRuns_ReplaySameHashEveryStep` — R9 | chơi lại chuỗi lệnh đã ghi: hash bằng nhau sau mỗi lệnh |

> Ghi chú hiệu năng: 4 test đầu dùng chung một lần chơi/hồ sơ qua `[OneTimeSetUp]` cache `RunRecord` + log bất biến?
> **Không** — mỗi test tự chạy để thất bại độc lập; nếu tổng > 15 s (D4) mới gộp thành 1 lần chơi + thu thập vi phạm.

#### RunStreamIndependenceTest (`Assets/Tests/EditMode/Core/Rules/RunStreamIndependenceTest.cs`)
Kịch bản cố định trên hồ sơ dễ: StartRun → Accept → (B: `Reroll`) → xếp Sắt ô 0, Than ô 1 (thêm Than bằng `StateFactory.AddItem`)
→ Brew → Ack → Skip ×2 → Offer(Thép) → AckBattle (Thắng → Spoils Mùa 1) → SkipReward → Decree Mùa 2. Chạy 50 seed.

| Test method | Kiểm tra |
|---|---|
| `Apply_ExtraRerollInSeasonOne_SameSpoilsOptions` | `SpoilsOptions` Mùa 1 của A = B |
| `Apply_ExtraRerollInSeasonOne_SameSeasonTwoDecree` | Mùa 2: `SeasonPricePercent`, `DecreeEquipment`, `ActiveModifier`, `NextModifier` của A = B |
| `Apply_ExtraRerollInSeasonOne_DifferentShopState` (đối chứng) | hash `Shop` stream của A ≠ B (reroll có thật sự rút) |

### Visual verification tests
(none — không có view)

### Manual tests
(none)

**Truy vết**: R1 → `RandomAgentTest.Next_AnyPhase_*`, `Next_SameSeed_*`, `Next_NoNoise*` · R2 → `Next_RecipeHint*`, R8
(nhánh thành phẩm) · R3 → R8 (nhánh Xúc tác / modifier / Spoils) · R4 → `Agent_*_EveryRunReachesEnded` · R5 →
`Agent_*_StepInvariantsHold` · R6 → `Agent_*_SeasonInvariantsHold` · R7 → `Agent_*_OutcomeMatches*`,
`Agent_EndedRun_RejectsEveryRunCommand` · R8 → `Agent_ThousandRuns_CoversEveryBranch` · R9 →
`Agent_ThousandRuns_ReplaySameHashEveryStep` · R10 → `RunStreamIndependenceTest.*`.
**AC**: run-flow AC-1, AC-2, AC-3, AC-5, AC-7 (logic) · GDD §6.1 AC-1 (Editor), AC-3 (loại trang bị + chiến lợi phẩm) ·
economy AC-1 (nếu đạt 10.000 lượt), AC-4.
**Testability**: **PASS** — Core thuần, RNG có seed, agent tách khỏi luật; vi phạm báo được seed + step để tái hiện.
**WARN** nhỏ: "pass rỗng" nếu agent không phủ nhánh → chặn bằng R8.

## 4. Các bước thực hiện

> `CLAUDE.md` §Git: Commit tự động **không**, test-first **không** → chỉ đề xuất message; gộp test vào commit xanh.
> Lệnh test (Editor đóng): `"$UNITY" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode
> -testResults Logs/editmode.xml -logFile -` (Linux: `UNITY="$HOME/Unity/Hub/Editor/6000.6.7f1/Editor/Unity"`);
> riêng class mới: thêm `-testFilter "Crucible.Tests.Core.Rules.FullRunInvariantTest"`.

- [x] **Bước 1 — Skeleton**: `RandomAgent` (`Next` trả `new SkipTurn()`), `RunRecorder.Play`, `RunRecord`, `StepContext`,
  `ContentFactory.CreateFullRun` (+ seasons, effects). Kiểm chứng: compile sạch; `ContentDb.Create` không ném với hồ sơ
  chuẩn + dễ; **997/997** xanh.
- [x] **Bước 2 — Test first**: viết test §3. Kiểm chứng: `RandomAgentTest` (trừ `DoesNotChangeState`) và
  `Agent_*_EveryRunReachesEnded` fail đúng lý do (agent chỉ Skip → kẹt ở Decree); ghi số pass/fail dự đoán trước.
  `RunStreamIndependenceTest` có thể xanh ngay (luật đã có) — chấp nhận, đó là test đặc tả hành vi hiện tại.
- [x] **Bước 3 — Implement agent**: trọng số theo bảng §2 → hàng đợi gợi ý công thức → nhiễu. Kiểm chứng: toàn bộ EditMode
  xanh; đo thời gian class mới (đọc `duration` trong `Logs/editmode.xml`) ≤ 15 s; 0 warning CS mới;
  `python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .` 0 lỗi.
  Vi phạm bất biến thật → D6 (dừng, `bug`).
- [x] **Bước 4 — Refactor**: gom bộ đếm nhánh R8 + kiểm bất biến vào helper riêng trong test; test vẫn xanh.
- [x] **Bước 5 — Tài liệu**: PLAN F-009 → **Done** (+ link plan này); run-flow tick AC-1/2/3/5/7 (logic); GDD §6.1 AC-1 ghi
  "Editor xong, chờ WebGL", AC-3 tick phần logic; economy AC-1 (nếu ≥ 10.000 lượt — ghi số thật) / AC-4; TECH §3.7 thêm
  `RandomAgent`/`RunRecorder`; TEST.md mục fuzz; LOG + CHECKPOINT.
  Commit đề xuất: `test(run): add RandomAgent full-run fuzz (1,000 runs to RunEnd) and stream independence tests`.

## 5. Rủi ro & đánh đổi

- **Thời gian test**: 2.000 run (R4–R9) + hash mỗi bước. Ước ~300–500 lệnh/run → ~1 triệu `Apply` + hash. Nếu > 15 s →
  gộp 4 test hồ sơ thành 1 lần chơi (D4), rồi mới giảm R9. Không giảm số run của R4.
- **Agent quá "dở"** → nhánh Thắng / Mùa 8 hiếm. R8 chặn pass rỗng; nếu R8 fail ở hồ sơ dễ, tăng `RecipeHintPercent` của
  hồ sơ dễ (không đổi luật). Số lần mỗi nhánh xuất hiện ghi vào Implementation Notes để làm mốc.
- **Ngưỡng R8 là "≥ 1"** với seed cố định → tất định, không flaky; nhưng đổi content/luật sau này có thể làm nhánh hiếm
  biến mất → test fail là tín hiệu đúng (phủ giảm), sửa bằng chỉnh agent/seed, ghi lý do.
- **Hồ sơ dễ đối thủ 1 %**: chỉ số đối thủ sau scale có thể về 0 — nếu `ContentDb`/`BattleStats` chặn hoặc chia 0 thì dùng
  mức thấp nhất hợp lệ (vd 5 %) và ghi lại; không đổi validator.
- **Test util lớn dần** (`RandomAgent` ~200 dòng): chấp nhận cho tới M3; GreedyAgent sẽ quyết định chuyển chỗ (D1).
- `RunLoopInvariantTest.Apply_RandomCommandsFromStartRun_AlwaysReachesSpoilsOrEnded` phần nào trùng R4 — giữ (D5), xem xét
  gỡ ở `refactor` riêng nếu suite chậm.

## 6. Implementation Notes (điền khi làm)

- **Design decisions**:
  - `RunRecorder.Start` ném `InvalidOperationException` khi `StartRun` bị từ chối (lỗi arrange, không phải bất biến).
  - `StepContext` là một object dùng lại giữa các bước; thông điệp assert chỉ dựng khi vi phạm (`Fail(c, …)`) — tránh
    nội suy chuỗi ~77.000 bước × 15 kiểm tra.
  - Bất biến Mùa: Mùa đóng ở `SeasonStarted` / `RunEnded`; trang bị kỳ vọng lấy từ `EquipmentBefore` bước đầu rồi
    `run.DecreeEquipment` sau mỗi lần đóng Mùa.
  - Đếm `ActiveModifier set` theo bước (không theo run) — chỉ cần ≥ 1.
- **Deviations**:
  - Hồ sơ dễ dùng đối thủ **5 %** (`EasyOpponentStatPercent`), không 1 %: 1 % làm HP/ATK đối thủ Mùa 1 về 0 (60 × 1 % ⌊⌋ = 0)
    — trận suy biến (theo rủi ro §5).
  - Gợi ý công thức (R2) **mở rộng**: nguyên liệu thiếu lấy từ ô Chợ còn hàng (`Buy(slot, cell)`), thiếu vàng → bán trước vật
    phẩm Túi không dùng. Chỉ lấy từ Túi thì 1.000/1.000 run bị Trục xuất, chỉ 10 thành phẩm (bộ khởi đầu không có Than,
    vàng 0) → R8 fail `Ended·Won`.
  - Hồ sơ dễ dùng `RecipeHintPercent` **75** (`EasyRecipeHintPercent`), chuẩn giữ 25: với 25 % chỉ 1 run Thắng / 3 run tới Mùa 8
    — pass nhưng quá mỏng. 75 % → 11 Thắng / 17 tới Mùa 8.
  - Bước 1 + 2 gộp một lần chạy Unity: viết agent + test, chèn tạm `return new SkipTurn()` đầu `Next` (skeleton) → 17 fail
    đúng lý do (kẹt ở Decree, `WrongPhase`, không brew), rồi gỡ dòng tạm. Không có lần chạy "skeleton 997/997" riêng (test cũ
    vẫn xanh trong cùng lần chạy: 1007 pass).
  - `RandomAgentTest.Reach` thử seed 0…49 tới khi tới phase đích (seed 2 cố định bị Trục xuất trước Spoils).
- **Tradeoffs**:
  - Pass sớm ở skeleton (đã dự đoán/chấp nhận): `RunStreamIndependenceTest` ×3 (luật đã có — test đặc tả), `SameSeed` (tầm
    thường khi mọi lệnh là `SkipTurn`), `StepInvariantsHold`/`SeasonInvariantsHold`/`Replay` (rỗng — run không rời Decree, Mùa
    không đóng). Chúng có ý nghĩa nhờ R8 + `EveryRunReachesEnded` cùng chạy trên agent thật.
  - Hồ sơ dễ vẫn chủ yếu Trục xuất (989/1000 tổng; ~2.963 lần ra về): agent hay bán/nung mất thành phẩm. Đủ cho phủ nhánh;
    tỉ lệ thắng là việc của GreedyAgent (M3).
  - Không gộp lần chơi giữa các test (D4): cả 3 class ~2,06 s (dự kiến 15 s) — mỗi run trung bình ~77 lệnh, không phải 300–500.
- **Test changes**: thêm `Reach` nhiều seed + `MaxDriveSeeds` (sau lần chạy implement đầu tiên — 2 case `Spoils` fail ở arrange).
- **Open questions**:
  - Số đếm phủ nhánh (mốc, 2026-11-04): 77.157 lệnh, 10.617 lượt; Exiled 989, Won 11, tới Mùa 8 17, Thua Mùa 8 vẫn Thắng 4,
    ChooseReward 362, SkipReward 181, ra về 2.963, thành phẩm 1.214, phế phẩm 2.317, Xúc tác kích hoạt 180, Kính soi 18, Nới Túi 45.
    Đổi content/luật làm nhánh hiếm (Kính soi 18, Thua Mùa 8 → Thắng 4) về 0 → chỉnh agent/seed, ghi lý do.
