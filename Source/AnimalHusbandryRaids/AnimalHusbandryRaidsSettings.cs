using Verse;

namespace AnimalHusbandryRaids;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class AnimalHusbandryRaidsSettings : ModSettings
{
    public bool OnlyVeneratedAnimals;
    public bool VerboseLogging;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref OnlyVeneratedAnimals, "OnlyVeneratedAnimals");
        Scribe_Values.Look(ref VerboseLogging, "VerboseLogging");
    }
}