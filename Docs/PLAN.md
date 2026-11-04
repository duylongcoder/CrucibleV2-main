# Plan — CrucibleV2 (tên tạm)

> Cấu hình quy trình thiết kế & triển khai: pipeline áp dụng cho từng feature, roadmap, backlog, trạng thái.
> Plan chi tiết của từng feature nằm ở `Docs/plans/<feature>.md` (tạo bằng `/unity-game-studio plan <feature>`).
> **Cập nhật**: 2026-11-03

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
| M0 Prototype | Giải mã khoảng cách công thức có thú vị không, hay chỉ là đoán mò? Người chơi có tự khám phá công thức ngoài bộ khởi đầu không (GDD §6.3 AC-7)? Kinh tế 0 vàng có kẹt/áp đảo không? | Lò nung (greybox 1 màn Bàn luyện, 6–8 công thức) + mô phỏng kinh tế bằng agent | TBD | Not Started |
| M1 Vertical Slice | Một run 8 Mùa trọn vẹn trên WebGL, người ngoài chơi không cần giải thích | Toàn bộ hệ thống MVP (GDD §4 #1–14), content tạm | TBD | Not Started |
| M2 Alpha | Đủ content MVP (tier 1–10, 3 bậc khó, 3 bộ khởi đầu), lưu/tiếp tục | + Meta, Lưu | TBD | Not Started |
| M3 Beta | Cân bằng (GreedyAgent, playtest), audio, polish | + Audio | TBD | Not Started |
| M4 Release | Phát hành WebGL | — | TBD | Not Started |

## 3. Backlog & trạng thái

| ID | Feature / việc | Hệ thống | Loại | Ưu tiên | Ước lượng | Trạng thái | Plan | Ghi chú |
|---|---|---|---|---|---|---|---|---|
| F-001 | Seed & RNG streams | #1 | Logic | MVP | S | Designed | — | GDD §6.1 |
| F-002 | Lò nung: lưới, chuỗi nạp, khớp công thức, `Quality` | #2 | Logic | MVP | M | Designed | — | design/brewing.md |
| F-003 | Kích hoạt ma thuật | #3 | Logic | MVP | S | Designed | — | design/brewing.md |
| F-004 | BrewLog + phát lại, timescale, chạm để tua | #4 | Logic/UI | MVP | M | Designed | — | design/brewing.md |
| F-005 | Túi | #5 | Logic | MVP | S | Designed | — | design/economy.md |
| F-006 | Chợ & kinh tế | #6 | Logic | MVP | M | Designed | — | design/economy.md |
| F-007 | Xúc tác & Effect primitive | #7 | Logic/Data | MVP | M | Designed | — | design/economy.md |
| F-008 | Thuộc tính ẩn & Kính soi | #8 | Logic | MVP | S | Designed | — | GDD §6.2 |
| F-009 | Vòng Mùa (state machine) | #9 | Logic | MVP | M | Designed | — | design/run-flow.md |
| F-010 | Rèn, Trận, Uy tín | #10 | Logic | MVP | S | Designed | — | design/run-flow.md |
| F-011 | Chiến lợi phẩm & modifier Mùa | #11 | Logic/Data | MVP | M | Designed | — | design/run-flow.md |
| F-012 | Meta: Sổ công thức, Sổ thí nghiệm, Danh vọng, bậc khó, bộ khởi đầu | #12 | Logic/UI | MVP | M | Designed | — | GDD §6.3 |
| F-013 | Lưu & tiếp tục | #13 | Integration | MVP | M | Designed | — | GDD §6.4 |
| F-014 | UI/UX greybox các màn | #14 | UI | MVP | L | Backlog | — | GDD §7 — cần `ui` |

Trạng thái: Backlog → Designed → Planned → In Progress → In Test → Done (hoặc Blocked: lý do).
Ước lượng: S = một phiên, M = 2–3 phiên, L = 4+ phiên (L nên chia nhỏ).

## 4. Đang làm (sprint / tuần hiện tại)

**Mục tiêu**: duyệt GDD → `arch` → prototype M0 (Lò nung greybox)

- [ ] Duyệt GDD + 3 file design (đang In Review)
- [ ] `/unity-game-studio arch`
- [ ] `/unity-game-studio plan prototype-brewing`

**Rủi ro**: giải mã khoảng cách có thể thành đoán mò tốn tài nguyên (P1/P3) — kiểm ở M0.

## 5. Đã cắt / hoãn

| Feature | Lý do | Ngày |
|---|---|---|
