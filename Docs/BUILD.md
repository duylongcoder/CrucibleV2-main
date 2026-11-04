# Build & Release — CrucibleV2 (tên tạm)

> Chủ sở hữu: DevOps · **Cập nhật**: 2026-11-04

## 1. Môi trường

| Mục | Giá trị |
|---|---|
| Unity | 6000.6.7f1 — module nền tảng cần: WebGL (chính), macOS / Linux Standalone (dev/test); Android/iOS sau. Máy dev: macOS + Linux (Linux cần cài 6000.6.7f1 — hiện chỉ có 6000.6.8f1) |
| Asset Serialization | Force Text |
| Version Control mode | Visible Meta Files (✅ đã sửa — kiểm 2026-11-04) |
| Git LFS | không (git-lfs chưa cài; chưa có binary lớn — xem lại khi thêm art/audio) |
| Merge tool scene/prefab | UnityYAMLMerge qua `.gitattributes` (`merge=unityyamlmerge`) — mỗi máy cấu hình driver, xem §3 |
| Nhánh | trunk-based: `main` luôn build được; nhánh ngắn; release = tag; `hotfix/*` từ tag |

### Kết quả setup check — 2026-11-04

| Mục | Trạng thái |
|---|---|
| Git repo, `.gitignore` | OK (scanner báo thiếu UserSettings là báo nhầm — có `/[Uu]ser[Ss]ettings/`) |
| Force Text / Visible Meta Files | OK / OK |
| Render pipeline | OK — URP gán qua QualitySettings |
| `.gitattributes` | ✅ tạo mới — UnityYAMLMerge, eol, binary; không LFS |
| asmdef game + test | ✅ tạo 6 asmdef (TECH §2) |
| Newtonsoft JSON | ✅ thêm `com.unity.nuget.newtonsoft-json` 3.2.1 vào manifest (ADR-003) — xác nhận khi mở Editor |
| `check_unity_project.py` | 0 lỗi, 29 cảnh báo — tất cả do máy Linux chưa có `Library/PackageCache`; chạy lại sau khi mở Editor |
| Unity trên máy Linux | ⚠ chỉ có 6000.6.8f1 → người dùng cài 6000.6.7f1 + WebGL qua Hub |

### Kết quả setup check — 2026-11-03

| Mục | Trạng thái |
|---|---|
| Git repo, không commit file rác | OK |
| `.gitignore` | OK — đã thêm `.DS_Store`, `.idea/`, `/[Aa]ssets/_Temp*` |
| Force Text | OK |
| Visible Meta Files | SAI — chờ người dùng đổi trong Project Settings |
| `.gitattributes` / LFS | Chưa có — người dùng chưa chọn |
| asmdef game + test asmdef | Chưa có — người dùng chưa chọn (chờ có code game) |
| `check_unity_project.py` | OK, 0 lỗi |

## 2. Cấu hình build theo nền tảng

| Nền tảng | Build Profile / target | Backend | Stripping | Scene list | Output | Ghi chú |
|---|---|---|---|---|---|---|
| {{Windows}} | {{win64}} | {{IL2CPP}} | {{Low}} | {{Build Profile / global}} | {{Builds/Win/Game.exe}} | {{}} |

- Define symbols: {{}}
- `link.xml` bảo vệ code dùng reflection: {{có/không}}
- Addressables: {{có — build content trước player build}}

## 3. Lệnh

```bash
# Cấu hình merge driver UnityYAMLMerge (một lần mỗi máy, git config cục bộ)
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'<UNITY_EDITOR_DIR>/Data/Tools/UnityYAMLMerge' merge -p %O %B %A %A"
# macOS: <UNITY_EDITOR_DIR> = /Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents
# Linux: <UNITY_EDITOR_DIR> = ~/Unity/Hub/Editor/6000.6.7f1/Editor
```

```bash
# Build bằng script (Editor đóng)
"{{UNITY}}" -batchmode -nographics -projectPath . -buildTarget {{win64}} \
  -executeMethod {{Game.Editor.CIBuild.Run}} -out {{Builds/Win/Game.exe}} -appVersion {{1.0.0}} -logFile -
# Build Profile có sẵn
"{{UNITY}}" -batchmode -nographics -quit -projectPath . \
  -activeBuildProfile "{{Assets/Settings/Build Profiles/WindowsRelease.asset}}" -build {{Builds/Win/Game.exe}} -logFile -
```

## 4. CI

| Workflow | Trigger | Bước | Secret cần |
|---|---|---|---|
| {{test.yml}} | push, PR | checkout (LFS) → cache Library → EditMode → PlayMode → artifact kết quả | `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` |
| {{build.yml}} | tag `v*` | test → build từng nền tảng → artifact | như trên |

Test loại khỏi CI: `[Category("IgnoreCI")]` — {{danh sách + lý do}}

## 5. Phiên bản

- Định dạng: {{SemVer MAJOR.MINOR.PATCH}}; `PlayerSettings.bundleVersion` đặt bởi build script.
- Mobile: {{bundleVersionCode / buildNumber tăng mỗi bản nộp store}}

## 6. Release checklist

**Code & chất lượng**
- [ ] Toàn bộ test xanh (EditMode + PlayMode); smoke list pass
- [ ] Không còn bug S1; S2 đã chốt chấp nhận hoặc sửa
- [ ] `check_unity_project.py` không ERROR; Console không lỗi khi chơi đường chính
- [ ] Hiệu năng trong ngân sách (`TECH.md` §6) đo trên Development build máy thật

**Build**
- [ ] Release build (không `Development`), đúng scene list, đúng version
- [ ] Build chạy được trên máy sạch; save/load hoạt động; save từ bản trước load được
- [ ] Kích thước build: {{}}

**Nội dung & store**
- [ ] Credits / license asset bên thứ ba đầy đủ (`ASSETS.md`)
- [ ] Icon, tên, mô tả, ảnh chụp store
- [ ] Yêu cầu nền tảng: {{Steam / itch / Google Play / App Store}}

**Phát hành**
- [ ] Changelog & patch notes (`LOG.md`)
- [ ] Tag phiên bản (người dùng tự push)

## 7. Lịch sử phát hành

| Phiên bản | Ngày | Nền tảng | Commit | Ghi chú |
|---|---|---|---|---|
