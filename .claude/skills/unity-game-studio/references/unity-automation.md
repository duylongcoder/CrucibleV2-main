# Điều khiển Unity Editor, chạy test, chụp màn hình

## Mục lục
1. Chọn backend
2. Luật cứng khi gọi Editor
3. Compile
4. Chạy test
5. Chạy C# trong Editor (editor script)
6. Chụp màn hình
7. Log
8. Batchmode CLI — tham khảo nhanh

## 1. Chọn backend
Kiểm tra theo thứ tự, dùng cái đầu tiên có sẵn; ghi lựa chọn vào `CLAUDE.md` §Môi trường.

| # | Backend | Nhận biết | Làm được |
|---|---|---|---|
| A | **Unity MCP** (plugin `com.ivanmurzak.unity.mcp`, CLI `unity-mcp-cli`) | Có tool MCP tên `tests-run`, `console-get-logs`, `script-execute`... hoặc lệnh `unity-mcp-cli` chạy được, Editor đang mở | Test, log Console, chạy C# (Roslyn), screenshot Game/Scene/camera, thao tác scene/prefab/asset, Play Mode |
| B | **Rider MCP** (Rider 2026.2+ + plugin "MCP Server Extension for Unity") | Có tool `run_unity_tests`, `get_unity_compilation_result`, `unity_play_control`, `run_method_in_unity` | Test, compile, Play Mode, gọi static method trong Editor, diagnostics IDE (`lint_files`, `get_file_problems`) |
| C | **Plugin/skill chính thức khác** (vd skill `unity-cli` của Unity nếu được cài) | Theo mô tả của skill đó | Theo skill đó |
| D | **Unity CLI batchmode** | Có đường dẫn Unity Editor (CLAUDE.md) và **Editor đang đóng** | Compile, test, `-executeMethod`, build |
| E | **Người dùng thao tác** | Không có gì ở trên | Đưa hướng dẫn từng bước + script `[MenuItem]` để người dùng bấm |

Không có backend phù hợp và cần cài → `commands/setup.md` §mcp (cài package phải được đồng ý).

## 2. Luật cứng khi gọi Editor
- **Không gọi hai lệnh Editor song song** (compile, test, play control, chạy method, screenshot). Chờ lệnh trước trả về.
  Song song gây xung đột domain reload → lỗi "canceled" / "did not connect".
- Lệnh trả `error`/`canceled` → **chờ ~10 giây** rồi thử lại cùng lệnh (domain reload thường mất vài giây); không
  chuyển sang lệnh khác để "kiểm tra". Hai lần liên tiếp vẫn lỗi → dừng, hỏi người dùng.
- Trước khi sửa code hoặc chạy editor script: Editor đang Play Mode → dừng Play Mode (Play Mode có thể bỏ qua recompile).
- Sau khi sửa code: xác nhận compile thành công trước khi chạy test/method.
- Test chạy có thể mất vài phút: không chạy lại khi đang chạy; timeout → thu hẹp filter.
- Batchmode cần Editor **đóng** (project bị khóa). Editor đang mở → hỏi người dùng đóng, hoặc dùng backend A/B.
- Kết quả "success" của công cụ chạy method chỉ có nghĩa là đã gọi được method; thành công thật = không có log
  `Error`/`Exception` trong phản hồi/Console.

## 3. Compile
- A: `assets-refresh` (buộc recompile khi file `.cs` thay đổi bên ngoài) → `console-get-logs` lọc Error (`error CS`).
- B: `get_unity_compilation_result`.
- D: `"<UNITY>" -batchmode -nographics -projectPath . -quit -logFile -` → tìm `error CS` / "Scripts have compiler errors"
  trong log; exit code khác 0 = lỗi.
- Không có backend: tối thiểu đọc lại code, kiểm `using`, kiểu, asmdef tham chiếu; báo rõ "chưa compile được để xác nhận".

## 4. Chạy test
Xác định assembly + mode của một file test: `scripts/resolve_test_target.sh <file.cs>` → `<AssemblyName>\t<EditMode|PlayMode>`.

- A: tool `tests-run` với `testMode` (`EditMode`/`PlayMode`), lọc `testAssembly`/`testNamespace`/`testClass`/`testMethod`
  (tên đầy đủ `Namespace.Class.Method`), `includeMessages`, `includeStacktrace`. Qua CLI:
  `unity-mcp-cli run-tool tests-run --input '{"testMode":"EditMode","testClass":"DamageCalculatorTest"}'`
  (JSON dài → `--input-file -` với heredoc).
- B: `run_unity_tests` với `assemblyNames`, `testMode`, filter.
- D (Editor đóng):
  ```bash
  "<UNITY>" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode \
    -testResults "$(pwd)/Logs/editmode.xml" -logFile - [-testFilter "Ns.Class.Method"] [-testCategory "!IgnoreCI"]
  ```
  - `-testPlatform PlayMode` cho PlayMode (hoặc tên target player để chạy trên player).
  - **Không cần `-quit`** với `-runTests` (Editor tự thoát khi xong). Lệnh có `-quit` mà báo 0 test → bỏ `-quit`. Kiểm tra
    flag với phiên bản Unity của dự án.
  - Exit code khác 0 → có test fail hoặc lỗi chạy; luôn đọc XML.
  - Đọc kết quả XML (NUnit 3): `test-run` có `total`, `passed`, `failed`, `skipped`, `inconclusive`; mỗi `test-case`
    `result="Failed"` có `<message>` và `<stack-trace>`.
- Báo cáo: tổng / pass / fail / skip, thời gian, từng fail (tên, message, dòng `Assets/...:line`).
- Chạy trên player build (khi cần kiểm `#if !UNITY_EDITOR`, IL2CPP, stripping): `-testPlatform <StandaloneOSX|StandaloneWindows64|...>`
  với batchmode; log ở `Player.log` (đường dẫn trong `commands/log.md`).

## 5. Chạy C# trong Editor (editor script)
- A: `script-execute` (Roslyn, một class có static method — code không lưu vào dự án). Phù hợp lệnh một lần. Nếu cần
  type của dự án, kiểm tra assembly đã nạp.
- B: viết file `Assets/_Temp/Editor/UgsTemp_<Tên>.cs` chứa `public static void Run()` → compile → `run_method_in_unity`.
- D: cùng file đó → `"<UNITY>" -batchmode -nographics -projectPath . -executeMethod UgsTemp_<Tên>.Run -quit -logFile -`.
- E: file đó thêm `[MenuItem("Tools/UGS Temp/<Tên>")]` → người dùng bấm menu.
- Sau khi chạy thành công: xóa script + `.meta` (từ filesystem, rồi refresh). Không để script tự xóa chính nó
  (`AssetDatabase.DeleteAsset` trên chính file gây domain reload giữa lúc gọi). Chạy lỗi → giữ để sửa.
- Thư mục `Assets/_Temp*` nên có trong `.gitignore`; script tạm không bao giờ commit.

## 6. Chụp màn hình
Ảnh bằng chứng lưu `Docs/evidence/<feature>/<yyyy-mm-dd>_<mô tả>.png`. Sau khi chụp, **mở ảnh xem** và mô tả đã thấy
gì (đúng/sai theo spec). Không chụp được → liệt kê bước kiểm thủ công.

- Editor, A: `screenshot-game-view` (Game view), `screenshot-scene-view`, `screenshot-camera` (camera cụ thể). Vào Play Mode
  trước nếu cần trạng thái runtime (`editor-application-set-state`), chờ vài frame, chụp, thoát Play Mode.
- Trong PlayMode test: `ScreenCapture.CaptureScreenshot(absolutePath)` sau `await Awaitable.EndOfFrameAsync()`; file
  được ghi sau frame đó → chờ thêm 1–2 frame trước khi kết thúc test.
- Player build (cách bền nhất để nhìn đúng cái sẽ ship): chạy build với tham số riêng, một bootstrap script tự chụp:
  ```
  <Game executable> -screen-width 1280 -screen-height 720 -screen-fullscreen 0 --scene Shop --screenshot /abs/path/shop.png --after 60
  ```
  Script (thêm vào dự án khi lần đầu cần — hỏi trước; đặt trong runtime asmdef, không hoạt động nếu không có `--screenshot`):
  ```csharp
  using System.Collections;
  using UnityEngine;
  using UnityEngine.SceneManagement;

  // Run-and-observe: Game --scene Shop --screenshot /abs/shop.png --after 60
  public class ScreenshotOnArg : MonoBehaviour
  {
      [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
      static void Boot()
      {
          if (Arg("--screenshot") == null) return;
          var go = new GameObject(nameof(ScreenshotOnArg));
          DontDestroyOnLoad(go);
          go.AddComponent<ScreenshotOnArg>();
      }

      static string Arg(string name)
      {
          var args = System.Environment.GetCommandLineArgs();
          for (int i = 0; i < args.Length - 1; i++)
              if (args[i] == name) return args[i + 1];
          return null;
      }

      IEnumerator Start()
      {
          var scene = Arg("--scene");
          if (!string.IsNullOrEmpty(scene)) yield return SceneManager.LoadSceneAsync(scene);
          int after = int.TryParse(Arg("--after"), out var n) ? n : 60;
          for (int i = 0; i < after; i++) yield return null;
          yield return new WaitForEndOfFrame();
          ScreenCapture.CaptureScreenshot(Arg("--screenshot"));
          for (int i = 0; i < 3; i++) yield return null; // file được ghi sau frame
          Application.Quit();
      }
  }
  ```
  Đường dẫn phải tuyệt đối; scene phải có trong Build Profile / Scene List. `-batchmode`/`-nographics` không hiển thị gì → không dùng khi chụp.

## 7. Log
- A: `console-get-logs` (`logTypeFilter`, `lastMinutes`, `includeStackTrace`), `console-clear-logs` trước một thao tác để cô lập lỗi.
- File: Editor.log / Editor-prev.log / Player.log — đường dẫn trong `commands/log.md` §unity.
- Lần chạy test vừa kích hoạt: lấy phần log sau dòng reset domain cuối cùng, so mtime `Editor.log` và `Editor-prev.log` để chọn đúng file.

## 8. Batchmode CLI — tham khảo nhanh
| Mục đích | Tham số |
|---|---|
| Không UI | `-batchmode` (+ `-nographics` khi không cần GPU; không dùng khi cần render/chụp) |
| Dự án | `-projectPath <path>` |
| Thoát khi xong | `-quit` (không dùng với `-runTests`) |
| Log ra stdout | `-logFile -` |
| Chạy method | `-executeMethod Namespace.Class.Method` (static, không tham số) |
| Nền tảng khởi động | `-buildTarget win64|osxuniversal|linux64|android|ios|webgl` (không `SwitchActiveBuildTarget` trong batchmode) |
| Build Profile | `-activeBuildProfile "Assets/Settings/Build Profiles/X.asset"` + `-build <out>` |
| Test | `-runTests -testPlatform EditMode|PlayMode -testResults <abs.xml> -testFilter <...> -testCategory <...>` |
| Tham số riêng | đọc bằng `System.Environment.GetCommandLineArgs()`; đặt tên không trùng tham số Unity (`-out`, `-appVersion`, `-dev` an toàn; `-version`, `-build`, `-quit` đã bị Unity dùng) |

Đường dẫn Editor mặc định: macOS `/Applications/Unity/Hub/Editor/<ver>/Unity.app/Contents/MacOS/Unity`;
Windows `C:\Program Files\Unity\Hub\Editor\<ver>\Editor\Unity.exe`; Linux `~/Unity/Hub/Editor/<ver>/Editor/Unity`.
CI chạy sạch cần kích hoạt license Unity trước (secret, không ghi vào repo).
