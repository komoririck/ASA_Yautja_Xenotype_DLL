using System.Linq;
using UnityEngine;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public static class KillPowerForPointsEvaluator
    {
        public static float GetMoodBonusFromRace(string killedPawn)
        {
            float combatPower = -9999;
            ThingDef raceDef = DefDatabase<ThingDef>.GetNamed(killedPawn, errorOnFail: false);
            if (raceDef != null)
            {
                PawnKindDef kindDef = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(pk => pk.race == raceDef);
                if (kindDef != null)
                {
                    if (Prefs.DevMode)
                        Log.Message($"Checking {killedPawn} with points {kindDef.combatPower}");

                    combatPower = kindDef.combatPower;
                }
            }
            float moodBonus;

            if (combatPower < 30f)
            {
                moodBonus = Mathf.Lerp(-4f, -1f, (combatPower - 10f) / 20f);
            }
            else if (combatPower < 100f)
            {
                moodBonus = Mathf.Lerp(-1f, 1f, (combatPower - 30f) / 70f);
            }
            else if (combatPower < 250f)
            {
                moodBonus = Mathf.Lerp(1f, 3f, (combatPower - 100f) / 150f);
            }
            else if (combatPower < 400f)
            {
                moodBonus = Mathf.Lerp(3f, 6f, (combatPower - 250f) / 150f);
            }
            else if (combatPower < 800f)
            {
                moodBonus = Mathf.Lerp(6f, 10f, (combatPower - 400f) / 400f);
            }
            else
            {
                moodBonus = Mathf.Lerp(10f, 12f, Mathf.Clamp01((combatPower - 800f) / 1200f));
            }
            return moodBonus;
        }
    }
}
