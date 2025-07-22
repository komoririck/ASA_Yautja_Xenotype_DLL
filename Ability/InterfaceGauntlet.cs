using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class HediffCompProperties_WarnIfInvisibleNearby : HediffCompProperties
    {
        public float radius = 30f;
        public List<string> invisibilityHediff = new List<string>();

        public HediffCompProperties_WarnIfInvisibleNearby()
        {
            compClass = typeof(HediffComp_WarnIfInvisibleNearby);
        }
    }

    public class HediffComp_WarnIfInvisibleNearby : HediffComp
    {
        public HediffCompProperties_WarnIfInvisibleNearby Props => (HediffCompProperties_WarnIfInvisibleNearby)props;
        private int nextCheckTick = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (parent.pawn.IsHashIntervalTick(60))
            {
                if (Find.TickManager.TicksGame >= nextCheckTick)
                {
                    if (CheckNearbyInvisible())
                    {
                        MoteMaker.ThrowText(parent.pawn.DrawPos, parent.pawn.Map, "Invisible Creature Detected", Color.yellow);
                        nextCheckTick = Find.TickManager.TicksGame + 600;
                    }
                }
            }
        }

        private bool CheckNearbyInvisible()
        {
            var map = parent.pawn.Map;
            if (map == null) return false;

            return map.mapPawns.AllPawnsSpawned.Any(other =>
                other != parent.pawn &&
                other.Spawned &&
                other.Position.InHorDistOf(parent.pawn.Position, Props.radius) &&
                other.health?.hediffSet?.hediffs?.Any(h => Props.invisibilityHediff.Contains(h.def.defName)) == true);

        }
    }
}
