using BearAdventure.Domain.World;

namespace BearAdventure.Domain.Gameplay;

public sealed class GameSessionState
{
    private readonly Dictionary<int, IslandDeltaState> _islands = new();
    private readonly Dictionary<int, IslandDefinition> _baselines = new();
    private readonly HashSet<int> _discovered = new() { 0 };
    private readonly List<ResidentState> _residents = new();

    public GameSessionState(InventoryState inventory, string seed = "bear-adventure-development-001")
    {
        Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        Seed = new WorldSeed(seed).Value;
    }

    public string Seed { get; }
    public InventoryState Inventory { get; }
    public int CurrentIslandId { get; set; }
    public PlayerLocation? PlayerLocation { get; set; }
    public long SimulationTick { get; set; }
    public double SimulationRemainder { get; set; }
    public double SimulationSeconds => SimulationTick * 0.5 + SimulationRemainder;
    public IReadOnlyDictionary<int, IslandDeltaState> Islands => _islands;
    public IReadOnlyDictionary<int, IslandDefinition> Baselines => _baselines;
    public IReadOnlySet<int> DiscoveredIslandIds => _discovered;
    public IReadOnlyList<ResidentState> Residents => _residents;

    public IslandDeltaState GetIslandState(int id)
    {
        _discovered.Add(id);
        if (!_islands.TryGetValue(id, out var state)) { state = new(); _islands.Add(id, state); }
        return state;
    }

    public IslandDefinition GetDefinition(int id, IslandGenerator generator)
    {
        if (!_baselines.TryGetValue(id, out var d))
        { d = generator.Generate(new WorldSeed(Seed), id); _baselines.Add(id, d); }
        return d;
    }

    public void SetBaseline(IslandDefinition definition) => _baselines[definition.IslandId] = definition;
    public void SetIslandState(int id, IslandDeltaState state) { _discovered.Add(id); _islands[id] = state; }
    public void DiscoverIsland(int id) => _discovered.Add(id);
    public void ReplaceDiscoveredIslands(IEnumerable<int> ids)
    { _discovered.Clear(); foreach (int id in ids) _discovered.Add(id); _discovered.Add(0); _discovered.Add(CurrentIslandId); }

    public void AddResident(ResidentState resident)
    {
        ArgumentNullException.ThrowIfNull(resident);
        if (_residents.Any(r => r.ResidentId == resident.ResidentId))
            throw new InvalidOperationException($"Duplicate resident identity {resident.ResidentId}.");
        _residents.Add(resident);
    }

    public void ReplaceResidents(IEnumerable<ResidentState> residents)
    {
        ArgumentNullException.ThrowIfNull(residents);
        _residents.Clear();
        var ids = new HashSet<int>();
        foreach (var resident in residents)
        {
            if (resident is null || !ids.Add(resident.ResidentId))
                throw new ArgumentException("Resident collection contains a null or duplicate identity.", nameof(residents));
            _residents.Add(resident);
        }
    }

    public ResidentState? FindResident(int residentId) => _residents.FirstOrDefault(r => r.ResidentId == residentId);

    public static GameSessionState CreateNormalStarter(string seed) =>
        new(InventoryState.CreateNormalStarter(), seed);

    public static GameSessionState CreateDevelopmentStarter(string seed = "bear-adventure-development-001") =>
        new(InventoryState.CreateDevelopmentStarter(), seed);
}
