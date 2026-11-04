# Test Strategy — {{TÊN GAME}}

> Chủ sở hữu: QA · **Cập nhật**: {{YYYY-MM-DD}}

## 1. Chiến lược

| Lớp | Đối tượng | Chế độ | Thư mục | Category |
|---|---|---|---|---|
| Editor test | Editor tools, kiểm tra asset (SO hợp lệ, prefab đủ component) | EditMode | `Assets/Tests/EditMode/Editor/` | — |
| Unit | Logic thuần C# (công thức, state machine, rules) | EditMode | `Assets/Tests/EditMode/` | — |
| Unit (Unity) | MonoBehaviour gọi trực tiếp, vòng đời | PlayMode | `Assets/Tests/PlayMode/` | — |
| Integration | Scene/prefab + component + asset liên kết, chuỗi thao tác UI | PlayMode | `Assets/Tests/PlayMode/Integration/` | `Integration` |
| Visual verification | Ảnh chụp: vị trí, typography, contrast, trạng thái hiển thị | PlayMode | như trên | `VisualVerification` |
| Performance | Thời gian, GC alloc | PlayMode | `Assets/Tests/PlayMode/Performance/` | `Performance` |
| Manual / Playtest | Feel, animation polish, cân bằng âm thanh | — | §5 | — |

- Tên test: `<Method>_<Condition>_<Expected>` (unit) · `<Condition>_<Expected>` (integration/visual).
- Test phụ thuộc timing trên máy chậm: `[Category("IgnoreCI")]` kèm lý do.
- Bằng chứng ảnh: `Docs/evidence/<feature>/<yyyy-mm-dd>_<screen>.png`.

## 2. Bằng chứng theo loại thay đổi

| Loại | Bằng chứng | Mức chặn |
|---|---|---|
| Logic | Unit test pass | Chặn |
| Integration | Integration test hoặc playtest ghi chép | Chặn |
| Visual / Feel | Ảnh + người dùng duyệt | Chặn |
| UI | Ảnh mỗi màn hình bị chạm | Chặn |
| Config / Data | Smoke check pass | Khuyến nghị |

## 3. Test plan theo feature

### {{F-001 — Tên feature}}
- **Loại**: {{}} · **GDD**: {{§6.x}} · **Plan**: {{Docs/plans/x.md}}

| Test | Lớp | Kiểm tra (kết quả quan sát được) | Trạng thái |
|---|---|---|---|
| `{{Method_Condition_Expected}}` | Unit | {{}} | {{Planned / Written / Passing}} |

**Kiểm tra thủ công**:
| Bước | Mong đợi | Pass khi |
|---|---|---|

## 4. Smoke list (10–15 kịch bản đường chính, chạy trước mỗi bản build cho QA)

| # | Kịch bản | Cách kiểm | Tự động? |
|---|---|---|---|
| 1 | Mở game → menu chính hiển thị, không lỗi Console | {{}} | {{}} |
| 2 | {{Vào màn 1 → chơi → thắng/thua → về menu}} | {{}} | {{}} |

## 5. Playtest

### Kịch bản playtest
- **Câu hỏi cần trả lời**: {{}}
- **Người chơi**: {{số lượng, kinh nghiệm}}
- **Quan sát**: chỗ lạc, kẹt, chán, chết oan, hiểu sai; thời gian hoàn thành; câu nói nguyên văn.

### Báo cáo playtest
| Ngày | Build | Người chơi | Ấn tượng 5 phút đầu | Điểm đau | Khoảnh khắc thích | Bug | Top 3 việc cần làm |
|---|---|---|---|---|---|---|---|

## 6. Lịch sử chạy test

| Ngày | Phạm vi | Kết quả (pass/fail/skip) | Ghi chú |
|---|---|---|---|
