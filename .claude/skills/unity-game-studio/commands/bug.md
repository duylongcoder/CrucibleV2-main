# Command: bug

**Vai trò**: QA / Tester (báo + tái hiện) → Developer (chẩn đoán + sửa) · **Ghi vào**: `Docs/BUGS.md` (tạo từ template nếu chưa có), `Assets/**`, `Assets/Tests/**`
**Đọc**: `references/unity-testing.md`, `references/unity-automation.md`, GDD/spec liên quan
**Tham số**:
- `bug "<mô tả>"` hoặc `bug <tên test đang fail>` — luồng đầy đủ: báo → tái hiện → chẩn đoán → sửa → xác minh
- `bug report "<mô tả>"` — chỉ ghi bug report
- `bug triage` — xếp lại mức độ/ưu tiên các bug Open, tìm xu hướng
- `bug verify <BUG-ID>` — xác minh bug đã sửa, đóng

## Luồng đầy đủ

Ghi chú điều tra (ghi **ngay** khi xảy ra, vào mục *Điều tra* của bug trong BUGS.md): làm rõ báo cáo, nguyên nhân gốc
(và khi nào đổi ý), giả thuyết đã loại trừ, đánh đổi khi chọn cách sửa, sửa test đã commit.

### 1. Làm rõ báo cáo (chưa đọc code)
Từ mô tả + spec/GDD (chưa đọc code — đọc code lúc này sẽ kéo "mong đợi" về phía code đang làm):
- **Condition**: bối cảnh gây lỗi · **Expected**: hành vi đúng theo spec · **Actual**: hành vi quan sát được.
Thiếu một trong ba → hỏi. Spec mâu thuẫn với báo cáo → hỏi bên nào đúng; spec sai/thiếu → đưa vào danh sách sửa.
Loại: **test đang fail** (bỏ bước 2) hay **bug hành vi**. Tạo mục BUGS.md (ID kế tiếp `BUG-NNNN`, mức S1–S4, ưu tiên,
trạng thái Open). Có log/stack trace → đính kèm rút gọn.

### 2. Viết test tái hiện
Tìm test gần với kịch bản nhất (vị trí đặt + mẫu phong cách). Bug liên quan hiển thị/thao tác UI → test tái hiện
phải ở lớp integration/visual. Thiết kế 1 test tái hiện (điều kiện tối thiểu gây lỗi — dùng đoán lỗi, chuyển trạng
thái) + vài regression test cho hành vi lân cận (để bước 6). Test hiện có đang kiểm sai hành vi → sửa test đó thay vì thêm mới.

### 3. Xác nhận test tái hiện FAIL
Chạy test. Không fail → xóa test, tìm rộng hơn, quay lại bước 2. 3 lần không tái hiện được → quay lại bước 1, hỏi lại
người dùng (bước tái hiện, môi trường, build vs editor). Nhiều test fail không rõ cái nào → hỏi.
Không viết được test tự động (chỉ xảy ra trên thiết bị, phụ thuộc phần cứng) → ghi bước tái hiện thủ công chính xác +
nói rõ lý do; vẫn tiếp tục bước 5.

### 4. Xác nhận với người dùng
Trình: file + tên test tái hiện, thông điệp fail thật. Người dùng xác nhận đúng bug → (nếu `CLAUDE.md` §Git cho
`Commit test-first: có`) commit test
(`test(<scope>): reproduce BUG-NNNN`). Trạng thái → Reproduced.

### 5. Chẩn đoán nguyên nhân gốc
Không sửa khi chưa có nguyên nhân gốc. Lần theo luồng mà test kích hoạt; đọc kỹ thông điệp lỗi và stack trace;
`git log`/`git diff` thay đổi gần đây; so với code tương tự đang chạy đúng; thêm log tạm ở ranh giới component nếu
cần (xóa sau). Grep **mọi chỗ gọi** hàm sẽ sửa — sửa một lần ở chỗ dùng chung, không vá từng triệu chứng.
Nghi phạm Unity thường gặp: thứ tự `Awake/Start/OnEnable` giữa object · event đăng ký 2 lần (thiếu hủy) · coroutine
dừng khi object inactive · async chạy tiếp sau khi destroy · `?.` trên object đã destroy · SO giữ giá trị runtime giữa
các lần Play (Domain Reload tắt) · physics trong `Update` · `transform.position` trên Rigidbody · trigger thiếu
Rigidbody · Layer Collision Matrix · tunneling (Collision Detection) · `Time.timeScale = 0` · Script Execution Order ·
stripping/IL2CPP chỉ lỗi trên build · thiếu scene trong Build Profile · field serialized đổi tên mất dữ liệu.

### 6. Regression coverage
Hành vi lân cận có thể vỡ do sửa nhưng chưa có test → thêm regression test; chúng phải **pass** (đang kiểm hành vi đúng).

### 7. Sửa & xác minh
Áp dụng sửa đã xác định. Chạy: test tái hiện **pass**, regression **pass**, test liên quan khác **pass**. Commit
`fix(<scope>): <mô tả> (BUG-NNNN)`. Trạng thái → Fixed.

### 8. Dọn & refactor
Xóa log tạm; bỏ test trùng; làm gọn code đã đụng; chạy lại test. Regression checklist có phạm vi (hệ thống bị chạm +
hệ thống tiêu thụ nó) ghi vào bug.

### 9. Báo cáo
Điều tra: làm rõ báo cáo, nguyên nhân gốc, đã loại trừ, đánh đổi, sửa test (ghi `- None` nếu trống), câu hỏi mở.

## triage
Đọc BUGS.md các bug Open: kiểm lại mức độ theo định nghĩa (`roles/qa.md`), tách mức độ và ưu tiên, đánh dấu
trùng lặp, bug không tái hiện được quá lâu (cần thêm thông tin), xu hướng (hệ thống nào nhiều bug → đề xuất refactor
hoặc test bổ sung). Đề xuất thay đổi → người dùng duyệt → cập nhật.

## verify
Chạy test tái hiện + regression; kiểm thủ công theo bước tái hiện nếu có; build khác editor nếu bug chỉ có trên build.
Pass → Verified/Closed + người/ngày/build; fail → mở lại kèm bằng chứng.
