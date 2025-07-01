using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MealPrinter.HarmonyPatches;

[HarmonyPatch(typeof(JobGiver_Autofeed), "TryGiveJob")]
public static class JobGiver_Autofeed_TryGiveJob
{
    public static bool Prefix(ref Job __result, Pawn pawn)
    {
        if (!pawn.CanReserve(pawn))
        {
            __result = null;
            return false;
        }

        // Find baby and food
        var baby = ChildcareUtility.FindAutofeedBaby(pawn, AutofeedMode.Urgent, out var food);

        if (baby is not { Spawned: true })
        {
            __result = null;
            return false;
        }

        // Prioritize checking for breastfeeders
        if (food != pawn && ChildcareUtility.ImmobileBreastfeederAvailable(pawn, baby, false, out var feeder, out _))
        {
            food = feeder;
        }

        // If the food source is a MealPrinter, search for alternative edible sources
        if (food is Building_MealPrinter)
        {
            food = findAlternativeBabyFood(pawn, baby);
            if (food == null)
            {
                __result = null;
                return false;
            }
        }

        // Proceed with job if valid food is found
        if (food != null)
        {
            __result = ChildcareUtility.MakeAutofeedBabyJob(pawn, baby, food);
            return false;
        }

        __result = null;
        return false;
    }

    // Searches for any edible baby food, insect jelly, or milk in the map
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