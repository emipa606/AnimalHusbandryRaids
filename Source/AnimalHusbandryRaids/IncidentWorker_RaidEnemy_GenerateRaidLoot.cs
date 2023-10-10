using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalHusbandryRaids;

[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GenerateRaidLoot", typeof(IncidentParms), typeof(float),
    typeof(List<Pawn>))]
public static class IncidentWorker_RaidEnemy_GenerateRaidLoot
{
    private static void Prefix(ref List<Pawn> pawns)
    {
        if (pawns == null || !pawns.Any())
        {
            return;
        }

        pawns = pawns.Where(pawn => !pawn.RaceProps.Animal).ToList();
    }
}