#!/usr/bin/env python3
"""Quét nhanh một dự án Unity (chỉ đọc) để điền CLAUDE.md / TECH.md.

Phát hiện:
  - Phiên bản Unity (ProjectSettings/ProjectVersion.txt)
  - Package trong Packages/manifest.json + nhóm đáng chú ý (URP/HDRP, Input System, Addressables, DI, async...)
  - Render pipeline, Active Input Handling, scripting backend, company/product name
  - Asset Serialization (Force Text?) và Version Control mode (Visible Meta Files?)
  - Assembly definitions (runtime / editor / test) và số test method
  - Scene trong Build Settings
  - Git repo, .gitignore có loại Library/Temp/... chưa
  - Thư mục code chính, số file .cs

Usage:
  python3 unity_project_scan.py <project-root> [--json | --markdown]

Chỉ dùng thư viện chuẩn. Không ghi bất kỳ file nào. Exit code 2 nếu không phải dự án Unity.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

SKIP_DIRS = {"Library", "Temp", "Logs", "obj", "UserSettings", "Build", "Builds", ".git", ".vs", ".idea", "node_modules"}

NOTABLE_PACKAGES = {
    "com.unity.render-pipelines.universal": "URP",
    "com.unity.render-pipelines.high-definition": "HDRP",
    "com.unity.inputsystem": "Input System",
    "com.unity.addressables": "Addressables",
    "com.unity.cinemachine": "Cinemachine",
    "com.unity.textmeshpro": "TextMeshPro (package riêng)",
    "com.unity.ugui": "uGUI",
    "com.unity.test-framework": "Unity Test Framework",
    "com.unity.test-framework.performance": "Performance Testing",
    "com.unity.ai.navigation": "AI Navigation",
    "com.unity.2d.tilemap.extras": "2D Tilemap Extras",
    "com.unity.2d.pixel-perfect": "2D Pixel Perfect",
    "com.unity.localization": "Localization",
    "com.unity.netcode.gameobjects": "Netcode for GameObjects",
    "com.unity.entities": "Entities (DOTS)",
    "com.unity.probuilder": "ProBuilder",
    "com.unity.memoryprofiler": "Memory Profiler",
    "com.unity.project-auditor": "Project Auditor",
    "com.cysharp.unitask": "UniTask",
    "jp.hadashikick.vcontainer": "VContainer",
    "com.svermeulen.extenject": "Zenject/Extenject",
    "com.demigiant.dotween": "DOTween",
    "com.ivanmurzak.unity.mcp": "Unity MCP (AI Game Developer)",
    "com.nowsprinting.test-helper": "Test Helper",
    "com.nowsprinting.test-helper.ui": "UI Test Helper",
}

# Dấu hiệu thư viện cài bằng .unitypackage / thư mục trong Assets
ASSET_FOLDER_HINTS = {
    "Plugins/Zenject": "Zenject (Assets)",
    "Plugins/UniTask": "UniTask (Assets)",
    "Plugins/Demigiant/DOTween": "DOTween (Assets)",
    "Plugins/VContainer": "VContainer (Assets)",
    "TextMesh Pro": "TMP Essential Resources",
}

GITIGNORE_REQUIRED = ["Library", "Temp", "Logs", "obj", "UserSettings"]

# asmdef có thể tham chiếu bằng GUID thay vì tên (UnityEngine.TestRunner, UnityEditor.TestRunner)
TEST_RUNNER_GUIDS = {"GUID:27619889b8ba8c24980f49ee34dbb44a", "GUID:0acc523941302664db1f4e527237feb3"}


def read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return ""


def yaml_scalar(text: str, key: str) -> str | None:
    m = re.search(rf"^\s*{re.escape(key)}:\s*(.*)$", text, re.MULTILINE)
    return m.group(1).strip() if m else None


def walk(root: Path, pattern: str):
    """rglob nhưng bỏ qua thư mục Unity tạm."""
    stack = [root]
    while stack:
        d = stack.pop()
        try:
            entries = list(d.iterdir())
        except OSError:
            continue
        for e in entries:
            if e.is_dir():
                if e.name in SKIP_DIRS or e.name.startswith("."):
                    continue
                stack.append(e)
            elif e.match(pattern):
                yield e


def scan_version(root: Path) -> dict:
    text = read_text(root / "ProjectSettings" / "ProjectVersion.txt")
    full = yaml_scalar(text, "m_EditorVersion")
    short = None
    if full:
        m = re.match(r"(\d+)\.(\d+)", full)
        if m:
            short = f"{m.group(1)}.{m.group(2)}"
    return {"full": full, "short": short}


def scan_packages(root: Path) -> dict:
    manifest = root / "Packages" / "manifest.json"
    deps: dict = {}
    if manifest.exists():
        try:
            deps = json.loads(read_text(manifest)).get("dependencies", {})
        except json.JSONDecodeError:
            deps = {"_error": "manifest.json không hợp lệ"}
    notable = {NOTABLE_PACKAGES[k]: v for k, v in deps.items() if k in NOTABLE_PACKAGES}
    embedded = sorted(p.name for p in (root / "Packages").iterdir() if p.is_dir()) if (root / "Packages").exists() else []
    return {"dependencies": deps, "notable": notable, "embedded": embedded}


def scan_settings(root: Path, packages: dict) -> dict:
    ps = read_text(root / "ProjectSettings" / "ProjectSettings.asset")
    gs = read_text(root / "ProjectSettings" / "GraphicsSettings.asset")
    es = read_text(root / "ProjectSettings" / "EditorSettings.asset")
    vcs = read_text(root / "ProjectSettings" / "VersionControlSettings.asset")

    input_map = {"0": "Input Manager (Old)", "1": "Input System Package (New)", "2": "Both"}
    active_input = yaml_scalar(ps, "activeInputHandler")

    # scriptingBackend: block dạng "scriptingBackend:\n    Standalone: 1"
    backends = {}
    m = re.search(r"^\s*scriptingBackend:\s*\n((?:\s{4,}\S.*\n)+)", ps, re.MULTILINE)
    if m:
        for line in m.group(1).splitlines():
            kv = line.strip().split(":")
            if len(kv) == 2:
                backends[kv[0].strip()] = {"0": "Mono", "1": "IL2CPP"}.get(kv[1].strip(), kv[1].strip())

    pipeline = "Built-in"
    deps = packages.get("dependencies", {})
    rp_ref = (yaml_scalar(gs, "m_CustomRenderPipeline") or "") + (yaml_scalar(gs, "m_DefaultRenderPipeline") or "")
    has_rp_asset = "guid:" in rp_ref
    if "com.unity.render-pipelines.high-definition" in deps:
        pipeline = "HDRP"
    elif "com.unity.render-pipelines.universal" in deps:
        pipeline = "URP" if has_rp_asset else "URP (package có, chưa thấy RP asset trong GraphicsSettings — kiểm tra Quality settings)"

    serialization = {"0": "Mixed", "1": "Force Binary", "2": "Force Text"}.get(yaml_scalar(es, "m_SerializationMode") or "", None)
    vc_mode = yaml_scalar(vcs, "m_Mode")
    if vc_mode is None:
        vc_mode = yaml_scalar(es, "m_ExternalVersionControlSupport")
    enter_play = yaml_scalar(es, "m_EnterPlayModeOptionsEnabled")
    enter_play_opts = yaml_scalar(es, "m_EnterPlayModeOptions")

    return {
        "company": yaml_scalar(ps, "companyName"),
        "product": yaml_scalar(ps, "productName"),
        "bundle_version": yaml_scalar(ps, "bundleVersion"),
        "active_input": input_map.get(active_input or "", active_input),
        "scripting_backend": backends,
        "render_pipeline": pipeline,
        "serialization": serialization,
        "version_control_mode": (vc_mode or "").strip('"') or None,
        "enter_play_mode_options": {"enabled": enter_play, "flags": enter_play_opts},
    }


def scan_asmdefs(root: Path) -> list[dict]:
    result = []
    for base in (root / "Assets", root / "Packages"):
        if not base.exists():
            continue
        for f in walk(base, "*.asmdef"):
            try:
                data = json.loads(read_text(f))
            except json.JSONDecodeError:
                result.append({"path": str(f.relative_to(root)), "error": "JSON không hợp lệ"})
                continue
            refs = data.get("references", [])
            precompiled = data.get("precompiledReferences", [])
            platforms = data.get("includePlatforms", [])
            constraints = data.get("defineConstraints", [])
            is_test = (
                "UnityEngine.TestRunner" in refs
                or "UnityEditor.TestRunner" in refs
                or "nunit.framework.dll" in precompiled
                or "UNITY_INCLUDE_TESTS" in constraints
                or any(r in TEST_RUNNER_GUIDS for r in refs)
            )
            editor_only = platforms == ["Editor"]
            kind = ("test-" + ("editmode" if editor_only else "playmode")) if is_test else ("editor" if editor_only else "runtime")
            test_count = 0
            if is_test:
                for cs in walk(f.parent, "*.cs"):
                    test_count += len(re.findall(r"\[(?:Test|UnityTest|TestCase|TestCaseSource)\b", read_text(cs)))
            result.append({
                "name": data.get("name"),
                "path": str(f.relative_to(root)),
                "kind": kind,
                "references": refs,
                "noEngineReferences": data.get("noEngineReferences", False),
                "test_attributes": test_count if is_test else None,
            })
    return sorted(result, key=lambda a: a["path"])


def scan_scenes(root: Path) -> list[dict]:
    text = read_text(root / "ProjectSettings" / "EditorBuildSettings.asset")
    scenes = []
    for m in re.finditer(r"-\s*enabled:\s*(\d)\s*\n\s*path:\s*(.+)", text):
        scenes.append({"enabled": m.group(1) == "1", "path": m.group(2).strip()})
    return scenes


def scan_code(root: Path) -> dict:
    assets = root / "Assets"
    cs_files = list(walk(assets, "*.cs")) if assets.exists() else []
    mono = so = 0
    top_dirs: dict[str, int] = {}
    namespaces: dict[str, int] = {}
    for f in cs_files:
        t = read_text(f)
        if re.search(r":\s*MonoBehaviour\b", t):
            mono += 1
        if re.search(r":\s*ScriptableObject\b", t):
            so += 1
        rel = f.relative_to(assets).parts
        if rel:
            top_dirs[rel[0]] = top_dirs.get(rel[0], 0) + 1
        m = re.search(r"^\s*namespace\s+([\w.]+)", t, re.MULTILINE)
        if m:
            ns_root = ".".join(m.group(1).split(".")[:2])
            namespaces[ns_root] = namespaces.get(ns_root, 0) + 1
    hints = [label for rel, label in ASSET_FOLDER_HINTS.items() if (assets / rel).exists()]
    return {
        "cs_files": len(cs_files),
        "monobehaviours": mono,
        "scriptableobjects": so,
        "files_by_top_folder": dict(sorted(top_dirs.items(), key=lambda kv: -kv[1])[:10]),
        "namespaces": dict(sorted(namespaces.items(), key=lambda kv: -kv[1])[:10]),
        "asset_folder_libraries": hints,
        "has_docs_folder": (root / "Docs").exists(),
        "has_claude_md": (root / "CLAUDE.md").exists(),
    }


def scan_git(root: Path) -> dict:
    gi = read_text(root / ".gitignore")
    # chấp nhận "Library/", "/Library/", "[Ll]ibrary/"
    missing = [d for d in GITIGNORE_REQUIRED if not re.search(rf"(?im)^\s*/?(\[{d[0].upper()}{d[0].lower()}\]|{d[0]}){d[1:]}", gi)] if gi else GITIGNORE_REQUIRED
    return {
        "is_repo": (root / ".git").exists(),
        "gitignore": bool(gi),
        "gitignore_missing": missing,
        "gitattributes": (root / ".gitattributes").exists(),
        "lfs": "filter=lfs" in read_text(root / ".gitattributes"),
    }


def to_markdown(r: dict) -> str:
    v, s, p, c, g = r["unity_version"], r["settings"], r["packages"], r["code"], r["git"]
    lines = ["# Unity project scan", "", "| Mục | Giá trị |", "|---|---|"]
    lines.append(f"| Unity | {v['full'] or 'KHÔNG RÕ'} |")
    lines.append(f"| Company / Product | {s['company']} / {s['product']} (v{s['bundle_version']}) |")
    lines.append(f"| Render pipeline | {s['render_pipeline']} |")
    lines.append(f"| Input | {s['active_input']} |")
    lines.append(f"| Scripting backend | {', '.join(f'{k}: {b}' for k, b in s['scripting_backend'].items()) or 'mặc định'} |")
    lines.append(f"| Asset Serialization | {s['serialization'] or 'không rõ'} |")
    lines.append(f"| Version Control mode | {s['version_control_mode'] or 'không rõ'} |")
    lines.append(f"| Enter Play Mode Options | enabled={s['enter_play_mode_options']['enabled']} flags={s['enter_play_mode_options']['flags']} |")
    lines.append(f"| Code | {c['cs_files']} file .cs · {c['monobehaviours']} MonoBehaviour · {c['scriptableobjects']} ScriptableObject |")
    lines.append(f"| Thư mục code chính | {', '.join(f'{k} ({n})' for k, n in c['files_by_top_folder'].items()) or '—'} |")
    lines.append(f"| Namespace chính | {', '.join(c['namespaces']) or '—'} |")
    lines.append(f"| Git | repo={g['is_repo']} · .gitignore={g['gitignore']} (thiếu: {', '.join(g['gitignore_missing']) or 'không'}) · LFS={g['lfs']} |")
    lines.append(f"| CLAUDE.md / Docs | {c['has_claude_md']} / {c['has_docs_folder']} |")
    lines += ["", "## Package đáng chú ý", ""]
    lines += [f"- {k}: `{ver}`" for k, ver in p["notable"].items()] or ["- (không có)"]
    if c["asset_folder_libraries"]:
        lines += [f"- {h}" for h in c["asset_folder_libraries"]]
    lines += ["", "## Assembly definitions", "", "| Tên | Loại | Đường dẫn | Test attr |", "|---|---|---|---|"]
    for a in r["asmdefs"]:
        lines.append(f"| {a.get('name')} | {a.get('kind', a.get('error'))} | `{a['path']}` | {a.get('test_attributes') or ''} |")
    if not r["asmdefs"]:
        lines.append("| (không có — code nằm trong Assembly-CSharp) | | | |")
    lines += ["", "## Scene trong Build Settings", ""]
    lines += [f"- {'[x]' if sc['enabled'] else '[ ]'} `{sc['path']}`" for sc in r["scenes"]] or ["- (trống hoặc dùng Build Profile riêng)"]
    warnings = []
    if s["serialization"] and s["serialization"] != "Force Text":
        warnings.append("Asset Serialization không phải Force Text → khó merge/diff scene, prefab.")
    if s["version_control_mode"] and "Visible" not in s["version_control_mode"]:
        warnings.append("Version Control mode không phải Visible Meta Files.")
    if g["gitignore_missing"]:
        warnings.append(f".gitignore thiếu: {', '.join(g['gitignore_missing'])}")
    if not any(a.get("kind", "").startswith("test") for a in r["asmdefs"]):
        warnings.append("Chưa có test assembly (EditMode/PlayMode).")
    if c["cs_files"] and not any(a.get("kind") == "runtime" for a in r["asmdefs"]):
        warnings.append("Code game chưa có asmdef runtime → test asmdef không tham chiếu được.")
    lines += ["", "## Cảnh báo", ""] + ([f"- {w}" for w in warnings] or ["- Không có"])
    return "\n".join(lines) + "\n"


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("root", nargs="?", default=".")
    fmt = ap.add_mutually_exclusive_group()
    fmt.add_argument("--json", action="store_true")
    fmt.add_argument("--markdown", action="store_true")
    args = ap.parse_args()

    root = Path(args.root).resolve()
    if not (root / "Assets").is_dir() or not (root / "ProjectSettings").is_dir():
        print(f"Không phải dự án Unity (thiếu Assets/ hoặc ProjectSettings/): {root}", file=sys.stderr)
        return 2

    packages = scan_packages(root)
    report = {
        "root": str(root),
        "unity_version": scan_version(root),
        "packages": packages,
        "settings": scan_settings(root, packages),
        "asmdefs": scan_asmdefs(root),
        "scenes": scan_scenes(root),
        "code": scan_code(root),
        "git": scan_git(root),
    }
    if args.json:
        print(json.dumps(report, ensure_ascii=False, indent=2))
    else:
        print(to_markdown(report))
    return 0


if __name__ == "__main__":
    sys.exit(main())
