# TOOLS 索引

工具与自写脚本目录。一级子项只有以工具命名的文件夹、`scripts/` 与 `TOOLS.md`。

| 时间 | 工具/脚本 | 用途 | 版本 | 大小 | 来源 | 安装方式/依赖 |
|---|---|---|---|---|---|---|
| 2026-08-16 | `scripts/extract-synchronize-localization.py` | 只读游戏 `SlayTheSpire2.pck`，提取 `SYNCHRONIZE.*` 原文，生成 `languages/raw/<lang>/cards.json`、`languages/原文汇总.md` 与 mod 侧的 `RE_SYNCHRONIZE.*` 本地化文件 | 无 | 5.1 KB | 项目自写 | 直接运行，需要 Python 3；用法 `python extract-synchronize-localization.py <SlayTheSpire2.pck 路径>` |
| 2026-08-16 | `scripts/gen-blank-icon.py` | 生成 420×420 全透明 PNG 占位图标 `RevertSynchronize/mod_image.png` | 无 | 981 B | 项目自写 | 直接运行，仅用 Python 标准库 |
| 2026-08-16 | `scripts/scan-mod-dlls.py` | 扫描本机各 mods 目录下 DLL 的补丁目标字符串（关键词与打出流程相关），用于排查其它 mod 挂点 | 无 | 1.5 KB | 项目自写 | 直接运行，仅用 Python 标准库；扫描路径写在脚本内 |

## 说明

- 脚本内的 `languages/`、`src/RevertSynchronize/` 等路径以项目根为基准；目录整理后源码位于 `9.SRC/RevertSynchronize/`，`extract-synchronize-localization.py` 与 `gen-blank-icon.py` 的输出路径仍需按新目录调整后才能直接运行。
- 未在本目录安装第三方工具；构建依赖的 .NET 9 SDK 与 Godot 4.5.1 为系统级安装，不在项目内。
