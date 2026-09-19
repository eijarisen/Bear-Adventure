using BearAdventure.Domain.Gameplay;
using BearAdventure.Domain.World;
using BearAdventure.Player;
using BearAdventure.World;
using Godot;

namespace BearAdventure.Interaction;

public partial class HarvestController : Node
{
    private const float HarvestRange = 100.0f;
    private const float MiningRange = 190.0f;
    private const float InteractionRange = 110.0f;
    private const int BoatWoodCost = 50;

    private BearController? _player;
    private IslandView? _island;
    private InventoryState? _inventory;

    private int? _activeNaturalFeatureId;
    private int? _activePlacedObjectId;
    private UndergroundCell? _activeMineCell;

    private double _progressSeconds;
    private string _statusText = string.Empty;
    private bool _interactWasDown;

    public event Action<string>? StatusChanged;

    public event Action<string>? HarvestCompleted;

    public event Action<ItemType>? StationRequested;

    public event Action<BoatSide>? TravelRequested;

    public bool MiningBlocked { get; set; }

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

        ResetInteraction();
    }

    public void DetachIsland()
    {
        ClearIndicators();

        if (_player is not null)
        {
            _player.MovementLocked =
                false;
        }

        _island = null;
        _activeNaturalFeatureId = null;
        _activePlacedObjectId = null;
        _activeMineCell = null;
        _progressSeconds = 0.0;

        _interactWasDown =
            Input.IsPhysicalKeyPressed(
                Key.F);

        SetStatus(
            string.Empty);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player is null
            || _island is null
            || _inventory is null)
        {
            return;
        }

        bool interactDown =
            Input.IsPhysicalKeyPressed(
                Key.F);

        NaturalFeatureSpawn? natural =
            _island.FindNearestHarvestable(
                _player.Position,
                HarvestRange);

        PlacedObjectState? planted =
            _island.FindNearestPlacedHarvestable(
                _player.Position,
                HarvestRange);

        Vector2 mouseViewportPosition =
            GetViewport().GetMousePosition();

        Vector2 mouseWorldPosition =
            _island
                .GetCanvasTransform()
                .AffineInverse()
                * mouseViewportPosition;

        UndergroundCell? mineCell =
            _island.FindHoveredMineableCell(
                mouseWorldPosition,
                _player.Position,
                MiningRange);

        HarvestTargetKind target =
            ChooseNearestHarvestTarget(
                natural,
                planted,
                mineCell);

        bool mouseMineDown =
            !MiningBlocked
            && Input.IsMouseButtonPressed(
                MouseButton.Left);

        bool harvestInputDown =
            interactDown
            || (target == HarvestTargetKind.Mining
                && mouseMineDown);

        if (harvestInputDown
            && target
                != HarvestTargetKind.None)
        {
            switch (target)
            {
                case HarvestTargetKind.Natural:
                    ProcessNaturalHarvest(
                        natural!.Value,
                        delta);
                    break;

                case HarvestTargetKind.Planted:
                    ProcessPlacedHarvest(
                        planted!,
                        delta);
                    break;

                case HarvestTargetKind.Mining:
                    ProcessMining(
                        mineCell!.Value,
                        delta);
                    break;
            }

            _interactWasDown = true;
            return;
        }

        _player.MovementLocked =
            false;

        ResetHarvestProgress();

        PlacedObjectState? interactive =
            _island.FindNearestInteractive(
                _player.Position,
                InteractionRange);

        GeneratedChestDefinition? generatedChest =
            _island.FindNearestGeneratedChest(
                _player.Position,
                InteractionRange);

        BoatSide? boatSide =
            _island.FindNearbyBoatSite(
                _player.Position,
                InteractionRange);

        if (!interactDown)
        {
            ShowIdlePrompt(
                target,
                natural,
                planted,
                mineCell,
                interactive,
                generatedChest,
                boatSide);

            _interactWasDown =
                false;

            return;
        }

        if (!_interactWasDown)
        {
            if (generatedChest is not null)
            {
                ActivateGeneratedChest(
                    generatedChest);
            }
            else if (interactive is not null)
            {
                ActivatePlacedObject(
                    interactive);
            }
            else if (boatSide is not null)
            {
                ActivateBoatSite(
                    boatSide.Value);
            }
            else
            {
                SetStatus(
                    "Nothing harvestable or usable nearby.");
            }
        }

        _interactWasDown =
            true;
    }

    private HarvestTargetKind ChooseNearestHarvestTarget(
        NaturalFeatureSpawn? natural,
        PlacedObjectState? planted,
        UndergroundCell? mineCell)
    {
        if (_player is null
            || _island is null)
        {
            return HarvestTargetKind.None;
        }

        // Mining is intentionally cursor-directed. If the mouse is over an
        // exposed mineable terrain cell in range, it wins over proximity
        // harvesting around the bear.
        if (mineCell is not null)
        {
            return HarvestTargetKind.Mining;
        }

        HarvestTargetKind best =
            HarvestTargetKind.None;

        float bestDistance =
            float.PositiveInfinity;

        if (natural is not null)
        {
            float distance =
                _player.Position
                    .DistanceSquaredTo(
                        _island
                            .GetFeatureWorldPosition(
                                natural.Value));

            if (distance < bestDistance)
            {
                bestDistance =
                    distance;

                best =
                    HarvestTargetKind.Natural;
            }
        }

        if (planted is not null)
        {
            float distance =
                _player.Position
                    .DistanceSquaredTo(
                        _island
                            .GetBuildCellCenter(
                                planted.CellX,
                                planted.LogicalLevel));

            if (distance < bestDistance)
            {
                best =
                    HarvestTargetKind.Planted;
            }
        }

        return best;
    }

    private void ProcessNaturalHarvest(
        NaturalFeatureSpawn feature,
        double delta)
    {
        if (_player is null
            || _island is null
            || _inventory is null)
        {
            return;
        }

        if (_activeNaturalFeatureId
            != feature.FeatureId)
        {
            _activeNaturalFeatureId =
                feature.FeatureId;

            _activePlacedObjectId =
                null;

            _activeMineCell =
                null;

            _progressSeconds =
                0.0;
        }

        _player.MovementLocked =
            true;

        _island.SetPlacedHarvestIndicator(
            null,
            0.0f);

        _island.SetMineIndicator(
            null,
            0.0f);

        double durationSeconds =
            HarvestRules.GetDurationSeconds(
                feature.Kind,
                _inventory);

        _progressSeconds +=
            delta;

        float ratio =
            durationSeconds <= 0.0
                ? 1.0f
                : (float)Math.Clamp(
                    _progressSeconds
                        / durationSeconds,
                    0.0,
                    1.0);

        _island.SetHarvestIndicator(
            feature.FeatureId,
            ratio);

        SetStatus(
            $"Harvesting {HarvestRules.GetDisplayName(feature)} " +
            $"{Math.Round(ratio * 100.0f):0}%");

        if (_progressSeconds
            >= durationSeconds)
        {
            CompleteNaturalHarvest(
                feature);
        }
    }

    private void ProcessPlacedHarvest(
        PlacedObjectState placed,
        double delta)
    {
        if (_player is null
            || _island is null
            || _inventory is null)
        {
            return;
        }

        if (_activePlacedObjectId
            != placed.PlacementId)
        {
            _activePlacedObjectId =
                placed.PlacementId;

            _activeNaturalFeatureId =
                null;

            _activeMineCell =
                null;

            _progressSeconds =
                0.0;
        }

        _player.MovementLocked =
            true;

        _island.SetHarvestIndicator(
            null,
            0.0f);

        _island.SetMineIndicator(
            null,
            0.0f);

        double durationSeconds =
            PlacedHarvestRules.GetDurationSeconds(
                placed,
                _inventory);

        _progressSeconds +=
            delta;

        float ratio =
            (float)Math.Clamp(
                _progressSeconds
                    / durationSeconds,
                0.0,
                1.0);

        _island.SetPlacedHarvestIndicator(
            placed.PlacementId,
            ratio);

        SetStatus(
            $"Harvesting {PlacedHarvestRules.GetDisplayName(placed)} " +
            $"{Math.Round(ratio * 100.0f):0}%");

        if (_progressSeconds
            >= durationSeconds)
        {
            CompletePlacedHarvest(
                placed);
        }
    }

    private void ProcessMining(
        UndergroundCell cell,
        double delta)
    {
        if (_player is null
            || _island is null
            || _inventory is null)
        {
            return;
        }

        if (_activeMineCell
            != cell)
        {
            _activeMineCell =
                cell;

            _activeNaturalFeatureId =
                null;

            _activePlacedObjectId =
                null;

            _progressSeconds =
                0.0;
        }

        _player.MovementLocked =
            true;

        _island.SetHarvestIndicator(
            null,
            0.0f);

        _island.SetPlacedHarvestIndicator(
            null,
            0.0f);

        double durationSeconds =
            MiningRules.GetDurationSeconds(
                _inventory);

        _progressSeconds +=
            delta;

        float ratio =
            (float)Math.Clamp(
                _progressSeconds
                    / durationSeconds,
                0.0,
                1.0);

        _island.SetMineIndicator(
            cell,
            ratio);

        SetStatus(
            $"Mining depth {Math.Abs(cell.LogicalLevel)} " +
            $"{Math.Round(ratio * 100.0f):0}%");

        if (_progressSeconds
            >= durationSeconds)
        {
            CompleteMining(
                cell);
        }
    }

    private void CompleteNaturalHarvest(
        NaturalFeatureSpawn requestedFeature)
    {
        if (_island is null
            || _inventory is null)
        {
            return;
        }

        if (!_island.TryHarvestFeature(
            requestedFeature.FeatureId,
            out NaturalFeatureSpawn harvestedFeature))
        {
            ResetHarvestProgress();
            return;
        }

        GrantRewards(
            HarvestRules.GetRewards(
                harvestedFeature,
                _inventory),
            HarvestRules.GetDisplayName(
                harvestedFeature));

        ResetHarvestProgress();

        SetStatus(
            "Searching for another nearby resource...");
    }

    private void CompletePlacedHarvest(
        PlacedObjectState requested)
    {
        if (_island is null
            || _inventory is null)
        {
            return;
        }

        if (!_island.TryHarvestPlacedObject(
            requested.PlacementId,
            out PlacedObjectState harvested))
        {
            ResetHarvestProgress();
            return;
        }

        GrantRewards(
            PlacedHarvestRules.GetRewards(
                harvested,
                _inventory),
            PlacedHarvestRules.GetDisplayName(
                harvested));

        ResetHarvestProgress();

        SetStatus(
            "Searching for another nearby resource...");
    }

    private void CompleteMining(
        UndergroundCell requestedCell)
    {
        if (_island is null)
        {
            return;
        }

        if (!_island.TryMineUndergroundCell(
            requestedCell,
            out UndergroundOreSpawn? ore))
        {
            ResetHarvestProgress();
            return;
        }

        GrantRewards(
            MiningRules.GetRewards(
                ore,
                _inventory!),
            "mined rock");

        ResetHarvestProgress();

        SetStatus(
            "Searching for another mineable cell...");
    }

    private void GrantRewards(
        IReadOnlyList<HarvestReward> rewards,
        string sourceName)
    {
        if (_inventory is null)
        {
            return;
        }

        foreach (HarvestReward reward
                 in rewards)
        {
            _inventory.Add(
                reward.Item,
                reward.Amount);
        }

        string rewardText =
            rewards.Count == 0
                ? "nothing"
                : string.Join(
                    ", ",
                    rewards.Select(
                        reward =>
                            $"+{reward.Amount} " +
                            PlacementRules
                                .GetDisplayName(
                                    reward.Item)
                                .ToLowerInvariant()));

        HarvestCompleted?.Invoke(
            $"{sourceName}: {rewardText}");
    }

    private void ActivateGeneratedChest(
        GeneratedChestDefinition chest)
    {
        if (_island is null)
        {
            return;
        }

        if (_island.IsGeneratedChestLooted(
            chest.ChestId))
        {
            SetStatus(
                "This chest is empty.");
            return;
        }

        if (!_island.TryLootGeneratedChest(
            chest.ChestId,
            out IReadOnlyList<HarvestReward> rewards))
        {
            return;
        }

        GrantRewards(
            rewards,
            "chest");

        SetStatus(
            "Chest looted.");
    }

    private void ActivatePlacedObject(
        PlacedObjectState placed)
    {
        if (_island is null
            || _inventory is null)
        {
            return;
        }

        if (placed.Item
            == ItemType.Beehive)
        {
            int honey =
                _island.CollectHiveHoney(
                    placed.PlacementId);

            if (honey <= 0)
            {
                SetStatus(
                    "The hive has no honey ready yet.");
                return;
            }

            _inventory.Add(
                ItemType.Honey,
                honey);

            HarvestCompleted?.Invoke(
                $"Collected +{honey} honey.");

            SetStatus(
                "Honey collected.");
            return;
        }

        if (placed.Item is
            ItemType.Forge
            or ItemType.Anvil
            or ItemType.Cauldron)
        {
            StationRequested?.Invoke(
                placed.Item);
        }
    }

    private void ActivateBoatSite(
        BoatSide side)
    {
        if (_island is null
            || _inventory is null)
        {
            return;
        }

        if (_island.IsBoatBuilt(side))
        {
            TravelRequested?.Invoke(
                side);

            return;
        }

        if (!_inventory.Has(
            ItemType.Wood,
            BoatWoodCost))
        {
            SetStatus(
                $"Boat construction requires {BoatWoodCost} wood.");

            return;
        }

        if (!_island.BuildBoat(side))
        {
            return;
        }

        _inventory.Add(
            ItemType.Wood,
            -BoatWoodCost);

        HarvestCompleted?.Invoke(
            $"Built the {side.ToString().ToLowerInvariant()} boat for {BoatWoodCost} wood.");

        SetStatus(
            "Press F again to sail.");
    }

    private void ShowIdlePrompt(
        HarvestTargetKind target,
        NaturalFeatureSpawn? natural,
        PlacedObjectState? planted,
        UndergroundCell? mineCell,
        PlacedObjectState? interactive,
        GeneratedChestDefinition? generatedChest,
        BoatSide? boatSide)
    {
        if (_island is null)
        {
            return;
        }

        switch (target)
        {
            case HarvestTargetKind.Natural:
                _island.SetPlacedHarvestIndicator(
                    null,
                    0.0f);

                _island.SetMineIndicator(
                    null,
                    0.0f);

                _island.SetHarvestIndicator(
                    natural!.Value.FeatureId,
                    0.0f);

                SetStatus(
                    $"Hold F to harvest " +
                    $"{HarvestRules.GetDisplayName(natural.Value)}.");

                return;

            case HarvestTargetKind.Planted:
                _island.SetHarvestIndicator(
                    null,
                    0.0f);

                _island.SetMineIndicator(
                    null,
                    0.0f);

                _island.SetPlacedHarvestIndicator(
                    planted!.PlacementId,
                    0.0f);

                SetStatus(
                    $"Hold F to harvest " +
                    $"{PlacedHarvestRules.GetDisplayName(planted)}.");

                return;

            case HarvestTargetKind.Mining:
                _island.SetHarvestIndicator(
                    null,
                    0.0f);

                _island.SetPlacedHarvestIndicator(
                    null,
                    0.0f);

                _island.SetMineIndicator(
                    mineCell,
                    0.0f);

                SetStatus(
                    mineCell!.Value.LogicalLevel > 0
                        ? $"Hovered surface cell L{mineCell.Value.LogicalLevel}: hold F or left mouse to mine."
                        : $"Hovered depth {Math.Abs(mineCell.Value.LogicalLevel)}: hold F or left mouse to mine.");

                return;
        }

        ClearIndicators();

        if (generatedChest is not null)
        {
            SetStatus(
                _island.IsGeneratedChestLooted(
                    generatedChest.ChestId)
                    ? "Generated chest is empty."
                    : "Press F to loot chest.");

            return;
        }

        if (interactive is not null)
        {
            if (interactive.Item
                == ItemType.Beehive)
            {
                SetStatus(
                    interactive.StoredOutput > 0
                        ? $"Press F to collect {interactive.StoredOutput} honey."
                        : "Hive is producing honey.");

                return;
            }

            SetStatus(
                $"Press F to use " +
                $"{PlacementRules.GetDisplayName(interactive.Item).ToLowerInvariant()}.");

            return;
        }

        if (boatSide is not null)
        {
            SetStatus(
                _island.IsBoatBuilt(
                    boatSide.Value)
                    ? $"Press F to sail {boatSide.Value.ToString().ToLowerInvariant()}."
                    : $"Press F to build boat ({BoatWoodCost} wood).");

            return;
        }

        SetStatus(
            string.Empty);
    }

    private void ResetHarvestProgress()
    {
        _activeNaturalFeatureId =
            null;

        _activePlacedObjectId =
            null;

        _activeMineCell =
            null;

        _progressSeconds =
            0.0;

        ClearIndicators();
    }

    private void ClearIndicators()
    {
        _island?.SetHarvestIndicator(
            null,
            0.0f);

        _island?.SetPlacedHarvestIndicator(
            null,
            0.0f);

        _island?.SetMineIndicator(
            null,
            0.0f);
    }

    private void ResetInteraction()
    {
        ResetHarvestProgress();

        _interactWasDown =
            Input.IsPhysicalKeyPressed(
                Key.F);

        if (_player is not null)
        {
            _player.MovementLocked =
                false;
        }

        SetStatus(
            string.Empty);
    }

    private void SetStatus(string value)
    {
        if (_statusText == value)
        {
            return;
        }

        _statusText =
            value;

        StatusChanged?.Invoke(
            value);
    }

    private enum HarvestTargetKind
    {
        None,
        Natural,
        Planted,
        Mining,
    }
}
