# 回调同步（RevertSynchronize）

《杀戮尖塔 2》mod：把故障机器人技能牌「同步」（Synchronize）整卡固定为 v0.110 效果。

A Slay the Spire 2 mod that pins the Defect card *Synchronize* to its v0.110 effect.

## 功能

- 卡牌数值固定为 v0.110：基础每有一种不同充能球，本回合获得 2 点集中；升级后 3 点。
- 卡牌元数据固定：1 费、技能牌、目标自身、罕见、无消耗。
- 卡面文本同步为 v0.110 模板，提供 deu、eng、esp、fra、ind、ita、jpn、kor、pol、ptb、rus、spa、tha、tur、zhs、zht 共 16 种语言。
- 直接在原卡上修改，不生成新卡牌、不禁用旧卡牌；卡图沿用原版。
- 采用外置补丁写死的方式实现，补丁体不读取原版实现，游戏改动数值或文本不影响结果。

## 兼容性

- 不依赖任何基础 mod，`dependencies` 为空。
- 支持 default 0.107.x 与 public beta 0.111.0 及以上版本。
- 在 v0.110.x 上自动停用：该版本的「同步」本就是目标效果，mod 不做任何干预。
- 启动时检查游戏版本，并检查游戏包体中是否存在「同步」及其结构；版本不支持或卡牌被移除、形状不符时自动禁用补丁，游戏保持原版行为，不崩溃。
- 不保证与盗版、修改版、移动版、老旧版本、冷门 mod 或次级 mod 的兼容性。

## 安装

1. 把 `RevertSynchronize.dll`、`RevertSynchronize.pck`、`RevertSynchronize.json` 放入游戏的 mods 目录下的 `RevertSynchronize` 子目录：
   `C:\Game\Steam\steamapps\common\Slay the Spire 2\mods\RevertSynchronize\`。
2. 启动游戏。启动日志中出现 `RevertSynchronize: Harmony patches applied ...` 表示补丁已生效。

## 构建

前置：.NET 9 SDK、Godot 4.5.1（mono 版），以及安装了《杀戮尖塔 2》的本地目录。

1. 修改 `9.SRC/RevertSynchronize/RevertSynchronize.csproj` 中的本机路径：`Sts2Path`、`GodotPath`。
2. 在 `9.SRC/RevertSynchronize/` 下执行 `build.ps1`。脚本依次执行：`dotnet build -c Debug -p:NuGetAudit=false` → Godot headless `--import` → `--export-pack "Windows Desktop"` → 拷贝 dll、pdb、json 到游戏 mods 目录。

工程引用游戏目录 `data_sts2_windows_x86_64` 下的 `sts2.dll` 与 `0Harmony.dll`，二者不打入产物。

## 目录结构

```
STS2Mod-回调同步/
├── 1.ADR/           决策记录（ADR.md 索引）
├── 2.PLANS/         施工计划（PLANS.md 索引）
├── 3.DOCS/          知识与调研报告（DOCS.md 索引）
├── 4.SDD/           软件设计文档（SDD.md 索引）
├── 5.ChangeLog/     版本日志分片（ChangeLog.md 索引）
├── 9.SRC/           源码（RevertSynchronize 工程）
├── 10.OUTPUT/       构建产物与发布包（output.md 索引，内容不入库）
├── PRIVACY/         机密信息（PRIVACY.md 索引，不入库）
├── TMP/             临时与过程文件（TMP.md 索引，不入库）
├── TOOLS/           自写脚本（TOOLS.md 索引）
├── description/     创意工坊与发布文案（中/英）
├── pics/            封面与截图素材
└── languages/       16 语言官方原文存档
```

## 文档索引

| 目录 | 入口 | 内容 |
|---|---|---|
| `1.ADR/` | [ADR.md](1.ADR/ADR.md) | 补丁策略与放弃方案 |
| `2.PLANS/` | [PLANS.md](2.PLANS/PLANS.md) | 施工计划与施工记录回填 |
| `3.DOCS/` | [DOCS.md](3.DOCS/DOCS.md) | 三版本差异调研与维护清单 |
| `4.SDD/` | [SDD.md](4.SDD/SDD.md) | 总体设计与模块划分 |
| `5.ChangeLog/` | [ChangeLog.md](5.ChangeLog/ChangeLog.md) | 版本开发日志 |
| `9.SRC/` | [README.md](9.SRC/RevertSynchronize/README.md) | 面向玩家的英文说明 |

## 版本

当前版本 `1.2`，与 `9.SRC/RevertSynchronize/RevertSynchronize.json` 的 `version` 字段一致。更新日志见 [CHANGELOG.md](CHANGELOG.md)。

## 致谢

- 本 mod 使用 Deepseek Harness（WebUI）+ DeepSeek V4 Flash（0731，GA）完成，感谢梁圣。
- 感谢《杀戮尖塔 2》制作组为方便玩家制作 mod 所做的努力。
- 感谢 Baselib 制作者提供的文档。

## 开源地址

<https://github.com/Rim-World/STS2Mod-Revert-Synchronize>
