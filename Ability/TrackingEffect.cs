using RimWorld;
using UnityEngine;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class HediffComp_TrackingEffect : HediffComp
    {
        public Pawn ASA_caster = null;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.Map == null || Pawn.Dead) return;

            if (ASA_caster.Map == null || ASA_caster.Dead) return;

            MoteAttached mote = ThingMaker.MakeThing(ThingDef.Named("ASA_Mote_TrackingMark")) as MoteAttached;
            if (mote == null)
            {
                Log.Error("ASA_Mote_TrackingMark is not a MoteAttached!");
                return;
            }

            mote.Scale = 0.7f;
            mote.Attach(Pawn); 
            GenSpawn.Spawn(mote, Pawn.Position, Pawn.Map, WipeMode.Vanish);

        }
    }

    public class HediffCompProperties_TrackingEffect : HediffCompProperties
    {
        public HediffCompProperties_TrackingEffect()
        {
            this.compClass = typeof(HediffComp_TrackingEffect);
        }
    }
}


