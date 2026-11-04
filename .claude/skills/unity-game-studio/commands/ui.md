# Command: ui

**Vai trò**: Designer (UX + visual) · **Tham số**: `<tên màn hình | HUD | flow>` · `--review` · `--build` (dựng hierarchy trong Unity — chuyển sang `commands/scene.md`)
**Ghi vào**: `Docs/ART.md` §7 (quy tắc UI chung), `Docs/GDD.md` §7 (luồng màn hình) hoặc `Docs/design/ui-<screen>.md` cho spec màn hình lớn
**Đọc**: GDD hệ thống liên quan (§UI Requirements), ART §4, §7, `CLAUDE.md` (uGUI hay UI Toolkit, input)

## Xác định framework UI
Dùng framework dự án đang dùng (`CLAUDE.md` §Môi trường): uGUI (Canvas, RectTransform, TextMeshPro) hoặc
UI Toolkit (UXML/USS, UIDocument). Dự án mới chưa chọn → nêu ưu/nhược ngắn (uGUI: world-space, hệ sinh thái,
animation dễ; UI Toolkit: layout kiểu CSS, nhiều element, data binding) và hỏi. Không trộn hai framework cho cùng
loại màn hình nếu không có lý do.

## Spec một màn hình

1. **Mục đích & ngữ cảnh**: người chơi đến từ đâu, muốn làm gì, đi đâu tiếp (luồng). Thông tin nào quan trọng
   nhất (phân cấp thị giác).
2. **Nội dung**: danh sách element: loại (label, button, toggle, list, gauge...), dữ liệu hiển thị (nguồn dữ liệu
   — UI chỉ **đọc** state, gửi lệnh qua event), trạng thái (normal/hover/pressed/disabled/selected/empty/loading/error).
3. **Bố cục**: wireframe ASCII; anchor/pivot cho từng nhóm (góc, cạnh, giữa), cách co giãn theo tỉ lệ màn hình
   (16:9 → 21:9, 4:3, dọc nếu mobile), safe area; CanvasScaler `Scale With Screen Size` + reference resolution
   từ ART §0.
4. **Tương tác**: chuột/phím/gamepad/touch — điều hướng focus (gamepad), phím tắt, vùng chạm tối thiểu (mobile
   ~44pt). Modal chặn tương tác phía sau.
5. **Văn bản**: font, cỡ (theo thang ART §7), tràn chữ (độ dài chuỗi tối đa; nếu đa ngôn ngữ dự trù +30–40%),
   fallback font cho ký tự cần hỗ trợ (tiếng Việt có dấu → kiểm tra glyph trong TMP Font Asset).
6. **Chuyển động & phản hồi**: thời lượng, easing; âm thanh khi bấm.
7. **Accessibility**: contrast chữ/nền, không chỉ dùng màu để mang nghĩa, cỡ chữ tối thiểu, tùy chọn (nếu GDD có).
8. **Tiêu chí kiểm**: viết sẵn cho QA — layout assertion (không chồng lấn, không tràn khung, nằm trong màn hình,
   bấm tới được) và khía cạnh cần kiểm bằng ảnh (contrast, typography, trạng thái hiển thị).
9. Duyệt từng phần → ghi. Quy tắc chung mới (vd màu nút primary) → ghi vào ART §7.

## HUD
Thêm: thông tin nào cần thấy liên tục vs theo ngữ cảnh; vị trí không che hành động chính; diegetic hay
screen-space; cập nhật theo event (không poll mỗi frame, không rebuild cả Canvas — tách Canvas tĩnh/động).

## `--review`
Soát spec/màn hình hiện có: phân cấp thông tin, nhất quán với ART §7, trạng thái đủ, điều hướng gamepad, tràn chữ,
contrast, chặn modal, hiệu năng (Canvas rebuild, Layout Group lồng sâu, Raycast Target thừa trên Image trang trí).
Kết luận `APPROVED` / `NEEDS REVISION` + danh sách cụ thể.

## Định nghĩa hoàn thành
- Spec có luồng, element + trạng thái, bố cục + anchor, tương tác, văn bản, tiêu chí kiểm cho QA.
- Bước tiếp theo: `scene` (dựng hierarchy) → `build` (logic/binding) → `test` (layout + ảnh).
