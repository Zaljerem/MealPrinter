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

    //the method was private, so I just copied and pasted that bitch
    public static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
    {
        if (eater.DevelopmentalStage.Baby())
        {
            return false;
        }

        if (allowSociallyImproper)
        {
            return true;
        }

        var animalsCare = !getter.RaceProps.Animal;
        return t.IsSociallyProper(getter) || t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare);
    }
}