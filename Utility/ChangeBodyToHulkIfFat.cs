using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ASA_Yautja_Xenotype
{
    //Since we're using the vanila framework to draw the "fur" as body overlaid ontop of the normal body,
    //when the yautja is fat the overlay dont work well because we don't have a fat body and are using Hulk instead

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    public static class ChangeBodyTypeOnSpawn
    {
        public static void Postfix(Pawn __result)
        {
            var pawn = __result;
            if (pawn?.story?.bodyType == BodyTypeDefOf.Fat && pawn.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("ASA_Yautja")) == true)
            {
                pawn.story.bodyType = BodyTypeDefOf.Hulk;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_GeneTracker),"AddGene",new[] { typeof(GeneDef), typeof(bool)})]
    public static class Patch_GeneTracker_AddGene
    {
        [HarmonyPostfix]
        public static void Postfix(Gene __result, GeneDef geneDef, bool xenogene, Pawn_GeneTracker __instance)
        {
            var pawn = __result;
            if (__result.pawn?.story?.bodyType == BodyTypeDefOf.Fat && __result.pawn.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("ASA_Yautja")) == true)
            {
                __result.pawn.story.bodyType = BodyTypeDefOf.Hulk;
            }
        }
    }
}




