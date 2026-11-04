# Vòng Mùa (Chiếu chỉ → Bàn luyện → Dâng → Rèn & Trận → Chiến lợi phẩm) — System Design

> **Trạng thái**: In Review (sửa sau review 2026-11-03) · **Cập nhật**: 2026-11-03
> **Pillar**: P3, P2, P4 · **Ưu tiên**: MVP · **Lớp**: Core · **Phụ thuộc**: tất cả hệ thống Core
> GDD.md §4 hệ thống #9, #10, #11. Số liệu là tạm, đọc từ config.

## Overview

Một run = **8 Mùa**. Mỗi Mùa gồm 4 màn + 1 màn thưởng:
**Chiếu chỉ** (popup: **loại trang bị** + thuộc tính ưa, modifier Mùa sau) → **Bàn luyện ×3** (Chợ + Lò cùng
màn) → **Dâng** (soi + chọn 1 khoáng, cận thần tới nhận) → **Rèn & Trận** (cutscene; đối thủ lộ ở đây) →
**Chiến lợi phẩm** (nếu thắng: vàng + chọn 1/3).
Thua trận mất 1 Uy tín; hết 3 Uy tín → trục xuất. Qua Mùa 8 còn Uy tín → thắng run.

## Player Fantasy

Sống còn theo từng Chiếu chỉ: "Nhà Vua cần rèn một tấm Khiên Hấp thụ — ta còn 3 mẻ lò." Đối thủ chỉ hiện ra khi
dũng sĩ đã cầm trang bị lên đường. P3.

## Detailed Rules

### Luật cốt lõi

**Khởi đầu run**: chọn bậc khó + bộ khởi đầu (seed tự sinh, không nhập — GDD §6.1); Túi = bộ khởi đầu; vàng 0; Uy tín 3; Mùa 1.

**Chiếu chỉ** (`Decree`) — đầu mỗi Mùa, tự động sinh rồi hiển thị:
- Đối thủ Mùa này (cố định theo `RunDef` + bậc khó, **không** random): tên, loại trận (Thi đấu / Chiến tranh /
  Đại chiến), mức khó định tính ★1–5 (designer gán trong `OpponentDef`). Chiếu chỉ **chỉ hiện tên vương quốc**
  (sửa 2026-11-04); ★ và loại trận lộ khi bắt đầu Trận. Vì đối thủ cố định, người chơi đã chơi nhiều run vẫn
  biết trước (hiểu biết là tiến bộ — P1).
- **Loại trang bị** sẽ rèn: rút từ bể trang bị của Mùa (stream `Forge`), có tính modifier đã công bố.
- Hệ số giá Mùa này (stream `Market`) — hiển thị ở header Chợ, không ở popup.
- Modifier của **Mùa sau** (stream `Season`, 0–1 modifier) — biết trước 1 Mùa.
- Hiển thị (spec: `ui-season.md` §A): "Mùa x" · tên vương quốc đối thủ · **câu** "Nhà Vua cần rèn một thanh <loại trang bị> <thuộc tính ưa>" — thuộc tính ưa (trọng
  số cao nhất của loại) tô màu riêng **+ icon**; dòng phụ modifier Mùa sau (nếu có); nút **Nhận chiếu chỉ**.
  Không soi, không mua bán (P4).

**Bàn luyện** — 3 lượt; mỗi lượt: hàng Chợ làm mới → Editing → Nung *hoặc* Bỏ qua lượt (chi tiết brewing.md).
Lượt bỏ qua vẫn tính vào 3 lượt.

**Dâng** (`Offering`) — sau lượt 3:
- Dùng Kính soi được; **không** mua bán.
- Chọn **1** thành phẩm đạt chuẩn bất kỳ trong Túi để dâng; luôn tiêu hao. "Đạt chuẩn" = khoáng làm ra từ công thức
  (không phải nguyên liệu gốc / phế phẩm). Khay cận thần chỉ là trạng thái hiển thị — đóng tab giữa Dâng thì khay trống
  lại (chốt 2026-11-04, F-010).
- "Để cận thần ra về" chỉ có khi Túi không còn thành phẩm đạt chuẩn.
- Không có thành phẩm đạt chuẩn → không dâng được → Rèn & Trận với kết quả **tự động Thua**.

**Rèn & Trận** (`Forge` + `Battle`, **cutscene** — sửa 2026-11-04):
- Rèn khoáng đã dâng thành trang bị **đúng loại đã công bố ở Chiếu chỉ**.
- **Thắng / Thua do mô phỏng Trận** (`design/battle.md`, thay luật so `Power` — sửa 2026-11-04). Tất định (tính
  ngay khi Dâng, lưu trước khi chiếu). Thi đấu/Chiến tranh/Đại chiến cùng luật.
- **Cutscene Rèn**: tối đa **9 nhịp × 1 s ở 1×** (≤ 9 s); **không tua**; chạy theo timescale chung 1× / 2× / 3×
  (tối đa 3× → ≤ 3 s). 8 nhịp (`ui-season.md` §C): cận thần mang khay tới lò rèn → thợ rèn cầm khoáng kiểm tra
  (**biểu cảm** 3 mức theo `Quality` so với chuẩn tier — không theo đối thủ / kết quả trận) → đúc (2 nhịp, **cảnh riêng
  mỗi loại trang bị**) → trang bị hoàn thành → cận thần mang tới dũng sĩ → dũng sĩ mặc → bước ra đấu trường.
- **Trận**: mô phỏng tất định 1-đấu-1 theo lượt, phát lại 6 nhịp — `design/battle.md`.
- **Timeline chung, vật phẩm gắn động**: một cấu hình animation dùng cho mọi Mùa; lúc chạy gắn vào các "slot":
  khoáng đã dâng (sprite + hiệu ứng Ma thuật nếu có), trang bị theo loại (Kiếm/Giáo/Giáp/Khiên) × khoáng (màu
  lấy từ khoáng), đối thủ (`OpponentDef`), phông theo loại trận.
- (Đã bỏ ràng buộc "không rò rỉ `Power`" — luật Trận công khai, diễn ra sau khi dâng; xem battle.md.)
- Không có gì để dâng → nút "Để cận thần ra về" → Kết quả Thua (bỏ qua cutscene và Trận).
- UI Trận: thanh máu + số sát thương + thuộc tính lộ (battle.md).

**Uy tín** (`Reputation`): khởi đầu 3; Thua → −1; về 0 → **Trục xuất** (kết thúc run ngay). MVP không có cách hồi.

**Chiến lợi phẩm** (`Spoils`) — sau trận Thắng, trừ Mùa 8 — hai phần (sửa 2026-11-04):
- **Tặng thẳng**: vàng `SpoilsGold` theo Mùa (TBD ở `balance`), cộng ngay.
- **Chọn 1/3**: rút 3 lựa chọn từ bể Mùa (stream `Reward`), lọc theo điều kiện xuất hiện; nhận đúng 1.
- Loại lựa chọn: Khoáng cấp cao (tier 4+, trần tier theo Mùa), Xúc tác (gồm loại hiếm), Nới Túi (+ chỗ, tới
  trần), Thuốc thử (sau MVP). Không có "+1 Uy tín". Nội dung cụ thể chốt sau.
- **Điều kiện xuất hiện theo Mùa** (`RewardDef`/bể chiến lợi phẩm của `RunDef`): mỗi Mùa khai báo bể riêng và
  có thể khai báo *lựa chọn bắt buộc* — vd Mùa 2 và Mùa 3: ≥ 1 trong 3 lựa chọn là Xúc tác (nếu còn chỗ Xúc
  tác), phục vụ onboarding (GDD §3). Lọc bỏ lựa chọn không nhận được trước khi rút; nếu bể còn < 3 lựa chọn
  hợp lệ thì hiện ít hơn.

**Kết thúc run**:
- Sau Rèn & Trận Mùa 8: Uy tín ≥ 1 → **Thắng run** (kể cả thua Đại chiến). Mở bậc khó kế.
- Uy tín = 0 ở bất kỳ Mùa nào → **Trục xuất**.
- Luôn quy đổi Danh vọng (GDD §6.3), xóa save run.

**Modifier Mùa** (`SeasonModifierDef`): 0–1 mỗi Mùa, biết trước 1 Mùa, MVP **chỉ đổi số**: `ScaleParam` (giá
mua, giá bán, giá Kính soi, giá reroll, chỉ số đối thủ (`OpponentStatScale`), trọng số bể trang bị — % nguyên trong giới hạn) và
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
| Spoils | Chọn 1 lựa chọn (`ChooseReward`) hoặc Bỏ qua (`SkipReward`) | Decree (Mùa + 1) | Vàng cộng khi vào Spoils |

### Tương tác với hệ thống khác
- Lò nung (brewing.md) chạy trong Workbench; Chợ/Túi (economy.md) trong Workbench Editing.
- Kính soi (GDD §6.2) dùng được ở Workbench Editing và Offering.
- Rèn đọc **toàn bộ** dòng thuộc tính (kể cả chưa lộ).

## Formulas

| Tên | Công thức | Biến (khoảng) | Ví dụ |
|---|---|---|---|
| Thắng / Thua | mô phỏng Trận — xem `design/battle.md` §Formulas | | (luật so `Power` đã bỏ 2026-11-04) |
| ★ khó | designer gán trong `OpponentDef` | ★1–5 | TBD ở `balance` |

Độ hợp loại trang bị (MVP, dùng trong Trận): Kiếm — Sắc bén; Giáo — Sắc bén, Phản đòn; Giáp — Hấp thụ; Khiên —
Phản đòn, Hấp thụ. `Magic` không tác dụng trong Trận (MVP).

## Edge Cases

- **Không có gì để dâng** → tự động Thua, vẫn qua Rèn & Trận (hiện "Không có gì để dâng").
- **Thua ở Mùa 8 với Uy tín 1** → Uy tín 0 → Trục xuất (Thua làm Uy tín về 0 trước khi xét thắng run).
  *Đối chiếu demo*: "hoàn thành Mùa 8 còn Uy tín = thắng run (kể cả khi thua Đại chiến)" → chỉ đúng khi Uy tín
  ≥ 2 trước trận. Ghi rõ ở đây để nhất quán.
- **Hòa trong Trận** (cùng gục / bằng % máu) → Thắng (battle.md).
- **Chiến lợi phẩm cả 3 không hợp lệ** (Túi đầy, đủ 5 Xúc tác, Túi ở trần) → không bày lựa chọn nào; "Bỏ qua" (nhận không gì). Bỏ qua luôn dùng được kể cả khi còn lựa chọn (chốt 2026-11-04).
- **Đóng tab ở bất kỳ trạng thái nào** → mở lại đúng trạng thái (GDD §6.4); Decree đã rút thì không rút lại.
- **Chiến lược áp đảo**: biết loại trang bị sớm → người chơi "chuyên" 1 thuộc tính? Bể trang bị mỗi Mùa ≥ 2
  loại có trọng số khác nhau + modifier trọng số bể → cần đa dạng. Kiểm bằng GreedyAgent.
- **Giữ khoáng tốt cho Mùa sau** (dâng khoáng yếu khi đối thủ yếu): vẫn hợp lệ nhưng **chỉ người chơi nhớ thứ tự
  đối thủ cố định** mới làm có chủ đích (đối thủ ẩn ở Chiếu chỉ) — người mới mất bớt quyết định này (P3, theo dõi
  ở playtest AC-10).

## Dependencies

| Hệ thống | Hướng | Hợp đồng |
|---|---|---|
| Seed & RNG | cần | `Forge` (ở Decree), `Market`, `Season`, `Reward` |
| Lò nung, Túi, Chợ | điều phối | mở/đóng Workbench, đếm lượt |
| Thuộc tính | cần | Trận đọc mọi dòng của khoáng đã dâng (battle.md) |
| Meta | cung cấp | kết quả run → Danh vọng, mở bậc khó |
| Lưu | cung cấp | điểm lưu sau mỗi chuyển trạng thái |

## Tuning Knobs

| Knob | Loại | Mặc định | Khoảng an toàn | Lý do | Nơi lưu |
|---|---|---|---|---|---|
| Số Mùa / run | gate | 8 | 6–10 | Run ~35–40 phút | `RunDef` |
| Lượt nung / Mùa | gate | 3 | 2–4 | | `RunDef` |
| Uy tín khởi đầu | gate | 3 | 2–5 | | `RunDef` |
| Chỉ số đối thủ từng Mùa (`OpponentDef`) | curve | TBD (răng cưa: tăng dần, Mùa 4 & 8 vọt) | — | Flow channel | `OpponentDef` × `DifficultyDef` |
| Bể trang bị từng Mùa + trọng số | curve | TBD | ≥ 2 loại/Mùa | Chống chuyên một thuộc tính | `RunDef` |
| `QualityWeight`, `Weight_i` | curve | TBD | — | | `EquipmentDef` |
| Trần tier Khoáng hiếm theo Mùa | gate | TBD | — | | `RewardDef` |
| `SpoilsGold` theo Mùa | curve | TBD | — | Nguồn vàng thứ hai ngoài bán (P3 — đừng làm mất khan hiếm) | `RunDef` |
| Nhịp / thời lượng cutscene | feel | ≤ 9 × 1 s ở 1× | — | P4: ≤ 3 s ở 3× | config Presentation |

## Visual / Audio / Game Feel

- Chiếu chỉ: cuộn giấy mở ra, dấu ấn hoàng gia; loại trang bị là hình lớn nhất trên chiếu.
- Rèn & Trận: cutscene ≤ 9 nhịp (xem Luật cốt lõi) — cận thần, thợ rèn, dũng sĩ, đối thủ là nhân vật mới (ART §5).
- Mất Uy tín: một biểu tượng Uy tín vỡ.

## UI Requirements

- Chiếu chỉ: popup một câu — **loại trang bị** + thuộc tính ưa (màu riêng + icon); modifier Mùa sau (nếu có);
  nút Nhận chiếu chỉ. **Không** hiện đối thủ.
- Dâng: giữ Túi + thẻ Bàn soi của Bàn luyện (Túi lọc chỉ thành phẩm đạt chuẩn; dòng đã soi; Kính soi dùng
  được); vùng trên thay lò bằng **cận thần** tới nhận khoáng; nhắc loại trang bị Mùa này; nút Dâng.
- Rèn & Trận: cutscene, chỉ có điều khiển timescale.
- Chiến lợi phẩm: một màn đơn giản — vàng đã nhận + 3 thẻ lựa chọn.
- Trận: xem battle.md §UI Requirements.

## Acceptance Criteria

**Chức năng**
- [x] AC-1 (logic, F-009 phần 2 `FullRunInvariantTest`): RandomAgent 1.000 run: mọi run tới RunEnd, không vi phạm bất biến.
- [x] AC-2 (logic, `Agent_FiveHundredRuns_SeasonInvariantsHold`): Loại trang bị hiện ở Chiếu chỉ = loại được rèn ở Rèn & Trận cùng Mùa (1.000 run).
- [x] AC-3 (logic, `Agent_FiveHundredRuns_SeasonInvariantsHold`): Mỗi Mùa đúng 1 lần Dâng (0–1 khoáng tiêu hao) và đúng 1 kết quả Thắng/Thua.
- [ ] AC-4: Hòa trong Trận → Thắng (battle.md).
- [x] AC-5 (logic, `Agent_FiveHundredRuns_StepInvariantsHold` I5): Uy tín 0 → RunEnd(Trục xuất) ngay; Uy tín không bao giờ tăng (MVP).
- [x] AC-6b (logic, F-011 `Enter_GuaranteeCatalystWithRoom_AlwaysOffersCatalyst`): Thắng ở Mùa 2 và Mùa 3 (còn chỗ Xúc tác) → luôn có ≥ 1 lựa chọn Xúc tác.
- [x] AC-6 (logic, F-011): Chiến lợi phẩm chỉ sau Thắng và không ở Mùa 8; vàng `SpoilsGold` cộng đúng 1 lần; tối đa 3 lựa chọn; nhận đúng 1 hoặc Bỏ qua (Bỏ qua luôn dùng được — chốt 2026-11-04).
- [ ] AC-6c: Cutscene Rèn & Trận cùng thời lượng cho mọi trận ở cùng timescale (Thắng sát nút = Thắng áp đảo); ≤ 9 s ở 1×, ≤ 3 s ở 3×.
- [ ] AC-6d: Không chuỗi / hình ảnh nào của đối thủ Mùa hiện tại xuất hiện trước nhịp lộ đối thủ.
- [x] AC-7 (logic, F-010 + `SeasonInvariantsHold`): Không có gì để dâng → tự động Thua.

**Trải nghiệm (playtest)**
- [ ] AC-8: ≥ 80% người chơi nói được loại trang bị của Mùa hiện tại khi được hỏi ở màn Dâng.
- [ ] AC-9: Một run trung bình mục tiêu 35–40 phút, chấp nhận 30–45 phút (đo 5+ người chơi).
- [ ] AC-10: Người chơi thua run nêu được ít nhất một quyết định của mình dẫn tới thua (không chỉ "xui").

## Open Questions

- Mức khó hiển thị ★1–5 theo bảng khoảng — chốt ở `balance`.
- ~~Chiến lợi phẩm có nút Bỏ qua chủ động không?~~ → **Có** (người dùng chốt 2026-11-04: có thể không muốn phần thưởng nào).
- Người chơi có được xem trước *bể* trang bị Mùa sau (chứ không phải loại) để lên kế hoạch không?
