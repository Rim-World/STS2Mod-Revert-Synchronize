# 更新日志

本文件遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/) 规范，版本号遵循语义化版本。

## [未发布]

### 变更

- 仓库整理为十目录结构：`adr/` → `1.ADR/`、`plans/` → `2.PLANS/`、`docs/` → `3.DOCS/`、`src/` → `9.SRC/`、`script/*.py` → `TOOLS/scripts/`、`mods/RevertSynchronize/` → `10.OUTPUT/01-构建产物/`。
- 文档改为中文编号命名：ADR 001、计划 001-G、知识 001-T。
- 新增 `4.SDD/`、`5.ChangeLog/`、`PRIVACY/`、`TMP/`、`TOOLS/` 目录与索引文件。
- 从远端恢复 `README.md`、`README.zh.md`、`LICENSE`、`.gitignore` 到 `9.SRC/RevertSynchronize/` 与项目根目录。

### 修复

- 修正计划文档中失效的工程路径与本机绝对路径引用。

### 提交编号

- `748a12eac647c03f3335307d7b8afe1aa5a27e42`：仓库整理提交。

## [1.0] - 2026-08-16

### 新增

- 首发版本：把故障机器人技能牌「同步」整卡固定为 v0.110 效果，基础每有一种不同充能球获得 2 点本回合集中，升级 3 点，无消耗。
- 外置 Harmony 补丁 12 个，覆盖数值、打出、升级、关键词、悬停提示、费用、类型、目标、稀有度、卡名与描述。
- 形状守卫与版本门：目标成员缺失时整体禁用补丁；v0.110.x 上停用全部补丁。
- 16 种语言的卡名与描述模板。
- `min_game_version` 设为 `0.107.0`。

### 已知问题

- 在 v0.107 上，基础版「同步」打出后仍进入消耗区：关键词补丁挂在基类 `CanonicalKeywords` getter，被 107 的 override 虚分派绕过。修复方案见 `1.ADR/001-同步卡回调至v0.110方案.md` 的修订记录。
