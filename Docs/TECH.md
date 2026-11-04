# Technical Design — CrucibleV2 (tên tạm)

> Chủ sở hữu: Developer · **Cập nhật**: 2026-11-03
> Kiến trúc tổng, quy ước code, các quyết định kỹ thuật (ADR), ngân sách hiệu năng.

## 1. Tổng quan kiến trúc

- **Mẫu chính**: ScriptableObject data + event channel, không DI (giả định khi init — chốt bằng `arch`)
- **Luồng khởi động**: {{Bootstrap scene → load scene chơi additive / ...}}
- **Quản lý scene**: {{Single / Additive; ai load gì}}
- **Giao tiếp giữa hệ thống**: {{C# event / SO event channel / message bus}} — UI không giữ state game.
- **Dữ liệu**: {{SO config ở Assets/_Project/Data; JSON; Addressables}}
- **Lưu game**: {{format, vị trí, versioning — xem §4}}

```
{{sơ đồ lớp: Presentation (UI, VFX, Audio) → Gameplay (systems) → Core (logic thuần C#, data) → Platform (input, save, services)}}
```

## 2. Assembly definitions

| Assembly | Đường dẫn | Tham chiếu | Nền tảng | Ghi chú |
|---|---|---|---|---|
| Unity.2D.Welcome | Assets/Welcome | — | Any | **Hiện có** — của template, không phải code game |
| Crucible.Core (dự kiến) | Assets/_Project/Scripts/Core | — | Any | Logic thuần, `noEngineReferences` nếu được |
| Crucible.Runtime (dự kiến) | Assets/_Project/Scripts/Runtime | Core | Any | MonoBehaviour |
| Crucible.Editor (dự kiến) | Assets/_Project/Scripts/Editor | Runtime | Editor | |
| Crucible.Tests.EditMode (dự kiến) | Assets/Tests/EditMode | Core, Runtime | Editor | `UNITY_INCLUDE_TESTS` |
| Crucible.Tests.PlayMode (dự kiến) | Assets/Tests/PlayMode | Runtime | Any | `UNITY_INCLUDE_TESTS` |

## 3. Hệ thống kỹ thuật

| Hệ thống | Class chính | Assembly | Phụ thuộc | GDD | Trạng thái |
|---|---|---|---|---|---|
| — | Chưa có code game (dự án template trống) | — | — | — | — |

### 3.x {{Tên hệ thống}}
- **Trách nhiệm**: {{}}
- **Public API**: {{chữ ký method/event}}
- **Dữ liệu**: {{SO / struct / serialized}}
- **Vòng đời**: {{ai tạo, ai hủy, đăng ký/hủy event ở đâu}}
- **Điểm inject cho test**: {{interface, random seed, time provider}}

## 4. Lưu game (nếu có)

- Lưu **dữ liệu** (giá trị, seed, flag), không lưu object/scene.
- Có trường `version`; migrate từ bản cũ → hiện tại khi load; validate; giữ bản backup tốt gần nhất.
- Ghi atomic: ghi file tạm → flush → rename đè. Autosave ở ranh giới an toàn, slot riêng.
- Vị trí: `Application.persistentDataPath/{{}}`.

## 5. Quy ước code (bổ sung cho CLAUDE.md)

- Forbidden: {{`Resources.Load` cho nội dung mới? `SendMessage`? singleton giữ state?}}
- Allowed libraries: {{}}
- Define symbols: {{}}

## 6. Ngân sách hiệu năng

| Hạng mục | Ngân sách | Đo bằng | Hiện tại | Ngày đo |
|---|---|---|---|---|
| FPS mục tiêu | {{60}} → {{16.67}} ms/frame (mobile — chốt thiết bị tham chiếu) | Profiler (Development build trên máy thật) | | |
| Gameplay scripts | {{~5 ms}} | CPU module | | |
| Physics | {{~3 ms}} | | | |
| Rendering (CPU submit) | {{~4 ms}} | | | |
| UI / khác | {{~2 ms}} | | | |
| GC Alloc mỗi frame (ổn định) | 0 B | `GC Allocated In Frame` | | |
| Draw calls / SetPass | {{}} | Frame Debugger | | |
| Bộ nhớ | {{MB}} | Memory Profiler | | |
| Thời gian load scene | {{s}} | | | |

## 7. Architecture Decision Records

| ADR | Tiêu đề | Trạng thái | Ngày |
|---|---|---|---|
| ADR-001 | {{}} | {{Proposed / Accepted / Superseded by ADR-00x}} | {{}} |

### ADR-001: {{Tiêu đề}}
- **Bối cảnh**: {{vấn đề, ràng buộc, phiên bản Unity liên quan}}
- **Các phương án**: {{A — ưu/nhược; B — ưu/nhược; C ...}}
- **Quyết định**: {{}}
- **Hệ quả**: {{tích cực, tiêu cực, việc phải làm theo}}
- **Luật cho code** (Required / Forbidden): {{}}
- **Tương thích phiên bản Unity**: {{API mới/đã deprecated cần kiểm tra}}

## 8. Nợ kỹ thuật

| ID | Mô tả | Vị trí | Ảnh hưởng | Kế hoạch |
|---|---|---|---|---|
