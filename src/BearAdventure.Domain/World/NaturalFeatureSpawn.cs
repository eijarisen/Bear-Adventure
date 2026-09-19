namespace BearAdventure.Domain.World;

public readonly record struct NaturalFeatureSpawn(
    int FeatureId,
    int CellX,
    int SurfaceLevel,
    NaturalFeatureKind Kind,
    int Variant);
