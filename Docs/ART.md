# Art Bible — CrucibleV2 (tên tạm)

> **Trạng thái**: Draft — §0–4 tạm chốt, §5–8 đã duyệt · **Cập nhật**: 2026-11-04 · Chủ sở hữu: Designer
> Đây là hợp đồng hình ảnh. Mọi asset mới được so với tài liệu này **ở kích thước thật trong game**.
> Rigor `standard` cần §1–4 và §8; `strict` cần đủ 10 mục.

## 0. Khung kỹ thuật

| Mục | Giá trị |
|---|---|
| 2D / 3D / 2.5D | 2D **pixel art** |
| Camera / góc nhìn | Orthographic, nhìn chính diện phẳng ("sân khấu bàn luyện"); camera không di chuyển |
| Độ phân giải tham chiếu | 1920×1080 (CanvasScaler) · **canvas pixel gốc 640×360**, scale nguyên ×3 (×2 720p, ×4 1440p, ×6 4K) |
| Pixels Per Unit (2D) | **32** — sprite nguyên liệu 32×32 = 1 unit; ô lưới lò 48×48 px gốc (= 144 px ở 1080p, ≥ 64 px chạm) |
| Render pipeline | URP 17.6.0 — 2D Renderer; Pixel Perfect Camera (ref 640×360, PPU 32) |
| Tỉ lệ khác 16:9 | Mở rộng vùng nền/môi trường (không kéo giãn, không scale lẻ); vùng chơi an toàn = 640×360 |
| Kích thước "nhân vật" trên màn hình | Không có nhân vật điều khiển; chủ thể là nguyên liệu 32×32 px gốc (96 px ở 1080p) |
| Filter | Point, không mipmap, không nén mất màu (palette khóa) |
| Ánh sáng | Sprite tự đổ bóng, **nguồn sáng cố định trên-trái**; ánh lửa lò / Kính soi làm trong engine (2D Light hoặc overlay), không vẽ sẵn |
| Chữ UI | **Font hi-res** (TMP, đủ dấu tiếng Việt) trên nền pixel — không dùng font pixel cho chữ; chi tiết ở `ui` / §7 |
| Nguồn art | **AI tạo ảnh + chỉnh tay**: tạo lớn → thu nhỏ nearest về lưới đích → ép về palette khóa (§4) → sửa tay |

## 1. Visual Identity Statement — "Lò là mặt trời"

**Luật một câu**: *Khi phân vân, thứ quan trọng thì sáng và ấm hơn; thứ chưa biết thì chìm trong bóng tối.*

Xưởng giả kim ấm cúng mà lò nung là nguồn sáng của cả thế giới: trung tâm (lò, lưới, nguyên liệu) sáng, bão hòa,
"cozy"; rìa màn hình tối, lạnh, lùi ra sau. Ánh sáng là ngôn ngữ thông tin.

| Nguyên tắc | Phép thử | Pillar phục vụ |
|---|---|---|
| **Value hierarchy theo ánh lò**: lưới & nguyên liệu có value sáng nhất; nền/môi trường ≤ 40% value (ngoài trời ≤ 55, §6) | Chuyển ảnh chụp gameplay sang grayscale → mắt vẫn rơi vào lưới trước tiên; nếu một vật trang trí sáng ngang nguyên liệu → hạ value nó | P2, P4 |
| **Ánh sáng = thông tin đã biết**: chỉ thứ người chơi đã được biết mới phát sáng — `Magic` (công khai) có hiệu ứng ma thuật riêng; dòng thuộc tính ẩn chưa soi **không có bất kỳ chỉ báo nào** (mọi thành phẩm trông như nhau bất kể còn bao nhiêu dòng ẩn); Kính soi "thắp sáng" để dòng mới hiện trong Bàn soi; không hiệu ứng nào tùy theo khoảng cách công thức hay ô đúng/sai | Khi một hiệu ứng ánh sáng/glow thay đổi theo dữ liệu ẩn → bỏ hiệu ứng đó (luật Không rò rỉ GDD §7, §6.2) | P2 |
| **Nguyên liệu như đồ chơi**: khối đơn giản, bo tròn, bão hòa, viền tối 1 px có màu (không đen thuần); mỗi khoáng một silhouette riêng | Thu sprite về 32×32 và tô đặc một màu → vẫn phân biệt được với mọi nguyên liệu khác cùng tier; nếu không → đổi hình khối, không chỉ đổi màu | P1 |

## 2. Mood & Atmosphere

Biến điều khiển chung: **cường độ & nhiệt độ ánh lò**. Cùng một xưởng, mỗi trạng thái khác nhau rõ.

| Trạng thái game | Cảm xúc mục tiêu | Ánh sáng (nhiệt độ màu, tương phản) | 3–5 tính từ | Năng lượng |
|---|---|---|---|---|
| Menu chính | Mời gọi, hoài niệm | Lò âm ỉ cam đậm (~2000K); xưởng gần tối; tương phản trung bình | yên, ấm, bí ẩn, chờ đợi | Thấp |
| Chiếu chỉ | Trang trọng, áp lực từ trên | Ánh trắng-vàng lạnh hơn từ cửa sổ/cuộn chiếu; lò lùi vào nền | uy nghi, nghiêm, rõ ràng | Trung bình, tĩnh |
| Bàn luyện (~80% thời gian) | Cozy, tập trung, tò mò | Lò sáng đều vàng-cam (~2700K); lưới sáng nhất; rìa tối dịu; tương phản vừa (nhìn lâu không mỏi) | ấm cúng, ngăn nắp, tò mò, an toàn | Trung bình-thấp, đều |
| Nung / phát lại | Hồi hộp → vỡ òa | Lò bùng theo từng nhịp nạp, tương phản tăng dần; thành phẩm = lóe vàng-trắng; phế phẩm = khói xám + ánh lò tụt (khác cả hình lẫn âm, không chỉ màu) | nhịp nhàng, nín thở, bùng nổ | Tăng dần, đỉnh ở kết quả |
| Dâng | Cân nhắc, nặng nề | Spotlight vào khoáng đang chọn, phần còn lại tối hẳn; tương phản cao | trang nghiêm, cân nhắc, cô đọng | Thấp, chậm |
| Rèn & Trận | Căng, kịch tính | Đỏ-cam lò rèn + tia lửa; tương phản cao nhất game; nền chiến trường xanh-xám lạnh đối lập | dữ dội, nóng, gấp, kim loại | Cao nhất |
| Thắng / Chiến lợi phẩm | Nhẹ nhõm, được thưởng | Lò bùng lại vàng ấm, sáng nhất game; 3 lựa chọn được thắp sáng **như nhau** | rực rỡ, hào phóng, ấm, nhẹ | Cao → lắng |
| Thua / Trục xuất | Mất mát nhưng muốn chơi lại | Lò tắt dần còn than hồng; xưởng lạnh xanh-tím; tương phản thấp, ít màu; Uy tín mất = ngọn lửa nhỏ lại | lạnh, lặng, tàn, không trừng phạt | Thấp, rơi xuống |

- "Cozy" thuộc về Bàn luyện / Menu / Thắng; "tối, ấm, kịch tính" thuộc về Dâng / Trận / Thua — màn chơi chính không bao giờ tối.
- Mọi biến đổi ánh sáng chỉ phản ánh **kết quả đã lộ** (thành/hỏng, thắng/thua), không phản ánh dữ liệu ẩn (§1 nguyên tắc 2).
- Hiệu ứng nung phải tua được (P4): tua → ánh sáng nhảy thẳng tới trạng thái cuối, không lộ thông tin trung gian.

## 3. Shape Language

Không có nhân vật điều khiển — "nhân vật" chính là **vật phẩm**. NPC (cận thần, thợ rèn, dũng sĩ, đối thủ) xuất
hiện ở Dâng và cutscene Rèn & Trận — silhouette/phong cách: §5 (TBD, A5). Mỗi `ItemDef.Kind` một họ silhouette; phép thử chung:
tô đặc một màu ở 32×32 vẫn phân biệt được loại.

| Loại | Họ hình khối | Đặc điểm phân biệt duy nhất |
|---|---|---|
| Khoáng (tier 1–10) | Vật rắn không vỏ — đá/tinh thể, đường bao lồi, cạnh vát bo | Không có vật chứa, không tay cầm |
| Phụ gia | Chất **trong vật chứa** — lọ tròn, túi buộc dây, bình cổ hẹp | Luôn thấy vật chứa |
| Kính soi | Vòng tròn + tay cầm (kính lúp viền đồng) | Duy nhất có lỗ tròn rỗng |
| Xúc tác | Bùa/vòng **đối xứng tâm** (4 hoặc 6 nhánh) | Duy nhất đối xứng xoay |
| Phế phẩm | Cục xỉ lõm, méo, không đối xứng, không viền màu | Silhouette lõm duy nhất |

- **Tier khoáng** đọc bằng độ phức tạp (mọi sprite cùng 32×32, không đổi kích thước): T1–3 đá thô tròn 2–3 mặt ·
  T4–7 tinh thể 4–6 mặt, đối xứng hơn, có ánh trong · T8–9 cụm nhiều mũi, viền sáng · T10 silhouette biểu tượng
  riêng, không khoáng nào khác dùng. Kênh dự phòng: chấm/vạch tier nhỏ ở góc ô.
- **Hình khối mang nghĩa**: tròn/bo = an toàn, tương tác được (nguyên liệu, nút, lò) · chữ nhật vững bo nhẹ = cấu
  trúc/vật chứa (lưới, Túi, Chợ, khung UI) · nhọn/răng cưa = áp lực/nguy hiểm (huy hiệu đối thủ, Chiến tranh/Đại
  chiến, Uy tín mất).
- **Hero shape vs phụ**: hero = miệng lò vòm tròn lớn ở giữa + lưới 3×3 trên bàn đá trước lò (khối lớn nhất, sáng
  nhất). Phụ = kệ, chai lọ, dụng cụ: khối chữ nhật đơn giản, value tối, ít chi tiết, **không dùng lại silhouette vật
  phẩm** (không nhầm với thứ tương tác được).
- **Ô lưới giống hệt nhau**, không trang trí khác theo vị trí — hình ảnh không gợi ý ô "đặc biệt" (P2).
- **Ma thuật** (công khai, GDD §6.2): cùng loại khoáng có/không `Magic` dùng **chung sprite**; khác biệt duy nhất là
  lớp hiệu ứng ma thuật (rune tím quay quanh + lấp lánh, overlay riêng, không vẽ vào sprite) — đọc được ở 32 px.
  Thuộc tính ẩn khác: không dấu hiệu nào trên sprite.
- **Ngữ pháp hình khối UI**: lặp lại vật liệu thế giới (gỗ, đồng; pixel 9-slice, góc bo 2 px gốc); nút chính bo
  tròn hơn khung chứa; chữ là font hi-res (§0).

## 4. Color System

Xem trước (swatch, cảnh pixel, grayscale, mô phỏng mù màu): https://claude.ai/artifact/BiTDoWMdQWEcQcrCtwmQLH

| Vai trò | Màu (hex) | L* | Ý nghĩa trong game | Kênh dự phòng (mù màu) |
|---|---|---|---|---|
| Nền xưởng | `#241A26` | 11 | Rìa tối, nền sâu nhất | — |
| Môi trường | `#4A3442` | 25 | Kệ, tường, đồ phụ (≤ 40% value) | — |
| Bóng lạnh | `#3A4766` | 30 | Đổ bóng, chiến trường, Thua | — |
| Khung UI (đồng) | `#8A5A3C` | 43 | Khung 9-slice, viền Túi/Chợ | — |
| **Primary — ánh lò** | `#F2A03A` / sáng `#FFE08A` | 72 / 90 | Lò, vùng hero, highlight | — |
| Phần thưởng | `#F6C945` | 83 | Vàng (tiền), thành phẩm, chiến lợi phẩm | Icon đồng xu; thành phẩm lóe hình sao |
| **Tương tác được** | `#3FD0C0` | 76 | Đang chọn, ô thả hợp lệ, nút chính — **chỉ dùng cho UI** | Viền dày 2 px + nhịp chuyển động |
| Nguy hiểm | `#D23C52` | 49 | Uy tín mất, đối thủ, cảnh báo | Hình nhọn/răng cưa (§3) + icon lửa tắt |
| Phế phẩm / trung tính | `#7D7470` | 50 | Phế phẩm, disabled | Silhouette lõm (§3) + khói |
| Ma thuật | `#A274FF` | 59 | Thành phẩm có `Magic` (công khai) | Rune lấp lánh + chuyển động, luôn kèm icon |
| Chữ | `#F4E6C8` | 92 | Chữ trên nền tối | Tương phản 13.6:1 (AAA) |

- **Cặp dễ nhầm đã xử lý**: không dùng cặp đỏ–xanh lá (thành = vàng + lóe, hỏng = xám + khói) · tím ma thuật /
  xanh bóng lạnh (2.9:1) → ma thuật luôn sáng hơn + rune bắt buộc · đỏ nguy hiểm / xám phế phẩm cùng L* ≈ 49 →
  không cùng ngữ cảnh + hình khối riêng · teal / ánh lò cùng độ sáng → viền 2 px + chuyển động.
- **Giá trị sáng-tối** (5 dải L*; cảnh ngoài trời được nâng môi trường tới 55 — §6): nền 10–25 · môi trường 25–40 · khung UI 40–55 · vật phẩm 55–85 · highlight/lóe
  85–100. Phép thử: ảnh gameplay grayscale vẫn đọc được chủ thể.
- **Nhiệt độ màu theo trạng thái**: ấm (Bàn luyện, Thắng) · trắng-vàng (Chiếu chỉ) · đỏ-cam đối lập xanh-xám
  (Trận) · xanh-tím (Thua) — khớp §2.
- **Palette khóa pixel art**: ~32 màu, ramp dựng từ màu vai trò theo *hue shift* (bóng ngả tím/lạnh, sáng ngả
  vàng); mọi asset AI phải ép về palette này. **Hex đủ 32 màu: TBD** — chốt cùng asset mẫu (seed) ở `asset`.
- **Palette UI**: dùng chung palette thế giới; teal chỉ dành cho UI tương tác, không xuất hiện trên sprite môi
  trường → thấy teal là biết bấm được.

> **Tạm chốt 2026-11-04.** Bố cục Bàn luyện đã xử lý ở [design/ui-ban-luyen.md](design/ui-ban-luyen.md) (A1).

## 5. Character Design Direction

NPC xuất hiện ở Dâng (`design/ui-season.md` §B), cutscene Rèn (§C) và Trận (`design/battle.md`). Không có nhân vật
điều khiển.

**Khung chung**
- **Chibi ~3 đầu, cao 64 px gốc** (192 px ở 1080p); đầu ~22 px để đọc biểu cảm. Khung: NPC 64×64, dũng sĩ 96×64
  (tầm vung vũ khí). Pivot **bottom-center** (chân).
- Viền 1 px có màu, sáng trên-trái (§0, §1); value nhân vật 40–85, môi trường ≤ 40 (§4); chuyển động = đổi frame +
  dịch bước px, 8 fps (§7).
- **Màu dành riêng không dùng cho quần áo**: teal (UI), tím ma thuật, đỏ nguy hiểm.

**Silhouette — mỗi vai một đặc điểm đọc được ở thumbnail**

| Vai | Hình khối chủ đạo | Đặc điểm duy nhất | Palette |
|---|---|---|---|
| Cận thần | chữ nhật đứng (áo dài thẳng) | mũ quan dẹt có lông + **khay tròn** luôn cầm | xanh than `#3A4766` + viền đồng |
| Thợ rèn | vuông / tròn (to ngang) | tạp dề da, râu, khăn trùm, tay áo xắn | nâu da + ánh lò cam |
| Dũng sĩ ta | tam giác ngược (vai rộng) | mũ trụ có chùm lông cam ánh lò | vải xám-nâu trung tính + lông `#F2A03A` |
| Dũng sĩ đối thủ | cùng rig dũng sĩ ta | **dáng mũ riêng** mỗi vương quốc + huy hiệu trên ngực | 8 palette vương quốc (giảm bão hòa, tránh màu dành riêng) |

- **Đối thủ**: 8 vương quốc (palette + huy hiệu + dáng mũ). Bậc khó thể hiện bằng lớp giáp: bậc 1 áo vải → bậc 2
  + giáp vai → bậc 3 + áo choàng, giáp kín. Tổng: 1 rig + 8 bộ màu / mũ / huy hiệu + 2 lớp giáp.
- **Biểu cảm thợ rèn** (3 mức, 3 kênh): thất vọng = mày chau + vai sụp + bong bóng "…" · bình thường = gật + "!" ·
  hài lòng = cười + giơ ngón cái + "★". Bong bóng icon là kênh dự phòng ở cỡ nhỏ.

**Trang bị — lớp chồng + đổi palette**
- 4 loại (Kiếm, Giáo, Giáp, Khiên) vẽ **một lần** bằng ramp xám 4 tông (viền / bóng / nền / sáng). Lúc chạy thay
  bằng ramp khoáng: mỗi `ItemDef` khai báo `EquipmentRamp` (4 màu từ palette khóa). Có Ma thuật → thêm overlay rune
  tím. Kỹ thuật (palette-swap shader / lookup) chốt ở `arch`.
- Kiếm, Giáo: tay phải · Khiên: tay trái · Giáp: lớp phủ thân.
- Trang bị là Giáp / Khiên → dũng sĩ đánh bằng **chùy gỗ trơn mặc định** (màu trung tính, không nhầm với trang bị
  vừa rèn).

**Animation tối thiểu** (frame)

| Nhân vật | Animation |
|---|---|
| Cận thần | idle 4 · đi cầm khay 6 · đưa / nhận 3 |
| Thợ rèn | idle 4 · cầm khoáng kiểm tra 4 · 3 biểu cảm × 4 · **4 cảnh đúc** (mỗi loại trang bị, 8 frame; bối cảnh §6) |
| Dũng sĩ (rig chung) | idle 4 · đi 6 · mặc trang bị 4 · lao tới 3 · chém 4 · trúng đòn 2 · đỡ (Hấp thụ) 2 · gục 4 · ăn mừng 4 |

Tổng ~150 frame.

**Phép thử**: tô đặc một màu 4 vai ở 64 px đặt cạnh nhau → phân biệt được vai chỉ bằng hình; 8 vương quốc phân biệt
được ở grayscale nhờ dáng mũ.

## 6. Environment Design Language

**Ngôn ngữ chung**
- Kiến trúc trung cổ giả tưởng ấm cúng: khối đá xám ấm + gỗ, cạnh bo nhẹ, chi tiết thấp (§3).
- Chất liệu pixel vẽ tay (không PBR): đá, gỗ, đồng, vải. Dither 2 màu **chỉ** cho mảng chuyển sắc lớn (trời,
  quầng sáng); không dither vật nhỏ; không texture ảnh.
- Lớp (camera đứng yên, không parallax): tường / phông xa → đạo cụ giữa → vật hero → khung tiền cảnh → overlay ánh
  sáng (2D Light hoặc sprite — A4). Nền 800×360, vùng an toàn 640×360 (§8); ghép sprite theo lớp, không Tilemap.
- **Value**: trong nhà ≤ 40 L* (§1) · **ngoài trời ≤ 55 L*, bão hòa thấp** (ngoại lệ chốt 2026-11-04) · nhân vật
  chênh ≥ 20 L* so với mảng nền ngay sau lưng + viền tối 1 px.
- Không dùng `#D23C52` (đỏ nguy hiểm) cho nền; trời hoàng hôn dùng ramp cam-tím trầm.

**Địa điểm**

| Địa điểm | Dùng cho | Hero / mật độ đạo cụ | Ánh sáng |
|---|---|---|---|
| **Xưởng giả kim** | Menu (lò âm ỉ), Chiếu chỉ (nền mờ), Bàn luyện, Dâng (lò lùi tối, spotlight khay) | hero = lò vòm; **mép dày, giữa trống**; khe môi trường giữa cột (`design/ui-ban-luyen.md` §3) | ấm 2700K từ lò; cường độ theo §2 |
| **Lò rèn hoàng gia** | cutscene nhịp 1–7 | hero = đe + lò rèn; mật độ vừa; **4 cảnh đúc chung nền**, khác đạo cụ tiền cảnh: Kiếm (đe + búa) · Giáo (đe + cán dài) · Giáp (khuôn ngực) · Khiên (khuôn tròn) | đỏ-cam, tia lửa, tương phản cao |
| **Nơi giao đấu** | nhịp 8 + Trận | **giữa trống** cho 2 dũng sĩ; mép: cờ huy hiệu vương quốc đối thủ | ngoài trời ≤ 55 L*, giảm bão hòa |

- **Nơi giao đấu — biến thể**: **Đấu trường** (Thi đấu: khán đài, cờ) và **Chiến trường** (Chiến tranh: đồi, trời u
  ám xanh-xám) dùng chung nền đất, chỉ đổi phông xa. **Đại chiến** (Mùa 8) là **cảnh riêng đầy đủ** — cao trào
  run: thành lũy hai phe, trời hoàng hôn rộng, cờ dày gấp đôi (chốt 2026-11-04).

**Kể chuyện — cửa sổ theo Mùa**
- Cửa sổ tròn cao trên tường xưởng, trong khe môi trường trái (~40×40 px gốc); ở Menu lộ rõ hơn.
- Mùa 1–2 xuân (cành hoa) · 3–4 hạ (nắng) · 5–6 thu (lá rơi) · 7–8 đông (tuyết — Đại chiến giữa đông).
- Ánh sáng cửa sổ rất nhẹ (≤ 40 L*), không cạnh tranh với lò. Asset: 1 khung + 4 cảnh.

**Danh sách asset môi trường**: Xưởng (nền + bộ đạo cụ + cửa sổ 4 cảnh) · Lò rèn (nền + 4 bộ đạo cụ tiền cảnh) ·
Nơi giao đấu (nền đất + 2 phông xa + 8 cờ, dùng huy hiệu §5) · Đại chiến (nền riêng).

**Phép thử**: đặt NPC §5 vào từng nền ở grayscale → nhân vật tách khỏi nền; Bàn luyện vẫn qua phép thử §1 (mắt rơi
vào lưới trước).

## 7. UI / HUD Visual Direction

Spec màn đầu tiên: [design/ui-ban-luyen.md](design/ui-ban-luyen.md).

| Mục | Quy định |
|---|---|
| HUD | Screen-space uGUI; xưởng (world sprite) phía sau; khung UI lặp vật liệu thế giới (gỗ, đồng, 9-slice) |
| Font chính / phụ | Alegreya SC (tiêu đề, tên vật phẩm) · Be Vietnam Pro (nội dung, số tabular). TMP atlas **tĩnh** Latin + tiếng Việt (U+00C0–U+01B0, U+1EA0–U+1EF9), SDF 2048², padding 10%; ký hiệu dùng TMP Sprite Asset pixel. ⚠ Kiểm glyph Alegreya SC — thiếu → Cormorant SC |
| Thang cỡ chữ | H1 48 / H2 34 / Body 26 / Caption 22 ở 1080p; tối thiểu **22**; line spacing +20% (dấu chồng) |
| Icon | Pixel 16×16, viền 1 px có màu như vật phẩm — không trộn icon flat/vector |
| Trạng thái control | primary: nền teal `#3FD0C0`, chữ `#10201E` · secondary: nền `#3A2A36`, khung đồng `#8A5A3C` · pressed: dịch xuống 3 px + khung tối · disabled: mờ 38%, giữ hình · selected: viền teal 3 px · xác nhận nguy hiểm: khung đỏ + icon |
| Chuyển động UI | Sprite pixel chỉ dịch bước 3 px / đổi frame / overlay — không scale, xoay lẻ. Thời lượng chuẩn 80 / 120 / 150 / 180 / 220 ms; ease-out cho "settle". Tùy chọn Giảm chuyển động |
| Safe area & tỉ lệ màn hình | Đủ 16:9 → 20:9; hẹp hơn 16:9 → panel phụ thu thành tab; mobile `Screen.safeArea` (sau MVP) |

Xung đột hình ảnh ↔ khả năng đọc đã quyết: (1) Scale With Screen Size ↔ pixel đều — xem §8.4; (2) thẻ Bàn soi cố
định tốn chỗ ↔ hợp touch, lấp khoảng trống → giữ cố định; (3) chữ đỏ tương phản 3.6:1 → chỉ ≥ 24 px đậm + icon.

## 8. Asset Standards (hợp đồng kỹ thuật)

### 8.1 Đặt tên & thư mục

`<loại>_<tên>_<biến thể>[_<frame 2 số>].png` — chữ thường, snake_case, không dấu.
Loại: `itm` (vật phẩm, kèm kind: `itm_min_`, `itm_add_`, `itm_lens_`, `itm_cat_`, `itm_slag`) · `env` · `ui` · `ico`
(icon HUD) · `vfx` · `bg`. Ví dụ: `itm_min_copper_t01.png`, `itm_add_saltpeter_t02.png`, `ui_btn_primary_pressed.png`,
`vfx_brew_success_03.png`, `bg_workshop_wall.png`.

- Asset game: `Assets/_Project/Art/{Items,Env,UI,Icons,VFX,Backgrounds,Palette}`; atlas: `Assets/_Project/Art/Atlases/`.
- File nguồn (ảnh AI gốc, Aseprite/PSD, prompt): `ArtSource/` ở gốc repo (ngoài `Assets/`, không import), **commit vào
  git** (không LFS). Ảnh AI gốc lớn lưu JPG/WebP để repo không phình.

### 8.2 Kích thước (px gốc; ×3 = px màn hình ở 1080p)

| Loại | Kích thước | Pivot | Ghi chú |
|---|---|---|---|
| Vật phẩm | **32×32** | Center | Nội dung trong 30×30, chừa 1 px cho viền; tier pip là sprite UI riêng, không vẽ vào vật phẩm |
| Ô lưới lò | 48×48 | Center | Một sprite cho cả 9 ô (§3) |
| Icon HUD | 16×16 | Center | Vùng chạm ≥ 64 px do RectTransform lo, không phóng icon |
| Khung UI 9-slice | 24×24, border 8 | Center | Nút: normal / pressed / disabled; viền teal "selected" là overlay 9-slice riêng |
| Hero lò | ~160×128 | Bottom-center | |
| Prop môi trường | bội số của 8 | Bottom-center | |
| Nền | **800×360** | Center | Vùng an toàn 640×360 ở giữa; phần dư phủ tới ~20:9 |
| VFX | frame 32×32 hoặc 64×64 | Center | Sheet lưới, 12 fps |

### 8.3 Import settings Unity (mọi sprite pixel)

| Thiết lập | Giá trị |
|---|---|
| Texture Type / Sprite Mode | Sprite (2D and UI) / Single (sheet animation: Multiple, cắt Grid By Cell Size) |
| Pixels Per Unit | **32** — mọi sprite, không ngoại lệ |
| Mesh Type | Full Rect (bắt buộc cho 9-slice; mọi sprite ≤ 64 px); Tight cho prop lớn |
| Filter Mode / Wrap Mode | **Point** / Clamp |
| Generate Mip Maps | Tắt |
| Compression | **None (RGBA32)** — BC/DXT (WebGL) và ASTC (mobile) làm lem pixel và lệch palette |
| sRGB / Alpha Is Transparency | Bật / Bật |
| Read/Write | Tắt |
| Generate Physics Shape | Tắt (game không dùng physics) |

**Sprite Atlas (V2)**: `atlas_items`, `atlas_ui` (gồm icon), `atlas_workshop` (env + hero lò), `atlas_vfx`; nền 800×360
để ngoài atlas. Padding 2 · Allow Rotation tắt · Tight Packing tắt · Filter Point · mipmap tắt · Compression None ·
Max size 2048 (mục tiêu ≤ 1024²).

### 8.4 Hiển thị

- **World**: Pixel Perfect Camera — Assets PPU 32, Reference 640×360, Upscale Render Texture tắt → scale nguyên (§0).
- **UI (uGUI)**: Canvas Scaler **Scale With Screen Size**, reference 1920×1080, Match = 1 (giữ chiều cao; màn rộng hơn
  thì thêm chỗ ngang), **Reference Pixels Per Unit = 96** → sprite PPU 32 ra ×3 với Set Native Size. Bật Canvas
  `Pixel Perfect`; kích thước/vị trí RectTransform là bội số của 3 (1 px gốc).
  - Hệ quả chấp nhận: ở độ phân giải khác bội số của 640×360 (vd 1366×768 → 2.13×) pixel UI to nhỏ không đều.
    Kiểm tra bằng mắt ở 1366×768 và 1440×900 khi làm UI; quá xấu → quay lại phương án scale nguyên bằng script.
- **Chữ**: TMP SDF hi-res, ngoài luật pixel (§0).

### 8.5 Kiểm tra khi nhận asset (cả quy trình AI)

1. Đúng kích thước bảng 8.2, là px gốc (không phải ảnh hi-res thu nhỏ có pixel lẻ).
2. **Mọi pixel thuộc palette khóa** (§4); alpha chỉ 0 hoặc 255 — không bán trong suốt, không anti-alias.
   (Tự động hóa bằng script Python kiểm palette/alpha — làm ở `asset`.)
3. Viền 1 px có màu; sáng trên-trái; đọc được khi đặt vào ảnh chụp gameplay (§1, §3).
4. Ghi nguồn, công cụ, prompt, license vào `Docs/ASSETS.md`.

### 8.6 Ngân sách (tạm — chốt ở `arch`, TECH §6)

| Hạng mục | Ngân sách |
|---|---|
| Texture runtime (WebGL) | ≤ 32 MB |
| Mỗi atlas | ≤ 1024² (4 MB RGBA32); 2048² chỉ khi thật cần |
| Draw call màn Bàn luyện | ≤ 30 |
| Audio | TBD — `gdd audio` |

**Xung đột art ↔ kỹ thuật đã quyết**:
- Không nén (đúng màu palette) ↔ tốn bộ nhớ hơn → chọn không nén; tổng texture nhỏ.
- Pixel UI đều tuyệt đối (scale nguyên) ↔ UI phủ kín mọi độ phân giải → **chọn Scale With Screen Size** (người dùng,
  2026-11-04); world vẫn scale nguyên qua Pixel Perfect Camera.

## 9. Reference Direction

| Tham chiếu | Lấy gì | Tránh gì (để không thành bản sao) |
|---|---|---|
| {{game/phim/phong cách}} | {{}} | {{}} |

Không dùng tên nghệ sĩ còn sống làm định nghĩa phong cách — chuyển thành thuộc tính quan sát được.

## 10. AI Prompt Card (nếu dùng công cụ tạo ảnh)

Khối bất biến (dán vào mọi prompt cùng họ asset):
```
ART DIRECTION: {{shape language}}, {{palette roles + hex}}, {{materials}}, {{edge/outline treatment}}
VIEW: {{orthographic side view / top-down 3/4}}, {{facing}}
GAME-SCALE READ: {{silhouette & chi tiết phải còn ở WxH px}}
TECHNICAL OUTPUT: {{kích thước}}, transparent background, {{số frame/ô}}
LOCKS: preserve {{identity, proportions, palette, outline, light direction}}
EXCLUDE: text, labels, watermark, mockup frame, scenery, duplicate objects, cropped edges
```
Khối thay đổi theo asset: `ROLE/PURPOSE`, `SUBJECT`.
Asset mẫu đã duyệt (seed): {{đường dẫn}} — mọi biến thể dùng mẫu này làm tham chiếu.

## Open Questions

| # | Câu hỏi | Làm ở | Ghi chú |
|---|---|---|---|
| A1 | ~~Bố cục & mật độ màn Bàn luyện~~ | `ui` | **Đã xử lý 2026-11-04** — design/ui-ban-luyen.md |
| A2 | Hex đủ 32 màu của palette khóa | `asset` (seed) | |
| A3 | Font: Alegreya SC + Be Vietnam Pro — chờ kiểm glyph tiếng Việt trong TMP | `asset` / `scene` | Chốt tạm 2026-11-04 (§7) |
| A4 | ~~2D Light hay overlay~~ — **Đã chốt 2026-11-04**: sprite overlay (TECH ADR-006) |
| A5 | ~~NPC~~ — **Đã xử lý 2026-11-04** (§5) | `art 5` | Phát sinh 2026-11-04 (run-flow) |
| A6 | ~~Sprite trang bị~~ — **Đã chốt**: shader Shader Graph map 4 chỉ số xám → `EquipmentRamp` qua `MaterialPropertyBlock` (TECH §1) |
| A7 | ~~§6 Environment~~ — **Đã xử lý 2026-11-04** (§6) |

## Lịch sử duyệt

| Ngày | Mục | Thay đổi | Duyệt bởi |
|---|---|---|---|
| 2026-11-04 | §0 | Pixel art, canvas gốc 640×360 (×3), PPU 32, Point; chữ UI font hi-res; nguồn AI + chỉnh tay | người dùng |
| 2026-11-04 | §1 | Hướng A "Lò là mặt trời" (chọn trong 3 hướng A/B/C) | người dùng |
| 2026-11-04 | §2 | 8 trạng thái, ánh lò là biến điều khiển chung | người dùng |
| 2026-11-04 | §3 | Họ silhouette theo `ItemDef.Kind`, tier theo độ phức tạp + chấm tier | người dùng |
| 2026-11-04 | §4 | Tạm chốt palette vai trò (12 màu); palette khóa 32 màu TBD; bố cục cần làm thêm (A1) | người dùng |
| 2026-11-04 | §8 | Asset Standards: PPU 32, Point, không nén, 4 atlas; UI Scale With Screen Size (ref PPU 96); nguồn ở `ArtSource/` trong git | người dùng |
| 2026-11-04 | §1, §3, §4 | `Magic` công khai (hiệu ứng overlay, chung sprite); dòng ẩn không có chỉ báo — khớp GDD §6.2 mới | người dùng |
| 2026-11-04 | §7 | Quy tắc UI chung (font, thang chữ, icon, trạng thái control, chuyển động pixel) từ `ui` Bàn luyện | người dùng |
| 2026-11-04 | §5 | Chibi 64 px; silhouette 4 vai; 8 vương quốc + lớp giáp theo bậc; biểu cảm thợ rèn 3 kênh; trang bị ramp xám → `EquipmentRamp`; chùy gỗ mặc định; ~150 frame | người dùng |
| 2026-11-04 | §6 | 3 địa điểm (Xưởng, Lò rèn, Nơi giao đấu) + Đại chiến cảnh riêng; ngoại lệ value ngoài trời ≤ 55; cửa sổ theo Mùa | người dùng |
