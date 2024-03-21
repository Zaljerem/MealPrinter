using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MealPrinter;

public class Building_MealPrinter : Building_NutrientPasteDispenser
{
    private const float barNutritionCost = 0.5f;

    public static readonly List<ThingDef> validMeals = [];
    private ThingDef mealToPrint;

    public override void PostMake()
    {
        base.PostMake();
        validMeals.Add(ThingDefOf.MealSimple);
        mealToPrint = ThingDefOf.MealSimple;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref mealToPrint, "mealToPrint");
    }

    //Inspect pane string
    public override string GetInspectString()
    {
        var text = base.GetInspectString();
        text += "CurrentPrintSetting".Translate(mealToPrint.label);
        text += "CurrentEfficiency".Translate(GetEfficiency());
        return text;
    }

    //Gizmos
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (MealPrinter_ThingDefOf.MealPrinter_HighRes.IsFinished && !validMeals.Contains(ThingDefOf.MealFine))
        {
            validMeals.Add(ThingDefOf.MealFine);
        }

        if (MealPrinter_ThingDefOf.MealPrinter_Recombinators.IsFinished &&
            !validMeals.Contains(ThingDefOf.MealNutrientPaste))
        {
            validMeals.Add(ThingDefOf.MealNutrientPaste);
        }

        yield return new Command_Action
        {
            defaultLabel = "PrintSettingButton".Translate(mealToPrint.label),
            defaultDesc = GetMealDesc(),
            icon = getMealIcon(),
            Order = -100,
            action = delegate
            {
                var options = new List<FloatMenuOption>();

                if (validMeals != null)
                {
                    foreach (var meal in validMeals)
                    {
                        string label = meal.LabelCap;
                        var option = new FloatMenuOption(label, delegate { SetMealToPrint(meal); });
                        options.Add(option);
                    }
                }

                if (options.Count > 0)
                {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
        };

        if (MealPrinter_ThingDefOf.MealPrinter_DeepResequencing.IsFinished)
        {
            if (!powerComp.PowerOn)
            {
                yield return new Command_Action
                {
                    defaultLabel = "ButtonBulkPrintBars".Translate(),
                    defaultDesc = "ButtonBulkPrintBarsDescNoPower".Translate(),
                    Disabled = true,
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/NutriBar"),
                    Order = -100,
                    action = TryBulkPrintBars
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "ButtonBulkPrintBars".Translate(),
                    defaultDesc = "ButtonBulkPrintBarsDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/NutriBar"),
                    Order = -100,
                    action = TryBulkPrintBars
                };
            }
        }
        else
        {
            yield return new Command_Action
            {
                defaultLabel = "ButtonBulkPrintBars".Translate(),
                defaultDesc = "ButtonBulkPrintBarsDescNoResearch".Translate(),
                Disabled = true,
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/NutriBar"),
                Order = -100,
                action = TryBulkPrintBars
            };
        }
    }

    //Util functions

    //Overriden base TryDispenseFood method
    public override Thing TryDispenseFood()
    {
        if (!CanDispenseNow)
        {
            return null;
        }

        var num = GetNutritionCost();

        var list = new List<ThingDef>();
        do
        {
            var thing = FindFeedInAnyHopper();
            if (thing == null)
            {
                Log.Error("Did not find enough food in hoppers while trying to dispense.");
                return null;
            }

            var num2 = Mathf.Min(thing.stackCount, Mathf.CeilToInt(num / thing.GetStatValue(StatDefOf.Nutrition)));
            num -= num2 * thing.GetStatValue(StatDefOf.Nutrition);
            list.Add(thing.def);
            thing.SplitOff(num2);
        } while (!(num <= 0f));

        playPrintSound();
        var thing2 = ThingMaker.MakeThing(mealToPrint);
        var compIngredients = thing2.TryGetComp<CompIngredients>();
        foreach (var thingDef in list)
        {
            compIngredients.RegisterIngredient(thingDef);
        }

        return thing2;
    }

    //Overriden base HasEnoughFeedstock method
    //This version considers the selected meal type
    public override bool HasEnoughFeedstockInHoppers()
    {
        var num = 0f;
        foreach (var intVec3 in AdjCellsCardinalInBounds)
        {
            Thing feedStock = null;
            Thing hopper = null;
            var thingList = intVec3.GetThingList(Map);
            foreach (var thing in thingList)
            {
                if (IsAcceptableFeedstock(thing.def))
                {
                    feedStock = thing;
                }

                if (thing.def == ThingDefOf.Hopper)
                {
                    hopper = thing;
                }
            }

            if (feedStock != null && hopper != null)
            {
                num += feedStock.stackCount * feedStock.GetStatValue(StatDefOf.Nutrition);
            }

            if (num >= GetNutritionCost())
            {
                return true;
            }
        }

        return false;
    }

    public List<Thing> GetAllHopperedFeedstock()
    {
        var allStock = new List<Thing>();
        for (var i = 0; i < AdjCellsCardinalInBounds.Count; i++)
        {
            var edifice = AdjCellsCardinalInBounds[i].GetEdifice(Map);
            if (edifice == null || edifice.def != ThingDefOf.Hopper)
            {
                continue;
            }

            var thingList = edifice.Position.GetThingList(Map);
            foreach (var thing in thingList)
            {
                if (IsAcceptableFeedstock(thing.def))
                {
                    allStock.Add(thing);
                }
            }
        }

        return allStock;
    }

    //Convert a given list of feedstock stacks into its equivalent in NutriBars
    public int FeedstockBarEquivalent(List<Thing> feedStocks)
    {
        var num = 0f;
        foreach (var thing in feedStocks)
        {
            num += thing.stackCount * thing.GetStatValue(StatDefOf.Nutrition);
        }

        return (int)Math.Floor(num / barNutritionCost);
    }

    //Bulk bar printing gui setup
    private void TryBulkPrintBars()
    {
        var feedStock = GetAllHopperedFeedstock();

        if (feedStock == null || FeedstockBarEquivalent(GetAllHopperedFeedstock()) <= 0)
        {
            Messages.Message("CannotBulkPrintBars".Translate(), MessageTypeDefOf.RejectInput, false);
            return;
        }

        var maxPossibleBars = FeedstockBarEquivalent(feedStock);
        var maxAllowedBars = 30;
        if (maxPossibleBars > maxAllowedBars)
        {
            maxPossibleBars = 30;
        }

        var window = new Dialog_PrintBars(TextGetter, 1, maxPossibleBars,
            delegate(int x, bool forbidden, bool rear) { ConfirmAction(x, feedStock, forbidden, rear); }, 1);
        Find.WindowStack.Add(window);
        return;

        string TextGetter(int x)
        {
            return "SetBarBatchSize".Translate(x, maxAllowedBars);
        }
    }

    //Bulk bar printing action
    public void ConfirmAction(int x, List<Thing> feedStock, bool forbidden, bool rear)
    {
        playPrintSound();

        var nutritionCost = x * barNutritionCost;

        var nutritionRemaining = nutritionCost;
        var list = new List<ThingDef>();
        do
        {
            var feed = FindFeedInAnyHopper();
            var nutritionToConsume = Mathf.Min(feed.stackCount,
                Mathf.CeilToInt(barNutritionCost / feed.GetStatValue(StatDefOf.Nutrition)));
            nutritionRemaining -= nutritionToConsume * feed.GetStatValue(StatDefOf.Nutrition);
            list.Add(feed.def);
            feed.SplitOff(nutritionToConsume);

            var bars = ThingMaker.MakeThing(MealPrinter_ThingDefOf.MealPrinter_NutriBar);
            var compIngredients = bars.TryGetComp<CompIngredients>();
            foreach (var thingDef in list)
            {
                compIngredients.RegisterIngredient(thingDef);
            }

            GenPlace.TryPlaceThing(bars, rear ? GetRearCell(InteractionCell) : InteractionCell, Map,
                ThingPlaceMode.Near);

            if (forbidden)
            {
                bars.SetForbidden(true);
            }
        } while (!(nutritionRemaining <= 0f));

        playPrintSound();
    }

    //Internally define set meal
    private void SetMealToPrint(ThingDef mealDef)
    {
        mealToPrint = mealDef;
    }

    //Get meal icon for gizmo
    private Texture2D getMealIcon()
    {
        return mealToPrint == ThingDefOf.MealSimple
            ? ContentFinder<Texture2D>.Get("UI/Buttons/MealSimple")
            : ContentFinder<Texture2D>.Get(mealToPrint == ThingDefOf.MealFine
                ? "UI/Buttons/MealFine"
                : "UI/Buttons/MealNutrientPaste");
    }

    //Get meal desc for gizmo
    private string GetMealDesc()
    {
        if (mealToPrint == ThingDefOf.MealSimple)
        {
            return "SimpleMealDesc".Translate();
        }

        return mealToPrint == ThingDefOf.MealFine ? "FineMealDesc".Translate() : "PasteMealDesc".Translate();
    }

    //Get print efficiency for inspect pane
    private string GetEfficiency()
    {
        if (mealToPrint == ThingDefOf.MealSimple)
        {
            return "SimpleMealEff".Translate();
        }

        return mealToPrint == ThingDefOf.MealFine ? "FineMealEff".Translate() : "PasteMealEff".Translate();
    }

    //Get nutrition cost for each potential meal type
    private float GetNutritionCost()
    {
        var num = 0.3f;
        if (mealToPrint.Equals(ThingDefOf.MealNutrientPaste))
        {
            num = (float)((def.building.nutritionCostPerDispense * 0.5) - 0.0001f);
        }
        else if (mealToPrint.Equals(ThingDefOf.MealSimple))
        {
            num = 0.5f - 0.0001f;
        }
        else if (mealToPrint.Equals(ThingDefOf.MealFine))
        {
            num = 0.75f - 0.0001f;
        }

        return num;
    }

    //Get the ThingDef of the current meal
    public ThingDef GetMealThing()
    {
        return mealToPrint;
    }

    //Check if pawn's food restrictions allow consumption of the set meal
    public bool CanPawnPrint(Pawn p)
    {
        return p.foodRestriction.CurrentFoodPolicy.Allows(mealToPrint);
    }

    //Plays print sound according to settings
    private void playPrintSound()
    {
        if (LoadedModManager.GetMod<MealPrinterMod>().GetSettings<MealPrinterSettings>().printSoundEnabled)
        {
            def.building.soundDispense.PlayOneShot(new TargetInfo(Position, Map));
        }
    }

    private IntVec3 GetRearCell(IntVec3 cell)
    {
        //0 1 2 3
        //facing up right down left
        //this has got to be the stupidest fucking possible means of getting this information and I do not care

        var rot = Rotation.ToString();
        switch (rot)
        {
            case "0":
                cell.z -= 6;
                break;
            case "1":
                cell.x -= 6;
                break;
            case "2":
                cell.z += 6;
                break;
            case "3":
                cell.x += 6;
                break;
            default:
                Log.Error("[MealPrinter] Couldn't get printer rotation, falling back onto Interaction Cell.");
                break;
        }

        return cell;
    }
}