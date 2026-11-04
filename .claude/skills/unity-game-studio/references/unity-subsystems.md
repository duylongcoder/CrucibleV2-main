# Unity subsystems — quy trình & bẫy thường gặp

Đối chiếu Unity 6.x. Dự án phiên bản khác → kiểm tài liệu đúng phiên bản. Nếu môi trường có skill chính thức của Unity
cho mảng đó (Input, 2D physics, NavMesh, UI, URP...), có thể dùng thêm để biết chi tiết.

## Mục lục
1. ScriptableObject architecture
2. Input System
3. Physics 3D
4. Physics 2D, Sprite & Tilemap
5. Animation (Animator)
6. AI Navigation (NavMesh)
7. Save / load
8. Build pipeline
9. Audio (tóm tắt)

## 1. ScriptableObject architecture
- **Config/data asset**: `[CreateAssetMenu(fileName = "...", menuName = "Game/...")] class XConfig : ScriptableObject` —
  designer chỉnh trong Inspector; MonoBehaviour giữ tham chiếu `[SerializeField] private XConfig _config;`.
- **Shared variable** (`FloatVariable`): `initialValue` serialized + `[NonSerialized] runtimeValue` reset trong `OnEnable`.
- **Event channel**: SO giữ danh sách listener (`Raise()` duyệt ngược), listener `Register` ở `OnEnable`, `Unregister` ở
  `OnDisable`. Payload có kiểu → channel cụ thể (`IntEvent`, `Vector3Event`) — Unity không tạo asset cho generic mở.
- **Runtime set**: SO giữ `List<T>`; object tự `Add` ở `OnEnable`, `Remove` ở `OnDisable` → thay cho `FindObjectsByType` mỗi frame.
- Bẫy:
  - Sửa SO lúc Play trong Editor **giữ lại** sau khi dừng → state runtime để ở field `[NonSerialized]` và reset.
  - Domain Reload tắt → `OnEnable` của SO không chạy lại khi Play → reset thêm qua
    `EditorApplication.playModeStateChanged` (ExitingEditMode) hoặc `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`.
  - SO là dữ liệu **dùng chung** — state riêng từng object (HP hiện tại của từng enemy) để trên MonoBehaviour.
  - SO không phải file save. `ScriptableObject.CreateInstance` lúc runtime → `Destroy` khi xong.
  - Event channel gọi đồng bộ — listener nặng làm chậm bên raise.

## 2. Input System (`com.unity.inputsystem`)
- Project Settings > Player > Active Input Handling: `Input System Package (New)` (hoặc `Both` nếu còn code cũ).
- Dùng `.inputactions`; Unity 6 có **project-wide actions** (`InputSystem.actions.FindAction("Player/Move")`, đã enable sẵn).
- Cách đọc: `PlayerInput` component (local/co-op), project-wide actions trong code, hoặc `InputActionReference`/asset riêng
  (phải `Enable()`/`Disable()`).
- Chuyển action map theo ngữ cảnh (Gameplay ↔ UI) thay vì if trong từng handler; project-wide actions bật **mọi** map lúc
  khởi động → tắt map không dùng.
- Nút bấm: callback `performed` hoặc `WasPressedThisFrame()`; `ReadValue` mỗi frame cho giá trị liên tục (Move).
- `+=` ở `OnEnable`, `-=` ở `OnDisable`.
- Bẫy: không có input → handling vẫn Old / quên `Enable()`; `InvalidOperationException` vì code cũ `Input.GetKey` khi chỉ bật New;
  `Send Messages` chỉ tới component cùng GameObject; kiểm bằng Input Debugger.
- Test: `InputTestFixture` (package Input System) để giả lập thiết bị trong test.

## 3. Physics 3D
- Vật mô phỏng có `Rigidbody`; vật bị chạm có `Collider`. `OnCollision*` cần ít nhất một Rigidbody **dynamic**.
- Mọi lực/velocity/`MovePosition` trong `FixedUpdate`; input đọc trong `Update`.
- Không gán `transform.position` cho Rigidbody dynamic (teleport, bỏ va chạm). Nếu buộc phải ghi transform rồi query cùng
  frame → `Physics.SyncTransforms()` một lần.
- Unity 6 đổi tên: `linearVelocity`, `linearDamping`, `angularDamping`, `PhysicsMaterial`.
- Trigger cần Rigidbody ở ít nhất một bên. `CharacterController` không gọi `OnCollisionEnter` → `OnControllerColliderHit`.
- Raycast mặc định trúng trigger (`queriesHitTriggers`) → `QueryTriggerInteraction.Ignore` khi cần. Ray bắt đầu trong
  collider không trúng collider đó. Dùng `NonAlloc`/buffer, `LayerMask`.
- Vật nhanh xuyên tường → Collision Detection `Continuous` (chỉ với static) / `Continuous Dynamic` / `Continuous Speculative`,
  hoặc spherecast theo quỹ đạo.
- MeshCollider lõm chỉ static/kinematic; vật dynamic → `Convex` hoặc compound primitive. Compound: một Rigidbody ở root.
- Body ngủ không tỉnh khi collider tĩnh dưới nó bị dời → `WakeUp()`. `Time.timeScale = 0` dừng mô phỏng.
- Layer Collision Matrix để cắt cặp không cần kiểm.

## 4. Physics 2D, Sprite & Tilemap
- 2D dùng `Rigidbody2D`/`Collider2D`, callback `OnCollisionEnter2D`/`OnTriggerEnter2D` (khác 3D). Cùng luật FixedUpdate.
- Unity 6: `Collider2D.compositeOperation = Merge` thay `usedByComposite`.
- Sprite import: Texture Type Sprite, Single/Multiple, **PPU thống nhất** toàn dự án, Filter Point cho pixel art,
  tắt mipmap cho 2D/UI, tắt Read/Write, nén theo nền tảng; Sprite Atlas theo màn để giảm draw call.
- Pixel art sắc nét: Pixel Perfect Camera (URP 2D), PPU khớp, camera ortho size theo PPU.
- Tilemap: `Grid` + một `Tilemap` mỗi lớp (nền, đất, tiền cảnh), sorting rõ ràng. Lớp va chạm: `TilemapCollider2D` +
  `CompositeCollider2D` + `Rigidbody2D (Static)`, Composite Operation = Merge (rẻ hơn, không khe). Rule Tile cần package
  `com.unity.2d.tilemap.extras` (hỏi trước khi thêm).
- Script: `SetTile(Vector3Int cell, tile)` dùng tọa độ **cell** — chuyển bằng `WorldToCell`/`CellToWorld`; sửa nhiều ô →
  `SetTilesBlock`; refresh có chọn lọc `RefreshTile` (tránh `RefreshAllTiles` trên map lớn).

## 5. Animation (Animator)
- Set parameter từ script (`SetFloat/SetBool/SetInteger/SetTrigger`), không ép state. Cache `Animator.StringToHash`.
- Blend Tree cho chuyển động liên tục (idle↔walk↔run theo Speed).
- Trigger bị "nhớ" nếu không transition nào tiêu thụ → `ResetTrigger` hoặc dùng Bool cho trạng thái kéo dài.
- Transition phản hồi nhanh (nhảy, trúng đòn): bỏ `Has Exit Time`.
- Root motion **hoặc** di chuyển bằng script, không cả hai.
- Layer + Avatar Mask cho phần thân trên; `CrossFadeInFixedTime` khi muốn thời lượng tính bằng giây.
- IK chỉ trong `OnAnimatorIK`, cần IK Pass + Humanoid.
- Animation event gọi method theo tên (chuỗi) → đổi tên method làm hỏng im lặng; ghi chú hoặc kiểm bằng editor test.

## 6. AI Navigation (`com.unity.ai.navigation`)
- Bake bằng component `NavMeshSurface` (không còn tab Bake cũ). Agent type (radius, height, step, slope) khớp `NavMeshAgent`.
- `SetDestination`; kiểm tới nơi bằng `!pathPending && remainingDistance <= stoppingDistance`; mục tiêu di chuyển chậm →
  repath theo timer, không mỗi frame.
- Vật cản động: `NavMeshObstacle` (carve). Nối đảo navmesh: `NavMeshLink` (`OffMeshLink` deprecated).
- Agent + Rigidbody: Rigidbody kinematic khi agent điều khiển. Không bật Agent và Obstacle cùng lúc trên một object.
- Dịch chuyển agent: `agent.Warp(pos)`; spawn: `NavMesh.SamplePosition` để đặt lên mesh.
- `NavMeshPath.status == PathPartial` → đích ở đảo khác.

## 7. Save / load
- Lưu **dữ liệu** (giá trị, seed, flag, ID), không lưu object/scene/reference runtime; dựng lại object từ dữ liệu khi load.
- Có `version` trong mọi file save; load: đọc version → migrate từng bước lên hiện tại → validate → áp dụng.
- Ghi **atomic**: ghi file tạm → flush → `File.Replace`/rename đè file thật; giữ bản backup tốt gần nhất, lỗi parse → dùng backup.
- `JsonUtility` (nhanh, không Dictionary/property) hoặc thư viện JSON dự án đã có. Vị trí `Application.persistentDataPath`.
  WebGL: `PlayerPrefs`/IndexedDB, cần `FS.syncfs` tùy phiên bản.
- Autosave ở ranh giới an toàn (qua màn, checkpoint), slot riêng, không ghi mỗi frame.
- Test bắt buộc: lưu → thoát → load khớp; load file từ phiên bản trước; file hỏng/không đọc được; ghi đè save có sẵn.

## 8. Build pipeline
- Chỉ scene trong Scene List (global hoặc của Build Profile đang build) được đóng gói; scene 0 là scene khởi động.
- Backend: Mono (lặp nhanh, desktop) / IL2CPP (AOT, cần toolchain C++ của nền tảng; bắt buộc ở nhiều nền tảng).
- Managed Stripping cao → bảo vệ code reflection bằng `link.xml` / `[Preserve]`.
- Script build: `BuildPipeline.BuildPlayer(BuildPlayerOptions{scenes, locationPathName, target, options})` hoặc
  `BuildPlayerWithProfileOptions{buildProfile, locationPathName}`; **kiểm `report.summary.result == BuildResult.Succeeded`**,
  CI dùng `EditorApplication.Exit(0|1)`.
  ```csharp
  public static class CIBuild
  {
      // -buildTarget win64 -executeMethod CIBuild.Run -out Builds/Win/Game.exe -appVersion 1.2.3 [-dev]
      public static void Run()
      {
          var args = System.Environment.GetCommandLineArgs();
          string Arg(string k, string d) { var i = System.Array.IndexOf(args, k); return i >= 0 && i + 1 < args.Length ? args[i + 1] : d; }
          PlayerSettings.bundleVersion = Arg("-appVersion", PlayerSettings.bundleVersion);
          var options = new BuildPlayerOptions
          {
              scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
              locationPathName = Arg("-out", "Builds/Game"),
              target = EditorUserBuildSettings.activeBuildTarget, // đã chọn bằng -buildTarget
              options = System.Array.IndexOf(args, "-dev") >= 0 ? BuildOptions.Development : BuildOptions.None,
          };
          var report = BuildPipeline.BuildPlayer(options);
          var ok = report.summary.result == BuildResult.Succeeded;
          Debug.Log($"[CIBuild] {(ok ? "OK" : "FAILED")} {report.summary.totalErrors} errors, {report.summary.totalSize} bytes");
          EditorApplication.Exit(ok ? 0 : 1);
      }
  }
  ```
  (cần `using System.Linq; using UnityEditor; using UnityEditor.Build.Reporting; using UnityEngine;`, đặt trong thư mục/asmdef Editor.)
- Batchmode: chọn nền tảng bằng `-buildTarget` lúc khởi động (define symbols compile đúng); không `SwitchActiveBuildTarget`.
  Một nền tảng mỗi lần chạy Editor.
- Addressables: build content (`AddressableAssetSettings.BuildPlayerContent`) **trước** player build trong CI.
- Release không bật `BuildOptions.Development`.

## 9. Audio (tóm tắt)
- Route mọi `AudioSource` qua Audio Mixer Group (Music/SFX/UI/Voice) để chỉnh âm lượng theo nhóm.
- SFX ngắn: Decompress On Load; nhạc dài: Streaming; 3D sound: mono. Pool `AudioSource` cho one-shot dày đặc.
- Âm thanh là kênh phản hồi game feel quan trọng — gắn theo event (on_hit, on_pickup), không poll.
