using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class WorkerService
{
    public static bool CanWork(ResidentState resident) => resident.Kind==ResidentKind.Bear && resident.Befriended;

    public static bool TryAssign(GameSessionState session, WorldQueries world, int residentId, int chestPlacementId, out string reason)
    {
        var resident=session.FindResident(residentId);
        if(resident is null || resident.Status!=ResidentStatus.World || resident.IslandId!=world.Definition.IslandId)
        { reason="That bear is not on this island."; return false; }
        if(!CanWork(resident)) { reason="Only a befriended ordinary bear can work."; return false; }
        var chest=world.State.PlacedObjects.FirstOrDefault(p=>p.PlacementId==chestPlacementId && p.Item==ItemType.Chest);
        if(chest is null) { reason="Choose an existing player-built chest."; return false; }
        bool created=resident.Worker is null;
        resident.Worker ??= new WorkerState();
        var worker=resident.Worker;
        worker.HomeChestPlacementId=chest.PlacementId;
        worker.WorkCenterCellX=chest.CellX;
        worker.WorkCenterLogicalLevel=chest.LogicalLevel;
        if(created || worker.CurrentCellX<0)
        {
            worker.CurrentCellX=resident.CellX;
            worker.CurrentLogicalLevel=resident.LogicalLevel;
        }
        worker.ClearTarget();
        reason=$"{resident.Name} will work around chest #{chest.PlacementId}.";
        return true;
    }

    public static bool TrySetRadius(GameSessionState session,int residentId,int radius,out string reason)
    {
        var worker=session.FindResident(residentId)?.Worker;
        if(worker is null) { reason="Worker is not configured."; return false; }
        if(radius is not (12 or 24 or 48 or 96)) { reason="Unsupported work radius."; return false; }
        worker.WorkRadiusCells=radius; worker.ClearTarget(); reason=$"Work radius set to {radius} cells."; return true;
    }

    public static bool TryCyclePriority(GameSessionState session,int residentId,WorkerJob job,out string reason)
    {
        var worker=session.FindResident(residentId)?.Worker;
        if(worker is null) { reason="Worker is not configured."; return false; }
        int value=worker.CyclePriority(job); worker.ClearTarget();
        reason=$"{Display(job)} priority: {PriorityName(value)}."; return true;
    }

    public static bool TryToggleProtection(GameSessionState session,int residentId,out string reason)
    {
        var worker=session.FindResident(residentId)?.Worker;
        if(worker is null) { reason="Worker is not configured."; return false; }
        worker.ProtectHiveFlowers=!worker.ProtectHiveFlowers; worker.ClearTarget();
        reason=worker.ProtectHiveFlowers?"Hive flowers are protected.":"Flower gathering may use hive gardens."; return true;
    }

    public static bool TryStop(GameSessionState session,int residentId,out string reason)
    {
        var worker=session.FindResident(residentId)?.Worker;
        if(worker is null) { reason="Worker is not configured."; return false; }
        worker.HomeChestPlacementId=0; worker.ClearTarget("No home chest assigned.");
        reason="Worker stopped. Cargo remains with the bear."; return true;
    }

    public static bool TryRecall(GameSessionState session,int residentId,out string reason)
    {
        var resident=session.FindResident(residentId);
        if(resident is null || resident.Kind!=ResidentKind.Bear || resident.Status!=ResidentStatus.World || resident.Worker is null)
        { reason="That worker is not available to recall."; return false; }
        resident.Worker.ClearTarget(); resident.Worker.HomeChestPlacementId=0; resident.Status=ResidentStatus.Roster;
        reason=resident.Worker.Cargo.IsEmpty ? $"{resident.Name} returned to the friends roster." :
            $"{resident.Name} returned with cargo preserved in the roster.";
        return true;
    }

    public static void OnPlacedFromRoster(ResidentState resident)
    {
        if(resident.Worker is null) return;
        resident.Worker.HomeChestPlacementId=0;
        resident.Worker.WorkCenterCellX=resident.CellX;
        resident.Worker.WorkCenterLogicalLevel=resident.LogicalLevel;
        resident.Worker.CurrentCellX=resident.CellX;
        resident.Worker.CurrentLogicalLevel=resident.LogicalLevel;
        resident.Worker.ClearTarget("Assign a home chest on this island.");
    }

    public static WorldPoint Position(ResidentState resident)
    {
        if(resident.Worker is not null && resident.Worker.Assigned && resident.Status==ResidentStatus.World)
            return resident.Worker.PresentationPoint();
        return WorldGrid.Center(resident.CellX,resident.LogicalLevel);
    }

    public static string Describe(ResidentState resident)
    {
        var worker=resident.Worker;
        if(worker is null) return "Not configured as a worker.";
        string phase=worker.Phase switch {
            WorkerPhase.TravellingToTarget=>"travelling to job", WorkerPhase.ReturningToChest=>"returning to chest",
            WorkerPhase.Working=>"working", WorkerPhase.Blocked=>"blocked", _=>"idle"};
        string cargo=worker.Cargo.IsEmpty?"empty cargo":string.Join(", ",worker.Cargo.Counts.Select(p=>$"{p.Value} {PlacementRules.GetDisplayName(p.Key)}"));
        return $"{phase}; chest #{worker.HomeChestPlacementId}; radius {worker.WorkRadiusCells}; {cargo}"+
            (string.IsNullOrWhiteSpace(worker.BlockedReason)?"":$"; {worker.BlockedReason}");
    }

    public static string Display(WorkerJob job) => job switch {
        WorkerJob.HarvestHoney=>"Honey", WorkerJob.HarvestTrees=>"Trees", WorkerJob.PlantTrees=>"Plant trees",
        WorkerJob.MineStone=>"Mining", WorkerJob.GatherMushrooms=>"Mushrooms", WorkerJob.HarvestCacti=>"Cacti",
        WorkerJob.GatherFlowers=>"Flowers", WorkerJob.GatherGrass=>"Grass", _=>job.ToString()};
    public static string PriorityName(int value) => value switch {0=>"Off",1=>"Low",2=>"Normal",3=>"High",_=>"Off"};
}
