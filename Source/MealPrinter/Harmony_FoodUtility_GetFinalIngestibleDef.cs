using HarmonyLib;
using RimWorld;
using Verse;

namespace MealPrinter;

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetFinalIngestibleDef))]
public static class Harmony_FoodUtility_GetFinalIngestibleDef
{
    private static bool Prefix(ref Thing foodSource, ref ThingDef __result)
    {
        if (foodSource is not Building_MealPrinter printer || !MealPrinterMod.BestFoodSourceOnMap)
        {
            return true;
        }

        __result = printer.GetMealThing();
        return false;
    }
}