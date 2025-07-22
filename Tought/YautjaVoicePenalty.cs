using RimWorld;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class ThoughtWorker_YautjaVoicePenalty : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            var yautjaVoiceGene = DefDatabase<GeneDef>.GetNamed("ASA_YautjaVoice");

            bool pHasGene = p.genes?.HasActiveGene(yautjaVoiceGene) == true;
            bool otherHasGene = other.genes?.HasActiveGene(yautjaVoiceGene) == true;

            if (pHasGene != otherHasGene)
            {
                return ThoughtState.ActiveDefault;
            }

            return ThoughtState.Inactive;
        }
    }
}
