# Asset Manifest — {{TÊN GAME}}

> Chủ sở hữu: Designer · **Cập nhật**: {{YYYY-MM-DD}} · Chuẩn chung: `Docs/ART.md` §8
> Mỗi asset ship phải có nguồn gốc + license. Placeholder (greybox) đánh dấu rõ `placeholder`.

## Tổng quan

| Trạng thái | Số lượng |
|---|---|
| planned | {{}} |
| in-progress | {{}} |
| imported (chưa duyệt) | {{}} |
| approved | {{}} |
| placeholder | {{}} |

## Danh mục

| ID | Asset | Vai trò gameplay | Đường dẫn Unity | Kích thước / frame | Pivot | Trạng thái | Nguồn / license |
|---|---|---|---|---|---|---|---|
| A-001 | {{char_player_idle}} | {{nhân vật chính, đứng yên}} | {{Assets/_Project/Art/Characters/...}} | {{64×64 ×4}} | {{bottom-center}} | {{planned}} | {{tự vẽ / tool + ngày / CC0 ...}} |

## Spec chi tiết

### A-001 — {{Tên asset}}

- **Dùng ở**: {{hệ thống / màn / màn hình UI}}
- **Trạng thái cần thể hiện**: {{idle, run, hit, die / normal, hover, pressed, disabled}}
- **Kích thước & định dạng**: {{}} · **Alpha**: {{có}} · **Frame**: {{số frame, fps}}
- **Pivot / anchor / baseline**: {{}}
- **Import settings**: Texture Type {{Sprite}}, Mode {{Single/Multiple}}, PPU {{}}, Filter {{Point}},
  Compression {{}}, Mipmaps {{off}}, Read/Write {{off}}, Atlas {{}}
- **Khóa hình ảnh** (không được trôi): {{palette, silhouette, tỉ lệ, hướng sáng, độ dày viền}}
- **Prompt (nếu tạo bằng AI)**: khối bất biến từ `ART.md` §10 +
  ```
  ROLE/PURPOSE: {{}}
  SUBJECT: {{}}
  ```
- **Kiểm tra**:
  - [ ] Đúng kích thước / số frame, alpha thật (không nền giả)
  - [ ] Xem ở kích thước thật trong game, trên nền gameplay thật
  - [ ] Đồng nhất với asset mẫu đã duyệt
  - [ ] Import settings đúng, không trôi pivot giữa các frame
  - [ ] Đã ghi nguồn gốc / license

## Nguồn gốc & license

| Asset | Nguồn (URL / công cụ) | Tác giả | Ngày | License | Ghi công | Hạn chế |
|---|---|---|---|---|---|---|
