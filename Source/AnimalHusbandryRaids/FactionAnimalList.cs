using System.Collections.Generic;
using Verse;

namespace AnimalHusbandryRaids;

internal class FactionAnimalList : DefModExtension
{
    public readonly List<string> FactionAnimals = new List<string>();
    public double AnimalCommonality = 1;
    public string FactionType = "Unassigned FactionType";
    public double PawnPercentage = 0.1;
}