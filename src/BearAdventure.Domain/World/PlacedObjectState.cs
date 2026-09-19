using BearAdventure.Domain.Gameplay;

namespace BearAdventure.Domain.World;

public sealed class PlacedObjectState
{
    public PlacedObjectState(
        int placementId,
        ItemType item,
        int cellX,
        int logicalLevel,
        BuildLayer layer)
    {
        PlacementId = placementId;
        Item = item;
        CellX = cellX;
        LogicalLevel = logicalLevel;
        Layer = layer;
    }

    public int PlacementId { get; }

    public ItemType Item { get; }

    public int CellX { get; }

    public int LogicalLevel { get; }

    public BuildLayer Layer { get; }

    public double GrowthSeconds { get; set; }

    public double ProductionSeconds { get; set; }

    public int StoredOutput { get; set; }
}
