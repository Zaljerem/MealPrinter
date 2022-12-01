using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MealPrinter;

[StaticConstructorOnStartup]
public static class MealPrinterMain
{
    static MealPrinterMain()
    {
        Log.Message("[MealPrinter] Okay, showtime!");
        new Harmony("MealPrinter").PatchAll(Assembly.GetExecutingAssembly());
    }

    //the method was private so i just copy pasted that bitch
    public static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
    {
        if (allowSociallyImproper)
        {
            return true;
        }

        var animalsCare = !getter.RaceProps.Animal;
        return t.IsSociallyProper(getter) || t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare);
    }
}