using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class WorldActions
{
    public const double InteractionRange = 110;
    public const double BuildRange = 320;
    public const double MineRange = 190;
    public const double HarvestRange = 100;
    public const int BoatWoodCost = 50;
    public static bool TryPlace(WorldQueries world, InventoryState inventory, WorldPoint actor, ItemType item,
        UndergroundCell cell, BuildLayer layer, out PlacedObjectState? placed, out string reason)
    {
        placed=null; reason=string.Empty;
        if (!world.InReach(actor, WorldGrid.Center(cell.CellX,cell.LogicalLevel),BuildRange))
        { reason="Too far away to build."; return false; }
        if (!world.CanPlace(item,cell,layer,WorldGrid.BearRect(actor),out reason)) return false;
        if (world.State.NextPlacementId >= int.MaxValue-1) { reason="Placement identity limit reached."; return false; }
        if (!inventory.Has(item)) { reason="No items remaining."; return false; }
        placed=world.State.AddPlacedObject(item,cell.CellX,cell.LogicalLevel,layer);
        if (item==ItemType.Sapling) placed.PlantKind=WorldQueries.PlantedKind(world.Definition.Biome);
        inventory.Add(item,-1);
        return true;
    }
    public static bool TryRemove(WorldQueries world, InventoryState inventory, WorldPoint actor, int placementId, out string reason)
    {
        var p=world.State.PlacedObjects.FirstOrDefault(p=>p.PlacementId==placementId);
        reason="Object no longer exists.";
        if (p is null) return false;
        if (!world.InReach(actor,WorldGrid.Center(p.CellX,p.LogicalLevel),BuildRange)) { reason="Too far away."; return false; }
        if (!world.CanRemove(p,out reason)) return false;
        var rewards=new List<HarvestReward> {new(p.Item,1)};
        if (p.Item==ItemType.Beehive && p.StoredOutput>0) rewards.Add(new(ItemType.Honey,p.StoredOutput));
        if (!InventoryTransactions.TryGrant(inventory,rewards,out reason)) return false;
        world.State.RemovePlacedObject(p.PlacementId);
        return true;
    }
    public static bool TryHarvestNatural(WorldQueries world, InventoryState inventory, WorldPoint actor, int featureId, out string reason)
    {
        reason="Resource no longer available.";
        var matches=world.Definition.NaturalFeatures.Where(f=>f.FeatureId==featureId).ToArray();
        if (matches.Length!=1) return false;
        var f=matches[0];
        if (!world.NaturalPresent(f) || !world.InReach(actor,WorldGrid.Feet(f.CellX,f.SurfaceLevel),HarvestRange)) return false;
        var rewards=HarvestRules.GetRewards(f,inventory,world.Definition.Biome);
        if (!InventoryTransactions.TryGrant(inventory,rewards,out reason)) return false;
        world.State.MarkNaturalFeatureHarvested(featureId,NaturalRegrowthRules.GetRegrowSeconds(f.Kind));
        reason=RewardMessage(HarvestRules.GetDisplayName(f),rewards); return true;
    }
    public static bool TryHarvestPlanted(WorldQueries world, InventoryState inventory, WorldPoint actor, int id, out string reason)
    {
        reason="Plant is not ready or is out of range.";
        var p=world.State.PlacedObjects.FirstOrDefault(p=>p.PlacementId==id);
        if (p is null || !PlacedHarvestRules.IsHarvestable(p) || !world.PlantCanGrow(p) ||
            !world.InReach(actor,WorldGrid.Center(p.CellX,p.LogicalLevel),HarvestRange)) return false;
        var rewards=PlacedHarvestRules.GetRewards(p,inventory);
        if (!InventoryTransactions.TryGrant(inventory,rewards,out reason)) return false;
        world.State.RemovePlacedObject(id); reason=RewardMessage(PlacedHarvestRules.GetDisplayName(p),rewards); return true;
    }
    public static bool TryMine(WorldQueries world, InventoryState inventory, WorldPoint actor, UndergroundCell cell, out string reason)
    {
        reason="Too far away to mine.";
        if (!world.InReach(actor,WorldGrid.Center(cell.CellX,cell.LogicalLevel),MineRange) || !world.CanMine(cell,out reason)) return false;
        UndergroundOreSpawn? ore=world.Definition.UndergroundOres.TryGetValue(cell,out var o) ? o : null;
        var rewards=MiningRules.GetRewards(ore,inventory);
        if (!InventoryTransactions.TryGrant(inventory,rewards,out reason)) return false;
        world.State.MarkUndergroundCellMined(cell); reason=RewardMessage("Mined rock",rewards); return true;
    }
    public static bool ValidateEntity(WorldQueries world, int currentIsland, WorldPoint actor, WorldEntityRef context,
        out PlacedObjectState? placed, out GeneratedChestDefinition? chest, out string reason)
    {
        placed=null; chest=null; reason="That interaction is no longer available.";
        return currentIsland==context.IslandId && world.TryEntity(context,out placed,out chest,out var center) && world.InReach(actor,center,InteractionRange);
    }
    public static bool TryStation(WorldQueries world, int currentIsland, InventoryState inventory, WorldPoint actor,
        WorldEntityRef context, string recipeId, ItemType? flower, out string reason)
    {
        if (!ValidateEntity(world,currentIsland,actor,context,out var station,out _,out reason) || station is null) return false;
        var recipe=StationCraftingService.Resolve(station.Item,recipeId,flower);
        if (recipe is null) { reason="Select a valid recipe and flower."; return false; }
        return InventoryTransactions.TryExchange(inventory,recipe.Cost,recipe.Result,1,out reason);
    }
    public static bool TryStorage(WorldQueries world, int currentIsland, WorldPoint actor, WorldEntityRef context,
        out InventoryState? contents, out bool takeOnly, out string reason)
    {
        contents=null; takeOnly=false;
        if (!ValidateEntity(world,currentIsland,actor,context,out var p,out var c,out reason)) return false;
        if (p?.Item==ItemType.Chest) { contents=p.Contents; return true; }
        if (c is not null) { contents=world.State.OpenGeneratedChest(c); takeOnly=true; return true; }
        reason="This object is not storage."; return false;
    }
    public static bool TryTransfer(WorldQueries world, int currentIsland, InventoryState bag, WorldPoint actor,
        WorldEntityRef context, bool deposit, ItemType? item, int amount, out string reason)
    {
        if (!TryStorage(world,currentIsland,actor,context,out var contents,out bool takeOnly,out reason)) return false;
        if (deposit && takeOnly) { reason="Generated loot chests are take-only."; return false; }
        var from=deposit ? bag : contents!; var to=deposit ? contents! : bag;
        IReadOnlyDictionary<ItemType,int> quantities=item is null ? from.Snapshot() : new Dictionary<ItemType,int> {
            [item.Value] = Math.Min(from.Get(item.Value),amount) };
        if (!InventoryTransactions.TryTransfer(from,to,quantities,out reason)) return false;
        if (takeOnly && contents!.IsEmpty) world.State.MarkGeneratedChestLooted(context.EntityId);
        world.State.Touch(); return true;
    }
    public static bool TryHoney(WorldQueries world, InventoryState bag, WorldPoint actor, WorldEntityRef id, out string reason)
    {
        if (!ValidateEntity(world,world.Definition.IslandId,actor,id,out var hive,out _,out reason) || hive?.Item!=ItemType.Beehive) return false;
        if (hive.StoredOutput<=0) { reason="No honey is ready."; return false; }
        int count=hive.StoredOutput;
        if (!InventoryTransactions.TryGrant(bag,new[] {new HarvestReward(ItemType.Honey,count)},out reason)) return false;
        hive.StoredOutput=0; world.State.Touch(); reason=$"Collected {count} honey."; return true;
    }
    public static bool TryBoat(WorldQueries world, InventoryState bag, WorldPoint actor, BoatSide side, out string reason)
    {
        reason="Move closer to the shoreline.";
        if (!Enum.IsDefined(side)) return false;
        int x=side==BoatSide.Left?0:world.Definition.WidthCells-1;
        if (!world.InReach(actor,WorldGrid.Feet(x,world.Definition.SurfaceLevels[x]),InteractionRange)) return false;
        if (world.State.IsBoatBuilt(side)) { reason="Boat is already built."; return false; }
        if (!bag.Has(ItemType.Wood,BoatWoodCost)) { reason=$"Boat needs {BoatWoodCost} wood."; return false; }
        bag.Add(ItemType.Wood,-BoatWoodCost); world.State.BuildBoat(side); world.State.Touch();
        reason="Boat built. Release and press F again to sail."; return true;
    }
    public static string RewardMessage(string source, IReadOnlyList<HarvestReward> rewards) => source+": "+string.Join(", ",rewards.Select(
        r=>$"+{r.Amount} {PlacementRules.GetDisplayName(r.Item).ToLowerInvariant()}"));
}
