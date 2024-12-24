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
        return MealPrinterMod.allowDispenserFull
               && MealPrinterMod.getter.RaceProps.ToolUser &&
               MealPrinterMod.getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)
               && (t.Faction == MealPrinterMod.getter.Faction || t.Faction == MealPrinterMod.getter.HostFaction)
               && (MealPrinterMod.allowForbidden || !t.IsForbidden(MealPrinterMod.getter))
               && t.powerComp.PowerOn
               && t.InteractionCell.Standable(t.Map)
               && MealPrinterMain.IsFoodSourceOnMapSociallyProper(t, MealPrinterMod.getter, MealPrinterMod.eater,
                   MealPrinterMod.allowSociallyImproper)
               && !MealPrinterMod.getter.IsWildMan()
               && t.CanPawnPrint(MealPrinterMod.eater)
               && !MealPrinterMod.eater.DevelopmentalStage.Baby()
               && t.HasEnoughFeedstockInHoppers()
               && MealPrinterMod.getter.Map.reachability.CanReachNonLocal(MealPrinterMod.getter.Position,
                   new TargetInfo(t.InteractionCell, t.Map),
                   PathEndMode.OnCell, TraverseParms.For(MealPrinterMod.getter, Danger.Some));
    }
}