using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RevertSynchronize.Patches;

/// <summary>
/// 关键词写死（v1.2 主挂点）：
/// CardModel.GetKeywordsWithSources 是三版本同名方法、一切关键词读取的必经点
/// （Keywords 属性内部、图鉴/预览文本、克隆、打出判定等全部经过它）。
/// Postfix 过滤 Exhaust —— 覆盖 107 CanonicalKeywords override 进缓存、战斗 hook 等一切来源。
/// 首触发时打印诊断日志（实例类型 + 过滤结果），便于版本回归排查。
/// </summary>
[HarmonyPatch(typeof(CardModel), "GetKeywordsWithSources")]
public static class SynchronizeKeywordsPatch
{
    private static bool _diagnosed;

    private static void Postfix(CardModel __instance, ref IReadOnlySet<CardKeyword> __result)
    {
        if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
        {
            IReadOnlySet<CardKeyword> filtered = __result.Where(k => k != CardKeyword.Exhaust).ToHashSet();
            if (!_diagnosed)
            {
                _diagnosed = true;
                Log.Info($"{ModEntry.ModId}: GetKeywordsWithSources filter applied on {__instance.GetType().Name} " +
                         $"(canonical={__instance.IsCanonical}, combat={__instance.CombatState != null}); " +
                         $"before=[{string.Join(",", __result)}] after=[{string.Join(",", filtered)}]");
            }
            __result = filtered;
        }
    }
}

/// <summary>
/// 107 特有：Synchronize.CanonicalKeywords override（返回 [Exhaust]）的源头补丁。
/// 仅 107 存在该成员（110/111 继承基类），由 ModEntry 条件挂载（PatchAll 无法处理不存在的目标）。
/// 清空源头后，canonical 实例 LocalKeywords 缓存首次填充即为空。
/// </summary>
public static class SynchronizeCanonicalKeywordsPatch
{
    public static void Postfix(Synchronize __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (ModConfig.IsReplaceEnabled)
        {
            __result = Array.Empty<CardKeyword>();
        }
    }
}

/// <summary>
/// 107 特有：打出结果堆钉死 —— GetResultPileTypeForCardPlay() 返回 PileType。
/// 仅 107 存在该方法（110/111 为 GetResultLocationForCardPlay 返回 CardLocation，
/// 由 GetKeywordsWithSources 过滤自动生效，不在此钉死以免引用 107 不存在的类型），
/// 由 ModEntry 条件挂载。Postfix 强制「同步」打出后进弃牌堆。
/// </summary>
public static class SynchronizeResultPilePatch
{
    public static void Postfix107(CardModel __instance, ref PileType __result)
    {
        if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
        {
            __result = PileType.Discard;
        }
    }
}
