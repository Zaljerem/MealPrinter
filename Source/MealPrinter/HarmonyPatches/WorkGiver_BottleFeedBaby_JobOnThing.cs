using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MealPrinter.HarmonyPatches;

[HarmonyPatch(typeof(WorkGiver_BottleFeedBaby), nameof(WorkGiver_BottleFeedBaby.JobOnThing))]
public static class WorkGiver_BottleFeedBaby_JobOnThing
{
    public static bool Prefix(WorkGiver_BottleFeedBaby __instance, ref Job __result, Pawn pawn, Thing t, bool forced)
    {
        // Run original checks
        if (!__instance.CanCreateManualFeedingJob(pawn, t, forced))
        {
            __result = null;
            return false; // Skip the original method
        }

        var baby = (Pawn)t;

        // Look for food, prioritizing multiple sources
        var foodSource = findAlternativeBabyFood(pawn, baby);

        switch (foodSource)
        {
            // **Block Meal Printer**
            case Building_MealPrinter:
            // Fail if no valid food is found
            case null:
                JobFailReason.Is("NoBabyFood".Translate());
                __result = null;
                return false;
            default:
                // Create the bottle-feeding job with the found food source
                __result = ChildcareUtility.MakeBottlefeedJob(baby, foodSource);
                return false; // Skip the original method since we've handled the result
        }
    }

    // Searches for Baby Food, Insect Jelly, or Milk on the map
    private static Thing findAlternativeBabyFood(Pawn pawn, Pawn baby)
    {
        var potentialFoods = new List<Thing>();
        if (baby.foodRestriction.BabyFoodAllowed(ThingDefOf.BabyFood))
        {
            potentialFoods.AddRange(pawn.Map.listerThings.ThingsOfDef(ThingDefOf.BabyFood));
        }

        if (baby.foodRestriction.BabyFoodAllowed(MealPrinterMain.InsectJelly))
        {
            potentialFoods.AddRange(pawn.Map.listerThings.ThingsOfDef(MealPrinterMain.InsectJelly));
        }

        if (baby.foodRestriction.BabyFoodAllowed(MealPrinterMain.Milk))
        {
            potentialFoods.AddRange(pawn.Map.listerThings.ThingsOfDef(MealPrinterMain.Milk));
        }

        foreach (var food in potentialFoods)
        {
            // **Skip Meal Printer Directly**
            if (food is Building_MealPrinter)
            {
                continue; // Ignore the meal printer entirely
            }

            if (!pawn.CanReserve(food) || food.stackCount <= 0)
            {
                continue;
            }

            // Ensure the food is edible by the baby
            if (FoodUtility.WillIngestFromInventoryNow(baby, food))
            {
                return food;
            }
        }

        return null;
    }
}