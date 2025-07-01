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
        new Harmony("MealPrinter").PatchAll(Assembly.GetExecutingAssembly());
    }

    public static ThingDef InsectJelly => ThingDef.Named("InsectJelly");
    public static ThingDef Milk => ThingDef.Named("Milk");

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