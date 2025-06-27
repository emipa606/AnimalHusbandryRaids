using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalHusbandryRaids;

[HarmonyPatch(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.GeneratePawns), typeof(PawnGroupMakerParms),
    typeof(bool))]
public static class PawnGroupMakerUtility_GeneratePawns
{
    private const int MaxTries = 100;

    private static readonly Dictionary<string, HashSet<PawnKindDef>> factionAnimalDefs = [];

    private static HashSet<string> updateAnimalList(string factionDefName, HashSet<string> currentAnimals)
    {
        var additions =
            $"{GenFilePaths.ConfigFolderPath}{Path.DirectorySeparatorChar}AnimalHusbandryRaids_{factionDefName}.additions";
        var deletions =
            $"{GenFilePaths.ConfigFolderPath}{Path.DirectorySeparatorChar}AnimalHusbandryRaids_{factionDefName}.deletions";
        try
        {
            if (!File.Exists(additions))
            {
                File.Create(additions).Dispose();
            }

            if (!File.Exists(deletions))
            {
                File.Create(deletions).Dispose();
            }

            var animalsToAdd = File.ReadAllLines(additions).ToList();
            currentAnimals.AddRange(animalsToAdd);
            var animalsToRemove = File.ReadAllLines(deletions).ToList();
            foreach (var animalToRemove in animalsToRemove)
            {
                currentAnimals.Remove(animalToRemove);
            }

            writeDebug(
                $"The following animals was found: {currentAnimals.ToCommaList()}, have added {animalsToAdd.ToCommaList()} and removed {animalsToRemove.ToCommaList()}");
        }
        catch (Exception exception)
        {
            writeDebug($"Failed to create files, {exception}.");
        }

        var returnValue = new HashSet<string>();
        foreach (var animalDef in currentAnimals)
        {
            if (GenDefDatabase.GetDefSilentFail(typeof(ThingDef), animalDef) == null)
            {
                continue;
            }

            writeDebug($"Adding, {animalDef} as possible animal.");

            returnValue.Add(animalDef);
        }

        return returnValue;
    }

    private static Pawn getAnimal(Faction faction, HashSet<PawnKindDef> animalDefs)
    {
        Pawn generatedPawn = null;

        var tries = 0;
        while (generatedPawn == null && tries < MaxTries)
        {
            try
            {
                generatedPawn = PawnGenerator.GeneratePawn(animalDefs.RandomElement(), faction);
                generatedPawn.inventory.DestroyAll();
                generatedPawn.mindState.canFleeIndividual = true;
            }
            catch
            {
                tries++;
            }
        }

        return generatedPawn;
    }

    private static void Postfix(ref IEnumerable<Pawn> __result, PawnGroupMakerParms parms)
    {
        if (parms.faction == null || parms.raidStrategy == null || parms.raidStrategy.ToString() == "Siege")
        {
            return;
        }

        var currentFaction = parms.faction;
        var resultingPawns = __result.ToList();
        var pawnsInRaid = resultingPawns.Count;
        HashSet<PawnKindDef> animalPawnKinds = [];
        var amountToAdd = (int)Math.Floor(pawnsInRaid * 0.2f);

        if (factionAnimalDefs.TryGetValue(currentFaction.Name, out var def))
        {
            writeDebug($"{currentFaction.def.defName} already have an animal list cached.");
            animalPawnKinds = def;
        }

        if (AnimalHusbandryRaidsMod.Instance.Settings.OnlyVeneratedAnimals)
        {
            if (!animalPawnKinds.Any())
            {
                var animals = currentFaction.ideos?.PrimaryIdeo?.VeneratedAnimals;

                if (animals?.Any() == true)
                {
                    foreach (var thingDef in animals)
                    {
                        var pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(thingDef.defName);
                        if (pawnKind != null)
                        {
                            animalPawnKinds.Add(pawnKind);
                        }
                    }
                }

                factionAnimalDefs[currentFaction.Name] = animalPawnKinds;
            }
        }
        else
        {
            if (!currentFaction.def.HasModExtension<FactionAnimalList>())
            {
                writeDebug($"{currentFaction.def.defName} does not have a FactionAnimalList assigned, ignoring.");
                return;
            }

            var modExtension = currentFaction.def.GetModExtension<FactionAnimalList>();
            if (modExtension.FactionType == "Unassigned FactionType")
            {
                Log.Warning(
                    $"[PawnGroupMakerUtility_GeneratePawns] {currentFaction.def.defName} does not have a FactionType assigned in its FactionAnimalList, ignoring.");
                return;
            }

            if (modExtension.FactionAnimals == null)
            {
                Log.Warning(
                    $"[PawnGroupMakerUtility_GeneratePawns] {currentFaction.def.defName} does not have a FactionAnimalList, ignoring.");
                return;
            }

            if (modExtension.AnimalCommonality < 1)
            {
                var randomValue = Rand.Value;
                if (modExtension.AnimalCommonality < randomValue)
                {
                    writeDebug(
                        $"{modExtension.AnimalCommonality} commonality was not enough to spawn animals this time, random value was {randomValue}.");
                    return;
                }

                writeDebug(
                    $"{modExtension.AnimalCommonality} commonality was enough to spawn animals this time, random value was {randomValue}.");
            }

            amountToAdd = (int)Math.Floor(pawnsInRaid * modExtension.PawnPercentage);
            if (amountToAdd == 0)
            {
                writeDebug("Too few pawns to add animals to, ignoring.");
                return;
            }

            writeDebug(
                $"Adding {amountToAdd} animals to raid from {currentFaction.Name}, {currentFaction.def.defName}.");

            if (!animalPawnKinds.Any())
            {
                var animalDefs = new HashSet<string>();
                foreach (var animalDef in modExtension.FactionAnimals)
                {
                    animalDefs.Add(animalDef);
                }

                animalDefs = updateAnimalList(modExtension.FactionType, animalDefs);
                foreach (var animalDef in animalDefs)
                {
                    var pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(animalDef);
                    if (pawnKind != null)
                    {
                        animalPawnKinds.Add(pawnKind);
                    }
                }

                factionAnimalDefs[currentFaction.Name] = animalPawnKinds;
            }
        }

        for (var i = 0; i < amountToAdd; i++)
        {
            var foundPawn = getAnimal(currentFaction, animalPawnKinds);
            if (foundPawn == null)
            {
                writeDebug($"Failed to find animal after {MaxTries} tries, generated {i} animals.");
                return;
            }

            resultingPawns.Add(foundPawn);
        }

        __result = resultingPawns;
    }

    private static void writeDebug(string message)
    {
        if (!AnimalHusbandryRaidsMod.Instance.Settings.VerboseLogging)
        {
            return;
        }

        Log.Message($"[PawnGroupMakerUtility_GeneratePawns] {message}");
    }
}