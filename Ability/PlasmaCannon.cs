using RimWorld;
using Verse;

namespace ASA_Yautja_Xenotype
{
    //Plasmacaster
    public class CompProperties_AbilityShootProjectile : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public SoundDef explosionSound;
        public DamageDef damageDef;
        public float explosionRadius = 0f;

        public CompProperties_AbilityShootProjectile()
        {
            this.compClass = typeof(CompAbilityEffect_ShootProjectile);
        }
    }
    public class CompAbilityEffect_ShootProjectile : CompAbilityEffect
    {
        public new CompProperties_AbilityShootProjectile Props => (CompProperties_AbilityShootProjectile)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            var pawn = this.parent.pawn;
            if (pawn == null || Props.projectileDef == null)
                return;

            var projectile = (Projectile_CustomRocket)GenSpawn.Spawn(Props.projectileDef, pawn.Position, pawn.Map);
            projectile.explosionRadius = Props.explosionRadius;
            projectile.Launch(pawn, target, target, ProjectileHitFlags.IntendedTarget);

        }
        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);

            if (target.IsValid && Props.explosionRadius > 0f)
            {
                GenDraw.DrawRadiusRing(target.Cell, Props.explosionRadius);
            }
        }

        public class Projectile_CustomRocket : Projectile_Explosive
        {
            public SoundDef explosionSound;
            public DamageDef damageDef;
            public float explosionRadius;

            protected override void Impact(Thing hitThing, bool blockedByShield = false)
            {
                if (Destroyed || Map == null)
                    return;

                GenExplosion.DoExplosion(
                    Position,
                    Map,
                    explosionRadius,
                    damageDef ?? def.projectile.damageDef,
                    launcher,
                    DamageAmount,
                    armorPenetration: ArmorPenetration,
                    explosionSound: explosionSound
                );

                if (!Destroyed)
                {
                    Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
