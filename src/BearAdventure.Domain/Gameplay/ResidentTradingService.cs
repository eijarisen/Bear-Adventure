namespace BearAdventure.Domain.Gameplay;

public static class ResidentTradingService
{
    public static IReadOnlyList<TradeOfferDefinition> GetOffers(ResidentState resident)
    {
        ArgumentNullException.ThrowIfNull(resident);
        var result = new List<TradeOfferDefinition>();
        switch (resident.Kind)
        {
            case ResidentKind.Bear:
                result.Add(Offer("honey", "Sell Honey (+5 friendship)", (ItemType.Honey,1), (ItemType.GoldCoin,2), 5, true));
                result.Add(Offer("juice", "Sell Juice (+1 friendship)", (ItemType.Juice,1), (ItemType.GoldCoin,2), 1, true));
                result.Add(Offer("soup", "Sell Soup (+2 friendship)", (ItemType.Soup,1), (ItemType.GoldCoin,5), 2, true));
                if (resident.OffersDiamondAxe)
                    result.Add(Offer("diamond-axe", "Buy Diamond Axe", (ItemType.GoldCoin,1000), (ItemType.DiamondAxe,1)));
                if (resident.OffersDiamondPickaxe)
                    result.Add(Offer("diamond-pickaxe", "Buy Diamond Pickaxe", (ItemType.GoldCoin,1000), (ItemType.DiamondPickaxe,1)));
                break;
            case ResidentKind.Cat:
                result.Add(Offer("red-yellow", "2 Red Flowers → 1 Yellow (+4 friendship)", (ItemType.RedFlower,2), (ItemType.YellowFlower,1), 4));
                result.Add(Offer("yellow-blue", "2 Yellow Flowers → 1 Blue (+4 friendship)", (ItemType.YellowFlower,2), (ItemType.BlueFlower,1), 4));
                result.Add(Offer("blue-orange", "2 Blue Flowers → 1 Orange (+4 friendship)", (ItemType.BlueFlower,2), (ItemType.OrangeFlower,1), 4));
                result.Add(Offer("orange-purple", "2 Orange Flowers → 1 Purple (+4 friendship)", (ItemType.OrangeFlower,2), (ItemType.PurpleFlower,1), 4));
                break;
            case ResidentKind.PolarBear:
                result.Add(Offer("fishing-rod", "Buy Fishing Rod", (ItemType.GoldCoin,100), (ItemType.FishingRod,1)));
                result.Add(Offer("fish", "Sell Fish (+5 trust)", (ItemType.Fish,1), (ItemType.GoldCoin,5), 5));
                break;
            case ResidentKind.Monkey:
                result.Add(Offer("banana", "Sell Banana (+5 trust)", (ItemType.Banana,1), (ItemType.GoldCoin,5), 5));
                break;
        }
        return result;
    }

    public static bool CanTrade(ResidentState resident, InventoryState bag, string offerId, out string reason)
    {
        ArgumentNullException.ThrowIfNull(resident); ArgumentNullException.ThrowIfNull(bag);
        var offer = GetOffers(resident).FirstOrDefault(o => o.Id == offerId);
        if (offer is null) { reason = "That offer is not available."; return false; }
        foreach (var (item,count) in offer.Cost)
            if (!bag.Has(item,count)) { reason = $"Need {count} {PlacementRules.GetDisplayName(item).ToLowerInvariant()}."; return false; }
        if (offer.UsesResidentWallet)
        {
            int payout = offer.Result.TryGetValue(ItemType.GoldCoin,out int amount) ? amount : 0;
            if (resident.Coins < payout) { reason = $"{resident.Name} is out of coins. Check back later."; return false; }
        }
        // Check result overflow without mutating the bag.
        foreach (var (item,count) in offer.Result)
        {
            try { checked { _ = bag.Get(item) + count; } }
            catch (OverflowException) { reason = "Inventory quantity limit reached."; return false; }
        }
        reason = string.Empty; return true;
    }

    public static bool TryTrade(ResidentState resident, InventoryState bag, string offerId, out string reason)
    {
        if (!CanTrade(resident,bag,offerId,out reason)) return false;
        var offer = GetOffers(resident).First(o => o.Id == offerId);
        if (!InventoryTransactions.TryExchange(bag,offer.Cost,offer.Result,1,out reason)) return false;
        if (offer.UsesResidentWallet && offer.Result.TryGetValue(ItemType.GoldCoin,out int payout)) resident.Coins -= payout;
        resident.Sympathy = Math.Min(100,resident.Sympathy + offer.SympathyGain);
        string relationship = resident.Kind is ResidentKind.PolarBear or ResidentKind.Monkey ? "trust" : "friendship";
        reason = offer.SympathyGain > 0
            ? $"Trade complete. {resident.Name}: {resident.Sympathy}/100 {relationship}."
            : "Trade complete.";
        return true;
    }

    public static bool TryClaimSpecialReward(ResidentState resident, InventoryState bag, out string reason)
    {
        ArgumentNullException.ThrowIfNull(resident); ArgumentNullException.ThrowIfNull(bag);
        if (resident.Kind is not (ResidentKind.PolarBear or ResidentKind.Monkey))
        { reason = "This resident has no special chest."; return false; }
        if (resident.Sympathy < 100) { reason = "Reach 100 trust first."; return false; }
        if (resident.SpecialChestClaimed) { reason = "The special chest is already empty."; return false; }
        HarvestReward[] rewards = resident.Kind == ResidentKind.PolarBear
            ? [new(ItemType.GoldCoin,150), new(ItemType.Honey,10)]
            : [new(ItemType.GoldCoin,200), new(ItemType.Honey,15)];
        if (!InventoryTransactions.TryGrant(bag,rewards,out reason)) return false;
        resident.SpecialChestClaimed = true;
        reason = resident.Kind == ResidentKind.PolarBear
            ? "Ice chest opened: +150 gold, +10 honey."
            : "Golden chest opened: +200 gold, +15 honey.";
        return true;
    }

    private static TradeOfferDefinition Offer(string id,string name,(ItemType,int) cost,(ItemType,int) result,
        int sympathy=0,bool wallet=false) => new(id,name,
        new Dictionary<ItemType,int>{{cost.Item1,cost.Item2}},
        new Dictionary<ItemType,int>{{result.Item1,result.Item2}},sympathy,wallet);
}
