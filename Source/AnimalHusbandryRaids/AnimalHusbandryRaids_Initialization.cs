using System.Reflection;
using HarmonyLib;
using Verse;

namespace AnimalHusbandryRaids;

[StaticConstructorOnStartup]
internal class AnimalHusbandryRaids_Initialization
{
    static AnimalHusbandryRaids_Initialization()
    {
        new Harmony("mlie.PawnGroupMakerUtility_GeneratePawns").PatchAll(Assembly.GetExecutingAssembly());
    }
}