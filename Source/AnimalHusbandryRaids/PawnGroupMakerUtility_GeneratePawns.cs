﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalHusbandryRaids;

[HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns", typeof(PawnGroupMakerParms), typeof(bool))]
public static class PawnGroupMakerUtility_GeneratePawns
{
    private const int maxTries = 100;

    private static HashSet<string> UpdateAnimalList(string factionDefName, HashSet<string> currentAnimals)
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

            WriteDebug(
                $"The following animals was found: {currentAnimals.ToCommaList()}, have added {animalsToAdd.ToCommaList()} and removed {animalsToRemove.ToCommaList()}");
        }
        catch (Exception exception)
        {
            WriteDebug($"Failed to create files, {exception}.");
        }

        var returnValue = new HashSet<string>();
        foreach (var animalDef in currentAnimals)
        {
            if (GenDefDatabase.GetDefSilentFail(typeof(ThingDef), animalDef) == null)
            {
                continue;
            }

            WriteDebug($"Adding, {animalDef} as possible animal.");

            returnValue.Add(animalDef);
        }

        return returnValue;
    }

    private static Pawn GetAnimal(Faction faction, HashSet<string> animalDefs)
    {
        Pawn GeneratedPawn = null;

        var tries = 0;
        while (GeneratedPawn == null && tries < maxTries)
        {
            try
            {
                GeneratedPawn = PawnGenerator.GeneratePawn(PawnKindDef.Named(animalDefs.RandomElement()), faction);
                GeneratedPawn.inventory.DestroyAll();
                GeneratedPawn.mindState.canFleeIndividual = true;
            }
            catch
            {
                tries++;
            }
        }

        return GeneratedPawn;
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

        int amountToAdd;
        HashSet<string> animalDefs;
        if (AnimalHusbandryRaidsMod.instance.Settings.OnlyVeneratedAnimals)
        {
            var animals = currentFaction.ideos?.PrimaryIdeo?.VeneratedAnimals;

            if (animals?.Any() == true)
            {
                amountToAdd = (int)Math.Floor(pawnsInRaid * 0.2f);
                if (amountToAdd > 0)
                {
                    WriteDebug(
                        $"Adding {amountToAdd} venerated animals to raid from {currentFaction.Name}, {currentFaction.def.defName}.");
                    animalDefs = [];
                    foreach (var animalDef in animals)
                    {
                        animalDefs.Add(animalDef.defName);
                    }

                    WriteDebug($"The following venerated animals was found: {animalDefs.ToCommaList()}");
                    for (var i = 0; i < amountToAdd; i++)
                    {
                        var foundPawn = GetAnimal(currentFaction, animalDefs);
                        if (foundPawn == null)
                        {
                            WriteDebug($"Failed to find animal after {maxTries} tries, generated {i} animals.");
                            return;
                        }

                        resultingPawns.Add(foundPawn);
                    }

                    __result = resultingPawns;
                    return;
                }
            }
        }

        if (!currentFaction.def.HasModExtension<FactionAnimalList>())
        {
            WriteDebug($"{currentFaction.def.defName} does not have a FactionAnimalList assigned, ignoring.");
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
                WriteDebug(
                    $"{modExtension.AnimalCommonality} commonality was not enough to spawn animals this time, random value was {randomValue}.");
                return;
            }

            WriteDebug(
                $"{modExtension.AnimalCommonality} commonality was enough to spawn animals this time, random value was {randomValue}.");
        }

        amountToAdd = (int)Math.Floor(pawnsInRaid * modExtension.PawnPercentage);
        if (amountToAdd == 0)
        {
            WriteDebug("Too few pawns to add animals to, ignoring.");
            return;
        }

        WriteDebug(
            $"Adding {amountToAdd} animals to raid from {currentFaction.Name}, {currentFaction.def.defName}.");

        animalDefs = [];
        foreach (var animalDef in modExtension.FactionAnimals)
        {
            animalDefs.Add(animalDef);
        }

        animalDefs = UpdateAnimalList(modExtension.FactionType, animalDefs);
        for (var i = 0; i < amountToAdd; i++)
        {
            var foundPawn = GetAnimal(currentFaction, animalDefs);
            if (foundPawn == null)
            {
                WriteDebug($"Failed to find animal after {maxTries} tries, generated {i} animals.");
                return;
            }

            resultingPawns.Add(foundPawn);
        }

        __result = resultingPawns;
    }

    private static void WriteDebug(string message)
    {
        if (!AnimalHusbandryRaidsMod.instance.Settings.VerboseLogging)
        {
            return;
        }

        Log.Message($"[PawnGroupMakerUtility_GeneratePawns] {message}");
    }
}