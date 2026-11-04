# Command: release

**Vai trò**: DevOps (+ QA cổng chất lượng) · **Tham số**: `<version> [platform]` · `checklist` (chỉ tạo/kiểm checklist) · `build <platform>` (chỉ build) · `hotfix <BUG-ID>`
**Ghi vào**: `Docs/BUILD.md` §6–7, `Docs/LOG.md` §Changelog, `ProjectSettings` (version — qua Editor/script)
**Đọc**: `Docs/BUILD.md`, `Docs/BUGS.md`, `Docs/TEST.md` §4, `Docs/TECH.md` §6, `references/unity-subsystems.md` §Build pipeline

## Luồng phát hành `<version>`
1. **Cổng chất lượng** (chặn nếu fail, báo rõ):
   - Toàn bộ test EditMode + PlayMode xanh (chạy thật trong phiên này).
   - Không còn bug S1 Open; S2 Open → người dùng xác nhận chấp nhận.
   - Smoke list pass (`test smoke`).
   - `check_unity_project.py` không ERROR; Console không lỗi trên đường chính.
   - Ngân sách hiệu năng: số đo gần nhất trong TECH §6 (cảnh báo nếu cũ hoặc đo trong Editor).
2. **Checklist** (BUILD.md §6): đánh dấu từng mục bằng bằng chứng; mục không tự kiểm được → liệt kê cho người dùng.
   Nền tảng cụ thể → thêm yêu cầu (Steam: depot/branch, store page; itch: butler channel; mobile: bundle ID, keystore/
   signing, version code, quyền; Web: nén, kích thước tải).
3. **Version**: đặt `PlayerSettings.bundleVersion` (+ version code mobile) qua build script tham số `-appVersion`
   hoặc editor script; ghi BUILD.md §5.
4. **Build** (`build <platform>`): Editor đóng → batchmode `-buildTarget <target> -executeMethod <CIBuild.Run>` hoặc
   `-activeBuildProfile ... -build`. Release = không `Development`. Kiểm `BuildReport` (script trả exit code), kích
   thước, scene list đúng. Không có script build → đề xuất tạo (`ci --build`).
5. **Smoke trên build**: chạy file build (nếu chạy được trên máy này) với tham số chụp màn hình
   (`references/unity-automation.md` §Chụp màn hình), đọc `Player.log` tìm Exception. Không chạy được → checklist
   thủ công cho người dùng.
6. **Changelog**: `log changelog <tag trước>` → phiên bản `<version>`; patch notes cho người chơi nếu cần.
7. **Ghi** BUILD.md §7 lịch sử phát hành. Đề xuất lệnh tag (`git tag -a v<version> -m ...`) — **người dùng tự chạy
   và push**.

## `hotfix <BUG-ID>`
Nhánh `hotfix/<version>` từ tag phát hành bị lỗi (đề xuất lệnh, người dùng xác nhận). Sửa theo luồng `bug` nhưng phạm
vi tối thiểu; chạy test + smoke; tăng PATCH version; build; changelog; nhắc merge lại vào `main`.

## Định nghĩa hoàn thành
- Mọi cổng chất lượng có bằng chứng hoặc được người dùng chấp nhận rõ ràng.
- Build thành công theo `BuildReport`; version đúng; changelog + lịch sử phát hành cập nhật.
- Không tự tag/push.
