using RimWorld;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class ThoughtExtension_YautjaUgly : DefModExtension
    {
        public GeneDef UglyToWho;
        public GeneDef UglyMotive;
    }
    public class ThoughtWorker_YautjaUgly : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            var ext = def.GetModExtension<ThoughtExtension_YautjaUgly>();
            if (ext == null)
                return ThoughtState.Inactive;

            bool pawnHasMotive = pawn.genes?.HasActiveGene(ext.UglyMotive) == true;
            bool otherHasMotive = other.genes?.HasActiveGene(ext.UglyMotive) == true;

            bool pawnHasUglyToWho = pawn.genes?.HasActiveGene(ext.UglyToWho) == true;
            bool otherHasUglyToWho = other.genes?.HasActiveGene(ext.UglyToWho) == true;

            if ((pawnHasMotive && !otherHasUglyToWho) || (otherHasMotive && !pawnHasUglyToWho))
            {
                return ThoughtState.ActiveAtStage(0);
            }

            return ThoughtState.Inactive;
        }

    }
}
