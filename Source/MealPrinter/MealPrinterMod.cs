using Mlie;
using UnityEngine;
using Verse;

namespace MealPrinter;

public class MealPrinterMod : Mod
{
    public static bool AllowForbidden;
    public static bool AllowDispenserFull;
    public static Pawn Getter;
    public static Pawn Eater;
    public static bool AllowSociallyImproper;
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
        listingStandard.CheckboxLabeled("PrintSFXLabel".Translate(), ref settings.PrintSoundEnabled);
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