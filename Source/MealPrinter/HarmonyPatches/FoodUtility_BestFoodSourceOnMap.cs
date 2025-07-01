using HarmonyLib;
using RimWorld;
using Verse;

namespace MealPrinter.HarmonyPatches;

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
public static class FoodUtility_BestFoodSourceOnMap
{
    private static void Prefix(ref Pawn getter, ref Pawn eater, ref bool allowDispenserFull,
        ref bool allowForbidden, ref bool allowSociallyImproper)
    {
        if (eater.DevelopmentalStage.Baby())
        {
            return;
        }

        MealPrinterMod.BestFoodSourceOnMap = true;
        MealPrinterMod.Getter = getter;
        MealPrinterMod.Eater = eater;
        MealPrinterMod.AllowDispenserFull = allowDispenserFull;
        MealPrinterMod.AllowForbidden = allowForbidden;
        MealPrinterMod.AllowSociallyImproper = allowSociallyImproper;
    }

    private static void Postfix()
    {
        MealPrinterMod.BestFoodSourceOnMap = false;
    }
}