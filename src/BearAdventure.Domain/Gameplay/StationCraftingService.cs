namespace BearAdventure.Domain.Gameplay;

public static class StationCraftingService
{
    public static IReadOnlyList<StationRecipeDefinition> GetRecipes(ItemType station) => station switch
    {
        ItemType.Forge => new[] { new StationRecipeDefinition("iron-bar", "Iron Bar", "1 iron ore + 1 wood") },
        ItemType.Anvil => new[] {
            new StationRecipeDefinition("iron-axe", "Iron Axe", "1 axe + 10 iron bars + 5 wood"),
            new StationRecipeDefinition("iron-pickaxe", "Iron Pickaxe", "1 pickaxe + 15 iron bars + 5 wood") },
        ItemType.Cauldron => new[] {
            new StationRecipeDefinition("juice", "Juice", "1 cactus + 1 selected flower"),
            new StationRecipeDefinition("soup", "Soup", "1 brown mushroom + 1 selected flower"),
            new StationRecipeDefinition("red-soup", "Red Mushroom Soup", "1 red mushroom + 1 selected flower") },
        _ => Array.Empty<StationRecipeDefinition>()
    };
    public static CraftingRecipe? Resolve(ItemType station, string id, ItemType? flower = null)
    {
        Dictionary<ItemType,int> Cost(params (ItemType Item, int Count)[] entries) =>
            entries.ToDictionary(e => e.Item, e => e.Count);
        CraftingRecipe Recipe(string name, ItemType output, Dictionary<ItemType,int> cost) =>
            new(id, name, cost, new Dictionary<ItemType,int> { [output] = 1 });
        return (station, id) switch
        {
            (ItemType.Forge, "iron-bar") => Recipe("Iron Bar", ItemType.IronBar, Cost((ItemType.IronOre,1),(ItemType.Wood,1))),
            (ItemType.Anvil, "iron-axe") => Recipe("Iron Axe", ItemType.IronAxe,
                Cost((ItemType.Axe,1),(ItemType.IronBar,10),(ItemType.Wood,5))),
            (ItemType.Anvil, "iron-pickaxe") => Recipe("Iron Pickaxe", ItemType.IronPickaxe,
                Cost((ItemType.Pickaxe,1),(ItemType.IronBar,15),(ItemType.Wood,5))),
            (ItemType.Cauldron, "juice") when flower.HasValue && PlacementRules.IsFlower(flower.Value) =>
                Recipe("Juice", ItemType.Juice, Cost((ItemType.Cactus,1),(flower.Value,1))),
            (ItemType.Cauldron, "soup") when flower.HasValue && PlacementRules.IsFlower(flower.Value) =>
                Recipe("Soup", ItemType.Soup, Cost((ItemType.MushroomBrown,1),(flower.Value,1))),
            (ItemType.Cauldron, "red-soup") when flower.HasValue && PlacementRules.IsFlower(flower.Value) =>
                Recipe("Red Mushroom Soup", ItemType.Soup, Cost((ItemType.MushroomRed,1),(flower.Value,1))),
            _ => null
        };
    }
    public static bool CanCraft(InventoryState inventory, ItemType station, string recipeId, ItemType? flower = null)
    {
        var recipe = Resolve(station, recipeId, flower);
        return recipe is not null && CraftingService.CanCraft(inventory, recipe);
    }
    public static bool TryCraft(InventoryState inventory, ItemType station, string recipeId, ItemType? flower = null)
    {
        var recipe = Resolve(station, recipeId, flower);
        return recipe is not null && CraftingService.TryCraft(inventory, recipe);
    }
}
