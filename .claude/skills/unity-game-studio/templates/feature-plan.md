# Plan: {{Tên feature}} ({{F-xxx}})

> **Trạng thái**: {{Draft | Approved | In Progress | Done}} · **Cập nhật**: {{YYYY-MM-DD}}
> **GDD**: {{Docs/GDD.md §6.x}} · **ADR**: {{ADR-00x / không}} · **Loại**: {{Logic | Integration | UI | Visual | Data}}

## 1. Bối cảnh

{{Vì sao cần thay đổi này; vấn đề hoặc yêu cầu; liên quan pillar nào}}

**Yêu cầu** (trích từ GDD, đánh số để truy vết):
- R1: {{}}
- R2: {{}}

**Ngoài phạm vi**: {{}}  ·  **TBD (không làm)**: {{}}

## 2. Thiết kế triển khai

| Class / file | Trách nhiệm | Assembly | Mới / sửa |
|---|---|---|---|
| `{{Assets/_Project/Scripts/Runtime/Combat/DamageCalculator.cs}}` | {{logic thuần tính damage}} | {{Game.Core}} | Mới |

**Public / internal API**
```csharp
{{public int Calculate(int baseDamage, float multiplier, int armor);}}
```

**Interface phụ thuộc (để inject/test)**: {{IRandomSource, ITimeProvider, ...}}
**Dữ liệu**: {{SO mới: CombatConfig (fields + khoảng hợp lệ)}}
**Scene / prefab bị chạm**: {{— sửa bằng editor script, xem commands/scene.md}}
**Lý do thiết kế**: {{}}

## 3. Test cases

### Editor tests
{{(none)}}

### Unit tests
#### {{DamageCalculatorTest}}
| Test method | Kiểm tra |
|---|---|
| `{{Calculate_ArmorExceedsDamage_ReturnsZero}}` | {{kết quả bằng 0}} |
| `{{Calculate_PositiveMultiplier_ScalesDamage}} (multiplier: {1, 1.5, 2})` | {{damage = base × multiplier − armor}} |

### Integration tests
{{(none)}}

### Visual verification tests
{{(none)}}

### Manual tests
| Đối tượng | Góc nhìn kiểm tra / cách kiểm |
|---|---|

**Truy vết**: R1 → {{test}}; R2 → {{test}}
**Testability**: {{PASS | WARN (vấn đề + cách xử lý) | FAIL}}

## 4. Các bước thực hiện

> Commit theo chính sách `CLAUDE.md` §Git. Commit ở Bước 2 (test đang fail) chỉ khi `Commit test-first: có`.

- [ ] **Bước 1 — Skeleton**: tạo type + chữ ký method (thân rỗng, trả giá trị mặc định); compile được. Commit `chore(<scope>): skeleton ...`
- [ ] **Bước 2 — Test first**: viết test theo §3; chạy và **xác nhận fail** đúng lý do. Commit `test(<scope>): ...`
- [ ] **Bước 3 — Implement**: viết code tới khi test xanh; compile sạch. Commit `feat(<scope>): ...`
- [ ] **Bước 4 — Refactor**: bỏ trùng lặp test, gộp test tham số hóa cùng phân vùng, làm gọn code; test vẫn xanh. Commit `refactor(<scope>): ...`
- [ ] **Bước 5 — Bằng chứng & tài liệu**: ảnh UI/visual vào `Docs/evidence/`; cập nhật PLAN/TECH/LOG.

## 5. Rủi ro & đánh đổi

{{}}

## 6. Implementation Notes (điền khi làm)

- **Design decisions**: {{- None}}
- **Deviations** (lệch so với plan và lý do): {{- None}}
- **Tradeoffs**: {{- None}}
- **Test changes** (sửa test đã commit ở Bước 2 và lý do): {{- None}}
- **Open questions**: {{- None}}
