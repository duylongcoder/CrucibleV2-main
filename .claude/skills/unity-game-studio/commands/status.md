# Command: status

**Vai trò**: tất cả · **Tham số**: không · **Ghi vào**: không (chỉ đọc)

Trả lời: "Dự án đang ở đâu và nên làm gì tiếp?"

## Bước

1. Đọc `CLAUDE.md` (thiếu → đề nghị `init`, dừng), khối CHECKPOINT trong `Docs/LOG.md`, `Docs/PLAN.md` §3–4.
2. Lướt nhanh (chỉ tiêu đề + bảng trạng thái, không đọc hết):
   - `GDD.md`: §1–3 đã điền chưa? Systems Index bao nhiêu hệ thống, trạng thái?
   - `ART.md`: §1–4 đã điền chưa?
   - `TECH.md`: kiến trúc + ADR?
   - `BUGS.md` (nếu có): số bug Open theo mức S1–S4.
   - `git status` / `git log --oneline -10` nếu là repo git.
3. Xác định giai đoạn:

| Giai đoạn | Dấu hiệu | Bước kế thường gặp |
|---|---|---|
| Concept | GDD §1–3 trống | `concept` |
| Pre-production | Có concept, Systems Index trống/ít, ART §1–4 trống | `systems`, `art`, `arch` |
| Prototype | Có hệ thống MVP thiết kế, ít/không code | `plan` → `build` cho hệ thống rủi ro nhất |
| Production | Backlog có feature In Progress/Planned | tiếp tục CHECKPOINT; `test`; `bug` |
| Polish | Đủ tính năng, nhiều bug/feedback | `playtest`, `optimize`, `bug` |
| Release | Milestone cuối | `release` |

4. Báo cáo ngắn (không quá ~15 dòng):
   - Giai đoạn + bằng chứng.
   - Việc dở dang từ CHECKPOINT (nếu có) — ưu tiên nối tiếp.
   - Rủi ro/chặn: bug S1, TBD chặn, tài liệu lệch code, test fail lần chạy gần nhất.
   - 1–3 command đề xuất kèm tham số cụ thể, xếp theo ưu tiên.

Không tự chạy command đề xuất; để người dùng chọn.
