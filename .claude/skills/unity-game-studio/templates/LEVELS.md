# Level Design — {{TÊN GAME}}

> Chủ sở hữu: Game Designer · **Cập nhật**: {{YYYY-MM-DD}}

## 1. Metrics (đo trước khi dựng hình)

| Metric | Giá trị | Nguồn (SO/const) |
|---|---|---|
| Tốc độ chạy | {{u/s}} | {{}} |
| Nhảy cao / xa tối đa | {{u}} | {{}} |
| Tầm tương tác / tấn công | {{u}} | {{}} |
| Khung nhìn camera | {{u × u}} | {{}} |
| Kích thước ô lưới / tile | {{}} | {{}} |

Mọi khoảng cách, hành lang, khe trong level đo theo các đơn vị này.

## 2. Danh sách màn

| ID | Tên | Loại | Thời lượng | Độ khó (1–10) | Cơ chế mới dạy | Trạng thái | Scene |
|---|---|---|---|---|---|---|---|
| L-01 | {{}} | {{Tutorial/Combat/Puzzle/Hub/Boss}} | {{phút}} | {{}} | {{}} | {{Concept/Layout/Graybox/Art/Polish/Final}} | {{Assets/.../L01.unity}} |

**Đường cong độ khó toàn game** (răng cưa — tăng dần, xả ở mốc, tăng lại ở mức nền cao hơn):
```
Độ khó
10 |                                   *
 7 |                    *    *       *
 4 |        *  *     *    *    *  *
 1 | *  *  *     *  *
   +--L01--L02--L03--L04--L05--L06--L07--L08--> 
```

## 3. Thiết kế từng màn

### L-01 — {{Tên}}

**Tóm tắt**: Loại {{}} · Thời lượng {{}} · Độ khó {{}} · Điều kiện vào: {{}} · Trạng thái: {{}}

**Mục đích**: {{dạy / kiểm tra cơ chế nào; câu chuyện; cảm xúc mục tiêu}}

**Layout**
```
[S] Bắt đầu  [E] Kết thúc  [C] Chiến đấu  [P] Puzzle  [R] Phần thưởng
[!] Câu chuyện  [?] Bí mật  [>] Một chiều  [=] Hai chiều  [@] NPC  [B] Boss

  [S]==[P]==[C]==[!]
          ?       \\
         [R]       [>]==[E]
```

**Đường chính (critical path)**:
1. {{}}

**Đường phụ**
| Đường | Điều kiện | Phần thưởng | Gợi ý phát hiện |
|---|---|---|---|

**Encounter / Puzzle**
| ID | Vị trí | Thành phần | Độ khó | Ghi chú |
|---|---|---|---|---|

**Nhịp độ (pacing)**
```
Cường độ
10 |                         *
 6 |        *  *          * * *
 2 | *  * *      *  *  *        *
   S---------------------------- E
```
Điểm nghỉ: {{}} · Cao trào: {{}}

**Dẫn hướng**: {{ánh sáng, đường nét, landmark, âm thanh dẫn mắt người chơi}}

**Âm thanh / hình ảnh**: {{nhạc theo vùng, ambience, palette — tham chiếu ART.md}}

**Ghi chú kỹ thuật**: {{số object ước tính, vùng nặng, hệ thống cần bật, dữ liệu SO của màn}}

**Kết quả playtest**: {{chỗ lạc/kẹt/chết oan → đã sửa gì}}
