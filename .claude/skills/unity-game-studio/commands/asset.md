# Command: asset

**Vai trò**: Designer (+ Developer cho import) · **Tham số**: `<hệ thống | màn | màn hình UI | tên asset>` · `--inventory` (lập danh mục toàn game) · `--import <đường dẫn>` (đưa file vào Unity & cấu hình) · `--check <đường dẫn>`
**Ghi vào**: `Docs/ASSETS.md` (tạo từ `templates/ASSETS.md` nếu chưa có), `Assets/**` khi import
**Đọc**: `ART.md` (bắt buộc — chưa có §1, §4, §8 thì đề nghị `art` trước), GDD hệ thống/màn liên quan

## `--inventory`
Đọc GDD, LEVELS, UI spec → liệt kê mọi thực thể cần hình ảnh (nhân vật, kẻ thù, vật phẩm, tile, VFX, UI screen,
HUD element, icon, âm thanh) và trạng thái của mỗi cái. Đề xuất danh mục, người dùng duyệt, ghi bảng ASSETS.md
(trạng thái `planned`). Tách rõ asset production và placeholder greybox.

## Spec asset (mặc định)
Với mỗi asset của mục tiêu:
1. Vai trò gameplay, trạng thái cần thể hiện, kích thước trên màn hình thật.
2. Kỹ thuật: kích thước canvas, số frame & fps, alpha, pivot/baseline (giữ cố định giữa các frame), PPU,
   import settings — lấy từ ART §8, không tự đặt chuẩn khác.
3. Khóa hình ảnh (palette, silhouette, tỉ lệ, viền, hướng sáng).
4. Prompt (nếu tạo bằng AI): khối bất biến ART §10 + `ROLE/PURPOSE` + `SUBJECT`. Biến thể dùng asset mẫu đã
   duyệt làm tham chiếu; mỗi lần sửa chỉ đổi một biến; yêu cầu nền trong suốt bằng tùy chọn của công cụ và
   **kiểm tra alpha thật**.
5. Checklist kiểm tra (trong template). Ghi vào ASSETS.md.

## `--check <file|thư mục>` (kiểm tra kỹ thuật ảnh trước khi import)
Dùng Python + Pillow nếu có (`python3 -c "import PIL"`; thiếu → hỏi trước khi `pip install`):
kích thước đúng spec, chia hết cho kích thước ô/frame, kênh alpha có pixel trong suốt thật (không phải nền
ca-rô giả), số màu (pixel art), viền trống quanh sprite (padding) đủ cho atlas. Ghép contact sheet
(nearest-neighbor, nền ca-rô) để người dùng xem nhanh cả họ asset. Báo cáo pass/fail từng file.

## `--import <đường dẫn>`
1. Copy file vào đúng thư mục theo `CLAUDE.md` / ART §8, tên đúng quy ước. **Không tạo `.meta`** — Unity tạo khi
   import.
2. Cấu hình import settings qua Editor: MCP (`assets-modify` / chạy C#) hoặc editor script tạm dùng
   `TextureImporter` (`textureType`, `spriteImportMode`, `spritePixelsPerUnit`, `filterMode`,
   `textureCompression`, `mipmapEnabled`, `isReadable`, `spritePivot`) rồi `SaveAndReimport()`. Theo quy trình editor
   script của `commands/scene.md` (chạy xong thì xóa script). Có thể tạo **Preset** để áp cho cả thư mục nếu
   dự án dùng Preset.
3. Sprite sheet: slice theo grid/cell size của spec (qua `ISpriteEditorDataProvider` hoặc người dùng dùng Sprite
   Editor); kiểm pivot giữa các frame không trôi.
4. Sprite Atlas theo ART §8 (thêm vào atlas có sẵn hoặc đề xuất atlas mới).
5. Không có backend Editor → hướng dẫn người dùng từng bước trong Inspector.
6. Xem trong game ở kích thước thật (screenshot Game view nếu backend hỗ trợ) → `Docs/evidence/assets/`.
7. Cập nhật ASSETS.md: đường dẫn, trạng thái `imported`, **nguồn gốc + license** (bắt buộc trước `approved`).

## Định nghĩa hoàn thành
- Spec đầy đủ trong ASSETS.md, nhất quán với ART.md.
- Asset import: đúng import settings, xem ở kích thước thật, có nguồn gốc/license.
- Không tạo/sửa `.meta` bằng tay.
