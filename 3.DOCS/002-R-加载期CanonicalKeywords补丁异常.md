# 加载期异常：`CanonicalKeywords` 条件补丁抛 `ArgumentException`

- 日期：2026-11-13
- 评估对象：本 mod v1.2（DLL 构建于 2026-11）
- 触发版本：0.110.0 及以上（含当前 0.113.0），**非 113 新引入**
- 结论：**加载期记 ERROR**，但主补丁已生效，功能未失效。属于守卫逻辑缺陷。

## 现象

`%APPDATA%\SlayTheSpire2\logs\godot.log:425-435`：

```
[WARN] Mod RevertSynchronize declares version 1.0 which is not a valid Semantic Version
[INFO] RevertSynchronize: loading...
[ERROR] RevertSynchronize: failed to apply patches: System.ArgumentException:
  You can only patch implemented methods/constructors. Patch the declared method virtual
  System.Collections.Generic.IEnumerable`1<...CardKeyword> MegaCrit.Sts2.Core.Models.CardModel::get_CanonicalKeywords() instead.
   at RevertSynchronize.ModEntry.PatchIfExists(...) ModEntry.cs:line 65
   at RevertSynchronize.ModEntry.ModLoaded() ModEntry.cs:line 38
```

同一次启动没有 `RevertSynchronize: Harmony patches applied ...`。

注意：该日志加载的是**创意工坊 v1.0 副本**（旧源码路径、`Version=1.0.0.0`），不是本地 v1.2；本地 v1.2 DLL 的 `ModEntry` 反编译代码与它逐行相同，因此结论同样成立。2026-11-13 04:45 的原始日志已被轮转删除，现有 `godot.log` 保留同一条异常。

## 原因

`ModEntry.cs:38-40` 用 `AccessTools.PropertyGetter(typeof(Synchronize), "CanonicalKeywords")` 探测「107 专属成员」。该调用沿继承链查找，0.110 及以上返回 `CardModel` 的 getter：

| 位置 | 内容 |
|---|---|
| `1.STS2-107Reference/.../Cards/Synchronize.cs:28` | 107 声明 `public override CanonicalKeywords`，返回 `[Exhaust]` |
| `2.STS2-111Reference/.../Cards/Synchronize.cs` | 无该成员 |
| `3.STS2-113Reference/.../Cards/Synchronize.cs` | 无该成员 |
| `3.STS2-113Reference/.../CardModel.cs:579` | `public virtual IEnumerable<CardKeyword> CanonicalKeywords` |

Harmony 2.4.2 在 `PatchProcessor.Patch()` 里检查 `original.IsDeclaredMember()`（即 `DeclaringType == ReflectedType`）。通过派生类型拿到的继承成员不满足该条件，抛 `ArgumentException`。这与「虚方法」无关：非虚的继承成员同样会被拒绝。

## 影响

- `_harmony.PatchAll(...)`（`ModEntry.cs:33`）先执行，核心补丁**全部生效**。
- 异常被 `ModLoaded` 的外层 `catch` 吞掉，后续第二个条件补丁不再尝试；该目标在 110+ 不存在，今天无实际损失。
- 副作用：每次启动记一条 ERROR；成功日志不再输出，排障时容易误判为「补丁未应用」。

## 修复方向

1. `AccessTools.DeclaredPropertyGetter` 替代 `AccessTools.PropertyGetter`（必须）。
2. 第二个条件补丁改用 `AccessTools.DeclaredMethod`（同类缺陷）。
3. 形状守卫中针对 `Synchronize` 的检查改用 `Declared*`，让守卫与 `PatchAll` 的解析口径一致。
4. `PatchIfExists` 增加独立异常处理，失败时记录目标成员与异常并继续。
5. 版本号改为三段式 `1.2.1`，消除加载器告警。

详见 `2.PLANS/002-S-条件补丁守卫三版本修复.md`。

## 验证

- 已验证：0.113 实机日志复现；用真实 0.113 `sts2.dll` + `0Harmony 2.4.2` 反射复现，异常文案逐字一致；`DeclaredPropertyGetter(typeof(Synchronize), ...)` 返回 `null`。
- 已验证：107 的 override `DeclaringType == ReflectedType`，可正常补丁；0.110 / 0.111 / 0.113 均无该 override。
- 未验证：修复后的运行表现（未改代码）。


## 修复结果（2026-11-13）

- `ModEntry` 的 `CanonicalKeywords` 探测改用 `AccessTools.DeclaredPropertyGetter`；0.110+ 返回 `null`，按原意跳过。
- `GetResultPileTypeForCardPlay` 探测改用 `AccessTools.DeclaredMethod`。
- 形状守卫 12 项检查统一改用 `DeclaredPropertyGetter` / `DeclaredMethod`，与 `PatchAll` 的解析口径一致。
- `PatchIfExists` 增加逐补丁 `try/catch`，记录目标成员、补丁类型、方法名与异常消息，不再中断后续条件补丁。
- manifest 版本 `1.2` → `1.2.1`。
- 已构建并覆盖安装到本地 `mods/RevertSynchronize`；0.113 启动日志显示两个条件补丁均按预期跳过，无 `ArgumentException`。107 实机待用户验收。


## 后续修复：1.2.2 ResultPile dupe（2026-11-13）

- 1.2.1 的 `SynchronizeResultPilePatch.Postfix107` 无条件把同步结果改为 `PileType.Discard`，连 dupe 的 `PileType.None` 也覆盖，导致 `HistoryCourse` 复制体滞留弃牌堆。
- 1.2.2 只把 `PileType.Exhaust` 改为 `PileType.Discard`，保留 `None`，dupe 正常 `RemoveFromCombat`。
- 独立子代理对照 0.107 源码做了对抗性审阅：`None`/`Exhaust`/`Discard`/Power/dupe/`ExhaustOnNextPlay` 分支均检查，结论 PASS；0.107 实机仍待用户验收。
