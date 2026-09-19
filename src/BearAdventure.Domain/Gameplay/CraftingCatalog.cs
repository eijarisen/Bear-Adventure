namespace BearAdventure.Domain.Gameplay;

public static class CraftingCatalog
{
    public static IReadOnlyList<CraftingRecipe> All { get; } =
    [
        Recipe(
            "beehive",
            "Beehive",
            Cost((ItemType.Wood, 30)),
            Result((ItemType.Beehive, 1))),

        Recipe(
            "axe",
            "Axe",
            Cost(
                (ItemType.Wood, 5),
                (ItemType.Stone, 10)),
            Result((ItemType.Axe, 1))),

        Recipe(
            "pickaxe",
            "Pickaxe",
            Cost(
                (ItemType.Wood, 5),
                (ItemType.Stone, 15)),
            Result((ItemType.Pickaxe, 1))),

        Recipe(
            "forge",
            "Forge",
            Cost((ItemType.Stone, 20)),
            Result((ItemType.Forge, 1))),

        Recipe(
            "anvil",
            "Anvil",
            Cost((ItemType.IronBar, 10)),
            Result((ItemType.Anvil, 1))),

        Recipe(
            "chest",
            "Chest",
            Cost(
                (ItemType.Wood, 100),
                (ItemType.IronBar, 5)),
            Result((ItemType.Chest, 1))),

        Recipe(
            "cauldron",
            "Cauldron",
            Cost((ItemType.IronBar, 30)),
            Result((ItemType.Cauldron, 1))),
    ];

    public static CraftingRecipe? Find(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return All.FirstOrDefault(
            recipe =>
                string.Equals(
                    recipe.Id,
                    id,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static CraftingRecipe Recipe(
        string id,
        string displayName,
        IReadOnlyDictionary<ItemType, int> cost,
        IReadOnlyDictionary<ItemType, int> result)
    {
        return new CraftingRecipe(
            id,
            displayName,
            cost,
            result);
    }

    private static IReadOnlyDictionary<ItemType, int> Cost(
        params (ItemType Item, int Amount)[] entries)
    {
        return entries.ToDictionary(
            entry => entry.Item,
            entry => entry.Amount);
    }

    private static IReadOnlyDictionary<ItemType, int> Result(
        params (ItemType Item, int Amount)[] entries)
    {
        return entries.ToDictionary(
            entry => entry.Item,
            entry => entry.Amount);
    }
}
