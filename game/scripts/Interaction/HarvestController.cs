using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.Simulation;
using BearAdventure.Domain.World;
using BearAdventure.Player;
using BearAdventure.World;
using Godot;

namespace BearAdventure.Interaction;

public partial class HarvestController : Node
{
    private enum TargetKind { None, Natural, Planted, Mine, Object, Resident, SpecialChest, Boat }
    private readonly record struct Target(TargetKind Kind,int Id=0,int X=0,int Level=0,WorldEntityKind EntityKind=WorldEntityKind.Placed);
    private BearController? _player;
    private IslandView? _island;
    private InventoryState? _inventory;
    private GameSessionState? _session;
    private WorldInputState? _input;
    private Target _active;
    private double _progress;
    private bool _instantUsed;
    private bool _fishing;
    private double _fishingProgress;
    private string _lastStatus="";
    public Func<bool>? BuildingSelected {get;set;}
    public event Action<string>? StatusChanged;
    public event Action<string>? Changed;
    public event Action<WorldEntityRef>? OpenEntityRequested;
    public event Action<int>? OpenResidentRequested;
    public event Action<int>? SpecialChestRequested;
    public event Action<BoatSide>? TravelRequested;

    public void Configure(BearController player,IslandView island,InventoryState inventory,WorldInputState input,GameSessionState session)
    { _player=player; _island=island; _inventory=inventory; _input=input; _session=session; CancelActions(); }

    public void CancelActions()
    {
        _active=default; _progress=0; _instantUsed=false; _fishing=false; _fishingProgress=0;
        if(_player is not null) { _player.MovementLocked=false; _player.FishingActive=false; }
        _island?.ClearIndicators(); Status("");
    }
    public void DetachIsland() { CancelActions(); _island=null; _session=null; }

    public override void _PhysicsProcess(double delta)
    {
        if(_input is null || _island is null || _inventory is null || _player is null || _session is null || _input.Blocked || _input.AwaitingRelease) return;
        var world=_island.Queries; var actor=IslandView.Point(_player.Position);

        if(_fishing)
        {
            bool moving=_input.Held(GameAction.Left)||_input.Held(GameAction.Right)||_input.Held(GameAction.Up)||_input.Held(GameAction.Down)||_input.Held(GameAction.Jump);
            if(moving || !FishingService.IsNearFishableWater(world,actor))
            { CancelFishing("Fishing cancelled."); return; }
            _player.MovementLocked=true; _player.FishingActive=true; _player.Velocity=Vector2.Zero;
            _fishingProgress+=delta;
            Status($"Fishing… {Math.Clamp(_fishingProgress/FishingService.CatchSeconds,0,1):P0}");
            if(_fishingProgress+1e-9<FishingService.CatchSeconds) return;
            _fishing=false; _fishingProgress=0; _player.MovementLocked=false; _player.FishingActive=false;
            if(InventoryTransactions.TryGrant(_inventory,new[]{new HarvestReward(ItemType.Fish,1)},out var fishReason))
                Changed?.Invoke("Caught +1 fish.");
            else Status(fishReason);
            return;
        }

        var pointer=IslandView.Point(_island.GetGlobalMousePosition());
        Target usable=FindUsable(actor,pointer,false);
        Target target=Resolve(actor,pointer);
        bool f=_input.Held(GameAction.Harvest);
        bool mouseMine=_input.Held(GameAction.Build) && !(BuildingSelected?.Invoke()??false) && target.Kind==TargetKind.Mine;
        bool r=_input.ConsumePress(GameAction.Use);
        bool g=_input.ConsumePress(GameAction.Fish);
        if(!f && !mouseMine) _instantUsed=false;

        if(g)
        {
            _active=default; _progress=0; _player.MovementLocked=false;
            if(FishingService.CanStart(world,actor,_inventory,out var fishingReason))
            { _fishing=true; _fishingProgress=0; _player.MovementLocked=true; _player.FishingActive=true; _player.Velocity=Vector2.Zero; Status("Fishing… 0%"); }
            else Status(fishingReason);
            return;
        }

        if(r)
        {
            _active=default; _progress=0; _player.MovementLocked=false;
            if(usable.Kind!=TargetKind.None) { Highlight(usable,0); Activate(usable); }
            else Status("No usable object or resident in reach.");
            return;
        }

        Highlight(target,_active==target?(float)Math.Clamp(_progress/Math.Max(0.001,Duration(target)),0,1):0);
        string prompt=Describe(target);
        if(usable.Kind!=TargetKind.None && usable!=target) prompt+=(prompt.Length>0?"\n":"")+"R: "+DescribeUse(usable);
        if(FishingService.IsNearFishableWater(world,actor))
            prompt+=(prompt.Length>0?"\n":"")+(_inventory.Has(ItemType.FishingRod)?"G: fish":"G: fishing rod required");
        Status(prompt);

        if(!(f||mouseMine)) { _active=default; _progress=0; _player.MovementLocked=false; return; }
        if(target.Kind is TargetKind.Object or TargetKind.Boat)
        {
            _player.MovementLocked=false;
            if(!_instantUsed) { _instantUsed=true; Activate(target); }
            return;
        }
        if(target.Kind is TargetKind.Resident or TargetKind.SpecialChest or TargetKind.None || _instantUsed) { _player.MovementLocked=false; return; }
        if(target.Kind==TargetKind.Mine && !world.CanMine(new(target.X,target.Level),out var blocked))
        { _player.MovementLocked=false; _progress=0; Status(blocked); return; }
        if(_active!=target) { _active=target; _progress=0; }
        _player.MovementLocked=true;
        var v=_player.Velocity; _player.Velocity=new Vector2(0,_player.ClimbEnabled?0:v.Y);
        _progress+=delta;
        double duration=Duration(target);
        Highlight(target,(float)Math.Clamp(_progress/duration,0,1));
        if(_progress+1e-9<duration) return;
        bool success=false; string message="Target changed.";
        switch(target.Kind)
        {
            case TargetKind.Natural: success=WorldActions.TryHarvestNatural(world,_inventory,actor,target.Id,out message); break;
            case TargetKind.Planted: success=WorldActions.TryHarvestPlanted(world,_inventory,actor,target.Id,out message); break;
            case TargetKind.Mine:
                success=WorldActions.TryMine(world,_inventory,actor,new(target.X,target.Level),out message);
                if(success) _island.RefreshTerrainRow(target.Level); break;
        }
        _active=default; _progress=0; _player.MovementLocked=false;
        if(success) { _island.RefreshPlacedCollision(); _island.QueueRedraw(); Changed?.Invoke(message); }
        else Status(message);
    }

    private void CancelFishing(string message)
    { _fishing=false; _fishingProgress=0; if(_player is not null) { _player.MovementLocked=false; _player.FishingActive=false; } Status(message); }

    private Target Resolve(WorldPoint actor,WorldPoint pointer)
    {
        var world=_island!.Queries;
        var hovered=FindUsable(actor,pointer,true);
        if(hovered.Kind!=TargetKind.None) return hovered;
        var cell=WorldGrid.CellAt(pointer);
        if(world.TerrainSolid(cell) && world.Exposed(cell) && world.InReach(actor,WorldGrid.Center(cell.CellX,cell.LogicalLevel),WorldActions.MineRange))
            return new(TargetKind.Mine,0,cell.CellX,cell.LogicalLevel);
        Target best=default; double distance=double.PositiveInfinity;
        foreach(var feature in world.Definition.NaturalFeatures)
        {
            double d=actor.DistanceSquared(WorldGrid.Feet(feature.CellX,feature.SurfaceLevel));
            if(d<distance && d<=WorldActions.HarvestRange*WorldActions.HarvestRange && world.NaturalPresent(feature))
            { distance=d; best=new(TargetKind.Natural,feature.FeatureId); }
        }
        foreach(var p in world.State.PlacedObjects)
        {
            double d=actor.DistanceSquared(WorldGrid.Center(p.CellX,p.LogicalLevel));
            if(d<distance && d<=WorldActions.HarvestRange*WorldActions.HarvestRange && PlacedHarvestRules.IsHarvestable(p) && world.PlantCanGrow(p))
            { distance=d; best=new(TargetKind.Planted,p.PlacementId); }
        }
        return best.Kind!=TargetKind.None?best:FindUsable(actor,pointer,false);
    }

    private Target FindUsable(WorldPoint actor,WorldPoint pointer,bool hoverOnly)
    {
        if(_island is null || _session is null) return default;
        var world=_island.Queries; Target best=default; double distance=double.PositiveInfinity;
        void Offer(Target target,WorldRect rect)
        {
            double d=actor.DistanceSquared(rect.Center);
            if(d>WorldActions.InteractionRange*WorldActions.InteractionRange || (hoverOnly && !rect.Contains(pointer))) return;
            if(d<distance) { distance=d; best=target; }
        }
        foreach(var p in world.State.PlacedObjects)
            if(p.Item is ItemType.Chest or ItemType.Forge or ItemType.Anvil or ItemType.Cauldron or ItemType.Beehive)
                Offer(new(TargetKind.Object,p.PlacementId),WorldGrid.CellRect(new(p.CellX,p.LogicalLevel)));
        foreach(var s in world.Definition.GeneratedStructures)
            Offer(new(TargetKind.Object,s.Chest.ChestId,0,0,WorldEntityKind.GeneratedChest),WorldGrid.CellRect(new(s.Chest.CellX,s.Chest.LogicalLevel)));
        foreach(var resident in _session.Residents.Where(r=>r.Status==ResidentStatus.World && r.IslandId==world.Definition.IslandId))
        {
            Offer(new(TargetKind.Resident,resident.ResidentId),ResidentService.Rect(resident,world.Definition));
            if(resident.Kind is ResidentKind.PolarBear or ResidentKind.Monkey)
                Offer(new(TargetKind.SpecialChest,resident.ResidentId),ResidentService.SpecialChestRect(resident,world.Definition));
        }
        foreach(var side in Enum.GetValues<BoatSide>())
        {
            int x=side==BoatSide.Left?0:world.Definition.WidthCells-1;
            Offer(new(TargetKind.Boat,(int)side),WorldGrid.CellRect(new(x,world.Definition.SurfaceLevels[x]+1)));
            if(world.State.IsBoatBuilt(side)) Offer(new(TargetKind.Boat,(int)side),WorldGrid.BoatHull(world.Definition.WidthCells,side));
        }
        return best;
    }

    private double Duration(Target target)
    {
        if(_island is null || _inventory is null) return 1;
        return target.Kind switch {
            TargetKind.Natural=>HarvestRules.GetDurationSeconds(_island.Definition.NaturalFeatures.First(f=>f.FeatureId==target.Id).Kind,_inventory),
            TargetKind.Planted=>PlacedHarvestRules.GetDurationSeconds(_island.State.PlacedObjects.First(p=>p.PlacementId==target.Id),_inventory),
            TargetKind.Mine=>MiningRules.GetDurationSeconds(_inventory),_=>1 };
    }

    private string Describe(Target t)
    {
        if(_island is null || _session is null) return "";
        switch(t.Kind)
        {
            case TargetKind.Natural: return "Hold F: harvest "+HarvestRules.GetDisplayName(_island.Definition.NaturalFeatures.First(f=>f.FeatureId==t.Id));
            case TargetKind.Planted: return "Hold F: harvest "+PlacedHarvestRules.GetDisplayName(_island.State.PlacedObjects.First(p=>p.PlacementId==t.Id));
            case TargetKind.Mine: return $"Hover + F / left mouse: mine cell ({t.X}, {t.Level})";
            case TargetKind.Boat: return _island.State.IsBoatBuilt((BoatSide)t.Id)?$"Press F: sail {(BoatSide)t.Id}":$"Press F: build boat ({WorldActions.BoatWoodCost} wood)";
            case TargetKind.Resident:
                var resident=_session.FindResident(t.Id); return resident is null?"":"R: talk to "+resident.Name+$" · {resident.Sympathy}/100";
            case TargetKind.SpecialChest:
                var owner=_session.FindResident(t.Id);
                if(owner is null) return "";
                if(owner.SpecialChestClaimed) return "R: special chest is empty";
                return owner.Sympathy>=100 ? "R: open "+(owner.Kind==ResidentKind.PolarBear?"ice chest":"golden chest") : $"R: chest locked · {owner.Sympathy}/100 trust";
            case TargetKind.Object:
                if(t.EntityKind==WorldEntityKind.GeneratedChest) return "Press F: open generated loot chest";
                var p=_island.State.PlacedObjects.First(p=>p.PlacementId==t.Id);
                if(p.Item==ItemType.Beehive) return "F: collect honey · "+WorldSimulationService.Describe(_island.Queries,p);
                return "Press F: open "+PlacementRules.GetDisplayName(p.Item);
            default:return "Hold F near plants to gather. Hover terrain to mine. R talks/uses. G fishes near water.";
        }
    }

    private string DescribeUse(Target t)
    {
        if(t.Kind==TargetKind.Resident && _session is not null)
        { var r=_session.FindResident(t.Id); return r is null?"resident":"talk to "+r.Name; }
        string text=Describe(t); return text.StartsWith("R: ",StringComparison.Ordinal)?text[3..]:text.Replace("Press F: ","");
    }

    private void Highlight(Target t,float progress)
    {
        _island!.ClearIndicators();
        switch(t.Kind)
        {
            case TargetKind.Natural: _island.SetHarvestIndicator(t.Id,progress); break;
            case TargetKind.Planted: _island.SetPlacedHarvestIndicator(t.Id,progress); break;
            case TargetKind.Mine: _island.SetMineIndicator(new(t.X,t.Level),progress); break;
            case TargetKind.Object: _island.SetEntityIndicator(new(_island.Definition.IslandId,t.EntityKind,t.Id)); break;
            case TargetKind.Resident: _island.SetResidentIndicator(t.Id); break;
            case TargetKind.SpecialChest: _island.SetResidentIndicator(t.Id); break;
        }
    }

    private void Activate(Target target)
    {
        if(_island is null || _inventory is null || _player is null || _session is null) return;
        if(target.Kind==TargetKind.Resident) { OpenResidentRequested?.Invoke(target.Id); return; }
        if(target.Kind==TargetKind.SpecialChest) { SpecialChestRequested?.Invoke(target.Id); return; }
        if(target.Kind==TargetKind.Boat)
        {
            var side=(BoatSide)target.Id;
            if(_island.State.IsBoatBuilt(side)) TravelRequested?.Invoke(side);
            else if(WorldActions.TryBoat(_island.Queries,_inventory,IslandView.Point(_player.Position),side,out var message))
            { _island.RefreshBoatCollision(); Changed?.Invoke(message); } else Status(message);
            return;
        }
        if(target.Kind!=TargetKind.Object) return;
        var id=new WorldEntityRef(_island.Definition.IslandId,target.EntityKind,target.Id);
        if(target.EntityKind==WorldEntityKind.Placed && _island.State.PlacedObjects.First(p=>p.PlacementId==target.Id).Item==ItemType.Beehive)
        {
            if(WorldActions.TryHoney(_island.Queries,_inventory,IslandView.Point(_player.Position),id,out var message))
            { _island.QueueRedraw(); Changed?.Invoke(message); } else Status(message);
        }
        else OpenEntityRequested?.Invoke(id);
    }

    private void Status(string text) { if(_lastStatus==text) return; _lastStatus=text; StatusChanged?.Invoke(text); }
}
