using BearAdventure.Domain.Gameplay;

namespace BearAdventure.Domain.World;

/// <summary>
/// Persistent changes layered over a deterministically generated island.
/// The untouched island itself is not serialized.
/// </summary>
public sealed class IslandDeltaState
{
    private readonly HashSet<int> _harvestedNaturalFeatureIds = new();
    private readonly Dictionary<int, double> _naturalRegrowthSeconds = new();
    private readonly List<PlacedObjectState> _placedObjects = new();
    private readonly HashSet<UndergroundCell> _minedUndergroundCells = new();
    private readonly HashSet<int> _lootedGeneratedChestIds = new();
    private int _nextPlacementId = 1;

    public IReadOnlySet<int> HarvestedNaturalFeatureIds
        => _harvestedNaturalFeatureIds;

    public IReadOnlyDictionary<int, double> NaturalRegrowthSeconds
        => _naturalRegrowthSeconds;

    public IReadOnlyList<PlacedObjectState> PlacedObjects
        => _placedObjects;

    public int HarvestedNaturalFeatureCount
        => _harvestedNaturalFeatureIds.Count;

    public IReadOnlySet<UndergroundCell> MinedUndergroundCells
        => _minedUndergroundCells;

    public IReadOnlySet<int> LootedGeneratedChestIds
        => _lootedGeneratedChestIds;

    public bool LeftBoatBuilt { get; private set; }

    public bool RightBoatBuilt { get; private set; }

    public bool IsNaturalFeatureHarvested(int featureId)
    {
        return _harvestedNaturalFeatureIds.Contains(featureId);
    }

    public bool MarkNaturalFeatureHarvested(
        int featureId,
        double? regrowSeconds)
    {
        bool added =
            _harvestedNaturalFeatureIds.Add(featureId);

        if (!added)
        {
            return false;
        }

        if (regrowSeconds is > 0.0)
        {
            _naturalRegrowthSeconds[featureId] =
                regrowSeconds.Value;
        }
        else
        {
            _naturalRegrowthSeconds.Remove(featureId);
        }

        return true;
    }

    public bool AdvanceNaturalRegrowth(double deltaSeconds)
    {
        if (deltaSeconds <= 0.0
            || _naturalRegrowthSeconds.Count == 0)
        {
            return false;
        }

        bool changed = false;
        int[] featureIds =
            _naturalRegrowthSeconds.Keys.ToArray();

        foreach (int featureId in featureIds)
        {
            double remaining =
                _naturalRegrowthSeconds[featureId]
                - deltaSeconds;

            if (remaining <= 0.0)
            {
                _naturalRegrowthSeconds.Remove(featureId);
                _harvestedNaturalFeatureIds.Remove(featureId);
                changed = true;
            }
            else
            {
                _naturalRegrowthSeconds[featureId] =
                    remaining;
            }
        }

        return changed;
    }

    public void ReplaceHarvestedNaturalFeatures(
        IEnumerable<int> featureIds)
    {
        ArgumentNullException.ThrowIfNull(featureIds);

        _harvestedNaturalFeatureIds.Clear();
        _naturalRegrowthSeconds.Clear();

        foreach (int featureId in featureIds)
        {
            if (featureId >= 0)
            {
                _harvestedNaturalFeatureIds.Add(featureId);
            }
        }
    }

    public void ReplaceNaturalRegrowth(
        IEnumerable<KeyValuePair<int, double>> regrowth)
    {
        ArgumentNullException.ThrowIfNull(regrowth);

        foreach ((int featureId, double remaining) in regrowth)
        {
            if (featureId < 0
                || remaining <= 0.0)
            {
                continue;
            }

            _harvestedNaturalFeatureIds.Add(featureId);
            _naturalRegrowthSeconds[featureId] =
                remaining;
        }
    }

    public PlacedObjectState AddPlacedObject(
        ItemType item,
        int cellX,
        int logicalLevel,
        BuildLayer layer)
    {
        var placed =
            new PlacedObjectState(
                _nextPlacementId++,
                item,
                cellX,
                logicalLevel,
                layer);

        _placedObjects.Add(placed);
        return placed;
    }

    public bool RemovePlacedObject(int placementId)
    {
        int index =
            _placedObjects.FindIndex(
                item =>
                    item.PlacementId == placementId);

        if (index < 0)
        {
            return false;
        }

        _placedObjects.RemoveAt(index);
        return true;
    }

    public void ReplacePlacedObjects(
        IEnumerable<PlacedObjectState> placedObjects)
    {
        ArgumentNullException.ThrowIfNull(placedObjects);

        _placedObjects.Clear();
        int highestId = 0;

        foreach (PlacedObjectState placed in placedObjects)
        {
            if (placed.PlacementId <= 0
                || placed.CellX < 0)
            {
                continue;
            }

            _placedObjects.Add(placed);
            highestId =
                Math.Max(
                    highestId,
                    placed.PlacementId);
        }

        _nextPlacementId =
            highestId + 1;
    }

    public bool IsUndergroundCellMined(
        UndergroundCell cell)
    {
        return _minedUndergroundCells.Contains(cell);
    }

    public bool MarkUndergroundCellMined(
        UndergroundCell cell)
    {
        return _minedUndergroundCells.Add(cell);
    }

    public void ReplaceMinedUndergroundCells(
        IEnumerable<UndergroundCell> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);

        _minedUndergroundCells.Clear();

        foreach (UndergroundCell cell in cells)
        {
            if (cell.CellX < 0
                || cell.LogicalLevel
                    > IslandGenerationSettings.HighestLogicalLevel
                || cell.LogicalLevel
                    < IslandGenerationSettings.DeepestLogicalLevel)
            {
                continue;
            }

            _minedUndergroundCells.Add(cell);
        }
    }

    public bool IsGeneratedChestLooted(
        int chestId)
    {
        return _lootedGeneratedChestIds.Contains(
            chestId);
    }

    public bool MarkGeneratedChestLooted(
        int chestId)
    {
        return _lootedGeneratedChestIds.Add(
            chestId);
    }

    public void ReplaceLootedGeneratedChestIds(
        IEnumerable<int> chestIds)
    {
        ArgumentNullException.ThrowIfNull(chestIds);

        _lootedGeneratedChestIds.Clear();

        foreach (int chestId in chestIds)
        {
            if (chestId >= 0)
            {
                _lootedGeneratedChestIds.Add(
                    chestId);
            }
        }
    }

    public bool IsBoatBuilt(BoatSide side)
    {
        return side switch
        {
            BoatSide.Left => LeftBoatBuilt,
            BoatSide.Right => RightBoatBuilt,
            _ => false,
        };
    }

    public bool BuildBoat(BoatSide side)
    {
        if (IsBoatBuilt(side))
        {
            return false;
        }

        SetBoatBuilt(side, true);
        return true;
    }

    public void SetBoatBuilt(
        BoatSide side,
        bool built)
    {
        switch (side)
        {
            case BoatSide.Left:
                LeftBoatBuilt = built;
                break;

            case BoatSide.Right:
                RightBoatBuilt = built;
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(side),
                    side,
                    "Unknown boat side.");
        }
    }
}
