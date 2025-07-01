using Verse;

namespace MealPrinter;

public class MealPrinterSettings : ModSettings
{
    public bool PrintSoundEnabled = true;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref PrintSoundEnabled, "printSoundEnabled", true);
        base.ExposeData();
    }
}