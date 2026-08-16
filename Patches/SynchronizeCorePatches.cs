using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace RevertSynchronize.Patches;

/// <summary>
/// 数值写死：CanonicalVars 整体替换为 v0.110 定义 ——
/// [基础 0, 额外 2, CalculatedFocus = 2 × 不同充能球种类数]（升级由 OnUpgrade +1 → 3）。
/// 卡面文字经 {CalculationExtra:diff()} / {CalculatedFocus:diff()} 占位符由本数组渲染。
/// 107 上同时修正基础值口径；不读取原版任何数值。
/// </summary>
[HarmonyPatch(typeof(Synchronize), "CanonicalVars", MethodType.Getter)]
public static class SynchronizeCanonicalVarsPatch
{
    private static void Postfix(Synchronize __instance, ref IEnumerable<DynamicVar> __result)
    {
        if (!ModConfig.IsReplaceEnabled)
        {
            return;
        }

        __result = new DynamicVar[]
        {
            new CalculationBaseVar(0m),
            new CalculationExtraVar(2m),
            new CalculatedVar("CalculatedFocus").WithMultiplier((CardModel card, Creature? _) =>
                (from orb in card.Owner.PlayerCombatState!.OrbQueue.Orbs
                 group orb by orb.Id).Count())
        };
    }
}

/// <summary>
/// 打出行为写死：整体替换 Synchronize.OnPlay（v0.110 语义）——
/// 播放入场动画后，对自身施加 SynchronizePower（临时集中，本回合有效），
/// 层数 = CalculatedFocus = 额外值 × 当前不同充能球种类数。
/// </summary>
[HarmonyPatch(typeof(Synchronize), "OnPlay")]
public static class SynchronizeOnPlayPatch
{
    private static bool Prefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, Synchronize __instance, ref Task __result)
    {
        if (!ModConfig.IsReplaceEnabled)
        {
            return true;
        }

        __result = HardCodedOnPlay(choiceContext, cardPlay, __instance);
        return false;
    }

    private static async Task HardCodedOnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, Synchronize instance)
    {
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.CastAnimDelay);
        decimal focus = ((CalculatedVar)instance.DynamicVars["CalculatedFocus"]).Calculate(cardPlay.Target);
        await PowerCmd.Apply<SynchronizePower>(choiceContext, instance.Owner.Creature, focus, instance.Owner.Creature, instance);
    }
}

/// <summary>
/// 升级行为写死：整体替换 Synchronize.OnUpgrade —— 额外值 +1（基础 2 → 升级 3，v0.110 语义）。
/// 107 原版升级为"移除消耗"且不加值，本补丁统一为 110 行为；
/// 升级数值经 DynamicVar.UpgradeValueBy 引擎通用机制记录，升级预览/图鉴渲染自动正确。
/// </summary>
[HarmonyPatch(typeof(Synchronize), "OnUpgrade")]
public static class SynchronizeOnUpgradePatch
{
    private static bool Prefix(Synchronize __instance)
    {
        if (!ModConfig.IsReplaceEnabled)
        {
            return true;
        }

        __instance.DynamicVars.CalculationExtra.UpgradeValueBy(1m);
        return false;
    }
}
