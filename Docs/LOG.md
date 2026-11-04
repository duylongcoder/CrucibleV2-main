# Project Log — CrucibleV2 (tên tạm)

<!--
  Hai vùng khác nhau:
  1. CHECKPOINT: GHI ĐÈ mỗi lần cập nhật, giữ dưới ~15 dòng. Đọc đầu mỗi phiên để nối tiếp công việc.
  2. Các mục bên dưới: GHI THÊM (append), mới nhất ở trên cùng của từng mục.
  Khi Session Log dài quá ~200 dòng: chuyển phần cũ sang Docs/log-archive/YYYY-MM.md.
-->

<!-- CHECKPOINT -->
**Cập nhật**: 2026-11-03
**Nhánh git**: `main`
**Đang làm**: chỉnh concept — khoảng cách là dữ liệu ẩn tuyệt đối, thêm nhóm công thức; Sổ thí nghiệm không ghi khoảng cách
**Bước tiếp theo**: người dùng duyệt `Docs/GDD.md` + `Docs/design/{brewing,economy,run-flow}.md` → `/unity-game-studio arch`
**Đang kẹt**: không
**File đang dở**: none
**Câu hỏi mở**: khám phá công thức mù (rủi ro cao — M0, AC-7/AC-8 §6.3); Version Control mode vẫn sai (đã commit sai ở e12665c; nghi collab-proxy); nguồn lực TBD
<!-- /CHECKPOINT -->

## Session Log

### 2026-11-03 — concept (khoảng cách) + setup check
- **Command**: /unity-game-studio concept, setup
- **Đã làm**: thêm nhóm công thức (`RecipeFamily`); khoảng cách là dữ liệu ẩn tuyệt đối; Sổ thí nghiệm chỉ ghi chuỗi nguyên liệu + tập phụ gia + kết quả; Sổ công thức mỗi nhóm một mục; bộ khởi đầu cấp `KnownFamilies`; kiểm tra lại Version Control
- **File**: Docs/GDD.md (§1, §3, §6.3, §9), Docs/design/brewing.md, Docs/LOG.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: Version Control vẫn `Unity Version Control`, file không đổi từ 21:17, đã bị commit ở e12665c; Editor đang mở
- **Còn lại**: sửa Version Control; duyệt GDD

### 2026-11-03 — gdd --quick (chốt câu hỏi mở)
- **Command**: /unity-game-studio (trả lời Open Questions)
- **Đã làm**: Sổ thí nghiệm → meta vĩnh viễn (gộp lưới trùng, `LabLogCap` 500, không tự xóa mục thành công); MVP không nhập seed; không hiện số nhịp 1–9
- **File**: Docs/GDD.md (§6.1, §6.3, §6.4, §7, §9), Docs/design/brewing.md, Docs/design/run-flow.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: Sổ thí nghiệm vĩnh viễn lộ khoảng cách công thức đã khám phá → ghi là hệ quả có chủ ý (P1), thêm câu hỏi cân bằng dài hạn
- **Còn lại**: duyệt GDD → Approved → arch

### 2026-11-03 — gdd --review
- **Command**: /unity-game-studio gdd --review
- **Đã làm**: soát GDD + 3 design → NEEDS REVISION (2 nghiêm trọng, 4 trung bình, 3 nhẹ); áp dụng toàn bộ sửa theo lựa chọn người dùng
- **File**: Docs/GDD.md, Docs/design/brewing.md, Docs/design/economy.md, Docs/design/run-flow.md, Docs/PLAN.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: R1 gộp Chợ vào Bàn luyện làm tràn Túi → lưới tính vào sức chứa; R2 thiếu nguồn biết công thức → người dùng chọn chỉ bộ khởi đầu, ghi rủi ro cao
- **Còn lại**: duyệt lần cuối → Approved

### 2026-11-03 — concept + gdd
- **Command**: /unity-game-studio concept → gdd
- **Đã làm**: viết GDD §1–9 từ demo_GDD.md; tinh chỉnh game flow + core loop; tách 3 GDD hệ thống
- **File**: Docs/GDD.md, Docs/design/brewing.md, Docs/design/economy.md, Docs/design/run-flow.md, Docs/PLAN.md, CLAUDE.md, Docs/BUILD.md
- **Test**: không (thiết kế)
- **Vấn đề gặp & cách xử lý**: demo là CLAUDE.md của dự án đã tới M6 — chỉ lấy phần thiết kế; kiến trúc Alchemy.* để `arch` quyết
- **Còn lại**: duyệt GDD; Open Questions trong từng file

### 2026-11-03 — setup
- **Command**: /unity-game-studio setup (check + apply)
- **Đã làm**: check vệ sinh dự án; thêm `.DS_Store`, `.idea/`, `/[Aa]ssets/_Temp*` vào .gitignore; tạo Docs/BUILD.md
- **File**: .gitignore, Docs/BUILD.md
- **Test**: không có (chưa có test assembly)
- **Vấn đề gặp & cách xử lý**: Hub đổi Version Control mode → Unity Version Control; Editor đang mở nên người dùng tự đổi lại trong Project Settings. Scanner báo nhầm thiếu UserSettings trong .gitignore (thực tế đã có)
- **Còn lại**: xác nhận Visible Meta Files; asmdef/test asmdef, .gitattributes chưa làm (người dùng chưa chọn)

### 2026-11-03 — init
- **Command**: /unity-game-studio init
- **Đã làm**: quét dự án (Unity 6000.6.7f1, URP 2D, Input System, template trống); tạo CLAUDE.md + Docs/{GDD,ART,PLAN,TECH,TEST,LOG}.md
- **File**: CLAUDE.md, Docs/*.md
- **Test**: không có (chưa có code game, chưa có test assembly)
- **Vấn đề gặp & cách xử lý**: Version Control mode ≠ Visible Meta Files; .gitignore thiếu UserSettings → để `setup` xử lý
- **Còn lại**: concept, setup

## Decision Log

| Ngày | Quyết định | Lý do / phương án bị loại | Ai quyết | Tài liệu liên quan |
|---|---|---|---|---|
| 2026-11-03 | Nền tảng: Mobile, input touch | — | Người dùng | CLAUDE.md |
| 2026-11-03 | Rigor `standard`; Claude chỉ đề xuất commit message, không commit test-first | Mặc định khuyến nghị | Người dùng | CLAUDE.md §Git |
| 2026-11-03 | Kiến trúc tạm: SO event channel, không DI | Giả định mặc định, chờ `arch` | Claude (giả định) | TECH.md §1 |
| 2026-11-03 | Nền tảng đổi: WebGL trước, Mobile sau (thay cho Mobile ở init) | Theo demo GDD + vẫn giữ hướng mobile | Người dùng | GDD §1, CLAUDE.md |
| 2026-11-03 | Loại trang bị công bố ở Chiếu chỉ | Thua vì quyết định, không vì RNG (P2/P3) | Người dùng | GDD §9, run-flow.md |
| 2026-11-03 | Gộp phase: Bàn luyện (Chợ + Lò), Dâng (gồm Xưởng), Rèn & Trận | Nhịp liền mạch (P4) | Người dùng | GDD §3 |
| 2026-11-03 | Chạm để tua phát lại + Sổ thí nghiệm | P4, P1 | Người dùng | brewing.md, GDD §6.3 |
| 2026-11-03 | Sổ thí nghiệm lưu vĩnh viễn, **không ghi khoảng cách** | Khoảng cách chỉ để người chơi tự kiểm nghiệm | Người dùng | GDD §6.3 |
| 2026-11-03 | Nhóm công thức: cùng nguyên liệu + thứ tự, khoảng cách khác → thành phẩm khác; Sổ công thức theo nhóm, không lộ số biến thể | Làm khoảng cách thành trục khám phá chính | Người dùng | brewing.md, GDD §6.3 |
| 2026-11-03 | MVP không nhập seed | Đơn giản MVP; sau MVP cân nhắc chia sẻ seed | Người dùng | GDD §6.1 |
| 2026-11-03 | Không hiện số nhịp 1–9 trên lưới | Giữ cảm giác thủ công, không "toán học"; dạy qua phát lại | Người dùng | brewing.md |
| 2026-11-03 | Vật phẩm trên lưới tính vào sức chứa Túi | Chợ cùng màn lưới → tránh tràn Túi | Người dùng (review R1) | brewing.md, economy.md |
| 2026-11-03 | Nguồn biết công thức: chỉ bộ khởi đầu + tự nung; không gợi ý | Loại: Chiếu chỉ gợi ý, Mảnh công thức (giữ làm dự phòng nếu M0 thất bại) | Người dùng (review R2) | GDD §6.3, §4 |

## Changelog

### [Unreleased]
- **Added**: {{}}
- **Changed**: {{}}
- **Fixed**: {{}}

## Unity Log Digest

<!-- Tóm tắt lỗi/cảnh báo đáng chú ý gom từ Console / Editor.log / Player.log. Lỗi thật → tạo bug trong BUGS.md. -->

| Ngày | Nguồn | Loại | Thông điệp (rút gọn) | Số lần | Xử lý |
|---|---|---|---|---|---|
