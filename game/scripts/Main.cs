using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.Simulation;
using BearAdventure.Domain.World;
using BearAdventure.Interaction;
using BearAdventure.Persistence;
using BearAdventure.Player;
using BearAdventure.Rendering;
using BearAdventure.UI;
using BearAdventure.World;
using Godot;

namespace BearAdventure;

public partial class Main : Node2D
{
    private const string DevelopmentWorldSeed =
        "bear-adventure-development-001";
    private const string SaveFileName =
        "bear-adventure-save.json";

    private readonly IslandGenerator _islandGenerator =
        new();
    private readonly WorldSeed _worldSeed =
        new(DevelopmentWorldSeed);
    private readonly SaveGameStore _saveStore =
        new();
    private readonly WorldSimulationService _worldSimulation =
        new();

    private GameSessionState _session =
        GameSessionState.CreateDevelopmentStarter();

    private IslandView? _islandView;
    private BearController? _player;
    private HarvestController? _harvestController;
    private BuildingController? _buildingController;
    private GameHud? _hud;
    private CanvasLayer? _parallaxLayer;
    private ParallaxBackdrop? _parallaxBackdrop;

    private double _simulationAccumulator;
    private double _autosaveAccumulator;

    public override void _Ready()
    {
        LoadSession();

        _parallaxLayer =
            new CanvasLayer
            {
                Name = "ParallaxLayer",
                Layer = -100,
            };
        AddChild(_parallaxLayer);

        _parallaxBackdrop =
            new ParallaxBackdrop
            {
                Name = "ParallaxBackdrop",
            };
        _parallaxLayer.AddChild(
            _parallaxBackdrop);

        _hud = new GameHud
        {
            Name = "Hud",
        };
        AddChild(_hud);
        _hud.ConfigureInventory(
            _session.Inventory);

        _player = new BearController
        {
            Name = "Bear",
        };
        AddChild(_player);

        _harvestController =
            new HarvestController
            {
                Name = "HarvestController",
            };
        AddChild(_harvestController);

        _buildingController =
            new BuildingController
            {
                Name = "BuildingController",
            };
        AddChild(_buildingController);

        _harvestController.StatusChanged +=
            OnHarvestStatusChanged;
        _harvestController.HarvestCompleted +=
            OnHarvestCompleted;
        _harvestController.StationRequested +=
            OnStationRequested;
        _harvestController.TravelRequested +=
            OnTravelRequested;

        _buildingController.StatusMessage +=
            OnBuildingNotification;
        _buildingController.InventoryChanged +=
            OnInventoryChanged;
        _buildingController.IslandChanged +=
            OnIslandChanged;
        _buildingController.SelectionChanged +=
            OnBuildSelectionChanged;

        _hud.PlaceableItemSelected +=
            OnPlaceableItemSelected;
        _hud.CraftRequested +=
            OnCraftRequested;
        _hud.StationCraftRequested +=
            OnStationCraftRequested;

        LoadIsland(
            _session.CurrentIslandId);

        _hud.ShowNotification(
            "Underground mining and generated structures are active.");
    }

    public override void _PhysicsProcess(double delta)
    {
        _ = delta;

        if (_player is null
            || _islandView is null)
        {
            return;
        }

        _player.ClimbEnabled =
            _islandView.IsInMineShaft(
                _player.Position);

        if (_harvestController is not null)
        {
            _harvestController.MiningBlocked =
                _buildingController?.SelectedItem is not null
                || (_hud?.AnyPanelOpen ?? false);
        }
    }

    public override void _Process(double delta)
    {
        UpdateParallaxCamera();

        _simulationAccumulator += delta;
        _autosaveAccumulator += delta;

        if (_simulationAccumulator >= 0.5)
        {
            bool changed =
                _worldSimulation.Advance(
                    _session,
                    _worldSeed,
                    _islandGenerator,
                    _simulationAccumulator);

            _simulationAccumulator = 0.0;

            if (changed)
            {
                _islandView?.QueueRedraw();
                UpdateIslandHud();
            }
        }

        if (_autosaveAccumulator >= 10.0)
        {
            _autosaveAccumulator = 0.0;
            SaveSession();
        }

    }

    public override void _ExitTree()
    {
        SaveSession();
    }

    private void LoadIsland(
        int islandId,
        BoatSide? arrivalSide = null)
    {
        _harvestController?.DetachIsland();
        _buildingController?.DetachIsland();

        _session.CurrentIslandId =
            islandId;

        if (_islandView is not null)
        {
            RemoveChild(_islandView);
            _islandView.QueueFree();
            _islandView = null;
        }

        IslandDefinition definition =
            _islandGenerator.Generate(
                _worldSeed,
                islandId);

        IslandDeltaState islandState =
            _session.GetIslandState(
                islandId);

        _islandView =
            new IslandView(
                definition,
                islandState)
            {
                Name = $"Island_{islandId}",
            };

        AddChild(_islandView);
        MoveChild(_islandView, 0);

        RenderingServer.SetDefaultClearColor(
            _islandView.SkyColor);

        _parallaxBackdrop?.Configure(
            definition.Biome,
            islandId);

        Vector2 spawn =
            arrivalSide is null
                ? _islandView
                    .GetSuggestedSpawnPosition()
                : _islandView
                    .GetBoatArrivalSpawn(
                        arrivalSide.Value);

        _player!.ConfigureIsland(
            _islandView.LandLeftX,
            _islandView.LandRightX,
            spawn);

        _player.ConfigureCamera(
            0.0f,
            _islandView.WorldWidth,
            _islandView.WorldTop,
            _islandView.WorldBottom);

        _harvestController!.Configure(
            _player,
            _islandView,
            _session.Inventory);

        _buildingController!.Configure(
            _player,
            _islandView,
            _session.Inventory);

        _hud!.SetBuildSelection(
            _buildingController.SelectedItem,
            _buildingController.Layer);

        UpdateIslandHud();
        UpdateMapHud();
        _hud.RefreshInventory();
        SaveSession();
    }

    private void OnHarvestStatusChanged(
        string text)
    {
        _hud?.SetInteractionText(text);
    }

    private void OnHarvestCompleted(
        string message)
    {
        _hud?.RefreshInventory();
        _hud?.ShowNotification(message);
        UpdateIslandHud();
        SaveSession();
    }

    private void OnPlaceableItemSelected(
        ItemType item)
    {
        _buildingController?.SelectItem(item);
    }

    private void OnCraftRequested(
        string recipeId)
    {
        CraftingRecipe? recipe =
            CraftingCatalog.Find(recipeId);

        if (recipe is null)
        {
            _hud?.ShowNotification(
                "Unknown crafting recipe.");
            return;
        }

        if (!CraftingService.TryCraft(
            _session.Inventory,
            recipe))
        {
            _hud?.ShowNotification(
                "Not enough materials.");
            return;
        }

        _hud?.RefreshInventory();
        _hud?.ShowNotification(
            $"Crafted {recipe.DisplayName}.");
        SaveSession();
    }

    private void OnTravelRequested(
        BoatSide departureSide)
    {
        int direction =
            (int)departureSide;

        int destinationId =
            _session.CurrentIslandId
            + direction;

        BoatSide arrivalSide =
            departureSide == BoatSide.Left
                ? BoatSide.Right
                : BoatSide.Left;

        IslandDeltaState destinationState =
            _session.GetIslandState(
                destinationId);

        // The boat used for the trip is also the arrival vessel, so the
        // opposite shore becomes a usable return point immediately.
        destinationState.SetBoatBuilt(
            arrivalSide,
            true);

        _session.DiscoverIsland(
            destinationId);

        LoadIsland(
            destinationId,
            arrivalSide);

        _hud?.ShowNotification(
            $"Discovered island {destinationId}.");
    }

    private void OnStationRequested(
        ItemType station)
    {
        _hud?.OpenStation(station);
    }

    private void OnStationCraftRequested(
        ItemType station,
        string recipeId)
    {
        if (!StationCraftingService.TryCraft(
            _session.Inventory,
            station,
            recipeId))
        {
            _hud?.ShowNotification(
                "Not enough materials.");
            return;
        }

        StationRecipeDefinition? recipe =
            StationCraftingService
                .GetRecipes(station)
                .FirstOrDefault(
                    item =>
                        item.Id == recipeId);

        _hud?.RefreshInventory();
        _hud?.ShowNotification(
            $"Made {recipe?.DisplayName ?? "item"}.");
        SaveSession();
    }

    private void OnBuildingNotification(
        string message)
    {
        _hud?.ShowNotification(message);
    }

    private void OnInventoryChanged()
    {
        _hud?.RefreshInventory();
        SaveSession();
    }

    private void OnIslandChanged()
    {
        UpdateIslandHud();
        SaveSession();
    }

    private void OnBuildSelectionChanged(
        ItemType? item,
        BuildLayer layer)
    {
        _hud?.SetBuildSelection(
            item,
            layer);
    }

    private void UpdateParallaxCamera()
    {
        if (_parallaxBackdrop is null
            || _player is null
            || _islandView is null)
        {
            return;
        }

        float viewportWidth =
            GetViewportRect().Size.X;

        float halfViewport =
            viewportWidth * 0.5f;

        float minimumCenter =
            halfViewport;

        float maximumCenter =
            Math.Max(
                minimumCenter,
                _islandView.WorldWidth
                - halfViewport);

        float cameraCenterX =
            Mathf.Clamp(
                _player.Position.X,
                minimumCenter,
                maximumCenter);

        _parallaxBackdrop.SetCameraX(
            cameraCenterX);
    }

    private void UpdateIslandHud()
    {
        if (_hud is null
            || _islandView is null)
        {
            return;
        }

        _hud.SetIslandInfo(
            DevelopmentWorldSeed,
            _islandView.Definition,
            _islandView
                .RemainingNaturalFeatureCount);
    }

    private void UpdateMapHud()
    {
        if (_hud is null)
        {
            return;
        }

        var lines =
            new List<string>
            {
                "Discovered islands",
                string.Empty,
            };

        foreach (int islandId
                 in _session.DiscoveredIslandIds
                     .OrderBy(id => id))
        {
            IslandDefinition definition =
                _islandGenerator.Generate(
                    _worldSeed,
                    islandId);

            string marker =
                islandId == _session.CurrentIslandId
                    ? "  < HERE"
                    : string.Empty;

            lines.Add(
                $"{islandId,5}   " +
                $"{definition.Biome.ToString().ToUpperInvariant(),-10}" +
                marker);
        }

        lines.Add(string.Empty);
        lines.Add(
            "Build boats at island edges to discover adjacent islands.");

        _hud.SetMapText(
            string.Join(
                System.Environment.NewLine,
                lines));
    }

    private void LoadSession()
    {
        string savePath =
            GetSavePath();

        GameSaveData? save =
            _saveStore.Load(
                savePath,
                out string? warning);

        if (save is null)
        {
            _session =
                GameSessionState
                    .CreateDevelopmentStarter();

            if (!string.IsNullOrWhiteSpace(warning))
            {
                GD.PushWarning(warning);
            }

            return;
        }

        if (!string.Equals(
            save.WorldSeed,
            DevelopmentWorldSeed,
            StringComparison.Ordinal))
        {
            GD.PushWarning(
                "Save world seed does not match the current development seed. " +
                "Starting a new session instead.");

            _session =
                GameSessionState
                    .CreateDevelopmentStarter();
            return;
        }

        _session =
            save.ToSession();

        if (!string.IsNullOrWhiteSpace(warning))
        {
            GD.PushWarning(warning);
        }
    }

    private void SaveSession()
    {
        try
        {
            GameSaveData save =
                GameSaveData.FromSession(
                    DevelopmentWorldSeed,
                    _session);

            _saveStore.Save(
                GetSavePath(),
                save);
        }
        catch (Exception exception)
        {
            GD.PushError(
                $"Could not save Bear Adventure: {exception}");

            _hud?.ShowNotification(
                "Save failed. See Godot output.");
        }
    }

    private static string GetSavePath()
    {
        return ProjectSettings.GlobalizePath(
            $"user://{SaveFileName}");
    }
}
