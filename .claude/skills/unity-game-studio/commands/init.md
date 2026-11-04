# Command: init

**Vai trò**: DevOps (chính) + Developer · **Tham số**: `[đường dẫn project]` (mặc định: thư mục hiện tại) · `--rescan` để chỉ cập nhật §Môi trường Unity trong CLAUDE.md
**Ghi vào**: `CLAUDE.md`, `Docs/GDD.md`, `Docs/ART.md`, `Docs/PLAN.md`, `Docs/TECH.md`, `Docs/TEST.md`, `Docs/LOG.md`

Mục tiêu: dự án có đủ bộ tài liệu cốt lõi, `CLAUDE.md` mô tả đúng môi trường Unity thật, để mọi command khác
chạy được.

## Bước

1. **Xác định gốc dự án.** Tìm thư mục chứa `Assets/`, `ProjectSettings/ProjectVersion.txt`. Không thấy:
   - Chưa có dự án Unity → hỏi: tạo dự án mới (người dùng tạo qua Unity Hub, hoặc Unity CLI
     `-createProject` nếu có) hay chỉ tạo tài liệu thiết kế trước. Nếu chỉ tài liệu: tạo ở thư mục hiện tại,
     để trống §Môi trường Unity.
   - Có nhiều dự án con → hỏi chọn.

2. **Quét dự án** (chỉ đọc):
   ```bash
   python3 <skill>/scripts/unity_project_scan.py <project-root> --markdown
   ```
   Lấy: phiên bản Unity, render pipeline, input handling, scripting backend, package chính, asmdef, test
   assembly, số test, scene trong build, Force Text / Visible Meta, `.gitignore`, git. Script lỗi → đọc tay
   `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `ProjectSettings/ProjectSettings.asset`,
   `ProjectSettings/EditorSettings.asset`, tìm `*.asmdef`.

3. **Đọc tài liệu đã có.** `CLAUDE.md`, `README*`, `Docs/`, `docs/`, file `.md` thiết kế. Có `CLAUDE.md` rồi →
   **không ghi đè**; đề xuất phần bổ sung/cập nhật dạng diff và hỏi trước. File trong `Docs/` đã tồn tại → giữ
   nguyên, chỉ tạo file còn thiếu.

4. **Hỏi những gì quét không ra** (gộp một lượt, tối đa 4 câu, có lựa chọn):
   - Tên game + mô tả một câu + thể loại (nếu chưa có trong tài liệu).
   - Nền tảng mục tiêu + input chính.
   - Rigor: `lean` / `standard` (khuyến nghị) / `strict`.
   - Kiến trúc dự định (nếu dự án trống): SO event channel / VContainer / Zenject / không DI.
   - Chính sách git (gộp vào câu rigor nếu cần bớt câu hỏi): Claude tự commit sau mỗi bước xanh, hay chỉ đề xuất
     message (mặc định)? Có commit test-first (test đang fail) không? → ghi `CLAUDE.md` §Git.
   Người dùng vắng mặt → chọn mặc định hợp lý, ghi rõ giả định ở `CLAUDE.md` §Ghi chú.

5. **Tạo tài liệu** từ `templates/`:
   - `CLAUDE.md` ở gốc dự án: điền từ kết quả quét + câu trả lời. Đường dẫn Unity: macOS
     `/Applications/Unity/Hub/Editor/<ver>/Unity.app/Contents/MacOS/Unity`, Windows
     `C:\Program Files\Unity\Hub\Editor\<ver>\Editor\Unity.exe` — kiểm tra tồn tại trước khi ghi.
     `{{SKILL_DIR}}` thay bằng đường dẫn thật của skill.
   - `Docs/GDD.md`, `ART.md`, `PLAN.md`, `TECH.md`, `TEST.md`, `LOG.md`: copy template, điền phần đã biết
     (tên, nền tảng, môi trường, asmdef vào TECH §2), để `{{...}}` ở phần chưa biết.
   - `LEVELS.md`, `ASSETS.md`, `BUGS.md`, `BUILD.md` **không tạo lúc này** — command liên quan sẽ tạo khi cần.
   - Dự án có code sẵn mà chưa có tài liệu: ghi vào `TECH.md` §3 danh sách hệ thống phát hiện được (namespace,
     MonoBehaviour chính, SO), đánh dấu `(suy ra từ code — cần xác nhận)`.

6. **Kiểm tra vệ sinh dự án** (chỉ báo cáo, không sửa): Force Text, Visible Meta Files, `.gitignore`,
   `check_unity_project.py` ERROR. Có vấn đề → đề xuất chạy `setup`.

7. **Ghi `LOG.md`**: CHECKPOINT (bước tiếp theo đề xuất) + một mục Session Log "init".

## Định nghĩa hoàn thành
- `CLAUDE.md` có §Môi trường Unity khớp `ProjectVersion.txt` và `manifest.json`.
- 6 file `Docs/` cốt lõi tồn tại; không ghi đè tài liệu người dùng đã có khi chưa được đồng ý.
- Báo cáo nêu: phát hiện gì, giả định gì, vấn đề vệ sinh, bước tiếp theo
  (thường là `concept` nếu dự án mới, `status` nếu dự án đang chạy).

## `--rescan`
Chạy lại bước 2, so với `CLAUDE.md` §Môi trường Unity, hiển thị khác biệt, cập nhật sau khi người dùng đồng ý.
Dùng sau khi nâng cấp Unity hoặc thêm package.
