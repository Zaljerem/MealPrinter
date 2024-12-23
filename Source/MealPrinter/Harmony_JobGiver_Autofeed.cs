using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MealPrinter
{
    [HarmonyPatch(typeof(JobGiver_Autofeed), "TryGiveJob")]
    public static class Harmony_JobGiver_Autofeed_TryGiveJob
    {
        public static bool Prefix(JobGiver_Autofeed __instance, ref Job __result, Pawn pawn)
        {
            if (!pawn.CanReserve(pawn))
            {
                __result = null;
                return false;
            }

            // Find baby and food
            Thing food;
            Pawn baby = ChildcareUtility.FindAutofeedBaby(pawn, AutofeedMode.Urgent, out food);

            if (baby == null || !baby.Spawned)
            {
                __result = null;
                return false;
            }

            // Prioritize checking for breastfeeders
            if (food != pawn && ChildcareUtility.ImmobileBreastfeederAvailable(pawn, baby, forced: false, out var feeder, out var _))
            {
                food = feeder;
            }

            // If the food source is a MealPrinter, search for alternative edible sources
            if (food is Building_MealPrinter)
            {
                food = FindAlternativeBabyFood(pawn, baby);
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
