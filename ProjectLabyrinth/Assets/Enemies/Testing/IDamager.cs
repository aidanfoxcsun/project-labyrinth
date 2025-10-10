// Interface for objects that can damage entities with a health component
// Hazards, projectiles, etc.
public interface IDamager
{
    float DamageAmount { get; }
}
