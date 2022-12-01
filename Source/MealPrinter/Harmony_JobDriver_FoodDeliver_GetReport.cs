using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MealPrinter;

[HarmonyPatch(typeof(JobDriver_FoodDeliver), nameof(JobDriver_FoodDeliver.GetReport))]
public static class Harmony_JobDriver_FoodDeliver_GetReport
{
    private static void Postfix(JobDriver_FoodDeliver __instance, ref string __result)
    {
        if (__instance.job.GetTarget(TargetIndex.A).Thing is Building_MealPrinter &&
            (Pawn)__instance.job.targetB.Thing != null)
        {
            __result = __instance.job.def.reportString.Replace("TargetA", "printed meal")
                .Replace("TargetB", __instance.job.targetB.Thing.LabelShort);
        }
    }
}