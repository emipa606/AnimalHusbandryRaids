using System.Collections.Generic;
using Verse;

namespace AnimalHusbandryRaids;

internal class FactionAnimalList : DefModExtension
{
    public readonly double AnimalCommonality = 1;
    public readonly List<string> FactionAnimals = new List<string>();
    public readonly string FactionType = "Unassigned FactionType";
    public readonly double PawnPercentage = 0.1;
}