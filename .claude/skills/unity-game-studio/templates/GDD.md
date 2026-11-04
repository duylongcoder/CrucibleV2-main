# Game Design Document — {{TÊN GAME}}

> **Trạng thái**: {{Draft | In Review | Approved}} · **Cập nhật**: {{YYYY-MM-DD}} · **Rigor**: {{lean|standard|strict}}
> Chủ sở hữu: Game Designer. Hệ thống lớn tách sang `Docs/design/<system>.md` và chỉ để link ở §6.

## 1. Concept

- **Elevator pitch** (1–2 câu): {{}}
- **Thể loại / tham chiếu**: {{game tham chiếu — lấy gì từ mỗi game, tránh gì}}
- **Người chơi mục tiêu**: {{ai, chơi khi nào, phiên dài bao lâu}}
- **Nền tảng & input**: {{}}
- **Điểm khác biệt (USP)**: {{}}
- **Phạm vi**: {{độ dài game, số màn/nội dung, thời gian & nguồn lực}}

## 2. Pillars

| # | Pillar | Ý nghĩa | Phép thử thiết kế ("khi phân vân X, chọn Y") |
|---|---|---|---|
| P1 | {{}} | {{}} | {{}} |
| P2 | {{}} | {{}} | {{}} |
| P3 | {{}} | {{}} | {{}} |

**Không phải game này** (anti-pillars): {{những thứ cố ý không làm}}

**Cảm xúc mục tiêu (MDA Aesthetics)**: {{Challenge / Discovery / Expression / Fantasy / ...}}

## 3. Core Loop

| Vòng | Thời lượng | Người chơi làm gì | Phần thưởng / phản hồi |
|---|---|---|---|
| Micro | ~30 giây | {{}} | {{}} |
| Meso | 5–15 phút | {{}} | {{}} |
| Macro | Một phiên | {{}} | {{điểm dừng tự nhiên + lý do quay lại}} |

```
{{sơ đồ vòng lặp dạng text: Hành động → Kết quả → Phần thưởng → Nâng cấp → Hành động}}
```

**Tiến trình (progression)**: {{trong một lượt chơi / giữa các lượt (meta)}}
**Điều kiện thắng / thua**: {{}}
**Onboarding**: {{10 phút đầu dạy gì, theo thứ tự nào — giới thiệu từng cơ chế riêng trước khi kết hợp}}

## 4. Systems Index

| # | Hệ thống | Loại | Ưu tiên | Trạng thái | Tài liệu | Phụ thuộc |
|---|---|---|---|---|---|---|
| 1 | {{Player Controller}} | Core | MVP | Not Started | §6.1 | {{Input}} |

Loại: Core · Gameplay · Progression · Economy · Persistence · UI · Audio · Narrative · Meta.
Ưu tiên: MVP → Vertical Slice → Alpha → Full Vision.
Trạng thái: Not Started → In Design → In Review → Approved → Implemented.

**Thứ tự thiết kế** (theo phụ thuộc): Foundation → Core → Feature → Presentation → Polish.
**Hệ thống rủi ro cao** (cần prototype sớm): {{hệ thống — loại rủi ro — cách giảm}}
**Phụ thuộc vòng**: {{không có / A↔B và cách tách bằng interface}}

## 5. Thế giới, nhân vật, nội dung

{{Bối cảnh, nhân vật chính, kẻ thù/vật thể chính, danh sách nội dung (số lượng màn, item, kẻ thù...)}}

## 6. Thiết kế hệ thống

> Mỗi hệ thống một mục con. Rigor `lean`: chỉ cần **Detailed Rules** + **Acceptance Criteria**.
> `standard`: + Overview, Edge Cases, Dependencies (+ Formulas nếu có số). `strict`: đủ tất cả.

### 6.1 {{Tên hệ thống}}

> Trạng thái: {{}} · Pillar: {{P1}} · Ưu tiên: {{MVP}} · Lớp: {{Foundation|Core|Feature|Presentation}}

**Overview** — {{một đoạn, người mới đọc hiểu được}}

**Player Fantasy** — {{người chơi cảm thấy gì; aesthetic MDA phục vụ}}

**Detailed Rules** — {{luật chính xác, không mơ hồ; Developer cài đặt được chỉ từ mục này}}
- Trạng thái & chuyển trạng thái: {{bảng state → event → state}}
- Tương tác với hệ thống khác: {{}}

**Formulas**

| Tên | Công thức | Biến (khoảng giá trị) | Ví dụ tính |
|---|---|---|---|
| {{damage}} | {{base × (1 + bonus) − armor}} | {{base 1–100, ...}} | {{10 × 1.2 − 3 = 9}} |

**Edge Cases** — {{giá trị 0/âm/tối đa, chia 0, tràn, input dồn dập, object đã destroy, chiến lược áp đảo và cách xử lý}}

**Dependencies** — {{cung cấp gì cho ai, cần gì từ ai, hướng dữ liệu}}

**Tuning Knobs**

| Knob | Loại (feel/curve/gate) | Mặc định | Khoảng an toàn | Lý do | Nơi lưu (SO/config) |
|---|---|---|---|---|---|

**Visual / Audio / Game Feel** — {{phản hồi khi sự kiện xảy ra: âm thanh, hạt, rung, hit-stop, tween}}

**UI Requirements** — {{thông tin cần hiển thị, ở đâu, khi nào}}

**Acceptance Criteria** (đo được, nhị phân)
- [ ] {{AC-1: Khi ..., thì ...}}
- [ ] {{AC-2 (trải nghiệm): playtest xác nhận ...}}

**Open Questions** — {{TBD + câu hỏi cần chốt}}

## 7. UI / UX tổng thể

{{Luồng màn hình (menu → chơi → kết quả), HUD, tham chiếu `Docs/ART.md` §UI/HUD}}

## 8. Audio

{{Hướng âm nhạc, SFX chính, sự kiện cần âm thanh}}

## 9. Lịch sử thay đổi thiết kế

| Ngày | Thay đổi | Lý do | Hệ thống bị ảnh hưởng |
|---|---|---|---|
