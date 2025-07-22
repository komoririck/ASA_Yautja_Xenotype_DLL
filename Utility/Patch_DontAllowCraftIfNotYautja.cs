using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;

namespace ASA_Yautja_Xenotype
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
    public static class Patch_DontAllowCraftIfNotYautja
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Thing thing, bool forced, ref Job __result)
        {
            if (__result == null) return;

            var billProd = __result.bill as Bill_Production;
            if (billProd?.recipe == null) return;

            bool restricted = billProd.recipe.products.Any(p =>
                p.thingDef?.tradeTags?.Contains("ASA_Yautja") == true);
            bool hasGene = pawn.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("ASA_YautjaVoice")) == true;

            if (restricted && !hasGene)
            {
                __result = null;
            }
        }
    }
}
