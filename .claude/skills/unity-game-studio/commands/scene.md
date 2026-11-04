# Command: scene

**Vai trò**: Developer (+ Designer cho UI layout, Game Designer cho blockout) · **Tham số**: `"<mô tả thay đổi>"` — vd `"tạo prefab Enemy_Slime có Rigidbody2D, CircleCollider2D, EnemyController"`, `"thêm HUD máu vào GameScene"`
**Ghi vào**: `.unity`, `.prefab`, `.asset` trong `Assets/**` — **thông qua Unity Editor API**, không sửa text
**Đọc**: `references/unity-assets-yaml.md`, `references/unity-automation.md`, spec UI (`commands/ui.md`) nếu là UI

## Luật
- Không Edit/Write trực tiếp `.unity`, `.prefab`. Không tạo/sửa `.meta`.
- Mọi thay đổi scene/prefab chạy qua **editor script tạm** hoặc công cụ MCP tương đương; script luôn kết thúc
  bằng lệnh lưu; không để scene/asset "dirty".
- Editor đang Play Mode → dừng Play Mode trước. Không gọi hai lệnh Editor song song.
- GameObject người chơi thao tác (button, toggle, input field...) có tên làm đường dẫn hierarchy **duy nhất** để
  test tìm được. Instance prefab lặp lại có tên riêng (`Enemy(0)`, `Enemy(Boss)`).

## Chọn cách thực hiện (theo backend có sẵn — `references/unity-automation.md`)
1. **Unity MCP có công cụ scene/gameobject** (`gameobject-create`, `gameobject-component-add`,
   `assets-prefab-*`, `scene-save`...) → dùng cho thay đổi đơn giản (vài object). Lưu scene/prefab sau cùng.
2. **Chạy C# trong Editor** (MCP `script-execute`, Rider `run_method_in_unity`) → cho thay đổi nhiều bước:
   viết editor script tạm.
3. **Không có backend** → viết editor script tạm có `[MenuItem("Tools/UGS Temp/<Tên>")]`, nhờ người dùng chạy từ
   menu (hoặc chạy batchmode `-executeMethod` khi Editor đóng), rồi kiểm tra kết quả.
4. Thay đổi rất nhỏ trên prefab không có backend nào → đưa hướng dẫn Inspector từng bước cho người dùng.

## Quy trình editor script tạm
1. Đặt ở `Assets/_Temp/Editor/UgsTemp_<Tên>.cs` (thư mục `Editor` → assembly editor). Có asmdef gần nhất không phải
   editor → kiểm tra bằng `scripts/resolve_test_target.sh` hoặc đặt cạnh asmdef Editor của dự án. Đề nghị thêm
   `Assets/_Temp*` vào `.gitignore` nếu chưa có.
2. Nội dung: một `public static void Run()`; dùng `ObjectFactory.CreateGameObject` / `ObjectFactory.AddComponent`
   (có Undo + Preset); `transform.SetParent(parent, worldPositionStays: false)`.
   - Scene mới: `EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects | EmptyScene, NewSceneMode.Single)`
     (Default khi load single — đã có Camera + Light; Empty khi load additive) → `EditorSceneManager.SaveScene(scene, path)`.
   - Scene có sẵn: `EditorSceneManager.OpenScene(path, OpenSceneMode.Single)` → sửa → `SaveScene(scene)`.
   - Prefab mới: dựng hierarchy → `PrefabUtility.SaveAsPrefabAsset(go, path)` → `Object.DestroyImmediate(go)`.
   - Prefab có sẵn: `PrefabUtility.LoadPrefabContents(path)` → sửa → `SaveAsPrefabAsset(root, path)` →
     `PrefabUtility.UnloadPrefabContents(root)`.
   - Tạo asset phụ (Material, ScriptableObject): `AssetDatabase.CreateAsset` rồi `AssetDatabase.SaveAssets()` cuối script.
   - Gán field serialized private: `new SerializedObject(component)` → `FindProperty("_field")` (auto-property:
     `"<Name>k__BackingField"`) → set → `ApplyModifiedPropertiesWithoutUndo()`.
   - UI: theo `references/unity-assets-yaml.md` §uGUI (Canvas + CanvasScaler ScaleWithScreenSize + GraphicRaycaster,
     EventSystem nếu chưa có, `DefaultControls.Create*`, layer "UI"). TextMeshPro hay Text legacy: theo dự án.
   - Cuối: `Debug.Log("[UGS] done: ...")`.
3. Compile → chạy → **thành công chỉ khi** không có log `Error`/`Exception` (kết quả "success" của công cụ chỉ có
   nghĩa là method đã được gọi).
4. Xác nhận thay đổi đã vào: `git diff --stat` (scene/prefab có sẵn) hoặc file tồn tại + hierarchy đúng (mới).
   Chụp Scene/Game view nếu có thể.
5. **Xóa editor script và `.meta` của nó** ngay sau khi chạy thành công (xóa file từ filesystem rồi refresh
   AssetDatabase; script không tự xóa chính nó). Chạy lỗi → giữ script để sửa và chạy lại.
6. Scene/prefab và asset được tạo **cần commit cùng `.meta`** (Unity tạo). Editor script tạm **không bao giờ commit**.

## Scene mới
Hỏi: load single hay additive? Có thêm vào Build Profile / Scene List không? Đặt ở `Assets/<_Project>/Scenes/`.

## Định nghĩa hoàn thành
- Scene/prefab mở được không lỗi Console; hierarchy đúng mô tả; tên object dùng cho test là duy nhất.
- Không còn editor script tạm; không sửa tay YAML; `check_unity_project.py` không ERROR mới.
- Báo cáo: object/component đã tạo, file thay đổi, phần cần người dùng xem lại trong Editor (bố cục, animation).
