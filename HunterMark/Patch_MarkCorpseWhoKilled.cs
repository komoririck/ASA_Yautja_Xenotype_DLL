using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class Patch_MarkCorpseWhoKilled
    {
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static class Patch_KillTracking
        {
            [HarmonyPostfix]
            public static void RecordKiller(Pawn __instance, DamageInfo? dinfo)
            {
                if (__instance.Corpse is Corpse corpse && dinfo.HasValue && dinfo.Value.Instigator is Pawn killer)
                {
                    var comp = corpse.TryGetComp<CompCorpseMarker>();
                    if (comp != null)
                        comp.killer = killer;
                }
            }
        }

    }
    [StaticConstructorOnStartup]
    public static class ApplyCompToAllCorpses
    {
        static ApplyCompToAllCorpses()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.thingClass == typeof(Corpse))
                {
                    if (def.comps == null)
                        def.comps = new List<CompProperties>();

                    if (!def.comps.Any(c => c.compClass == typeof(CompCorpseMarker)))
                    {
                        def.comps.Add(new CompProperties
                        {
                            compClass = typeof(CompCorpseMarker)
                        });
                    }
                }
            }
        }
    }
}
