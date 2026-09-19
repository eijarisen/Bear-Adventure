namespace BearAdventure.Domain.World;

public readonly record struct UndergroundCell(
    int CellX,
    int LogicalLevel);
