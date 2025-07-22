using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ASA_Yautja_Xenotype
{
    public class CompProperties_AddAbilityOnEquip : CompProperties
    {
        public AbilityDef abilityDef;

        public CompProperties_AddAbilityOnEquip()
        {
            this.compClass = typeof(CompAddAbilityOnEquip);
        }
    }
    public class CompAddAbilityOnEquip : ThingComp
    {
        public CompProperties_AddAbilityOnEquip Props => (CompProperties_AddAbilityOnEquip)props;

        private int lastUsedTick = -1;
        private int charges = -1;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", -1);
            Scribe_Values.Look(ref charges, "charges", -1);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            var ability = pawn.abilities.GetAbility(Props.abilityDef);
            if (ability != null)
            {
                charges = ability.RemainingCharges;

                if (ability.CooldownTicksRemaining > 0)
                {
                    lastUsedTick = Find.TickManager.TicksGame - (ability.def.cooldownTicksRange.min - ability.CooldownTicksRemaining);
                }
                else
                {
                    lastUsedTick = -1;
                }

                pawn.abilities.RemoveAbility(Props.abilityDef);
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (charges == -1)
            {
                charges = Props.abilityDef.charges;
            }

            var ability = pawn.abilities.GetAbility(Props.abilityDef);
            if (ability == null)
            {
                pawn.abilities.GainAbility(Props.abilityDef);
                ability = pawn.abilities.GetAbility(Props.abilityDef);
            }

            ability.RemainingCharges = charges;

            if (lastUsedTick >= 0)
            {
                int ticksPassed = Find.TickManager.TicksGame - lastUsedTick;
                int cooldownDuration = ability.def.cooldownTicksRange.min;
                int remainingCooldown = cooldownDuration - ticksPassed;

                if (remainingCooldown > 0)
                {
                    ability.StartCooldown(remainingCooldown);
                }
                else
                {
                    ability.ResetCooldown();
                }
            }
            else
            {
                ability.ResetCooldown();
            }
        }

    }
}
