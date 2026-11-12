# 加载期异常：`CanonicalKeywords` 条件补丁抛 `ArgumentException`

- 日期：2026-11-13
- 评估对象：本 mod v1.2（DLL 构建于 2026-11）
- 触发版本：游戏 v0.110.0 及以上（含当前 v0.113.0），**非 113 新引入**
- 结论：**加载期记 ERROR**，但主补丁已生效，功能未失效。属于守卫逻辑缺陷。

## 现象

v0.113.0 实机日志（`%APPDATA%\SlayTheSpire2\logs\godot2026-11-13T04.45.16.log:769`）：

```
[ERROR] RevertSynchronize: failed to apply patches: System.ArgumentException:
  You can only patch implemented methods/constructors. Patch the declared method virtual
  System.Collections.Generic.IEnumerable`1<...CardKeyword> MegaCrit.Sts2.Core.Models.CardModel::get_CanonicalKeywords() instead.
   at RevertSynchronize.ModEntry.PatchIfExists(...) ModEntry.cs:line 65
   at RevertSynchronize.ModEntry.ModLoaded() ModEntry.cs:line 38
```

同一段日志上方没有 `RevertSynchronize: Harmony patches applied ...`，其后也没有新的补丁日志。

## 原因

`ModEntry.cs:38-40` 用 `AccessTools.PropertyGetter(typeof(Synchronize), "CanonicalKeywords")` 探测「107 特有成员」。该调用不是「声明查找」，会沿继承链返回 `CardModel` 的 `protected virtual` getter：

| 位置 | 内容 |
|---|---|
| `STS2-Reference/3.STS2-113Reference/decompiled/MegaCrit.Sts2.Core.Models/CardModel.cs:579` | `public virtual IEnumerable<CardKeyword> CanonicalKeywords => ...` |
| `STS2-Reference/3.STS2-113Reference/decompiled/MegaCrit.Sts2.Core.Models.Cards/Synchronize.cs` | 113 无该成员 override，继承基类 |
| `STS2-Reference/1.STS2-107Reference/decompiled/MegaCrit.Sts2.Core.Models.Cards/Synchronize.cs:28` | 107 才有 override |

因此 `PatchIfExists` 判定「目标存在」，把**基类虚方法**交给 Harmony。Harmony 拒绝「用派生类型补丁基类虚方法」，抛 `ArgumentException`。

注释 `ModEntry.cs:35-37` 的假设「110/111 无此成员（继承基类），自动跳过」不成立：`PropertyGetter` 会找到继承来的成员。

## 影响

- `_harmony.PatchAll(...)`（`ModEntry.cs:33`）先执行，`CanonicalVars`、`OnPlay`、`OnUpgrade`、关键词过滤、文本、元数据补丁**全部生效**，卡牌行为未被破坏。
- 异常被 `ModLoaded` 的外层 `catch` 吞掉，后续「第二个条件补丁」`GetResultPileTypeForCardPlay` 不再尝试。该目标在 110+ 本来就不存在，因此今天无实际损失。
- 副作用：每次启动记一条 ERROR；`Harmony patches applied` 成功日志不再输出，排障时容易误判为「补丁未应用」。

## 修复方向（未实施）

1. 用 `AccessTools.DeclaredPropertyGetter` 替代 `AccessTools.PropertyGetter`，让继承成员返回 `null`，符合原意。
2. 或先判断 `PropertyGetter(...).DeclaringType == typeof(Synchronize)`。
3. 把两个条件补丁分别 `try/catch`，一个失败不影响另一个。

## 验证

- 已验证：v0.113.0 实机日志复现（2026-11-13 04:45）；两次启动各一份（`godot2026-11-13T04.42.56.log:774`、`godot2026-11-13T04.45.16.log:769`）。
- 已验证：111 与 113 的 `Synchronize.cs` 均无 `CanonicalKeywords` override，异常在 110+ 均会触发。
- 未验证：修复后的运行表现（未改代码）。
