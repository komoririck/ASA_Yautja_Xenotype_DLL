using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ASA_Yautja_Xenotype
{
    [HarmonyPatch(typeof(JobDriver_Insult), "MakeNewToils")]
    public static class Patch_JobDriver_Insult_Prefix
    {
        [HarmonyPostfix]
        public static void Postfix(JobDriver_Insult __instance)
        {
            var ___pawn = __instance.pawn;
            var recipient = (Pawn)(Thing)___pawn.CurJob.GetTarget(TargetIndex.A);

            if (___pawn.IsAnimal || recipient.IsAnimal)
                return;

            if (recipient.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("ASA_YautjaMarkKiller")) == false)
                return;

            Hediff pawnIniti = ___pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("MarkedByHunt"));
            Hediff pawnRecip = recipient.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("MarkedByHunt"));

            if (pawnIniti == null && pawnRecip != null)
            {
                ReplaceThought.Action(recipient, DefDatabase<ThoughtDef>.GetNamed("ASA_SocialFightPrideWound"), DefDatabase<ThoughtDef>.GetNamed("ASA_InsultedByUnworthy"));
                recipient.interactions?.StartSocialFight(___pawn);
            }
        }
    }

    [HarmonyPatch(typeof(InteractionWorker), nameof(InteractionWorker.Interacted))]
    public static class Patch_Interacted
    {
        [HarmonyPostfix]
        public static void Postfix(InteractionWorker __instance, Pawn initiator, Pawn recipient)
        {
            var ___pawn = initiator;

            if (___pawn.IsAnimal || recipient.IsAnimal)
                return;

            if (recipient.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("ASA_YautjaMarkKiller")) == false)
                return;

            Hediff pawnIniti = ___pawn.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamed("MarkedByHunt"));

            Hediff pawnRecip = recipient.health.hediffSet.GetFirstHediffOfDef(
                DefDatabase<HediffDef>.GetNamed("MarkedByHunt"));

            if (pawnIniti == null && pawnRecip != null)
            {
                if (Rand.Chance(0.15f)) {
                    ReplaceThought.Action(recipient, DefDatabase<ThoughtDef>.GetNamed("ASA_InsultedByUnworthy"), DefDatabase<ThoughtDef>.GetNamed("ASA_SocialFightPrideWound"));
                    recipient.interactions?.StartSocialFight(initiator);
                }
            }
        }
    }
    public class ReplaceThought
    { 
         public static void Action(Pawn pawn, ThoughtDef toRemove, ThoughtDef toAdd)
        {
            pawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDef(toRemove);
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(toAdd);
        }
    }
}