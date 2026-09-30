using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;
using BearAdventure.Domain.Simulation;

namespace BearAdventure.Persistence;

public sealed class GameSaveData
{
    public const int CurrentFormatVersion = 4;
    public const string AcceptedGenerationProfile = "surface-v1+abundance-v1+caves-v1+structures-v1";
    public int FormatVersion { get; set; }
    public int GeneratorVersion { get; set; }
    public string GenerationProfile { get; set; } = string.Empty;
    public string WorldSeed { get; set; } = string.Empty;
    public int CurrentIslandId { get; set; }
    public List<int> DiscoveredIslandIds { get; set; } = new() { 0 };
    public Dictionary<string,int> Inventory { get; set; } = new();
    public Dictionary<int,IslandSaveData> Islands { get; set; } = new();
    public Dictionary<int,IslandBaselineData> Baselines { get; set; } = new();
    public PlayerLocation? PlayerLocation { get; set; }
    public long SimulationTick { get; set; }
    public double SimulationRemainder { get; set; }
    public List<ResidentSaveData> Residents { get; set; } = new();

    public static GameSaveData FromSession(string seed, GameSessionState session)
    {
        if (seed!=session.Seed) throw new InvalidDataException("Session seed mismatch.");
        return FromSession(session);
    }
    public static GameSaveData FromSession(GameSessionState session)
    {
        var generator=new IslandGenerator();
        foreach(int id in session.DiscoveredIslandIds.OrderBy(id=>id))
            ResidentService.EnsureForIsland(session,session.GetDefinition(id,generator));
        var data=new GameSaveData {
            FormatVersion=CurrentFormatVersion,
            GeneratorVersion=BearAdventure.Domain.World.WorldSeed.GeneratorVersion,
            GenerationProfile=AcceptedGenerationProfile, WorldSeed=session.Seed,
            CurrentIslandId=session.CurrentIslandId,
            DiscoveredIslandIds=session.DiscoveredIslandIds.OrderBy(id=>id).ToList(),
            Inventory=WriteInventory(session.Inventory), PlayerLocation=session.PlayerLocation,
            SimulationTick=session.SimulationTick, SimulationRemainder=session.SimulationRemainder,
            Residents=session.Residents.OrderBy(r=>r.ResidentId).Select(ResidentSaveData.From).ToList() };
        foreach (int id in data.DiscoveredIslandIds)
        {
            data.Baselines[id]=IslandBaselineData.From(session.GetDefinition(id,generator));
            data.Islands[id]=IslandSaveData.From(session.GetIslandState(id));
        }
        return data;
    }
    public GameSessionState ToSession()
    {
        Require(FormatVersion is 1 or 2 or 3 or CurrentFormatVersion,$"Unsupported save format {FormatVersion}.");
        Require(GeneratorVersion==BearAdventure.Domain.World.WorldSeed.GeneratorVersion,"Unsupported generator version.");
        Require(!string.IsNullOrWhiteSpace(WorldSeed) && WorldSeed.Length<=4096,"Invalid world seed.");
        Require(Inventory is not null && Islands is not null && DiscoveredIslandIds is not null,"Missing save collections.");
        Require(SimulationTick>=0 && SimulationTick < long.MaxValue-1_000_000,"Invalid simulation clock.");
        Require(double.IsFinite(SimulationRemainder) && SimulationRemainder>=0 && SimulationRemainder<WorldSimulationService.TickSeconds,"Invalid simulation remainder.");
        if(FormatVersion>=2) Require(GenerationProfile==AcceptedGenerationProfile && Baselines is not null,"Unknown generation profile.");
        if(FormatVersion>=3) Require(Residents is not null,"Missing resident collection.");
        var session=new GameSessionState(ReadInventory(Inventory!),WorldSeed) { CurrentIslandId=CurrentIslandId,
            SimulationTick=SimulationTick, SimulationRemainder=SimulationRemainder };
        // Version 1 sometimes omitted untouched visited islands. Preserve every recorded identity.
        var ids=DiscoveredIslandIds!.Concat(Islands!.Keys).Append(CurrentIslandId).Append(0).Distinct().OrderBy(id=>id).ToArray();
        session.ReplaceDiscoveredIslands(ids);
        var generator=new IslandGenerator();
        foreach(int id in ids)
        {
            IslandDefinition definition;
            if(FormatVersion==1)
                definition=generator.Generate(new BearAdventure.Domain.World.WorldSeed(WorldSeed),id);
            else
            {
                Require(Baselines!.TryGetValue(id,out var snapshot) && snapshot is not null,$"Missing baseline for island {id}.");
                definition=snapshot!.ToDefinition(id);
            }
            session.SetBaseline(definition);
            var saved=Islands.TryGetValue(id,out var value) ? value : new IslandSaveData();
            Require(saved is not null,$"Null island state {id}.");
            session.SetIslandState(id,saved!.ToState(definition,FormatVersion));
        }
        if(FormatVersion>=3)
        {
            var residents=new List<ResidentState>(); var residentIds=new HashSet<int>();
            foreach(var savedResident in Residents!)
            {
                Require(savedResident is not null,"Null resident.");
                var resident=savedResident!.ToState(ids);
                Require(residentIds.Add(resident.ResidentId),"Duplicate resident identity.");
                int definitionId=resident.Status==ResidentStatus.World || resident.Worker is not null ? resident.IslandId : resident.OriginIslandId;
                if(resident.Worker is not null) Require(ids.Contains(resident.IslandId),"Worker last island is not discovered.");
                var residentDefinition=session.GetDefinition(definitionId,generator);
                Require(resident.CellX>=0 && resident.CellX<residentDefinition.WidthCells,"Resident is outside the island baseline.");
                if(resident.Status==ResidentStatus.World && resident.Worker is null)
                    Require(resident.LogicalLevel==residentDefinition.SurfaceLevels[resident.CellX]+1,"World resident is not anchored to its saved surface.");
                if(resident.Worker is { } worker)
                {
                    Require(resident.Kind==ResidentKind.Bear && resident.Befriended,"Only befriended ordinary bears may carry worker state.");
                    Require(worker.CurrentCellX>=0 && worker.CurrentCellX<residentDefinition.WidthCells &&
                        worker.WorkCenterCellX>=0 && worker.WorkCenterCellX<residentDefinition.WidthCells,
                        "Worker is outside the island baseline.");
                }
                residents.Add(resident);
            }
            session.ReplaceResidents(residents);
        }
        else
        {
            foreach(int id in ids) ResidentService.EnsureForIsland(session,session.GetDefinition(id,generator));
        }
        // New schema saves still guarantee a resident for every discovered island.
        foreach(int id in ids) ResidentService.EnsureForIsland(session,session.GetDefinition(id,generator));
        // An impossible/obsolete player position uses a safe spawn; it does not invalidate other progress.
        if(PlayerLocation is not null && PlayerLocation.IslandId==CurrentIslandId &&
            double.IsFinite(PlayerLocation.X) && double.IsFinite(PlayerLocation.Y)) session.PlayerLocation=PlayerLocation;
        return session;
    }
    internal static Dictionary<string,int> WriteInventory(InventoryState inventory) => inventory.Counts
        .OrderBy(p=>p.Key.ToString(),StringComparer.Ordinal).ToDictionary(p=>p.Key.ToString(),p=>p.Value);
    internal static InventoryState ReadInventory(Dictionary<string,int> entries)
    {
        Require(entries is not null,"Missing inventory.");
        var result=new InventoryState(); var seen=new HashSet<ItemType>();
        foreach(var (name,count) in entries!)
        {
            var item=Parse<ItemType>(name);
            Require(count>=0 && seen.Add(item),"Invalid or duplicate inventory quantity.");
            result.Set(item,count);
        }
        return result;
    }
    internal static T Parse<T>(string value) where T:struct,Enum
    {
        Require(Enum.TryParse<T>(value,false,out var parsed) && Enum.IsDefined(parsed),$"Unknown {typeof(T).Name}: {value}.");
        return parsed;
    }
    internal static void Require([System.Diagnostics.CodeAnalysis.DoesNotReturnIf(false)] bool condition,string message) { if(!condition) throw new InvalidDataException(message); }
}

public sealed class ResidentSaveData
{
    public int ResidentId {get;set;}
    public string Name {get;set;} = string.Empty;
    public string Kind {get;set;} = string.Empty;
    public string Status {get;set;} = ResidentStatus.World.ToString();
    public int OriginIslandId {get;set;}
    public int IslandId {get;set;}
    public int CellX {get;set;}
    public int LogicalLevel {get;set;}
    public int AppearanceVariant {get;set;}
    public int Sympathy {get;set;}
    public int Coins {get;set;}
    public int MaxCoins {get;set;}
    public double CoinRegenSeconds {get;set;}
    public bool OffersDiamondAxe {get;set;}
    public bool OffersDiamondPickaxe {get;set;}
    public bool SpecialChestClaimed {get;set;}
    public WorkerSaveData? Worker {get;set;}

    internal static ResidentSaveData From(ResidentState r) => new() { ResidentId=r.ResidentId,Name=r.Name,Kind=r.Kind.ToString(),
        Status=r.Status.ToString(),OriginIslandId=r.OriginIslandId,IslandId=r.IslandId,CellX=r.CellX,LogicalLevel=r.LogicalLevel,
        AppearanceVariant=r.AppearanceVariant,Sympathy=r.Sympathy,Coins=r.Coins,MaxCoins=r.MaxCoins,CoinRegenSeconds=r.CoinRegenSeconds,
        OffersDiamondAxe=r.OffersDiamondAxe,OffersDiamondPickaxe=r.OffersDiamondPickaxe,SpecialChestClaimed=r.SpecialChestClaimed,
        Worker=r.Worker is null?null:WorkerSaveData.From(r.Worker) };

    internal ResidentState ToState(IReadOnlyCollection<int> discovered)
    {
        GameSaveData.Require(ResidentId>0 && Name is not null && Name.Length is >0 and <=64,"Invalid resident identity/name.");
        var kind=GameSaveData.Parse<ResidentKind>(Kind); var status=GameSaveData.Parse<ResidentStatus>(Status);
        GameSaveData.Require(discovered.Contains(OriginIslandId),"Resident origin island is not discovered.");
        GameSaveData.Require(status==ResidentStatus.Roster || discovered.Contains(IslandId),"Resident island is not discovered.");
        GameSaveData.Require(CellX>=0 && CellX<4096 && LogicalLevel>=-100 && LogicalLevel<=100 && AppearanceVariant>=0 && AppearanceVariant<=100,
            "Invalid resident location/appearance.");
        GameSaveData.Require(Sympathy>=0 && Sympathy<=100 && Coins>=0 && MaxCoins>=0 && Coins<=MaxCoins && MaxCoins<=1_000_000,
            "Invalid resident relationship/economy.");
        GameSaveData.Require(double.IsFinite(CoinRegenSeconds) && CoinRegenSeconds>=0 && CoinRegenSeconds<20.000001,"Invalid resident restock timer.");
        GameSaveData.Require(status!=ResidentStatus.Roster || kind is ResidentKind.Bear or ResidentKind.Cat,"Only bears and cats may enter the friends roster.");
        return new ResidentState(ResidentId,Name,kind,OriginIslandId,IslandId,CellX,LogicalLevel,AppearanceVariant) { Status=status,Sympathy=Sympathy,
            Coins=Coins,MaxCoins=MaxCoins,CoinRegenSeconds=CoinRegenSeconds,OffersDiamondAxe=OffersDiamondAxe,
            OffersDiamondPickaxe=OffersDiamondPickaxe,SpecialChestClaimed=SpecialChestClaimed,Worker=Worker?.ToState() };
    }
}

public sealed class WorkerSaveData
{
    public int HomeChestPlacementId {get;set;}
    public int WorkCenterCellX {get;set;}
    public int WorkCenterLogicalLevel {get;set;}
    public int WorkRadiusCells {get;set;}
    public bool ProtectHiveFlowers {get;set;}
    public Dictionary<string,int> Cargo {get;set;} = new();
    public Dictionary<string,int> Priorities {get;set;} = new();
    public string Phase {get;set;} = WorkerPhase.Idle.ToString();
    public string TargetJob {get;set;} = WorkerJob.HarvestHoney.ToString();
    public string TargetKind {get;set;} = WorkerTargetKind.None.ToString();
    public int TargetId {get;set;}
    public int TargetCellX {get;set;}
    public int TargetLogicalLevel {get;set;}
    public int TargetStandCellX {get;set;}
    public int TargetStandLogicalLevel {get;set;}
    public int CurrentCellX {get;set;}
    public int CurrentLogicalLevel {get;set;}
    public int TravelFromCellX {get;set;}
    public int TravelFromLogicalLevel {get;set;}
    public int TravelToCellX {get;set;}
    public int TravelToLogicalLevel {get;set;}
    public double TravelTotalSeconds {get;set;}
    public double RemainingSeconds {get;set;}
    public string BlockedReason {get;set;} = string.Empty;
    public long CompletedJobs {get;set;}

    internal static WorkerSaveData From(WorkerState w) => new() {
        HomeChestPlacementId=w.HomeChestPlacementId,WorkCenterCellX=w.WorkCenterCellX,WorkCenterLogicalLevel=w.WorkCenterLogicalLevel,
        WorkRadiusCells=w.WorkRadiusCells,ProtectHiveFlowers=w.ProtectHiveFlowers,Cargo=GameSaveData.WriteInventory(w.Cargo),
        Priorities=w.Priorities.ToDictionary(p=>p.Key.ToString(),p=>p.Value),Phase=w.Phase.ToString(),TargetJob=w.TargetJob.ToString(),
        TargetKind=w.TargetKind.ToString(),TargetId=w.TargetId,TargetCellX=w.TargetCellX,TargetLogicalLevel=w.TargetLogicalLevel,
        TargetStandCellX=w.TargetStandCellX,TargetStandLogicalLevel=w.TargetStandLogicalLevel,CurrentCellX=w.CurrentCellX,
        CurrentLogicalLevel=w.CurrentLogicalLevel,TravelFromCellX=w.TravelFromCellX,TravelFromLogicalLevel=w.TravelFromLogicalLevel,
        TravelToCellX=w.TravelToCellX,TravelToLogicalLevel=w.TravelToLogicalLevel,TravelTotalSeconds=w.TravelTotalSeconds,
        RemainingSeconds=w.RemainingSeconds,BlockedReason=w.BlockedReason,CompletedJobs=w.CompletedJobs };

    internal WorkerState ToState()
    {
        GameSaveData.Require(HomeChestPlacementId>=0 && WorkCenterCellX>=0 && WorkCenterCellX<4096 &&
            WorkCenterLogicalLevel>=-100 && WorkCenterLogicalLevel<=100 && WorkRadiusCells is 12 or 24 or 48 or 96,
            "Invalid worker assignment.");
        GameSaveData.Require(Cargo is not null && Priorities is not null && BlockedReason is not null && BlockedReason.Length<=512,
            "Invalid worker collections/message.");
        var worker=new WorkerState {
            HomeChestPlacementId=HomeChestPlacementId,WorkCenterCellX=WorkCenterCellX,WorkCenterLogicalLevel=WorkCenterLogicalLevel,
            WorkRadiusCells=WorkRadiusCells,ProtectHiveFlowers=ProtectHiveFlowers,Phase=GameSaveData.Parse<WorkerPhase>(Phase),
            TargetJob=GameSaveData.Parse<WorkerJob>(TargetJob),TargetKind=GameSaveData.Parse<WorkerTargetKind>(TargetKind),
            TargetId=TargetId,TargetCellX=TargetCellX,TargetLogicalLevel=TargetLogicalLevel,TargetStandCellX=TargetStandCellX,
            TargetStandLogicalLevel=TargetStandLogicalLevel,CurrentCellX=CurrentCellX,CurrentLogicalLevel=CurrentLogicalLevel,
            TravelFromCellX=TravelFromCellX,TravelFromLogicalLevel=TravelFromLogicalLevel,TravelToCellX=TravelToCellX,
            TravelToLogicalLevel=TravelToLogicalLevel,TravelTotalSeconds=TravelTotalSeconds,RemainingSeconds=RemainingSeconds,
            BlockedReason=BlockedReason,CompletedJobs=CompletedJobs };
        worker.Cargo.ReplaceWith(GameSaveData.ReadInventory(Cargo).Counts);
        var priorities=new Dictionary<WorkerJob,int>();
        foreach(var (name,value) in Priorities) { var job=GameSaveData.Parse<WorkerJob>(name); GameSaveData.Require(value is >=0 and <=3,"Invalid worker priority."); priorities[job]=value; }
        worker.ReplacePriorities(priorities);
        GameSaveData.Require(TargetCellX>=-1 && TargetCellX<4096 && TargetStandCellX>=-1 && TargetStandCellX<4096 &&
            TargetLogicalLevel>=-100 && TargetLogicalLevel<=100 && TargetStandLogicalLevel>=-100 && TargetStandLogicalLevel<=100 &&
            CurrentCellX>=0 && CurrentCellX<4096 && CurrentLogicalLevel>=-100 && CurrentLogicalLevel<=100 &&
            TravelFromCellX>=0 && TravelFromCellX<4096 && TravelToCellX>=0 && TravelToCellX<4096 &&
            TravelFromLogicalLevel>=-100 && TravelFromLogicalLevel<=100 && TravelToLogicalLevel>=-100 && TravelToLogicalLevel<=100,
            "Invalid worker location.");
        GameSaveData.Require(double.IsFinite(TravelTotalSeconds) && TravelTotalSeconds>=0 && TravelTotalSeconds<100000 &&
            double.IsFinite(RemainingSeconds) && RemainingSeconds>=0 && RemainingSeconds<100000 && CompletedJobs>=0,
            "Invalid worker timing/counter.");
        if(worker.Phase==WorkerPhase.Idle || worker.Phase==WorkerPhase.Blocked) worker.TargetKind=WorkerTargetKind.None;
        return worker;
    }
}

public sealed class IslandSaveData
{
    public List<int> HarvestedNaturalFeatureIds { get; set; } = new();
    public Dictionary<int,double> NaturalRegrowthSeconds { get; set; } = new();
    public List<PlacedObjectSaveData> PlacedObjects { get; set; } = new();
    public List<UndergroundCellSaveData> MinedUndergroundCells { get; set; } = new();
    public List<int> LootedGeneratedChestIds { get; set; } = new();
    public Dictionary<int,Dictionary<string,int>> GeneratedChestContents { get; set; } = new();
    public bool LeftBoatBuilt { get; set; }
    public bool RightBoatBuilt { get; set; }
    public int NextPlacementId { get; set; }

    internal static IslandSaveData From(IslandDeltaState state) => new() {
        HarvestedNaturalFeatureIds=state.HarvestedNaturalFeatureIds.OrderBy(i=>i).ToList(),
        NaturalRegrowthSeconds=state.NaturalRegrowthSeconds.OrderBy(p=>p.Key).ToDictionary(p=>p.Key,p=>p.Value),
        PlacedObjects=state.PlacedObjects.OrderBy(p=>p.PlacementId).Select(PlacedObjectSaveData.From).ToList(),
        MinedUndergroundCells=state.MinedUndergroundCells.OrderBy(c=>c.CellX).ThenBy(c=>c.LogicalLevel)
            .Select(c=>new UndergroundCellSaveData{CellX=c.CellX,LogicalLevel=c.LogicalLevel}).ToList(),
        LootedGeneratedChestIds=state.LootedGeneratedChestIds.OrderBy(i=>i).ToList(),
        GeneratedChestContents=state.GeneratedChestContents.OrderBy(p=>p.Key).ToDictionary(p=>p.Key,p=>GameSaveData.WriteInventory(p.Value)),
        LeftBoatBuilt=state.LeftBoatBuilt,RightBoatBuilt=state.RightBoatBuilt,NextPlacementId=state.NextPlacementId };

    internal IslandDeltaState ToState(IslandDefinition d,int format)
    {
        GameSaveData.Require(HarvestedNaturalFeatureIds is not null && NaturalRegrowthSeconds is not null &&
            PlacedObjects is not null && MinedUndergroundCells is not null && LootedGeneratedChestIds is not null && GeneratedChestContents is not null,
            "Null island collections.");
        var ids=d.NaturalFeatures.Select(f=>f.FeatureId).ToHashSet();
        GameSaveData.Require(HarvestedNaturalFeatureIds!.Distinct().Count()==HarvestedNaturalFeatureIds.Count && HarvestedNaturalFeatureIds.All(ids.Contains),"Invalid harvested resource identity.");
        foreach(var (id,time) in NaturalRegrowthSeconds!)
            GameSaveData.Require(HarvestedNaturalFeatureIds.Contains(id) && double.IsFinite(time) && time>=0,"Invalid regrowth timer.");
        var result=new IslandDeltaState();
        result.ReplaceHarvestedNaturalFeatures(HarvestedNaturalFeatureIds);
        result.ReplaceNaturalRegrowth(NaturalRegrowthSeconds);
        var placed=new List<PlacedObjectState>(); var pids=new HashSet<int>(); var occupied=new HashSet<(int,int,BuildLayer)>();
        foreach(var p in PlacedObjects!)
        {
            GameSaveData.Require(p is not null,"Null placed object.");
            var item=GameSaveData.Parse<ItemType>(p!.Item); var layer=GameSaveData.Parse<BuildLayer>(p.Layer);
            var cell=new UndergroundCell(p.CellX,p.LogicalLevel);
            GameSaveData.Require(p.PlacementId>0 && p.PlacementId<int.MaxValue-1 && pids.Add(p.PlacementId) && WorldGrid.IsInBounds(d,cell) &&
                PlacementRules.IsPlaceable(item) && occupied.Add((cell.CellX,cell.LogicalLevel,layer)),"Invalid placement identity, location or duplicate occupied cell.");
            GameSaveData.Require(PlacementRules.IsBlock(item) || layer==BuildLayer.Solid,"Object on an invalid layer.");
            GameSaveData.Require(double.IsFinite(p.GrowthSeconds) && p.GrowthSeconds>=0 && p.GrowthSeconds<=60 &&
                double.IsFinite(p.ProductionSeconds) && p.ProductionSeconds>=0 && (format==1 || p.ProductionSeconds<45) && p.StoredOutput>=0 && p.StoredOutput<=3,"Invalid production state.");
            var state=new PlacedObjectState(p.PlacementId,item,p.CellX,p.LogicalLevel,layer) {
                GrowthSeconds=p.GrowthSeconds,ProductionSeconds=p.StoredOutput==3?0:Math.Min(p.ProductionSeconds,44.999999),StoredOutput=p.StoredOutput,
                PlantKind=p.PlantKind is null?WorldQueries.PlantedKind(d.Biome):GameSaveData.Parse<NaturalFeatureKind>(p.PlantKind) };
            GameSaveData.Require(state.PlantKind is NaturalFeatureKind.Tree or NaturalFeatureKind.Pine or NaturalFeatureKind.Palm or NaturalFeatureKind.Cactus,"Invalid planted tree species.");
            state.Contents.ReplaceWith(GameSaveData.ReadInventory(p.Contents).Counts);
            GameSaveData.Require(item==ItemType.Chest || state.Contents.IsEmpty,"Only chests may contain storage.");
            GameSaveData.Require(item==ItemType.Beehive || p.StoredOutput==0,"Only hives may contain honey output.");
            placed.Add(state);
        }
        result.ReplacePlacedObjects(placed);
        if(format>=2) result.SetNextPlacementId(NextPlacementId);
        var mined=MinedUndergroundCells!.Select(c=> {GameSaveData.Require(c is not null,"Null terrain edit."); return new UndergroundCell(c!.CellX,c.LogicalLevel);}).ToArray();
        GameSaveData.Require(mined.Distinct().Count()==mined.Length && mined.All(c=>WorldGrid.IsInBounds(d,c) && c.LogicalLevel<=d.SurfaceLevels[c.CellX]),"Invalid mined cells.");
        result.ReplaceMinedUndergroundCells(mined);
        var chestIds=d.GeneratedStructures.Select(s=>s.Chest.ChestId).ToHashSet();
        GameSaveData.Require(LootedGeneratedChestIds!.Distinct().Count()==LootedGeneratedChestIds.Count && LootedGeneratedChestIds.All(chestIds.Contains),"Unknown chest identity.");
        result.ReplaceLootedGeneratedChestIds(LootedGeneratedChestIds);
        foreach(var (id,contents) in GeneratedChestContents!)
        {
            GameSaveData.Require(chestIds.Contains(id),"Unknown stored loot chest.");
            var inventory=GameSaveData.ReadInventory(contents);
            GameSaveData.Require(!result.IsGeneratedChestLooted(id) || inventory.IsEmpty,"Looted chest has contradictory contents.");
            result.RestoreGeneratedChest(id,inventory);
        }
        result.SetBoatBuilt(BoatSide.Left,LeftBoatBuilt); result.SetBoatBuilt(BoatSide.Right,RightBoatBuilt);
        // Legacy depleted vegetation without timers remains depleted; do not guess its prior elapsed time.
        return result;
    }
}
public sealed class UndergroundCellSaveData { public int CellX {get;set;} public int LogicalLevel {get;set;} }
public sealed class PlacedObjectSaveData
{
    public int PlacementId {get;set;}
    public string Item {get;set;} = string.Empty;
    public int CellX {get;set;}
    public int LogicalLevel {get;set;}
    public string Layer {get;set;} = BuildLayer.Solid.ToString();
    public double GrowthSeconds {get;set;}
    public double ProductionSeconds {get;set;}
    public int StoredOutput {get;set;}
    public string? PlantKind {get;set;}
    public Dictionary<string,int> Contents {get;set;} = new();
    internal static PlacedObjectSaveData From(PlacedObjectState p) => new() {PlacementId=p.PlacementId,Item=p.Item.ToString(),
        CellX=p.CellX,LogicalLevel=p.LogicalLevel,Layer=p.Layer.ToString(),GrowthSeconds=p.GrowthSeconds,
        ProductionSeconds=p.ProductionSeconds,StoredOutput=p.StoredOutput,PlantKind=p.PlantKind.ToString(),Contents=GameSaveData.WriteInventory(p.Contents)};
}
