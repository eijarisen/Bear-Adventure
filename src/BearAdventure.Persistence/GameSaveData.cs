using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;

namespace BearAdventure.Persistence;

public sealed class GameSaveData
{
    public const int CurrentFormatVersion = 1;

    public int FormatVersion { get; set; } =
        CurrentFormatVersion;

    public int GeneratorVersion { get; set; } =
        BearAdventure.Domain.World.WorldSeed.GeneratorVersion;

    public string WorldSeed { get; set; } =
        string.Empty;

    public int CurrentIslandId { get; set; }

    public List<int> DiscoveredIslandIds { get; set; } =
        new() { 0 };

    public Dictionary<string, int> Inventory { get; set; } =
        new();

    public Dictionary<int, IslandSaveData> Islands { get; set; } =
        new();

    public static GameSaveData FromSession(
        string worldSeed,
        GameSessionState session)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(worldSeed);
        ArgumentNullException.ThrowIfNull(session);

        var result =
            new GameSaveData
            {
                WorldSeed =
                    worldSeed,
                CurrentIslandId =
                    session.CurrentIslandId,
                DiscoveredIslandIds =
                    session.DiscoveredIslandIds
                        .OrderBy(id => id)
                        .ToList(),
            };

        foreach ((ItemType item, int count)
                 in session.Inventory.Counts)
        {
            if (count > 0)
            {
                result.Inventory[item.ToString()] =
                    count;
            }
        }

        foreach ((int islandId, IslandDeltaState state)
                 in session.Islands)
        {
            bool hasPersistentState =
                state.HarvestedNaturalFeatureCount > 0
                || state.PlacedObjects.Count > 0
                || state.MinedUndergroundCells.Count > 0
                || state.LootedGeneratedChestIds.Count > 0
                || state.LeftBoatBuilt
                || state.RightBoatBuilt;

            if (!hasPersistentState)
            {
                continue;
            }

            var islandSave =
                new IslandSaveData
                {
                    HarvestedNaturalFeatureIds =
                        state.HarvestedNaturalFeatureIds
                            .OrderBy(id => id)
                            .ToList(),

                    NaturalRegrowthSeconds =
                        state.NaturalRegrowthSeconds
                            .ToDictionary(
                                pair => pair.Key,
                                pair => pair.Value),

                    LeftBoatBuilt =
                        state.LeftBoatBuilt,

                    RightBoatBuilt =
                        state.RightBoatBuilt,

                    MinedUndergroundCells =
                        state.MinedUndergroundCells
                            .Select(
                                cell =>
                                    new UndergroundCellSaveData
                                    {
                                        CellX =
                                            cell.CellX,
                                        LogicalLevel =
                                            cell.LogicalLevel,
                                    })
                            .ToList(),

                    LootedGeneratedChestIds =
                        state.LootedGeneratedChestIds
                            .OrderBy(id => id)
                            .ToList(),
                };

            foreach (PlacedObjectState placed
                     in state.PlacedObjects)
            {
                islandSave.PlacedObjects.Add(
                    new PlacedObjectSaveData
                    {
                        PlacementId =
                            placed.PlacementId,
                        Item =
                            placed.Item.ToString(),
                        CellX =
                            placed.CellX,
                        LogicalLevel =
                            placed.LogicalLevel,
                        Layer =
                            placed.Layer.ToString(),
                        GrowthSeconds =
                            placed.GrowthSeconds,
                        ProductionSeconds =
                            placed.ProductionSeconds,
                        StoredOutput =
                            placed.StoredOutput,
                    });
            }

            result.Islands[islandId] =
                islandSave;
        }

        return result;
    }

    public GameSessionState ToSession()
    {
        var inventory =
            new InventoryState();

        foreach ((string itemName, int count)
                 in Inventory)
        {
            if (count <= 0)
            {
                continue;
            }

            if (Enum.TryParse(
                itemName,
                ignoreCase: true,
                out ItemType item))
            {
                inventory.Set(item, count);
            }
        }

        var session =
            new GameSessionState(inventory)
            {
                CurrentIslandId =
                    CurrentIslandId,
            };

        session.ReplaceDiscoveredIslands(
            DiscoveredIslandIds);

        foreach ((int islandId, IslandSaveData islandData)
                 in Islands)
        {
            var state =
                new IslandDeltaState();

            state.ReplaceHarvestedNaturalFeatures(
                islandData.HarvestedNaturalFeatureIds);

            state.ReplaceNaturalRegrowth(
                islandData.NaturalRegrowthSeconds);

            var placedObjects =
                new List<PlacedObjectState>();

            foreach (PlacedObjectSaveData placed
                     in islandData.PlacedObjects)
            {
                if (!Enum.TryParse(
                    placed.Item,
                    ignoreCase: true,
                    out ItemType item))
                {
                    continue;
                }

                if (!Enum.TryParse(
                    placed.Layer,
                    ignoreCase: true,
                    out BuildLayer layer))
                {
                    layer =
                        BuildLayer.Solid;
                }

                if (!PlacementRules.IsPlaceable(item))
                {
                    continue;
                }

                var placedState =
                    new PlacedObjectState(
                        placed.PlacementId,
                        item,
                        placed.CellX,
                        placed.LogicalLevel,
                        layer)
                    {
                        GrowthSeconds =
                            Math.Max(
                                0.0,
                                placed.GrowthSeconds),
                        ProductionSeconds =
                            Math.Max(
                                0.0,
                                placed.ProductionSeconds),
                        StoredOutput =
                            Math.Max(
                                0,
                                placed.StoredOutput),
                    };

                placedObjects.Add(
                    placedState);
            }

            state.ReplacePlacedObjects(
                placedObjects);

            state.SetBoatBuilt(
                BoatSide.Left,
                islandData.LeftBoatBuilt);

            state.SetBoatBuilt(
                BoatSide.Right,
                islandData.RightBoatBuilt);

            state.ReplaceMinedUndergroundCells(
                islandData.MinedUndergroundCells
                    .Select(
                        cell =>
                            new UndergroundCell(
                                cell.CellX,
                                cell.LogicalLevel)));

            state.ReplaceLootedGeneratedChestIds(
                islandData.LootedGeneratedChestIds);

            session.SetIslandState(
                islandId,
                state);
        }

        return session;
    }
}

public sealed class IslandSaveData
{
    public List<int> HarvestedNaturalFeatureIds { get; set; } =
        new();

    public Dictionary<int, double> NaturalRegrowthSeconds { get; set; } =
        new();

    public List<PlacedObjectSaveData> PlacedObjects { get; set; } =
        new();

    public bool LeftBoatBuilt { get; set; }

    public bool RightBoatBuilt { get; set; }

    public List<UndergroundCellSaveData> MinedUndergroundCells { get; set; } =
        new();

    public List<int> LootedGeneratedChestIds { get; set; } =
        new();
}

public sealed class UndergroundCellSaveData
{
    public int CellX { get; set; }

    public int LogicalLevel { get; set; }
}

public sealed class PlacedObjectSaveData
{
    public int PlacementId { get; set; }

    public string Item { get; set; } =
        string.Empty;

    public int CellX { get; set; }

    public int LogicalLevel { get; set; }

    public string Layer { get; set; } =
        BuildLayer.Solid.ToString();

    public double GrowthSeconds { get; set; }

    public double ProductionSeconds { get; set; }

    public int StoredOutput { get; set; }
}
