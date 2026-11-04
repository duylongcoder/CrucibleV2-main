# Build & Release — {{TÊN GAME}}

> Chủ sở hữu: DevOps · **Cập nhật**: {{YYYY-MM-DD}}

## 1. Môi trường

| Mục | Giá trị |
|---|---|
| Unity | {{6000.x.yf1}} — module nền tảng cần: {{Windows IL2CPP, Android, iOS, WebGL}} |
| Asset Serialization | Force Text |
| Version Control mode | Visible Meta Files |
| Git LFS | {{có/không — pattern}} |
| Merge tool scene/prefab | {{UnityYAMLMerge / không}} |
| Nhánh | trunk-based: `main` luôn build được; nhánh ngắn; release = tag; `hotfix/*` từ tag |

## 2. Cấu hình build theo nền tảng

| Nền tảng | Build Profile / target | Backend | Stripping | Scene list | Output | Ghi chú |
|---|---|---|---|---|---|---|
| {{Windows}} | {{win64}} | {{IL2CPP}} | {{Low}} | {{Build Profile / global}} | {{Builds/Win/Game.exe}} | {{}} |

- Define symbols: {{}}
- `link.xml` bảo vệ code dùng reflection: {{có/không}}
- Addressables: {{có — build content trước player build}}

## 3. Lệnh

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
