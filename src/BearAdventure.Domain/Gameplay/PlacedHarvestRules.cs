using BearAdventure.Domain.Simulation;
using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class PlacedHarvestRules
{
    public static bool IsHarvestable(
        PlacedObjectState placed)
    {
        ArgumentNullException.ThrowIfNull(placed);

        if (placed.Item == ItemType.Sapling)
        {
            return placed.GrowthSeconds
                >= WorldSimulationService.SaplingGrowthSeconds;
        }

        return PlacementRules.IsFlower(placed.Item)
            || placed.Item == ItemType.Grass;
    }

    public static bool IsLargeMatureTree(
        PlacedObjectState placed)
    {
        if (placed.Item != ItemType.Sapling)
        {
            return false;
        }

        unchecked
        {
            uint value =
                (uint)placed.PlacementId
                * 2654435761u;

            value ^=
                (uint)placed.CellX
                * 2246822519u;

            value ^= value >> 16;

            return value % 100u < 50u;
        }
    }

    public static double GetDurationSeconds(
        PlacedObjectState placed,
        InventoryState inventory)
    {
        ArgumentNullException.ThrowIfNull(placed);
        ArgumentNullException.ThrowIfNull(inventory);

        if (placed.Item == ItemType.Sapling)
        {
            return inventory.Has(ItemType.DiamondAxe)
                ? 0.25
                : inventory.Has(ItemType.IronAxe)
                    ? 0.50
                    : inventory.Has(ItemType.Axe)
                        ? 1.0
                        : 2.0;
        }

        return 0.35;
    }

    public static IReadOnlyList<HarvestReward> GetRewards(
        PlacedObjectState placed,
        InventoryState inventory)
    {
        ArgumentNullException.ThrowIfNull(placed);
        ArgumentNullException.ThrowIfNull(inventory);

        if (placed.Item == ItemType.Sapling)
        {
            bool diamond =
                inventory.Has(
                    ItemType.DiamondAxe);

            int wood =
                IsLargeMatureTree(placed)
                    ? 2
                    : 1;

            if (diamond)
            {
                wood *= 2;
            }

            return
            [
                new HarvestReward(
                    ItemType.Wood,
                    wood),
                new HarvestReward(
                    ItemType.Sapling,
                    diamond
                        ? 2
                        : 1),
            ];
        }

        if (PlacementRules.IsFlower(placed.Item)
            || placed.Item == ItemType.Grass)
        {
            return
            [
                new HarvestReward(
                    placed.Item,
                    1),
            ];
        }

        return Array.Empty<HarvestReward>();
    }

    public static string GetDisplayName(
        PlacedObjectState placed)
    {
        ArgumentNullException.ThrowIfNull(placed);

        return placed.Item == ItemType.Sapling
            ? (IsLargeMatureTree(placed)
                ? "large planted tree"
                : "small planted tree")
            : PlacementRules
                .GetDisplayName(placed.Item)
                .ToLowerInvariant();
    }
}
