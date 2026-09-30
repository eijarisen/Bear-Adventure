namespace BearAdventure.Domain.Gameplay;

public static class CraftingService
{
    public static bool CanCraft(InventoryState inventory, CraftingRecipe recipe) =>
        InventoryTransactions.CanExchange(inventory, recipe.Cost, recipe.Result, 1, out _);
    public static bool TryCraft(InventoryState inventory, CraftingRecipe recipe) =>
        InventoryTransactions.TryExchange(inventory, recipe.Cost, recipe.Result, 1, out _);
    public static string DescribeCost(CraftingRecipe recipe) => string.Join(", ", recipe.Cost.Select(
        pair => $"{pair.Value} {PlacementRules.GetDisplayName(pair.Key).ToLowerInvariant()}"));
}
