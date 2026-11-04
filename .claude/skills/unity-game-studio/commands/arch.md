# Command: arch

**Vai trò**: Developer · **Tham số**: `[hệ thống | "overview"]` · `adr "<quyết định>"` · `--review` (đối chiếu code với TECH.md)
**Ghi vào**: `Docs/TECH.md` · **Đọc**: GDD (hệ thống liên quan), `CLAUDE.md`, code hiện có, `references/unity-coding-rules.md`, `references/unity-subsystems.md`

Thiết kế hệ thống kỹ thuật trước khi viết code: lớp, assembly, luồng dữ liệu, vòng đời, và ghi lại quyết định
khó đảo ngược.

## `overview` (kiến trúc tổng — chạy một lần đầu dự án hoặc khi tái cấu trúc)

1. **Hiện trạng**: dự án có code → khảo sát: asmdef, namespace, cách hệ thống nói chuyện với nhau (event C#, SO
   channel, singleton, DI container), cách load scene, cách lưu dữ liệu. Ghi đúng những gì đang có, không lý tưởng hóa.
2. **Ràng buộc**: phiên bản Unity (API nào có/không — `references/unity-coding-rules.md` §Theo phiên bản), nền
   tảng (IL2CPP → không `Reflection.Emit`, cẩn thận stripping), quy mô team, rigor.
3. **Đề xuất** (dự án mới hoặc khi được yêu cầu) 2–3 phương án cho các trục chính, mỗi trục nêu ưu/nhược ngắn,
   khuyến nghị một:
   - Giao tiếp: C# event trực tiếp / ScriptableObject event channel / DI container (VContainer, Zenject) — package
     mới thì phải hỏi.
   - Dữ liệu cấu hình: ScriptableObject / JSON / Addressables.
   - Luồng scene: Bootstrap + additive / single scene.
   - Async: `Awaitable` (Unity 2023.1+) / Coroutine / UniTask (nếu đã có).
   - UI: uGUI / UI Toolkit.
   - Assembly: Core (logic thuần, có thể `noEngineReferences`) / Runtime / Editor / Tests.
4. Duyệt → ghi TECH §1–2; quyết định quan trọng → ADR.

## `<hệ thống>` (thiết kế kỹ thuật một hệ thống)

1. Đọc GDD hệ thống (Rules, States, Formulas, Dependencies, Tuning Knobs). Mơ hồ → hỏi Game Designer/người dùng.
2. Thiết kế với **khả năng test** là ràng buộc:
   - Logic thuần C# (công thức, state machine) ở class riêng, không phụ thuộc UnityEngine → test EditMode.
   - MonoBehaviour mỏng (Humble Object): nhận input/event Unity, gọi logic, cập nhật hiển thị.
   - Phụ thuộc qua interface; random/time inject; không static state ẩn; không `new` dependency bên ngoài trong constructor.
   - Method có ≥3 tham số độc lập tạo nhiều tổ hợp → tách phần tính toán thuần ra đơn vị riêng.
   - Tuning knob → field trong ScriptableObject config (tên field, kiểu, `[Range]`, `[Tooltip]`).
3. Ghi TECH §3.x: trách nhiệm, public API (chữ ký), dữ liệu, vòng đời (ai tạo/hủy, đăng ký/hủy event ở đâu),
   điểm inject, vị trí file + assembly.
4. Đụng hệ thống khác / public API → liệt kê điểm bị ảnh hưởng.

## `adr "<quyết định>"`
Viết ADR trong TECH §7 theo template: bối cảnh, phương án (≥2, có ưu/nhược), quyết định, hệ quả, luật cho code
(Required/Forbidden), tương thích phiên bản Unity. Trạng thái `Proposed` → `Accepted` khi người dùng duyệt. ADR
cũ bị thay → `Superseded by ADR-xxx`, không xóa. Ghi thêm dòng vào Decision Log của `LOG.md`.

Viết ADR khi: chọn/bỏ package, kiến trúc giao tiếp, format save, cấu trúc assembly, render pipeline, chiến lược
asset loading, đổi public API giữa hệ thống. Rigor `lean`: Decision Log là đủ.

## `--review`
Đối chiếu code với TECH.md và ADR: vi phạm luật Required/Forbidden, phụ thuộc ngược lớp (Core tham chiếu
Presentation), hệ thống chưa được ghi, tài liệu lỗi thời. Báo cáo; không tự sửa code ngoài phạm vi.

## Định nghĩa hoàn thành
- TECH.md mô tả được hệ thống đủ để viết `plan`; quyết định khó đảo ngược có ADR (rigor ≥ standard).
- Bước tiếp theo: `plan <feature>`.
