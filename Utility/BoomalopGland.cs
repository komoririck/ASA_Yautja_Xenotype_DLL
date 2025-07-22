using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class Patch_SurgeryFailureBoomalop
    {
        public class Recipe_RemoveBoomalopCombustionGland : Recipe_RemoveImplant
        {
            public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
            {
                float surgerySuccessChance = billDoer.GetStatValue(StatDefOf.MedicalSurgerySuccessChance);
                float finalChance = surgerySuccessChance * this.recipe.surgerySuccessChanceFactor;
                bool surgerySucceeded = Rand.Chance(Mathf.Clamp01(finalChance));

                if (!surgerySucceeded)
                {
                    Find.LetterStack.ReceiveLetter("Boomalop glands mishandling!", 
                    $"The surgery to remove {pawn.LabelShort}'s gland failed. {billDoer.NameShortColored} made a mistake handling boomalop glands triggering an explosion!",
                    LetterDefOf.NegativeEvent,
                    pawn
                    );
                    explode(pawn);
                } else
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("BoomalopCombustionGland"));
                    GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }

                Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("BoomalopCombustionGland"));
                if (existing != null)
                    pawn.health.RemoveHediff(existing);
            }
        }

        public class Recipe_InstallBoomalopCombustionGland : Recipe_InstallImplant
        {
            public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
            {
                if (billDoer != null)
                {
                    float surgerySuccessChance = billDoer.GetStatValue(StatDefOf.MedicalSurgerySuccessChance);
                    float finalChance = surgerySuccessChance * this.recipe.surgerySuccessChanceFactor;
                    bool surgerySucceeded = Rand.Chance(Mathf.Clamp01(finalChance));

                    if (!surgerySucceeded)
                    {
                        Find.LetterStack.ReceiveLetter("Boomalop glands mishandling!",
                            $"The surgery to install {pawn.LabelShort}'s gland failed. {billDoer.NameShortColored} made a mistake handling boomalop glands triggering an explosion!",
                            LetterDefOf.NegativeEvent,
                            pawn
                        );
                        explode(pawn);
                    }
                }

                base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
            }

        }

        public class Hediff_BoomalopCombustionGland : Hediff_Implant
        {
            public override void Notify_PawnDied(DamageInfo? dinfo, Hediff cause)
            {
                base.Notify_PawnDied(dinfo, cause);

                if (pawn?.MapHeld != null)
                {
                    Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("BoomalopCombustionGland"));
                    if (existing != null)
                        pawn.health.RemoveHediff(existing);

                    explode(pawn);

                }
            }
        }
        static void explode(Pawn pawn)
        {
            GenExplosion.DoExplosion(
                center: pawn.PositionHeld,
                map: pawn.MapHeld,
                radius: 3f,
                damType: DamageDefOf.Flame,
                instigator: pawn,
                damAmount: 25,
                armorPenetration: 0.4f,
                explosionSound: SoundDefOf.Artillery_ShellLoaded
            );
        }
    }
}
