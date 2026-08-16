using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RevertSynchronize.Patches;

/// <summary>
/// 文本写死：本地化文件新增 RE_SYNCHRONIZE.title/.description 键
/// （模组专属前缀，避免与其他 mod 覆盖 SYNCHRONIZE.* 键冲突），由这里切换到新键。
/// 描述为官方 v0.110 模板（数字由写死 DynamicVars 渲染为基础 2 / 升级 3）。
/// </summary>
[HarmonyPatch(typeof(CardModel), "TitleLocString", MethodType.Getter)]
public static class SynchronizeTitlePatch
{
    private static void Postfix(CardModel __instance, ref LocString __result)
    {
        if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
        {
            __result = new LocString("cards", "RE_SYNCHRONIZE.title");
        }
    }
}

[HarmonyPatch(typeof(CardModel), "Description", MethodType.Getter)]
public static class SynchronizeDescriptionPatch
{
    private static void Postfix(CardModel __instance, ref LocString __result)
    {
        if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
        {
            __result = new LocString("cards", "RE_SYNCHRONIZE.description");
        }
    }
}
