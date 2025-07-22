using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class ASA_Yautja_XenotypeMod : Mod
    {
        public ASA_Yautja_XenotypeMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("ASA_Yautja_Xenotype");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    public static class Patch_PawnGenerate_AddMarkHediff
    {
        public static void Postfix(Pawn __result)
        {
            if (__result?.genes?.Xenotype != null && __result.genes.Xenotype.defName == "ASA_Yautja" && Rand.Chance(0.8f))
            {
                var hediffDef = DefDatabase<HediffDef>.GetNamed("MarkedByHunt", false);
                if (hediffDef == null)
                {
                    Log.Error("[ASA] HediffDef 'MarkedByHunt' not found.");
                    return;
                }

                var hediff = HediffMaker.MakeHediff(hediffDef, __result) as Hediff_MarkedByHunt;
                if (hediff != null)
                {
                    var label = PreyDatabase.GetRandomPreyLabel();
                    if (!label.NullOrEmpty())
                        hediff.markedTargetLabel = label;

                    BodyPartRecord headPart = __result.RaceProps.body.AllParts
                        .FirstOrDefault(part => part.def == BodyPartDefOf.Head);

                    __result.health.AddHediff(hediff, headPart);
                }
            }
        }
    }
    public static class PreyDatabase
    {
        private static List<string> cachedPreyLabels;

        public static string GetRandomPreyLabel()
        {
            if (cachedPreyLabels == null)
            {
                cachedPreyLabels = DefDatabase<PawnKindDef>.AllDefs
                    .Where(k => k.RaceProps != null && k.RaceProps.IsFlesh && !k.RaceProps.Humanlike)
                    .Select(k => k.LabelCap.ToString())
                    .Distinct()
                    .ToList();
            }
            return cachedPreyLabels.RandomElement();
        }
    }

    public class Hediff_MarkedByHunt : Hediff
    {
        public string markedTargetLabel;

        public override string LabelBase
        {
            get
            {
                return base.LabelBase + (markedTargetLabel != null ? $" ({markedTargetLabel})" : "");
            }
        }

        public override string TipStringExtra
        {
            get
            {
                return $"Marked from a kill: {markedTargetLabel ?? "unknown beast"}";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref markedTargetLabel, "markedTargetLabel");
        }
    }
}
