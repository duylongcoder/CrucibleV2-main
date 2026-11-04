# Command: balance

**Vai trò**: Game Designer (+ Developer khi cần mô phỏng) · **Tham số**: `<hệ thống | "economy" | "progression">` · `--simulate`
**Ghi vào**: `Docs/GDD.md` (Formulas, Tuning Knobs, §9), dữ liệu ScriptableObject (khi được duyệt)
**Đọc**: GDD hệ thống, các `.asset` dữ liệu liên quan, `references/design-frameworks.md` §4–5

Tìm giá trị lệch, tiến trình gãy, chiến lược áp đảo, kinh tế mất cân bằng — dựa trên số liệu thật.

## Bước

1. **Nguồn số liệu**: công thức trong GDD + giá trị thật trong dữ liệu (ScriptableObject `.asset` dạng YAML text
   → đọc trực tiếp các field; JSON/CSV config). Ghi rõ đọc từ file nào. Không có số liệu → dừng, nói thiếu gì.
2. **Phân tích** theo loại:
   - *Sức mạnh / chi phí*: chuẩn hóa về một thước đo chung (vd DPS tương đương, giá trị/chi phí). Liệt kê
     outlier (> ±20–30% so với đường cong cùng tier).
   - *Tiến trình*: thời gian để đạt mốc (TTK/TTC, số lượt/phút cho mỗi level/unlock); có đoạn phẳng (chán) hoặc
     dựng đứng (nản)? Đường cong răng cưa hợp lý?
   - *Kinh tế*: liệt kê mọi nguồn (faucet) và nơi tiêu (sink); cân bằng theo độ dài phiên mục tiêu; tích lũy vô
     hạn? có pity cho phần thưởng ngẫu nhiên?
   - *Lựa chọn*: có lựa chọn luôn thắng (lựa chọn giả)? cấu trúc transitive / intransitive (oẳn tù tì) / bất đối
     xứng có giữ được không?
   - *Vòng lặp phản hồi*: vòng dương (kẻ thắng càng thắng) có cơ chế hãm chưa?
3. **`--simulate`**: viết script mô phỏng nhỏ (Python hoặc EditMode test dùng logic thuần C# của dự án nếu có)
   chạy công thức trên dải giá trị / N lượt với seed cố định; xuất bảng. Script đặt trong scratchpad, không
   commit trừ khi người dùng muốn giữ làm công cụ (`Tools/balance/`).
4. **Báo cáo**: Health (`HEALTHY` / `CONCERNS` / `CRITICAL`), bảng outlier, chiến lược áp đảo, phân tích tiến
   trình, đề xuất thay đổi (giá trị cũ → mới, lý do, tác động dự kiến).
5. **Áp dụng** (khi người dùng duyệt): sửa giá trị ở nơi lưu thật.
   - Giá trị trong `.asset`: ưu tiên sửa qua Editor (MCP / hướng dẫn người dùng). Sửa tay YAML chỉ khi Editor
     đóng, theo `references/unity-assets-yaml.md`.
   - Cập nhật GDD (Tuning Knobs, §9 lịch sử) cùng lúc.
   - Có test khóa giá trị/công thức → cập nhật test như thay đổi đặc tả (ghi rõ).
6. CHECKPOINT; đề xuất `playtest` để kiểm cảm giác sau khi đổi số.

## Định nghĩa hoàn thành
- Báo cáo dựa trên số liệu có nguồn; mọi thay đổi số có lý do và đã ghi vào GDD.
- Không tự đổi dữ liệu khi chưa được duyệt.
