# 0.107 `ResultPile` dupe 副作用修复

- 日期：2026-11-13
- 状态：已完成（独立子代理对抗审阅与验收 PASS；0.113 启动验证通过；0.107 实机待用户验收）
- 目标版本：v1.2.2
- 关联：`2.PLANS/002-S-条件补丁守卫三版本修复.md`「既有缺陷」、`3.DOCS/002-R-加载期CanonicalKeywords补丁异常.md`

## 问题

0.107 的 `CardModel.GetResultPileTypeForCardPlay()` 先判断 `IsDupe`：

```csharp
if (IsDupe || Type == CardType.Power) return PileType.None;
if (ExhaustOnNextPlay || Keywords.Contains(CardKeyword.Exhaust)) return PileType.Exhaust;
return PileType.Discard;
```

`PileType.None` 在 `CardModel` 结算末尾走 `CardPileCmd.RemoveFromCombat`，临时复制体消失。

本 mod 的 `SynchronizeResultPilePatch.Postfix107` 当前无条件改写：

```csharp
if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
{
    __result = PileType.Discard;
}
```

它把 dupe 的 `PileType.None` 也覆盖成 `PileType.Discard`。`HistoryCourse` 遗物自动打出的同步 dupe 因此不会被移除，会留在弃牌堆并再次被抽到，本次战斗内等于多一张同步。

## 修复

只把「原本要消耗」改为「进弃牌堆」，保留 dupe 的 `None`：

```csharp
if (__instance is Synchronize && ModConfig.IsReplaceEnabled
    && __result == PileType.Exhaust)
{
    __result = PileType.Discard;
}
```

- 普通同步：0.107 原版因 `Exhaust` 返回 `PileType.Exhaust`，改为 `Discard`，符合 v0.110 无消耗行为。
- dupe：原版返回 `PileType.None`，保持不变，正常从战斗中移除。
- 其他结果：不干预，避免覆盖未来版本/其他补丁的结算。
- `ExhaustOnNextPlay` 导致的 `Exhaust` 也改为 `Discard`，与本 mod「同步不消耗」的目标一致。

## 不做

- 不改 0.110 / 0.111 / 0.113 的结算；该补丁只挂载在 0.107。
- 不改 `GetKeywordsWithSources` 过滤、数值、文本补丁。
- 不引入 0.107 独有类型的编译期引用；补丁仍由 `PatchIfExists` 条件挂载。

## 改动清单

| 文件 | 改动 |
|---|---|
| `9.SRC/RevertSynchronize/Patches/SynchronizeKeywordsPatch.cs` | `Postfix107` 增加 `__result == PileType.Exhaust` 条件 |
| `9.SRC/RevertSynchronize/ModEntry.cs` | 更新条件补丁注释（不再说“钉死”） |
| `9.SRC/RevertSynchronize/RevertSynchronize.json` | 版本 1.2.1 → 1.2.2 |
| `5.ChangeLog/`、`CHANGELOG.md`、`README.md`、`2.PLANS/PLANS.md`、`10.OUTPUT/output.md` | 记录 v1.2.2、索引与构建产物 |
| `3.DOCS/002-R-...` | 追加修复结果 |
| `4.SDD/001-总体设计.md` | 补充 `ResultPile` 条件说明 |

## 验收

| 版本 | 检查 | 通过标准 |
|---|---|---|
| 0.107 | 持有 `HistoryCourse`，上一回合最后打出攻击/技能为同步；本回合自动打出的 dupe | dupe 不进入弃牌堆，正常移除；普通同步打出后进弃牌堆、不消耗 |
| 0.107 | 普通同步、`ExhaustOnNextPlay` 场景 | 不消耗；无异常 |
| 0.113 | 启动 | `ResultPile(107)=False`，补丁不挂载；无新增异常 |
| 0.113 | 既有同步功能 | 与 v1.2.1 一致：1 费、不消耗、每种不同充能球 2/3 集中 |

## 验证方法

1. 静态：对照 0.107 `CardModel.GetResultPileTypeForCardPlay()` 与 `CardModel` 结算 switch，确认 `None` → `RemoveFromCombat`、`Exhaust` → `Exhaust`。
2. 构建：0 警告 0 错误。
3. 运行：0.113 启动回归；0.107 本机当前无环境，标记未验证，验收清单交给用户。
4. 独立子代理对抗性审阅：检查条件是否遗漏 dupe、`ExhaustOnNextPlay`、其他 postfix 覆盖与版本门。

## 风险与回退

- 若未来 0.107 的 `PileType.None` 被用于非 dupe 的同步，保留 `None` 会走 `RemoveFromCombat`；当前 0.107 源码只在 `IsDupe`/Power 分支返回 `None`，同步是 Skill，风险不成立。
- 回退：恢复无条件 `__result = PileType.Discard`；0.113 不受影响。

## 施工记录回填

| 时间 | 实际施工内容 | 验收状态 |
|---|---|---|
| 2026-11-13 09:54 | `Postfix107` 只改 `Exhaust`，保留 dupe 的 `None`；版本 1.2.2；构建并覆盖安装；独立子代理对抗审阅 PASS | 0.113 启动日志 `ResultPile(107)=False`、无异常；0.107 实机待用户验收 |
| 2026-11-13 09:58 | 独立子代理复核部署产物：DLL 哈希、IL 条件、0.113 日志与文档一致性 | 最终验收 PASS；仅 0.107 实机待用户 |
