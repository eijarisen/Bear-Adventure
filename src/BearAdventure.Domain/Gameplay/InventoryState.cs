using System.Collections.ObjectModel;

namespace BearAdventure.Domain.Gameplay;

public sealed class InventoryState
{
    private readonly Dictionary<ItemType, int> _counts = new();
    private readonly ReadOnlyDictionary<ItemType, int> _view;
    public InventoryState() { _view = new(_counts); }
    public IReadOnlyDictionary<ItemType, int> Counts => _view;
    public long Revision { get; private set; }
    public int Get(ItemType item) => _counts.GetValueOrDefault(item);
    public bool Has(ItemType item, int amount = 1) => amount > 0 && Get(item) >= amount;
    public bool IsEmpty => _counts.Count == 0;

    public void Set(ItemType item, int amount)
    {
        if (!Enum.IsDefined(item)) throw new ArgumentOutOfRangeException(nameof(item));
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (Get(item) == amount) return;
        if (amount == 0) _counts.Remove(item); else _counts[item] = amount;
        Revision++;
    }
    public void Add(ItemType item, int amount) => Set(item, checked(Get(item) + amount));
    public Dictionary<ItemType, int> Snapshot() => new(_counts);
    public void ReplaceWith(IEnumerable<KeyValuePair<ItemType, int>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var next = new Dictionary<ItemType, int>();
        foreach (var (item, count) in items)
        {
            if (!Enum.IsDefined(item) || count < 0) throw new ArgumentException("Invalid inventory entry.");
            if (count > 0) next.Add(item, count);
        }
        // Validation happens before any mutation (including self-replacement).
        _counts.Clear();
        foreach (var entry in next) _counts.Add(entry.Key, entry.Value);
        Revision++;
    }
    public static InventoryState CreateNormalStarter() => ProgressionService.CreateNormalStarter();

    public static InventoryState CreateDevelopmentStarter()
    {
        var result = new InventoryState();
        result.Set(ItemType.Axe, 1); result.Set(ItemType.Pickaxe, 1);
        return result;
    }
}
