// Interface for defining basic Behaviors that all entites share.
// Here, the word 'entity' specifically is defined as a GameObject which can receive damage and interact with the player
public interface IEntityBehavior
{
    void OnDeath();
    void OnHit();
}
