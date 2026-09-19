using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class MiningRules
{
    public static double GetDurationSeconds(
        InventoryState inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        if (inventory.Has(ItemType.DiamondPickaxe))
        {
            return 0.25;
        }

        if (inventory.Has(ItemType.IronPickaxe))
        {
            return 0.50;
        }

        if (inventory.Has(ItemType.Pickaxe))
        {
            return 1.0;
        }

        return 2.5;
    }

    public static IReadOnlyList<HarvestReward> GetRewards(
        UndergroundOreSpawn? ore,
        InventoryState inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        bool diamond =
            inventory.Has(
                ItemType.DiamondPickaxe);

        int stone =
            diamond
                ? 2
                : 1;

        if (ore is null)
        {
            return
            [
                new HarvestReward(
                    ItemType.Stone,
                    stone),
            ];
        }

        return ore.Value.Kind switch
        {
            UndergroundOreKind.Iron =>
            [
                new HarvestReward(
                    ItemType.Stone,
                    stone),
                new HarvestReward(
                    ItemType.IronOre,
                    Math.Max(
                        1,
                        ore.Value.Richness)
                    * (diamond ? 2 : 1)),
            ],

            _ =>
            [
                new HarvestReward(
                    ItemType.Stone,
                    stone),
            ],
        };
    }
}
