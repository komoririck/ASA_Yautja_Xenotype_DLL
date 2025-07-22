using RimWorld;
using Verse;
using Verse.AI.Group;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;

namespace ASA_Yautja_Xenotype
{
    public class QuestNode_Root_YautjaShip : QuestNode
    {
        private const int TicksToShuttleArrival = 180;
        private const int TicksToReinforcements = 10000;
        private const int TicksToBeginAssault = 5000;

        protected override void RunInt()
        {
            Quest quest = QuestGen.quest;
            Slate slate = QuestGen.slate;
            Map map = QuestGen_Get.GetMap();

            float points = slate.Get("points", 0f);
            int endTicks = 5240;
            string questTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("YautjaShip");
            string attackedSignal = QuestGenUtility.HardcodedSignalWithQuestID("shuttlePawns.TookDamageFromPlayer");
            string defendTimeoutSignal = QuestGen.GenerateNewSignal("DefendTimeout");
            string beginAssaultSignal = QuestGen.GenerateNewSignal("BeginAssault");
            string assaultBeganSignal = QuestGen.GenerateNewSignal("AssaultBegan");
            string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved");

            slate.Set("map", map);
            List<FactionRelation> relations = new List<FactionRelation>();
            foreach (Faction fac in Find.FactionManager.AllFactionsListForReading)
            {
                relations.Add(new FactionRelation(fac, FactionRelationKind.Hostile));
            }

            Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(DefDatabase<FactionDef>.GetNamed("ASA_Yautja_Outer_Clan"), relations, hidden: true);
            faction.temporary = true;
            Find.FactionManager.Add(faction);
            quest.ReserveFaction(faction);

            List<Pawn> shuttlePawns = new List<Pawn>();
            PawnGenerationRequest hunterRequest = new PawnGenerationRequest(
                DefDatabase<PawnKindDef>.GetNamed("ASA_Quest_Fighter_Ancient"),
                faction,
                PawnGenerationContext.NonPlayer,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: true,
                mustBeCapableOfViolence: true,
                forcedEndogenes: new List<GeneDef>() { DefDatabase<GeneDef>.GetNamed("ArchiteMetabolism") },
                forcedXenotype: DefDatabase<XenotypeDef>.GetNamed("ASA_Yautja")
            );

            for (int i = 0; i < 3; i++)
            {
                Pawn hunter = quest.GeneratePawn(hunterRequest);
                hunter.health.forceDowned = true;
                shuttlePawns.Add(hunter);
            }

            slate.Set("shuttlePawns", shuttlePawns);

            Thing shuttle = ThingMaker.MakeThing(ThingDefOf.Shuttle);
            quest.SetFaction(Gen.YieldSingle(shuttle), faction);

            TryFindShuttleCrashPosition(map, shuttle.def.size, out var shuttleCrashPos);
            TransportShip ship = quest.GenerateTransportShip(TransportShipDefOf.Ship_Shuttle, shuttlePawns, shuttle).transportShip;
            quest.AddShipJob_WaitTime(ship, 60, false).showGizmos = false;
            quest.AddShipJob(ship, ShipJobDefOf.Unload);
            QuestUtility.AddQuestTag(ref ship.questTags, questTag);
            quest.End(QuestEndOutcome.Fail, 0, null, inSignal);

            int reinforcementsCount = Mathf.RoundToInt(PointsToReinforcementsCountCurve.Evaluate(points));
            if (reinforcementsCount > 0)
                endTicks = 10060;

            List<Pawn> reinforcements = null;
            if (reinforcementsCount > 0)
            {
                reinforcements = new List<Pawn>();
                PawnGenerationRequest reinfRequest = new PawnGenerationRequest(
                    DefDatabase<PawnKindDef>.GetNamed("ASA_Quest_Fighter_Ancient_Reinforce"),
                    faction,
                    PawnGenerationContext.NonPlayer,
                    forceGenerateNewPawn: true,
                    allowDead: false,
                    allowDowned: false,
                    canGeneratePawnRelations: true,
                    mustBeCapableOfViolence: true,
                    forcedXenotype: DefDatabase<XenotypeDef>.GetNamed("ASA_Yautja")
                );

                for (int i = 0; i < reinforcementsCount; i++)
                {
                    reinforcements.Add(quest.GeneratePawn(reinfRequest));
                }

                quest.BiocodeWeapons(reinforcements);
            }

            quest.Delay(TicksToShuttleArrival, () =>
            {
                quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, false, QuestPart.SignalListenMode.OngoingOnly, shuttlePawns, false, "[yautjaShuttleCrashedLetterText]", null, "[yautjaShuttleCrashedLetterLabel]");
                quest.AddShipJob_Arrive(ship, map.Parent, shuttlePawns[0], shuttleCrashPos, ShipJobStartMode.Force_DelayCurrent, faction);
                quest.DefendPoint(map.Parent, shuttlePawns[0], shuttleCrashPos, shuttlePawns, faction, null, null, 5f);
                quest.Delay(TicksToBeginAssault, () =>
                {
                    quest.SignalPass(null, null, attackedSignal);
                }).debugLabel = "Assault delay";

                quest.AnySignal(new[] { attackedSignal, defendTimeoutSignal }, null, Gen.YieldSingle(beginAssaultSignal));

                quest.SignalPassActivable(() =>
                {
                    quest.AnyPawnInCombatShape(shuttlePawns, () =>
                    {
                        QuestPart_AssaultColony assault = quest.AssaultColony(faction, map.Parent, shuttlePawns);
                        assault.canKidnap = false;
                        assault.canSteal = false;
                        assault.canTimeoutOrFlee = false;
                        quest.Letter(LetterDefOf.ThreatSmall, null, null, null, null, false, QuestPart.SignalListenMode.OngoingOnly, shuttlePawns, false, "[assaultBeginLetterText]", null, "[assaultBeginLetterLabel]");
                    }, null, null, assaultBeganSignal);
                }, null, beginAssaultSignal, null, null, assaultBeganSignal);

                if (reinforcementsCount > 0)
                {
                    quest.Delay(TicksToReinforcements, () =>
                    {
                        quest.Letter(LetterDefOf.ThreatBig, null, null, null, null, false, QuestPart.SignalListenMode.OngoingOnly, reinforcements, false, "[reinforcementsArrivedLetterText]", null, "[reinforcementsArrivedLetterLabel]");
                        DropCellFinder.TryFindRaidDropCenterClose(out var dropSpot, map);
                        quest.DropPods(map.Parent, reinforcements, null, null, null, null, false, false, false, false, null, null, QuestPart.SignalListenMode.OngoingOnly, dropSpot);
                        QuestPart_AssaultColony reinfAssault = quest.AssaultColony(faction, map.Parent, reinforcements);
                        reinfAssault.canSteal = false;
                        reinfAssault.canTimeoutOrFlee = false;
                    }).debugLabel = "Reinforcements delay";
                }

                quest.Delay(endTicks, () =>
                {
                    QuestGen_End.End(quest, QuestEndOutcome.Success);
                }).debugLabel = "End delay";

            }, null, null, null, false, null, null, false, null, null, null, false, QuestPart.SignalListenMode.OngoingOnly, true).debugLabel = "Arrival delay";
        }

        protected override bool TestRunInt(Slate slate)
        {
            Map map = QuestGen_Get.GetMap();
            if (map == null) return false;
            return TryFindShuttleCrashPosition(map, ThingDefOf.ShuttleCrashed.size, out _);
        }

        private static readonly SimpleCurve PointsToReinforcementsCountCurve = new SimpleCurve
        {
            new CurvePoint(2000f, 0f),
            new CurvePoint(2500f, 2f),
            new CurvePoint(5000f, 4f),
            new CurvePoint(8000f, 6f)
        };

        private bool TryFindShuttleCrashPosition(Map map, IntVec2 size, out IntVec3 spot)
        {
            return DropCellFinder.FindSafeLandingSpot(out spot, null, map, 35, 15, 25, size, ThingDefOf.ShuttleCrashed.interactionCellOffset);
        }
    }
}
//QuestNode_Root_SanguophageShip