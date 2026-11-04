# Command: art

**Vai trò**: Designer · **Tham số**: `[số mục: 1-4 | 8 | all | resume]` (mặc định theo rigor) · `--audit` (soát asset hiện có so với ART.md) · `--from-project` (suy ra art bible từ asset đã có)
**Ghi vào**: `Docs/ART.md` · **Đọc**: GDD §1–3 + Systems Index, asset hiện có (ảnh chụp, sprite, font), `ART.md` hiện có

Tạo art bible — hợp đồng hình ảnh đảm bảo mọi asset trông như cùng một game.

## Phạm vi theo rigor
`lean`: §0, §1, §4, §8 · `standard`: §0–4 + §8 (khuyến nghị) · `strict`: tất cả. Hỏi người dùng chọn phạm vi,
đánh dấu phương án theo rigor là (Recommended). Đừng viết 10 mục khi dự án chỉ cần 4.

## Bước

1. **Kiểm tra cái đã có trước khi bịa**: asset trong `Assets/**/Art`, `Sprites`, `UI`, font, ảnh chụp game,
   import settings mẫu (`.meta` chỉ **đọc** để biết PPU/filter đang dùng). Đã có hướng đi nhất quán → giữ, ghi lại
   thành văn bản (trừ khi người dùng muốn đổi).
2. **Hỏi khung**: tham chiếu (game, phim, phong cách — để người dùng tự gõ), 2D/3D, camera, độ phân giải, PPU,
   kỹ năng/nguồn art (tự vẽ, mua, AI). Ghi §0.
3. **§1 Visual Identity**: đưa 2–3 hướng khác nhau (mỗi hướng: luật một câu + 2–3 nguyên tắc có phép thử + pillar),
   khuyến nghị một. Duyệt → ghi ngay. Mọi mục sau dựa trên mục này.
4. **§2–4 cùng nhau** (mood, shape, color phải nhất quán với nhau) nhưng trình bày & duyệt **từng mục**:
   - §2: mỗi trạng thái game có cảm xúc, ánh sáng, tính từ, năng lượng — các trạng thái phải khác nhau rõ.
   - §3: silhouette đọc được ở thumbnail; hình khối mang nghĩa (an toàn/nguy hiểm/tương tác).
   - §4: 5–7 màu có vai trò; giá trị sáng-tối đọc được ở grayscale; màu mang nghĩa có kênh dự phòng cho mù màu
     (kiểm tra các cặp đỏ–xanh lá, xanh dương–tím dễ nhầm).
5. **§5–7** (nếu trong phạm vi): nhân vật đọc được trên nền môi trường do §6 định nghĩa; §7 UI phối hợp
   `commands/ui.md` — xung đột thẩm mỹ ↔ khả năng đọc thì nêu cả hai phía, để người dùng quyết.
6. **§8 Asset Standards**: đặt tên, định dạng, kích thước, pivot, **import settings Unity cụ thể** (Texture Type,
   Sprite Mode, PPU, Filter Mode, Compression, Mipmaps, Read/Write, Sprite Atlas), ngân sách (lấy từ TECH §6).
   Ghi cả sở thích art lẫn giới hạn kỹ thuật khi chúng mâu thuẫn + lựa chọn.
7. **§9 References**: 3–5 tham chiếu, mỗi cái lấy gì / tránh gì. Không dùng tên nghệ sĩ còn sống làm định nghĩa.
8. **§10 Prompt Card** (nếu dùng AI tạo ảnh): khối bất biến chuyển từ §1–4, §8 thành chữ cụ thể (hex, góc nhìn,
   viền, kích thước). Đề nghị chốt **một asset mẫu (seed)** trước khi tạo hàng loạt.
9. Lịch sử duyệt; CHECKPOINT.

## `--audit`
1. Liệt kê asset trong thư mục art; đọc import settings (từ `.meta`, chỉ đọc) và kích thước ảnh.
2. So với §8: tên sai quy ước, kích thước lệch, PPU/filter/nén không đúng, Read/Write bật thừa, texture không
   lũy thừa 2 khi cần nén, thiếu atlas, asset không được tham chiếu (orphan — dùng `scripts/find_guid_refs.py`).
3. So với §1–4 (nếu có ảnh): palette trôi, viền/hướng sáng khác, tỉ lệ khác.
4. Báo cáo bảng vấn đề + cách sửa. Sửa import settings: qua Editor (MCP / `AssetImporter` trong editor script
   theo `commands/scene.md`) — **không sửa `.meta` bằng tay**. Đổi tên: qua Editor hoặc `git mv` kèm `.meta`.

## Định nghĩa hoàn thành
- Các mục trong phạm vi đã duyệt từng mục; §8 có import settings Unity cụ thể.
- Bước tiếp theo: `asset` (spec + manifest) hoặc `ui`.
