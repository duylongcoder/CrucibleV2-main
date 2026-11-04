# UI Spec — Màn Bàn luyện

> **Trạng thái**: In Review (đủ §1–8) · **Cập nhật**: 2026-11-04 · Chủ sở hữu: Designer
> Framework: uGUI + TextMeshPro (`CLAUDE.md`). Quy tắc chung: `Docs/ART.md` §0, §4, §7, §8.
> Nguồn yêu cầu: `GDD.md` §7, `design/brewing.md` / `economy.md` / `run-flow.md` §UI Requirements, `GDD.md` §6.2.
> Mockup: https://claude.ai/artifact/6DFZnC2X4rcmFpbsQiEZM9

## 1. Mục đích & ngữ cảnh

- **Đến từ**: Chiếu chỉ (đầu Mùa) hoặc kết quả lượt nung trước. **Đi tới**: lượt kế (×3) → Dâng.
- **Người chơi muốn**: xếp nguyên liệu lên lưới theo nhịp → Nung; mua/bán để có nguyên liệu; soi thành phẩm.
- **Phân cấp thị giác**: (1) Lưới lò + vật phẩm trên lưới → (2) Túi (nguyên liệu sẵn có) → (3) Chiếu chỉ thu gọn
  (loại trang bị + thuộc tính ưa) → (4) Chợ → (5) vàng, Uy tín, Mùa/lượt.
- Màn chiếm ~80% thời gian chơi → tương phản vừa, không nhấp nháy thường trực (ART §2 "Bàn luyện").

## 2. Element & trạng thái

UI chỉ **đọc** state (Run / Túi / Chợ / lượt nung / kệ Xúc tác) và gửi lệnh qua event. Tên lệnh tạm, chốt ở `arch`:
`PlaceItem`, `MoveCell`, `RemoveFromGrid`, `Buy`, `Sell`, `Reroll`, `Inspect`, `Brew`, `SkipTurn`,
`ReorderCatalyst`, `SetTimescale`, `SkipReplay`.

**1 · HUD trên**

| Element | Dữ liệu | Trạng thái / hành vi |
|---|---|---|
| Mùa / lượt | Mùa x/8, lượt y/3 | — |
| Chip Chiếu chỉ | loại trang bị + thuộc tính ưa (màu + icon) — **không** có đối thủ (run-flow) | Chạm → mở lại popup Chiếu chỉ (chỉ đọc) |
| Uy tín | 3 ngọn lửa | sáng / tắt (xám **+ hình ✕**, không chỉ màu) |
| Vàng | số vàng | đổi → số bay +/−; mua thiếu vàng → rung |
| Sổ công thức · Menu | — | Sổ tắt khi đang phát lại; Menu luôn bật |

**2 · Chợ** — header: hệ số giá Mùa ↑ / ↓ / = (định tính).

| Element | Trạng thái |
|---|---|
| Ô hàng (≤ 6, số ô TBD economy) | mua được · thiếu vàng (giá đỏ + ✕ + sprite mờ) · bị chặn (Túi đầy / Xúc tác 5/5: icon khóa, lý do ở Bàn soi) · đang chọn (viền teal) · đã mua (ô trống) · đang làm mới (lật) |
| Nút "Làm mới · giá lần kế" | thường · tắt (thiếu vàng) · pressed |
| Vùng bán | chỉ nổi khi đang kéo vật phẩm của mình; kéo vào → "Bán · giá" |
| Tab thu gọn (< 16:9) | đóng · mở (ngăn phủ) |

**3 · Bàn soi**
- Rỗng: "Chạm một vật phẩm để xem".
- Nội dung: icon (kèm hiệu ứng ma thuật nếu có) · tên · loại · tier · gốc/thành phẩm/phế phẩm · nhãn **Ma thuật**
  (công khai) · "Đã soi:" danh sách dòng đã lộ (không bao giờ hiện số dòng còn ẩn).
- Nút theo ngữ cảnh: thành phẩm trong Túi → Soi / Bán / Đặt lên lò · hàng Chợ → Mua (tắt kèm lý do) · vật phẩm
  trên lưới → Gỡ về Túi · phế phẩm, Xúc tác → Bán.
- **Nút Soi luôn bật khi Túi có Kính**, kể cả khi thành phẩm đã lộ hết — nút tự tắt sẽ thành chỉ báo "hết dòng ẩn"
  (GDD §6.2). Bị từ chối → "Không còn gì để soi", Kính không tiêu hao. Có cả hai loại Kính → bước chọn Kính
  thường / Kính tốt.
- Khi phát lại: mờ, không thao tác.

**4 · Lưới lò**

| Trạng thái ô | Hình |
|---|---|
| Trống (= nhịp chờ) | 9 ô giống hệt nhau, không đánh số |
| Có vật phẩm | sprite ×3 (+ hiệu ứng ma thuật nếu có) |
| Thả được | viền teal khi đang kéo / đang ở chế độ đặt |
| Kéo lên trên | teal đậm; ô đã có vật phẩm → đổi chỗ |
| Không thả được (vd Xúc tác) | không ô nào teal; thả → bật về chỗ cũ + rung nhẹ |
| Khóa (phát lại) | khung tối hơn, không nhận input |
| Nhịp hiện tại | viền + quầng vàng ánh lò `#FFE08A` |

**Overlay kết quả** (sau phát lại / tua): sprite lớn + tên; thành phẩm = lóe sáng (+ hiệu ứng ma thuật nếu có);
phế phẩm = khói + chỉ chữ "Phế phẩm"; thành phẩm lần đầu → dòng "Đã ghi vào Sổ công thức"; chạm → lượt kế.

**5 · Thanh hành động**
- Nung: tắt khi lưới trống. Bỏ qua: luôn bật; modal xác nhận nếu lưới có vật phẩm.
- Timescale 1× / 2× / 3×: luôn chọn được, nhớ lựa chọn, tác dụng khi phát lại.
- Dòng gợi ý theo ngữ cảnh ("Kéo hoặc chạm để đặt…" / "Đang nung…").

**6 · Kệ Xúc tác** — 5 ô; trái → phải = thứ tự kích hoạt; kéo để đổi chỗ (vạch báo vị trí chèn). Nháy "đã kích
hoạt" **chỉ lúc hiện kết quả**, không nháy giữa phát lại.

**7 · Túi** — header "x/sức chứa · gồm n trên lò".
- **Luôn vẽ đủ 24 ô (trần)**: ô vượt sức chứa hiện tại là ô khóa "＋" (mục tiêu Nới Túi nhìn thấy được).
- Trạng thái ô: có vật phẩm · đang chọn (teal) · đang kéo (bóng 40% tại chỗ) · sọc "trên lò" (số ô = số vật phẩm
  trên lưới) · trống · khóa.
- Túi đầy mà cố mua → header đỏ + rung.

**Modal thuộc màn này** (spec riêng sau): Sổ công thức · Chiếu chỉ (xem lại) · Menu · Xác nhận Bỏ
qua · Kết quả nung.

## 3. Bố cục

Reference 1920×1080; Canvas Scaler Scale With Screen Size, Match = 1, Reference PPU 96 (ART §8.4). Nền pixel
800×360 ×3 phía sau (vùng an toàn 640×360).

```
┌───────────────────────────── 1 HUD (96 px, neo trên, kéo giãn) ─────────────────────────────┐
│ Mùa 3/8 · Lượt 2/3        [Chiếu chỉ: Kiếm · Sắc bén]                Uy tín ●●○  ●7 [Sổ][☰] │
├──────────────┬─8─┬──────────────────────────────────────┬─8─┬───────────────────────────┤
│ 2 CHỢ        │   │            (lò hero, nguồn sáng)       │   │ 6 KỆ XÚC TÁC  ○○○□□  3/5  │
│ ▢▢▢  giá ↑   │ k │        ┌────┬────┬────┐              │ k ├───────────────────────────┤
│ ▢▢▢          │ h │        │    │    │    │  4 LƯỚI 3×3  │ h │ 7 TÚI  12/14 · 3 trên lò   │
│ [Làm mới·2]  │ e │        ├────┼────┼────┤  ô 144 px    │ e │ ▢▢▢▢                       │
│ [kéo để bán] │   │        │    │    │    │              │   │ ▢▢ ▨▨   ▨ = đang trên lò   │
├──────────────┤ m │        ├────┼────┼────┤              │ m │ ▨ □□ ＋  ＋ = nới được      │
│ 3 BÀN SOI    │ ô │        │    │    │    │              │ ô │                            │
│ icon · tên   │ i │        └────┴────┴────┘              │ i │                            │
│ Đã soi: ...  │   │   5 [Bỏ qua] [ NUNG ] [1× 2× 3×]      │   │                            │
│ [Soi][Bán][Đặt]│ │     dòng gợi ý                        │   │                            │
└──────────────┴───┴──────────────────────────────────────┴───┴───────────────────────────┘
```

| # | Nhóm | Anchor / pivot | Vị trí & kích thước ở 1920×1080 | Ghi chú |
|---|---|---|---|---|
| 1 | HUD trên | top-stretch, pivot (0.5, 1) | cao 96; padding ngang 48 | Canvas tĩnh riêng (ít đổi) |
| 2 | Chợ | top-left, pivot (0, 1) | x 48, y 126, 480×480 | Tối đa 6 ô hàng (3×2) — số ô TBD ở economy |
| 3 | Bàn soi | bottom-left, pivot (0, 0) | x 48, đáy 48, 480×402 | Thẻ cố định thay tooltip nổi |
| 4 | Lưới lò | center, pivot (0.5, 0.5) | tâm (960, 552), 504×504; ô 144, khe 12 | Lò hero (sprite world) phía sau |
| 5 | Thanh hành động + gợi ý | bottom-center, pivot (0.5, 0) | rộng 600, đỉnh y 840; gợi ý ở y 960 | |
| 6 | Kệ Xúc tác | top-right, pivot (1, 1) | phải 48, y 126, 480×156; ô 84 (sprite ×2) | Xúc tác vẫn tính vào sức chứa Túi |
| 7 | Túi | bottom-right, pivot (1, 0) | phải 48, đáy 48, 480×738; ô 102 (sprite 96 + viền 3), khe 12, 4 cột × 6 hàng | Luôn vẽ 24 ô; ô sọc "trên lò" + ô khóa "＋" |
| 8 | Khe môi trường | — (nền world) | giữa cột 2/3 ↔ 4 ↔ 6/7 | Dụng cụ treo, kệ lọ — value tối, không giống vật phẩm (ART §3) |

Mọi kích thước/vị trí là bội số của 3 (1 px gốc).

**Co giãn theo tỉ lệ** (chiều cao cố định 1080 vì Match = 1):
- **16:9 (1920)**: như trên.
- **Rộng hơn (tới 20:9 = 2400)**: cột trái/phải bám cạnh, lưới ở giữa → khe môi trường rộng ra, nền hiện thêm kệ
  ở mép. Không kéo giãn panel.
- **Hẹp hơn 16:9 (vd 16:10 = 1728)**: không đủ 3 cột → Chợ + Bàn soi thu thành **tab dọc** bên trái (72 px);
  chạm tab → mở ngăn phủ lên (modal nhẹ, chạm ngoài để đóng). Chợ vẫn mở sẵn đầu lượt (GDD §7).
- **Safe area mobile** (sau MVP): các nhóm neo cạnh nằm trong `Screen.safeArea`; nền vẫn tràn viền.

**Khi đang phát lại**: lưới khóa; ô của nhịp hiện tại sáng vàng ánh lò; Nung/Bỏ qua tắt; timescale bật; dòng
"Chạm vào lò để tua" phía trên lưới; Chợ/Túi/Bàn soi/Kệ mờ và không nhận input.

## 4. Tương tác

WebGL chuột trước, touch sau — mọi thao tác có **hai đường** (kéo-thả và chạm-chọn → chạm-đích); không phụ thuộc hover.
Input System (`Assets/Settings/InputSystem_Actions.inputactions`) + uGUI EventSystem.

**Chạm / click**
- Chạm vật phẩm (Túi, Chợ, lưới, kệ) → chọn (teal) + hiện ở Bàn soi; chạm lại → bỏ chọn.
- Đang chọn vật phẩm Túi → ô lưới trống sáng teal; chạm ô trống → đặt; chạm ô có vật phẩm → đổi chỗ (vật phẩm cũ về Túi).
- Đang chọn vật phẩm trên lưới → chạm ô khác: di chuyển / đổi chỗ; chạm vùng Túi: gỡ về Túi.
- Nút "Đặt lên lò" (Bàn soi) → chế độ đặt như trên; Esc / chạm ngoài → hủy.
- Hàng Chợ: chạm → Bàn soi → **Mua** (nút Mua chính là bước xác nhận). Hàng vào Túi; Xúc tác vào thẳng kệ.

**Kéo-thả** — bắt đầu kéo khi con trỏ đi ≥ 12 px (dưới ngưỡng = chạm). Vật phẩm theo con trỏ ở ×3 + bóng; đích
hợp lệ sáng teal.

| Từ → đến | Kết quả |
|---|---|
| Túi → ô lưới | đặt / đổi chỗ |
| Lưới → lưới | di chuyển / đổi chỗ |
| Lưới → Túi | gỡ về Túi |
| Chợ → Túi / ô lưới | **modal xác nhận** "Mua X · giá?" → mua (+ đặt vào ô nếu thả lên lưới); bị chặn → bật về, lý do ở Bàn soi |
| Túi / lưới / kệ → vùng bán | bán |
| Kệ → kệ | đổi thứ tự Xúc tác |
| Đích không hợp lệ | bật về chỗ cũ + rung nhẹ |

- **Xác nhận bán** chỉ cho món giá trị: thành phẩm tier ≥ `SellConfirmTier` (mặc định 4, config), vật phẩm có
  Ma thuật, Xúc tác. Món rẻ bán ngay (P4).
- Đang kéo → bỏ qua pointer thứ hai.

**Khi phát lại**: chạm bất kỳ đâu trên vùng lò (lưới + lò hero) → tua (`SkipReplay`); chỉ timescale và Menu
nhận input. Overlay kết quả: chạm bất kỳ → đóng.

**Modal** (Sổ, Menu, Chiếu chỉ, xác nhận): chặn mọi tương tác phía sau; chạm ngoài / Esc → đóng (trừ modal xác
nhận — phải bấm nút).

**Phím tắt (WebGL)**

| Phím | Tác dụng |
|---|---|
| Space / Enter | Nung · tua (khi phát lại) · đóng kết quả · đồng ý (modal xác nhận) |
| 1 / 2 / 3 | timescale |
| R | Làm mới Chợ |
| Esc | bỏ chọn → đóng modal → Menu (theo thứ tự) |
| Chuột phải trên ô lưới | gỡ về Túi |

Gamepad: không trong MVP (không làm điều hướng focus). Vùng chạm: mọi đích ≥ 64 px ở 1080p (ô kệ 84, nút ≥ 72).

## 5. Văn bản

- **Font** (Google Fonts, OFL): tiêu đề / tên vật phẩm **Alegreya SC**; nội dung, số, nút **Be Vietnam Pro** (số
  tabular). ⚠ Kiểm đủ glyph tiếng Việt của Alegreya SC trong TMP Font Asset trước khi chốt — thiếu → Cormorant SC.
- **Thang cỡ chữ (1080p)**: H1 48 (tiêu đề modal) · H2 34 (tiêu đề panel, tên vật phẩm) · Body 26 · Caption **22
  (tối thiểu)**. Line spacing +20% cho dấu chồng.
- **TMP Font Asset**: atlas **tĩnh** bộ Latin + tiếng Việt dựng sẵn (U+00C0–U+01B0, U+1EA0–U+1EF9) — không dynamic
  (sinh glyph lúc chạy trên WebGL dễ giật); SDF 2048², padding 10%. Ký hiệu (★ ↑ ↓ ✕, đồng xu, ngọn lửa) dùng
  **TMP Sprite Asset** pixel.
- **Tràn chữ**: tên vật phẩm (Bàn soi) Auto Size 28–34, tối đa 2 dòng; chip Chiếu chỉ tối đa ~24 ký tự rồi "…". MVP chỉ tiếng Việt; khung chữ dự trù +30% cho tiếng Anh.
- **Màu chữ**: chính `#F4E6C8` (12:1 trên ô, 10.9:1 trên nút) · phụ `#B9A79A` (7.2:1) · trên nút teal `#10201E`
  (8.8:1) · **đỏ** `#D23C52` chỉ 3.6:1 → chỉ ở ≥ 24 px đậm và luôn kèm icon.

## 6. Chuyển động & phản hồi

**Luật**: sprite pixel không scale / xoay lẻ — chỉ dịch theo bước 3 px (1 px gốc), đổi frame, overlay. Khung UI và
chữ được tween bình thường.

| Sự kiện | Thời lượng / cách |
|---|---|
| Chọn (viền teal) | 80 ms |
| Bàn soi đổi nội dung | 120 ms ease-out |
| Đặt vào ô | nảy 6 px, 2 bước, 100 ms |
| Bật về | 180 ms ease-in-out |
| Rung lỗi | ±6 px × 3, 200 ms |
| Chợ lật hàng | 1 frame lật mỗi ô, lệch 40 ms |
| Mở tab Chợ | 220 ms ease-out |
| Modal | fade 150 ms + trượt 6 px |
| Số vàng bay | 600 ms |
| Phát lại | nhịp `BrewTickSeconds` / timescale; ô hiện tại sáng 150 ms; lò bùng theo nhịp |
| Kết quả | thành phẩm lóe 400 ms · phế phẩm khói 600 ms · tua → nhảy thẳng |
| Hiệu ứng Ma thuật | vòng 4 frame, 6 fps |

Không animation nào khóa input (trừ overlay kết quả). Hook âm thanh: chọn, đặt, bật về, mua, bán, lỗi, từng nhịp,
thành / hỏng — nội dung chờ `gdd audio`.

## 7. Accessibility

- Không mang nghĩa chỉ bằng màu: thiếu vàng = đỏ + ✕ · Uy tín mất = ✕ · chọn / thả được = viền dày + teal · nhịp
  hiện tại = quầng + viền · phế phẩm = khói + chữ · Ma thuật = rune chuyển động + nhãn.
- Tùy chọn **Giảm chuyển động** (Menu, MVP): tắt rung, lóe, chớp; phát lại vẫn chạy nhưng không có lò bùng.
- Không giới hạn thời gian (theo lượt). Chữ tối thiểu 22 px. Sau MVP: chữ lớn +20%.

## 8. Tiêu chí kiểm (cho QA)

**Layout** — chạy ở **1920×1080, 1728×1080 (16:10), 2400×1080 (20:9)**:
- L1: các nhóm tương tác (HUD, Chợ, Bàn soi, lưới, thanh hành động, Kệ, Túi) không chồng lấn.
- L2: mọi element nằm trọn trong màn hình (và safe area).
- L3: mọi đích chạm ≥ 64 đơn vị canvas.
- L4: < 16:9 → Chợ + Bàn soi ở dạng tab; ≥ 16:9 → mở.
- L5: Túi luôn 24 ô; số ô khóa = 24 − sức chứa; số ô sọc = số vật phẩm trên lưới.
- L6: chuỗi dài nhất (+30%) không tràn (TMP `isTextOverflowing` = false).

**Hành vi**
- B1: Nung tắt khi lưới trống.
- B2: đang phát lại → Chợ / Túi / Bàn soi / Kệ không nhận input; chạm lò → nhảy tới kết quả.
- B3: nút Soi bật bất cứ khi nào Túi có Kính, bất kể thành phẩm còn dòng ẩn (**không rò rỉ**).
- B4: quét text mọi trạng thái — không chuỗi nào chứa số dòng ẩn, `Power`, khoảng cách công thức.
- B5: kéo từ Chợ → modal xác nhận; hủy → vàng và Túi không đổi.
- B6: thả Xúc tác lên lưới → bật về, lưới không đổi.
- B7: 9 ô lưới cùng sprite, không nhãn số.
- B8 (thủ công): mỗi thao tác kéo-thả có đường chạm tương đương.

**Ảnh bằng chứng** (`Docs/evidence/ui-ban-luyen/`): 16:9 xếp lưới / phát lại / kết quả thành / kết quả phế phẩm ·
16:10 tab đóng + mở · 20:9 · grayscale · Giảm chuyển động.

**Hiệu năng**: Canvas tách HUD tĩnh / panel / lưới / lớp kéo-thả; tắt Raycast Target trên Image trang trí; không
lồng Layout Group nhiều cấp; cập nhật theo event, không poll.

## Lịch sử duyệt

| Ngày | Mục | Thay đổi | Duyệt bởi |
|---|---|---|---|
| 2026-11-04 | §1, §3 | Bố cục 3 cột + thẻ Bàn soi cố định + kệ Xúc tác + ô "trên lò" trong Túi | người dùng |
| 2026-11-04 | §2 | Element & trạng thái; Túi luôn vẽ 24 ô (ô khóa tới trần); nút Soi luôn bật | người dùng |
| 2026-11-04 | §4 | Hai đường thao tác; kéo-mua từ Chợ cần xác nhận; xác nhận bán đồ giá trị; phím tắt | người dùng |
| 2026-11-04 | §5–7 | Font Alegreya SC + Be Vietnam Pro (chờ kiểm glyph); thang chữ; luật chuyển động pixel; Giảm chuyển động | người dùng |
| 2026-11-04 | §8 | Tiêu chí QA; layout test chỉ 3 độ phân giải cao 1080 | người dùng |
| 2026-11-04 | §2, §3, §5 | Bỏ Sổ thí nghiệm (một nút Sổ); chip Chiếu chỉ không có đối thủ | người dùng |
