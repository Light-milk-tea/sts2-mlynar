using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MlynarMod.MlynarModCode.Powers;

namespace MlynarMod.MlynarModCode.Patches;

[HarmonyPatch(typeof(PowerModel), nameof(PowerModel.ShouldRemoveDueToAmount))]
public static class KeepPoisePatch
{
    public static void Postfix(PowerModel __instance, ref bool __result)
    {
        if (__instance is PoisePower)
            __result = false;
    }
}
