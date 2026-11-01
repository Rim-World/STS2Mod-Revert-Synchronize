#!/usr/bin/env python3
"""生成空白占位 mod 图标（420x420 全透明 PNG），供构建期使用。"""
import struct
import zlib
from pathlib import Path

W = H = 420


def chunk(tag: bytes, data: bytes) -> bytes:
    return (struct.pack(">I", len(data)) + tag + data
            + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF))


def main() -> None:
    out = Path(__file__).resolve().parent.parent / "src" / "RevertSynchronize" / "RevertSynchronize" / "mod_image.png"
    # RGBA, 每行: filter byte 0 + W*4 bytes
    raw = b"".join(b"\x00" + b"\x00\x00\x00\x00" * W for _ in range(H))
    png = (b"\x89PNG\r\n\x1a\n"
           + chunk(b"IHDR", struct.pack(">IIBBBBB", W, H, 8, 6, 0, 0, 0))
           + chunk(b"IDAT", zlib.compress(raw, 9))
           + chunk(b"IEND", b""))
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_bytes(png)
    print(f"wrote {out} ({len(png)} bytes, {W}x{H} RGBA transparent)")


if __name__ == "__main__":
    main()
