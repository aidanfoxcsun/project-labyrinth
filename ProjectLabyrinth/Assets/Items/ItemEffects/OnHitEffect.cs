[System.Serializable]
public class OnHitEffect : ItemEffect
{
    public float poisonChance;
    public float burnChance;
    public float freezeChance;

    public override void Apply(PlayerStats stats)
    {
        stats.onHitEffects.Add(this);
    }

    public override string DebugEffects()
    {
        return $"Posion Chance: {poisonChance}\nBurn Chance: {burnChance}\nFreeze Chance: {freezeChance}";
    }
}
