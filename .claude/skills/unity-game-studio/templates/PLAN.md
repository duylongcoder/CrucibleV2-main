# Plan — {{TÊN GAME}}

> Cấu hình quy trình thiết kế & triển khai: pipeline áp dụng cho từng feature, roadmap, backlog, trạng thái.
> Plan chi tiết của từng feature nằm ở `Docs/plans/<feature>.md` (tạo bằng `/unity-game-studio plan <feature>`).
> **Cập nhật**: {{YYYY-MM-DD}}

## 1. Quy trình (pipeline)

### Toàn bộ gameplay (một lần, đầu dự án)
```
concept → systems → art (§1–4) → arch → [prototype hệ thống rủi ro] → gdd (MVP) → plan → build → test → playtest
```

### Mỗi feature
| Bước | Command | Vai trò | Đầu ra | Cổng qua bước |
|---|---|---|---|---|
| 1. Thiết kế | `gdd <system>` | Game Designer | GDD hệ thống | Acceptance Criteria đo được, không còn TBD chặn |
| 2. Hình ảnh / UI | `asset`, `ui` | Designer | Spec asset / UI | Chỉ khi feature có phần hiển thị mới |
| 3. Kỹ thuật | `arch` (nếu cần ADR) → `plan <feature>` | Developer | `Docs/plans/<feature>.md` | Testability PASS/WARN |
| 4. Xây dựng | `build <feature>` | Developer | Code + test | Test xanh, compile sạch |
| 5. Kiểm thử | `test <feature>` | QA | Test bổ sung, ảnh bằng chứng | Bằng chứng theo loại thay đổi |
| 6. Đóng | `log` | Tất cả | PLAN/LOG cập nhật | Trạng thái = Done |

Thay đổi nhỏ (tuning số, chỉnh luật nhỏ) có thể đi tắt: `balance` hoặc `gdd --quick` → `build`.

### Definition of Done (mặc định)
- [ ] Acceptance Criteria trong GDD đều có test hoặc bằng chứng.
- [ ] Compile sạch; toàn bộ test liên quan xanh.
- [ ] UI/visual có ảnh trong `Docs/evidence/`.
- [ ] Không còn TODO không có ghi chú `shortcut:`.
- [ ] Tài liệu cập nhật; commit theo quy ước.

## 2. Milestones

| Milestone | Mục tiêu (câu hỏi cần trả lời) | Hệ thống bao gồm | Hạn | Trạng thái |
|---|---|---|---|---|
| M0 Prototype | {{core loop có vui không?}} | {{}} | {{}} | {{}} |
| M1 Vertical Slice | {{một lát cắt hoàn chỉnh đạt chất lượng cuối}} | {{}} | {{}} | {{}} |
| M2 Alpha | {{đủ tính năng, nội dung tạm}} | {{}} | {{}} | {{}} |
| M3 Beta | {{đủ nội dung, sửa bug, cân bằng}} | {{}} | {{}} | {{}} |
| M4 Release | {{}} | {{}} | {{}} | {{}} |

## 3. Backlog & trạng thái

| ID | Feature / việc | Hệ thống | Loại | Ưu tiên | Ước lượng | Trạng thái | Plan | Ghi chú |
|---|---|---|---|---|---|---|---|---|
| F-001 | {{}} | {{}} | {{Logic/Integration/UI/Visual/Data}} | {{MVP}} | {{S/M/L}} | {{Backlog}} | {{Docs/plans/x.md}} | {{}} |

Trạng thái: Backlog → Designed → Planned → In Progress → In Test → Done (hoặc Blocked: lý do).
Ước lượng: S = một phiên, M = 2–3 phiên, L = 4+ phiên (L nên chia nhỏ).

## 4. Đang làm (sprint / tuần hiện tại)

**Mục tiêu**: {{}}

- [ ] {{F-001 ...}}

**Rủi ro**: {{}}

## 5. Đã cắt / hoãn

| Feature | Lý do | Ngày |
|---|---|---|
