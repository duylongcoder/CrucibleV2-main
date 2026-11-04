# Command: ci

**Vai trò**: DevOps · **Tham số**: `[github | gitlab | local]` (mặc định github) · `--build` (thêm job build player) · `--diagnose <log | link>` (chẩn đoán pipeline fail)
**Ghi vào**: `.github/workflows/*.yml` (hoặc `.gitlab-ci.yml`), `Assets/<_Project>/Scripts/Editor/Build/CIBuild.cs`, `Docs/BUILD.md` §3–4
**Đọc**: `references/unity-automation.md` §CLI, `references/unity-subsystems.md` §Build pipeline, `Docs/BUILD.md`

## Bước (GitHub Actions + GameCI — phổ biến nhất)
1. Hỏi: nền tảng build, có chạy PlayMode trên CI không (cần nhiều thời gian), license Unity (Personal/Pro), dùng Git LFS?
   runner GitHub-hosted hay self-hosted?
2. **Workflow test** (`.github/workflows/test.yml`), trigger push + pull_request:
   - `actions/checkout` (bật `lfs: true` nếu dùng LFS).
   - Cache `Library/` theo hash của `Assets/**`, `Packages/**`, `ProjectSettings/**` (tiết kiệm nhiều phút).
   - `game-ci/unity-test-runner` (phiên bản major ổn định hiện hành — kiểm tra trên GitHub Marketplace trước khi ghi),
     `testMode: editmode` rồi `playmode` (hoặc `all`), `customParameters: -testCategory "!IgnoreCI"` nếu dự án dùng
     category này, upload artifact kết quả.
   - Secret: `UNITY_LICENSE` (Personal: file .ulf) hoặc `UNITY_SERIAL` (Pro), `UNITY_EMAIL`, `UNITY_PASSWORD`.
     **Không** ghi giá trị secret vào repo; chỉ hướng dẫn người dùng thêm trong Settings → Secrets.
3. **`--build`**: script build editor (`CIBuild.Run`, theo `references/unity-subsystems.md` §Build pipeline): đọc
   `-out`, `-appVersion`, `-dev`; kiểm `BuildReport.summary.result`; `EditorApplication.Exit(0|1)`. Workflow build dùng
   `game-ci/unity-builder` hoặc gọi trực tiếp `-executeMethod`, trigger tag `v*` hoặc thủ công (`workflow_dispatch`),
   matrix theo `targetPlatform`, upload artifact. Addressables → build content trước player.
4. Pin phiên bản action (không dùng `@main`). Unity version lấy từ `ProjectVersion.txt` (GameCI tự đọc nếu để `auto`).
5. Kiểm YAML hợp lệ (`python3 -c "import yaml,sys; yaml.safe_load(open(sys.argv[1]))" file` nếu có PyYAML).
   Không thể chạy CI thật từ đây → nói rõ, hướng dẫn người dùng push nhánh thử (người dùng tự push).
6. Ghi `Docs/BUILD.md` §3–4.

## `local`
Script shell (`Tools/ci/run-tests.sh`) chạy EditMode + PlayMode batchmode, xuất XML vào `Logs/`, exit code ≠ 0 khi
có fail; dùng làm pre-push thủ công. Editor phải đóng.

## `--diagnose`
Phân loại trước khi sửa: (1) runner thiếu năng lực — license, module nền tảng, toolchain IL2CPP/Android SDK, dung
lượng; (2) lỗi thật — compile/test fail (tái hiện cục bộ); (3) flaky (chỉ retry khi đã chứng minh flaky). Đề xuất sửa
giữ bước trong pipeline + nêu đánh đổi. Không xóa bước test để pipeline xanh.

## Định nghĩa hoàn thành
- Workflow hợp lệ, không chứa secret, action đã pin phiên bản; build script trả exit code đúng.
- BUILD.md mô tả trigger, bước, secret cần.
