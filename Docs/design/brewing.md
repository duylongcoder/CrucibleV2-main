# Lò nung (lưới, chuỗi nạp, khớp công thức, ma thuật, phát lại) — System Design

> **Trạng thái**: In Review (sửa sau review 2026-11-03) · **Cập nhật**: 2026-11-03
> **Pillar**: P1, P2 · **Ưu tiên**: MVP · **Lớp**: Core · **Phụ thuộc**: Seed & RNG (GDD §6.1), Túi (economy.md)
> GDD.md §4 hệ thống #2, #3, #4. Số liệu là tạm, đọc từ config.

## Overview

Người chơi xếp nguyên liệu từ Túi lên lưới 3×3 rồi nhấn **Nung**. Lưới không có ý nghĩa không gian: nó là
**dòng thời gian viết gọn** — 9 ô đọc theo thứ tự quét hàng thành 9 nhịp (tick); ô trống là một nhịp chờ.
Chuỗi nhịp khớp đúng một công thức (đúng nguyên liệu, đúng thứ tự, **đúng khoảng cách**) → ra thành phẩm;
không khớp → phế phẩm. Khoảng cách là bí mật của công thức. Phụ gia có thể lấp các nhịp chờ mà không làm sai
công thức; chúng chỉ tăng xác suất **Kích hoạt ma thuật** — cửa vào nhánh Khoáng Tối thượng. Kết quả tính ngay
khi Nung; phần hiển thị phát lại từng nhịp và có thể tua.

## Player Fantasy

Canh lửa đúng nhịp như một thợ cả: "thả sắt, chờ một nhịp, thả than…" — rồi khoảnh khắc thành phẩm hiện ra
(hoặc một cục phế phẩm nhắc rằng nhịp còn sai). Discovery + Challenge.

## Detailed Rules

### Luật cốt lõi

**Lưới**
- Cố định **3×3 = 9 ô = 9 tick** (config toàn game). Không mở rộng/thu nhỏ.
- Mỗi ô chứa tối đa **1** vật phẩm. Đặt được: Khoáng (thành phẩm đạt chuẩn hoặc nguyên liệu gốc), Phụ gia.
  **Không** đặt được: Phế phẩm, Kính soi, Xúc tác, Thuốc thử.
- Đặt lên lưới = rút khỏi Túi; gỡ khỏi lưới = trả về Túi. **Vật phẩm trên lưới vẫn tính vào sức chứa Túi**
  (Túi + lưới ≤ sức chứa — economy.md) → gỡ/trả về và nhận kết quả luôn vừa. Tự do thêm/gỡ/thay/di
  chuyển/đổi chỗ tới khi nhấn Nung. Không bắt buộc lấp đầy. Cho phép trùng loại.
- Mỗi lượt Bàn luyện bắt đầu với lưới trống. Rời Bàn luyện bằng Bỏ qua lượt → vật phẩm trên lưới trả về Túi.
- **Nung = khóa lưới**: mọi lệnh chỉnh sửa lưới bị từ chối cho tới khi lượt kết thúc.

**Chuỗi nạp (`FeedSequence`)** — đọc trái → phải, trên → dưới; phần tử `i` = tick `i`:
```
|   | A |   |
| B |   |   |     →  [ _, A, _, B, _, _, C, D, E ]
| C | D | E |
```

**Công thức** (`ItemDef.Recipe`, nhúng trong item thành phẩm):
- `Pattern`: danh sách slot; mỗi slot là một item id hoặc `Gap`. Slot đầu và cuối luôn là nguyên liệu; độ dài
  ≤ 9. Slot nguyên liệu có thể mang cờ `RequiresMagic`.
- `AllowedAdditives`: phụ gia được phép đứng ở **bất kỳ** slot `Gap` nào.
- `BaseEssence`, các dòng thuộc tính khởi đầu.
- `Recipe` = null → nguyên liệu gốc (chỉ từ Chợ / bộ khởi đầu / chiến lợi phẩm).
- Công thức là content cố định: giống nhau ở mọi run, seed, bậc khó; modifier không bao giờ đổi công thức.

**Nhóm công thức** (`RecipeFamily`) — khái niệm thiết kế trung tâm (chốt 2026-11-03):
- Nhóm = tập các công thức có **cùng chuỗi nguyên liệu theo thứ tự** (bỏ `Gap`). Các công thức trong nhóm chỉ
  khác nhau ở **khoảng cách** (số và vị trí `Gap`) → mỗi biến thể ra một thành phẩm khác.
  Ví dụ nhóm `Sắt → Than`: `[Sắt, Than]` → Thép · `[Sắt, Gap, Than]` → Gang · `[Sắt, Gap, Gap, Than]` → Phế phẩm
  (không có biến thể).
- Một nhóm có 1..N biến thể (content quyết định). Biết một biến thể **không** cho biết nhóm còn biến thể nào.
- **Khoảng cách là dữ liệu ẩn tuyệt đối**: người chơi chỉ biết bằng tự kiểm nghiệm (xếp, nung, quan sát). Không
  màn hình, sổ, tooltip, thông báo, log release hay dữ liệu meta nào hiển thị/ghi lại khoảng cách của một công
  thức — kể cả công thức người chơi đã nung thành công (GDD §6.3). Lưới đang xếp trên màn hình là ngoại lệ duy
  nhất (người chơi đang tự thao tác).

**Khớp công thức (`RecipeMatcher`)**
1. Bỏ các ô trống ở đầu `FeedSequence`.
2. So từng slot của `Pattern`: slot nguyên liệu phải đúng item đó (và đã Kích hoạt ma thuật nếu slot có
   `RequiresMagic`); slot `Gap` phải trống **hoặc** là phụ gia nằm trong `AllowedAdditives`.
3. Mọi ô sau khi hết `Pattern` phải trống.
4. Khớp đúng 1 công thức → tạo `Product` (xem Formulas), tung ma thuật + thuộc tính (stream `Chance`), chạy
   Effect của Xúc tác (`OnRecipeMatch`). Không khớp → 1 Phế phẩm (`ScrapDef`), chạy `OnBrewFail`.
5. Validator content đảm bảo không có chuỗi nào khớp 2 công thức.

Hệ quả có chủ ý: cùng nhóm, khác khoảng cách → khác thành phẩm (hoặc phế phẩm nếu không có biến thể đó); có/không có
phụ gia hợp lệ trong khoảng trống → **cùng** thành phẩm, cùng `Quality` cơ bản; không có luật thứ tự tier
(thành phẩm có thể cao/ngang/thấp hơn nguyên liệu).

**Thành phẩm & tiêu hao**
- Lượt nung có ≥ 1 vật phẩm trên lưới → đúng **1** kết quả (thành phẩm hoặc phế phẩm) vào Túi.
- Lưới trống mà nhấn Nung → bị từ chối (dùng Bỏ qua lượt).
- **Mọi** vật phẩm trên lưới (kể cả phụ gia) bị tiêu hao khi Nung.
- Vì ≥ 1 vật phẩm trên lưới (đã tính vào sức chứa) bị tiêu hao, kết quả luôn vừa Túi — kể cả khi người chơi
  mua thêm sau khi xếp lưới.
- Thành phẩm đạt chuẩn dùng được làm nguyên liệu (chuỗi tầng); phế phẩm thì không, chỉ bán được.

### Kích hoạt ma thuật

- Mỗi lượt nung khớp công thức → tung **một lần** (stream `Chance`) xem thành phẩm có dòng `Magic` không.
- `MagicChance% = min(Trần, Σ truyền thừa + Σ phụ gia)`, số nguyên phần trăm:
  - *Truyền thừa*: mỗi nguyên liệu trên lưới **đã kích hoạt ma thuật** góp `tier × 5%`.
  - *Phụ gia*: tổng `MagicChance` của mọi phụ gia trong khoảng trống (tier 1/2/3 = 10/20/30%).
  - Không có cả hai → 0%.
- `Magic` là thuộc tính **công khai** (sửa 2026-11-04, GDD §6.2): thấy ngay khi ra lò qua hiệu ứng ma thuật trên
  sprite, không qua Kính soi. MVP: không cộng sức mạnh trang bị.
- Nhánh Khoáng Tối thượng dùng slot `RequiresMagic`; nguyên liệu chưa kích hoạt ở slot đó → phế phẩm, không
  báo lý do (người chơi tự thấy nguyên liệu nào có/không có hiệu ứng ma thuật).

### Nhật ký lượt nung (`BrewLog`) & phát lại

- Khi Nung, toàn bộ lượt được tính **ngay lập tức** (lưu xong trước khi phát lại). `BrewLog` chỉ chứa: sự kiện
  từng tick ("nạp X" / "chờ") + **một** sự kiện kết quả cuối.
- Với cùng một lưới, `BrewLog` khớp và hỏng giống hệt nhau tới trước sự kiện kết quả.
- Phát lại: phát **mọi tick từ vật phẩm đầu tới vật phẩm cuối**, kể cả nhịp chờ ở giữa; không phát ô trống
  trước vật phẩm đầu/sau vật phẩm cuối; không dừng sớm; kết quả chỉ hiện sau vật phẩm cuối.
- Nhịp: `BrewTickSeconds` = 3 s ở 1×; timescale 1× / 2× / 3× (người chơi chọn, nhớ lựa chọn).
- **Chạm để tua** (mới): chạm/click bất kỳ đâu trên lò khi đang phát lại → nhảy thẳng tới kết quả. Không lộ
  thông tin vì kết quả vốn chỉ hiện sau vật phẩm cuối, và độ dài phát lại phụ thuộc lưới người chơi tự xếp
  (không phụ thuộc công thức).
- `MeltTicks` (fiction: kim loại "nóng chảy" mất bao lâu) chỉ để hiển thị, không được tương quan với công thức.

### Trạng thái & chuyển trạng thái (một lượt Bàn luyện)

| Trạng thái | Sự kiện | Trạng thái kế | Ghi chú |
|---|---|---|---|
| Editing | Đặt / gỡ / di chuyển / mua / bán / soi | Editing | Lưới mở |
| Editing | Nung (lưới ≥ 1 vật phẩm) | Brewing | Khóa lưới; kết quả đã tính + đã lưu |
| Editing | Nung (lưới trống) | Editing | Từ chối |
| Editing | Bỏ qua lượt | TurnEnd | Vật phẩm trên lưới về Túi |
| Brewing | Phát lại xong / chạm để tua | Result | Hiện thành phẩm/phế phẩm; thành phẩm mới → Sổ công thức |
| Brewing | Lệnh chỉnh lưới / soi / mua bán | Brewing | Từ chối |
| Result | Tiếp tục | TurnEnd | |
| TurnEnd | — | Editing (lượt kế) hoặc Dâng (sau lượt 3) | Xem run-flow.md |

### Tương tác với hệ thống khác
- Túi (economy.md): rút/trả vật phẩm; nhận kết quả.
- Xúc tác (economy.md): Effect chạy ở `OnBrewStart`, `OnRecipeMatch`, `OnBrewFail`, `OnBrewEnd`.
- Thuộc tính & Kính soi (GDD §6.2): Lò tạo dòng thuộc tính; Kính soi lộ chúng.
- Meta (GDD §6.3): nung ra → mở `ItemUnlocked` + `RecipeUnlocked`, thêm thành phẩm vào mục nhóm của Sổ công
  thức. Không có nhật ký lượt nung (Sổ thí nghiệm đã bỏ 2026-11-04).

## Formulas

| Tên | Công thức | Biến (khoảng) | Ví dụ |
|---|---|---|---|
| `Quality` | `Q₀ = Σ Essence × (1 + Σ AddPotency)`; rồi với từng `MulPotency%` **sắp tăng dần**: `Q = ⌊Q × MulPotency% / 100⌋`; mỗi bước `min(QualityCap, Q)` — số nguyên `long` (ADR-004, chốt 2026-11-04) | Essence ≥ 0; AddPotency ≥ 0; MulPotency% 100–500 mỗi lần; `QualityCap` 10¹⁵ (config) | Essence 18, +3 Potency, ×150% → 18 × 4 = 72 → ⌊72 × 150 / 100⌋ = **108** |
| `MagicChance%` | `min(Cap, Σ_{ngl ma thuật} tier×5 + Σ_{phụ gia} MagicChance)` | Cap 90; tier 1–10; MagicChance phụ gia 1–50 | 1 ngl ma thuật tier 4 (20) + phụ gia tier 2 (20) + tier 1 (10) = **50%** |
| Thời lượng phát lại | `(iCuối − iĐầu + 1) × BrewTickSeconds / timescale` | i 0–8; timescale 1–3 | Lưới đầy ở 1× = 27 s; ở 3× = 9 s |

- Essence khởi đầu = `BaseEssence`; chỉ Xúc tác (và Thuốc thử sau MVP) cộng thêm. **Phụ gia không cộng
  Essence/Potency.**
- Mọi `AddPotency` áp trước, mọi `MulPotency` áp sau, bất kể thứ tự kích hoạt.

## Edge Cases

- **Lưới chỉ có phụ gia** → không khớp (slot đầu/cuối công thức luôn là nguyên liệu) → phế phẩm; phụ gia mất.
- **Nguyên liệu ở ô 9 (tick cuối)**, công thức dài hơn phần còn lại → không khớp → phế phẩm.
- **Phụ gia không thuộc `AllowedAdditives`** trong khoảng trống → phế phẩm (không báo lý do).
- **Phụ gia đặt ở vị trí slot nguyên liệu** → phế phẩm.
- **Hết nguyên liệu và không có vàng** → chỉ còn Bỏ qua lượt (luôn hợp lệ — không kẹt).
- **Xếp lưới rồi mua thêm tới đầy**: Túi + lưới = sức chứa → Chợ từ chối mua tiếp; Nung → lưới tiêu hao ≥ 1, kết
  quả vừa; Bỏ qua → vật phẩm về Túi, vừa. Xúc tác `CreateIngredient` khi Túi + lưới đầy → bị chặn, không tạo.
- **MagicChance vượt trần** → clamp 90%. Nhiều phụ gia cộng dồn → vẫn clamp.
- **Spam Nung / input dồn dập** trong lúc Brewing → lệnh thứ hai bị từ chối.
- **Đóng tab giữa lúc phát lại** → kết quả đã lưu; mở lại vào Result.
- **Chiến lược áp đảo**: nhồi phụ gia vào mọi khoảng trống → chỉ tăng ma thuật, không tăng `Quality` →
  không áp đảo trận; chi phí phụ gia giới hạn. Thử mò mọi khoảng cách → tốn nguyên liệu (P3); Sổ thí
  nghiệm chỉ cho biết chuỗi nào đã ra những kết quả nào, **không** nhớ hộ khoảng cách — người chơi phải tự nhớ.

## Dependencies

| Hệ thống | Hướng | Hợp đồng (cung cấp / cần) |
|---|---|---|
| Seed & RNG | cần | stream `Chance` cho ma thuật + thuộc tính |
| Túi | hai chiều | rút/trả vật phẩm; đặt kết quả |
| Xúc tác & Effect | cần | chạy Effect theo trigger lượt nung |
| Thuộc tính & Kính soi | cung cấp | dòng thuộc tính của `Product` |
| Vòng Mùa | cung cấp | sự kiện kết thúc lượt; đếm lượt (nung + bỏ qua ≤ 3) |
| Meta | cung cấp | sự kiện "nung ra X" (kèm nhóm) — **không** chứa vị trí ô / khoảng cách |
| UI | cung cấp | `BrewLog` (không rò rỉ) để phát lại |

## Tuning Knobs

| Knob | Loại | Mặc định | Khoảng an toàn | Lý do | Nơi lưu |
|---|---|---|---|---|---|
| Kích thước lưới | gate | 3×3 | cố định MVP | Giới hạn công thức ≤ 9 tick → chiều sâu từ chuỗi tầng | config toàn game |
| `BrewTickSeconds` | feel | 3 s | 1–4 s | Đủ chậm để "nghe" nhịp; tua được | config Presentation |
| Timescale | feel | 1× / 2× / 3× | — | | config Presentation |
| Trần ma thuật | curve | 90% | 50–95% | Luôn còn rủi ro | config |
| Truyền thừa | curve | tier × 5% | 2–10 / tier | Ngl tier cao đã ma thuật → dễ giữ nhánh | config |
| `MagicChance` phụ gia t1/t2/t3 | curve | 10/20/30% | 1–50 / phụ gia | | `ItemDef` (Additive) |

## Visual / Audio / Game Feel

- Mỗi tick: vật phẩm "rơi" vào lò + âm nạp riêng theo loại; nhịp chờ: âm lửa/nhịp đập trống — để người chơi
  "nghe" được nhịp mình đã xếp.
- Thành phẩm: lóe sáng + âm thăng; phế phẩm: khói xám + âm trầm. Hai kết quả phải khác hẳn (đọc được < 0.5 s).
- Không hiệu ứng nào khác nhau giữa lượt khớp và hỏng **trước** sự kiện kết quả.

## UI Requirements

- Lưới 3×3 lớn ở trung tâm, ô trống hiển thị rõ là "nhịp chờ". **Không** hiện số nhịp 1–9 trên ô (chốt
  2026-11-03); thứ tự quét hàng được dạy bằng phát lại — highlight lần lượt từng ô đúng thứ tự — từ lượt nung
  đầu tiên.
- Nút Nung (vô hiệu khi lưới trống), Bỏ qua lượt (luôn bật, có xác nhận nếu lưới có vật phẩm).
- Khi phát lại: highlight tick hiện tại; nút timescale; gợi ý "chạm để tua".
- Phế phẩm: chỉ ghi "Phế phẩm" — không "gần đúng", không chỉ ô sai.
- Thành phẩm có `Magic`: hiệu ứng ma thuật trên sprite ở mọi nơi vật phẩm xuất hiện (lưới, Túi, Chợ, Bàn soi).

## Acceptance Criteria

**Chức năng**
- [ ] AC-1: Ví dụ `Quality` ra đúng 108 (Essence 18, +3, ×1.5) bất kể thứ tự áp Effect.
- [ ] AC-2: Cùng nguyên liệu A, B với khoảng cách 0 và 1 → hai thành phẩm khác nhau (theo content test).
- [ ] AC-3: Lệch 1 tick so với công thức → phế phẩm (trừ khi trùng đúng một biến thể khác của nhóm).
- [ ] AC-3b: Nhóm `Sắt → Than` (content test): khoảng cách 0 → Thép, 1 → Gang, 2 → Phế phẩm.
- [ ] AC-4: Thêm phụ gia hợp lệ vào khoảng trống → cùng thành phẩm, cùng `Quality` cơ bản, `MagicChance` tăng đúng.
- [ ] AC-5: Slot `RequiresMagic` + nguyên liệu chưa ma thuật → phế phẩm.
- [ ] AC-6: `MagicChance` = 50% cho ví dụ trong Formulas; tổng > 90 → 90.
- [ ] AC-7: Với cùng một lưới, `BrewLog` khớp và hỏng trùng nhau tới trước sự kiện kết quả.
- [ ] AC-8: Trong Brewing, mọi lệnh chỉnh lưới / soi / mua bán bị từ chối.
- [ ] AC-9: Mọi vật phẩm trên lưới bị tiêu hao; đúng 1 kết quả vào Túi.
- [ ] AC-10: Lưới trống → Nung bị từ chối; Bỏ qua lượt luôn hợp lệ.
- [ ] AC-11: Chạm để tua → hiện kết quả ngay; state không đổi so với xem hết.

**Trải nghiệm (playtest)**
- [ ] AC-12: ≥ 70% người chơi mới tự giải được công thức có 1 nhịp chờ trong ≤ 4 lần thử (Mùa 2).
- [ ] AC-13: ≥ 60% người chơi dùng tua hoặc 2×/3× sau Mùa 2 (xác nhận tua là cần thiết, không phải thừa).

## Open Questions

- Nút Nung khi lưới chỉ có phụ gia: cho phép (ra phế phẩm) hay chặn như lưới trống? Đề xuất: cho phép — chặn sẽ
  lộ luật "slot đầu phải là nguyên liệu" (thực ra đã là luật công khai, cân nhắc).
- ~~Hiện số nhịp 1–9 trên ô?~~ Chốt 2026-11-03: không.
- **Cân bằng nhánh ma thuật sau khi `Magic` công khai** (hướng A, chốt 2026-11-04): ma thuật là tài nguyên có kế
  hoạch. **Yêu cầu thiết kế (người dùng)**: xác suất giữ được `Magic` khi nung lên tier cao hơn phải **giảm dần
  theo tier thành phẩm** → tier 10 vẫn khó. ⚠ Công thức hiện tại ngược lại (truyền thừa `tier × 5%` → tier càng
  cao càng dễ). Cách làm (trần theo tier / hệ số giữ theo tier / truyền thừa giảm theo tier) **chốt ở `balance`**
  (mô phỏng riêng, ngoài M0), cùng số slot `RequiresMagic` trong chuỗi tier 10 và giá Kính soi.
- AC trải nghiệm bổ sung cần đo ở M0: ≥ 80% người chơi mới mô tả đúng thứ tự đọc lưới (trái→phải, trên→dưới)
  sau Mùa 1 — nếu không đạt, xem lại quyết định này.
