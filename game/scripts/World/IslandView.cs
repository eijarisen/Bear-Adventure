using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.Simulation;
using BearAdventure.Domain.World;
using BearAdventure.Rendering;
using Godot;

namespace BearAdventure.World;

public partial class IslandView : Node2D
{
    public const float CellSize = 48.0f;
    public const int WaterMarginCells = 7;
    public const int BoatConstructionShoreCells = 2;

    private readonly IslandDefinition _island;
    private readonly IslandDeltaState _state;
    private readonly BiomePalette _palette;

    private int? _highlightedFeatureId;
    private float _highlightedFeatureProgress;
    private int? _highlightedPlacementId;
    private float _highlightedPlacementProgress;

    private StaticBody2D? _terrainCollisionBody;
    private StaticBody2D? _placedCollisionBody;
    private StaticBody2D? _boatCollisionBody;

    private UndergroundCell? _highlightedMineCell;
    private float _highlightedMineProgress;

    private ItemType? _previewItem;
    private int _previewCellX;
    private int _previewLogicalLevel;
    private BuildLayer _previewLayer;
    private bool _previewValid;

    public IslandView(
        IslandDefinition island,
        IslandDeltaState state)
    {
        _island =
            island
            ?? throw new ArgumentNullException(nameof(island));
        _state =
            state
            ?? throw new ArgumentNullException(nameof(state));
        _palette = BiomePalette.For(island.Biome);
    }

    public IslandDefinition Definition => _island;

    public IslandDeltaState State => _state;

    public float LandLeftX => WaterMarginCells * CellSize;

    public float LandRightX =>
        LandLeftX + _island.WidthCells * CellSize;

    public float WorldWidth =>
        LandRightX + WaterMarginCells * CellSize;

    public float WorldTop =>
        -IslandGenerationSettings.HighestLogicalLevel * CellSize;

    public float WorldBottom =>
        -IslandGenerationSettings.DeepestLogicalLevel * CellSize;

    public Color SkyColor => _palette.Sky;

    public int RemainingNaturalFeatureCount =>
        _island.NaturalFeatures.Count(
            feature =>
                !_state.IsNaturalFeatureHarvested(
                    feature.FeatureId));

    public override void _Ready()
    {
        _terrainCollisionBody =
            new StaticBody2D
            {
                Name = "TerrainCollision",
            };
        AddChild(_terrainCollisionBody);
        RebuildTerrainCollision();

        _placedCollisionBody =
            new StaticBody2D
            {
                Name = "PlayerBuiltCollision",
            };
        AddChild(_placedCollisionBody);
        RebuildPlacedCollision();

        _boatCollisionBody =
            new StaticBody2D
            {
                Name = "BoatCollision",
                CollisionLayer = 1,
                CollisionMask = 1,
            };
        AddChild(_boatCollisionBody);
        RebuildBoatCollision();

        ZIndex = -10;
        QueueRedraw();
    }

    public Vector2 GetSuggestedSpawnPosition()
    {
        int cellX = _island.SuggestedSpawnCell;
        float x =
            LandLeftX + (cellX + 0.5f) * CellSize;
        float surfaceY =
            LevelToWorldY(
                _island.SurfaceLevels[cellX]);

        return new Vector2(
            x,
            surfaceY - 31.0f);
    }

    public Vector2 GetBoatArrivalSpawn(BoatSide side)
    {
        int cellX =
            side == BoatSide.Left
                ? BoatConstructionShoreCells + 1
                : _island.WidthCells
                    - BoatConstructionShoreCells
                    - 2;

        cellX =
            Math.Clamp(
                cellX,
                0,
                _island.WidthCells - 1);

        float x =
            LandLeftX
            + (cellX + 0.5f) * CellSize;

        float surfaceY =
            LevelToWorldY(
                _island.SurfaceLevels[cellX]);

        return new Vector2(
            x,
            surfaceY - 31.0f);
    }

    public BoatSide? FindNearbyBoatSite(
        Vector2 worldPosition,
        float maximumDistance)
    {
        float maximumDistanceSquared =
            maximumDistance * maximumDistance;

        foreach (BoatSide side in Enum.GetValues<BoatSide>())
        {
            Vector2 center =
                GetBoatSiteCenter(side);

            if (worldPosition.DistanceSquaredTo(center)
                <= maximumDistanceSquared)
            {
                return side;
            }
        }

        return null;
    }

    public bool IsBoatBuilt(BoatSide side)
    {
        return _state.IsBoatBuilt(side);
    }

    public bool BuildBoat(BoatSide side)
    {
        bool built =
            _state.BuildBoat(side);

        if (built)
        {
            RebuildBoatCollision();
            QueueRedraw();
        }

        return built;
    }

    public Vector2 GetBoatSiteCenter(BoatSide side)
    {
        int cellX =
            side == BoatSide.Left
                ? 0
                : _island.WidthCells - 1;

        float x =
            LandLeftX
            + (cellX + 0.5f) * CellSize;

        float y =
            LevelToWorldY(
                _island.SurfaceLevels[cellX]);

        return new Vector2(x, y);
    }

    private Vector2 GetBoatWaterCenter(
        BoatSide side)
    {
        float waterOffset =
            CellSize * 0.88f;

        float x =
            side == BoatSide.Left
                ? LandLeftX - waterOffset
                : LandRightX + waterOffset;

        return new Vector2(
            x,
            4.0f);
    }

    public float LevelToWorldY(int logicalLevel)
    {
        return -logicalLevel * CellSize;
    }

    public Vector2 GetFeatureWorldPosition(
        NaturalFeatureSpawn feature)
    {
        return new Vector2(
            LandLeftX
                + (feature.CellX + 0.5f) * CellSize,
            LevelToWorldY(feature.SurfaceLevel));
    }

    public NaturalFeatureSpawn? FindNearestHarvestable(
        Vector2 worldPosition,
        float maximumDistance)
    {
        NaturalFeatureSpawn? nearest = null;
        float bestDistanceSquared =
            maximumDistance * maximumDistance;

        foreach (NaturalFeatureSpawn feature
                 in _island.NaturalFeatures)
        {
            if (_state.IsNaturalFeatureHarvested(
                feature.FeatureId))
            {
                continue;
            }

            Vector2 featurePosition =
                GetFeatureWorldPosition(feature);

            float distanceSquared =
                worldPosition.DistanceSquaredTo(
                    featurePosition);

            if (distanceSquared <= bestDistanceSquared)
            {
                nearest = feature;
                bestDistanceSquared = distanceSquared;
            }
        }

        return nearest;
    }

    public PlacedObjectState? FindNearestPlacedHarvestable(
        Vector2 worldPosition,
        float maximumDistance)
    {
        PlacedObjectState? nearest = null;
        float bestDistanceSquared =
            maximumDistance * maximumDistance;

        foreach (PlacedObjectState placed
                 in _state.PlacedObjects)
        {
            if (!PlacedHarvestRules.IsHarvestable(placed))
            {
                continue;
            }

            Vector2 center =
                GetBuildCellCenter(
                    placed.CellX,
                    placed.LogicalLevel);

            float distanceSquared =
                worldPosition.DistanceSquaredTo(center);

            if (distanceSquared <= bestDistanceSquared)
            {
                nearest = placed;
                bestDistanceSquared =
                    distanceSquared;
            }
        }

        return nearest;
    }

    public PlacedObjectState? FindNearestInteractive(
        Vector2 worldPosition,
        float maximumDistance)
    {
        PlacedObjectState? nearest = null;
        float bestDistanceSquared =
            maximumDistance * maximumDistance;

        foreach (PlacedObjectState placed
                 in _state.PlacedObjects)
        {
            if (placed.Item is not (
                ItemType.Beehive
                or ItemType.Forge
                or ItemType.Anvil
                or ItemType.Cauldron))
            {
                continue;
            }

            Vector2 center =
                GetBuildCellCenter(
                    placed.CellX,
                    placed.LogicalLevel);

            float distanceSquared =
                worldPosition.DistanceSquaredTo(
                    center);

            if (distanceSquared <= bestDistanceSquared)
            {
                nearest = placed;
                bestDistanceSquared =
                    distanceSquared;
            }
        }

        return nearest;
    }

    public int CollectHiveHoney(int placementId)
    {
        PlacedObjectState? hive =
            _state.PlacedObjects.FirstOrDefault(
                placed =>
                    placed.PlacementId == placementId
                    && placed.Item == ItemType.Beehive);

        if (hive is null
            || hive.StoredOutput <= 0)
        {
            return 0;
        }

        int collected =
            hive.StoredOutput;

        hive.StoredOutput = 0;
        QueueRedraw();
        return collected;
    }

    public bool TryHarvestFeature(
        int featureId,
        out NaturalFeatureSpawn harvestedFeature)
    {
        foreach (NaturalFeatureSpawn feature
                 in _island.NaturalFeatures)
        {
            if (feature.FeatureId != featureId)
            {
                continue;
            }

            if (_state.IsNaturalFeatureHarvested(featureId))
            {
                break;
            }

            if (!_state.MarkNaturalFeatureHarvested(
                featureId,
                NaturalRegrowthRules.GetRegrowSeconds(
                    feature.Kind)))
            {
                break;
            }

            harvestedFeature = feature;
            _highlightedFeatureId = null;
            _highlightedFeatureProgress = 0.0f;
            QueueRedraw();
            return true;
        }

        harvestedFeature = default;
        return false;
    }

    public bool TryHarvestPlacedObject(
        int placementId,
        out PlacedObjectState harvested)
    {
        PlacedObjectState? placed =
            _state.PlacedObjects.FirstOrDefault(
                item =>
                    item.PlacementId == placementId);

        if (placed is null
            || !PlacedHarvestRules.IsHarvestable(placed))
        {
            harvested = null!;
            return false;
        }

        harvested = placed;

        if (!_state.RemovePlacedObject(placementId))
        {
            harvested = null!;
            return false;
        }

        _highlightedPlacementId = null;
        _highlightedPlacementProgress = 0.0f;

        RebuildPlacedCollision();
        QueueRedraw();
        return true;
    }

    public void SetPlacedHarvestIndicator(
        int? placementId,
        float progress)
    {
        progress =
            Mathf.Clamp(
                progress,
                0.0f,
                1.0f);

        bool changed =
            _highlightedPlacementId != placementId
            || !Mathf.IsEqualApprox(
                _highlightedPlacementProgress,
                progress);

        _highlightedPlacementId =
            placementId;
        _highlightedPlacementProgress =
            progress;

        if (changed)
        {
            QueueRedraw();
        }
    }

    public void SetHarvestIndicator(
        int? featureId,
        float progress)
    {
        progress = Mathf.Clamp(progress, 0.0f, 1.0f);

        bool changed =
            _highlightedFeatureId != featureId
            || !Mathf.IsEqualApprox(
                _highlightedFeatureProgress,
                progress);

        _highlightedFeatureId = featureId;
        _highlightedFeatureProgress = progress;

        if (changed)
        {
            QueueRedraw();
        }
    }


    public bool IsInMineShaft(
        Vector2 worldPosition)
    {
        int shaftRight =
            _island.MineShaftLeftCell
            + _island.MineShaftWidthCells;

        float left =
            LandLeftX
            + _island.MineShaftLeftCell
                * CellSize;

        float right =
            LandLeftX
            + shaftRight
                * CellSize;

        int highestSurface =
            Enumerable
                .Range(
                    _island.MineShaftLeftCell,
                    _island.MineShaftWidthCells)
                .Select(
                    x =>
                        _island.SurfaceLevels[x])
                .Max();

        float top =
            LevelToWorldY(
                highestSurface)
            - CellSize;

        float bottom =
            WorldBottom
            + CellSize;

        return worldPosition.X
                >= left - 8.0f
            && worldPosition.X
                <= right + 8.0f
            && worldPosition.Y
                >= top
            && worldPosition.Y
                <= bottom;
    }

    public UndergroundCell? FindHoveredMineableCell(
        Vector2 mouseWorldPosition,
        Vector2 playerWorldPosition,
        float maximumDistance)
    {
        if (!TryWorldToTerrainCell(
            mouseWorldPosition,
            out int cellX,
            out int logicalLevel))
        {
            return null;
        }

        var cell =
            new UndergroundCell(
                cellX,
                logicalLevel);

        if (!IsSolidTerrainCell(cell)
            || !IsMineCellExposed(cell))
        {
            return null;
        }

        Vector2 center =
            GetUndergroundCellCenter(
                cell);

        if (playerWorldPosition.DistanceSquaredTo(center)
            > maximumDistance * maximumDistance)
        {
            return null;
        }

        return cell;
    }

    public Vector2 GetUndergroundCellCenter(
        UndergroundCell cell)
    {
        return GetTerrainCellRect(
            cell.CellX,
            cell.LogicalLevel)
            .GetCenter();
    }

    public bool TryMineUndergroundCell(
        UndergroundCell cell,
        out UndergroundOreSpawn? ore)
    {
        ore = null;

        if (!IsSolidTerrainCell(cell)
            || !IsMineCellExposed(cell)
            || !_state.MarkUndergroundCellMined(
                cell))
        {
            return false;
        }

        if (_island.UndergroundOres.TryGetValue(
            cell,
            out UndergroundOreSpawn foundOre))
        {
            ore =
                foundOre;
        }

        _highlightedMineCell = null;
        _highlightedMineProgress = 0.0f;

        RebuildTerrainCollision();
        QueueRedraw();
        return true;
    }

    public void SetMineIndicator(
        UndergroundCell? cell,
        float progress)
    {
        progress =
            Mathf.Clamp(
                progress,
                0.0f,
                1.0f);

        bool changed =
            _highlightedMineCell != cell
            || !Mathf.IsEqualApprox(
                _highlightedMineProgress,
                progress);

        _highlightedMineCell =
            cell;

        _highlightedMineProgress =
            progress;

        if (changed)
        {
            QueueRedraw();
        }
    }

    public GeneratedChestDefinition? FindNearestGeneratedChest(
        Vector2 worldPosition,
        float maximumDistance)
    {
        GeneratedChestDefinition? nearest =
            null;

        float bestDistanceSquared =
            maximumDistance
            * maximumDistance;

        foreach (GeneratedStructureDefinition structure
                 in _island.GeneratedStructures)
        {
            GeneratedChestDefinition chest =
                structure.Chest;

            Vector2 center =
                GetBuildCellCenter(
                    chest.CellX,
                    chest.LogicalLevel);

            float distanceSquared =
                worldPosition
                    .DistanceSquaredTo(
                        center);

            if (distanceSquared
                <= bestDistanceSquared)
            {
                nearest =
                    chest;

                bestDistanceSquared =
                    distanceSquared;
            }
        }

        return nearest;
    }

    public bool IsGeneratedChestLooted(
        int chestId)
    {
        return _state
            .IsGeneratedChestLooted(
                chestId);
    }

    public bool TryLootGeneratedChest(
        int chestId,
        out IReadOnlyList<HarvestReward> rewards)
    {
        GeneratedChestDefinition? chest =
            _island.GeneratedStructures
                .Select(
                    structure =>
                        structure.Chest)
                .FirstOrDefault(
                    item =>
                        item.ChestId
                        == chestId);

        if (chest is null
            || _state.IsGeneratedChestLooted(
                chestId)
            || !_state.MarkGeneratedChestLooted(
                chestId))
        {
            rewards =
                Array.Empty<HarvestReward>();
            return false;
        }

        rewards =
            chest.Loot
                .Select(
                    pair =>
                        new HarvestReward(
                            pair.Key,
                            pair.Value))
                .ToArray();

        QueueRedraw();
        return true;
    }

    public bool TryWorldToBuildCell(
        Vector2 worldPosition,
        out int cellX,
        out int logicalLevel)
    {
        float localX =
            worldPosition.X - LandLeftX;

        cellX =
            Mathf.FloorToInt(
                localX / CellSize);

        logicalLevel =
            Mathf.FloorToInt(
                -worldPosition.Y / CellSize) + 1;

        return cellX >= 0
            && cellX < _island.WidthCells
            && logicalLevel
                <= IslandGenerationSettings.HighestLogicalLevel
            && logicalLevel
                > IslandGenerationSettings.DeepestLogicalLevel;
    }

    public Rect2 GetBuildCellRect(
        int cellX,
        int logicalLevel)
    {
        return new Rect2(
            LandLeftX + cellX * CellSize,
            LevelToWorldY(logicalLevel),
            CellSize,
            CellSize);
    }

    public Vector2 GetBuildCellCenter(
        int cellX,
        int logicalLevel)
    {
        return GetBuildCellRect(
            cellX,
            logicalLevel).GetCenter();
    }

    public bool CanPlaceObject(
        ItemType item,
        int cellX,
        int logicalLevel,
        BuildLayer layer,
        Rect2 playerRect,
        out string reason)
    {
        reason = string.Empty;

        if (!PlacementRules.IsPlaceable(item))
        {
            reason = "That item cannot be placed.";
            return false;
        }

        if (cellX < 0
            || cellX >= _island.WidthCells)
        {
            reason = "Build inside the island.";
            return false;
        }

        if (cellX < BoatConstructionShoreCells
            || cellX >= _island.WidthCells
                - BoatConstructionShoreCells)
        {
            reason = "The shoreline boat spot must stay clear.";
            return false;
        }

        if (logicalLevel
            <= _island.SurfaceLevels[cellX])
        {
            reason = "That cell is inside terrain.";
            return false;
        }

        if (logicalLevel
            > IslandGenerationSettings.HighestLogicalLevel)
        {
            reason = "That cell is above the build limit.";
            return false;
        }

        bool background =
            PlacementRules.IsBlock(item)
            && layer == BuildLayer.Background;

        bool occupied =
            _state.PlacedObjects.Any(
                placed =>
                    placed.CellX == cellX
                    && placed.LogicalLevel == logicalLevel
                    && (background
                        ? placed.Layer == BuildLayer.Background
                        : placed.Layer != BuildLayer.Background));

        if (occupied)
        {
            reason = "That cell is already occupied.";
            return false;
        }

        bool naturalFeatureOccupiesSurfaceCell =
            logicalLevel
                == _island.SurfaceLevels[cellX] + 1
            && _island.NaturalFeatures.Any(
                feature =>
                    feature.CellX == cellX
                    && !_state.IsNaturalFeatureHarvested(
                        feature.FeatureId));

        if (!background
            && naturalFeatureOccupiesSurfaceCell)
        {
            reason = "Harvest the existing resource first.";
            return false;
        }

        bool generatedChestOccupiesCell =
            _island.GeneratedStructures.Any(
                structure =>
                    structure.Chest.CellX
                        == cellX
                    && structure.Chest.LogicalLevel
                        == logicalLevel);

        if (generatedChestOccupiesCell)
        {
            reason =
                "A generated chest occupies that cell.";
            return false;
        }

        Rect2 placementRect =
            GetBuildCellRect(
                cellX,
                logicalLevel);

        if (PlacementRules.IsSolid(item, layer)
            && placementRect.Intersects(playerRect))
        {
            reason = "Cannot build a solid object on the bear.";
            return false;
        }

        bool supported =
            HasFloorSupport(
                cellX,
                logicalLevel);

        if (PlacementRules.IsBlock(item))
        {
            supported = supported
                || HasAdjacentBlockSupport(
                    cellX,
                    logicalLevel,
                    layer);
        }

        if (!supported)
        {
            reason = "The object needs ground or a connected block.";
            return false;
        }

        return true;
    }

    public PlacedObjectState AddPlacedObject(
        ItemType item,
        int cellX,
        int logicalLevel,
        BuildLayer layer)
    {
        PlacedObjectState placed =
            _state.AddPlacedObject(
                item,
                cellX,
                logicalLevel,
                layer);

        RebuildPlacedCollision();
        QueueRedraw();
        return placed;
    }

    public PlacedObjectState? FindPlacedObjectAt(
        int cellX,
        int logicalLevel)
    {
        PlacedObjectState? foreground =
            _state.PlacedObjects.FirstOrDefault(
                placed =>
                    placed.CellX == cellX
                    && placed.LogicalLevel == logicalLevel
                    && placed.Layer != BuildLayer.Background);

        if (foreground is not null)
        {
            return foreground;
        }

        return _state.PlacedObjects.FirstOrDefault(
            placed =>
                placed.CellX == cellX
                && placed.LogicalLevel == logicalLevel);
    }

    public bool RemovePlacedObject(
        int placementId,
        out ItemType returnedItem)
    {
        PlacedObjectState? placed =
            _state.PlacedObjects.FirstOrDefault(
                item =>
                    item.PlacementId == placementId);

        if (placed is null)
        {
            returnedItem = default;
            return false;
        }

        returnedItem = placed.Item;

        if (!_state.RemovePlacedObject(placementId))
        {
            return false;
        }

        RebuildPlacedCollision();
        QueueRedraw();
        return true;
    }

    public void SetBuildPreview(
        ItemType? item,
        int cellX,
        int logicalLevel,
        BuildLayer layer,
        bool valid)
    {
        bool changed =
            _previewItem != item
            || _previewCellX != cellX
            || _previewLogicalLevel != logicalLevel
            || _previewLayer != layer
            || _previewValid != valid;

        _previewItem = item;
        _previewCellX = cellX;
        _previewLogicalLevel = logicalLevel;
        _previewLayer = layer;
        _previewValid = valid;

        if (changed)
        {
            QueueRedraw();
        }
    }

    public void ClearBuildPreview()
    {
        if (_previewItem is null)
        {
            return;
        }

        _previewItem = null;
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawOcean();
        DrawUndergroundBackdrop();
        DrawTerrain();
        DrawUndergroundOres();
        DrawMineLadder();
        DrawBoatSites();
        DrawGeneratedStructures();
        DrawPlacedObjects(backgroundOnly: true);
        DrawNaturalFeatures();
        DrawGeneratedChests();
        DrawPlacedObjects(backgroundOnly: false);
        DrawMineIndicator();
        DrawBuildPreview();
    }

    private void RebuildTerrainCollision()
    {
        if (_terrainCollisionBody is null)
        {
            return;
        }

        foreach (Node child
                 in _terrainCollisionBody.GetChildren())
        {
            _terrainCollisionBody.RemoveChild(
                child);
            child.QueueFree();
        }

        int highestSurface =
            _island.SurfaceLevels.Max();

        for (int logicalLevel = highestSurface;
             logicalLevel
                >= IslandGenerationSettings.DeepestLogicalLevel;
             logicalLevel--)
        {
            int runStart = -1;

            for (int cellX = 0;
                 cellX <= _island.WidthCells;
                 cellX++)
            {
                bool solid =
                    cellX < _island.WidthCells
                    && IsSolidTerrainCell(
                        new UndergroundCell(
                            cellX,
                            logicalLevel));

                if (solid
                    && runStart < 0)
                {
                    runStart =
                        cellX;
                }

                if ((!solid
                        || cellX
                            == _island.WidthCells)
                    && runStart >= 0)
                {
                    int runEndExclusive =
                        cellX;

                    AddTerrainCollisionRun(
                        runStart,
                        runEndExclusive
                            - runStart,
                        logicalLevel);

                    runStart =
                        -1;
                }
            }
        }
    }

    private void AddTerrainCollisionRun(
        int startCell,
        int lengthCells,
        int logicalLevel)
    {
        if (_terrainCollisionBody is null
            || lengthCells <= 0)
        {
            return;
        }

        Rect2 rect =
            new(
                LandLeftX
                    + startCell
                        * CellSize,
                LevelToWorldY(
                    logicalLevel),
                lengthCells
                    * CellSize,
                CellSize);

        var shape =
            new RectangleShape2D
            {
                Size =
                    rect.Size,
            };

        _terrainCollisionBody.AddChild(
            new CollisionShape2D
            {
                Shape =
                    shape,
                Position =
                    rect.GetCenter(),
            });
    }

    private bool IsSolidTerrainCell(
        UndergroundCell cell)
    {
        if (cell.CellX < 0
            || cell.CellX
                >= _island.WidthCells
            || cell.LogicalLevel
                < IslandGenerationSettings.DeepestLogicalLevel
            || cell.LogicalLevel
                > _island.SurfaceLevels[cell.CellX])
        {
            return false;
        }

        if (_island.UndergroundOpenCells.Contains(
                cell)
            || _state.IsUndergroundCellMined(
                cell))
        {
            return false;
        }

        return true;
    }

    private bool IsMineCellExposed(
        UndergroundCell cell)
    {
        UndergroundCell[] neighbors =
        [
            new(
                cell.CellX - 1,
                cell.LogicalLevel),
            new(
                cell.CellX + 1,
                cell.LogicalLevel),
            new(
                cell.CellX,
                cell.LogicalLevel - 1),
            new(
                cell.CellX,
                cell.LogicalLevel + 1),
        ];

        return neighbors.Any(
            neighbor =>
                !IsSolidTerrainCell(
                    neighbor));
    }

    private Rect2 GetTerrainCellRect(
        int cellX,
        int logicalLevel)
    {
        return new Rect2(
            LandLeftX
                + cellX * CellSize,
            LevelToWorldY(
                logicalLevel),
            CellSize,
            CellSize);
    }

    private bool TryWorldToTerrainCell(
        Vector2 worldPosition,
        out int cellX,
        out int logicalLevel)
    {
        cellX =
            Mathf.FloorToInt(
                (worldPosition.X
                    - LandLeftX)
                / CellSize);

        logicalLevel =
            Mathf.FloorToInt(
                -worldPosition.Y
                / CellSize)
            + 1;

        if (cellX < 0
            || cellX >= _island.WidthCells
            || logicalLevel
                < IslandGenerationSettings.DeepestLogicalLevel
            || logicalLevel
                > IslandGenerationSettings.HighestLogicalLevel)
        {
            return false;
        }

        return logicalLevel
            <= _island.SurfaceLevels[cellX];
    }

    private void RebuildBoatCollision()
    {
        if (_boatCollisionBody is null)
        {
            return;
        }

        foreach (Node child
                 in _boatCollisionBody.GetChildren())
        {
            _boatCollisionBody.RemoveChild(
                child);

            child.QueueFree();
        }

        foreach (BoatSide side
                 in Enum.GetValues<BoatSide>())
        {
            if (!_state.IsBoatBuilt(side))
            {
                continue;
            }

            Vector2 center =
                GetBoatWaterCenter(side);

            var shape =
                new RectangleShape2D
                {
                    Size =
                        new Vector2(
                            96.0f,
                            18.0f),
                };

            _boatCollisionBody.AddChild(
                new CollisionShape2D
                {
                    Shape =
                        shape,
                    Position =
                        new Vector2(
                            center.X,
                            center.Y + 16.0f),
                });
        }
    }

    private void RebuildPlacedCollision()
    {
        if (_placedCollisionBody is null)
        {
            return;
        }

        foreach (Node child in _placedCollisionBody.GetChildren())
        {
            child.QueueFree();
        }

        foreach (PlacedObjectState placed in _state.PlacedObjects)
        {
            if (!PlacementRules.IsSolid(
                placed.Item,
                placed.Layer))
            {
                continue;
            }

            Rect2 rect =
                GetBuildCellRect(
                    placed.CellX,
                    placed.LogicalLevel);

            var shape =
                new RectangleShape2D
                {
                    Size = rect.Size,
                };

            _placedCollisionBody.AddChild(
                new CollisionShape2D
                {
                    Shape = shape,
                    Position = rect.GetCenter(),
                });
        }
    }

    private bool HasFloorSupport(
        int cellX,
        int logicalLevel)
    {
        if (logicalLevel
            == _island.SurfaceLevels[cellX] + 1)
        {
            return true;
        }

        return _state.PlacedObjects.Any(
            placed =>
                placed.CellX == cellX
                && placed.LogicalLevel
                    == logicalLevel - 1
                && PlacementRules.IsSolid(
                    placed.Item,
                    placed.Layer));
    }

    private bool HasAdjacentBlockSupport(
        int cellX,
        int logicalLevel,
        BuildLayer layer)
    {
        foreach (PlacedObjectState placed in _state.PlacedObjects)
        {
            if (!PlacementRules.IsBlock(placed.Item))
            {
                continue;
            }

            if (layer == BuildLayer.Solid
                && placed.Layer != BuildLayer.Solid)
            {
                continue;
            }

            int dx =
                Math.Abs(placed.CellX - cellX);
            int dy =
                Math.Abs(
                    placed.LogicalLevel
                    - logicalLevel);

            if (dx + dy == 1)
            {
                return true;
            }
        }

        return false;
    }

    private void DrawUndergroundBackdrop()
    {
        float top =
            LevelToWorldY(0);

        float bottom =
            WorldBottom
            + CellSize;

        if (bottom <= top)
        {
            return;
        }

        // One continuous underground void. Mined/open cells above level 0
        // reveal the normal sky/parallax; cells at level 0 and below reveal
        // this cave background.
        DrawRect(
            new Rect2(
                LandLeftX,
                top,
                LandRightX - LandLeftX,
                bottom - top),
            new Color(
                0.035f,
                0.045f,
                0.055f));

        Color grid =
            new(
                0.08f,
                0.09f,
                0.10f,
                0.24f);

        for (float y = top;
             y <= bottom;
             y += CellSize)
        {
            DrawLine(
                new Vector2(
                    LandLeftX,
                    y),
                new Vector2(
                    LandRightX,
                    y),
                grid,
                1.0f);
        }
    }

    private void DrawUndergroundOpenings()
    {
        Color cave =
            new(
                0.035f,
                0.045f,
                0.055f);

        Color caveGrid =
            new(
                0.08f,
                0.09f,
                0.10f,
                0.55f);

        foreach (UndergroundCell cell
                 in _island.UndergroundOpenCells)
        {
            if (cell.CellX < 0
                || cell.CellX
                    >= _island.WidthCells
                || cell.LogicalLevel > 0
                || cell.LogicalLevel
                    > _island.SurfaceLevels[
                        cell.CellX])
            {
                continue;
            }

            Rect2 rect =
                GetTerrainCellRect(
                    cell.CellX,
                    cell.LogicalLevel);

            DrawRect(
                rect,
                cave);

            DrawRect(
                rect,
                caveGrid,
                filled: false,
                width: 1.0f);
        }

        foreach (UndergroundCell cell
                 in _state.MinedUndergroundCells)
        {
            if (cell.LogicalLevel > 0)
            {
                continue;
            }

            Rect2 rect =
                GetTerrainCellRect(
                    cell.CellX,
                    cell.LogicalLevel);

            DrawRect(
                rect,
                cave);

            DrawRect(
                rect,
                caveGrid,
                filled: false,
                width: 1.0f);
        }
    }

    private void DrawUndergroundOres()
    {
        foreach ((UndergroundCell cell, UndergroundOreSpawn ore)
                 in _island.UndergroundOres)
        {
            if (!IsSolidTerrainCell(cell)
                || !IsMineCellExposed(cell))
            {
                continue;
            }

            Rect2 rect =
                GetTerrainCellRect(
                    cell.CellX,
                    cell.LogicalLevel);

            Color color =
                ore.Kind switch
                {
                    UndergroundOreKind.Iron =>
                        new Color(
                            0.72f,
                            0.48f,
                            0.33f),

                    _ =>
                        new Color(
                            0.72f,
                            0.72f,
                            0.72f),
                };

            DrawCircle(
                rect.Position
                    + new Vector2(
                        15.0f,
                        16.0f),
                5.0f,
                color);

            DrawCircle(
                rect.Position
                    + new Vector2(
                        31.0f,
                        30.0f),
                ore.Richness > 1
                    ? 6.0f
                    : 4.0f,
                color);
        }
    }

    private void DrawMineLadder()
    {
        float shaftLeft =
            LandLeftX
            + _island.MineShaftLeftCell
                * CellSize;

        float shaftWidth =
            _island.MineShaftWidthCells
            * CellSize;

        float centerX =
            shaftLeft
            + shaftWidth / 2.0f;

        int highestSurface =
            Enumerable
                .Range(
                    _island.MineShaftLeftCell,
                    _island.MineShaftWidthCells)
                .Select(
                    x =>
                        _island.SurfaceLevels[x])
                .Max();

        float top =
            LevelToWorldY(
                highestSurface)
            - 5.0f;

        float bottom =
            WorldBottom
            + CellSize;

        Color wood =
            new(
                0.44f,
                0.27f,
                0.11f);

        DrawLine(
            new Vector2(
                centerX - 13.0f,
                top),
            new Vector2(
                centerX - 13.0f,
                bottom),
            wood,
            5.0f);

        DrawLine(
            new Vector2(
                centerX + 13.0f,
                top),
            new Vector2(
                centerX + 13.0f,
                bottom),
            wood,
            5.0f);

        for (float y = top + 14.0f;
             y < bottom;
             y += 24.0f)
        {
            DrawLine(
                new Vector2(
                    centerX - 13.0f,
                    y),
                new Vector2(
                    centerX + 13.0f,
                    y),
                wood,
                3.0f);
        }
    }

    private void DrawGeneratedStructures()
    {
        foreach (GeneratedStructureDefinition structure
                 in _island.GeneratedStructures)
        {
            float centerX =
                LandLeftX
                + (structure.CenterCellX
                    + 0.5f)
                    * CellSize;

            float groundY =
                LevelToWorldY(
                    structure.BaseSurfaceLevel);

            float width =
                structure.WidthCells
                * CellSize
                * 0.88f;

            if (structure.Kind
                == GeneratedStructureKind.House)
            {
                Color wall =
                    _island.Biome
                        == BiomeType.Desert
                            ? new Color(
                                0.78f,
                                0.58f,
                                0.33f)
                            : new Color(
                                0.55f,
                                0.35f,
                                0.18f);

                DrawRect(
                    new Rect2(
                        centerX - width / 2.0f,
                        groundY - 120.0f,
                        width,
                        120.0f),
                    wall);

                DrawColoredPolygon(
                    new Vector2[]
                    {
                        new(
                            centerX - width / 2.0f - 14.0f,
                            groundY - 120.0f),
                        new(
                            centerX,
                            groundY - 190.0f),
                        new(
                            centerX + width / 2.0f + 14.0f,
                            groundY - 120.0f),
                    },
                    new Color(
                        0.34f,
                        0.18f,
                        0.09f));

                DrawRect(
                    new Rect2(
                        centerX - 21.0f,
                        groundY - 70.0f,
                        42.0f,
                        70.0f),
                    new Color(
                        0.20f,
                        0.11f,
                        0.05f));
            }
            else
            {
                Color stone =
                    new(
                        0.43f,
                        0.45f,
                        0.48f);

                DrawRect(
                    new Rect2(
                        centerX - width / 2.0f,
                        groundY - 145.0f,
                        width,
                        145.0f),
                    stone);

                float towerWidth =
                    58.0f;

                DrawRect(
                    new Rect2(
                        centerX - width / 2.0f,
                        groundY - 205.0f,
                        towerWidth,
                        205.0f),
                    new Color(
                        0.37f,
                        0.39f,
                        0.42f));

                DrawRect(
                    new Rect2(
                        centerX + width / 2.0f - towerWidth,
                        groundY - 205.0f,
                        towerWidth,
                        205.0f),
                    new Color(
                        0.37f,
                        0.39f,
                        0.42f));

                DrawRect(
                    new Rect2(
                        centerX - 28.0f,
                        groundY - 85.0f,
                        56.0f,
                        85.0f),
                    new Color(
                        0.18f,
                        0.19f,
                        0.21f));
            }
        }
    }

    private void DrawGeneratedChests()
    {
        foreach (GeneratedStructureDefinition structure
                 in _island.GeneratedStructures)
        {
            GeneratedChestDefinition chest =
                structure.Chest;

            Rect2 cellRect =
                GetBuildCellRect(
                    chest.CellX,
                    chest.LogicalLevel);

            Vector2 center =
                cellRect.GetCenter();

            bool looted =
                _state.IsGeneratedChestLooted(
                    chest.ChestId);

            Color wood =
                looted
                    ? new Color(
                        0.28f,
                        0.18f,
                        0.10f)
                    : new Color(
                        0.48f,
                        0.25f,
                        0.08f);

            DrawRect(
                new Rect2(
                    center.X - 20.0f,
                    cellRect.End.Y - 30.0f,
                    40.0f,
                    27.0f),
                wood);

            DrawRect(
                new Rect2(
                    center.X - 4.0f,
                    cellRect.End.Y - 20.0f,
                    8.0f,
                    10.0f),
                new Color(
                    0.94f,
                    0.72f,
                    0.16f));

            if (looted)
            {
                DrawLine(
                    new Vector2(
                        center.X - 20.0f,
                        cellRect.End.Y - 31.0f),
                    new Vector2(
                        center.X + 18.0f,
                        cellRect.End.Y - 42.0f),
                    new Color(
                        0.32f,
                        0.20f,
                        0.10f),
                    6.0f);
            }
        }
    }

    private void DrawMineIndicator()
    {
        if (_highlightedMineCell is null)
        {
            return;
        }

        Rect2 rect =
            GetTerrainCellRect(
                _highlightedMineCell.Value.CellX,
                _highlightedMineCell.Value.LogicalLevel);

        Color highlight =
            new(
                1.0f,
                0.78f,
                0.22f,
                0.95f);

        DrawRect(
            rect,
            new Color(
                highlight.R,
                highlight.G,
                highlight.B,
                0.14f));

        DrawRect(
            rect,
            highlight,
            filled: false,
            width: 2.0f);

        float barWidth =
            rect.Size.X - 10.0f;

        DrawRect(
            new Rect2(
                rect.Position.X + 5.0f,
                rect.Position.Y + 5.0f,
                barWidth,
                6.0f),
            new Color(
                0.02f,
                0.025f,
                0.03f,
                0.82f));

        DrawRect(
            new Rect2(
                rect.Position.X + 6.0f,
                rect.Position.Y + 6.0f,
                (barWidth - 2.0f)
                    * _highlightedMineProgress,
                4.0f),
            highlight);
    }

    private void DrawPlacedObjects(
        bool backgroundOnly)
    {
        foreach (PlacedObjectState placed in _state.PlacedObjects)
        {
            bool isBackground =
                placed.Layer == BuildLayer.Background;

            if (isBackground != backgroundOnly)
            {
                continue;
            }

            DrawPlacedObject(
                placed.Item,
                placed.CellX,
                placed.LogicalLevel,
                placed.Layer,
                preview: false);
        }
    }

    private void DrawBuildPreview()
    {
        if (_previewItem is null)
        {
            return;
        }

        Rect2 rect =
            GetBuildCellRect(
                _previewCellX,
                _previewLogicalLevel);

        Color status =
            _previewValid
                ? new Color(
                    0.22f,
                    0.92f,
                    0.42f,
                    0.28f)
                : new Color(
                    0.95f,
                    0.20f,
                    0.22f,
                    0.28f);

        DrawRect(
            rect,
            status);

        DrawRect(
            rect,
            new Color(
                status.R,
                status.G,
                status.B,
                0.95f),
            filled: false,
            width: 3.0f);

        DrawPlacedObject(
            _previewItem.Value,
            _previewCellX,
            _previewLogicalLevel,
            _previewLayer,
            preview: true);
    }

    private void DrawPlacedObject(
        ItemType item,
        int cellX,
        int logicalLevel,
        BuildLayer layer,
        bool preview)
    {
        Rect2 rect =
            GetBuildCellRect(
                cellX,
                logicalLevel);

        float alpha =
            preview
                ? 0.55f
                : (layer == BuildLayer.Background
                    ? 0.48f
                    : 1.0f);

        Color WithAlpha(Color color)
        {
            return new Color(
                color.R,
                color.G,
                color.B,
                color.A * alpha);
        }

        Vector2 center =
            rect.GetCenter();

        switch (item)
        {
            case ItemType.Wood:
                DrawRect(
                    rect,
                    WithAlpha(
                        new Color(
                            0.45f,
                            0.23f,
                            0.09f)));
                DrawLine(
                    rect.Position
                        + new Vector2(5.0f, 12.0f),
                    rect.Position
                        + new Vector2(
                            rect.Size.X - 5.0f,
                            12.0f),
                    WithAlpha(
                        new Color(
                            0.66f,
                            0.38f,
                            0.16f)),
                    3.0f);
                break;

            case ItemType.Stone:
                DrawRect(
                    rect,
                    WithAlpha(
                        new Color(
                            0.42f,
                            0.44f,
                            0.47f)));
                DrawLine(
                    rect.Position
                        + new Vector2(7.0f, 8.0f),
                    rect.End
                        - new Vector2(9.0f, 12.0f),
                    WithAlpha(
                        new Color(
                            0.29f,
                            0.30f,
                            0.32f)),
                    2.0f);
                break;

            case ItemType.Sandstone:
                DrawRect(
                    rect,
                    WithAlpha(
                        new Color(
                            0.82f,
                            0.61f,
                            0.31f)));
                break;

            case ItemType.Cactus:
                DrawRect(
                    rect,
                    WithAlpha(
                        new Color(
                            0.11f,
                            0.43f,
                            0.21f)));
                break;

            case ItemType.Sapling:
                DrawPlacedSapling(
                    cellX,
                    logicalLevel,
                    alpha,
                    preview);
                break;

            case ItemType.RedFlower:
            case ItemType.YellowFlower:
            case ItemType.BlueFlower:
            case ItemType.OrangeFlower:
            case ItemType.PurpleFlower:
            case ItemType.PinkFlower:
                DrawPlacedFlower(
                    item,
                    center.X,
                    rect.End.Y,
                    alpha);
                break;

            case ItemType.Grass:
                DrawLine(
                    new Vector2(
                        center.X - 10.0f,
                        rect.End.Y),
                    new Vector2(
                        center.X - 3.0f,
                        rect.End.Y - 25.0f),
                    WithAlpha(
                        new Color(
                            0.12f,
                            0.54f,
                            0.18f)),
                    4.0f);
                DrawLine(
                    new Vector2(
                        center.X + 9.0f,
                        rect.End.Y),
                    new Vector2(
                        center.X + 2.0f,
                        rect.End.Y - 30.0f),
                    WithAlpha(
                        new Color(
                            0.12f,
                            0.54f,
                            0.18f)),
                    4.0f);
                break;

            case ItemType.Beehive:
                DrawRect(
                    new Rect2(
                        center.X - 17.0f,
                        rect.End.Y - 35.0f,
                        34.0f,
                        31.0f),
                    WithAlpha(
                        new Color(
                            0.91f,
                            0.65f,
                            0.10f)));
                DrawLine(
                    new Vector2(
                        center.X - 17.0f,
                        rect.End.Y - 24.0f),
                    new Vector2(
                        center.X + 17.0f,
                        rect.End.Y - 24.0f),
                    WithAlpha(
                        new Color(
                            0.54f,
                            0.32f,
                            0.06f)),
                    3.0f);
                DrawCircle(
                    new Vector2(
                        center.X,
                        rect.End.Y - 14.0f),
                    4.0f,
                    WithAlpha(
                        new Color(
                            0.18f,
                            0.12f,
                            0.04f)));

                if (!preview)
                {
                    PlacedObjectState? hiveState =
                        _state.PlacedObjects.FirstOrDefault(
                            placed =>
                                placed.CellX == cellX
                                && placed.LogicalLevel == logicalLevel
                                && placed.Item == ItemType.Beehive);

                    if (hiveState is not null
                        && hiveState.StoredOutput > 0)
                    {
                        for (int i = 0;
                             i < hiveState.StoredOutput;
                             i++)
                        {
                            DrawCircle(
                                new Vector2(
                                    center.X
                                        - 10.0f
                                        + i * 10.0f,
                                    rect.Position.Y
                                        - 8.0f),
                                4.0f,
                                new Color(
                                    1.0f,
                                    0.70f,
                                    0.08f,
                                    alpha));
                        }
                    }
                }

                break;

            case ItemType.Chest:
                DrawRect(
                    new Rect2(
                        center.X - 19.0f,
                        rect.End.Y - 31.0f,
                        38.0f,
                        29.0f),
                    WithAlpha(
                        new Color(
                            0.39f,
                            0.19f,
                            0.07f)));
                DrawRect(
                    new Rect2(
                        center.X - 3.0f,
                        rect.End.Y - 20.0f,
                        6.0f,
                        9.0f),
                    WithAlpha(
                        new Color(
                            0.95f,
                            0.72f,
                            0.16f)));
                break;

            case ItemType.Forge:
                DrawRect(
                    new Rect2(
                        center.X - 20.0f,
                        rect.End.Y - 36.0f,
                        40.0f,
                        34.0f),
                    WithAlpha(
                        new Color(
                            0.18f,
                            0.19f,
                            0.21f)));
                DrawCircle(
                    new Vector2(
                        center.X,
                        rect.End.Y - 16.0f),
                    8.0f,
                    WithAlpha(
                        new Color(
                            1.0f,
                            0.35f,
                            0.05f)));
                break;

            case ItemType.Anvil:
                DrawRect(
                    new Rect2(
                        center.X - 20.0f,
                        rect.End.Y - 28.0f,
                        40.0f,
                        10.0f),
                    WithAlpha(
                        new Color(
                            0.35f,
                            0.37f,
                            0.40f)));
                DrawRect(
                    new Rect2(
                        center.X - 8.0f,
                        rect.End.Y - 18.0f,
                        16.0f,
                        16.0f),
                    WithAlpha(
                        new Color(
                            0.29f,
                            0.31f,
                            0.34f)));
                break;

            case ItemType.Cauldron:
                DrawCircle(
                    new Vector2(
                        center.X,
                        rect.End.Y - 18.0f),
                    17.0f,
                    WithAlpha(
                        new Color(
                            0.13f,
                            0.15f,
                            0.16f)));
                DrawRect(
                    new Rect2(
                        center.X - 20.0f,
                        rect.End.Y - 35.0f,
                        40.0f,
                        7.0f),
                    WithAlpha(
                        new Color(
                            0.08f,
                            0.09f,
                            0.10f)));
                break;
        }

        if (!preview
            && _highlightedPlacementId is not null)
        {
            PlacedObjectState? highlighted =
                _state.PlacedObjects.FirstOrDefault(
                    placed =>
                        placed.PlacementId
                        == _highlightedPlacementId.Value);

            if (highlighted is not null
                && highlighted.CellX == cellX
                && highlighted.LogicalLevel == logicalLevel)
            {
                DrawPlacedHarvestIndicator(
                    rect,
                    highlighted);
            }
        }
    }

    private void DrawPlacedHarvestIndicator(
        Rect2 rect,
        PlacedObjectState placed)
    {
        float top =
            placed.Item == ItemType.Sapling
                ? rect.Position.Y - 54.0f
                : rect.Position.Y - 8.0f;

        float width = 58.0f;
        float left =
            rect.GetCenter().X
            - width / 2.0f;

        DrawRect(
            new Rect2(
                left,
                top,
                width,
                7.0f),
            new Color(
                0.02f,
                0.025f,
                0.03f,
                0.82f));

        DrawRect(
            new Rect2(
                left + 1.0f,
                top + 1.0f,
                (width - 2.0f)
                    * _highlightedPlacementProgress,
                5.0f),
            new Color(
                1.0f,
                0.83f,
                0.25f,
                0.95f));
    }

    private void DrawPlacedSapling(
        int cellX,
        int logicalLevel,
        float alpha,
        bool preview)
    {
        Rect2 rect =
            GetBuildCellRect(
                cellX,
                logicalLevel);

        Vector2 center =
            rect.GetCenter();

        double growthSeconds = 0.0;

        if (!preview)
        {
            PlacedObjectState? state =
                _state.PlacedObjects.FirstOrDefault(
                    placed =>
                        placed.CellX == cellX
                        && placed.LogicalLevel == logicalLevel
                        && placed.Item == ItemType.Sapling);

            growthSeconds =
                state?.GrowthSeconds ?? 0.0;
        }

        float ratio =
            preview
                ? 0.0f
                : (float)Math.Clamp(
                    growthSeconds
                    / WorldSimulationService.SaplingGrowthSeconds,
                    0.0,
                    1.0);

        if (ratio >= 1.0f)
        {
            PlacedObjectState? matureState =
                preview
                    ? null
                    : _state.PlacedObjects.FirstOrDefault(
                        placed =>
                            placed.CellX == cellX
                            && placed.LogicalLevel == logicalLevel
                            && placed.Item == ItemType.Sapling);

            bool large =
                matureState is not null
                && PlacedHarvestRules.IsLargeMatureTree(
                    matureState);

            float height =
                large
                    ? 110.0f
                    : 84.0f;

            DrawRect(
                new Rect2(
                    center.X - 6.0f,
                    rect.End.Y - height + 28.0f,
                    12.0f,
                    height - 28.0f),
                new Color(
                    0.33f,
                    0.18f,
                    0.08f,
                    alpha));

            Color leaves =
                new(
                    0.10f,
                    0.45f,
                    0.18f,
                    alpha);

            DrawCircle(
                new Vector2(
                    center.X,
                    rect.End.Y - height + 18.0f),
                26.0f,
                leaves);

            DrawCircle(
                new Vector2(
                    center.X - 17.0f,
                    rect.End.Y - height + 31.0f),
                18.0f,
                leaves);

            DrawCircle(
                new Vector2(
                    center.X + 17.0f,
                    rect.End.Y - height + 31.0f),
                18.0f,
                leaves);

            return;
        }

        float saplingHeight =
            24.0f + ratio * 30.0f;

        DrawLine(
            new Vector2(
                center.X,
                rect.End.Y),
            new Vector2(
                center.X,
                rect.End.Y - saplingHeight),
            new Color(
                0.35f,
                0.20f,
                0.08f,
                alpha),
            4.0f);

        DrawCircle(
            new Vector2(
                center.X - 7.0f,
                rect.End.Y - saplingHeight),
            8.0f + ratio * 4.0f,
            new Color(
                0.12f,
                0.52f,
                0.21f,
                alpha));

        DrawCircle(
            new Vector2(
                center.X + 7.0f,
                rect.End.Y - saplingHeight + 3.0f),
            8.0f + ratio * 4.0f,
            new Color(
                0.12f,
                0.52f,
                0.21f,
                alpha));
    }

    private void DrawPlacedFlower(
        ItemType item,
        float x,
        float groundY,
        float alpha)
    {
        Color petal =
            item switch
            {
                ItemType.RedFlower =>
                    new Color(0.95f, 0.20f, 0.18f, alpha),
                ItemType.YellowFlower =>
                    new Color(0.95f, 0.80f, 0.15f, alpha),
                ItemType.BlueFlower =>
                    new Color(0.20f, 0.40f, 0.95f, alpha),
                ItemType.OrangeFlower =>
                    new Color(0.95f, 0.45f, 0.10f, alpha),
                ItemType.PurpleFlower =>
                    new Color(0.58f, 0.16f, 0.78f, alpha),
                ItemType.PinkFlower =>
                    new Color(1.0f, 0.55f, 0.70f, alpha),
                _ =>
                    new Color(1.0f, 1.0f, 1.0f, alpha),
            };

        DrawLine(
            new Vector2(x, groundY),
            new Vector2(x, groundY - 20.0f),
            new Color(
                0.12f,
                0.45f,
                0.18f,
                alpha),
            3.0f);

        DrawCircle(
            new Vector2(x, groundY - 22.0f),
            7.0f,
            petal);

        DrawCircle(
            new Vector2(x, groundY - 22.0f),
            2.0f,
            new Color(
                1.0f,
                0.84f,
                0.22f,
                alpha));
    }

    private void DrawDistantBackground()
    {
        Color distant = _palette.Distant;
        float horizonY = -260.0f;
        float spacing = 420.0f;

        int count =
            (int)Math.Ceiling(
                WorldWidth / spacing) + 1;

        for (int i = 0; i < count; i++)
        {
            float x =
                i * spacing - 120.0f;
            float radius =
                _island.Biome == BiomeType.Mountain
                    ? 250.0f
                    : 170.0f;

            DrawCircle(
                new Vector2(
                    x,
                    horizonY + radius),
                radius,
                distant);
        }
    }

    private void DrawOcean()
    {
        float oceanTop = 0.0f;
        float oceanHeight =
            6.0f * CellSize;

        DrawRect(
            new Rect2(
                0.0f,
                oceanTop,
                LandLeftX,
                oceanHeight),
            _palette.Water);

        DrawRect(
            new Rect2(
                LandRightX,
                oceanTop,
                WorldWidth - LandRightX,
                oceanHeight),
            _palette.Water);

        Color foam =
            new(0.82f, 0.94f, 1.0f, 0.78f);

        DrawLine(
            new Vector2(
                0.0f,
                oceanTop + 4.0f),
            new Vector2(
                LandLeftX,
                oceanTop + 4.0f),
            foam,
            3.0f);

        DrawLine(
            new Vector2(
                LandRightX,
                oceanTop + 4.0f),
            new Vector2(
                WorldWidth,
                oceanTop + 4.0f),
            foam,
            3.0f);
    }

    private void DrawTerrain()
    {
        int highestSurface =
            _island.SurfaceLevels.Max();

        for (int logicalLevel = highestSurface;
             logicalLevel
                >= IslandGenerationSettings.DeepestLogicalLevel;
             logicalLevel--)
        {
            int runStart =
                -1;

            for (int cellX = 0;
                 cellX <= _island.WidthCells;
                 cellX++)
            {
                bool solid =
                    cellX < _island.WidthCells
                    && IsSolidTerrainCell(
                        new UndergroundCell(
                            cellX,
                            logicalLevel));

                if (solid
                    && runStart < 0)
                {
                    runStart =
                        cellX;
                }

                if ((!solid
                        || cellX == _island.WidthCells)
                    && runStart >= 0)
                {
                    int runLength =
                        cellX - runStart;

                    DrawRect(
                        new Rect2(
                            LandLeftX
                                + runStart
                                    * CellSize,
                            LevelToWorldY(
                                logicalLevel),
                            runLength
                                * CellSize,
                            CellSize),
                        _palette.Ground);

                    runStart =
                        -1;
                }
            }
        }

        // Draw the biome surface cap only where the cell above is open.
        for (int cellX = 0;
             cellX < _island.WidthCells;
             cellX++)
        {
            for (int logicalLevel =
                    _island.SurfaceLevels[cellX];
                 logicalLevel
                    >= IslandGenerationSettings.DeepestLogicalLevel;
                 logicalLevel--)
            {
                var cell =
                    new UndergroundCell(
                        cellX,
                        logicalLevel);

                if (!IsSolidTerrainCell(cell))
                {
                    continue;
                }

                var above =
                    new UndergroundCell(
                        cellX,
                        logicalLevel + 1);

                if (IsSolidTerrainCell(above))
                {
                    continue;
                }

                Rect2 rect =
                    GetTerrainCellRect(
                        cellX,
                        logicalLevel);

                DrawRect(
                    new Rect2(
                        rect.Position.X,
                        rect.Position.Y,
                        rect.Size.X,
                        8.0f),
                    _palette.Surface);
            }
        }
    }

    private void DrawBoatSites()
    {
        DrawBoatSite(BoatSide.Left);
        DrawBoatSite(BoatSide.Right);
    }

    private void DrawBoatSite(BoatSide side)
    {
        int shoreStartCell =
            side == BoatSide.Left
                ? 0
                : _island.WidthCells
                    - BoatConstructionShoreCells;

        float shoreX =
            LandLeftX
            + shoreStartCell * CellSize;

        float shoreWidth =
            BoatConstructionShoreCells
            * CellSize;

        int edgeCell =
            side == BoatSide.Left
                ? 0
                : _island.WidthCells - 1;

        float groundY =
            LevelToWorldY(
                _island.SurfaceLevels[edgeCell]);

        bool built =
            _state.IsBoatBuilt(side);

        Color marker =
            built
                ? new Color(
                    0.44f,
                    0.28f,
                    0.12f,
                    0.42f)
                : new Color(
                    _palette.Accent.R,
                    _palette.Accent.G,
                    _palette.Accent.B,
                    0.42f);

        DrawRect(
            new Rect2(
                shoreX,
                groundY - 7.0f,
                shoreWidth,
                14.0f),
            marker);

        DrawLine(
            new Vector2(
                side == BoatSide.Left
                    ? LandLeftX
                    : LandRightX,
                groundY - 3.0f),
            new Vector2(
                side == BoatSide.Left
                    ? LandLeftX
                    : LandRightX,
                groundY + 18.0f),
            _palette.Accent,
            4.0f);

        if (!built)
        {
            return;
        }

        Vector2 center =
            GetBoatWaterCenter(side);

        Color hull =
            new(0.39f, 0.20f, 0.08f);

        DrawRect(
            new Rect2(
                center.X - 48.0f,
                center.Y + 7.0f,
                96.0f,
                18.0f),
            hull);

        DrawLine(
            new Vector2(
                center.X,
                center.Y + 6.0f),
            new Vector2(
                center.X,
                center.Y - 55.0f),
            new Color(
                0.29f,
                0.17f,
                0.08f),
            5.0f);

        float sailDirection =
            side == BoatSide.Left
                ? -1.0f
                : 1.0f;

        DrawColoredPolygon(
            new Vector2[]
            {
                new Vector2(
                    center.X,
                    center.Y - 52.0f),
                new Vector2(
                    center.X,
                    center.Y - 8.0f),
                new Vector2(
                    center.X
                        + 38.0f * sailDirection,
                    center.Y - 17.0f),
            },
            new Color(
                0.89f,
                0.82f,
                0.64f));

        DrawLine(
            new Vector2(
                center.X - 48.0f,
                center.Y + 7.0f),
            new Vector2(
                center.X + 48.0f,
                center.Y + 7.0f),
            new Color(
                0.67f,
                0.39f,
                0.13f),
            4.0f);
    }

    private void DrawNaturalFeatures()
    {
        foreach (NaturalFeatureSpawn feature
                 in _island.NaturalFeatures)
        {
            if (_state.IsNaturalFeatureHarvested(
                feature.FeatureId))
            {
                continue;
            }

            Vector2 position =
                GetFeatureWorldPosition(feature);
            float x = position.X;
            float y = position.Y;

            switch (feature.Kind)
            {
                case NaturalFeatureKind.Tree:
                    DrawTree(
                        x,
                        y,
                        feature.Variant,
                        HarvestRules.IsLarge(feature));
                    break;

                case NaturalFeatureKind.Pine:
                    DrawPine(
                        x,
                        y,
                        feature.Variant,
                        HarvestRules.IsLarge(feature));
                    break;

                case NaturalFeatureKind.Palm:
                    DrawPalm(
                        x,
                        y,
                        feature.Variant,
                        HarvestRules.IsLarge(feature));
                    break;

                case NaturalFeatureKind.Cactus:
                    DrawCactus(
                        x,
                        y,
                        feature.Variant,
                        HarvestRules.IsLarge(feature));
                    break;

                case NaturalFeatureKind.Rock:
                    DrawRock(x, y, feature.Variant);
                    break;

                case NaturalFeatureKind.Flower:
                    DrawFlower(x, y, feature.Variant);
                    break;

                case NaturalFeatureKind.Grass:
                    DrawGrass(x, y);
                    break;

                case NaturalFeatureKind.Bush:
                    DrawBush(x, y, feature.Variant);
                    break;

                case NaturalFeatureKind.Mushroom:
                    DrawMushroom(x, y, feature.Variant);
                    break;
            }

            if (_highlightedFeatureId
                == feature.FeatureId)
            {
                DrawHarvestIndicator(
                    feature,
                    x,
                    y);
            }
        }
    }

    private void DrawHarvestIndicator(
        NaturalFeatureSpawn feature,
        float x,
        float y)
    {
        Color highlight =
            new(
                1.0f,
                0.83f,
                0.25f,
                0.88f);

        float radius =
            feature.Kind switch
            {
                NaturalFeatureKind.Tree
                    or NaturalFeatureKind.Pine
                    or NaturalFeatureKind.Palm
                        => 38.0f,
                NaturalFeatureKind.Bush
                    or NaturalFeatureKind.Cactus
                        => 28.0f,
                _ => 22.0f,
            };

        DrawArc(
            new Vector2(
                x,
                y - radius),
            radius,
            0.0f,
            Mathf.Tau,
            40,
            highlight,
            2.5f,
            true);

        float barWidth = 64.0f;
        float barY =
            y - radius * 2.0f - 14.0f;

        DrawRect(
            new Rect2(
                x - barWidth / 2.0f,
                barY,
                barWidth,
                7.0f),
            new Color(
                0.02f,
                0.025f,
                0.03f,
                0.80f));

        DrawRect(
            new Rect2(
                x - barWidth / 2.0f + 1.0f,
                barY + 1.0f,
                (barWidth - 2.0f)
                    * _highlightedFeatureProgress,
                5.0f),
            highlight);
    }

    private void DrawTree(
        float x,
        float y,
        int variant,
        bool large)
    {
        float height =
            (large ? 105.0f : 78.0f)
            + variant * 5.0f;
        Color trunk =
            new(0.33f, 0.18f, 0.08f);
        Color leaves =
            new(
                0.10f + variant * 0.015f,
                0.43f,
                0.18f);

        DrawRect(
            new Rect2(
                x - 6.0f,
                y - height + 26.0f,
                12.0f,
                height - 26.0f),
            trunk);

        DrawCircle(
            new Vector2(
                x,
                y - height + 18.0f),
            26.0f,
            leaves);

        DrawCircle(
            new Vector2(
                x - 18.0f,
                y - height + 28.0f),
            19.0f,
            leaves);

        DrawCircle(
            new Vector2(
                x + 18.0f,
                y - height + 28.0f),
            19.0f,
            leaves);
    }

    private void DrawPine(
        float x,
        float y,
        int variant,
        bool large)
    {
        float height =
            (large ? 112.0f : 86.0f)
            + variant * 5.0f;
        Color trunk =
            new(0.27f, 0.18f, 0.12f);
        Color needles =
            new(0.10f, 0.30f, 0.22f);

        DrawRect(
            new Rect2(
                x - 5.0f,
                y - height + 30.0f,
                10.0f,
                height - 30.0f),
            trunk);

        DrawCircle(
            new Vector2(
                x,
                y - height + 22.0f),
            19.0f,
            needles);

        DrawCircle(
            new Vector2(
                x,
                y - height + 38.0f),
            24.0f,
            needles);

        DrawCircle(
            new Vector2(
                x,
                y - height + 56.0f),
            29.0f,
            needles);
    }

    private void DrawPalm(
        float x,
        float y,
        int variant,
        bool large)
    {
        float height =
            (large ? 112.0f : 86.0f)
            + variant * 5.0f;
        Color trunk =
            new(0.48f, 0.29f, 0.12f);
        Color leaves =
            new(0.08f, 0.46f, 0.20f);

        Vector2 crown =
            new(
                x + 8.0f,
                y - height);

        DrawLine(
            new Vector2(x, y),
            crown,
            trunk,
            10.0f,
            true);

        DrawLine(
            crown,
            crown + new Vector2(-34.0f, -8.0f),
            leaves,
            9.0f,
            true);

        DrawLine(
            crown,
            crown + new Vector2(34.0f, -10.0f),
            leaves,
            9.0f,
            true);

        DrawLine(
            crown,
            crown + new Vector2(-24.0f, 10.0f),
            leaves,
            8.0f,
            true);

        DrawLine(
            crown,
            crown + new Vector2(25.0f, 11.0f),
            leaves,
            8.0f,
            true);

        DrawCircle(
            crown + new Vector2(-5.0f, 8.0f),
            4.0f,
            new Color(0.42f, 0.24f, 0.09f));
    }

    private void DrawCactus(
        float x,
        float y,
        int variant,
        bool large)
    {
        float height =
            (large ? 72.0f : 50.0f)
            + variant * 5.0f;
        Color cactus =
            new(0.10f, 0.48f, 0.24f);

        DrawLine(
            new Vector2(x, y),
            new Vector2(x, y - height),
            cactus,
            16.0f,
            true);

        DrawLine(
            new Vector2(x, y - 25.0f),
            new Vector2(x - 14.0f, y - 34.0f),
            cactus,
            10.0f,
            true);

        DrawLine(
            new Vector2(x - 14.0f, y - 34.0f),
            new Vector2(x - 14.0f, y - 45.0f),
            cactus,
            10.0f,
            true);

        if (variant > 0)
        {
            DrawLine(
                new Vector2(x, y - 34.0f),
                new Vector2(x + 14.0f, y - 42.0f),
                cactus,
                10.0f,
                true);
        }
    }

    private void DrawRock(
        float x,
        float y,
        int variant)
    {
        float radius =
            11.0f + variant * 3.0f;

        Color rock =
            _island.Biome == BiomeType.Snowy
                ? new Color(
                    0.67f,
                    0.73f,
                    0.79f)
                : new Color(
                    0.38f,
                    0.40f,
                    0.42f);

        DrawCircle(
            new Vector2(
                x,
                y - radius * 0.55f),
            radius,
            rock);

        DrawRect(
            new Rect2(
                x - radius,
                y - radius * 0.6f,
                radius * 2.0f,
                radius * 0.6f),
            rock);
    }

    private void DrawFlower(
        float x,
        float y,
        int variant)
    {
        Color[] petals =
        {
            new Color(0.95f, 0.20f, 0.18f),
            new Color(0.95f, 0.80f, 0.15f),
            new Color(0.20f, 0.40f, 0.95f),
        };

        DrawLine(
            new Vector2(x, y),
            new Vector2(x, y - 18.0f),
            new Color(0.12f, 0.45f, 0.18f),
            3.0f);

        DrawCircle(
            new Vector2(x, y - 20.0f),
            6.0f,
            petals[
                Math.Abs(variant)
                % petals.Length]);

        DrawCircle(
            new Vector2(x, y - 20.0f),
            2.0f,
            new Color(1.0f, 0.82f, 0.20f));
    }

    private void DrawGrass(
        float x,
        float y)
    {
        Color grass =
            new(0.12f, 0.54f, 0.18f);

        DrawLine(
            new Vector2(x - 8.0f, y),
            new Vector2(x - 3.0f, y - 20.0f),
            grass,
            3.0f);

        DrawLine(
            new Vector2(x, y),
            new Vector2(x, y - 25.0f),
            grass,
            3.0f);

        DrawLine(
            new Vector2(x + 8.0f, y),
            new Vector2(x + 3.0f, y - 19.0f),
            grass,
            3.0f);
    }

    private void DrawBush(
        float x,
        float y,
        int variant)
    {
        Color bush =
            new(
                0.07f,
                0.37f + variant * 0.03f,
                0.13f);

        DrawCircle(
            new Vector2(x - 11.0f, y - 13.0f),
            14.0f,
            bush);

        DrawCircle(
            new Vector2(x + 10.0f, y - 14.0f),
            15.0f,
            bush);

        DrawCircle(
            new Vector2(x, y - 22.0f),
            15.0f,
            bush);
    }

    private void DrawMushroom(
        float x,
        float y,
        int variant)
    {
        Color cap =
            variant == 0
                ? new Color(
                    0.47f,
                    0.24f,
                    0.10f)
                : new Color(
                    0.80f,
                    0.12f,
                    0.12f);

        DrawRect(
            new Rect2(
                x - 3.0f,
                y - 12.0f,
                6.0f,
                12.0f),
            new Color(0.90f, 0.83f, 0.66f));

        DrawCircle(
            new Vector2(x, y - 13.0f),
            8.0f,
            cap);

        DrawRect(
            new Rect2(
                x - 8.0f,
                y - 13.0f,
                16.0f,
                8.0f),
            cap);
    }
}
