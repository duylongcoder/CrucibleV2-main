# Vòng Mùa (Chiếu chỉ → Bàn luyện → Dâng → Rèn & Trận → Chiến lợi phẩm) — System Design

> **Trạng thái**: In Review (sửa sau review 2026-11-03) · **Cập nhật**: 2026-11-03
> **Pillar**: P3, P2, P4 · **Ưu tiên**: MVP · **Lớp**: Core · **Phụ thuộc**: tất cả hệ thống Core
> GDD.md §4 hệ thống #9, #10, #11. Số liệu là tạm, đọc từ config.

## Overview

Một run = **8 Mùa**. Mỗi Mùa gồm 4 màn + 1 màn thưởng:
**Chiếu chỉ** (công bố đối thủ, **loại trang bị** sẽ rèn, modifier Mùa sau) → **Bàn luyện ×3** (Chợ + Lò cùng
màn) → **Dâng** (soi + chọn 1 khoáng) → **Rèn & Trận** (một cảnh liền) → **Chiến lợi phẩm** (nếu thắng).
Thua trận mất 1 Uy tín; hết 3 Uy tín → trục xuất. Qua Mùa 8 còn Uy tín → thắng run.

## Player Fantasy

Sống còn theo từng Chiếu chỉ: "Mùa này Vua cần Khiên chống lại Bá tước ★★★ — ta còn 3 mẻ lò." P3.

## Detailed Rules

### Luật cốt lõi

**Khởi đầu run**: chọn bậc khó + bộ khởi đầu (seed tự sinh, không nhập — GDD §6.1); Túi = bộ khởi đầu; vàng 0; Uy tín 3; Mùa 1.

**Chiếu chỉ** (`Decree`) — đầu mỗi Mùa, tự động sinh rồi hiển thị:
- Đối thủ Mùa này (cố định theo `RunDef` + bậc khó, **không** random): tên, loại trận (Thi đấu / Chiến tranh /
  Đại chiến), mức khó định tính ★1–5 (suy từ `Power` theo bảng khoảng).
- **Loại trang bị** sẽ rèn (mới): rút từ bể trang bị của Mùa (stream `Forge`), có tính modifier đã công bố.
- Hệ số giá Mùa này (stream `Market`, hiển thị định tính).
- Modifier của **Mùa sau** (stream `Season`, 0–1 modifier) — biết trước 1 Mùa.
- Màn chỉ đọc rồi Tiếp tục (không soi, không mua bán) — giữ ngắn (P4).

**Bàn luyện** — 3 lượt; mỗi lượt: hàng Chợ làm mới → Editing → Nung *hoặc* Bỏ qua lượt (chi tiết brewing.md).
Lượt bỏ qua vẫn tính vào 3 lượt.

**Dâng** (`Offering`) — sau lượt 3:
- Dùng Kính soi được; **không** mua bán.
- Chọn **1** thành phẩm đạt chuẩn bất kỳ trong Túi để dâng; luôn tiêu hao.
- Không có thành phẩm đạt chuẩn → không dâng được → Rèn & Trận với kết quả **tự động Thua**.

**Rèn & Trận** (`Forge` + `Battle`, một cảnh):
- Rèn khoáng đã dâng thành trang bị **đúng loại đã công bố ở Chiếu chỉ**.
- `Power` trang bị ≥ `Power` đối thủ → **Thắng**; ngược lại **Thua**. Tất định. Thi đấu/Chiến tranh/Đại chiến
  cùng luật.
- UI chỉ hiện: trang bị (loại + tên khoáng) và Thắng/Thua. Không hiện `Power`.

**Uy tín** (`Reputation`): khởi đầu 3; Thua → −1; về 0 → **Trục xuất** (kết thúc run ngay). MVP không có cách hồi.

**Chiến lợi phẩm** (`Spoils`) — sau trận Thắng, trừ Mùa 8:
- Rút 3 lựa chọn từ bể Mùa (stream `Reward`), lọc theo điều kiện xuất hiện; nhận đúng 1.
- Loại: Xúc tác, Nới Túi (+ chỗ, tới trần), Khoáng hiếm (tier 4+, trần tier theo Mùa), Thuốc thử (sau MVP).
  Không có vàng, không có "+1 Uy tín".
- **Điều kiện xuất hiện theo Mùa** (`RewardDef`/bể chiến lợi phẩm của `RunDef`): mỗi Mùa khai báo bể riêng và
  có thể khai báo *lựa chọn bắt buộc* — vd Mùa 2 và Mùa 3: ≥ 1 trong 3 lựa chọn là Xúc tác (nếu còn chỗ Xúc
  tác), phục vụ onboarding (GDD §3). Lọc bỏ lựa chọn không nhận được trước khi rút; nếu bể còn < 3 lựa chọn
  hợp lệ thì hiện ít hơn.

**Kết thúc run**:
- Sau Rèn & Trận Mùa 8: Uy tín ≥ 1 → **Thắng run** (kể cả thua Đại chiến). Mở bậc khó kế.
- Uy tín = 0 ở bất kỳ Mùa nào → **Trục xuất**.
- Luôn quy đổi Danh vọng (GDD §6.3), xóa save run.

**Modifier Mùa** (`SeasonModifierDef`): 0–1 mỗi Mùa, biết trước 1 Mùa, MVP **chỉ đổi số**: `ScaleParam` (giá
mua, giá bán, giá Kính soi, giá reroll, `Power` đối thủ, trọng số bể trang bị — % nguyên trong giới hạn) và
`OffsetParam` (số reroll miễn phí 0–2). Không đổi số lượt nung, không đổi công thức.

### Trạng thái & chuyển trạng thái (`RunStateMachine`)

| Trạng thái | Sự kiện | Trạng thái kế | Ghi chú |
|---|---|---|---|
| RunStart | Bắt đầu | Decree (Mùa 1) | |
| Decree | Tiếp tục | Workbench (lượt 1) | Đã rút loại trang bị, hệ số giá, modifier Mùa sau |
| Workbench (lượt k) | Nung → kết quả / Bỏ qua | Workbench (k+1) nếu k < 3 | Hàng Chợ làm mới |
| Workbench (lượt 3) | Nung → kết quả / Bỏ qua | Offering | |
| Offering | Dâng khoáng X | Resolution | X tiêu hao |
| Offering | Không có gì để dâng | Resolution (tự Thua) | Tự động, không cần lệnh |
| Resolution | Thắng, Mùa < 8 | Spoils | |
| Resolution | Thắng, Mùa = 8 | RunEnd(Thắng) | |
| Resolution | Thua, Uy tín −1 = 0 | RunEnd(Trục xuất) | |
| Resolution | Thua, Uy tín > 0, Mùa < 8 | Decree (Mùa + 1) | |
| Resolution | Thua, Uy tín > 0, Mùa = 8 | RunEnd(Thắng) | |
| Spoils | Chọn 1/3 | Decree (Mùa + 1) | |

### Tương tác với hệ thống khác
- Lò nung (brewing.md) chạy trong Workbench; Chợ/Túi (economy.md) trong Workbench Editing.
- Kính soi (GDD §6.2) dùng được ở Workbench Editing và Offering.
- Rèn đọc **toàn bộ** dòng thuộc tính (kể cả chưa lộ).

## Formulas

| Tên | Công thức | Biến (khoảng) | Ví dụ |
|---|---|---|---|
| `Power` trang bị | `Quality × QualityWeight + Σ (Stat_i × Weight_i[loại])` | `QualityWeight` chung mọi loại; `Weight_i` ≥ 0; `double`, clamp ≥ 0 | Quality 108 × 1 + Sắc bén 20 × 2 = 148 |
| Thắng | `Power trang bị ≥ Power đối thủ × ModifierPower%` | | 148 ≥ 140 → Thắng |
| ★ khó | bảng khoảng `Power` đối thủ → ★1–5 | | TBD ở `balance` |

Trọng số loại trang bị (MVP): Kiếm — Sắc bén; Giáo — Sắc bén, Phản đòn; Giáp — Hấp thụ; Khiên — Phản đòn, Hấp
thụ. `Magic` không cộng `Power` (MVP).

## Edge Cases

- **Không có gì để dâng** → tự động Thua, vẫn qua Rèn & Trận (hiện "Không có gì để dâng").
- **Thua ở Mùa 8 với Uy tín 1** → Uy tín 0 → Trục xuất (Thua làm Uy tín về 0 trước khi xét thắng run).
  *Đối chiếu demo*: "hoàn thành Mùa 8 còn Uy tín = thắng run (kể cả khi thua Đại chiến)" → chỉ đúng khi Uy tín
  ≥ 2 trước trận. Ghi rõ ở đây để nhất quán.
- **`Power` bằng nhau** → Thắng.
- **Chiến lợi phẩm cả 3 không hợp lệ** (Túi đầy, đủ 5 Xúc tác, Túi ở trần) → cho phép "Bỏ qua" (nhận không gì).
- **Đóng tab ở bất kỳ trạng thái nào** → mở lại đúng trạng thái (GDD §6.4); Decree đã rút thì không rút lại.
- **Chiến lược áp đảo**: biết loại trang bị sớm → người chơi "chuyên" 1 thuộc tính? Bể trang bị mỗi Mùa ≥ 2
  loại có trọng số khác nhau + modifier trọng số bể → cần đa dạng. Kiểm bằng GreedyAgent.
- **Giữ khoáng tốt cho Mùa sau** (dâng khoáng yếu khi đối thủ ★1): hợp lệ, là quyết định chiến lược (P3).

## Dependencies

| Hệ thống | Hướng | Hợp đồng |
|---|---|---|
| Seed & RNG | cần | `Forge` (ở Decree), `Market`, `Season`, `Reward` |
| Lò nung, Túi, Chợ | điều phối | mở/đóng Workbench, đếm lượt |
| Thuộc tính | cần | đọc mọi dòng để tính `Power` |
| Meta | cung cấp | kết quả run → Danh vọng, mở bậc khó |
| Lưu | cung cấp | điểm lưu sau mỗi chuyển trạng thái |

## Tuning Knobs

| Knob | Loại | Mặc định | Khoảng an toàn | Lý do | Nơi lưu |
|---|---|---|---|---|---|
| Số Mùa / run | gate | 8 | 6–10 | Run ~35–40 phút | `RunDef` |
| Lượt nung / Mùa | gate | 3 | 2–4 | | `RunDef` |
| Uy tín khởi đầu | gate | 3 | 2–5 | | `RunDef` |
| `Power` đối thủ từng Mùa | curve | TBD (răng cưa: tăng dần, Mùa 4 & 8 vọt) | — | Flow channel | `OpponentDef` × `DifficultyDef` |
| Bể trang bị từng Mùa + trọng số | curve | TBD | ≥ 2 loại/Mùa | Chống chuyên một thuộc tính | `RunDef` |
| `QualityWeight`, `Weight_i` | curve | TBD | — | | `EquipmentDef` |
| Trần tier Khoáng hiếm theo Mùa | gate | TBD | — | | `RewardDef` |

## Visual / Audio / Game Feel

- Chiếu chỉ: cuộn giấy mở ra, dấu ấn hoàng gia; loại trang bị là hình lớn nhất trên chiếu.
- Rèn & Trận: búa rèn 2–3 nhịp → binh lính cầm trang bị → giao tranh ngắn (≤ 5 s, tua được) → Thắng/Thua lớn.
- Mất Uy tín: một biểu tượng Uy tín vỡ.

## UI Requirements

- Chiếu chỉ: đối thủ (tên, loại trận, ★), **loại trang bị** + thuộc tính nó ưa (icon), hệ số giá (↑/↓),
  modifier Mùa sau.
- Dâng: Túi lọc chỉ thành phẩm đạt chuẩn; dòng đã soi; nhắc loại trang bị Mùa này; nút Kính soi.
- Không hiện `Power` ở bất kỳ đâu.

## Acceptance Criteria

**Chức năng**
- [ ] AC-1: RandomAgent 1.000 run: mọi run tới RunEnd, không vi phạm bất biến.
- [ ] AC-2: Loại trang bị hiện ở Chiếu chỉ = loại được rèn ở Rèn & Trận cùng Mùa (1.000 run).
- [ ] AC-3: Mỗi Mùa đúng 1 lần Dâng (0–1 khoáng tiêu hao) và đúng 1 kết quả Thắng/Thua.
- [ ] AC-4: `Power` bằng nhau → Thắng.
- [ ] AC-5: Uy tín 0 → RunEnd(Trục xuất) ngay; Uy tín không bao giờ tăng (MVP).
- [ ] AC-6b: Thắng ở Mùa 2 và Mùa 3 (còn chỗ Xúc tác) → luôn có ≥ 1 lựa chọn Xúc tác.
- [ ] AC-6: Chiến lợi phẩm chỉ sau Thắng và không ở Mùa 8; tối đa 3 lựa chọn; nhận đúng 1 (hoặc Bỏ qua khi không lựa chọn nào hợp lệ).
- [ ] AC-7: Không có gì để dâng → tự động Thua.

**Trải nghiệm (playtest)**
- [ ] AC-8: ≥ 80% người chơi nói được loại trang bị của Mùa hiện tại khi được hỏi ở màn Dâng.
- [ ] AC-9: Một run trung bình mục tiêu 35–40 phút, chấp nhận 30–45 phút (đo 5+ người chơi).
- [ ] AC-10: Người chơi thua run nêu được ít nhất một quyết định của mình dẫn tới thua (không chỉ "xui").

## Open Questions

- Mức khó hiển thị ★1–5 theo bảng khoảng — chốt ở `balance`.
- Chiến lợi phẩm có nút Bỏ qua chủ động (khi vẫn có lựa chọn hợp lệ) không? Đề xuất: không (MVP).
- Người chơi có được xem trước *bể* trang bị Mùa sau (chứ không phải loại) để lên kế hoạch không?
