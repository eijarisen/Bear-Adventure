using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.Random;

namespace BearAdventure.Domain.World;

public sealed class IslandGenerator
{
    private readonly IslandGenerationSettings _settings;

    public IslandGenerator(IslandGenerationSettings? settings = null)
    {
        _settings = settings ?? IslandGenerationSettings.Default;
    }

    public IslandDefinition Generate(WorldSeed worldSeed, int islandId)
    {
        ArgumentNullException.ThrowIfNull(worldSeed);

        DeterministicRandom identityRandom = new(worldSeed.Derive(islandId, "identity"));
        BiomeType biome = islandId == 0
            ? BiomeType.Forest
            : (BiomeType)identityRandom.NextInt(0, Enum.GetValues<BiomeType>().Length);

        int width = identityRandom.NextInt(
            _settings.MinimumWidthCells,
            _settings.MaximumWidthCells + 1);

        ulong islandSeed = worldSeed.Derive(islandId, "island");
        int[] surfaceLevels = GenerateSurface(worldSeed, islandId, biome, width);
        IReadOnlyList<NaturalFeatureSpawn> features = GenerateFeatures(
            worldSeed,
            islandId,
            biome,
            surfaceLevels);

        UndergroundGenerationResult underground =
            GenerateUnderground(
                worldSeed,
                islandId,
                biome,
                surfaceLevels);

        IReadOnlyList<GeneratedStructureDefinition> structures =
            GenerateStructures(
                worldSeed,
                islandId,
                biome,
                surfaceLevels,
                underground.MineShaftLeftCell,
                underground.MineShaftWidthCells);

        return new IslandDefinition
        {
            IslandId = islandId,
            IslandSeed = islandSeed,
            GeneratorVersion = WorldSeed.GeneratorVersion,
            Biome = biome,
            WidthCells = width,
            BoatSiteWidthCells = _settings.BoatSiteWidthCells,
            SurfaceLevels = surfaceLevels,
            NaturalFeatures = features,
            UndergroundOpenCells = underground.OpenCells,
            UndergroundOres = underground.Ores,
            MineShaftLeftCell = underground.MineShaftLeftCell,
            MineShaftWidthCells = underground.MineShaftWidthCells,
            GeneratedStructures = structures,
        };
    }

    private int[] GenerateSurface(WorldSeed worldSeed, int islandId, BiomeType biome, int width)
    {
        DeterministicRandom random = new(worldSeed.Derive(islandId, "terrain"));
        TerrainProfile profile = TerrainProfile.ForBiome(biome);

        int[] levels = new int[width];
        int leftInterior = _settings.BoatSiteWidthCells;
        int rightInteriorExclusive = width - _settings.BoatSiteWidthCells;

        int current = 0;
        int target = 0;

        for (int x = leftInterior; x < rightInteriorExclusive; x++)
        {
            if (random.Chance(profile.NewTargetChance))
            {
                target = random.NextInt(profile.MinimumLevel, profile.MaximumLevel + 1);
            }

            if (random.Chance(profile.Ruggedness))
            {
                int direction = random.NextInt(-1, 2);
                current += direction;
            }
            else if (current != target && random.Chance(profile.TargetPull))
            {
                current += Math.Sign(target - current);
            }

            current = Math.Clamp(current, profile.MinimumLevel, profile.MaximumLevel);
            levels[x] = current;
        }

        // Guarantee a walkable ramp from each reserved boat area into the generated interior.
        for (int x = leftInterior; x < rightInteriorExclusive; x++)
        {
            int maxDistanceFromZero = x - leftInterior + 1;
            levels[x] = Math.Clamp(levels[x], -maxDistanceFromZero, maxDistanceFromZero);

            if (x > leftInterior)
            {
                levels[x] = Math.Clamp(levels[x], levels[x - 1] - 1, levels[x - 1] + 1);
            }
        }

        for (int x = rightInteriorExclusive - 1; x >= leftInterior; x--)
        {
            int maxDistanceFromZero = rightInteriorExclusive - x;
            levels[x] = Math.Clamp(levels[x], -maxDistanceFromZero, maxDistanceFromZero);

            if (x < rightInteriorExclusive - 1)
            {
                levels[x] = Math.Clamp(levels[x], levels[x + 1] - 1, levels[x + 1] + 1);
            }
        }

        // Both boat zones are intentionally flat at the shared surface reference level.
        for (int x = 0; x < _settings.BoatSiteWidthCells; x++)
        {
            levels[x] = IslandGenerationSettings.GroundReferenceLevel;
        }

        for (int x = width - _settings.BoatSiteWidthCells; x < width; x++)
        {
            levels[x] = IslandGenerationSettings.GroundReferenceLevel;
        }

        return levels;
    }

    private UndergroundGenerationResult GenerateUnderground(
        WorldSeed worldSeed,
        int islandId,
        BiomeType biome,
        int[] surfaceLevels)
    {
        int width =
            surfaceLevels.Length;

        int shaftWidth = 2;
        int shaftLeft =
            Math.Clamp(
                width / 2 - shaftWidth / 2,
                _settings.BoatSiteWidthCells + 8,
                width
                    - _settings.BoatSiteWidthCells
                    - shaftWidth
                    - 8);

        var openCells =
            new HashSet<UndergroundCell>();

        for (int x = shaftLeft;
             x < shaftLeft + shaftWidth;
             x++)
        {
            for (int level = surfaceLevels[x];
                 level >= IslandGenerationSettings.DeepestLogicalLevel;
                 level--)
            {
                openCells.Add(
                    new UndergroundCell(
                        x,
                        level));
            }
        }

        DeterministicRandom caveRandom =
            new(
                worldSeed.Derive(
                    islandId,
                    "underground-caves-v1"));

        int roomIndex = 0;

        for (int floorLevel = -10;
             floorLevel >= -90;
             floorLevel -= 10)
        {
            bool extendLeft =
                roomIndex % 2 == 0;

            int length =
                caveRandom.NextInt(
                    9,
                    19);

            int startX =
                extendLeft
                    ? shaftLeft - length
                    : shaftLeft + shaftWidth;

            int endXExclusive =
                extendLeft
                    ? shaftLeft
                    : shaftLeft + shaftWidth + length;

            startX =
                Math.Max(
                    _settings.BoatSiteWidthCells + 3,
                    startX);

            endXExclusive =
                Math.Min(
                    width
                        - _settings.BoatSiteWidthCells
                        - 3,
                    endXExclusive);

            for (int x = startX;
                 x < endXExclusive;
                 x++)
            {
                for (int height = 1;
                     height <= 3;
                     height++)
                {
                    int level =
                        floorLevel + height;

                    if (level
                        <= surfaceLevels[x])
                    {
                        openCells.Add(
                            new UndergroundCell(
                                x,
                                level));
                    }
                }
            }

            roomIndex++;
        }

        for (int chamber = 0;
             chamber < 12;
             chamber++)
        {
            int centerX =
                caveRandom.NextInt(
                    _settings.BoatSiteWidthCells + 5,
                    width
                        - _settings.BoatSiteWidthCells
                        - 5);

            int centerLevel =
                caveRandom.NextInt(
                    -94,
                    -5);

            int halfWidth =
                caveRandom.NextInt(
                    2,
                    6);

            int halfHeight =
                caveRandom.NextInt(
                    1,
                    3);

            for (int x = centerX - halfWidth;
                 x <= centerX + halfWidth;
                 x++)
            {
                if (x < 0
                    || x >= width)
                {
                    continue;
                }

                for (int level = centerLevel - halfHeight;
                     level <= centerLevel + halfHeight;
                     level++)
                {
                    if (level
                        <= surfaceLevels[x]
                        && level
                            >= IslandGenerationSettings.DeepestLogicalLevel)
                    {
                        openCells.Add(
                            new UndergroundCell(
                                x,
                                level));
                    }
                }
            }
        }

        var ores =
            new Dictionary<UndergroundCell, UndergroundOreSpawn>();

        DeterministicRandom oreRandom =
            new(
                worldSeed.Derive(
                    islandId,
                    "underground-ore-v1"));

        for (int level = -1;
             level >= IslandGenerationSettings.DeepestLogicalLevel;
             level--)
        {
            double depthRatio =
                Math.Abs(level)
                / 100.0;

            double baseChance =
                0.018
                + depthRatio * 0.032;

            if (biome == BiomeType.Mountain)
            {
                baseChance += 0.025;
            }

            for (int x = 2;
                 x < width - 2;
                 x++)
            {
                if (level > surfaceLevels[x])
                {
                    continue;
                }

                var cell =
                    new UndergroundCell(
                        x,
                        level);

                if (openCells.Contains(cell)
                    || !oreRandom.Chance(baseChance))
                {
                    continue;
                }

                int richness =
                    depthRatio > 0.55
                    && oreRandom.Chance(0.22)
                        ? 2
                        : 1;

                ores[cell] =
                    new UndergroundOreSpawn(
                        cell,
                        UndergroundOreKind.Iron,
                        richness);
            }
        }

        return new UndergroundGenerationResult(
            openCells,
            ores,
            shaftLeft,
            shaftWidth);
    }

    private IReadOnlyList<GeneratedStructureDefinition> GenerateStructures(
        WorldSeed worldSeed,
        int islandId,
        BiomeType biome,
        int[] surfaceLevels,
        int mineShaftLeftCell,
        int mineShaftWidthCells)
    {
        DeterministicRandom random =
            new(
                worldSeed.Derive(
                    islandId,
                    "generated-structures-v1"));

        int targetCount =
            islandId == 0
                ? 1
                : (random.Chance(0.62)
                    ? (random.Chance(0.18) ? 2 : 1)
                    : 0);

        if (targetCount == 0)
        {
            return Array.Empty<GeneratedStructureDefinition>();
        }

        var result =
            new List<GeneratedStructureDefinition>();

        int minimumX =
            _settings.BoatSiteWidthCells + 8;

        int maximumX =
            surfaceLevels.Length
            - _settings.BoatSiteWidthCells
            - 9;

        for (int structureIndex = 0;
             structureIndex < targetCount;
             structureIndex++)
        {
            GeneratedStructureKind kind =
                islandId == 0 && structureIndex == 0
                    ? GeneratedStructureKind.House
                    : (random.Chance(0.28)
                        ? GeneratedStructureKind.Castle
                        : GeneratedStructureKind.House);

            int widthCells =
                kind == GeneratedStructureKind.Castle
                    ? 10
                    : 6;

            int chosenCenter = -1;

            for (int attempt = 0;
                 attempt < 80;
                 attempt++)
            {
                int candidate =
                    random.NextInt(
                        minimumX,
                        maximumX + 1);

                int candidateStart =
                    candidate - widthCells / 2;

                int candidateEnd =
                    candidateStart + widthCells - 1;

                if (candidateStart < minimumX
                    || candidateEnd > maximumX)
                {
                    continue;
                }

                int shaftRight =
                    mineShaftLeftCell
                    + mineShaftWidthCells
                    - 1;

                if (candidateStart
                        <= shaftRight + 5
                    && candidateEnd
                        >= mineShaftLeftCell - 5)
                {
                    continue;
                }

                bool overlaps =
                    result.Any(
                        structure =>
                            Math.Abs(
                                structure.CenterCellX
                                - candidate)
                            < (structure.WidthCells
                                + widthCells)
                                / 2
                                + 4);

                if (overlaps)
                {
                    continue;
                }

                int minLevel =
                    int.MaxValue;
                int maxLevel =
                    int.MinValue;

                for (int x = candidateStart;
                     x <= candidateEnd;
                     x++)
                {
                    minLevel =
                        Math.Min(
                            minLevel,
                            surfaceLevels[x]);

                    maxLevel =
                        Math.Max(
                            maxLevel,
                            surfaceLevels[x]);
                }

                if (maxLevel - minLevel > 2)
                {
                    continue;
                }

                chosenCenter =
                    candidate;
                break;
            }

            if (chosenCenter < 0)
            {
                continue;
            }

            int baseSurfaceLevel =
                surfaceLevels[chosenCenter];

            int chestLogicalLevel =
                surfaceLevels[chosenCenter] + 1;

            var loot =
                GenerateStructureLoot(
                    random,
                    kind,
                    biome);

            int chestId =
                1_000
                + structureIndex;

            var chest =
                new GeneratedChestDefinition(
                    chestId,
                    chosenCenter,
                    chestLogicalLevel,
                    loot);

            result.Add(
                new GeneratedStructureDefinition(
                    structureIndex,
                    kind,
                    chosenCenter,
                    widthCells,
                    baseSurfaceLevel,
                    chest));
        }

        return result;
    }

    private static IReadOnlyDictionary<ItemType, int> GenerateStructureLoot(
        DeterministicRandom random,
        GeneratedStructureKind kind,
        BiomeType biome)
    {
        var loot =
            new Dictionary<ItemType, int>();

        if (kind == GeneratedStructureKind.House)
        {
            loot[ItemType.Wood] =
                random.NextInt(
                    10,
                    26);

            loot[ItemType.Sapling] =
                random.NextInt(
                    1,
                    5);

            loot[ItemType.GoldCoin] =
                random.NextInt(
                    5,
                    16);

            if (random.Chance(0.35))
            {
                loot[ItemType.Honey] =
                    random.NextInt(
                        1,
                        4);
            }
        }
        else
        {
            loot[ItemType.Stone] =
                random.NextInt(
                    15,
                    31);

            loot[ItemType.IronOre] =
                random.NextInt(
                    4,
                    10);

            loot[ItemType.GoldCoin] =
                random.NextInt(
                    25,
                    51);

            if (random.Chance(0.30))
            {
                loot[ItemType.IronBar] =
                    random.NextInt(
                        1,
                        4);
            }
        }

        if (biome == BiomeType.Desert
            && random.Chance(0.45))
        {
            loot[ItemType.Cactus] =
                random.NextInt(
                    2,
                    6);
        }

        return loot;
    }

    private IReadOnlyList<NaturalFeatureSpawn> GenerateFeatures(
        WorldSeed worldSeed,
        int islandId,
        BiomeType biome,
        int[] surfaceLevels)
    {
        DeterministicRandom random = new(worldSeed.Derive(islandId, "vegetation"));
        List<NaturalFeatureSpawn> result = new();
        int nextFeatureId = 0;

        int clearance = _settings.BoatSiteWidthCells + _settings.MinimumFeatureClearanceFromBoatSite;
        int endExclusive = surfaceLevels.Length - clearance;

        for (int x = clearance; x < endExclusive; x++)
        {
            if (!IsStableFeatureCell(surfaceLevels, x))
            {
                continue;
            }

            double roll = random.NextDouble();
            NaturalFeatureKind? kind = ChooseFeature(biome, roll);
            if (kind is null)
            {
                continue;
            }

            result.Add(new NaturalFeatureSpawn(
                nextFeatureId++,
                x,
                surfaceLevels[x],
                kind.Value,
                random.NextInt(0, 3)));
        }

        // Supplemental abundance pass. Original resources above keep their
        // original sequential feature IDs, so existing harvested save deltas
        // remain valid. New resources use stable cell-based IDs.
        HashSet<int> occupiedCells =
            result
                .Select(feature => feature.CellX)
                .ToHashSet();

        const int supplementalFeatureIdBase = 100_000;
        int supplementalClearance =
            Math.Max(
                3,
                _settings.MinimumFeatureClearanceFromBoatSite);

        int supplementalEndExclusive =
            surfaceLevels.Length - supplementalClearance;

        for (int x = supplementalClearance;
             x < supplementalEndExclusive;
             x++)
        {
            if (occupiedCells.Contains(x)
                || !IsSupplementalFeatureCell(
                    surfaceLevels,
                    x))
            {
                continue;
            }

            DeterministicRandom cellRandom =
                new(
                    worldSeed.Derive(
                        islandId,
                        $"vegetation-abundance-v1-{x}"));

            NaturalFeatureKind? kind =
                ChooseSupplementalFeature(
                    biome,
                    cellRandom.NextDouble());

            if (kind is null)
            {
                continue;
            }

            result.Add(
                new NaturalFeatureSpawn(
                    supplementalFeatureIdBase + x,
                    x,
                    surfaceLevels[x],
                    kind.Value,
                    cellRandom.NextInt(0, 3)));

            occupiedCells.Add(x);
        }

        return result
            .OrderBy(feature => feature.CellX)
            .ThenBy(feature => feature.FeatureId)
            .ToArray();
    }

    private static bool IsStableFeatureCell(int[] levels, int x)
    {
        return x > 0
            && x < levels.Length - 1
            && levels[x - 1] == levels[x]
            && levels[x + 1] == levels[x];
    }

    private static bool IsSupplementalFeatureCell(
        int[] levels,
        int x)
    {
        if (x <= 0
            || x >= levels.Length - 1)
        {
            return false;
        }

        return Math.Abs(levels[x - 1] - levels[x]) <= 1
            && Math.Abs(levels[x + 1] - levels[x]) <= 1;
    }

    private static NaturalFeatureKind? ChooseSupplementalFeature(
        BiomeType biome,
        double roll)
    {
        return biome switch
        {
            BiomeType.Forest => roll switch
            {
                < 0.28 => NaturalFeatureKind.Tree,
                < 0.50 => NaturalFeatureKind.Flower,
                < 0.56 => NaturalFeatureKind.Mushroom,
                < 0.70 => NaturalFeatureKind.Rock,
                _ => null,
            },

            BiomeType.Desert => roll switch
            {
                < 0.26 => NaturalFeatureKind.Cactus,
                < 0.46 => NaturalFeatureKind.Rock,
                < 0.55 => NaturalFeatureKind.Flower,
                _ => null,
            },

            BiomeType.Jungle => roll switch
            {
                < 0.30 => NaturalFeatureKind.Palm,
                < 0.50 => NaturalFeatureKind.Grass,
                < 0.62 => NaturalFeatureKind.Bush,
                < 0.68 => NaturalFeatureKind.Mushroom,
                < 0.76 => NaturalFeatureKind.Rock,
                _ => null,
            },

            BiomeType.Snowy => roll switch
            {
                < 0.34 => NaturalFeatureKind.Pine,
                < 0.55 => NaturalFeatureKind.Rock,
                < 0.62 => NaturalFeatureKind.Mushroom,
                _ => null,
            },

            BiomeType.Mountain => roll switch
            {
                < 0.58 => NaturalFeatureKind.Rock,
                < 0.70 => NaturalFeatureKind.Grass,
                _ => null,
            },

            _ => null,
        };
    }

    private static NaturalFeatureKind? ChooseFeature(BiomeType biome, double roll)
    {
        return biome switch
        {
            BiomeType.Forest => roll switch
            {
                < 0.18 => NaturalFeatureKind.Tree,
                < 0.27 => NaturalFeatureKind.Flower,
                < 0.31 => NaturalFeatureKind.Mushroom,
                < 0.37 => NaturalFeatureKind.Rock,
                _ => null,
            },
            BiomeType.Desert => roll switch
            {
                < 0.17 => NaturalFeatureKind.Cactus,
                < 0.22 => NaturalFeatureKind.Rock,
                < 0.245 => NaturalFeatureKind.Flower,
                _ => null,
            },
            BiomeType.Jungle => roll switch
            {
                < 0.21 => NaturalFeatureKind.Palm,
                < 0.36 => NaturalFeatureKind.Grass,
                < 0.44 => NaturalFeatureKind.Bush,
                < 0.49 => NaturalFeatureKind.Mushroom,
                _ => null,
            },
            BiomeType.Snowy => roll switch
            {
                < 0.23 => NaturalFeatureKind.Pine,
                < 0.31 => NaturalFeatureKind.Rock,
                < 0.34 => NaturalFeatureKind.Mushroom,
                _ => null,
            },
            BiomeType.Mountain => roll switch
            {
                < 0.30 => NaturalFeatureKind.Rock,
                < 0.34 => NaturalFeatureKind.Grass,
                _ => null,
            },
            _ => null,
        };
    }

    private sealed record UndergroundGenerationResult(
        IReadOnlySet<UndergroundCell> OpenCells,
        IReadOnlyDictionary<UndergroundCell, UndergroundOreSpawn> Ores,
        int MineShaftLeftCell,
        int MineShaftWidthCells);

    private readonly record struct TerrainProfile(
        int MinimumLevel,
        int MaximumLevel,
        double Ruggedness,
        double TargetPull,
        double NewTargetChance)
    {
        public static TerrainProfile ForBiome(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Forest => new(-3, 5, 0.24, 0.58, 0.05),
                BiomeType.Desert => new(-2, 3, 0.13, 0.48, 0.04),
                BiomeType.Jungle => new(-4, 6, 0.28, 0.58, 0.06),
                BiomeType.Snowy => new(-4, 6, 0.22, 0.55, 0.05),
                BiomeType.Mountain => new(-7, 12, 0.48, 0.68, 0.08),
                _ => new(-3, 5, 0.24, 0.58, 0.05),
            };
        }
    }
}
