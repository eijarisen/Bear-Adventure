namespace BearAdventure.Domain.World;

public readonly record struct UndergroundOreSpawn(
    UndergroundCell Cell,
    UndergroundOreKind Kind,
    int Richness);
