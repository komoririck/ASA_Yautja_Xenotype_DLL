using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace ASA_Yautja_Xenotype
{
    public class CompProperties_AbilityDestroySpecificGearAfterDelay : AbilityCompProperties
    {
        public string targetApparelDefName;
        public int delayTicks = 600;

        public CompProperties_AbilityDestroySpecificGearAfterDelay()
        {
            this.compClass = typeof(CompAbilityEffect_DestroySpecificGearAfterDelay);
        }
    }

    public class CompAbilityEffect_DestroySpecificGearAfterDelay : CompAbilityEffect
    {
        public CompProperties_AbilityDestroySpecificGearAfterDelay Props => (CompProperties_AbilityDestroySpecificGearAfterDelay)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            var props = Props;
            if (props == null)
            {
                Log.Error($"[CompAbilityEffect_DestroySpecificGearAfterDelay] Props cast failed on ability {parent?.def?.defName}.");
                return;
            }

            Pawn pawn = parent.pawn;
            if (pawn == null)
                return;

            Thing targetGear = pawn.apparel?.WornApparel?.FirstOrDefault(a => a.def.defName == props.targetApparelDefName);

            if (targetGear != null)
            {
                pawn.Map.GetComponent<MapComponent_DelayedActions>()?.ScheduleAction(targetGear, props.delayTicks);
            }
            else
            {
                Log.Warning($"[DestroySpecificGearAfterDelay] {pawn} does not have gear {props.targetApparelDefName} equipped.");
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props == null || parent?.pawn == null)
            {
                return false;
            }

            return parent.pawn.apparel?.WornApparel?.Any(a => a.def.defName == Props.targetApparelDefName) == true;
        }
    }

    public class MapComponent_DelayedActions : MapComponent
    {
        private class ScheduledAction
        {
            public Action action;
            public Thing target;
            public int tickDue;
            public int remainingTicks;
        }

        private List<ScheduledAction> scheduledActions = new List<ScheduledAction>();

        public MapComponent_DelayedActions(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (scheduledActions.Count == 0) return;

            int currentTick = Find.TickManager.TicksGame;
            for (int i = scheduledActions.Count - 1; i >= 0; i--)
            {
                var action = scheduledActions[i];

                if (currentTick >= action.tickDue)
                {
                    try
                    {
                        action.action?.Invoke();

                        if (action.target != null && !action.target.Destroyed)
                        {
                            GenExplosion.DoExplosion(
                                action.target.Position,
                                map,
                                15f,
                                DamageDefOf.Bomb,
                                action.target,
                                -1,
                                armorPenetration: 0.6f
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Exception in Delayed Action: {ex}");
                    }
                    scheduledActions.RemoveAt(i);
                }
                else
                {
                    action.remainingTicks--;

                    if (action.remainingTicks % 60 == 0 && action.target != null && action.target.Spawned)
                    {
                        MoteMaker.ThrowText(
                            action.target.DrawPos,
                            map,
                            $"{action.remainingTicks / 60}s",
                            Color.red
                        );
                    }
                }
            }
        }

        public void ScheduleAction(Thing target, int delayTicks)
        {
            int executeAt = Find.TickManager.TicksGame + delayTicks;

            scheduledActions.Add(new ScheduledAction
            {
                action = () =>
                {
                    if (target != null && !target.Destroyed)
                    {
                        target.Destroy();
                        Messages.Message($"{target.LabelCap} was destroyed after delay.", MessageTypeDefOf.NegativeEvent);
                    }
                },
                target = target,
                tickDue = executeAt,
                remainingTicks = delayTicks
            });
        }
    }
}
