using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse;
using RimWorld;

namespace ASA_Yautja_Xenotype
{
    public class JobDriver_MarkKillerWithTrophy : JobDriver
    {
        public Pawn Killer => job.targetB.Thing as Pawn;
        public Corpse Corpse => job.targetA.Thing as Corpse;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed)
            && pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(30);//.WithProgressBarToilDelay(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_General.Wait(30);//.WithProgressBarToilDelay(TargetIndex.B);

            yield return new Toil
            {
                initAction = () =>
                {
                    var killer = TargetB.Thing as Pawn;
                    var corpse = TargetA.Thing as Corpse;
                    var thoughtPawn = this.pawn;

                    if (killer == null && corpse == null) return;

                    string corpseRace = corpse.InnerPawn.def.defName;

                    Hediff existing = killer.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed("MarkedByHunt"));
                    if (existing != null)
                        killer.health.RemoveHediff(existing);

                    BodyPartRecord headPart = killer.RaceProps.body.AllParts.FirstOrDefault(p => p.def.defName == "Head");
                    if (headPart != null)
                    {
                        var hediff = (Hediff_MarkedByHunt)HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MarkedByHunt"), killer, headPart);
                        hediff.markedTargetLabel = corpseRace;
                        killer.health.AddHediff(hediff, headPart);
                    }

                    if (pawn.needs?.mood?.thoughts?.memories != null)
                    {
                        Thought_Memory thoughtToRemove = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(DefDatabase<ThoughtDef>.GetNamed("ASA_MarkedKillPride"));
                        if (thoughtToRemove != null)
                            pawn.needs.mood.thoughts.memories.RemoveMemory(thoughtToRemove);
                    }

                    float moodBonus = KillPowerForPointsEvaluator.GetMoodBonusFromRace(corpseRace);
                    var thought = (Thought_MarkedKillDynamic)ThoughtMaker.MakeThought(DefDatabase<ThoughtDef>.GetNamed("ASA_MarkedKillPride"));
                    thought.moodPowerFactor = moodBonus;
                    killer.needs.mood?.thoughts?.memories.TryGainMemory(thought);
                    if (Prefs.DevMode)
                        Log.Message($"Mood bonus from killed {corpseRace}: {moodBonus}");

                    GameComponent_KillRegistry.Instance.lastMarkedKills[killer] = corpseRace;

                    if (corpse.TryGetComp<CompCorpseMarker>() is CompCorpseMarker comp && comp.killer != null)
                        comp.killer = null;
                    
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
