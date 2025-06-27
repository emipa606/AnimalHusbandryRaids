using Mlie;
using UnityEngine;
using Verse;

namespace AnimalHusbandryRaids;

[StaticConstructorOnStartup]
internal class AnimalHusbandryRaidsMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static AnimalHusbandryRaidsMod Instance;

    private static string currentVersion;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public AnimalHusbandryRaidsMod(ModContentPack content) : base(content)
    {
        Instance = this;
        Settings = GetSettings<AnimalHusbandryRaidsSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal AnimalHusbandryRaidsSettings Settings { get; }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Animal Husbandry Raids";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        listingStandard.Gap();

        if (ModLister.IdeologyInstalled)
        {
            listingStandard.CheckboxLabeled("AHR.OnlyVeneratedAnimals".Translate(), ref Settings.OnlyVeneratedAnimals);
        }

        listingStandard.CheckboxLabeled("AHR.VerboseLogging".Translate(), ref Settings.VerboseLogging);
        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("AHR.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }
}