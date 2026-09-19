namespace BearAdventure.Domain.World;

public sealed record GeneratedStructureDefinition(
    int StructureId,
    GeneratedStructureKind Kind,
    int CenterCellX,
    int WidthCells,
    int BaseSurfaceLevel,
    GeneratedChestDefinition Chest);
