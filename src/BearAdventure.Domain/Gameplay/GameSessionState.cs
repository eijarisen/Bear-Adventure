using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public sealed class GameSessionState
{
    private readonly Dictionary<int, IslandDeltaState> _islands = new();
    private readonly HashSet<int> _discoveredIslandIds = new() { 0 };

    public GameSessionState(InventoryState inventory)
    {
        Inventory =
            inventory
            ?? throw new ArgumentNullException(nameof(inventory));
    }

    public InventoryState Inventory { get; }

    public int CurrentIslandId { get; set; }

    public IReadOnlyDictionary<int, IslandDeltaState> Islands
        => _islands;

    public IReadOnlySet<int> DiscoveredIslandIds
        => _discoveredIslandIds;

    public IslandDeltaState GetIslandState(int islandId)
    {
        DiscoverIsland(islandId);

        if (_islands.TryGetValue(
            islandId,
            out IslandDeltaState? state))
        {
            return state;
        }

        state = new IslandDeltaState();
        _islands.Add(islandId, state);
        return state;
    }

    public void SetIslandState(
        int islandId,
        IslandDeltaState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        DiscoverIsland(islandId);
        _islands[islandId] = state;
    }

    public void DiscoverIsland(int islandId)
    {
        _discoveredIslandIds.Add(islandId);
    }

    public void ReplaceDiscoveredIslands(
        IEnumerable<int> islandIds)
    {
        ArgumentNullException.ThrowIfNull(islandIds);

        _discoveredIslandIds.Clear();
        _discoveredIslandIds.Add(0);

        foreach (int islandId in islandIds)
        {
            _discoveredIslandIds.Add(islandId);
        }

        _discoveredIslandIds.Add(CurrentIslandId);
    }

    public static GameSessionState CreateDevelopmentStarter()
    {
        return new GameSessionState(
            InventoryState.CreateDevelopmentStarter());
    }
}
