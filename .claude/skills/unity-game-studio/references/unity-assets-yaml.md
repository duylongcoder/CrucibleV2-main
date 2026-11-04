# Scene, prefab, asset, `.meta` & GUID

## Mục lục
1. Luật chung
2. `.meta` và GUID
3. Đổi tên / di chuyển / xóa an toàn
4. Sửa tay YAML (`.asset`, `.mat`) — chỉ khi buộc phải
5. Mẫu editor script
6. uGUI — giá trị mặc định khi dựng bằng code
7. Kiểm tra sau thay đổi

## 1. Luật chung
- `.unity`, `.prefab`: **không bao giờ** sửa text trực tiếp. Dùng editor script / công cụ MCP (`commands/scene.md`).
- `.asset` (ScriptableObject), `.mat`: ưu tiên sửa qua Editor. Sửa tay chỉ khi Editor **không mở/không truy cập được**
  và theo §4. Không chắc Editor có đang mở → coi như đang mở (lần lưu kế của Editor sẽ ghi đè/xung đột).
- Không sửa tay: `AnimationClip`, `AnimatorController`, `Texture2D`, `Mesh`, `AudioClip`, `LightingDataAsset`, `TerrainData`,
  asset binary, file trong `Packages/` của bên thứ ba, `ProjectSettings/*.asset` (đổi qua Editor/API).
- Dự án phải ở chế độ **Force Text** (file bắt đầu bằng `%YAML 1.1`). File không bắt đầu như vậy → binary/mixed → không sửa.

## 2. `.meta` và GUID
- Mỗi file/thư mục trong `Assets/` (và package nhúng) có `.meta` chứa `guid:` 32 hex + import settings.
- Tham chiếu giữa asset là `{fileID: N, guid: <32hex>, type: 2|3}` (`type: 3` = script, `2` = asset). Script MonoBehaviour
  được tham chiếu bằng `m_Script: {fileID: 11500000, guid: <guid của .cs.meta>, type: 3}`.
- **Không tạo `.meta`** (Unity tạo khi import) · **không sửa/xóa `.meta`** · **không bịa GUID**.
- Copy file kèm `.meta` → GUID trùng → hỏng tham chiếu. Copy asset: qua Editor (Ctrl+D / `AssetDatabase.CopyAsset`) để có GUID mới.
- Kiểm tra: `python3 <skill>/scripts/check_unity_project.py <root>` (GUID trùng, `.meta` bị bỏ lại khi di chuyển,
  `.meta` hỏng, script bị mất tham chiếu, tên class ≠ tên file).
- Tìm mọi nơi tham chiếu một asset/script: `python3 <skill>/scripts/find_guid_refs.py <root> <đường dẫn asset | guid>` —
  đừng chỉ grep tên class (scene/prefab tham chiếu bằng GUID, không bằng tên).

## 3. Đổi tên / di chuyển / xóa an toàn
| Việc | Cách đúng | Sai thường gặp |
|---|---|---|
| Đổi tên/di chuyển asset hoặc script | Qua Editor (Project window, `AssetDatabase.MoveAsset`, MCP `assets-move`) hoặc `git mv a.cs b.cs && git mv a.cs.meta b.cs.meta` | `mv` file mà bỏ `.meta` → GUID mới → "Missing (Mono Script)" |
| Đổi tên class MonoBehaviour/SO | Đổi tên file **và** class cùng lúc (tên file = tên class), giữ `.meta` | Chỉ đổi class → Unity không gắn script |
| Đổi tên field serialized | `[FormerlySerializedAs("old")]` | Mất giá trị đã gán ở mọi scene/prefab |
| Đổi tên/namespace type trong `[SerializeReference]` | `[MovedFrom(true, "OldNs", "OldAsm", "OldName")]` | Mất dữ liệu managed reference |
| Đổi kiểu field / chuyển field sang component khác | Editor script migrate (đọc cũ → ghi mới → lưu) rồi mới xóa field cũ | Đổi thẳng → dữ liệu mất im lặng |
| Xóa script/asset | `find_guid_refs.py` trước; còn tham chiếu → không xóa, báo danh sách | Xóa → scene/prefab hỏng |

## 4. Sửa tay YAML (`.asset` ScriptableObject, `.mat`)
- Giữ nguyên 2 dòng đầu: `%YAML 1.1` và `%TAG !u! tag:unity3d.com,2011:`.
- Giữ document marker: SO `--- !u!114 &11400000`, Material `--- !u!21 &2100000`.
- SO: thứ tự trường đầu giữ nguyên (`m_ObjectHideFlags` … `m_Script` → `m_Name` → `m_EditorClassIdentifier`), field
  người dùng ở sau. `m_Name` = tên file không đuôi.
- `m_Script` GUID: copy từ asset cùng loại hoặc từ `<Script>.cs.meta` — không bịa.
- Field auto-property: `<PropertyName>k__BackingField` — không đổi thành tên property.
- Dạng giá trị Unity: bool `0`/`1`; int/enum số trần; float thập phân; `Vector3 {x: 0, y: 0, z: 0}`;
  `Quaternion {x: 0, y: 0, z: 0, w: 1}`; `Color {r: 1, g: 1, b: 1, a: 1}`; tham chiếu cục bộ `{fileID: N}`; null `{fileID: 0}`;
  tham chiếu chéo `{fileID: N, guid: X, type: 2}`; list tham chiếu dạng block `- {fileID: ...}`.
- Chuỗi ASCII không quote; chuỗi có ký tự ngoài ASCII (tiếng Việt) → trong ngoặc kép, escape `\uXXXX` từng ký tự.
- Thụt 2 dấu cách, LF, UTF-8 không BOM, xuống dòng cuối file. Không thêm comment, alias `*`, tag, `|`/`>`.
- Sau khi sửa: để Unity import lại (focus Editor / refresh), kiểm Console không lỗi, xem diff Unity tạo ra khi lưu lại.

## 5. Mẫu editor script
```csharp
// Assets/_Temp/Editor/UgsTemp_CreateEnemyPrefab.cs — TẠM, xóa sau khi chạy thành công
using UnityEditor;
using UnityEngine;

public static class UgsTemp_CreateEnemyPrefab
{
    [MenuItem("Tools/UGS Temp/Create Enemy Prefab")] // chỉ cần khi người dùng tự bấm
    public static void Run()
    {
        const string path = "Assets/_Project/Prefabs/Enemies/Enemy_Slime.prefab";
        var root = ObjectFactory.CreateGameObject("Enemy_Slime");
        try
        {
            var body = ObjectFactory.AddComponent<Rigidbody2D>(root);
            body.gravityScale = 0f;
            ObjectFactory.AddComponent<CircleCollider2D>(root);
            var controller = ObjectFactory.AddComponent<EnemyController>(root);

            // Gán field serialized private qua SerializedObject
            var so = new SerializedObject(controller);
            so.FindProperty("_config").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<EnemyConfig>("Assets/_Project/Data/Enemies/Slime.asset");
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out var ok);
            if (!ok || prefab == null) throw new System.Exception($"Save prefab failed: {path}");
            Debug.Log($"[UGS] done: {path}");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}
```
Sửa prefab có sẵn: `var root = PrefabUtility.LoadPrefabContents(path);` → sửa → `PrefabUtility.SaveAsPrefabAsset(root, path);`
→ `PrefabUtility.UnloadPrefabContents(root);` (trong `finally`).
Scene có sẵn: `var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);` → sửa →
`EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);`.
Tạo asset: `AssetDatabase.CreateAsset(obj, path);` … `AssetDatabase.SaveAssets();` cuối script.
Import settings: `var ti = (TextureImporter)AssetImporter.GetAtPath(path); ti.textureType = TextureImporterType.Sprite;
ti.spritePixelsPerUnit = 32; ti.filterMode = FilterMode.Point; ti.mipmapEnabled = false; ti.SaveAndReimport();`.

## 6. uGUI — giá trị mặc định khi dựng bằng code
(Tương đương menu GameObject > UI.) Theo framework/phong cách của dự án nếu đã có.
- **Canvas**: `ObjectFactory.CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))`,
  layer `UI`, `renderMode = ScreenSpaceOverlay`. CanvasScaler nên `ScaleWithScreenSize`, `referenceResolution` theo
  ART §0, `screenMatchMode = MatchWidthOrHeight`, `matchWidthOrHeight` 0 (theo chiều rộng) hoặc 0.5 — mặc định
  `ConstantPixelSize` không co giãn theo màn hình.
- **EventSystem**: chỉ tạo nếu chưa có trong stage: `ObjectFactory.CreateGameObject("EventSystem", typeof(EventSystem))`
  rồi `UnityEditor.EventSystems.InputModuleComponentFactory.AddInputModule(go)` (tự chọn `InputSystemUIInputModule`
  khi dùng Input System).
- **Control chuẩn**: `UnityEngine.UI.DefaultControls.CreateButton/CreateText/CreateImage/CreatePanel/CreateScrollView(res)`
  với `DefaultControls.Resources` lấy sprite built-in: `AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")`,
  `"UI/Skin/Background.psd"`, `"UI/Skin/InputFieldBackground.psd"`, `"UI/Skin/Knob.psd"`, `"UI/Skin/Checkmark.psd"`,
  `"UI/Skin/DropdownArrow.psd"`, `"UI/Skin/UIMask.psd"`. TextMeshPro: `TMP_DefaultControls` (namespace `TMPro`) tương tự;
  thiếu TMP Essential Resources → `TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false)` (không dùng menu
  item vì mở hộp thoại modal chặn lệnh).
- Font legacy built-in: Unity 2022.2+ `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` (trước đó `"Arial.ttf"`).
- Legacy `Text` với alignment giữa theo chiều dọc: dùng `verticalOverflow = Overflow` (với `Truncate` + rect thấp hơn
  line height, mesh có thể rỗng — không có chữ, không có lỗi).
- **Bố cục**: đặt anchor trước, `pivot` trùng điểm anchor (góc/cạnh), rồi mới `anchoredPosition`. ScrollView: Content
  cần `VerticalLayoutGroup` + `ContentSizeFitter (verticalFit = PreferredSize)` để cuộn theo nội dung. Không đặt
  ContentSizeFitter trên con của Layout Group đang "Control Child Size" cùng trục (xung đột). Danh sách runtime: xóa con
  cũ trước khi điền lại.
- Ảnh trang trí không cần nhận click: `raycastTarget = false` (giảm chi phí, tránh chặn nhầm).
- Đặt tên GameObject tương tác sao cho đường dẫn hierarchy duy nhất (test tìm được).

## 7. Kiểm tra sau thay đổi
1. Console không có lỗi khi mở scene/prefab; không có "Missing (Mono Script)" / "Missing Prefab".
2. `check_unity_project.py` không ERROR mới.
3. `git diff --stat` chỉ gồm file dự kiến; editor script tạm đã xóa.
4. Scene/prefab/asset mới **commit kèm `.meta`** do Unity tạo.
