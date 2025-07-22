using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class CompProperties_TrackingCone : CompProperties_AbilityEffect
    {
        public float range = 7f;
        public float lineWidthEnd = 4f;
        public HediffDef hediffToApply;
        public float severity = 1f;
        public EffecterDef flashEffecter;
        public int castDelayTicks = 15;

        public CompProperties_TrackingCone()
        {
            this.compClass = typeof(CompAbilityEffect_TrackingCone);
        }
    }

    public class CompAbilityEffect_TrackingCone : CompAbilityEffect
    {
        private readonly List<IntVec3> tmpCells = new List<IntVec3>();

        public CompProperties_TrackingCone Props => (CompProperties_TrackingCone)props;
        public Pawn Pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Map map = Pawn.Map;

            foreach (IntVec3 cell in AffectedCells(target))
            {
                foreach (Thing t in cell.GetThingList(map))
                {
                    if (t is Pawn targetPawn && targetPawn != Pawn)
                    {
                        if (!targetPawn.health.hediffSet.HasHediff(Props.hediffToApply))
                        {
                            Hediff hediff = HediffMaker.MakeHediff(Props.hediffToApply, targetPawn);
                            hediff.Severity = Props.severity;
                            targetPawn.health.AddHediff(hediff);
                            hediff.TryGetComp<HediffComp_TrackingEffect>().ASA_caster = Pawn;
                        }
                    }
                }
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(AffectedCells(target));
        }

        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            tmpCells.Clear();
            Vector3 source = Pawn.Position.ToVector3Shifted();
            IntVec3 dest = target.Cell.ClampInsideMap(Pawn.Map);

            if (Pawn.Position == dest)
                return tmpCells;

            float len = (dest - Pawn.Position).LengthHorizontal;
            float dx = (dest.x - Pawn.Position.x) / len;
            float dz = (dest.z - Pawn.Position.z) / len;

            dest.x = Mathf.RoundToInt(Pawn.Position.x + dx * Props.range);
            dest.z = Mathf.RoundToInt(Pawn.Position.z + dz * Props.range);

            float angleCenter = Vector3.SignedAngle(dest.ToVector3Shifted() - source, Vector3.right, Vector3.up);
            float halfWidth = Props.lineWidthEnd / 2f;
            float maxDist = Mathf.Sqrt(Props.range * Props.range + halfWidth * halfWidth);
            float angleSpread = Mathf.Rad2Deg * Mathf.Asin(halfWidth / maxDist);

            int count = GenRadial.NumCellsInRadius(Props.range);
            for (int i = 0; i < count; i++)
            {
                IntVec3 c = Pawn.Position + GenRadial.RadialPattern[i];
                if (!c.InBounds(Pawn.Map) || c == Pawn.Position) continue;

                float angle = Vector3.SignedAngle(c.ToVector3Shifted() - source, Vector3.right, Vector3.up);
                if (Mathf.Abs(Mathf.DeltaAngle(angle, angleCenter)) <= angleSpread)
                    tmpCells.Add(c);
            }

            return tmpCells;
        }
    }

}
