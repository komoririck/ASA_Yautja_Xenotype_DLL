using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ASA_Yautja_Xenotype
{
    public class CompProperties_CheckEquipRestrictions : CompProperties
    {
        public GeneDef requiresGene;

        public CompProperties_CheckEquipRestrictions()
        {
            this.compClass = typeof(CompCheckEquipRestrictions);
        }
    }

    public class CompCheckEquipRestrictions : ThingComp
    {
        public CompProperties_CheckEquipRestrictions Props => (CompProperties_CheckEquipRestrictions)this.props;

        public static ApparelLayerDef LeftForearmSlot;
        public static ApparelLayerDef RightForearmSlot;
        public static ApparelLayerDef ShoulderSlot;

        static void CustomApparelLayerDefOf()
        {
            LeftForearmSlot = new ApparelLayerDef { defName = "LeftForearmSlot", label = "Left Forearm Slot" };
            RightForearmSlot = new ApparelLayerDef { defName = "RightForearmSlot", label = "Right Forearm Slot" };
            ShoulderSlot = new ApparelLayerDef { defName = "ShoulderSlot", label = "Shoulder Slot" };
            DefDatabase<ApparelLayerDef>.Add(LeftForearmSlot);
            DefDatabase<ApparelLayerDef>.Add(RightForearmSlot);
            DefDatabase<ApparelLayerDef>.Add(ShoulderSlot);
        }
    }

    [HarmonyPatch(typeof(JobDriver_ForceTargetWear), "MakeNewToils")]
    [HarmonyPatch(typeof(JobDriver_Wear), "MakeNewToils")]
    public static class Patch_JobDriver_Wear
    {
        static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_Wear __instance)
        {
            foreach (var toil in __result)
            {
                toil.AddPreInitAction(() =>
                {
                    var pawn = __instance.pawn;

                    var apparel = pawn.CurJob?.targetA.Thing as Apparel;
                    if (apparel == null)
                        return;

                    if (!CanEquipRestrictedItem(pawn, apparel, out string reason))
                    {
                        Messages.Message(reason, pawn, MessageTypeDefOf.RejectInput);
                        __instance.EndJobWith(JobCondition.Incompletable);
                    }
                });
                yield return toil;
            }
        }
        public static bool CanEquipRestrictedItem(Pawn pawn, Thing item, out string reason)
        {
            reason = "";
            var comp = item.TryGetComp<CompCheckEquipRestrictions>();
            if (comp == null)
                return true;

            if (comp.Props.requiresGene != null && (pawn?.genes?.HasActiveGene(comp.Props.requiresGene) != true))
            {
                reason = $"{pawn.NameShortColored} cannot use this item. Lacks gene: {comp.Props.requiresGene.label}";
                return false;
            }

            if (!pawn.health.hediffSet.GetNotMissingParts().Any(p => p.def == BodyPartDefOf.Shoulder))
            {
                reason = $"No body parts suitable for equipping this item.";
                return false;
            }

            if (pawn.apparel.WornApparel.Any(apparel => apparel.def.apparel.layers.Contains(CompCheckEquipRestrictions.ShoulderSlot)))
            {
                reason = $"Already have an item in the Shoulder Slot.";
                return false;
            }

            if (pawn.IsFormingCaravan() || pawn.IsCaravanMember())
            {
                reason = "Cannot equip this item while in caravan.";
                return false;
            }

            return true;
        }
    }
}
