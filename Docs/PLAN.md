# Plan — CrucibleV2 (tên tạm)

> Cấu hình quy trình thiết kế & triển khai: pipeline áp dụng cho từng feature, roadmap, backlog, trạng thái.
> Plan chi tiết của từng feature nằm ở `Docs/plans/<feature>.md` (tạo bằng `/unity-game-studio plan <feature>`).
> **Cập nhật**: 2026-11-04

## 1. Quy trình (pipeline)

### Toàn bộ gameplay (một lần, đầu dự án)
```
concept → systems → art (§1–4) → arch → [prototype hệ thống rủi ro] → gdd (MVP) → plan → build → test → playtest
```

### Mỗi feature
| Bước | Command | Vai trò | Đầu ra | Cổng qua bước |
|---|---|---|---|---|
| 1. Thiết kế | `gdd <system>` | Game Designer | GDD hệ thống | Acceptance Criteria đo được, không còn TBD chặn |
| 2. Hình ảnh / UI | `asset`, `ui` | Designer | Spec asset / UI | Chỉ khi feature có phần hiển thị mới |
| 3. Kỹ thuật | `arch` (nếu cần ADR) → `plan <feature>` | Developer | `Docs/plans/<feature>.md` | Testability PASS/WARN |
| 4. Xây dựng | `build <feature>` | Developer | Code + test | Test xanh, compile sạch |
| 5. Kiểm thử | `test <feature>` | QA | Test bổ sung, ảnh bằng chứng | Bằng chứng theo loại thay đổi |
| 6. Đóng | `log` | Tất cả | PLAN/LOG cập nhật | Trạng thái = Done |

Thay đổi nhỏ (tuning số, chỉnh luật nhỏ) có thể đi tắt: `balance` hoặc `gdd --quick` → `build`.

### Definition of Done (mặc định)
- [ ] Acceptance Criteria trong GDD đều có test hoặc bằng chứng.
- [ ] Compile sạch; toàn bộ test liên quan xanh.
- [ ] UI/visual có ảnh trong `Docs/evidence/`.
- [ ] Không còn TODO không có ghi chú `shortcut:`.
- [ ] Tài liệu cập nhật; commit theo quy ước.

## 2. Milestones

| Milestone | Mục tiêu (câu hỏi cần trả lời) | Hệ thống bao gồm | Hạn | Trạng thái |
|---|---|---|---|---|
| M0 Prototype | Giải mã khoảng cách công thức có thú vị không, hay chỉ là đoán mò? Người chơi có tự khám phá công thức ngoài bộ khởi đầu không (GDD §6.3 AC-7)? Kinh tế 0 vàng có kẹt/áp đảo không? | Lò nung (greybox 1 màn Bàn luyện, 6–8 công thức) + mô phỏng kinh tế bằng agent | TBD | Not Started |
| M1 Vertical Slice | Một run 8 Mùa trọn vẹn trên WebGL, người ngoài chơi không cần giải thích | Toàn bộ hệ thống MVP (GDD §4 #1–14), content tạm | TBD | Not Started |
| M2 Alpha | Đủ content MVP (tier 1–10, 3 bậc khó, 3 bộ khởi đầu), lưu/tiếp tục | + Meta, Lưu | TBD | Not Started |
| M3 Beta | Cân bằng (GreedyAgent, playtest), audio, polish | + Audio | TBD | Not Started |
| M4 Release | Phát hành WebGL | — | TBD | Not Started |

## 3. Backlog & trạng thái

| ID | Feature / việc | Hệ thống | Loại | Ưu tiên | Ước lượng | Trạng thái | Plan | Ghi chú |
|---|---|---|---|---|---|---|---|---|
| F-000 | Core foundation: ContentDb, RunState, Command/GameRules, StateHash, `StartRun` | #1, #9 | Logic | MVP | M | Done | [plans/core-foundation.md](plans/core-foundation.md) | TECH §3.1–3.5; 69 test EditMode xanh 2026-11-04 |
| F-001 | Seed & RNG streams | #1 | Logic | MVP | S | In Test | [plans/core-foundation.md](plans/core-foundation.md) | GDD §6.1 — nền xanh; AC-2/AC-3 cần Chợ/Chiếu chỉ (F-006/F-009), AC-1 đa nền tảng cần build WebGL |
| F-002 | Lò nung: lưới, chuỗi nạp, khớp công thức, `Quality` | #2 | Logic | MVP | M | Done | [plans/brewing.md](plans/brewing.md) | design/brewing.md; Quality số nguyên; 160 test EditMode xanh 2026-11-04; AC-4 (MagicChance)/6 → F-003, AC-7/11 → F-004, AC-8 soi/mua → F-006/F-008, AC-10 Bỏ qua → F-009 |
| F-003 | Kích hoạt ma thuật | #3 | Logic | MVP | S | Done | [plans/magic.md](plans/magic.md) | design/brewing.md; 3 bảng theo tier (Inherit/Cap/Retain), số liệu chốt ở `balance`; 196 test EditMode xanh 2026-11-04 |
| F-004 | BrewLog + phát lại, timescale, chạm để tua | #4 | Logic | MVP | S | Done | [plans/brewlog.md](plans/brewlog.md) | design/brewing.md; 255 test EditMode xanh 2026-11-04; phạm vi Core (`BrewLog`, `SetTimescale`) + `BrewReplayTimeline` (Runtime, thuần C#); **view phát lại → F-014** |
| F-005 | Túi | #5 | Logic | MVP | S | Done | [plans/bag.md](plans/bag.md) | design/economy.md; 312 test EditMode xanh 2026-11-04; `BagRules` (`HasRoom` Túi + lưới, `Expand` tới trần), `RunData.MaxBagCapacity` + validate, `ReorderCatalyst`; AC-2/AC-2b đầy đủ + `BagFull` → F-006, Nới Túi → F-011 |
| F-006 | Chợ & kinh tế | #6 | Logic | MVP | M | Done | [plans/market.md](plans/market.md) | design/economy.md; 430 test EditMode xanh 2026-11-04; `PriceFormula`, `MarketRules`, `Buy(slot, cell?)`/`Sell`/`Reroll`, `MarketData`, `ItemData.MarketPrice`; **F-009 phải gọi** `RollSeasonPrice` ở Decree + `RefreshShop` đầu lượt (AC-2b Bỏ qua, AC-2c, GDD §6.1 AC-2); `OnShopRefresh` → F-007; modifier giá + reroll miễn phí → F-011; bể theo Danh vọng → F-012; số `MarketPrice`/`UtilitySlotWeights` → balance |
| F-007 | Xúc tác & Effect primitive | #7 | Logic/Data | MVP | M | Designed | — | design/economy.md |
| F-008 | Thuộc tính ẩn & Kính soi | #8 | Logic | MVP | S | Designed | — | GDD §6.2; cấp SB/PD/HT cho `BattleStats.ForHero` ở `OfferingRules.Offer` (F-010 đang truyền 0) |
| F-009 | Vòng Mùa (state machine) | #9 | Logic | MVP | M | In Progress | [plans/run-loop.md](plans/run-loop.md) | design/run-flow.md; **phần 1 run-loop Done** 2026-11-04, 486 test EditMode xanh (Decree → Workbench ×3 → Offering: `SeasonRules`, `AcceptDecree`/`SkipTurn`/`AckBrewResult`, `RunState.Turn`, `RunData.TurnsPerSeason`; S rút lúc vào Decree); Resolution → Mùa kế / RunEnd đã làm ở F-010; **phần 2** chỉ còn AC-1 RandomAgent 1.000 run tới RunEnd (đã gỡ ngõ cụt Spoils — F-011 Phần A) |
| F-010 | Rèn, Trận, Uy tín | #10 | Logic | MVP | M | Done | [plans/forge-battle.md](plans/forge-battle.md) | design/run-flow.md, battle.md; 665 test EditMode xanh 2026-11-04; `ForgeRules.RollEquipment` (Decree: Market → Forge), `OfferingRules` (`Offer(uid)`/`DismissCourtier`, khay = view), `BattleStats` + `BattleSimulator` + `BattleLog` (SB/PD/HT ta = 0 tới **F-008**), `SeasonRules.AckBattle` đủ Resolution; `BattleData`/`EquipmentData`/`OpponentData`/`SeasonData`, `DifficultyData.OpponentStatPercent`; Spoils chưa có lệnh → **F-011** |
| F-011 | Chiến lợi phẩm & modifier Mùa | #11 | Logic/Data | MVP | M | Done | [plans/spoils.md](plans/spoils.md) | design/run-flow.md; 820 test EditMode xanh 2026-11-04. **A Spoils**: `RewardData`/`Reward`, `SeasonData.SpoilsGold/RewardPool/GuaranteeCatalyst`, `SpoilsRules` (stream `Reward`, lọc trước khi rút), `ChooseReward(i)`/`SkipReward` (Bỏ qua luôn dùng được). **B modifier**: `ModifierData`/`SeasonModifier`, `SeasonData.ModifierPool/NoModifierWeight`, `ModifierRules` (stream `Season` ở Decree, Mùa 1 không modifier), giá mua/bán/Kính/reroll + reroll miễn phí, `OpponentStats`, `ForgeWeight`. **F-013 DTO phải mang** `SpoilsOptions`, `ActiveModifier`, `NextModifier` (Id chuỗi); view màn Chiến lợi phẩm + dòng modifier Chiếu chỉ → F-014/F-015; số thật → balance |
| F-012 | Meta: Sổ công thức, Danh vọng, bậc khó, bộ khởi đầu | #12 | Logic/UI | MVP | M | Designed | — | GDD §6.3 |
| F-013 | Lưu & tiếp tục | #13 | Integration | MVP | M | Designed | — | GDD §6.4; DTO phải mang `RunState.Turn` (F-009), `RunState.LastBrew` + `MetaState.Timescale` (F-004), `SeasonPricePercent` + `Shop` (ô + `RerollCount`) (F-006), `DecreeEquipment` + `LastBattle` + `Outcome` (F-010); run `Ended` giữ `IsActive` → tạo `RunState` mới cho run sau |
| F-014 | UI/UX greybox các màn | #14 | UI | MVP | L | Designed | — | design/ui-ban-luyen.md, ui-season.md, ui-meta.md; còn: màn Kết thúc run, Trận (#17); + view phát lại BrewLog (từ F-004: highlight tick, chạm lò để tua, nút/phím timescale, SO `BrewTickSeconds`) |
| F-017 | Trận — mô phỏng tất định + phát lại 6 nhịp | #17 | Logic/UI | MVP | M | Designed | — | design/battle.md; **mô phỏng lõi chuyển sang F-010** (2026-11-04) — còn: lộ thuộc tính + ghi Sổ (AC-6), biểu cảm thợ rèn, timeline 6 nhịp (AC-3), view |

Trạng thái: Backlog → Designed → Planned → In Progress → In Test → Done (hoặc Blocked: lý do).
Ước lượng: S = một phiên, M = 2–3 phiên, L = 4+ phiên (L nên chia nhỏ).

## 4. Đang làm (sprint / tuần hiện tại)

**Mục tiêu**: duyệt GDD → `arch` → prototype M0 (Lò nung greybox)

- [ ] Duyệt GDD + 3 file design (đang In Review)
- [ ] `/unity-game-studio arch`
- [ ] `/unity-game-studio plan prototype-brewing`

**Rủi ro**: giải mã khoảng cách có thể thành đoán mò tốn tài nguyên (P1/P3) — kiểm ở M0.
**Rủi ro (2026-11-04)**: đối thủ ẩn ở Chiếu chỉ → người mới mất quyết định "dâng hay giữ" (theo dõi AC-10).
**Đã chấp nhận có chủ đích** (người dùng 2026-11-04):
- Bỏ Sổ thí nghiệm → người chơi tự nhớ / tự ghi — **mục tiêu**: nuôi nội dung ngoài game và cộng đồng chia sẻ
  công thức (GDD §1 USP).
- Vàng từ chiến lợi phẩm → vẫn khan hiếm vì Chợ chỉ bán tier ≤ 3; lên tier cao phải nung nhiều tầng và tích
  nhiều khoáng → tốn nhiều vàng.
- `Magic` công khai → mở route tier 10 cần Magic, nhưng **giữ Magic qua các tầng phải khó dần theo tier** →
  tier 10 vẫn khó. ⚠ Công thức truyền thừa hiện tại (`tier × 5%`) đi ngược ý này → `balance` phải sửa.

## 5. Đã cắt / hoãn

| Feature | Lý do | Ngày |
|---|---|---|
