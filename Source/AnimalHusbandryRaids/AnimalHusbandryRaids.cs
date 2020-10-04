using RimWorld;
using Verse;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

namespace AnimalHusbandryRaids
{
    [StaticConstructorOnStartup]
    class AnimalHusbandryRaids_Initialization
    {
        static AnimalHusbandryRaids_Initialization()
        {
            var harmony = new Harmony("mlie.AnimalHusbandryRaids");
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
        }
    }

    [HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns", new Type[] { typeof(PawnGroupMakerParms), typeof(bool) })]
    public static class AnimalHusbandryRaids
    {
        private const int maxTries = 100;

        private static List<string> UpdateAnimalList(string factionDefName, List<string> currentAnimals)
        {
            string additions = $@"{GenFilePaths.ConfigFolderPath}{Path.DirectorySeparatorChar}AnimalHusbandryRaids_{factionDefName}.additions";
            string deletions = $@"{GenFilePaths.ConfigFolderPath}{Path.DirectorySeparatorChar}AnimalHusbandryRaids_{factionDefName}.deletions";
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
                foreach (string animalToRemove in animalsToRemove)
                {
                    currentAnimals.Remove(animalToRemove);
                }
                //if (Prefs.DevMode) Log.Message($"[AnimalHusbandryRaids] The following animals was found: {currentAnimals.ToCommaList()}, have added {animalsToAdd.ToCommaList()} and removed {animalsToRemove.ToCommaList()}");
            }
            catch (Exception exception)
            {
                if (Prefs.DevMode) Log.Message($"[AnimalHusbandryRaids] Failed to create files, {exception.ToString()}.");
            }

            var returnValue = new List<string>();
            foreach (string animalDef in currentAnimals)
            {
                if(GenDefDatabase.GetDefSilentFail(typeof(ThingDef), animalDef) != null)
                {
                    returnValue.Add(animalDef);
                }
            }
            return returnValue;
        }

        private static Pawn GetAnimal(Faction faction)
        {
            Pawn GeneratedPawn = null;
            FactionAnimalList modExtension = faction.def.GetModExtension<FactionAnimalList>();
            List<string> animalDefs = modExtension.FactionAnimals;
            animalDefs = UpdateAnimalList(modExtension.FactionType, animalDefs);

            int tries = 0;
            while (GeneratedPawn == null && tries < maxTries)
            {
                try
                {                    
                    GeneratedPawn = PawnGenerator.GeneratePawn(PawnKindDef.Named(animalDefs.RandomElement()), faction);                    
                }
                catch
                {
                    tries++;
                }
            }
            return GeneratedPawn;
        }

        static void Postfix(ref IEnumerable<Pawn> __result, PawnGroupMakerParms parms, bool warnOnZeroResults = true)
        {
            Faction currentFaction = parms.faction;
            if (!currentFaction.def.HasModExtension<FactionAnimalList>())
            {
                //if (Prefs.DevMode) Log.Message($"[AnimalHusbandryRaids] {currentFaction.def.defName} does not have a FactionAnimalList assigned, ignoring.");
                return;
            }
            FactionAnimalList modExtension = currentFaction.def.GetModExtension<FactionAnimalList>();
            if (modExtension.FactionType == "Unassigned FactionType")
            {
                Log.Warning($"[AnimalHusbandryRaids] {currentFaction.def.defName} does not have a FactionType assigned in its FactionAnimalList, ignoring.");
                return;
            }

            List<Pawn> resultingPawns = __result.ToList();
            int pawnsInRaid = resultingPawns.Count;
            int amountToAdd = (int)Math.Floor(pawnsInRaid * modExtension.PawnPercentage);
            if (amountToAdd == 0)
            {
                //if (Prefs.DevMode) Log.Message("[AnimalHusbandryRaids] Too few pawns to add animals to, ignoring.");
                return;
            }
            if (Prefs.DevMode) Log.Message($"[AnimalHusbandryRaids] Adding {amountToAdd} animals to raid from {currentFaction.Name}, {currentFaction.def.defName}.");
            for (int i = 0; i < amountToAdd; i++)
            {
                Pawn foundPawn = GetAnimal(currentFaction);
                if (foundPawn == null)
                {
                    if (Prefs.DevMode) Log.Message($"[AnimalHusbandryRaids] Failed to find animal after {maxTries} tries, generated {i} animals.");
                    return;
                }
                resultingPawns.Add(foundPawn);
            }
            __result = resultingPawns;
        }

    }
}