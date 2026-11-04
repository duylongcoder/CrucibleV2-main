# UI Spec — Menu chính · Menu trong run · Sổ công thức

> **Trạng thái**: In Review · **Cập nhật**: 2026-11-04 · Chủ sở hữu: Designer
> Framework uGUI + TMP. Quy tắc chung: `ART.md` §2 (mood Menu), §4, §7. Luật: `GDD.md` §6.3, §6.4, §7.

## G. Menu chính

- Nền: xưởng, lò cháy âm ỉ (ART §2 "Menu"). Logo trên; nút xếp dọc giữa-trái; phiên bản góc dưới-phải.

| Nút | Hành vi |
|---|---|
| **Tiếp tục** | mờ (disabled) khi không có run dở; có → dòng phụ "Mùa 3/8 · Bậc 1", là nút primary |
| **Run mới** | có run dở → modal xác nhận "Run đang dở (Mùa 3/8) sẽ bị xóa"; rồi → panel Run mới |
| **Sổ công thức** | mở Sổ (§I) |
| **Cài đặt** | modal: âm lượng nhạc / hiệu ứng (0–100), Giảm chuyển động, Toàn màn hình, timescale mặc định 1× / 2× / 3× |
| **Giới thiệu** | tên game, phiên bản, credit, license font (OFL), ghi chú dùng asset AI |

- **Panel Run mới**: Bậc khó (3 thẻ; chưa mở = khóa + "Thắng run bậc trước để mở") · Bộ khởi đầu (3 thẻ: tên, icon
  nguyên liệu khởi đầu, chuỗi các nhóm công thức cấp sẵn; chưa mở = "Cần <mốc> Danh vọng") · Danh vọng hiện tại ·
  nút **Bắt đầu**.
- Không có nút Thoát (WebGL). Nút chính mặc định: Tiếp tục nếu có run dở, ngược lại Run mới (Enter).

## H. Menu trong run (nút ☰)

Tiếp tục · Sổ công thức · Cài đặt · Về menu chính — kèm dòng "Tiến trình đã được tự lưu" (không cần xác nhận khi
thoát, GDD §6.4). Mở được ở mọi màn trong run; khi phát lại / cutscene: phát lại tạm dừng trong lúc mở menu.

## I. Sổ công thức

Modal toàn màn hình. Mở từ Bàn luyện (trừ lúc phát lại), Menu chính, Menu trong run.

```
┌ Sổ công thức                                               Đã mở 12/40     [✕] ┐
│ DANH SÁCH (640, cuộn)               │ CHI TIẾT                                   │
│ ▸ Nguyên liệu gốc                   │  [icon ×4]  Lam tinh thạch                 │
│    ▢ Quặng đồng   ▢ Than   ▢ ???    │             Khoáng · Tier 5                │
│ ▸ ⬚ Đồng → ⬚ Than   (nhóm đã biết)  │  Mở lúc: 04/11/2026 21:30                   │
│    ▢ Thép    ▢ ???                  │  Công thức: ⬚ Đồng   ⬚ Lam thạch  (đều nhau)│
│ ▸ ???                (nhóm chưa biết)│  Thuộc tính có thể nhận:                    │
│    ▢ ???    ▢ ???                   │   [◆ Sắc bén] [Phản đòn] [???]              │
└──────────────────────────────────────────────────────────────────────────────────┘
```

**Danh sách** (không có bộ lọc)
- "Nguyên liệu gốc" trên cùng, sau đó các **nhóm công thức**; mọi thứ **sắp theo tier** rồi theo ID cố định.
- **Vị trí cố định**: mục vừa mở **không** được đẩy lên đầu — giữ nguyên chỗ, chỉ đổi từ ??? sang có nội dung.
- Header nhóm: chuỗi nguyên liệu nếu nhóm đã biết (bộ khởi đầu cấp hoặc đã nung ra một biến thể), ngược lại "???".
- ⚠ Thứ tự biến thể trong nhóm **không được suy ra từ khoảng cách** (`Pattern`) — vị trí trong danh sách không được
  lộ nhịp. Luật dữ liệu: ID / thứ tự content độc lập với gap.
- Bộ đếm "Đã mở x/N" (x = số khoáng có `ItemUnlocked`).

**Chi tiết theo trạng thái**

| Trạng thái | Icon | Tên / tier | Mở lúc | Công thức | Thuộc tính có thể nhận |
|---|---|---|---|---|---|
| Chưa mở | khung trống | ??? | ??? | ??? | ??? |
| Mở do mua / nhận | ✔ | ✔ | ✔ | ??? | ✔ (theo khám phá) |
| Đã nung ra | ✔ | ✔ | ✔ | chuỗi icon **cách đều**, không khoảng cách | ✔ |
| Nguyên liệu gốc | ✔ | ✔ | ✔ | "Nguyên liệu gốc" | (không có mục) |

- **Thuộc tính có thể nhận**: danh sách thuộc tính phụ của khoáng (`ItemDef.PossibleStats`, content). "Đã mở" =
  người chơi **đã soi ra** thuộc tính đó trên một viên cùng loại. Thuộc tính đã mở hiện tên; còn ít nhất một chưa
  mở → **đúng một chip ???** (không lộ còn bao nhiêu); mở hết → không còn chip ???. Không liệt kê `Quality` (luôn
  có) và Ma thuật (công khai).
- "Mở lúc" = thời điểm `ItemUnlocked` lần đầu (giờ địa phương, `dd/MM/yyyy HH:mm`).

**Tương tác**: chạm mục → chi tiết; ↑ / ↓ chuyển mục; Esc / ✕ đóng. Danh sách dài → ScrollRect có ảo hóa (virtualize)
nếu > 100 mục.

## J. Tiêu chí kiểm

- G1: Không có run dở → Tiếp tục disabled; có → Run mới hỏi xác nhận trước khi xóa.
- G2: Bậc khó / bộ khởi đầu chưa mở không chọn được, có lý do.
- I1: Mở một khoáng mới → vị trí các mục trong danh sách không đổi (so snapshot thứ tự trước / sau).
- I2: Thứ tự biến thể trong mỗi nhóm không tương quan với khoảng cách công thức (test dữ liệu content).
- I3: Mục "Mở do mua" không có công thức; "Đã nung ra" có chuỗi icon và **không** có dữ liệu khoảng cách.
- I4: Chip ??? thuộc tính xuất hiện ⇔ còn thuộc tính trong `PossibleStats` chưa khám phá; tối đa 1 chip.
- I5: Text Sổ không chứa khoảng cách / số nhịp (quét chuỗi).
- Ảnh bằng chứng (`Docs/evidence/ui-meta/`): Menu có / không run dở · panel Run mới có mục khóa · Sổ: mục chưa mở,
  mở do mua, đã nung ra (thuộc tính có ??? / đã mở hết).

## Lịch sử duyệt

| Ngày | Mục | Thay đổi | Duyệt bởi |
|---|---|---|---|
| 2026-11-04 | G–J | Menu chính, Menu trong run, Sổ công thức theo khoáng (nhóm + ???, vị trí cố định, sắp theo tier, không bộ lọc, bộ đếm x/N, thuộc tính có thể nhận) | người dùng |
