# DOCS 索引

知识与报告目录：沉淀可复用的上下文、踩坑经验、用户要求的报告与证据陈列。工程决策与规划不写在这里
（分别归 `1.ADR/` 与 `2.PLANS/`）。

编号三位递增；分类标识：`T`（任务知识）、`R`（用户要求的报告）、`E`（证据陈列）。

| 编号 | 文档 | 分类 | 时间 | 说明 |
|---|---|---|---|---|
| [001](001-T-同步卡三版本差异调研.md) | 同步卡三版本差异调研 | T | 2026-08-16 | 三版本（v0.107 / v0.110 / v0.111）反编译逐行差异、卡面文本模板、版本门 API、关键词补丁虚分派陷阱与维护清单 |
| [002](002-R-加载期CanonicalKeywords补丁异常.md) | 加载期 CanonicalKeywords 补丁异常 | R | 2026-11-13 | v0.113.0 实机日志复现：`AccessTools.PropertyGetter` 返回继承自 `CardModel` 的虚 getter，Harmony 拒绝补丁并抛 `ArgumentException`；已按报告改用 `Declared*` 并完成 0.113 启动验证；v1.2.2 另修 ResultPile dupe 副作用 |

## 目录说明

- `1.ADR/` — 决策记录（`ADR.md` 索引）
- `2.PLANS/` — 施工计划（`PLANS.md` 索引）
- `3.DOCS/` — 本目录，知识与报告
- `4.SDD/` — 软件设计文档（`SDD.md` 索引）
- `5.ChangeLog/` — 版本日志分片（`ChangeLog.md` 索引）
- `9.SRC/RevertSynchronize/` — Godot/C# 工程（补丁源码、16 语言本地化、`build.ps1`、manifest、`README.md`）
- `10.OUTPUT/01-构建产物/` — dll/pdb/pck/json 暂存（不入库）
- `TOOLS/scripts/` — 提取与生成脚本（`extract-synchronize-localization.py`、`gen-blank-icon.py`、`scan-mod-dlls.py`）
- `languages/` — 16 语言官方原文存档（`raw/`）与汇总表
- `description/` — 创意工坊/N 网简介（中/英）
- `pics/` — 封面与截图素材
