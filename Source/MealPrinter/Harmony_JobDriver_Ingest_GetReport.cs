using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace MealPrinter;

[HarmonyPatch(typeof(JobDriver_Ingest), nameof(JobDriver_Ingest.GetReport))]
public static class Harmony_JobDriver_Ingest_GetReport
{
    private static void Postfix(JobDriver_Ingest __instance, ref string __result)
    {
        __result = __instance.job.def.reportString.Replace("TargetA",
            __instance.job.GetTarget(TargetIndex.A).Thing is Building_MealPrinter
                ? "printed meal"
                : __instance.job.GetTarget(TargetIndex.A).Thing.Label);
    }
}