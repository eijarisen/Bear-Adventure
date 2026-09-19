using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class HarvestRules
{
    public static double GetDurationSeconds(
        NaturalFeatureKind kind,
        InventoryState inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        bool bush =
            kind == NaturalFeatureKind.Bush;

        if (kind is NaturalFeatureKind.Tree
            or NaturalFeatureKind.Pine
            or NaturalFeatureKind.Palm
            or NaturalFeatureKind.Cactus
            or NaturalFeatureKind.Bush)
        {
            double seconds =
                inventory.Has(ItemType.DiamondAxe)
                    ? 0.25
                    : inventory.Has(ItemType.IronAxe)
                        ? 0.50
                        : inventory.Has(ItemType.Axe)
                            ? 1.0
                            : 2.0;

            return bush
                ? seconds / 2.0
                : seconds;
        }

        if (kind == NaturalFeatureKind.Rock)
        {
            return inventory.Has(ItemType.DiamondPickaxe)
                ? 0.25
                : inventory.Has(ItemType.IronPickaxe)
                    ? 0.50
                    : inventory.Has(ItemType.Pickaxe)
                        ? 1.0
                        : 2.0;
        }

        return kind switch
        {
            NaturalFeatureKind.Flower
                or NaturalFeatureKind.Grass
                or NaturalFeatureKind.Mushroom
                    => 0.35,

            _ => 1.0,
        };
    }

    public static IReadOnlyList<HarvestReward> GetRewards(
        NaturalFeatureSpawn feature,
        InventoryState inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        return feature.Kind switch
        {
            NaturalFeatureKind.Tree =>
                TreeRewards(
                    feature,
                    inventory,
                    palm: false),

            NaturalFeatureKind.Pine =>
                TreeRewards(
                    feature,
                    inventory,
                    palm: false),

            NaturalFeatureKind.Palm =>
                TreeRewards(
                    feature,
                    inventory,
                    palm: true),

            NaturalFeatureKind.Cactus =>
                CactusRewards(
                    feature,
                    inventory),

            NaturalFeatureKind.Rock =>
                RockRewards(
                    feature,
                    inventory),

            NaturalFeatureKind.Flower =>
            [
                new HarvestReward(
                    FlowerItem(feature.Variant),
                    1),
            ],

            NaturalFeatureKind.Grass =>
            [
                new HarvestReward(
                    ItemType.Grass,
                    1),
            ],

            // Original game bushes always yielded exactly one wood.
            NaturalFeatureKind.Bush =>
            [
                new HarvestReward(
                    ItemType.Wood,
                    1),
            ],

            NaturalFeatureKind.Mushroom =>
            [
                new HarvestReward(
                    feature.Variant == 0
                        ? ItemType.MushroomBrown
                        : ItemType.MushroomRed,
                    1),
            ],

            _ =>
                Array.Empty<HarvestReward>(),
        };
    }

    public static bool IsLarge(
        NaturalFeatureSpawn feature)
    {
        return feature.Kind switch
        {
            // Original tree, pine and palm generation produced large variants
            // approximately half the time.
            NaturalFeatureKind.Tree
                or NaturalFeatureKind.Pine
                or NaturalFeatureKind.Palm
                    => PositiveModulo(
                        feature.FeatureId,
                        2) == 0,

            // Original cactus height range made about 40% large.
            NaturalFeatureKind.Cactus
                    => PositiveModulo(
                        feature.FeatureId,
                        5) < 2,

            _ => false,
        };
    }

    public static string GetDisplayName(
        NaturalFeatureSpawn feature)
    {
        string baseName =
            GetDisplayName(
                feature.Kind);

        if (feature.Kind is
            NaturalFeatureKind.Tree
            or NaturalFeatureKind.Pine
            or NaturalFeatureKind.Palm
            or NaturalFeatureKind.Cactus)
        {
            return IsLarge(feature)
                ? $"large {baseName}"
                : $"small {baseName}";
        }

        return baseName;
    }

    public static string GetDisplayName(
        NaturalFeatureKind kind)
    {
        return kind switch
        {
            NaturalFeatureKind.Tree => "tree",
            NaturalFeatureKind.Pine => "pine tree",
            NaturalFeatureKind.Palm => "palm tree",
            NaturalFeatureKind.Cactus => "cactus",
            NaturalFeatureKind.Rock => "rock",
            NaturalFeatureKind.Flower => "flower",
            NaturalFeatureKind.Grass => "grass",
            NaturalFeatureKind.Bush => "bush",
            NaturalFeatureKind.Mushroom => "mushroom",
            _ => "resource",
        };
    }

    private static IReadOnlyList<HarvestReward> TreeRewards(
        NaturalFeatureSpawn feature,
        InventoryState inventory,
        bool palm)
    {
        bool diamond =
            inventory.Has(
                ItemType.DiamondAxe);

        int wood =
            IsLarge(feature)
                ? 2
                : 1;

        if (diamond)
        {
            wood *= 2;
        }

        int saplings =
            diamond
                ? 2
                : 1;

        var rewards =
            new List<HarvestReward>
            {
                new(
                    ItemType.Wood,
                    wood),
                new(
                    ItemType.Sapling,
                    saplings),
            };

        // Original jungle palms had a 10% banana drop. Use the feature
        // identity for the roll so loading a save cannot reroll the result.
        if (palm
            && StablePercent(
                feature,
                salt: 0x77)
                < 10)
        {
            rewards.Add(
                new HarvestReward(
                    ItemType.Banana,
                    diamond
                        ? 2
                        : 1));
        }

        return rewards;
    }

    private static IReadOnlyList<HarvestReward> CactusRewards(
        NaturalFeatureSpawn feature,
        InventoryState inventory)
    {
        bool diamond =
            inventory.Has(
                ItemType.DiamondAxe);

        int cactus =
            IsLarge(feature)
                ? 2
                : 1;

        if (diamond)
        {
            cactus *= 2;
        }

        var rewards =
            new List<HarvestReward>
            {
                new(
                    ItemType.Cactus,
                    cactus),
            };

        // Original game: 1% chance for a pink flower.
        if (StablePercent(
            feature,
            salt: 0x99)
            < 1)
        {
            rewards.Add(
                new HarvestReward(
                    ItemType.PinkFlower,
                    diamond
                        ? 2
                        : 1));
        }

        return rewards;
    }

    private static IReadOnlyList<HarvestReward> RockRewards(
        NaturalFeatureSpawn feature,
        InventoryState inventory)
    {
        bool diamond =
            inventory.Has(
                ItemType.DiamondPickaxe);

        bool ironTool =
            inventory.Has(
                ItemType.IronPickaxe)
            || diamond;

        int stone =
            diamond
                ? 2
                : 1;

        bool hasIron =
            ironTool
            || StablePercent(
                feature,
                salt: 0xB3)
                < 25;

        if (!hasIron)
        {
            return
            [
                new HarvestReward(
                    ItemType.Stone,
                    stone),
            ];
        }

        return
        [
            new HarvestReward(
                ItemType.Stone,
                stone),
            new HarvestReward(
                ItemType.IronOre,
                diamond
                    ? 2
                    : 1),
        ];
    }

    private static ItemType FlowerItem(
        int variant)
    {
        return PositiveModulo(
            variant,
            3) switch
        {
            0 => ItemType.RedFlower,
            1 => ItemType.YellowFlower,
            _ => ItemType.BlueFlower,
        };
    }

    private static int StablePercent(
        NaturalFeatureSpawn feature,
        int salt)
    {
        unchecked
        {
            uint value =
                (uint)feature.FeatureId
                * 2654435761u;

            value ^=
                (uint)feature.CellX
                * 2246822519u;

            value ^=
                (uint)(feature.Variant + 1)
                * 3266489917u;

            value ^=
                (uint)salt;

            value ^= value >> 16;
            value *= 2246822519u;
            value ^= value >> 13;

            return (int)(value % 100u);
        }
    }

    private static int PositiveModulo(
        int value,
        int modulus)
    {
        int result =
            value % modulus;

        return result < 0
            ? result + modulus
            : result;
    }
}
