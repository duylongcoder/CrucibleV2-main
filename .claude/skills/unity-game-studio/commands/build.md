# Command: build

**Vai trò**: Developer · **Tham số**: `<feature>` (phải có `Docs/plans/<feature>.md` đã duyệt) · `--quick "<mô tả>"` (thay đổi nhỏ không cần plan)
**Ghi vào**: `Assets/**`, `Assets/Tests/**`, `Docs/plans/<feature>.md` §6, `Docs/PLAN.md`, `Docs/TECH.md`, `Docs/evidence/`
**Đọc**: plan, GDD hệ thống, `references/unity-coding-rules.md`, `references/unity-testing.md`, `references/unity-automation.md`

Xây dựng feature trong Unity theo plan, test-first. (Đây là "build feature", không phải build player — build
player là `release`.)

## Trước khi bắt đầu
- Chưa có plan → feature nhỏ (1–2 file, không đổi public API, không chạm scene/prefab) có thể dùng `--quick`; còn
  lại đề nghị `plan <feature>` trước.
- Đọc plan + code liên quan. Kiểm tra Editor có đang Play Mode không (Play Mode có thể bỏ qua recompile) → dừng
  Play Mode trước khi sửa code.
- Ghi chú triển khai: mỗi khi (a) phải tự quyết vì spec mơ hồ, (b) cố ý lệch plan, (c) chọn giữa các phương án,
  (d) sửa test đã commit — **ghi ngay** một dòng vào `Docs/plans/<feature>.md` §6, không đợi cuối.

## Bước

Commit ở các bước dưới đây theo chính sách `CLAUDE.md` §Git (mặc định: chỉ đề xuất message, không tự commit).

### 1. Skeleton (compile được)
Tạo type + chữ ký method đúng vị trí file/assembly trong plan. Method mới: thân rỗng / trả giá trị mặc định
(`0`, `false`, `null`). Method sửa: chỉ đổi chữ ký. Không tạo `.meta`. Kiểm tra compile
(`references/unity-automation.md` §Compile). Commit: `chore(<scope>): add <feature> skeleton`.

### 2. Test first (phải FAIL)
- Tạo test assembly nếu chưa có: `python3 <skill>/scripts/create_test_asmdef.py --mode editmode|playmode ...`.
- Viết test theo bảng test cases trong plan (`references/unity-testing.md` §Viết test).
- Chạy test vừa viết → **xác nhận fail đúng lý do** (assertion fail, không phải compile error hay exception
  setup). Test pass bất ngờ → **dừng**, báo người dùng (test không kiểm được gì, hoặc tính năng đã có).
- `Commit test-first: có` → commit `test(<scope>): add tests for <feature>`; ngược lại giữ chưa commit, gộp vào commit bước 3.
  Từ đây, sửa test phải ghi vào §6 *Test changes*.

### 3. Implement
Viết code tới khi test xanh. Theo `references/unity-coding-rules.md`. Giá trị tuning vào SO config. Cần scene/prefab
→ `commands/scene.md`. Chạy test liên quan → xanh; compile không error/warning mới. Commit: `feat(<scope>): ...`.

### 4. Refactor
- Test: bỏ test trùng thật sự (cùng điều kiện **và** cùng assertion); gộp test khác tham số nhưng cùng phân vùng
  thành test tham số hóa; không gộp nhiều điều kiện vào một test multi-assert. Khi nghi ngờ → giữ nguyên.
- Code: bỏ trùng lặp, đơn giản hóa, đặt tên rõ, xóa code chết **trong phạm vi feature**. Không đổi hành vi.
- Chạy lại test → xanh. Commit: `refactor(<scope>): ...`.

### 5. Bằng chứng & tài liệu
- Thay đổi người chơi nhìn thấy (UI/visual/feel) → chạy game, chụp màn hình (`references/unity-automation.md`
  §Chụp màn hình) lưu `Docs/evidence/<feature>/`, xem ảnh và mô tả đã thấy gì. Không chụp được → liệt kê bước
  kiểm tra thủ công cụ thể cho người dùng.
- Cập nhật: plan (trạng thái, §6 Implementation Notes đầy đủ 5 mục, `- None` nếu trống), PLAN.md backlog
  (`In Test` hoặc `Done` theo Definition of Done), TECH.md §3 nếu API khác thiết kế, GDD Systems Index → `Implemented`.
- CHECKPOINT + Session Log.

## Luật dừng
- Cùng test fail 2 lần liên tiếp sau khi sửa → dừng, báo: đang fail gì & nguyên nhân khả dĩ, đã sửa gì bao nhiêu
  lần, phương án tiếp theo. Hỏi người dùng.
- Phát hiện spec sai/thiếu → dừng, hỏi; không tự sáng tác luật chơi.
- Cần package mới, đổi public API ngoài plan, sửa hệ thống khác → hỏi trước.

## `--quick "<mô tả>"`
Vẫn: đọc code → test (hoặc cập nhật test) → sửa → chạy test → báo cáo. Bỏ skeleton/plan file. Ghi Session Log.

## Định nghĩa hoàn thành
- Đủ 5 bước; commit theo chính sách `CLAUDE.md` §Git (không tự commit → liệt kê thay đổi theo nhóm kèm commit message đề xuất).
- Test liên quan xanh với output thật; compile sạch; bằng chứng hiển thị (nếu có phần hiển thị).
- Báo cáo theo mẫu trong SKILL.md + Implementation Notes.
