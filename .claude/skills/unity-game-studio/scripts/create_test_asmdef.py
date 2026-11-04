#!/usr/bin/env python3
"""Tạo file .asmdef cho test assembly của Unity Test Framework.

Usage:
  python3 create_test_asmdef.py --name Game.Tests.EditMode --mode editmode --ref Game.Core --ref Game.Runtime \
      --out Assets/Tests/EditMode [--root-namespace Game.Tests] [--dry-run]

  --mode editmode  → includePlatforms ["Editor"]  (logic thuần C#, editor tools)
  --mode playmode  → includePlatforms []          (vòng đời MonoBehaviour, scene, physics; chạy được trên player)
  --ref            → tên assembly production cần test (lặp lại được). Kiểm tra tên có tồn tại trong dự án.

Luôn đặt: overrideReferences + nunit.framework.dll, autoReferenced=false, defineConstraints=["UNITY_INCLUDE_TESTS"],
references UnityEngine.TestRunner (+ UnityEditor.TestRunner cho editmode).
Không ghi đè file đã tồn tại. Không tạo .meta (Unity tự tạo khi import).
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


def existing_assembly_names(project_root: Path) -> set[str]:
    names = set()
    for base in (project_root / "Assets", project_root / "Packages"):
        if not base.exists():
            continue
        for f in base.rglob("*.asmdef"):
            if any(p in {"Library", "Temp"} for p in f.parts):
                continue
            try:
                names.add(json.loads(f.read_text(encoding="utf-8")).get("name", ""))
            except (OSError, json.JSONDecodeError):
                pass
    return names


def find_project_root(start: Path) -> Path | None:
    for p in [start, *start.parents]:
        if (p / "Assets").is_dir() and (p / "ProjectSettings").is_dir():
            return p
    return None


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--name", required=True, help="Tên assembly test, vd Game.Tests.EditMode")
    ap.add_argument("--mode", required=True, choices=["editmode", "playmode"])
    ap.add_argument("--ref", action="append", default=[], help="Assembly production được test")
    ap.add_argument("--out", required=True, help="Thư mục chứa asmdef (trong Assets/)")
    ap.add_argument("--root-namespace", default="")
    ap.add_argument("--allow-missing", action="store_true", help="Vẫn ghi reference tới assembly chưa tồn tại")
    ap.add_argument("--dry-run", action="store_true")
    a = ap.parse_args()

    out_dir = Path(a.out).resolve()
    root = find_project_root(out_dir if out_dir.exists() else out_dir.parent)
    if root is None:
        root = find_project_root(Path.cwd())
    if root is None:
        print("Không tìm thấy gốc dự án Unity (Assets/ + ProjectSettings/).", file=sys.stderr)
        return 2
    try:
        out_dir.relative_to(root / "Assets")
    except ValueError:
        print(f"--out phải nằm trong {root / 'Assets'} (Unity chỉ compile Assets/ và Packages/).", file=sys.stderr)
        return 2

    known = existing_assembly_names(root)
    missing = [r for r in a.ref if r not in known]
    if missing and not a.allow_missing:
        print(f"Không tìm thấy asmdef tên {missing} trong dự án. Code trong Assembly-CSharp không tham chiếu được "
              f"từ asmdef — cần tạo asmdef cho code game trước (hoặc dùng --allow-missing nếu sắp tạo).", file=sys.stderr)
        return 1
    if a.name in known:
        print(f"Đã có assembly tên {a.name}.", file=sys.stderr)
        return 1

    refs = list(a.ref) + ["UnityEngine.TestRunner"]
    if a.mode == "editmode":
        refs.append("UnityEditor.TestRunner")
    data = {
        "name": a.name,
        "rootNamespace": a.root_namespace,
        "references": refs,
        "includePlatforms": ["Editor"] if a.mode == "editmode" else [],
        "excludePlatforms": [],
        "allowUnsafeCode": False,
        "overrideReferences": True,
        "precompiledReferences": ["nunit.framework.dll"],
        "autoReferenced": False,
        "defineConstraints": ["UNITY_INCLUDE_TESTS"],
        "versionDefines": [],
        "noEngineReferences": False,
    }
    target = out_dir / f"{a.name}.asmdef"
    text = json.dumps(data, indent=4) + "\n"
    if a.dry_run:
        print(f"# {target}\n{text}")
        return 0
    if target.exists():
        print(f"{target} đã tồn tại — không ghi đè.", file=sys.stderr)
        return 1
    out_dir.mkdir(parents=True, exist_ok=True)
    target.write_text(text, encoding="utf-8")
    print(f"Đã tạo {target.relative_to(root)}")
    print("Tiếp theo: để Unity import (tự tạo .meta); nếu test 'internal' thì thêm "
          f"[assembly: InternalsVisibleTo(\"{a.name}\")] vào AssemblyInfo.cs của assembly production.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
