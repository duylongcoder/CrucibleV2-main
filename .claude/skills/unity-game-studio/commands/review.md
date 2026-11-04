# Command: review

**Vai trò**: Developer (+ QA cho khả năng test) · **Tham số**: `[file | thư mục | "diff" | commit-range]` (mặc định `diff` = thay đổi chưa commit so với HEAD) · `--fix` (áp dụng thay đổi bắt buộc sau khi người dùng đồng ý)
**Ghi vào**: báo cáo; `Docs/TECH.md` §8 cho nợ kỹ thuật được chấp nhận · **Đọc**: `references/unity-coding-rules.md`, `references/performance.md`, TECH.md (ADR), GDD/plan liên quan

## Bước
1. Xác định phạm vi: `git diff` / `git diff --stat` hoặc file chỉ định. Đọc toàn bộ file bị đổi + test tương ứng.
   Có plan/GDD → đọc để so hành vi với spec.
2. Soát theo thứ tự ưu tiên (bỏ mục không liên quan):

| Nhóm | Câu hỏi |
|---|---|
| Đúng | Làm đúng spec/plan? Edge case trong GDD được xử lý? Có lỗi logic, off-by-one, null? |
| Vòng đời Unity | Event subscribe/unsubscribe cặp đôi? Coroutine/async hủy theo vòng đời (`destroyCancellationToken`)? Truy cập object đã destroy (`?.`/`??` trên `UnityEngine.Object`)? Phụ thuộc thứ tự `Awake` giữa object? |
| Serialization | Đổi tên field serialized thiếu `[FormerlySerializedAs]`? `public` field chỉ để hiện Inspector? Kiểu không serialize được? SO bị sửa lúc runtime mà không reset? |
| Hiệu năng | `Find`/`GetComponent`/`Camera.main` (bản cũ) trong Update? alloc mỗi frame (string, LINQ, closure, boxing, `new` collection)? `CompareTag`? Physics `NonAlloc`? Instantiate/Destroy liên tục thay vì pool? |
| Kiến trúc | Theo pattern dự án/ADR? UI giữ state game? singleton giữ state? phụ thuộc ngược lớp? magic number thay vì SO? |
| Test | Có test cho logic mới? test có thể fail? đặt tên đúng quy ước? không control flow trong test? không chờ thời gian cố định? dọn GameObject? |
| An toàn | `try/catch` nuốt lỗi? secret hard-code? `Reflection.Emit` (IL2CPP)? reflection-only thiếu `[Preserve]`/`link.xml`? |
| Asset | `.meta` đi kèm file mới/đổi tên? YAML sửa tay? |

3. **Báo cáo**:
   ```
   ## Review: <phạm vi>
   Verdict: APPROVED | APPROVED WITH SUGGESTIONS | CHANGES REQUIRED
   ### Bắt buộc sửa  (file:dòng — vấn đề — hậu quả cụ thể — cách sửa)
   ### Đề xuất       (không chặn)
   ### Điểm tốt
   ### Ngoài phạm vi (ghi nhận, không sửa trong review này)
   ```
   Mỗi phát hiện nói rõ **điều gì sẽ hỏng** và **khi nào** — không phát hiện chung chung.
4. `--fix`: chỉ áp dụng mục "Bắt buộc sửa" người dùng đồng ý; chạy test sau khi sửa.

## Khi nhận review từ người khác
Đánh giá từng góp ý theo ngữ cảnh: hợp → áp dụng; không hợp (code cố ý làm khác) → từ chối, báo lý do, thêm comment
"why not" nếu code làm điều không hiển nhiên. Góp ý xóa test vì "trùng": giữ nếu test đó ứng với một yêu cầu/trường
hợp được nêu trong spec/plan.
