# API Documentation — DungeonGenerator
## Component: DungeonGenerator.GenerateDungeon()

### Description
GenerateDungeon() is the core procedural generation method for our Project Labyrinth.
    It'll build a grid-based dungeon layout, assigns special room types (Start, Boss, Treasure, Upgrade, Normal), computes door connections, and spawns all room prefabs and their contents including rocks, enemies, boss, and treasure items.

## Signature

void GenerateDungeon()

## Parameters

GenerateDungeon() does not take direct function parameters.
    It relies on configuration fields set thats provided in the Unity Inspector.

## Generation Settings

int seed
    Random seed for dungeon generation.
    If 0, a time-based seed is used.

int targetRooms
    Target number of grid cells to occupy.
    Large rooms count as 4 cells.

int minBossDistance
    Minimum required BFS distance between the Start room and Boss room.


## Layout Settings

Vector2 roomSpacing
    World-space spacing between room origins.

bool autoDetectSpacing
    Automatically calculates spacing based on roomPrefab renderer bounds.

float verticalOverlap
    Adjustment applied during spacing auto-detection.


## Prefabs

GameObject roomPrefab (Or something that you can use as a placeholder)

GameObject bossPrefab

GameObject treasureItemPrefab

GameObject[] rockPrefabs

GameObject[] enemyPrefabs



## Enemy / Rock Settings

minEnemiesPerRoom

maxEnemiesPerRoom

minRocksPerRoom

maxRocksPerRoom



## Return Value

None are required for this part of the code. 

This method modifies the Unity scene by:

Instantiating room GameObjects

Enabling doors and disabling walls based on adjacency

Spawning rocks in normal rooms

Spawning enemies using RoomEnemySpawner

Spawning the boss using BossSpawner

Spawning treasure items in Treasure rooms

Updating internal state collections



## Internal State Modified

occupied — Tracks grid cells used by rooms

occupiedByLargeRooms — Tracks cells belonging to large rooms

largeRoomOrigins — Stores origins of 2×2 rooms

roomTypes — Maps grid positions to RoomType

spawnedRooms — Maps grid positions to instantiated room GameObjects



## Errors / Exceptions (Common Failure Cases)

roomPrefab is null → Instantiation failure

Missing NodeGraphGenerator on room prefab → NullReferenceException

Incorrect door or wall child names → Doors will not toggle correctly

Missing layers "Rocks" or "Doors" → Enemy spawner masks fail

bossPrefab not assigned → Boss room spawns without boss

enemyPrefabs empty → No enemies in normal rooms



## Example Usage

Automatic generation on scene start:

void Start() => GenerateDungeon();

Generating a new floor during gameplay:

generator.GenerateNewFloor();



## Notes / Limitations

Layout uses randomized frontier expansion with adjacency control.

Boss room is selected as the farthest room from Start using BFS.

Treasure and Upgrade rooms are placed using depth prioritization with fallback logic.

Seed-based generation allows deterministic layouts when seed is non-zero.

Generation depends on correctly assigned prefabs and inspector configuration.