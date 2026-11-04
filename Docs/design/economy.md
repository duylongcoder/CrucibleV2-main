# Kinh tế (Túi, Chợ, Xúc tác & Effect) — System Design

> **Trạng thái**: In Review (sửa sau review 2026-11-03) · **Cập nhật**: 2026-11-03
> **Pillar**: P3, P1 · **Ưu tiên**: MVP · **Lớp**: Core / Feature · **Phụ thuộc**: Seed & RNG (GDD §6.1), Lò nung (brewing.md)
> GDD.md §4 hệ thống #5, #6, #7. Số liệu là tạm, đọc từ config.

## Overview

Run bắt đầu với **0 vàng** và một bộ nguyên liệu nhỏ trong **Túi** — kho duy nhất, nhìn thấy toàn bộ, sức chứa
có hạn. Vàng chỉ đến từ **bán**. **Chợ** là ngăn kéo trong màn Bàn luyện, làm mới hàng mỗi lượt nung, chỉ bán
nguyên liệu gốc tier 1–3 → tier cao phải tự nung. **Xúc tác** là vật phẩm giữ suốt run (kiểu Joker), sửa kết
quả lượt nung qua Effect primitive. Áp lực: bán gì để mua gì, nung thử hay giữ để dâng.

## Player Fantasy

Nhà giả kim tháo vát: bán bớt khoáng thừa lấy vốn, săn đúng món ở chợ, xây một bộ Xúc tác "độc quyền". P3.

## Detailed Rules

### Túi
- Kho duy nhất; người chơi thấy toàn bộ nội dung. Không có tay bài/rút/đổi.
- Sức chứa khởi đầu 14 (trần 24), nới bằng chiến lợi phẩm "Nới Túi".
- Mọi vật phẩm (khoáng, phụ gia, Kính soi, Xúc tác, phế phẩm) chiếm 1 chỗ; không gộp chồng.
- Túi và lưới là hai vùng hiển thị riêng, nhưng **dùng chung sức chứa**: `số vật phẩm Túi + số vật phẩm trên lưới
  ≤ sức chứa`. (Sửa sau review: Chợ nằm cùng màn với lưới, người chơi mua được sau khi xếp lưới — nếu lưới
  không tính, thành phẩm hoặc vật phẩm trả về sẽ tràn Túi.)
- Đầy → không mua / nhận thêm được (chiến lợi phẩm vật phẩm bị vô hiệu nếu đầy — xem Edge Cases).

### Chợ
- Là ngăn kéo trong Bàn luyện; dùng được trong trạng thái Editing của mỗi lượt (không trong Brewing, không ở
  Chiếu chỉ, Dâng, Rèn & Trận, Chiến lợi phẩm).
- **Hàng làm mới đầu mỗi lượt Bàn luyện** (3 lần/Mùa, stream `Shop`). **5 ô cố định** cả run (chốt 2026-11-04):
  - **4 ô nguyên liệu**: khoáng / phụ gia gốc (quy tắc bên dưới); **được trùng** món trong cùng lượt.
  - **1 ô tiện ích**: Kính soi hoặc Xúc tác (trọng số `UtilitySlotWeights`, TBD); đã đủ `CatalystSlots` → chỉ rút
    Kính soi.
  - Reroll làm mới cả 5 ô. Danh vọng / modifier **không** thêm ô (MVP).
- **Bán**: khoáng & phụ gia **gốc** (không có `Recipe`) tier ≤ `ShopMaxTier` (3), tỉ lệ tier 1/2/3 = 70/25/5;
  Kính soi; Xúc tác. **Không bao giờ** bán tier 4+ hay item có `Recipe`.
- **Mua**: giá = `MarketPrice × HệSốMùa% × Modifier%`, ≥ 1. Mỗi món mua một lần (ô hàng biến mất).
- **Bán (người chơi)**: mọi thứ, kể cả khoáng tier cao, Xúc tác. Giá bán = giá mua tương ứng × 75%, ≥ 0.
  Phế phẩm giá cố định thấp (`ScrapSellPrice`, **không** nhân hệ số Mùa — chốt 2026-11-04). Giá **không** phụ thuộc
  thuộc tính, `Quality` hay `Magic`. Bán được cả từ lưới.
- Bể ô nguyên liệu: khoáng + phụ gia gốc **gộp chung** trong mỗi tier; tier không có hàng bị bỏ khi rút (trọng số chia
  lại). Xúc tác đang sở hữu **vẫn có thể** xuất hiện ở ô tiện ích. Mua kéo thẳng lên ô lưới là một lệnh (chốt 2026-11-04).
- **Reroll**: 2 vàng, +1 mỗi lần trong cùng lượt, reset mỗi lượt.
- **Hệ số giá Mùa**: rút ở Chiếu chỉ (stream `Market`), số nguyên % trong 70–130, áp cho cả Mùa.
- Vàng giữ qua các lượt và Mùa; mất khi run kết thúc. Nguồn vàng: **bán** + **phần thưởng trận thắng** (Chiến lợi
  phẩm, run-flow.md — sửa 2026-11-04). Không lãi. Bán hết vốn là lựa chọn
  hợp lệ.

### Xúc tác & Effect primitive
- Xúc tác: mua ở Chợ hoặc nhận từ chiến lợi phẩm; nằm trong Túi (chiếm chỗ); tối đa **5** Xúc tác cùng lúc
  (`CatalystSlots`) — mua/nhận thêm bị chặn. Không đặt lên lưới.
- Mọi Xúc tác trong Túi đều có hiệu lực, chạy theo **thứ tự trong Túi** (người chơi sắp xếp được trên kệ, chỉ ở bước
  Editing). **Chỉ thứ tự Xúc tác có nghĩa với luật** — thứ tự các vật phẩm khác trong Túi do view tự sắp (chốt 2026-11-04).
- Mọi hiệu ứng = **Trigger + Condition + Action**, chỉ từ whitelist:
  - Trigger: `OnBrewStart`, `OnRecipeMatch`, `OnBrewFail`, `OnBrewEnd`, `OnShopRefresh` (`OnSeasonStart` sau MVP)
    — `OnShopRefresh` kích hoạt **đúng 1 lần mỗi lượt Bàn luyện**, khi hàng Chợ làm mới đầu lượt; mở/đóng ngăn
    Chợ và reroll **không** kích hoạt.
  - Condition: `IngredientIs`, `RecipeIs`, `AdditiveCount`, `Chance`
  - Action: `AddEssence`, `AddPotency`, `MulPotency` (≤ ×5/lần), `AddStat` (có giới hạn), `GainGold`,
    `CreateIngredient` (vào Túi, chặn nếu đầy, giới hạn số lần), `AddMagicChance` (chỉ phụ gia; 1–50%/phụ gia)
- Ràng buộc: `AddEssence/AddPotency/MulPotency/AddStat` chỉ đi với `OnRecipeMatch`; `OnShopRefresh` chỉ dùng
  điều kiện `Chance`; phụ gia chỉ dùng `AddMagicChance` — chính là field `MagicChance` của phụ gia, không khai báo
  Effect (chốt 2026-11-04, F-007 D1). Mọi điều kiện `Chance` luôn được tung.
- Thứ tự kích hoạt (chốt 2026-11-04, F-007 D3): pha `OnBrewStart` → `OnRecipeMatch`/`OnBrewFail` → `OnBrewEnd`; trong pha
  theo thứ tự Túi, rồi thứ tự khai báo. Mỗi Effect: 1 action, ≤ 3 điều kiện khác loại (AND); Xúc tác ≤ 3 Effect.
- `AddStat` cộng vào dòng có sẵn hoặc thêm dòng mới (kẹp trần), dòng vẫn ẩn (D6). `GainGold`/`CreateIngredient` áp sau
  khi kết quả nung vào Túi; `CreateIngredient` tạo 1 nguyên liệu gốc, "giới hạn số lần" = mỗi trigger ≤ 1 lần / lượt (D7).
- Thêm content = thêm data. Content cần primitive mới → đề xuất thiết kế, không tự thêm.

### Trạng thái
Không có state machine riêng; Chợ sống trong trạng thái Editing của Bàn luyện (brewing.md).

## Formulas

| Tên | Công thức | Biến (khoảng) | Ví dụ |
|---|---|---|---|
| Giá mua | `max(1, ⌊MarketPrice × S × M / 10000⌋)` | S hệ số Mùa 70–130 %; M = modifier giá mua % (Kính soi: modifier giá Kính **thay** giá mua) 1–1000 | 10 × 120 × 100 / 10000 = 12 |
| Giá bán | `⌊Giá mua × SellRate × M_bán / 10000⌋` | SellRate 75 %; M_bán modifier % với `SellRate × M_bán < 10000` (mua-bán luôn lỗ) | 12 × 75 × 100 / 10000 = 9 |
| Reroll lần k (k từ 0) | `k < F` → 0; ngược lại `⌊(RerollBase + (k − F) × RerollStep) × M_reroll / 100⌋` | F reroll miễn phí 0–2 (modifier); RerollBase 2, RerollStep 1 | F = 0: 2, 3, 4; F = 2: 0, 0, 2, 3 |

## Edge Cases

- **0 vàng, Túi rỗng nguyên liệu** → chỉ còn Bỏ qua lượt; tới Dâng không có gì → tự động Thua. Run vẫn đi tới
  kết thúc (không kẹt).
- **Túi + lưới đầy** khi mua → từ chối (gỡ bớt khỏi lưới cũng không giúp — vật phẩm chỉ đổi chỗ). Chiến lợi phẩm là vật phẩm khi Túi đầy → lựa chọn đó bị vô hiệu (hiển thị
  "Túi đầy"); nếu cả 3 vô hiệu → cho phép nhận không gì (xem run-flow.md).
- **Mua bán lặp sinh lời**: giá bán = 75% giá mua cùng Mùa → mua rồi bán ngay luôn lỗ. Hệ số Mùa thay đổi giữa
  các Mùa → mua rẻ Mùa này, bán đắt Mùa sau là chiến lược hợp lệ nhưng chiếm chỗ Túi (P3).
- **Nung khoáng tier cao rồi bán**: hợp lệ; giá tier cao phải > tổng giá nguyên liệu (thưởng cho kỹ năng) nhưng
  không tới mức "nung–bán" áp đảo việc dâng (kiểm bằng GreedyAgent ở `balance`).
- **Bộ khởi đầu nhiều vật phẩm hơn sức chứa khởi đầu** → lỗi content (validator từ chối), không cắt bớt (chốt 2026-11-04).
- **Reroll vô hạn** → chi phí tăng tuyến tính, giới hạn bởi vàng.
- **6 Xúc tác** → không thể; mua/nhận thứ 6 bị chặn.
- **Thứ tự Xúc tác**: chỉ ảnh hưởng hiệu ứng có thứ tự (vd `GainGold` trước `CreateIngredient`); `Quality` không
  phụ thuộc thứ tự (Add trước, Mul sau).

## Dependencies

| Hệ thống | Hướng | Hợp đồng |
|---|---|---|
| Seed & RNG | cần | `Shop` (hàng, `Chance` của `OnShopRefresh`), `Market` (hệ số Mùa) |
| Lò nung | hai chiều | Túi cấp/nhận vật phẩm; Xúc tác chạy Effect theo trigger lượt nung |
| Vòng Mùa | cần | biết lượt hiện tại (làm mới hàng), modifier Mùa (giá) |
| Chiến lợi phẩm | cần | Nới Túi, Xúc tác, Khoáng hiếm vào Túi |
| Meta | cung cấp | sự kiện mua/nhận → `ItemUnlocked`; Danh vọng mở nguyên liệu mới vào bể Chợ |

## Tuning Knobs

| Knob | Loại | Mặc định | Khoảng an toàn | Lý do | Nơi lưu |
|---|---|---|---|---|---|
| Sức chứa Túi / trần | gate | 14 / 24 | 10–30 | Đủ chỗ cho ~3 lượt nung + dự trữ | `RunDef` |
| `ShopMaxTier` | gate | 3 | cố định | Tier cao phải tự nung | `RunDef` |
| Tỉ lệ tier Chợ | curve | 70/25/5 | — | | `RunDef` |
| Số ô hàng / lượt | gate | 5 (4 nguyên liệu + 1 tiện ích) | 4–6 (layout tối đa 6) | Đủ lựa chọn mà vẫn khan hiếm; kiểm ở M0 | `RunDef` |
| `UtilitySlotWeights` Kính / Xúc tác | curve | TBD | — | | `RunDef` |
| Biên độ giá Mùa | curve | 70–130 % | 50–150 | Tạo cơ hội buôn giữa Mùa | `RunDef` |
| SellRate | curve | 75 % | 50–90 | < 100 để chặn mua-bán sinh lời | `RunDef` |
| Reroll base / bước | gate | 2 / +1 | — | | `RunDef` |
| `CatalystSlots` | gate | 5 | 3–6 | | `RunDef` |
| `MarketPrice` từng item | curve | TBD | — | Chốt ở `balance` | `ItemDef` |

## Visual / Audio / Game Feel

Tiếng xu khi bán/mua; ô hàng mới "lật" khi làm mới/reroll; Túi đầy rung nhẹ + màu cảnh báo khi cố mua.

## UI Requirements

- Túi: lưới icon, đếm `x / sức chứa`; kéo Xúc tác trên kệ để sắp thứ tự kích hoạt.
- Chợ: ngăn kéo cạnh lưới; mỗi ô hàng có giá; nút reroll hiện giá lần kế; vùng "kéo vào để bán" hoặc nút bán
  trên tooltip vật phẩm.
- Hệ số giá Mùa hiển thị định tính (↑ / ↓ so với giá chuẩn).

## Acceptance Criteria

- [x] AC-1: Chợ không bao giờ bày bán vật phẩm tier > 3 hoặc có `Recipe` (fuzz 10.000 lượt). (2026-11-04: `Agent_FiveHundredRuns_StepInvariantsHold` I4 — 10.617 lượt + mọi reroll qua 1.000 run)
- [ ] AC-2: Túi + lưới = sức chứa → mua bị từ chối, vàng không đổi.
- [x] AC-2b: Xếp lưới, mua tới đầy, rồi Nung hoặc Bỏ qua → số vật phẩm Túi + lưới ≤ sức chứa sau mọi bước. (2026-11-04: F-006 + `Apply_SkipTurnWithGridItems_KeepsUsedSlots`, `RunLoopInvariantTest`)
- [x] AC-2c: `OnShopRefresh` kích hoạt đúng 3 lần mỗi Mùa bất kể số lần mở ngăn Chợ / reroll. (2026-11-04: `Apply_SeasonWithRerolls_ShopRefreshFiresOncePerTurn`)
- [ ] AC-3: Bán 2 thành phẩm cùng loại khác thuộc tính → cùng giá.
- [x] AC-4: Giá mua ≥ 1, giá bán ≥ 0, vàng ≥ 0 sau mọi hành động. (2026-11-04: `Agent_FiveHundredRuns_StepInvariantsHold` I4, sau mỗi lệnh của 1.000 run)
- [ ] AC-5: Reroll lần 1/2/3 trong một lượt có giá 2/3/4; lượt sau về 2.
- [ ] AC-6: Mỗi lần làm mới / reroll → đúng 5 ô: 4 ô nguyên liệu gốc + 1 ô Kính soi hoặc Xúc tác; đủ 5 Xúc tác → ô tiện ích luôn là Kính soi.
- [x] AC-6: Mua Xúc tác thứ 6 bị từ chối. (`Apply_BuyCatalystWhenSlotsFull_RejectsWithCatalystSlotsFull`, `Apply_ChooseCatalystWhenSlotsFull_RejectsCatalystSlotsFull`)
- [ ] AC-7: Mua rồi bán ngay cùng món → vàng giảm (không có vòng sinh lời trong một lượt).
- [ ] AC-8 (cân bằng): GreedyAgent "chỉ nung để bán, dâng thứ rẻ nhất" có tỉ lệ thắng run thấp hơn agent cân bằng.

## Open Questions

- Số ô hàng mỗi lượt và giá cụ thể → `balance`.
- Bán Xúc tác: được (mặc định) — có cần giá đặc biệt không?
- Thuốc thử (Reagent): sau MVP; validator chặn content Reagent tới lúc đó.
