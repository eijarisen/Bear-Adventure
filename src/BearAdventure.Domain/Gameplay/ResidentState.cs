namespace BearAdventure.Domain.Gameplay;

public sealed class ResidentState
{
    public ResidentState(int residentId, string name, ResidentKind kind, int originIslandId,
        int islandId, int cellX, int logicalLevel, int appearanceVariant)
    {
        if (residentId <= 0) throw new ArgumentOutOfRangeException(nameof(residentId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Resident name is required.", nameof(name));
        ResidentId = residentId;
        Name = name.Trim();
        Kind = kind;
        OriginIslandId = originIslandId;
        IslandId = islandId;
        CellX = cellX;
        LogicalLevel = logicalLevel;
        AppearanceVariant = Math.Max(0, appearanceVariant);
    }

    public int ResidentId { get; }
    public string Name { get; }
    public ResidentKind Kind { get; }
    public int OriginIslandId { get; }
    public int IslandId { get; set; }
    public int CellX { get; set; }
    public int LogicalLevel { get; set; }
    public int AppearanceVariant { get; }
    public ResidentStatus Status { get; set; } = ResidentStatus.World;
    public int Sympathy { get; set; }
    public int Coins { get; set; }
    public int MaxCoins { get; set; }
    public double CoinRegenSeconds { get; set; }
    public bool OffersDiamondAxe { get; set; }
    public bool OffersDiamondPickaxe { get; set; }
    public bool SpecialChestClaimed { get; set; }
    public WorkerState? Worker { get; set; }

    public bool Befriended => Sympathy >= 100;
    public bool Relocatable => Kind is ResidentKind.Bear or ResidentKind.Cat;
}
