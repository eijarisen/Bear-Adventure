using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;
using Godot;
using System.Text;

namespace BearAdventure.UI;

public partial class GameHud : CanvasLayer
{
    private InventoryState? _inventory;

    private Label? _islandLabel;
    private Label? _compactInventoryLabel;
    private Label? _buildSelectionLabel;
    private ColorRect? _inventoryPanel;
    private VBoxContainer? _inventoryItems;
    private ColorRect? _craftingPanel;
    private VBoxContainer? _craftingItems;
    private ColorRect? _stationPanel;
    private Label? _stationTitle;
    private VBoxContainer? _stationItems;
    private ItemType? _activeStation;
    private ColorRect? _mapPanel;
    private Label? _mapLabel;
    private Label? _interactionLabel;
    private Label? _notificationLabel;

    private bool _inventoryKeyWasDown;
    private bool _craftingKeyWasDown;
    private bool _mapKeyWasDown;
    private bool _escapeKeyWasDown;
    private double _notificationRemaining;

    public event Action<ItemType>? PlaceableItemSelected;

    public event Action<string>? CraftRequested;

    public event Action<ItemType, string>? StationCraftRequested;

    public bool AnyPanelOpen =>
        (_inventoryPanel?.Visible ?? false)
        || (_craftingPanel?.Visible ?? false)
        || (_stationPanel?.Visible ?? false)
        || (_mapPanel?.Visible ?? false);

    public override void _Ready()
    {
        Layer = 50;

        CreateIslandPanel();
        CreateCompactInventory();
        CreateBuildSelection();
        CreateControls();
        CreateInteractionPrompt();
        CreateInventoryPanel();
        CreateCraftingPanel();
        CreateStationPanel();
        CreateMapPanel();
        CreateNotification();

        RefreshInventory();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey key
            || !key.Pressed
            || key.Echo
            || key.Keycode != Key.Escape)
        {
            return;
        }

        if (AnyPanelOpen)
        {
            HidePanels();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        bool inventoryDown =
            Input.IsPhysicalKeyPressed(Key.E);
        bool craftingDown =
            Input.IsPhysicalKeyPressed(Key.C);
        bool mapDown =
            Input.IsPhysicalKeyPressed(Key.M);
        bool escapeDown =
            Input.IsPhysicalKeyPressed(Key.Escape);

        if (escapeDown
            && !_escapeKeyWasDown)
        {
            HidePanels();
        }

        if (inventoryDown
            && !_inventoryKeyWasDown)
        {
            ToggleInventory();
        }

        if (craftingDown
            && !_craftingKeyWasDown)
        {
            ToggleCrafting();
        }

        if (mapDown
            && !_mapKeyWasDown)
        {
            ToggleMap();
        }

        _inventoryKeyWasDown =
            inventoryDown;
        _craftingKeyWasDown =
            craftingDown;
        _mapKeyWasDown =
            mapDown;
        _escapeKeyWasDown =
            escapeDown;

        if (_notificationLabel is not null
            && _notificationRemaining > 0.0)
        {
            _notificationRemaining -= delta;

            if (_notificationRemaining <= 0.0)
            {
                _notificationLabel.Text =
                    string.Empty;
                _notificationLabel.Visible =
                    false;
            }
        }
    }

    public void ConfigureInventory(
        InventoryState inventory)
    {
        _inventory =
            inventory
            ?? throw new ArgumentNullException(nameof(inventory));

        RefreshInventory();
    }

    public void SetIslandInfo(
        string worldSeed,
        IslandDefinition island,
        int remainingFeatures)
    {
        if (_islandLabel is null)
        {
            return;
        }

        int harvested =
            island.NaturalFeatures.Count
            - remainingFeatures;

        _islandLabel.Text =
            $"WORLD SEED  {worldSeed}\n" +
            $"ISLAND      {island.IslandId}\n" +
            $"BIOME       {island.Biome.ToString().ToUpperInvariant()}\n" +
            $"LENGTH      {island.WidthCells} CELLS\n" +
            $"RESOURCES   {remainingFeatures} ACTIVE / {harvested} HARVESTED";
    }

    public void SetMapText(string text)
    {
        if (_mapLabel is not null)
        {
            _mapLabel.Text = text;
        }
    }

    public void SetInteractionText(
        string text)
    {
        if (_interactionLabel is not null)
        {
            _interactionLabel.Text = text;
            _interactionLabel.Visible =
                !string.IsNullOrWhiteSpace(text);
        }
    }

    public void SetBuildSelection(
        ItemType? item,
        BuildLayer layer)
    {
        if (_buildSelectionLabel is null)
        {
            return;
        }

        if (item is null)
        {
            _buildSelectionLabel.Text =
                "BUILD: none";
            return;
        }

        string layerText =
            PlacementRules.UsesBuildLayer(item.Value)
                ? $"  [{layer}]  Q toggles layer"
                : string.Empty;

        _buildSelectionLabel.Text =
            $"BUILD: {PlacementRules.GetDisplayName(item.Value)}" +
            layerText +
            "  |  Left click place  Right click recover/deselect";
    }

    public void RefreshInventory()
    {
        if (_inventory is null)
        {
            return;
        }

        if (_compactInventoryLabel is not null)
        {
            _compactInventoryLabel.Text =
                BuildCompactInventoryText(
                    _inventory);
        }

        RebuildInventoryButtons();
        RebuildCraftingButtons();
        RebuildStationButtons();
    }

    public void OpenStation(ItemType station)
    {
        if (_stationPanel is null
            || _stationTitle is null)
        {
            return;
        }

        _activeStation = station;
        HidePanels();
        _stationPanel.Visible = true;
        _stationTitle.Text =
            PlacementRules.GetDisplayName(station).ToUpperInvariant();

        RebuildStationButtons();
    }

    public void HidePanels()
    {
        if (_inventoryPanel is not null)
        {
            _inventoryPanel.Visible = false;
        }

        if (_craftingPanel is not null)
        {
            _craftingPanel.Visible = false;
        }

        if (_stationPanel is not null)
        {
            _stationPanel.Visible = false;
        }

        if (_mapPanel is not null)
        {
            _mapPanel.Visible = false;
        }
    }

    public void ShowNotification(
        string message)
    {
        if (_notificationLabel is null)
        {
            return;
        }

        _notificationLabel.Text =
            message;
        _notificationLabel.Visible = true;
        _notificationRemaining = 3.0;
    }

    private void ToggleInventory()
    {
        if (_inventoryPanel is null)
        {
            return;
        }

        bool next =
            !_inventoryPanel.Visible;

        HidePanels();
        _inventoryPanel.Visible = next;
    }

    private void ToggleCrafting()
    {
        if (_craftingPanel is null)
        {
            return;
        }

        bool next =
            !_craftingPanel.Visible;

        HidePanels();
        _craftingPanel.Visible = next;
    }

    private void ToggleMap()
    {
        if (_mapPanel is null)
        {
            return;
        }

        bool next =
            !_mapPanel.Visible;

        HidePanels();
        _mapPanel.Visible = next;
    }

    private void CreateIslandPanel()
    {
        var background = new ColorRect
        {
            Position =
                new Vector2(16.0f, 16.0f),
            Size =
                new Vector2(430.0f, 156.0f),
            Color =
                new Color(
                    0.03f,
                    0.04f,
                    0.055f,
                    0.84f),
            MouseFilter =
                Control.MouseFilterEnum.Ignore,
        };
        AddChild(background);

        _islandLabel = new Label
        {
            Position =
                new Vector2(30.0f, 28.0f),
            Size =
                new Vector2(400.0f, 135.0f),
        };

        _islandLabel.AddThemeFontSizeOverride(
            "font_size",
            17);
        _islandLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                0.90f,
                0.94f,
                0.98f));

        AddChild(_islandLabel);
    }

    private void CreateCompactInventory()
    {
        var background = new ColorRect
        {
            AnchorLeft = 1.0f,
            AnchorRight = 1.0f,
            OffsetLeft = -300.0f,
            OffsetTop = 16.0f,
            OffsetRight = -16.0f,
            OffsetBottom = 172.0f,
            Color =
                new Color(
                    0.03f,
                    0.04f,
                    0.055f,
                    0.84f),
            MouseFilter =
                Control.MouseFilterEnum.Ignore,
        };
        AddChild(background);

        _compactInventoryLabel = new Label
        {
            AnchorLeft = 1.0f,
            AnchorRight = 1.0f,
            OffsetLeft = -284.0f,
            OffsetTop = 28.0f,
            OffsetRight = -28.0f,
            OffsetBottom = 160.0f,
        };

        _compactInventoryLabel.AddThemeFontSizeOverride(
            "font_size",
            16);
        _compactInventoryLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                0.94f,
                0.90f,
                0.76f));

        AddChild(_compactInventoryLabel);
    }

    private void CreateBuildSelection()
    {
        _buildSelectionLabel = new Label
        {
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            OffsetLeft = -520.0f,
            OffsetTop = 18.0f,
            OffsetRight = 520.0f,
            OffsetBottom = 50.0f,
            HorizontalAlignment =
                HorizontalAlignment.Center,
            Text = "BUILD: none",
        };

        _buildSelectionLabel.AddThemeFontSizeOverride(
            "font_size",
            14);
        _buildSelectionLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                1.0f,
                0.84f,
                0.34f));

        AddChild(_buildSelectionLabel);
    }

    private void CreateControls()
    {
        var background = new ColorRect
        {
            AnchorTop = 1.0f,
            AnchorBottom = 1.0f,
            OffsetLeft = 16.0f,
            OffsetTop = -94.0f,
            OffsetRight = 1120.0f,
            OffsetBottom = -16.0f,
            Color =
                new Color(
                    0.03f,
                    0.04f,
                    0.055f,
                    0.80f),
            MouseFilter =
                Control.MouseFilterEnum.Ignore,
        };
        AddChild(background);

        var controls = new Label
        {
            AnchorTop = 1.0f,
            AnchorBottom = 1.0f,
            OffsetLeft = 30.0f,
            OffsetTop = -83.0f,
            OffsetRight = 1100.0f,
            OffsetBottom = -24.0f,
            Text =
                "A/D: walk   W/Space: jump   W/S ladder: climb   Hover terrain + F/left mouse: mine   E: inventory   C: crafting\n" +
                "M: map   Esc: close window   Q: build layer   Left-drag: build when item selected   Right-drag: remove",
        };

        controls.AddThemeFontSizeOverride(
            "font_size",
            14);
        controls.AddThemeColorOverride(
            "font_color",
            new Color(
                0.82f,
                0.87f,
                0.92f));

        AddChild(controls);
    }

    private void CreateInteractionPrompt()
    {
        _interactionLabel = new Label
        {
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            AnchorTop = 1.0f,
            AnchorBottom = 1.0f,
            OffsetLeft = -300.0f,
            OffsetTop = -145.0f,
            OffsetRight = 300.0f,
            OffsetBottom = -110.0f,
            HorizontalAlignment =
                HorizontalAlignment.Center,
            Visible = false,
        };

        _interactionLabel.AddThemeFontSizeOverride(
            "font_size",
            17);
        _interactionLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                1.0f,
                0.86f,
                0.32f));

        AddChild(_interactionLabel);
    }

    private void CreateInventoryPanel()
    {
        _inventoryPanel = CreateModalPanel(
            "INVENTORY / BUILD",
            new Vector2(
                560.0f,
                500.0f));

        var scroll =
            new ScrollContainer
            {
                Position =
                    new Vector2(
                        24.0f,
                        66.0f),
                Size =
                    new Vector2(
                        512.0f,
                        410.0f),
                HorizontalScrollMode =
                    ScrollContainer.ScrollMode.Disabled,
            };
        _inventoryPanel.AddChild(scroll);

        _inventoryItems =
            new VBoxContainer
            {
                CustomMinimumSize =
                    new Vector2(
                        490.0f,
                        0.0f),
            };
        scroll.AddChild(_inventoryItems);
    }

    private void CreateCraftingPanel()
    {
        _craftingPanel = CreateModalPanel(
            "CRAFTING",
            new Vector2(
                610.0f,
                510.0f));

        var scroll =
            new ScrollContainer
            {
                Position =
                    new Vector2(
                        24.0f,
                        66.0f),
                Size =
                    new Vector2(
                        562.0f,
                        420.0f),
                HorizontalScrollMode =
                    ScrollContainer.ScrollMode.Disabled,
            };
        _craftingPanel.AddChild(scroll);

        _craftingItems =
            new VBoxContainer
            {
                CustomMinimumSize =
                    new Vector2(
                        540.0f,
                        0.0f),
            };
        scroll.AddChild(_craftingItems);
    }

    private ColorRect CreateModalPanel(
        string titleText,
        Vector2 size)
    {
        var panel =
            new ColorRect
            {
                AnchorLeft = 0.5f,
                AnchorRight = 0.5f,
                AnchorTop = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -size.X / 2.0f,
                OffsetTop = -size.Y / 2.0f,
                OffsetRight = size.X / 2.0f,
                OffsetBottom = size.Y / 2.0f,
                Color =
                    new Color(
                        0.025f,
                        0.03f,
                        0.04f,
                        0.96f),
                Visible = false,
                MouseFilter =
                    Control.MouseFilterEnum.Stop,
            };
        AddChild(panel);

        var title =
            new Label
            {
                Position =
                    new Vector2(
                        20.0f,
                        18.0f),
                Size =
                    new Vector2(
                        size.X - 40.0f,
                        34.0f),
                Text = titleText,
                HorizontalAlignment =
                    HorizontalAlignment.Center,
            };
        title.AddThemeFontSizeOverride(
            "font_size",
            24);
        title.AddThemeColorOverride(
            "font_color",
            new Color(
                1.0f,
                0.82f,
                0.26f));
        panel.AddChild(title);

        return panel;
    }

    private void CreateStationPanel()
    {
        _stationPanel =
            CreateModalPanel(
                "WORKSTATION",
                new Vector2(
                    610.0f,
                    420.0f));

        _stationTitle =
            (Label)_stationPanel.GetChild(0);

        var scroll =
            new ScrollContainer
            {
                Position =
                    new Vector2(
                        24.0f,
                        66.0f),
                Size =
                    new Vector2(
                        562.0f,
                        320.0f),
                HorizontalScrollMode =
                    ScrollContainer.ScrollMode.Disabled,
            };
        _stationPanel.AddChild(scroll);

        _stationItems =
            new VBoxContainer
            {
                CustomMinimumSize =
                    new Vector2(
                        540.0f,
                        0.0f),
            };
        scroll.AddChild(_stationItems);
    }

    private void CreateMapPanel()
    {
        _mapPanel =
            CreateModalPanel(
                "DISCOVERY MAP",
                new Vector2(
                    560.0f,
                    470.0f));

        _mapLabel =
            new Label
            {
                Position =
                    new Vector2(
                        34.0f,
                        74.0f),
                Size =
                    new Vector2(
                        492.0f,
                        360.0f),
            };

        _mapLabel.AddThemeFontSizeOverride(
            "font_size",
            18);

        _mapLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                0.90f,
                0.94f,
                0.98f));

        _mapPanel.AddChild(_mapLabel);
    }

    private void CreateNotification()
    {
        _notificationLabel = new Label
        {
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            OffsetLeft = -390.0f,
            OffsetTop = 56.0f,
            OffsetRight = 390.0f,
            OffsetBottom = 96.0f,
            HorizontalAlignment =
                HorizontalAlignment.Center,
            Visible = false,
        };

        _notificationLabel.AddThemeFontSizeOverride(
            "font_size",
            18);
        _notificationLabel.AddThemeColorOverride(
            "font_color",
            new Color(
                0.52f,
                1.0f,
                0.60f));

        AddChild(_notificationLabel);
    }

    private void RebuildInventoryButtons()
    {
        if (_inventory is null
            || _inventoryItems is null)
        {
            return;
        }

        ClearChildren(_inventoryItems);

        foreach (ItemType item in Enum.GetValues<ItemType>())
        {
            int count =
                _inventory.Get(item);

            if (count <= 0)
            {
                continue;
            }

            bool placeable =
                PlacementRules.IsPlaceable(item);

            var button =
                new Button
                {
                    Text =
                        $"{PlacementRules.GetDisplayName(item)}   x{count}" +
                        (placeable
                            ? "     [select for building]"
                            : string.Empty),
                    CustomMinimumSize =
                        new Vector2(
                            470.0f,
                            42.0f),
                    Disabled = !placeable,
                };

            if (placeable)
            {
                ItemType capturedItem = item;
                button.Pressed += () =>
                {
                    PlaceableItemSelected?.Invoke(
                        capturedItem);
                    HidePanels();
                };
            }

            _inventoryItems.AddChild(button);
        }

        if (_inventoryItems.GetChildCount() == 0)
        {
            _inventoryItems.AddChild(
                new Label
                {
                    Text =
                        "(inventory is empty)",
                });
        }
    }

    private void RebuildCraftingButtons()
    {
        if (_inventory is null
            || _craftingItems is null)
        {
            return;
        }

        ClearChildren(_craftingItems);

        foreach (CraftingRecipe recipe in CraftingCatalog.All)
        {
            bool canCraft =
                CraftingService.CanCraft(
                    _inventory,
                    recipe);

            var button =
                new Button
                {
                    Text =
                        $"{recipe.DisplayName}\n" +
                        $"Cost: {CraftingService.DescribeCost(recipe)}",
                    CustomMinimumSize =
                        new Vector2(
                            520.0f,
                            62.0f),
                    Disabled = !canCraft,
                };

            string recipeId =
                recipe.Id;

            button.Pressed += () =>
                CraftRequested?.Invoke(
                    recipeId);

            _craftingItems.AddChild(button);
        }
    }

    private void RebuildStationButtons()
    {
        if (_inventory is null
            || _stationItems is null
            || _activeStation is null)
        {
            return;
        }

        ClearChildren(_stationItems);

        IReadOnlyList<StationRecipeDefinition> recipes =
            StationCraftingService.GetRecipes(
                _activeStation.Value);

        foreach (StationRecipeDefinition recipe
                 in recipes)
        {
            bool canCraft =
                StationCraftingService.CanCraft(
                    _inventory,
                    _activeStation.Value,
                    recipe.Id);

            var button =
                new Button
                {
                    Text =
                        $"{recipe.DisplayName}\n" +
                        $"Cost: {recipe.CostDescription}",
                    CustomMinimumSize =
                        new Vector2(
                            520.0f,
                            62.0f),
                    Disabled =
                        !canCraft,
                };

            ItemType station =
                _activeStation.Value;
            string recipeId =
                recipe.Id;

            button.Pressed += () =>
                StationCraftRequested?.Invoke(
                    station,
                    recipeId);

            _stationItems.AddChild(button);
        }

        if (recipes.Count == 0)
        {
            _stationItems.AddChild(
                new Label
                {
                    Text =
                        "(no recipes available)",
                });
        }
    }

    private static void ClearChildren(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            child.QueueFree();
        }
    }

    private static string BuildCompactInventoryText(
        InventoryState inventory)
    {
        return
            $"TOOLS\n" +
            $"Axe      {(inventory.Has(ItemType.DiamondAxe) ? "DIAMOND" : (inventory.Has(ItemType.IronAxe) ? "IRON" : (inventory.Has(ItemType.Axe) ? "YES" : "NO")))}\n" +
            $"Pickaxe  {(inventory.Has(ItemType.DiamondPickaxe) ? "DIAMOND" : (inventory.Has(ItemType.IronPickaxe) ? "IRON" : (inventory.Has(ItemType.Pickaxe) ? "YES" : "NO")))}\n\n" +
            $"Wood {inventory.Get(ItemType.Wood)}   " +
            $"Stone {inventory.Get(ItemType.Stone)}   " +
            $"Ore {inventory.Get(ItemType.IronOre)}\n" +
            $"Saplings {inventory.Get(ItemType.Sapling)}   " +
            $"Hives {inventory.Get(ItemType.Beehive)}\n" +
            $"Honey {inventory.Get(ItemType.Honey)}   " +
            $"Bars {inventory.Get(ItemType.IronBar)}";
    }
}
