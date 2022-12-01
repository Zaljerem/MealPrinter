using HarmonyLib;
using RimWorld;
using Verse;

namespace MealPrinter;

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap_NewTemp))]
public static class Harmony_FoodUtility_BestFoodSourceOnMap
{
    private static void Prefix(ref Pawn getter, ref Pawn eater, ref bool allowDispenserFull,
        ref bool allowForbidden, ref bool allowSociallyImproper)
    {
        MealPrinterMod.BestFoodSourceOnMap = true;
        MealPrinterMod.getter = getter;
        MealPrinterMod.eater = eater;
        MealPrinterMod.allowDispenserFull = allowDispenserFull;
        MealPrinterMod.allowForbidden = allowForbidden;
        MealPrinterMod.allowSociallyImproper = allowSociallyImproper;
    }

    private static void Postfix(ref Thing __result)
    {
        MealPrinterMod.BestFoodSourceOnMap = false;
    }
}