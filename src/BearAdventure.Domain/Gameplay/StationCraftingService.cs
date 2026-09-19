namespace BearAdventure.Domain.Gameplay;

public static class StationCraftingService
{
    private static readonly IReadOnlyList<StationRecipeDefinition> ForgeRecipes =
    [
        new(
            "iron-bar",
            "Iron Bar",
            "1 iron ore + 1 wood"),
    ];

    private static readonly IReadOnlyList<StationRecipeDefinition> AnvilRecipes =
    [
        new(
            "iron-axe",
            "Iron Axe",
            "1 axe + 10 iron bars + 5 wood"),

        new(
            "iron-pickaxe",
            "Iron Pickaxe",
            "1 pickaxe + 15 iron bars + 5 wood"),
    ];

    private static readonly IReadOnlyList<StationRecipeDefinition> CauldronRecipes =
    [
        new(
            "juice",
            "Juice",
            "1 cactus + 1 flower"),

        new(
            "soup",
            "Soup",
            "1 brown mushroom + 1 flower"),
    ];

    public static IReadOnlyList<StationRecipeDefinition> GetRecipes(
        ItemType station)
    {
        return station switch
        {
            ItemType.Forge => ForgeRecipes,
            ItemType.Anvil => AnvilRecipes,
            ItemType.Cauldron => CauldronRecipes,
            _ => Array.Empty<StationRecipeDefinition>(),
        };
    }

    public static bool CanCraft(
        InventoryState inventory,
        ItemType station,
        string recipeId)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeId);

        return (station, recipeId) switch
        {
            (ItemType.Forge, "iron-bar") =>
                inventory.Has(ItemType.IronOre)
                && inventory.Has(ItemType.Wood),

            (ItemType.Anvil, "iron-axe") =>
                inventory.Has(ItemType.Axe)
                && inventory.Has(ItemType.IronBar, 10)
                && inventory.Has(ItemType.Wood, 5),

            (ItemType.Anvil, "iron-pickaxe") =>
                inventory.Has(ItemType.Pickaxe)
                && inventory.Has(ItemType.IronBar, 15)
                && inventory.Has(ItemType.Wood, 5),

            (ItemType.Cauldron, "juice") =>
                inventory.Has(ItemType.Cactus)
                && HasAnyFlower(inventory),

            (ItemType.Cauldron, "soup") =>
                inventory.Has(ItemType.MushroomBrown)
                && HasAnyFlower(inventory),

            _ => false,
        };
    }

    public static bool TryCraft(
        InventoryState inventory,
        ItemType station,
        string recipeId)
    {
        if (!CanCraft(
            inventory,
            station,
            recipeId))
        {
            return false;
        }

        switch (station, recipeId)
        {
            case (ItemType.Forge, "iron-bar"):
                inventory.Add(ItemType.IronOre, -1);
                inventory.Add(ItemType.Wood, -1);
                inventory.Add(ItemType.IronBar, 1);
                return true;

            case (ItemType.Anvil, "iron-axe"):
                inventory.Add(ItemType.Axe, -1);
                inventory.Add(ItemType.IronBar, -10);
                inventory.Add(ItemType.Wood, -5);
                inventory.Add(ItemType.IronAxe, 1);
                return true;

            case (ItemType.Anvil, "iron-pickaxe"):
                inventory.Add(ItemType.Pickaxe, -1);
                inventory.Add(ItemType.IronBar, -15);
                inventory.Add(ItemType.Wood, -5);
                inventory.Add(ItemType.IronPickaxe, 1);
                return true;

            case (ItemType.Cauldron, "juice"):
                inventory.Add(ItemType.Cactus, -1);
                ConsumeAnyFlower(inventory);
                inventory.Add(ItemType.Juice, 1);
                return true;

            case (ItemType.Cauldron, "soup"):
                inventory.Add(ItemType.MushroomBrown, -1);
                ConsumeAnyFlower(inventory);
                inventory.Add(ItemType.Soup, 1);
                return true;

            default:
                return false;
        }
    }

    private static bool HasAnyFlower(
        InventoryState inventory)
    {
        return FlowerItems.Any(
            flower => inventory.Has(flower));
    }

    private static void ConsumeAnyFlower(
        InventoryState inventory)
    {
        foreach (ItemType flower in FlowerItems)
        {
            if (!inventory.Has(flower))
            {
                continue;
            }

            inventory.Add(flower, -1);
            return;
        }

        throw new InvalidOperationException(
            "A flower was expected but none was available.");
    }

    private static ItemType[] FlowerItems { get; } =
    [
        ItemType.RedFlower,
        ItemType.YellowFlower,
        ItemType.BlueFlower,
        ItemType.OrangeFlower,
        ItemType.PurpleFlower,
        ItemType.PinkFlower,
    ];
}
