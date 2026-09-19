namespace BearAdventure.Domain.World;

public sealed record IslandGenerationSettings
{
    public const int HighestLogicalLevel = 100;
    public const int GroundReferenceLevel = 0;
    public const int DeepestLogicalLevel = -100;
    public const int TotalLogicalLevels = 201;

    // Provisional until island-length balance is playtested.
    public int MinimumWidthCells { get; init; } = 120;
    public int MaximumWidthCells { get; init; } = 180;

    public int BoatSiteWidthCells { get; init; } = 10;

    public int MinimumFeatureClearanceFromBoatSite { get; init; } = 3;

    public static IslandGenerationSettings Default { get; } = new();
}
