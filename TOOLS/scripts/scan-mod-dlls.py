#!/usr/bin/env python3
"""Scan all installed mod DLLs for patch-target strings related to card keywords/play flow."""
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

TARGETS = [
    "Keywords",
    "GetKeywordsWithSources",
    "CanonicalKeywords",
    "GetResultPileTypeForCardPlay",
    "GetResultLocationForCardPlay",
    "OnPlayWrapper",
    "MoveCardToResultPileAfterPlay",
    "ModifyKeywordsInCombat",
    "ModifyCardPlayResultPileTypeAndPosition",
    "Synchronize",
    "Exhaust",
]

ROOTS = [
    Path(r"C:\Game\Steam\steamapps\common\Slay the Spire 2\mods"),
    Path(r"C:\Game\Steam\steamapps\common\Slay the Spire 2\mods（手动从N网下载，107及以前）"),
    Path(r"C:\Game\Steam\steamapps\common\Slay the Spire 2\mods（仅含有适用109版本的薄荷尖塔）"),
    Path(r"C:\Game\Steam\steamapps\workshop\content\2868840"),
]

def utf16_find(data: bytes, needle: str) -> bool:
    return needle.encode("utf-16-le") in data

def scan(path: Path):
    data = path.read_bytes()
    hits = []
    for t in TARGETS:
        if utf16_find(data, t):
            hits.append(t)
    return hits

seen = set()
for root in ROOTS:
    if not root.exists():
        continue
    for dll in root.rglob("*.dll"):
        try:
            rel = str(dll.relative_to(root))
        except ValueError:
            rel = str(dll)
        if rel in seen:
            continue
        seen.add(rel)
        hits = scan(dll)
        if hits:
            print(f"{rel} :: {', '.join(hits)}")
