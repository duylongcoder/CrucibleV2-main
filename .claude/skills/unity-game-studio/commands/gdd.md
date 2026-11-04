# Command: gdd

**Vai trò**: Game Designer · **Tham số**: `<tên hệ thống>` · `--quick` (thay đổi nhỏ: tuning/luật nhỏ) · `--review` (soát GDD hiện có) · `--from-code` (viết GDD ngược từ code đã có)
**Ghi vào**: `Docs/GDD.md` §6.x hoặc `Docs/design/<system>.md` (template `templates/system-gdd.md`)
**Đọc**: GDD §1–4, GDD các hệ thống phụ thuộc, `ART.md` (nếu hệ thống có hiển thị), `references/design-frameworks.md`

## Chọn nơi ghi
- Hệ thống nhỏ/vừa → mục con trong `GDD.md` §6.
- Hệ thống lớn (>~150 dòng, nhiều công thức/trạng thái) → `Docs/design/<system-kebab>.md`; GDD §6 chỉ giữ
  tóm tắt 2 dòng + link.

## Mục bắt buộc theo rigor
| Rigor | Mục |
|---|---|
| lean | Detailed Rules, Acceptance Criteria |
| standard | Overview, Detailed Rules, Edge Cases, Dependencies, Acceptance Criteria (+ Formulas nếu có số, + Tuning Knobs nếu có giá trị chỉnh) |
| strict | Tất cả mục trong template |

## Bước (thiết kế mới)

1. **Ngữ cảnh**: đọc pillars, core loop, mục hệ thống trong Systems Index, GDD các hệ thống phụ thuộc
   (nhất quán tên gọi, số liệu). Hệ thống chưa có trong Systems Index → hỏi thêm vào không.
2. **Tạo khung** mục/ file với các heading và `[Chưa thiết kế]`, ghi ngay (để tiếp tục được nếu bị ngắt).
3. **Thiết kế từng mục** theo thứ tự: Overview → Player Fantasy → Detailed Rules → Formulas → Edge Cases →
   Dependencies → Tuning Knobs → Visual/Audio/Feel → UI → Acceptance Criteria. Mỗi mục:
   - Hỏi những gì cần biết (từng câu), đưa 2–3 phương án khi có lựa chọn thiết kế thật, khuyến nghị một.
   - Viết bản nháp → người dùng duyệt → ghi ngay → mục kế tiếp.
4. **Chất lượng từng mục**
   - *Detailed Rules*: Developer cài được mà không phải đoán; có bảng trạng thái → sự kiện → trạng thái nếu
     hệ thống có state.
   - *Formulas*: tên, biểu thức, bảng biến + khoảng giá trị, khoảng đầu ra, ví dụ tính bằng số. Đường cong phi
     tuyến → mô tả hình dạng (linear / quadratic / log / S-curve) và vì sao.
   - *Edge Cases*: 0, âm, tối đa, tràn, chia 0, danh sách rỗng, input dồn dập/đồng thời, object bị destroy giữa
     chừng, chuyển scene giữa chừng, chiến lược áp đảo + cách chặn.
   - *Tuning Knobs*: loại feel/curve/gate, mặc định, khoảng an toàn, lý do, nơi lưu (ScriptableObject cụ thể).
   - *Acceptance Criteria*: nhị phân, đo được; tách tiêu chí chức năng và tiêu chí trải nghiệm (cần playtest).
     Tiêu chí mơ hồ → đề xuất phiên bản đo được.
   - Random → ghi rõ cần seed/inject để test & tái lập.
5. **Soát chéo**: mâu thuẫn với GDD khác (số liệu, tên, luật)? phục vụ pillar nào? có tạo chiến lược áp đảo?
   Ghi vấn đề vào Open Questions.
6. **Cập nhật** Systems Index (trạng thái `In Review` hoặc `Approved` khi người dùng duyệt toàn bộ), PLAN.md
   backlog (`Designed`), §9 lịch sử thay đổi. CHECKPOINT.

## `--quick` (thay đổi nhỏ)
Phân loại: *Tuning* (đổi số) / *Tweak* (chỉnh luật) / *Addition nhỏ*. Viết spec ngắn: thay đổi gì, vì sao,
giá trị cũ → mới, hệ thống bị ảnh hưởng, acceptance criteria. Cập nhật đúng mục trong GDD + §9. Đổi số ảnh
hưởng công thức/hệ thống khác → liệt kê; nếu đụng ADR/code đã có → ghi vào báo cáo cho Developer.

## `--review`
Soát một GDD: đầy đủ mục theo rigor, nhất quán nội bộ, implementable (Developer có phải đoán không?),
acceptance criteria đo được, mâu thuẫn với GDD khác. Kết luận: `APPROVED` / `NEEDS REVISION` (liệt kê từng vấn
đề theo mục + đề xuất sửa cụ thể). Không tự sửa khi chưa được đồng ý.

## `--from-code`
Đọc code hệ thống (class, SO, giá trị serialized trong asset, test), viết GDD mô tả **hành vi hiện tại**,
đánh dấu `(suy ra từ code)`. Chỗ hành vi có vẻ là bug hoặc không rõ ý đồ → Open Questions, không tự "sửa thiết kế".

## Định nghĩa hoàn thành
- Đủ mục theo rigor, không còn `[Chưa thiết kế]`; TBD nằm trong Open Questions.
- Systems Index + PLAN.md đồng bộ.
- Bước tiếp theo: `plan <system>` (Developer) và/hoặc `asset`/`ui` (Designer) nếu có phần hiển thị.
