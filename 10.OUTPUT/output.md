# OUTPUT 索引

最终产物目录。入库的只有本索引文件，其余内容由根 `.gitignore` 排除。

| 时间 | 名称 | 版本号 | 大小 | 说明 |
|---|---|---|---|---|
| 2026-08-16 20:12 | `01-构建产物/RevertSynchronize.dll` | 1.0 | 18.5 KB | 编译产物，Harmony 补丁与入口程序集 |
| 2026-08-16 20:12 | `01-构建产物/RevertSynchronize.pck` | 1.0 | 240.8 KB | Godot 导出的资源包：图标与 16 个 `cards.json` |
| 2026-08-16 20:12 | `01-构建产物/RevertSynchronize.json` | 1.0 | 497 B | mod 清单 |
| 2026-08-16 20:12 | `01-构建产物/RevertSynchronize.pdb` | 1.0 | 15.2 KB | 调试符号 |
| 2026-11-01 19:53 | `02-发布包/RevertSynchronize.v1.0.zip` | 1.0 | 253.8 KB | Release `v1.0` 资产：游戏 mods 目录的 dll/pck/json/pdb（已排除以 `.` 开头的项） |
| 2026-11-01 19:53 | `02-发布包/RevertSynchronizeSourceCode.zip` | 1.0 | 150.6 KB | Release `v1.0` 资产：`9.SRC/RevertSynchronize/` 源码（排除 `.godot/`、`bin/`、`obj/`、dll/pck/pdb） |

## 分类说明

- `01-构建产物/`：`build.ps1` 输出的 dll、pck、json、pdb 暂存，与安装到游戏 mods 目录的文件一致。
- `02-发布包/`：GitHub Release 资产压缩包，打包后登记到上表。

产物时间取自文件修改时间；版本号取自 `9.SRC/RevertSynchronize/RevertSynchronize.json` 的 `version` 字段。
