---
name: unity-game-studio
description: >-
  Bộ quy trình làm game Unity theo vai trò (Game Designer, Designer, Developer, QA/Tester, DevOps) với
  command riêng cho từng đầu việc: khởi tạo dự án, game design (GDD), art style (ART), thiết kế hệ thống,
  thiết kế màn chơi, xây dựng trong Unity, test & fix bug, optimize, build/release, ghi log. Tạo và duy trì
  bộ tài liệu dự án CLAUDE.md, Docs/GDD.md, ART.md, PLAN.md, TECH.md, TEST.md, LOG.md... Dùng khi người dùng
  gọi /unity-game-studio <command>, hoặc nhắc tới GDD, art bible, level design, game design, Unity project
  setup, test plan, bug, profiling, build Unity, CI cho dự án Unity.
---

# Unity Game Studio

Một "studio" thu gọn cho dự án Unity: 5 vai trò, mỗi đầu việc là một command, mọi kết quả được ghi vào
bộ tài liệu dự án để các phiên làm việc sau (và người khác) nối tiếp được.

Skill này chỉ dành cho **Unity (C#)**. Nếu dự án dùng engine khác, nói rõ và dừng.

## Cách gọi

```
/unity-game-studio <command> [tham số]
# ví dụ: init | gdd combat | plan inventory | bug "nhân vật kẹt tường" | test run EditMode
```

- Không có command → chạy `status` (xem dự án đang ở đâu, gợi ý bước tiếp theo).
- Command không khớp → liệt kê bảng command bên dưới và hỏi lại.
- Mỗi command có file hướng dẫn riêng ở `commands/<command>.md`. **Luôn đọc file đó trước khi làm.**
- Nếu dự án đã cài wrapper (xem `extras/README.md`), có thể gọi tắt `/ugs-<command>`.

## Bảng command theo vai trò

| Vai trò | Command | Đầu việc | Ghi vào |
|---|---|---|---|
| Chung | `init` | Khởi tạo bộ tài liệu, quét dự án Unity hiện có | `CLAUDE.md`, `Docs/*` |
| Chung | `status` | Dự án đang ở giai đoạn nào, việc tiếp theo | — (chỉ đọc) |
| Chung | `log` | Ghi log phiên làm việc, quyết định, changelog, gom lỗi từ Unity Console | `Docs/LOG.md`, `Docs/BUGS.md` |
| Game Designer | `concept` | Ý tưởng, pillars, core loop, phạm vi | `Docs/GDD.md` §1–3 |
| Game Designer | `gdd` | Thiết kế chi tiết một hệ thống gameplay | `Docs/GDD.md` hoặc `Docs/design/<system>.md` |
| Game Designer | `systems` | Liệt kê hệ thống, phụ thuộc, thứ tự thiết kế | `Docs/GDD.md` §Systems Index |
| Game Designer | `balance` | Công thức, đường cong, kinh tế, tìm chiến lược áp đảo | `Docs/GDD.md`, dữ liệu SO |
| Game Designer | `level` | Thiết kế màn chơi: layout, nhịp độ, encounter | `Docs/LEVELS.md` |
| Designer | `art` | Art bible: phong cách, màu, hình khối, chuẩn asset | `Docs/ART.md` |
| Designer | `asset` | Spec từng asset, prompt AI, import settings, manifest | `Docs/ASSETS.md` |
| Designer | `ui` | UX/UI spec cho màn hình, HUD, luồng menu | `Docs/ART.md` §UI, `Docs/GDD.md` §UI |
| Developer | `arch` | Thiết kế hệ thống kỹ thuật, quyết định kiến trúc (ADR) | `Docs/TECH.md` |
| Developer | `plan` | Kế hoạch triển khai một feature (test-first) hoặc roadmap | `Docs/PLAN.md`, `Docs/plans/<feature>.md` |
| Developer | `build` | Xây dựng feature trong Unity theo plan, test-first | `Assets/**`, `Docs/PLAN.md` |
| Developer | `scene` | Tạo/sửa scene, prefab, UI hierarchy qua editor script | `Assets/**` |
| Developer | `refactor` | Refactor an toàn, giữ nguyên hành vi | `Assets/**` |
| Developer | `review` | Code review theo chuẩn Unity của dự án | báo cáo |
| QA / Tester | `test` | Test plan, viết/chạy/sửa test EditMode & PlayMode | `Docs/TEST.md`, `Assets/Tests/**` |
| QA / Tester | `bug` | Báo bug → tái hiện bằng test → sửa → regression | `Docs/BUGS.md` |
| QA / Tester | `playtest` | Kịch bản & báo cáo playtest, chuyển thành việc cần làm | `Docs/TEST.md` §Playtest |
| Dev / DevOps | `optimize` | Đo profiler, tìm bottleneck, tối ưu có số liệu | `Docs/TECH.md` §Performance |
| DevOps | `setup` | Vệ sinh dự án: git, .gitignore, Force Text, asmdef, test asmdef | `Docs/BUILD.md` |
| DevOps | `ci` | Pipeline CI: test + build tự động | `.github/workflows/*`, `Docs/BUILD.md` |
| DevOps | `release` | Build player, checklist phát hành, changelog | `Docs/BUILD.md`, `Docs/LOG.md` |

Vai trò quyết định *cách nghĩ* khi làm command. Đọc `roles/<role>.md` tương ứng:
`game-designer.md`, `designer.md`, `developer.md`, `qa.md`, `devops.md`.

## Bộ tài liệu dự án

Tài liệu nằm ở **thư mục gốc dự án Unity** (cùng cấp `Assets/`), không đặt trong `Assets/` — mọi file
trong `Assets/` sẽ bị Unity import và sinh `.meta`.

| File | Mục đích | Chủ sở hữu | Tạo khi |
|---|---|---|---|
| `CLAUDE.md` | Mô tả dự án, môi trường Unity, quy ước, mục lục tài liệu, lệnh test/build | DevOps + Developer | `init` |
| `Docs/GDD.md` | Game Design: concept, pillars, core loop, systems index, thiết kế hệ thống | Game Designer | `init` |
| `Docs/ART.md` | Art bible: nhận diện hình ảnh, màu, shape, UI visual, chuẩn asset, prompt card | Designer | `init` |
| `Docs/PLAN.md` | Quy trình & roadmap: milestone, backlog, trạng thái feature, link tới plan chi tiết | Developer + GD | `init` |
| `Docs/TECH.md` | Kiến trúc kỹ thuật, quy ước code, ADR, ngân sách hiệu năng | Developer | `init` |
| `Docs/TEST.md` | Chiến lược test, test plan theo feature, smoke list, playtest | QA | `init` |
| `Docs/LOG.md` | Checkpoint phiên, nhật ký phiên, nhật ký quyết định, changelog | Tất cả | `init` |
| `Docs/LEVELS.md` | Thiết kế màn chơi | Game Designer | lần đầu chạy `level` |
| `Docs/ASSETS.md` | Danh mục & spec asset, nguồn gốc/license | Designer | lần đầu chạy `asset` |
| `Docs/BUGS.md` | Danh sách bug, mức độ, trạng thái, test tái hiện | QA | lần đầu chạy `bug` |
| `Docs/BUILD.md` | Cấu hình build, nền tảng, CI, quy trình release | DevOps | lần đầu chạy `setup`/`ci`/`release` |
| `Docs/plans/<feature>.md` | Plan chi tiết một feature (thiết kế class, test cases, bước làm) | Developer | `plan` |
| `Docs/design/<system>.md` | GDD riêng cho hệ thống lớn (khi GDD.md quá dài) | Game Designer | `gdd` |
| `Docs/evidence/` | Ảnh chụp màn hình bằng chứng cho UI/visual | QA | `test`, `build` |

Template nằm ở `templates/`. Khi tạo file mới, copy template rồi điền — không tự nghĩ cấu trúc khác.

### Giao thức tài liệu

1. **Đầu mỗi command**: đọc `CLAUDE.md` và khối `CHECKPOINT` trong `Docs/LOG.md`. Chỉ đọc thêm tài liệu mà
   command yêu cầu. Không nạp toàn bộ `Docs/` vào ngữ cảnh.
2. **Tài liệu là nguồn sự thật.** Code lệch tài liệu → hỏi người dùng bên nào đúng, rồi sửa bên sai.
3. **Cuối mỗi command có thay đổi**: cập nhật tài liệu liên quan (trạng thái trong `PLAN.md`, bug trong
   `BUGS.md`...) và ghi đè khối `CHECKPOINT` trong `LOG.md` (việc đang làm, bước tiếp theo, đang kẹt ở đâu).
4. Viết tài liệu thiết kế **theo từng mục**: trình bày bản nháp mục đó, người dùng duyệt, ghi ngay, sang mục
   kế. Không viết cả tài liệu dài một lần rồi mới hỏi.
5. Chỗ chưa quyết định ghi `TBD` kèm câu hỏi ở mục *Open Questions*. Mục đánh dấu `TBD` không được thiết kế
   hay code cho tới khi được chốt.

## Luật cốt lõi (áp dụng cho mọi command)

**Giao tiếp**
- Trả lời tiếng Việt, giữ thuật ngữ kỹ thuật tiếng Anh (MonoBehaviour, ScriptableObject, prefab, GC allocation...).
- Đi thẳng vào vấn đề. Nhiều cách tiếp cận → nêu ngắn gọn ưu/nhược rồi đề xuất một phương án.
- Không chắc về hành vi Unity hoặc API theo phiên bản → nói rõ, kiểm tra tài liệu Unity đúng phiên bản
  (xem `references/unity-coding-rules.md` §Tra cứu). Không suy đoán khi chưa đọc code.

**Trước khi sửa**
- Đọc code liên quan: chỗ gọi, prefab/scene tham chiếu, test hiện có, quy ước đặt tên. Theo pattern sẵn có.
- Mơ hồ → hỏi trước. Thay đổi lớn (kiến trúc, input, save/load, đổi public API) → đề xuất kế hoạch, chờ xác nhận.
- Thay đổi nhỏ, tập trung, một việc một commit. Vấn đề phát hiện ngoài phạm vi → ghi vào báo cáo, không sửa.

**Luật riêng Unity** (chi tiết: `references/unity-coding-rules.md`, `references/unity-assets-yaml.md`)
- Không sửa tay `.unity`, `.prefab`; dùng editor script (`commands/scene.md`). `.asset`/`.mat` chỉ sửa tay
  theo `references/unity-assets-yaml.md` và chỉ khi Editor không mở.
- Không tạo, sửa, xóa `.meta`. Đổi tên/di chuyển asset: qua Unity Editor hoặc `git mv` kèm `.meta`.
- Đổi tên field serialized → thêm `[FormerlySerializedAs("tenCu")]`.
- Không `GameObject.Find`, `FindObject*`, `GetComponent` trong `Update`/`FixedUpdate`/`LateUpdate` — cache ở `Awake`.
- Vòng lặp nóng: không string concat, LINQ, boxing, `new List<>` mỗi frame. Pool bằng `UnityEngine.Pool`.
- `UnityEngine.Object` đã destroy: dùng `== null`/bool của Unity, cẩn thận `?.`, `??`, `is null`.
- Event: đăng ký `OnEnable` → hủy `OnDisable`; đăng ký `Awake`/`Start` → hủy `OnDestroy`. Coroutine/async
  hủy theo vòng đời (`destroyCancellationToken`).
- Tách logic thuần C# khỏi MonoBehaviour để test EditMode. Giá trị gameplay để trong ScriptableObject/config.
- Không sửa `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, file sinh tự động, `.meta` của bên thứ ba.

**Test & hoàn thành** (chi tiết: `references/unity-testing.md`)
- Feature mới/sửa bug: viết test trước hoặc cùng lúc. Bug: test tái hiện phải **fail** trước khi sửa.
- Không xóa/tắt test đang fail để qua build. Test flaky → tìm nguyên nhân gốc, không tăng timeout/retry.
- **Không báo hoàn thành khi chưa có bằng chứng mới**: compile sạch + test liên quan chạy xanh (đọc kết quả
  thật). Thay đổi người chơi nhìn thấy (UI, visual, feel) → cần ảnh chụp lưu ở `Docs/evidence/` hoặc nêu rõ
  bước kiểm tra thủ công còn lại. Không chạy được test → nói rõ lý do.

**An toàn**
- Không thêm package UPM / Asset Store khi chưa hỏi. Không hard-code secret. Không `try/catch` nuốt lỗi.
- Không chạy lệnh phá hủy (xóa asset, reset project, force push) khi chưa xác nhận.
- **Commit**: chỉ tự commit khi `CLAUDE.md` §Git ghi `Commit tự động: có`; mặc định đề xuất commit message và để
  người dùng commit. Không commit khi test đang fail — trừ commit "test-first" (test mới cố ý fail) nếu `CLAUDE.md`
  ghi `Commit test-first: có`. Không push. Message: `<type>(<scope>): <mô tả>`.

## Luồng chạy một command

1. Đọc `CLAUDE.md` + `CHECKPOINT` của `Docs/LOG.md`. Chưa có `CLAUDE.md` → đề nghị chạy `init` trước
   (trừ khi command là `init`/`status`).
2. Đọc `commands/<command>.md` và `roles/<role>.md` của command đó.
3. Đọc các `references/*` mà command chỉ định — chỉ phần cần.
4. Thực hiện theo các bước trong command. Dùng task list khi có từ 3 bước trở lên.
5. Kiểm chứng theo "Định nghĩa hoàn thành" của command.
6. Cập nhật tài liệu + `CHECKPOINT`.
7. Báo cáo theo mẫu:

```
## Kết quả
- Đã làm: ...
- File thay đổi: ...
- Test: đã thêm ..., kết quả chạy ... (hoặc lý do không chạy được)
- Cần kiểm tra thủ công trong Editor: ... (prefab, animation, UI, feel)
- Rủi ro / việc còn lại: ...
- Bước tiếp theo đề xuất: /unity-game-studio <command> ...
```

## Điều khiển Unity Editor

Command nào cần compile, chạy test, chạy editor script, chụp màn hình thì đọc
`references/unity-automation.md` để chọn backend theo thứ tự: Unity MCP (nếu có) → Rider MCP (nếu có) →
Unity CLI batchmode (Editor phải đóng). Luật cứng: không gọi 2 lệnh Editor song song; lỗi `error`/`canceled`
thì chờ ~10 giây rồi thử lại, hai lần liên tiếp vẫn lỗi thì dừng và hỏi người dùng. Editor đang mở mà cần
batchmode → hỏi người dùng đóng Editor.

## Mức độ chặt (rigor)

Đặt trong `CLAUDE.md` mục `Rigor`. Mặc định `standard`.

| Rigor | Tài liệu bắt buộc | Test | Dùng khi |
|---|---|---|---|
| `lean` | GDD §1–3 + PLAN; GDD hệ thống tối giản (Rules + Acceptance) | Logic thuần C# + test tái hiện bug | Prototype, game jam |
| `standard` | GDD 5 mục cốt lõi (+Formulas nếu có số), ART §1–4, TECH | EditMode + PlayMode cho tích hợp; ảnh bằng chứng cho UI | Mặc định |
| `strict` | GDD đủ mục, ART đủ 9 mục, ADR cho mọi quyết định khó đảo ngược | Như standard + layout test, perf test, review trước merge | Sản xuất dài hạn, team |

Tài liệu nhiều hơn không làm game hay hơn — chọn mức thấp nhất đủ dùng, nâng dần khi dự án lớn lên.

## Tài nguyên

- `roles/` — cách nghĩ và trách nhiệm từng vai trò.
- `commands/` — quy trình từng command.
- `templates/` — mẫu tài liệu dự án.
- `references/unity-coding-rules.md` — chuẩn C#/Unity hiện đại theo phiên bản, lifecycle, async, serialization.
- `references/unity-testing.md` — thiết kế & viết test Unity Test Framework.
- `references/unity-automation.md` — điều khiển Editor, chạy test, batchmode, chụp màn hình.
- `references/unity-assets-yaml.md` — scene/prefab/meta/GUID, editor script, uGUI mặc định.
- `references/unity-subsystems.md` — Input System, physics, animation, NavMesh, Tilemap, SO, build pipeline.
- `references/design-frameworks.md` — MDA, flow, core loop, cân bằng, kinh tế, level, game feel.
- `references/performance.md` — ngân sách khung hình, profiler, GC, batching.
- `scripts/unity_project_scan.py` — quét phiên bản Unity, pipeline, package, asmdef, test (dùng cho `init`).
- `scripts/check_unity_project.py` — kiểm tra `.meta`/GUID trùng, thiếu, script mất tham chiếu.
- `scripts/find_guid_refs.py` — tìm mọi scene/prefab/asset tham chiếu tới một script/asset theo GUID.
- `scripts/create_test_asmdef.py` — tạo asmdef cho test EditMode/PlayMode.
- `scripts/resolve_test_target.sh` — xác định assembly + test mode của một file test.
