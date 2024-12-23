using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace MealPrinter
{

    [HarmonyPatch(typeof(WorkGiver_BottleFeedBaby), "JobOnThing")]
    public static class Harmony_WorkGiver_BottleFeedBaby_JobOnThing
    {
        public static bool Prefix(WorkGiver_BottleFeedBaby __instance, ref Job __result, Pawn pawn, Thing t, bool forced)
        {
            // Run original checks
            if (!__instance.CanCreateManualFeedingJob(pawn, t, forced))
            {
                __result = null;
                return false;  // Skip the original method
            }

            Pawn baby = (Pawn)t;

            // Look for food, prioritizing multiple sources
            Thing foodSource = FindAlternativeBabyFood(pawn, baby);

            // **Block Meal Printer**
            if (foodSource is Building_MealPrinter)
            {
                JobFailReason.Is("NoBabyFood".Translate());
                __result = null;
                return false;
            }

            // Fail if no valid food is found
            if (foodSource == null)
            {
                JobFailReason.Is("NoBabyFood".Translate());
                __result = null;
                return false;
            }

            // Create the bottle-feeding job with the found food source
            __result = ChildcareUtility.MakeBottlefeedJob(baby, foodSource);
            return false;  // Skip the original method since we've handled the result
        }

        // Searches for Baby Food, Insect Jelly, or Milk on the map
        private static Thing FindAlternativeBabyFood(Pawn pawn, Pawn baby)
        {
            List<Thing> potentialFoods = new List<Thing>();
            if (baby.foodRestriction.BabyFoodAllowed(ThingDefOf.BabyFood))
            {
                potentialFoods.AddRange(pawn.Map.listerThings.ThingsOfDef(ThingDefOf.BabyFood));
            }
            if (baby.foodRestriction.BabyFoodAllowed(ThingDef.Named("InsectJelly")))
            {
                potentialFoods.AddRange(pawn.Map.listerThings.ThingsOfDef(ThingDef.Named("InsectJelly")));
            }
            if (baby.foodRestriction.BabyFoodAllowed(ThingDef.Named("Milk")))
            {
                potentialFoods.AddRange(pawn.Map.listerThings.ThingsOfDef(ThingDef.Named("Milk")));
            }

            foreach (Thing food in potentialFoods)
            {
                // **Skip Meal Printer Directly**
                if (food is Building_MealPrinter)
                {
                    continue;  // Ignore the meal printer entirely
                }

                if (pawn.CanReserve(food) && food.stackCount > 0)
                {
                    // Ensure the food is edible by the baby
                    if (FoodUtility.WillIngestFromInventoryNow(baby, food))
                    {
                        return food;
                    }
                }
            }

            return null;
        }
    }



}
