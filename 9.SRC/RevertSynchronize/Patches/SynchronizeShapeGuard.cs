using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RevertSynchronize.Patches;

/// <summary>
/// 守卫：确认游戏里确实有「同步」（Synchronize）这张卡才会启用全部补丁。
/// 1) ValidateStatic()：ModLoaded 时检查 Synchronize 类型与所有补丁目标成员存在；
///    任一缺失（例如未来版本删卡/改结构）→ 不应用任何补丁并记录警告，游戏保持原版行为。
/// 2) IsSupported()：运行时延迟检查 ModelDb 中该卡的规范实例存在（ModelDb 未就绪时暂放行，
///    下次访问再评估）。补丁目标全部经 ModConfig.IsReplaceEnabled 统一接入。
/// </summary>
public static class SynchronizeShapeGuard
{
    private static bool _staticOk;

    private static bool? _modelOk;

    private static bool _warned;

    /// <summary>类型与补丁目标成员的存在性检查（无副作用，ModLoaded 时调用一次）。</summary>
    public static bool ValidateStatic()
    {
        try
        {
            _staticOk =
                AccessTools.PropertyGetter(typeof(CardModel), "CanonicalEnergyCost") != null
                && AccessTools.PropertyGetter(typeof(CardModel), "Type") != null
                && AccessTools.PropertyGetter(typeof(CardModel), "TargetType") != null
                && AccessTools.PropertyGetter(typeof(CardModel), "Rarity") != null
                && AccessTools.Method(typeof(CardModel), "GetKeywordsWithSources") != null
                && AccessTools.PropertyGetter(typeof(CardModel), "TitleLocString") != null
                && AccessTools.PropertyGetter(typeof(CardModel), "Description") != null
                && AccessTools.PropertyGetter(typeof(CardEnergyCost), "Canonical") != null
                && AccessTools.PropertyGetter(typeof(Synchronize), "CanonicalVars") != null
                && AccessTools.PropertyGetter(typeof(Synchronize), "ExtraHoverTips") != null
                && AccessTools.Method(typeof(Synchronize), "OnPlay") != null
                && AccessTools.Method(typeof(Synchronize), "OnUpgrade") != null;
        }
        catch
        {
            _staticOk = false;
        }

        if (!_staticOk)
        {
            MarkUnsupported("Synchronize or required members were not found in this game version");
        }

        return _staticOk;
    }

    /// <summary>运行时是否启用：静态检查通过且 ModelDb 中确实存在「同步」。</summary>
    public static bool IsSupported()
    {
        if (!_staticOk)
        {
            return false;
        }

        if (_modelOk.HasValue)
        {
            return _modelOk.Value;
        }

        try
        {
            _ = ModelDb.Card<Synchronize>();
            _modelOk = true;
        }
        catch
        {
            // ModelDb 尚未就绪时暂不拦截，下次访问再评估
            return true;
        }

        return _modelOk.Value;
    }

    private static void MarkUnsupported(string reason)
    {
        if (!_warned)
        {
            _warned = true;
            Log.Warn($"{ModEntry.ModId}: {reason}; all patches are disabled.");
        }
    }
}
