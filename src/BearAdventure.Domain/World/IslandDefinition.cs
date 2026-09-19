namespace BearAdventure.Domain.World;

public sealed class IslandDefinition
{
    public required int IslandId { get; init; }

    public required ulong IslandSeed { get; init; }

    public required int GeneratorVersion { get; init; }

    public required BiomeType Biome { get; init; }

    public required int WidthCells { get; init; }

    public required int BoatSiteWidthCells { get; init; }

    public required int[] SurfaceLevels { get; init; }

    public required IReadOnlyList<NaturalFeatureSpawn> NaturalFeatures { get; init; }

    public required IReadOnlySet<UndergroundCell> UndergroundOpenCells { get; init; }

    public required IReadOnlyDictionary<UndergroundCell, UndergroundOreSpawn> UndergroundOres { get; init; }

    public required int MineShaftLeftCell { get; init; }

    public required int MineShaftWidthCells { get; init; }

    public required IReadOnlyList<GeneratedStructureDefinition> GeneratedStructures { get; init; }

    public int LeftBoatSiteStartCell => 0;

    public int RightBoatSiteStartCell =>
        WidthCells - BoatSiteWidthCells;

    public int SuggestedSpawnCell =>
        BoatSiteWidthCells + 2;
}
