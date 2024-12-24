using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MealPrinter.HarmonyPatches;

[HarmonyPatch(typeof(JobDriver_FoodFeedPatient), nameof(JobDriver_FoodFeedPatient.GetReport))]
public static class JobDriver_FoodFeedPatient_GetReport
{
    private static void Postfix(JobDriver_FoodFeedPatient __instance, ref string __result)
    {
        if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_MealPrinter &&
            (Pawn)__instance.job.targetB.Thing != null)
        {
            __result = __instance.job.def.reportString.Replace("TargetA", "printed meal")
                .Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
        }
    }
}