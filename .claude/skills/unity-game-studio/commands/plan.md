# Command: plan

**Vai trò**: Developer (+ QA cho test cases, Game Designer cho roadmap) · **Tham số**: `<feature>` · `roadmap` · `sprint`
**Ghi vào**: `Docs/plans/<feature>.md` (template `templates/feature-plan.md`), `Docs/PLAN.md`
**Đọc**: GDD hệ thống, TECH.md, code liên quan, `references/unity-testing.md` §Thiết kế test

Không code trong command này. Đầu ra là plan đủ chi tiết để `build` chạy mà không phải đoán.

## `<feature>` — plan triển khai test-first

1. **Hiểu yêu cầu**: đọc GDD (Rules, Edge Cases, Acceptance Criteria). Trích yêu cầu thành R1, R2... Mục `TBD`
   → bỏ khỏi phạm vi (chỉ hỏi nếu nó chặn cả thiết kế). Là bug chứ không phải feature → chuyển sang `bug`.
2. **Khảo sát code**: class liên quan, chỗ gọi, prefab/scene tham chiếu (`scripts/find_guid_refs.py` cho script
   sẽ sửa), test gần nhất (làm mẫu phong cách), asmdef. Ghi lại để không phải tìm lại.
3. **Thiết kế triển khai** (chỉ thiết kế — không test case ở bước này):
   - Class + trách nhiệm, chữ ký public/internal method, interface phụ thuộc, dữ liệu SO, vị trí file + assembly.
   - Test qua cùng "seam" mà code production dùng; không mở `private` thành `public` chỉ để test.
   - Tên class/method người dùng đặt không hợp với spec → đề xuất tên tốt hơn, chấp nhận lựa chọn cuối của người dùng.
4. **Thiết kế test cases** (tách bước, theo `references/unity-testing.md`):
   - Gán từng đối tượng vào lớp: Editor / Unit / Integration / Visual verification / Manual (manual chỉ cho
     thứ không tự động được: feel, animation polish, âm thanh).
   - Kỹ thuật: phân vùng tương đương, giá trị biên, bảng quyết định, chuyển trạng thái, đoán lỗi.
   - Một method = một phân vùng = một kết quả. Cột *Kiểm tra* chỉ ghi kết quả quan sát được (không attribute,
     không cách dựng input, không lặp lại điều kiện).
   - Truy vết: mỗi R có ít nhất một test cùng lớp (yêu cầu về UI phải có integration/visual test).
   - Đánh giá **Testability**: `PASS` / `WARN` (ghi vào Rủi ro & đánh đổi) / `FAIL` (SUT quá lớn, state ẩn,
     tổ hợp bùng nổ, không inject được) → quay lại bước 3 sửa thiết kế (tối đa 1 lần), vẫn FAIL → hỏi người dùng:
     chấp nhận / sửa yêu cầu / cho gợi ý thiết kế.
5. **Bước thực hiện**: dùng 5 bước chuẩn trong template (Skeleton → Test first → Implement → Refactor → Bằng chứng
   & tài liệu) + bước riêng của dự án (scene/prefab cần sửa, dữ liệu SO cần tạo, Build Profile...). Mỗi bước có
   lệnh kiểm chứng cụ thể.
6. **Ghi** `Docs/plans/<feature>.md`; PLAN.md backlog → `Planned` + link plan. Trình bày tóm tắt và **chờ người
   dùng duyệt** trước khi `build`.

## `roadmap`
Từ Systems Index + ưu tiên: dựng/cập nhật PLAN.md §2 milestones (mỗi milestone là một câu hỏi cần trả lời,
vd "M0: core loop có vui không?") và §3 backlog. Feature ước lượng L → đề xuất chia nhỏ. Rủi ro cao → đưa lên sớm.

## `sprint`
Chọn việc cho chu kỳ tới từ backlog: Must / Should / Nice, theo phụ thuộc và năng lực (hỏi số phiên/tuần).
Ghi PLAN.md §4 + rủi ro. Chuyển việc chưa xong của sprint trước (carryover) kèm lý do.

## Định nghĩa hoàn thành
- Plan có: yêu cầu đánh số, thiết kế class + vị trí file/assembly, test cases theo lớp với truy vết, testability,
  bước thực hiện có lệnh kiểm chứng.
- Người dùng đã duyệt. Bước tiếp theo: `build <feature>`.
