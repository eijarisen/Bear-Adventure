using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;
using BearAdventure.Player;
using BearAdventure.World;
using Godot;

namespace BearAdventure.Interaction;

public partial class BuildingController : Node2D
{
    private const float MaximumBuildDistance = 320.0f;

    private BearController? _player;
    private IslandView? _island;
    private InventoryState? _inventory;

    private ItemType? _selectedItem;
    private BuildLayer _buildLayer = BuildLayer.Solid;
    private bool _layerKeyWasDown;

    private bool _leftMouseHeld;
    private bool _rightMouseHeld;
    private BuildPointerCell? _lastPlaceAttempt;
    private CellPointer? _lastRemoveAttempt;

    public event Action<string>? StatusMessage;

    public event Action? InventoryChanged;

    public event Action? IslandChanged;

    public event Action<ItemType?, BuildLayer>? SelectionChanged;

    public ItemType? SelectedItem => _selectedItem;

    public BuildLayer Layer => _buildLayer;

    public void Configure(
        BearController player,
        IslandView island,
        InventoryState inventory)
    {
        _player =
            player
            ?? throw new ArgumentNullException(nameof(player));
        _island =
            island
            ?? throw new ArgumentNullException(nameof(island));
        _inventory =
            inventory
            ?? throw new ArgumentNullException(nameof(inventory));

        _leftMouseHeld = false;
        _rightMouseHeld = false;
        _lastPlaceAttempt = null;
        _lastRemoveAttempt = null;

        if (_selectedItem is not null
            && _inventory.Get(_selectedItem.Value) <= 0)
        {
            ClearSelection();
        }

        UpdatePreview();
    }

    public void DetachIsland()
    {
        _island?.ClearBuildPreview();

        _leftMouseHeld = false;
        _rightMouseHeld = false;
        _lastPlaceAttempt = null;
        _lastRemoveAttempt = null;
        _island = null;
    }

    public void SelectItem(ItemType item)
    {
        if (_inventory is null)
        {
            return;
        }

        if (!PlacementRules.IsPlaceable(item))
        {
            StatusMessage?.Invoke(
                $"{PlacementRules.GetDisplayName(item)} cannot be placed.");
            return;
        }

        if (_inventory.Get(item) <= 0)
        {
            StatusMessage?.Invoke(
                $"No {PlacementRules.GetDisplayName(item)} available.");
            return;
        }

        _selectedItem = item;

        if (!PlacementRules.UsesBuildLayer(item))
        {
            _buildLayer = BuildLayer.Solid;
        }

        _lastPlaceAttempt = null;

        SelectionChanged?.Invoke(
            _selectedItem,
            _buildLayer);

        UpdatePreview();
    }

    public void ClearSelection()
    {
        _selectedItem = null;
        _lastPlaceAttempt = null;
        _island?.ClearBuildPreview();

        SelectionChanged?.Invoke(
            null,
            _buildLayer);
    }

    public override void _Process(double delta)
    {
        _ = delta;

        bool layerDown =
            Input.IsPhysicalKeyPressed(Key.Q);

        if (layerDown
            && !_layerKeyWasDown
            && _selectedItem is not null
            && PlacementRules.UsesBuildLayer(
                _selectedItem.Value))
        {
            _buildLayer =
                _buildLayer == BuildLayer.Solid
                    ? BuildLayer.Background
                    : BuildLayer.Solid;

            _lastPlaceAttempt = null;

            SelectionChanged?.Invoke(
                _selectedItem,
                _buildLayer);

            StatusMessage?.Invoke(
                $"Build layer: {_buildLayer}.");
        }

        _layerKeyWasDown = layerDown;

        if (_leftMouseHeld)
        {
            TryPlaceSelected();
        }

        if (_rightMouseHeld)
        {
            TryDeconstructAtMouse();
        }

        UpdatePreview();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouse)
        {
            return;
        }

        if (mouse.ButtonIndex == MouseButton.Left)
        {
            _leftMouseHeld = mouse.Pressed;

            if (mouse.Pressed)
            {
                _lastPlaceAttempt = null;
                TryPlaceSelected();
            }
            else
            {
                _lastPlaceAttempt = null;
            }

            GetViewport().SetInputAsHandled();
            return;
        }

        if (mouse.ButtonIndex == MouseButton.Right)
        {
            _rightMouseHeld = mouse.Pressed;

            if (mouse.Pressed)
            {
                _lastRemoveAttempt = null;

                if (!TryDeconstructAtMouse())
                {
                    ClearSelection();
                }
            }
            else
            {
                _lastRemoveAttempt = null;
            }

            GetViewport().SetInputAsHandled();
        }
    }

    private void TryPlaceSelected()
    {
        if (_selectedItem is null
            || _player is null
            || _island is null
            || _inventory is null)
        {
            return;
        }

        ItemType item =
            _selectedItem.Value;

        if (_inventory.Get(item) <= 0)
        {
            ClearSelection();
            return;
        }

        Vector2 worldMouse =
            GetGlobalMousePosition();

        if (!_island.TryWorldToBuildCell(
            worldMouse,
            out int cellX,
            out int logicalLevel))
        {
            return;
        }

        BuildLayer effectiveLayer =
            PlacementRules.UsesBuildLayer(item)
                ? _buildLayer
                : BuildLayer.Solid;

        var pointerCell =
            new BuildPointerCell(
                cellX,
                logicalLevel,
                item,
                effectiveLayer);

        if (_lastPlaceAttempt == pointerCell)
        {
            return;
        }

        _lastPlaceAttempt = pointerCell;

        Vector2 center =
            _island.GetBuildCellCenter(
                cellX,
                logicalLevel);

        if (_player.Position.DistanceTo(center)
            > MaximumBuildDistance)
        {
            return;
        }

        if (!_island.CanPlaceObject(
            item,
            cellX,
            logicalLevel,
            effectiveLayer,
            GetPlayerRect(),
            out _))
        {
            return;
        }

        _island.AddPlacedObject(
            item,
            cellX,
            logicalLevel,
            effectiveLayer);

        _inventory.Add(item, -1);

        InventoryChanged?.Invoke();
        IslandChanged?.Invoke();

        if (_inventory.Get(item) <= 0)
        {
            ClearSelection();
        }
    }

    private bool TryDeconstructAtMouse()
    {
        if (_player is null
            || _island is null
            || _inventory is null)
        {
            return false;
        }

        Vector2 worldMouse =
            GetGlobalMousePosition();

        if (!_island.TryWorldToBuildCell(
            worldMouse,
            out int cellX,
            out int logicalLevel))
        {
            return false;
        }

        var pointerCell =
            new CellPointer(
                cellX,
                logicalLevel);

        if (_lastRemoveAttempt == pointerCell)
        {
            return true;
        }

        _lastRemoveAttempt = pointerCell;

        Vector2 center =
            _island.GetBuildCellCenter(
                cellX,
                logicalLevel);

        if (_player.Position.DistanceTo(center)
            > MaximumBuildDistance)
        {
            return true;
        }

        PlacedObjectState? placed =
            _island.FindPlacedObjectAt(
                cellX,
                logicalLevel);

        if (placed is null)
        {
            return false;
        }

        if (placed.Item == ItemType.Beehive
            && placed.StoredOutput > 0)
        {
            _inventory.Add(
                ItemType.Honey,
                placed.StoredOutput);
        }

        if (!_island.RemovePlacedObject(
            placed.PlacementId,
            out ItemType returnedItem))
        {
            return false;
        }

        _inventory.Add(
            returnedItem,
            1);

        InventoryChanged?.Invoke();
        IslandChanged?.Invoke();
        return true;
    }

    private void UpdatePreview()
    {
        if (_selectedItem is null
            || _player is null
            || _island is null)
        {
            _island?.ClearBuildPreview();
            return;
        }

        Vector2 worldMouse =
            GetGlobalMousePosition();

        if (!_island.TryWorldToBuildCell(
            worldMouse,
            out int cellX,
            out int logicalLevel))
        {
            _island.ClearBuildPreview();
            return;
        }

        ItemType item =
            _selectedItem.Value;

        BuildLayer effectiveLayer =
            PlacementRules.UsesBuildLayer(item)
                ? _buildLayer
                : BuildLayer.Solid;

        bool inRange =
            _player.Position.DistanceTo(
                _island.GetBuildCellCenter(
                    cellX,
                    logicalLevel))
            <= MaximumBuildDistance;

        bool valid =
            inRange
            && _island.CanPlaceObject(
                item,
                cellX,
                logicalLevel,
                effectiveLayer,
                GetPlayerRect(),
                out _);

        _island.SetBuildPreview(
            item,
            cellX,
            logicalLevel,
            effectiveLayer,
            valid);
    }

    private Rect2 GetPlayerRect()
    {
        if (_player is null)
        {
            return default;
        }

        return new Rect2(
            _player.Position
                - new Vector2(
                    17.0f,
                    29.0f),
            new Vector2(
                34.0f,
                58.0f));
    }

    private readonly record struct CellPointer(
        int CellX,
        int LogicalLevel);

    private readonly record struct BuildPointerCell(
        int CellX,
        int LogicalLevel,
        ItemType Item,
        BuildLayer Layer);
}
