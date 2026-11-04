# Trận (combat & hoạt ảnh trình diễn) — System Design

> **Trạng thái**: In Review · **Cập nhật**: 2026-11-04
> **Pillar**: P2, P4 · **Ưu tiên**: MVP · **Lớp**: Presentation · **Phụ thuộc**: #10 Rèn & Uy tín, #9 Vòng Mùa, #8 Thuộc tính
> Systems Index #17. Ràng buộc đã có: `design/run-flow.md` §Rèn & Trận, `design/ui-season.md` §D.

## Overview
Mỗi Mùa có một trận **1-đấu-1**: dũng sĩ của ta (mặc trang bị vừa rèn từ khoáng đã dâng) đấu dũng sĩ của vương quốc
đối thủ. Trận là **mô phỏng tất định theo lượt, không RNG** — **thay thế** luật so `Power` cũ (run-flow, sửa
2026-11-04). Chỉ số dũng sĩ ta = chỉ số gốc + trang bị: `Quality` quy đổi thành sức đánh / máu theo loại trang bị;
Sắc bén, Phản đòn, Hấp thụ mỗi thuộc tính một cơ chế, nhân theo độ hợp của loại trang bị. Kết quả tính ngay khi
dâng và lưu (`BattleLog`), rồi phát lại **luôn đủ 6 nhịp × 1 s** ở 1× (5 nhịp giao đấu + 1 nhịp kết quả), theo
timescale, không tua. Luật trận đọc được (thanh máu + số sát thương — P2); thuộc tính ẩn của trang bị **lộ khi
kích hoạt** và được ghi vào Sổ công thức (P1).

## Player Fantasy
"Viên khoáng mình luyện quyết định trận đấu." Thanh Kiếm Sắc bén chém thêm máu, tấm Khiên Phản đòn dội ngược đòn
đánh — và khi thua, người chơi thấy rõ vì sao ("đối thủ Hấp thụ cao, Kiếm của ta thiếu Sắc bén"), mang bài học
sang Mùa sau / run sau. Pillar: P1 (hiểu biết), P2 (luật đọc được), P3 (chọn khoáng để dâng có hệ quả rõ).

## Detailed Rules

### Luật cốt lõi

Mọi phép tính **số nguyên, làm tròn xuống** (tất định trên WebGL). Không dùng RNG.

**Chỉ số**: `HP`, `ATK`, `SB` (Sắc bén), `PD` (Phản đòn), `HT` (Hấp thụ).

**Dũng sĩ ta** = gốc `HeroBaseHP`, `HeroBaseATK` (`RunDef`, cố định cả run) + trang bị (`EquipmentDef` theo loại):

| Loại | `AtkPerQ` | `HpPerQ` | Hợp 100% với | Thuộc tính khác |
|---|---|---|---|---|
| Kiếm | 50% | 20% | Sắc bén | 50% |
| Giáo | 40% | 30% | Sắc bén, Phản đòn | 50% |
| Giáp | 15% | 80% | Hấp thụ | 50% |
| Khiên | 20% | 60% | Phản đòn, Hấp thụ | 50% |

- `ATK = HeroBaseATK + Quality × AtkPerQ / 100`; `HP = HeroBaseHP + Quality × HpPerQ / 100`.
- `SB / PD / HT` = giá trị thuộc tính trên khoáng × độ hợp (100 hoặc `OffAffinity` 50) / 100. Thuộc tính khoáng không
  có → 0. Bảng "hợp" lấy từ trọng số trang bị cũ của run-flow.
- `Magic`: không tác dụng trong trận (MVP) — chỉ hiệu ứng hình ảnh.

**Đối thủ**: `HP, ATK, SB, PD, HT` từ `OpponentDef` × hệ số `DifficultyDef`. ★ do designer gán trong `OpponentDef`.

**Một hiệp** (ta luôn đánh trước):
1. Ta đánh: `gốc = ATK × (100 + SB) / 100`; đối thủ nhận `max(1, gốc × (100 − min(HTCap, HT_đt)) / 100)`; ta bị
   dội ngược `nhận × min(PDCap, PD_đt) / 100`.
2. **Cả hai còn đứng** → đối thủ đánh, cùng công thức đổi vai. Ta gục vì dội ngược ở đòn của mình mà đối thủ còn đứng →
   hết trận, ta thua (đối thủ không đánh nữa — chốt 2026-11-04, F-010 D5).
3. HP ≤ 0 → gục. Cả hai cùng gục trong một hiệp (do dội ngược) → **ta thắng** (giữ luật "bằng nhau → Thắng").

**Kết thúc**: có người gục → hết trận. Sau `MaxRounds` (10) hiệp cả hai còn đứng → bên có **tổng sát thương gây
ra** lớn hơn thắng (đòn trúng + dội ngược gây cho đối thủ); bằng nhau → ta thắng. (Chặn lối phòng thủ thuần —
chốt 2026-11-04.)

**Không dâng** (không có khoáng đạt chuẩn) → không vào Trận, Thua (run-flow).

**Lộ thuộc tính**: lần đầu một thuộc tính trên trang bị ta có tác dụng trong trận (SB tăng sát thương, HT giảm sát
thương nhận, PD dội ngược) → hiện tên cạnh dũng sĩ; đánh dấu "đã khám phá" cho **loại khoáng đã dâng** (Sổ công
thức, GDD §6.3). Thuộc tính bằng 0 hoặc không kích hoạt → không lộ.

**`BattleLog`**: tính ngay khi Dâng, lưu cùng save (như `BrewLog`): danh sách hiệp (đòn, sát thương, dội ngược,
HP sau), thuộc tính lộ, kết quả. Đóng tab giữa cutscene / Trận → mở lại vào Kết quả.

**Phát lại — luôn 6 nhịp × 1 s ở 1×**: chia các hiệp thành 5 đoạn liên tiếp đều nhất có thể (đoạn đầu nhận phần
dư); nhịp 1–5 phát mỗi đoạn, các đòn trong đoạn chia đều 1 s; nhịp 6 = kết quả (gục / phân thắng theo tổng sát thương).
Timescale 1× / 2× / 3×, không tua (P4: ≤ 2 s ở 3×). Lúc bắt đầu nhịp 1: lộ ★ + loại trận.

### Trạng thái & chuyển trạng thái

| Trạng thái | Sự kiện | Trạng thái kế | Ghi chú |
|---|---|---|---|
| (Offering) | Dâng khoáng X | Simulated | Tính `BattleLog` + lưu, trước mọi hoạt ảnh |
| Simulated | Cutscene Rèn xong | Playing (nhịp 1) | Lộ ★ + loại trận |
| Playing (nhịp k < 6) | Hết 1 s / timescale | Playing (k+1) | |
| Playing (nhịp 6) | Hết nhịp | Resolution (run-flow) | Thắng / Thua |

### Tương tác với hệ thống khác
- Rèn: cung cấp loại trang bị (Chiếu chỉ) + khoáng đã dâng (`Quality`, thuộc tính).
- Vòng Mùa: nhận kết quả → Resolution (Uy tín, Chiến lợi phẩm).
- Meta: ghi thuộc tính lộ trong trận vào "thuộc tính đã khám phá" của khoáng.

## Formulas

| Tên | Công thức | Biến (khoảng) | Ví dụ |
|---|---|---|---|
| ATK ta | `HeroBaseATK + ⌊Q × AtkPerQ / 100⌋` | Q = `Quality`; AtkPerQ 0–100 | 10 + ⌊40 × 50/100⌋ = 30 (Kiếm) |
| HP ta | `HeroBaseHP + ⌊Q × HpPerQ / 100⌋` | HpPerQ 0–100 | 100 + ⌊40 × 20/100⌋ = 108 |
| Chỉ số phụ ta | `⌊v × Aff / 100⌋` | v giá trị trên khoáng; Aff 100 / 50 | SB 20 × 100% = 20 |
| Đòn gốc | `⌊ATK × (100 + SB) / 100⌋` | SB ≥ 0 | ⌊30 × 120/100⌋ = 36 |
| Sát thương nhận | `max(1, ⌊gốc × (100 − min(HTCap, HT)) / 100⌋)` | HTCap 75 | ⌊36 × 90/100⌋ = 32 |
| Dội ngược | `⌊nhận × min(PDCap, PD) / 100⌋` | PDCap 50 | PD 0 → 0 |
| Thắng sau `MaxRounds` | `Σ sát thương ta gây ≥ Σ sát thương đối thủ gây` | gồm dội ngược | — |

**Ví dụ trọn trận**: ta HP 108 / ATK 30 / SB 20 (Kiếm, Q 40) vs đối thủ HP 150 / ATK 20 / HT 10. Ta trúng 32 mỗi
đòn, nhận 20 mỗi đòn. Hết hiệp 4: đối thủ 22, ta 28. Hiệp 5 ta đánh trước → đối thủ gục → **Thắng**. 5 hiệp → 5
nhịp mỗi nhịp 1 hiệp; nhịp 6 kết quả.

## Edge Cases
- **Chỉ số 0**: `HeroBaseATK` ≥ 1, mỗi đòn ≥ 1 sát thương → không kẹt vô hạn; `MaxRounds` chặn cuối.
- **Hấp thụ / Phản đòn quá trần** → clamp `HTCap` 75 / `PDCap` 50.
- **Dội ngược** không bị Hấp thụ giảm và **không dội tiếp** (không vòng lặp). Dội ngược hạ được người đánh; cả hai
  gục cùng hiệp → ta thắng.
- **Tràn số**: phép tính trung gian dùng `long`.
- **< 5 hiệp** (vd gục hiệp 1): nhịp thừa diễn dò xét / thủ thế; đòn quyết định luôn rơi vào nhịp 5, kết quả nhịp 6.
  **6–10 hiệp**: mỗi nhịp 2 hiệp (đoạn đầu nhận phần dư).
- **Đổi timescale giữa trận** → áp từ nhịp kế. **Đóng tab** → mở lại vào Kết quả, không chiếu lại.
- **Chiến lược áp đảo** (kiểm bằng GreedyAgent ở `balance`): dồn HP + Hấp thụ để cầm cự → đã chặn bằng tie-break
  tổng sát thương; Phản đòn tối đa vs đối thủ ATK cao → chặn một phần bằng `PDCap` 50; dồn Sắc bén không trần →
  theo dõi (có thể thêm `SBCap`).
- **Chiếu chỉ ép loại trang bị**, người chơi chỉ chọn khoáng: thuộc tính đúng "ưa" mạnh nhất, thuộc tính khác vẫn
  giữ `OffAffinity` 50%.

## Dependencies

| Hệ thống | Hướng | Hợp đồng |
|---|---|---|
| Thuộc tính & Kính soi (#8) | cần | `Quality` + **mọi dòng** của khoáng đã dâng (kể cả chưa soi) |
| Rèn / Vòng Mùa (#9, #10) | hai chiều | nhận loại trang bị + khoáng; trả Thắng / Thua → Resolution |
| Meta / Sổ (#12) | cung cấp | sự kiện "thuộc tính S lộ trên khoáng X" |
| Lưu (#13) | cung cấp | `BattleLog` trong save run |
| Seed & RNG (#1) | không | Trận không dùng RNG |

## Tuning Knobs

| Knob | Loại | Mặc định | Khoảng an toàn | Lý do | Nơi lưu |
|---|---|---|---|---|---|
| `HeroBaseHP` / `HeroBaseATK` | curve | 100 / 10 | — | sàn cho mọi trang bị | `RunDef` |
| `AtkPerQ`, `HpPerQ` theo loại | curve | bảng Detailed Rules | 0–100 | cá tính từng loại | `EquipmentDef` |
| `OffAffinity` | curve | 50% | 0–100 | thuộc tính lệch loại vẫn có ích | config Trận |
| `HTCap` / `PDCap` | gate | 75% / 50% | 50–90 / 20–75 | chống bất tử / tự hạ | config Trận |
| `MaxRounds` | gate | 10 | 6–15 | ≤ 2 hiệp / nhịp | config Trận |
| Chỉ số đối thủ theo Mùa × bậc | curve | TBD (răng cưa, Mùa 4 & 8 vọt) | — | flow channel | `OpponentDef` × `DifficultyDef` |
| `OpponentStatScale` (modifier) | curve | 100% | 80–130 | modifier Mùa chỉ đổi số | `SeasonModifierDef` |
| Độ dài nhịp | feel | 1 s ở 1× | — | P4 | config Presentation |

Hệ quả: Kính soi + thuộc tính "ưa" thành quyết định trung tâm khi chọn khoáng dâng → `balance` cần GreedyAgent riêng
cho Trận.

## Visual / Audio / Game Feel
- Đấu trường nhìn ngang: ta bên trái, đối thủ bên phải (huy hiệu vương quốc); chung rig dũng sĩ. Phông theo loại
  trận: Thi đấu = đấu trường · Chiến tranh = chiến trường · Đại chiến = cảnh lớn (MVP: chỉ đổi phông).
- Mỗi đòn: lao tới → chém → lùi. Sắc bén = vệt sáng + số to hơn · Hấp thụ = khiên mờ + số nhỏ · Phản đòn = tia
  dội ngược · Gục = ngã. Luật pixel ART §7 (dịch bước 3 px, đổi frame, không scale).
- Âm thanh: chém, đỡ, dội ngược, gục, nhạc thắng / thua (nội dung chờ `gdd audio`).

## UI Requirements
- Nhịp 1: lộ ★ + loại trận; tên hai dũng sĩ + vương quốc; **thanh máu** hai bên (trên đầu dũng sĩ, **không ghi
  số HP** — chốt 2026-11-04); icon trang bị của ta.
- **Số sát thương bay lên**, phân biệt bằng icon + cỡ (không chỉ màu): thường = trắng · Sắc bén = vàng, to, icon ◆ ·
  bị Hấp thụ = xám, nhỏ, icon khiên · dội ngược = đỏ, icon mũi tên quay ngược.
- Thuộc tính lộ lần đầu: nhãn "◆ Sắc bén!" cạnh dũng sĩ ta, 0.8 s.
- Hết `MaxRounds`: nhịp 6 hiện "Tổng sát thương: 214 – 198".
- Timescale góc dưới-phải; không nút nào khác. Sau nhịp 6 → popup Kết quả (`ui-season.md` §E).

## Acceptance Criteria

**Chức năng**
- [ ] AC-1: Cùng đầu vào → cùng `BattleLog` (1.000 bộ ngẫu nhiên); Trận không đọc RNG stream nào.
- [ ] AC-2: Ví dụ trọn trận ở Formulas → Thắng ở hiệp 5.
- [ ] AC-3: Mọi trận đủ 6 nhịp: 6 s ở 1×, 2 s ở 3×.
- [ ] AC-4: Hết `MaxRounds` → bên tổng sát thương lớn hơn thắng; bằng nhau → ta thắng.
- [ ] AC-5: Cả hai cùng gục → ta thắng.
- [ ] AC-6: Thuộc tính chỉ lộ khi kích hoạt, ghi vào Sổ của đúng loại khoáng; thuộc tính bằng 0 không lộ.
- [ ] AC-7: HT / PD bị chặn ở trần; mỗi đòn ≥ 1 sát thương.
- [ ] AC-8: Đóng tab giữa cutscene / Trận → mở lại vào Kết quả, cùng state hash.

**Trải nghiệm (playtest)**
- [ ] AC-9: ≥ 70% người chơi thua nêu được nguyên nhân bằng chỉ số / thuộc tính.
- [ ] AC-10: ≥ 60% người chơi dùng Kính soi trước khi Dâng từ Mùa 3.

## Open Questions
- `Magic` có tác dụng trong Trận sau MVP không?
- Cần `SBCap` không? (theo dõi ở `balance`)
- Đường cong chỉ số đối thủ theo Mùa × bậc khó; ★ gán tay có khớp sức mạnh thật không (`balance`).
- Đại chiến (Mùa 8) có cần cảnh riêng không?
