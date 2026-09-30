namespace BearAdventure.Domain.Gameplay;

public sealed record TradeOfferDefinition(
    string Id,
    string DisplayName,
    IReadOnlyDictionary<ItemType, int> Cost,
    IReadOnlyDictionary<ItemType, int> Result,
    int SympathyGain = 0,
    bool UsesResidentWallet = false);
