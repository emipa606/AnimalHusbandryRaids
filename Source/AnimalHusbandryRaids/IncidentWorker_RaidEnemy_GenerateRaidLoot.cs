using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalHusbandryRaids
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GenerateRaidLoot", typeof(IncidentParms), typeof(float),
        typeof(List<Pawn>))]
    public static class IncidentWorker_RaidEnemy_GenerateRaidLoot
    {
        private static void Prefix(ref List<Pawn> pawns)
        {
            var unused = pawns.Count;
            pawns = pawns.Where(pawn => !pawn.RaceProps.Animal).ToList();
            // Log.Message($"[AnimalHusbandyRaids]: Removed {raiders - pawns.Count} animals from the loot-generator");
        }
    }
}