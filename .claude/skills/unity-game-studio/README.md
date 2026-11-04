# unity-game-studio

Skill cho Claude làm game **Unity** theo vai trò: Game Designer, Designer (Art/UI/UX), Developer, QA/Tester, DevOps.
Mỗi đầu việc là một command; mọi kết quả được ghi vào bộ tài liệu dự án để phiên sau nối tiếp được.

## Cài đặt

**Claude Code**
- Theo người dùng: copy thư mục `unity-game-studio/` vào `~/.claude/skills/`
- Theo dự án (chia sẻ qua git): copy vào `<ProjectRoot>/.claude/skills/`
- Tùy chọn: lệnh tắt `/ugs-*` — xem `extras/README.md`

**Claude app (claude.ai / desktop)**: nén thư mục thành zip (thư mục `unity-game-studio/` ở gốc zip) và tải lên ở
Settings → Capabilities → Skills.

Yêu cầu: Python 3.10+ cho các script (chỉ thư viện chuẩn). Điều khiển Unity Editor: một trong Unity MCP, Rider MCP,
hoặc Unity CLI batchmode (xem `references/unity-automation.md`).

## Bắt đầu

```
/unity-game-studio init          # tạo CLAUDE.md + Docs/, quét môi trường Unity
/unity-game-studio status        # đang ở đâu, nên làm gì tiếp
/unity-game-studio concept       # dự án mới: ý tưởng → pillars → core loop
```

Luồng điển hình cho một feature: `gdd <hệ thống>` → (`asset` / `ui`) → `plan <feature>` → `build <feature>` →
`test run` → `log`.

## Cấu trúc

```
unity-game-studio/
├── SKILL.md              # router: vai trò, bảng command, luật cốt lõi, giao thức tài liệu
├── roles/                # cách nghĩ của 5 vai trò
├── commands/             # 24 command — mỗi đầu việc một file
├── templates/            # CLAUDE.md, GDD, ART, PLAN, TECH, TEST, LOG, LEVELS, ASSETS, BUGS, BUILD, feature-plan, system-gdd
├── references/           # chuẩn code Unity, test, điều khiển Editor, YAML/meta, subsystems, thiết kế, hiệu năng
├── scripts/              # quét dự án, kiểm .meta/GUID, tìm tham chiếu GUID, tạo test asmdef
└── extras/               # wrapper slash command /ugs-*
```

## Tài liệu dự án được tạo

| File | Mục đích |
|---|---|
| `CLAUDE.md` | Mô tả dự án, môi trường Unity, quy ước, lệnh, mục lục tài liệu |
| `Docs/GDD.md` | Game design: concept, pillars, core loop, systems index, thiết kế hệ thống |
| `Docs/ART.md` | Art bible: nhận diện, màu, hình khối, UI, chuẩn asset + import settings, prompt card |
| `Docs/PLAN.md` | Quy trình thiết kế/triển khai, milestone, backlog, trạng thái |
| `Docs/TECH.md` | Kiến trúc, assembly, ADR, ngân sách hiệu năng, nợ kỹ thuật |
| `Docs/TEST.md` | Chiến lược test, test plan, smoke list, playtest |
| `Docs/LOG.md` | Checkpoint phiên, nhật ký, quyết định, changelog, tóm tắt log Unity |
| `Docs/LEVELS.md` · `ASSETS.md` · `BUGS.md` · `BUILD.md` | Tạo khi cần: màn chơi · danh mục asset · bug · build & release |
| `Docs/plans/<feature>.md` · `Docs/design/<system>.md` | Plan chi tiết feature · GDD hệ thống lớn |

Nguồn & giấy phép: `CREDITS.md`.
