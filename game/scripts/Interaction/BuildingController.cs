using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;
using BearAdventure.Player;
using BearAdventure.World;
using Godot;

namespace BearAdventure.Interaction;

public partial class BuildingController : Node2D
{
    private BearController? _player;
    private IslandView? _island;
    private InventoryState? _inventory;
    private WorldInputState? _input;
    private double _repeat;
    private UndergroundCell? _lastCell;
    private string _lastMessage="";
    public ItemType? SelectedItem {get;private set;}
    public BuildLayer Layer {get;private set;} = BuildLayer.Solid;
    public event Action<string>? StatusMessage;
    public event Action? Changed;
    public event Action<ItemType?,BuildLayer>? SelectionChanged;
    public void Configure(BearController player,IslandView island,InventoryState inventory,WorldInputState input)
    {
        _player=player; _island=island; _inventory=inventory; _input=input; CancelActions();
        if(SelectedItem.HasValue && !inventory.Has(SelectedItem.Value)) ClearSelection();
    }
    public void CancelActions() { _repeat=0; _lastCell=null; _lastMessage=""; _island?.ClearBuildPreview(); }
    public void DetachIsland() { CancelActions(); _island=null; }
    public void SelectItem(ItemType item)
    {
        if(_inventory is null || !_inventory.Has(item) || !PlacementRules.IsPlaceable(item)) return;
        SelectedItem=item; if(!PlacementRules.UsesBuildLayer(item)) Layer=BuildLayer.Solid;
        SelectionChanged?.Invoke(SelectedItem,Layer); CancelActions();
    }
    public void ClearSelection() { SelectedItem=null; CancelActions(); SelectionChanged?.Invoke(null,Layer); }
    public override void _PhysicsProcess(double delta)
    {
        if(_input is null || _island is null || _inventory is null || _player is null || _input.Blocked || _input.AwaitingRelease) return;
        if(_input.ConsumePress(GameAction.Layer) && SelectedItem.HasValue && PlacementRules.UsesBuildLayer(SelectedItem.Value))
        { Layer=Layer==BuildLayer.Solid?BuildLayer.Background:BuildLayer.Solid; SelectionChanged?.Invoke(SelectedItem,Layer); }
        var mouse=IslandView.Point(GetGlobalMousePosition()); var cell=WorldGrid.CellAt(mouse);
        var actor=IslandView.Point(_player.Position);
        bool build=_input.Held(GameAction.Build) && SelectedItem.HasValue;
        bool remove=_input.Held(GameAction.Remove);
        bool initialRemove=_input.ConsumePress(GameAction.Remove);
        _repeat-=delta;
        if(!build && !remove) { _lastCell=null; _repeat=0; }
        if((build || remove) && (_repeat<=0 || _lastCell!=cell))
        {
            _lastCell=cell; _repeat=0.12;
            if(remove)
            {
                var placed=_island.FindPlacedObjectAt(cell.CellX,cell.LogicalLevel);
                if(placed is not null)
                {
                    if(WorldActions.TryRemove(_island.Queries,_inventory,actor,placed.PlacementId,out var reason))
                    { _island.RefreshPlacedCollision(); Changed?.Invoke(); _lastMessage=""; }
                    else Message(reason);
                }
                else if(initialRemove) ClearSelection();
            }
            else if(SelectedItem.HasValue)
            {
                if(WorldActions.TryPlace(_island.Queries,_inventory,actor,SelectedItem.Value,cell,Layer,out _,out var reason))
                {
                    _island.RefreshPlacedCollision(); Changed?.Invoke(); _lastMessage="";
                    if(!_inventory.Has(SelectedItem.Value)) ClearSelection();
                }
                else Message(reason);
            }
        }
        if(SelectedItem.HasValue && _island.Queries.InBounds(cell))
        {
            bool valid=_inventory.Has(SelectedItem.Value) && _island.Queries.InReach(actor,WorldGrid.Center(cell.CellX,cell.LogicalLevel),WorldActions.BuildRange)
                && _island.Queries.CanPlace(SelectedItem.Value,cell,Layer,WorldGrid.BearRect(actor),out _);
            _island.SetBuildPreview(SelectedItem,cell.CellX,cell.LogicalLevel,Layer,valid);
        }
        else _island.ClearBuildPreview();
    }
    private void Message(string text) { if(_lastMessage==text) return; _lastMessage=text; StatusMessage?.Invoke(text); }
}
