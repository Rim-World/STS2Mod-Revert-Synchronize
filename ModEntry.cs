using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RevertSynchronize.Patches;

namespace RevertSynchronize;

[ModInitializer("ModLoaded")]
public static class ModEntry
{
    public const string ModId = "RevertSynchronize";

    private static Harmony? _harmony;

    public static void ModLoaded()
    {
        try
        {
            Log.Info($"{ModId}: loading...");

            // 静态形状检查：Synchronize 类型与全部补丁目标成员存在才应用补丁。
            if (!SynchronizeShapeGuard.ValidateStatic())
            {
                Log.Warn($"{ModId}: Synchronize or required members were not found in this game version; all patches are disabled.");
                return;
            }

            _harmony = new Harmony(ModId);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            // 条件补丁（仅目标成员存在时挂载；PatchAll 无法处理不存在的目标）：
            // 1) 107 特有：Synchronize.CanonicalKeywords override（返回 [Exhaust]）→ 源头清空。
            //    110/111 无此成员（继承基类），自动跳过。
            bool hasCanonicalKeywords = PatchIfExists(
                AccessTools.PropertyGetter(typeof(Synchronize), "CanonicalKeywords"),
                typeof(SynchronizeCanonicalKeywordsPatch), nameof(SynchronizeCanonicalKeywordsPatch.Postfix));

            // 2) 107 特有：GetResultPileTypeForCardPlay() → 打出结果钉死为弃牌堆。
            //    110/111 为 GetResultLocationForCardPlay()（返回 CardLocation），
            //    由 GetKeywordsWithSources 过滤自动生效，无需钉死。
            bool hasResultPile = PatchIfExists(
                AccessTools.Method(typeof(CardModel), "GetResultPileTypeForCardPlay"),
                typeof(SynchronizeResultPilePatch), nameof(SynchronizeResultPilePatch.Postfix107));

            Log.Info($"{ModId}: Harmony patches applied (Synchronize -> v0.110 behavior); " +
                     $"conditional: CanonicalKeywords(107)={hasCanonicalKeywords}, ResultPile(107)={hasResultPile}");
        }
        catch (Exception e)
        {
            Log.Error($"{ModId}: failed to apply patches: {e}");
        }
    }

    private static bool PatchIfExists(MethodBase? target, Type patchType, string postfixMethodName)
    {
        if (target == null)
        {
            return false;
        }

        _harmony!.Patch(target, postfix: new HarmonyMethod(patchType, postfixMethodName));
        return true;
    }
}
