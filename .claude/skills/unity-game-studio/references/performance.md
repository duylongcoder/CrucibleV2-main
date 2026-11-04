# Hiệu năng Unity — đo, ngân sách, sửa

## 1. Ngân sách khung hình
```
60 FPS = 16.67 ms   |   30 FPS = 33.3 ms   |   120 FPS = 8.33 ms
CPU và GPU chạy song song; cái chậm hơn quyết định FPS.
Ví dụ @60: scripts ~5 ms · physics ~3 ms · render submit (CPU) ~4 ms · UI/khác ~2 ms · dư phòng ~2.6 ms
GC alloc ở trạng thái ổn định: 0 B/frame
```
Thêm: draw call/SetPass, bộ nhớ (texture, mesh, audio), thời gian load scene, kích thước build. Ghi ở `Docs/TECH.md` §6.

## 2. Đo
- **Profiler** (CPU Usage, GPU Usage, Rendering, Memory). Deep Profile chỉ khi cần (làm méo số).
- **Frame Debugger**: từng draw call, vì sao batch bị vỡ.
- **Memory Profiler** (package) cho rò rỉ/bộ nhớ tăng không giải thích được.
- **Project Auditor** (tích hợp Editor từ Unity 6.4, package trước đó) — soát tĩnh code & cài đặt tốn kém.
- Đo thật: **Development build + Autoconnect Profiler trên máy mục tiêu**. Số trong Editor bị nhiễu bởi Editor — ghi rõ khi chỉ đo được trong Editor.
- Code: `ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15)`;
  bộ đếm `"CPU Main Thread Frame Time"`, `"GPU Frame Time"` (bật Frame Timing Stats trong Player Settings cho build release),
  `"GC Allocated In Frame"` (chỉ development); `FrameTimingManager`.
- Đọc split: CPU main thread ≈ frame time → nút cổ chai CPU (scripts/physics/animation/UI); GPU frame time ≈ frame time → GPU
  (fill rate, overdraw, shader, độ phân giải, post-processing).

## 3. Quy trình
1. Mục tiêu bằng số + tình huống tệ nhất tái hiện được.
2. Đo trước khi sửa.
3. CPU hay GPU?
4. Sửa chi phí lớn nhất; ưu tiên thuật toán (làm ít hơn) hơn vi tối ưu.
5. Đo lại cùng scene/máy/loại build; giữ hoặc hoàn tác theo số.
6. Ghi số trước/sau; đặt ngân sách/test để không tụt lại.

## 4. Sửa — CPU
- **Script**: cache tham chiếu (không `Find*`/`GetComponent` mỗi frame); không alloc mỗi frame (string, LINQ, closure,
  boxing, `new` collection — tái sử dụng, `ListPool`); bỏ `Update` rỗng; chạy logic đắt theo timer/mỗi N frame, chia
  qua nhiều frame (time-slicing); spatial partition (grid/quadtree) thay vì duyệt mọi object; runtime set thay `FindObjectsByType`.
- **Pool** mọi thứ spawn dày (đạn, hạt, số damage, enemy theo đợt, audio one-shot): `ObjectPool<T>` với prewarm lúc load,
  giới hạn kích thước, chính sách khi tràn.
- **Physics**: Layer Collision Matrix tối giản; collider primitive thay MeshCollider; `NonAlloc` queries; giảm số Rigidbody
  hoạt động; Fixed Timestep hợp lý; tránh `transform.position` trên Rigidbody.
- **Animation**: Animator trên vật ngoài màn hình → Culling Mode `CullUpdateTransforms`/`CullCompletely`; ít layer/parameter.
- **UI (uGUI)**: tách Canvas tĩnh/động (một element đổi làm rebuild cả Canvas); tránh Layout Group lồng sâu và
  ContentSizeFitter dư; tắt `raycastTarget` ở Graphic không nhận input; ẩn bằng `CanvasGroup.alpha` hoặc tắt Canvas thay vì
  SetActive liên tục trên cây lớn; pool phần tử list.
- **GC**: Incremental GC bật; mục tiêu 0 B/frame; kiểm bằng `Is.Not.AllocatingGCMemory()` trong test cho hàm nóng.

## 5. Sửa — GPU / Rendering
- **Draw call**: atlas texture + dùng chung material để batch (2D: Sprite Atlas); URP/HDRP: SRP Batcher (shader tương thích);
  nhiều mesh giống nhau → GPU Resident Drawer (Forward+, API có compute, không OpenGL ES) hoặc GPU instancing (bị SRP
  Batcher ưu tiên trước với shader tương thích); static batching cho vật không di chuyển; dynamic batching không còn được
  khuyến nghị ngoài thiết bị yếu.
- **Overdraw**: giảm lớp trong suốt/hạt/full-screen chồng nhau; sprite mesh type Tight; cắt vùng trong suốt.
- **Ánh sáng/bóng**: bake ánh sáng tĩnh; giới hạn real-time light & shadow caster; khoảng cách shadow hợp lý.
- **Fill rate**: giảm render scale/độ phân giải trên thiết bị yếu; giảm post-processing đắt.
- **LOD, culling**: LOD Group cho 3D, occlusion culling cho scene phức tạp, tắt object xa.

## 6. Bộ nhớ & load
- Texture: kích thước theo nhu cầu hiển thị thật, nén theo nền tảng (ASTC mobile, BC desktop), tắt Read/Write, mipmap chỉ
  cho 3D/scale động.
- Audio: Load Type theo độ dài (Decompress On Load cho SFX ngắn, Compressed In Memory/Streaming cho nhạc), mono cho 3D,
  sample rate vừa đủ.
- Mesh: tắt Read/Write nếu không cần; nén mesh.
- Load: additive scene + load async (`SceneManager.LoadSceneAsync`, Addressables `LoadAssetAsync`); tránh `Resources` folder
  lớn (đóng gói mọi thứ, tăng thời gian khởi động); prewarm pool/shader lúc màn hình loading.
- Rò rỉ: event không hủy giữ object; static collection giữ tham chiếu; Addressables load không `Release`.

## 7. Bẫy
- Tối ưu không đo; đo trong Editor rồi kết luận cho thiết bị; sửa nhầm phía CPU/GPU; vi tối ưu khi vấn đề là thuật toán;
  tối ưu prototype chưa biết có vui không; không có ngân sách nên hiệu năng tụt dần không ai biết.
