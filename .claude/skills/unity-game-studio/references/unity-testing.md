# Unity Test Framework — Thiết kế & viết test

## Mục lục
1. Cấu trúc & chế độ
2. Thiết kế test (chọn lớp, kỹ thuật, phân vùng)
3. Random, thời gian, PRNG
4. Viết test (quy ước, constraint, async)
5. Multi-frame & UI
6. Test double & phần không public
7. Log, GameObject, dọn dẹp
8. Attribute chọn lọc & thời gian
9. Performance & GC
10. Lỗi thường gặp

## 1. Cấu trúc & chế độ
- **EditMode** (`Assets/Tests/EditMode/`, asmdef `includePlatforms: ["Editor"]`): logic thuần C# (không cần vòng
  đời MonoBehaviour), editor tools, kiểm tra asset. Nhanh nhất — ưu tiên.
- **PlayMode** (`Assets/Tests/PlayMode/`, asmdef mọi nền tảng): vòng đời MonoBehaviour, physics, coroutine/async,
  scene, UI. Chạy được trên player build.
- Chế độ do **asmdef/thư mục** quyết định, không do attribute: `[UnityTest]` chạy được ở cả hai.
- Một SUT không nên chia test ở cả hai chế độ (hai runner không chạy cùng lúc).
- Test asmdef: tham chiếu assembly production + `UnityEngine.TestRunner`, `UnityEditor.TestRunner`;
  `precompiledReferences: ["nunit.framework.dll"]`, `overrideReferences: true`, `autoReferenced: false`,
  `defineConstraints: ["UNITY_INCLUDE_TESTS"]`. Tạo bằng `scripts/create_test_asmdef.py`.
- Code game nằm trong `Assembly-CSharp` (không asmdef) → test asmdef không tham chiếu được → cần asmdef cho code game.
- Test `internal`: `[assembly: InternalsVisibleTo("<Test asm>")]` ở `AssemblyInfo.cs` cạnh asmdef production.
- Thư mục phụ: `TestDoubles/` (stub, spy, fake), `TestUtils/` (creation method, assertion riêng), `Scenes/` (scene test).

## 2. Thiết kế test
**Chọn lớp** (theo mức tích hợp, thấp nhất có thể):
1. Editor test — editor extension, kiểm tra asset (SO hợp lệ, prefab đủ component, nhất quán chéo asset).
2. Unit — chạy bằng lời gọi trực tiếp (kể cả hành vi theo Awake/Start/Update). Kiểm kỹ đơn vị ít tích hợp; đơn vị
   tích hợp cao thì ít test, tập trung tương tác.
3. Integration — scene/prefab (hoặc hierarchy dựng trong test) + component + asset liên kết: event system nhiều frame,
   chuyển scene, asset linkage, chuỗi thao tác UI, chặn modal, **layout assertion** (không chồng lấn, không tràn khung,
   nằm trong màn hình/container cha, bấm tới được, cỡ chữ BestFit không dưới ngưỡng).
4. Visual verification — ảnh chụp + phân tích ảnh: vị trí tương đối, typography, contrast, trạng thái hiển thị (nút
   disabled xám), chất lượng render (méo sprite, bóng ma, cắt mép), một khung hình xác định của animation.
5. Manual — chỉ khi không tự động/không phân tích ảnh được: feel, độ mượt animation, cân bằng âm thanh.
Thiết kế integration trước khi rơi xuống visual/manual. Thuộc tính hình học kiểm bằng assertion → không làm thêm test ảnh cho cùng thuộc tính.

**Kỹ thuật**: phân vùng tương đương (phân vùng không hợp lệ mà spec chưa nói kết quả → hỏi, không bịa) · giá trị
biên (gộp vào test tham số hóa của phân vùng; spec không phân biệt gần biên thì bỏ) · chuyển trạng thái (mỗi chuyển
trực tiếp A→B một lần) · bảng quyết định (nhiều điều kiện kết hợp) · đoán lỗi (bấm liên tục, bấm đồng thời, input khi
chuyển scene/loading, xuyên tường, phân phối random lệch/chu kỳ lặp, tràn số, mất mạng).

**Ưu tiên test theo đặc tả** hơn test theo cấu trúc cài đặt (dễ vỡ khi refactor).

**Một method = một phân vùng = một kết quả xác định.** Không tham số hóa *kết quả mong đợi*. Nếu kết quả đổi theo
input → hai phân vùng → hai method. Trong một phân vùng, gộp đại diện + giá trị biên vào một test tham số hóa.

Ví dụ `drawPileButton.interactable` theo số lá bài:
| Test | Kiểm tra |
|---|---|
| `Sync_DrawPileIsEmpty_DrawPileButtonIsNotInteractable` | nút không bấm được |
| `Sync_DrawPileHasCards_DrawPileButtonIsInteractable` (count: {1, 5}) | nút bấm được |

**Ký hiệu tham số trong thiết kế**: `(flag: (bool))` mọi giá trị bool · `(dir: (Direction))` mọi giá trị enum ·
`(a: {0,1}, b: {3,4})` tổ hợp đầy đủ · `({a:0,b:3}, {a:1,b:4})` tổ hợp chọn lọc · `(use pairwise)` ≥3 tham số nhiều giá trị.

**Cột "Kiểm tra" khi thiết kế** chỉ ghi thuộc tính quan sát được + giá trị mong đợi. Không ghi: attribute, sync/async,
cách dựng input, điều kiện đã có trong tên test, tên helper, tên field private, lý do tồn tại test.

**Phụ thuộc trả lỗi**: thư viện/framework bên ngoài → test (stub lỗi); code nội bộ dự án → bỏ qua; không chắc → hỏi.

**Bug**: một test tái hiện (fail trước, pass sau) + regression test cho hành vi lân cận. **Refactor**: cover rồi mới modify.

**Truy vết**: mỗi yêu cầu có test **cùng lớp** chứng kiến trực tiếp (yêu cầu về UI → integration/visual, không phải unit).

**Testability** (khi thiết kế): `PASS` / `WARN` (nhiều test double, integration lớn, tổ hợp FSM nhiều) / `FAIL` (không vạch
được ranh giới unit, state ẩn, số test tăng nhanh hơn tuyến tính, không inject được phụ thuộc).

## 3. Random, thời gian, PRNG
- Không dùng `UnityEngine.Random` không seed hoặc `Time.time` thật trong logic cần test → inject `System.Random(seed)` /
  `IRandomSource`, `ITimeProvider`.
- Chiến lược: **stub PRNG** khi spec ánh xạ xác định (≥0.5 → mặt ngửa) · **kiểm khoảng** khi chỉ spec khoảng
  (`Is.InRange`, kèm `[Repeat]`) · **kiểm thống kê** khi spec phân phối (lấy mẫu N lần, kiểm mean/histogram, seed cố định) ·
  **kiểm đặc tính** cho nội dung thủ tục (đường từ start tới exit tồn tại, ràng buộc của thuật toán), không so output chính xác.
- Cùng seed → cùng kết quả: test riêng cho tính tái lập (quan trọng cho seed chia sẻ được).

## 4. Viết test
- Class `<ClassName>Test` (hoặc theo dự án), `[TestFixture]`; namespace = namespace SUT.
- Method: `<Method>_<Condition>_<Expected>` (unit, editor test cho method); `<Condition>_<Expected>` (integration,
  visual, editor test cho asset). `<Method>` khớp đúng tên method production. `<Expected>` chủ động, cụ thể
  (`ReturnsZero`, `ThrowsArgumentException`, `RaisesDeathEvent`), không dùng từ điều kiện (`Matches…`, `…WhenX`).
- Biến: `sut`, `actual`, `expected`; test double tiền tố vai trò `stub`/`spy`/`fake`/`dummy`.
- Arrange / Act / Assert tách bằng dòng trống (comment AAA tùy dự án).
- **Constraint model**: `Assert.That(actual, Is.EqualTo(expected))`, không `Assert.AreEqual`.
  Sai số: `.Within(0.01f)` hoặc `.Using(new FloatEqualityComparer(0.01f))`; `Vector3EqualityComparer`,
  `QuaternionEqualityComparer`, `ColorEqualityComparer`. Collection: `Has.Some.Matches<T>(...)`, `Is.EquivalentTo`,
  `Has.None.Matches<T>`. Exception: `Assert.That(() => sut.Do(-1), Throws.TypeOf<ArgumentException>())`.
- Một `Assert.That` mỗi test; ngoại lệ: nhiều thuộc tính của **một** trạng thái sau chuyển đổi → nhiều assert, mỗi
  assert có `message` ngắn.
- **Không control flow** trong test (`if/switch/for/foreach/?:`). Ngoại lệ: vòng `while` chờ trạng thái (tốt hơn là
  tách thành helper async), `try/catch` kiểm exception async.
- Async (UTF ≥ 1.3, khuyến nghị ≥ 1.4.6, Unity ≥ 2023.1 cho `Awaitable`): `[Test] public async Task X()` +
  `await Awaitable.NextFrameAsync()`. Dự án cũ hơn → `[UnityTest] IEnumerator` + `yield return null`.
  Exception từ async: `try { await sut.DoAsync(); Assert.Fail("..."); } catch (ArgumentException e) { ... }`.
- Tham số hóa: `[TestCase]`, `[TestCaseSource]` (+ `TestCaseData.SetName`), `[Values]`, `[ValueSource]`, `[Pairwise]`.
  Không dùng `[Sequential]`. Không tham số hóa giá trị mong đợi.
- Creation method `CreateSut(...)`: tham số mặc định **trung tính** (0, null, false, rỗng) — không đặt giá trị "trông hợp
  lý" (hp = 79) làm mặc định; không có giá trị trung tính → bắt buộc truyền.
- `[SetUp]` lo khởi tạo/dọn trạng thái sót; `[TearDown]` giải phóng tài nguyên (vd object `DontDestroyOnLoad`).
  Giá trị ảnh hưởng assertion để **trong** test, không giấu trong SetUp.
- Tiền điều kiện môi trường (scene, asset): `Assume.That(...)` → inconclusive thay vì fail.
- Không XML doc trong test.

## 5. Multi-frame & UI
- Chờ **điều kiện**, không chờ số frame/giây cố định: `while (director.IsPlaying) await Awaitable.NextFrameAsync();`
  (tốt nhất bọc vào helper `WaitUntilIdleAsync()` có timeout). Không `Thread.Sleep`, `Task.Delay`, `WaitForSeconds` để "cho chắc".
- Thao tác gây `Destroy` trễ / rebuild UI → thêm một `NextFrameAsync` trước khi assert hierarchy mới.
- Layout: `Canvas.ForceUpdateCanvases()` rồi `await Awaitable.NextFrameAsync()` (raycast cần một lần render thật).
- Thao tác UI nên đi qua `EventSystem` (giống người dùng thật), không gọi thẳng `onClick.Invoke()` khi kiểm luồng
  người dùng. Nếu dự án có package `com.nowsprinting.test-helper.ui`: `GameObjectFinder` (chờ object xuất hiện, kiểm
  bấm tới được/bị modal che), `UguiClickOperator`, `UguiTextInputOperator`, constraint layout. **Không tự thêm package** —
  đề xuất và hỏi.
- Kiểm modal chặn: thử chạm phần tử phía sau khi modal mở (không tới được), đóng modal rồi thử lại (tới được).
- Test tạo GameObject: dọn trong `[TearDown]`, hoặc tạo scene test riêng. Test cần scene → load scene test
  (`Tests/Scenes/<TestClass>.unity`, dựng bằng `commands/scene.md`).
- Visual verification: chụp `ScreenCapture.CaptureScreenshot(absPath)` (PlayMode) vào thư mục kết quả, `[Category("VisualVerification")]`,
  `[Description("Sau khi chạy, kiểm ảnh theo: <các khía cạnh>")]`. Sau khi chạy, đọc ảnh và đánh giá từng khía cạnh.

## 6. Test double & phần không public
- Ưu tiên fake/stub đơn giản qua interface; hạn chế mock framework. Spy ghi lại lời gọi vào property public để assert.
- Spy MonoBehaviour: `[AddComponentMenu("")]` để không hiện trong Add Component.
- Không test `private` (đó là test cài đặt). Không dùng reflection để chạm `private`.
- `internal` chỉ khi tách logic có ≥3 điều kiện độc lập làm tổ hợp bùng nổ; `[Category("Internal")]`.
- Seam chỉ cho test bọc `#if UNITY_INCLUDE_TESTS`.
- MonoBehaviour: tách logic ra class thuần (Humble Object) trước; `MonoBehaviourTest<T>` chỉ khi cần quan sát chính vòng đời.

## 7. Log, GameObject, dọn dẹp
- `Debug.LogError`/`LogException`/`LogAssertion` làm test fail. Lỗi mong đợi từ engine/thư viện ngoài:
  `LogAssert.Expect(LogType.Error, "...")` (gọi **trước** điểm await/yield). Cuối test: `LogAssert.NoUnexpectedReceived()`.
- Log của chính code dự án: inject logger spy thay vì `LogAssert`.
- `LogAssert.ignoreFailingMessages` reset mỗi test — không đặt trong SetUp.
- Object đã destroy: `Assert.That((bool)obj, Is.False)` hoặc `Assert.That(obj == null, Is.True)`.

## 8. Attribute chọn lọc & thời gian
- `[Category("Integration")]`, `[Category("VisualVerification")]`, `[Category("Acceptance")]`, `[Category("Smoke")]`,
  `[Category("IgnoreCI")]` (phụ thuộc timing máy chậm, `GameViewResolution`); CI lọc `-testCategory "!IgnoreCI"`.
- `[UnityPlatform(exclude = new[] { RuntimePlatform.WebGLPlayer })]`, `[Ignore("lý do")]` (tạm, không để qua build),
  `[Explicit]`.
- `[Timeout(ms)]` (tính **tổng** cả lần repeat), `[MaxTime(ms)]`, `[Repeat(n)]`. `[Retry]` không phải cách sửa flaky.

## 9. Performance & GC
- Không cấp phát: `using Is = UnityEngine.TestTools.Constraints.Is;` → `Assert.That(() => sut.Tick(), Is.Not.AllocatingGCMemory());`
  (chỉ delegate đồng bộ; `Debug.Log` bên trong làm fail).
- Package `com.unity.test-framework.performance` (hỏi trước khi thêm): `[Test, Performance]`, `Measure.Method(...)`
  (chỉ delegate đồng bộ), `Measure.Frames()`, `.GC()` đếm **số lần** cấp phát chứ không phải byte; kết quả ở
  `Application.persistentDataPath/PerformanceTestResults.json`. Ngưỡng phải ghi rõ và test fail khi vượt.

## 10. Lỗi thường gặp
| Triệu chứng | Nguyên nhân |
|---|---|
| "0 tests" / test không hiện | File không nằm dưới test asmdef, asmdef thiếu `UNITY_INCLUDE_TESTS`/tham chiếu, compile error |
| `[Test] async Task` không chạy | UTF quá cũ → nâng `com.unity.test-framework` (≥1.4.6) — là thêm/đổi package: hỏi trước |
| Test PlayMode treo | Chờ điều kiện không bao giờ đúng, thiếu timeout; `Assert.ThrowsAsync` (gây treo Editor) |
| Pass ở máy dev, fail CI | Phụ thuộc độ phân giải, timing, thứ tự test, state static |
| Pass lẻ, fail khi chạy cả bộ | State rò rỉ giữa test (SO runtime value, static, `DontDestroyOnLoad`) |
