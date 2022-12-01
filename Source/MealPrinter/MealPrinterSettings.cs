using Verse;

namespace MealPrinter;

public class MealPrinterSettings : ModSettings
{
    public bool printSoundEnabled = true;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref printSoundEnabled, "printSoundEnabled", true);
        base.ExposeData();
    }
}