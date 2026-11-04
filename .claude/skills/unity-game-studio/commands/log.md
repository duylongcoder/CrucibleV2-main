# Command: log

**Vai trò**: tất cả (DevOps cho changelog, QA cho log Unity) · **Ghi vào**: `Docs/LOG.md`, `Docs/BUGS.md`
**Tham số**:
- `log` hoặc `log session` — ghi lại phiên làm việc hiện tại + cập nhật CHECKPOINT
- `log decision "<quyết định>"` — thêm vào Decision Log
- `log changelog [từ-tag-hoặc-commit]` — sinh changelog từ git
- `log unity [editor|player|console]` — gom lỗi/cảnh báo từ Unity, tóm tắt, tạo bug cho lỗi thật
- `log archive` — chuyển Session Log cũ sang `Docs/log-archive/`

## session
1. Tổng hợp từ cuộc hội thoại + `git diff --stat` / `git log` từ checkpoint trước: command đã chạy, việc đã
   làm, file, test, vấn đề + cách xử lý, việc còn lại.
2. Thêm mục mới **lên đầu** Session Log (mới nhất ở trên).
3. **Ghi đè** khối CHECKPOINT: đang làm, bước tiếp theo (đủ cụ thể để làm tiếp khi mất hết ngữ cảnh),
   đang kẹt, file dở, câu hỏi mở, nhánh git hiện tại.
4. Session Log > ~200 dòng → đề nghị `log archive`.

## decision
Thêm một dòng vào Decision Log: ngày, quyết định, lý do + phương án bị loại, ai quyết, tài liệu liên quan.
Quyết định kỹ thuật khó đảo ngược (kiến trúc, package, save format, public API) → đề nghị viết ADR trong
`TECH.md` §7 bằng `arch`.

## changelog
1. `git log --pretty=format:'%h %s' <từ>..HEAD`. Không có tham số → từ tag gần nhất (`git describe --tags --abbrev=0`),
   không có tag → 50 commit gần nhất.
2. Nhóm theo Conventional Commit: `feat` → Added, `fix` → Fixed, `refactor`/`perf`/thay đổi hành vi → Changed,
   bỏ `chore`/`test`/`docs` khỏi bản cho người chơi.
3. Ghi vào `## Changelog` → `[Unreleased]` (hoặc phiên bản người dùng chỉ định).
4. Nếu được yêu cầu bản cho người chơi (patch notes): viết lại bằng ngôn ngữ người chơi, không thuật ngữ code,
   không lộ chi tiết khai thác bảo mật.

## unity
Nguồn log (chọn theo tham số, mặc định thử theo thứ tự):
- **Console qua MCP** (nếu có backend MCP — xem `references/unity-automation.md`): lấy log Error/Exception/Warning
  N phút gần nhất kèm stack trace.
- **Editor.log**: macOS `~/Library/Logs/Unity/Editor.log`, Windows `%LOCALAPPDATA%\Unity\Editor\Editor.log`,
  Linux `~/.config/unity3d/Editor.log`. Lần chạy trước: `Editor-prev.log`.
- **Player.log**: macOS `~/Library/Logs/<Company>/<Product>/Player.log`, Windows
  `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\Player.log`, Linux `~/.config/unity3d/<Company>/<Product>/Player.log`.
  `<Company>`/`<Product>` lấy từ `ProjectSettings/ProjectSettings.asset` (`companyName`, `productName`).

Xử lý:
1. Lọc dòng `error`, `Exception`, `NullReferenceException`, `MissingReferenceException`, `Assertion failed`,
   `warning CS`, `The referenced script ... is missing`. Bỏ nhiễu đã biết (licensing, package resolve thành công).
2. Gom nhóm theo thông điệp (bỏ phần số/địa chỉ thay đổi), đếm số lần, giữ 1 stack trace mẫu, xác định file:dòng
   trong `Assets/` nếu có.
3. Ghi bảng vào `## Unity Log Digest`.
4. Mỗi nhóm là lỗi thật trong code dự án và chưa có trong `BUGS.md` → đề xuất tạo bug (chạy luồng báo bug của
   `commands/bug.md`, chỉ phần report). Hỏi trước khi tạo hàng loạt.
5. Không dán toàn bộ log vào tài liệu; chỉ tóm tắt + mẫu.

## Hướng dẫn log trong code game (khi người dùng hỏi cách "lưu log")
- Không để `Debug.Log` rải rác trong vòng lặp nóng (mỗi lời gọi cấp phát string + stack trace).
- Nếu dự án chưa có wrapper log: đề xuất (chưa tự tạo) một class tĩnh nhỏ với
  `[System.Diagnostics.Conditional("ENABLE_LOG")]` hoặc `[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]`
  để lệnh log bị loại khỏi release build, kèm category/tag.
- Lỗi thật dùng `Debug.LogError` / `Debug.LogException(e, context)` — không nuốt exception.
- Cần log ra file trên thiết bị: `Application.logMessageReceivedThreaded` ghi vào `Application.persistentDataPath`,
  thread-safe, giới hạn kích thước, xoay vòng file. Đây là thay đổi kiến trúc nhỏ → hỏi trước.
