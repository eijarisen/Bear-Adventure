using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Simulation;

public sealed class WorldSimulationService
{
    public const double SaplingGrowthSeconds = 60.0;
    public const double HoneyCycleSeconds = 45.0;
    public const int HoneyCapacity = 3;
    public const int HoneyFlowerRadiusCells = 5;
    public const int MinimumFlowersForHoney = 3;

    private readonly Dictionary<int, IslandDefinition> _islandCache = new();

    public bool Advance(
        GameSessionState session,
        WorldSeed worldSeed,
        IslandGenerator islandGenerator,
        double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(worldSeed);
        ArgumentNullException.ThrowIfNull(islandGenerator);

        if (deltaSeconds <= 0.0)
        {
            return false;
        }

        bool visibleStateChanged = false;

        foreach ((int islandId, IslandDeltaState islandState)
                 in session.Islands)
        {
            if (islandState.AdvanceNaturalRegrowth(deltaSeconds))
            {
                visibleStateChanged = true;
            }

            if (islandState.PlacedObjects.Count == 0)
            {
                continue;
            }

            IslandDefinition? definition = null;

            foreach (PlacedObjectState placed
                     in islandState.PlacedObjects)
            {
                if (placed.Item == ItemType.Sapling)
                {
                    double oldGrowth = placed.GrowthSeconds;
                    placed.GrowthSeconds =
                        Math.Min(
                            SaplingGrowthSeconds,
                            placed.GrowthSeconds
                            + deltaSeconds);

                    if ((int)(oldGrowth / 5.0)
                        != (int)(placed.GrowthSeconds / 5.0))
                    {
                        visibleStateChanged = true;
                    }

                    continue;
                }

                if (placed.Item != ItemType.Beehive)
                {
                    continue;
                }

                if (placed.StoredOutput >= HoneyCapacity)
                {
                    continue;
                }

                definition ??=
                    GetIsland(
                        islandId,
                        worldSeed,
                        islandGenerator);

                int flowers =
                    CountAvailableFlowers(
                        definition,
                        islandState,
                        placed.CellX);

                if (flowers < MinimumFlowersForHoney)
                {
                    continue;
                }

                placed.ProductionSeconds += deltaSeconds;

                while (placed.ProductionSeconds
                        >= HoneyCycleSeconds
                    && placed.StoredOutput
                        < HoneyCapacity)
                {
                    placed.ProductionSeconds -=
                        HoneyCycleSeconds;
                    placed.StoredOutput++;
                    visibleStateChanged = true;
                }
            }
        }

        return visibleStateChanged;
    }

    private IslandDefinition GetIsland(
        int islandId,
        WorldSeed worldSeed,
        IslandGenerator islandGenerator)
    {
        if (_islandCache.TryGetValue(
            islandId,
            out IslandDefinition? cached))
        {
            return cached;
        }

        IslandDefinition generated =
            islandGenerator.Generate(
                worldSeed,
                islandId);

        _islandCache[islandId] =
            generated;

        return generated;
    }

    private static int CountAvailableFlowers(
        IslandDefinition island,
        IslandDeltaState state,
        int hiveCellX)
    {
        int count = 0;

        foreach (NaturalFeatureSpawn feature
                 in island.NaturalFeatures)
        {
            if (feature.Kind
                    != NaturalFeatureKind.Flower
                || state.IsNaturalFeatureHarvested(
                    feature.FeatureId)
                || Math.Abs(
                    feature.CellX
                    - hiveCellX)
                    > HoneyFlowerRadiusCells)
            {
                continue;
            }

            count++;
        }

        foreach (PlacedObjectState placed
                 in state.PlacedObjects)
        {
            if (!PlacementRules.IsFlower(
                placed.Item)
                || Math.Abs(
                    placed.CellX
                    - hiveCellX)
                    > HoneyFlowerRadiusCells)
            {
                continue;
            }

            count++;
        }

        return count;
    }
}
