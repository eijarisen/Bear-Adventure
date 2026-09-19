using BearAdventure.Domain.Gameplay;

namespace BearAdventure.Domain.World;

public sealed record GeneratedChestDefinition(
    int ChestId,
    int CellX,
    int LogicalLevel,
    IReadOnlyDictionary<ItemType, int> Loot);
