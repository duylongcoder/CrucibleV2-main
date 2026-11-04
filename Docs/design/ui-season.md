# UI Spec — Chuỗi màn trong một Mùa (Chiếu chỉ · Dâng · cutscene Rèn · Kết quả & Chiến lợi phẩm)

> **Trạng thái**: In Review · **Cập nhật**: 2026-11-04 · Chủ sở hữu: Designer
> Framework uGUI + TMP. Quy tắc chung: `ART.md` §4, §7, §8. Luật: `design/run-flow.md`. Màn chính: `ui-ban-luyen.md`.
> Trận (combat + hoạt ảnh) **thiết kế riêng** — ở đây chỉ ghi ràng buộc (§D).

## A. Chiếu chỉ

- **Mục đích**: công bố mục tiêu Mùa, giữ ngắn (P4). Đến từ: đầu run / Chiến lợi phẩm / Kết quả thua. Đi tới: Bàn luyện lượt 1.
- **Bố cục**: popup cuộn chiếu ở giữa, trên nền xưởng làm mờ (anchor center, ~960×720).

```
        ┌──────────── cuộn chiếu (mở ra 220 ms) ────────────┐
        │                     MÙA 3                         │
        │        Vương quốc Vael khiêu chiến  [huy hiệu]     │
        │              [ icon Kiếm lớn ×4 ]                 │
        │   Nhà Vua cần rèn một thanh Kiếm  ◆ Sắc bén       │
        │   (Mùa sau: giá Kính soi tăng)   ← chỉ khi có     │
        │                [ Nhận chiếu chỉ ]                 │
        └───────────────────────────────────────────────────┘
```

| Element | Dữ liệu | Ghi chú |
|---|---|---|
| Tiêu đề | "Mùa x" | H1 |
| Đối thủ | **chỉ tên vương quốc** + huy hiệu | ★ và loại trận lộ ở Trận |
| Icon trang bị | loại trang bị, sprite ×4 | hình lớn nhất trên chiếu |
| Câu yêu cầu | "Nhà Vua cần rèn một <loại> <thuộc tính ưa>" | thuộc tính ưa = trọng số cao nhất của loại; vàng `#F6C945` **+ icon** |
| Modifier Mùa sau | một dòng | chỉ khi Mùa sau có modifier |
| Nút "Nhận chiếu chỉ" | — | primary teal; Space / Enter |

- Sau khi đóng: chip HUD Bàn luyện giữ tóm tắt; chạm chip → mở lại popup (chỉ đọc, nút đổi thành "Đóng").
- Hệ số giá Mùa hiển thị ở header Chợ, không ở đây.

## B. Dâng

- **Mục đích**: chọn 1 thành phẩm đạt chuẩn để dâng (luôn tiêu hao). Có thể soi; không mua bán.
- **Bố cục**: dùng lại khung Bàn luyện (anchor như `ui-ban-luyen.md` §3), thay:

```
┌ HUD (Mùa 3/8 · Dâng) ───────────────────────────────────────────────┐
│ CHIẾU CHỈ (thay Chợ)  │     [cận thần cầm KHAY]       │ TÚI (lọc)   │
│ Kiếm ◆ Sắc bén        │        ┌──────────┐           │ ▢ đạt chuẩn │
│ Vương quốc Vael       │        │  khay    │           │ ░ mờ: không │
│───────────────────────│        └──────────┘           │   dâng được │
│ BÀN SOI (Soi được)    │   [ DÂNG LÊN ] (tắt khi trống)│             │
└──────────────────────────────────────────────────────────────────────┘
```

| Element | Trạng thái / hành vi |
|---|---|
| Thẻ Chiếu chỉ (thay Chợ) | nhắc loại trang bị + thuộc tính ưa + tên vương quốc |
| Cận thần + khay (giữa, world sprite) | khay trống · có khoáng (sprite ×3 + Ma thuật) · khay là đích thả (viền teal khi kéo) |
| Túi | giữ 24 ô; món không dâng được (gốc, phế phẩm, Kính, Xúc tác) **mờ + không thao tác** — không ẩn để bố cục không nhảy |
| Kệ Xúc tác | ẩn |
| Bàn soi | như Bàn luyện; nút: Soi · **Đặt lên khay** |
| Nút "Dâng lên" | tắt khi khay trống → modal xác nhận "Dâng X? Khoáng sẽ bị tiêu hao" → cận thần quay đi → cutscene |
| Không có khoáng đạt chuẩn | khay trống + dòng "Không có khoáng đạt chuẩn để dâng"; nút đổi thành "Để cận thần ra về" → Kết quả **Thua** (bỏ qua cutscene và Trận) |

- **Tương tác**: chạm thành phẩm → Bàn soi → "Đặt lên khay", hoặc kéo Túi → khay. Khay có món → món mới thay, món cũ
  về Túi. Kéo khay → Túi = gỡ.

## C. Cutscene Rèn

- **Luật** (run-flow): ≤ 9 nhịp × 1 s ở 1×; **không tua**; timescale chung 1× / 2× / 3× (≤ 3 s ở 3×). Timeline
  chung, vật phẩm gắn lúc chạy.

| Nhịp | Nội dung | Gắn lúc chạy |
|---|---|---|
| 1 | Cận thần mang khay tới lò rèn | sprite khoáng (+ Ma thuật) |
| 2 | Thợ rèn cầm khoáng lên kiểm tra + **biểu cảm** | khoáng; biểu cảm theo `Quality` (dưới) |
| 3–4 | Đúc trang bị — **mỗi loại trang bị một cảnh** (Kiếm / Giáo / Giáp / Khiên) | cảnh theo loại |
| 5 | Trang bị hoàn thành | sprite trang bị theo loại × màu khoáng (+ Ma thuật) |
| 6 | Cận thần nhận trang bị, mang tới dũng sĩ | trang bị |
| 7 | Dũng sĩ mặc trang bị | trang bị trên dũng sĩ |
| 8 | Bước ra đấu trường → **Trận** | — |

- **Biểu cảm thợ rèn** (quyết định người dùng 2026-11-04): 3 mức định tính — thất vọng / bình thường / hài lòng —
  theo `Quality` so với mức chuẩn của tier (`SmithReactionBands`, config). **Không** dựa trên đối thủ hay kết quả trận
  (không được báo trước kết quả). Ngoại lệ có chủ đích với dòng ẩn — GDD §6.2.
- **UI**: nút timescale góc dưới-phải; một dòng phụ đề mỗi nhịp ("Thợ rèn đang đúc Kiếm…"). Không có nút khác.

## D. Trận — ràng buộc cho bản thiết kế riêng

- Luật + UI: `design/battle.md` (mô phỏng tất định theo lượt, 6 nhịp, thanh máu + số sát thương, lộ thuộc tính).
- Lộ ★ và loại trận khi bắt đầu Trận (tên vương quốc đã biết từ Chiếu chỉ).

## E. Kết quả & Chiến lợi phẩm

Popup giữa màn trên nền đấu trường (anchor center).

| Trường hợp | Nội dung | Nút |
|---|---|---|
| **Thắng** (Mùa 1–7) | "THẮNG" + "Kiếm <khoáng> thắng Vương quốc <tên>" → **+X vàng** (đếm 600 ms) → **≤ 3 thẻ lựa chọn** (icon, tên, loại, mô tả ngắn) — mọi thẻ hiện ra đều nhận được (lọc trước khi rút, run-flow) | chọn thẻ (teal) → **Nhận**; **Bỏ qua** luôn có (không nhận gì, vàng vẫn giữ) |
| Không còn lựa chọn nào nhận được | dòng "Không có phần thưởng phù hợp" (Túi đầy / đủ 5 Xúc tác / Túi ở trần) | **Bỏ qua** |
| **Thắng Mùa 8** | "THẮNG" (không có vàng — Mùa cuối không có Chiến lợi phẩm) | **Hoàn thành** → kết thúc run |
| **Thua** | "THUA" + một ngọn lửa Uy tín vỡ (✕) | **Tiếp tục** → Chiếu chỉ Mùa sau / Trục xuất nếu Uy tín = 0 |

- Nội dung phần thưởng: TBD (thiết kế sau).

## F. Tiêu chí kiểm

- A1: Chiếu chỉ không chứa ★, loại trận, hệ số giá (quét text).
- A2: Thuộc tính ưa có icon đi kèm (không chỉ màu).
- B1: Món không dâng được không chọn / kéo được; "Dâng lên" tắt khi khay trống.
- B2: Không có khoáng đạt chuẩn → nút "Để cận thần ra về" → Thua, không vào cutscene.
- C1: Cutscene ≤ 9 s ở 1×, ≤ 3 s ở 3×; không input nào tua được.
- C2: Biểu cảm thợ rèn chỉ phụ thuộc `Quality` + tier: hai khoáng cùng `Quality`, khác đối thủ → cùng biểu cảm.
- C3: Cảnh đúc đúng loại trang bị của Chiếu chỉ (1.000 Mùa ngẫu nhiên).
- E1: Thắng → vàng cộng đúng 1 lần; nhận đúng 1 thẻ; Thua → không có thẻ.
- Ảnh bằng chứng (`Docs/evidence/ui-season/`): Chiếu chỉ có / không modifier · Dâng có / không khoáng đạt chuẩn ·
  3 biểu cảm thợ rèn · Thắng (thẻ đủ / có thẻ mờ) · Thua.

## Lịch sử duyệt

| Ngày | Mục | Thay đổi | Duyệt bởi |
|---|---|---|---|
| 2026-11-04 | A–F | Chiếu chỉ chỉ tên vương quốc; Dâng dùng khay của cận thần; cutscene 8 nhịp có biểu cảm thợ rèn theo `Quality`; Trận tách riêng; popup Kết quả + vàng + chọn 1/3 | người dùng |
