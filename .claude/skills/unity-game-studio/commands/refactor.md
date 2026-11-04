# Command: refactor

**Vai trò**: Developer · **Tham số**: `<file | class | thư mục>` + `"<mục tiêu>"` — vd `refactor PlayerController "tách logic di chuyển ra class thuần C#"`
**Ghi vào**: `Assets/**`, `Assets/Tests/**`, `Docs/TECH.md` (nếu cấu trúc đổi), `Docs/LOG.md`
**Đọc**: code mục tiêu + chỗ gọi + test, `references/unity-coding-rules.md`, `references/unity-assets-yaml.md` §Đổi tên

Refactor = **giữ nguyên hành vi**. Thay đổi tính năng không thuộc command này.

## Bước

1. **Phạm vi**: đọc code mục tiêu, mọi chỗ gọi (grep tên + `scripts/find_guid_refs.py <script.cs>` để tìm
   scene/prefab/SO tham chiếu), test hiện có. Xác định public API & hợp đồng giữa hệ thống — giữ nguyên trừ khi
   được yêu cầu đổi; buộc phải đổi → liệt kê mọi điểm bị ảnh hưởng và hỏi.
2. **Lưới an toàn**: phần code sẽ đổi đã có test bao phủ hành vi chưa? Chưa → viết **characterization test**
   ghi lại hành vi hiện tại (kể cả hành vi có vẻ lạ — ghi chú, không sửa), chạy xanh, commit trước
   (`test(<scope>): characterize ...`).
3. **Kế hoạch bước nhỏ**: liệt kê chuỗi bước, mỗi bước một kỹ thuật an toàn: đổi tên, trích method, trích logic
   thuần C# ra class riêng (Humble Object), thay magic number bằng const/SO config, guard clause thay if lồng,
   thay `Find`/`GetComponent` lặp lại bằng cache, tách event subscribe đúng vòng đời. Thay đổi lớn (kiến trúc, nhiều
   hệ thống) → trình kế hoạch, chờ xác nhận.
4. **Thực hiện từng bước**, sau mỗi bước: compile + chạy test. Fail → dừng, tìm nguyên nhân trước khi đi tiếp
   (không chồng thêm sửa đổi lên trạng thái đỏ).
5. **Luật Unity khi refactor**
   - Đổi tên field serialized/public → `[FormerlySerializedAs("tenCu")]` (giữ dữ liệu đã gán trong scene/prefab).
   - Đổi tên/di chuyển file script → qua Editor hoặc `git mv` **cả `.meta`** để giữ GUID; tên file = tên class MonoBehaviour.
   - Đổi namespace/tên class MonoBehaviour/ScriptableObject: GUID giữ nguyên nếu `.meta` đi theo; nhưng
     `[SerializeReference]` lưu tên type → cần `[MovedFrom]` (UnityEngine.Scripting.APIUpdating).
   - Chuyển field giữa component / đổi kiểu serialized → dữ liệu cũ mất; cần bước migrate (editor script đọc giá
     trị cũ ghi sang mới) — báo người dùng và kiểm tra prefab/scene sau đó.
   - Xóa script: kiểm GUID refs trước; còn tham chiếu → không xóa, báo danh sách.
6. **Kiểm tra cuối**: toàn bộ test liên quan xanh; `check_unity_project.py` không ERROR (missing-script,
   moved-without-meta); mở scene/prefab bị ảnh hưởng không có "Missing (Mono Script)" (qua backend hoặc nhờ người dùng).
7. Commit `refactor(<scope>): ...` — không trộn với thay đổi tính năng. Vấn đề phát hiện ngoài phạm vi → ghi vào
   báo cáo + TECH §8 Nợ kỹ thuật, không sửa.

## Định nghĩa hoàn thành
- Hành vi không đổi (test chứng minh); public API giữ nguyên hoặc thay đổi đã được duyệt và liệt kê đủ điểm ảnh hưởng.
- Không mất tham chiếu serialized; không `.meta` mồ côi/thiếu.
- Báo cáo: các bước đã làm, test thêm, điểm cần kiểm tra thủ công trong Editor.
