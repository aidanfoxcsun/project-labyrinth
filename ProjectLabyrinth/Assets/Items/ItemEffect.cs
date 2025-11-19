[System.Serializable]
public abstract class ItemEffect
{
    public abstract void Apply(PlayerStats stats);
    public abstract string DebugEffects();
}
