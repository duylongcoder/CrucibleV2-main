#!/usr/bin/env python3
"""Tìm mọi file text-serialized (scene, prefab, asset, material, controller...) tham chiếu tới một asset/script theo GUID.

Dùng trước khi đổi tên, di chuyển, xóa script hoặc asset — grep tên class là không đủ vì Unity tham chiếu bằng GUID.

Usage:
  python3 find_guid_refs.py <project-root> <đường dẫn asset | đường dẫn .meta | guid 32 hex> [--json] [--orphans DIR]

Ví dụ:
  python3 find_guid_refs.py . Assets/_Project/Scripts/Runtime/EnemyController.cs
  python3 find_guid_refs.py . 0123456789abcdef0123456789abcdef
  python3 find_guid_refs.py . --orphans Assets/_Project/Art     # asset trong DIR không được file nào tham chiếu

Ghi chú:
  - Chỉ đọc file text bắt đầu bằng "%YAML" (dự án cần Force Text). File binary bị bỏ qua và được đếm.
  - Tham chiếu từ code (Resources.Load("..."), Addressables key, AssetDatabase path) KHÔNG phát hiện được bằng GUID —
    script in thêm dòng grep theo tên file để kiểm tra tay.
  - --orphans cho kết quả "không thấy tham chiếu", không phải "chắc chắn không dùng": asset có thể được load bằng
    đường dẫn/label, nằm trong Resources/, StreamingAssets/, hoặc được Build Settings tham chiếu.
Exit code: 0 có kết quả/không lỗi, 1 không tìm thấy GUID của đầu vào, 2 tham số sai.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

YAML_EXT = {
    ".unity", ".prefab", ".asset", ".mat", ".controller", ".overrideController", ".anim", ".playable",
    ".mask", ".physicMaterial", ".physicsMaterial2D", ".spriteatlas", ".spriteatlasv2", ".lighting",
    ".preset", ".signal", ".mixer", ".guiskin", ".fontsettings", ".terrainlayer", ".brush", ".shadervariants",
    ".renderTexture", ".flare", ".cubemap",
}
SKIP_DIRS = {"Library", "Temp", "Logs", "obj", "UserSettings", "Build", "Builds", ".git"}
# Không liệt kê ở --orphans: code/assembly được tham chiếu bằng cách khác (tên type, asmdef name)
ORPHAN_SKIP_EXT = {".cs", ".asmdef", ".asmref", ".dll", ".so", ".a", ".rsp", ".md", ".txt", ".xml", ".jslib", ".mm", ".m", ".h"}
GUID_RE = re.compile(r"^[0-9a-f]{32}$")
REF_RE = re.compile(r"guid:\s*([0-9a-f]{32})")


def iter_files(base: Path, exts: set[str] | None = None):
    stack = [base]
    while stack:
        d = stack.pop()
        try:
            entries = list(d.iterdir())
        except OSError:
            continue
        for e in entries:
            if e.is_dir():
                if e.name in SKIP_DIRS or e.name.startswith(".") or e.name.endswith("~"):
                    continue
                stack.append(e)
            elif exts is None or e.suffix in exts:
                yield e


def guid_of(meta: Path) -> str | None:
    try:
        m = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", meta.read_text(encoding="utf-8", errors="replace"), re.MULTILINE)
    except OSError:
        return None
    return m.group(1) if m else None


def resolve_target(root: Path, target: str) -> tuple[str | None, Path | None]:
    if GUID_RE.match(target.lower()):
        return target.lower(), None
    p = Path(target)
    if not p.is_absolute():
        p = root / p
    meta = p if p.suffix == ".meta" else p.with_name(p.name + ".meta")
    return guid_of(meta), p


def search_roots(root: Path) -> list[Path]:
    roots = [root / "Assets"]
    pk = root / "Packages"
    if pk.exists():
        roots += [d for d in pk.iterdir() if d.is_dir()]
    roots.append(root / "ProjectSettings")
    return [r for r in roots if r.exists()]


def find_refs(root: Path, guid: str) -> tuple[list[dict], int]:
    hits, binary = [], 0
    for base in search_roots(root):
        for f in iter_files(base, YAML_EXT):
            try:
                text = f.read_text(encoding="utf-8", errors="replace")
            except OSError:
                continue
            if not text.startswith("%YAML"):
                binary += 1
                continue
            if guid not in text:
                continue
            lines = [i + 1 for i, line in enumerate(text.splitlines()) if guid in line]
            hits.append({"file": str(f.relative_to(root)), "count": len(lines), "lines": lines[:10]})
    return sorted(hits, key=lambda h: h["file"]), binary


def find_orphans(root: Path, directory: Path) -> list[str]:
    referenced: set[str] = set()
    for base in search_roots(root):
        for f in iter_files(base, YAML_EXT):
            try:
                text = f.read_text(encoding="utf-8", errors="replace")
            except OSError:
                continue
            if text.startswith("%YAML"):
                referenced.update(REF_RE.findall(text))
    orphans = []
    for meta in iter_files(directory, {".meta"}):
        asset = meta.with_name(meta.name[:-5])
        if asset.is_dir() or not asset.exists() or asset.suffix.lower() in ORPHAN_SKIP_EXT:
            continue
        g = guid_of(meta)
        if g and g not in referenced:
            orphans.append(str(asset.relative_to(root)))
    return sorted(orphans)


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("root")
    ap.add_argument("target", nargs="?")
    ap.add_argument("--orphans", metavar="DIR")
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()
    root = Path(a.root).resolve()
    if not (root / "Assets").is_dir():
        print(f"Không thấy Assets/ trong {root}", file=sys.stderr)
        return 2

    if a.orphans:
        d = Path(a.orphans)
        d = d if d.is_absolute() else root / d
        orphans = find_orphans(root, d)
        if a.json:
            print(json.dumps({"directory": str(d), "unreferenced": orphans}, ensure_ascii=False, indent=2))
        else:
            print(f"Asset trong {d.relative_to(root)} không thấy tham chiếu GUID ({len(orphans)}):")
            for o in orphans:
                print(f"  - {o}")
            print("Lưu ý: vẫn có thể được load bằng đường dẫn/label/Resources/Addressables/Build Settings — kiểm tra trước khi xóa.")
        return 0

    if not a.target:
        ap.error("cần <target> hoặc --orphans DIR")
    guid, path = resolve_target(root, a.target)
    if not guid:
        print(f"Không đọc được GUID cho {a.target} (thiếu .meta?)", file=sys.stderr)
        return 1
    hits, binary = find_refs(root, guid)
    result = {"target": a.target, "guid": guid, "references": hits, "binary_files_skipped": binary}
    if a.json:
        print(json.dumps(result, ensure_ascii=False, indent=2))
    else:
        print(f"GUID {guid} ({a.target}) được tham chiếu bởi {len(hits)} file:")
        for h in hits:
            print(f"  - {h['file']}  ({h['count']} lần; dòng {', '.join(map(str, h['lines']))})")
        if binary:
            print(f"Bỏ qua {binary} file binary (dự án không Force Text?) — có thể sót tham chiếu.")
        if path is not None:
            stem = path.stem
            print(f"Kiểm tra thêm tham chiếu bằng tên/đường dẫn trong code:  grep -rn \"{stem}\" Assets --include=*.cs")
    return 0


if __name__ == "__main__":
    sys.exit(main())
