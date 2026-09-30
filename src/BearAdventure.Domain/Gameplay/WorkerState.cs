using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public enum WorkerJob
{
    HarvestHoney,
    HarvestTrees,
    PlantTrees,
    MineStone,
    GatherMushrooms,
    HarvestCacti,
    GatherFlowers,
    GatherGrass,
}

public enum WorkerPhase
{
    Idle,
    TravellingToTarget,
    Working,
    ReturningToChest,
    Blocked,
}

public enum WorkerTargetKind
{
    None,
    NaturalFeature,
    PlacedObject,
    MineCell,
    PlantSpot,
}

public sealed class WorkerState
{
    private readonly Dictionary<WorkerJob,int> _priorities = new();

    public WorkerState()
    {
        foreach(WorkerJob job in Enum.GetValues<WorkerJob>())
            _priorities[job] = DefaultPriority(job);
    }

    public int HomeChestPlacementId { get; set; }
    public int WorkCenterCellX { get; set; }
    public int WorkCenterLogicalLevel { get; set; }
    public int WorkRadiusCells { get; set; } = 24;
    public bool ProtectHiveFlowers { get; set; } = true;
    public InventoryState Cargo { get; } = new();

    public WorkerPhase Phase { get; set; } = WorkerPhase.Idle;
    public WorkerJob TargetJob { get; set; }
    public WorkerTargetKind TargetKind { get; set; }
    public int TargetId { get; set; }
    public int TargetCellX { get; set; }
    public int TargetLogicalLevel { get; set; }
    public int TargetStandCellX { get; set; }
    public int TargetStandLogicalLevel { get; set; }

    public int CurrentCellX { get; set; }
    public int CurrentLogicalLevel { get; set; }
    public int TravelFromCellX { get; set; }
    public int TravelFromLogicalLevel { get; set; }
    public int TravelToCellX { get; set; }
    public int TravelToLogicalLevel { get; set; }
    public double TravelTotalSeconds { get; set; }
    public double RemainingSeconds { get; set; }
    public string BlockedReason { get; set; } = string.Empty;
    public long CompletedJobs { get; set; }

    public bool Assigned => HomeChestPlacementId > 0;
    public IReadOnlyDictionary<WorkerJob,int> Priorities => _priorities;

    public int Priority(WorkerJob job) => _priorities.TryGetValue(job,out int value) ? value : 0;

    public void SetPriority(WorkerJob job,int value)
    {
        if(!Enum.IsDefined(job) || value is <0 or >3) throw new ArgumentOutOfRangeException(nameof(value));
        _priorities[job]=value;
    }

    public int CyclePriority(WorkerJob job)
    {
        int next=(Priority(job)+1)%4;
        SetPriority(job,next);
        return next;
    }

    public void ReplacePriorities(IEnumerable<KeyValuePair<WorkerJob,int>> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _priorities.Clear();
        foreach(WorkerJob job in Enum.GetValues<WorkerJob>()) _priorities[job]=0;
        foreach(var (job,value) in values) SetPriority(job,value);
    }

    public void ClearTarget(string reason="")
    {
        Phase=string.IsNullOrWhiteSpace(reason)?WorkerPhase.Idle:WorkerPhase.Blocked;
        TargetKind=WorkerTargetKind.None;
        TargetId=0;
        TargetCellX=0;
        TargetLogicalLevel=0;
        TargetStandCellX=0;
        TargetStandLogicalLevel=0;
        TravelTotalSeconds=0;
        RemainingSeconds=0;
        BlockedReason=reason;
    }

    public WorldPoint PresentationPoint()
    {
        if((Phase is WorkerPhase.TravellingToTarget or WorkerPhase.ReturningToChest) && TravelTotalSeconds>1e-9)
        {
            double progress=Math.Clamp(1.0-RemainingSeconds/TravelTotalSeconds,0,1);
            var from=Feet(TravelFromCellX,TravelFromLogicalLevel);
            var to=Feet(TravelToCellX,TravelToLogicalLevel);
            return new WorldPoint(from.X+(to.X-from.X)*progress,from.Y+(to.Y-from.Y)*progress);
        }
        return Feet(CurrentCellX,CurrentLogicalLevel);
    }

    private static WorldPoint Feet(int cellX,int logicalLevel)
    {
        var rect=WorldGrid.CellRect(new(cellX,logicalLevel));
        return new WorldPoint(rect.Center.X,rect.Bottom);
    }

    public static int DefaultPriority(WorkerJob job) => job switch
    {
        WorkerJob.HarvestHoney => 3,
        WorkerJob.HarvestTrees => 3,
        WorkerJob.PlantTrees => 2,
        WorkerJob.MineStone => 2,
        WorkerJob.GatherMushrooms => 2,
        WorkerJob.HarvestCacti => 2,
        WorkerJob.GatherFlowers => 1,
        WorkerJob.GatherGrass => 1,
        _ => 0,
    };
}
