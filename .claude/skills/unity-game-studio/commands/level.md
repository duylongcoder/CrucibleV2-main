# Command: level

**Vai trò**: Game Designer (+ Designer cho hình ảnh, Developer cho dựng) · **Tham số**: `<ID hoặc tên màn>` · `--curve` (đường cong độ khó toàn game) · `--blockout` (dựng greybox trong Unity) · `--review`
**Ghi vào**: `Docs/LEVELS.md` (tạo từ `templates/LEVELS.md` nếu chưa có), scene trong `Assets/` (khi `--blockout`)
**Đọc**: GDD §3 (onboarding), hệ thống liên quan, `ART.md` §2/§6, `references/design-frameworks.md` §6

## Thiết kế một màn

1. **Metrics trước**: §1 LEVELS.md có tốc độ, tầm nhảy, tầm tương tác, khung camera chưa? Chưa → lấy từ SO/code
   (đọc giá trị serialized) hoặc hỏi. Mọi kích thước trong màn đo bằng các đơn vị này.
2. **Mục đích**: màn dạy/kiểm tra cơ chế nào? Theo nguyên tắc *dạy → luyện → thử thách*: cơ chế mới xuất hiện
   ở chỗ an toàn trước, rồi mới kết hợp và gây áp lực. Cảm xúc mục tiêu, beat câu chuyện.
3. **Layout**: sơ đồ ASCII với ký hiệu chuẩn; critical path từng bước; đường phụ/bí mật (thưởng tò mò, không
   phạt người đi đường chính); landmark & dẫn hướng (ánh sáng, đường nét, âm thanh) thay vì tường vô hình.
4. **Encounter / puzzle**: bảng vị trí, thành phần, độ khó; với puzzle ghi lời giải mong đợi + lời giải tắt
   có thể (chấp nhận hay chặn). Màn sinh thủ tục → mô tả luật sinh + ràng buộc phải đúng (luôn có đường tới
   đích, độ khó trong khoảng) thay vì layout cố định.
5. **Nhịp độ**: biểu đồ cường độ, điểm nghỉ, cao trào; xen kẽ căng–nghỉ, không dồn liên tục.
6. Duyệt từng phần → ghi vào LEVELS.md; cập nhật bảng §2 và đường cong §2 nếu đổi độ khó.

## `--curve`
Đường cong độ khó toàn game theo hình răng cưa; mỗi màn: cơ chế mới, độ khó 1–10, thời lượng. Kiểm: cơ chế
được dạy trước khi bị kiểm tra? có bước nhảy độ khó đột ngột? thời lượng tổng khớp phạm vi trong GDD §1?

## `--blockout` (dựng greybox)
1. Cần layout đã duyệt. Dựng bằng **editor script** theo `commands/scene.md` (không sửa tay `.unity`):
   primitive (Cube/Plane, hoặc ProBuilder nếu dự án đã có — không tự thêm package), tile greybox nếu 2D
   (Tilemap — `references/unity-subsystems.md` §Tilemap), đặt Start/Exit/encounter marker là GameObject rỗng có tên rõ.
2. Đúng tỉ lệ metrics; collider đầy đủ; không art.
3. Scene mới → hỏi có thêm vào Build Profile/Scene List không.
4. Chụp ảnh Scene/Game view (nếu backend hỗ trợ) lưu `Docs/evidence/levels/<id>-blockout.png`.
5. Trạng thái màn → `Graybox`.

## `--review`
Soát một màn: critical path rõ? dẫn hướng đủ? metrics đúng (khe nhảy được, hành lang đủ rộng)? nhịp độ? độ khó
khớp đường cong? Trả danh sách vấn đề theo mục + đề xuất sửa không gian cụ thể; không phán quyết chung chung.

## Định nghĩa hoàn thành
- LEVELS.md có đủ mục cho màn; metrics có nguồn; người dùng duyệt layout.
- `--blockout`: scene mở được không lỗi, editor script tạm đã xóa, có ảnh chụp.
- Bước tiếp theo: `playtest <màn>` sau blockout; `asset` cho phần dressing.
