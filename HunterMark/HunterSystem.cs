using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace ASA_Yautja_Xenotype
{
    public class GameComponent_KillRegistry : GameComponent
    {
        public Dictionary<Pawn, string> lastMarkedKills = new Dictionary<Pawn, string>();

        public static GameComponent_KillRegistry Instance => Current.Game.GetComponent<GameComponent_KillRegistry>();

        public GameComponent_KillRegistry(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref lastMarkedKills, "lastMarkedKills", LookMode.Reference, LookMode.Reference);
        }
    }


    public class ThoughtWorker_MarkedKillPride : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return ThoughtState.ActiveDefault;
        }

        public override float MoodMultiplier(Pawn p)
        {
            return 1f;
        }
    }

    public class CompCorpseMarker : ThingComp
    {
        public Pawn killer;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref killer, "killer");
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("ASA_YautjaMarkKiller")) != true)
                yield break;

            var corpse = this.parent as Corpse;
            if (corpse.IsNotFresh())
                yield break;

            if (killer == null || killer.Dead || killer.Destroyed)
                yield break;

            float newBonus = KillPowerForPointsEvaluator.GetMoodBonusFromRace(corpse.InnerPawn.def.defName);
            float previousBonus = -9999f;

            if (GameComponent_KillRegistry.Instance.lastMarkedKills.TryGetValue(killer, out string previousCorpse))
            {
                ThingDef raceDef = DefDatabase<ThingDef>.GetNamed(previousCorpse, errorOnFail: false);
                if (raceDef != null && raceDef.race != null)
                {
                    previousBonus = KillPowerForPointsEvaluator.GetMoodBonusFromRace(previousCorpse);
                }
            }

            if (Prefs.DevMode)
                Log.Message($"[TrophyMark] Killer: {killer.NameShortColored}, New Bonus: {newBonus}, Previous Bonus: {previousBonus}");

            if (newBonus <= previousBonus)
                yield break;

            yield return new FloatMenuOption($"Mark {killer.NameShortColored} with trophy", () =>
            {
                GameComponent_KillRegistry.Instance.lastMarkedKills[killer] = corpse.def.defName;

                Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("MarkKillerWithTrophy"), parent, killer);
                selPawn.jobs.TryTakeOrderedJob(job);
            });
        }

    }
}
