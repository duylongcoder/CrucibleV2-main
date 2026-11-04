# Command: setup

**Vai trò**: DevOps · **Tham số**: `[check]` (mặc định: chỉ kiểm tra & báo cáo) · `apply` (sửa các mục người dùng chọn) · `asmdef` (đề xuất/tạo cấu trúc assembly) · `mcp` (cài/kết nối backend điều khiển Editor)
**Ghi vào**: `.gitignore`, `.gitattributes`, `ProjectSettings/EditorSettings.asset` (qua Editor), asmdef, `Docs/BUILD.md`, `CLAUDE.md` §Lệnh
**Đọc**: `roles/devops.md` §Checklist, `references/unity-automation.md`

## check (mặc định, chỉ đọc)
1. `python3 <skill>/scripts/unity_project_scan.py <root> --markdown` và
   `python3 <skill>/scripts/check_unity_project.py <root>`.
2. Kiểm từng mục, mỗi mục `OK` / `THIẾU` / `SAI` + cách sửa:
   - Git repo? `.gitignore` loại `[Ll]ibrary/ [Tt]emp/ [Ll]ogs/ [Oo]bj/ [Uu]ser[Ss]ettings/ [Bb]uild*/ *.csproj *.sln *.suo *.user .vs/ .idea/ .DS_Store`
     và `Assets/_Temp*` (editor script tạm của skill)? Có file trong `Library/` đã bị commit?
   - Asset Serialization = Force Text (`EditorSettings.asset`: `m_SerializationMode: 2`)? Visible Meta Files
     (`VersionControlSettings.asset` mode "Visible Meta Files" hoặc `m_ExternalVersionControlSupport`)?
   - Git LFS cho binary lớn (`*.psd *.fbx *.wav *.png`... tùy dự án)? Merge driver `UnityYAMLMerge`?
   - asmdef: code game trong `Assembly-CSharp` hay có asmdef? Editor code tách? Test asmdef EditMode/PlayMode có
     `UNITY_INCLUDE_TESTS`?
   - Unity Test Framework có / phiên bản (≥1.4.6 để dùng `[Test] async Task` thoải mái).
   - Lệnh test/build trong `CLAUDE.md` chạy được? Đường dẫn Unity đúng?
   - `check_unity_project.py`: GUID trùng, `.meta` bị bỏ lại, script mất tham chiếu.
3. Báo cáo bảng; đề xuất `setup apply` cho các mục cần sửa.

## apply
Người dùng chọn mục (AskUserQuestion, multi-select). Mỗi mục:
- `.gitignore` / `.gitattributes`: thêm dòng còn thiếu, không xóa dòng có sẵn. File đã lỡ commit trong `Library/` →
  đề xuất `git rm -r --cached Library` (hỏi trước — thao tác ảnh hưởng lịch sử làm việc của team).
- Force Text / Visible Meta: đổi qua Editor (Project Settings > Editor) — hướng dẫn hoặc chạy editor script
  `EditorSettings.serializationMode = SerializationMode.ForceText;` `VersionControlSettings.mode = "Visible Meta Files";`.
  Lưu ý: đổi sang Force Text sẽ re-serialize toàn bộ asset → diff lớn, nên làm ở commit riêng.
- Git LFS: chỉ đề xuất lệnh `git lfs install` + `.gitattributes`; không tự migrate lịch sử.
- Ghi kết quả vào `Docs/BUILD.md` §1 (tạo từ template nếu chưa có).

## asmdef
1. Liệt kê thư mục code + namespace; đề xuất: `<Game>.Core` (logic thuần, `noEngineReferences: true` nếu không dùng
   UnityEngine), `<Game>.Runtime`, `<Game>.Editor` (includePlatforms Editor), test asmdef.
2. Tác động: code đang ở `Assembly-CSharp` mà dùng type từ Editor folder / Plugins → kiểm tham chiếu; package bên thứ
   ba không có asmdef → không tham chiếu được từ asmdef (cần `Assembly-CSharp-firstpass` hoặc asmdef cho nó).
3. Thay đổi cấu trúc lớn → kế hoạch từng bước, chờ xác nhận; tạo asmdef (file JSON — được phép viết; Unity tự tạo
   `.meta`), compile sau mỗi bước, chạy test.
4. Cập nhật `CLAUDE.md` §Môi trường + `TECH.md` §2.

## mcp
Giải thích các backend (`references/unity-automation.md`) và giúp người dùng cài cái họ chọn — đây là thêm package
vào dự án, **phải được đồng ý**:
- Unity MCP (IvanMurzak, OpenUPM `com.ivanmurzak.unity.mcp`; CLI `unity-mcp-cli install-plugin <path>`,
  `setup-mcp claude-code <path>`, cần Node.js ^20.19 hoặc ≥22.12).
- Rider MCP Server (Rider 2026.2+) + plugin "MCP Server Extension for Unity".
- Hoặc chỉ dùng Unity CLI batchmode (không cài gì).
Ghi lựa chọn vào `CLAUDE.md` §Môi trường (Backend điều khiển Editor).

## Định nghĩa hoàn thành
- `check`: báo cáo đủ mục. `apply`: chỉ đổi mục được chọn, Unity mở lại không lỗi, test vẫn chạy.
