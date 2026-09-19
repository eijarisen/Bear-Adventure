namespace BearAdventure.Domain.Gameplay;

public sealed class InventoryState
{
    private readonly Dictionary<ItemType, int> _counts = new();

    public IReadOnlyDictionary<ItemType, int> Counts => _counts;

    public int Get(ItemType item)
    {
        return _counts.TryGetValue(item, out int count) ? count : 0;
    }

    public bool Has(ItemType item, int amount = 1)
    {
        return amount > 0 && Get(item) >= amount;
    }

    public void Set(ItemType item, int amount)
    {
        if (amount <= 0)
        {
            _counts.Remove(item);
            return;
        }

        _counts[item] = amount;
    }

    public void Add(ItemType item, int amount)
    {
        if (amount == 0)
        {
            return;
        }

        int next = Get(item) + amount;
        if (next < 0)
        {
            throw new InvalidOperationException(
                $"Inventory operation would make {item} negative.");
        }

        Set(item, next);
    }

    public Dictionary<ItemType, int> Snapshot()
    {
        return new Dictionary<ItemType, int>(_counts);
    }

    public void ReplaceWith(IEnumerable<KeyValuePair<ItemType, int>> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _counts.Clear();

        foreach ((ItemType item, int count) in items)
        {
            if (count > 0)
            {
                _counts[item] = count;
            }
        }
    }

    public static InventoryState CreateDevelopmentStarter()
    {
        var inventory = new InventoryState();

        // Temporary until crafting is implemented. This makes the first tool
        // timing behavior testable without debug console commands.
        inventory.Set(ItemType.Axe, 1);
        inventory.Set(ItemType.Pickaxe, 1);

        return inventory;
    }
}
