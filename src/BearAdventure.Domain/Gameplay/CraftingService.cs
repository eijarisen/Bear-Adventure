namespace BearAdventure.Domain.Gameplay;

public static class CraftingService
{
    public static bool CanCraft(
        InventoryState inventory,
        CraftingRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(recipe);

        return recipe.Cost.All(
            pair =>
                inventory.Get(pair.Key) >= pair.Value);
    }

    public static bool TryCraft(
        InventoryState inventory,
        CraftingRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(recipe);

        if (!CanCraft(inventory, recipe))
        {
            return false;
        }

        foreach ((ItemType item, int amount) in recipe.Cost)
        {
            inventory.Add(item, -amount);
        }

        foreach ((ItemType item, int amount) in recipe.Result)
        {
            inventory.Add(item, amount);
        }

        return true;
    }

    public static string DescribeCost(CraftingRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        return string.Join(
            ", ",
            recipe.Cost.Select(
                pair =>
                    $"{pair.Value} " +
                    PlacementRules
                        .GetDisplayName(pair.Key)
                        .ToLowerInvariant()));
    }
}
