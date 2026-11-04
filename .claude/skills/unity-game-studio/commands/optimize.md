# Command: optimize

**Vai trò**: Developer (+ DevOps đo trên build, Designer cho phần art/texture) · **Tham số**: `[scene | hệ thống | "memory" | "load" | "build-size"]` · `--audit` (soát tĩnh code & cài đặt, chưa đo) · `budget` (đặt ngân sách)
**Ghi vào**: `Docs/TECH.md` §6 (ngân sách + số đo), §8 (nợ), code/cài đặt khi được duyệt · **Đọc**: `references/performance.md`, TECH §6

## Nguyên tắc
Đo trước, sửa sau. Không có số đo → không có tối ưu. Báo cáo bằng số trước/sau trên **cùng scene, cùng máy, cùng
loại build** — không bao giờ "chắc sẽ nhanh hơn".

## budget
Hỏi FPS mục tiêu + nền tảng thấp nhất. Chia ngân sách khung hình (vd 60 FPS = 16.67 ms: scripts ~5, physics ~3,
render submit ~4, UI/khác ~2, dư phòng), GC alloc ổn định 0 B/frame, draw call/SetPass, bộ nhớ, thời gian load, kích
thước build. Ghi TECH §6.

## Đo & tối ưu (mặc định)
1. **Mục tiêu & tái hiện**: con số mục tiêu; scene/tình huống tệ nhất tái hiện được ("thỉnh thoảng giật" → tìm
   thao tác cụ thể gây giật).
2. **Đo**:
   - Có backend chạy C# trong Editor / người dùng mở Profiler: Profiler (CPU, GPU, Rendering, Memory), Frame Debugger
     cho draw call. Không bật Deep Profile trừ khi cần (làm sai số liệu).
   - Đo thật nên trên **Development build + Autoconnect Profiler trên máy mục tiêu**; số đo trong Editor phải ghi
     "đo trong Editor".
   - Đo bằng code (khi cần tự động): `ProfilerRecorder` với `"Main Thread"`/`"CPU Main Thread Frame Time"`,
     `"GPU Frame Time"` (cần bật Frame Timing Stats), `"GC Allocated In Frame"` (chỉ development); hoặc performance
     test (`com.unity.test-framework.performance` — package mới phải hỏi).
   - Không tự đo được → đưa người dùng checklist đo (mở gì, nhìn cột nào, chụp gì) rồi phân tích số họ gửi.
3. **CPU hay GPU?** Cái nào gần frame time hơn là bottleneck. Sửa nhầm phía = không có tác dụng.
4. **Sửa chi phí lớn nhất trước**, ưu tiên giải pháp thuật toán (làm ít việc hơn, cache, chạy thưa hơn, chia nhỏ qua
   nhiều frame, spatial partition) hơn vi tối ưu. Danh mục sửa phổ biến: `references/performance.md` §Sửa.
5. **Đo lại**, giữ hoặc hoàn tác theo số liệu. Mỗi tối ưu: một commit `perf(<scope>): ...` với số trước/sau.
6. Cập nhật TECH §6 (cột Hiện tại + ngày đo). Có cách giữ cố định → đề xuất performance test / kiểm tra trong CI.

## `--audit` (tĩnh, chưa đo)
Quét code & cài đặt tìm ứng viên (không phải kết luận): `Find*`/`GetComponent`/`Camera.main` trong Update,
LINQ/`string +`/`new List` trong hàm per-frame, `Instantiate/Destroy` lặp (thiếu pool), `Update()` rỗng, `OnGUI` rỗng,
`Physics.*All` thay vì `NonAlloc`, texture không nén/quá lớn/Read-Write bật, audio Load Type sai, Canvas lớn rebuild
liên tục, Layout Group lồng sâu, Raycast Target thừa, real-time light/shadow quá nhiều, thiếu static batching/
atlas, MeshCollider lõm trên vật động. Nếu dự án có Project Auditor → dùng kết quả của nó. Báo cáo xếp hạng theo
tác động ước tính + chi phí sửa, nói rõ "cần đo để xác nhận".

## Định nghĩa hoàn thành
- Mỗi thay đổi có số trước/sau và nơi đo; tối ưu không có tác dụng đã hoàn tác.
- TECH §6 cập nhật. Hành vi game không đổi (test xanh).
