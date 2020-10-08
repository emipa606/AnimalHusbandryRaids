using System;
using System.Collections.Generic;
using Verse;

namespace AnimalHusbandryRaids
{
    class FactionAnimalList : DefModExtension
    {
        public string FactionType = "Unassigned FactionType";
        public double PawnPercentage = 0.1;
        public List<String> FactionAnimals = new List<string>();
    }
}
