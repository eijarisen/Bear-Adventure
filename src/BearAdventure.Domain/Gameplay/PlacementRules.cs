namespace BearAdventure.Domain.Gameplay;

public static class PlacementRules
{
    public static bool IsPlaceable(ItemType item)
    {
        return IsBlock(item)
            || IsPlantable(item)
            || item is ItemType.Beehive
                or ItemType.Chest
                or ItemType.Forge
                or ItemType.Anvil
                or ItemType.Cauldron;
    }

    public static bool IsBlock(ItemType item)
    {
        return item is ItemType.Wood
            or ItemType.Stone
            or ItemType.Sandstone
            or ItemType.Cactus;
    }

    public static bool IsPlantable(ItemType item)
    {
        return item is ItemType.Sapling
            or ItemType.RedFlower
            or ItemType.YellowFlower
            or ItemType.BlueFlower
            or ItemType.OrangeFlower
            or ItemType.PurpleFlower
            or ItemType.PinkFlower
            or ItemType.Grass;
    }

    public static bool IsFlower(ItemType item)
    {
        return item is ItemType.RedFlower
            or ItemType.YellowFlower
            or ItemType.BlueFlower
            or ItemType.OrangeFlower
            or ItemType.PurpleFlower
            or ItemType.PinkFlower;
    }

    public static bool UsesBuildLayer(ItemType item)
    {
        return IsBlock(item);
    }

    public static bool IsSolid(ItemType item, BuildLayer layer)
    {
        if (IsBlock(item))
        {
            return layer == BuildLayer.Solid;
        }

        return item is ItemType.Chest
            or ItemType.Forge
            or ItemType.Anvil
            or ItemType.Cauldron;
    }

    public static string GetDisplayName(ItemType item)
    {
        return item switch
        {
            ItemType.RedFlower => "Red Flower",
            ItemType.YellowFlower => "Yellow Flower",
            ItemType.BlueFlower => "Blue Flower",
            ItemType.OrangeFlower => "Orange Flower",
            ItemType.PurpleFlower => "Purple Flower",
            ItemType.PinkFlower => "Pink Flower",
            ItemType.MushroomBrown => "Brown Mushroom",
            ItemType.MushroomRed => "Red Mushroom",
            ItemType.IronOre => "Iron Ore",
            ItemType.IronBar => "Iron Bar",
            ItemType.IronAxe => "Iron Axe",
            ItemType.IronPickaxe => "Iron Pickaxe",
            ItemType.DiamondAxe => "Diamond Axe",
            ItemType.DiamondPickaxe => "Diamond Pickaxe",
            ItemType.GoldCoin => "Gold Coin",
            _ => item.ToString(),
        };
    }
}
