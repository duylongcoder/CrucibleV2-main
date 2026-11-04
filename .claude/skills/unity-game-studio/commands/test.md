# Command: test

**Vai trò**: QA / Tester (+ Developer) · **Ghi vào**: `Docs/TEST.md`, `Assets/Tests/**`, `Docs/evidence/`
**Đọc**: `references/unity-testing.md`, `references/unity-automation.md`, GDD (Acceptance Criteria, Formulas, Edge Cases), plan feature
**Tham số**:
- `test plan <feature>` — thiết kế test plan cho feature (ghi TEST.md §3)
- `test write <feature | class>` — viết test còn thiếu cho code đã có
- `test run [EditMode|PlayMode|all] [filter]` — chạy test và báo cáo
- `test refine <file | thư mục>` — soát & chỉnh test hiện có theo chuẩn
- `test smoke` — chạy smoke list (TEST.md §4)
- `test setup` — tạo test assembly / cấu trúc thư mục (gọi `create_test_asmdef.py`)
- `test flaky <test>` — điều tra test chập chờn

## plan
1. Đọc GDD + plan feature. Phân loại thay đổi (Logic / Integration / UI / Visual / Data) → bằng chứng bắt buộc
   (`roles/qa.md`).
2. Thiết kế test cases theo `references/unity-testing.md` §Thiết kế: chọn lớp, kỹ thuật, một method một phân vùng,
   cột kiểm tra chỉ ghi kết quả quan sát được. Tiêu chí mơ hồ → đề xuất phiên bản đo được, hỏi chốt.
3. Truy vết mỗi Acceptance Criterion → test cùng lớp. Thiếu → thêm hoặc ghi lý do miễn.
4. Kiểm tra thủ công (chỉ cho thứ không tự động được): bước, mong đợi, điều kiện pass nhị phân.
5. Ghi TEST.md §3; plan feature có §3 trống → đồng bộ sang.

## write
1. Đọc code + spec. Thiết kế test theo §plan (từ spec, **không** từ cách code đang làm — đọc spec trước code).
2. Viết theo `references/unity-testing.md` §Viết test. Thiếu seam để test (static, `new` bên trong, `Time.time`)
   → đề xuất seam tối thiểu (`#if UNITY_INCLUDE_TESTS` nếu chỉ phục vụ test) và hỏi trước khi sửa production code.
3. Chạy. Test cho code đã có thường **pass** — để chắc test có tác dụng, tạm làm sai SUT (đảo điều kiện / đổi
   hằng số) xem test fail, rồi hoàn lại (ghi vào báo cáo đã kiểm như vậy). Test fail vì code sai thật → đó là bug →
   chuyển `bug`, không sửa test cho pass.

## run
1. Có thay đổi code → kiểm compile trước. Xác định assembly/mode: `scripts/resolve_test_target.sh <file test>`.
2. Chạy theo backend (`references/unity-automation.md` §Chạy test). Không chạy lại khi đang chạy; timeout → thu hẹp filter.
3. Đọc kết quả (XML NUnit / output công cụ): tổng, pass, fail, skip, inconclusive, thời gian; mỗi fail: tên,
   message, dòng stack trace trong `Assets/`.
4. Test `[Category("VisualVerification")]` chạy để xác nhận thay đổi trong phiên này → đọc ảnh chụp, đánh giá
   từng khía cạnh trong `[Description]`, báo đã thấy gì. Chạy hồi quy trên code không đổi → bỏ qua phân tích ảnh.
5. Ghi TEST.md §6 (một dòng). Fail lặp lại 2 lần liên tiếp sau khi sửa → dừng, hỏi người dùng.

## refine
Soát test theo chuẩn (chỉ những gì kiểm được từ code test): tên, lớp đúng, một phân vùng/method, không control flow,
không chờ thời gian cố định, constraint model, dọn GameObject, test double đúng vai, test trùng thật sự (cùng điều
kiện **và** cùng assertion), test `internal` mà đã có test public cùng kết quả. Liệt kê phát hiện → người dùng đồng
ý → sửa → chạy xanh. Không bao giờ đổi coverage lấy gọn gàng; nghi ngờ thì giữ.

## smoke
Chạy test tự động thuộc smoke (`[Category("Smoke")]` nếu có) + liệt kê kịch bản thủ công trong TEST.md §4 cho người
dùng tích. Kết quả: `PASS` / `FAIL` (build chưa sẵn sàng cho QA). Ghi TEST.md §6.

## setup
1. Đọc asmdef production (`unity_project_scan.py` liệt kê). Đề xuất cấu trúc:
   `Assets/Tests/EditMode/<Asm>.Tests.EditMode.asmdef` (Editor only, tham chiếu assembly logic) và
   `Assets/Tests/PlayMode/<Asm>.Tests.PlayMode.asmdef` (mọi nền tảng). Theo quy ước sẵn có nếu dự án đã có thư mục test khác.
2. Tạo: `python3 <skill>/scripts/create_test_asmdef.py --name <Asm>.Tests.EditMode --mode editmode --ref <Asm> --out Assets/Tests/EditMode`.
3. Code trong `Assembly-CSharp` (không asmdef) → test asmdef không tham chiếu được; đề xuất tạo asmdef cho code game
   trước (thay đổi cấu trúc — hỏi, phối hợp `setup`).
4. Test `internal`: `[assembly: InternalsVisibleTo("<Asm>.Tests.EditMode")]` trong `AssemblyInfo.cs` cạnh asmdef production.
5. Viết một test mẫu chạy xanh để chứng minh cấu hình đúng.

## flaky
Chạy lại test nhiều lần (`[Repeat(n)]` tạm thời hoặc lặp lệnh) để đo tỉ lệ fail. Nghi phạm thường gặp: phụ thuộc thứ
tự test / state tĩnh rò rỉ (SO runtime value, `DontDestroyOnLoad`), `Random` không seed, `Time`/`WaitForSeconds` cố
định, chờ frame cố định thay vì chờ điều kiện, async không await, GameObject không dọn. Tìm nguyên nhân gốc, sửa, chạy
lại N lần xanh. Không tăng timeout, không thêm `[Retry]` làm cách sửa.

## Định nghĩa hoàn thành
- Kết quả dựa trên lần chạy thật trong phiên này; báo số liệu cụ thể.
- Test mới có bằng chứng là có thể fail (fail trước khi implement, hoặc đã thử làm sai SUT).
- UI/visual có ảnh trong `Docs/evidence/` hoặc bước kiểm thủ công cụ thể còn lại.
