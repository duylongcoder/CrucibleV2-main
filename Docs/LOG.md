# Project Log — CrucibleV2 (tên tạm)

<!--
  Hai vùng khác nhau:
  1. CHECKPOINT: GHI ĐÈ mỗi lần cập nhật, giữ dưới ~15 dòng. Đọc đầu mỗi phiên để nối tiếp công việc.
  2. Các mục bên dưới: GHI THÊM (append), mới nhất ở trên cùng của từng mục.
  Khi Session Log dài quá ~200 dòng: chuyển phần cũ sang Docs/log-archive/YYYY-MM.md.
-->

<!-- CHECKPOINT -->
**Cập nhật**: 2026-11-04
**Nhánh git**: `develop`
**Đang làm**: xong `build run-loop` (F-009 phần 1). 486/486 EditMode xanh. **Chưa commit** (message đề xuất trong báo cáo)
**Bước tiếp theo**: người dùng commit → F-007 (Xúc tác, chèn `OnShopRefresh` vào `SeasonRules.StartTurn`) hoặc F-010 (Rèn/Trận/Uy tín — Dâng + rút `Forge` ở `EnterDecree`) → F-009 phần 2
**Đang kẹt**: không
**File đang dở**: không
**Câu hỏi mở**: số liệu `MarketPrice` từng item + `UtilitySlotWeights` (balance); số liệu 3 bảng ma thuật theo tier (balance); phụ gia `MagicChance` field vs Effect `AddMagicChance` (F-007); đồng bộ IndexedDB WebGL; ART A2/A3; battle Open Questions; màn Kết thúc run; GDD In Review
<!-- /CHECKPOINT -->

## Session Log

### 2026-11-04 — build run-loop
- **Command**: /unity-game-studio build run-loop
- **Đã làm**:
  - Duyệt D1–D6 (giữ nguyên đề xuất).
  - Core: `SeasonRules` (`EnterDecree`, `AcceptDecree`, `SkipTurn`, `AckBrewResult`, private `StartTurn`/`EndTurn`), 3 Command + 4 sự kiện (`DecreeAccepted`, `TurnStarted`, `TurnSkipped`, `OfferingStarted`), `RejectReason.NoPendingResult`, `RunState.Turn`, `RunData.TurnsPerSeason` + validate, `StateHasher` ghi `Turn`; `StartRun` gọi `EnterDecree` (S rút ngay Mùa 1).
- **File**:
  - Assets/_Project/Scripts/Core/** (8 mới, 8 sửa)
  - Assets/Tests/EditMode/** (6 mới, 5 sửa)
  - Docs/plans/run-loop.md, plans/market.md, PLAN.md, TECH.md, design/brewing.md, design/economy.md, LOG.md
- **Test**:
  - Skeleton: 430/430.
  - Test-first: 486 → 48 mới fail đúng lý do; 1 fail do exception + 4 pass sai ở skeleton → đã sửa test (ghi §6); 9 pass đã dự đoán (fuzz bất biến, test biên).
  - Implement: **486/486 xanh** lần đầu; refactor 486/486; 0 warning CS; `check_unity_project.py` 0 lỗi.
- **Ghi chú**: commit `3c5fe1e` có message "feat(run-loop)…" nhưng nội dung là F-006 Chợ + file plan run-loop (code run-loop nằm ở thay đổi chưa commit lần này).
- **Còn lại**: F-009 phần 2 (Resolution → Mùa kế / RunEnd, AC-1 1.000 run) sau F-010/F-011/F-017; `OnShopRefresh` (F-007); DTO `Turn` (F-013); view Chiếu chỉ / Tiếp tục / Bỏ qua lượt (F-014)

### 2026-11-04 — plan run-loop
- **Command**: /unity-game-studio plan run-loop
- **Đã làm**: plan F-009 phần 1 — 11 yêu cầu; `SeasonRules` (`EnterDecree` rút S lúc vào Decree, `AcceptDecree`, `SkipTurn` trả lưới về cuối Túi theo ô 0→8, `AckBrewResult`; private `StartTurn` = Editing + `RefreshShop`, `EndTurn` → lượt kế hoặc Offering), `RunState.Turn` (1–N ở Workbench, 0 nơi khác), `RunData.TurnsPerSeason` (mặc định 3, validate ≥ 1), 4 sự kiện, `RejectReason.NoPendingResult`; ~60 test EditMode (unit + `RunLoopTest` + fuzz `RunLoopInvariantTest`)
- **Quyết định (đề xuất, chờ duyệt)**: D1 rút Decree lúc vào (sửa test F-006 StartRun S = 100); D2 class `SeasonRules`; D3 vào Offering xóa hàng Chợ; D4 Ack giữ `LastBrew`; D5 Skip khi ResultPending → `GridLocked`, Ack khi Editing → `NoPendingResult`; D6 giữ 2 fuzz cũ, thêm fuzz vòng thật
- **File**: Docs/plans/run-loop.md (mới), Docs/PLAN.md, Docs/LOG.md
- **Test**: chưa (plan)

### 2026-11-04 — build market
- **Command**: /unity-game-studio build market
- **Đã làm**:
  - Duyệt D1–D6 (giữ nguyên đề xuất).
  - Core: `MarketData`, `ItemData.MarketPrice`, `ContentDb` (+ tham số `market`, validate, bể `ShopPool`/`Lenses`/`Catalysts`), `ShopSlot`/`ShopState`, `RunState.SeasonPricePercent`/`Shop`, `PriceFormula`, `MarketRules` (giá, `RollSeasonPrice`, `RefreshShop`, `Buy`/`Sell`/`Reroll`), 3 Command + 3 sự kiện, 5 `RejectReason`, `BagRules.CountCatalysts`, `GridRules.IsPlaceable` public, `StartRun` đặt S = 100 + xóa hàng, `StateHasher` ghi Chợ.
- **File**:
  - Assets/_Project/Scripts/Core/** (11 mới, 9 sửa)
  - Assets/Tests/EditMode/** (8 mới, 9 sửa)
  - Docs/plans/market.md, PLAN.md, TECH.md, GDD.md, design/economy.md, LOG.md
- **Test**:
  - Test-first: 430 → 312 cũ xanh, 116 mới fail đúng lý do, 2 pass (1 biên đã dự đoán, 1 test yếu đã sửa).
  - Implement: 425/430 — fuzz bất biến thiếu độ phủ (vàng cạn) → harness +20 vàng/lượt giả → **430/430 xanh**; 0 warning CS; `check_unity_project.py` 0 lỗi.
- **Còn lại**: nối `RefreshShop`/`RollSeasonPrice` vào vòng lượt (F-009); `OnShopRefresh` (F-007); modifier giá + reroll miễn phí (F-011); bể theo Danh vọng (F-012); save DTO (F-013); view Chợ (F-014)

### 2026-11-04 — build bag
- **Command**: /unity-game-studio build bag
- **Đã làm**:
  - Duyệt D1–D3, R5.
  - Core: `BagRules` (`UsedSlots`/`FreeSlots`/`HasRoom`, `CanExpand`/`Expand`, `IndexOfUid`, luật `ReorderCatalyst`), `GridRules.CountItems`, Command `ReorderCatalyst` + `CatalystsReordered`, `RejectReason.ItemNotCatalyst/InvalidCatalystIndex`, `RunData.MaxBagCapacity`, `ContentDb` validate sức chứa + bộ khởi đầu.
  - Refactor: `BrewingRules.IsGridEmpty` → `CountItems`; `GridRules.IndexOfUid` → `BagRules.IndexOfUid`.
- **File**:
  - Assets/_Project/Scripts/Core/** (3 mới, 6 sửa)
  - Assets/Tests/EditMode/** (5 mới, 6 sửa)
  - Docs/plans/bag.md, PLAN.md, TECH.md, GDD.md, design/economy.md, LOG.md
- **Test**:
  - Skeleton: 255/255.
  - Test-first: 312 → 31 mới fail đúng lý do; 26 pass ở skeleton (22 đã dự đoán, 4 test yếu đã sửa).
  - Implement: **312/312 xanh** lần đầu; refactor 312/312; 0 warning CS; `check_unity_project.py` 0 lỗi.
- **Còn lại**: `Buy` + `BagFull` + AC-2/AC-2b đầy đủ (F-006); `CatalystSlots` (F-006/F-007); Nới Túi (F-011); view Túi/kệ (F-014)

### 2026-11-04 — plan bag
- **Command**: /unity-game-studio plan bag
- **Đã làm**: plan F-005 — 7 yêu cầu; `BagRules` (`UsedSlots`/`FreeSlots`/`HasRoom` = Túi + lưới, `CanExpand`/`Expand` tới trần), `GridRules.CountItems`, `RunData.MaxBagCapacity` + validate (sức chứa khởi đầu ≤ trần, bộ khởi đầu ≤ sức chứa), Command `ReorderCatalyst(uid, toIndex)` theo vị trí kệ (vật phẩm khác giữ chỉ số) + `CatalystsReordered`, 2 `RejectReason`; ~45 test EditMode (unit + fuzz bất biến sức chứa)
- **Quyết định (đề xuất, chờ duyệt)**: D1 chỉ Xúc tác sắp thứ tự (thứ tự vật phẩm khác do view); D2 `ReorderCatalyst` chỉ ở Workbench·Editing; D3 `BagFull` để F-006; R5 bộ khởi đầu vượt sức chứa = lỗi content
- **File**: Docs/plans/bag.md (mới), Docs/PLAN.md, Docs/LOG.md
- **Test**: chưa (plan)

### 2026-11-04 — build brewlog
- **Command**: /unity-game-studio build brewlog
- **Đã làm**:
  - Core: `BrewLog`/`BrewTick`/`BrewLogResult` (Core/Logs). `Brew` chụp log trước khi tiêu hao → `RunState.LastBrew` + `BrewCompleted.Log`; `StartRun` xóa log.
  - Core: `MetaState.Timescale` (mặc định 1), Command `SetTimescale` + `SettingsRules` + `TimescaleChanged`, `RejectReason.InvalidTimescale`.
  - `StateHasher` ghi `LastBrew` + `Timescale`.
  - Runtime: `BrewReplayTimeline` (script đầu tiên của `Crucible.Runtime`).
- **File**:
  - Assets/_Project/Scripts/Core/** (6 mới, 8 sửa), Runtime/Replay (1 mới)
  - Assets/Tests/EditMode/** (4 mới, 3 sửa)
  - Docs/plans/brewlog.md, PLAN.md, TECH.md, GDD.md, design/brewing.md, LOG.md
- **Test**:
  - Skeleton: 196/196.
  - Test-first: 255 → 51 mới fail đúng lý do; 8 pass ở skeleton (2 test yếu đã sửa).
  - Implement: **255/255 xanh** lần đầu; 0 warning CS; `check_unity_project.py` 0 lỗi.
- **Còn lại**: view phát lại (F-014); DTO save mang `LastBrew` + `Timescale` (F-013); `AckBrewResult` (F-009)

### 2026-11-04 — plan brewlog
- **Command**: /unity-game-studio plan brewlog
- **Đã làm**: plan F-004 — 8 yêu cầu; Core `BrewLog` (`FirstCell`, `Ticks` nạp/chờ từ vật phẩm đầu → cuối, `Result`; không chứa `Recipe`), `RunState.LastBrew` + `BrewCompleted.Log` + hash; `MetaState.Timescale` (1–3, mặc định 1) + Command `SetTimescale` (`SettingsRules`); Runtime `BrewReplayTimeline` (đồng hồ phát lại thuần C#, đổi timescale giữa chừng, `Skip`); ~50 test EditMode
- **Quyết định (đề xuất, chờ duyệt)**: view phát lại → F-014 (Runtime chưa có màn); `SkipReplay` không là Command Core (chỉ Presentation; state sau tua ≡ xem hết); giữ `LastBrewResultUid` (không trộn refactor)
- **File**: Docs/plans/brewlog.md (mới), Docs/PLAN.md, Docs/LOG.md
- **Test**: chưa (plan)

### 2026-11-04 — build magic
- **Command**: /unity-game-studio build magic
- **Đã làm**: Core — `MagicChanceFormula.Compute/ForBrew` (3 bảng theo tier), `BrewingData` 3 bảng + hằng tier/%, `ItemData.MagicChance`, validate content (bảng 10 phần tử 0–100, phụ gia 0–50, chỉ phụ gia có MagicChance, Mineral/Additive tier 1–10), `BrewingRules` tung 1 lần `Chance` khi khớp; TECH §3.4 thứ tự rút `Chance`
- **File**: Assets/_Project/Scripts/Core/** (1 mới, 4 sửa), Assets/Tests/EditMode/** (3 mới, 3 sửa), Docs/plans/magic.md, Docs/PLAN.md, Docs/TECH.md, Docs/GDD.md, Docs/LOG.md
- **Test**: skeleton + test → 196: 160 cũ xanh, 30 mới fail đúng, 6 pass tầm thường (5 đã dự đoán); implement → **196/196 xanh** lần đầu; 0 warning CS; `check_unity_project.py` 0 lỗi
- **Còn lại**: F-008 dòng thuộc tính rút `Chance` sau ma thuật; F-007 Xúc tác

### 2026-11-04 — plan magic
- **Command**: /unity-game-studio plan magic
- **Đã làm**: plan F-003 — 7 yêu cầu; `MagicChanceFormula.Compute/ForBrew`, tung đúng 1 lần `Chance` khi khớp (kể cả 0%), phế phẩm không rút; `BrewingData` 3 bảng theo tier + `ItemData.MagicChance`; ~33 test EditMode
- **Quyết định**: công thức ma thuật tham số hóa `min(Cap[tierSP], ⌊(Σ Inherit[tier] + Σ phụ gia) × Retain[tierSP]/100⌋)`, mặc định = công thức gốc (người dùng chốt) → sửa brewing.md §Formulas + Open Questions; tung ma thuật là lần rút `Chance` đầu tiên của lượt nung
- **File**: Docs/plans/magic.md (mới), Docs/design/brewing.md, Docs/PLAN.md, Docs/LOG.md
- **Test**: chưa (plan)

### 2026-11-04 — build brewing
- **Command**: /unity-game-studio build brewing
- **Đã làm**: Core — `RecipeData/RecipeSlotData` → `Recipe/RecipeSlot` (validate: 1–9 slot, đầu/cuối nguyên liệu, nguyên liệu Mineral, phụ gia Additive, không trùng hình, ≤ 1 Scrap), `BrewingData` (`QualityCap`), `QualityFormula` (long, Mul% tăng dần + floor), `RecipeMatcher`, `GridRules` (Place/Remove/Move + `ValidateEditable`), `BrewingRules` (tiêu hao lưới, 1 kết quả, `ResultPending`), state `Grid/WorkbenchStep/LastBrewResultUid`, `ItemInstance.Magic/Quality`, hash field mới; 5 class test mới + sửa 4
- **File**: Assets/_Project/Scripts/Core/** (14 mới, 9 sửa), Assets/Tests/EditMode/** (5 mới, 5 sửa), Docs/plans/brewing.md, Docs/PLAN.md, Docs/TECH.md, Docs/GDD.md, Docs/LOG.md
- **Test**: skeleton + test → 160: 69 cũ xanh, 80 mới fail đúng lý do, 11 pass tầm thường (đã dự đoán); implement → **160/160 xanh** lần đầu; refactor → 160/160; 0 warning CS; `check_unity_project.py` 0 lỗi
- **Vấn đề gặp & cách xử lý**: `Has.None.AnyOf` không có trong NUnit 3.5 → đổi assertion; `ContentDb` phụ thuộc `Rules` → dời hằng trần sang `BrewingData`
- **Còn lại**: F-003 ma thuật (Brew sẽ rút 1 lần `Chance`); F-004 BrewLog; F-007 Effect Xúc tác vào `QualityFormula`

### 2026-11-04 — plan brewing
- **Command**: /unity-game-studio plan brewing
- **Đã làm**: plan F-002 — 10 yêu cầu; lưới 9 ô + `PlaceItem/RemoveFromGrid/MoveCell/Brew`, `RecipeMatcher`, `QualityFormula`, Recipe trong `ContentDb` (validate cùng hình, ≤ 1 Scrap), `BrewingData.QualityCap`; ~60 test EditMode; F-003/F-004/F-007/F-009 tách rõ
- **Quyết định**: `Quality` = `long`, MulPotency theo %, nhân theo thứ tự tăng dần + floor mỗi bước, trần `QualityCap` 10¹⁵ (người dùng chốt) → sửa brewing.md §Formulas; "Nung khi chỉ có phụ gia" → cho phép, ra phế phẩm (theo Edge Cases)
- **File**: Docs/plans/brewing.md (mới), Docs/design/brewing.md, Docs/PLAN.md, Docs/LOG.md
- **Test**: chưa (plan)

### 2026-11-04 — build core-foundation
- **Command**: /unity-game-studio build core-foundation
- **Đã làm**: Core — `SplitMix64`, `Pcg32` (Lemire, `Roll` 1 lần rút), `RngStreams` (7 stream, capture/restore), `ContentDb` (ordinal), `RunState`/`MetaState`/`ItemInstance`, `CanonicalWriter` + `StateHasher` (FNV-1a 64), `GameRules` + `RunStartRules`/`StartRun`; test EditMode 6 class + `TestUtils`
- **File**: Assets/_Project/Scripts/Core/{Random,Content,State,Commands,Rules}/*.cs (26), Assets/Tests/EditMode/{Core,TestUtils}/*.cs (9), Docs/plans/core-foundation.md, Docs/PLAN.md, Docs/TECH.md, Docs/GDD.md, Docs/LOG.md
- **Test**: test-first 69 → 62 fail đúng lý do; implement → 68/69 (lỗi assertion `Has.Count` trên mảng, sửa test) → **69/69 xanh**, 0 warning CS mới; `check_unity_project.py` 0 lỗi
- **Vấn đề gặp & cách xử lý**: batchmode đổi `ProjectSettings.asset` (bỏ define Sentis) — để nguyên, không commit
- **Còn lại**: AC-2/AC-3 GDD §6.1 khi có Chợ / Chiếu chỉ; AC-1 trên WebGL

### 2026-11-04 — plan core
- **Command**: /unity-game-studio plan core
- **Đã làm**: chia "core" (cỡ L) → plan nền F-000 + F-001; 10 yêu cầu, ~40 test EditMode (Pcg32/SplitMix64 vector chuẩn, stream độc lập, ContentDb, StartRun, StateHasher, ràng buộc assembly); 5 bước test-first
- **File**: Docs/plans/core-foundation.md (mới), Docs/PLAN.md, Docs/LOG.md
- **Test**: chưa (plan)

### 2026-11-04 — setup
- **Command**: /unity-game-studio setup (check + apply)
- **Đã làm**: check: Visible Meta Files đã OK; thiếu asmdef, Newtonsoft, `.gitattributes`; máy Linux chỉ có Unity 6000.6.8f1. Apply: 6 asmdef (Core noEngineReferences, Content, Runtime, Editor, Tests.EditMode + Newtonsoft, Tests.PlayMode); manifest +1 dòng Newtonsoft 3.2.1; `.gitattributes` UnityYAMLMerge; CLAUDE.md lệnh test theo `$UNITY` macOS/Linux; BUILD.md kết quả + lệnh cấu hình merge driver
- **File**: Assets/_Project/Scripts/*/Crucible.*.asmdef, Assets/Tests/{EditMode,PlayMode}/*.asmdef, Packages/manifest.json, .gitattributes, CLAUDE.md, Docs/BUILD.md, Docs/TECH.md, Docs/LOG.md
- **Test**: không chạy được — máy thiếu Unity 6000.6.7f1, chưa có Library
- **Vấn đề gặp & cách xử lý**: lần ghi manifest đầu sắp xếp lại toàn file → khôi phục, chỉ chèn 1 dòng; 29 cảnh báo GUID do thiếu PackageCache (chưa xác nhận lỗi)
- **Còn lại**: mở Editor để xác nhận compile + import package

### 2026-11-04 — arch core
- **Command**: /unity-game-studio arch core
- **Đã làm**: TECH §3.1–3.7 — ContentDb (Id chuỗi → ItemId), RunState/MetaState mutable, 20 Command + GameRules (validate trước), Pcg32 + 7 stream, BrewLog/BattleLog, RunViewBuilder, StateHasher, GameSession + ISaveStore/IClock; §2 EditMode tham chiếu Runtime
- **File**: Docs/TECH.md, Docs/LOG.md
- **Test**: không (thiết kế)

### 2026-11-04 — arch overview
- **Command**: /unity-game-studio arch
- **Đã làm**: kiến trúc tổng (TECH §1 sơ đồ lớp + luồng Command, §2 asmdef, §4 lưu, §5 quy ước, §6 ngân sách, §7 ADR-001…007)
- **File**: Docs/TECH.md, CLAUDE.md, Docs/ART.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: giả định init "SO event channel" thay bằng lõi C# thuần (ADR-001); Newtonsoft là package mới — người dùng đã duyệt, cài ở `setup`
- **Còn lại**: thiết kế kỹ thuật từng hệ thống; kiểm đồng bộ IndexedDB WebGL

### 2026-11-04 — art 6
- **Command**: /unity-game-studio art 6
- **Đã làm**: ART §6 — 3 địa điểm (Xưởng dùng cho Menu/Chiếu chỉ/Bàn luyện/Dâng; Lò rèn chung nền, 4 bộ đạo cụ đúc; Nơi giao đấu: Đấu trường/Chiến trường chung nền đất) + Đại chiến cảnh riêng; ngoại lệ value ngoài trời ≤ 55; cửa sổ theo Mùa (4 cảnh); sửa §1/§4 tham chiếu ngoại lệ
- **File**: Docs/ART.md, Docs/LOG.md
- **Test**: không (thiết kế)

### 2026-11-04 — art 5
- **Command**: /unity-game-studio art 5
- **Đã làm**: ART §5 — chibi ~3 đầu 64 px; silhouette cận thần / thợ rèn / dũng sĩ / đối thủ; 8 vương quốc + giáp theo bậc; biểu cảm thợ rèn 3 kênh (mặt, dáng, bong bóng); trang bị vẽ ramp xám → `EquipmentRamp` của khoáng; chùy gỗ mặc định cho Giáp/Khiên; ~150 frame
- **File**: Docs/ART.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Còn lại**: shader palette-swap (arch); §6 môi trường (A7)

### 2026-11-04 — gdd battle
- **Command**: /unity-game-studio gdd battle
- **Đã làm**: design/battle.md đủ mục — 1-đấu-1, mô phỏng tất định không RNG (HP/ATK/SB/PD/HT, quy đổi theo loại trang bị, OffAffinity 50%), ta đánh trước, MaxRounds 10 → tie-break tổng sát thương; phát lại luôn 6 nhịp; thanh máu không số + số sát thương; thuộc tính lộ khi kích hoạt → Sổ
- **File**: Docs/design/battle.md (mới), Docs/design/run-flow.md, Docs/design/ui-season.md, Docs/GDD.md, Docs/PLAN.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: thay luật so `Power` → dọn mọi tham chiếu `Power`; ràng buộc "không rò rỉ `Power`" bỏ (luật trận công khai, sau khi dâng); lối phòng thủ thuần → tie-break tổng sát thương
- **Còn lại**: balance Trận; ART đấu trường / NPC

### 2026-11-04 — ui các màn còn lại
- **Command**: /unity-game-studio ui
- **Đã làm**: spec Chiếu chỉ (tên vương quốc, câu yêu cầu), Dâng (khay cận thần), cutscene Rèn 8 nhịp (biểu cảm thợ rèn theo `Quality`, cảnh đúc theo loại), ràng buộc Trận, Kết quả & Chiến lợi phẩm (vàng + 1/3), Menu chính / trong run, Sổ công thức theo khoáng; "Kiếm" thống nhất; giữ "Mùa"
- **File**: Docs/design/ui-season.md (mới), Docs/design/ui-meta.md (mới), Docs/design/run-flow.md, Docs/GDD.md, Docs/PLAN.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: Sổ ??? mâu thuẫn GDD "không ?/N" → người dùng chấp nhận lộ tổng số + biến thể/nhóm; biểu cảm thợ rèn lộ `Quality` → ngoại lệ có chủ đích (chỉ sau khi dâng, không theo `Power`); thứ tự mục Sổ không được suy ra từ khoảng cách
- **Còn lại**: Trận (#17), màn Kết thúc run, ART §5 NPC

### 2026-11-04 — chốt Q1–Q12 (thiết kế các màn)
- **Command**: /unity-game-studio (tiếp `ui`)
- **Đã làm**: Chợ 5 ô (4 nguyên liệu + 1 tiện ích); đối thủ ẩn ở Chiếu chỉ, lộ trong cutscene; Rèn & Trận = cutscene ≤ 9 × 1 s, không tua, timescale ≤ 3×, timeline chung + vật phẩm gắn động; Chiến lợi phẩm = vàng + chọn 1/3; bỏ Sổ thí nghiệm; Menu chính; ma thuật hướng A; P4 thêm ngoại lệ "≤ 3 s ở 3×"
- **File**: Docs/GDD.md, Docs/design/{economy,run-flow,brewing,ui-ban-luyen}.md, Docs/ART.md, Docs/PLAN.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: các quyết định va chạm P3 (đối thủ ẩn, vàng thưởng), P4 (cutscene không tua), P1 (bỏ Sổ thí nghiệm) → ghi rủi ro + AC theo dõi ở PLAN/GDD
- **Còn lại**: spec UI các màn mới; ART §5 NPC. Người dùng chấp nhận 3 rủi ro kèm lý do (ghi PLAN §4); công thức giữ Magic theo tier → `balance`

### 2026-11-04 — ui Bàn luyện
- **Command**: /unity-game-studio ui
- **Đã làm**: spec Bàn luyện §1–8 (mục đích, element & trạng thái, bố cục 3 cột + anchor, tương tác, văn bản, chuyển động, accessibility, tiêu chí QA); ART §7; mockup artifact
- **File**: Docs/design/ui-ban-luyen.md (mới), Docs/GDD.md (§6.2, §8, §9), Docs/design/brewing.md, Docs/ART.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: ART §1 (khói cho dòng chưa soi) mâu thuẫn GDD §6.2 → người dùng quyết: `Magic` công khai, thuộc tính phụ vẫn ẩn và không có chỉ báo; nút Soi luôn bật để không thành chỉ báo
- **Còn lại**: spec các màn khác; kiểm glyph font; arch

### 2026-11-04 — art 8
- **Command**: /unity-game-studio art 8
- **Đã làm**: viết §8 Asset Standards — đặt tên/thư mục, kích thước, import settings, Sprite Atlas V2, hiển thị (Pixel Perfect Camera + Canvas Scaler), checklist nhận asset, ngân sách tạm
- **File**: Docs/ART.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: người dùng chọn Scale With Screen Size cho UI thay vì scale nguyên → ghi hệ quả pixel không đều ở độ phân giải lẻ + điểm kiểm tra 1366×768
- **Còn lại**: ngân sách chờ TECH §6 (`arch`); script kiểm palette ở `asset`

### 2026-11-04 — art (khảo sát phong cách)
- **Command**: /unity-game-studio art
- **Đã làm**: khung §0 (pixel art, 640×360 ×3, PPU 32, chữ hi-res, AI + chỉnh tay); §1 chọn hướng A "Lò là mặt trời" trong 3 hướng; §2 mood 8 trạng thái; §3 shape language theo loại vật phẩm; nháp §4 + trang swatch có grayscale/mô phỏng mù màu
- **File**: Docs/ART.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: AI tạo pixel art "giả" → quy trình downscale nearest + ép palette khóa; font pixel thiếu dấu tiếng Việt → chữ UI dùng font hi-res
- **Còn lại**: §4 đã tạm chốt; người dùng muốn cập nhật bố cục/mật độ màn hình sau (ART A1); viết §8

### 2026-11-03 — concept (khoảng cách) + setup check
- **Command**: /unity-game-studio concept, setup
- **Đã làm**: thêm nhóm công thức (`RecipeFamily`); khoảng cách là dữ liệu ẩn tuyệt đối; Sổ thí nghiệm chỉ ghi chuỗi nguyên liệu + tập phụ gia + kết quả; Sổ công thức mỗi nhóm một mục; bộ khởi đầu cấp `KnownFamilies`; kiểm tra lại Version Control
- **File**: Docs/GDD.md (§1, §3, §6.3, §9), Docs/design/brewing.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: Version Control vẫn `Unity Version Control`, file không đổi từ 21:17, đã bị commit ở e12665c; Editor đang mở
- **Còn lại**: sửa Version Control; duyệt GDD

### 2026-11-03 — gdd --quick (chốt câu hỏi mở)
- **Command**: /unity-game-studio (trả lời Open Questions)
- **Đã làm**: Sổ thí nghiệm → meta vĩnh viễn (gộp lưới trùng, `LabLogCap` 500, không tự xóa mục thành công); MVP không nhập seed; không hiện số nhịp 1–9
- **File**: Docs/GDD.md (§6.1, §6.3, §6.4, §7, §9), Docs/design/brewing.md, Docs/design/run-flow.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: Sổ thí nghiệm vĩnh viễn lộ khoảng cách công thức đã khám phá → ghi là hệ quả có chủ ý (P1), thêm câu hỏi cân bằng dài hạn
- **Còn lại**: duyệt GDD → Approved → arch

### 2026-11-03 — gdd --review
- **Command**: /unity-game-studio gdd --review
- **Đã làm**: soát GDD + 3 design → NEEDS REVISION (2 nghiêm trọng, 4 trung bình, 3 nhẹ); áp dụng toàn bộ sửa theo lựa chọn người dùng
- **File**: Docs/GDD.md, Docs/design/brewing.md, Docs/design/economy.md, Docs/design/run-flow.md, Docs/PLAN.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: R1 gộp Chợ vào Bàn luyện làm tràn Túi → lưới tính vào sức chứa; R2 thiếu nguồn biết công thức → người dùng chọn chỉ bộ khởi đầu, ghi rủi ro cao
- **Còn lại**: duyệt lần cuối → Approved

### 2026-11-03 — concept + gdd
- **Command**: /unity-game-studio concept → gdd
- **Đã làm**: viết GDD §1–9 từ demo_GDD.md; tinh chỉnh game flow + core loop; tách 3 GDD hệ thống
- **File**: Docs/GDD.md, Docs/design/brewing.md, Docs/design/economy.md, Docs/design/run-flow.md, Docs/PLAN.md, CLAUDE.md, Docs/BUILD.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: demo là CLAUDE.md của dự án đã tới M6 — chỉ lấy phần thiết kế; kiến trúc Alchemy.* để `arch` quyết
- **Còn lại**: duyệt GDD; Open Questions trong từng file

### 2026-11-03 — setup
- **Command**: /unity-game-studio setup (check + apply)
- **Đã làm**: check vệ sinh dự án; thêm `.DS_Store`, `.idea/`, `/[Aa]ssets/_Temp*` vào .gitignore; tạo Docs/BUILD.md
- **File**: .gitignore, Docs/BUILD.md
- **Test**: không có (chưa có test assembly)
- **Vấn đề gặp & cách xử lý**: Hub đổi Version Control mode → Unity Version Control; Editor đang mở nên người dùng tự đổi lại trong Project Settings. Scanner báo nhầm thiếu UserSettings trong .gitignore (thực tế đã có)
- **Còn lại**: xác nhận Visible Meta Files; asmdef/test asmdef, .gitattributes chưa làm (người dùng chưa chọn)

### 2026-11-03 — init
- **Command**: /unity-game-studio init
- **Đã làm**: quét dự án (Unity 6000.6.7f1, URP 2D, Input System, template trống); tạo CLAUDE.md + Docs/{GDD,ART,PLAN,TECH,TEST,LOG}.md
- **File**: CLAUDE.md, Docs/*.md
- **Test**: không có (chưa có code game, chưa có test assembly)
- **Vấn đề gặp & cách xử lý**: Version Control mode ≠ Visible Meta Files; .gitignore thiếu UserSettings → để `setup` xử lý
- **Còn lại**: concept, setup

## Decision Log

| Ngày | Quyết định | Lý do / phương án bị loại | Ai quyết | Tài liệu liên quan |
|---|---|---|---|---|
| 2026-11-04 | Kiến trúc: lõi C# thuần + Command/Reducer; Boot + 1 scene Game; save Newtonsoft JSON (thêm package); PCG32 + số nguyên; SO → ContentDb; ánh sáng sprite overlay; RunView lọc thông tin ẩn | Loại: SO event channel (khó tất định/lưu), scene additive, JsonUtility, URP 2D Light | Người dùng | TECH ADR-001…007 |
| 2026-11-03 | Nền tảng: Mobile, input touch | — | Người dùng | CLAUDE.md |
| 2026-11-03 | Rigor `standard`; Claude chỉ đề xuất commit message, không commit test-first | Mặc định khuyến nghị | Người dùng | CLAUDE.md §Git |
| 2026-11-03 | Kiến trúc tạm: SO event channel, không DI | Giả định mặc định, chờ `arch` | Claude (giả định) | TECH.md §1 |
| 2026-11-03 | Nền tảng đổi: WebGL trước, Mobile sau (thay cho Mobile ở init) | Theo demo GDD + vẫn giữ hướng mobile | Người dùng | GDD §1, CLAUDE.md |
| 2026-11-03 | Loại trang bị công bố ở Chiếu chỉ | Thua vì quyết định, không vì RNG (P2/P3) | Người dùng | GDD §9, run-flow.md |
| 2026-11-03 | Gộp phase: Bàn luyện (Chợ + Lò), Dâng (gồm Xưởng), Rèn & Trận | Nhịp liền mạch (P4) | Người dùng | GDD §3 |
| 2026-11-03 | Chạm để tua phát lại + Sổ thí nghiệm | P4, P1 | Người dùng | brewing.md, GDD §6.3 |
| 2026-11-03 | Sổ thí nghiệm lưu vĩnh viễn, **không ghi khoảng cách** | Khoảng cách chỉ để người chơi tự kiểm nghiệm | Người dùng | GDD §6.3 |
| 2026-11-03 | Nhóm công thức: cùng nguyên liệu + thứ tự, khoảng cách khác → thành phẩm khác; Sổ công thức theo nhóm, không lộ số biến thể | Làm khoảng cách thành trục khám phá chính | Người dùng | brewing.md, GDD §6.3 |
| 2026-11-03 | MVP không nhập seed | Đơn giản MVP; sau MVP cân nhắc chia sẻ seed | Người dùng | GDD §6.1 |
| 2026-11-03 | Không hiện số nhịp 1–9 trên lưới | Giữ cảm giác thủ công, không "toán học"; dạy qua phát lại | Người dùng | brewing.md |
| 2026-11-03 | Vật phẩm trên lưới tính vào sức chứa Túi | Chợ cùng màn lưới → tránh tràn Túi | Người dùng (review R1) | brewing.md, economy.md |
| 2026-11-03 | Nguồn biết công thức: chỉ bộ khởi đầu + tự nung; không gợi ý | Loại: Chiếu chỉ gợi ý, Mảnh công thức (giữ làm dự phòng nếu M0 thất bại) | Người dùng (review R2) | GDD §6.3, §4 |

## Changelog

### [Unreleased]
- **Added**: {{}}
- **Changed**: {{}}
- **Fixed**: {{}}

## Unity Log Digest

<!-- Tóm tắt lỗi/cảnh báo đáng chú ý gom từ Console / Editor.log / Player.log. Lỗi thật → tạo bug trong BUGS.md. -->

| Ngày | Nguồn | Loại | Thông điệp (rút gọn) | Số lần | Xử lý |
|---|---|---|---|---|---|
