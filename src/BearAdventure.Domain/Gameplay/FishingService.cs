using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class FishingService
{
    public const double CatchSeconds = 2.0;
    public const double WaterReach = 150.0;

    public static bool IsNearFishableWater(WorldQueries world, WorldPoint actor)
    {
        ArgumentNullException.ThrowIfNull(world);
        double left = WorldGrid.LandLeft;
        double right = WorldGrid.LandLeft + world.Definition.WidthCells * WorldGrid.CellSize;
        WorldPoint leftWater = new(left - WorldGrid.CellSize * 0.55, 22);
        WorldPoint rightWater = new(right + WorldGrid.CellSize * 0.55, 22);
        return actor.DistanceSquared(leftWater) <= WaterReach * WaterReach ||
            actor.DistanceSquared(rightWater) <= WaterReach * WaterReach;
    }

    public static bool CanStart(WorldQueries world, WorldPoint actor, InventoryState bag, out string reason)
    {
        if (!bag.Has(ItemType.FishingRod)) { reason = "You need a fishing rod."; return false; }
        if (!IsNearFishableWater(world,actor)) { reason = "Move next to the ocean to fish."; return false; }
        reason = string.Empty; return true;
    }
}
