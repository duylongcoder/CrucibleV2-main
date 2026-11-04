# Unity C# — Chuẩn viết code

Đọc trước khi viết/sửa code. Phần theo phiên bản: áp dụng mọi tính năng **tới** phiên bản dự án (đọc
`ProjectSettings/ProjectVersion.txt`), không dùng tính năng của phiên bản mới hơn.

## Mục lục
1. Tra cứu tài liệu theo phiên bản
2. Cấu trúc & đặt tên
3. MonoBehaviour & serialization
4. Vòng đời (execution order)
5. Event
6. Coroutine & async
7. Null & object đã destroy
8. Hot path & GC
9. IL2CPP, reflection, stripping
10. Code theo phiên bản Unity
11. Tính năng theo phiên bản
12. Comment & diagnostics

## 1. Tra cứu tài liệu theo phiên bản
Phiên bản `6000.3.7f1` → `<SHORT>` = `6000.3`.
1. Local (nếu đã cài module Documentation qua Unity Hub): thư mục `Documentation/` trong thư mục cài Editor —
   macOS thường ở `/Applications/Unity/Hub/Editor/<FULL>/Documentation/`; Windows tìm trong
   `C:\Program Files\Unity\Hub\Editor\<FULL>\` (`find`/`dir /s Documentation`).
2. Web: `https://docs.unity3d.com/<SHORT>/Documentation/Manual/` và `.../ScriptReference/`.
3. Mã nguồn C# của engine: `github.com/Unity-Technologies/UnityCsReference` (nhánh `<SHORT>`).
4. Package đã cài: `Library/PackageCache/<package>@<ver>/` (đọc README, source).
5. Unity Discussions: thêm `/print` vào URL để đọc đủ.
API không tìm thấy trong tài liệu đúng phiên bản → nói rõ "chưa xác minh", không khẳng định từ trí nhớ.

## 2. Cấu trúc & đặt tên
- Runtime code trong thư mục có asmdef runtime; editor code trong `Editor/` hoặc asmdef `includePlatforms: ["Editor"]`.
- Một public type mỗi file; tên file MonoBehaviour/ScriptableObject **bằng** tên class (không thì Unity không gắn được).
- Namespace theo thư mục (theo quy ước dự án trong `CLAUDE.md`).
- `PascalCase` cho type/method/property/public; `_camelCase` private field; `camelCase` local/tham số; hằng `PascalCase` (hoặc theo dự án).
- Không dùng `System.Reflection.Emit` (IL2CPP không hỗ trợ).

## 3. MonoBehaviour & serialization
- Field cho Inspector: `[SerializeField] private` (không `public` chỉ để hiện). Hoặc property theo dự án:
  ```csharp
  [field: SerializeField, Tooltip("Tốc độ di chuyển (unit/s)")]
  public float MoveSpeed { get; private set; } = 6f;
  ```
  Backing field serialize thành `<MoveSpeed>k__BackingField`.
- Tham số chỉnh bằng cách chơi thử (tốc độ, thời gian spawn...) → `SerializeField` có `[Tooltip]` hoặc `const` có
  comment. Giá trị gameplay dùng chung → ScriptableObject config.
- `[Header]`, `[Range]`, `[Min]`, `[Tooltip]` để Inspector dễ dùng; `[RequireComponent]` cho phụ thuộc bắt buộc.
- Class thường muốn serialize: `[System.Serializable]`. Interface/đa hình: `[SerializeReference]`.
- **Đổi tên field serialized**: `[FormerlySerializedAs("oldName")]` (namespace `UnityEngine.Serialization`). Giữ
  attribute ít nhất tới khi mọi asset đã được lưu lại.
- Đổi tên/namespace type dùng trong `[SerializeReference]` → `[MovedFrom(...)]`.
- `TryGetComponent<T>(out var c)` thay `GetComponent` + null check.
- `CompareTag("Enemy")` thay `tag == "Enemy"`.
- Instance prefab lặp lại → đặt `name` duy nhất để test tìm được.

## 4. Vòng đời
| Hàm | Khi nào | Dùng cho |
|---|---|---|
| `Awake` | Một lần khi load (kể cả component disabled; không chạy khi GameObject inactive) | Cache component, tự khởi tạo |
| `OnEnable` | Mỗi lần enable | Đăng ký event |
| `Start` | Một lần trước Update đầu, chỉ khi enabled; sau **mọi** `Awake` | Nối với object khác |
| `FixedUpdate` | Mỗi bước physics (mặc định 0.02 s; 0..n lần/frame) | Lực, `MovePosition`, velocity |
| `Update` | Mỗi frame | Đọc input, logic không physics |
| `LateUpdate` | Sau mọi Update + animation | Camera follow, ghi đè animation |
| `OnDisable` | Mỗi lần disable/destroy | Hủy event |
| `OnDestroy` | Khi destroy | Giải phóng tài nguyên |

- Thứ tự giữa các object (kể cả cùng class) **không đảm bảo**; cần thứ tự → Script Execution Order hoặc khởi tạo tường minh.
- `Awake` không được dựa vào state do `Awake` object khác tạo; `Start` thì được.
- `SceneManager.sceneLoaded` chạy sau `OnEnable`, trước `Start`.
- Input đọc trong `Update`, áp dụng trong `FixedUpdate`.
- Callback render `OnPreRender/OnPostRender/OnRenderImage` chỉ Built-in; URP/HDRP dùng `RenderPipelineManager.*`.
- `OnGUI` rỗng vẫn tốn mỗi frame → xóa. `OnMouse*` chỉ chạy với Input Manager cũ.
- Khởi tạo không cần MonoBehaviour: `[RuntimeInitializeOnLoadMethod]` (runtime), `[InitializeOnLoad]`/`[InitializeOnLoadMethod]` (editor).
- Domain Reload tắt (Enter Play Mode Options, mặc định cho dự án mới từ Unity 6.6) → biến `static` và state SO **không
  reset** khi Play; reset trong `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.

## 5. Event
- Ưu tiên `System.Action` / `Action<T>` (theo dự án nếu dùng UnityEvent / SO channel).
- Tên event dạng phân từ: `DoorOpening` (trước), `DoorOpened` (sau). Hàm raise: `OnDoorOpened()`. Handler bên nghe:
  `<Subject>_<Event>` (vd `Door_Opened`).
- Đăng ký `OnEnable` ↔ hủy `OnDisable`; đăng ký `Awake`/`Start` ↔ hủy `OnDestroy`. Không hủy → handler chạy trên
  object đã destroy hoặc chạy 2 lần.
- Invoke an toàn: `Opened?.Invoke(x)`. Duyệt danh sách listener ngược khi listener có thể tự hủy trong handler.

## 6. Coroutine & async
- Coroutine dừng khi GameObject `SetActive(false)` hoặc bị destroy; **không** dừng khi chỉ disable component. Giữ
  handle `Coroutine` để `StopCoroutine` chính xác; re-start trong `OnEnable` nếu cần.
- Cache `new WaitForSeconds(x)` dùng lại trong vòng lặp. `WaitForSecondsRealtime` cho menu pause (`timeScale = 0`).
- Unity 2023.1+: ưu tiên `async Awaitable` (theo dự án nếu dùng UniTask):
  `Awaitable.NextFrameAsync(ct)`, `WaitForSecondsAsync(t, ct)`, `FixedUpdateAsync(ct)`, `EndOfFrameAsync(ct)`,
  `BackgroundThreadAsync()` / `MainThreadAsync()`.
- Async **không** tự dừng khi object destroy → truyền `destroyCancellationToken`. Ngoài MonoBehaviour: `CancellationTokenSource`
  hủy trong `OnDestroy`/`Dispose`. Thoát Play Mode: `Application.exitCancellationToken`.
- `Awaitable` được pool → chỉ await **một lần** mỗi instance. `async Awaitable` cho hàm tự gọi; `async void` chỉ cho
  message Unity (`Start`), bọc `try/catch` (exception trong `async void` không có ai bắt).
- Trong `try` có await với token nhận từ tham số: bắt `OperationCanceledException` và `throw;` **trước** `catch (Exception)`
  để hủy lan ra ngoài.
- Sau `BackgroundThreadAsync`, không gọi API Unity cho tới khi `MainThreadAsync`.

## 7. Null & object đã destroy
- `UnityEngine.Object` override `==` và bool: object đã destroy `== null` là `true` dù tham chiếu C# chưa null.
- `?.`, `??`, `is null`, `ReferenceEquals` **bỏ qua** override đó → không dùng với `UnityEngine.Object` khi cần biết đã
  destroy. Dùng `if (obj == null)` / `if (!obj)`.
- `Destroy` có hiệu lực cuối frame; `DestroyImmediate` chỉ trong editor code.

## 8. Hot path & GC
Trong `Update`/`FixedUpdate`/`LateUpdate`/callback physics/hàm gọi mỗi frame:
- Không `GameObject.Find*`, `FindObject(s)ByType`, `GetComponent*`, `Camera.main` (đời cũ) → cache ở `Awake`/`Start`
  hoặc dùng runtime set/registry.
- Không cấp phát: string nối/format, LINQ, closure/lambda bắt biến, boxing (enum làm key Dictionary không có comparer,
  struct qua interface), `new List/Array` (dùng `Clear()` và tái sử dụng), `foreach` trên collection không phải List/Array cũ.
- Physics: `RaycastNonAlloc`, `OverlapSphereNonAlloc` (hoặc overload nhận buffer).
- Spawn/despawn liên tục → `UnityEngine.Pool.ObjectPool<T>` (2021.1+), `ListPool<T>`, `HashSetPool<T>`.
- `Span<T>`/`stackalloc` cho buffer tạm (2021.2+). `MathF` cho float.
- Update không có việc → không khai báo `Update`; dùng event, timer, hoặc chạy thưa (mỗi N frame/giây).
- `Animator.StringToHash`, `Shader.PropertyToID` cache vào `static readonly int`.
- Mục tiêu: 0 B GC alloc/frame ở trạng thái ổn định.

## 9. IL2CPP, reflection, stripping
- Method chỉ gọi qua reflection → `[UnityEngine.Scripting.Preserve]`; assembly/type dùng reflection/JSON → `link.xml`.
- Lỗi `MissingMethodException`/`TypeLoadException` chỉ trên build → nghi stripping.
- Generic virtual method / generic với value type có thể cần AOT hint trên IL2CPP.

## 10. Code theo phiên bản Unity
- Code trong `Assets/` build bằng **một** phiên bản → viết thẳng cho phiên bản đó, không `#if UNITY_x_OR_NEWER`.
- Code trong `Packages/` (UPM package hỗ trợ dải phiên bản): code chỉ cần ở một phía mốc phiên bản → bọc
  `#if UNITY_<ver>_OR_NEWER` (không có `_OR_OLDER`: dùng `#if !UNITY_<ver>_OR_NEWER`), kèm comment nói phiên bản
  đó thay đổi gì. API chỉ deprecated nhưng vẫn chạy mọi phiên bản → `#pragma warning disable CS0618` + comment.
- Có/không có package → `versionDefines` trong asmdef (vd `ENABLE_UNITASK`), không dùng `#if` phiên bản Unity.
- Seam chỉ cho test → `#if UNITY_INCLUDE_TESTS`.

## 11. Tính năng theo phiên bản (dùng mọi thứ tới phiên bản dự án)
| Từ | Dùng | Thay cho |
|---|---|---|
| 2020.2 | C# 8: switch expression, property/tuple pattern; `[NonReorderable]`; `ProfilerRecorder` | switch statement dài, cast + check |
| 2021.1 | `UnityEngine.Pool` (`ObjectPool<T>`, `ListPool<T>`...) | pool tự viết |
| 2021.2 | .NET Standard 2.1: `Span<T>`, `array[^1]`, `array[1..4]`, `MathF`; C# 9: `new()`, `init`, `record` | `new T[]` tạm, `Skip/Take`, `(float)Math.*` |
| 2022.3 | `FindObjectsByType<T>(...)`, `FindAnyObjectByType<T>()` | `FindObjectOfType`, `FindObjectsOfType` (obsolete) |
| 2023.1 | `Awaitable`, `destroyCancellationToken`, `didAwake`/`didStart` | Coroutine cho luồng tuần tự |
| 2023.2 | TextMeshPro gộp vào uGUI 2.0 — không thêm package `com.unity.textmeshpro` riêng | |
| 6000.0 | Physics đổi tên: `linearVelocity`, `linearDamping`, `angularDamping`, `PhysicsMaterial`; Unity Test Framework là core package (từ 6000.0.44f1) | `velocity`, `drag`, `PhysicMaterial` |
| 6000.0 | 2D: `Collider2D.compositeOperation` | `usedByComposite` |
| 6000.3 | UI Test Framework (`com.unity.test-framework.ui`) cho test UI Toolkit | |
| 6000.4 | `EntityId` thay `InstanceID`/`GetInstanceID()` (không cast int, không dựa hash/thứ tự); `FindObjectsByType<T>()` bỏ tham số `FindObjectsSortMode`; `FindFirstObjectByType` obsolete → `FindAnyObjectByType`; ECS packages là core | |

Input: dự án mới dùng Input System package (`com.unity.inputsystem`), không `Input.GetKey` (xem `unity-subsystems.md`).
Phiên bản mới hơn bảng này → tra tài liệu đúng phiên bản trước khi dùng API.

## 12. Comment & diagnostics
- Comment chỉ để giải thích **vì sao** khi code không tự nói được. Comment "why not" bắt buộc khi cố ý tránh cách
  chuẩn (cách chuẩn hỏng trong môi trường này, chọn cách dài hơn vì đúng hơn, guard trông thừa nhưng cần).
- XML doc cho public API; triển khai interface/override → `/// <inheritdoc/>`.
- Shortcut có giới hạn đã biết: `// shortcut: <giới hạn>, <khi nào nâng cấp>`.
- Diagnostic/review không hợp ngữ cảnh: được suppress ở phạm vi hẹp nhất (`#pragma warning disable <ID>` cho analyzer
  Roslyn; `// ReSharper disable once <Inspection>` chỉ cho inspection ReSharper), kèm comment lý do, và **báo người dùng**.
- Không `try/catch` nuốt lỗi; log bằng `Debug.LogException(e, this)` hoặc ném lại.
