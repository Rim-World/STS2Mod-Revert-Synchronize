#!/usr/bin/env python3
"""
RevertSynchronize 素材提取脚本（只读游戏 pck，输出到本项目目录）：
1) languages/raw/<lang>/cards.json          —— SYNCHRONIZE.* 官方原文（审阅存档）
2) languages/原文汇总.md                     —— 16 语言标题/描述汇总表
3) src/RevertSynchronize/RevertSynchronize/localization/<lang>/cards.json
                                             —— RE_SYNCHRONIZE.* mod 本地化文件
用法: python extract-synchronize-localization.py <SlayTheSpire2.pck 路径>
"""
import json
import struct
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

LANGS = ["deu", "eng", "esp", "fra", "ind", "ita", "jpn", "kor",
         "pol", "ptb", "rus", "spa", "tha", "tur", "zhs", "zht"]


def parse_pck(pck_path):
    entries = []
    with open(pck_path, "rb") as f:
        magic = f.read(4)
        if magic != b"GDPC":
            raise ValueError(f"Invalid PCK magic: {magic!r}")
        pack_version = struct.unpack("<I", f.read(4))[0]
        if pack_version <= 2:
            raise ValueError(f"Unsupported pack version: {pack_version}")
        f.read(12)   # major/minor/patch
        f.read(4)    # flags
        file_base = struct.unpack("<Q", f.read(8))[0]
        dir_offset = struct.unpack("<Q", f.read(8))[0]
        f.seek(dir_offset)
        file_count = struct.unpack("<I", f.read(4))[0]
        for _ in range(file_count):
            path_len = struct.unpack("<I", f.read(4))[0]
            path = f.read(path_len).split(b"\x00", 1)[0].decode("utf-8", errors="replace")
            offset = struct.unpack("<Q", f.read(8))[0]
            size = struct.unpack("<Q", f.read(8))[0]
            f.read(16)   # md5
            f.read(4)    # flags
            entries.append((path, offset, size))
    return file_base, entries


def main():
    if len(sys.argv) < 2:
        print("usage: extract-synchronize-localization.py <pck>")
        return 1
    pck = sys.argv[1]
    root = Path(__file__).resolve().parent.parent
    raw_dir = root / "languages" / "raw"
    mod_loc_dir = root / "src" / "RevertSynchronize" / "RevertSynchronize" / "localization"

    file_base, entries = parse_pck(pck)

    tables = {}   # lang -> dict
    with open(pck, "rb") as f:
        for path, offset, size in entries:
            if not path.startswith("localization/") or not path.endswith(".json"):
                continue
            parts = path.split("/")
            if len(parts) < 3:
                continue
            lang = parts[1]
            if lang not in LANGS:
                continue
            f.seek(file_base + offset)
            try:
                table = json.loads(f.read(size))
            except Exception:
                continue
            if not isinstance(table, dict):
                continue
            hits = {k: v for k, v in table.items() if k.startswith("SYNCHRONIZE.")}
            if hits:
                tables.setdefault(lang, {}).update(hits)

    missing_title = [l for l in LANGS if l not in tables or "SYNCHRONIZE.title" not in tables[l]]
    print("langs with data:", sorted(tables))
    if missing_title:
        print("missing SYNCHRONIZE.title (will fall back to eng):", missing_title)

    eng_title = tables["eng"].get("SYNCHRONIZE.title", "Synchronize")

    for lang in LANGS:
        table = tables.get(lang, {})
        title = table.get("SYNCHRONIZE.title", eng_title)
        desc = table.get("SYNCHRONIZE.description", "")
        # 1) raw archive
        raw = {"SYNCHRONIZE.title": title, "SYNCHRONIZE.description": desc}
        raw_path = raw_dir / lang / "cards.json"
        raw_path.parent.mkdir(parents=True, exist_ok=True)
        raw_path.write_text(json.dumps(raw, ensure_ascii=False, indent=2), encoding="utf-8")
        # 2) mod localization
        mod = {"RE_SYNCHRONIZE.title": title, "RE_SYNCHRONIZE.description": desc}
        mod_path = mod_loc_dir / lang / "cards.json"
        mod_path.parent.mkdir(parents=True, exist_ok=True)
        mod_path.write_text(json.dumps(mod, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"wrote {lang}")

    # 3) summary markdown
    lines = ["# 「同步」（Synchronize）官方文本汇总（v0.111.0 pck 提取，供审阅）",
             "",
             "> 提取自本机 `SlayTheSpire2.pck`（v0.111.0）`localization/<lang>/cards.json` 的 `SYNCHRONIZE.*` 键。",
             "> 描述为模板原文（数字由代码 DynamicVars 渲染）；本 mod 目标为 v0.110 数值（基础 2 / 升级 3）。",
             "",
             "| 语言 | 标题 | 描述模板 |",
             "|---|---|---|"]
    for lang in LANGS:
        table = tables.get(lang, {})
        title = table.get("SYNCHRONIZE.title", eng_title)
        desc = table.get("SYNCHRONIZE.description", "")
        if "SYNCHRONIZE.title" not in table:
            title += " ⚠️(缺键,回退eng)"
        lines.append(f"| {lang} | {title} | {desc} |")
    (root / "languages" / "原文汇总.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
    print("wrote 原文汇总.md")
    return 0


if __name__ == "__main__":
    sys.exit(main())
