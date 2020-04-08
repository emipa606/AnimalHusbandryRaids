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

        private const double empirePercent = 0.1;
        private const double tribalPercent = 0.3;
        private const double outlanderPercent = 0.2;
        private const double piratePercent = 0.2;

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
            return currentAnimals;
        }

        private static Pawn GetAnimal(string factionDefName)
        {
            Pawn GeneratedPawn = null;
            List<string> animalDefs;
            switch (factionDefName)
            {
                case "Empire":
                    animalDefs = new List<string>
                    {
                        "Bear_Grizzly",
                        "Elephant",
                        "AEXP_Lion",             // Vanilla Animals Expanded — Desert
                        "AEXP_GreatDane",        // Vanilla Animals Expanded — Cats and Dogs
                        "AEXP_BlackBear",        // Vanilla Animals Expanded — Boreal Forest
                        "AEXP_Gorilla",          // Vanilla Animals Expanded — Tropical Rainforest
                        "AEXP_Tiger",            // Vanilla Animals Expanded — Tropical Rainforest
                        "AEXP_IndianElephant"    // Vanilla Animals Expanded — Tropical Swamp
                    };
                    UpdateAnimalList("Empire", animalDefs);
                    break;
                case "OutlanderCivil":
                case "OutlanderRough":
                    animalDefs = new List<string>
                    {
                        "LabradorRetriever",
                        "Husky",
                        "AEXP_GermanShepherd"    // Vanilla Animals Expanded — Cats and Dogs
                    };
                    UpdateAnimalList("Outlanders", animalDefs);
                    break;
                case "TribeCivil":
                case "TribeRough":
                case "TribeSavage":
                    animalDefs = new List<string>
                    {
                        "WildBoar",
                        "Wolf_Timber",
                        "AEXP_Coyote",            // Vanilla Animals Expanded — Arid Shrubland
                        "AEXP_ArcticCoyote"       // Vanilla Animals Expanded — Boreal Forest
                    };
                    UpdateAnimalList("Tribal", animalDefs);
                    break;
                case "Pirate":
                    animalDefs = new List<string>
                    {
                        "Boomrat",
                        "Cougar",
                        "AEXP_Hyena",            // Vanilla Animals Expanded — Desert
                        "AEXP_Rottweiler",       // Vanilla Animals Expanded — Cats and Dogs
                        "AEXP_Jaguar"            // Vanilla Animals Expanded — Tropical Rainforest
                    };
                    UpdateAnimalList("Pirate", animalDefs);
                    break;
                default:
                    return null;
            }

            int tries = 0;
            while (GeneratedPawn == null && tries < maxTries)
            {
                try
                {
                    GeneratedPawn = PawnGenerator.GeneratePawn(PawnKindDef.Named(animalDefs.RandomElement()), FactionUtility.DefaultFactionFrom(FactionDef.Named(factionDefName)));
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
            if (currentFaction.def.defName != "Empire" &&
                currentFaction.def.defName != "OutlanderCivil" &&
                currentFaction.def.defName != "OutlanderRough" &&
                currentFaction.def.defName != "TribeCivil" &&
                currentFaction.def.defName != "TribeRough" &&
                currentFaction.def.defName != "TribeSavage" &&
                currentFaction.def.defName != "Pirate")
            {
                //if (Prefs.DevMode) Log.Message($"[AnimalHusbandryRaids] {currentFaction.def.defName} is not a known faction, ignoring.");
                return;
            }

            List<Pawn> resultingPawns = __result.ToList();
            int pawnsInRaid = resultingPawns.Count;
            int amountToAdd;
            switch (currentFaction.def.defName)
            {
                case "Empire":
                    amountToAdd = (int)Math.Floor(pawnsInRaid * empirePercent);
                    break;
                case "OutlanderCivil":
                case "OutlanderRough":
                    amountToAdd = (int)Math.Floor(pawnsInRaid * outlanderPercent);
                    break;
                case "TribeCivil":
                case "TribeRough":
                case "TribeSavage":
                    amountToAdd = (int)Math.Floor(pawnsInRaid * tribalPercent);
                    break;
                case "Pirate":
                    amountToAdd = (int)Math.Floor(pawnsInRaid * piratePercent);
                    break;
                default:
                    return;
            }
            if (amountToAdd == 0)
            {
                //if (Prefs.DevMode) Log.Message("[AnimalHusbandryRaids] Too few pawns to add animals to, ignoring.");
                return;
            }
            if (Prefs.DevMode) Log.Message($"[AnimalHusbandryRaids] Adding {amountToAdd} animals to raid from {currentFaction.def.defName}.");
            for (int i = 0; i < amountToAdd; i++)
            {
                Pawn foundPawn = GetAnimal(currentFaction.def.defName);
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