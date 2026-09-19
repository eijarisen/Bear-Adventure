namespace BearAdventure.Domain.Gameplay;

public sealed record CraftingRecipe(
    string Id,
    string DisplayName,
    IReadOnlyDictionary<ItemType, int> Cost,
    IReadOnlyDictionary<ItemType, int> Result);
