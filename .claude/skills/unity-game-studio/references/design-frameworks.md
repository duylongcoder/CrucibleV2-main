# Khung tư duy thiết kế game

Dùng để đặt câu hỏi và kiểm tra thiết kế — không phải để trích dẫn cho có. Mỗi điểm thiết kế phải gắn với một
pillar của dự án; khung lý thuyết chỉ là công cụ hỗ trợ.

## Mục lục
1. MDA & cảm xúc mục tiêu
2. Nhu cầu tâm lý & động lực
3. Vòng lặp & flow
4. Cân bằng
5. Kinh tế
6. Level design
7. Game feel (juice)
8. Ngẫu nhiên & roguelike
9. Phạm vi & prototype

## 1. MDA (Mechanics – Dynamics – Aesthetics)
Thiết kế **ngược** từ cảm xúc: Aesthetics (người chơi cảm thấy gì) → Dynamics (hành vi nảy sinh khi chơi) → Mechanics
(luật ta viết). Tám loại aesthetic: Sensation, Fantasy, Narrative, Challenge, Fellowship, Discovery, Expression, Submission.
Chọn 1–3 làm chủ đạo; mỗi cơ chế nói rõ phục vụ aesthetic nào.

## 2. Nhu cầu tâm lý & động lực
- **Self-Determination Theory**: Autonomy (lựa chọn có ý nghĩa, nhiều đường khả thi — tránh lựa chọn giả), Competence
  (kỹ năng tăng rõ, phản hồi cho biết *vì sao* thắng/thua), Relatedness (gắn kết với nhân vật/thế giới/người chơi khác).
- **Kiểu người chơi**: Achiever (mục tiêu đo được, mốc), Explorer (bí mật, chiều sâu hệ thống), Socializer, Competitor.
  Chi tiết hơn: Action, Social, Mastery, Achievement, Immersion, Creativity — chọn nhóm mục tiêu, đừng phục vụ tất cả.

## 3. Vòng lặp & flow
- **Vòng lặp lồng nhau**: micro ~30 giây (hành động tự thân thú vị) · meso 5–15 phút (mục tiêu → phần thưởng) · macro
  một phiên (tiến trình + điểm dừng tự nhiên + lý do quay lại). Cơ chế không nối vào vòng nào → ứng viên cắt.
- **Flow channel**: thử thách tăng cùng kỹ năng; độ khó dạng **răng cưa** (căng dần → xả ở mốc → bắt lại ở nền cao hơn);
  tránh phẳng (chán) và vách đứng (nản).
- **Onboarding**: 10 phút đầu dạy bằng chơi, không bằng chữ; mỗi cơ chế giới thiệu riêng ở chỗ an toàn trước khi kết hợp.
- **Phản hồi**: mọi hành động có hệ quả đọc được trong ~0.5 giây; phản hồi chiến lược trong vòng meso.
- **Thất bại**: chi phí thất bại tỉ lệ với tần suất — thất bại thường xuyên phải hồi phục nhanh.

## 4. Cân bằng
- **Đường cong sức mạnh**: tuyến tính (đều), bậc hai (tăng tốc), log (giảm dần lợi ích), S-curve (chậm–nhanh–bão hòa).
  Nói rõ chọn đường nào và vì sao.
- **Chuẩn hóa**: đưa về thước đo chung (DPS tương đương, giá trị/chi phí) để so; neo bằng TTK (time-to-kill) / TTC
  (time-to-complete) mục tiêu, các số khác suy ra từ đó.
- **Kiểu cân bằng**: transitive (đắt hơn = mạnh hơn, A>B>C), intransitive (oẳn tù tì), frustra (trông mất cân bằng nhưng có
  phản đòn ẩn), asymmetric (khả năng khác, sức sống ngang nhau).
- **Tuning knob**: feel (playtest), curve (mô hình toán), gate (độ dài phiên). Ghi khoảng an toàn + lý do mặc định.
- **Chiến lược áp đảo**: tự hỏi "người tối ưu hóa sẽ làm gì?"; phân biệt làm chủ lành mạnh vs khai thác thoái hóa;
  vòng phản hồi dương (giàu càng giàu) cần cơ chế hãm.

## 5. Kinh tế
- Liệt kê mọi **faucet** (nguồn vào) và **sink** (nơi tiêu); cân trên độ dài phiên mục tiêu; tránh tích lũy vô hạn và lạm phát.
- Phần thưởng xác suất có **pity** (đảm bảo trong N lần). Hiển thị tỉ lệ minh bạch.
- Monetization (nếu có): không pay-to-win ở bối cảnh cạnh tranh, không dark pattern.

## 6. Level design
- **Metrics trước, hình sau**: đo tầm nhảy, tốc độ, tầm với, khung camera; mọi khe/hành lang đo theo đơn vị đó.
- **Blockout/greybox** đúng tỉ lệ trước art; sửa ở giai đoạn rẻ.
- **Critical path** + golden path; nhánh phụ/bí mật thưởng sự tò mò, không phạt người đi thẳng.
- **Dạy → luyện → thử thách**; **nhịp độ** xen căng–nghỉ; dẫn hướng bằng ánh sáng, đường nét, landmark, âm thanh.
- **Gate có chủ đích**: khóa/chìa, kỹ năng, đường một chiều.
- **Playtest**: quan sát chỗ lạc, kẹt, chán, chết oan → sửa blockout; chỉ trang trí khi đã chơi tốt.
- Puzzle: một ý tưởng mới mỗi màn; lời giải mong đợi rõ; quyết định có chấp nhận lời giải tắt không; độ khó đến từ
  nhận ra ý tưởng, không từ thao tác tỉ mỉ.

## 7. Game feel (juice)
- Gắn phản hồi vào **sự kiện rời rạc** (on_hit, on_land, on_pickup, on_death); thiếu event → thêm event trước.
- Kênh: âm thanh, hạt, rung camera, hit-stop, flash, knockback, tween (scale/position), số bật lên. Bắt đầu 2–3 kênh, thêm
  tới khi đọc rõ thì dừng.
- Chuyển động có easing (overshoot cho "pop", ease-out cho "settle"); linear trông máy móc.
- Hit-stop & rung dành cho va chạm quan trọng; ngắn; theo bậc quan trọng (nhỏ/vừa/lớn). Rung camera là **hình ảnh**, không
  dời vật lý; hit-stop dùng timeScale/tạm dừng thời gian thực, không làm kẹt logic.
- Tùy chọn giảm rung/flash cho accessibility.

## 8. Ngẫu nhiên & roguelike
- Random có **seed** và tái lập được (seed chia sẻ được là tính năng); tách luồng RNG theo mục đích (map, loot, combat) để
  thay đổi một hệ thống không làm xáo trộn hệ thống khác.
- Nội dung sinh thủ tục phải thỏa **ràng buộc kiểm được** (luôn đi tới đích, độ khó trong khoảng, không phần thưởng bất khả).
- Biến thể run-to-run đến từ lựa chọn có ý nghĩa, không chỉ từ may rủi; người chơi nên thua vì quyết định, không vì RNG.
- Tiến trình meta (giữa các run) cân nhắc kỹ: thưởng thời gian chơi nhưng không thay thế kỹ năng.

## 9. Phạm vi & prototype
- Ưu tiên MVP → Vertical Slice → Alpha → Full Vision. MVP = ít nhất để trả lời "có vui không?".
- Mỗi prototype trả lời **một câu hỏi**; kết luận PROCEED / PIVOT / KILL; code prototype là đồ bỏ (để ngoài code chính
  hoặc đánh dấu rõ).
- Tài liệu nhiều hơn không làm game hay hơn — viết đủ để người làm không phải đoán, rồi đi chơi thử.
