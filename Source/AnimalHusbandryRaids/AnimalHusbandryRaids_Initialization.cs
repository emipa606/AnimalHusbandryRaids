using System.Reflection;
using HarmonyLib;
using Verse;

namespace AnimalHusbandryRaids;

[StaticConstructorOnStartup]
internal class AnimalHusbandryRaids_Initialization
{
    static AnimalHusbandryRaids_Initialization()
    {
        var harmony = new Harmony("mlie.PawnGroupMakerUtility_GeneratePawns");
        var assembly = Assembly.GetExecutingAssembly();
        harmony.PatchAll(assembly);
    }
}