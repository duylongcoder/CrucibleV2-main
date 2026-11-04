# Command: playtest

**Vai trò**: QA (tổ chức) + Game Designer (phân tích) · **Tham số**: `plan "<câu hỏi>"` · `report` (nhập ghi chú thô → báo cáo có cấu trúc) · `self <màn | feature>` (tự chơi kiểm tra nhanh qua backend Editor)
**Ghi vào**: `Docs/TEST.md` §5, `Docs/BUGS.md`, `Docs/PLAN.md` backlog · **Đọc**: GDD (pillars, cảm xúc mục tiêu), LEVELS.md

## plan
1. Một buổi playtest trả lời **1–3 câu hỏi** cụ thể (vd "người chơi có hiểu combo mà không cần tutorial?").
2. Kịch bản: build/scene nào, người chơi (số lượng, kinh nghiệm), nhiệm vụ đưa cho họ (không giải thích cách chơi
   nếu đang kiểm onboarding), thời lượng.
3. Quan sát gì: chỗ lạc, kẹt, chán, chết oan, hiểu sai; thời gian hoàn thành; lời nói nguyên văn; khoảnh khắc thích.
4. Câu hỏi sau buổi (ngắn, không dẫn dắt). Nếu có telemetry: sự kiện cần ghi.
5. Ghi TEST.md §5.

## report
1. Nhận ghi chú thô (người dùng dán/viết). Tách: ấn tượng 5 phút đầu, luồng chơi, điểm đau, điểm khó hiểu, khoảnh
   khắc thích, bug, phản hồi theo feature, số liệu.
2. Phân biệt **quan sát** (người chơi làm gì) với **ý kiến** (người chơi nói gì) — ưu tiên quan sát.
3. Đối chiếu pillar và cảm xúc mục tiêu: trải nghiệm có khớp không?
4. Chuyển thành việc: bug → `BUGS.md` (qua luồng `bug report`); vấn đề thiết kế → Open Questions trong GDD hoặc
   backlog PLAN.md; cân bằng → đề xuất `balance`. Top 3 ưu tiên.
5. Ghi bảng báo cáo TEST.md §5.

## self (tự kiểm nhanh, không thay người chơi thật)
Nếu backend cho phép vào Play Mode + chụp màn hình + đọc Console: vào scene, để chạy N giây / thao tác qua script test
(dùng integration test hoặc editor script), chụp ảnh các thời điểm chính, đọc Console. Báo cáo: lỗi Console, ảnh và
nhận xét khả năng đọc/bố cục. Nêu rõ: tự kiểm **không đánh giá được** cảm giác điều khiển, độ vui, độ khó thật.

## Định nghĩa hoàn thành
- Báo cáo có top 3 việc và mỗi phát hiện đã được chuyển vào đúng tài liệu (bug / GDD / backlog).
