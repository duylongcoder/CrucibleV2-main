# Game Design Document — Lò Giả Kim (working title: Crucible)

> **Trạng thái**: In Review (sửa sau `gdd --review` 2026-11-03) · **Cập nhật**: 2026-11-03 · **Rigor**: standard
> Chủ sở hữu: Game Designer. Hệ thống lớn tách sang `Docs/design/<system>.md` và chỉ để link ở §6.
> Nguồn gốc: viết lại từ `demo_GDD.md` (CLAUDE.md dự án Crucible v2 — "Run liên tục"), có tinh chỉnh game flow
> và core loop (xem §9). Mọi con số trong tài liệu là **tạm**, đọc từ config, chỉnh bằng `balance`/playtest.

## 1. Concept

- **Elevator pitch**: Puzzle roguelite 2D — bạn là nhà giả kim triều đình, xếp nguyên liệu lên lò 3×3 như xếp
  một **dòng thời gian**, tự giải mã công thức bí mật để luyện khoáng thạch cho Nhà vua rèn trang bị ra trận;
  thua 3 trận là bị trục xuất.
- **Thể loại / tham chiếu**:

  | Tham chiếu | Lấy gì | Tránh gì |
  |---|---|---|
  | Balatro | Cấu trúc run có "boss" mỗi chặng; vật phẩm giữ suốt run tạo combo (Xúc tác) | Bài/tay bài; điểm số bùng nổ vô hạn |
  | Potion Craft | Fantasy nhà giả kim; niềm vui tìm ra công thức | Bản đồ tự do, mô phỏng vật lý |
  | Mastermind / Wordle | Suy luận từ thông tin ẩn; kiến thức tích lũy qua nhiều lần thử | Phản hồi chi tiết từng ô (ở đây chỉ biết thành/hỏng) |

- **Người chơi mục tiêu**: người thích roguelite + puzzle suy luận (Explorer + Achiever); chơi trên trình
  duyệt, một run mục tiêu 35–40 phút (chấp nhận 30–45 — playtest), dừng giữa chừng được.
- **Nền tảng & input**: **WebGL trước** — màn hình ngang, reference 1920×1080, chuột (kéo-thả **và**
  chạm/click-để-đặt). **Mobile touch sau** → mọi thao tác phải làm được bằng chạm, không phụ thuộc hover
  (tooltip = nhấn giữ / chạm vào).
- **USP**: Lưới là dòng thời gian — mỗi ô là một nhịp; **khoảng cách giữa các nguyên liệu là dữ liệu ẩn**: cùng
  một nhóm nguyên liệu + thứ tự, khoảng cách khác → thành phẩm khác; người chơi chỉ biết bằng tự kiểm nghiệm.
  Khoảng cách cố định ở mọi seed → hiểu biết chính là tiến bộ; chia sẻ công thức trong cộng đồng là trải nghiệm
  được chấp nhận.
- **Phạm vi MVP**: run 8 Mùa; khoáng & phụ gia tier 1–10 (Khoáng thạch Ma thuật Tối thượng = tier 10); 3 bậc
  khó × 8 đối thủ; 3 bộ khởi đầu; meta = Sổ công thức + Danh vọng. **Sau MVP**: Endless, modifier đổi luật,
  Thuốc thử, bản Mobile. Nguồn lực/thời gian: **TBD**.
- **Ràng buộc kỹ thuật phát sinh từ thiết kế** (chi tiết thuộc `TECH.md`, chốt ở `arch`):
  tất định tuyệt đối (cùng seed + cùng input → cùng kết quả, kể cả WebGL); lưu sau mỗi hành động; content là
  data; tầng hiển thị không được biết thông tin người chơi chưa được biết.

## 2. Pillars

| # | Pillar | Ý nghĩa | Phép thử thiết kế ("khi phân vân X, chọn Y") |
|---|---|---|---|
| P1 | **Hiểu biết là tiến bộ** | Thứ người chơi mang qua các run là kiến thức (công thức, chiến lược), không phải chỉ số | Khi phân vân giữa thưởng *chỉ số* và thưởng *lựa chọn/thông tin* → chọn lựa chọn/thông tin. Công thức **không bao giờ** random. |
| P2 | **Bí mật công bằng** | Chỉ công thức là bí mật; mọi luật còn lại đọc được, tất định, học được | Khi một thông tin *không phải công thức* làm người chơi thua mà không hiểu vì sao → lộ ra (ít nhất ở dạng định tính). Khi thông tin *là* công thức → giữ kín. |
| P3 | **Áp lực có tính toán** | Tài nguyên khan hiếm, mỗi Mùa buộc đánh đổi: bán để mua, thử nghiệm hay chơi chắc, dâng hay giữ | Khi phân vân cho thêm tài nguyên hay buộc đánh đổi → buộc đánh đổi (nhưng luôn có đường đi tiếp — không kẹt). |
| P4 | **Tôn trọng thời gian** | Nhịp liền mạch, dừng là dừng, không bắt xem lại thứ đã biết | Khi phân vân giữa trình diễn và nhịp độ → nhịp độ (trình diễn phải tua được, **hoặc** ≤ 3 s ở timescale tối đa 3× — vd cutscene Rèn & Trận). |

**Không phải game này** (anti-pillars): không phản xạ/hành động thời gian thực; không online/PvP; không gacha,
không tiền thật; không công thức ngẫu nhiên theo seed; không grind chỉ số meta để thắng thay kỹ năng; không
mô phỏng vật lý/hóa học (fiction chỉ để dễ hình dung).

**Cảm xúc mục tiêu (MDA Aesthetics)**: **Discovery** (chính — giải mã công thức, chuỗi tầng, nhánh ma thuật) ·
**Challenge** (áp lực Uy tín, kinh tế) · **Fantasy** (nhà giả kim triều đình, sống chết theo lò nung).

## 3. Core Loop

### Game flow một Mùa (đã gộp phase — 4 màn + 1 màn thưởng)

```
┌─ CHIẾU CHỈ ─────────────────────────────────────────────────────────────┐
│ Popup: "Nhà Vua cần rèn một thanh <LOẠI TRANG BỊ> <thuộc tính ưa>"      │
│ · Modifier Mùa sau (nếu có). Đối thủ CHƯA lộ (lộ trong Rèn & Trận)      │
└──────────────┬──────────────────────────────────────────────────────────┘
               ▼
┌─ BÀN LUYỆN  (lượt 1 → 2 → 3) ───────────────────────────────────────────┐
│ [Ngăn Chợ: mua / bán / reroll / Kính soi]   [Lưới 3×3]   [Túi]          │
│ Xếp tự do → NUNG (khóa lưới) → phát lại theo nhịp (chạm để tua)         │
│            → thành phẩm / phế phẩm vào Túi (+ Sổ công thức nếu mới)     │
│ hoặc BỎ QUA lượt                                                        │
└──────────────┬──────────────────────────────────────────────────────────┘
               ▼
┌─ DÂNG ──────────────────────────────────────────────────────────────────┐
│ Soi (Kính soi) + chọn 1 khoáng đạt chuẩn để dâng (luôn tiêu hao)        │
│ Không mua bán. Không có gì để dâng → tự động Thua                       │
└──────────────┬──────────────────────────────────────────────────────────┘
               ▼
┌─ RÈN & TRẬN (cutscene ≤ 9 s ở 1×, không tua, theo timescale) ───────────┐
│ Cận thần → thợ rèn rèn trang bị → dũng sĩ nhận → lộ đối thủ → Thắng/Thua│
└──────────────┬──────────────────────────────────────────────────────────┘
       Thắng ──┤── Thua: Uy tín −1 (về 0 → TRỤC XUẤT, hết run)
               ▼
   CHIẾN LỢI PHẨM (vàng tặng thẳng + chọn 1/3, trừ Mùa 8) → Chiếu chỉ Mùa sau | KẾT THÚC RUN sau Mùa 8
```

### Ba vòng lặp

| Vòng | Thời lượng | Người chơi làm gì | Phần thưởng / phản hồi |
|---|---|---|---|
| Micro | ~30–60 giây (một lượt nung) | Chọn nguyên liệu, xếp lên lưới theo nhịp → Nung → xem/tua phát lại | Thành phẩm mới (khoảnh khắc "ra rồi!"), hoặc phế phẩm = dữ kiện mới cho suy luận (người chơi tự nhớ / tự ghi) |
| Meso | ~4–5 phút (một Mùa) | Đọc Chiếu chỉ → đặt mục tiêu (khoáng đủ mạnh, hợp loại trang bị) → 3 lượt Bàn luyện (mua/bán/nung) → soi & chọn khoáng dâng → Trận | Thắng trận + chiến lợi phẩm (Xúc tác, nới Túi, khoáng hiếm); hoặc mất Uy tín |
| Macro | 35–40 phút (một run 8 Mùa; chấp nhận 30–45) | Leo chuỗi tầng công thức, xây bộ Xúc tác, giữ Uy tín qua 8 trận; với tay: nhánh ma thuật → Khoáng Tối thượng | Thắng run → mở bậc khó kế; Danh vọng → mở bộ khởi đầu / nguyên liệu Chợ / Xúc tác mới; Sổ công thức dày thêm. **Điểm dừng tự nhiên**: cuối mỗi Mùa và cuối run. **Lý do quay lại**: công thức chưa giải, bậc khó mới, Khoáng Tối thượng. |

```
Xếp nguyên liệu ─▶ Nung ─▶ Thành phẩm / Phế phẩm ─▶ (Bán ▸ Mua) ─▶ Xếp tầng cao hơn ─┐
      ▲                          │ thành phẩm mới → Sổ công thức                       │
      └──────────────────────────┴─────────────────────────────────────────────────┘
   ×3 lượt ─▶ Dâng khoáng tốt nhất ─▶ Rèn (loại đã biết) ─▶ Trận ─▶ Chiến lợi phẩm ─▶ Mùa sau
   ×8 Mùa  ─▶ Kết thúc run ─▶ Danh vọng + Sổ công thức ─▶ Run mới (bộ khởi đầu / bậc khó mới)
```

**Tiến trình (progression)**
- *Trong run*: vàng (từ bán + phần thưởng trận thắng), Túi lớn dần (Nới Túi), bộ Xúc tác (≤ 5), leo tier qua **chuỗi tầng** (thành
  phẩm làm nguyên liệu cho công thức sau; Chợ chỉ bán tier 1–3), nhánh ma thuật dẫn tới tier 10.
- *Giữa các run (meta)*: Sổ công thức (nguyên liệu + thứ tự, **không** có khoảng cách), Danh vọng mở **lựa
  chọn** (bộ khởi đầu, nguyên liệu vào bể Chợ, Xúc tác mới), thắng run mở bậc khó kế. Không tăng chỉ số.

**Điều kiện thắng / thua**
- **Thua run (trục xuất)**: Uy tín (khởi đầu 3) về 0 — mỗi trận Thua −1.
- **Thắng run**: hoàn thành Mùa 8 còn ≥ 1 Uy tín (kể cả thua Đại chiến Mùa 8).
- Run kết thúc (thắng hay thua) luôn quy đổi Danh vọng. Khoáng Tối thượng là mục tiêu xa, không bắt buộc.

**Onboarding** (đề xuất — bậc khó 1, bộ khởi đầu đầu tiên; chỉnh bằng playtest):

| Thời điểm | Dạy gì | Cách dạy (bằng chơi, ít chữ) |
|---|---|---|
| Mùa 1, lượt 1 | Kéo/chạm đặt nguyên liệu, Nung, phát lại theo nhịp | Bộ khởi đầu ghi sẵn nhóm nhập môn `A → B`; xếp liền A, B ra thành phẩm thứ nhất; Túi đủ nguyên liệu |
| Mùa 1, lượt 2–3 | Chợ: bán để có vàng, mua để nung tiếp; Bỏ qua lượt | Túi khởi đầu không đủ cho lượt 3 nếu không bán |
| Mùa 1, cuối | Dâng → Rèn → Trận | Đối thủ ★1 gần như chắc thắng với khoáng nhập môn |
| Mùa 2 | **Khoảng cách là dữ liệu ẩn**: cùng nhóm, nhịp khác → kết quả khác; phế phẩm; Sổ công thức | Chiếu chỉ Mùa 2 yêu cầu loại trang bị hợp với thành phẩm thứ hai của nhóm `A → B` (cách 1 nhịp); một dòng gợi ý duy nhất "Cùng nguyên liệu, nhịp khác — kết quả khác"; Sổ công thức hiện nhóm "A → B" có 2 thành phẩm (cùng chuỗi, khác kết quả) |
| Mùa 2–3 | Thuộc tính ẩn + Kính soi; loại trang bị ở Chiếu chỉ | Chiếu chỉ Mùa 3 yêu cầu loại trang bị cần thuộc tính cụ thể; Chợ có Kính soi giá rẻ |
| Mùa 3–4 | Phụ gia & Kích hoạt ma thuật; Xúc tác | Chiến lợi phẩm Mùa 2–3 đảm bảo có 1 Xúc tác (run-flow.md §Chiến lợi phẩm); phụ gia tier 1 xuất hiện ở Chợ |

## 4. Systems Index

| # | Hệ thống | Loại | Ưu tiên | Trạng thái | Tài liệu | Phụ thuộc |
|---|---|---|---|---|---|---|
| 1 | Seed & RNG streams | Core | MVP | In Review | §6.1 | — |
| 2 | Lò nung: lưới, chuỗi nạp, khớp công thức, thành phẩm, `Quality` | Core | MVP | In Review | [design/brewing.md](design/brewing.md) | 1 |
| 3 | Kích hoạt ma thuật | Core | MVP | In Review | [design/brewing.md](design/brewing.md) §Ma thuật | 1, 2 |
| 4 | Nhật ký lượt nung & phát lại (timescale, tua) | Core / UI | MVP | In Review | [design/brewing.md](design/brewing.md) §BrewLog | 2 |
| 5 | Túi | Core | MVP | In Review | [design/economy.md](design/economy.md) §Túi | — |
| 6 | Chợ & kinh tế | Economy | MVP | In Review | [design/economy.md](design/economy.md) §Chợ | 1, 5 |
| 7 | Xúc tác & Effect primitive | Gameplay | MVP | In Review | [design/economy.md](design/economy.md) §Xúc tác | 2, 5 |
| 8 | Thuộc tính ẩn & Kính soi | Gameplay | MVP | In Review | §6.2 | 1, 2, 5 |
| 9 | Vòng Mùa (Chiếu chỉ → Bàn luyện → Dâng → Rèn & Trận → Chiến lợi phẩm) | Core | MVP | In Review | [design/run-flow.md](design/run-flow.md) | 1–8 |
| 10 | Rèn, Trận, Uy tín | Gameplay | MVP | In Review | [design/run-flow.md](design/run-flow.md) §Rèn & Trận | 8, 9 |
| 11 | Chiến lợi phẩm & modifier Mùa | Gameplay | MVP | In Review | [design/run-flow.md](design/run-flow.md) §Chiến lợi phẩm | 1, 9 |
| 12 | Meta: Sổ công thức, Danh vọng, bậc khó, bộ khởi đầu | Progression / Meta | MVP | In Review | §6.3 | 2, 9 |
| 13 | Lưu & tiếp tục (dừng là dừng) | Persistence | MVP | In Review | §6.4 | 1, 9 |
| 14 | UI/UX tổng thể | UI | MVP | In Review | §7 | 2–12 |
| 15 | Audio | Audio | Vertical Slice | Not Started | §8 | 4, 10 |
| 16 | Thuốc thử (Reagent) | Gameplay | Full Vision | Not Started | — | 7, 8 |
| 17 | Trận — mô phỏng tất định theo lượt + phát lại 6 nhịp (thay luật so `Power`) | Gameplay | MVP | In Review | [design/battle.md](design/battle.md) | 8, 10 |
| 17 | Endless, modifier đổi luật | Meta | Full Vision | Not Started | — | 9, 11 |

**Thứ tự thiết kế** (theo phụ thuộc): Foundation (1, 13) → Core (2, 3, 5, 9) → Feature (4, 6, 7, 8, 10, 11, 12) →
Presentation (14, 15) → Polish.

**Hệ thống rủi ro cao** (cần prototype sớm):
- **Lò nung + khoảng cách bí mật** — rủi ro *trải nghiệm*: giải mã có thú vị hay chỉ là đoán mò tốn tài
  nguyên? → prototype greybox 1 màn Bàn luyện, 6–8 công thức, không kinh tế.
- **Khám phá công thức mù** (quyết định 2026-11-03: không có nguồn gợi ý ngoài bộ khởi đầu) — rủi ro *trải
  nghiệm*: ngoài các công thức bộ khởi đầu cấp, người chơi phải đoán cả nguyên liệu, thứ tự **và** khoảng
  cách. Không gian thử có thể quá lớn → người chơi chỉ chơi lại vài công thức đã biết. → Prototype M0 phải đo
  (xem §6.3 Open Questions). Nếu thất bại, phương án dự phòng: Chiếu chỉ ban gợi ý / Mảnh công thức.
- **Kinh tế 0 vàng khởi đầu** — rủi ro *kẹt/áp đảo*: có vòng lặp bán–mua sinh lời vô hạn hoặc kẹt cứng? →
  mô phỏng RandomAgent/GreedyAgent trước khi làm UI.

**Phụ thuộc vòng**: không có. Xúc tác (7) tác động lên Lò (2) qua Effect primitive, Lò không biết Xúc tác cụ thể.

## 5. Thế giới, nhân vật, nội dung

- **Bối cảnh**: một vương quốc thời trung cổ giả tưởng; Nhà vua cần trang bị cho binh lính thi đấu và chinh
  chiến. Người chơi là nhà giả kim của triều đình — mỗi Mùa nhận Chiếu chỉ, dâng khoáng thạch; làm Vua thất
  vọng 3 lần → bị trục xuất / xử tử.
- **Chủ đề vật phẩm**: khoáng sản, khoáng vật, kim loại và phi kim có giá trị. Fiction (nóng chảy, thép/gang,
  `MeltTicks`) chỉ để dễ hình dung; luật không mô phỏng.
- **Loại vật phẩm** (`ItemDef.Kind`): Khoáng (Mineral, tier 1–10), Phụ gia (Additive, tier 1–10), Kính soi
  (Lens), Xúc tác (Catalyst), Thuốc thử (Reagent — sau MVP). Phế phẩm là một def chung.
- **Đối thủ**: 8 đối thủ cố định mỗi bậc khó; loại trận Thi đấu / Chiến tranh / Đại chiến (Mùa 8).
- **Trang bị**: Kiếm, Giáo, Giáp, Khiên (MVP) — khác nhau ở trọng số thuộc tính.
- **Danh sách nội dung MVP** (số lượng chốt ở `balance`):

  | Nội dung | Số lượng | Ghi chú |
  |---|---|---|
  | Khoáng gốc (Chợ) | TBD | tier 1–3, không có `Recipe` |
  | Khoáng thành phẩm | TBD | tier 1–10, mỗi cái một `Recipe`; ≥ 1 chuỗi tới tier 10 |
  | Phụ gia | TBD | tier 1–3 ở Chợ; tier cao từ chiến lợi phẩm/nung |
  | Kính soi | 2 cấp | `RevealLines` 1 / 3 |
  | Xúc tác | TBD | |
  | Thuộc tính | 5 | `Quality`, `Magic`, Phản đòn, Hấp thụ, Sắc bén |
  | Đối thủ | 3 × 8 | |
  | Bộ khởi đầu | 3 | |
  | Modifier Mùa | TBD | chỉ đổi số |

## 6. Thiết kế hệ thống

> Hệ thống lớn: [design/brewing.md](design/brewing.md) (Lò nung, ma thuật, phát lại) ·
> [design/economy.md](design/economy.md) (Túi, Chợ, Xúc tác) · [design/run-flow.md](design/run-flow.md)
> (vòng Mùa, Rèn & Trận, Uy tín, chiến lợi phẩm). Dưới đây là các hệ thống nhỏ/vừa.

### 6.1 Seed & RNG streams

> Trạng thái: In Review · Pillar: P2 · Ưu tiên: MVP · Lớp: Foundation

**Overview** — Mọi ngẫu nhiên trong run sinh từ một seed, tách thành các stream độc lập theo hệ thống để thay
đổi một hệ thống không làm lệch hệ thống khác; cùng seed + cùng chuỗi hành động → cùng kết quả.

**Detailed Rules**
- Run có `seed` (số nguyên 64-bit) **sinh tự động** lúc tạo run. MVP: người chơi **không** nhập seed (chốt
  2026-11-03). Seed vẫn lưu trong save để tái lập, test và debug.
- 7 stream: `Shop` (hàng Chợ), `Chance` (ma thuật + thuộc tính thành phẩm), `Inspect` (dòng được lộ),
  `Forge` (loại trang bị — **rút ở Chiếu chỉ**), `Market` (hệ số giá Mùa), `Season` (modifier), `Reward`
  (chiến lợi phẩm).
- Không hệ thống nào dùng random ngoài stream của mình. Công thức **không** dùng random.
- Một hành động bị từ chối (không hợp lệ) không được tiêu thụ random.

**Edge Cases** — Seed 0 hợp lệ. Reroll Chợ tiêu thụ `Shop` → cùng seed nhưng reroll khác số lần → hàng khác
(chấp nhận). Mọi điều kiện `Chance` của một Effect luôn được tung (không dừng sớm) để số lượt rút không phụ
thuộc kết quả.

**Dependencies** — Cung cấp random cho 2, 3, 6, 8, 9, 10, 11. Seed lưu trong save (§6.4).

**Acceptance Criteria**
- [ ] AC-1: Cùng seed + cùng chuỗi hành động → cùng state hash sau mỗi hành động (1.000 lần lặp; Editor, Standalone, WebGL).
- [ ] AC-2: Cùng seed → cùng hàng Chợ lượt 1, cùng hệ số giá Mùa 1, cùng loại trang bị Mùa 1 trên cả 3 nền tảng.
- [ ] AC-3: Thêm 1 lần reroll Chợ không đổi loại trang bị, chiến lợi phẩm, hay ma thuật của cùng lượt nung.

**Open Questions** — ~~Nhập seed ở MVP?~~ Chốt 2026-11-03: không. Sau MVP: cân nhắc hiện/chia sẻ seed ở màn
kết thúc run và cho nhập seed ("run thử thách").

### 6.2 Thuộc tính ẩn & Kính soi

> Trạng thái: In Review · Pillar: P2, P3 · Ưu tiên: MVP · Lớp: Feature

**Overview** — Mỗi thành phẩm đạt chuẩn có các *dòng thuộc tính* ẩn quyết định sức mạnh trang bị. Người chơi
chỉ thấy dòng nào đã soi bằng Kính soi (vật phẩm tiêu hao). Vì loại trang bị được công bố ở Chiếu chỉ, soi giúp
chọn đúng khoáng để dâng → Kính soi là một quyết định kinh tế có ý nghĩa.

**Player Fantasy** — Soi đá dưới kính lúp, phát hiện viên "sắc bén" đúng thứ Vua cần cho Kiếm.

**Detailed Rules**
- Thuộc tính MVP: `Quality` (luôn có, = Essence × Potency — xem brewing), `Magic` (có/không), Phản đòn, Hấp
  thụ, Sắc bén. Danh sách là content.
- **`Magic` là thuộc tính công khai** (sửa 2026-11-04): không phải dòng ẩn — thấy ngay khi thành phẩm ra lò qua
  hiệu ứng ma thuật trên sprite (cùng loại khoáng, bề ngoài giống nhau, chỉ khác hiệu ứng) + nhãn trong thẻ vật
  phẩm. Kính soi **không** tính `Magic`; "dòng ẩn" chỉ gồm `Quality` và các thuộc tính phụ (Phản đòn, Hấp thụ,
  Sắc bén...).
- Dòng thuộc tính sinh lúc tạo thành phẩm (stream `Chance`) theo dòng khởi đầu của công thức; Xúc tác sửa qua
  Effect (`AddStat`); phụ gia chỉ tác động `Magic`.
- Thuộc tính **không** ảnh hưởng giá bán.
- Mặc định không lộ dòng nào. Mỗi thành phẩm (instance) lưu tập dòng đã lộ, giữ suốt run; tiêu hao thì mất.
- Hai loại Kính: **Kính thường** (`RevealLines` 1) và **Kính tốt** (`RevealLines` 3).
- Dùng Kính soi lên 1 thành phẩm: nếu số dòng chưa lộ ≤ `RevealLines` → lộ hết; ngược lại lộ ngẫu nhiên
  `RevealLines` dòng **trong các dòng chưa lộ** (stream `Inspect`). Kính bị tiêu hao.
- Không cho soi: phế phẩm; thành phẩm đã lộ hết (không tiêu hao Kính).
- Soi được ở Bàn luyện (khi không đang phát lại lượt nung) và ở Dâng.
- Thành phẩm không gộp chồng; mỗi instance chiếm 1 chỗ Túi.

**Edge Cases** — Thành phẩm chỉ có `Quality` (1 dòng) → Kính thường lộ hết. Dùng Kính tốt lên thành phẩm còn
1 dòng chưa lộ → lộ 1, vẫn tiêu hao Kính. Soi thành phẩm đã lộ hết → bị từ chối (không tiêu hao) — người chơi
suy ra "không còn dòng ẩn" mà không tốn Kính; chấp nhận vì họ đã trả Kính để lộ các dòng còn lại.

**Dependencies** — Cần Lò (2) tạo dòng thuộc tính, Túi (5), RNG `Inspect`. Cung cấp tập dòng đã lộ cho UI; Rèn
(10) đọc **toàn bộ** dòng (kể cả chưa lộ).

**Tuning Knobs**

| Knob | Loại | Mặc định | Khoảng an toàn | Lý do | Nơi lưu |
|---|---|---|---|---|---|
| `RevealLines` Kính thường / Kính tốt | gate | 1 / 3 | 1–5 | Kính rẻ lộ ít, đắt lộ gần hết | `ItemDef` (Lens) |
| Giá Kính | curve | TBD | — | Cân với giá khoáng tier 1–2 | `ItemDef.MarketPrice` |

**UI Requirements** — Chỉ hiện các dòng đã lộ. **Không** hiện số dòng còn ẩn, cũng không có ký hiệu "còn dòng
chưa soi" — số/chỉ báo dòng ẩn lộ việc thành phẩm có thuộc tính phụ hay không, đúng thứ Kính soi bán cho người
chơi. `Magic` luôn hiển thị (hiệu ứng sprite + nhãn), không qua Kính soi. **Ngoại lệ có chủ đích** (2026-11-04):
biểu cảm thợ rèn trong cutscene lộ `Quality` của khoáng **đã dâng** ở 3 mức định tính — chỉ sau khi dâng (không
đổi được quyết định Mùa đó), không phản ánh đối thủ / kết quả trận. Thuộc tính ẩn còn lộ khi kích hoạt trong Trận (battle.md) — cũng chỉ sau khi dâng.

**Acceptance Criteria**
- [ ] AC-1: Thành phẩm có 4 dòng, Kính `RevealLines` 1 → lộ đúng 1 dòng chưa lộ; dùng lần 2 → lộ dòng khác.
- [ ] AC-2: Thành phẩm đã lộ hết → lệnh soi bị từ chối, Kính không bị tiêu hao.
- [ ] AC-3: Bán 2 thành phẩm cùng loại khác thuộc tính → cùng giá.
- [ ] AC-4: Không thông tin dòng chưa lộ nào xuất hiện ở UI/log release.
- [ ] AC-5: Thành phẩm kích hoạt ma thuật → hiệu ứng ma thuật hiện ngay khi ra lò, không cần soi; Kính soi lên
  thành phẩm đó không lộ/tiêu tốn lượt cho `Magic`.

**Open Questions** — Không còn (chốt 2026-11-03: không hiện số dòng / chỉ báo dòng ẩn).

### 6.3 Meta: Sổ công thức, Danh vọng, bậc khó, bộ khởi đầu

> Trạng thái: In Review · Pillar: P1, P2 · Ưu tiên: MVP · Lớp: Feature

**Overview** — Thứ giữ lại giữa các run: Sổ công thức (theo **nhóm công thức**, không có khoảng cách), Danh vọng
(XP meta mở thêm lựa chọn), bậc khó đã mở. (Sổ thí nghiệm đã **bỏ** 2026-11-04 — lượt nung hỏng không được ghi
lại; người chơi tự nhớ / tự ghi.)
**Khoảng cách là dữ liệu ẩn duy nhất người chơi phải tự kiểm nghiệm và tự ghi nhớ** — không màn hình, sổ,
tooltip hay save nào hiển thị lại khoảng cách cho người chơi (P2).

**Detailed Rules**
- **Nhóm công thức** (`RecipeFamily`, xem brewing.md): các công thức có cùng chuỗi nguyên liệu theo thứ tự,
  khác nhau ở khoảng cách → ra thành phẩm khác nhau. Một nhóm có 1..N biến thể.
- **Mở khóa theo từng khoáng**: hai cờ `ItemUnlocked`, `RecipeUnlocked`. Nung ra → mở cả hai. Mua ở Chợ / nhận
  từ chiến lợi phẩm → chỉ `ItemUnlocked`.
- **Nguồn biết nhóm công thức** (chốt 2026-11-03): chỉ có (a) bộ khởi đầu cấp sẵn (`StarterKitDef.KnownFamilies`
  — cho biết nguyên liệu + thứ tự của nhóm, **không** cho biết nhóm có những thành phẩm nào hay khoảng cách nào)
  và (b) tự nung ra. **Không** có Chiếu chỉ gợi ý hay vật phẩm lộ công thức.
- **Sổ công thức** (meta, vĩnh viễn — sửa 2026-11-04, spec `design/ui-meta.md` §I): **mỗi khoáng một mục**, liệt kê
  **mọi** khoáng trong content (nguyên liệu gốc + thành phẩm), gom theo nhóm công thức, sắp theo tier, **vị trí
  cố định**. Khoáng chưa mở → mọi trường "???" (người chơi thấy được tổng số khoáng và số biến thể của mỗi nhóm —
  chấp nhận có chủ đích). Mục đã mở gồm: tên, icon, thời điểm mở, công thức (chuỗi nguyên liệu theo thứ tự, **chỉ
  khi đã nung ra**, không khoảng cách), thuộc tính có thể nhận (đã khám phá → tên; còn chưa khám phá → một ô ???).
  Header nhóm hiện chuỗi nguyên liệu khi nhóm đã biết (bộ khởi đầu cấp hoặc đã nung ra một biến thể). Không ghi
  khoảng cách, phụ gia.
- **Thuộc tính đã khám phá** (meta, mới): mỗi khoáng lưu tập thuộc tính phụ người chơi đã soi ra trên một viên cùng
  loại (trong `ItemDef.PossibleStats`).
- **Không có Sổ thí nghiệm** (bỏ 2026-11-04, quyết định người dùng): game không ghi lại lượt nung hỏng hay chuỗi
  đã thử. Dữ kiện "cùng chuỗi, khác nhịp → khác kết quả" đến từ Sổ công thức (một nhóm liệt kê nhiều thành phẩm)
  và trí nhớ / ghi chép của người chơi.
- **Danh vọng**: quy đổi cuối run (thắng hay thua):
  `Renown = (Thắng × 10 + TierCaoNhất × 5 + (thắng run ? 30 : 0)) × HệSốBậcKhó%` (làm tròn xuống).
  Tự mở khi đạt mốc (không tiêu Danh vọng): mốc tạm 40 / 80 / 100 / 120 / 150 → bộ khởi đầu, nguyên liệu mới
  vào bể Chợ, Xúc tác mới. Mở khóa chỉ thêm lựa chọn, không tăng chỉ số.
- **Bậc khó**: 3 bậc; mỗi bậc 8 đối thủ riêng + hệ số Danh vọng. Bậc 1 mở sẵn; thắng run ở bậc i mở bậc i+1.
- **Bộ khởi đầu**: 3 bộ, chọn ở panel Run mới. Bộ 1 mở sẵn, bộ khác mở theo mốc Danh vọng. Mỗi bộ gồm nguyên
  liệu khởi đầu + 1–3 nhóm công thức ghi sẵn vào Sổ. Bộ 1 cấp nhóm nhập môn `A → B` có ≥ 2 biến thể: xếp liền
  (không nhịp chờ) ra thành phẩm thứ nhất, cách 1 nhịp ra thành phẩm thứ hai (onboarding §3).
- Vàng, Túi, Xúc tác, Uy tín **không** vào meta.

**Formulas**

| Tên | Công thức | Biến (khoảng) | Ví dụ |
|---|---|---|---|
| Danh vọng | ⌊(W×10 + T×5 + V×30) × D / 100⌋ | W thắng 0–8, T tier cao nhất 1–10, V 0/1, D hệ số bậc % 100–200 | W=6, T=5, V=1, D=100 → 60+25+30 = 115 |

**Edge Cases**
- Thua ngay Mùa 3 không thắng trận nào, tier 2 → 10 × D% Danh vọng (vẫn > 0, khuyến khích thử).
- Mua khoáng tier 3 → không mở công thức. Nung thành phẩm đã có trong Sổ → Sổ không đổi.
- Người chơi tự ghi khoảng cách ra ngoài game / cộng đồng chia sẻ → chấp nhận (là "tự kiểm nghiệm").

**Dependencies** — Đọc sự kiện nung (2), mua/nhận (6, 11), kết quả run (9). Lưu file meta riêng (§6.4).

**Tuning Knobs**

| Knob | Loại | Mặc định | Khoảng | Nơi lưu |
|---|---|---|---|---|
| Hệ số Danh vọng bậc 1/2/3 | curve | 100 / 150 / 200 % | 100–300 | `DifficultyDef` |
| Mốc mở khóa | gate | 40/80/100/120/150 | — | `UnlockRenown` trên từng def |

**UI Requirements**
- Sổ công thức: xem `design/ui-meta.md` §I — chuỗi icon nguyên liệu **cách đều nhau** (không gợi ý nhịp); mục
  ??? và bộ đếm "Đã mở x/N" được phép (sửa 2026-11-04); thứ tự biến thể trong nhóm không suy ra từ khoảng cách.
- Sổ công thức mở được trong Bàn luyện (trừ lúc phát lại) và từ Menu chính.

**Acceptance Criteria**
- [ ] AC-1: Nung ra khoáng X lần đầu → Sổ công thức có mục nhóm của X (đúng chuỗi nguyên liệu) với X trong danh sách thành phẩm; không có dữ liệu khoảng cách.
- [ ] AC-1b: Nung ra 2 biến thể khác khoảng cách của cùng nhóm → cả 2 mục trong cùng nhóm có cùng chuỗi công thức; vị trí các mục không đổi.
- [ ] AC-1c: Soi ra thuộc tính S trên một viên khoáng X → Sổ mục X hiện S; còn thuộc tính chưa khám phá → đúng 1 ô ???.
- [ ] AC-2: Mua khoáng Y → `ItemUnlocked(Y)` = true, `RecipeUnlocked(Y)` = false.
- [ ] AC-3: Lượt nung ra phế phẩm → meta không đổi (không có nhật ký lượt nung).
- [ ] AC-3b: Kết thúc run (thắng/thua) → Sổ công thức còn nguyên ở run mới.
- [ ] AC-3d: Dữ liệu meta đã lưu không chứa vị trí ô hay khoảng cách (kiểm bằng test đọc file save).
- [ ] AC-4: Thắng run bậc 1 → bậc 2 mở; Danh vọng tính đúng ví dụ 115.
- [ ] AC-5 (trải nghiệm): ≥ 70% người playtest mở Sổ công thức ít nhất 1 lần trong run đầu; ghi nhận bao nhiêu người tự ghi chép ngoài game (đo gánh nhớ sau khi bỏ Sổ thí nghiệm).
- [ ] AC-6: Chọn bộ khởi đầu 1 → Sổ có đúng các nhóm `KnownFamilies` của bộ (chuỗi nguyên liệu, danh sách thành phẩm trống).
- [ ] AC-7 (trải nghiệm, M0): ≥ 50% người chơi tự khám phá ≥ 1 thành phẩm **ngoài** biến thể nhập môn trong 2 run đầu; nếu không đạt → kích hoạt phương án gợi ý dự phòng.
- [ ] AC-8 (trải nghiệm, M0): ≥ 70% người chơi, sau Mùa 2, phát biểu được "cùng nguyên liệu, nhịp khác → kết quả khác".

**Open Questions**
- Khám phá công thức mù có đủ vui không (rủi ro cao, xem §4)? Đo bằng AC-7, AC-8 ở M0.
- (Bỏ Sổ thí nghiệm 2026-11-04 → gánh nhớ tăng, AC-7/AC-8 rủi ro hơn; xem lại sau M0.) Có cần một "ghi chú tay" trong Sổ công thức để người chơi tự ghi nhịp không? (Do người chơi tự viết → không
  vi phạm "game không hiển thị khoảng cách"; nhưng làm giảm thử thách ghi nhớ.) Đề xuất: không ở MVP, xem lại
  sau playtest.

### 6.4 Lưu & tiếp tục

> Trạng thái: In Review · Pillar: P4 · Ưu tiên: MVP · Lớp: Foundation

**Overview** — "Dừng là dừng": người chơi tắt game hay đóng tab ở bất kỳ đâu, mở lại tiếp tục đúng chỗ đó.
Có hai vùng lưu tách biệt: **save run** (một run dở, xóa khi run kết thúc) và **meta** (vĩnh viễn). Cách cài đặt
(format, replay, snapshot, storage WebGL) thuộc `TECH.md` — mục này chỉ định nghĩa hành vi người chơi thấy.

**Detailed Rules**
- Lưu sau **mỗi** hành động hợp lệ. Tắt game/đóng tab ở bất kỳ màn nào → mở lại vào đúng trạng thái đó.
- Đóng tab giữa lúc đang phát lại lượt nung → mở lại thấy kết quả lượt nung (lượt đã được tính ngay khi Nung).
- **Save run gồm**: seed, bậc khó, bộ khởi đầu, toàn bộ trạng thái run (Mùa, lượt, Túi, lưới, vàng, Uy tín, hàng
  Chợ hiện tại, Chiếu chỉ đã rút, dòng thuộc tính đã lộ).
- **Meta gồm**: Sổ công thức + cờ mở khóa (kèm thời điểm mở), thuộc tính đã khám phá theo khoáng, Danh vọng, bậc khó đã mở, bộ khởi đầu đã mở, timescale ưa thích.
- Có run dở → menu chính hiện "Tiếp tục"; run kết thúc → xóa save run (meta giữ nguyên).
- Không có "save scum": không có nút tải lại lượt trước.

**Edge Cases**
- **Save run hỏng / không đọc được** → báo "Không thể tiếp tục run", xóa save run, giữ meta; không crash.
- **Save run của phiên bản game cũ** → migrate nếu có đường migrate; không có → như save hỏng (meta luôn phải
  migrate được).
- **Meta hỏng** → giữ bản backup tốt gần nhất; không có backup → bắt đầu meta mới và báo người chơi.
- **Đóng tab đúng lúc đang ghi** → lần mở sau đọc được bản trước hoặc bản sau, không bao giờ nửa vời.
- **Hai tab cùng mở game** → TBD (TECH): tab sau chỉ đọc, hoặc cảnh báo.

**Dependencies** — Cần Seed & RNG (§6.1, tái lập), Vòng Mùa (điểm lưu sau mỗi chuyển trạng thái), Meta (§6.3).
Cung cấp "Tiếp tục" cho menu chính (§7).

**Acceptance Criteria**
- [ ] AC-1: Đóng tab trình duyệt ở mỗi màn (Chiếu chỉ, Bàn luyện đang xếp, đang phát lại, Dâng, Chiến lợi phẩm) → mở lại đúng trạng thái, cùng state hash.
- [ ] AC-2: Run kết thúc → không còn nút "Tiếp tục"; meta còn nguyên.
- [ ] AC-3: Đóng tab ngay sau Nung ra thành phẩm mới → mở lại → Sổ công thức đã có thành phẩm đó.
- [ ] AC-4: Save run bị sửa hỏng bằng tay → game mở được, báo lỗi, meta không mất.

## 7. UI / UX tổng thể

**Luồng màn hình**
```
Menu chính ─▶ Run mới (chọn bậc khó, bộ khởi đầu) ─▶ [Mùa ×8: Chiếu chỉ ▸ Bàn luyện ×3 ▸ Dâng ▸
Rèn & Trận ▸ Chiến lợi phẩm] ─▶ Kết thúc run (Thắng / Trục xuất + Danh vọng) ─▶ Menu chính
Menu chính: Run mới · Tiếp tục (tắt nếu không có run dở) · Sổ công thức · Cài đặt · Giới thiệu
(Run mới khi đang có run dở → xác nhận ghi đè; không có nút Thoát — WebGL)
```

**HUD cố định trong run**: Mùa x/8 · lượt nung y/3 · Uy tín (3 biểu tượng) · Vàng · nút Sổ công thức · loại
trang bị + thuộc tính ưa của Mùa này (thu gọn; **không** có đối thủ — lộ ở Rèn & Trận).

**Màn Bàn luyện** (màn chính, dùng ~80% thời gian): Lưới 3×3 ở giữa; Túi một bên; ngăn Chợ trượt ra từ
bên kia (mở sẵn đầu lượt, thu gọn được); nút Nung / Bỏ qua lượt; điều khiển timescale 1×/2×/3× và "chạm để
tua" khi đang phát lại.

**Nguyên tắc tương tác** (WebGL trước, Mobile sau): kéo-thả **và** chạm-chọn → chạm-ô; tooltip mở bằng chạm /
nhấn giữ, không hover; vùng chạm ≥ 64 px ở 1920×1080. Chi tiết: [design/ui-ban-luyen.md](design/ui-ban-luyen.md) (Bàn luyện), `Docs/ART.md` §7.

**Không rò rỉ**: không UI nào (tooltip, Sổ, thông báo lỗi, animation) để lộ khoảng cách công thức, ô sai, dòng
hay thuộc tính chưa soi (trừ hai ngoại lệ sau khi dâng: biểu cảm thợ rèn, thuộc tính kích hoạt trong Trận).

## 8. Audio

TBD — chạy `/unity-game-studio gdd audio` ở Vertical Slice. Sự kiện cần âm thanh (nháp): đặt nguyên liệu, nhịp
nạp từng tick, nhịp chờ, thành phẩm / phế phẩm (khác hẳn nhau), ma thuật kích hoạt (khi ra lò), mua/bán,
Chiếu chỉ, Rèn, Thắng/Thua trận, trục xuất.

## 9. Lịch sử thay đổi thiết kế

| Ngày | Thay đổi | Lý do | Hệ thống bị ảnh hưởng |
|---|---|---|---|
| 2026-11-03 | Tạo GDD từ `demo_GDD.md` (Crucible v2 — Run liên tục) | Chốt concept | Tất cả |
| 2026-11-03 | **Loại trang bị công bố ở Chiếu chỉ** (trước: rút sau khi Dâng) | P2/P3: người chơi thua vì quyết định, không vì RNG; Mùa có mục tiêu rõ; Kính soi có ý nghĩa | 1, 8, 9, 10 |
| 2026-11-03 | **Gộp phase**: Chợ + Lò → Bàn luyện; Xưởng → Dâng; Rèn + Trận một cảnh | P4: bớt 24 lần chuyển màn/run, hợp mobile sau này | 6, 9, 14 |
| 2026-11-03 | **Chạm để tua** phát lại lượt nung | P4: ~24 lượt × tới 27 s ở 1×; tua không lộ khoảng cách vì kết quả chỉ hiện sau nguyên liệu cuối | 4 |
| 2026-11-03 | **Sổ thí nghiệm** (ban đầu trong run → vĩnh viễn, xem dưới) | P1: giảm gánh nhớ, giữ suy luận; không thêm thông tin mới | 12 |
| 2026-11-03 | Nền tảng: WebGL trước, Mobile sau | Theo mẫu + mục tiêu mobile | 14 |
| 2026-11-03 | `gdd --review` → sửa: **lưới tính vào sức chứa Túi** (R1) | Gộp Chợ vào Bàn luyện làm tràn Túi khi mua sau khi xếp lưới | 2, 5, 6 |
| 2026-11-03 | Nguồn biết công thức: **chỉ bộ khởi đầu + tự nung** (R2) | Quyết định người dùng; đánh dấu rủi ro cao, đo ở M0 | 12 |
| 2026-11-03 | Không hiện số dòng thuộc tính còn ẩn (M1) | Đếm dòng sẽ lộ `Magic` | 8 |
| 2026-11-03 | `OnShopEnter` → `OnShopRefresh` (1 lần/lượt) (M2); bể chiến lợi phẩm theo Mùa có lựa chọn bắt buộc (M3); Sổ thí nghiệm vào save (M4) | Nhất quán sau gộp phase / onboarding | 7, 11, 13 |
| 2026-11-03 | **Sổ thí nghiệm vĩnh viễn** (meta), `LabLogCap` 500 | Quyết định người dùng | 12, 13 |
| 2026-11-03 | **Khoảng cách là dữ liệu ẩn tuyệt đối**: Sổ thí nghiệm không ghi lưới/khoảng cách (chỉ chuỗi nguyên liệu, tập phụ gia, kết quả); thêm **nhóm công thức** (`RecipeFamily`) — Sổ công thức mỗi nhóm một mục, liệt kê thành phẩm đã tìm, không lộ số biến thể; bộ khởi đầu cấp nhóm (`KnownFamilies`) | Người dùng: khoảng cách chỉ để người chơi tự kiểm nghiệm; thay thế "hệ quả có chủ ý" trước đó | 2, 12, 13 |
| 2026-11-03 | MVP không nhập seed (seed tự sinh, vẫn lưu) | Quyết định người dùng | 1, 9 |
| 2026-11-03 | Không hiện số nhịp 1–9 trên ô lưới | Quyết định người dùng; thứ tự quét dạy qua phát lại | 2, 14 |
| 2026-11-03 | Kính thường / Kính tốt; thời lượng run 35–40 (chấp nhận 30–45); §6.4 bổ sung mục | L1–L3 | 8, 13 |
| 2026-11-04 | Chợ **5 ô cố định**: 4 nguyên liệu gốc (được trùng) + 1 ô tiện ích (Kính soi / Xúc tác) | Q1–Q5 người dùng | 6 |
| 2026-11-04 | **Đối thủ ẩn ở Chiếu chỉ**, lộ trong cutscene sau khi dũng sĩ nhận trang bị; Chiếu chỉ = popup một câu | Q6 người dùng; đối thủ cố định theo `RunDef` → người chơi quen vẫn biết trước (P1) | 9, 10, 14 |
| 2026-11-04 | **Rèn & Trận = cutscene** ≤ 9 nhịp × 1 s ở 1×, **không tua**, chỉ timescale (tối đa 3× → ≤ 3 s); timeline chung, vật phẩm gắn động; P4 bổ sung ngoại lệ "≤ 3 s ở 3×" | Q7–Q8 người dùng | 10, 14 |
| 2026-11-04 | **Chiến lợi phẩm = vàng tặng thẳng + chọn 1/3** (trước: không có vàng) | Q9 người dùng | 6, 11 |
| 2026-11-04 | **Bỏ Sổ thí nghiệm**, chỉ giữ Sổ công thức | Q10 người dùng; gánh nhớ tăng — theo dõi AC-5/7/8 | 12, 13, 14 |
| 2026-11-04 | Menu chính: Run mới · Tiếp tục · Sổ công thức · Cài đặt · Giới thiệu | Q11 | 14 |
| 2026-11-04 | Nhánh ma thuật theo hướng A "tài nguyên có kế hoạch" — chỉnh trần/truyền thừa/`RequiresMagic`/giá Kính ở `balance` | Q12, hệ quả `Magic` công khai | 3, 8 |
| 2026-11-04 | **Trận = mô phỏng tất định theo lượt** (HP/ATK/SB/PD/HT, quy đổi theo loại trang bị) thay luật so `Power`; phát lại luôn 6 nhịp; thanh máu + số sát thương; thuộc tính ẩn lộ khi kích hoạt và ghi Sổ | Người dùng (`gdd battle`) | 8, 10, 12, 17 |
| 2026-11-04 | **Sổ công thức theo khoáng**: liệt kê mọi khoáng (??? khi chưa mở), gom theo nhóm, sắp theo tier, vị trí cố định, bộ đếm x/N; thêm thời điểm mở + thuộc tính đã khám phá vào meta | Người dùng (`ui`) | 12, 13, 14 |
| 2026-11-04 | Chiếu chỉ hiện tên vương quốc đối thủ (★/loại trận lộ ở Trận); cutscene Rèn 8 nhịp, thợ rèn có biểu cảm theo `Quality`; **Trận** là hệ thống combat riêng (TBD); Dâng = đặt lên khay cận thần | Người dùng (`ui`) | 8, 9, 10, 14 |
| 2026-11-04 | Chấp nhận có chủ đích: bỏ Sổ thí nghiệm (nuôi cộng đồng / nội dung ngoài game); vàng thưởng (vẫn khan hiếm vì Chợ ≤ tier 3); giữ `Magic` khó dần theo tier (công thức chốt ở `balance`) | Người dùng | 3, 6, 12 |
| 2026-11-04 | **`Magic` công khai** (trước: dòng ẩn chỉ thấy bằng Kính soi); thuộc tính phụ vẫn ẩn, vẫn không có chỉ báo dòng ẩn | Người dùng (khi làm `ui` Bàn luyện): ma thuật nhận ra được bằng hiệu ứng; còn giúp slot `RequiresMagic` công bằng (P2) | 3, 8, 14 |
