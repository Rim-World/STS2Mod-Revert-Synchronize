using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace RevertSynchronize.Patches;

/// <summary>
/// 元数据写死（v0.110 定义）：费用 1 / 类型 技能 / 目标 自身 / 稀有度 罕见 /
/// 无关键词（清除 107 基础版的 Exhaust，110/111 上为锁定）/ 悬停提示仅 Focus。
/// 游戏更新给这张卡添加/修改任何元数据均不影响。
/// </summary>

[HarmonyPatch(typeof(CardModel), "CanonicalEnergyCost", MethodType.Getter)]
public static class SynchronizeEnergyCostPatch
{
    private static void Postfix(CardModel __instance, ref int __result)
    {
        if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
        {
            __result = 1;
        }
    }
}

[HarmonyPatch(typeof(CardEnergyCost), "Canonical", MethodType.Getter)]
public static class SynchronizeEnergyCostCanonicalPatch
{
    private static readonly FieldInfo? CardField = AccessTools.Field(typeof(CardEnergyCost), "_card");

    private static void Postfix(CardEnergyCost __instance, ref int __result)
    {
        if (CardField != null && CardField.GetValue(__instance) is Synchronize && ModConfig.IsReplaceEnabled)
        {
            __result = 1;
        }
    }
}

[HarmonyPatch(typeof(CardModel), "Type", MethodType.Getter)]
public static class SynchronizeTypePatch
{
    private static void Postfix(CardModel __instance, ref CardType __result)
    {
        if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
        {
            __result = CardType.Skill;
        }
    }
}

[HarmonyPatch(typeof(CardModel), "TargetType", MethodType.Getter)]
public static class SynchronizeTargetTypePatch
{
    private static void Postfix(CardModel __instance, ref TargetType __result)
    {
        if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
        {
            __result = TargetType.Self;
        }
    }
}

[HarmonyPatch(typeof(CardModel), "Rarity", MethodType.Getter)]
public static class SynchronizeRarityPatch
{
    private static void Postfix(CardModel __instance, ref CardRarity __result)
    {
        if (__instance is Synchronize && ModConfig.IsReplaceEnabled)
        {
            __result = CardRarity.Uncommon;
        }
    }
}

[HarmonyPatch(typeof(Synchronize), "ExtraHoverTips", MethodType.Getter)]
public static class SynchronizeHoverTipsPatch
{
    private static void Postfix(Synchronize __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (!ModConfig.IsReplaceEnabled)
        {
            return;
        }

        __result = new IHoverTip[] { HoverTipFactory.FromPower<FocusPower>() };
    }
}
