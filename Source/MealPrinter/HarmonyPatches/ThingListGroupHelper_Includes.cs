using HarmonyLib;
using Verse;

namespace MealPrinter.HarmonyPatches;

[HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
public static class ThingListGroupHelper_Includes
{
    private static bool Prefix(ref ThingRequestGroup group, ref ThingDef def, ref bool __result)
    {
        if (group != ThingRequestGroup.FoodSource && group != ThingRequestGroup.FoodSourceNotPlantOrTree)
        {
            return true;
        }

        if (def.thingClass != typeof(Building_MealPrinter))
        {
            return true;
        }

        __result = true;

        return false;
    }
}