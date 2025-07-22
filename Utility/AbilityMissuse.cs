using RimWorld;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class CompProperties_AbilityMissuse : CompProperties_AbilityEffect
    {
        public float failChance;
        public GeneDef requiresGene;

        public CompProperties_AbilityMissuse()
        {
            this.compClass = typeof(Comp_AbilityMissuse);
        }
    }

    public class Comp_AbilityMissuse : CompAbilityEffect
    {
        public CompProperties_AbilityMissuse Props => (CompProperties_AbilityMissuse)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            bool explode = false;

            if (Rand.Chance(Props.failChance))
            {
                explode = true;
            }

            Pawn pawn = this.parent.pawn;
            if (Props.requiresGene != null && pawn?.genes?.HasActiveGene(Props.requiresGene) == true)
            {
                explode = false;
            }

            if (explode)
            {
                Find.LetterStack.ReceiveLetter(
                    "Yautja equipment mishandling!",
                    $"{pawn.NameShortColored} made a mistake handling his equipment, causing a malfunction!",
                    LetterDefOf.NegativeEvent,
                    pawn
                );

                GenExplosion.DoExplosion(
                    pawn.Position,
                    pawn.Map,
                    2f,
                    DamageDefOf.Burn,
                    pawn,
                    5,
                    armorPenetration: 0.25f,
                    explosionSound: SoundDefOf.Power_OnSmall
                );
            }
            else
            {
                base.Apply(target, dest);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest);
        }
    }
}
