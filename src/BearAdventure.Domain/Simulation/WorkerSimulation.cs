using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Simulation;

public static class WorkerSimulation
{
    public const double SecondsPerPathStep = 0.32;
    public const double MinimumTravelSeconds = 0.5;
    public const double HarvestWorkSeconds = 1.5;
    public const double GatherWorkSeconds = 0.65;
    public const double HoneyWorkSeconds = 0.45;
    public const double PlantWorkSeconds = 1.5;
    public const double MineWorkSeconds = 1.5;

    private static readonly InventoryState BareWorkerTools = new();

    public static bool AdvanceTick(GameSessionState session, IReadOnlyDictionary<int,WorldQueries> worlds, double tickSeconds)
    {
        bool changed=false;
        var reserved=BuildReservations(session);
        foreach(var resident in session.Residents.OrderBy(r=>r.ResidentId))
        {
            var worker=resident.Worker;
            if(resident.Kind!=ResidentKind.Bear || resident.Status!=ResidentStatus.World || worker is null || !worker.Assigned) continue;
            if(!worlds.TryGetValue(resident.IslandId,out var world)) continue;
            if(AdvanceWorker(session,resident,worker,world,reserved,tickSeconds)) changed=true;
        }
        return changed;
    }

    private static bool AdvanceWorker(GameSessionState session, ResidentState resident, WorkerState worker, WorldQueries world,
        HashSet<WorkerTargetKey> reserved, double dt)
    {
        var chest=HomeChest(world,worker);
        if(chest is null)
        {
            if(worker.Phase!=WorkerPhase.Blocked || worker.BlockedReason!="Home chest is missing.")
            { worker.ClearTarget("Home chest is missing."); return true; }
            return false;
        }

        switch(worker.Phase)
        {
            case WorkerPhase.TravellingToTarget:
            case WorkerPhase.ReturningToChest:
                if(!ValidateTravelDestination(world,worker)) { ReleaseReservation(resident.IslandId,worker,reserved); worker.ClearTarget("Route changed; recalculating."); return true; }
                worker.RemainingSeconds=Math.Max(0,worker.RemainingSeconds-dt);
                if(worker.RemainingSeconds>1e-9) return true;
                worker.CurrentCellX=worker.TravelToCellX; worker.CurrentLogicalLevel=worker.TravelToLogicalLevel;
                if(worker.Phase==WorkerPhase.ReturningToChest)
                {
                    if(!ReferenceEquals(chest,HomeChest(world,worker))) { worker.ClearTarget("Home chest moved or was removed."); return true; }
                    if(!worker.Cargo.IsEmpty)
                    {
                        if(!InventoryTransactions.TryTransfer(worker.Cargo,chest.Contents,worker.Cargo.Snapshot(),out var transferReason))
                        { worker.ClearTarget(transferReason); return true; }
                        world.State.Touch();
                    }
                    ReleaseReservation(resident.IslandId,worker,reserved); worker.ClearTarget(); return true;
                }
                worker.Phase=WorkerPhase.Working; worker.RemainingSeconds=WorkSeconds(worker.TargetJob); worker.TravelTotalSeconds=0;
                return true;

            case WorkerPhase.Working:
                if(!ValidateTarget(world,worker)) { ReleaseReservation(resident.IslandId,worker,reserved); worker.ClearTarget("Target changed before work finished."); return true; }
                worker.RemainingSeconds=Math.Max(0,worker.RemainingSeconds-dt);
                if(worker.RemainingSeconds>1e-9) return true;
                bool performed=PerformJob(world,worker,chest,out string jobReason);
                ReleaseReservation(resident.IslandId,worker,reserved);
                if(!performed) { worker.ClearTarget(jobReason); return true; }
                worker.CompletedJobs++;
                if(worker.Cargo.IsEmpty) { worker.ClearTarget(); return true; }
                return StartReturn(world,worker,chest,out _);

            case WorkerPhase.Blocked:
            case WorkerPhase.Idle:
            default:
                if(!worker.Cargo.IsEmpty) return StartReturn(world,worker,chest,out _);
                if(TryChooseTarget(world,worker,reserved,out var candidate,out string blocked))
                {
                    Reserve(candidate.Key,reserved);
                    StartTarget(worker,candidate);
                    return true;
                }
                string message=string.IsNullOrWhiteSpace(blocked)?"No reachable enabled jobs in the work area.":blocked;
                if(worker.Phase!=WorkerPhase.Blocked || worker.BlockedReason!=message)
                { worker.ClearTarget(message); return true; }
                return false;
        }
    }

    private static bool TryChooseTarget(WorldQueries world, WorkerState worker, HashSet<WorkerTargetKey> reserved,
        out WorkerCandidate candidate, out string reason)
    {
        candidate=default; reason=string.Empty;
        var start=new UndergroundCell(worker.CurrentCellX,worker.CurrentLogicalLevel);
        var distances=WorkerNavigation.Distances(world,start);
        if(distances.Count==0) { reason="Worker has no supported route from its current position."; return false; }
        foreach(var job in Enum.GetValues<WorkerJob>().OrderByDescending(worker.Priority).ThenBy(j=>(int)j))
        {
            if(worker.Priority(job)<=0) continue;
            WorkerCandidate? best=null;
            foreach(var c in Candidates(world,worker,job,reserved,distances))
            {
                if(!distances.TryGetValue(c.StandCell,out int steps)) continue;
                var withPath=c with {PathSteps=steps};
                if(best is null || steps<best.Value.PathSteps) best=withPath;
            }
            if(best.HasValue) { candidate=best.Value; return true; }
        }
        reason="No reachable enabled jobs in the work area."; return false;
    }

    private static IEnumerable<WorkerCandidate> Candidates(WorldQueries world, WorkerState worker, WorkerJob job,
        HashSet<WorkerTargetKey> reserved, IReadOnlyDictionary<UndergroundCell,int> distances)
    {
        bool InArea(int x,int level) => Math.Abs(x-worker.WorkCenterCellX)<=worker.WorkRadiusCells &&
            Math.Abs(level-worker.WorkCenterLogicalLevel)<=worker.WorkRadiusCells;

        if(job==WorkerJob.HarvestHoney)
        {
            foreach(var p in world.State.PlacedObjects.Where(p=>p.Item==ItemType.Beehive && p.StoredOutput>0 && InArea(p.CellX,p.LogicalLevel)))
            {
                var key=new WorkerTargetKey(world.Definition.IslandId,WorkerTargetKind.PlacedObject,p.PlacementId,p.CellX,p.LogicalLevel);
                if(reserved.Contains(key)) continue;
                var target=new UndergroundCell(p.CellX,p.LogicalLevel);
                if(WorkerNavigation.TryReachAdjacent(world,distances,target,out var stand,out int steps))
                    yield return new(job,WorkerTargetKind.PlacedObject,p.PlacementId,target,stand,steps,key);
            }
            yield break;
        }

        if(job is WorkerJob.HarvestTrees or WorkerJob.GatherFlowers or WorkerJob.GatherGrass or WorkerJob.GatherMushrooms or WorkerJob.HarvestCacti or WorkerJob.MineStone)
        {
            foreach(var f in world.Definition.NaturalFeatures)
            {
                if(!world.NaturalPresent(f) || !InArea(f.CellX,f.SurfaceLevel+1) || !Matches(job,f.Kind) ||
                    (job==WorkerJob.GatherFlowers && worker.ProtectHiveFlowers && ProtectedFlower(world,f.CellX,f.SurfaceLevel+1))) continue;
                var key=new WorkerTargetKey(world.Definition.IslandId,WorkerTargetKind.NaturalFeature,f.FeatureId,f.CellX,f.SurfaceLevel+1);
                if(reserved.Contains(key)) continue;
                var stand=new UndergroundCell(f.CellX,f.SurfaceLevel+1);
                if(distances.TryGetValue(stand,out int steps)) yield return new(job,WorkerTargetKind.NaturalFeature,f.FeatureId,stand,stand,steps,key);
            }
            foreach(var p in world.State.PlacedObjects)
            {
                bool match=job switch {
                    WorkerJob.HarvestTrees=>p.Item==ItemType.Sapling && p.PlantKind!=NaturalFeatureKind.Cactus && PlacedHarvestRules.IsHarvestable(p),
                    WorkerJob.HarvestCacti=>p.Item==ItemType.Sapling && p.PlantKind==NaturalFeatureKind.Cactus && PlacedHarvestRules.IsHarvestable(p),
                    WorkerJob.GatherFlowers=>PlacementRules.IsFlower(p.Item),
                    WorkerJob.GatherGrass=>p.Item==ItemType.Grass,
                    _=>false};
                if(!match || !InArea(p.CellX,p.LogicalLevel) || !PlacedHarvestRules.IsHarvestable(p) || !world.PlantCanGrow(p) ||
                    (job==WorkerJob.GatherFlowers && worker.ProtectHiveFlowers && ProtectedFlower(world,p.CellX,p.LogicalLevel))) continue;
                var key=new WorkerTargetKey(world.Definition.IslandId,WorkerTargetKind.PlacedObject,p.PlacementId,p.CellX,p.LogicalLevel);
                if(reserved.Contains(key)) continue;
                var target=new UndergroundCell(p.CellX,p.LogicalLevel);
                if(WorkerNavigation.TryReachAdjacent(world,distances,target,out var stand,out int steps))
                    yield return new(job,WorkerTargetKind.PlacedObject,p.PlacementId,target,stand,steps,key);
            }
        }

        if(job==WorkerJob.PlantTrees)
        {
            var chest=HomeChest(world,worker);
            if(chest is null || !chest.Contents.Has(ItemType.Sapling)) yield break;
            for(int x=Math.Max(WorldGrid.ShoreCells,worker.WorkCenterCellX-worker.WorkRadiusCells);
                x<=Math.Min(world.Definition.WidthCells-WorldGrid.ShoreCells-1,worker.WorkCenterCellX+worker.WorkRadiusCells);x++)
            {
                int level=world.Definition.SurfaceLevels[x]+1;
                if(!InArea(x,level)) continue;
                var cell=new UndergroundCell(x,level);
                if(!world.CanPlace(ItemType.Sapling,cell,BuildLayer.Solid,new WorldRect(-100000,-100000,1,1),out _)) continue;
                var key=new WorkerTargetKey(world.Definition.IslandId,WorkerTargetKind.PlantSpot,0,x,level);
                if(reserved.Contains(key)) continue;
                if(WorkerNavigation.TryReachAdjacent(world,distances,cell,out var stand,out int steps))
                    yield return new(job,WorkerTargetKind.PlantSpot,0,cell,stand,steps,key);
            }
            yield break;
        }

        if(job==WorkerJob.MineStone)
        {
            int x0=Math.Max(0,worker.WorkCenterCellX-worker.WorkRadiusCells), x1=Math.Min(world.Definition.WidthCells-1,worker.WorkCenterCellX+worker.WorkRadiusCells);
            int l0=Math.Max(IslandGenerationSettings.DeepestLogicalLevel,worker.WorkCenterLogicalLevel-worker.WorkRadiusCells);
            int l1=Math.Min(IslandGenerationSettings.HighestLogicalLevel,worker.WorkCenterLogicalLevel+worker.WorkRadiusCells);
            var raw=new List<(UndergroundCell Cell,int Estimate)>();
            for(int x=x0;x<=x1;x++) for(int level=l0;level<=l1;level++)
            {
                var cell=new UndergroundCell(x,level);
                if(world.CanMine(cell,out _)) raw.Add((cell,Heuristic(new(worker.CurrentCellX,worker.CurrentLogicalLevel),cell)));
            }
            foreach(var (cell,_) in raw.OrderBy(p=>p.Estimate).Take(64))
            {
                var key=new WorkerTargetKey(world.Definition.IslandId,WorkerTargetKind.MineCell,0,cell.CellX,cell.LogicalLevel);
                if(reserved.Contains(key)) continue;
                if(WorkerNavigation.TryReachAdjacent(world,distances,cell,out var stand,out int steps))
                    yield return new(job,WorkerTargetKind.MineCell,0,cell,stand,steps,key);
            }
        }
    }

    private static bool Matches(WorkerJob job,NaturalFeatureKind kind) => job switch {
        WorkerJob.HarvestTrees=>kind is NaturalFeatureKind.Tree or NaturalFeatureKind.Pine or NaturalFeatureKind.Palm,
        WorkerJob.GatherFlowers=>kind==NaturalFeatureKind.Flower,
        WorkerJob.GatherGrass=>kind==NaturalFeatureKind.Grass,
        WorkerJob.GatherMushrooms=>kind==NaturalFeatureKind.Mushroom,
        WorkerJob.HarvestCacti=>kind==NaturalFeatureKind.Cactus,
        WorkerJob.MineStone=>kind==NaturalFeatureKind.Rock,
        _=>false};

    private static bool ProtectedFlower(WorldQueries world,int x,int level) => world.State.PlacedObjects.Any(p=>p.Item==ItemType.Beehive &&
        Math.Abs(p.CellX-x)<=WorldSimulationService.HoneyFlowerRadiusCells && Math.Abs(p.LogicalLevel-level)<=WorldSimulationService.HoneyFlowerVerticalCells);

    private static void StartTarget(WorkerState worker,WorkerCandidate c)
    {
        worker.TargetJob=c.Job; worker.TargetKind=c.Kind; worker.TargetId=c.TargetId; worker.TargetCellX=c.TargetCell.CellX;
        worker.TargetLogicalLevel=c.TargetCell.LogicalLevel; worker.TargetStandCellX=c.StandCell.CellX; worker.TargetStandLogicalLevel=c.StandCell.LogicalLevel;
        worker.BlockedReason=string.Empty;
        StartTravel(worker,c.StandCell,Math.Max(MinimumTravelSeconds,c.PathSteps*SecondsPerPathStep),WorkerPhase.TravellingToTarget);
    }

    private static bool StartReturn(WorldQueries world,WorkerState worker,PlacedObjectState chest,out string reason)
    {
        worker.TargetKind=WorkerTargetKind.None; worker.TargetId=0;
        var from=new UndergroundCell(worker.CurrentCellX,worker.CurrentLogicalLevel);
        var target=new UndergroundCell(chest.CellX,chest.LogicalLevel);
        if(!WorkerNavigation.TryReachAdjacent(world,from,target,out var stand,out int steps))
        { worker.ClearTarget("Home chest is unreachable. Cargo preserved."); reason=worker.BlockedReason; return true; }
        StartTravel(worker,stand,Math.Max(MinimumTravelSeconds,steps*SecondsPerPathStep),WorkerPhase.ReturningToChest);
        reason=string.Empty; return true;
    }

    private static void StartTravel(WorkerState worker,UndergroundCell destination,double seconds,WorkerPhase phase)
    {
        worker.TravelFromCellX=worker.CurrentCellX; worker.TravelFromLogicalLevel=worker.CurrentLogicalLevel;
        worker.TravelToCellX=destination.CellX; worker.TravelToLogicalLevel=destination.LogicalLevel;
        worker.TravelTotalSeconds=seconds; worker.RemainingSeconds=seconds; worker.Phase=phase;
    }

    private static bool ValidateTravelDestination(WorldQueries world,WorkerState worker)
    {
        var from=new UndergroundCell(worker.CurrentCellX,worker.CurrentLogicalLevel);
        var to=new UndergroundCell(worker.TravelToCellX,worker.TravelToLogicalLevel);
        return WorkerNavigation.TryPathLength(world,from,to,out _);
    }

    private static bool ValidateTarget(WorldQueries world,WorkerState worker) => worker.TargetKind switch {
        WorkerTargetKind.NaturalFeature=>world.Definition.NaturalFeatures.Any(f=>f.FeatureId==worker.TargetId && world.NaturalPresent(f)),
        WorkerTargetKind.PlacedObject=>world.State.PlacedObjects.Any(p=>p.PlacementId==worker.TargetId &&
            (worker.TargetJob==WorkerJob.HarvestHoney?p.Item==ItemType.Beehive && p.StoredOutput>0:PlacedHarvestRules.IsHarvestable(p) && world.PlantCanGrow(p))),
        WorkerTargetKind.MineCell=>world.CanMine(new(worker.TargetCellX,worker.TargetLogicalLevel),out _),
        WorkerTargetKind.PlantSpot=>world.CanPlace(ItemType.Sapling,new(worker.TargetCellX,worker.TargetLogicalLevel),BuildLayer.Solid,new WorldRect(-100000,-100000,1,1),out _),
        _=>false};

    private static bool PerformJob(WorldQueries world,WorkerState worker,PlacedObjectState chest,out string reason)
    {
        reason=string.Empty;
        switch(worker.TargetKind)
        {
            case WorkerTargetKind.NaturalFeature:
            {
                var found=world.Definition.NaturalFeatures.Where(f=>f.FeatureId==worker.TargetId).ToArray();
                if(found.Length!=1 || !world.NaturalPresent(found[0])) { reason="Resource was already taken."; return false; }
                var f=found[0];
                var rewards=HarvestRules.GetRewards(f,BareWorkerTools,world.Definition.Biome);
                if(!InventoryTransactions.TryGrant(worker.Cargo,rewards,out reason)) return false;
                world.State.MarkNaturalFeatureHarvested(f.FeatureId,NaturalRegrowthRules.GetRegrowSeconds(f.Kind));
                return true;
            }
            case WorkerTargetKind.PlacedObject:
            {
                var p=world.State.PlacedObjects.FirstOrDefault(p=>p.PlacementId==worker.TargetId);
                if(p is null) { reason="Target no longer exists."; return false; }
                if(worker.TargetJob==WorkerJob.HarvestHoney)
                {
                    if(p.Item!=ItemType.Beehive || p.StoredOutput<=0) { reason="Hive is no longer ready."; return false; }
                    worker.Cargo.Add(ItemType.Honey,p.StoredOutput); p.StoredOutput=0; p.ProductionSeconds=0; world.State.Touch(); return true;
                }
                if(!PlacedHarvestRules.IsHarvestable(p) || !world.PlantCanGrow(p)) { reason="Plant is no longer harvestable."; return false; }
                var rewards=PlacedHarvestRules.GetRewards(p,BareWorkerTools);
                if(!InventoryTransactions.TryGrant(worker.Cargo,rewards,out reason)) return false;
                world.State.RemovePlacedObject(p.PlacementId); return true;
            }
            case WorkerTargetKind.MineCell:
            {
                var cell=new UndergroundCell(worker.TargetCellX,worker.TargetLogicalLevel);
                if(!world.CanMine(cell,out reason)) return false;
                UndergroundOreSpawn? ore=world.Definition.UndergroundOres.TryGetValue(cell,out var found)?found:null;
                if(!InventoryTransactions.TryGrant(worker.Cargo,MiningRules.GetRewards(ore,BareWorkerTools),out reason)) return false;
                world.State.MarkUndergroundCellMined(cell); return true;
            }
            case WorkerTargetKind.PlantSpot:
            {
                var cell=new UndergroundCell(worker.TargetCellX,worker.TargetLogicalLevel);
                if(!chest.Contents.Has(ItemType.Sapling)) { reason="Home chest has no saplings."; return false; }
                if(!world.CanPlace(ItemType.Sapling,cell,BuildLayer.Solid,new WorldRect(-100000,-100000,1,1),out reason)) return false;
                chest.Contents.Add(ItemType.Sapling,-1);
                var planted=world.State.AddPlacedObject(ItemType.Sapling,cell.CellX,cell.LogicalLevel,BuildLayer.Solid);
                planted.PlantKind=WorldQueries.PlantedKind(world.Definition.Biome); world.State.Touch(); return true;
            }
            default: reason="Unknown worker target."; return false;
        }
    }

    private static PlacedObjectState? HomeChest(WorldQueries world,WorkerState worker) => world.State.PlacedObjects.FirstOrDefault(
        p=>p.PlacementId==worker.HomeChestPlacementId && p.Item==ItemType.Chest);

    private static double WorkSeconds(WorkerJob job) => job switch {
        WorkerJob.HarvestHoney=>HoneyWorkSeconds, WorkerJob.PlantTrees=>PlantWorkSeconds, WorkerJob.MineStone=>MineWorkSeconds,
        WorkerJob.GatherFlowers or WorkerJob.GatherGrass or WorkerJob.GatherMushrooms=>GatherWorkSeconds, _=>HarvestWorkSeconds};

    private static int Heuristic(UndergroundCell a,UndergroundCell b) => Math.Abs(a.CellX-b.CellX)+Math.Abs(a.LogicalLevel-b.LogicalLevel);

    private static HashSet<WorkerTargetKey> BuildReservations(GameSessionState session)
    {
        var result=new HashSet<WorkerTargetKey>();
        foreach(var resident in session.Residents)
        {
            var w=resident.Worker;
            if(resident.Status!=ResidentStatus.World || w is null || w.TargetKind==WorkerTargetKind.None) continue;
            result.Add(new(resident.IslandId,w.TargetKind,w.TargetId,w.TargetCellX,w.TargetLogicalLevel));
        }
        return result;
    }
    private static void ReleaseReservation(int islandId,WorkerState worker,HashSet<WorkerTargetKey> reserved) => reserved.Remove(new(
        islandId,worker.TargetKind,worker.TargetId,worker.TargetCellX,worker.TargetLogicalLevel));
    private static void Reserve(WorkerTargetKey key,HashSet<WorkerTargetKey> reserved) => reserved.Add(key);

    private readonly record struct WorkerCandidate(WorkerJob Job,WorkerTargetKind Kind,int TargetId,UndergroundCell TargetCell,
        UndergroundCell StandCell,int PathSteps,WorkerTargetKey Key);
    private readonly record struct WorkerTargetKey(int IslandId,WorkerTargetKind Kind,int TargetId,int CellX,int LogicalLevel);
}
