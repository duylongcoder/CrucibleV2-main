# Lò Giả Kim (working title: Crucible) — repo CrucibleV2

> File này được nạp vào mọi phiên làm việc. Giữ ngắn: ngữ cảnh dự án + quy ước + mục lục.
> Chi tiết nằm trong `Docs/`. Cập nhật bằng `/unity-game-studio init` (quét lại) hoặc sửa tay.

## Dự án

- **Mô tả một câu**: Puzzle roguelite 2D — nhà giả kim triều đình xếp nguyên liệu lên lò 3×3 như một dòng thời gian, giải mã công thức bí mật (khoảng cách nhịp) để luyện khoáng cho Vua rèn trang bị; thua 3 trận là bị trục xuất. Chi tiết: `Docs/GDD.md`
- **Thể loại / góc nhìn**: puzzle roguelite, 2D, màn hình ngang, reference 1920×1080
- **Nền tảng mục tiêu**: WebGL trước (chuột: kéo-thả + click-để-đặt), Mobile touch sau — UI không phụ thuộc hover
- **Giai đoạn**: Pre-production (GDD In Review)
- **Rigor**: standard
- **Ngôn ngữ tài liệu & tên test**: Tiếng Việt cho mô tả; tên class/method/test bằng tiếng Anh.

## Môi trường Unity

| Mục | Giá trị |
|---|---|
| Unity version | 6000.6.7f1 |
| Render pipeline | URP 17.6.0 — 2D Renderer (`Assets/Settings/UniversalRP.asset`, `Renderer2D.asset`) |
| Scripting backend | IL2CPP (WebGL bắt buộc IL2CPP; Android cấu hình sẵn từ template) |
| Input | Input System package 1.20.1 (`Assets/Settings/InputSystem_Actions.inputactions`) |
| UI | uGUI 2.6.0 (+ TextMeshPro tích hợp) |
| Kiến trúc chính | ScriptableObject data + event channel, không DI (giả định — chốt bằng `arch`) |
| Async | Awaitable (Unity 6) |
| Test framework | Unity Test Framework 1.8.0 (NUnit) — EditMode + PlayMode |
| Assembly definitions | `Assets/Welcome/Unity.2D.Welcome.asmdef` (của template, không phải code game) — chưa có asmdef game |
| Test assemblies | Chưa có — tạo bằng `/unity-game-studio setup` |
| Package đáng chú ý | 2D Tilemap Extras 9.0.1 |
| Đường dẫn Unity Editor | `/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity` |
| Backend điều khiển Editor | CLI batchmode (chưa có Unity MCP); Unity CLI có tại `~/.unity/bin/unity` |

## Cấu trúc thư mục

```
CrucibleV2/
├── CLAUDE.md
├── Docs/                  # tài liệu dự án (ngoài Assets để Unity không import)
├── Assets/
│   ├── _Project/              # (dự kiến — chưa tạo)
│   │   ├── Scripts/Runtime/   # code game (asmdef)
│   │   ├── Scripts/Editor/    # editor tools (asmdef, Editor only)
│   │   ├── Data/              # ScriptableObject assets (config, tuning)
│   │   ├── Prefabs/  Scenes/  Art/  Audio/  UI/
│   ├── Scenes/SampleScene.unity   # scene template
│   ├── Settings/                  # URP, Input Actions, Volume
│   ├── Welcome/                   # asset hướng dẫn của template
│   └── Tests/
│       ├── EditMode/          # logic thuần C#
│       └── PlayMode/          # vòng đời MonoBehaviour, physics, scene
├── Packages/  ProjectSettings/
```

## Lệnh

```bash
# EditMode test (Editor phải đóng)
"/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml -logFile -
# PlayMode test
"/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml -logFile -
# Một test cụ thể: thêm  -testFilter <Namespace.Class.Method>
# Kiểm tra .meta/GUID
python3 .claude/skills/unity-game-studio/scripts/check_unity_project.py .
# Build
TBD — xem Docs/BUILD.md (tạo bằng /unity-game-studio setup hoặc release)
```

## Quy ước

- Namespace: `Crucible.<Folder>`; một public type mỗi file; tên file = tên MonoBehaviour.
- Field Inspector: `[SerializeField] private` + `[Tooltip]`; private field `_camelCase`; public `PascalCase`.
- Giá trị gameplay trong ScriptableObject ở `Assets/_Project/Data/`, không hard-code.
- Event C#: `Action`/`Action<T>`; tên sự kiện dạng phân từ (`DoorOpening`, `DoorOpened`).
- Test: class `<ClassName>Test(s)`, method `<Method>_<Condition>_<Expected>`; Arrange–Act–Assert.
- Commit: `<type>(<scope>): <mô tả>` — vd `feat(combat): add shield damage reduction`.

## Git

- Nhánh chính: `main` · Quy trình: trunk-based, nhánh ngắn
- Commit tự động: không — `có` thì Claude commit sau mỗi bước xanh; `không` thì chỉ đề xuất message.
- Commit test-first: không — `có` thì được commit test mới đang fail (bước Test first / test tái hiện bug) để giữ
  bằng chứng test viết trước code; `không` thì gộp test vào commit cùng phần sửa khi đã xanh.
- Không bao giờ: push, force push, tạo tag (người dùng tự làm).
- Asset naming: xem `Docs/ART.md` §8 Asset Standards.

## Luật bắt buộc

- Đọc code liên quan trước khi sửa; theo pattern sẵn có; thay đổi nhỏ, một việc một commit.
- Không sửa tay `.unity`/`.prefab`; không tạo/sửa `.meta`; đổi tên/di chuyển kèm `.meta` (`git mv`/Editor).
- Đổi tên field serialized → `[FormerlySerializedAs("tenCu")]`.
- Không `Find`/`GetComponent` trong `Update`; không alloc mỗi frame trong vòng lặp nóng.
- Event `OnEnable`↔`OnDisable`, `Awake/Start`↔`OnDestroy`; async dùng `destroyCancellationToken`.
- Test trước hoặc cùng lúc; bug phải có test tái hiện fail trước khi sửa; không tắt test để qua build.
- Không thêm package khi chưa hỏi; không lệnh phá hủy khi chưa xác nhận; không commit khi test fail; không push.
- Không sửa `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`.

## Mục lục tài liệu

| File | Nội dung | Đọc khi |
|---|---|---|
| `Docs/GDD.md` | Concept, pillars, core loop, systems index, thiết kế hệ thống | Làm gameplay |
| `Docs/ART.md` | Art bible, UI visual, chuẩn asset | Làm hình ảnh/UI |
| `Docs/PLAN.md` | Roadmap, milestone, backlog, trạng thái feature | Bắt đầu việc mới |
| `Docs/TECH.md` | Kiến trúc, ADR, ngân sách hiệu năng | Viết code hệ thống |
| `Docs/TEST.md` | Chiến lược test, smoke list, playtest | Viết/chạy test |
| `Docs/LOG.md` | Checkpoint, nhật ký phiên, quyết định, changelog | **Đầu mỗi phiên** (khối CHECKPOINT) |
| `Docs/LEVELS.md` | Thiết kế màn chơi | Làm level |
| `Docs/ASSETS.md` | Danh mục & spec asset | Làm/import asset |
| `Docs/BUGS.md` | Danh sách bug | Sửa bug |
| `Docs/BUILD.md` | Build, CI, release | Build/phát hành |

## Ghi chú riêng của dự án

- Giả định khi init (2026-11-03): tên game tạm = `CrucibleV2`; namespace `Crucible`; kiến trúc SO event channel,
  không DI; Async = Awaitable — cần xác nhận ở `concept`/`arch`.
- Version Control mode bị Unity Hub đổi thành **Unity Version Control** khi tạo project; phải là **Visible Meta Files**
  (xem Docs/BUILD.md §1). Kiểm tra lại sau mỗi lần mở project qua Hub.
- `Assets/Welcome/` là nội dung hướng dẫn của template; có thể xóa khi dọn dự án (hỏi trước).
