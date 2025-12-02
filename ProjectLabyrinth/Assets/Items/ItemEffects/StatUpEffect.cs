using System.Diagnostics;

[System.Serializable]
public class StatUpEffect : ItemEffect
{
    public float hp;
    public float flatDamage;
    public float damageScaling;
    public float speed;
    public float range;
    public float fireRate;

    public override void Apply(PlayerStats stats)
    {
        stats.maxHP += hp;
        stats.flatDamage += flatDamage;
        stats.damageScaling += damageScaling;
        stats.speed += speed;
        stats.range += range;
        stats.fireRate += fireRate;
    }

    public override string DebugEffects()
    {
        return $"\n\tHealth: +{hp}\n\tDamage: +{flatDamage}\n\tDamage Scaling: +{damageScaling}\n\tSpeed: +{speed}\n\tRange: +{range}\n\tFire Rate: +{fireRate}";
    }
}
