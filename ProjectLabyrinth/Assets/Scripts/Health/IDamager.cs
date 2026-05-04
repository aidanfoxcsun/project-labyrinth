// Interface for objects that can damage entities with a health component
// Hazards, projectiles, etc.
public interface IDamager
{
    float DamageAmount { get; }
    bool PlayerSourced { get; } // If the damager is coming from the player or not.
    // Essentially, if true => it can damage enemies, if false => it can damage the player.
}
