# {{TÊN DỰ ÁN}}

> File này được nạp vào mọi phiên làm việc. Giữ ngắn: ngữ cảnh dự án + quy ước + mục lục.
> Chi tiết nằm trong `Docs/`. Cập nhật bằng `/unity-game-studio init` (quét lại) hoặc sửa tay.

## Dự án

- **Mô tả một câu**: {{elevator pitch — thể loại, người chơi làm gì, điểm khác biệt}}
- **Thể loại / góc nhìn**: {{vd: puzzle roguelike, 2D top-down}}
- **Nền tảng mục tiêu**: {{PC / Mobile / Web / Console}} — input chính: {{chuột-phím / touch / gamepad}}
- **Giai đoạn**: {{Concept | Pre-production | Prototype | Production | Polish | Release}}
- **Rigor**: {{lean | standard | strict}}
- **Ngôn ngữ tài liệu & tên test**: Tiếng Việt cho mô tả; tên class/method/test bằng tiếng Anh.

## Môi trường Unity

| Mục | Giá trị |
|---|---|
| Unity version | {{6000.x.yf1 — từ ProjectSettings/ProjectVersion.txt}} |
| Render pipeline | {{URP / HDRP / Built-in}} |
| Scripting backend | {{Mono / IL2CPP}} |
| Input | {{Input System package / Input Manager (old) / Both}} |
| UI | {{uGUI + TextMeshPro / UI Toolkit}} |
| Kiến trúc chính | {{ScriptableObject event channel / Zenject / VContainer / không DI}} |
| Async | {{Awaitable / UniTask / Coroutine}} |
| Test framework | Unity Test Framework {{version}} (NUnit) — EditMode + PlayMode |
| Assembly definitions | {{Assets/_Project/Scripts/Runtime/Game.asmdef, ...}} |
| Test assemblies | {{Assets/Tests/EditMode/*.asmdef, Assets/Tests/PlayMode/*.asmdef}} |
| Package đáng chú ý | {{Addressables, Cinemachine, ...}} |
| Đường dẫn Unity Editor | {{/Applications/Unity/Hub/Editor/<ver>/Unity.app/Contents/MacOS/Unity}} |
| Backend điều khiển Editor | {{Unity MCP / Rider MCP / CLI batchmode}} |

## Cấu trúc thư mục

```
{{ProjectRoot}}/
├── CLAUDE.md
├── Docs/                  # tài liệu dự án (ngoài Assets để Unity không import)
├── Assets/
│   ├── {{_Project}}/
│   │   ├── Scripts/Runtime/   # code game (asmdef)
│   │   ├── Scripts/Editor/    # editor tools (asmdef, Editor only)
│   │   ├── Data/              # ScriptableObject assets (config, tuning)
│   │   ├── Prefabs/  Scenes/  Art/  Audio/  UI/
│   └── Tests/
│       ├── EditMode/          # logic thuần C#
│       └── PlayMode/          # vòng đời MonoBehaviour, physics, scene
├── Packages/  ProjectSettings/
```

## Lệnh

```bash
# EditMode test (Editor phải đóng)
"{{UNITY}}" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml -logFile -
# PlayMode test
"{{UNITY}}" -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml -logFile -
# Một test cụ thể: thêm  -testFilter <Namespace.Class.Method>
# Kiểm tra .meta/GUID
python3 {{SKILL_DIR}}/scripts/check_unity_project.py .
# Build
{{lệnh build — xem Docs/BUILD.md}}
```

## Quy ước

- Namespace: `{{Company.Game}}.<Folder>`; một public type mỗi file; tên file = tên MonoBehaviour.
- Field Inspector: `[SerializeField] private` + `[Tooltip]`; private field `_camelCase`; public `PascalCase`.
- Giá trị gameplay trong ScriptableObject ở `Assets/{{_Project}}/Data/`, không hard-code.
- Event C#: `Action`/`Action<T>`; tên sự kiện dạng phân từ (`DoorOpening`, `DoorOpened`).
- Test: class `<ClassName>Test(s)`, method `<Method>_<Condition>_<Expected>`; Arrange–Act–Assert.
- Commit: `<type>(<scope>): <mô tả>` — vd `feat(combat): add shield damage reduction`.

## Git

- Nhánh chính: `{{main}}` · Quy trình: {{trunk-based, nhánh ngắn}}
- Commit tự động: {{không}} — `có` thì Claude commit sau mỗi bước xanh; `không` thì chỉ đề xuất message.
- Commit test-first: {{không}} — `có` thì được commit test mới đang fail (bước Test first / test tái hiện bug) để giữ
  bằng chứng test viết trước code; `không` thì gộp test vào commit cùng phần sửa khi đã xanh.
- Không bao giờ: push, force push, tạo tag (người dùng tự làm).
- Asset naming: `{{xem Docs/ART.md §Asset Standards}}`.

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

{{Luật cấm, thư viện được phép, quirks đã biết...}}
