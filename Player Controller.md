# Project Labyrinth API Documentation

## Player Controls

### KnightController2D

Handles Knight movement, melee combat, guarding, shield bash, death state, and bomb placement.

#### Inspector Fields

| Field | Type | Description |
|---|---|---|
| moveSpeed | float | Controls Knight movement speed. |
| attackKey | KeyCode | Button used for sword attack. |
| guardKey | KeyCode | Button held for guarding. |
| bashKey | KeyCode | Button used for shield bash. |
| bombKey | KeyCode | Button used to place bombs. |
| bombPrefab | GameObject | Bomb object spawned when placing a bomb. |
| bombCount | int | Number of bombs available. |
| bombPlacementOffset | float | Distance from player where bomb spawns. |

#### Main Methods

| Method | Description |
|---|---|
| TriggerHit() | Plays hit animation when Knight takes damage. |
| Die() | Sets death state and stops movement. |
| SetFacing(Vector2 dir) | Manually updates the Knight’s facing direction. |
| AddBombs(int amount) | Adds bombs to the Knight’s available bomb count. |

---

### ArcherController2D

Handles Archer movement, ranged attack animation, evasion, damage, death state, and bomb placement.

#### Inspector Fields

| Field | Type | Description |
|---|---|---|
| moveSpeed | float | Controls Archer movement speed. |
| attackButton | string | Input button used for ranged attack. |
| evadeButton | string | Input button used for evasion. |
| bombKey | KeyCode | Button used to place bombs. |
| bombPrefab | GameObject | Bomb object spawned when placing a bomb. |
| bombCount | int | Number of bombs available. |
| bombPlacementOffset | float | Distance from player where bomb spawns. |

#### Main Methods

| Method | Description |
|---|---|
| TakeDamage(int damage, Vector2 hitFromPosition) | Reduces Archer HP and triggers hit/death logic. |
| TakeHit(Vector2 hitFromPosition) | Plays hit animation and applies hit stun. |
| Die() | Triggers death animation and stops movement. |
| AddBombs(int amount) | Adds bombs to the Archer’s available bomb count. |

---

## Bomb System

### Bomb

Handles fuse timing, explosion radius, damage, and explosion cleanup.

#### Inspector Fields

| Field | Type | Description |
|---|---|---|
| fuseTime | float | Time before the bomb explodes. |
| explosionRadius | float | Radius of the explosion. |
| damage | int | Damage dealt to enemies inside the radius. |
| damageLayers | LayerMask | Layers affected by the explosion. |

---

### BombPickup

Pickup object that increases the player's bomb count.

#### Inspector Fields

| Field | Type | Description |
|---|---|---|
| bombAmount | int | Number of bombs added when collected. |

#### Behavior

When the player touches the pickup, the script checks whether the player has a Knight or Archer controller. If found, it calls `AddBombs()` and destroys the pickup object.