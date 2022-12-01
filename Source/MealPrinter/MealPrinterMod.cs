using Mlie;
using UnityEngine;
using Verse;

namespace MealPrinter;

public class MealPrinterMod : Mod
{
    public static bool allowForbidden;
    public static bool allowDispenserFull;
    public static Pawn getter;
    public static Pawn eater;
    public static bool allowSociallyImproper;
    public static bool BestFoodSourceOnMap;

    private static string currentVersion;
    private readonly MealPrinterSettings settings;

    public MealPrinterMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<MealPrinterSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("PrintSFXLabel".Translate(), ref settings.printSoundEnabled);
        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("MePr.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "MealPrinterModName".Translate();
    }
}