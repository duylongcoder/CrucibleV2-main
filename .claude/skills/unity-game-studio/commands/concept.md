# Command: concept

**Vai trò**: Game Designer · **Tham số**: `[ý tưởng thô]` · `--pivot` (đổi hướng concept đã có)
**Ghi vào**: `Docs/GDD.md` §1 Concept, §2 Pillars, §3 Core Loop · **Đọc**: `references/design-frameworks.md` §1–3

Biến ý tưởng (hoặc "chưa có ý tưởng") thành concept đủ rõ để map hệ thống và bắt đầu prototype.

## Luật
- **Không code, không scaffold** trong command này. Đầu ra là thiết kế đã được duyệt.
- Hỏi **từng câu một**, ưu tiên câu có lựa chọn. Tối đa ~6 câu trước khi đưa ra phương án.
- Người dùng đã có tài liệu/ý tưởng chi tiết → đọc và chỉ hỏi phần thiếu.

## Bước

1. **Đọc ngữ cảnh**: `CLAUDE.md`, `GDD.md` hiện có. GDD §1–3 đã điền và không có `--pivot` → hỏi muốn chỉnh
   mục nào thay vì làm lại.
2. **Làm rõ** (từng câu): cảm xúc chủ đạo muốn người chơi có · hành động người chơi lặp lại nhiều nhất ·
   game tham chiếu (thích gì, không thích gì) · nền tảng & độ dài phiên · ràng buộc (thời gian, một mình hay
   team, kỹ năng art).
3. **Chưa có ý tưởng** → đề xuất 3 concept khác nhau thật sự (khác verb chính hoặc khác cảm xúc, không chỉ
   khác bối cảnh), mỗi concept: pitch 1 câu, verb chính, vòng lặp micro, điểm khác biệt, rủi ro lớn nhất,
   độ khó làm (S/M/L). Khuyến nghị một cái kèm lý do.
4. **Trình bày từng mục, duyệt xong mới ghi**:
   - §1 Concept: pitch, thể loại & tham chiếu (lấy gì / tránh gì), người chơi mục tiêu, USP, phạm vi.
   - §2 Pillars: 3 (tối đa 5) pillar, mỗi pillar có phép thử "khi phân vân X → chọn Y"; anti-pillars;
     aesthetic MDA mục tiêu.
   - §3 Core Loop: bảng micro/meso/macro, sơ đồ vòng lặp, tiến trình, thắng/thua, onboarding 10 phút đầu.
5. **Tự soát** sau khi ghi: còn chỗ mơ hồ? mâu thuẫn giữa pillar và loop? phạm vi có vừa nguồn lực? Sửa ngay.
6. **Rủi ro & prototype**: chỉ ra 1–2 giả định "nếu sai thì concept chết" (vd: core loop có vui không, điều
   khiển có đủ chính xác không) → đề xuất prototype nhỏ nhất để kiểm (scene greybox, 1 cơ chế, không art).
7. Cập nhật `PLAN.md` §2 milestone M0 Prototype với câu hỏi cần trả lời. Ghi CHECKPOINT.

## Định nghĩa hoàn thành
- GDD §1–3 đã được người dùng duyệt từng mục; không còn `{{...}}` ở §1–3 (trừ mục cố ý `TBD`).
- Có danh sách giả định rủi ro + prototype đề xuất.
- Bước tiếp theo: `systems` (liệt kê hệ thống) hoặc `plan prototype-<tên>`.
