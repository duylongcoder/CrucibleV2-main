# Command: systems

**Vai trò**: Game Designer (+ Developer góp ý phụ thuộc kỹ thuật) · **Tham số**: không · `--refresh` (cập nhật sau thay đổi)
**Ghi vào**: `Docs/GDD.md` §4 Systems Index · **Đọc**: GDD §1–3, `TECH.md` §3 (nếu có code)

Phân rã concept thành các hệ thống, xác định phụ thuộc và thứ tự thiết kế/xây dựng.

## Bước

1. **Đọc** GDD §1–3. Trống → đề nghị `concept` trước, dừng.
2. **Liệt kê hệ thống tường minh** (được nhắc trong concept/loop) và **hệ thống ngầm** (cần nhưng chưa nói:
   input, camera, scene flow/state machine game, save/load, settings, audio manager, UI framework, pause,
   tutorial, random/seed, localization nếu đa ngôn ngữ). Đánh dấu hệ thống ngầm `(suy ra)`.
3. **Người dùng duyệt danh sách**: thêm/bớt/gộp/tách. Dự án đã có code → đối chiếu `TECH.md` §3 / quét
   `Assets/**/*.cs` theo namespace để đánh dấu hệ thống đã tồn tại (trạng thái `Implemented (chưa có GDD)`).
4. **Phụ thuộc**: với mỗi hệ thống, cần dữ liệu/sự kiện từ hệ thống nào. Sắp xếp theo lớp
   Foundation → Core → Feature → Presentation → Polish. Phát hiện **phụ thuộc vòng** → đề xuất tách bằng
   interface/event hoặc thiết kế song song.
5. **Ưu tiên**: MVP (thiếu thì không trả lời được "có vui không"), Vertical Slice, Alpha, Full Vision. Gợi ý
   tự động theo concept rồi để người dùng chỉnh. Giữ MVP nhỏ nhất có thể.
6. **Rủi ro**: hệ thống chưa chắc về kỹ thuật/thiết kế/phạm vi → đánh dấu prototype sớm.
7. **Ghi** §4: bảng Systems Index, thứ tự thiết kế, rủi ro, phụ thuộc vòng. Thêm feature tương ứng vào
   `PLAN.md` §3 backlog (trạng thái Backlog, ưu tiên theo tier).
8. CHECKPOINT: hệ thống MVP đầu tiên cần `gdd`.

## Định nghĩa hoàn thành
- Mọi hệ thống có loại, ưu tiên, phụ thuộc; không còn phụ thuộc vòng chưa có hướng xử lý.
- Thứ tự thiết kế rõ; backlog PLAN.md đồng bộ.
- Bước tiếp theo: `gdd <hệ thống MVP đầu tiên>`; song song `art` nếu chưa có ART §1–4.
