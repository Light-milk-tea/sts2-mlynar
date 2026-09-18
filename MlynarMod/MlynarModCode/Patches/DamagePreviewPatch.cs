using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;

namespace MlynarMod.MlynarModCode.Patches;

[HarmonyPatch]
public static class DamagePreviewPatch
{
    [HarmonyPatch(typeof(DamageVar), nameof(DamageVar.UpdateCardPreview))]
    [HarmonyPrefix]
    public static void PrefixDamage() => MlynarRuntime.PreviewBonusApplied = false;

    [HarmonyPatch(typeof(DamageVar), nameof(DamageVar.UpdateCardPreview))]
    [HarmonyPostfix]
    public static void PostfixDamage(DamageVar __instance, CardModel card, bool runGlobalHooks) =>
        ApplyIfMissing(__instance, card, runGlobalHooks);

    [HarmonyPatch(typeof(CalculatedDamageVar), nameof(CalculatedDamageVar.UpdateCardPreview))]
    [HarmonyPrefix]
    public static void PrefixCalculated() => MlynarRuntime.PreviewBonusApplied = false;

    [HarmonyPatch(typeof(CalculatedDamageVar), nameof(CalculatedDamageVar.UpdateCardPreview))]
    [HarmonyPostfix]
    public static void PostfixCalculated(CalculatedDamageVar __instance, CardModel card, bool runGlobalHooks) =>
        ApplyIfMissing(__instance, card, runGlobalHooks);

    private static void ApplyIfMissing(DynamicVar var, CardModel card, bool runGlobalHooks)
    {
        if (!runGlobalHooks || MlynarRuntime.PreviewBonusApplied) return;
        if (var is DamageVar damage && !damage.Props.IsPoweredAttack()) return;
        if (var is CalculatedDamageVar calculated && !calculated.Props.IsPoweredAttack()) return;

        var bonus = MlynarRuntime.AttackBonus(card);
        if (bonus != 0)
            var.PreviewValue += bonus;
    }
}
