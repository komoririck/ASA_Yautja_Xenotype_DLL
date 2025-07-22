using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class CompProperties_AddHediffOnEquip : CompProperties
    {
        public HediffDef hediffDef;

        public CompProperties_AddHediffOnEquip()
        {
            this.compClass = typeof(CompAddHediffOnEquip);
        }
    }
    public class CompAddHediffOnEquip : ThingComp
    {
        public CompProperties_AddHediffOnEquip Props => (CompProperties_AddHediffOnEquip)this.props;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (Props.hediffDef != null && pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef) == null)
            {
                pawn.health.AddHediff(Props.hediffDef);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }
}
