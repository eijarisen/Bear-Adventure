using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public static class ProgressionService
{
    public static InventoryState CreateNormalStarter()
    {
        // Normal worlds start without development tools/resources. Bare-hand
        // harvesting remains deliberately possible, so the first axe/pickaxe
        // is earned through the crafting loop rather than granted.
        return new InventoryState();
    }

    public static string GetContextHint(GameSessionState session, IslandDefinition island)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(island);
        InventoryState bag = session.Inventory;

        if (!bag.Has(ItemType.Axe) && bag.Get(ItemType.Wood) < 5)
            return "First goal: hold F near trees to gather wood.";
        if (!bag.Has(ItemType.Axe) && bag.Get(ItemType.Stone) < 10)
            return "Gather stone, then press C and craft an axe.";
        if (!bag.Has(ItemType.Axe))
            return "Press C and craft an axe (5 wood, 10 stone).";
        if (!bag.Has(ItemType.Pickaxe) && bag.Get(ItemType.Stone) < 15)
            return "Gather more stone for a pickaxe (5 wood, 15 stone).";
        if (!bag.Has(ItemType.Pickaxe))
            return "Press C and craft a pickaxe.";
        if (!bag.Has(ItemType.Forge) && bag.Get(ItemType.Stone) < 20)
            return "A forge opens iron progression; collect 20 stone.";
        if (!bag.Has(ItemType.Forge))
            return "Craft and place a forge, then smelt iron ore into bars.";
        if (!bag.Has(ItemType.Beehive))
            return "Build a flower garden and craft a beehive to start honey production.";
        if (!session.DiscoveredIslandIds.Any(id => id != 0))
            return "Build a 50-wood shore boat to discover another biome.";
        if (!session.Residents.Any(r => r.Befriended && r.Kind == ResidentKind.Bear))
            return "Trade with ordinary bears; at 100 friendship they can become workers.";
        if (!session.Residents.Any(r => r.Worker?.Assigned == true))
            return "Assign a befriended ordinary bear to a chest to automate a settlement.";
        return $"Explore {island.Biome.ToString().ToLowerInvariant()} island {island.IslandId}, improve the settlement, or expand automation.";
    }

    public static IReadOnlyDictionary<ItemType,string> AcquisitionPaths { get; } =
        new Dictionary<ItemType,string>
        {
            [ItemType.Wood]="trees, bushes, planted mature trees",
            [ItemType.Stone]="rocks and underground mining",
            [ItemType.Sandstone]="desert rocks",
            [ItemType.Sapling]="trees and planted mature trees",
            [ItemType.RedFlower]="natural flowers",
            [ItemType.YellowFlower]="natural flowers or cat exchange",
            [ItemType.BlueFlower]="natural flowers or cat exchange",
            [ItemType.OrangeFlower]="cat exchange",
            [ItemType.PurpleFlower]="cat exchange",
            [ItemType.PinkFlower]="rare cactus harvest",
            [ItemType.Cactus]="desert cacti",
            [ItemType.MushroomBrown]="mushrooms",
            [ItemType.MushroomRed]="mushrooms",
            [ItemType.Grass]="grass",
            [ItemType.IronOre]="rocks and underground ore",
            [ItemType.IronBar]="forge",
            [ItemType.Honey]="beehive",
            [ItemType.GoldCoin]="trades and generated chests",
            [ItemType.Axe]="crafting",
            [ItemType.Pickaxe]="crafting",
            [ItemType.IronAxe]="anvil",
            [ItemType.IronPickaxe]="anvil",
            [ItemType.DiamondAxe]="rare bear trade",
            [ItemType.DiamondPickaxe]="rare bear trade",
            [ItemType.Beehive]="crafting",
            [ItemType.Chest]="crafting",
            [ItemType.Forge]="crafting",
            [ItemType.Anvil]="crafting",
            [ItemType.Cauldron]="crafting",
            [ItemType.Juice]="cauldron",
            [ItemType.Soup]="cauldron",
            [ItemType.Banana]="jungle palms",
            [ItemType.Fish]="fishing",
            [ItemType.FishingRod]="polar-bear trade",
        };
    public static IReadOnlyDictionary<ItemType,string> UsePaths { get; } =
        new Dictionary<ItemType,string>
        {
            [ItemType.Wood]="crafting, building, boats, forge fuel",
            [ItemType.Stone]="crafting and building",
            [ItemType.Sandstone]="building",
            [ItemType.Sapling]="planting and worker planting",
            [ItemType.RedFlower]="planting, cooking, cat exchange, honey gardens",
            [ItemType.YellowFlower]="planting, cooking, cat exchange, honey gardens",
            [ItemType.BlueFlower]="planting, cooking, cat exchange, honey gardens",
            [ItemType.OrangeFlower]="planting, cooking, cat exchange, honey gardens",
            [ItemType.PurpleFlower]="planting, cooking and honey gardens",
            [ItemType.PinkFlower]="planting, cooking and honey gardens",
            [ItemType.Cactus]="building and cooking",
            [ItemType.MushroomBrown]="cooking",
            [ItemType.MushroomRed]="cooking",
            [ItemType.Grass]="planting and decoration",
            [ItemType.IronOre]="forge",
            [ItemType.IronBar]="anvil and advanced crafting",
            [ItemType.Honey]="bear trading",
            [ItemType.GoldCoin]="fishing rod and rare tool purchases",
            [ItemType.Axe]="faster harvesting and anvil upgrade",
            [ItemType.Pickaxe]="faster mining and anvil upgrade",
            [ItemType.IronAxe]="faster harvesting",
            [ItemType.IronPickaxe]="faster mining",
            [ItemType.DiamondAxe]="fastest harvesting and doubled yields",
            [ItemType.DiamondPickaxe]="fastest mining and doubled yields",
            [ItemType.Beehive]="honey production",
            [ItemType.Chest]="storage and worker home",
            [ItemType.Forge]="smelting",
            [ItemType.Anvil]="smithing",
            [ItemType.Cauldron]="cooking",
            [ItemType.Juice]="bear trading",
            [ItemType.Soup]="bear trading",
            [ItemType.Banana]="monkey trust trade",
            [ItemType.Fish]="polar bear trust trade",
            [ItemType.FishingRod]="fishing",
        };

}
