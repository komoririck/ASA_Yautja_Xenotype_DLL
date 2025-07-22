using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class CompAgeWatcher : ThingComp
    {
        public override void CompTickRare()
        {
            base.CompTickRare();

            if (parent is Pawn pawn)
            {
                int age = pawn.ageTracker.AgeBiologicalYears;

                if (age >= 2400 && age < 2800)
                {
                    if (Rand.MTBEventOccurs(5f, 60000f, 250f))
                    {
                        GiveAgeRelatedDisease(pawn);
                    }
                }
                else if (age >= 2800)
                {
                    if (Rand.MTBEventOccurs(1f, 60000f, 250f))
                    {
                        GiveAgeRelatedDisease(pawn);
                    }
                }
            }
        }
        private void GiveAgeRelatedDisease(Pawn pawn)
        {
            var ageRelatedDiseases = new List<HediffDef>
            {
                HediffDef.Named("HeartArteryBlockage"),
                HediffDef.Named("Dementia"),
                HediffDef.Named("Stroke"),
                HediffDef.Named("Arthritis"),
                HediffDef.Named("Cataract")
            };

            var diseaseDef = ageRelatedDiseases.RandomElement();

            pawn.health.AddHediff(diseaseDef);

            Messages.Message($"{pawn.LabelShort} has developed {diseaseDef.label}!", pawn, MessageTypeDefOf.NegativeEvent);
        }
    }
}
