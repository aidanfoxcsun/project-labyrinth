[System.Serializable]
public class FlagModifierEffect : ItemEffect
{
    public bool canFly;
    public bool piercing;
    public bool spectral;

    public override void Apply(PlayerStats stats)
    {
        stats.canFly |= canFly;
        stats.piercing |= piercing;
        stats.spectral |= spectral;
    }

    public override string DebugEffects()
    {
        return $"Can Fly? {canFly}\nPiercing Shots? {piercing}\nSpectral Shots? {spectral}";
    }
}
