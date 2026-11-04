# Art Bible — CrucibleV2 (tên tạm)

> **Trạng thái**: Draft · **Cập nhật**: 2026-11-03 · Chủ sở hữu: Designer
> Đây là hợp đồng hình ảnh. Mọi asset mới được so với tài liệu này **ở kích thước thật trong game**.
> Rigor `standard` cần §1–4 và §8; `strict` cần đủ 10 mục.

## 0. Khung kỹ thuật

| Mục | Giá trị |
|---|---|
| 2D / 3D / 2.5D | 2D (theo template; xác nhận ở `art`) |
| Camera / góc nhìn | {{orthographic top-down / side / isometric / perspective 3rd person}} |
| Độ phân giải tham chiếu | {{1920×1080; CanvasScaler reference resolution}} |
| Pixels Per Unit (2D) | {{vd 32}} — kích thước ô lưới / tile: {{}} |
| Render pipeline | URP 17.6.0 — 2D Renderer |
| Kích thước nhân vật trên màn hình | {{vd ~64 px cao}} |
| Filter | {{Point (pixel art) / Bilinear}} |

## 1. Visual Identity Statement

**Luật một câu**: {{"Khi phân vân, chọn ..."}}

| Nguyên tắc | Phép thử | Pillar phục vụ |
|---|---|---|
| {{}} | {{khi X mơ hồ → chọn Y}} | {{P1}} |

## 2. Mood & Atmosphere

| Trạng thái game | Cảm xúc mục tiêu | Ánh sáng (nhiệt độ màu, tương phản) | 3–5 tính từ | Năng lượng |
|---|---|---|---|---|
| Khám phá | {{}} | {{}} | {{}} | {{}} |
| Chiến đấu / thử thách | {{}} | {{}} | {{}} | {{}} |
| Thắng / Thua | {{}} | {{}} | {{}} | {{}} |
| Menu | {{}} | {{}} | {{}} | {{}} |

## 3. Shape Language

- **Silhouette nhân vật**: {{đọc được ở thumbnail; mỗi archetype một đặc điểm phân biệt}}
- **Hình khối môi trường**: {{góc cạnh / bo tròn / hữu cơ — vì sao}}
- **Ngữ pháp hình khối UI**: {{UI lặp lại thế giới hay là ngôn ngữ HUD riêng}}
- **Hero shape vs phụ**: {{cái gì thu hút mắt, cái gì lùi về sau}}

## 4. Color System

| Vai trò | Màu (hex) | Ý nghĩa trong game | Kênh dự phòng (mù màu) |
|---|---|---|---|
| Primary | {{#}} | {{}} | — |
| Nguy hiểm | {{#}} | {{}} | {{icon/hình/âm thanh}} |
| Phần thưởng | {{#}} | {{}} | {{}} |
| Tương tác được | {{#}} | {{}} | {{}} |
| Trung tính / nền | {{#}} | {{}} | — |

- **Giá trị sáng-tối**: {{số dải value; ảnh grayscale vẫn đọc được chủ thể}}
- **Nhiệt độ màu theo khu vực**: {{}}
- **Palette UI** và chỗ khác palette thế giới: {{}}

## 5. Character Design Direction

{{Archetype người chơi; phân biệt kẻ thù / NPC / đồng minh trong nháy mắt; phong cách biểu cảm & dáng; mức chi tiết còn lại ở khoảng cách camera}}

## 6. Environment Design Language

{{Phong cách kiến trúc; texture (painted / PBR / stylized — vì sao); mật độ prop theo loại khu vực; kể chuyện bằng môi trường}}

## 7. UI / HUD Visual Direction

| Mục | Quy định |
|---|---|
| HUD | {{diegetic / screen-space}} |
| Font chính / phụ | {{tên font, TMP Font Asset, fallback cho ngôn ngữ cần hỗ trợ}} |
| Thang cỡ chữ | {{H1/H2/Body/Caption ở reference resolution}}; tối thiểu đọc được: {{}} |
| Icon | {{flat / outline / illustrated — chỉ chọn một}} |
| Trạng thái control | {{normal / highlighted / pressed / disabled / selected — màu, hình}} |
| Chuyển động UI | {{thời lượng, easing, overshoot cho "pop", ease-out cho "settle"}} |
| Safe area & tỉ lệ màn hình | {{16:9 → 21:9, notch mobile}} |

Xung đột hình ảnh ↔ khả năng đọc đã quyết: {{mô tả + lựa chọn}}

## 8. Asset Standards (hợp đồng kỹ thuật)

**Đặt tên**: `{{category}}_{{name}}_{{variant}}_{{size}}.{{ext}}` — vd `char_slime_idle_01.png`, `ui_btn_primary_hover.png`, `vfx_hit_spark_small.png`

| Loại | Định dạng nguồn | Kích thước / poly | Import settings Unity | Ngân sách |
|---|---|---|---|---|
| Sprite nhân vật | PNG RGBA | {{64×64, pivot bottom-center}} | Sprite (Multiple), PPU {{32}}, Point, no mipmap, compression {{}} | {{}} |
| Tile | PNG | {{32×32}} | {{}} | {{}} |
| UI | PNG / SVG | {{9-slice border}} | Sprite (Single), mesh Full Rect | {{}} |
| Icon | PNG | {{128×128}} | {{}} | {{}} |
| Model 3D | FBX | {{tris}} | Scale 1, {{LOD}} | {{}} |
| Texture 3D | PNG/TGA | {{1024/2048}} | {{sRGB, compression theo nền tảng}} | {{MB}} |
| Audio | WAV 48k | — | {{Load Type, mono cho 3D}} | {{}} |

- **Sprite Atlas**: {{nhóm atlas theo màn/scene}}
- **Read/Write**: tắt trừ khi cần đọc pixel lúc chạy.
- Xung đột sở thích nghệ thuật ↔ giới hạn kỹ thuật: {{ghi cả hai + lựa chọn}}

## 9. Reference Direction

| Tham chiếu | Lấy gì | Tránh gì (để không thành bản sao) |
|---|---|---|
| {{game/phim/phong cách}} | {{}} | {{}} |

Không dùng tên nghệ sĩ còn sống làm định nghĩa phong cách — chuyển thành thuộc tính quan sát được.

## 10. AI Prompt Card (nếu dùng công cụ tạo ảnh)

Khối bất biến (dán vào mọi prompt cùng họ asset):
```
ART DIRECTION: {{shape language}}, {{palette roles + hex}}, {{materials}}, {{edge/outline treatment}}
VIEW: {{orthographic side view / top-down 3/4}}, {{facing}}
GAME-SCALE READ: {{silhouette & chi tiết phải còn ở WxH px}}
TECHNICAL OUTPUT: {{kích thước}}, transparent background, {{số frame/ô}}
LOCKS: preserve {{identity, proportions, palette, outline, light direction}}
EXCLUDE: text, labels, watermark, mockup frame, scenery, duplicate objects, cropped edges
```
Khối thay đổi theo asset: `ROLE/PURPOSE`, `SUBJECT`.
Asset mẫu đã duyệt (seed): {{đường dẫn}} — mọi biến thể dùng mẫu này làm tham chiếu.

## Lịch sử duyệt

| Ngày | Mục | Thay đổi | Duyệt bởi |
|---|---|---|---|
