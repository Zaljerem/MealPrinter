using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MealPrinter.HarmonyPatches;

[HarmonyPatch(typeof(FoodUtility), "SpawnedFoodSearchInnerScan", null)]
public static class FoodUtility_SpawnedFoodSearchInnerScan
{
    public static bool Prefix(ref Predicate<Thing> validator)
    {
        var malidator = validator;

        validator = salivator;
        return true;

        bool salivator(Thing x)
        {
            return x is Building_MealPrinter rep ? PrintDel(rep) : malidator(x);
        }
    }

    private static bool PrintDel(Building_MealPrinter t)
    {
        return MealPrinterMod.AllowDispenserFull
               && MealPrinterMod.Getter.RaceProps.ToolUser &&
               MealPrinterMod.Getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)
               && (t.Faction == MealPrinterMod.Getter.Faction || t.Faction == MealPrinterMod.Getter.HostFaction)
               && (MealPrinterMod.AllowForbidden || !t.IsForbidden(MealPrinterMod.Getter))
               && t.powerComp.PowerOn
               && t.InteractionCell.Standable(t.Map)
               && MealPrinterMain.IsFoodSourceOnMapSociallyProper(t, MealPrinterMod.Getter, MealPrinterMod.Eater,
                   MealPrinterMod.AllowSociallyImproper)
               && !MealPrinterMod.Getter.IsWildMan()
               && t.CanPawnPrint(MealPrinterMod.Eater)
               && !MealPrinterMod.Eater.DevelopmentalStage.Baby()
               && t.HasEnoughFeedstockInHoppers()
               && MealPrinterMod.Getter.Map.reachability.CanReachNonLocal(MealPrinterMod.Getter.Position,
                   new TargetInfo(t.InteractionCell, t.Map),
                   PathEndMode.OnCell, TraverseParms.For(MealPrinterMod.Getter, Danger.Some));
    }
}