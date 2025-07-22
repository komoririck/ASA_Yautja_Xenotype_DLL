using HarmonyLib;
using System.Linq;
using Verse;

namespace ASA_Yautja_Xenotype
{
    [HarmonyPatch(typeof(ShotReport), nameof(ShotReport.HitReportFor))]
    public static class Patch_HitReportFor_Prefix
    {
        static void Prefix(ref Thing caster, ref Verb verb, ref LocalTargetInfo target)
        {
            var shooter = caster as Pawn;
            var targetPawn = target.Thing as Pawn;

            if (shooter == null || targetPawn == null) return;

            if (!targetPawn.NonHumanlikeOrWildMan()) 
            {
                if (shooter.Faction.Name.Equals(targetPawn.Faction.Name))
                    return;
            }

            if (CheckIfHasHediffToBeTracked(shooter, targetPawn))
            {
                shooter.health.AddHediff(HediffDef.Named("ASA_Hediff_TrackingBoost"));
            }
        }

        static void Postfix(Thing caster)
        {
            var shooter = caster as Pawn;
            if (shooter == null) return;
            var hediffsToRemove = shooter.health.hediffSet.hediffs.Where(h => h.def.defName == "ASA_Hediff_TrackingBoost").ToList();

            foreach (var hediff in hediffsToRemove)
                shooter.health.RemoveHediff(hediff);
        }
        public static bool CheckIfHasHediffToBeTracked(Pawn shooter, Pawn targetPawn)
        {
            if (shooter == null || targetPawn == null) return false;

            var hediff = targetPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("ASA_Hediff_MaskTracking"));
            if (hediff == null) return false;

            var comp = hediff.TryGetComp<HediffComp_TrackingEffect>();
            if (comp == null || comp.ASA_caster != shooter) return false;

            return true;
        }
    }
}
